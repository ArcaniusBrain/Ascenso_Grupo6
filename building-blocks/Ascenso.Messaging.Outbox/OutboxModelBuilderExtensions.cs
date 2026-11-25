using Ascenso.SharedKernel.TiposBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ascenso.Messaging.Outbox;

public static class OutboxModelBuilderExtensions
{
    /// <summary>
    /// Registra las tablas técnicas outbox_messages e inbox_messages dentro del schema propio del servicio.
    /// </summary>
    public static ModelBuilder ConfigurarOutboxEInbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("outbox_messages");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.EventType).HasMaxLength(200).IsRequired();
            builder.Property(x => x.SchemaVersion).IsRequired();
            builder.Property(x => x.PayloadJson).IsRequired();
            builder.Property(x => x.CorrelationId).IsRequired();
            builder.Property(x => x.CausationId).IsRequired();
            builder.Property(x => x.OccurredAtUtc).IsRequired();
            builder.Property(x => x.LastError).HasMaxLength(2000);

            builder.HasIndex(x => new { x.ProcessedAtUtc, x.OccurredAtUtc })
                .HasDatabaseName("ix_outbox_pendientes");
        });

        modelBuilder.Entity<InboxMessage>(builder =>
        {
            builder.ToTable("inbox_messages");
            builder.HasKey(x => new { x.ConsumerName, x.EventId });

            builder.Property(x => x.ConsumerName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.EventType).HasMaxLength(200).IsRequired();
            builder.Property(x => x.ProcessedAtUtc).IsRequired();
        });

        return modelBuilder;
    }

    /// <summary>
    /// Configura Version como token de concurrencia explícito en cualquier raíz de agregado (DDD-07, ADR-16, ADR-25).
    /// </summary>
    public static EntityTypeBuilder<TAgregado> ConfigurarVersionExplicita<TAgregado, TId>(
        this EntityTypeBuilder<TAgregado> builder)
        where TAgregado : RaizDeAgregado<TId>
        where TId : notnull
    {
        builder.Property(x => x.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .IsRequired();

        return builder;
    }
}
