using System.Reflection;
using System.Text.Json;
using Ascenso.Contracts;
using Ascenso.Contracts.Eventos;
using Ascenso.Messaging.Outbox;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.Trazabilidad;
using Xunit;

namespace Ascenso.BuildingBlocks.Tests;

public sealed class RegistroDecisionDominioTests
{
    [Fact]
    public void Catalogo_TipoSujeto_Y_TipoDecision_Coinciden_Con_DDD_v3_3()
    {
        Assert.Equal(5, Enum.GetValues<TipoSujeto>().Length);
        Assert.Equal(38, Enum.GetValues<TipoDecision>().Length);
        Assert.Equal(6, TipoDecisionExtensions.AmbitoFinanciero.Count);

        Assert.True(TipoDecision.PAGO_CONFIRMADO.EsAmbitoFinanciero());
        Assert.True(TipoDecision.PAGO_FALLIDO.EsAmbitoFinanciero());
        Assert.True(TipoDecision.PAGO_DEVUELTO.EsAmbitoFinanciero());
        Assert.True(TipoDecision.PAGO_REGISTRADO.EsAmbitoFinanciero());
        Assert.True(TipoDecision.DATOS_PAGO_MODIFICADOS.EsAmbitoFinanciero());
        Assert.True(TipoDecision.APORTE_REGISTRADO.EsAmbitoFinanciero());

        Assert.False(TipoDecision.SUSPENSION.EsAmbitoFinanciero());
        Assert.False(TipoDecision.REVOCACION.EsAmbitoFinanciero());
        Assert.False(TipoDecision.CONSOLIDACION.EsAmbitoFinanciero());
    }

    [Fact]
    public void HU35_1_Escenario1_Suspension_Registra_Quien_Cuando_Tipo_Sujeto_Y_Porque()
    {
        var lauraId = Guid.NewGuid();
        var becaCarlosId = Guid.NewGuid();
        var programaId = Guid.NewGuid();
        var carlosUsuarioId = Guid.NewGuid();
        var fecha = new DateTimeOffset(2026, 10, 5, 14, 30, 0, TimeSpan.FromHours(-4));

        var registro = RegistroDecision.Crear(
            tipo: TipoDecision.SUSPENSION,
            sujeto: SujetoRef.DesdeGuid(TipoSujeto.BECA, becaCarlosId),
            actor: ActorDecision.Usuario(lauraId, "COORDINADOR"),
            ocurridoEnUtc: fecha,
            justificacionTexto: "No entregó constancia de matrícula",
            servicioOrigen: "S4",
            correlationId: Guid.NewGuid(),
            programaId: programaId,
            propietarioUsuarioId: carlosUsuarioId,
            estadoAnterior: "Activa",
            estadoNuevo: "Suspendida");

        Assert.Equal(lauraId, registro.Actor.UsuarioId);
        Assert.False(registro.Actor.EsSistema);
        Assert.Equal(fecha.ToUniversalTime(), registro.OcurridoEnUtc);
        Assert.Equal(TipoDecision.SUSPENSION, registro.Tipo);
        Assert.Equal(TipoSujeto.BECA, registro.Sujeto.Tipo);
        Assert.Equal(becaCarlosId.ToString("D"), registro.Sujeto.Id);
        Assert.Equal("No entregó constancia de matrícula", registro.Justificacion.Texto);
        Assert.Equal(programaId, registro.ProgramaId);
        Assert.Equal(carlosUsuarioId, registro.PropietarioUsuarioId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HU35_1_Escenario2_Decision_Sin_Justificacion_Es_Rechazada_Con_Mensaje_Exacto(string? justificacionInvalida)
    {
        var ex = Assert.Throws<ExcepcionDeDominio>(() =>
            RegistroDecision.Crear(
                tipo: TipoDecision.REVOCACION,
                sujeto: SujetoRef.DesdeGuid(TipoSujeto.BECA, Guid.NewGuid()),
                actor: ActorDecision.Usuario(Guid.NewGuid(), "COORDINADOR"),
                ocurridoEnUtc: DateTimeOffset.UtcNow,
                justificacionTexto: justificacionInvalida,
                servicioOrigen: "S4",
                correlationId: Guid.NewGuid()));

        Assert.Equal("Escribe la justificación de esta decisión.", ex.Message);
    }

    [Fact]
    public void HU35_1_Escenarios4y5_RegistroDecision_Es_Inmutable_Y_No_Es_Agregado()
    {
        var tipo = typeof(RegistroDecision);

        Assert.False(tipo.BaseType?.IsGenericType == true && tipo.BaseType.GetGenericTypeDefinition() == typeof(RaizDeAgregado<>));

        // Ninguna propiedad puede tener setter público
        var settersPublicos = tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.SetMethod is { IsPublic: true })
            .ToList();
        Assert.Empty(settersPublicos);

        // Ningún rol, incluida Dirección, puede editar o eliminar
        var direccion = new ContextoSolicitanteDecision(Guid.NewGuid(), PerfilAccesoDecision.DIRECCION, TienePoderDeDecisionEnCaso: true);
        var ex = Assert.Throws<ExcepcionInmutabilidadRegistro>(() =>
            PoliticaAutorizacionDecision.ValidarIntentoModificacionOEliminacion(direccion));

        Assert.Equal("El registro de decisiones no se puede modificar ni eliminar.", ex.Message);
    }

    [Fact]
    public void HU35_1_Escenario6_Nueva_Decision_Crea_Registro_Nuevo_Conservando_Anterior()
    {
        var becaId = Guid.NewGuid();
        var laura = ActorDecision.Usuario(Guid.NewGuid(), "COORDINADOR");

        var suspension = RegistroDecision.Crear(
            TipoDecision.SUSPENSION,
            SujetoRef.DesdeGuid(TipoSujeto.BECA, becaId),
            laura,
            new DateTimeOffset(2026, 10, 5, 10, 0, 0, TimeSpan.Zero),
            "No entregó constancia de matrícula",
            "S4",
            Guid.NewGuid(),
            estadoAnterior: "Activa",
            estadoNuevo: "Suspendida");

        var reanudacion = RegistroDecision.Crear(
            TipoDecision.REANUDACION,
            SujetoRef.DesdeGuid(TipoSujeto.BECA, becaId),
            laura,
            new DateTimeOffset(2026, 10, 20, 10, 0, 0, TimeSpan.Zero),
            "Consignó constancia vigente",
            "S4",
            Guid.NewGuid(),
            estadoAnterior: "Suspendida",
            estadoNuevo: "Activa");

        Assert.NotEqual(suspension.Id, reanudacion.Id);
        Assert.Equal("No entregó constancia de matrícula", suspension.Justificacion.Texto);
        Assert.Equal(TipoDecision.SUSPENSION, suspension.Tipo);
        Assert.Equal(TipoDecision.REANUDACION, reanudacion.Tipo);
    }

    [Fact]
    public void HU35_1_Escenario7_Rol_Sin_Poder_De_Decision_No_Puede_Registrar()
    {
        var enlaceUniversidad = new ContextoSolicitanteDecision(
            Guid.NewGuid(),
            PerfilAccesoDecision.ENLACE_UNIVERSIDAD,
            TienePoderDeDecisionEnCaso: false);

        Assert.Throws<ExcepcionAccesoDenegadoDecision>(() =>
            PoliticaAutorizacionDecision.ValidarPermisoRegistro(enlaceUniversidad, Guid.NewGuid()));

        var becario = new ContextoSolicitanteDecision(
            Guid.NewGuid(),
            PerfilAccesoDecision.POSTULANTE_O_BECARIO,
            TienePoderDeDecisionEnCaso: false);

        Assert.Throws<ExcepcionAccesoDenegadoDecision>(() =>
            PoliticaAutorizacionDecision.ValidarPermisoRegistro(becario, Guid.NewGuid()));
    }

    [Fact]
    public void HU35_1_Inactiva_Entrada_Automatica_Sistema_Y_Salida_Por_Coordinador()
    {
        var becaId = Guid.NewGuid();
        var programaId = Guid.NewGuid();
        var becarioId = Guid.NewGuid();

        var entradaInactiva = RegistroDecision.CrearAutomatica(
            tipo: TipoDecision.INACTIVACION,
            sujeto: SujetoRef.DesdeGuid(TipoSujeto.BECA, becaId),
            ocurridoEnUtc: new DateTimeOffset(2026, 11, 1, 4, 0, 0, TimeSpan.Zero),
            justificacionAutomatica: RegistroDecision.JustificacionInactivacionNoRenovacion,
            servicioOrigen: "S4",
            correlationId: Guid.NewGuid(),
            programaId: programaId,
            propietarioUsuarioId: becarioId,
            estadoAnterior: "Activa",
            estadoNuevo: "Inactiva");

        Assert.True(entradaInactiva.Actor.EsSistema);
        Assert.Null(entradaInactiva.Actor.UsuarioId);
        Assert.Equal("SISTEMA", entradaInactiva.Actor.Rol);
        Assert.Equal(RegistroDecision.JustificacionInactivacionNoRenovacion, entradaInactiva.Justificacion.Texto);

        var coordinadorId = Guid.NewGuid();
        var salidaInactiva = RegistroDecision.Crear(
            tipo: TipoDecision.SALIDA_DE_INACTIVA,
            sujeto: SujetoRef.DesdeGuid(TipoSujeto.BECA, becaId),
            actor: ActorDecision.Usuario(coordinadorId, "COORDINADOR"),
            ocurridoEnUtc: new DateTimeOffset(2026, 11, 10, 15, 0, 0, TimeSpan.Zero),
            justificacionTexto: "Becario subió constancia faltante, se reactiva la beca",
            servicioOrigen: "S4",
            correlationId: Guid.NewGuid(),
            programaId: programaId,
            propietarioUsuarioId: becarioId,
            estadoAnterior: "Inactiva",
            estadoNuevo: "Activa");

        Assert.False(salidaInactiva.Actor.EsSistema);
        Assert.Equal(coordinadorId, salidaInactiva.Actor.UsuarioId);
        Assert.Equal(TipoDecision.SALIDA_DE_INACTIVA, salidaInactiva.Tipo);
    }

    [Fact]
    public void HU35_2_Puerto_IConsultaRegistroDecisiones_Es_Estrictamente_Solo_Lectura()
    {
        var metodos = typeof(IConsultaRegistroDecisiones)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Select(m => m.Name)
            .ToList();

        Assert.All(metodos, nombre => Assert.StartsWith("Consultar", nombre, StringComparison.Ordinal));
        Assert.Equal("Este caso aún no tiene decisiones registradas.", ResultadoConsultaDecisiones.MensajeCasoSinDecisiones);
    }

    [Fact]
    public void Contrato_DecisionRegistradaV1_Serializa_En_Envelope_Y_Sin_Pii_Prohibida()
    {
        var payload = new DecisionRegistradaV1(
            RegistroDecisionId: Guid.NewGuid(),
            TipoDecision: nameof(TipoDecision.SUSPENSION),
            TipoSujeto: nameof(TipoSujeto.BECA),
            SujetoId: Guid.NewGuid().ToString("D"),
            ProgramaId: Guid.NewGuid(),
            PropietarioUsuarioId: Guid.NewGuid(),
            ActorUsuarioId: Guid.NewGuid(),
            ActorRol: "COORDINADOR",
            ActorEsSistema: false,
            OcurridoEnUtc: DateTimeOffset.UtcNow,
            Justificacion: "No entregó constancia de matrícula",
            EstadoAnterior: "Activa",
            EstadoNuevo: "Suspendida",
            ServicioOrigen: "S4",
            DatosResumenJson: null);

        var sobre = new IntegrationEventEnvelope<DecisionRegistradaV1>(
            EventId: payload.RegistroDecisionId,
            EventType: nameof(DecisionRegistradaV1),
            SchemaVersion: 1,
            AggregateId: Guid.NewGuid(),
            OccurredAtUtc: payload.OcurridoEnUtc,
            Producer: "S4",
            CorrelationId: Guid.NewGuid(),
            CausationId: payload.RegistroDecisionId,
            Payload: payload);

        var json = JsonSerializer.Serialize(sobre);
        var deserializado = JsonSerializer.Deserialize<IntegrationEventEnvelope<DecisionRegistradaV1>>(json);

        Assert.NotNull(deserializado);
        Assert.Equal(payload.RegistroDecisionId, deserializado!.Payload.RegistroDecisionId);
        Assert.Equal(payload.PropietarioUsuarioId, deserializado.Payload.PropietarioUsuarioId);

        var nombresProhibidos = new[] { "NumeroCuenta", "CuentaBancaria", "Password", "Clave", "IngresoFamiliar", "Cedula" };
        var propiedades = typeof(DecisionRegistradaV1).GetProperties().Select(p => p.Name).ToList();
        foreach (var prohibida in nombresProhibidos)
        {
            Assert.DoesNotContain(propiedades, p => p.Contains(prohibida, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void SqlAppendOnly_PostgreSql_Revoca_Update_Y_Delete_Y_Crea_Trigger()
    {
        var sql = RegistroDecisionSqlAppendOnly.GenerarSqlPostgreSql("s4_becas");

        Assert.Contains("REVOKE UPDATE, DELETE, TRUNCATE ON TABLE \"s4_becas\".\"registro_decisiones\" FROM \"s4_becas_app\"", sql);
        Assert.Contains("BEFORE UPDATE OR DELETE ON \"s4_becas\".\"registro_decisiones\"", sql);
        Assert.Contains("public.impedir_modificacion_registro_decisiones()", sql);
    }
}
