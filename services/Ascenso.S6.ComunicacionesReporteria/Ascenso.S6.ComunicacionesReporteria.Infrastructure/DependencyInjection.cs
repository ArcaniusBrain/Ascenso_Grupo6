using Ascenso.Messaging.Outbox;
using Ascenso.S6.ComunicacionesReporteria.Application.CasosDeUso.Consentimientos;
using Ascenso.S6.ComunicacionesReporteria.Application.Puertos;
using Ascenso.S6.ComunicacionesReporteria.Domain.Repositorios;
using Ascenso.S6.ComunicacionesReporteria.Infrastructure.Persistencia;
using Ascenso.S6.ComunicacionesReporteria.Infrastructure.Procesamiento;
using Ascenso.SharedKernel.Tiempo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ascenso.S6.ComunicacionesReporteria.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddComunicacionesReporteriaService(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? configurarDbContext = null)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegistrarConsentimientoDonanteCommand).Assembly));

        services.AddSingleton<IRelojNegocio>(new RelojSistemaCaracas(TimeProvider.System));
        services.AddSingleton<AppendOnlyRegistroDecisionInterceptor>();

        services.AddDbContext<ComunicacionesDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetRequiredService<AppendOnlyRegistroDecisionInterceptor>());

            if (configurarDbContext is not null)
            {
                configurarDbContext(options);
                return;
            }

            var connectionString = configuration.GetConnectionString("ComunicacionesDb");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__ef_migrations_history", ComunicacionesDbContext.Schema);
                });
            }
        });

        services.AddScoped<IConsentimientoDonanteRepository, ConsentimientoDonanteRepositoryEfCore>();
        services.AddScoped<ISolicitudReporteImpactoRepository, SolicitudReporteImpactoRepositoryEfCore>();
        services.AddScoped<IRepositorioProyeccionImpacto, RepositorioProyeccionImpactoEfCore>();
        services.AddScoped<IUnidadTrabajoComunicaciones, UnidadTrabajoComunicacionesEfCore>();

        services.AddSingleton<ColaReportesImpactoChannel>();
        services.AddSingleton<IColaProcesamientoReportesImpacto>(sp => sp.GetRequiredService<ColaReportesImpactoChannel>());

        return services;
    }
}
