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

public sealed class ConsultorElementosPeriodoPorDefecto : IConsultorElementosPeriodo
{
    /// <summary>
    /// En este primer incremento de S2 (RF-38) aún no existen tablas de Programas (RF-11) ni proyecciones locales
    /// de ciclos/renovaciones; cuando se incorpore RF-11 en S2, esta implementación consultará los elementos ligados.
    /// </summary>
    public Task<int> ContarElementosLigadosAsync(CodigoPeriodo codigoPeriodo, CancellationToken ct = default) =>
        Task.FromResult(0);
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
