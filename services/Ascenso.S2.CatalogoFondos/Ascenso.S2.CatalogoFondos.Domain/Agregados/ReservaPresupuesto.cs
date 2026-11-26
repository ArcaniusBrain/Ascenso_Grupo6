using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Entidad interna de PresupuestoPrograma que registra una reserva atómica de oferta nueva (R32/A2)
/// o solicitud de renovación (R31/A1) con clave de negocio única e idempotencia por operationId.
/// </summary>
public sealed class ReservaPresupuesto : Entidad<Guid>
{
    public Guid PresupuestoProgramaId { get; private set; }
    public TipoReservaPresupuesto Tipo { get; private set; }
    public EstadoReservaPresupuesto Estado { get; private set; }
    public string ClaveNegocio { get; private set; } = string.Empty;
    public Guid OperationId { get; private set; }
    public Guid? PostulacionId { get; private set; }
    public Guid? CicloId { get; private set; }
    public Guid? BecaId { get; private set; }
    public string CodigoPeriodo { get; private set; } = string.Empty;
    public Dinero Monto { get; private set; } = default!;
    public int DuracionPeriodos { get; private set; }
    public DateTimeOffset CreadaEnUtc { get; private set; }
    public DateTimeOffset? VinculadaEnUtc { get; private set; }
    public DateTimeOffset? CerradaEnUtc { get; private set; }
    public MotivoLiberacionReserva? MotivoLiberacion { get; private set; }
    public Guid? RecursoVinculadoId { get; private set; }

    private ReservaPresupuesto()
    {
    }

    internal ReservaPresupuesto(
        Guid id,
        Guid presupuestoProgramaId,
        TipoReservaPresupuesto tipo,
        string claveNegocio,
        Guid operationId,
        Guid? postulacionId,
        Guid? cicloId,
        Guid? becaId,
        string codigoPeriodo,
        Dinero monto,
        int duracionPeriodos,
        DateTimeOffset creadaEnUtc) : base(id)
    {
        PresupuestoProgramaId = presupuestoProgramaId;
        Tipo = tipo;
        Estado = EstadoReservaPresupuesto.RESERVADA;
        ClaveNegocio = claveNegocio;
        OperationId = operationId;
        PostulacionId = postulacionId;
        CicloId = cicloId;
        BecaId = becaId;
        CodigoPeriodo = codigoPeriodo;
        Monto = monto;
        DuracionPeriodos = duracionPeriodos;
        CreadaEnUtc = creadaEnUtc;
    }

    public static string ClaveOferta(Guid postulacionId, Guid cicloId) =>
        $"OFERTA:{postulacionId:N}:{cicloId:N}";

    public static string ClaveRenovacion(Guid becaId, CodigoPeriodo periodo) =>
        $"RENOVACION:{becaId:N}:{periodo.Valor}";

    /// <summary>
    /// Estado mapeado al enum de contracts/openapi-interno.yaml (RESERVADA | VINCULADA | LIBERADA).
    /// </summary>
    public string EstadoContratoInterno() => Estado switch
    {
        EstadoReservaPresupuesto.RESERVADA => "RESERVADA",
        EstadoReservaPresupuesto.VINCULADA or EstadoReservaPresupuesto.CONFIRMADA => "VINCULADA",
        EstadoReservaPresupuesto.LIBERADA => "LIBERADA",
        _ => Estado.ToString()
    };

    internal void Vincular(Guid? recursoId, DateTimeOffset ahoraUtc)
    {
        if (Estado == EstadoReservaPresupuesto.LIBERADA)
        {
            throw new ExcepcionDeDominio(
                "No se puede vincular una reserva que ya fue liberada.",
                "RESERVA_YA_LIBERADA");
        }

        if (Estado == EstadoReservaPresupuesto.RESERVADA)
        {
            Estado = EstadoReservaPresupuesto.VINCULADA;
            VinculadaEnUtc = ahoraUtc;
            RecursoVinculadoId = recursoId;
        }
    }

    internal void Confirmar(DateTimeOffset ahoraUtc)
    {
        if (Estado == EstadoReservaPresupuesto.LIBERADA)
        {
            throw new ExcepcionDeDominio(
                "No se puede confirmar una reserva que ya fue liberada.",
                "RESERVA_YA_LIBERADA");
        }

        if (Estado is EstadoReservaPresupuesto.RESERVADA or EstadoReservaPresupuesto.VINCULADA)
        {
            Estado = EstadoReservaPresupuesto.CONFIRMADA;
            VinculadaEnUtc ??= ahoraUtc;
            CerradaEnUtc = ahoraUtc;
        }
    }

    internal void MarcarLiberada(MotivoLiberacionReserva motivo, DateTimeOffset ahoraUtc)
    {
        Estado = EstadoReservaPresupuesto.LIBERADA;
        MotivoLiberacion = motivo;
        CerradaEnUtc = ahoraUtc;
    }
}
