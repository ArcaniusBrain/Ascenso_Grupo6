namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

public enum TipoReservaPresupuesto
{
    OFERTA_NUEVA = 1,
    RENOVACION = 2
}

public enum EstadoReservaPresupuesto
{
    RESERVADA = 1,
    VINCULADA = 2,
    CONFIRMADA = 3,
    LIBERADA = 4
}

public enum MotivoLiberacionReserva
{
    COMPENSACION = 1,
    RECHAZO = 2,
    EXPIRACION = 3,
    REVOCACION = 4
}
