using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Eventos;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using Xunit;

namespace Ascenso.S2.CatalogoFondos.UnitTests;

public sealed class CalendarioAcademicoDominioTests
{
    private static readonly DateTimeOffset AhoraBase = new(2026, 8, 10, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AbrirPeriodo_Registra_En_Secuencia_Y_Ordena_Cronologicamente_Ascendente()
    {
        var calendario = CalendarioAcademico.CrearInstitucional();

        calendario.AbrirPeriodo("2026-10", AhoraBase);
        calendario.AbrirPeriodo("2025-2", AhoraBase.AddMinutes(1));
        calendario.AbrirPeriodo("2026-2", AhoraBase.AddMinutes(2));
        calendario.AbrirPeriodo("2026-1", AhoraBase.AddMinutes(3));

        Assert.Equal(4, calendario.Version);
        Assert.Equal(
            ["2025-2", "2026-1", "2026-2", "2026-10"],
            calendario.ObtenerPeriodosOrdenados().Select(p => p.Codigo.Valor).ToArray());
        Assert.Equal(4, calendario.EventosPendientes.OfType<PeriodoAbiertoEvento>().Count());
    }

    [Fact]
    public void AbrirPeriodo_Duplicado_Rechaza_Con_Mensaje_Literal_HU38_1_E2()
    {
        var calendario = CalendarioAcademico.CrearInstitucional();
        calendario.AbrirPeriodo("2026-2", AhoraBase);

        var ex = Assert.Throws<ExcepcionDeDominio>(() => calendario.AbrirPeriodo(" 2026-2 ", AhoraBase));

        Assert.Equal("PERIODO_DUPLICADO", ex.Codigo);
        Assert.Equal("El periodo 2026-2 ya existe.", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void AbrirPeriodo_Vacio_Rechaza_Con_Mensaje_Literal_HU38_1_E7(string? codigoInvalido)
    {
        var calendario = CalendarioAcademico.CrearInstitucional();

        var ex = Assert.Throws<ExcepcionDeDominio>(() => calendario.AbrirPeriodo(codigoInvalido, AhoraBase));

        Assert.Equal("CODIGO_PERIODO_VACIO", ex.Codigo);
        Assert.Equal("Escribe el nombre del periodo académico.", ex.Message);
    }

    [Fact]
    public void MarcarActual_Deja_Un_Unico_Periodo_Actual_Y_Registra_Evento()
    {
        var calendario = CalendarioAcademico.CrearInstitucional();
        calendario.AbrirPeriodo("2026-1", AhoraBase);
        calendario.AbrirPeriodo("2026-2", AhoraBase);
        calendario.LimpiarEventos();

        var r1 = calendario.MarcarActual(new CodigoPeriodo("2026-1"), AhoraBase.AddHours(1));
        Assert.True(r1.HuboCambio);
        Assert.Null(r1.CodigoPeriodoAnterior);
        Assert.Equal("2026-1", calendario.PeriodoActual()!.Codigo.Valor);

        var r2 = calendario.MarcarActual(new CodigoPeriodo("2026-2"), AhoraBase.AddHours(2));
        Assert.True(r2.HuboCambio);
        Assert.Equal("2026-1", r2.CodigoPeriodoAnterior!.Valor);
        Assert.Equal("2026-2", calendario.PeriodoActual()!.Codigo.Valor);
        Assert.Equal(1, calendario.Periodos.Count(p => p.EsActual));

        var ultimoEvento = calendario.EventosPendientes.OfType<PeriodoMarcadoActualEvento>().Last();
        Assert.Equal("2026-2", ultimoEvento.CodigoPeriodo);
        Assert.Equal("2026-1", ultimoEvento.CodigoPeriodoAnterior);
    }

    [Fact]
    public void MarcarActual_Mismo_Periodo_Ya_Actual_Es_Idempotente_HU38_2_E8()
    {
        var calendario = CalendarioAcademico.CrearInstitucional();
        calendario.AbrirPeriodo("2026-2", AhoraBase);
        calendario.MarcarActual(new CodigoPeriodo("2026-2"), AhoraBase);
        var versionAntes = calendario.Version;
        calendario.LimpiarEventos();

        var resultado = calendario.MarcarActual(new CodigoPeriodo("2026-2"), AhoraBase.AddMinutes(5));

        Assert.False(resultado.HuboCambio);
        Assert.Equal(versionAntes, calendario.Version);
        Assert.Empty(calendario.EventosPendientes);
    }

    [Fact]
    public void Periodo_Cerrado_No_Puede_Marcarse_Actual_Y_Actual_No_Puede_Cerrarse()
    {
        var calendario = CalendarioAcademico.CrearInstitucional();
        calendario.AbrirPeriodo("2025-2", AhoraBase);
        calendario.AbrirPeriodo("2026-1", AhoraBase);
        calendario.MarcarActual(new CodigoPeriodo("2026-1"), AhoraBase);

        var exCerrarActual = Assert.Throws<ExcepcionDeDominio>(
            () => calendario.CerrarPeriodo(new CodigoPeriodo("2026-1")));
        Assert.Equal("PERIODO_ACTUAL_NO_CERRABLE", exCerrarActual.Codigo);

        var cerrado = calendario.CerrarPeriodo(new CodigoPeriodo("2025-2"));
        Assert.Equal(EstadoPeriodo.CERRADO, cerrado.Estado);

        var exMarcarCerrado = Assert.Throws<ExcepcionDeDominio>(
            () => calendario.MarcarActual(new CodigoPeriodo("2025-2"), AhoraBase));
        Assert.Equal("PERIODO_CERRADO_NO_MARCABLE", exMarcarCerrado.Codigo);
    }

    [Fact]
    public void InicioInscripcionesPeriodo_Rechaza_Fecha_Vacia_Y_Evalua_Desde_00_00_UTC_Menos_4()
    {
        var universidadId = Guid.NewGuid();
        var codigo = new CodigoPeriodo("2026-2");

        var ex = Assert.Throws<ExcepcionDeDominio>(() =>
            InicioInscripcionesPeriodo.Crear(universidadId, "Universidad Metropolitana", codigo, null, AhoraBase));
        Assert.Equal("FECHA_INICIO_INSCRIPCIONES_OBLIGATORIA", ex.Codigo);

        var inicio = InicioInscripcionesPeriodo.Crear(
            universidadId,
            "Universidad Metropolitana",
            codigo,
            new DateOnly(2026, 9, 15),
            AhoraBase);

        // 2026-09-14 23:59:59 UTC-4 = 2026-09-15 03:59:59 UTC -> todavía no inician
        var relojAntes = new RelojFijoTest(new DateTimeOffset(2026, 9, 15, 3, 59, 59, TimeSpan.Zero));
        Assert.False(inicio.InscripcionesIniciadas(relojAntes));

        // 2026-09-15 00:00:00 UTC-4 = 2026-09-15 04:00:00 UTC -> ya iniciaron
        var relojEnPunto = new RelojFijoTest(new DateTimeOffset(2026, 9, 15, 4, 0, 0, TimeSpan.Zero));
        Assert.True(inicio.InscripcionesIniciadas(relojEnPunto));
    }

    [Fact]
    public void PoliticaAutorizacion_Restringe_Escritura_A_Direccion_Y_Lectura_A_Internos()
    {
        var direccion = new ActorSolicitudCatalogo(Guid.NewGuid(), "DIRECCION");
        var coordinador = new ActorSolicitudCatalogo(Guid.NewGuid(), "COORDINADOR");
        var finanzas = new ActorSolicitudCatalogo(Guid.NewGuid(), "FINANZAS");
        var evaluador = new ActorSolicitudCatalogo(Guid.NewGuid(), "EVALUADOR");
        var becario = new ActorSolicitudCatalogo(Guid.NewGuid(), "BECARIO");

        PoliticaAutorizacionCalendario.ExigirDireccion(direccion);
        PoliticaAutorizacionCalendario.ExigirPersonalInterno(direccion);
        PoliticaAutorizacionCalendario.ExigirPersonalInterno(coordinador);
        PoliticaAutorizacionCalendario.ExigirPersonalInterno(finanzas);
        PoliticaAutorizacionCalendario.ExigirPersonalInterno(evaluador);

        var exCoord = Assert.Throws<ExcepcionDeDominio>(() => PoliticaAutorizacionCalendario.ExigirDireccion(coordinador));
        Assert.Equal("ACCESO_DENEGADO_CALENDARIO", exCoord.Codigo);

        var exBecario = Assert.Throws<ExcepcionDeDominio>(() => PoliticaAutorizacionCalendario.ExigirPersonalInterno(becario));
        Assert.Equal("ACCESO_DENEGADO_CALENDARIO", exBecario.Codigo);
    }

    private sealed class RelojFijoTest : IRelojNegocio
    {
        public RelojFijoTest(DateTimeOffset ahoraUtc)
        {
            AhoraUtc = ahoraUtc;
        }

        public DateTimeOffset AhoraUtc { get; }
        public DateTimeOffset AhoraCaracas => AhoraUtc.ToOffset(RelojSistemaCaracas.DesfaseCaracas);
        public DateOnly HoyCaracas => DateOnly.FromDateTime(AhoraCaracas.DateTime);
    }
}
