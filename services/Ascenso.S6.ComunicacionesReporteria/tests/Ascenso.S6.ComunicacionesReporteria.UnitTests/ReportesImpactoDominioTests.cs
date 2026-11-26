using System.Text.Json;
using Ascenso.S6.ComunicacionesReporteria.Application.Puertos;
using Ascenso.S6.ComunicacionesReporteria.Domain.Agregados;
using Ascenso.S6.ComunicacionesReporteria.Domain.Modelos;
using Ascenso.S6.ComunicacionesReporteria.Domain.Servicios;
using Ascenso.SharedKernel.TiposBase;
using Xunit;

namespace Ascenso.S6.ComunicacionesReporteria.UnitTests;

public sealed class ReportesImpactoDominioTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 1, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public void HU33_2_ConsentimientoDonante_Valida_Ids_Y_Emite_Evento()
    {
        var becarioId = Guid.NewGuid();
        var donanteId = Guid.NewGuid();

        var consentimiento = ConsentimientoDonante.Aceptar(becarioId, donanteId, Ahora);
        Assert.Equal(becarioId, consentimiento.BecarioId);
        Assert.Equal(donanteId, consentimiento.DonanteId);
        Assert.Equal(Ahora, consentimiento.AceptadoEnUtc);
        Assert.Single(consentimiento.EventosPendientes.OfType<ConsentimientoDonanteRegistradoEvento>());

        Assert.Throws<ExcepcionDeDominio>(() => ConsentimientoDonante.Aceptar(Guid.Empty, donanteId, Ahora));
        Assert.Throws<ExcepcionDeDominio>(() => ConsentimientoDonante.Aceptar(becarioId, Guid.Empty, Ahora));
    }

    [Fact]
    public void HU33_1_CapaAnonimizacion_Oculta_Sensibles_Filtra_Desembolsos_Y_Aplica_Consentimiento()
    {
        var donanteId = Guid.NewGuid();
        var otroDonanteId = Guid.NewGuid();
        var progIng = Guid.NewGuid();
        var progSalud = Guid.NewGuid();
        var carlosId = Guid.NewGuid();
        var anaId = Guid.NewGuid();

        var aportes = new[]
        {
            new AporteDonanteProyectado(donanteId, "Fundación Horizonte", progIng, "Becas Ingeniería", "2026-1", 10000m, "USD"),
            new AporteDonanteProyectado(donanteId, "Fundación Horizonte", progSalud, "Becas Salud", "2026-1", 5000m, "USD"),
            new AporteDonanteProyectado(otroDonanteId, "Otro Patrocinador Secreto", progIng, "Becas Ingeniería", "2026-1", 9000m, "USD")
        };

        var becarios = new[]
        {
            new BecarioProyectadoEntrada(carlosId, progIng, "Carlos Mendoza", "V-20111222", "UCAB", "Ingeniería Informática", 450m, "Madre y dos hermanos"),
            new BecarioProyectadoEntrada(anaId, progIng, "Ana Gómez", "V-20999888", "UCAB", "Ingeniería Civil", 210m, "Cuatro personas")
        };

        var desembolsos = new[]
        {
            new DesembolsoProyectadoEntrada(progIng, "2026-1", 4000m, "USD", EstadoDesembolsoProyectado.CONFIRMADO),
            new DesembolsoProyectadoEntrada(progIng, "2026-1", 500m, "USD", EstadoDesembolsoProyectado.FALLIDO),
            new DesembolsoProyectadoEntrada(progIng, "2026-1", 300m, "USD", EstadoDesembolsoProyectado.DEVUELTO)
        };

        var academicos = new[]
        {
            new ReporteAcademicoProyectadoEntrada(carlosId, progIng, "2026-1", 18.0m, 5, 0, OrigenReporteAcademicoProyectado.API),
            new ReporteAcademicoProyectadoEntrada(anaId, progIng, "2026-1", 16.0m, 4, 1, OrigenReporteAcademicoProyectado.MANUAL)
        };

        // Caso A (HU-33.1 E4, E5): ningún consentimiento -> nadie presentado individualmente
        var sinConsentimiento = ServicioAnonimizacionImpacto.ConstruirReporte(
            donanteId, "Fundación Horizonte", "2026-1", aportes, becarios, desembolsos, academicos, new HashSet<Guid>(), Ahora);

        var jsonSinCons = JsonSerializer.Serialize(sinConsentimiento);
        Assert.DoesNotContain("Carlos Mendoza", jsonSinCons);
        Assert.DoesNotContain("V-20111222", jsonSinCons);
        Assert.DoesNotContain("Ana Gómez", jsonSinCons);
        Assert.DoesNotContain("V-20999888", jsonSinCons);
        Assert.DoesNotContain("Madre y dos hermanos", jsonSinCons);
        Assert.DoesNotContain("Otro Patrocinador Secreto", jsonSinCons);

        // Caso B (HU-33.1 E6, E10, E11, C20): Carlos aceptó -> muestra nombre y cédula de Carlos, NUNCA datos socioeconómicos ni a Ana
        var conCarlos = ServicioAnonimizacionImpacto.ConstruirReporte(
            donanteId, "Fundación Horizonte", "2026-1", aportes, becarios, desembolsos, academicos, new HashSet<Guid> { carlosId }, Ahora);

        Assert.Equal(2, conCarlos.Programas.Count);
        Assert.Equal(15000m, conCarlos.TotalAportadoDonante);
        Assert.Equal(4000m, conCarlos.TotalDesembolsadoConfirmado);

        var ing = conCarlos.Programas.Single(p => p.ProgramaId == progIng);
        Assert.Equal(2, ing.NumeroBecarios);
        Assert.Equal(4000m, ing.EjecucionPresupuestaria.DesembolsadoConfirmado);
        Assert.Equal(17.0m, ing.ResultadosAcademicos.PromedioGeneral);
        Assert.Null(ing.ResultadosAcademicos.MensajeSinDatos);
        Assert.Single(ing.BecariosPresentados);
        Assert.Equal("Carlos Mendoza", ing.BecariosPresentados[0].NombreCompleto);
        Assert.Equal("V-20111222", ing.BecariosPresentados[0].Cedula);
        Assert.Equal(18.0m, ing.BecariosPresentados[0].PromedioAcademico);

        var salud = conCarlos.Programas.Single(p => p.ProgramaId == progSalud);
        Assert.Equal("Aún no hay datos académicos para este periodo.", salud.ResultadosAcademicos.MensajeSinDatos);

        var jsonConCarlos = JsonSerializer.Serialize(conCarlos);
        Assert.DoesNotContain("Ana Gómez", jsonConCarlos);
        Assert.DoesNotContain("V-20999888", jsonConCarlos);
        Assert.DoesNotContain("450", jsonConCarlos);
        Assert.DoesNotContain("210", jsonConCarlos);
        Assert.DoesNotContain("Madre y dos hermanos", jsonConCarlos);
        Assert.DoesNotContain("Cuatro personas", jsonConCarlos);
        Assert.DoesNotContain("Otro Patrocinador Secreto", jsonConCarlos);
    }

    [Fact]
    public void PoliticaAutorizacionImpacto_Controla_Roles_En_Consentimientos_Y_Reportes()
    {
        var becarioId = Guid.NewGuid();
        var otroId = Guid.NewGuid();

        PoliticaAutorizacionImpacto.ExigirPropioBecario(new ActorSolicitudComunicaciones(becarioId, "BECARIO"), becarioId);
        Assert.Throws<ExcepcionDeDominio>(() =>
            PoliticaAutorizacionImpacto.ExigirPropioBecario(new ActorSolicitudComunicaciones(becarioId, "BECARIO"), otroId));
        Assert.Throws<ExcepcionDeDominio>(() =>
            PoliticaAutorizacionImpacto.ExigirPropioBecario(new ActorSolicitudComunicaciones(Guid.NewGuid(), "COORDINADOR"), becarioId));

        PoliticaAutorizacionImpacto.ExigirGeneracionReporte(new ActorSolicitudComunicaciones(Guid.NewGuid(), "DIRECCION"));
        PoliticaAutorizacionImpacto.ExigirGeneracionReporte(new ActorSolicitudComunicaciones(Guid.NewGuid(), "COORDINADOR"));
        PoliticaAutorizacionImpacto.ExigirLecturaReporte(new ActorSolicitudComunicaciones(Guid.NewGuid(), "FINANZAS"));

        Assert.Throws<ExcepcionDeDominio>(() =>
            PoliticaAutorizacionImpacto.ExigirGeneracionReporte(new ActorSolicitudComunicaciones(Guid.NewGuid(), "EVALUADOR")));
        Assert.Throws<ExcepcionDeDominio>(() =>
            PoliticaAutorizacionImpacto.ExigirLecturaReporte(new ActorSolicitudComunicaciones(Guid.NewGuid(), "BECARIO")));
        Assert.Throws<ExcepcionDeDominio>(() =>
            PoliticaAutorizacionImpacto.ExigirLecturaReporte(new ActorSolicitudComunicaciones(Guid.NewGuid(), "POSTULANTE")));
    }
}
