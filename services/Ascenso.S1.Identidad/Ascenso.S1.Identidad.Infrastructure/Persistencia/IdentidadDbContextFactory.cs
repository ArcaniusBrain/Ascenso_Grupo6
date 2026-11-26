using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ascenso.S1.Identidad.Infrastructure.Persistencia;

/// <summary>
/// Fábrica de tiempo de diseño para ejecutar `dotnet ef migrations` y `dotnet ef database update`
/// sobre el schema s1_identidad sin requerir credenciales en código fuente.
/// </summary>
public sealed class IdentidadDbContextFactory : IDesignTimeDbContextFactory<IdentidadDbContext>
{
    public IdentidadDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ASCENSO_MIGRATION_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__IdentidadDb")
            ?? "Host=postgres;Port=5432;Database=ascenso;Username=s1_identidad_migrator;Password=placeholder";

        var options = new DbContextOptionsBuilder<IdentidadDbContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__ef_migrations_history", IdentidadDbContext.Schema);
            })
            .Options;

        return new IdentidadDbContext(options);
    }
}
