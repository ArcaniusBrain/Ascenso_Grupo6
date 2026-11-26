using System.Security.Claims;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;
using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Infrastructure;
using Ascenso.SharedKernel.TiposBase;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddCatalogoFondosService(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = feature?.Error;

        var (status, title, code, detail) = exception switch
        {
            ExcepcionDeDominio ex => MapearExcepcionDeDominio(ex),
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Conflicto de concurrencia",
                "CONCURRENCIA_CONFLICTO",
                "El calendario académico fue modificado simultáneamente por otro usuario. Recarga e intenta nuevamente."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno del servidor",
                "ERROR_INTERNO",
                "Ocurrió un error inesperado.")
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["code"] = code;
        problem.Extensions["correlationId"] = Guid.NewGuid();

        await context.Response.WriteAsJsonAsync(problem);
    });
});

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Ascenso.S2.CatalogoFondos" }));

// ── Calendario académico (RF-38: HU-38.1 y HU-38.2) ───────────────────────────
app.MapGet("/api/v1/periodos", async (HttpContext http, IMediator mediator, CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var lista = await mediator.Send(new ListarPeriodosQuery(actor), ct);
    return Results.Ok(lista);
});

app.MapPost("/api/v1/periodos", async (
    HttpContext http,
    AbrirPeriodoRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var creado = await mediator.Send(new AbrirPeriodoCommand(actor, body.CodigoPeriodo), ct);
    return Results.Created($"/api/v1/periodos/{Uri.EscapeDataString(creado.Codigo)}", creado);
});

app.MapGet("/api/v1/periodos/{codigoPeriodo}", async (
    string codigoPeriodo,
    HttpContext http,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var detalle = await mediator.Send(new ConsultarPeriodoDetalleQuery(actor, codigoPeriodo), ct);
    return Results.Ok(detalle);
});

app.MapPost("/api/v1/periodos/{codigoPeriodo}/cerrar", async (
    string codigoPeriodo,
    HttpContext http,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var cerrado = await mediator.Send(new CerrarPeriodoCommand(actor, codigoPeriodo), ct);
    return Results.Ok(cerrado);
});

app.MapPost("/api/v1/periodos/{codigoPeriodo}/inicios-inscripciones", async (
    string codigoPeriodo,
    HttpContext http,
    RegistrarInicioInscripcionesRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var creado = await mediator.Send(
        new RegistrarInicioInscripcionesCommand(
            actor,
            codigoPeriodo,
            body.UniversidadId,
            body.NombreUniversidad,
            body.FechaInicio),
        ct);

    return Results.Created(
        $"/api/v1/periodos/{Uri.EscapeDataString(creado.CodigoPeriodo)}/inicios-inscripciones/{creado.Id}",
        creado);
});

app.MapPost("/api/v1/periodos/actual", async (
    HttpContext http,
    MarcarPeriodoRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var confirmacion = await mediator.Send(
        new MarcarPeriodoActualCommand(actor, body.CodigoPeriodo, body.OperationId),
        ct);

    return Results.Ok(confirmacion);
});

app.Run();

static IResult NoAutenticado(HttpContext http) =>
    Results.Problem(
        title: "No autenticado",
        detail: "Se requiere usuario autenticado.",
        statusCode: StatusCodes.Status401Unauthorized,
        instance: http.Request.Path);

static ActorSolicitudCatalogo? ResolverActor(HttpContext http)
{
    Guid? usuarioId = null;
    var sub = http.User.FindFirst("sub")?.Value;
    if (Guid.TryParse(sub, out var desdeClaim))
    {
        usuarioId = desdeClaim;
    }
    else if (http.Request.Headers.TryGetValue("X-Actor-Usuario-Id", out var headerId)
             && Guid.TryParse(headerId.ToString(), out var desdeHeader))
    {
        usuarioId = desdeHeader;
    }

    if (usuarioId is null)
    {
        return null;
    }

    var rol = http.User.FindFirst(ClaimTypes.Role)?.Value
              ?? http.User.FindFirst("role")?.Value
              ?? (http.Request.Headers.TryGetValue("X-Actor-Rol", out var headerRol)
                  ? headerRol.ToString()
                  : string.Empty);

    return new ActorSolicitudCatalogo(usuarioId.Value, rol);
}

static (int Status, string Title, string Code, string Detail) MapearExcepcionDeDominio(ExcepcionDeDominio ex) =>
    ex.Codigo switch
    {
        "NO_AUTENTICADO"
            => (StatusCodes.Status401Unauthorized, "No autenticado", ex.Codigo, ex.Message),
        "ACCESO_DENEGADO_CALENDARIO"
            => (StatusCodes.Status403Forbidden, "Acceso denegado", ex.Codigo, ex.Message),
        "PERIODO_NO_ENCONTRADO"
            => (StatusCodes.Status404NotFound, "Recurso no encontrado", ex.Codigo, ex.Message),
        "PERIODO_DUPLICADO" or "INICIO_INSCRIPCIONES_DUPLICADO"
            => (StatusCodes.Status409Conflict, "Conflicto de negocio", ex.Codigo, ex.Message),
        _ => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio incumplida", ex.Codigo, ex.Message)
    };

public sealed record AbrirPeriodoRequest(string? CodigoPeriodo);

public sealed record RegistrarInicioInscripcionesRequest(
    Guid UniversidadId,
    string? NombreUniversidad,
    DateOnly? FechaInicio);

public sealed record MarcarPeriodoRequest(string CodigoPeriodo, Guid OperationId);

public partial class Program;
