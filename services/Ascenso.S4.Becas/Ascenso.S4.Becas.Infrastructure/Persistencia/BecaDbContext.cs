using Ascenso.Messaging.Outbox;
using Ascenso.S4.Becas.Domain.Becas;
using Ascenso.SharedKernel.Trazabilidad;
using Ascenso.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Ascenso.S4.Becas.Infrastructure.Persistencia;

public sealed class BecaDbContext(DbContextOptions<BecaDbContext> options) : DbContext(options)
{
    public const string Schema = "s4_becas";

    public DbSet<Beca> Becas => Set<Beca>();
    public DbSet<TramoBeca> TramosBeca => Set<TramoBeca>();
    public DbSet<ReferenciaPersona> ReferenciasPersonas => Set<ReferenciaPersona>();
    public DbSet<ReferenciaPrograma> ReferenciasProgramas => Set<ReferenciaPrograma>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<RegistroDecision> RegistrosDecision => Set<RegistroDecision>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
        {
            modelBuilder.HasDefaultSchema(Schema);
        }

        modelBuilder.Entity<Beca>(builder =>
        {
            builder.ToTable("becas");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.PostulanteId).HasColumnName("postulante_id").IsRequired();
            builder.Property(x => x.ProgramaId).HasColumnName("programa_id").IsRequired();
            builder.Property(x => x.OfertaId).HasColumnName("oferta_id").IsRequired();
            builder.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.Property(x => x.Alcance).HasColumnName("alcance").HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(x => x.Modalidad).HasColumnName("modalidad").HasConversion<string>().HasMaxLength(30).IsRequired();
            builder.ConfigurarVersionExplicita<Beca, Guid>();

            builder.HasMany(x => x.Tramos)
                .WithOne()
                .HasForeignKey("beca_id")
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.Tramos).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(x => x.PostulanteId).HasDatabaseName("ix_becas_postulante");
            builder.HasIndex(x => x.ProgramaId).HasDatabaseName("ix_becas_programa");
        });

        modelBuilder.Entity<TramoBeca>(builder =>
        {
            builder.ToTable("tramos_beca");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property<Guid>("beca_id").HasColumnName("beca_id").IsRequired();

            builder.Property(x => x.Periodo)
                .HasColumnName("periodo")
                .HasConversion(p => p.Valor, v => new CodigoPeriodo(v))
                .HasMaxLength(20)
                .IsRequired();

            builder.OwnsOne(x => x.MontoAprobado, dinero =>
            {
                dinero.Property(d => d.Cantidad).HasColumnName("monto_cantidad").HasPrecision(18, 2).IsRequired();
                dinero.Property(d => d.Moneda).HasColumnName("monto_moneda").HasMaxLength(3).IsRequired();
            });
            builder.Navigation(x => x.MontoAprobado).IsRequired();

            builder.Property(x => x.Origen).HasColumnName("origen").HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.HasIndex(x => x.Periodo).HasDatabaseName("ix_tramos_beca_periodo");
        });

        modelBuilder.Entity<ReferenciaPersona>(builder =>
        {
            builder.ToTable("referencia_personas");
            builder.HasKey(x => x.PersonaId);
            builder.Property(x => x.PersonaId).HasColumnName("persona_id").ValueGeneratedNever();
            builder.Property(x => x.Cedula).HasColumnName("cedula").HasMaxLength(20).IsRequired();
            builder.Property(x => x.NombreCompleto).HasColumnName("nombre_completo").HasMaxLength(200).IsRequired();
            builder.HasIndex(x => x.Cedula).IsUnique().HasDatabaseName("ux_referencia_personas_cedula");
        });

        modelBuilder.Entity<ReferenciaPrograma>(builder =>
        {
            builder.ToTable("referencia_programas");
            builder.HasKey(x => x.ProgramaId);
            builder.Property(x => x.ProgramaId).HasColumnName("programa_id").ValueGeneratedNever();
            builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
            builder.Property(x => x.Alcance).HasColumnName("alcance").HasConversion<string>().HasMaxLength(20).IsRequired();
        });

        modelBuilder.ConfigurarOutboxEInbox();
        modelBuilder.ConfigurarRegistroDecisiones();
    }
}
