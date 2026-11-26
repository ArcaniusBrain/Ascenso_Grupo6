using System.Text.Json;
using Ascenso.Messaging.Outbox;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;
using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Ascenso.S2.CatalogoFondos.Infrastructure.Persistencia;

public sealed class CalendarioAcademicoRepositoryEfCore : ICalendarioAcademicoRepository
{
    private readonly CatalogoFondosDbContext _db;

    public CalendarioAcademicoRepositoryEfCore(CatalogoFondosDbContext db) => _db = db;

    public Task<CalendarioAcademico?> ObtenerInstitucionalAsync(CancellationToken ct = default) =>
        _db.CalendariosAcademicos
            .Include(c => c.Periodos)
            .FirstOrDefaultAsync(c => c.Id == CalendarioAcademico.IdInstitucional, ct);

    public async Task AgregarAsync(CalendarioAcademico calendario, CancellationToken ct = default)
    {
        await _db.CalendariosAcademicos.AddAsync(calendario, ct);
    }
}

public sealed class InicioInscripcionesRepositoryEfCore : IInicioInscripcionesRepository
{
    private readonly CatalogoFondosDbContext _db;

    public InicioInscripcionesRepositoryEfCore(CatalogoFondosDbContext db) => _db = db;

    public Task<InicioInscripcionesPeriodo?> ObtenerPorUniversidadYPeriodoAsync(
        Guid universidadId,
        CodigoPeriodo codigoPeriodo,
        CancellationToken ct = default) =>
        _db.IniciosInscripcionesPeriodo.FirstOrDefaultAsync(
            i => i.UniversidadId == universidadId && i.CodigoPeriodo == codigoPeriodo,
            ct);

    public async Task<IReadOnlyList<InicioInscripcionesPeriodo>> ListarPorPeriodoAsync(
        CodigoPeriodo codigoPeriodo,
        CancellationToken ct = default) =>
        await _db.IniciosInscripcionesPeriodo
            .Where(i => i.CodigoPeriodo == codigoPeriodo)
            .ToListAsync(ct);

    public async Task AgregarAsync(InicioInscripcionesPeriodo inicio, CancellationToken ct = default)
    {
        await _db.IniciosInscripcionesPeriodo.AddAsync(inicio, ct);
    }
}

public sealed class EscritorOutboxCatalogoEfCore : IEscritorOutboxCatalogo
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly CatalogoFondosDbContext _db;

    public EscritorOutboxCatalogoEfCore(CatalogoFondosDbContext db) => _db = db;

    public async Task RegistrarEventoAsync<TEvento>(
        Guid eventId,
        string eventType,
        int schemaVersion,
        TEvento payload,
        Guid correlationId,
        Guid causationId,
        DateTimeOffset occurredAtUtc,
        CancellationToken ct = default)
    {
        var payloadJson = JsonSerializer.Serialize(payload, JsonOptions);
        var mensaje = OutboxMessage.Crear(
            eventId,
            eventType,
            schemaVersion,
            payloadJson,
            correlationId,
            causationId,
            occurredAtUtc);

        await _db.OutboxMessages.AddAsync(mensaje, ct);
    }
}

public sealed class RegistroIdempotenciaComandosEfCore : IRegistroIdempotenciaComandos
{
    private readonly CatalogoFondosDbContext _db;

    public RegistroIdempotenciaComandosEfCore(CatalogoFondosDbContext db) => _db = db;

    public async Task<OperacionConfirmadaDto?> BuscarAsync(Guid operationId, CancellationToken ct = default)
    {
        var registro = await _db.OperacionesIdempotentes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OperationId == operationId, ct);

        return registro is null
            ? null
            : new OperacionConfirmadaDto(
                registro.ResultadoId,
                registro.Estado,
                registro.CorrelationId,
                registro.Version);
    }

    public async Task RegistrarAsync(
        Guid operationId,
        OperacionConfirmadaDto resultado,
        DateTimeOffset procesadoEnUtc,
        CancellationToken ct = default)
    {
        var registro = OperacionIdempotenteCatalogo.Crear(
            operationId,
            resultado.Id,
            resultado.Estado,
            resultado.CorrelationId,
            resultado.Version,
            procesadoEnUtc);

        await _db.OperacionesIdempotentes.AddAsync(registro, ct);
    }
}

public sealed class ConsultorElementosPeriodoEfCore : IConsultorElementosPeriodo
{
    private readonly CatalogoFondosDbContext _db;

    public ConsultorElementosPeriodoEfCore(CatalogoFondosDbContext db) => _db = db;

    public Task<int> ContarElementosLigadosAsync(CodigoPeriodo codigoPeriodo, CancellationToken ct = default) =>
        _db.Programas.CountAsync(p => p.PeriodoInicio == codigoPeriodo, ct);
}

public sealed class UnidadTrabajoCatalogoEfCore : IUnidadTrabajoCatalogo
{
    private readonly CatalogoFondosDbContext _db;

    public UnidadTrabajoCatalogoEfCore(CatalogoFondosDbContext db) => _db = db;

    public Task<int> GuardarCambiosAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}

public sealed class UniversidadAliadaRepositoryEfCore : IUniversidadAliadaRepository
{
    private readonly CatalogoFondosDbContext _db;

    public UniversidadAliadaRepositoryEfCore(CatalogoFondosDbContext db) => _db = db;

    public Task<UniversidadAliada?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _db.UniversidadesAliadas
            .Include(u => u.Carreras)
            .Include(u => u.Costos)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<bool> ExisteNombreNormalizadoAsync(
        string nombreNormalizado,
        Guid? excluirUniversidadId = null,
        CancellationToken ct = default) =>
        _db.UniversidadesAliadas.AnyAsync(
            u => u.NombreNormalizado == nombreNormalizado
                 && (!excluirUniversidadId.HasValue || u.Id != excluirUniversidadId.Value),
            ct);

    public async Task<IReadOnlyList<UniversidadAliada>> ListarTodasAsync(CancellationToken ct = default) =>
        await _db.UniversidadesAliadas
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task AgregarAsync(UniversidadAliada universidad, CancellationToken ct = default)
    {
        await _db.UniversidadesAliadas.AddAsync(universidad, ct);
    }
}

public sealed class ProgramaRepositoryEfCore : IProgramaRepository
{
    private readonly CatalogoFondosDbContext _db;

    public ProgramaRepositoryEfCore(CatalogoFondosDbContext db) => _db = db;

    public Task<Programa?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Programas
            .Include(p => p.Donantes)
            .Include(p => p.Restricciones)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> ExisteNombreNormalizadoAsync(
        string nombreNormalizado,
        Guid? excluirProgramaId = null,
        CancellationToken ct = default) =>
        _db.Programas.AnyAsync(
            p => p.NombreNormalizado == nombreNormalizado
                 && (!excluirProgramaId.HasValue || p.Id != excluirProgramaId.Value),
            ct);

    public async Task<IReadOnlyList<Programa>> ListarTodosAsync(CancellationToken ct = default) =>
        await _db.Programas
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<int> ContarPorPeriodoAsync(CodigoPeriodo codigoPeriodo, CancellationToken ct = default) =>
        _db.Programas.CountAsync(p => p.PeriodoInicio == codigoPeriodo, ct);

    public async Task AgregarAsync(Programa programa, CancellationToken ct = default)
    {
        await _db.Programas.AddAsync(programa, ct);
    }
}

public sealed class DonanteRepositoryEfCore : IDonanteRepository
{
    private readonly CatalogoFondosDbContext _db;

    public DonanteRepositoryEfCore(CatalogoFondosDbContext db) => _db = db;

    public Task<Donante?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Donantes
            .Include(d => d.Aportes)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<bool> ExisteNombreNormalizadoAsync(
        string nombreNormalizado,
        Guid? excluirDonanteId = null,
        CancellationToken ct = default) =>
        _db.Donantes.AnyAsync(
            d => d.NombreNormalizado == nombreNormalizado
                 && (!excluirDonanteId.HasValue || d.Id != excluirDonanteId.Value),
            ct);

    public async Task<IReadOnlyList<Donante>> ListarTodosAsync(CancellationToken ct = default) =>
        await _db.Donantes
            .Include(d => d.Aportes)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task AgregarAsync(Donante donante, CancellationToken ct = default)
    {
        await _db.Donantes.AddAsync(donante, ct);
    }
}

public sealed class ConsultorDonantesYAportesEfCore : IConsultorDonantesYAportes
{
    private readonly CatalogoFondosDbContext _db;

    public ConsultorDonantesYAportesEfCore(CatalogoFondosDbContext db) => _db = db;

    public Task<bool> ExistenDonantesRegistradosAsync(CancellationToken ct = default) =>
        _db.Donantes.AnyAsync(ct);

    public async Task<DonanteRegistradoInfo?> ObtenerDonanteAsync(Guid donanteId, CancellationToken ct = default)
    {
        var donante = await _db.Donantes
            .AsNoTracking()
            .Where(d => d.Id == donanteId)
            .Select(d => new { d.Id, d.Nombre })
            .FirstOrDefaultAsync(ct);

        return donante is null ? null : new DonanteRegistradoInfo(donante.Id, donante.Nombre);
    }

    public async Task<IReadOnlyDictionary<Guid, AportesDonanteEnPrograma>> ObtenerAportesPorProgramaAsync(
        Guid programaId,
        CancellationToken ct = default)
    {
        var aportes = await _db.Aportes
            .AsNoTracking()
            .Where(a => a.ProgramaId == programaId)
            .ToListAsync(ct);

        if (aportes.Count == 0)
        {
            return new Dictionary<Guid, AportesDonanteEnPrograma>();
        }

        var donanteIds = aportes.Select(a => a.DonanteId).Distinct().ToArray();
        var nombres = await _db.Donantes
            .AsNoTracking()
            .Where(d => donanteIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.Nombre, ct);

        return aportes
            .GroupBy(a => a.DonanteId)
            .ToDictionary(
                g => g.Key,
                g => new AportesDonanteEnPrograma(
                    g.Key,
                    nombres.TryGetValue(g.Key, out var nombre) ? nombre : g.Key.ToString(),
                    g.Sum(a => a.Monto.Cantidad),
                    g.First().Monto.Moneda,
                    g.Count()));
    }
}
