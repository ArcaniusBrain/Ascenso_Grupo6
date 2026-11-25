using Ascenso.S1.Identidad.Application.CasosDeUso.Autenticacion;
using Ascenso.S1.Identidad.Application.CasosDeUso.Otp;
using Ascenso.S1.Identidad.Application.CasosDeUso.TiposUsuario;
using Ascenso.S1.Identidad.Application.CasosDeUso.Usuarios;
using Ascenso.S1.Identidad.Application.Puertos;
using Ascenso.S1.Identidad.Infrastructure;
using Ascenso.S1.Identidad.Infrastructure.Seguridad;
using Ascenso.SharedKernel.Seguridad;
using Ascenso.SharedKernel.TiposBase;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddIdentidadService(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = feature?.Error;

        var (status, title, code, detail) = exception switch
        {
            ExcepcionEnvioCodigo ex => (StatusCodes.Status503ServiceUnavailable, "Servicio de envío no disponible", ex.Codigo, ex.Message),
            ExcepcionDeDominio ex => MapearExcepcionDeDominio(ex),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflicto de concurrencia", "CONCURRENCIA_CONFLICTO", "El recurso fue modificado simultáneamente. Intenta nuevamente."),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor", "ERROR_INTERNO", "Ocurrió un error inesperado.")
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

        await context.Response.WriteAsJsonAsync(problem);
    });
});

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "Ascenso.S1.Identidad" }));

// ── Tipos de usuario (HU-37.1) ────────────────────────────────────────────────
app.MapGet("/api/v1/tipos-usuario", async (IMediator mediator, CancellationToken ct) =>
{
    var tipos = await mediator.Send(new ListarTiposUsuarioQuery(), ct);
    return Results.Ok(tipos);
});

app.MapPost("/api/v1/tipos-usuario", async (
    HttpContext http,
    CrearTipoUsuarioRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actorId = ResolverActorUsuarioId(http);
    if (actorId is null)
    {
        return Results.Problem(title: "No autenticado", detail: "Se requiere usuario autenticado.", statusCode: StatusCodes.Status401Unauthorized);
    }

    var creado = await mediator.Send(
        new CrearTipoUsuarioCommand(
            actorId.Value,
            body.Nombre,
            body.Permisos,
            body.AccesoFinancieroODecision,
            body.EsEnlaceUniversidad),
        ct);

    return Results.Created($"/api/v1/tipos-usuario/{creado.Id}", creado);
});

app.MapPut("/api/v1/tipos-usuario/{tipoUsuarioId:guid}/permisos", async (
    Guid tipoUsuarioId,
    HttpContext http,
    ActualizarPermisosRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actorId = ResolverActorUsuarioId(http);
    if (actorId is null)
    {
        return Results.Problem(title: "No autenticado", detail: "Se requiere usuario autenticado.", statusCode: StatusCodes.Status401Unauthorized);
    }

    var actualizado = await mediator.Send(
        new ActualizarPermisosTipoUsuarioCommand(actorId.Value, tipoUsuarioId, body.Permisos, body.AccesoFinancieroODecision),
        ct);

    return Results.Ok(actualizado);
});

app.MapDelete("/api/v1/tipos-usuario/{tipoUsuarioId:guid}", async (
    Guid tipoUsuarioId,
    HttpContext http,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actorId = ResolverActorUsuarioId(http);
    if (actorId is null)
    {
        return Results.Problem(title: "No autenticado", detail: "Se requiere usuario autenticado.", statusCode: StatusCodes.Status401Unauthorized);
    }

    await mediator.Send(new EliminarTipoUsuarioCommand(actorId.Value, tipoUsuarioId), ct);
    return Results.NoContent();
});

// ── Usuarios internos y asignación de roles (HU-37.1) ─────────────────────────
app.MapGet("/api/v1/usuarios", async (HttpContext http, IMediator mediator, CancellationToken ct) =>
{
    var actorId = ResolverActorUsuarioId(http);
    if (actorId is null)
    {
        return Results.Problem(title: "No autenticado", detail: "Se requiere usuario autenticado.", statusCode: StatusCodes.Status401Unauthorized);
    }

    var lista = await mediator.Send(new ListarUsuariosQuery(actorId.Value), ct);
    return Results.Ok(lista);
});

app.MapPost("/api/v1/usuarios", async (
    HttpContext http,
    CrearUsuarioInternoRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actorId = ResolverActorUsuarioId(http);
    if (actorId is null)
    {
        return Results.Problem(title: "No autenticado", detail: "Se requiere usuario autenticado.", statusCode: StatusCodes.Status401Unauthorized);
    }

    var creado = await mediator.Send(
        new CrearUsuarioInternoCommand(
            actorId.Value,
            body.Cedula,
            body.NombreCompleto,
            body.Correo,
            body.Telefono,
            body.TipoUsuarioIds,
            body.CanalPreferido,
            body.ProgramasAsignados,
            body.UniversidadId,
            body.EsEvaluadorExterno,
            body.ContrasenaTemporal),
        ct);

    return Results.Created($"/api/v1/usuarios/{creado.Id}", creado);
});

app.MapPut("/api/v1/usuarios/{usuarioId:guid}/roles", async (
    Guid usuarioId,
    HttpContext http,
    CambiarRolesRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actorId = ResolverActorUsuarioId(http);
    if (actorId is null)
    {
        return Results.Problem(title: "No autenticado", detail: "Se requiere usuario autenticado.", statusCode: StatusCodes.Status401Unauthorized);
    }

    var actualizado = await mediator.Send(
        new CambiarRolesUsuarioCommand(actorId.Value, usuarioId, body.TipoUsuarioIds),
        ct);

    return Results.Ok(actualizado);
});

// ── Autenticación y verificación en dos pasos (HU-37.2, ADR-09) ───────────────
app.MapPost("/api/v1/auth/login", async (LoginRequest body, IMediator mediator, CancellationToken ct) =>
{
    var resultado = await mediator.Send(new IniciarSesionPaso1Command(body.Correo, body.Contrasena), ct);
    return resultado.RequiereSegundoFactor
        ? Results.Accepted(value: resultado)
        : Results.Ok(resultado);
});

app.MapPost("/api/v1/auth/2fa/verificar", async (Verificar2FaRequest body, IMediator mediator, CancellationToken ct) =>
{
    var sesion = await mediator.Send(new VerificarSegundoFactorCommand(body.DesafioId, body.Codigo), ct);
    return Results.Ok(sesion);
});

app.MapPost("/api/v1/auth/2fa/reenviar", async (Reenviar2FaRequest body, IMediator mediator, CancellationToken ct) =>
{
    var resultado = await mediator.Send(new ReenviarSegundoFactorCommand(body.UsuarioId), ct);
    return Results.Accepted(value: resultado);
});

app.MapPost("/api/v1/auth/2fa/recuperar", async (Recuperar2FaRequest body, IMediator mediator, CancellationToken ct) =>
{
    var resultado = await mediator.Send(new RecuperarSegundoFactorCommand(body.Correo), ct);
    return Results.Accepted(value: resultado);
});

// ── Códigos de un solo uso por operación sensible (ADR-10, DDD-10, R4–R6, R42) ──
app.MapPost("/api/v1/otp/challenges", async (
    HttpContext http,
    ChallengeRequest body,
    IMediator mediator,
    CancellationToken ct) =>
{
    var actorId = ResolverActorUsuarioId(http);
    if (actorId is null)
    {
        return Results.Problem(title: "No autenticado", detail: "Se requiere usuario autenticado.", statusCode: StatusCodes.Status401Unauthorized);
    }

    var desafio = await mediator.Send(
        new SolicitarDesafioOtpCommand(actorId.Value, body.Operacion, body.ResourceId, body.Canal),
        ct);

    return Results.Ok(new ChallengeResponse(desafio.ChallengeId, desafio.ExpiraEnUtc));
});

// Frontera privada S4 -> S1 (contracts/openapi-interno.yaml; NUNCA expuesta por YARP)
app.MapPost("/internal/v1/otp/validar-y-consumir", async (
    HttpContext http,
    OtpInternoCommandRequest body,
    ValidadorScopeInternoOtp validadorInterno,
    IMediator mediator,
    CancellationToken ct) =>
{
    var auth = validadorInterno.ValidarAuthorizationHeader(http.Request.Headers.Authorization.ToString());
    if (auth.Estado == EstadoAutorizacionInterna.NO_AUTENTICADO)
    {
        return Results.Problem(title: "Error de frontera privada", detail: auth.Motivo, statusCode: StatusCodes.Status401Unauthorized);
    }

    if (auth.Estado == EstadoAutorizacionInterna.PROHIBIDO)
    {
        return Results.Problem(title: "Acceso denegado en frontera privada", detail: auth.Motivo, statusCode: StatusCodes.Status403Forbidden);
    }

    var resultado = await mediator.Send(
        new ValidarYConsumirOtpCommand(
            body.UsuarioId,
            body.Operacion,
            body.RecursoId,
            body.ChallengeId,
            body.Codigo,
            body.OperationId),
        ct);

    return Results.Ok(new OtpInternoResponseDto(resultado.Consumido, resultado.ChallengeId));
});

app.Run();

static Guid? ResolverActorUsuarioId(HttpContext http)
{
    var sub = http.User.FindFirst("sub")?.Value;
    if (Guid.TryParse(sub, out var desdeClaim))
    {
        return desdeClaim;
    }

    if (http.Request.Headers.TryGetValue("X-Actor-Usuario-Id", out var header)
        && Guid.TryParse(header.ToString(), out var desdeHeader))
    {
        return desdeHeader;
    }

    return null;
}

static (int Status, string Title, string Code, string Detail) MapearExcepcionDeDominio(ExcepcionDeDominio ex) =>
    ex.Codigo switch
    {
        "CREDENCIALES_INVALIDAS" or "SEGUNDO_FACTOR_INVALIDO" or "SEGUNDO_FACTOR_EXPIRADO" or "SEGUNDO_FACTOR_YA_USADO"
            => (StatusCodes.Status401Unauthorized, "Autenticación fallida", ex.Codigo, ex.Message),
        "ACCESO_DENEGADO_IDENTIDAD" or "AUTO_ESCALAMIENTO_BLOQUEADO" or "PERMISO_RUBRICAS_DENEGADO" or "USUARIO_DESACTIVADO"
            => (StatusCodes.Status403Forbidden, "Acceso denegado", ex.Codigo, ex.Message),
        "ACTOR_NO_ENCONTRADO" or "USUARIO_NO_ENCONTRADO" or "TIPO_USUARIO_NO_ENCONTRADO" or "USUARIO_NO_HABILITADO"
            => (StatusCodes.Status404NotFound, "Recurso no encontrado", ex.Codigo, ex.Message),
        "TIPO_USUARIO_DUPLICADO" or "CEDULA_DUPLICADA" or "CORREO_DUPLICADO" or "TIPO_USUARIO_EN_USO" or "OTP_REUTILIZADO"
            => (StatusCodes.Status409Conflict, "Conflicto de negocio", ex.Codigo, ex.Message),
        "BLOQUEO_2FA_ACTIVO" or "OTP_BLOQUEADO"
            => (StatusCodes.Status429TooManyRequests, "Demasiados intentos fallidos", ex.Codigo, ex.Message),
        "KEYCLOAK_NO_DISPONIBLE" or "FALLO_ENVIO_SEGUNDO_FACTOR"
            => (StatusCodes.Status503ServiceUnavailable, "Servicio temporalmente no disponible", ex.Codigo, ex.Message),
        _ => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio incumplida", ex.Codigo, ex.Message)
    };

public sealed record CrearTipoUsuarioRequest(
    string? Nombre,
    IReadOnlyCollection<string>? Permisos,
    bool AccesoFinancieroODecision = false,
    bool EsEnlaceUniversidad = false);

public sealed record ActualizarPermisosRequest(
    IReadOnlyCollection<string> Permisos,
    bool? AccesoFinancieroODecision = null);

public sealed record CrearUsuarioInternoRequest(
    string Cedula,
    string NombreCompleto,
    string Correo,
    string Telefono,
    IReadOnlyCollection<Guid>? TipoUsuarioIds,
    CanalCodigo CanalPreferido = CanalCodigo.CORREO,
    IReadOnlyCollection<Guid>? ProgramasAsignados = null,
    Guid? UniversidadId = null,
    bool EsEvaluadorExterno = false,
    string? ContrasenaTemporal = null);

public sealed record CambiarRolesRequest(IReadOnlyCollection<Guid> TipoUsuarioIds);
public sealed record LoginRequest(string Correo, string Contrasena);
public sealed record Verificar2FaRequest(Guid DesafioId, string? Codigo);
public sealed record Reenviar2FaRequest(Guid UsuarioId);
public sealed record Recuperar2FaRequest(string Correo);
public sealed record ChallengeRequest(TipoOperacionSensible Operacion, string ResourceId, CanalCodigo Canal);
public sealed record ChallengeResponse(Guid ChallengeId, DateTimeOffset ExpiraEnUtc);
public sealed record OtpInternoCommandRequest(
    Guid UsuarioId,
    TipoOperacionSensible Operacion,
    string RecursoId,
    Guid ChallengeId,
    string Codigo,
    Guid OperationId);
public sealed record OtpInternoResponseDto(bool Consumido, Guid ChallengeId);

public partial class Program;
