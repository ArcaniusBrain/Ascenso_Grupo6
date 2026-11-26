using System.Reflection;
using Ascenso.S4.Becas.Domain.Becas;
using Xunit;

namespace Ascenso.S4.Becas.ArchitectureTests;

public sealed class ArquitecturaS4Tests
{
    private static readonly string[] PrefijosInfraestructuraProhibidos =
    [
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
        "MassTransit",
        "MongoDB",
        "Firebase",
        "Google.Cloud",
        "Npgsql",
        "RabbitMQ",
        "Hangfire"
    ];

    [Fact]
    public void Domain_NoReferenciaInfraestructuraNiCapasSuperiores()
    {
        var referencias = ObtenerReferencias(typeof(Beca).Assembly);

        foreach (var prohibido in PrefijosInfraestructuraProhibidos)
        {
            Assert.DoesNotContain(referencias, r => r.StartsWith(prohibido, StringComparison.OrdinalIgnoreCase));
        }

        Assert.DoesNotContain(referencias, r => r.StartsWith("Ascenso.S4.Becas.Application", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referencias, r => r.StartsWith("Ascenso.S4.Becas.Infrastructure", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referencias, r => r.StartsWith("Ascenso.S4.Becas.Api", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Application_NoReferenciaInfrastructureNiApiNiInfraestructura()
    {
        var assembly = Assembly.Load("Ascenso.S4.Becas.Application");
        var referencias = ObtenerReferencias(assembly);

        foreach (var prohibido in PrefijosInfraestructuraProhibidos)
        {
            Assert.DoesNotContain(referencias, r => r.StartsWith(prohibido, StringComparison.OrdinalIgnoreCase));
        }

        Assert.DoesNotContain(referencias, r => r.StartsWith("Ascenso.S4.Becas.Infrastructure", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(referencias, r => r.StartsWith("Ascenso.S4.Becas.Api", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Infrastructure_NoReferenciaApi()
    {
        var assembly = Assembly.Load("Ascenso.S4.Becas.Infrastructure");
        var referencias = ObtenerReferencias(assembly);

        Assert.DoesNotContain(referencias, r => r.StartsWith("Ascenso.S4.Becas.Api", StringComparison.OrdinalIgnoreCase));
    }

    private static string[] ObtenerReferencias(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty).ToArray();
}
