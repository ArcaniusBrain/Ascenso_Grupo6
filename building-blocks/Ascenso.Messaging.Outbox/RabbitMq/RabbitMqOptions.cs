namespace Ascenso.Messaging.Outbox.RabbitMq;

/// <summary>
/// Configuración de mensajería RabbitMQ leída exclusivamente desde IConfiguration (sección "RabbitMq").
/// Si Host está vacío, los workers de despacho y consumo quedan inactivos de forma segura (ej. pruebas locales sin Docker).
/// </summary>
public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public const string ExchangePorDefecto = "ascenso.eventos";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ExchangeName { get; set; } = ExchangePorDefecto;
    public int PollingIntervalMs { get; set; } = 1500;
    public int BatchSize { get; set; } = 50;

    public bool EstaHabilitado => !string.IsNullOrWhiteSpace(Host);
}
