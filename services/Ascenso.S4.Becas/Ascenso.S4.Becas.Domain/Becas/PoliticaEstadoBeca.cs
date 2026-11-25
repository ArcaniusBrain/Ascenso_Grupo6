namespace Ascenso.S4.Becas.Domain.Becas;

/// <summary>
/// Única fuente de verdad en S4 sobre qué estados cuentan como beca activa (S6, HU-18.1, HU-16.1, HU-01.2).
/// </summary>
public static class PoliticaEstadoBeca
{
    public static bool CuentaComoActiva(EstadoBeca estado) => estado is
        EstadoBeca.Otorgada or
        EstadoBeca.Activa or
        EstadoBeca.Suspendida or
        EstadoBeca.Renovada;
}
