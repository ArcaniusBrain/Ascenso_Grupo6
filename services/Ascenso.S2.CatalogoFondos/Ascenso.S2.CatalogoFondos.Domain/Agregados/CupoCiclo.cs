using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Entidad interna de PresupuestoPrograma que controla el cupo de nuevos becarios de un ciclo (DDD v3.3 §8, HU-12.1 E11).
/// Permite verificar cupo del programa y cupo del ciclo en la misma transacción versionada.
/// </summary>
public sealed class CupoCiclo : Entidad<Guid>
{
    public Guid PresupuestoProgramaId { get; private set; }
    public Guid CicloId { get; private set; }
    public int Limite { get; private set; }
    public int Ocupados { get; private set; }

    private CupoCiclo()
    {
    }

    internal CupoCiclo(Guid id, Guid presupuestoProgramaId, Guid cicloId, int limite) : base(id)
    {
        if (cicloId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("Indica el ciclo de postulación.", "CICLO_ID_INVALIDO");
        }

        if (limite <= 0)
        {
            throw new ExcepcionDeDominio("El cupo del ciclo debe ser mayor que cero.", "CUPO_CICLO_INVALIDO");
        }

        PresupuestoProgramaId = presupuestoProgramaId;
        CicloId = cicloId;
        Limite = limite;
        Ocupados = 0;
    }

    public bool HayCupo() => Ocupados < Limite;

    internal void ActualizarLimite(int nuevoLimite)
    {
        if (nuevoLimite <= 0 || nuevoLimite < Ocupados)
        {
            throw new ExcepcionDeDominio(
                "El límite del ciclo debe ser mayor que cero y no inferior a los cupos ya ocupados.",
                "CUPO_CICLO_INVALIDO");
        }

        Limite = nuevoLimite;
    }

    internal void Ocupar()
    {
        if (!HayCupo())
        {
            throw new ExcepcionDeDominio(
                PresupuestoPrograma.MensajeSinCupoCiclo,
                "CUPO_CICLO_INSUFICIENTE");
        }

        Ocupados++;
    }

    internal void Liberar()
    {
        if (Ocupados > 0)
        {
            Ocupados--;
        }
    }
}
