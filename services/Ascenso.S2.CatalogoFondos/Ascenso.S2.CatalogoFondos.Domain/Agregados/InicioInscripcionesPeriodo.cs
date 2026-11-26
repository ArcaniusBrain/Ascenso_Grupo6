using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Fecha única de inicio de inscripciones de una universidad aliada para un periodo académico (HU-38.1 E10–E13, C4).
/// Interpretada en zona horaria fija America/Caracas desde las 00:00 UTC-4 (Anexo B · HU-38.1 #4, R20, C15).
/// No se altera ni cierra al cambiar el periodo actual (HU-38.2 E11).
/// </summary>
public sealed class InicioInscripcionesPeriodo : RaizDeAgregado<Guid>
{
    public Guid UniversidadId { get; private set; }
    public string NombreUniversidad { get; private set; } = string.Empty;
    public CodigoPeriodo CodigoPeriodo { get; private set; } = default!;
    public DateOnly FechaInicio { get; private set; }
    public DateTimeOffset RegistradoEnUtc { get; private set; }

    private InicioInscripcionesPeriodo()
    {
    }

    private InicioInscripcionesPeriodo(
        Guid id,
        Guid universidadId,
        string nombreUniversidad,
        CodigoPeriodo codigoPeriodo,
        DateOnly fechaInicio,
        DateTimeOffset registradoEnUtc) : base(id)
    {
        UniversidadId = universidadId;
        NombreUniversidad = nombreUniversidad;
        CodigoPeriodo = codigoPeriodo;
        FechaInicio = fechaInicio;
        RegistradoEnUtc = registradoEnUtc;
    }

    public static InicioInscripcionesPeriodo Crear(
        Guid universidadId,
        string? nombreUniversidad,
        CodigoPeriodo codigoPeriodo,
        DateOnly? fechaInicio,
        DateTimeOffset ahoraUtc)
    {
        if (universidadId == Guid.Empty)
        {
            throw new ExcepcionDeDominio(
                "Debes indicar la universidad aliada.",
                "UNIVERSIDAD_OBLIGATORIA");
        }

        if (string.IsNullOrWhiteSpace(nombreUniversidad))
        {
            throw new ExcepcionDeDominio(
                "Debes indicar el nombre de la universidad aliada.",
                "NOMBRE_UNIVERSIDAD_OBLIGATORIO");
        }

        if (fechaInicio is null)
        {
            throw new ExcepcionDeDominio(
                "Debes indicar la fecha de inicio de inscripciones.",
                "FECHA_INICIO_INSCRIPCIONES_OBLIGATORIA");
        }

        return new InicioInscripcionesPeriodo(
            Guid.NewGuid(),
            universidadId,
            nombreUniversidad.Trim(),
            codigoPeriodo,
            fechaInicio.Value,
            ahoraUtc);
    }

    /// <summary>
    /// Determina si ya llegó la fecha y hora (00:00 UTC-4) de inicio de inscripciones de esta universidad (R20, C15).
    /// </summary>
    public bool InscripcionesIniciadas(IRelojNegocio reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        return reloj.AhoraCaracas >= RelojSistemaCaracas.InicioDeDiaCaracas(FechaInicio);
    }
}
