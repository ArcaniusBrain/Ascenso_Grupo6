using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Donantes;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Programas;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Universidades;
using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Infrastructure;
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

// ── Universidades aliadas, convenios, costos e inscripciones (RF-14: HU-14.1, HU-14.2, HU-14.3) ──
app.MapGet("/api/v1/universidades", async (HttpContext http, IMediator mediator, CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var lista = await mediator.Send(new ListarUniversidadesQuery(actor), ct);
    return Results.Ok(lista);
});

app.MapPost("/api/v1/universidades", async (
    HttpContext http,
    RegistrarUniversidadRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var creada = await mediator.Send(
        new RegistrarUniversidadAliadaCommand(
            actor,
            body.Nombre,
            body.TerminosConvenio,
            body.ViaDatosAcademicos,
            body.ContactoDesignado),
        ct);

    return Results.Created($"/api/v1/universidades/{creada.Id}", creada);
});

app.MapGet("/api/v1/universidades/{universidadId:guid}", async (
    Guid universidadId,
    HttpContext http,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var detalle = await mediator.Send(new ConsultarUniversidadDetalleQuery(actor, universidadId), ct);
    return Results.Ok(detalle);
});

app.MapPut("/api/v1/universidades/{universidadId:guid}/convenio", async (
    Guid universidadId,
    HttpContext http,
    ActualizarConvenioRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var actualizada = await mediator.Send(
        new ActualizarConvenioUniversidadCommand(
            actor,
            universidadId,
            body.Nombre,
            body.TerminosConvenio,
            body.ViaDatosAcademicos,
            body.ContactoDesignado),
        ct);

    return Results.Ok(actualizada);
});

app.MapPost("/api/v1/universidades/{universidadId:guid}/carreras", async (
    Guid universidadId,
    HttpContext http,
    AgregarCarreraRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var carrera = await mediator.Send(
        new AgregarCarreraElegibleCommand(actor, universidadId, body.Nombre),
        ct);

    return Results.Created($"/api/v1/universidades/{universidadId}/carreras/{carrera.Id}", carrera);
});

app.MapPost("/api/v1/universidades/{universidadId:guid}/costos-matricula", async (
    Guid universidadId,
    HttpContext http,
    RegistrarCostoMatriculaRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var costo = await mediator.Send(
        new RegistrarCostoMatriculaCommand(actor, universidadId, body.CodigoPeriodo, body.Monto, body.Moneda),
        ct);

    return Results.Created(
        $"/api/v1/universidades/{universidadId}/costos-matricula/{Uri.EscapeDataString(costo.CodigoPeriodo)}",
        costo);
});

app.MapGet("/api/v1/universidades/{universidadId:guid}/costos-matricula/{codigoPeriodo}", async (
    Guid universidadId,
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

    var resultado = await mediator.Send(
        new ConsultarCostoMatriculaPeriodoQuery(actor, universidadId, codigoPeriodo),
        ct);

    return Results.Ok(resultado);
});

app.MapPost("/api/v1/universidades/{universidadId:guid}/inicios-inscripciones", async (
    Guid universidadId,
    HttpContext http,
    RegistrarInicioInscripcionesUniversidadRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var inicio = await mediator.Send(
        new RegistrarInicioInscripcionesUniversidadCommand(actor, universidadId, body.CodigoPeriodo, body.FechaInicio),
        ct);

    return Results.Created(
        $"/api/v1/universidades/{universidadId}/inicios-inscripciones/{Uri.EscapeDataString(inicio.CodigoPeriodo)}",
        inicio);
});

app.MapGet("/api/v1/universidades/{universidadId:guid}/inicios-inscripciones/{codigoPeriodo}", async (
    Guid universidadId,
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

    var resultado = await mediator.Send(
        new ConsultarInicioInscripcionesUniversidadQuery(actor, universidadId, codigoPeriodo),
        ct);

    return Results.Ok(resultado);
});

// ── Programas de becas (RF-11: HU-11.1 y HU-11.2) ─────────────────────────────
app.MapGet("/api/v1/programas", async (HttpContext http, IMediator mediator, CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var lista = await mediator.Send(new ListarProgramasQuery(actor), ct);
    return Results.Ok(lista);
});

app.MapPost("/api/v1/programas", async (
    HttpContext http,
    CrearProgramaRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var creado = await mediator.Send(
        new CrearProgramaCommand(
            actor,
            body.Nombre,
            body.PeriodoInicio,
            body.CupoBecarios,
            body.DuracionPeriodos,
            body.MontoPorPeriodo,
            body.Moneda,
            body.ModalidadPago,
            body.Alcance,
            body.PermiteAcumulacion),
        ct);

    return Results.Created($"/api/v1/programas/{creado.Id}", creado);
});

app.MapGet("/api/v1/programas/{programaId:guid}", async (
    Guid programaId,
    HttpContext http,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var detalle = await mediator.Send(new ConsultarProgramaDetalleQuery(actor, programaId), ct);
    return Results.Ok(detalle);
});

app.MapPut("/api/v1/programas/{programaId:guid}/configuracion-base", async (
    Guid programaId,
    HttpContext http,
    ActualizarConfiguracionProgramaRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var actualizado = await mediator.Send(
        new ActualizarConfiguracionBaseProgramaCommand(
            actor,
            programaId,
            body.Nombre,
            body.MontoPorPeriodo,
            body.Moneda,
            body.ModalidadPago,
            body.Alcance,
            body.PermiteAcumulacion),
        ct);

    return Results.Ok(actualizado);
});

app.MapPost("/api/v1/programas/{programaId:guid}/donantes", async (
    Guid programaId,
    HttpContext http,
    AsociarDonanteProgramaRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var detalle = await mediator.Send(
        new AsociarDonanteAProgramaCommand(actor, programaId, body.DonanteId),
        ct);

    return Results.Ok(detalle);
});

app.MapPut("/api/v1/programas/{programaId:guid}/elegibilidad", async (
    Guid programaId,
    HttpContext http,
    ConfigurarElegibilidadRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var detalle = await mediator.Send(
        new ConfigurarElegibilidadProgramaCommand(actor, programaId, body.Restricciones),
        ct);

    return Results.Ok(detalle);
});

app.MapPut("/api/v1/programas/{programaId:guid}/condiciones", async (
    Guid programaId,
    HttpContext http,
    ConfigurarCondicionesRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var promedioTexto = body.PromedioMinimo switch
    {
        null => null,
        { ValueKind: JsonValueKind.String } e => e.GetString(),
        { ValueKind: JsonValueKind.Number } e => e.GetRawText(),
        { ValueKind: JsonValueKind.Null } => null,
        { } e => e.GetRawText()
    };

    var detalle = await mediator.Send(
        new ConfigurarCondicionesProgramaCommand(
            actor,
            programaId,
            promedioTexto,
            body.ExigeHorasServicio,
            body.HorasServicio,
            body.ReportesExigidos,
            body.DuracionMaxPausaMeses,
            body.DescripcionRenovacion),
        ct);

    return Results.Ok(detalle);
});

// ── Donantes y aportes (RF-13: HU-13.1 y HU-13.2) ─────────────────────────────
app.MapGet("/api/v1/donantes", async (HttpContext http, IMediator mediator, CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var lista = await mediator.Send(new ListarDonantesQuery(actor), ct);
    return Results.Ok(lista);
});

app.MapPost("/api/v1/donantes", async (
    HttpContext http,
    RegistrarDonanteRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var creado = await mediator.Send(
        new RegistrarDonanteCommand(actor, body.Nombre, body.Tipo, body.Contacto),
        ct);

    return Results.Created($"/api/v1/donantes/{creado.Id}", creado);
});

app.MapGet("/api/v1/donantes/{donanteId:guid}", async (
    Guid donanteId,
    HttpContext http,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var detalle = await mediator.Send(new ConsultarDonanteDetalleQuery(actor, donanteId), ct);
    return Results.Ok(detalle);
});

app.MapGet("/api/v1/donantes/{donanteId:guid}/aportes", async (
    Guid donanteId,
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

    var historial = await mediator.Send(
        new ConsultarHistorialAportesDonanteQuery(actor, donanteId, codigoPeriodo),
        ct);

    return Results.Ok(historial);
});

app.MapPost("/api/v1/donantes/{donanteId:guid}/aportes", async (
    Guid donanteId,
    HttpContext http,
    RegistrarAporteDesdeDonanteRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var resultado = await mediator.Send(
        new RegistrarAporteCommand(
            actor,
            donanteId,
            body.ProgramaId,
            body.Cantidad,
            body.Moneda,
            body.CodigoPeriodo),
        ct);

    return Results.Created($"/api/v1/donantes/{donanteId}/aportes/{resultado.Aporte.Id}", resultado.Aporte);
});

app.MapPost("/api/v1/programas/{programaId:guid}/aportes", async (
    Guid programaId,
    HttpContext http,
    RegistrarAporteProgramaRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actor = ResolverActor(http);
    if (actor is null)
    {
        return NoAutenticado(http);
    }

    var resultado = await mediator.Send(
        new RegistrarAporteCommand(
            actor,
            body.DonanteId,
            programaId,
            body.Cantidad,
            body.Moneda,
            body.CodigoPeriodo),
        ct);

    return Results.Ok(resultado.Confirmacion);
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

    return new ActorSolicitudCatalogo(usuarioId.Value, rol, programasAsignados);
}

static (int Status, string Title, string Code, string Detail) MapearExcepcionDeDominio(ExcepcionDeDominio ex) =>
    ex.Codigo switch
    {
        "NO_AUTENTICADO"
            => (StatusCodes.Status401Unauthorized, "No autenticado", ex.Codigo, ex.Message),
        "ACCESO_DENEGADO_CALENDARIO" or "ACCESO_DENEGADO_UNIVERSIDADES" or "ACCESO_DENEGADO_PROGRAMAS" or "ACCESO_DENEGADO_DONANTES"
            => (StatusCodes.Status403Forbidden, "Acceso denegado", ex.Codigo, ex.Message),
        "PERIODO_NO_ENCONTRADO" or "UNIVERSIDAD_NO_ENCONTRADA" or "PROGRAMA_NO_ENCONTRADO" or "DONANTE_NO_ENCONTRADO"
            => (StatusCodes.Status404NotFound, "Recurso no encontrado", ex.Codigo, ex.Message),
        "PERIODO_DUPLICADO" or "INICIO_INSCRIPCIONES_DUPLICADO" or "UNIVERSIDAD_DUPLICADA" or "CARRERA_DUPLICADA" or "PROGRAMA_DUPLICADO" or "DONANTE_YA_ASOCIADO" or "DONANTE_DUPLICADO"
            => (StatusCodes.Status409Conflict, "Conflicto de negocio", ex.Codigo, ex.Message),
        _ => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio incumplida", ex.Codigo, ex.Message)
    };

public sealed record AbrirPeriodoRequest(string? CodigoPeriodo);

public sealed record RegistrarInicioInscripcionesRequest(
    Guid UniversidadId,
    string? NombreUniversidad,
    DateOnly? FechaInicio);

public sealed record MarcarPeriodoRequest(string CodigoPeriodo, Guid OperationId);

public sealed record RegistrarUniversidadRequest(
    string? Nombre,
    string? TerminosConvenio,
    ViaDatosAcademicos ViaDatosAcademicos,
    ContactoDesignadoDto? ContactoDesignado);

public sealed record ActualizarConvenioRequest(
    string? Nombre,
    string? TerminosConvenio,
    ViaDatosAcademicos ViaDatosAcademicos,
    ContactoDesignadoDto? ContactoDesignado);

public sealed record AgregarCarreraRequest(string? Nombre);

public sealed record RegistrarCostoMatriculaRequest(
    string CodigoPeriodo,
    decimal Monto,
    string? Moneda = "USD");

public sealed record RegistrarInicioInscripcionesUniversidadRequest(
    string CodigoPeriodo,
    DateOnly? FechaInicio);

public sealed record CrearProgramaRequest(
    string? Nombre,
    string? PeriodoInicio,
    int? CupoBecarios,
    int? DuracionPeriodos,
    decimal? MontoPorPeriodo,
    string? Moneda,
    ModalidadPago? ModalidadPago,
    AlcancePrograma? Alcance,
    bool PermiteAcumulacion = false);

public sealed record ActualizarConfiguracionProgramaRequest(
    string? Nombre,
    decimal? MontoPorPeriodo,
    string? Moneda,
    ModalidadPago? ModalidadPago,
    AlcancePrograma? Alcance,
    bool PermiteAcumulacion = false);

public sealed record AsociarDonanteProgramaRequest(Guid DonanteId);

public sealed record ConfigurarElegibilidadRequest(IReadOnlyList<RestriccionUniversidadInput>? Restricciones);

public sealed record ConfigurarCondicionesRequest(
    JsonElement? PromedioMinimo,
    bool ExigeHorasServicio,
    int? HorasServicio,
    IReadOnlyList<string>? ReportesExigidos,
    int? DuracionMaxPausaMeses,
    string? DescripcionRenovacion);

public sealed record RegistrarDonanteRequest(
    string? Nombre,
    TipoDonante? Tipo,
    ContactoDonanteDto? Contacto);

public sealed record RegistrarAporteDesdeDonanteRequest(
    Guid ProgramaId,
    decimal? Cantidad,
    string? Moneda,
    string? CodigoPeriodo);

public sealed record RegistrarAporteProgramaRequest(
    Guid DonanteId,
    decimal? Cantidad,
    string? Moneda,
    string? CodigoPeriodo);

public partial class Program;
