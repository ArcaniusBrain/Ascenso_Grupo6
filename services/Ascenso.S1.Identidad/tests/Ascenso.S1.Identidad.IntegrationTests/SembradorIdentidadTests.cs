using Ascenso.S1.Identidad.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ascenso.S1.Identidad.IntegrationTests;

public sealed class SembradorIdentidadTests
{
    [Fact]
    public async Task SembrarTiposPredefinidos_InsertaSieteRolesYEsIdempotente()
    {
        SQLitePCL.Batteries_V2.Init();
        var dbPath = Path.Combine(Path.GetTempPath(), $"ascenso_s1_seed_{Guid.NewGuid():N}.db");

        try
        {
            var options = new DbContextOptionsBuilder<IdentidadDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            await using var db = new IdentidadDbContext(options);
            await db.Database.EnsureCreatedAsync();

            var primera = await SembradorIdentidadDesarrollo.SembrarTiposPredefinidosAsync(db);
            Assert.Equal(7, primera);

            var segunda = await SembradorIdentidadDesarrollo.SembrarTiposPredefinidosAsync(db);
            Assert.Equal(0, segunda);

            var nombres = await db.TiposUsuario.Select(t => t.NombreNormalizado).ToListAsync();
            Assert.Contains("DIRECCION", nombres);
            Assert.Contains("COORDINADOR", nombres);
            Assert.Contains("FINANZAS", nombres);
            Assert.Contains("COMITE", nombres);
            Assert.Contains("ENLACE_UNIVERSIDAD", nombres);
            Assert.Contains("POSTULANTE", nombres);
            Assert.Contains("BECARIO", nombres);
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath))
            {
                try { File.Delete(dbPath); } catch { }
            }
        }
    }
}
