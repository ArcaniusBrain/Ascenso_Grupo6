namespace Ascenso.SharedKernel.Tiempo;

/// <summary>
/// Reloj de negocio inyectable (ADR-17, R20).
/// Almacena instantes en UTC y evalúa fechas límite en la zona fija America/Caracas (UTC-4, sin horario de verano).
/// </summary>
public interface IRelojNegocio
{
    DateTimeOffset AhoraUtc { get; }
    DateTimeOffset AhoraCaracas { get; }
    DateOnly HoyCaracas { get; }
}

public sealed class RelojSistemaCaracas : IRelojNegocio
{
    public static readonly TimeSpan DesfaseCaracas = TimeSpan.FromHours(-4);

    private readonly TimeProvider _timeProvider;

    public RelojSistemaCaracas(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public DateTimeOffset AhoraUtc => _timeProvider.GetUtcNow();

    public DateTimeOffset AhoraCaracas => AhoraUtc.ToOffset(DesfaseCaracas);

    public DateOnly HoyCaracas => DateOnly.FromDateTime(AhoraCaracas.DateTime);

    public static DateTimeOffset ConvertirACaracas(DateTimeOffset momento) => momento.ToOffset(DesfaseCaracas);

    public static DateTimeOffset FinDeDiaCaracas(DateOnly fecha)
        => new(fecha.Year, fecha.Month, fecha.Day, 23, 59, 59, 999, DesfaseCaracas);

    public static DateTimeOffset InicioDeDiaCaracas(DateOnly fecha)
        => new(fecha.Year, fecha.Month, fecha.Day, 0, 0, 0, DesfaseCaracas);
}
