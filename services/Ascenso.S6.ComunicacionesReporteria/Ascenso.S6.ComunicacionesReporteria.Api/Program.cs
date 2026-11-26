using System.Security.Claims;
using System.Text.Json.Serialization;
using Ascenso.S6.ComunicacionesReporteria.Api.Workers;
using Ascenso.S6.ComunicacionesReporteria.Application.CasosDeUso.Consentimientos;
using Ascenso.S6.ComunicacionesReporteria.Application.CasosDeUso.ReportesImpacto;
using Ascenso.S6.ComunicacionesReporteria.Application.Puertos;
using Ascenso.S6.ComunicacionesReporteria.Infrastructure;
using Ascenso.S6.ComunicacionesReporteria.Infrastructure.Persistencia;
using Ascenso.SharedKernel.TiposBase;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddProblemDetails();
builder.Services.AddComunicacionesReporteriaService(builder.Configuration);
builder.Services.AddHostedService<ProcesadorReportesImpactoWorker>();

var app = builder.Build();

if (args.Contains("--migrar", StringComparer.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ComunicacionesDbContext>();
    await db.Database.MigrateAsync();
    Console.WriteLine($"S6 migrado correctamente (schema '{ComunicacionesDbContext.Schema}').");
    return;
}

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
                "El recurso fue modificado simultáneamente. Intenta nuevamente."),
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

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Ascenso.S6.ComunicacionesReporteria" }));

// ── Consentimiento irrevocable del becario por donante (RF-33 · HU-33.2, R36/A6) ──
app.MapPost("/api/v1/consentimientos-donantes", async (
    HttpContext http,
    RegistrarConsentimientoRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var resultado = await mediator.Send(
        new RegistrarConsentimientoDonanteCommand(
            actor,
            body.DonanteId,
            body.AceptaPresentacionIndividual,
            body.BecarioId),
        ct);

    return Results.Ok(resultado);
});

app.MapGet("/api/v1/consentimientos-donantes/mios", async (
    HttpContext http,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var lista = await mediator.Send(new ListarMisConsentimientosQuery(actor), ct);
    return Results.Ok(lista);
});

// ── Reportes de impacto a donantes con anonimización por diseño (RF-33 · HU-33.1) ──
app.MapPost("/api/v1/reportes/impacto", async (
    HttpContext http,
    SolicitarReporteImpactoRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var solicitud = await mediator.Send(
        new SolicitarReporteImpactoCommand(actor, body.DonanteId, body.CodigoPeriodo),
        ct);

    return Results.Accepted($"/api/v1/reportes/impacto/{solicitud.ReporteId}", solicitud);
});

app.MapGet("/api/v1/reportes/impacto/{reporteId:guid}", async (
    Guid reporteId,
    HttpContext http,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var reporte = await mediator.Send(new ConsultarReporteImpactoQuery(actor, reporteId), ct);
    return Results.Ok(reporte);
});

app.MapGet("/api/v1/reportes/impacto", async (
    Guid? donanteId,
    string? codigoPeriodo,
    HttpContext http,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var reportes = await mediator.Send(new ListarReportesImpactoQuery(actor, donanteId, codigoPeriodo), ct);
    return Results.Ok(reportes);
});

app.Run();

static IResult NoAutenticado(HttpContext http) =>
    Results.Problem(
        title: "No autenticado",
        detail: "Se requiere usuario autenticado.",
        statusCode: StatusCodes.Status401Unauthorized,
        instance: http.Request.Path);

static ActorSolicitudComunicaciones? ResolverActor(HttpContext http)
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

    List<Guid>? programasAsignados = null;
    if (http.Request.Headers.TryGetValue("X-Actor-Programas", out var headerProgramas)
        && !string.IsNullOrWhiteSpace(headerProgramas.ToString()))
    {
        programasAsignados = headerProgramas.ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToList();
    }

    return new ActorSolicitudComunicaciones(usuarioId.Value, rol, programasAsignados);
}

static (int Status, string Title, string Code, string Detail) MapearExcepcionDeDominio(ExcepcionDeDominio ex) =>
    ex.Codigo switch
    {
        "NO_AUTENTICADO"
            => (StatusCodes.Status401Unauthorized, "No autenticado", ex.Codigo, ex.Message),
        "ACCESO_DENEGADO_CONSENTIMIENTO" or "ACCESO_DENEGADO_REPORTES"
            => (StatusCodes.Status403Forbidden, "Acceso denegado", ex.Codigo, ex.Message),
        "PERIODO_NO_ENCONTRADO" or "REPORTE_NO_ENCONTRADO"
            => (StatusCodes.Status404NotFound, "Recurso no encontrado", ex.Codigo, ex.Message),
        _ => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio incumplida", ex.Codigo, ex.Message)
    };

public sealed record RegistrarConsentimientoRequest(
    Guid DonanteId,
    bool AceptaPresentacionIndividual,
    Guid? BecarioId = null);

public sealed record SolicitarReporteImpactoRequest(
    Guid DonanteId,
    string? CodigoPeriodo);

public partial class Program;
