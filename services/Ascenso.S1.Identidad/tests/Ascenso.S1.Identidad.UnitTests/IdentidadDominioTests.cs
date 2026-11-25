using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.S1.Identidad.Domain.Servicios;
using Ascenso.SharedKernel.Seguridad;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using Xunit;

namespace Ascenso.S1.Identidad.UnitTests;

public sealed class IdentidadDominioTests
{
    [Fact]
    public void HU37_1_Escenario9_TipoUsuario_Sin_Nombre_Es_Rechazado()
    {
        var ex = Assert.Throws<ExcepcionDeDominio>(() =>
            TipoUsuario.Crear("   ", new[] { CatalogoPermisos.ConsultasResponder }));

        Assert.Equal("Escribe el nombre del tipo de usuario.", ex.Message);
    }

    [Fact]
    public void HU37_1_Escenario11_Tipo_Con_Permiso_Financiero_Exige_2FA_Y_Asistente_Comunicaciones_Queda_Exento()
    {
        var auditor = TipoUsuario.Crear(
            "Auditor interno",
            new[] { CatalogoPermisos.FinanzasConsultar });

        Assert.True(auditor.AccesoFinancieroODecision);
        Assert.True(auditor.ExigeSegundoFactor());

        // C14: tipo sin acceso financiero ni de decisión queda exento de 2FA
        var asistente = TipoUsuario.Crear(
            "Asistente de comunicaciones",
            new[] { CatalogoPermisos.ConsultasResponder });

        Assert.False(asistente.AccesoFinancieroODecision);
        Assert.False(asistente.ExigeSegundoFactor());
    }

    [Fact]
    public void HU37_1_Escenario10_Usuario_Sin_Rol_Es_Rechazado()
    {
        var ex = Assert.Throws<ExcepcionDeDominio>(() =>
            Usuario.Crear(
                new Cedula("V-20123456"),
                "Carlos Pérez",
                new Contacto("carlos@ascenso.org", "+584121234567"),
                Array.Empty<Guid>()));

        Assert.Equal("Asigna un rol al usuario.", ex.Message);
    }

    [Fact]
    public void HU37_1_Escenario4_Evaluador_No_Ve_Puntuaciones_Ajenas_Antes_Del_Cierre()
    {
        var luisId = Guid.NewGuid();
        var anaId = Guid.NewGuid();
        var tipoEvaluador = TipoUsuario.Crear(
            "Evaluador",
            new[] { CatalogoPermisos.EvaluacionesPuntuar, CatalogoPermisos.EvaluacionesVerAjenasPostCierre });

        var evaluaciones = new[]
        {
            new EvaluacionPuntuacionResumen(luisId, 88.5m, "Buen perfil"),
            new EvaluacionPuntuacionResumen(anaId, 92.0m, "Excelente promedio")
        };

        var antesDeCierre = MotorAutorizacionS1.FiltrarPuntuacionesVisibles(
            luisId,
            new[] { tipoEvaluador },
            periodoRevisionCerrado: false,
            evaluaciones);

        Assert.Single(antesDeCierre);
        Assert.Equal(luisId, antesDeCierre[0].EvaluadorId);

        var despuesDeCierre = MotorAutorizacionS1.FiltrarPuntuacionesVisibles(
            luisId,
            new[] { tipoEvaluador },
            periodoRevisionCerrado: true,
            evaluaciones);

        Assert.Equal(2, despuesDeCierre.Count);
    }

    [Fact]
    public void HU37_1_Escenario5_Finanzas_No_Puede_Modificar_Rubricas()
    {
        var tipoFinanzas = TipoUsuario.Crear(
            "Finanzas",
            new[] { CatalogoPermisos.FinanzasConsultar, CatalogoPermisos.FinanzasDesembolsar });

        var ex = Assert.Throws<ExcepcionDeDominio>(() =>
            MotorAutorizacionS1.ValidarModificacionRubrica(new[] { tipoFinanzas }));

        Assert.Equal("Tu rol no permite modificar rúbricas de evaluación.", ex.Message);
    }

    [Fact]
    public void HU37_1_Escenario6_Finanzas_No_Recibe_Datos_Socioeconomicos_En_La_Respuesta()
    {
        var tipoFinanzas = TipoUsuario.Crear(
            "Finanzas",
            new[] { CatalogoPermisos.FinanzasConsultar });

        var expediente = new ExpedientePostulacionCompleto(
            Guid.NewGuid(),
            "María Gómez",
            "Ingeniería Informática",
            IngresoFamiliarMensual: 450m,
            ComposicionFamiliar: "Madre y dos hermanos");

        var proyectadoFinanzas = MotorAutorizacionS1.ProyectarPostulacionSegunConfidencialidad(
            new[] { tipoFinanzas },
            expediente);

        Assert.Null(proyectadoFinanzas.IngresoFamiliarMensual);
        Assert.Null(proyectadoFinanzas.ComposicionFamiliar);
        Assert.Equal("María Gómez", proyectadoFinanzas.NombrePostulante);

        var tipoCoordinador = TipoUsuario.Crear(
            "Coordinador",
            new[] { CatalogoPermisos.DatosSocioeconomicosVer });

        var proyectadoCoord = MotorAutorizacionS1.ProyectarPostulacionSegunConfidencialidad(
            new[] { tipoCoordinador },
            expediente);

        Assert.Equal(450m, proyectadoCoord.IngresoFamiliarMensual);
        Assert.Equal("Madre y dos hermanos", proyectadoCoord.ComposicionFamiliar);
    }

    [Fact]
    public void HU37_1_Escenarios2y8_Solo_Direccion_Crea_Usuarios_Y_Bloquea_AutoEscalamiento()
    {
        var lauraId = Guid.NewGuid();
        var tipoCoordinador = TipoUsuario.Crear("Coordinador", new[] { CatalogoPermisos.BecasDecidir });

        Assert.Throws<ExcepcionDeDominio>(() =>
            MotorAutorizacionS1.ValidarGestionIdentidadPorDireccion(
                lauraId,
                new[] { tipoCoordinador },
                CatalogoPermisos.UsuariosGestionar));

        Assert.Throws<ExcepcionDeDominio>(() =>
            MotorAutorizacionS1.ValidarAsignacionDeRoles(lauraId, new[] { tipoCoordinador }, lauraId));
    }

    [Fact]
    public void HU37_2_DesafioSegundoFactor_Vigencia_15_Minutos_Consumo_Unico_Y_Bloqueo_Al_Tercer_Fallo()
    {
        var ahora = new DateTimeOffset(2026, 11, 25, 14, 0, 0, TimeSpan.Zero);
        var usuarioId = Guid.NewGuid();

        // Código hasheado, nunca texto plano, y consumo único (C14)
        var desafioValido = DesafioSegundoFactor.Crear(usuarioId, CanalCodigo.CORREO, "123456", ahora);
        Assert.DoesNotContain("123456", desafioValido.CodigoHash);
        Assert.Equal(ahora.AddMinutes(15), desafioValido.ExpiraEnUtc);

        Assert.Equal(ResultadoValidacionSegundoFactor.VALIDO, desafioValido.ValidarYConsumir("123456", ahora.AddMinutes(15)));
        Assert.Equal(ResultadoValidacionSegundoFactor.YA_USADO_O_INVALIDADO, desafioValido.ValidarYConsumir("123456", ahora.AddMinutes(15)));

        // Escenario 9: vencido pasado 15 minutos
        var desafioVencido = DesafioSegundoFactor.Crear(usuarioId, CanalCodigo.CORREO, "654321", ahora);
        Assert.Equal(
            ResultadoValidacionSegundoFactor.EXPIRADO,
            desafioVencido.ValidarYConsumir("654321", ahora.AddMinutes(15).AddSeconds(1)));

        // Escenario 10: 3 intentos fallidos disparan bloqueo
        var desafioIntentos = DesafioSegundoFactor.Crear(usuarioId, CanalCodigo.SMS, "777888", ahora);
        Assert.Equal(ResultadoValidacionSegundoFactor.CODIGO_INCORRECTO, desafioIntentos.ValidarYConsumir("000001", ahora.AddMinutes(1)));
        Assert.Equal(ResultadoValidacionSegundoFactor.CODIGO_INCORRECTO, desafioIntentos.ValidarYConsumir("000002", ahora.AddMinutes(2)));
        Assert.Equal(ResultadoValidacionSegundoFactor.BLOQUEO_POR_INTENTOS, desafioIntentos.ValidarYConsumir("000003", ahora.AddMinutes(3)));
    }

    [Fact]
    public void HU37_2_Resolucion_Canal_Evaluador_Externo_Sms_Y_Recuperacion()
    {
        var tipoEvaluador = TipoUsuario.Crear("Evaluador", new[] { CatalogoPermisos.EvaluacionesPuntuar });
        var ramirez = Usuario.Crear(
            new Cedula("V-11223344"),
            "Profesor Ramírez",
            new Contacto("ramirez@universidad.edu", "+584141112233"),
            new[] { tipoEvaluador.Id },
            canalPreferido: CanalCodigo.SMS,
            esEvaluadorExterno: true);

        // R4f: evaluador externo siempre por CORREO
        Assert.Equal(CanalCodigo.CORREO, ramirez.ResolverCanalSegundoFactor(esRecuperacion: false));

        var tipoCoord = TipoUsuario.Crear("Coordinador", new[] { CatalogoPermisos.BecasDecidir });
        var laura = Usuario.Crear(
            new Cedula("V-15223344"),
            "Laura Coordinadora",
            new Contacto("laura@ascenso.org", "+584149998877"),
            new[] { tipoCoord.Id },
            canalPreferido: CanalCodigo.SMS);

        // R4a: canal elegido por Laura = SMS; R4e: recuperación = CORREO
        Assert.Equal(CanalCodigo.SMS, laura.ResolverCanalSegundoFactor(esRecuperacion: false));
        Assert.Equal(CanalCodigo.CORREO, laura.ResolverCanalSegundoFactor(esRecuperacion: true));
    }

    [Fact]
    public void ClienteApiUniversidad_Valida_Universidad_Y_Revocacion()
    {
        var ucabId = Guid.NewGuid();
        var cliente = ClienteApiUniversidad.Crear(ucabId, "uni-ucab-client");

        Assert.True(cliente.PuedeEnviarPor(ucabId));
        Assert.False(cliente.PuedeEnviarPor(Guid.NewGuid()));

        cliente.Revocar();
        Assert.False(cliente.PuedeEnviarPor(ucabId));
        Assert.Equal(1, cliente.Version);
    }
}
