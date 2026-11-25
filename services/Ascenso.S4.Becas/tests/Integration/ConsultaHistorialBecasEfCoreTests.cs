using Ascenso.S4.Becas.Domain.Becas;
using Ascenso.S4.Becas.Infrastructure.Persistencia;
using Ascenso.SharedKernel.ValueObjects;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ascenso.S4.Becas.IntegrationTests;

public sealed class ConsultaHistorialBecasEfCoreTests
{
    [Fact]
    public async Task ConsultaPorCedulaProgramaYPeriodoAnterior_RetornaDatosEsperadosSinCamposSocioeconomicos()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<BecaDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new BecaDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var mariaId = Guid.NewGuid();
        var luisId = Guid.NewGuid();
        var arteId = Guid.NewGuid();
        var ingenieriaId = Guid.NewGuid();

        db.ReferenciasPersonas.AddRange(
            new ReferenciaPersona(mariaId, "V-10000001", "María Rodríguez"),
            new ReferenciaPersona(luisId, "V-10000003", "Luis Torres"));

        db.ReferenciasProgramas.AddRange(
            new ReferenciaPrograma(arteId, "Becas Arte", Alcance.Parcial),
            new ReferenciaPrograma(ingenieriaId, "Becas Ingeniería", Alcance.Completo));

        db.Becas.AddRange(
            new Beca(
                Guid.NewGuid(), mariaId, arteId, Guid.NewGuid(),
                EstadoBeca.Concluida, Alcance.Parcial, ModalidadPago.Mensual,
                [new TramoBeca(Guid.NewGuid(), new CodigoPeriodo("2025-2"), new Dinero(500m, "USD"), OrigenTramo.Oferta)]),
            new Beca(
                Guid.NewGuid(), mariaId, ingenieriaId, Guid.NewGuid(),
                EstadoBeca.Suspendida, Alcance.Completo, ModalidadPago.Mensual,
                [new TramoBeca(Guid.NewGuid(), new CodigoPeriodo("2026-2"), new Dinero(1200m, "USD"), OrigenTramo.Oferta)]),
            new Beca(
                Guid.NewGuid(), luisId, arteId, Guid.NewGuid(),
                EstadoBeca.Inactiva, Alcance.Parcial, ModalidadPago.PorPeriodo,
                [new TramoBeca(Guid.NewGuid(), new CodigoPeriodo("2025-2"), new Dinero(600m, "USD"), OrigenTramo.Oferta)]));

        await db.SaveChangesAsync();

        var consulta = new ConsultaHistorialBecasEfCore(db);

        var persona = await consulta.ObtenerPersonaPorCedulaAsync("V-10000001", CancellationToken.None);
        Assert.NotNull(persona);

        var porPersona = await consulta.ListarPorPersonaIdAsync(persona.PersonaId, CancellationToken.None);
        Assert.Equal(2, porPersona.Count);
        Assert.Contains(porPersona, x => x.Estado == "Suspendida" && x.CuentaComoActiva);
        Assert.Contains(porPersona, x => x.Estado == "Concluida" && !x.CuentaComoActiva);

        var porPrograma = await consulta.ListarPorProgramaAsync(arteId, CancellationToken.None);
        Assert.Equal(2, porPrograma.Count);

        var porPeriodoAnterior = await consulta.ListarPorPeriodoAsync("2025-2", CancellationToken.None);
        Assert.Equal(2, porPeriodoAnterior.Count);
        Assert.All(porPeriodoAnterior, x => Assert.Equal("2025-2", x.Periodo));

        var periodoVacio = await consulta.ListarPorPeriodoAsync("2025-1", CancellationToken.None);
        Assert.Empty(periodoVacio);
    }
}
