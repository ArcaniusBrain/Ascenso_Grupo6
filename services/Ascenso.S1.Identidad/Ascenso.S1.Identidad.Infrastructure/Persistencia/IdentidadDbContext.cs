using System.Text.Json;
using Ascenso.Messaging.Outbox;
using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ascenso.S1.Identidad.Infrastructure.Persistencia;

/// <summary>
/// DbContext exclusivo de S1 (schema aislado s1_identidad en PostgreSQL, ADR-05).
/// Incluye Outbox/Inbox y RegistroDecision append-only (ADR-12, D-6).
/// </summary>
public sealed class IdentidadDbContext : DbContext
{
    public const string Schema = "s1_identidad";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<TipoUsuario> TiposUsuario => Set<TipoUsuario>();
    public DbSet<ClienteApiUniversidad> ClientesApiUniversidad => Set<ClienteApiUniversidad>();
    public DbSet<DesafioSegundoFactor> DesafiosSegundoFactor => Set<DesafioSegundoFactor>();
    public DbSet<DesafioOtpOperacion> DesafiosOtpOperacion => Set<DesafioOtpOperacion>();
    public DbSet<BloqueoOtpOperacion> BloqueosOtpOperacion => Set<BloqueoOtpOperacion>();

    public IdentidadDbContext(DbContextOptions<IdentidadDbContext> options) : base(options)
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

        var stringSetComparer = new ValueComparer<HashSet<string>>(
            (a, b) => a != null && b != null && a.SetEquals(b),
            v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.ToUpperInvariant().GetHashCode())),
            v => new HashSet<string>(v, StringComparer.OrdinalIgnoreCase));

        var guidSetComparer = new ValueComparer<HashSet<Guid>>(
            (a, b) => a != null && b != null && a.SetEquals(b),
            v => v.Aggregate(0, (h, g) => HashCode.Combine(h, g.GetHashCode())),
            v => new HashSet<Guid>(v));

        modelBuilder.Entity<TipoUsuario>(builder =>
        {
            builder.ToTable("tipos_usuario");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(120).IsRequired();
            builder.Property(x => x.NombreNormalizado).HasColumnName("nombre_normalizado").HasMaxLength(120).IsRequired();
            builder.Property(x => x.AccesoFinancieroODecision).HasColumnName("acceso_financiero_o_decision").IsRequired();
            builder.Property(x => x.EsEnlaceUniversidad).HasColumnName("es_enlace_universidad").IsRequired();
            builder.Property(x => x.EsPredefinido).HasColumnName("es_predefinido").IsRequired();
            builder.ConfigurarVersionExplicita<TipoUsuario, Guid>();

            builder.Ignore(x => x.Permisos);
            builder.Property<HashSet<string>>("_permisos")
                .HasColumnName("permisos_json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    s => JsonSerializer.Deserialize<HashSet<string>>(s, JsonOptions) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                    stringSetComparer)
                .IsRequired();

            builder.HasIndex(x => x.NombreNormalizado)
                .IsUnique()
                .HasDatabaseName("ix_tipos_usuario_nombre_unico");
        });

        modelBuilder.Entity<Usuario>(builder =>
        {
            builder.ToTable("usuarios");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();

            builder.Property(x => x.Cedula)
                .HasColumnName("cedula")
                .HasConversion(c => c.Numero, s => new Cedula(s))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.NombreCompleto).HasColumnName("nombre_completo").HasMaxLength(200).IsRequired();

            builder.OwnsOne(x => x.Contacto, contacto =>
            {
                contacto.Property(c => c.Correo)
                    .HasColumnName("correo")
                    .HasMaxLength(200)
                    .IsRequired();

                contacto.Property(c => c.Telefono)
                    .HasColumnName("telefono")
                    .HasMaxLength(30)
                    .IsRequired();

                contacto.HasIndex(c => c.Correo)
                    .IsUnique()
                    .HasDatabaseName("ix_usuarios_correo_unico");
            });

            builder.Navigation(x => x.Contacto).IsRequired();

            builder.Property(x => x.CanalPreferido)
                .HasColumnName("canal_preferido")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Estado)
                .HasColumnName("estado")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.EsEvaluadorExterno).HasColumnName("es_evaluador_externo").IsRequired();
            builder.Property(x => x.UniversidadId).HasColumnName("universidad_id");
            builder.Property(x => x.KeycloakUserId).HasColumnName("keycloak_user_id").HasMaxLength(120);
            builder.Property(x => x.SelloSeguridad).HasColumnName("sello_seguridad").IsRequired();
            builder.Property(x => x.Bloqueado2FaHastaUtc).HasColumnName("bloqueado_2fa_hasta_utc");
            builder.ConfigurarVersionExplicita<Usuario, Guid>();

            builder.Ignore(x => x.TipoUsuarioIds);
            builder.Property<HashSet<Guid>>("_tipoUsuarioIds")
                .HasColumnName("tipo_usuario_ids_json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    s => JsonSerializer.Deserialize<HashSet<Guid>>(s, JsonOptions) ?? new HashSet<Guid>(),
                    guidSetComparer)
                .IsRequired();

            builder.Ignore(x => x.ProgramasAsignados);
            builder.Property<HashSet<Guid>>("_programasAsignados")
                .HasColumnName("programas_asignados_json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    s => JsonSerializer.Deserialize<HashSet<Guid>>(s, JsonOptions) ?? new HashSet<Guid>(),
                    guidSetComparer)
                .IsRequired();

            builder.HasIndex(x => x.Cedula)
                .IsUnique()
                .HasDatabaseName("ix_usuarios_cedula_unico");
        });

        modelBuilder.Entity<ClienteApiUniversidad>(builder =>
        {
            builder.ToTable("clientes_api_universidad");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.UniversidadId).HasColumnName("universidad_id").IsRequired();
            builder.Property(x => x.ClientIdKeycloak).HasColumnName("client_id_keycloak").HasMaxLength(120).IsRequired();
            builder.Property(x => x.Activo).HasColumnName("activo").IsRequired();
            builder.ConfigurarVersionExplicita<ClienteApiUniversidad, Guid>();

            builder.HasIndex(x => x.ClientIdKeycloak)
                .IsUnique()
                .HasDatabaseName("ix_clientes_api_client_id_unico");
        });

        modelBuilder.Entity<DesafioSegundoFactor>(builder =>
        {
            builder.ToTable("desafios_segundo_factor");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.UsuarioId).HasColumnName("usuario_id").IsRequired();
            builder.Property(x => x.Canal).HasColumnName("canal").HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.CodigoHash).HasColumnName("codigo_hash").HasMaxLength(128).IsRequired();
            builder.Property(x => x.Salt).HasColumnName("salt").HasMaxLength(64).IsRequired();
            builder.Property(x => x.CreadoEnUtc).HasColumnName("creado_en_utc").IsRequired();
            builder.Property(x => x.ExpiraEnUtc).HasColumnName("expira_en_utc").IsRequired();
            builder.Property(x => x.IntentosFallidos).HasColumnName("intentos_fallidos").IsRequired();
            builder.Property(x => x.ConsumidoEnUtc).HasColumnName("consumido_en_utc");
            builder.Property(x => x.InvalidadoEnUtc).HasColumnName("invalidado_en_utc");
            builder.Property(x => x.EsRecuperacionCorreo).HasColumnName("es_recuperacion_correo").IsRequired();
            builder.ConfigurarVersionExplicita<DesafioSegundoFactor, Guid>();

            builder.HasIndex(x => new { x.UsuarioId, x.ConsumidoEnUtc, x.InvalidadoEnUtc })
                .HasDatabaseName("ix_desafios_2fa_usuario_activos");
        });

        modelBuilder.Entity<DesafioOtpOperacion>(builder =>
        {
            builder.ToTable("desafios_otp_operacion");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.UsuarioId).HasColumnName("usuario_id").IsRequired();
            builder.Property(x => x.Operacion).HasColumnName("operacion").HasConversion<string>().HasMaxLength(60).IsRequired();
            builder.Property(x => x.RecursoId).HasColumnName("recurso_id").HasMaxLength(120).IsRequired();
            builder.Property(x => x.Canal).HasColumnName("canal").HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.CodigoHash).HasColumnName("codigo_hash").HasMaxLength(128).IsRequired();
            builder.Property(x => x.Salt).HasColumnName("salt").HasMaxLength(64).IsRequired();
            builder.Property(x => x.CreadoEnUtc).HasColumnName("creado_en_utc").IsRequired();
            builder.Property(x => x.ExpiraEnUtc).HasColumnName("expira_en_utc").IsRequired();
            builder.Property(x => x.IntentosFallidos).HasColumnName("intentos_fallidos").IsRequired();
            builder.Property(x => x.ConsumidoEnUtc).HasColumnName("consumido_en_utc");
            builder.Property(x => x.OperationIdConsumidor).HasColumnName("operation_id_consumidor");
            builder.Property(x => x.InvalidadoEnUtc).HasColumnName("invalidado_en_utc");
            builder.ConfigurarVersionExplicita<DesafioOtpOperacion, Guid>();

            builder.HasIndex(x => new { x.UsuarioId, x.Operacion, x.RecursoId, x.ConsumidoEnUtc, x.InvalidadoEnUtc })
                .HasDatabaseName("ix_desafios_otp_terna_activos");
        });

        modelBuilder.Entity<BloqueoOtpOperacion>(builder =>
        {
            builder.ToTable("bloqueos_otp_operacion");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.UsuarioId).HasColumnName("usuario_id").IsRequired();
            builder.Property(x => x.Operacion).HasColumnName("operacion").HasConversion<string>().HasMaxLength(60).IsRequired();
            builder.Property(x => x.RecursoId).HasColumnName("recurso_id").HasMaxLength(120).IsRequired();
            builder.Property(x => x.BloqueadoHastaUtc).HasColumnName("bloqueado_hasta_utc").IsRequired();
            builder.ConfigurarVersionExplicita<BloqueoOtpOperacion, Guid>();

            builder.HasIndex(x => new { x.UsuarioId, x.Operacion, x.RecursoId })
                .IsUnique()
                .HasDatabaseName("ix_bloqueos_otp_terna_unico");
        });
    }
}
