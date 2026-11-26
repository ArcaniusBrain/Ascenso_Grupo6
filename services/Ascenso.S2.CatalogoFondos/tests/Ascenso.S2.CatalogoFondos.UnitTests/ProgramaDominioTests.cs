using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Eventos;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using Xunit;

namespace Ascenso.S2.CatalogoFondos.UnitTests;

public sealed class ProgramaDominioTests
{
    private static readonly DateTimeOffset AhoraBase = new(2026, 8, 20, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Crear_Programa_Valido_Fija_Monto_Modalidad_Alcance_Y_Registra_Evento()
    {
        var programa = Programa.Crear(
            "  Becas   Ingeniería ",
            new CodigoPeriodo("2026-2"),
            cupoBecarios: 20,
            duracionPeriodos: 8,
            montoPorPeriodo: 2000m,
            moneda: "USD",
            modalidadPago: ModalidadPago.DIRECTO_UNIVERSIDAD,
            alcance: AlcancePrograma.COMPLETO,
            permiteAcumulacion: true, // forzado a false por ser COMPLETO (C17 / HU-01.2)
            AhoraBase);

        Assert.Equal("Becas Ingeniería", programa.Nombre);
        Assert.Equal("BECAS INGENIERÍA", programa.NombreNormalizado);
        Assert.Equal(2000m, programa.MontoPorPeriodo.Cantidad);
        Assert.Equal(ModalidadPago.DIRECTO_UNIVERSIDAD, programa.ModalidadPago);
        Assert.Equal(AlcancePrograma.COMPLETO, programa.Alcance);
        Assert.False(programa.PermiteAcumulacion);
        Assert.Single(programa.EventosPendientes.OfType<ProgramaCreadoEvento>());
    }

    [Fact]
    public void Crear_Programa_Rechaza_Campos_Faltantes_O_No_Positivos_Con_Mensajes_Literales_HU11_1()
    {
        var periodo = new CodigoPeriodo("2026-2");

        Assert.Equal("Indica el cupo de becarios.",
            Assert.Throws<ExcepcionDeDominio>(() =>
                Programa.Crear("Becas Arte", periodo, null, 8, 1500m, "USD", ModalidadPago.MENSUAL, AlcancePrograma.PARCIAL, false, AhoraBase)).Message);

        Assert.Equal("El cupo de becarios debe ser mayor que cero.",
            Assert.Throws<ExcepcionDeDominio>(() =>
                Programa.Crear("Becas Arte", periodo, 0, 8, 1500m, "USD", ModalidadPago.MENSUAL, AlcancePrograma.PARCIAL, false, AhoraBase)).Message);

        Assert.Equal("La duración del programa debe ser mayor que cero.",
            Assert.Throws<ExcepcionDeDominio>(() =>
                Programa.Crear("Becas Arte", periodo, 10, 0, 1500m, "USD", ModalidadPago.MENSUAL, AlcancePrograma.PARCIAL, false, AhoraBase)).Message);

        Assert.Equal("Indica el monto de la beca por periodo.",
            Assert.Throws<ExcepcionDeDominio>(() =>
                Programa.Crear("Becas Arte", periodo, 10, 8, null, "USD", ModalidadPago.MENSUAL, AlcancePrograma.PARCIAL, false, AhoraBase)).Message);

        Assert.Equal("El monto de la beca por periodo debe ser mayor que cero.",
            Assert.Throws<ExcepcionDeDominio>(() =>
                Programa.Crear("Becas Arte", periodo, 10, 8, 0m, "USD", ModalidadPago.MENSUAL, AlcancePrograma.PARCIAL, false, AhoraBase)).Message);

        Assert.Equal("Elige si el programa es completo o parcial.",
            Assert.Throws<ExcepcionDeDominio>(() =>
                Programa.Crear("Becas Arte", periodo, 10, 8, 1500m, "USD", ModalidadPago.MENSUAL, null, false, AhoraBase)).Message);
    }

    [Fact]
    public void AsociarDonante_Evita_Duplicados_Con_Mensaje_Literal_HU11_2_E10()
    {
        var programa = Programa.Crear(
            "Becas Ingeniería",
            new CodigoPeriodo("2026-2"),
            20, 8, 2000m, "USD",
            ModalidadPago.SEMESTRAL,
            AlcancePrograma.PARCIAL,
            true,
            AhoraBase);

        Assert.True(programa.PermiteAcumulacion);

        var donanteId = Guid.NewGuid();
        programa.AsociarDonante(donanteId, AhoraBase);

        var ex = Assert.Throws<ExcepcionDeDominio>(() => programa.AsociarDonante(donanteId, AhoraBase));
        Assert.Equal("DONANTE_YA_ASOCIADO", ex.Codigo);
        Assert.Equal("Este donante ya está asociado al programa.", ex.Message);
    }

    [Fact]
    public void CondicionesPermanencia_Valida_Negativos_Y_Sin_Horas_Servicio_HU11_2()
    {
        Assert.Equal("El promedio mínimo debe ser un número.",
            Assert.Throws<ExcepcionDeDominio>(() => CondicionesPermanencia.Crear(null, false, null, null, 0, null)).Message);

        Assert.Equal("El promedio mínimo no puede ser negativo.",
            Assert.Throws<ExcepcionDeDominio>(() => CondicionesPermanencia.Crear(-5m, false, null, null, 0, null)).Message);

        Assert.Equal("Las horas de servicio no pueden ser negativas.",
            Assert.Throws<ExcepcionDeDominio>(() => CondicionesPermanencia.Crear(15m, true, -10, null, 0, null)).Message);

        Assert.Equal("La duración máxima de pausa no puede ser negativa.",
            Assert.Throws<ExcepcionDeDominio>(() => CondicionesPermanencia.Crear(15m, false, null, null, -1, null)).Message);

        var sinHoras = CondicionesPermanencia.Crear(14m, false, 99, ["reporte académico por periodo"], 0, "Renovación anual");
        Assert.Null(sinHoras.HorasServicio);
        Assert.Equal("Horas de servicio: no aplica", sinHoras.ObtenerLeyendaHorasServicio());
        Assert.Equal(0, sinHoras.DuracionMaxPausaMeses);
    }

    [Fact]
    public void PoliticaAutorizacionProgramas_Respeta_Roles_Y_Alcance_De_Coordinador()
    {
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();

        PoliticaAutorizacionProgramas.ExigirGestionPrograma(new ActorSolicitudCatalogo(Guid.NewGuid(), "DIRECCION"), p2);
        PoliticaAutorizacionProgramas.ExigirGestionPrograma(new ActorSolicitudCatalogo(Guid.NewGuid(), "COORDINADOR", [p1]), p1);

        var exOtroProg = Assert.Throws<ExcepcionDeDominio>(() =>
            PoliticaAutorizacionProgramas.ExigirGestionPrograma(new ActorSolicitudCatalogo(Guid.NewGuid(), "COORDINADOR", [p1]), p2));
        Assert.Equal("ACCESO_DENEGADO_PROGRAMAS", exOtroProg.Codigo);

        var exEval = Assert.Throws<ExcepcionDeDominio>(() =>
            PoliticaAutorizacionProgramas.ExigirCreacion(new ActorSolicitudCatalogo(Guid.NewGuid(), "EVALUADOR")));
        Assert.Equal("ACCESO_DENEGADO_PROGRAMAS", exEval.Codigo);
    }
}
