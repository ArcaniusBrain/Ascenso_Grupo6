using Ascenso.S6.ComunicacionesReporteria.Application.CasosDeUso.Consentimientos;
using Ascenso.S6.ComunicacionesReporteria.Domain.Agregados;
using Ascenso.SharedKernel.TiposBase;
using Xunit;

namespace Ascenso.S6.ComunicacionesReporteria.ArchitectureTests;

/// <summary>
/// Pruebas automatizadas de arquitectura exigidas por la Clase 09 (láminas 31 y 39) y ADR-04 para S6.
/// </summary>
public sealed class ArquitecturaComunicacionesTests
{
    private static readonly string[] DependenciasProhibidasEnDomain =
    [
        "Ascenso.S6.ComunicacionesReporteria.Application",
        "Ascenso.S6.ComunicacionesReporteria.Infrastructure",
        "Ascenso.S6.ComunicacionesReporteria.Api",
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
        "Ascenso.S6.ComunicacionesReporteria.Infrastructure",
        "Ascenso.S6.ComunicacionesReporteria.Api",
        "Ascenso.Messaging.Outbox",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
        "Npgsql"
    ];

    [Fact]
    public void S6_Domain_No_Referencia_Tecnologia_Externa_Ni_Capas_Exteriores()
    {
        var referencias = typeof(ConsentimientoDonante).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToArray();

        foreach (var prohibida in DependenciasProhibidasEnDomain)
        {
            Assert.DoesNotContain(referencias, r => r.StartsWith(prohibida, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void S6_Application_No_Referencia_Infrastructure_Ni_Api()
    {
        var referencias = typeof(RegistrarConsentimientoDonanteCommand).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToArray();

        foreach (var prohibida in DependenciasProhibidasEnApplication)
        {
            Assert.DoesNotContain(referencias, r => r.StartsWith(prohibida, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void Todas_Las_Raices_De_S6_Heredan_RaizDeAgregado_Con_Version_Explicita()
    {
        var raicesEsperadas = new[]
        {
            typeof(ConsentimientoDonante),
            typeof(SolicitudReporteImpacto)
        };

        foreach (var raiz in raicesEsperadas)
        {
            Assert.True(
                raiz.BaseType == typeof(RaizDeAgregado<Guid>),
                $"{raiz.Name} debe heredar de RaizDeAgregado<Guid> (ADR-25).");
        }
    }
}
