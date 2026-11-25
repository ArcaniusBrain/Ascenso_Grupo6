using Ascenso.S1.Identidad.Application.CasosDeUso.Autenticacion;
using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.SharedKernel.TiposBase;
using Xunit;

namespace Ascenso.S1.Identidad.ArchitectureTests;

/// <summary>
/// Pruebas automatizadas de arquitectura exigidas por la Clase 09 (láminas 31 y 39) y ADR-04.
/// </summary>
public sealed class ArquitecturaIdentidadTests
{
    private static readonly string[] DependenciasProhibidasEnDomain =
    [
        "Ascenso.S1.Identidad.Application",
        "Ascenso.S1.Identidad.Infrastructure",
        "Ascenso.S1.Identidad.Api",
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
        "Ascenso.S1.Identidad.Infrastructure",
        "Ascenso.S1.Identidad.Api",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
        "Npgsql"
    ];

    [Fact]
    public void S1_Domain_No_Referencia_Tecnologia_Externa_Ni_Capas_Exteriores()
    {
        var referencias = typeof(Usuario).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToArray();

        foreach (var prohibida in DependenciasProhibidasEnDomain)
        {
            Assert.DoesNotContain(referencias, r => r.StartsWith(prohibida, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void S1_Application_No_Referencia_Infrastructure_Ni_Api()
    {
        var referencias = typeof(IniciarSesionPaso1Command).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToArray();

        foreach (var prohibida in DependenciasProhibidasEnApplication)
        {
            Assert.DoesNotContain(referencias, r => r.StartsWith(prohibida, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void Todas_Las_Raices_De_S1_Heredan_RaizDeAgregado_Con_Version_Explicita()
    {
        var raicesEsperadas = new[]
        {
            typeof(Usuario),
            typeof(TipoUsuario),
            typeof(ClienteApiUniversidad),
            typeof(DesafioSegundoFactor),
            typeof(DesafioOtpOperacion),
            typeof(BloqueoOtpOperacion)
        };

        foreach (var raiz in raicesEsperadas)
        {
            Assert.True(
                raiz.BaseType == typeof(RaizDeAgregado<Guid>),
                $"{raiz.Name} debe heredar de RaizDeAgregado<Guid> (ADR-25).");
        }
    }
}
