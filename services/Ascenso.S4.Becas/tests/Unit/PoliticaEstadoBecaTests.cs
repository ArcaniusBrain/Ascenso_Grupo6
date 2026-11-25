using Ascenso.S4.Becas.Domain.Becas;
using Xunit;

namespace Ascenso.S4.Becas.UnitTests;

public sealed class PoliticaEstadoBecaTests
{
    [Theory]
    [InlineData(EstadoBeca.Otorgada, true)]
    [InlineData(EstadoBeca.Activa, true)]
    [InlineData(EstadoBeca.Suspendida, true)]
    [InlineData(EstadoBeca.Renovada, true)]
    [InlineData(EstadoBeca.Inactiva, false)]
    [InlineData(EstadoBeca.Concluida, false)]
    [InlineData(EstadoBeca.Revocada, false)]
    public void CuentaComoActiva_ClasificaLosSieteEstadosSegunReglaS6(EstadoBeca estado, bool esperado)
    {
        var resultado = PoliticaEstadoBeca.CuentaComoActiva(estado);

        Assert.Equal(esperado, resultado);
    }
}
