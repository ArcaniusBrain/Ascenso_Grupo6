using Ascenso.S6.ComunicacionesReporteria.Application.Puertos;
using Ascenso.S6.ComunicacionesReporteria.Domain.Agregados;
using Ascenso.S6.ComunicacionesReporteria.Domain.Modelos;
using Ascenso.S6.ComunicacionesReporteria.Domain.Repositorios;
using Ascenso.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Ascenso.S6.ComunicacionesReporteria.Infrastructure.Persistencia;

public sealed class ConsentimientoDonanteRepositoryEfCore : IConsentimientoDonanteRepository
{
    private readonly ComunicacionesDbContext _db;

    public ConsentimientoDonanteRepositoryEfCore(ComunicacionesDbContext db) => _db = db;

    public Task<ConsentimientoDonante?> ObtenerPorBecarioYDonanteAsync(
        Guid becarioId,
        Guid donanteId,
        CancellationToken ct = default) =>
        _db.ConsentimientosDonante.FirstOrDefaultAsync(
            c => c.BecarioId == becarioId && c.DonanteId == donanteId,
            ct);

    public async Task<IReadOnlyList<ConsentimientoDonante>> ListarPorBecarioAsync(
        Guid becarioId,
        CancellationToken ct = default) =>
        await _db.ConsentimientosDonante
            .AsNoTracking()
            .Where(c => c.BecarioId == becarioId)
            .ToListAsync(ct);

    public async Task<IReadOnlySet<Guid>> ObtenerBecariosConConsentimientoAsync(
        Guid donanteId,
        CancellationToken ct = default)
    {
        var ids = await _db.ConsentimientosDonante
            .AsNoTracking()
            .Where(c => c.DonanteId == donanteId)
            .Select(c => c.BecarioId)
            .ToListAsync(ct);

        return ids.ToHashSet();
    }

    public async Task AgregarAsync(ConsentimientoDonante consentimiento, CancellationToken ct = default)
    {
        await _db.ConsentimientosDonante.AddAsync(consentimiento, ct);
    }
}

public sealed class SolicitudReporteImpactoRepositoryEfCore : ISolicitudReporteImpactoRepository
{
    private readonly ComunicacionesDbContext _db;

    public SolicitudReporteImpactoRepositoryEfCore(ComunicacionesDbContext db) => _db = db;

    public Task<SolicitudReporteImpacto?> ObtenerPorIdAsync(Guid reporteId, CancellationToken ct = default) =>
        _db.SolicitudesReporteImpacto.FirstOrDefaultAsync(s => s.Id == reporteId, ct);

    public async Task<IReadOnlyList<SolicitudReporteImpacto>> ListarAsync(
        Guid? donanteId = null,
        string? codigoPeriodo = null,
        CancellationToken ct = default)
    {
        IQueryable<SolicitudReporteImpacto> query = _db.SolicitudesReporteImpacto.AsNoTracking();

        if (donanteId.HasValue)
        {
            query = query.Where(s => s.DonanteId == donanteId.Value);
        }

        if (!string.IsNullOrWhiteSpace(codigoPeriodo))
        {
            var periodo = new CodigoPeriodo(codigoPeriodo);
            query = query.Where(s => s.Periodo == periodo);
        }

        return await query.ToListAsync(ct);
    }

    public async Task AgregarAsync(SolicitudReporteImpacto solicitud, CancellationToken ct = default)
    {
        await _db.SolicitudesReporteImpacto.AddAsync(solicitud, ct);
    }
}

public sealed class RepositorioProyeccionImpactoEfCore : IRepositorioProyeccionImpacto
{
    private readonly ComunicacionesDbContext _db;

    public RepositorioProyeccionImpactoEfCore(ComunicacionesDbContext db) => _db = db;

    public Task<bool> ExistePeriodoEnCalendarioAsync(string codigoPeriodo, CancellationToken ct = default) =>
        _db.ProyeccionPeriodos.AnyAsync(p => p.CodigoPeriodo == codigoPeriodo, ct);

    public async Task<IReadOnlyList<AporteDonanteProyectado>> ObtenerAportesDonanteEnPeriodoAsync(
        Guid donanteId,
        string codigoPeriodo,
        CancellationToken ct = default) =>
        await _db.ProyeccionAportes
            .AsNoTracking()
            .Where(a => a.DonanteId == donanteId && a.CodigoPeriodo == codigoPeriodo)
            .Select(a => new AporteDonanteProyectado(
                a.DonanteId,
                a.NombreDonante,
                a.ProgramaId,
                a.NombrePrograma,
                a.CodigoPeriodo,
                a.MontoAportado,
                a.Moneda))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BecarioProyectadoEntrada>> ObtenerBecariosDeProgramasAsync(
        IReadOnlyCollection<Guid> programaIds,
        CancellationToken ct = default)
    {
        if (programaIds.Count == 0)
        {
            return [];
        }

        return await _db.ProyeccionBecarios
            .AsNoTracking()
            .Where(b => programaIds.Contains(b.ProgramaId))
            .Select(b => new BecarioProyectadoEntrada(
                b.BecarioId,
                b.ProgramaId,
                b.NombreCompleto,
                b.Cedula,
                b.Universidad,
                b.Carrera,
                b.IngresoFamiliarSensible,
                b.ComposicionFamiliarSensible))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DesembolsoProyectadoEntrada>> ObtenerDesembolsosEnPeriodoAsync(
        IReadOnlyCollection<Guid> programaIds,
        string codigoPeriodo,
        CancellationToken ct = default)
    {
        if (programaIds.Count == 0)
        {
            return [];
        }

        return await _db.ProyeccionDesembolsos
            .AsNoTracking()
            .Where(d => programaIds.Contains(d.ProgramaId) && d.CodigoPeriodo == codigoPeriodo)
            .Select(d => new DesembolsoProyectadoEntrada(
                d.ProgramaId,
                d.CodigoPeriodo,
                d.Monto,
                d.Moneda,
                d.Estado))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ReporteAcademicoProyectadoEntrada>> ObtenerReportesAcademicosEnPeriodoAsync(
        IReadOnlyCollection<Guid> programaIds,
        string codigoPeriodo,
        CancellationToken ct = default)
    {
        if (programaIds.Count == 0)
        {
            return [];
        }

        return await _db.ProyeccionReportesAcademicos
            .AsNoTracking()
            .Where(r => programaIds.Contains(r.ProgramaId) && r.CodigoPeriodo == codigoPeriodo)
            .Select(r => new ReporteAcademicoProyectadoEntrada(
                r.BecarioId,
                r.ProgramaId,
                r.CodigoPeriodo,
                r.Promedio,
                r.MateriasAprobadas,
                r.MateriasReprobadas,
                r.Origen))
            .ToListAsync(ct);
    }
}

public sealed class UnidadTrabajoComunicacionesEfCore : IUnidadTrabajoComunicaciones
{
    private readonly ComunicacionesDbContext _db;

    public UnidadTrabajoComunicacionesEfCore(ComunicacionesDbContext db) => _db = db;

    public Task<int> GuardarCambiosAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
