using Ascenso.Messaging.Outbox;
using Ascenso.Messaging.Outbox.RabbitMq;
using Ascenso.SharedKernel.Tiempo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Ascenso.BuildingBlocks.Tests;

public sealed class OutboxDispatcherTests
{
    [Fact]
    public async Task ProcesarLotePendiente_PublicaEnOrdenMarcaProcesadoYNoRepublica()
    {
        var reloj = new RelojFijoPruebas(new DateTimeOffset(2026, 11, 26, 17, 0, 0, TimeSpan.Zero));
        var options = new DbContextOptionsBuilder<OutboxPruebaDbContext>()
            .UseInMemoryDatabase($"outbox-ok-{Guid.NewGuid():N}")
            .Options;

        await using var db = new OutboxPruebaDbContext(options);

        var primero = OutboxMessage.Crear(
            Guid.NewGuid(),
            "PeriodoMarcadoActual.v1",
            1,
            """{"codigoPeriodo":"2026-1"}""",
            Guid.NewGuid(),
            Guid.NewGuid(),
            reloj.AhoraUtc.AddMinutes(-5));

        var segundo = OutboxMessage.Crear(
            Guid.NewGuid(),
            "AporteRegistrado.v1",
            1,
            """{"monto":10000}""",
            Guid.NewGuid(),
            Guid.NewGuid(),
            reloj.AhoraUtc.AddMinutes(-1));

        db.OutboxMessages.AddRange(segundo, primero);
        await db.SaveChangesAsync();

        var publicador = new PublicadorEnMemoria();
        var procesador = new ProcesadorOutboxLote<OutboxPruebaDbContext>(
            db,
            publicador,
            reloj,
            NullLogger<ProcesadorOutboxLote<OutboxPruebaDbContext>>.Instance);

        var r1 = await procesador.ProcesarLotePendienteAsync("s2-catalogo-fondos", batchSize: 10);
        Assert.Equal(2, r1.Publicados);
        Assert.Equal(0, r1.Fallidos);
        Assert.Equal(2, publicador.Publicados.Count);
        Assert.Equal("PeriodoMarcadoActual.v1", publicador.Publicados[0].EventType);
        Assert.Equal("AporteRegistrado.v1", publicador.Publicados[1].EventType);
        Assert.Equal("s2-catalogo-fondos", publicador.Publicados[0].Producer);

        // Segunda corrida no republica mensajes ya marcados con ProcessedAtUtc
        var r2 = await procesador.ProcesarLotePendienteAsync("s2-catalogo-fondos", batchSize: 10);
        Assert.Equal(0, r2.Publicados);
        Assert.Equal(2, publicador.Publicados.Count);
    }

    [Fact]
    public async Task ProcesarLotePendiente_AnteFalloDelBrokerRegistraIntentoYMantienePendiente()
    {
        var reloj = new RelojFijoPruebas(new DateTimeOffset(2026, 11, 26, 17, 0, 0, TimeSpan.Zero));
        var options = new DbContextOptionsBuilder<OutboxPruebaDbContext>()
            .UseInMemoryDatabase($"outbox-fail-{Guid.NewGuid():N}")
            .Options;

        await using var db = new OutboxPruebaDbContext(options);

        var mensaje = OutboxMessage.Crear(
            Guid.NewGuid(),
            "CupoLiberado.v1",
            1,
            """{"montoLiberado":2000}""",
            Guid.NewGuid(),
            Guid.NewGuid(),
            reloj.AhoraUtc);

        db.OutboxMessages.Add(mensaje);
        await db.SaveChangesAsync();

        var publicadorFallido = new PublicadorEnMemoria { DebeFallar = true };
        var procesador = new ProcesadorOutboxLote<OutboxPruebaDbContext>(
            db,
            publicadorFallido,
            reloj,
            NullLogger<ProcesadorOutboxLote<OutboxPruebaDbContext>>.Instance);

        var r = await procesador.ProcesarLotePendienteAsync("s2-catalogo-fondos");
        Assert.Equal(0, r.Publicados);
        Assert.Equal(1, r.Fallidos);

        var persistido = await db.OutboxMessages.SingleAsync();
        Assert.Null(persistido.ProcessedAtUtc);
        Assert.Equal(1, persistido.Attempts);
        Assert.Contains("Broker temporalmente caído", persistido.LastError);

        // Al recuperarse el broker, el mismo mensaje se despacha exitosamente
        publicadorFallido.DebeFallar = false;
        var rRecup = await procesador.ProcesarLotePendienteAsync("s2-catalogo-fondos");
        Assert.Equal(1, rRecup.Publicados);
        Assert.Equal(0, rRecup.Fallidos);
        Assert.NotNull((await db.OutboxMessages.SingleAsync()).ProcessedAtUtc);
    }

    private sealed class OutboxPruebaDbContext : DbContext
    {
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public OutboxPruebaDbContext(DbContextOptions<OutboxPruebaDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigurarOutboxEInbox();
        }
    }

    private sealed class PublicadorEnMemoria : IPublicadorEventosIntegracion
    {
        public bool DebeFallar { get; set; }
        public List<EventoIntegracionMensaje> Publicados { get; } = [];

        public Task PublicarAsync(EventoIntegracionMensaje mensaje, CancellationToken ct = default)
        {
            if (DebeFallar)
            {
                throw new InvalidOperationException("Broker temporalmente caído");
            }

            Publicados.Add(mensaje);
            return Task.CompletedTask;
        }
    }

    private sealed class RelojFijoPruebas(DateTimeOffset ahoraUtc) : IRelojNegocio
    {
        public DateTimeOffset AhoraUtc => ahoraUtc;
        public DateTimeOffset AhoraCaracas => ahoraUtc.ToOffset(TimeSpan.FromHours(-4));
        public DateOnly HoyCaracas => DateOnly.FromDateTime(AhoraCaracas.DateTime);
    }
}
