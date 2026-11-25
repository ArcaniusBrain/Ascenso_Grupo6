using System.Reflection;
using System.Text.Json;
using Ascenso.Contracts;
using Ascenso.Contracts.Eventos;
using Ascenso.SharedKernel.TiposBase;
using Xunit;

namespace Ascenso.BuildingBlocks.Tests;

public sealed class ContractsTests
{
    private static readonly string[] PropiedadesProhibidas =
    [
        "Password", "Clave", "Contrasena", "NumeroCuenta", "CuentaBancaria",
        "IngresoFamiliar", "ComposicionFamiliar", "Token", "Secret", "OtpCodigo"
    ];

    [Fact]
    public void IntegrationEventEnvelope_SerializaYDeserializaCorrectamente()
    {
        var payload = new OfertaEmitidaV1(
            OfertaId: Guid.NewGuid(),
            ReservaId: Guid.NewGuid(),
            PostulacionId: Guid.NewGuid(),
            PostulanteId: Guid.NewGuid(),
            ProgramaId: Guid.NewGuid(),
            CicloId: Guid.NewGuid(),
            CodigoPeriodo: "2026-2",
            MontoTotalReservado: 16000m,
            DuracionPeriodos: 8,
            Moneda: "USD",
            FechaLimite: new DateOnly(2026, 12, 15));

        var sobre = new IntegrationEventEnvelope<OfertaEmitidaV1>(
            EventId: Guid.NewGuid(),
            EventType: "OfertaEmitida",
            SchemaVersion: 1,
            AggregateId: payload.OfertaId,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            Producer: "S4",
            CorrelationId: Guid.NewGuid(),
            CausationId: Guid.NewGuid(),
            Payload: payload);

        var json = JsonSerializer.Serialize(sobre);
        var recuperado = JsonSerializer.Deserialize<IntegrationEventEnvelope<OfertaEmitidaV1>>(json);

        Assert.NotNull(recuperado);
        Assert.Equal(sobre.EventId, recuperado!.EventId);
        Assert.Equal(sobre.Payload, recuperado.Payload);
    }

    [Fact]
    public void ContratosDeIntegracion_NoExponenEntidadesDeDominioNiDatosProhibidos()
    {
        var assembly = typeof(IntegrationEventEnvelope<>).Assembly;
        var tiposPublicos = assembly.GetExportedTypes();

        Assert.NotEmpty(tiposPublicos);

        foreach (var tipo in tiposPublicos)
        {
            Assert.False(EsSubtipoDeEntidad(tipo), $"El contrato {tipo.Name} no debe heredar de Entidad<TId>.");

            foreach (var propiedad in tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var prohibida = PropiedadesProhibidas.FirstOrDefault(p =>
                    propiedad.Name.Contains(p, StringComparison.OrdinalIgnoreCase));

                Assert.True(prohibida is null,
                    $"El contrato {tipo.Name}.{propiedad.Name} expone un campo sensible prohibido ({prohibida}).");
            }
        }
    }

    private static bool EsSubtipoDeEntidad(Type tipo)
    {
        var actual = tipo.BaseType;
        while (actual is not null)
        {
            if (actual.IsGenericType && actual.GetGenericTypeDefinition() == typeof(Entidad<>))
            {
                return true;
            }
            actual = actual.BaseType;
        }
        return false;
    }
}
