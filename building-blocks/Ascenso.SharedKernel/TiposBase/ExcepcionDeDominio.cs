namespace Ascenso.SharedKernel.TiposBase;

public class ExcepcionDeDominio : Exception
{
    public string Codigo { get; }

    public ExcepcionDeDominio(string mensaje, string codigo = "REGLA_DOMINIO")
        : base(mensaje)
    {
        Codigo = codigo;
    }
}
