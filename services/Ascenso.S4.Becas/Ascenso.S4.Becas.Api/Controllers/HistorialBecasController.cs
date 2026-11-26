using System.Security.Claims;
using Ascenso.S4.Becas.Application.HistorialBecas;
using Ascenso.S4.Becas.Domain.Becas;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ascenso.S4.Becas.Api.Controllers;

[ApiController]
[Route("api/v1/becas")]
public sealed class HistorialBecasController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpGet("historial")]
    [ProducesResponseType(typeof(HistorialBecasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerHistorial(
        [FromQuery] string? cedula,
        [FromQuery] Guid? programaId,
        [FromQuery] string? periodo,
        CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Problem(
                title: "No autenticado",
                detail: "Se requiere autenticación válida para consultar el historial de becas.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var rol = ResolverRol(User);
        var cedulaActor = User.FindFirstValue("cedula");

        var resultado = await _sender.Send(
            new ConsultarHistorialBecasQuery(rol, cedulaActor, cedula, programaId, periodo),
            cancellationToken);

        if (!resultado.Autorizado)
        {
            return Problem(
                title: "Acceso denegado",
                detail: resultado.Mensaje,
                statusCode: StatusCodes.Status403Forbidden);
        }

        if (resultado.CriterioInvalido)
        {
            return Problem(
                title: "Criterio de consulta inválido",
                detail: resultado.Mensaje,
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (resultado.NoEncontrado)
        {
            return Problem(
                title: "Recurso no encontrado",
                detail: resultado.Mensaje,
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(new HistorialBecasResponse(resultado.Items, resultado.Mensaje));
    }

    private static RolHistorialBecas ResolverRol(ClaimsPrincipal user)
    {
        var rolClaim = user.FindFirstValue(ClaimTypes.Role) ?? user.FindFirstValue("role") ?? string.Empty;
        return rolClaim.Trim().ToUpperInvariant() switch
        {
            "COORDINADOR" => RolHistorialBecas.Coordinador,
            "BECARIO" => RolHistorialBecas.Becario,
            "POSTULANTE" => RolHistorialBecas.Postulante,
            _ => RolHistorialBecas.Otro
        };
    }
}

public sealed record HistorialBecasResponse(
    IReadOnlyList<BecaHistorialItemDto> Items,
    string? Mensaje);
