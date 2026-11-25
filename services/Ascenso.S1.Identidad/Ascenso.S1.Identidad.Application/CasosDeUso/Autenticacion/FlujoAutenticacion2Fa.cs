using Ascenso.S1.Identidad.Application.Puertos;
using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.S1.Identidad.Domain.Repositorios;
using Ascenso.SharedKernel.Seguridad;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;
using MediatR;

namespace Ascenso.S1.Identidad.Application.CasosDeUso.Autenticacion;

public sealed record ResultadoInicioSesionPaso1(
    bool RequiereSegundoFactor,
    Guid? DesafioId,
    CanalCodigo? CanalEnviado,
    DateTimeOffset? ExpiraEnUtc,
    SesionAutenticadaDto? Sesion,
    string? Mensaje = null);

public sealed record IniciarSesionPaso1Command(
    string Correo,
    string Contrasena) : IRequest<ResultadoInicioSesionPaso1>;

public sealed record VerificarSegundoFactorCommand(
    Guid DesafioId,
    string? Codigo) : IRequest<SesionAutenticadaDto>;

public sealed record ReenviarSegundoFactorCommand(
    Guid UsuarioId) : IRequest<ResultadoInicioSesionPaso1>;

public sealed record RecuperarSegundoFactorCommand(
    string Correo) : IRequest<ResultadoInicioSesionPaso1>;

public sealed class IniciarSesionPaso1Handler(
    IUsuarioRepository usuarios,
    ITipoUsuarioRepository tipos,
    IDesafioSegundoFactorRepository desafios,
    IProveedorIdentidadKeycloak keycloak,
    ICanalEnvioCodigo canalEnvio,
    IGeneradorCodigoSeguro generadorCodigo,
    IServicioSesionAutenticada servicioSesion,
    IRelojNegocio reloj,
    ConfiguracionAutenticacion2Fa config,
    IUnidadTrabajoIdentidad uow) : IRequestHandler<IniciarSesionPaso1Command, ResultadoInicioSesionPaso1>
{
    public async Task<ResultadoInicioSesionPaso1> Handle(IniciarSesionPaso1Command request, CancellationToken cancellationToken)
    {
        var correo = (request.Correo ?? string.Empty).Trim().ToLowerInvariant();
        if (correo.Length == 0 || string.IsNullOrEmpty(request.Contrasena))
        {
            throw new ExcepcionDeDominio("Credenciales inválidas.", "CREDENCIALES_INVALIDAS");
        }

        // Paso 1: validar contraseña en Keycloak (HU-37.2 Escenario 6: si falla, NO pide ni envía 2FA)
        var validacion = await keycloak.ValidarCredencialesAsync(correo, request.Contrasena, cancellationToken);
        if (!validacion.Exitoso)
        {
            throw new ExcepcionDeDominio("Credenciales inválidas.", "CREDENCIALES_INVALIDAS");
        }

        var usuario = await usuarios.ObtenerPorCorreoAsync(correo, cancellationToken)
            ?? throw new ExcepcionDeDominio("El usuario no se encuentra habilitado en ASCENSO.", "USUARIO_NO_HABILITADO");

        if (usuario.Estado != EstadoUsuario.ACTIVO)
        {
            throw new ExcepcionDeDominio("La cuenta de usuario se encuentra desactivada.", "USUARIO_DESACTIVADO");
        }

        var tiposUsuario = await tipos.ObtenerPorIdsAsync(usuario.TipoUsuarioIds, cancellationToken);

        // Si el rol NO exige segundo factor (ej. postulante o tipo sin acceso financiero ni de decisión, C14), abrir sesión
        if (!usuario.RequiereSegundoFactor(tiposUsuario))
        {
            var sesionDirecta = await servicioSesion.EmitirSesionAsync(
                usuario,
                tiposUsuario,
                segundoFactorVerificado: false,
                validacion.AccessTokenPrimario,
                cancellationToken);

            return new ResultadoInicioSesionPaso1(
                RequiereSegundoFactor: false,
                DesafioId: null,
                CanalEnviado: null,
                ExpiraEnUtc: null,
                Sesion: sesionDirecta);
        }

        // Si exige 2FA: comprobar si está bloqueado por 3 intentos fallidos (1 hora, HU-37.2 E10)
        var ahoraUtc = reloj.AhoraUtc;
        if (usuario.EstaBloqueado2Fa(ahoraUtc))
        {
            throw new ExcepcionDeDominio(
                "Superaste el máximo de intentos fallidos. Debes esperar 1 hora para volver a intentarlo.",
                "BLOQUEO_2FA_ACTIVO");
        }

        // Invalidar desafíos previos activos del usuario (C14)
        var activosPrevios = await desafios.ObtenerActivosPorUsuarioAsync(usuario.Id, cancellationToken);
        foreach (var previo in activosPrevios)
        {
            previo.Invalidar(ahoraUtc);
        }

        var canal = usuario.ResolverCanalSegundoFactor(esRecuperacion: false);
        var destino = canal == CanalCodigo.CORREO ? usuario.Contacto.Correo : usuario.Contacto.Telefono;
        var codigoPlano = generadorCodigo.GenerarCodigoNumerico(6);

        var desafio = DesafioSegundoFactor.Crear(
            usuarioId: usuario.Id,
            canal: canal,
            codigoPlano: codigoPlano,
            ahoraUtc: ahoraUtc,
            vigenciaMinutos: config.VigenciaMinutos,
            esRecuperacionCorreo: false);

        // HU-37.2 Escenario 5: si falla el envío, NO se abre la sesión y se permite reintentar
        await canalEnvio.EnviarCodigoSegundoFactorAsync(
            canal,
            destino,
            codigoPlano,
            config.VigenciaMinutos,
            esRecuperacion: false,
            cancellationToken);

        await desafios.AgregarAsync(desafio, cancellationToken);
        await uow.GuardarCambiosAsync(cancellationToken);

        // HU-37.2 Escenario 7: NUNCA emitir token ni abrir sesión en el paso 1 cuando exige 2FA
        return new ResultadoInicioSesionPaso1(
            RequiereSegundoFactor: true,
            DesafioId: desafio.Id,
            CanalEnviado: canal,
            ExpiraEnUtc: desafio.ExpiraEnUtc,
            Sesion: null,
            Mensaje: "Ingresa el código de verificación enviado para completar tu inicio de sesión.");
    }
}

public sealed class VerificarSegundoFactorHandler(
    IUsuarioRepository usuarios,
    ITipoUsuarioRepository tipos,
    IDesafioSegundoFactorRepository desafios,
    IServicioSesionAutenticada servicioSesion,
    IRelojNegocio reloj,
    ConfiguracionAutenticacion2Fa config,
    IUnidadTrabajoIdentidad uow) : IRequestHandler<VerificarSegundoFactorCommand, SesionAutenticadaDto>
{
    public async Task<SesionAutenticadaDto> Handle(VerificarSegundoFactorCommand request, CancellationToken cancellationToken)
    {
        if (request.DesafioId == Guid.Empty)
        {
            throw new ExcepcionDeDominio(DesafioSegundoFactor.MensajeCodigoInvalido, "SEGUNDO_FACTOR_INVALIDO");
        }

        var desafio = await desafios.ObtenerPorIdAsync(request.DesafioId, cancellationToken)
            ?? throw new ExcepcionDeDominio(DesafioSegundoFactor.MensajeCodigoInvalido, "SEGUNDO_FACTOR_INVALIDO");

        var usuario = await usuarios.ObtenerPorIdAsync(desafio.UsuarioId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El usuario no existe.", "USUARIO_NO_ENCONTRADO");

        var ahoraUtc = reloj.AhoraUtc;

        if (usuario.EstaBloqueado2Fa(ahoraUtc))
        {
            throw new ExcepcionDeDominio(
                "Superaste el máximo de intentos fallidos. Debes esperar 1 hora para volver a intentarlo.",
                "BLOQUEO_2FA_ACTIVO");
        }

        var resultado = desafio.ValidarYConsumir(request.Codigo, ahoraUtc);

        switch (resultado)
        {
            case ResultadoValidacionSegundoFactor.VALIDO:
                usuario.LimpiarBloqueo2Fa();
                await uow.GuardarCambiosAsync(cancellationToken);

                var tiposUsuario = await tipos.ObtenerPorIdsAsync(usuario.TipoUsuarioIds, cancellationToken);
                return await servicioSesion.EmitirSesionAsync(
                    usuario,
                    tiposUsuario,
                    segundoFactorVerificado: true,
                    accessTokenKeycloak: null,
                    cancellationToken);

            case ResultadoValidacionSegundoFactor.EXPIRADO:
                throw new ExcepcionDeDominio(
                    "El código de verificación ha vencido. Solicita un código nuevo.",
                    "SEGUNDO_FACTOR_EXPIRADO");

            case ResultadoValidacionSegundoFactor.YA_USADO_O_INVALIDADO:
                throw new ExcepcionDeDominio(
                    DesafioSegundoFactor.MensajeCodigoInvalido,
                    "SEGUNDO_FACTOR_YA_USADO");

            case ResultadoValidacionSegundoFactor.BLOQUEO_POR_INTENTOS:
                usuario.RegistrarBloqueo2Fa(ahoraUtc.AddHours(config.HorasBloqueo));
                await uow.GuardarCambiosAsync(cancellationToken);
                throw new ExcepcionDeDominio(
                    "Superaste los 3 intentos fallidos. Debes esperar 1 hora antes de volver a intentarlo.",
                    "BLOQUEO_2FA_ACTIVO");

            case ResultadoValidacionSegundoFactor.CODIGO_INCORRECTO:
            default:
                await uow.GuardarCambiosAsync(cancellationToken);
                throw new ExcepcionDeDominio(
                    DesafioSegundoFactor.MensajeCodigoInvalido,
                    "SEGUNDO_FACTOR_INVALIDO");
        }
    }
}

public sealed class ReenviarSegundoFactorHandler(
    IUsuarioRepository usuarios,
    IDesafioSegundoFactorRepository desafios,
    ICanalEnvioCodigo canalEnvio,
    IGeneradorCodigoSeguro generadorCodigo,
    IRelojNegocio reloj,
    ConfiguracionAutenticacion2Fa config,
    IUnidadTrabajoIdentidad uow) : IRequestHandler<ReenviarSegundoFactorCommand, ResultadoInicioSesionPaso1>
{
    public async Task<ResultadoInicioSesionPaso1> Handle(ReenviarSegundoFactorCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.ObtenerPorIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El usuario no existe.", "USUARIO_NO_ENCONTRADO");

        var ahoraUtc = reloj.AhoraUtc;
        if (usuario.EstaBloqueado2Fa(ahoraUtc))
        {
            throw new ExcepcionDeDominio(
                "Superaste el máximo de intentos fallidos. Debes esperar 1 hora para volver a intentarlo.",
                "BLOQUEO_2FA_ACTIVO");
        }

        // C14: el reenvío invalida inmediatamente cualquier código anterior activo
        var activos = await desafios.ObtenerActivosPorUsuarioAsync(usuario.Id, cancellationToken);
        foreach (var previo in activos)
        {
            previo.Invalidar(ahoraUtc);
        }

        var canal = usuario.ResolverCanalSegundoFactor(esRecuperacion: false);
        var destino = canal == CanalCodigo.CORREO ? usuario.Contacto.Correo : usuario.Contacto.Telefono;
        var codigoPlano = generadorCodigo.GenerarCodigoNumerico(6);

        var nuevoDesafio = DesafioSegundoFactor.Crear(
            usuario.Id,
            canal,
            codigoPlano,
            ahoraUtc,
            config.VigenciaMinutos,
            esRecuperacionCorreo: false);

        await canalEnvio.EnviarCodigoSegundoFactorAsync(
            canal,
            destino,
            codigoPlano,
            config.VigenciaMinutos,
            esRecuperacion: false,
            cancellationToken);

        await desafios.AgregarAsync(nuevoDesafio, cancellationToken);
        await uow.GuardarCambiosAsync(cancellationToken);

        return new ResultadoInicioSesionPaso1(
            RequiereSegundoFactor: true,
            DesafioId: nuevoDesafio.Id,
            CanalEnviado: canal,
            ExpiraEnUtc: nuevoDesafio.ExpiraEnUtc,
            Sesion: null,
            Mensaje: "Se envió un nuevo código de verificación.");
    }
}

public sealed class RecuperarSegundoFactorHandler(
    IUsuarioRepository usuarios,
    IDesafioSegundoFactorRepository desafios,
    ICanalEnvioCodigo canalEnvio,
    IGeneradorCodigoSeguro generadorCodigo,
    IRelojNegocio reloj,
    ConfiguracionAutenticacion2Fa config,
    IUnidadTrabajoIdentidad uow) : IRequestHandler<RecuperarSegundoFactorCommand, ResultadoInicioSesionPaso1>
{
    public async Task<ResultadoInicioSesionPaso1> Handle(RecuperarSegundoFactorCommand request, CancellationToken cancellationToken)
    {
        var correo = (request.Correo ?? string.Empty).Trim().ToLowerInvariant();
        var usuario = await usuarios.ObtenerPorCorreoAsync(correo, cancellationToken)
            ?? throw new ExcepcionDeDominio("No existe una cuenta activa asociada a ese correo.", "USUARIO_NO_ENCONTRADO");

        var ahoraUtc = reloj.AhoraUtc;
        if (usuario.EstaBloqueado2Fa(ahoraUtc))
        {
            throw new ExcepcionDeDominio(
                "Superaste el máximo de intentos fallidos. Debes esperar 1 hora para volver a intentarlo.",
                "BLOQUEO_2FA_ACTIVO");
        }

        var activos = await desafios.ObtenerActivosPorUsuarioAsync(usuario.Id, cancellationToken);
        foreach (var previo in activos)
        {
            previo.Invalidar(ahoraUtc);
        }

        // HU-37.2 Escenario 12 / R4e: recuperación siempre por correo
        var canal = usuario.ResolverCanalSegundoFactor(esRecuperacion: true);
        var codigoPlano = generadorCodigo.GenerarCodigoNumerico(6);

        var desafioRecuperacion = DesafioSegundoFactor.Crear(
            usuario.Id,
            canal,
            codigoPlano,
            ahoraUtc,
            config.VigenciaMinutos,
            esRecuperacionCorreo: true);

        await canalEnvio.EnviarCodigoSegundoFactorAsync(
            canal,
            usuario.Contacto.Correo,
            codigoPlano,
            config.VigenciaMinutos,
            esRecuperacion: true,
            cancellationToken);

        await desafios.AgregarAsync(desafioRecuperacion, cancellationToken);
        await uow.GuardarCambiosAsync(cancellationToken);

        return new ResultadoInicioSesionPaso1(
            RequiereSegundoFactor: true,
            DesafioId: desafioRecuperacion.Id,
            CanalEnviado: CanalCodigo.CORREO,
            ExpiraEnUtc: desafioRecuperacion.ExpiraEnUtc,
            Sesion: null,
            Mensaje: "Se envió un código de recuperación a tu correo registrado.");
    }
}
