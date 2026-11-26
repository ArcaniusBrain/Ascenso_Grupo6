using Ascenso.S4.Becas.Application.HistorialBecas;
using Ascenso.S4.Becas.Domain.Becas;
using Microsoft.EntityFrameworkCore;

namespace Ascenso.S4.Becas.Infrastructure.Persistencia;

public sealed class ConsultaHistorialBecasEfCore(BecaDbContext dbContext) : IConsultaHistorialBecas
{
    private readonly BecaDbContext _db = dbContext;

    public async Task<PersonaReferenciaDto?> ObtenerPersonaPorCedulaAsync(
        string cedulaNormalizada,
        CancellationToken cancellationToken)
    {
        return await _db.ReferenciasPersonas
            .AsNoTracking()
            .Where(p => p.Cedula == cedulaNormalizada)
            .Select(p => new PersonaReferenciaDto(p.PersonaId, p.Cedula, p.NombreCompleto))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BecaHistorialItemDto>> ListarPorPersonaIdAsync(
        Guid personaId,
        CancellationToken cancellationToken)
    {
        var filas = await (
            from b in _db.Becas.AsNoTracking().Include(x => x.Tramos)
            join p in _db.ReferenciasPersonas.AsNoTracking() on b.PostulanteId equals p.PersonaId
            join pr in _db.ReferenciasProgramas.AsNoTracking() on b.ProgramaId equals pr.ProgramaId
            where b.PostulanteId == personaId
            select new { Beca = b, Persona = p, Programa = pr })
            .ToListAsync(cancellationToken);

        return MapearItems(filas.Select(x => (x.Beca, x.Persona, x.Programa)));
    }

    public async Task<ProgramaReferenciaDto?> ObtenerProgramaAsync(
        Guid programaId,
        CancellationToken cancellationToken)
    {
        return await _db.ReferenciasProgramas
            .AsNoTracking()
            .Where(p => p.ProgramaId == programaId)
            .Select(p => new ProgramaReferenciaDto(p.ProgramaId, p.Nombre, p.Alcance))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BecaHistorialItemDto>> ListarPorProgramaAsync(
        Guid programaId,
        CancellationToken cancellationToken)
    {
        var filas = await (
            from b in _db.Becas.AsNoTracking().Include(x => x.Tramos)
            join p in _db.ReferenciasPersonas.AsNoTracking() on b.PostulanteId equals p.PersonaId
            join pr in _db.ReferenciasProgramas.AsNoTracking() on b.ProgramaId equals pr.ProgramaId
            where b.ProgramaId == programaId
            select new { Beca = b, Persona = p, Programa = pr })
            .ToListAsync(cancellationToken);

        return MapearItems(filas.Select(x => (x.Beca, x.Persona, x.Programa)));
    }

    public async Task<IReadOnlyList<BecaHistorialItemDto>> ListarPorPeriodoAsync(
        string periodoNormalizado,
        CancellationToken cancellationToken)
    {
        var filas = await (
            from b in _db.Becas.AsNoTracking().Include(x => x.Tramos)
            join p in _db.ReferenciasPersonas.AsNoTracking() on b.PostulanteId equals p.PersonaId
            join pr in _db.ReferenciasProgramas.AsNoTracking() on b.ProgramaId equals pr.ProgramaId
            select new { Beca = b, Persona = p, Programa = pr })
            .ToListAsync(cancellationToken);

        return filas
            .SelectMany(f => f.Beca.Tramos
                .Where(t => t.Periodo.Valor == periodoNormalizado)
                .Select(t => CrearDto(f.Beca, f.Persona, f.Programa, t)))
            .ToList();
    }

    private static IReadOnlyList<BecaHistorialItemDto> MapearItems(
        IEnumerable<(Beca Beca, ReferenciaPersona Persona, ReferenciaPrograma Programa)> filas)
    {
        var resultado = new List<BecaHistorialItemDto>();
        foreach (var (beca, persona, programa) in filas)
        {
            foreach (var tramo in beca.Tramos)
            {
                resultado.Add(CrearDto(beca, persona, programa, tramo));
            }
        }

        return resultado;
    }

    private static BecaHistorialItemDto CrearDto(
        Beca beca,
        ReferenciaPersona persona,
        ReferenciaPrograma programa,
        TramoBeca tramo) =>
        new(
            beca.Id,
            beca.PostulanteId,
            persona.NombreCompleto,
            persona.Cedula,
            beca.ProgramaId,
            programa.Nombre,
            programa.Alcance.ToString(),
            tramo.Periodo.Valor,
            tramo.MontoAprobado.Cantidad,
            tramo.MontoAprobado.Moneda,
            beca.Estado.ToString(),
            PoliticaEstadoBeca.CuentaComoActiva(beca.Estado));
}
