using System.Text.Json;
using Ascenso.Contracts.Eventos;
using Ascenso.S6.ComunicacionesReporteria.Domain.Modelos;
using Ascenso.S6.ComunicacionesReporteria.Infrastructure.Mensajeria;
using Ascenso.S6.ComunicacionesReporteria.Infrastructure.Persistencia;
using Ascenso.SharedKernel.Tiempo;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ascenso.S6.ComunicacionesReporteria.IntegrationTests;

public sealed class ProyectorEventosImpactoTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task ProyectorEventosImpacto_ConsumeEventosS2YS4_ConIdempotenciaInboxEstricta()
    {
        SQLitePCL.Batteries_V2.Init();
        var dbPath = Path.Combine(Path.GetTempPath(), $"ascenso_s6_inbox_{Guid.NewGuid():N}.db");

        try
        {
            var options = new DbContextOptionsBuilder<ComunicacionesDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            await using var db = new ComunicacionesDbContext(options);
            await db.Database.EnsureCreatedAsync();

            var reloj = new RelojFijo(new DateTimeOffset(2026, 11, 26, 18, 0, 0, TimeSpan.Zero));
            var proyector = new ProyectorEventosImpacto(db, reloj);

            var donanteId = Guid.NewGuid();
            var programaId = Guid.NewGuid();
            var aporteId = Guid.NewGuid();
            var pagoId = Guid.NewGuid();

            // 1. Evento PeriodoMarcadoActual.v1 (desde S2)
            var evPeriodoId = Guid.NewGuid();
            var payloadPeriodo = JsonSerializer.Serialize(
                new PeriodoMarcadoActualV1(Guid.NewGuid(), "2026-2", "2026-1"),
                JsonOptions);

            var rPeriodo = await proyector.ProcesarEventoAsync(
                evPeriodoId,
                ProyectorEventosImpacto.EventoPeriodoMarcadoActual,
                payloadPeriodo);
            Assert.Equal(ResultadoConsumoEvento.PROCESADO, rPeriodo);

            // 2. Evento AporteRegistrado.v1 (desde S2)
            var evAporteId = Guid.NewGuid();
            var payloadAporte = JsonSerializer.Serialize(
                new AporteRegistradoV1(
                    aporteId,
                    donanteId,
                    "Fundación Horizonte",
                    programaId,
                    "2026-2",
                    15000m,
                    "USD",
                    reloj.AhoraUtc),
                JsonOptions);

            var rAporte1 = await proyector.ProcesarEventoAsync(
                evAporteId,
                ProyectorEventosImpacto.EventoAporteRegistrado,
                payloadAporte);
            Assert.Equal(ResultadoConsumoEvento.PROCESADO, rAporte1);

            // 3. Reentrega duplicada del mismo EventId -> Inbox debe omitir sin duplicar
            var rAporteDup = await proyector.ProcesarEventoAsync(
                evAporteId,
                ProyectorEventosImpacto.EventoAporteRegistrado,
                payloadAporte);
            Assert.Equal(ResultadoConsumoEvento.DUPLICADO_OMITIDO, rAporteDup);

            // 4. Evento PagoConfirmado.v1 (desde S4)
            var evPagoId = Guid.NewGuid();
            var payloadPago = JsonSerializer.Serialize(
                new PagoConfirmadoV1(
                    Guid.NewGuid(),
                    programaId,
                    Guid.NewGuid(),
                    pagoId,
                    2000m,
                    "USD",
                    "UCAB",
                    new DateOnly(2026, 10, 15)),
                JsonOptions);

            var rPago = await proyector.ProcesarEventoAsync(
                evPagoId,
                ProyectorEventosImpacto.EventoPagoConfirmado,
                payloadPago);
            Assert.Equal(ResultadoConsumoEvento.PROCESADO, rPago);

            // Verificaciones sobre las proyecciones CQRS e Inbox persistidos
            var periodos = await db.ProyeccionPeriodos.Select(p => p.CodigoPeriodo).ToListAsync();
            Assert.Contains("2026-2", periodos);
            Assert.Contains("2026-1", periodos);

            var aportes = await db.ProyeccionAportes.ToListAsync();
            Assert.Single(aportes);
            Assert.Equal(15000m, aportes[0].MontoAportado);
            Assert.Equal("Fundación Horizonte", aportes[0].NombreDonante);

            var desembolsos = await db.ProyeccionDesembolsos.ToListAsync();
            Assert.Single(desembolsos);
            Assert.Equal(EstadoDesembolsoProyectado.CONFIRMADO, desembolsos[0].Estado);
            Assert.Equal(2000m, desembolsos[0].Monto);
            Assert.Equal("2026-2", desembolsos[0].CodigoPeriodo);

            var inboxCount = await db.InboxMessages.CountAsync();
            Assert.Equal(3, inboxCount);
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath))
            {
                try { File.Delete(dbPath); } catch { }
            }
        }
    }

    private sealed class RelojFijo(DateTimeOffset ahoraUtc) : IRelojNegocio
    {
        public DateTimeOffset AhoraUtc => ahoraUtc;
        public DateTimeOffset AhoraCaracas => ahoraUtc.ToOffset(TimeSpan.FromHours(-4));
        public DateOnly HoyCaracas => DateOnly.FromDateTime(AhoraCaracas.DateTime);
    }
}
