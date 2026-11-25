using Ascenso.Messaging.Outbox;
using Ascenso.SharedKernel.TiposBase;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ascenso.BuildingBlocks.Tests;

public sealed class OutboxModelTests
{
    private sealed class BecaSimulada : RaizDeAgregado<Guid>
    {
        public string Estado { get; private set; } = "ACTIVA";

        private BecaSimulada() { }

        public BecaSimulada(Guid id) : base(id) { }

        public void ConfirmarPagoSimulado()
        {
            // Simula un cambio en una entidad hija que incrementa la versión de la raíz (DDD-07 / ADR-25).
            IncrementarVersion();
        }

        public void RevocarSimulado()
        {
            Estado = "REVOCADA";
            IncrementarVersion();
        }
    }

    private sealed class PruebaDbContext(DbContextOptions<PruebaDbContext> options) : DbContext(options)
    {
        public DbSet<BecaSimulada> Becas => Set<BecaSimulada>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigurarOutboxEInbox();
            modelBuilder.Entity<BecaSimulada>(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.ConfigurarVersionExplicita<BecaSimulada, Guid>();
            });
        }
    }

    [Fact]
    public void ModeloOutboxEInbox_ConfiguraClaveUnicaCompuestaYTokenDeConcurrencia()
    {
        var options = new DbContextOptionsBuilder<PruebaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new PruebaDbContext(options);

        var inboxEntity = db.Model.FindEntityType(typeof(InboxMessage));
        Assert.NotNull(inboxEntity);
        var pkInbox = inboxEntity!.FindPrimaryKey();
        Assert.NotNull(pkInbox);
        Assert.Equal(["ConsumerName", "EventId"], pkInbox!.Properties.Select(p => p.Name).ToArray());

        var becaEntity = db.Model.FindEntityType(typeof(BecaSimulada));
        var versionProp = becaEntity!.FindProperty(nameof(RaizDeAgregado<Guid>.Version));
        Assert.NotNull(versionProp);
        Assert.True(versionProp!.IsConcurrencyToken);
    }

    [Fact]
    public async Task ConcurrenciaOptimista_ChocaCuandoDosOperacionesModificanLaMismaRaiz()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<PruebaDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var becaId = Guid.NewGuid();
        await using (var seed = new PruebaDbContext(options))
        {
            seed.Becas.Add(new BecaSimulada(becaId));
            await seed.SaveChangesAsync();
        }

        await using var ctxPago = new PruebaDbContext(options);
        await using var ctxRevocacion = new PruebaDbContext(options);

        var becaEnPago = await ctxPago.Becas.SingleAsync(b => b.Id == becaId);
        var becaEnRevocacion = await ctxRevocacion.Becas.SingleAsync(b => b.Id == becaId);

        becaEnPago.ConfirmarPagoSimulado();
        await ctxPago.SaveChangesAsync();

        becaEnRevocacion.RevocarSimulado();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => ctxRevocacion.SaveChangesAsync());
    }
}
