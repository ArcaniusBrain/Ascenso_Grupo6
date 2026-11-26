using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.SharedKernel.TiposBase;
using Xunit;

namespace Ascenso.S2.CatalogoFondos.ArchitectureTests;

/// <summary>
/// Pruebas automatizadas de arquitectura exigidas por la Clase 09 (láminas 31 y 39) y ADR-04 para S2.
/// </summary>
public sealed class ArquitecturaCatalogoFondosTests
{
    private static readonly string[] DependenciasProhibidasEnDomain =
    [
        "Ascenso.S2.CatalogoFondos.Application",
        "Ascenso.S2.CatalogoFondos.Infrastructure",
        "Ascenso.S2.CatalogoFondos.Api",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
        "Npgsql",
        "MassTransit",
        "MongoDB",
        "Firebase",
        "Google.Cloud",
        "RabbitMQ",
        "Hangfire",
        "System.Net.Http"
    ];

    private static readonly string[] DependenciasProhibidasEnApplication =
    [
        "Ascenso.S2.CatalogoFondos.Infrastructure",
        "Ascenso.S2.CatalogoFondos.Api",
        "Ascenso.Messaging.Outbox",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
        "Npgsql"
    ];

    [Fact]
    public void S2_Domain_No_Referencia_Tecnologia_Externa_Ni_Capas_Exteriores()
    {
        var referencias = typeof(CalendarioAcademico).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToArray();

        foreach (var prohibida in DependenciasProhibidasEnDomain)
        {
            Assert.DoesNotContain(referencias, r => r.StartsWith(prohibida, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void S2_Application_No_Referencia_Infrastructure_Ni_Api()
    {
        var referencias = typeof(AbrirPeriodoCommand).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToArray();

        foreach (var prohibida in DependenciasProhibidasEnApplication)
        {
            Assert.DoesNotContain(referencias, r => r.StartsWith(prohibida, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void Todas_Las_Raices_De_S2_Heredan_RaizDeAgregado_Con_Version_Explicita()
    {
        var raicesEsperadas = new[]
        {
            typeof(CalendarioAcademico),
            typeof(InicioInscripcionesPeriodo)
        };

        foreach (var raiz in raicesEsperadas)
        {
            Assert.True(
                raiz.BaseType == typeof(RaizDeAgregado<Guid>),
                $"{raiz.Name} debe heredar de RaizDeAgregado<Guid> (ADR-25).");
        }
    }
}
