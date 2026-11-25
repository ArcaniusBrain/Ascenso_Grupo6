using Ascenso.Contracts;
using Ascenso.SharedKernel.TiposBase;
using Xunit;

namespace Ascenso.BuildingBlocks.Tests;

public sealed class ArquitecturaBuildingBlocksTests
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
    public void SharedKernel_NoReferenciaInfraestructura()
    {
        var referencias = typeof(RaizDeAgregado<>).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToArray();

        foreach (var prohibido in PrefijosInfraestructuraProhibidos)
        {
            Assert.DoesNotContain(referencias, r => r.StartsWith(prohibido, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void Contracts_NoReferenciaSharedKernelNiInfraestructura()
    {
        var referencias = typeof(IntegrationEventEnvelope<>).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(referencias, r => r.Equals("Ascenso.SharedKernel", StringComparison.OrdinalIgnoreCase));

        foreach (var prohibido in PrefijosInfraestructuraProhibidos)
        {
            Assert.DoesNotContain(referencias, r => r.StartsWith(prohibido, StringComparison.OrdinalIgnoreCase));
        }
    }
}
