using Ascenso.Messaging.Outbox;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Ascenso.S2.CatalogoFondos.Infrastructure.Persistencia;

/// <summary>
/// DbContext exclusivo de S2 Catálogo y Fondos (schema aislado s2_catalogo_fondos en PostgreSQL, ADR-05).
/// Incluye Outbox/Inbox y RegistroDecision append-only (ADR-12, ADR-13, D-6).
/// </summary>
public sealed class CatalogoFondosDbContext : DbContext
{
    public const string Schema = "s2_catalogo_fondos";

    public DbSet<CalendarioAcademico> CalendariosAcademicos => Set<CalendarioAcademico>();
    public DbSet<PeriodoAcademico> PeriodosAcademicos => Set<PeriodoAcademico>();
    public DbSet<InicioInscripcionesPeriodo> IniciosInscripcionesPeriodo => Set<InicioInscripcionesPeriodo>();
    public DbSet<OperacionIdempotenteCatalogo> OperacionesIdempotentes => Set<OperacionIdempotenteCatalogo>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public CatalogoFondosDbContext(DbContextOptions<CatalogoFondosDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
        {
            modelBuilder.HasDefaultSchema(Schema);
        }

        modelBuilder.ConfigurarOutboxEInbox();
        modelBuilder.ConfigurarRegistroDecisiones();

        modelBuilder.Entity<CalendarioAcademico>(builder =>
        {
            builder.ToTable("calendarios_academicos");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.ConfigurarVersionExplicita<CalendarioAcademico, Guid>();

            builder.HasMany(x => x.Periodos)
                .WithOne()
                .HasForeignKey(p => p.CalendarioAcademicoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.Periodos)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<PeriodoAcademico>(builder =>
        {
            builder.ToTable("periodos_academicos");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.CalendarioAcademicoId).HasColumnName("calendario_academico_id").IsRequired();

            builder.Property(x => x.Codigo)
                .HasColumnName("codigo")
                .HasConversion(c => c.Valor, s => new CodigoPeriodo(s))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Estado)
                .HasColumnName("estado")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.EsActual).HasColumnName("es_actual").IsRequired();
            builder.Property(x => x.RegistradoEnUtc).HasColumnName("registrado_en_utc").IsRequired();

            builder.HasIndex(x => new { x.CalendarioAcademicoId, x.Codigo })
                .IsUnique()
                .HasDatabaseName("ix_periodos_academicos_codigo_unico");
        });

        modelBuilder.Entity<InicioInscripcionesPeriodo>(builder =>
        {
            builder.ToTable("inicios_inscripciones_periodo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.UniversidadId).HasColumnName("universidad_id").IsRequired();
            builder.Property(x => x.NombreUniversidad).HasColumnName("nombre_universidad").HasMaxLength(200).IsRequired();

            builder.Property(x => x.CodigoPeriodo)
                .HasColumnName("codigo_periodo")
                .HasConversion(c => c.Valor, s => new CodigoPeriodo(s))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").IsRequired();
            builder.Property(x => x.RegistradoEnUtc).HasColumnName("registrado_en_utc").IsRequired();
            builder.ConfigurarVersionExplicita<InicioInscripcionesPeriodo, Guid>();

            builder.HasIndex(x => new { x.UniversidadId, x.CodigoPeriodo })
                .IsUnique()
                .HasDatabaseName("ix_inicios_inscripciones_universidad_periodo_unico");
        });

        modelBuilder.Entity<OperacionIdempotenteCatalogo>(builder =>
        {
            builder.ToTable("operaciones_idempotentes_catalogo");
            builder.HasKey(x => x.OperationId);
            builder.Property(x => x.OperationId).HasColumnName("operation_id").ValueGeneratedNever();
            builder.Property(x => x.ResultadoId).HasColumnName("resultado_id").IsRequired();
            builder.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(40).IsRequired();
            builder.Property(x => x.CorrelationId).HasColumnName("correlation_id").IsRequired();
            builder.Property(x => x.Version).HasColumnName("version").IsRequired();
            builder.Property(x => x.ProcesadoEnUtc).HasColumnName("procesado_en_utc").IsRequired();
        });
    }
}
