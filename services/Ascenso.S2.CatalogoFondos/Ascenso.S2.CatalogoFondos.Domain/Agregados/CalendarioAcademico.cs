using Ascenso.S2.CatalogoFondos.Domain.Eventos;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

public sealed record ResultadoMarcadoActual(
    bool HuboCambio,
    CodigoPeriodo CodigoPeriodoActual,
    CodigoPeriodo? CodigoPeriodoAnterior);

/// <summary>
/// Raíz de agregado institucional del calendario académico (RF-38 · HU-38.1 y HU-38.2, DDD v3.3 §7 y §15.7).
/// Protege con Version explícita (ADR-25) la unicidad de los códigos de periodo y la existencia
/// de como máximo un único periodo actual incluso ante solicitudes simultáneas.
/// </summary>
public sealed class CalendarioAcademico : RaizDeAgregado<Guid>
{
    public static readonly Guid IdInstitucional = Guid.Parse("20000000-0000-4000-8000-000000000001");

    private readonly List<PeriodoAcademico> _periodos = [];

    public IReadOnlyCollection<PeriodoAcademico> Periodos => _periodos.AsReadOnly();

    private CalendarioAcademico()
    {
    }

    private CalendarioAcademico(Guid id) : base(id)
    {
    }

    public static CalendarioAcademico CrearInstitucional() => new(IdInstitucional);

    public PeriodoAcademico AbrirPeriodo(string? codigoTexto, DateTimeOffset ahoraUtc)
    {
        if (string.IsNullOrWhiteSpace(codigoTexto))
        {
            throw new ExcepcionDeDominio(
                "Escribe el nombre del periodo académico.",
                "CODIGO_PERIODO_VACIO");
        }

        var codigo = new CodigoPeriodo(codigoTexto);

        if (_periodos.Any(p => p.Codigo == codigo))
        {
            throw new ExcepcionDeDominio(
                $"El periodo {codigo.Valor} ya existe.",
                "PERIODO_DUPLICADO");
        }

        var periodo = new PeriodoAcademico(Guid.NewGuid(), Id, codigo, ahoraUtc);
        _periodos.Add(periodo);

        IncrementarVersion();
        Registrar(new PeriodoAbiertoEvento(Id, codigo.Valor, ahoraUtc));

        return periodo;
    }

    public PeriodoAcademico CerrarPeriodo(CodigoPeriodo codigo)
    {
        var periodo = ObtenerPeriodoORechazar(codigo);

        if (periodo.EsActual)
        {
            throw new ExcepcionDeDominio(
                $"No se puede cerrar el periodo actual {codigo.Valor} sin marcar otro periodo como actual antes.",
                "PERIODO_ACTUAL_NO_CERRABLE");
        }

        if (periodo.Estado == EstadoPeriodo.CERRADO)
        {
            return periodo;
        }

        periodo.Cerrar();
        IncrementarVersion();
        return periodo;
    }

    public ResultadoMarcadoActual MarcarActual(CodigoPeriodo codigo, DateTimeOffset ahoraUtc)
    {
        var destino = ObtenerPeriodoORechazar(codigo);

        // HU-38.2 Escenario 8: repetir el periodo ya actual es idempotente y no duplica cambios ni eventos.
        if (destino.EsActual)
        {
            return new ResultadoMarcadoActual(false, destino.Codigo, destino.Codigo);
        }

        // Anexo B · HU-38.2 #3 (C15) y DDD v3.3 §15.7: solo un periodo ABIERTO puede marcarse como actual.
        if (destino.Estado != EstadoPeriodo.ABIERTO)
        {
            throw new ExcepcionDeDominio(
                $"El periodo {destino.Codigo.Valor} está cerrado y no puede marcarse como actual.",
                "PERIODO_CERRADO_NO_MARCABLE");
        }

        var anterior = _periodos.FirstOrDefault(p => p.EsActual);
        anterior?.DesmarcarActual();
        destino.MarcarComoActual();

        IncrementarVersion();
        Registrar(new PeriodoMarcadoActualEvento(
            Id,
            destino.Codigo.Valor,
            anterior?.Codigo.Valor,
            ahoraUtc));

        return new ResultadoMarcadoActual(true, destino.Codigo, anterior?.Codigo);
    }

    public PeriodoAcademico? PeriodoActual() => _periodos.FirstOrDefault(p => p.EsActual);

    public PeriodoAcademico? ObtenerPeriodo(CodigoPeriodo codigo) =>
        _periodos.FirstOrDefault(p => p.Codigo == codigo);

    public bool ExistePeriodo(CodigoPeriodo codigo) =>
        _periodos.Any(p => p.Codigo == codigo);

    /// <summary>
    /// Devuelve los periodos ordenados cronológicamente en forma ascendente (Anexo B · HU-38.1 #1, C15).
    /// </summary>
    public IReadOnlyList<PeriodoAcademico> ObtenerPeriodosOrdenados() =>
        _periodos
            .OrderBy(p => p.Codigo, ComparadorCodigoPeriodo.Instancia)
            .ToArray();

    private PeriodoAcademico ObtenerPeriodoORechazar(CodigoPeriodo codigo)
    {
        var periodo = _periodos.FirstOrDefault(p => p.Codigo == codigo);
        if (periodo is null)
        {
            throw new ExcepcionDeDominio(
                $"El periodo {codigo.Valor} no está registrado.",
                "PERIODO_NO_ENCONTRADO");
        }

        return periodo;
    }
}
