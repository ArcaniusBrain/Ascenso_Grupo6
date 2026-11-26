using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.TiposBase;
using MediatR;

namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Programas;

public sealed record ListarProgramasQuery(ActorSolicitudCatalogo Actor)
    : IRequest<IReadOnlyList<ProgramaResumenDto>>;

public sealed class ListarProgramasHandler
    : IRequestHandler<ListarProgramasQuery, IReadOnlyList<ProgramaResumenDto>>
{
    private readonly IProgramaRepository _programaRepo;

    public ListarProgramasHandler(IProgramaRepository programaRepo) => _programaRepo = programaRepo;

    public async Task<IReadOnlyList<ProgramaResumenDto>> Handle(
        ListarProgramasQuery request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionProgramas.ExigirLectura(request.Actor);

        var todos = await _programaRepo.ListarTodosAsync(cancellationToken);
        return todos
            .OrderBy(p => p.Nombre, StringComparer.OrdinalIgnoreCase)
            .Select(CrearProgramaHandler.MapearResumen)
            .ToArray();
    }
}

public sealed record ConsultarProgramaDetalleQuery(
    ActorSolicitudCatalogo Actor,
    Guid ProgramaId) : IRequest<ProgramaDetalleDto>;

public sealed class ConsultarProgramaDetalleHandler
    : IRequestHandler<ConsultarProgramaDetalleQuery, ProgramaDetalleDto>
{
    public const string LeyendaSinAportes = "Sin aportes registrados";

    private readonly IProgramaRepository _programaRepo;
    private readonly IConsultorDonantesYAportes _consultorDonantes;
    private readonly IUniversidadAliadaRepository _universidadRepo;

    public ConsultarProgramaDetalleHandler(
        IProgramaRepository programaRepo,
        IConsultorDonantesYAportes consultorDonantes,
        IUniversidadAliadaRepository universidadRepo)
    {
        _programaRepo = programaRepo;
        _consultorDonantes = consultorDonantes;
        _universidadRepo = universidadRepo;
    }

    public async Task<ProgramaDetalleDto> Handle(
        ConsultarProgramaDetalleQuery request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionProgramas.ExigirLectura(request.Actor);

        var programa = await _programaRepo.ObtenerPorIdAsync(request.ProgramaId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El programa de becas indicado no está registrado.", "PROGRAMA_NO_ENCONTRADO");

        return await ConstruirDetalleAsync(programa, _consultorDonantes, _universidadRepo, cancellationToken);
    }

    internal static async Task<ProgramaDetalleDto> ConstruirDetalleAsync(
        Programa programa,
        IConsultorDonantesYAportes consultorDonantes,
        IUniversidadAliadaRepository universidadRepo,
        CancellationToken ct)
    {
        var aportesPorDonante = await consultorDonantes.ObtenerAportesPorProgramaAsync(programa.Id, ct);
        var donantesDto = new List<DonanteAporteResumenDto>();
        var presupuestoTotal = 0m;

        foreach (var asociacion in programa.Donantes.OrderBy(d => d.AsociadoEnUtc))
        {
            if (aportesPorDonante.TryGetValue(asociacion.DonanteId, out var aporte) && aporte.CantidadAportes > 0)
            {
                presupuestoTotal += aporte.TotalAportado;
                donantesDto.Add(new DonanteAporteResumenDto(
                    asociacion.DonanteId,
                    aporte.NombreDonante,
                    aporte.TotalAportado,
                    aporte.Moneda,
                    null));
            }
            else
            {
                var info = await consultorDonantes.ObtenerDonanteAsync(asociacion.DonanteId, ct);
                donantesDto.Add(new DonanteAporteResumenDto(
                    asociacion.DonanteId,
                    info?.Nombre ?? asociacion.DonanteId.ToString(),
                    0m,
                    programa.MontoPorPeriodo.Moneda,
                    LeyendaSinAportes));
            }
        }

        // Incluir también aportes de donantes que pudieran existir antes de asociar explícitamente
        foreach (var kv in aportesPorDonante)
        {
            if (programa.Donantes.Any(d => d.DonanteId == kv.Key))
            {
                continue;
            }

            if (kv.Value.CantidadAportes > 0)
            {
                presupuestoTotal += kv.Value.TotalAportado;
                donantesDto.Add(new DonanteAporteResumenDto(
                    kv.Key,
                    kv.Value.NombreDonante,
                    kv.Value.TotalAportado,
                    kv.Value.Moneda,
                    null));
            }
        }

        var restriccionesAgrupadas = new List<RestriccionElegibilidadDto>();
        foreach (var grupo in programa.Restricciones.GroupBy(r => r.UniversidadId))
        {
            var primero = grupo.First();
            var carreras = grupo
                .Where(x => !string.IsNullOrWhiteSpace(x.CarreraNombre))
                .Select(x => x.CarreraNombre!)
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            string? avisoSinCarreras = null;
            if (carreras.Length == 0)
            {
                var uni = await universidadRepo.ObtenerPorIdAsync(grupo.Key, ct);
                if (uni is not null && uni.Carreras.Count == 0)
                {
                    avisoSinCarreras = $"{uni.Nombre} no tiene carreras elegibles registradas.";
                }
            }

            restriccionesAgrupadas.Add(new RestriccionElegibilidadDto(
                grupo.Key,
                primero.NombreUniversidad,
                carreras,
                avisoSinCarreras));
        }

        CondicionesPermanenciaDto? condicionesDto = programa.Condiciones is null
            ? null
            : new CondicionesPermanenciaDto(
                programa.Condiciones.PromedioMinimo,
                programa.Condiciones.ExigeHorasServicio,
                programa.Condiciones.HorasServicio,
                programa.Condiciones.ObtenerLeyendaHorasServicio(),
                programa.Condiciones.ObtenerReportesExigidos(),
                programa.Condiciones.DuracionMaxPausaMeses,
                programa.Condiciones.DescripcionRenovacion);

        return new ProgramaDetalleDto(
            programa.Id,
            programa.Nombre,
            programa.PeriodoInicio.Valor,
            programa.CupoBecarios,
            programa.DuracionPeriodos,
            programa.MontoPorPeriodo.Cantidad,
            programa.MontoPorPeriodo.Moneda,
            programa.ModalidadPago.ToString(),
            programa.Alcance.ToString(),
            programa.PermiteAcumulacion,
            presupuestoTotal,
            donantesDto,
            restriccionesAgrupadas.Count == 0 ? Programa.MensajeSinRestriccion : null,
            restriccionesAgrupadas,
            condicionesDto,
            programa.Version);
    }
}
