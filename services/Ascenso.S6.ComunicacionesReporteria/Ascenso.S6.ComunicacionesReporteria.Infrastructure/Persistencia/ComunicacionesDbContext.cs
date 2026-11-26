using Ascenso.Messaging.Outbox;
using Ascenso.S6.ComunicacionesReporteria.Domain.Agregados;
using Ascenso.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Ascenso.S6.ComunicacionesReporteria.Infrastructure.Persistencia;

/// <summary>
/// DbContext exclusivo de S6 Comunicaciones y Reportería (schema aislado s6_comunicaciones en PostgreSQL, ADR-05).
/// Incluye Outbox/Inbox y RegistroDecision append-only (ADR-12, ADR-13, D-6).
/// </summary>
public sealed class ComunicacionesDbContext : DbContext
{
    public const string Schema = "s6_comunicaciones";

    public DbSet<ConsentimientoDonante> ConsentimientosDonante => Set<ConsentimientoDonante>();
    public DbSet<SolicitudReporteImpacto> SolicitudesReporteImpacto => Set<SolicitudReporteImpacto>();
    public DbSet<PeriodoCalendarioProyeccion> ProyeccionPeriodos => Set<PeriodoCalendarioProyeccion>();
    public DbSet<AporteImpactoProyeccion> ProyeccionAportes => Set<AporteImpactoProyeccion>();
    public DbSet<BecarioImpactoProyeccion> ProyeccionBecarios => Set<BecarioImpactoProyeccion>();
    public DbSet<DesembolsoImpactoProyeccion> ProyeccionDesembolsos => Set<DesembolsoImpactoProyeccion>();
    public DbSet<ReporteAcademicoImpactoProyeccion> ProyeccionReportesAcademicos => Set<ReporteAcademicoImpactoProyeccion>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public ComunicacionesDbContext(DbContextOptions<ComunicacionesDbContext> options) : base(options)
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

        modelBuilder.Entity<ConsentimientoDonante>(builder =>
        {
            builder.ToTable("consentimientos_donante");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.BecarioId).HasColumnName("becario_id").IsRequired();
            builder.Property(x => x.DonanteId).HasColumnName("donante_id").IsRequired();
            builder.Property(x => x.AceptadoEnUtc).HasColumnName("aceptado_en_utc").IsRequired();
            builder.ConfigurarVersionExplicita<ConsentimientoDonante, Guid>();

            // R36/A6 y HU-33.2 E4: un único consentimiento irrevocable por (becario, donante).
            builder.HasIndex(x => new { x.BecarioId, x.DonanteId })
                .IsUnique()
                .HasDatabaseName("ix_consentimientos_donante_becario_donante_unico");
        });

        modelBuilder.Entity<SolicitudReporteImpacto>(builder =>
        {
            builder.ToTable("solicitudes_reporte_impacto");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.DonanteId).HasColumnName("donante_id").IsRequired();
            builder.Property(x => x.NombreDonante).HasColumnName("nombre_donante").HasMaxLength(200).IsRequired();

            builder.Property(x => x.Periodo)
                .HasColumnName("codigo_periodo")
                .HasConversion(p => p.Valor, s => new CodigoPeriodo(s))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.SolicitadoPorUsuarioId).HasColumnName("solicitado_por_usuario_id").IsRequired();
            builder.Property(x => x.RolSolicitante).HasColumnName("rol_solicitante").HasMaxLength(50).IsRequired();
            builder.Property(x => x.ProgramasAlcanceCsv).HasColumnName("programas_alcance_csv").HasMaxLength(2000);

            builder.Property(x => x.Estado)
                .HasColumnName("estado")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.MensajeEstado).HasColumnName("mensaje_estado").HasMaxLength(500);
            builder.Property(x => x.SolicitadoEnUtc).HasColumnName("solicitado_en_utc").IsRequired();
            builder.Property(x => x.CompletadoEnUtc).HasColumnName("completado_en_utc");
            builder.Property(x => x.ContenidoJson).HasColumnName("contenido_json");
            builder.ConfigurarVersionExplicita<SolicitudReporteImpacto, Guid>();

            builder.HasIndex(x => new { x.DonanteId, x.Periodo })
                .HasDatabaseName("ix_solicitudes_reporte_impacto_donante_periodo");
        });

        modelBuilder.Entity<PeriodoCalendarioProyeccion>(builder =>
        {
            builder.ToTable("proyeccion_periodos_calendario");
            builder.HasKey(x => x.CodigoPeriodo);
            builder.Property(x => x.CodigoPeriodo).HasColumnName("codigo_periodo").HasMaxLength(20);
        });

        modelBuilder.Entity<AporteImpactoProyeccion>(builder =>
        {
            builder.ToTable("proyeccion_aportes_impacto");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.DonanteId).HasColumnName("donante_id").IsRequired();
            builder.Property(x => x.NombreDonante).HasColumnName("nombre_donante").HasMaxLength(200).IsRequired();
            builder.Property(x => x.ProgramaId).HasColumnName("programa_id").IsRequired();
            builder.Property(x => x.NombrePrograma).HasColumnName("nombre_programa").HasMaxLength(200).IsRequired();
            builder.Property(x => x.CodigoPeriodo).HasColumnName("codigo_periodo").HasMaxLength(20).IsRequired();
            builder.Property(x => x.MontoAportado).HasColumnName("monto_aportado").HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.Moneda).HasColumnName("moneda").HasMaxLength(3).IsRequired();

            builder.HasIndex(x => new { x.DonanteId, x.CodigoPeriodo })
                .HasDatabaseName("ix_proyeccion_aportes_donante_periodo");
        });

        modelBuilder.Entity<BecarioImpactoProyeccion>(builder =>
        {
            builder.ToTable("proyeccion_becarios_impacto");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.BecarioId).HasColumnName("becario_id").IsRequired();
            builder.Property(x => x.ProgramaId).HasColumnName("programa_id").IsRequired();
            builder.Property(x => x.NombreCompleto).HasColumnName("nombre_completo").HasMaxLength(200).IsRequired();
            builder.Property(x => x.Cedula).HasColumnName("cedula").HasMaxLength(40).IsRequired();
            builder.Property(x => x.Universidad).HasColumnName("universidad").HasMaxLength(200).IsRequired();
            builder.Property(x => x.Carrera).HasColumnName("carrera").HasMaxLength(200).IsRequired();
            builder.Property(x => x.IngresoFamiliarSensible).HasColumnName("ingreso_familiar_sensible").HasPrecision(18, 2);
            builder.Property(x => x.ComposicionFamiliarSensible).HasColumnName("composicion_familiar_sensible").HasMaxLength(500);

            builder.HasIndex(x => x.ProgramaId).HasDatabaseName("ix_proyeccion_becarios_programa");
        });

        modelBuilder.Entity<DesembolsoImpactoProyeccion>(builder =>
        {
            builder.ToTable("proyeccion_desembolsos_impacto");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.ProgramaId).HasColumnName("programa_id").IsRequired();
            builder.Property(x => x.CodigoPeriodo).HasColumnName("codigo_periodo").HasMaxLength(20).IsRequired();
            builder.Property(x => x.Monto).HasColumnName("monto").HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.Moneda).HasColumnName("moneda").HasMaxLength(3).IsRequired();
            builder.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>().HasMaxLength(20).IsRequired();

            builder.HasIndex(x => new { x.ProgramaId, x.CodigoPeriodo })
                .HasDatabaseName("ix_proyeccion_desembolsos_programa_periodo");
        });

        modelBuilder.Entity<ReporteAcademicoImpactoProyeccion>(builder =>
        {
            builder.ToTable("proyeccion_reportes_academicos_impacto");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.BecarioId).HasColumnName("becario_id").IsRequired();
            builder.Property(x => x.ProgramaId).HasColumnName("programa_id").IsRequired();
            builder.Property(x => x.CodigoPeriodo).HasColumnName("codigo_periodo").HasMaxLength(20).IsRequired();
            builder.Property(x => x.Promedio).HasColumnName("promedio").HasPrecision(6, 2).IsRequired();
            builder.Property(x => x.MateriasAprobadas).HasColumnName("materias_aprobadas").IsRequired();
            builder.Property(x => x.MateriasReprobadas).HasColumnName("materias_reprobadas").IsRequired();
            builder.Property(x => x.Origen).HasColumnName("origen").HasConversion<string>().HasMaxLength(20).IsRequired();

            builder.HasIndex(x => new { x.ProgramaId, x.CodigoPeriodo })
                .HasDatabaseName("ix_proyeccion_academicos_programa_periodo");
        });
    }
}
