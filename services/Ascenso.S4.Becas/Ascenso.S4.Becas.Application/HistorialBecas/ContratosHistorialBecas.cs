using Ascenso.S4.Becas.Domain.Becas;
using MediatR;

namespace Ascenso.S4.Becas.Application.HistorialBecas;

public sealed record ConsultarHistorialBecasQuery(
    RolHistorialBecas RolActor,
    string? CedulaActor,
    string? CedulaConsultada,
    Guid? ProgramaId,
    string? Periodo) : IRequest<HistorialBecasResultado>;

public sealed record PersonaReferenciaDto(Guid PersonaId, string Cedula, string NombreCompleto);

public sealed record ProgramaReferenciaDto(Guid ProgramaId, string Nombre, Alcance Alcance);

/// <summary>
/// Elemento del historial de becas. Excluye intencionalmente todo dato socioeconómico (HU-18.1).
/// </summary>
public sealed record BecaHistorialItemDto(
    Guid BecaId,
    Guid PostulanteId,
    string NombrePostulante,
    string CedulaPostulante,
    Guid ProgramaId,
    string NombrePrograma,
    string Alcance,
    string Periodo,
    decimal MontoPeriodo,
    string Moneda,
    string Estado,
    bool CuentaComoActiva);

public sealed record HistorialBecasResultado(
    bool Autorizado,
    bool NoEncontrado,
    bool CriterioInvalido,
    string? Mensaje,
    IReadOnlyList<BecaHistorialItemDto> Items)
{
    public static HistorialBecasResultado Denegado(string mensaje) =>
        new(false, false, false, mensaje, []);

    public static HistorialBecasResultado RecursoNoEncontrado(string mensaje) =>
        new(true, true, false, mensaje, []);

    public static HistorialBecasResultado Invalido(string mensaje) =>
        new(true, false, true, mensaje, []);

    public static HistorialBecasResultado Exito(IReadOnlyList<BecaHistorialItemDto> items, string? mensaje = null) =>
        new(true, false, false, mensaje, items);
}

public interface IConsultaHistorialBecas
{
    Task<PersonaReferenciaDto?> ObtenerPersonaPorCedulaAsync(string cedulaNormalizada, CancellationToken cancellationToken);
    Task<IReadOnlyList<BecaHistorialItemDto>> ListarPorPersonaIdAsync(Guid personaId, CancellationToken cancellationToken);
    Task<ProgramaReferenciaDto?> ObtenerProgramaAsync(Guid programaId, CancellationToken cancellationToken);
    Task<IReadOnlyList<BecaHistorialItemDto>> ListarPorProgramaAsync(Guid programaId, CancellationToken cancellationToken);
    Task<IReadOnlyList<BecaHistorialItemDto>> ListarPorPeriodoAsync(string periodoNormalizado, CancellationToken cancellationToken);
}
