using Ascenso.SharedKernel.TiposBase;
using Xunit;

namespace Ascenso.BuildingBlocks.Tests;

public sealed class TiposBaseTests
{
    private sealed record EventoPrueba(Guid RaizId, DateTimeOffset Fecha) : IEventoDeDominio;

    private sealed class HijoPrueba : Entidad<Guid>
    {
        public string Estado { get; private set; } = "INICIAL";

        public HijoPrueba(Guid id) : base(id) { }

        public void CambiarEstado(string nuevo) => Estado = nuevo;
    }

    private sealed class RaizPrueba : RaizDeAgregado<Guid>
    {
        private readonly List<HijoPrueba> _hijos = [];
        public IReadOnlyCollection<HijoPrueba> Hijos => _hijos.AsReadOnly();

        public RaizPrueba(Guid id) : base(id) { }

        public void AgregarYModificarHijo(Guid hijoId, string nuevoEstado, DateTimeOffset ahora)
        {
            var hijo = new HijoPrueba(hijoId);
            hijo.CambiarEstado(nuevoEstado);
            _hijos.Add(hijo);

            // DDD-07 / ADR-25: todo cambio en una entidad interna incrementa Version en la raíz.
            IncrementarVersion();
            Registrar(new EventoPrueba(Id, ahora));
        }
    }

    [Fact]
    public void Entidades_ConMismoId_SonIguales()
    {
        var id = Guid.NewGuid();
        var a = new HijoPrueba(id);
        var b = new HijoPrueba(id);

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Entidades_ConDistintoId_NoSonIguales()
    {
        var a = new HijoPrueba(Guid.NewGuid());
        var b = new HijoPrueba(Guid.NewGuid());

        Assert.NotEqual(a, b);
        Assert.True(a != b);
    }

    [Fact]
    public void RaizDeAgregado_IncrementaVersionYRegistraEvento_AlModificarEntidadInterna()
    {
        var raiz = new RaizPrueba(Guid.NewGuid());
        Assert.Equal(0, raiz.Version);
        Assert.Empty(raiz.EventosPendientes);

        var ahora = DateTimeOffset.UtcNow;
        raiz.AgregarYModificarHijo(Guid.NewGuid(), "CONFIRMADO", ahora);

        Assert.Equal(1, raiz.Version);
        Assert.Single(raiz.EventosPendientes);

        raiz.LimpiarEventos();
        Assert.Empty(raiz.EventosPendientes);
        Assert.Equal(1, raiz.Version);
    }
}
