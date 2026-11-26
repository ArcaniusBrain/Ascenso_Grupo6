using Ascenso.S4.Becas.Domain.Becas;
using Xunit;

namespace Ascenso.S4.Becas.UnitTests;

public sealed class PoliticaAccesoHistorialBecasTests
{
    [Fact]
    public void Coordinador_PuedeConsultarCualquierPersonaYAgregados()
    {
        Assert.True(PoliticaAccesoHistorialBecas.PuedeConsultarPorCedula(RolHistorialBecas.Coordinador, "V-11111111", "V-12345678"));
        Assert.True(PoliticaAccesoHistorialBecas.PuedeConsultarAgregado(RolHistorialBecas.Coordinador));
    }

    [Fact]
    public void Direccion_PuedeConsultarCualquierPersonaYAgregados()
    {
        Assert.True(PoliticaAccesoHistorialBecas.PuedeConsultarPorCedula(RolHistorialBecas.Direccion, "V-22222222", "V-12345678"));
        Assert.True(PoliticaAccesoHistorialBecas.PuedeConsultarAgregado(RolHistorialBecas.Direccion));
    }

    [Fact]
    public void Becario_SoloPuedeConsultarSuPropiaCedulaYNoAgregados()
    {
        Assert.True(PoliticaAccesoHistorialBecas.PuedeConsultarPorCedula(RolHistorialBecas.Becario, "v-12345678", "V-12345678"));
        Assert.False(PoliticaAccesoHistorialBecas.PuedeConsultarPorCedula(RolHistorialBecas.Becario, "V-12345678", "V-87654321"));
        Assert.False(PoliticaAccesoHistorialBecas.PuedeConsultarAgregado(RolHistorialBecas.Becario));
    }

    [Fact]
    public void Postulante_NoPuedeConsultarAgregadosNiHistorialesAjenos()
    {
        Assert.False(PoliticaAccesoHistorialBecas.PuedeConsultarPorCedula(RolHistorialBecas.Postulante, "V-12345678", "V-87654321"));
        Assert.False(PoliticaAccesoHistorialBecas.PuedeConsultarAgregado(RolHistorialBecas.Postulante));
    }
}
