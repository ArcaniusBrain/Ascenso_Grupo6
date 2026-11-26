using Ascenso.S4.Becas.Domain.Becas;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using MediatR;

namespace Ascenso.S4.Becas.Application.HistorialBecas;

public sealed class ConsultarHistorialBecasHandler(IConsultaHistorialBecas consulta)
    : IRequestHandler<ConsultarHistorialBecasQuery, HistorialBecasResultado>
{
    private readonly IConsultaHistorialBecas _consulta = consulta;

    public async Task<HistorialBecasResultado> Handle(
        ConsultarHistorialBecasQuery request,
        CancellationToken cancellationToken)
    {
        var criterios = (string.IsNullOrWhiteSpace(request.CedulaConsultada) ? 0 : 1)
                      + (request.ProgramaId.HasValue ? 1 : 0)
                      + (string.IsNullOrWhiteSpace(request.Periodo) ? 0 : 1);

        if (criterios != 1)
        {
            return HistorialBecasResultado.Invalido(
                "Debe indicar exactamente un criterio de consulta: cédula, programaId o periodo.");
        }

        if (!string.IsNullOrWhiteSpace(request.CedulaConsultada))
        {
            return await ConsultarPorCedulaAsync(request, cancellationToken);
        }

        if (!PoliticaAccesoHistorialBecas.PuedeConsultarAgregado(request.RolActor))
        {
            return HistorialBecasResultado.Denegado(
                "No tienes permiso para consultar el historial agregado por programa o por periodo.");
        }

        if (request.ProgramaId.HasValue)
        {
            return await ConsultarPorProgramaAsync(request.ProgramaId.Value, cancellationToken);
        }

        return await ConsultarPorPeriodoAsync(request.Periodo!, cancellationToken);
    }

    private async Task<HistorialBecasResultado> ConsultarPorCedulaAsync(
        ConsultarHistorialBecasQuery request,
        CancellationToken cancellationToken)
    {
        string cedulaNormalizada;
        try
        {
            cedulaNormalizada = new Cedula(request.CedulaConsultada!).Numero;
        }
        catch (ExcepcionDeDominio ex)
        {
            return HistorialBecasResultado.Invalido(ex.Message);
        }

        try
        {
            if (!PoliticaAccesoHistorialBecas.PuedeConsultarPorCedula(request.RolActor, request.CedulaActor, cedulaNormalizada))
            {
                return HistorialBecasResultado.Denegado(
                    "No tienes permiso para consultar el historial de becas de otra persona.");
            }
        }
        catch (ExcepcionDeDominio ex)
        {
            return HistorialBecasResultado.Invalido(ex.Message);
        }

        var persona = await _consulta.ObtenerPersonaPorCedulaAsync(cedulaNormalizada, cancellationToken);
        if (persona is null)
        {
            return HistorialBecasResultado.RecursoNoEncontrado(
                $"No se encontró a una persona con la identificación {cedulaNormalizada}.");
        }

        var becas = await _consulta.ListarPorPersonaIdAsync(persona.PersonaId, cancellationToken);
        return becas.Count == 0
            ? HistorialBecasResultado.Exito([], $"{persona.NombreCompleto} no tiene becas registradas.")
            : HistorialBecasResultado.Exito(becas);
    }

    private async Task<HistorialBecasResultado> ConsultarPorProgramaAsync(
        Guid programaId,
        CancellationToken cancellationToken)
    {
        if (programaId == Guid.Empty)
        {
            return HistorialBecasResultado.Invalido("El identificador del programa es obligatorio.");
        }

        var programa = await _consulta.ObtenerProgramaAsync(programaId, cancellationToken);
        if (programa is null)
        {
            return HistorialBecasResultado.RecursoNoEncontrado(
                $"No se encontró el programa {programaId}.");
        }

        var becas = await _consulta.ListarPorProgramaAsync(programaId, cancellationToken);
        return becas.Count == 0
            ? HistorialBecasResultado.Exito([], $"No hay becas en el programa {programa.Nombre}.")
            : HistorialBecasResultado.Exito(becas);
    }

    private async Task<HistorialBecasResultado> ConsultarPorPeriodoAsync(
        string periodoRaw,
        CancellationToken cancellationToken)
    {
        string periodoNormalizado;
        try
        {
            periodoNormalizado = new CodigoPeriodo(periodoRaw).Valor;
        }
        catch (ExcepcionDeDominio ex)
        {
            return HistorialBecasResultado.Invalido(ex.Message);
        }

        var becas = await _consulta.ListarPorPeriodoAsync(periodoNormalizado, cancellationToken);
        return becas.Count == 0
            ? HistorialBecasResultado.Exito([], $"No hay becas para el periodo {periodoNormalizado}.")
            : HistorialBecasResultado.Exito(becas);
    }
}
