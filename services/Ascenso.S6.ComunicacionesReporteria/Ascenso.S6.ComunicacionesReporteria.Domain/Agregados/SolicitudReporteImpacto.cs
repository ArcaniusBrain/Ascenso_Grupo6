using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S6.ComunicacionesReporteria.Domain.Agregados;

public enum EstadoSolicitudReporteImpacto
{
    PREPARANDO = 1,
    LISTO = 2,
    FALLIDO = 3
}

/// <summary>
/// Raíz de agregado que representa una solicitud asíncrona y el snapshot conservado de un reporte de impacto (HU-33.1 E9, E12, C20).
/// </summary>
public sealed class SolicitudReporteImpacto : RaizDeAgregado<Guid>
{
    public const string MensajePreparando = "El reporte se está preparando.";
    public const string MensajeSinDatosAcademicos = "Aún no hay datos académicos para este periodo.";

    public Guid DonanteId { get; private set; }
    public string NombreDonante { get; private set; } = string.Empty;
    public CodigoPeriodo Periodo { get; private set; } = default!;
    public Guid SolicitadoPorUsuarioId { get; private set; }
    public string RolSolicitante { get; private set; } = string.Empty;
    public string? ProgramasAlcanceCsv { get; private set; }
    public EstadoSolicitudReporteImpacto Estado { get; private set; }
    public string? MensajeEstado { get; private set; }
    public DateTimeOffset SolicitadoEnUtc { get; private set; }
    public DateTimeOffset? CompletadoEnUtc { get; private set; }
    public string? ContenidoJson { get; private set; }

    private SolicitudReporteImpacto()
    {
    }

    private SolicitudReporteImpacto(
        Guid id,
        Guid donanteId,
        string nombreDonante,
        CodigoPeriodo periodo,
        Guid solicitadoPorUsuarioId,
        string rolSolicitante,
        string? programasAlcanceCsv,
        DateTimeOffset solicitadoEnUtc) : base(id)
    {
        DonanteId = donanteId;
        NombreDonante = nombreDonante;
        Periodo = periodo;
        SolicitadoPorUsuarioId = solicitadoPorUsuarioId;
        RolSolicitante = rolSolicitante;
        ProgramasAlcanceCsv = programasAlcanceCsv;
        Estado = EstadoSolicitudReporteImpacto.PREPARANDO;
        MensajeEstado = MensajePreparando;
        SolicitadoEnUtc = solicitadoEnUtc;
    }

    public static SolicitudReporteImpacto Crear(
        Guid donanteId,
        string nombreDonante,
        CodigoPeriodo periodo,
        Guid solicitadoPorUsuarioId,
        string rolSolicitante,
        IReadOnlyCollection<Guid>? programasAlcance,
        DateTimeOffset ahoraUtc)
    {
        if (donanteId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("Indica el donante del reporte.", "DONANTE_INVALIDO");
        }

        if (string.IsNullOrWhiteSpace(nombreDonante))
        {
            throw new ExcepcionDeDominio("Indica el nombre del donante.", "DONANTE_INVALIDO");
        }

        var alcanceCsv = programasAlcance is { Count: > 0 }
            ? string.Join(",", programasAlcance.Distinct())
            : null;

        return new SolicitudReporteImpacto(
            Guid.NewGuid(),
            donanteId,
            nombreDonante.Trim(),
            periodo,
            solicitadoPorUsuarioId,
            rolSolicitante.Trim().ToUpperInvariant(),
            alcanceCsv,
            ahoraUtc);
    }

    public IReadOnlyList<Guid> ObtenerProgramasAlcance() =>
        string.IsNullOrWhiteSpace(ProgramasAlcanceCsv)
            ? []
            : ProgramasAlcanceCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToArray();

    public void Completar(string contenidoJson, DateTimeOffset ahoraUtc)
    {
        if (string.IsNullOrWhiteSpace(contenidoJson))
        {
            throw new ExcepcionDeDominio("El contenido del reporte no puede estar vacío.", "REPORTE_CONTENIDO_VACIO");
        }

        ContenidoJson = contenidoJson;
        Estado = EstadoSolicitudReporteImpacto.LISTO;
        MensajeEstado = null;
        CompletadoEnUtc = ahoraUtc;
        IncrementarVersion();
    }

    public void MarcarFallido(string detalle, DateTimeOffset ahoraUtc)
    {
        Estado = EstadoSolicitudReporteImpacto.FALLIDO;
        MensajeEstado = string.IsNullOrWhiteSpace(detalle) ? "No se pudo generar el reporte." : detalle.Trim();
        CompletadoEnUtc = ahoraUtc;
        IncrementarVersion();
    }

    public static string MensajePeriodoInexistente(string codigoPeriodo) =>
        $"El periodo {codigoPeriodo} no existe en el calendario académico.";

    public static string MensajeSinAportesEnPeriodo(string codigoPeriodo) =>
        $"Este donante no tiene aportes registrados en el periodo {codigoPeriodo}.";
}
