using System.Globalization;
using Ascenso.S2.CatalogoFondos.Domain.Eventos;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Raíz de agregado PresupuestoPrograma (RF-12 · HU-12.1, HU-12.2, DDD v3.3 §8, ADR-25, R27, R31/A1, R32/A2).
/// Único guardián transaccional en S2 de presupuesto, cupo del programa y cupos por ciclo.
/// Protege: Disponible = Aportes - Otorgado - OfertasPendientes - RenovacionesPendientes >= 0
/// y CupoDisponible = CupoBecarios - CuposOcupados >= 0.
/// </summary>
public sealed class PresupuestoPrograma : RaizDeAgregado<Guid>
{
    public const string MensajeSinAportes = "Este programa no tiene aportes registrados.";
    public const string MensajeSinCupo = "El programa no tiene cupo disponible.";
    public const string MensajeSinCupoCiclo = "El ciclo de postulación no tiene cupo disponible.";

    private static readonly CultureInfo CulturaMontos = CultureInfo.GetCultureInfo("es-ES");

    private readonly List<CupoCiclo> _cuposCiclo = [];
    private readonly List<ReservaPresupuesto> _reservas = [];

    public Guid ProgramaId { get; private set; }
    public string Moneda { get; private set; } = "USD";
    public decimal Aportes { get; private set; }
    public decimal Otorgado { get; private set; }
    public decimal OfertasPendientes { get; private set; }
    public decimal RenovacionesPendientes { get; private set; }
    public decimal Desembolsado { get; private set; }
    public int CupoBecarios { get; private set; }
    public int CuposOcupados { get; private set; }
    public DateTimeOffset ActualizadoEnUtc { get; private set; }

    public IReadOnlyCollection<CupoCiclo> CuposCiclo => _cuposCiclo.AsReadOnly();
    public IReadOnlyCollection<ReservaPresupuesto> Reservas => _reservas.AsReadOnly();

    public decimal MontoDisponible => Aportes - Otorgado - OfertasPendientes - RenovacionesPendientes;
    public int CupoDisponible => CupoBecarios - CuposOcupados;

    private PresupuestoPrograma()
    {
    }

    private PresupuestoPrograma(
        Guid id,
        Guid programaId,
        int cupoBecarios,
        string moneda,
        DateTimeOffset ahoraUtc) : base(id)
    {
        ProgramaId = programaId;
        CupoBecarios = cupoBecarios;
        Moneda = moneda;
        ActualizadoEnUtc = ahoraUtc;
    }

    public static PresupuestoPrograma Crear(
        Guid programaId,
        int cupoBecarios,
        string? moneda,
        DateTimeOffset ahoraUtc)
    {
        if (programaId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("Indica el programa de becas.", "PROGRAMA_ID_INVALIDO");
        }

        if (cupoBecarios <= 0)
        {
            throw new ExcepcionDeDominio(Programa.MensajeCupoDebeSerPositivo, "CUPO_PROGRAMA_INVALIDO");
        }

        var codigoMoneda = string.IsNullOrWhiteSpace(moneda) ? "USD" : moneda.Trim().ToUpperInvariant();
        if (codigoMoneda.Length != 3)
        {
            throw new ExcepcionDeDominio("La moneda debe tener código ISO-3 de 3 letras.", "MONEDA_INVALIDA");
        }

        return new PresupuestoPrograma(Guid.NewGuid(), programaId, cupoBecarios, codigoMoneda, ahoraUtc);
    }

    public Dinero Disponible() => new(Math.Max(0m, MontoDisponible), Moneda);

    public void RegistrarAporte(Dinero monto, DateTimeOffset ahoraUtc)
    {
        ValidarMoneda(monto);
        if (monto.Cantidad <= 0m)
        {
            throw new ExcepcionDeDominio(Donante.MensajeMontoDebeSerPositivo, "MONTO_APORTE_INVALIDO");
        }

        Aportes += monto.Cantidad;
        ActualizadoEnUtc = ahoraUtc;
        IncrementarVersion();
    }

    public CupoCiclo RegistrarCupoCiclo(Guid cicloId, int limite, DateTimeOffset ahoraUtc)
    {
        var existente = _cuposCiclo.FirstOrDefault(c => c.CicloId == cicloId);
        if (existente is not null)
        {
            existente.ActualizarLimite(limite);
            ActualizadoEnUtc = ahoraUtc;
            IncrementarVersion();
            return existente;
        }

        var nuevo = new CupoCiclo(Guid.NewGuid(), Id, cicloId, limite);
        _cuposCiclo.Add(nuevo);
        ActualizadoEnUtc = ahoraUtc;
        IncrementarVersion();
        return nuevo;
    }

    /// <summary>
    /// Reserva atómicamente el monto total de toda la duración de la beca (R32/A2) y 1 cupo del programa (y del ciclo si aplica).
    /// Idempotente por operationId; bloquea duplicados activos por (postulacionId, cicloId).
    /// </summary>
    public ReservaPresupuesto ReservarOferta(
        Guid postulacionId,
        Guid cicloId,
        string codigoPeriodo,
        Dinero montoTotalDuracion,
        int duracionPeriodos,
        Guid operationId,
        DateTimeOffset ahoraUtc)
    {
        if (postulacionId == Guid.Empty || cicloId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("Indica la postulación y el ciclo para la reserva.", "CLAVE_RESERVA_INVALIDA");
        }

        if (operationId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("Indica el operationId de idempotencia.", "OPERATION_ID_INVALIDO");
        }

        if (duracionPeriodos <= 0)
        {
            throw new ExcepcionDeDominio("La duración comprometida debe ser mayor que cero.", "DURACION_INVALIDA");
        }

        var porOperacion = _reservas.FirstOrDefault(r => r.OperationId == operationId);
        if (porOperacion is not null)
        {
            return porOperacion;
        }

        var clave = ReservaPresupuesto.ClaveOferta(postulacionId, cicloId);
        if (_reservas.Any(r => r.ClaveNegocio == clave && r.Estado != EstadoReservaPresupuesto.LIBERADA))
        {
            throw new ExcepcionDeDominio(
                "Ya existe una reserva activa para esta postulación en este ciclo.",
                "RESERVA_DUPLICADA");
        }

        ValidarMoneda(montoTotalDuracion);
        if (montoTotalDuracion.Cantidad <= 0m)
        {
            throw new ExcepcionDeDominio("El monto a reservar debe ser mayor que cero.", "MONTO_RESERVA_INVALIDO");
        }

        if (CupoDisponible < 1)
        {
            throw new ExcepcionDeDominio(MensajeSinCupo, "CUPO_INSUFICIENTE");
        }

        var cupoCiclo = _cuposCiclo.FirstOrDefault(c => c.CicloId == cicloId);
        if (cupoCiclo is not null && !cupoCiclo.HayCupo())
        {
            throw new ExcepcionDeDominio(MensajeSinCupoCiclo, "CUPO_CICLO_INSUFICIENTE");
        }

        // Igualdad exacta (MontoDisponible == montoTotalDuracion.Cantidad) SÍ alcanza (HU-12.1 E8).
        if (MontoDisponible < montoTotalDuracion.Cantidad)
        {
            throw new ExcepcionDeDominio(
                MensajePresupuestoInsuficiente(MontoDisponible, montoTotalDuracion.Cantidad),
                "PRESUPUESTO_INSUFICIENTE");
        }

        OfertasPendientes += montoTotalDuracion.Cantidad;
        CuposOcupados++;
        cupoCiclo?.Ocupar();

        var reserva = new ReservaPresupuesto(
            Guid.NewGuid(),
            Id,
            TipoReservaPresupuesto.OFERTA_NUEVA,
            clave,
            operationId,
            postulacionId,
            cicloId,
            becaId: null,
            codigoPeriodo,
            montoTotalDuracion,
            duracionPeriodos,
            ahoraUtc);

        _reservas.Add(reserva);
        ActualizadoEnUtc = ahoraUtc;
        IncrementarVersion();
        return reserva;
    }

    /// <summary>
    /// Reserva atómicamente el monto fijado al enviar una solicitud de renovación (R31/A1, HU-12.2).
    /// No ocupa un nuevo cupo porque el becario ya ocupa su cupo en el programa.
    /// </summary>
    public ReservaPresupuesto ReservarRenovacion(
        Guid becaId,
        CodigoPeriodo periodo,
        Dinero montoSolicitado,
        Guid operationId,
        DateTimeOffset ahoraUtc)
    {
        if (becaId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("Indica la beca a renovar.", "CLAVE_RESERVA_INVALIDA");
        }

        if (operationId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("Indica el operationId de idempotencia.", "OPERATION_ID_INVALIDO");
        }

        var porOperacion = _reservas.FirstOrDefault(r => r.OperationId == operationId);
        if (porOperacion is not null)
        {
            return porOperacion;
        }

        var clave = ReservaPresupuesto.ClaveRenovacion(becaId, periodo);
        if (_reservas.Any(r => r.ClaveNegocio == clave && r.Estado != EstadoReservaPresupuesto.LIBERADA))
        {
            throw new ExcepcionDeDominio(
                "Ya existe una reserva de renovación activa para esta beca en este periodo.",
                "RESERVA_DUPLICADA");
        }

        ValidarMoneda(montoSolicitado);
        if (montoSolicitado.Cantidad <= 0m)
        {
            throw new ExcepcionDeDominio("El monto de la renovación debe ser mayor que cero.", "MONTO_RESERVA_INVALIDO");
        }

        if (MontoDisponible < montoSolicitado.Cantidad)
        {
            throw new ExcepcionDeDominio(
                MensajePresupuestoInsuficiente(MontoDisponible, montoSolicitado.Cantidad),
                "PRESUPUESTO_INSUFICIENTE");
        }

        RenovacionesPendientes += montoSolicitado.Cantidad;

        var reserva = new ReservaPresupuesto(
            Guid.NewGuid(),
            Id,
            TipoReservaPresupuesto.RENOVACION,
            clave,
            operationId,
            postulacionId: null,
            cicloId: null,
            becaId: becaId,
            codigoPeriodo: periodo.Valor,
            monto: montoSolicitado,
            duracionPeriodos: 1,
            creadaEnUtc: ahoraUtc);

        _reservas.Add(reserva);
        ActualizadoEnUtc = ahoraUtc;
        IncrementarVersion();
        return reserva;
    }

    public ReservaPresupuesto VincularReserva(Guid reservaId, Guid? recursoId, DateTimeOffset ahoraUtc)
    {
        var reserva = ObtenerReservaObligatoria(reservaId);
        if (reserva.Estado is EstadoReservaPresupuesto.VINCULADA or EstadoReservaPresupuesto.CONFIRMADA)
        {
            return reserva;
        }

        reserva.Vincular(recursoId, ahoraUtc);
        ActualizadoEnUtc = ahoraUtc;
        IncrementarVersion();
        return reserva;
    }

    /// <summary>
    /// Confirma la aceptación de una oferta nueva: mueve el compromiso de OfertasPendientes a Otorgado.
    /// El cupo ya estaba ocupado y permanece ocupado (incluso si luego pasa a Inactiva, R27 / HU-12.1 E12).
    /// </summary>
    public ReservaPresupuesto ConfirmarOtorgamiento(Guid reservaId, DateTimeOffset ahoraUtc)
    {
        var reserva = ObtenerReservaObligatoria(reservaId);
        if (reserva.Tipo != TipoReservaPresupuesto.OFERTA_NUEVA)
        {
            throw new ExcepcionDeDominio(
                "La reserva indicada corresponde a una renovación, no a una oferta nueva.",
                "TIPO_RESERVA_INVALIDO");
        }

        if (reserva.Estado == EstadoReservaPresupuesto.CONFIRMADA)
        {
            return reserva;
        }

        reserva.Confirmar(ahoraUtc);
        OfertasPendientes = Math.Max(0m, OfertasPendientes - reserva.Monto.Cantidad);
        Otorgado += reserva.Monto.Cantidad;
        ActualizadoEnUtc = ahoraUtc;
        IncrementarVersion();
        return reserva;
    }

    /// <summary>
    /// Confirma la aprobación de una renovación: mueve el compromiso de RenovacionesPendientes a Otorgado.
    /// </summary>
    public ReservaPresupuesto ConfirmarRenovacion(Guid reservaId, DateTimeOffset ahoraUtc)
    {
        var reserva = ObtenerReservaObligatoria(reservaId);
        if (reserva.Tipo != TipoReservaPresupuesto.RENOVACION)
        {
            throw new ExcepcionDeDominio(
                "La reserva indicada corresponde a una oferta nueva, no a una renovación.",
                "TIPO_RESERVA_INVALIDO");
        }

        if (reserva.Estado == EstadoReservaPresupuesto.CONFIRMADA)
        {
            return reserva;
        }

        reserva.Confirmar(ahoraUtc);
        RenovacionesPendientes = Math.Max(0m, RenovacionesPendientes - reserva.Monto.Cantidad);
        Otorgado += reserva.Monto.Cantidad;
        ActualizadoEnUtc = ahoraUtc;
        IncrementarVersion();
        return reserva;
    }

    /// <summary>
    /// Libera de forma idempotente una reserva pendiente (compensación, rechazo o expiración)
    /// o una beca otorgada ante revocación (HU-12.1 E2, HU-12.2 E3, HU-22.2).
    /// </summary>
    public ResultadoLiberacionReserva LiberarReserva(
        Guid reservaId,
        MotivoLiberacionReserva motivo,
        DateTimeOffset ahoraUtc,
        decimal? montoNoDesembolsadoRevocacion = null)
    {
        var reserva = ObtenerReservaObligatoria(reservaId);
        if (reserva.Estado == EstadoReservaPresupuesto.LIBERADA)
        {
            return new ResultadoLiberacionReserva(reserva, YaEstabaLiberada: true, SeLiberoCupo: false, MontoLiberado: 0m);
        }

        if (reserva.Estado == EstadoReservaPresupuesto.CONFIRMADA)
        {
            if (motivo != MotivoLiberacionReserva.REVOCACION)
            {
                throw new ExcepcionDeDominio(
                    "Una reserva ya confirmada solo puede liberarse por revocación de la beca.",
                    "RESERVA_CONFIRMADA_NO_LIBERABLE");
            }

            var aLiberar = montoNoDesembolsadoRevocacion ?? reserva.Monto.Cantidad;
            if (aLiberar < 0m || aLiberar > reserva.Monto.Cantidad)
            {
                throw new ExcepcionDeDominio(
                    "El monto no desembolsado a liberar debe estar entre 0 y el monto otorgado.",
                    "MONTO_LIBERACION_INVALIDO");
            }

            var liberadoReal = Math.Min(aLiberar, Otorgado);
            Otorgado = Math.Max(0m, Otorgado - liberadoReal);

            var liberoCupo = false;
            if (reserva.Tipo == TipoReservaPresupuesto.OFERTA_NUEVA)
            {
                if (CuposOcupados > 0)
                {
                    CuposOcupados--;
                    liberoCupo = true;
                }

                if (reserva.CicloId.HasValue)
                {
                    _cuposCiclo.FirstOrDefault(c => c.CicloId == reserva.CicloId.Value)?.Liberar();
                }
            }

            reserva.MarcarLiberada(motivo, ahoraUtc);
            ActualizadoEnUtc = ahoraUtc;
            IncrementarVersion();

            if (liberoCupo)
            {
                Registrar(new CupoLiberadoEvento(
                    ProgramaId,
                    reserva.CicloId,
                    reserva.Id,
                    motivo.ToString(),
                    liberadoReal,
                    Moneda,
                    ahoraUtc));
            }

            return new ResultadoLiberacionReserva(reserva, YaEstabaLiberada: false, SeLiberoCupo: liberoCupo, MontoLiberado: liberadoReal);
        }

        // Reserva en estado RESERVADA o VINCULADA (pendiente)
        var montoLiberado = reserva.Monto.Cantidad;
        var seLiberoCupo = false;

        if (reserva.Tipo == TipoReservaPresupuesto.OFERTA_NUEVA)
        {
            OfertasPendientes = Math.Max(0m, OfertasPendientes - montoLiberado);
            if (CuposOcupados > 0)
            {
                CuposOcupados--;
                seLiberoCupo = true;
            }

            if (reserva.CicloId.HasValue)
            {
                _cuposCiclo.FirstOrDefault(c => c.CicloId == reserva.CicloId.Value)?.Liberar();
            }
        }
        else
        {
            RenovacionesPendientes = Math.Max(0m, RenovacionesPendientes - montoLiberado);
        }

        reserva.MarcarLiberada(motivo, ahoraUtc);
        ActualizadoEnUtc = ahoraUtc;
        IncrementarVersion();

        if (seLiberoCupo)
        {
            Registrar(new CupoLiberadoEvento(
                ProgramaId,
                reserva.CicloId,
                reserva.Id,
                motivo.ToString(),
                montoLiberado,
                Moneda,
                ahoraUtc));
        }

        return new ResultadoLiberacionReserva(reserva, YaEstabaLiberada: false, SeLiberoCupo: seLiberoCupo, MontoLiberado: montoLiberado);
    }

    /// <summary>
    /// Libera cupo y monto no desembolsado ante BecaRevocada.v1 (DDD v3.3 §8, HU-22.2).
    /// BecaInactivada NO llama este método porque conserva cupo y presupuesto ocupados (R27, HU-12.1 E12).
    /// </summary>
    public void LiberarBecaRevocada(Dinero noDesembolsado, Guid? cicloId, Guid referenciaId, DateTimeOffset ahoraUtc)
    {
        ValidarMoneda(noDesembolsado);
        if (noDesembolsado.Cantidad < 0m)
        {
            throw new ExcepcionDeDominio("El monto a liberar no puede ser negativo.", "MONTO_LIBERACION_INVALIDO");
        }

        var montoAplicado = Math.Min(noDesembolsado.Cantidad, Otorgado);
        Otorgado = Math.Max(0m, Otorgado - montoAplicado);

        if (CuposOcupados > 0)
        {
            CuposOcupados--;
        }

        if (cicloId.HasValue)
        {
            _cuposCiclo.FirstOrDefault(c => c.CicloId == cicloId.Value)?.Liberar();
        }

        ActualizadoEnUtc = ahoraUtc;
        IncrementarVersion();
        Registrar(new CupoLiberadoEvento(
            ProgramaId,
            cicloId,
            referenciaId,
            MotivoLiberacionReserva.REVOCACION.ToString(),
            montoAplicado,
            Moneda,
            ahoraUtc));
    }

    public void RegistrarDesembolso(Dinero monto, DateTimeOffset ahoraUtc)
    {
        ValidarMoneda(monto);
        if (monto.Cantidad <= 0m)
        {
            throw new ExcepcionDeDominio("El monto del desembolso debe ser mayor que cero.", "MONTO_DESEMBOLSO_INVALIDO");
        }

        if (Desembolsado + monto.Cantidad > Otorgado)
        {
            throw new ExcepcionDeDominio(
                "El desembolso acumulado no puede superar el monto otorgado del programa.",
                "DESEMBOLSO_EXCEDE_OTORGADO");
        }

        Desembolsado += monto.Cantidad;
        ActualizadoEnUtc = ahoraUtc;
        IncrementarVersion();
    }

    public static string MensajePresupuestoInsuficiente(decimal disponible, decimal requerido) =>
        $"Presupuesto insuficiente: disponible {FormatearMonto(disponible)}, la beca requiere {FormatearMonto(requerido)}.";

    public static string FormatearMonto(decimal valor)
    {
        var esEntero = valor == decimal.Truncate(valor);
        return esEntero
            ? valor.ToString("#,##0", CulturaMontos)
            : valor.ToString("#,##0.##", CulturaMontos);
    }

    private ReservaPresupuesto ObtenerReservaObligatoria(Guid reservaId) =>
        _reservas.FirstOrDefault(r => r.Id == reservaId)
        ?? throw new ExcepcionDeDominio("La reserva indicada no está registrada.", "RESERVA_NO_ENCONTRADA");

    private void ValidarMoneda(Dinero dinero)
    {
        if (!string.Equals(dinero.Moneda, Moneda, StringComparison.OrdinalIgnoreCase))
        {
            throw new ExcepcionDeDominio(
                $"La moneda {dinero.Moneda} no coincide con la moneda del programa ({Moneda}).",
                "MONEDA_INCOMPATIBLE");
        }
    }
}

public sealed record ResultadoLiberacionReserva(
    ReservaPresupuesto Reserva,
    bool YaEstabaLiberada,
    bool SeLiberoCupo,
    decimal MontoLiberado);
