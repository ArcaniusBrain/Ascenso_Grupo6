namespace Ascenso.S4.Becas.Domain.Becas;

/// <summary>
/// Estados del ciclo de vida de la beca (DDD_v3.3 §11b, §15.2).
/// </summary>
public enum EstadoBeca
{
    Otorgada,
    Activa,
    Suspendida,
    Renovada,
    Inactiva,
    Concluida,
    Revocada
}

/// <summary>
/// Marca del programa para la regla de no doble beneficio (C2, HU-18.1).
/// </summary>
public enum Alcance
{
    Completo,
    Parcial
}

/// <summary>
/// Modalidad de pago heredada del programa (DDD_v3.3 §11b).
/// </summary>
public enum ModalidadPago
{
    Mensual,
    PorPeriodo,
    DirectoUniversidad
}

/// <summary>
/// Origen de un tramo aprobado de la beca (DDD_v3.3 §11b).
/// </summary>
public enum OrigenTramo
{
    Oferta,
    Renovacion
}
