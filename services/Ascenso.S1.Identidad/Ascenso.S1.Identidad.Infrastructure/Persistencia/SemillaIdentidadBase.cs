using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.S1.Identidad.Domain.Servicios;
using Microsoft.EntityFrameworkCore;

namespace Ascenso.S1.Identidad.Infrastructure.Persistencia;

/// <summary>
/// Semilla idempotente de los 6 tipos de usuario base configurables de ASCENSO (HU-37.1, C14).
/// </summary>
public static class SemillaIdentidadBase
{
    public static readonly Guid TipoDireccionId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    public static readonly Guid TipoCoordinadorId = Guid.Parse("11111111-1111-1111-1111-111111111102");
    public static readonly Guid TipoEvaluadorId = Guid.Parse("11111111-1111-1111-1111-111111111103");
    public static readonly Guid TipoFinanzasId = Guid.Parse("11111111-1111-1111-1111-111111111104");
    public static readonly Guid TipoEnlaceUniversidadId = Guid.Parse("11111111-1111-1111-1111-111111111105");
    public static readonly Guid TipoPostulanteId = Guid.Parse("11111111-1111-1111-1111-111111111106");

    public static async Task AsegurarTiposBaseAsync(IdentidadDbContext db, CancellationToken cancellationToken = default)
    {
        var existentes = await db.TiposUsuario.Select(t => t.Id).ToListAsync(cancellationToken);
        var setExistentes = existentes.ToHashSet();

        var tiposBase = new[]
        {
            TipoUsuario.Crear(
                "Dirección",
                new[]
                {
                    CatalogoPermisos.UsuariosGestionar,
                    CatalogoPermisos.TiposUsuarioGestionar,
                    CatalogoPermisos.PeriodosGestionar,
                    CatalogoPermisos.ProgramasGestionar,
                    CatalogoPermisos.RubricasModificar,
                    CatalogoPermisos.DatosSocioeconomicosVer,
                    CatalogoPermisos.BecasDecidir,
                    CatalogoPermisos.FinanzasConsultar,
                    CatalogoPermisos.ReportesInstitucionalConsultar,
                    CatalogoPermisos.RegistroDecisionesConsultarTodos
                },
                accesoFinancieroODecision: true,
                esEnlaceUniversidad: false,
                esPredefinido: true,
                id: TipoDireccionId),

            TipoUsuario.Crear(
                "Coordinador",
                new[]
                {
                    CatalogoPermisos.ProgramasGestionar,
                    CatalogoPermisos.RubricasModificar,
                    CatalogoPermisos.DatosSocioeconomicosVer,
                    CatalogoPermisos.BecasDecidir,
                    CatalogoPermisos.ConsultasResponder,
                    CatalogoPermisos.ReportesImpactoGenerar,
                    CatalogoPermisos.RegistroDecisionesConsultarPrograma,
                    CatalogoPermisos.EvaluacionesVerAjenasPostCierre
                },
                accesoFinancieroODecision: true,
                esEnlaceUniversidad: false,
                esPredefinido: true,
                id: TipoCoordinadorId),

            TipoUsuario.Crear(
                "Evaluador",
                new[]
                {
                    CatalogoPermisos.EvaluacionesPuntuar,
                    CatalogoPermisos.EvaluacionesVerAjenasPostCierre,
                    CatalogoPermisos.DatosSocioeconomicosVer
                },
                accesoFinancieroODecision: true,
                esEnlaceUniversidad: false,
                esPredefinido: true,
                id: TipoEvaluadorId),

            TipoUsuario.Crear(
                "Finanzas",
                new[]
                {
                    CatalogoPermisos.FinanzasConsultar,
                    CatalogoPermisos.FinanzasDesembolsar,
                    CatalogoPermisos.RegistroDecisionesConsultarFinanciero
                },
                accesoFinancieroODecision: true,
                esEnlaceUniversidad: false,
                esPredefinido: true,
                id: TipoFinanzasId),

            TipoUsuario.Crear(
                "Enlace Universidad",
                new[]
                {
                    CatalogoPermisos.ReportesAcademicosUniversidadEnviar
                },
                accesoFinancieroODecision: false,
                esEnlaceUniversidad: true,
                esPredefinido: true,
                id: TipoEnlaceUniversidadId),

            TipoUsuario.Crear(
                "Postulante",
                new[]
                {
                    CatalogoPermisos.RegistroDecisionesConsultarPropio
                },
                accesoFinancieroODecision: false,
                esEnlaceUniversidad: false,
                esPredefinido: true,
                id: TipoPostulanteId)
        };

        var agregoAlguno = false;
        foreach (var tipo in tiposBase)
        {
            if (!setExistentes.Contains(tipo.Id))
            {
                db.TiposUsuario.Add(tipo);
                agregoAlguno = true;
            }
        }

        if (agregoAlguno)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
