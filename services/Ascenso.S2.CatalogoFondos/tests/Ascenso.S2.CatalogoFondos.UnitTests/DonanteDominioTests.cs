using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Eventos;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using Xunit;

namespace Ascenso.S2.CatalogoFondos.UnitTests;

public sealed class DonanteDominioTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 11, 25, 23, 0, 0, TimeSpan.Zero);

    [Fact]
    public void HU13_1_E1_E2_Crear_Organizacion_Y_Persona_Sin_Aportes_Iniciales()
    {
        var org = Donante.Crear(
            "  Empresa   Alfa ",
            TipoDonante.ORGANIZACION,
            new ContactoDonante("contacto@alfa.org", "+58-212-5550101"),
            Ahora);

        Assert.Equal("Empresa Alfa", org.Nombre);
        Assert.Equal("EMPRESA ALFA", org.NombreNormalizado);
        Assert.Equal(TipoDonante.ORGANIZACION, org.Tipo);
        Assert.Equal("contacto@alfa.org", org.Contacto.Correo);
        Assert.Equal("+58-212-5550101", org.Contacto.Telefono);
        Assert.Empty(org.Aportes);
        Assert.Equal(0, org.Version);
        Assert.Single(org.EventosPendientes, e => e is DonanteRegistradoEvento);

        var persona = Donante.Crear(
            "Juan Pérez",
            TipoDonante.PERSONA,
            new ContactoDonante("juan.perez@correo.com", null),
            Ahora);

        Assert.Equal(TipoDonante.PERSONA, persona.Tipo);
        Assert.Equal("juan.perez@correo.com", persona.Contacto.Correo);
        Assert.Null(persona.Contacto.Telefono);
        Assert.Empty(persona.Aportes);
    }

    [Fact]
    public void HU13_1_E3_Falta_Contacto_O_Correo_Invalido_Lanza_Excepcion_Literal()
    {
        var exNulo = Assert.Throws<ExcepcionDeDominio>(() =>
            Donante.Crear("Empresa Alfa", TipoDonante.ORGANIZACION, null, Ahora));
        Assert.Equal("Indica los datos de contacto del donante.", exNulo.Message);

        var exVacio = Assert.Throws<ExcepcionDeDominio>(() =>
            new ContactoDonante("   ", ""));
        Assert.Equal("Indica los datos de contacto del donante.", exVacio.Message);

        var exCorreo = Assert.Throws<ExcepcionDeDominio>(() =>
            new ContactoDonante("correo-sin-arroba", null));
        Assert.Equal("CORREO_DONANTE_INVALIDO", exCorreo.Codigo);
    }

    [Fact]
    public void HU13_2_E1_E4_Aportes_AppendOnly_Varios_Programas_Y_Periodos_Incrementan_Version()
    {
        var donante = Donante.Crear(
            "Empresa Alfa",
            TipoDonante.ORGANIZACION,
            new ContactoDonante("contacto@alfa.org", null),
            Ahora);

        var progIng = Guid.NewGuid();
        var progArte = Guid.NewGuid();

        var a1 = donante.RegistrarAporte(progIng, new CodigoPeriodo("2026-1"), 25000m, "USD", Ahora.AddMinutes(1));
        var a2 = donante.RegistrarAporte(progIng, new CodigoPeriodo("2026-2"), 30000m, "USD", Ahora.AddMinutes(2));
        var a3 = donante.RegistrarAporte(progArte, new CodigoPeriodo("2026-2"), 10000m, "USD", Ahora.AddMinutes(3));

        Assert.Equal(3, donante.Aportes.Count);
        Assert.Equal(3, donante.Version);
        Assert.Equal(65000m, donante.TotalAportado());
        Assert.Equal(25000m, donante.TotalAportado(new CodigoPeriodo("2026-1")));
        Assert.Equal(40000m, donante.TotalAportado(new CodigoPeriodo("2026-2")));
        Assert.Equal(0m, donante.TotalAportado(new CodigoPeriodo("2025-1")));

        var historial = donante.ObtenerHistorial();
        Assert.Equal([a1.Id, a2.Id, a3.Id], historial.Select(x => x.Id).ToArray());

        var filtrado = donante.ObtenerHistorial(new CodigoPeriodo("2026-1"));
        Assert.Single(filtrado);
        Assert.Equal(25000m, filtrado[0].Monto.Cantidad);
        Assert.Equal(3, donante.EventosPendientes.OfType<AporteRegistradoEvento>().Count());
    }

    [Fact]
    public void HU13_2_E2_E6_E7_E9_Validaciones_Y_Mensajes_Literales()
    {
        var donante = Donante.Crear(
            "Empresa Alfa",
            TipoDonante.ORGANIZACION,
            new ContactoDonante("contacto@alfa.org", null),
            Ahora);

        var progId = Guid.NewGuid();

        var exPeriodo = Assert.Throws<ExcepcionDeDominio>(() =>
            donante.RegistrarAporte(progId, null, 1000m, "USD", Ahora));
        Assert.Equal("Indica el periodo del aporte.", exPeriodo.Message);

        var exCero = Assert.Throws<ExcepcionDeDominio>(() =>
            donante.RegistrarAporte(progId, new CodigoPeriodo("2026-1"), 0m, "USD", Ahora));
        Assert.Equal("El monto del aporte debe ser mayor que cero.", exCero.Message);

        var exNegativo = Assert.Throws<ExcepcionDeDominio>(() =>
            donante.RegistrarAporte(progId, new CodigoPeriodo("2026-1"), -500m, "USD", Ahora));
        Assert.Equal("El monto del aporte debe ser mayor que cero.", exNegativo.Message);

        Assert.Equal("Este donante no tiene aportes registrados.", Donante.MensajeSinAportes);
        Assert.Equal(
            "No hay aportes de este donante en el periodo 2025-1.",
            Donante.MensajeSinAportesEnPeriodo(new CodigoPeriodo("2025-1")));
    }

    [Fact]
    public void PoliticaAutorizacionDonantes_Valida_Roles_Y_Alcance_De_Coordinador()
    {
        var prog1 = Guid.NewGuid();
        var prog2 = Guid.NewGuid();

        var direccion = new ActorSolicitudCatalogo(Guid.NewGuid(), "DIRECCION");
        var coordLimitado = new ActorSolicitudCatalogo(Guid.NewGuid(), "COORDINADOR", [prog1]);
        var finanzas = new ActorSolicitudCatalogo(Guid.NewGuid(), "FINANZAS");
        var evaluador = new ActorSolicitudCatalogo(Guid.NewGuid(), "EVALUADOR");
        var becario = new ActorSolicitudCatalogo(Guid.NewGuid(), "BECARIO");
        var postulante = new ActorSolicitudCatalogo(Guid.NewGuid(), "POSTULANTE");

        PoliticaAutorizacionDonantes.ExigirGestionDonante(direccion);
        PoliticaAutorizacionDonantes.ExigirRegistroAporte(coordLimitado, prog1);
        PoliticaAutorizacionDonantes.ExigirLecturaHistorial(finanzas);

        Assert.Throws<ExcepcionDeDominio>(() => PoliticaAutorizacionDonantes.ExigirRegistroAporte(coordLimitado, prog2));
        Assert.Throws<ExcepcionDeDominio>(() => PoliticaAutorizacionDonantes.ExigirGestionDonante(finanzas));
        Assert.Throws<ExcepcionDeDominio>(() => PoliticaAutorizacionDonantes.ExigirLecturaHistorial(evaluador));
        Assert.Throws<ExcepcionDeDominio>(() => PoliticaAutorizacionDonantes.ExigirLecturaHistorial(becario));
        Assert.Throws<ExcepcionDeDominio>(() => PoliticaAutorizacionDonantes.ExigirLecturaHistorial(postulante));
    }
}
