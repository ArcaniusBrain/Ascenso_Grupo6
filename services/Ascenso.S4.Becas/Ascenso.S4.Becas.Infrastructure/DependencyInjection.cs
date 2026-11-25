using Ascenso.Messaging.Outbox;
using Ascenso.S4.Becas.Application.HistorialBecas;
using Ascenso.S4.Becas.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ascenso.S4.Becas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddS4Infrastructure(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDb)
    {
        services.AddSingleton<AppendOnlyRegistroDecisionInterceptor>();
        services.AddDbContext<BecaDbContext>((sp, options) =>
        {
            configureDb(options);
            options.AddInterceptors(sp.GetRequiredService<AppendOnlyRegistroDecisionInterceptor>());
        });

        services.AddScoped<IConsultaHistorialBecas, ConsultaHistorialBecasEfCore>();
        return services;
    }
}
