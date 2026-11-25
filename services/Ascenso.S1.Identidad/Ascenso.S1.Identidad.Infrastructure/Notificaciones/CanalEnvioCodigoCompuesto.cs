using System.Net;
using System.Net.Mail;
using Ascenso.S1.Identidad.Application.Puertos;
using Ascenso.SharedKernel.Seguridad;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ascenso.S1.Identidad.Infrastructure.Notificaciones;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 1025;
    public string FromAddress { get; set; } = "no-reply@ascenso.org";
    public string FromName { get; set; } = "ASCENSO — Fundación Ascenso";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableSsl { get; set; }
}

public interface ICanalSms
{
    Task EnviarSmsAsync(string telefono, string mensaje, CancellationToken cancellationToken = default);
}

/// <summary>
/// Adaptador SMS simulado/extensible para desarrollo y pruebas (ADR-20, pendiente C8).
/// Al elegir proveedor comercial (p. ej. Twilio), se reemplaza únicamente esta implementación.
/// </summary>
public sealed class CanalSmsSimulado(ILogger<CanalSmsSimulado> logger) : ICanalSms
{
    public Task EnviarSmsAsync(string telefono, string mensaje, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("SMS 2FA enviado (modo simulado) al destino terminado en {Sufijo}.",
            telefono.Length >= 4 ? telefono[^4..] : "****");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Adaptador compuesto de envío de códigos 2FA por correo SMTP real (Mailpit/servidor) o SMS (HU-37.2, ADR-20).
/// </summary>
public sealed class CanalEnvioCodigoCompuesto(
    IOptions<SmtpOptions> smtpOptions,
    ICanalSms canalSms) : ICanalEnvioCodigo
{
    public async Task EnviarCodigoSegundoFactorAsync(
        CanalCodigo canal,
        string destino,
        string codigo,
        int vigenciaMinutos,
        bool esRecuperacion,
        CancellationToken cancellationToken = default)
    {
        var tipo = esRecuperacion ? "recuperación de segundo factor" : "verificación en dos pasos";
        var cuerpo = $"Tu código de {tipo} en ASCENSO es: {codigo}. Vence en {vigenciaMinutos} minutos y es de un solo uso.";

        try
        {
            if (canal == CanalCodigo.SMS)
            {
                await canalSms.EnviarSmsAsync(destino, cuerpo, cancellationToken);
                return;
            }

            var cfg = smtpOptions.Value;
            if (string.IsNullOrWhiteSpace(cfg.Host))
            {
                throw new ExcepcionEnvioCodigo("No se pudo entregar el código por correo: servidor SMTP no configurado.");
            }

            using var mensaje = new MailMessage
            {
                From = new MailAddress(cfg.FromAddress, cfg.FromName),
                Subject = esRecuperacion
                    ? "ASCENSO — Código de recuperación de segundo factor"
                    : "ASCENSO — Código de verificación en dos pasos",
                Body = cuerpo,
                IsBodyHtml = false
            };
            mensaje.To.Add(destino);

            using var cliente = new SmtpClient(cfg.Host, cfg.Port)
            {
                EnableSsl = cfg.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(cfg.Username))
            {
                cliente.Credentials = new NetworkCredential(cfg.Username, cfg.Password);
            }

            await cliente.SendMailAsync(mensaje, cancellationToken);
        }
        catch (ExcepcionEnvioCodigo)
        {
            throw;
        }
        catch (Exception)
        {
            throw new ExcepcionEnvioCodigo(
                "No se pudo entregar el código de verificación. Puedes solicitar el reenvío nuevamente.");
        }
    }
}
