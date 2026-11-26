using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Eventos;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using Xunit;

namespace Ascenso.S2.CatalogoFondos.UnitTests;

public sealed class UniversidadAliadaDominioTests
{
    private static readonly DateTimeOffset AhoraBase = new(2026, 8, 15, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Crear_Universidad_Registra_Convenio_Contacto_Y_Evento()
    {
        var contacto = new ContactoDesignado("Ana Ruiz", "ana.ruiz@ucab.edu.ve", "+58-412-5550101");
        var ucab = UniversidadAliada.Crear(
            "  UCAB  ",
            "Convenio marco 2026 con intercambio de rendimiento",
            ViaDatosAcademicos.API,
            contacto,
            AhoraBase);

        Assert.Equal("UCAB", ucab.Nombre);
        Assert.Equal("UCAB", ucab.NombreNormalizado);
        Assert.Equal(ViaDatosAcademicos.API, ucab.Convenio.Via);
        Assert.Equal("Ana Ruiz", ucab.ContactoDesignado.NombreCompleto);
        Assert.Single(ucab.EventosPendientes.OfType<UniversidadRegistradaEvento>());
    }

    [Fact]
    public void Crear_Sin_Contacto_O_Sin_Terminos_Rechaza_Con_Mensajes_Literales_HU14_1()
    {
        var exContactoNull = Assert.Throws<ExcepcionDeDominio>(() =>
            UniversidadAliada.Crear("UCAB", "Convenio 2026", ViaDatosAcademicos.API, null, AhoraBase));
        Assert.Equal("Indica el contacto designado de la universidad.", exContactoNull.Message);

        var exContactoVacio = Assert.Throws<ExcepcionDeDominio>(() =>
            new ContactoDesignado("   "));
        Assert.Equal("Indica el contacto designado de la universidad.", exContactoVacio.Message);

        var exTerminos = Assert.Throws<ExcepcionDeDominio>(() =>
            UniversidadAliada.Crear("UCAB", "  ", ViaDatosAcademicos.MANUAL, new ContactoDesignado("Ana Ruiz"), AhoraBase));
        Assert.Equal("Indica los términos del convenio.", exTerminos.Message);
    }

    [Fact]
    public void ActualizarDatos_Permite_Corregir_Mayusculas_Minusculas_E_Incrementa_Version()
    {
        var uni = UniversidadAliada.Crear(
            "universidad   metropolitana",
            "Convenio inicial",
            ViaDatosAcademicos.MANUAL,
            new ContactoDesignado("Carlos Pérez"),
            AhoraBase);

        Assert.Equal("UNIVERSIDAD METROPOLITANA", uni.NombreNormalizado);

        uni.ActualizarDatos(
            "Universidad Metropolitana",
            "Convenio actualizado",
            ViaDatosAcademicos.API,
            new ContactoDesignado("Carlos Pérez", "cperez@unimet.edu.ve"));

        Assert.Equal("Universidad Metropolitana", uni.Nombre);
        Assert.Equal("UNIVERSIDAD METROPOLITANA", uni.NombreNormalizado);
        Assert.Equal(ViaDatosAcademicos.API, uni.Convenio.Via);
        Assert.Equal(1, uni.Version);
    }

    [Fact]
    public void Carreras_Son_Aisladas_Por_Universidad_Y_Rechazan_Nombre_Vacio_HU14_1()
    {
        var ucab = UniversidadAliada.Crear("UCAB", "Convenio UCAB", ViaDatosAcademicos.API, new ContactoDesignado("Ana Ruiz"), AhoraBase);
        var unimet = UniversidadAliada.Crear("UNIMET", "Convenio UNIMET", ViaDatosAcademicos.MANUAL, new ContactoDesignado("Luis Gómez"), AhoraBase);

        var exVacia = Assert.Throws<ExcepcionDeDominio>(() => ucab.AgregarCarrera("   "));
        Assert.Equal("Indica el nombre de la carrera.", exVacia.Message);

        ucab.AgregarCarrera("Ingeniería Informática");

        Assert.Single(ucab.Carreras);
        Assert.Empty(unimet.Carreras);

        var exDup = Assert.Throws<ExcepcionDeDominio>(() => ucab.AgregarCarrera("ingeniería   informática"));
        Assert.Equal("CARRERA_DUPLICADA", exDup.Codigo);
    }

    [Fact]
    public void CostosMatricula_Son_AppendOnly_Conservan_Historial_Y_Rechazan_Cero_O_Negativo_HU14_2()
    {
        var ucab = UniversidadAliada.Crear("UCAB", "Convenio UCAB", ViaDatosAcademicos.API, new ContactoDesignado("Ana Ruiz"), AhoraBase);
        var p1 = new CodigoPeriodo("2026-1");
        var p2 = new CodigoPeriodo("2026-2");

        var exCero = Assert.Throws<ExcepcionDeDominio>(() => ucab.RegistrarCosto(p2, 0m, "USD", AhoraBase));
        Assert.Equal("El costo de matrícula debe ser mayor que cero.", exCero.Message);

        var exNeg = Assert.Throws<ExcepcionDeDominio>(() => ucab.RegistrarCosto(p2, -100m, "USD", AhoraBase));
        Assert.Equal("El costo de matrícula debe ser mayor que cero.", exNeg.Message);

        ucab.RegistrarCosto(p1, 900m, null, AhoraBase);
        ucab.RegistrarCosto(p2, 1000m, "USD", AhoraBase.AddDays(1));
        ucab.RegistrarCosto(p2, 1050m, "USD", AhoraBase.AddDays(2));

        Assert.Equal(900m, ucab.CostoVigente(p1)!.Costo.Cantidad);
        Assert.Equal("USD", ucab.CostoVigente(p1)!.Costo.Moneda);
        Assert.Equal(1050m, ucab.CostoVigente(p2)!.Costo.Cantidad);
        Assert.Equal(2, ucab.CostoVigente(p2)!.VersionRegistro);

        var historialP2 = ucab.HistorialCostos(p2);
        Assert.Equal(2, historialP2.Count);
        Assert.Equal(1000m, historialP2[0].Costo.Cantidad);
        Assert.Equal(1050m, historialP2[1].Costo.Cantidad);
        Assert.Equal(3, ucab.EventosPendientes.OfType<CostoMatriculaRegistradoEvento>().Count());
    }

    [Fact]
    public void PoliticaAutorizacionUniversidades_Permite_Direccion_Y_Coordinador_Y_Bloquea_Enlace()
    {
        PoliticaAutorizacionUniversidades.ExigirGestionUniversidad(new ActorSolicitudCatalogo(Guid.NewGuid(), "DIRECCION"));
        PoliticaAutorizacionUniversidades.ExigirGestionUniversidad(new ActorSolicitudCatalogo(Guid.NewGuid(), "COORDINADOR"));

        foreach (var rolBloqueado in new[] { "ENLACE_UNIVERSIDAD", "FINANZAS", "EVALUADOR", "BECARIO", "POSTULANTE" })
        {
            var ex = Assert.Throws<ExcepcionDeDominio>(() =>
                PoliticaAutorizacionUniversidades.ExigirGestionUniversidad(
                    new ActorSolicitudCatalogo(Guid.NewGuid(), rolBloqueado)));
            Assert.Equal("ACCESO_DENEGADO_UNIVERSIDADES", ex.Codigo);
        }
    }
}
