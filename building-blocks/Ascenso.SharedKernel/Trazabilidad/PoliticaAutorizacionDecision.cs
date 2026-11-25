namespace Ascenso.SharedKernel.Trazabilidad;

/// <summary>
/// Perfil funcional relevante para evaluar el acceso al registro de decisiones (RF-35, RF-37).
/// </summary>
public enum PerfilAccesoDecision
{
    DIRECCION = 1,
    COORDINADOR = 2,
    FINANZAS = 3,
    COMITE_EVALUADOR = 4,
    ENLACE_UNIVERSIDAD = 5,
    POSTULANTE_O_BECARIO = 6
}

/// <summary>
/// Alcance efectivo autorizado para consultar el registro de decisiones (HU-35.2).
/// </summary>
public enum AlcanceConsultaDecisiones
{
    TODOS = 1,
    SOLO_PROGRAMAS_ASIGNADOS = 2,
    SOLO_AMBITO_FINANCIERO = 3,
    SOLO_CASO_PROPIO = 4
}

/// <summary>
/// Contexto autenticado del usuario que intenta registrar o consultar decisiones.
/// </summary>
public sealed record ContextoSolicitanteDecision(
    Guid UsuarioId,
    PerfilAccesoDecision Perfil,
    bool TienePoderDeDecisionEnCaso = false,
    IReadOnlySet<Guid>? ProgramasAsignados = null)
{
    public IReadOnlySet<Guid> ProgramasEfectivos { get; } =
        ProgramasAsignados ?? new HashSet<Guid>();
}

/// <summary>
/// Reglas puras de autorización sobre el registro de decisiones (HU-35.1 E4/E5/E7 y HU-35.2).
/// Se evalúan en la capa de dominio/aplicación y se refuerzan en la consulta de datos, nunca solo en UI.
/// </summary>
public static class PoliticaAutorizacionDecision
{
    /// <summary>
    /// Valida que el solicitante tenga poder de decisión sobre el caso antes de registrar una decisión (HU-35.1 E7).
    /// </summary>
    public static void ValidarPermisoRegistro(ContextoSolicitanteDecision solicitante, Guid? programaId = null)
    {
        ArgumentNullException.ThrowIfNull(solicitante);

        if (solicitante.UsuarioId == Guid.Empty)
        {
            throw new ExcepcionAccesoDenegadoDecision("El usuario autenticado es inválido.");
        }

        if (solicitante.Perfil is PerfilAccesoDecision.ENLACE_UNIVERSIDAD or PerfilAccesoDecision.POSTULANTE_O_BECARIO)
        {
            throw new ExcepcionAccesoDenegadoDecision(
                "No tienes permiso para registrar decisiones sobre este caso.");
        }

        if (!solicitante.TienePoderDeDecisionEnCaso)
        {
            throw new ExcepcionAccesoDenegadoDecision(
                "No tienes permiso para registrar decisiones sobre este caso.");
        }

        if (solicitante.Perfil == PerfilAccesoDecision.COORDINADOR
            && programaId.HasValue
            && !solicitante.ProgramasEfectivos.Contains(programaId.Value))
        {
            throw new ExcepcionAccesoDenegadoDecision(
                "No tienes permiso para decidir sobre un programa que no coordinas.");
        }
    }

    /// <summary>
    /// Determina el alcance autorizado de consulta de solo lectura para el solicitante (HU-35.2).
    /// </summary>
    public static AlcanceConsultaDecisiones ResolverAlcanceConsulta(ContextoSolicitanteDecision solicitante)
    {
        ArgumentNullException.ThrowIfNull(solicitante);

        if (solicitante.UsuarioId == Guid.Empty)
        {
            throw new ExcepcionAccesoDenegadoDecision("El usuario autenticado es inválido.");
        }

        return solicitante.Perfil switch
        {
            PerfilAccesoDecision.DIRECCION => AlcanceConsultaDecisiones.TODOS,
            PerfilAccesoDecision.COORDINADOR => AlcanceConsultaDecisiones.SOLO_PROGRAMAS_ASIGNADOS,
            PerfilAccesoDecision.FINANZAS => AlcanceConsultaDecisiones.SOLO_AMBITO_FINANCIERO,
            PerfilAccesoDecision.POSTULANTE_O_BECARIO => AlcanceConsultaDecisiones.SOLO_CASO_PROPIO,
            _ => throw new ExcepcionAccesoDenegadoDecision(
                "No tienes permiso para consultar el registro de decisiones.")
        };
    }

    /// <summary>
    /// Valida si un registro concreto es visible para el solicitante según su alcance (HU-35.2).
    /// </summary>
    public static bool PuedeVerRegistro(ContextoSolicitanteDecision solicitante, RegistroDecision registro)
    {
        ArgumentNullException.ThrowIfNull(solicitante);
        ArgumentNullException.ThrowIfNull(registro);

        return solicitante.Perfil switch
        {
            PerfilAccesoDecision.DIRECCION => true,
            PerfilAccesoDecision.COORDINADOR =>
                registro.ProgramaId.HasValue && solicitante.ProgramasEfectivos.Contains(registro.ProgramaId.Value),
            PerfilAccesoDecision.FINANZAS => registro.Tipo.EsAmbitoFinanciero(),
            PerfilAccesoDecision.POSTULANTE_O_BECARIO =>
                registro.PropietarioUsuarioId.HasValue && registro.PropietarioUsuarioId.Value == solicitante.UsuarioId,
            _ => false
        };
    }

    /// <summary>
    /// Ningún rol, incluida Dirección, puede editar ni eliminar un registro de decisión (HU-35.1 E4 y E5).
    /// </summary>
    public static void ValidarIntentoModificacionOEliminacion(ContextoSolicitanteDecision solicitante)
    {
        ArgumentNullException.ThrowIfNull(solicitante);
        throw new ExcepcionInmutabilidadRegistro();
    }
}
