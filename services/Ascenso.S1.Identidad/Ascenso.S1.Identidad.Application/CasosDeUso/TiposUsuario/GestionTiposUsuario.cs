using Ascenso.S1.Identidad.Application.Puertos;
using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.S1.Identidad.Domain.Repositorios;
using Ascenso.S1.Identidad.Domain.Servicios;
using Ascenso.SharedKernel.TiposBase;
using MediatR;

namespace Ascenso.S1.Identidad.Application.CasosDeUso.TiposUsuario;

public sealed record TipoUsuarioDto(
    Guid Id,
    string Nombre,
    IReadOnlyCollection<string> Permisos,
    bool AccesoFinancieroODecision,
    bool EsEnlaceUniversidad,
    bool ExigeSegundoFactor,
    bool EsPredefinido);

public sealed record CrearTipoUsuarioCommand(
    Guid ActorUsuarioId,
    string? Nombre,
    IReadOnlyCollection<string>? Permisos,
    bool AccesoFinancieroODecision = false,
    bool EsEnlaceUniversidad = false) : IRequest<TipoUsuarioDto>;

public sealed record ActualizarPermisosTipoUsuarioCommand(
    Guid ActorUsuarioId,
    Guid TipoUsuarioId,
    IReadOnlyCollection<string> NuevosPermisos,
    bool? AccesoFinancieroExplicito = null) : IRequest<TipoUsuarioDto>;

public sealed record EliminarTipoUsuarioCommand(
    Guid ActorUsuarioId,
    Guid TipoUsuarioId) : IRequest<Unit>;

public sealed record ListarTiposUsuarioQuery : IRequest<IReadOnlyList<TipoUsuarioDto>>;

public sealed class CrearTipoUsuarioHandler(
    IUsuarioRepository usuarios,
    ITipoUsuarioRepository tipos,
    IUnidadTrabajoIdentidad uow) : IRequestHandler<CrearTipoUsuarioCommand, TipoUsuarioDto>
{
    public async Task<TipoUsuarioDto> Handle(CrearTipoUsuarioCommand request, CancellationToken cancellationToken)
    {
        var actor = await usuarios.ObtenerPorIdAsync(request.ActorUsuarioId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El usuario solicitante no existe.", "ACTOR_NO_ENCONTRADO");
        var tiposActor = await tipos.ObtenerPorIdsAsync(actor.TipoUsuarioIds, cancellationToken);

        MotorAutorizacionS1.ValidarGestionIdentidadPorDireccion(
            actor.Id,
            tiposActor,
            CatalogoPermisos.TiposUsuarioGestionar);

        var nuevoTipo = TipoUsuario.Crear(
            request.Nombre,
            request.Permisos,
            request.AccesoFinancieroODecision,
            request.EsEnlaceUniversidad);

        var existente = await tipos.ObtenerPorNombreAsync(nuevoTipo.Nombre, cancellationToken);
        if (existente is not null)
        {
            throw new ExcepcionDeDominio(
                TipoUsuario.MensajeNombreDuplicado(nuevoTipo.Nombre),
                "TIPO_USUARIO_DUPLICADO");
        }

        await tipos.AgregarAsync(nuevoTipo, cancellationToken);
        await uow.GuardarCambiosAsync(cancellationToken);

        return Mapear(nuevoTipo);
    }

    internal static TipoUsuarioDto Mapear(TipoUsuario t) => new(
        t.Id,
        t.Nombre,
        t.Permisos.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray(),
        t.AccesoFinancieroODecision,
        t.EsEnlaceUniversidad,
        t.ExigeSegundoFactor(),
        t.EsPredefinido);
}

public sealed class ActualizarPermisosTipoUsuarioHandler(
    IUsuarioRepository usuarios,
    ITipoUsuarioRepository tipos,
    IProveedorIdentidadKeycloak keycloak,
    IUnidadTrabajoIdentidad uow) : IRequestHandler<ActualizarPermisosTipoUsuarioCommand, TipoUsuarioDto>
{
    public async Task<TipoUsuarioDto> Handle(ActualizarPermisosTipoUsuarioCommand request, CancellationToken cancellationToken)
    {
        var actor = await usuarios.ObtenerPorIdAsync(request.ActorUsuarioId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El usuario solicitante no existe.", "ACTOR_NO_ENCONTRADO");
        var tiposActor = await tipos.ObtenerPorIdsAsync(actor.TipoUsuarioIds, cancellationToken);

        MotorAutorizacionS1.ValidarGestionIdentidadPorDireccion(
            actor.Id,
            tiposActor,
            CatalogoPermisos.TiposUsuarioGestionar);

        var tipo = await tipos.ObtenerPorIdAsync(request.TipoUsuarioId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El tipo de usuario no existe.", "TIPO_USUARIO_NO_ENCONTRADO");

        tipo.ActualizarPermisos(request.NuevosPermisos, request.AccesoFinancieroExplicito);

        // C14: exigir nuevo inicio de sesión obligatorio a los usuarios afectados
        var usuariosAfectados = await usuarios.ObtenerPorTipoAsync(tipo.Id, cancellationToken);
        foreach (var usuario in usuariosAfectados)
        {
            usuario.InvalidarSesionesActivas();
            if (!string.IsNullOrWhiteSpace(usuario.KeycloakUserId))
            {
                await keycloak.CerrarSesionesUsuarioAsync(usuario.KeycloakUserId, cancellationToken);
            }
        }

        await uow.GuardarCambiosAsync(cancellationToken);
        return CrearTipoUsuarioHandler.Mapear(tipo);
    }
}

public sealed class EliminarTipoUsuarioHandler(
    IUsuarioRepository usuarios,
    ITipoUsuarioRepository tipos,
    IUnidadTrabajoIdentidad uow) : IRequestHandler<EliminarTipoUsuarioCommand, Unit>
{
    public async Task<Unit> Handle(EliminarTipoUsuarioCommand request, CancellationToken cancellationToken)
    {
        var actor = await usuarios.ObtenerPorIdAsync(request.ActorUsuarioId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El usuario solicitante no existe.", "ACTOR_NO_ENCONTRADO");
        var tiposActor = await tipos.ObtenerPorIdsAsync(actor.TipoUsuarioIds, cancellationToken);

        MotorAutorizacionS1.ValidarGestionIdentidadPorDireccion(
            actor.Id,
            tiposActor,
            CatalogoPermisos.TiposUsuarioGestionar);

        var tipo = await tipos.ObtenerPorIdAsync(request.TipoUsuarioId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El tipo de usuario no existe.", "TIPO_USUARIO_NO_ENCONTRADO");

        if (tipo.EsPredefinido)
        {
            throw new ExcepcionDeDominio("No se puede eliminar un tipo de usuario base del sistema.", "TIPO_PREDEFINIDO_PROTEGIDO");
        }

        if (await usuarios.ExistenUsuariosConTipoAsync(tipo.Id, cancellationToken))
        {
            throw new ExcepcionDeDominio(
                "No se puede eliminar un tipo de usuario que tiene usuarios asignados.",
                "TIPO_USUARIO_EN_USO");
        }

        tipos.Eliminar(tipo);
        await uow.GuardarCambiosAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed class ListarTiposUsuarioHandler(ITipoUsuarioRepository tipos)
    : IRequestHandler<ListarTiposUsuarioQuery, IReadOnlyList<TipoUsuarioDto>>
{
    public async Task<IReadOnlyList<TipoUsuarioDto>> Handle(ListarTiposUsuarioQuery request, CancellationToken cancellationToken)
    {
        var lista = await tipos.ObtenerTodosAsync(cancellationToken);
        return lista
            .OrderBy(t => t.Nombre, StringComparer.OrdinalIgnoreCase)
            .Select(CrearTipoUsuarioHandler.Mapear)
            .ToList();
    }
}
