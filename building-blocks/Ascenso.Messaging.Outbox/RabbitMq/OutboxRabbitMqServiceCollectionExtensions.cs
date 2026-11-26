using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ascenso.Messaging.Outbox.RabbitMq;

public static class OutboxRabbitMqServiceCollectionExtensions
{
    /// <summary>
    /// Registra las opciones RabbitMQ, el publicador y el OutboxDispatcherWorker para un microservicio (S1..S6).
    /// </summary>
    public static IServiceCollection AddOutboxRabbitMqDispatcher<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string producerName)
        where TDbContext : DbContext
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.TryAddSingleton<IPublicadorEventosIntegracion, PublicadorRabbitMq>();
        services.AddScoped<ProcesadorOutboxLote<TDbContext>>();

        services.AddHostedService(sp => new OutboxDispatcherWorker<TDbContext>(
            sp.GetRequiredService<IServiceScopeFactory>(),
            sp.GetRequiredService<IOptions<RabbitMqOptions>>(),
            producerName,
            sp.GetRequiredService<ILogger<OutboxDispatcherWorker<TDbContext>>>()));

        return services;
    }
}
