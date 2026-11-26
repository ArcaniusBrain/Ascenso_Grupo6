using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S4.Becas.Domain.Becas;

public enum RolHistorialBecas
{
    Coordinador,
    Direccion,
    Becario,
    Postulante,
    Otro
}

/// <summary>
/// Reglas de acceso por rol para la consulta del historial de becas (HU-18.1, RF-37).
/// </summary>
public static class PoliticaAccesoHistorialBecas
{
    public static bool PuedeConsultarPorCedula(RolHistorialBecas rol, string? cedulaActor, string cedulaConsultada)
    {
        if (rol is RolHistorialBecas.Coordinador or RolHistorialBecas.Direccion)
        {
            return true;
        }

        if (rol != RolHistorialBecas.Becario || string.IsNullOrWhiteSpace(cedulaActor))
        {
            return false;
        }

        var actorNormalizada = new Cedula(cedulaActor).Numero;
        var consultadaNormalizada = new Cedula(cedulaConsultada).Numero;
        return string.Equals(actorNormalizada, consultadaNormalizada, StringComparison.Ordinal);
    }

    public static bool PuedeConsultarAgregado(RolHistorialBecas rol) =>
        rol is RolHistorialBecas.Coordinador or RolHistorialBecas.Direccion;
}
