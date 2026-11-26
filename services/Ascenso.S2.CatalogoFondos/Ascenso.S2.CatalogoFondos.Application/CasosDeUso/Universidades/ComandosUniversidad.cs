using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;
using MediatR;

namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Universidades;

public sealed record RegistrarUniversidadAliadaCommand(
    ActorSolicitudCatalogo Actor,
    string? Nombre,
    string? TerminosConvenio,
    ViaDatosAcademicos ViaDatosAcademicos,
    ContactoDesignadoDto? ContactoDesignado) : IRequest<UniversidadResumenDto>;

public sealed class RegistrarUniversidadAliadaHandler
    : IRequestHandler<RegistrarUniversidadAliadaCommand, UniversidadResumenDto>
{
    private readonly IUniversidadAliadaRepository _universidadRepo;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public RegistrarUniversidadAliadaHandler(
        IUniversidadAliadaRepository universidadRepo,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _universidadRepo = universidadRepo;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public async Task<UniversidadResumenDto> Handle(
        RegistrarUniversidadAliadaCommand request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionUniversidades.ExigirGestionUniversidad(request.Actor);

        var contacto = request.ContactoDesignado is null
            ? null
            : new ContactoDesignado(
                request.ContactoDesignado.NombreCompleto,
                request.ContactoDesignado.Correo,
                request.ContactoDesignado.Telefono);

        var universidad = UniversidadAliada.Crear(
            request.Nombre,
            request.TerminosConvenio,
            request.ViaDatosAcademicos,
            contacto,
            _reloj.AhoraUtc);

        if (await _universidadRepo.ExisteNombreNormalizadoAsync(universidad.NombreNormalizado, null, cancellationToken))
        {
            throw new ExcepcionDeDominio(
                $"Ya existe una universidad aliada registrada con el nombre '{universidad.Nombre}'.",
                "UNIVERSIDAD_DUPLICADA");
        }

        await _universidadRepo.AgregarAsync(universidad, cancellationToken);
        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        return MapearResumen(universidad);
    }

    internal static UniversidadResumenDto MapearResumen(UniversidadAliada u) =>
        new(
            u.Id,
            u.Nombre,
            u.Convenio.Terminos,
            u.Convenio.Via.ToString(),
            new ContactoDesignadoDto(
                u.ContactoDesignado.NombreCompleto,
                u.ContactoDesignado.Correo,
                u.ContactoDesignado.Telefono),
            u.Version);
}

public sealed record ActualizarConvenioUniversidadCommand(
    ActorSolicitudCatalogo Actor,
    Guid UniversidadId,
    string? Nombre,
    string? TerminosConvenio,
    ViaDatosAcademicos ViaDatosAcademicos,
    ContactoDesignadoDto? ContactoDesignado) : IRequest<UniversidadResumenDto>;

public sealed class ActualizarConvenioUniversidadHandler
    : IRequestHandler<ActualizarConvenioUniversidadCommand, UniversidadResumenDto>
{
    private readonly IUniversidadAliadaRepository _universidadRepo;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;

    public ActualizarConvenioUniversidadHandler(
        IUniversidadAliadaRepository universidadRepo,
        IUnidadTrabajoCatalogo unidadTrabajo)
    {
        _universidadRepo = universidadRepo;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<UniversidadResumenDto> Handle(
        ActualizarConvenioUniversidadCommand request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionUniversidades.ExigirGestionUniversidad(request.Actor);

        var universidad = await _universidadRepo.ObtenerPorIdAsync(request.UniversidadId, cancellationToken)
            ?? throw new ExcepcionDeDominio("La universidad aliada indicada no está registrada.", "UNIVERSIDAD_NO_ENCONTRADA");

        var contacto = request.ContactoDesignado is null
            ? null
            : new ContactoDesignado(
                request.ContactoDesignado.NombreCompleto,
                request.ContactoDesignado.Correo,
                request.ContactoDesignado.Telefono);

        var claveNueva = UniversidadAliada.NormalizarClaveTexto(request.Nombre);
        if (!string.IsNullOrEmpty(claveNueva)
            && await _universidadRepo.ExisteNombreNormalizadoAsync(claveNueva, universidad.Id, cancellationToken))
        {
            throw new ExcepcionDeDominio(
                $"Ya existe otra universidad aliada registrada con el nombre '{UniversidadAliada.LimpiarNombreVisible(request.Nombre)}'.",
                "UNIVERSIDAD_DUPLICADA");
        }

        universidad.ActualizarDatos(
            request.Nombre,
            request.TerminosConvenio,
            request.ViaDatosAcademicos,
            contacto);

        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        return RegistrarUniversidadAliadaHandler.MapearResumen(universidad);
    }
}

public sealed record AgregarCarreraElegibleCommand(
    ActorSolicitudCatalogo Actor,
    Guid UniversidadId,
    string? NombreCarrera) : IRequest<CarreraElegibleDto>;

public sealed class AgregarCarreraElegibleHandler
    : IRequestHandler<AgregarCarreraElegibleCommand, CarreraElegibleDto>
{
    private readonly IUniversidadAliadaRepository _universidadRepo;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;

    public AgregarCarreraElegibleHandler(
        IUniversidadAliadaRepository universidadRepo,
        IUnidadTrabajoCatalogo unidadTrabajo)
    {
        _universidadRepo = universidadRepo;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<CarreraElegibleDto> Handle(
        AgregarCarreraElegibleCommand request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionUniversidades.ExigirGestionUniversidad(request.Actor);

        var universidad = await _universidadRepo.ObtenerPorIdAsync(request.UniversidadId, cancellationToken)
            ?? throw new ExcepcionDeDominio("La universidad aliada indicada no está registrada.", "UNIVERSIDAD_NO_ENCONTRADA");

        var carrera = universidad.AgregarCarrera(request.NombreCarrera);
        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        return new CarreraElegibleDto(carrera.Id, carrera.Nombre);
    }
}
