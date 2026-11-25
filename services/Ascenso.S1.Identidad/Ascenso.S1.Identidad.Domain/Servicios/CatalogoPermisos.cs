namespace Ascenso.S1.Identidad.Domain.Servicios;

/// <summary>
/// Catálogo único de permisos de ASCENSO (Anexo A HU-37.1, ADR-11).
/// Los tipos de usuario almacenan estos códigos como datos configurables en base de datos,
/// permitiendo crear nuevos tipos sin cambiar código ni redesplegar.
/// </summary>
public static class CatalogoPermisos
{
    public const string UsuariosGestionar = "usuarios.gestionar";
    public const string TiposUsuarioGestionar = "tipos_usuario.gestionar";
    public const string PeriodosGestionar = "periodos.gestionar";
    public const string ProgramasGestionar = "programas.gestionar";
    public const string RubricasModificar = "rubricas.modificar";
    public const string EvaluacionesPuntuar = "evaluaciones.puntuar";
    public const string EvaluacionesVerAjenasPostCierre = "evaluaciones.ver_ajenas_post_cierre";
    public const string DatosSocioeconomicosVer = "datos_socioeconomicos.ver";
    public const string BecasDecidir = "becas.decidir";
    public const string FinanzasConsultar = "finanzas.consultar";
    public const string FinanzasDesembolsar = "finanzas.desembolsar";
    public const string ConsultasResponder = "consultas.responder";
    public const string ReportesInstitucionalConsultar = "reportes.institucional";
    public const string ReportesImpactoGenerar = "reportes.impacto";
    public const string ReportesAcademicosUniversidadEnviar = "reportes_academicos.universidad_enviar";
    public const string RegistroDecisionesConsultarTodos = "registro_decisiones.consultar_todos";
    public const string RegistroDecisionesConsultarPrograma = "registro_decisiones.consultar_programa";
    public const string RegistroDecisionesConsultarFinanciero = "registro_decisiones.consultar_financiero";
    public const string RegistroDecisionesConsultarPropio = "registro_decisiones.consultar_propio";

    private static readonly HashSet<string> PermisosConAccesoFinancieroODecision = new(StringComparer.OrdinalIgnoreCase)
    {
        UsuariosGestionar,
        TiposUsuarioGestionar,
        PeriodosGestionar,
        ProgramasGestionar,
        RubricasModificar,
        EvaluacionesPuntuar,
        BecasDecidir,
        FinanzasConsultar,
        FinanzasDesembolsar,
        ReportesInstitucionalConsultar,
        ReportesImpactoGenerar,
        ReportesAcademicosUniversidadEnviar,
        RegistroDecisionesConsultarTodos,
        RegistroDecisionesConsultarPrograma,
        RegistroDecisionesConsultarFinanciero
    };

    public static bool ImplicaAccesoFinancieroODecision(string permiso) =>
        !string.IsNullOrWhiteSpace(permiso) && PermisosConAccesoFinancieroODecision.Contains(permiso.Trim());

    public static bool AlgunoImplicaAccesoFinancieroODecision(IEnumerable<string> permisos) =>
        permisos.Any(ImplicaAccesoFinancieroODecision);
}
