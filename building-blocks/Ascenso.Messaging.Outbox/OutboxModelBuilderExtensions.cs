using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.Trazabilidad;
using Ascenso.SharedKernel.ValueObjects;
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
    /// Registra la tabla append-only registro_decisiones dentro del schema propio del servicio (RF-35, D-6, ADR-12).
    /// Debe combinarse con AppendOnlyRegistroDecisionInterceptor y RegistroDecisionSqlAppendOnly en las migraciones.
    /// </summary>
    public static ModelBuilder ConfigurarRegistroDecisiones(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RegistroDecision>(builder =>
        {
            builder.ToTable("registro_decisiones");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();

            builder.Property(x => x.Tipo)
                .HasColumnName("tipo_decision")
                .HasConversion<string>()
                .HasMaxLength(80)
                .IsRequired();

            builder.OwnsOne(x => x.Sujeto, sujeto =>
            {
                sujeto.Property(s => s.Tipo)
                    .HasColumnName("tipo_sujeto")
                    .HasConversion<string>()
                    .HasMaxLength(40)
                    .IsRequired();

                sujeto.Property(s => s.Id)
                    .HasColumnName("sujeto_id")
                    .HasMaxLength(120)
                    .IsRequired();

                sujeto.HasIndex(s => new { s.Tipo, s.Id })
                    .HasDatabaseName("ix_registro_decisiones_sujeto");
            });

            builder.Navigation(x => x.Sujeto).IsRequired();

            builder.OwnsOne(x => x.Actor, actor =>
            {
                actor.Property(a => a.UsuarioId)
                    .HasColumnName("actor_usuario_id");

                actor.Property(a => a.Rol)
                    .HasColumnName("actor_rol")
                    .HasMaxLength(80)
                    .IsRequired();

                actor.Property(a => a.EsSistema)
                    .HasColumnName("actor_es_sistema")
                    .IsRequired();
            });

            builder.Navigation(x => x.Actor).IsRequired();

            builder.Property(x => x.ProgramaId).HasColumnName("programa_id");
            builder.Property(x => x.PropietarioUsuarioId).HasColumnName("propietario_usuario_id");

            builder.Property(x => x.OcurridoEnUtc)
                .HasColumnName("ocurrido_en_utc")
                .IsRequired();

            builder.Property(x => x.Justificacion)
                .HasColumnName("justificacion")
                .HasConversion(
                    j => j.Texto,
                    texto => new Justificacion(texto))
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(x => x.EstadoAnterior)
                .HasColumnName("estado_anterior")
                .HasMaxLength(60);

            builder.Property(x => x.EstadoNuevo)
                .HasColumnName("estado_nuevo")
                .HasMaxLength(60);

            builder.Property(x => x.DatosJson)
                .HasColumnName("datos_json");

            builder.Property(x => x.ServicioOrigen)
                .HasColumnName("servicio_origen")
                .HasMaxLength(40)
                .IsRequired();

            builder.Property(x => x.CorrelationId)
                .HasColumnName("correlation_id")
                .IsRequired();

            builder.HasIndex(x => x.OcurridoEnUtc)
                .HasDatabaseName("ix_registro_decisiones_fecha");

            builder.HasIndex(x => x.ProgramaId)
                .HasDatabaseName("ix_registro_decisiones_programa");

            builder.HasIndex(x => x.PropietarioUsuarioId)
                .HasDatabaseName("ix_registro_decisiones_propietario");

            builder.HasIndex(x => x.CorrelationId)
                .HasDatabaseName("ix_registro_decisiones_correlacion");
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
