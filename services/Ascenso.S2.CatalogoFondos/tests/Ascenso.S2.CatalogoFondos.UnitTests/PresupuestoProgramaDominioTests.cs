using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Eventos;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using Xunit;

namespace Ascenso.S2.CatalogoFondos.UnitTests;

public sealed class PresupuestoProgramaDominioTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 11, 26, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public void HU12_1_Formula_Cuatro_Terminos_Igualdad_Exacta_Y_Cupos()
    {
        var programaId = Guid.NewGuid();
        var presupuesto = PresupuestoPrograma.Crear(programaId, cupoBecarios: 2, moneda: "USD", Ahora);
        Assert.Equal(0m, presupuesto.MontoDisponible);
        Assert.Equal(2, presupuesto.CupoDisponible);

        presupuesto.RegistrarAporte(new Dinero(2400m, "USD"), Ahora.AddMinutes(1));
        Assert.Equal(2400m, presupuesto.MontoDisponible);
        Assert.Equal(1, presupuesto.Version);

        var cicloId = Guid.NewGuid();
        presupuesto.RegistrarCupoCiclo(cicloId, limite: 2, Ahora.AddMinutes(2));

        // Oferta 1: 1200 USD (ocupa 1 cupo)
        var r1 = presupuesto.ReservarOferta(Guid.NewGuid(), cicloId, "2026-2", new Dinero(1200m, "USD"), 2, Guid.NewGuid(), Ahora.AddMinutes(3));
        Assert.Equal(1200m, presupuesto.OfertasPendientes);
        Assert.Equal(1200m, presupuesto.MontoDisponible);
        Assert.Equal(1, presupuesto.CupoDisponible);

        // Confirmar oferta 1: pasa de OfertasPendientes a Otorgado, mantiene cupo ocupado (también en Inactiva, R27)
        presupuesto.ConfirmarOtorgamiento(r1.Id, Ahora.AddMinutes(4));
        Assert.Equal(0m, presupuesto.OfertasPendientes);
        Assert.Equal(1200m, presupuesto.Otorgado);
        Assert.Equal(1200m, presupuesto.MontoDisponible);
        Assert.Equal(1, presupuesto.CupoDisponible);

        // HU-12.1 E8: monto exactamente igual al disponible (1200 == 1200) SÍ alcanza y deja disponible en 0
        var r2 = presupuesto.ReservarOferta(Guid.NewGuid(), cicloId, "2026-2", new Dinero(1200m, "USD"), 2, Guid.NewGuid(), Ahora.AddMinutes(5));
        Assert.Equal(0m, presupuesto.MontoDisponible);
        Assert.Equal(0, presupuesto.CupoDisponible);

        // HU-12.1 E9: sin cupo disponible -> mensaje literal
        var exSinCupo = Assert.Throws<ExcepcionDeDominio>(() =>
            presupuesto.ReservarOferta(Guid.NewGuid(), cicloId, "2026-2", new Dinero(100m, "USD"), 1, Guid.NewGuid(), Ahora.AddMinutes(6)));
        Assert.Equal("El programa no tiene cupo disponible.", exSinCupo.Message);

        // HU-12.1 E2: liberar oferta 2 devuelve cupo y presupuesto y emite CupoLiberadoEvento (una sola vez si se repite)
        var lib1 = presupuesto.LiberarReserva(r2.Id, MotivoLiberacionReserva.RECHAZO, Ahora.AddMinutes(7));
        Assert.False(lib1.YaEstabaLiberada);
        Assert.True(lib1.SeLiberoCupo);
        Assert.Equal(1200m, presupuesto.MontoDisponible);
        Assert.Equal(1, presupuesto.CupoDisponible);
        Assert.Single(presupuesto.EventosPendientes.OfType<CupoLiberadoEvento>());

        var lib2 = presupuesto.LiberarReserva(r2.Id, MotivoLiberacionReserva.RECHAZO, Ahora.AddMinutes(8));
        Assert.True(lib2.YaEstabaLiberada);
        Assert.Equal(1200m, presupuesto.MontoDisponible);
        Assert.Equal(1, presupuesto.CupoDisponible);
        Assert.Single(presupuesto.EventosPendientes.OfType<CupoLiberadoEvento>());
    }

    [Fact]
    public void HU12_1_E5_Y_HU12_2_E1_E2_E3_Presupuesto_Insuficiente_Y_Renovaciones_Pendientes()
    {
        var presupuesto = PresupuestoPrograma.Crear(Guid.NewGuid(), cupoBecarios: 5, moneda: "USD", Ahora);
        presupuesto.RegistrarAporte(new Dinero(6000m, "USD"), Ahora.AddMinutes(1));

        // Renovación pendiente de 5.000 (HU-12.2 E1)
        var ren = presupuesto.ReservarRenovacion(
            Guid.NewGuid(),
            new CodigoPeriodo("2027-1"),
            new Dinero(5000m, "USD"),
            Guid.NewGuid(),
            Ahora.AddMinutes(2));

        Assert.Equal(5000m, presupuesto.RenovacionesPendientes);
        Assert.Equal(1000m, presupuesto.MontoDisponible);
        Assert.Equal(5, presupuesto.CupoDisponible); // Renovación no consume cupo nuevo

        // HU-12.2 E2: oferta nueva de 1.200 con 1.000 disponibles -> mensaje literal exacto
        var exInsuf = Assert.Throws<ExcepcionDeDominio>(() =>
            presupuesto.ReservarOferta(Guid.NewGuid(), Guid.NewGuid(), "2027-1", new Dinero(1200m, "USD"), 2, Guid.NewGuid(), Ahora.AddMinutes(3)));
        Assert.Equal("Presupuesto insuficiente: disponible 1.000, la beca requiere 1.200.", exInsuf.Message);

        // HU-12.1 E5 formato 800 vs 1.200
        Assert.Equal(
            "Presupuesto insuficiente: disponible 800, la beca requiere 1.200.",
            PresupuestoPrograma.MensajePresupuestoInsuficiente(800m, 1200m));

        // HU-12.2 E3: rechazar renovación libera los 5.000 sin emitir CupoLiberadoEvento
        var lib = presupuesto.LiberarReserva(ren.Id, MotivoLiberacionReserva.RECHAZO, Ahora.AddMinutes(4));
        Assert.False(lib.SeLiberoCupo);
        Assert.Equal(0m, presupuesto.RenovacionesPendientes);
        Assert.Equal(6000m, presupuesto.MontoDisponible);
        Assert.Empty(presupuesto.EventosPendientes.OfType<CupoLiberadoEvento>());
    }

    [Fact]
    public void Idempotencia_OperationId_ClaveUnica_Y_Liberacion_Beca_Revocada()
    {
        var presupuesto = PresupuestoPrograma.Crear(Guid.NewGuid(), cupoBecarios: 3, moneda: "USD", Ahora);
        presupuesto.RegistrarAporte(new Dinero(10000m, "USD"), Ahora.AddMinutes(1));

        var postId = Guid.NewGuid();
        var cicloId = Guid.NewGuid();
        var opId = Guid.NewGuid();

        var r1 = presupuesto.ReservarOferta(postId, cicloId, "2026-2", new Dinero(2000m, "USD"), 2, opId, Ahora.AddMinutes(2));
        var r1Reintento = presupuesto.ReservarOferta(postId, cicloId, "2026-2", new Dinero(2000m, "USD"), 2, opId, Ahora.AddMinutes(3));
        Assert.Equal(r1.Id, r1Reintento.Id);
        Assert.Equal(1, presupuesto.CuposOcupados);

        // Mismo (postulacionId, cicloId) con otro operationId -> RESERVA_DUPLICADA
        var exDup = Assert.Throws<ExcepcionDeDominio>(() =>
            presupuesto.ReservarOferta(postId, cicloId, "2026-2", new Dinero(2000m, "USD"), 2, Guid.NewGuid(), Ahora.AddMinutes(4)));
        Assert.Equal("RESERVA_DUPLICADA", exDup.Codigo);

        // Confirmar, registrar desembolso parcial de 500 y revocar liberando 1500 no desembolsados (HU-22.2)
        presupuesto.ConfirmarOtorgamiento(r1.Id, Ahora.AddMinutes(5));
        presupuesto.RegistrarDesembolso(new Dinero(500m, "USD"), Ahora.AddMinutes(6));
        presupuesto.LiberarBecaRevocada(new Dinero(1500m, "USD"), cicloId, Guid.NewGuid(), Ahora.AddMinutes(7));

        Assert.Equal(500m, presupuesto.Otorgado);
        Assert.Equal(500m, presupuesto.Desembolsado);
        Assert.Equal(9500m, presupuesto.MontoDisponible);
        Assert.Equal(0, presupuesto.CuposOcupados);
        Assert.Single(presupuesto.EventosPendientes.OfType<CupoLiberadoEvento>());
    }

    [Fact]
    public void PoliticaAutorizacionPresupuesto_Solo_Permite_Direccion_Coordinador_Y_Finanzas()
    {
        PoliticaAutorizacionPresupuesto.ExigirConsultaPresupuesto(new ActorSolicitudCatalogo(Guid.NewGuid(), "DIRECCION"));
        PoliticaAutorizacionPresupuesto.ExigirConsultaPresupuesto(new ActorSolicitudCatalogo(Guid.NewGuid(), "COORDINADOR"));
        PoliticaAutorizacionPresupuesto.ExigirConsultaPresupuesto(new ActorSolicitudCatalogo(Guid.NewGuid(), "FINANZAS"));

        foreach (var rol in new[] { "BECARIO", "POSTULANTE", "EVALUADOR", "ENLACE_UNIVERSIDAD" })
        {
            var ex = Assert.Throws<ExcepcionDeDominio>(() =>
                PoliticaAutorizacionPresupuesto.ExigirConsultaPresupuesto(new ActorSolicitudCatalogo(Guid.NewGuid(), rol)));
            Assert.Equal("ACCESO_DENEGADO_PRESUPUESTO", ex.Codigo);
        }
    }
}
