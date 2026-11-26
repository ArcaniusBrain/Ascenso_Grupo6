using Ascenso.Messaging.Outbox;
using Ascenso.Messaging.Outbox.RabbitMq;
using Ascenso.S1.Identidad.Application.CasosDeUso.Autenticacion;
using Ascenso.S1.Identidad.Application.CasosDeUso.Otp;
using Ascenso.S1.Identidad.Application.Puertos;
using Ascenso.S1.Identidad.Domain.Repositorios;
using Ascenso.S1.Identidad.Infrastructure.Keycloak;
using Ascenso.S1.Identidad.Infrastructure.Notificaciones;
using Ascenso.S1.Identidad.Infrastructure.Persistencia;
using Ascenso.S1.Identidad.Infrastructure.Seguridad;
using Ascenso.SharedKernel.Seguridad;
using Ascenso.SharedKernel.Tiempo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ascenso.S1.Identidad.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentidadService(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? configurarDbContext = null)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IniciarSesionPaso1Command).Assembly));

        services.AddSingleton<IRelojNegocio>(new RelojSistemaCaracas(TimeProvider.System));
        services.AddSingleton<AppendOnlyRegistroDecisionInterceptor>();

        var vigenciaMinutos = configuration.GetValue<int?>("Autenticacion2Fa:VigenciaMinutos") ?? 15;
        var horasBloqueo = configuration.GetValue<int?>("Autenticacion2Fa:HorasBloqueo") ?? 1;
        services.AddSingleton(new ConfiguracionAutenticacion2Fa(vigenciaMinutos, horasBloqueo));

        services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.SectionName));
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.Configure<SesionTokenOptions>(configuration.GetSection(SesionTokenOptions.SectionName));

        services.AddDbContext<IdentidadDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetRequiredService<AppendOnlyRegistroDecisionInterceptor>());

            if (configurarDbContext is not null)
            {
                configurarDbContext(options);
                return;
            }

            var connectionString = configuration.GetConnectionString("IdentidadDb");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__ef_migrations_history", IdentidadDbContext.Schema);
                });
            }
        });

        services.AddScoped<IUsuarioRepository, UsuarioRepositoryEfCore>();
        services.AddScoped<ITipoUsuarioRepository, TipoUsuarioRepositoryEfCore>();
        services.AddScoped<IClienteApiUniversidadRepository, ClienteApiUniversidadRepositoryEfCore>();
        services.AddScoped<IDesafioSegundoFactorRepository, DesafioSegundoFactorRepositoryEfCore>();
        services.AddScoped<IDesafioOtpOperacionRepository, DesafioOtpOperacionRepositoryEfCore>();
        services.AddScoped<IBloqueoOtpOperacionRepository, BloqueoOtpOperacionRepositoryEfCore>();
        services.AddScoped<IUnidadTrabajoIdentidad, UnidadTrabajoIdentidadEfCore>();
        services.AddScoped<IServicioOtp, ServicioOtpAplicacion>();
        services.AddSingleton<ValidadorScopeInternoOtp>();

        services.AddHttpClient<IProveedorIdentidadKeycloak, ProveedorIdentidadKeycloakHttp>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddScoped<ICanalSms, CanalSmsSimulado>();
        services.AddScoped<ICanalEnvioCodigo, CanalEnvioCodigoCompuesto>();
        services.AddSingleton<IGeneradorCodigoSeguro, GeneradorCodigoSeguroCripto>();
        services.AddSingleton<IServicioSesionAutenticada, ServicioSesionAutenticadaJwt>();

        services.AddOutboxRabbitMqDispatcher<IdentidadDbContext>(configuration, "s1-identidad");

        return services;
    }
}
