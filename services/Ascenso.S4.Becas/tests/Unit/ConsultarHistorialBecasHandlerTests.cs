using Ascenso.S4.Becas.Application.HistorialBecas;
using Ascenso.S4.Becas.Domain.Becas;
using Xunit;

namespace Ascenso.S4.Becas.UnitTests;

public sealed class ConsultarHistorialBecasHandlerTests
{
    private static readonly Guid MariaId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PedroId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid LuisId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid MariaSinBecasId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid AnaId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static readonly Guid BecasArteId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid BecasIngenieriaId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid BecasSaludId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid BecasMusicaId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    [Fact]
    public async Task Escenario01_BecasEnVariosProgramasYPeriodos_SenalaBecaActiva()
    {
        var handler = CrearHandlerConDatosBase();

        var res = await handler.Handle(
            new ConsultarHistorialBecasQuery(RolHistorialBecas.Coordinador, null, "V-10000001", null, null),
            CancellationToken.None);

        Assert.True(res.Autorizado);
        Assert.Equal(2, res.Items.Count);
        var concluida = Assert.Single(res.Items, x => x.NombrePrograma == "Becas Arte" && x.Periodo == "2025-2");
        var suspendida = Assert.Single(res.Items, x => x.NombrePrograma == "Becas Ingeniería" && x.Periodo == "2026-2");
        Assert.Equal("Concluida", concluida.Estado);
        Assert.False(concluida.CuentaComoActiva);
        Assert.Equal("Parcial", concluida.Alcance);
        Assert.Equal("Suspendida", suspendida.Estado);
        Assert.True(suspendida.CuentaComoActiva);
        Assert.Equal("Completo", suspendida.Alcance);
    }

    [Fact]
    public async Task Escenario02_PersonaSinBecas_DevuelveMensajeExacto()
    {
        var handler = CrearHandlerConDatosBase();

        var res = await handler.Handle(
            new ConsultarHistorialBecasQuery(RolHistorialBecas.Coordinador, null, "V-10000004", null, null),
            CancellationToken.None);

        Assert.True(res.Autorizado);
        Assert.False(res.NoEncontrado);
        Assert.Empty(res.Items);
        Assert.Equal("María Gómez no tiene becas registradas.", res.Mensaje);
    }

    [Fact]
    public async Task Escenario03_PersonaNoEncontrada_DevuelveMensajeExacto()
    {
        var handler = CrearHandlerConDatosBase();

        var res = await handler.Handle(
            new ConsultarHistorialBecasQuery(RolHistorialBecas.Coordinador, null, "V-12345678", null, null),
            CancellationToken.None);

        Assert.True(res.Autorizado);
        Assert.True(res.NoEncontrado);
        Assert.Equal("No se encontró a una persona con la identificación V-12345678.", res.Mensaje);
    }

    [Fact]
    public async Task Escenario04_ConsultaPorPrograma_DevuelveBecasDeEsePrograma()
    {
        var handler = CrearHandlerConDatosBase();

        var res = await handler.Handle(
            new ConsultarHistorialBecasQuery(RolHistorialBecas.Coordinador, null, null, BecasIngenieriaId, null),
            CancellationToken.None);

        Assert.True(res.Autorizado);
        Assert.Equal(2, res.Items.Count);
        Assert.All(res.Items, x => Assert.Equal("Becas Ingeniería", x.NombrePrograma));
    }

    [Fact]
    public async Task Escenario05_ConsultaPorPeriodoAnterior_DevuelveSoloEsePeriodo()
    {
        var handler = CrearHandlerConDatosBase();

        var res = await handler.Handle(
            new ConsultarHistorialBecasQuery(RolHistorialBecas.Coordinador, null, null, null, "2026-1"),
            CancellationToken.None);

        Assert.True(res.Autorizado);
        Assert.Equal(2, res.Items.Count);
        Assert.All(res.Items, x => Assert.Equal("2026-1", x.Periodo));
    }

    [Fact]
    public async Task Escenario06_PeriodoSinBecas_DevuelveMensajeExacto()
    {
        var handler = CrearHandlerConDatosBase();

        var res = await handler.Handle(
            new ConsultarHistorialBecasQuery(RolHistorialBecas.Coordinador, null, null, null, "2025-1"),
            CancellationToken.None);

        Assert.True(res.Autorizado);
        Assert.Empty(res.Items);
        Assert.Equal("No hay becas para el periodo 2025-1.", res.Mensaje);
    }

    [Fact]
    public void Escenario07_HistorialNoExponeDatosSocioeconomicos()
    {
        var propiedades = typeof(BecaHistorialItemDto).GetProperties().Select(p => p.Name).ToArray();

        Assert.DoesNotContain(propiedades, p => p.Contains("Ingreso", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propiedades, p => p.Contains("Composicion", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propiedades, p => p.Contains("Socioeconom", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Escenario08_BecarioNoConsultaHistorialDeOtraPersona()
    {
        var handler = CrearHandlerConDatosBase();

        var res = await handler.Handle(
            new ConsultarHistorialBecasQuery(RolHistorialBecas.Becario, "V-10000003", "V-10000001", null, null),
            CancellationToken.None);

        Assert.False(res.Autorizado);
        Assert.Empty(res.Items);
    }

    [Fact]
    public async Task Escenario09_BecarioNoUsaBusquedaPorPrograma()
    {
        var handler = CrearHandlerConDatosBase();

        var res = await handler.Handle(
            new ConsultarHistorialBecasQuery(RolHistorialBecas.Becario, "V-10000003", null, BecasIngenieriaId, null),
            CancellationToken.None);

        Assert.False(res.Autorizado);
        Assert.Empty(res.Items);
    }

    [Fact]
    public async Task Escenario10_HistorialIncluyeBecasRevocadasSinSenalDeActiva()
    {
        var handler = CrearHandlerConDatosBase();

        var res = await handler.Handle(
            new ConsultarHistorialBecasQuery(RolHistorialBecas.Coordinador, null, "V-10000002", null, null),
            CancellationToken.None);

        var revocada = Assert.Single(res.Items, x => x.Estado == "Revocada");
        Assert.False(revocada.CuentaComoActiva);
    }

    [Fact]
    public async Task Escenario11_ProgramaSinBecasRegistradas_DevuelveMensajeExacto()
    {
        var handler = CrearHandlerConDatosBase();

        var res = await handler.Handle(
            new ConsultarHistorialBecasQuery(RolHistorialBecas.Coordinador, null, null, BecasMusicaId, null),
            CancellationToken.None);

        Assert.True(res.Autorizado);
        Assert.Empty(res.Items);
        Assert.Equal("No hay becas en el programa Becas Música.", res.Mensaje);
    }

    [Fact]
    public async Task Escenario12_PostulanteNoUsaBusquedaPorPrograma()
    {
        var handler = CrearHandlerConDatosBase();

        var res = await handler.Handle(
            new ConsultarHistorialBecasQuery(RolHistorialBecas.Postulante, "V-10000004", null, BecasIngenieriaId, null),
            CancellationToken.None);

        Assert.False(res.Autorizado);
        Assert.Empty(res.Items);
    }

    [Fact]
    public async Task Escenario13_HistorialIncluyeBecasInactivasSinSenalDeActiva()
    {
        var handler = CrearHandlerConDatosBase();

        var res = await handler.Handle(
            new ConsultarHistorialBecasQuery(RolHistorialBecas.Coordinador, null, "V-10000003", null, null),
            CancellationToken.None);

        var inactiva = Assert.Single(res.Items, x => x.Estado == "Inactiva");
        Assert.False(inactiva.CuentaComoActiva);
    }

    /// <summary>
    /// Escenario: Dirección consulta el historial por persona y por programa.
    /// </summary>
    [Fact]
    public async Task DireccionConsultaHistorialPorPersonaYPorPrograma()
    {
        var handler = CrearHandlerConDatosBase();

        var porPersona = await handler.Handle(
            new ConsultarHistorialBecasQuery(RolHistorialBecas.Direccion, "V-99999998", "V-10000001", null, null),
            CancellationToken.None);

        var porPrograma = await handler.Handle(
            new ConsultarHistorialBecasQuery(RolHistorialBecas.Direccion, "V-99999998", null, BecasIngenieriaId, null),
            CancellationToken.None);

        Assert.True(porPersona.Autorizado);
        Assert.Equal(2, porPersona.Items.Count);
        Assert.True(porPrograma.Autorizado);
        Assert.Equal(2, porPrograma.Items.Count);
    }

    private static ConsultarHistorialBecasHandler CrearHandlerConDatosBase()
    {
        var personas = new List<PersonaReferenciaDto>
        {
            new(MariaId, "V-10000001", "María Rodríguez"),
            new(PedroId, "V-10000002", "Pedro Pérez"),
            new(LuisId, "V-10000003", "Luis Torres"),
            new(MariaSinBecasId, "V-10000004", "María Gómez"),
            new(AnaId, "V-10000005", "Ana Morales")
        };

        var programas = new List<ProgramaReferenciaDto>
        {
            new(BecasArteId, "Becas Arte", Alcance.Parcial),
            new(BecasIngenieriaId, "Becas Ingeniería", Alcance.Completo),
            new(BecasSaludId, "Becas Salud", Alcance.Parcial),
            new(BecasMusicaId, "Becas Música", Alcance.Parcial)
        };

        var items = new List<BecaHistorialItemDto>
        {
            new(Guid.NewGuid(), MariaId, "María Rodríguez", "V-10000001", BecasArteId, "Becas Arte", "Parcial", "2025-2", 500m, "USD", "Concluida", false),
            new(Guid.NewGuid(), MariaId, "María Rodríguez", "V-10000001", BecasIngenieriaId, "Becas Ingeniería", "Completo", "2026-2", 1200m, "USD", "Suspendida", true),
            new(Guid.NewGuid(), PedroId, "Pedro Pérez", "V-10000002", BecasArteId, "Becas Arte", "Parcial", "2025-2", 500m, "USD", "Revocada", false),
            new(Guid.NewGuid(), PedroId, "Pedro Pérez", "V-10000002", BecasIngenieriaId, "Becas Ingeniería", "Completo", "2026-1", 1100m, "USD", "Activa", true),
            new(Guid.NewGuid(), AnaId, "Ana Morales", "V-10000005", BecasArteId, "Becas Arte", "Parcial", "2026-1", 600m, "USD", "Activa", true),
            new(Guid.NewGuid(), LuisId, "Luis Torres", "V-10000003", BecasSaludId, "Becas Salud", "Parcial", "2025-2", 700m, "USD", "Inactiva", false)
        };

        return new ConsultarHistorialBecasHandler(new ConsultaHistorialFake(personas, programas, items));
    }

    private sealed class ConsultaHistorialFake(
        IReadOnlyList<PersonaReferenciaDto> personas,
        IReadOnlyList<ProgramaReferenciaDto> programas,
        IReadOnlyList<BecaHistorialItemDto> items) : IConsultaHistorialBecas
    {
        public Task<PersonaReferenciaDto?> ObtenerPersonaPorCedulaAsync(string cedulaNormalizada, CancellationToken cancellationToken) =>
            Task.FromResult(personas.FirstOrDefault(p => p.Cedula == cedulaNormalizada));

        public Task<IReadOnlyList<BecaHistorialItemDto>> ListarPorPersonaIdAsync(Guid personaId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<BecaHistorialItemDto>>(items.Where(i => i.PostulanteId == personaId).ToList());

        public Task<ProgramaReferenciaDto?> ObtenerProgramaAsync(Guid programaId, CancellationToken cancellationToken) =>
            Task.FromResult(programas.FirstOrDefault(p => p.ProgramaId == programaId));

        public Task<IReadOnlyList<BecaHistorialItemDto>> ListarPorProgramaAsync(Guid programaId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<BecaHistorialItemDto>>(items.Where(i => i.ProgramaId == programaId).ToList());

        public Task<IReadOnlyList<BecaHistorialItemDto>> ListarPorPeriodoAsync(string periodoNormalizado, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<BecaHistorialItemDto>>(items.Where(i => i.Periodo == periodoNormalizado).ToList());
    }
}
