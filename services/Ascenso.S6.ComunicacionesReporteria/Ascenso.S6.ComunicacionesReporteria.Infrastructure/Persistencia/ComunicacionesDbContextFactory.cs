using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ascenso.S6.ComunicacionesReporteria.Infrastructure.Persistencia;

/// <summary>
/// Fábrica de tiempo de diseño para ejecutar `dotnet ef migrations` y `dotnet ef database update`
/// sobre el schema s6_comunicaciones sin requerir credenciales en código fuente.
/// </summary>
public sealed class ComunicacionesDbContextFactory : IDesignTimeDbContextFactory<ComunicacionesDbContext>
{
    public ComunicacionesDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ASCENSO_MIGRATION_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__ComunicacionesDb")
            ?? "Host=postgres;Port=5432;Database=ascenso;Username=s6_comunicaciones_migrator;Password=placeholder";

        var options = new DbContextOptionsBuilder<ComunicacionesDbContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__ef_migrations_history", ComunicacionesDbContext.Schema);
            })
            .Options;

        return new ComunicacionesDbContext(options);
    }
}
