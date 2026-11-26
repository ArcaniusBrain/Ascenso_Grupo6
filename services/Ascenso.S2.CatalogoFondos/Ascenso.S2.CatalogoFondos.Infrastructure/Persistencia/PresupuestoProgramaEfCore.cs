using Ascenso.Messaging.Outbox;
using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Ascenso.S2.CatalogoFondos.Infrastructure.Persistencia;

public sealed class PresupuestoProgramaRepositoryEfCore : IPresupuestoProgramaRepository
{
    private readonly CatalogoFondosDbContext _db;

    public PresupuestoProgramaRepositoryEfCore(CatalogoFondosDbContext db) => _db = db;

    public Task<PresupuestoPrograma?> ObtenerPorProgramaIdAsync(Guid programaId, CancellationToken ct = default) =>
        _db.PresupuestosPrograma
            .Include(p => p.CuposCiclo)
            .Include(p => p.Reservas)
            .FirstOrDefaultAsync(p => p.ProgramaId == programaId, ct);

    public async Task<PresupuestoPrograma?> ObtenerPorReservaIdAsync(Guid reservaId, CancellationToken ct = default)
    {
        var presupuestoId = await _db.ReservasPresupuesto
            .AsNoTracking()
            .Where(r => r.Id == reservaId)
            .Select(r => (Guid?)r.PresupuestoProgramaId)
            .FirstOrDefaultAsync(ct);

        if (!presupuestoId.HasValue)
        {
            return null;
        }

        return await _db.PresupuestosPrograma
            .Include(p => p.CuposCiclo)
            .Include(p => p.Reservas)
            .FirstOrDefaultAsync(p => p.Id == presupuestoId.Value, ct);
    }

    public async Task<PresupuestoPrograma?> ObtenerPorOperationIdAsync(Guid operationId, CancellationToken ct = default)
    {
        var presupuestoId = await _db.ReservasPresupuesto
            .AsNoTracking()
            .Where(r => r.OperationId == operationId)
            .Select(r => (Guid?)r.PresupuestoProgramaId)
            .FirstOrDefaultAsync(ct);

        if (!presupuestoId.HasValue)
        {
            return null;
        }

        return await _db.PresupuestosPrograma
            .Include(p => p.CuposCiclo)
            .Include(p => p.Reservas)
            .FirstOrDefaultAsync(p => p.Id == presupuestoId.Value, ct);
    }

    public async Task<IReadOnlyList<ReservaPresupuesto>> ListarReservasSinVinculoAnterioresAAsync(
        DateTimeOffset limiteUtc,
        CancellationToken ct = default)
    {
        // Filtrar en SQL por estado RESERVADA y evaluar DateTimeOffset en memoria para compatibilidad Npgsql + SQLite.
        var pendientes = await _db.ReservasPresupuesto
            .AsNoTracking()
            .Where(r => r.Estado == EstadoReservaPresupuesto.RESERVADA)
            .ToListAsync(ct);

        return pendientes
            .Where(r => r.CreadaEnUtc <= limiteUtc)
            .OrderBy(r => r.CreadaEnUtc)
            .ToArray();
    }

    public async Task AgregarAsync(PresupuestoPrograma presupuesto, CancellationToken ct = default)
    {
        await _db.PresupuestosPrograma.AddAsync(presupuesto, ct);
    }
}

public sealed class EjecutorConcurrenciaPresupuestoEfCore : IEjecutorConcurrenciaPresupuesto
{
    private const int MaxIntentos = 5;
    private readonly CatalogoFondosDbContext _db;

    public EjecutorConcurrenciaPresupuestoEfCore(CatalogoFondosDbContext db) => _db = db;

    public async Task<TResultado> EjecutarConReintentoAsync<TResultado>(
        Func<CancellationToken, Task<TResultado>> accion,
        CancellationToken ct = default)
    {
        for (var intento = 1; intento <= MaxIntentos; intento++)
        {
            try
            {
                return await accion(ct);
            }
            catch (DbUpdateConcurrencyException) when (intento < MaxIntentos)
            {
                _db.ChangeTracker.Clear();
            }
        }

        throw new ExcepcionDeDominio(
            "El presupuesto del programa fue modificado simultáneamente por otra operación. Intenta nuevamente.",
            "CONCURRENCIA_PRESUPUESTO");
    }
}

public sealed class SincronizadorPresupuestoProgramaEfCore : ISincronizadorPresupuestoPrograma
{
    public const string ConsumidorAporte = "S2.PresupuestoPrograma.Aporte";

    private readonly CatalogoFondosDbContext _db;
    private readonly IPresupuestoProgramaRepository _presupuestoRepo;
    private readonly IProgramaRepository _programaRepo;
    private readonly IEjecutorConcurrenciaPresupuesto _ejecutor;

    public SincronizadorPresupuestoProgramaEfCore(
        CatalogoFondosDbContext db,
        IPresupuestoProgramaRepository presupuestoRepo,
        IProgramaRepository programaRepo,
        IEjecutorConcurrenciaPresupuesto ejecutor)
    {
        _db = db;
        _presupuestoRepo = presupuestoRepo;
        _programaRepo = programaRepo;
        _ejecutor = ejecutor;
    }

    public async Task InicializarParaProgramaAsync(
        Programa programa,
        DateTimeOffset ahoraUtc,
        CancellationToken ct = default)
    {
        var existente = await _presupuestoRepo.ObtenerPorProgramaIdAsync(programa.Id, ct);
        if (existente is not null)
        {
            return;
        }

        var presupuesto = PresupuestoPrograma.Crear(
            programa.Id,
            programa.CupoBecarios,
            programa.MontoPorPeriodo.Moneda,
            ahoraUtc);

        await _presupuestoRepo.AgregarAsync(presupuesto, ct);
        await _db.SaveChangesAsync(ct);
    }

    public Task AplicarAporteIdempotenteAsync(
        Guid aporteId,
        Guid programaId,
        Dinero monto,
        DateTimeOffset ahoraUtc,
        CancellationToken ct = default) =>
        _ejecutor.EjecutarConReintentoAsync(async token =>
        {
            var yaAplicado = await _db.InboxMessages
                .AsNoTracking()
                .AnyAsync(m => m.ConsumerName == ConsumidorAporte && m.EventId == aporteId, token);

            if (yaAplicado)
            {
                return true;
            }

            var presupuesto = await _presupuestoRepo.ObtenerPorProgramaIdAsync(programaId, token);
            if (presupuesto is null)
            {
                var programa = await _programaRepo.ObtenerPorIdAsync(programaId, token);
                if (programa is null)
                {
                    return false;
                }

                presupuesto = PresupuestoPrograma.Crear(
                    programa.Id,
                    programa.CupoBecarios,
                    programa.MontoPorPeriodo.Moneda,
                    ahoraUtc);
                await _presupuestoRepo.AgregarAsync(presupuesto, token);
            }

            presupuesto.RegistrarAporte(monto, ahoraUtc);
            await _db.InboxMessages.AddAsync(InboxMessage.Crear(ConsumidorAporte, aporteId, "AporteRegistrado.v1", ahoraUtc), token);
            await _db.SaveChangesAsync(token);
            return true;
        }, ct);
}
