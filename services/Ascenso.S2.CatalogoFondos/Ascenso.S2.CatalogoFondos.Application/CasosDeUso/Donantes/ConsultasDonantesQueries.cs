using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using MediatR;

namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Donantes;

public sealed record ListarDonantesQuery(ActorSolicitudCatalogo Actor)
    : IRequest<IReadOnlyList<DonanteResumenDto>>;

public sealed class ListarDonantesHandler
    : IRequestHandler<ListarDonantesQuery, IReadOnlyList<DonanteResumenDto>>
{
    private readonly IDonanteRepository _donanteRepo;

    public ListarDonantesHandler(IDonanteRepository donanteRepo) => _donanteRepo = donanteRepo;

    public async Task<IReadOnlyList<DonanteResumenDto>> Handle(
        ListarDonantesQuery request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionDonantes.ExigirLecturaHistorial(request.Actor);

        var todos = await _donanteRepo.ListarTodosAsync(cancellationToken);
        return todos
            .OrderBy(d => d.Nombre, StringComparer.OrdinalIgnoreCase)
            .Select(RegistrarDonanteHandler.MapearResumen)
            .ToArray();
    }
}

public sealed record ConsultarDonanteDetalleQuery(
    ActorSolicitudCatalogo Actor,
    Guid DonanteId) : IRequest<DonanteResumenDto>;

public sealed class ConsultarDonanteDetalleHandler
    : IRequestHandler<ConsultarDonanteDetalleQuery, DonanteResumenDto>
{
    private readonly IDonanteRepository _donanteRepo;

    public ConsultarDonanteDetalleHandler(IDonanteRepository donanteRepo) => _donanteRepo = donanteRepo;

    public async Task<DonanteResumenDto> Handle(
        ConsultarDonanteDetalleQuery request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionDonantes.ExigirLecturaHistorial(request.Actor);

        var donante = await _donanteRepo.ObtenerPorIdAsync(request.DonanteId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El donante indicado no está registrado.", "DONANTE_NO_ENCONTRADO");

        return RegistrarDonanteHandler.MapearResumen(donante);
    }
}

public sealed record ConsultarHistorialAportesDonanteQuery(
    ActorSolicitudCatalogo Actor,
    Guid DonanteId,
    string? CodigoPeriodo = null) : IRequest<HistorialAportesDonanteDto>;

public sealed class ConsultarHistorialAportesDonanteHandler
    : IRequestHandler<ConsultarHistorialAportesDonanteQuery, HistorialAportesDonanteDto>
{
    private readonly IDonanteRepository _donanteRepo;
    private readonly IProgramaRepository _programaRepo;

    public ConsultarHistorialAportesDonanteHandler(
        IDonanteRepository donanteRepo,
        IProgramaRepository programaRepo)
    {
        _donanteRepo = donanteRepo;
        _programaRepo = programaRepo;
    }

    public async Task<HistorialAportesDonanteDto> Handle(
        ConsultarHistorialAportesDonanteQuery request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionDonantes.ExigirLecturaHistorial(request.Actor);

        var donante = await _donanteRepo.ObtenerPorIdAsync(request.DonanteId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El donante indicado no está registrado.", "DONANTE_NO_ENCONTRADO");

        CodigoPeriodo? filtro = string.IsNullOrWhiteSpace(request.CodigoPeriodo)
            ? null
            : new CodigoPeriodo(request.CodigoPeriodo);

        var aportes = donante.ObtenerHistorial(filtro);

        string? mensajeSinAportes = null;
        if (aportes.Count == 0)
        {
            mensajeSinAportes = filtro is null
                ? Donante.MensajeSinAportes
                : Donante.MensajeSinAportesEnPeriodo(filtro);
        }

        var programas = await _programaRepo.ListarTodosAsync(cancellationToken);
        var nombresPrograma = programas.ToDictionary(p => p.Id, p => p.Nombre);

        var aportesDto = aportes
            .Select(a => new AporteDto(
                a.Id,
                donante.Id,
                donante.Nombre,
                a.ProgramaId,
                nombresPrograma.TryGetValue(a.ProgramaId, out var nombrePrograma)
                    ? nombrePrograma
                    : a.ProgramaId.ToString(),
                a.Periodo.Valor,
                a.Monto.Cantidad,
                a.Monto.Moneda,
                a.RegistradoEnUtc))
            .ToArray();

        return new HistorialAportesDonanteDto(
            donante.Id,
            donante.Nombre,
            donante.Tipo.ToString(),
            filtro?.Valor,
            aportes.Sum(a => a.Monto.Cantidad),
            mensajeSinAportes,
            aportesDto);
    }
}
