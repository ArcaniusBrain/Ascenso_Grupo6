using System.Xml.Linq;
using Ascenso.SharedKernel.TiposBase;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ascenso.BuildingBlocks.Tests;

public sealed class MediatRIntegracionTests
{
    public sealed record EventoDominioEjemplo(Guid EntidadId, DateTimeOffset Fecha) : IEventoDeDominio;

    public sealed class CapturadorEventos
    {
        public List<EventoDominioEjemplo> Recibidos { get; } = [];
    }

    public sealed class ManejadorEventoEjemplo(CapturadorEventos capturador)
        : INotificationHandler<EventoDominioEjemplo>
    {
        public Task Handle(EventoDominioEjemplo notification, CancellationToken cancellationToken)
        {
            capturador.Recibidos.Add(notification);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task MediatR12_PublicaEventoDeDominioDelSharedKernel()
    {
        var services = new ServiceCollection();
        var capturador = new CapturadorEventos();
        services.AddSingleton(capturador);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<MediatRIntegracionTests>());

        await using var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IPublisher>();

        var evento = new EventoDominioEjemplo(Guid.NewGuid(), DateTimeOffset.UtcNow);
        await publisher.Publish(evento);

        Assert.Single(capturador.Recibidos);
        Assert.Equal(evento.EntidadId, capturador.Recibidos[0].EntidadId);
    }

    [Fact]
    public void DirectoryPackagesProps_FijaMediatRPorDebajoDeVersion13_Adr27()
    {
        var directorio = new DirectoryInfo(AppContext.BaseDirectory);
        FileInfo? archivoPaquetes = null;

        while (directorio is not null)
        {
            var candidato = Path.Combine(directorio.FullName, "Directory.Packages.props");
            if (File.Exists(candidato))
            {
                archivoPaquetes = new FileInfo(candidato);
                break;
            }
            directorio = directorio.Parent;
        }

        Assert.NotNull(archivoPaquetes);

        var xml = XDocument.Load(archivoPaquetes!.FullName);
        var paqueteMediatR = xml.Descendants("PackageVersion")
            .FirstOrDefault(e => string.Equals(e.Attribute("Include")?.Value, "MediatR", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(paqueteMediatR);
        var versionTexto = paqueteMediatR!.Attribute("Version")?.Value;
        Assert.False(string.IsNullOrWhiteSpace(versionTexto));

        var version = Version.Parse(versionTexto!);
        Assert.True(version.Major < 13, $"ADR-27 prohíbe MediatR >= 13.0.0 por licencia comercial; encontrada {version}.");
    }
}
