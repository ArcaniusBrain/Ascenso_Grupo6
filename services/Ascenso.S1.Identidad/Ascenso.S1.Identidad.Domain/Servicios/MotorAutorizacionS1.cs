using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S1.Identidad.Domain.Servicios;

public sealed record EvaluacionPuntuacionResumen(Guid EvaluadorId, decimal Puntaje, string Comentario);

public sealed record ExpedientePostulacionCompleto(
    Guid PostulacionId,
    string NombrePostulante,
    string Carrera,
    decimal? IngresoFamiliarMensual,
    string? ComposicionFamiliar);

public sealed record ExpedientePostulacionProyectado(
    Guid PostulacionId,
    string NombrePostulante,
    string Carrera,
    decimal? IngresoFamiliarMensual,
    string? ComposicionFamiliar);

/// <summary>
/// Motor centralizado de autorización y confidencialidad de S1 (HU-37.1, ADR-11).
/// Evalúa permisos configurables, alcance, estado del proceso (S9) y masking de campos socioeconómicos en backend.
/// </summary>
public static class MotorAutorizacionS1
{
    public const string MensajeNoModificarRubricas = "Tu rol no permite modificar rúbricas de evaluación.";

    public static bool TienePermiso(IEnumerable<TipoUsuario> tiposDelUsuario, string permiso) =>
        tiposDelUsuario.Any(t => t.TienePermiso(permiso));

    /// <summary>
    /// Solo Dirección (permiso usuarios.gestionar / tipos_usuario.gestionar) puede crear usuarios, tipos o asignar roles (HU-37.1 E2 y E8).
    /// </summary>
    public static void ValidarGestionIdentidadPorDireccion(
        Guid actorUsuarioId,
        IEnumerable<TipoUsuario> tiposDelActor,
        string permisoRequerido)
    {
        if (actorUsuarioId == Guid.Empty || !TienePermiso(tiposDelActor, permisoRequerido))
        {
            throw new ExcepcionDeDominio(
                "No tienes permiso para gestionar usuarios o roles en la plataforma.",
                "ACCESO_DENEGADO_IDENTIDAD");
        }
    }

    /// <summary>
    /// Impide que cualquier usuario sin permiso de Dirección se autoasigne un rol distinto del suyo (HU-37.1 E8).
    /// </summary>
    public static void ValidarAsignacionDeRoles(
        Guid actorUsuarioId,
        IEnumerable<TipoUsuario> tiposDelActor,
        Guid usuarioObjetivoId)
    {
        if (!TienePermiso(tiposDelActor, CatalogoPermisos.UsuariosGestionar))
        {
            var mensaje = actorUsuarioId == usuarioObjetivoId
                ? "No puedes asignarte un rol distinto del que tienes autorizado."
                : "Solo Dirección puede asignar o modificar roles de usuarios.";

            throw new ExcepcionDeDominio(mensaje, "AUTO_ESCALAMIENTO_BLOQUEADO");
        }
    }

    /// <summary>
    /// Impide que el equipo financiero u otros roles sin permiso modifiquen rúbricas (HU-37.1 E5).
    /// </summary>
    public static void ValidarModificacionRubrica(IEnumerable<TipoUsuario> tiposDelActor)
    {
        if (!TienePermiso(tiposDelActor, CatalogoPermisos.RubricasModificar))
        {
            throw new ExcepcionDeDominio(MensajeNoModificarRubricas, "PERMISO_RUBRICAS_DENEGADO");
        }
    }

    /// <summary>
    /// Regla de estado (HU-37.1 E4, HU-08.2, S9): mientras el periodo de revisión esté abierto,
    /// un evaluador solo recibe sus propias puntuaciones y jamás las de otros evaluadores.
    /// </summary>
    public static IReadOnlyList<EvaluacionPuntuacionResumen> FiltrarPuntuacionesVisibles(
        Guid evaluadorSolicitanteId,
        IEnumerable<TipoUsuario> tiposDelSolicitante,
        bool periodoRevisionCerrado,
        IEnumerable<EvaluacionPuntuacionResumen> evaluaciones)
    {
        ArgumentNullException.ThrowIfNull(evaluaciones);

        if (periodoRevisionCerrado && TienePermiso(tiposDelSolicitante, CatalogoPermisos.EvaluacionesVerAjenasPostCierre))
        {
            return evaluaciones.ToList();
        }

        return evaluaciones
            .Where(e => e.EvaluadorId == evaluadorSolicitanteId)
            .ToList();
    }

    /// <summary>
    /// Regla de confidencialidad en backend (HU-37.1 E6, ADR-11):
    /// omite ingreso familiar y composición familiar de la respuesta cuando el rol carece de datos_socioeconomicos.ver.
    /// </summary>
    public static ExpedientePostulacionProyectado ProyectarPostulacionSegunConfidencialidad(
        IEnumerable<TipoUsuario> tiposDelSolicitante,
        ExpedientePostulacionCompleto expediente)
    {
        ArgumentNullException.ThrowIfNull(expediente);

        var puedeVerSocioeconomicos = TienePermiso(tiposDelSolicitante, CatalogoPermisos.DatosSocioeconomicosVer);

        return new ExpedientePostulacionProyectado(
            PostulacionId: expediente.PostulacionId,
            NombrePostulante: expediente.NombrePostulante,
            Carrera: expediente.Carrera,
            IngresoFamiliarMensual: puedeVerSocioeconomicos ? expediente.IngresoFamiliarMensual : null,
            ComposicionFamiliar: puedeVerSocioeconomicos ? expediente.ComposicionFamiliar : null);
    }
}
