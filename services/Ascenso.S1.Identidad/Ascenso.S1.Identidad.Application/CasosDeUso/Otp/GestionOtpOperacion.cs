using Ascenso.S1.Identidad.Application.Puertos;
using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.S1.Identidad.Domain.Repositorios;
using Ascenso.SharedKernel.Seguridad;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;
using MediatR;

namespace Ascenso.S1.Identidad.Application.CasosDeUso.Otp;

public sealed record SolicitarDesafioOtpCommand(
    Guid UsuarioId,
    TipoOperacionSensible Operacion,
    string ResourceId,
    CanalCodigo Canal) : IRequest<DesafioOtp>;

public sealed record OtpInternoResultado(bool Consumido, Guid ChallengeId);

public sealed record ValidarYConsumirOtpCommand(
    Guid UsuarioId,
    TipoOperacionSensible Operacion,
    string RecursoId,
    Guid ChallengeId,
    string Codigo,
    Guid OperationId) : IRequest<OtpInternoResultado>;

public sealed class SolicitarDesafioOtpHandler(
    IUsuarioRepository usuarios,
    IDesafioOtpOperacionRepository desafiosOtp,
    IBloqueoOtpOperacionRepository bloqueosOtp,
    ICanalEnvioCodigo canalEnvio,
    IGeneradorCodigoSeguro generadorCodigo,
    IRelojNegocio reloj,
    ConfiguracionAutenticacion2Fa config,
    IUnidadTrabajoIdentidad uow) : IRequestHandler<SolicitarDesafioOtpCommand, DesafioOtp>
{
    public async Task<DesafioOtp> Handle(SolicitarDesafioOtpCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.ObtenerPorIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El usuario solicitante no existe.", "USUARIO_NO_ENCONTRADO");

        if (usuario.Estado != EstadoUsuario.ACTIVO)
        {
            throw new ExcepcionDeDominio("La cuenta de usuario se encuentra desactivada.", "USUARIO_DESACTIVADO");
        }

        var recursoNormalizado = DesafioOtpOperacion.NormalizarRecurso(request.ResourceId);
        if (recursoNormalizado.Length == 0)
        {
            throw new ExcepcionDeDominio("El identificador del recurso es obligatorio para emitir un OTP.", "OTP_RECURSO_OBLIGATORIO");
        }

        var ahoraUtc = reloj.AhoraUtc;
        var bloqueo = await bloqueosOtp.ObtenerPorTernaAsync(usuario.Id, request.Operacion, recursoNormalizado, cancellationToken);
        if (bloqueo is not null && bloqueo.EstaVigente(ahoraUtc))
        {
            throw new ExcepcionDeDominio(
                "Superaste los 3 intentos fallidos para esta operación. Debes esperar 1 hora antes de solicitar un código nuevo.",
                "OTP_BLOQUEADO");
        }

        // Reenvío/solicitud nueva invalida desafíos activos previos de la misma terna (R4d, C14)
        var activosPrevios = await desafiosOtp.ObtenerActivosPorTernaAsync(usuario.Id, request.Operacion, recursoNormalizado, cancellationToken);
        foreach (var previo in activosPrevios)
        {
            previo.Invalidar(ahoraUtc);
        }

        var codigoPlano = generadorCodigo.GenerarCodigoNumerico(6);
        var nuevoDesafio = DesafioOtpOperacion.Crear(
            usuarioId: usuario.Id,
            operacion: request.Operacion,
            recursoId: recursoNormalizado,
            canal: request.Canal,
            codigoPlano: codigoPlano,
            ahoraUtc: ahoraUtc,
            vigenciaMinutos: config.VigenciaMinutos);

        var destino = request.Canal == CanalCodigo.CORREO ? usuario.Contacto.Correo : usuario.Contacto.Telefono;

        // Si el envío falla, NO se persiste ningún desafío usable
        await canalEnvio.EnviarCodigoSegundoFactorAsync(
            request.Canal,
            destino,
            codigoPlano,
            config.VigenciaMinutos,
            esRecuperacion: false,
            cancellationToken);

        await desafiosOtp.AgregarAsync(nuevoDesafio, cancellationToken);
        await uow.GuardarCambiosAsync(cancellationToken);

        return new DesafioOtp(nuevoDesafio.Id, nuevoDesafio.ExpiraEnUtc);
    }
}

public sealed class ValidarYConsumirOtpHandler(
    IDesafioOtpOperacionRepository desafiosOtp,
    IBloqueoOtpOperacionRepository bloqueosOtp,
    IRelojNegocio reloj,
    ConfiguracionAutenticacion2Fa config,
    IUnidadTrabajoIdentidad uow) : IRequestHandler<ValidarYConsumirOtpCommand, OtpInternoResultado>
{
    public async Task<OtpInternoResultado> Handle(ValidarYConsumirOtpCommand request, CancellationToken cancellationToken)
    {
        if (request.ChallengeId == Guid.Empty)
        {
            throw new ExcepcionDeDominio(DesafioOtpOperacion.MensajeCodigoNoValido, "OTP_INVALIDO");
        }

        var recursoNormalizado = DesafioOtpOperacion.NormalizarRecurso(request.RecursoId);
        var ahoraUtc = reloj.AhoraUtc;

        var bloqueo = await bloqueosOtp.ObtenerPorTernaAsync(request.UsuarioId, request.Operacion, recursoNormalizado, cancellationToken);
        if (bloqueo is not null && bloqueo.EstaVigente(ahoraUtc))
        {
            throw new ExcepcionDeDominio(
                "Superaste los 3 intentos fallidos para esta operación. Debes esperar 1 hora antes de volver a intentarlo.",
                "OTP_BLOQUEADO");
        }

        var desafio = await desafiosOtp.ObtenerPorIdAsync(request.ChallengeId, cancellationToken)
            ?? throw new ExcepcionDeDominio(DesafioOtpOperacion.MensajeCodigoNoValido, "OTP_INVALIDO");

        var resultado = desafio.ValidarYConsumir(
            request.UsuarioId,
            request.Operacion,
            recursoNormalizado,
            request.Codigo,
            request.OperationId,
            ahoraUtc);

        switch (resultado)
        {
            case ResultadoConsumoOtp.CONSUMIDO:
                await uow.GuardarCambiosAsync(cancellationToken);
                return new OtpInternoResultado(true, desafio.Id);

            case ResultadoConsumoOtp.CONSUMIDO_IDEMPOTENTE:
                return new OtpInternoResultado(true, desafio.Id);

            case ResultadoConsumoOtp.BLOQUEO_POR_INTENTOS:
            {
                var hastaUtc = ahoraUtc.AddHours(config.HorasBloqueo);
                if (bloqueo is null)
                {
                    await bloqueosOtp.AgregarAsync(
                        BloqueoOtpOperacion.Crear(request.UsuarioId, request.Operacion, recursoNormalizado, hastaUtc),
                        cancellationToken);
                }
                else
                {
                    bloqueo.ExtenderHasta(hastaUtc);
                }

                await uow.GuardarCambiosAsync(cancellationToken);
                throw new ExcepcionDeDominio(
                    "Superaste los 3 intentos fallidos para esta operación. Debes esperar 1 hora antes de volver a intentarlo.",
                    "OTP_BLOQUEADO");
            }

            case ResultadoConsumoOtp.CODIGO_INCORRECTO:
                await uow.GuardarCambiosAsync(cancellationToken);
                throw new ExcepcionDeDominio(DesafioOtpOperacion.MensajeCodigoNoValido, "OTP_INVALIDO");

            case ResultadoConsumoOtp.EXPIRADO:
                throw new ExcepcionDeDominio(DesafioOtpOperacion.MensajeCodigoNoValido, "OTP_EXPIRADO");

            case ResultadoConsumoOtp.RECHAZADO_REUSO_O_INVALIDADO:
                throw new ExcepcionDeDominio(DesafioOtpOperacion.MensajeCodigoNoValido, "OTP_REUTILIZADO");

            case ResultadoConsumoOtp.VINCULO_NO_COINCIDE:
            default:
                throw new ExcepcionDeDominio(DesafioOtpOperacion.MensajeCodigoNoValido, "OTP_VINCULO_INVALIDO");
        }
    }
}

/// <summary>
/// Implementación directa del contrato transversal IServicioOtp de Ascenso.SharedKernel dentro de S1.
/// </summary>
public sealed class ServicioOtpAplicacion(IMediator mediator) : IServicioOtp
{
    public Task<DesafioOtp> SolicitarDesafioAsync(
        Guid usuarioId,
        TipoOperacionSensible operacion,
        string recursoId,
        CanalCodigo canal,
        CancellationToken cancellationToken = default) =>
        mediator.Send(new SolicitarDesafioOtpCommand(usuarioId, operacion, recursoId, canal), cancellationToken);

    public async Task<bool> ValidarYConsumirAsync(
        Guid usuarioId,
        TipoOperacionSensible operacion,
        string recursoId,
        Guid challengeId,
        string codigo,
        Guid operationId,
        CancellationToken cancellationToken = default)
    {
        var res = await mediator.Send(
            new ValidarYConsumirOtpCommand(usuarioId, operacion, recursoId, challengeId, codigo, operationId),
            cancellationToken);
        return res.Consumido;
    }
}
