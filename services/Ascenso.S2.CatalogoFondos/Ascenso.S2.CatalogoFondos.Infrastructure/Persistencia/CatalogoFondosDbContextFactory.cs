using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ascenso.S2.CatalogoFondos.Infrastructure.Persistencia;

/// <summary>
/// Fábrica de tiempo de diseño para ejecutar `dotnet ef migrations` y `dotnet ef database update`
/// sobre el schema s2_catalogo_fondos sin requerir credenciales en código fuente.
/// </summary>
public sealed class CatalogoFondosDbContextFactory : IDesignTimeDbContextFactory<CatalogoFondosDbContext>
{
    public CatalogoFondosDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ASCENSO_MIGRATION_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__CatalogoFondosDb")
            ?? "Host=postgres;Port=5432;Database=ascenso;Username=s2_catalogo_fondos_migrator;Password=placeholder";

        var options = new DbContextOptionsBuilder<CatalogoFondosDbContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__ef_migrations_history", CatalogoFondosDbContext.Schema);
            })
            .Options;

        return new CatalogoFondosDbContext(options);
    }
}
