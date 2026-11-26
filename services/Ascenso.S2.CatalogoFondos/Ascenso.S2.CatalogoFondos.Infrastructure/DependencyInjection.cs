using Ascenso.Messaging.Outbox;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;
using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.S2.CatalogoFondos.Infrastructure.Persistencia;
using Ascenso.SharedKernel.Tiempo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ascenso.S2.CatalogoFondos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogoFondosService(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? configurarDbContext = null)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AbrirPeriodoCommand).Assembly));

        services.AddSingleton<IRelojNegocio>(new RelojSistemaCaracas(TimeProvider.System));
        services.AddSingleton<AppendOnlyRegistroDecisionInterceptor>();

        services.AddDbContext<CatalogoFondosDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetRequiredService<AppendOnlyRegistroDecisionInterceptor>());

            if (configurarDbContext is not null)
            {
                configurarDbContext(options);
                return;
            }

            var connectionString = configuration.GetConnectionString("CatalogoFondosDb");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__ef_migrations_history", CatalogoFondosDbContext.Schema);
                });
            }
        });

        services.AddScoped<ICalendarioAcademicoRepository, CalendarioAcademicoRepositoryEfCore>();
        services.AddScoped<IInicioInscripcionesRepository, InicioInscripcionesRepositoryEfCore>();
        services.AddScoped<IUniversidadAliadaRepository, UniversidadAliadaRepositoryEfCore>();
        services.AddScoped<IProgramaRepository, ProgramaRepositoryEfCore>();
        services.AddScoped<IDonanteRepository, DonanteRepositoryEfCore>();
        services.AddScoped<IEscritorOutboxCatalogo, EscritorOutboxCatalogoEfCore>();
        services.AddScoped<IRegistroIdempotenciaComandos, RegistroIdempotenciaComandosEfCore>();
        services.AddScoped<IConsultorElementosPeriodo, ConsultorElementosPeriodoEfCore>();
        services.AddScoped<IConsultorDonantesYAportes, ConsultorDonantesYAportesEfCore>();
        services.AddScoped<IUnidadTrabajoCatalogo, UnidadTrabajoCatalogoEfCore>();

        return services;
    }
}
