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
    public DbSet<UniversidadAliada> UniversidadesAliadas => Set<UniversidadAliada>();
    public DbSet<CarreraElegible> CarrerasElegibles => Set<CarreraElegible>();
    public DbSet<CostoMatricula> CostosMatricula => Set<CostoMatricula>();
    public DbSet<Programa> Programas => Set<Programa>();
    public DbSet<RestriccionElegibilidad> RestriccionesElegibilidad => Set<RestriccionElegibilidad>();
    public DbSet<AsociacionDonantePrograma> AsociacionesDonantesPrograma => Set<AsociacionDonantePrograma>();
    public DbSet<Donante> Donantes => Set<Donante>();
    public DbSet<Aporte> Aportes => Set<Aporte>();
    public DbSet<PresupuestoPrograma> PresupuestosPrograma => Set<PresupuestoPrograma>();
    public DbSet<CupoCiclo> CuposCiclo => Set<CupoCiclo>();
    public DbSet<ReservaPresupuesto> ReservasPresupuesto => Set<ReservaPresupuesto>();
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

        modelBuilder.Entity<UniversidadAliada>(builder =>
        {
            builder.ToTable("universidades_aliadas");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
            builder.Property(x => x.NombreNormalizado).HasColumnName("nombre_normalizado").HasMaxLength(200).IsRequired();
            builder.Property(x => x.RegistradaEnUtc).HasColumnName("registrada_en_utc").IsRequired();
            builder.ConfigurarVersionExplicita<UniversidadAliada, Guid>();

            builder.OwnsOne(x => x.Convenio, convenio =>
            {
                convenio.Property(c => c.Terminos)
                    .HasColumnName("convenio_terminos")
                    .HasMaxLength(4000)
                    .IsRequired();

                convenio.Property(c => c.Via)
                    .HasColumnName("convenio_via_datos_academicos")
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();
            });
            builder.Navigation(x => x.Convenio).IsRequired();

            builder.OwnsOne(x => x.ContactoDesignado, contacto =>
            {
                contacto.Property(c => c.NombreCompleto)
                    .HasColumnName("contacto_nombre")
                    .HasMaxLength(200)
                    .IsRequired();

                contacto.Property(c => c.Correo)
                    .HasColumnName("contacto_correo")
                    .HasMaxLength(200);

                contacto.Property(c => c.Telefono)
                    .HasColumnName("contacto_telefono")
                    .HasMaxLength(40);
            });
            builder.Navigation(x => x.ContactoDesignado).IsRequired();

            builder.HasMany(x => x.Carreras)
                .WithOne()
                .HasForeignKey(c => c.UniversidadAliadaId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Carreras).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.Costos)
                .WithOne()
                .HasForeignKey(c => c.UniversidadAliadaId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Costos).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => x.NombreNormalizado)
                .IsUnique()
                .HasDatabaseName("ix_universidades_aliadas_nombre_unico");
        });

        modelBuilder.Entity<CarreraElegible>(builder =>
        {
            builder.ToTable("carreras_elegibles");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.UniversidadAliadaId).HasColumnName("universidad_aliada_id").IsRequired();
            builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
            builder.Property(x => x.NombreNormalizado).HasColumnName("nombre_normalizado").HasMaxLength(200).IsRequired();

            builder.HasIndex(x => new { x.UniversidadAliadaId, x.NombreNormalizado })
                .IsUnique()
                .HasDatabaseName("ix_carreras_elegibles_universidad_nombre_unico");
        });

        modelBuilder.Entity<CostoMatricula>(builder =>
        {
            builder.ToTable("costos_matricula");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.UniversidadAliadaId).HasColumnName("universidad_aliada_id").IsRequired();

            builder.Property(x => x.Periodo)
                .HasColumnName("codigo_periodo")
                .HasConversion(p => p.Valor, s => new CodigoPeriodo(s))
                .HasMaxLength(20)
                .IsRequired();

            builder.OwnsOne(x => x.Costo, dinero =>
            {
                dinero.Property(d => d.Cantidad).HasColumnName("monto").HasPrecision(18, 2).IsRequired();
                dinero.Property(d => d.Moneda).HasColumnName("moneda").HasMaxLength(3).IsRequired();
            });
            builder.Navigation(x => x.Costo).IsRequired();

            builder.Property(x => x.VersionRegistro).HasColumnName("version_registro").IsRequired();
            builder.Property(x => x.RegistradoEnUtc).HasColumnName("registrado_en_utc").IsRequired();

            // Append-only (HU-14.2 Anexo A): SIN clave única sobre (universidad, periodo);
            // cada corrección en el mismo periodo incrementa version_registro conservando el historial.
            builder.HasIndex(x => new { x.UniversidadAliadaId, x.Periodo, x.VersionRegistro })
                .IsUnique()
                .HasDatabaseName("ix_costos_matricula_universidad_periodo_version");
        });

        modelBuilder.Entity<Programa>(builder =>
        {
            builder.ToTable("programas");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
            builder.Property(x => x.NombreNormalizado).HasColumnName("nombre_normalizado").HasMaxLength(200).IsRequired();

            builder.Property(x => x.PeriodoInicio)
                .HasColumnName("periodo_inicio")
                .HasConversion(p => p.Valor, s => new CodigoPeriodo(s))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.CupoBecarios).HasColumnName("cupo_becarios").IsRequired();
            builder.Property(x => x.DuracionPeriodos).HasColumnName("duracion_periodos").IsRequired();

            builder.OwnsOne(x => x.MontoPorPeriodo, dinero =>
            {
                dinero.Property(d => d.Cantidad).HasColumnName("monto_por_periodo").HasPrecision(18, 2).IsRequired();
                dinero.Property(d => d.Moneda).HasColumnName("moneda").HasMaxLength(3).IsRequired();
            });
            builder.Navigation(x => x.MontoPorPeriodo).IsRequired();

            builder.Property(x => x.ModalidadPago)
                .HasColumnName("modalidad_pago")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Alcance)
                .HasColumnName("alcance")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.PermiteAcumulacion).HasColumnName("permite_acumulacion").IsRequired();
            builder.Property(x => x.CreadoEnUtc).HasColumnName("creado_en_utc").IsRequired();
            builder.ConfigurarVersionExplicita<Programa, Guid>();

            builder.OwnsOne(x => x.Condiciones, cond =>
            {
                cond.Property(c => c.PromedioMinimo).HasColumnName("cond_promedio_minimo").HasPrecision(6, 2);
                cond.Property(c => c.ExigeHorasServicio).HasColumnName("cond_exige_horas_servicio");
                cond.Property(c => c.HorasServicio).HasColumnName("cond_horas_servicio");
                cond.Property(c => c.ReportesExigidosTexto).HasColumnName("cond_reportes_exigidos").HasMaxLength(2000);
                cond.Property(c => c.DuracionMaxPausaMeses).HasColumnName("cond_duracion_max_pausa_meses");
                cond.Property(c => c.DescripcionRenovacion).HasColumnName("cond_descripcion_renovacion").HasMaxLength(2000);
            });

            builder.HasMany(x => x.Donantes)
                .WithOne()
                .HasForeignKey(d => d.ProgramaId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Donantes).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.Restricciones)
                .WithOne()
                .HasForeignKey(r => r.ProgramaId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Restricciones).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => x.NombreNormalizado)
                .IsUnique()
                .HasDatabaseName("ix_programas_nombre_unico");
        });

        modelBuilder.Entity<AsociacionDonantePrograma>(builder =>
        {
            builder.ToTable("programa_donantes");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.ProgramaId).HasColumnName("programa_id").IsRequired();
            builder.Property(x => x.DonanteId).HasColumnName("donante_id").IsRequired();
            builder.Property(x => x.AsociadoEnUtc).HasColumnName("asociado_en_utc").IsRequired();

            builder.HasIndex(x => new { x.ProgramaId, x.DonanteId })
                .IsUnique()
                .HasDatabaseName("ix_programa_donantes_unico");
        });

        modelBuilder.Entity<RestriccionElegibilidad>(builder =>
        {
            builder.ToTable("programa_restricciones_elegibilidad");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.ProgramaId).HasColumnName("programa_id").IsRequired();
            builder.Property(x => x.UniversidadId).HasColumnName("universidad_id").IsRequired();
            builder.Property(x => x.NombreUniversidad).HasColumnName("nombre_universidad").HasMaxLength(200).IsRequired();
            builder.Property(x => x.CarreraNombre).HasColumnName("carrera_nombre").HasMaxLength(200);

            builder.HasIndex(x => new { x.ProgramaId, x.UniversidadId, x.CarreraNombre })
                .HasDatabaseName("ix_programa_restricciones_universidad_carrera");
        });

        modelBuilder.Entity<Donante>(builder =>
        {
            builder.ToTable("donantes");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
            builder.Property(x => x.NombreNormalizado).HasColumnName("nombre_normalizado").HasMaxLength(200).IsRequired();

            builder.Property(x => x.Tipo)
                .HasColumnName("tipo")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.OwnsOne(x => x.Contacto, contacto =>
            {
                contacto.Property(c => c.Correo)
                    .HasColumnName("contacto_correo")
                    .HasMaxLength(200);

                contacto.Property(c => c.Telefono)
                    .HasColumnName("contacto_telefono")
                    .HasMaxLength(40);
            });
            builder.Navigation(x => x.Contacto).IsRequired();

            builder.Property(x => x.RegistradoEnUtc).HasColumnName("registrado_en_utc").IsRequired();
            builder.ConfigurarVersionExplicita<Donante, Guid>();

            builder.HasMany(x => x.Aportes)
                .WithOne()
                .HasForeignKey(a => a.DonanteId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Aportes).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => x.NombreNormalizado)
                .IsUnique()
                .HasDatabaseName("ix_donantes_nombre_unico");
        });

        modelBuilder.Entity<Aporte>(builder =>
        {
            builder.ToTable("aportes");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.DonanteId).HasColumnName("donante_id").IsRequired();
            builder.Property(x => x.ProgramaId).HasColumnName("programa_id").IsRequired();

            builder.Property(x => x.Periodo)
                .HasColumnName("codigo_periodo")
                .HasConversion(p => p.Valor, s => new CodigoPeriodo(s))
                .HasMaxLength(20)
                .IsRequired();

            builder.OwnsOne(x => x.Monto, dinero =>
            {
                dinero.Property(d => d.Cantidad).HasColumnName("monto").HasPrecision(18, 2).IsRequired();
                dinero.Property(d => d.Moneda).HasColumnName("moneda").HasMaxLength(3).IsRequired();
            });
            builder.Navigation(x => x.Monto).IsRequired();

            builder.Property(x => x.RegistradoEnUtc).HasColumnName("registrado_en_utc").IsRequired();

            // Ledger append-only (HU-13.2): índices no únicos para consulta por donante/periodo y suma por programa.
            builder.HasIndex(x => new { x.DonanteId, x.Periodo })
                .HasDatabaseName("ix_aportes_donante_periodo");

            builder.HasIndex(x => x.ProgramaId)
                .HasDatabaseName("ix_aportes_programa");
        });

        modelBuilder.Entity<PresupuestoPrograma>(builder =>
        {
            builder.ToTable("presupuestos_programa");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.ProgramaId).HasColumnName("programa_id").IsRequired();
            builder.Property(x => x.Moneda).HasColumnName("moneda").HasMaxLength(3).IsRequired();
            builder.Property(x => x.Aportes).HasColumnName("aportes").HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.Otorgado).HasColumnName("otorgado").HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.OfertasPendientes).HasColumnName("ofertas_pendientes").HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.RenovacionesPendientes).HasColumnName("renovaciones_pendientes").HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.Desembolsado).HasColumnName("desembolsado").HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.CupoBecarios).HasColumnName("cupo_becarios").IsRequired();
            builder.Property(x => x.CuposOcupados).HasColumnName("cupos_ocupados").IsRequired();
            builder.Property(x => x.ActualizadoEnUtc).HasColumnName("actualizado_en_utc").IsRequired();
            builder.ConfigurarVersionExplicita<PresupuestoPrograma, Guid>();

            builder.HasMany(x => x.CuposCiclo)
                .WithOne()
                .HasForeignKey(c => c.PresupuestoProgramaId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.CuposCiclo).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.Reservas)
                .WithOne()
                .HasForeignKey(r => r.PresupuestoProgramaId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(x => x.Reservas).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => x.ProgramaId)
                .IsUnique()
                .HasDatabaseName("ix_presupuestos_programa_programa_unico");
        });

        modelBuilder.Entity<CupoCiclo>(builder =>
        {
            builder.ToTable("cupos_ciclo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.PresupuestoProgramaId).HasColumnName("presupuesto_programa_id").IsRequired();
            builder.Property(x => x.CicloId).HasColumnName("ciclo_id").IsRequired();
            builder.Property(x => x.Limite).HasColumnName("limite").IsRequired();
            builder.Property(x => x.Ocupados).HasColumnName("ocupados").IsRequired();

            builder.HasIndex(x => new { x.PresupuestoProgramaId, x.CicloId })
                .IsUnique()
                .HasDatabaseName("ix_cupos_ciclo_presupuesto_ciclo_unico");
        });

        modelBuilder.Entity<ReservaPresupuesto>(builder =>
        {
            builder.ToTable("reservas_presupuesto");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.PresupuestoProgramaId).HasColumnName("presupuesto_programa_id").IsRequired();

            builder.Property(x => x.Tipo)
                .HasColumnName("tipo")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Estado)
                .HasColumnName("estado")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.ClaveNegocio).HasColumnName("clave_negocio").HasMaxLength(120).IsRequired();
            builder.Property(x => x.OperationId).HasColumnName("operation_id").IsRequired();
            builder.Property(x => x.PostulacionId).HasColumnName("postulacion_id");
            builder.Property(x => x.CicloId).HasColumnName("ciclo_id");
            builder.Property(x => x.BecaId).HasColumnName("beca_id");
            builder.Property(x => x.CodigoPeriodo).HasColumnName("codigo_periodo").HasMaxLength(20).IsRequired();

            builder.OwnsOne(x => x.Monto, dinero =>
            {
                dinero.Property(d => d.Cantidad).HasColumnName("monto").HasPrecision(18, 2).IsRequired();
                dinero.Property(d => d.Moneda).HasColumnName("moneda").HasMaxLength(3).IsRequired();
            });
            builder.Navigation(x => x.Monto).IsRequired();

            builder.Property(x => x.DuracionPeriodos).HasColumnName("duracion_periodos").IsRequired();
            builder.Property(x => x.CreadaEnUtc).HasColumnName("creada_en_utc").IsRequired();
            builder.Property(x => x.VinculadaEnUtc).HasColumnName("vinculada_en_utc");
            builder.Property(x => x.CerradaEnUtc).HasColumnName("cerrada_en_utc");
            builder.Property(x => x.MotivoLiberacion)
                .HasColumnName("motivo_liberacion")
                .HasConversion<string>()
                .HasMaxLength(20);
            builder.Property(x => x.RecursoVinculadoId).HasColumnName("recurso_vinculado_id");

            builder.HasIndex(x => x.OperationId)
                .IsUnique()
                .HasDatabaseName("ix_reservas_presupuesto_operation_id_unico");

            builder.HasIndex(x => new { x.PresupuestoProgramaId, x.ClaveNegocio })
                .HasDatabaseName("ix_reservas_presupuesto_clave_negocio");

            builder.HasIndex(x => new { x.Estado, x.CreadaEnUtc })
                .HasDatabaseName("ix_reservas_presupuesto_estado_creada");
        });
    }
}
