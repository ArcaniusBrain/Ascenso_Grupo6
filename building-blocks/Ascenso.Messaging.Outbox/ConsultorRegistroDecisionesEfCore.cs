using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.Trazabilidad;
using Microsoft.EntityFrameworkCore;

namespace Ascenso.Messaging.Outbox;

/// <summary>
/// Implementación de solo lectura (AsNoTracking) de IConsultaRegistroDecisiones sobre EF Core (HU-35.2).
/// Aplica la autorización por función y alcance en la consulta de datos, ordena cronológicamente y nunca muta el registro.
/// </summary>
public sealed class ConsultorRegistroDecisionesEfCore : IConsultaRegistroDecisiones
{
    private readonly DbContext _dbContext;
    private readonly IRelojNegocio _reloj;

    public ConsultorRegistroDecisionesEfCore(DbContext dbContext, IRelojNegocio reloj)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _reloj = reloj ?? throw new ArgumentNullException(nameof(reloj));
    }

    public async Task<ResultadoConsultaDecisiones> ConsultarPorSujetoAsync(
        SujetoRef sujeto,
        ContextoSolicitanteDecision solicitante,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sujeto);
        ArgumentNullException.ThrowIfNull(solicitante);

        var alcance = PoliticaAutorizacionDecision.ResolverAlcanceConsulta(solicitante);

        var registrosSujeto = await _dbContext.Set<RegistroDecision>()
            .AsNoTracking()
            .Where(x => x.Sujeto.Tipo == sujeto.Tipo && x.Sujeto.Id == sujeto.Id)
            .ToListAsync(cancellationToken);

        var filtrados = FiltrarYValidarPorAlcanceEnSujeto(registrosSujeto, solicitante, alcance);

        var ordenados = filtrados
            .OrderBy(x => x.OcurridoEnUtc)
            .ThenBy(x => x.Id)
            .Select(Mapear)
            .ToList();

        if (ordenados.Count == 0)
        {
            return ResultadoConsultaDecisiones.Vacio(_reloj.AhoraUtc);
        }

        return new ResultadoConsultaDecisiones(
            ActualizadoEnUtc: _reloj.AhoraUtc,
            Items: ordenados,
            Total: ordenados.Count);
    }

    public async Task<ResultadoConsultaDecisiones> ConsultarAsync(
        ConsultaRegistroDecisionesFiltro filtro,
        ContextoSolicitanteDecision solicitante,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        ArgumentNullException.ThrowIfNull(solicitante);

        var alcance = PoliticaAutorizacionDecision.ResolverAlcanceConsulta(solicitante);

        if (alcance == AlcanceConsultaDecisiones.SOLO_PROGRAMAS_ASIGNADOS
            && filtro.ProgramaId.HasValue
            && !solicitante.ProgramasEfectivos.Contains(filtro.ProgramaId.Value))
        {
            throw new ExcepcionAccesoDenegadoDecision(
                "No tienes permiso para consultar decisiones de un programa que no coordinas.");
        }

        var query = _dbContext.Set<RegistroDecision>().AsNoTracking().AsQueryable();

        if (filtro.TipoSujeto.HasValue)
        {
            query = query.Where(x => x.Sujeto.Tipo == filtro.TipoSujeto.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.SujetoId))
        {
            var sujetoId = filtro.SujetoId.Trim();
            query = query.Where(x => x.Sujeto.Id == sujetoId);
        }

        if (filtro.ProgramaId.HasValue)
        {
            query = query.Where(x => x.ProgramaId == filtro.ProgramaId.Value);
        }

        var candidatos = await query.ToListAsync(cancellationToken);

        IEnumerable<RegistroDecision> autorizados;
        if (filtro.TipoSujeto.HasValue && !string.IsNullOrWhiteSpace(filtro.SujetoId))
        {
            autorizados = FiltrarYValidarPorAlcanceEnSujeto(candidatos, solicitante, alcance);
        }
        else
        {
            autorizados = candidatos.Where(r => PoliticaAutorizacionDecision.PuedeVerRegistro(solicitante, r));
        }

        if (filtro.DesdeUtc.HasValue)
        {
            var desde = filtro.DesdeUtc.Value.ToUniversalTime();
            autorizados = autorizados.Where(x => x.OcurridoEnUtc >= desde);
        }

        if (filtro.HastaUtc.HasValue)
        {
            var hasta = filtro.HastaUtc.Value.ToUniversalTime();
            autorizados = autorizados.Where(x => x.OcurridoEnUtc <= hasta);
        }

        var listaOrdenada = autorizados
            .OrderBy(x => x.OcurridoEnUtc)
            .ThenBy(x => x.Id)
            .ToList();

        var total = listaOrdenada.Count;
        if (total == 0)
        {
            return ResultadoConsultaDecisiones.Vacio(_reloj.AhoraUtc);
        }

        var pagina = Math.Max(1, filtro.Pagina);
        var tamano = Math.Clamp(filtro.TamanoPagina, 1, 200);

        var paginados = listaOrdenada
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(Mapear)
            .ToList();

        return new ResultadoConsultaDecisiones(
            ActualizadoEnUtc: _reloj.AhoraUtc,
            Items: paginados,
            Total: total);
    }

    private static List<RegistroDecision> FiltrarYValidarPorAlcanceEnSujeto(
        List<RegistroDecision> registrosSujeto,
        ContextoSolicitanteDecision solicitante,
        AlcanceConsultaDecisiones alcance)
    {
        if (registrosSujeto.Count == 0)
        {
            return registrosSujeto;
        }

        switch (alcance)
        {
            case AlcanceConsultaDecisiones.TODOS:
                return registrosSujeto;

            case AlcanceConsultaDecisiones.SOLO_CASO_PROPIO:
            {
                var propios = registrosSujeto
                    .Where(r => r.PropietarioUsuarioId.HasValue && r.PropietarioUsuarioId.Value == solicitante.UsuarioId)
                    .ToList();

                if (propios.Count == 0)
                {
                    throw new ExcepcionAccesoDenegadoDecision(
                        "No tienes permiso para consultar el registro de decisiones de otro postulante o becario.");
                }

                return propios;
            }

            case AlcanceConsultaDecisiones.SOLO_PROGRAMAS_ASIGNADOS:
            {
                var deSusProgramas = registrosSujeto
                    .Where(r => r.ProgramaId.HasValue && solicitante.ProgramasEfectivos.Contains(r.ProgramaId.Value))
                    .ToList();

                if (deSusProgramas.Count == 0)
                {
                    throw new ExcepcionAccesoDenegadoDecision(
                        "No tienes permiso para consultar decisiones de un programa que no coordinas.");
                }

                return deSusProgramas;
            }

            case AlcanceConsultaDecisiones.SOLO_AMBITO_FINANCIERO:
                return registrosSujeto
                    .Where(r => r.Tipo.EsAmbitoFinanciero())
                    .ToList();

            default:
                throw new ExcepcionAccesoDenegadoDecision("No tienes permiso para consultar el registro de decisiones.");
        }
    }

    private static DecisionProyectada Mapear(RegistroDecision r) => new(
        Id: r.Id,
        Tipo: r.Tipo,
        TipoSujeto: r.Sujeto.Tipo,
        SujetoId: r.Sujeto.Id,
        ProgramaId: r.ProgramaId,
        PropietarioUsuarioId: r.PropietarioUsuarioId,
        ActorUsuarioId: r.Actor.UsuarioId,
        ActorRol: r.Actor.Rol,
        ActorEsSistema: r.Actor.EsSistema,
        OcurridoEnUtc: r.OcurridoEnUtc,
        Justificacion: r.Justificacion.Texto,
        EstadoAnterior: r.EstadoAnterior,
        EstadoNuevo: r.EstadoNuevo,
        ServicioOrigen: r.ServicioOrigen,
        CorrelationId: r.CorrelationId,
        DatosJson: r.DatosJson);
}
