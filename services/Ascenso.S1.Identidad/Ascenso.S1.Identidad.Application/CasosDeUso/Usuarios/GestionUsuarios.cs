using Ascenso.S1.Identidad.Application.Puertos;
using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.S1.Identidad.Domain.Repositorios;
using Ascenso.S1.Identidad.Domain.Servicios;
using Ascenso.SharedKernel.Seguridad;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using MediatR;

namespace Ascenso.S1.Identidad.Application.CasosDeUso.Usuarios;

public sealed record UsuarioDto(
    Guid Id,
    string Cedula,
    string NombreCompleto,
    string Correo,
    string Telefono,
    CanalCodigo CanalPreferido,
    EstadoUsuario Estado,
    bool EsEvaluadorExterno,
    Guid? UniversidadId,
    IReadOnlyList<Guid> TipoUsuarioIds,
    IReadOnlyList<string> NombresRoles,
    bool RequiereSegundoFactor);

public sealed record CrearUsuarioInternoCommand(
    Guid ActorUsuarioId,
    string Cedula,
    string NombreCompleto,
    string Correo,
    string Telefono,
    IReadOnlyCollection<Guid>? TipoUsuarioIds,
    CanalCodigo CanalPreferido = CanalCodigo.CORREO,
    IReadOnlyCollection<Guid>? ProgramasAsignados = null,
    Guid? UniversidadId = null,
    bool EsEvaluadorExterno = false,
    string? ContrasenaTemporal = null) : IRequest<UsuarioDto>;

public sealed record CambiarRolesUsuarioCommand(
    Guid ActorUsuarioId,
    Guid UsuarioObjetivoId,
    IReadOnlyCollection<Guid> NuevosTipoUsuarioIds) : IRequest<UsuarioDto>;

public sealed record ListarUsuariosQuery(Guid ActorUsuarioId) : IRequest<IReadOnlyList<UsuarioDto>>;

public sealed class CrearUsuarioInternoHandler(
    IUsuarioRepository usuarios,
    ITipoUsuarioRepository tipos,
    IProveedorIdentidadKeycloak keycloak,
    IUnidadTrabajoIdentidad uow) : IRequestHandler<CrearUsuarioInternoCommand, UsuarioDto>
{
    public async Task<UsuarioDto> Handle(CrearUsuarioInternoCommand request, CancellationToken cancellationToken)
    {
        var actor = await usuarios.ObtenerPorIdAsync(request.ActorUsuarioId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El usuario solicitante no existe.", "ACTOR_NO_ENCONTRADO");
        var tiposActor = await tipos.ObtenerPorIdsAsync(actor.TipoUsuarioIds, cancellationToken);

        MotorAutorizacionS1.ValidarGestionIdentidadPorDireccion(
            actor.Id,
            tiposActor,
            CatalogoPermisos.UsuariosGestionar);

        var cedula = new Cedula(request.Cedula);
        var contacto = new Contacto(request.Correo, request.Telefono);

        if (request.TipoUsuarioIds is null || request.TipoUsuarioIds.Count == 0)
        {
            throw new ExcepcionDeDominio(Usuario.MensajeRolObligatorio, "USUARIO_ROL_OBLIGATORIO");
        }

        if (await usuarios.ObtenerPorCedulaAsync(cedula, cancellationToken) is not null)
        {
            throw new ExcepcionDeDominio("Ya existe un usuario registrado con esa cédula.", "CEDULA_DUPLICADA");
        }

        if (await usuarios.ObtenerPorCorreoAsync(contacto.Correo, cancellationToken) is not null)
        {
            throw new ExcepcionDeDominio("Ya existe un usuario registrado con ese correo electrónico.", "CORREO_DUPLICADO");
        }

        var tiposResueltos = await tipos.ObtenerPorIdsAsync(request.TipoUsuarioIds, cancellationToken);
        if (tiposResueltos.Count != request.TipoUsuarioIds.Distinct().Count())
        {
            throw new ExcepcionDeDominio("Uno o más roles indicados no existen.", "TIPO_USUARIO_NO_ENCONTRADO");
        }

        var keycloakId = await keycloak.CrearUsuarioAsync(
            contacto.Correo,
            request.NombreCompleto,
            request.ContrasenaTemporal,
            cancellationToken);

        var nuevoUsuario = Usuario.Crear(
            cedula: cedula,
            nombreCompleto: request.NombreCompleto,
            contacto: contacto,
            tipoUsuarioIds: request.TipoUsuarioIds,
            canalPreferido: request.CanalPreferido,
            programasAsignados: request.ProgramasAsignados,
            universidadId: request.UniversidadId,
            esEvaluadorExterno: request.EsEvaluadorExterno,
            keycloakUserId: keycloakId);

        await usuarios.AgregarAsync(nuevoUsuario, cancellationToken);
        await uow.GuardarCambiosAsync(cancellationToken);

        return Mapear(nuevoUsuario, tiposResueltos);
    }

    internal static UsuarioDto Mapear(Usuario u, IReadOnlyList<TipoUsuario> tiposResueltos) => new(
        Id: u.Id,
        Cedula: u.Cedula.Numero,
        NombreCompleto: u.NombreCompleto,
        Correo: u.Contacto.Correo,
        Telefono: u.Contacto.Telefono,
        CanalPreferido: u.CanalPreferido,
        Estado: u.Estado,
        EsEvaluadorExterno: u.EsEvaluadorExterno,
        UniversidadId: u.UniversidadId,
        TipoUsuarioIds: u.TipoUsuarioIds.ToArray(),
        NombresRoles: tiposResueltos.Select(t => t.Nombre).OrderBy(n => n).ToArray(),
        RequiereSegundoFactor: u.RequiereSegundoFactor(tiposResueltos));
}

public sealed class CambiarRolesUsuarioHandler(
    IUsuarioRepository usuarios,
    ITipoUsuarioRepository tipos,
    IProveedorIdentidadKeycloak keycloak,
    IUnidadTrabajoIdentidad uow) : IRequestHandler<CambiarRolesUsuarioCommand, UsuarioDto>
{
    public async Task<UsuarioDto> Handle(CambiarRolesUsuarioCommand request, CancellationToken cancellationToken)
    {
        var actor = await usuarios.ObtenerPorIdAsync(request.ActorUsuarioId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El usuario solicitante no existe.", "ACTOR_NO_ENCONTRADO");
        var tiposActor = await tipos.ObtenerPorIdsAsync(actor.TipoUsuarioIds, cancellationToken);

        // HU-37.1 Escenario 8: bloquea auto-escalamiento y cualquier cambio de rol por quien no es Dirección
        MotorAutorizacionS1.ValidarAsignacionDeRoles(actor.Id, tiposActor, request.UsuarioObjetivoId);

        var objetivo = await usuarios.ObtenerPorIdAsync(request.UsuarioObjetivoId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El usuario indicado no existe.", "USUARIO_NO_ENCONTRADO");

        var tiposResueltos = await tipos.ObtenerPorIdsAsync(request.NuevosTipoUsuarioIds, cancellationToken);
        if (tiposResueltos.Count != request.NuevosTipoUsuarioIds.Distinct().Count())
        {
            throw new ExcepcionDeDominio("Uno o más roles indicados no existen.", "TIPO_USUARIO_NO_ENCONTRADO");
        }

        objetivo.AsignarRoles(request.NuevosTipoUsuarioIds);

        // C14: forzar nuevo inicio de sesión obligatorio
        if (!string.IsNullOrWhiteSpace(objetivo.KeycloakUserId))
        {
            await keycloak.CerrarSesionesUsuarioAsync(objetivo.KeycloakUserId, cancellationToken);
        }

        await uow.GuardarCambiosAsync(cancellationToken);
        return CrearUsuarioInternoHandler.Mapear(objetivo, tiposResueltos);
    }
}

public sealed class ListarUsuariosHandler(
    IUsuarioRepository usuarios,
    ITipoUsuarioRepository tipos) : IRequestHandler<ListarUsuariosQuery, IReadOnlyList<UsuarioDto>>
{
    public async Task<IReadOnlyList<UsuarioDto>> Handle(ListarUsuariosQuery request, CancellationToken cancellationToken)
    {
        var actor = await usuarios.ObtenerPorIdAsync(request.ActorUsuarioId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El usuario solicitante no existe.", "ACTOR_NO_ENCONTRADO");
        var tiposActor = await tipos.ObtenerPorIdsAsync(actor.TipoUsuarioIds, cancellationToken);

        MotorAutorizacionS1.ValidarGestionIdentidadPorDireccion(
            actor.Id,
            tiposActor,
            CatalogoPermisos.UsuariosGestionar);

        var todosTipos = await tipos.ObtenerTodosAsync(cancellationToken);
        var mapaTipos = todosTipos.ToDictionary(t => t.Id);
        var todosUsuarios = await usuarios.ObtenerTodosAsync(cancellationToken);

        return todosUsuarios
            .OrderBy(u => u.NombreCompleto, StringComparer.OrdinalIgnoreCase)
            .Select(u => CrearUsuarioInternoHandler.Mapear(
                u,
                u.TipoUsuarioIds.Where(mapaTipos.ContainsKey).Select(id => mapaTipos[id]).ToList()))
            .ToList();
    }
}
