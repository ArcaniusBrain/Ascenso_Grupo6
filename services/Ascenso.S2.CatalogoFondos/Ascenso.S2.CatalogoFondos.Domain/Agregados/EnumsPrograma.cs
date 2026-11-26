namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Modalidad de desembolso fijada por el programa de becas (DDD v3.3 §8, HU-11.1, RF-19).
/// </summary>
public enum ModalidadPago
{
    MENSUAL = 1,
    SEMESTRAL = 2,
    DIRECTO_UNIVERSIDAD = 3
}

/// <summary>
/// Etiqueta del programa para la regla de no doble beneficio (DDD v3.3 §8, HU-11.1, HU-01.2).
/// No interviene en el cálculo del monto fijo por periodo.
/// </summary>
public enum AlcancePrograma
{
    COMPLETO = 1,
    PARCIAL = 2
}
