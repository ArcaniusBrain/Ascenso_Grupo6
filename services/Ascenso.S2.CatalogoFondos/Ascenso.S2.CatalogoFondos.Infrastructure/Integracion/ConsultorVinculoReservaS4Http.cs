using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Microsoft.Extensions.Configuration;

namespace Ascenso.S2.CatalogoFondos.Infrastructure.Integracion;

/// <summary>
/// Cliente resiliente S2 -> S4 para conciliación segura de reservas sin vincular (ADR-26 / C19).
/// Aplica timeout de 3 segundos por intento, máximo 2 reintentos en GET idempotente, circuit breaker ligero
/// y política fail closed: ante cualquier error, timeout o configuración faltante devuelve Disponible=false
/// para que S2 nunca libere una reserva sin confirmación autoritativa de S4.
/// </summary>
public sealed class ConsultorVinculoReservaS4Http : IConsultorVinculoReservaS4
{
    public const int TimeoutSegundos = 3;
    public const int MaxReintentosIdempotentes = 2;
    private const int UmbralFallosCircuito = 3;
    private static readonly TimeSpan DuracionCircuitoAbierto = TimeSpan.FromSeconds(15);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly string? _baseUrl;
    private readonly string? _bearerToken;

    private int _fallosConsecutivos;
    private DateTimeOffset? _circuitoAbiertoHastaUtc;
    private readonly object _lockCircuito = new();

    public ConsultorVinculoReservaS4Http(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["ServiciosInternos:S4BecasBaseUrl"]?.TrimEnd('/');
        _bearerToken = configuration["ServiciosInternos:S4TokenServicio"];
    }

    public async Task<ResultadoConsultaVinculoS4> ConsultarVinculoAsync(
        Guid reservaId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_baseUrl))
        {
            return new ResultadoConsultaVinculoS4(
                Disponible: false,
                Estado: null,
                Error: "No está configurada la URL interna de S4 (ServiciosInternos:S4BecasBaseUrl).");
        }

        if (CircuitoAbierto())
        {
            return new ResultadoConsultaVinculoS4(
                Disponible: false,
                Estado: null,
                Error: "Circuit breaker abierto hacia S4; se pospone la conciliación (fail closed).");
        }

        string? ultimoError = null;
        for (var intento = 0; intento <= MaxReintentosIdempotentes; intento++)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(TimeoutSegundos));

                using var req = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{_baseUrl}/internal/v1/reservas/{reservaId}/vinculo");

                if (!string.IsNullOrWhiteSpace(_bearerToken))
                {
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
                }

                using var resp = await _httpClient.SendAsync(req, cts.Token);
                if (resp.IsSuccessStatusCode)
                {
                    var dto = await resp.Content.ReadFromJsonAsync<EstadoVinculoReservaS4>(JsonOptions, cts.Token);
                    if (dto is not null)
                    {
                        RegistrarExito();
                        return new ResultadoConsultaVinculoS4(Disponible: true, Estado: dto);
                    }

                    ultimoError = "S4 respondió 200 sin cuerpo válido.";
                }
                else
                {
                    ultimoError = $"S4 respondió HTTP {(int)resp.StatusCode}.";
                }
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                ultimoError = $"Timeout de {TimeoutSegundos}s consultando S4.";
            }
            catch (HttpRequestException ex)
            {
                ultimoError = ex.Message;
            }
        }

        RegistrarFallo();
        return new ResultadoConsultaVinculoS4(Disponible: false, Estado: null, Error: ultimoError);
    }

    private bool CircuitoAbierto()
    {
        lock (_lockCircuito)
        {
            if (_circuitoAbiertoHastaUtc is null)
            {
                return false;
            }

            if (DateTimeOffset.UtcNow >= _circuitoAbiertoHastaUtc.Value)
            {
                _circuitoAbiertoHastaUtc = null;
                _fallosConsecutivos = 0;
                return false;
            }

            return true;
        }
    }

    private void RegistrarExito()
    {
        lock (_lockCircuito)
        {
            _fallosConsecutivos = 0;
            _circuitoAbiertoHastaUtc = null;
        }
    }

    private void RegistrarFallo()
    {
        lock (_lockCircuito)
        {
            _fallosConsecutivos++;
            if (_fallosConsecutivos >= UmbralFallosCircuito)
            {
                _circuitoAbiertoHastaUtc = DateTimeOffset.UtcNow.Add(DuracionCircuitoAbierto);
            }
        }
    }
}
