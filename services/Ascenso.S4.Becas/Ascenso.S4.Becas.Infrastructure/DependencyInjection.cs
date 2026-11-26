using Ascenso.Messaging.Outbox;
using Ascenso.S4.Becas.Application.HistorialBecas;
using Ascenso.S4.Becas.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ascenso.S4.Becas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddS4Infrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? configurarDbContext = null)
    {
        services.AddSingleton<AppendOnlyRegistroDecisionInterceptor>();
        services.AddDbContext<BecaDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetRequiredService<AppendOnlyRegistroDecisionInterceptor>());

            if (configurarDbContext is not null)
            {
                configurarDbContext(options);
                return;
            }

            var connectionString = configuration.GetConnectionString("BecasDb");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__ef_migrations_history", BecaDbContext.Schema);
                });
            }
        });

        services.AddScoped<IConsultaHistorialBecas, ConsultaHistorialBecasEfCore>();
        return services;
    }
}
