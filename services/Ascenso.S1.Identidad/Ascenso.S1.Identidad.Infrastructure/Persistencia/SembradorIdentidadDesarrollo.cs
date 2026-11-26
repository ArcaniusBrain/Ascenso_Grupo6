using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.S1.Identidad.Domain.Servicios;
using Microsoft.EntityFrameworkCore;

namespace Ascenso.S1.Identidad.Infrastructure.Persistencia;

/// <summary>
/// Sembrador idempotente del catálogo base de tipos de usuario predefinidos en S1 (HU-37.1, ADR-11).
/// No modifica registros existentes ni elimina tipos personalizados.
/// </summary>
public static class SembradorIdentidadDesarrollo
{
    public static async Task<int> SembrarTiposPredefinidosAsync(
        IdentidadDbContext db,
        CancellationToken ct = default)
    {
        var definiciones = new[]
        {
            (
                Nombre: "DIRECCION",
                Permisos: new[]
                {
                    CatalogoPermisos.UsuariosGestionar,
                    CatalogoPermisos.TiposUsuarioGestionar,
                    CatalogoPermisos.PeriodosGestionar,
                    CatalogoPermisos.ProgramasGestionar,
                    CatalogoPermisos.RubricasModificar,
                    CatalogoPermisos.EvaluacionesVerAjenasPostCierre,
                    CatalogoPermisos.DatosSocioeconomicosVer,
                    CatalogoPermisos.BecasDecidir,
                    CatalogoPermisos.FinanzasConsultar,
                    CatalogoPermisos.ReportesInstitucionalConsultar,
                    CatalogoPermisos.ReportesImpactoGenerar,
                    CatalogoPermisos.RegistroDecisionesConsultarTodos
                },
                AccesoFinancieroODecision: true,
                EsEnlaceUniversidad: false
            ),
            (
                Nombre: "COORDINADOR",
                Permisos: new[]
                {
                    CatalogoPermisos.PeriodosGestionar,
                    CatalogoPermisos.ProgramasGestionar,
                    CatalogoPermisos.RubricasModificar,
                    CatalogoPermisos.EvaluacionesVerAjenasPostCierre,
                    CatalogoPermisos.DatosSocioeconomicosVer,
                    CatalogoPermisos.BecasDecidir,
                    CatalogoPermisos.ConsultasResponder,
                    CatalogoPermisos.ReportesImpactoGenerar,
                    CatalogoPermisos.RegistroDecisionesConsultarPrograma
                },
                AccesoFinancieroODecision: true,
                EsEnlaceUniversidad: false
            ),
            (
                Nombre: "FINANZAS",
                Permisos: new[]
                {
                    CatalogoPermisos.FinanzasConsultar,
                    CatalogoPermisos.FinanzasDesembolsar,
                    CatalogoPermisos.RegistroDecisionesConsultarFinanciero
                },
                AccesoFinancieroODecision: true,
                EsEnlaceUniversidad: false
            ),
            (
                Nombre: "COMITE",
                Permisos: new[]
                {
                    CatalogoPermisos.EvaluacionesPuntuar,
                    CatalogoPermisos.DatosSocioeconomicosVer
                },
                AccesoFinancieroODecision: true,
                EsEnlaceUniversidad: false
            ),
            (
                Nombre: "ENLACE_UNIVERSIDAD",
                Permisos: new[]
                {
                    CatalogoPermisos.ReportesAcademicosUniversidadEnviar
                },
                AccesoFinancieroODecision: false,
                EsEnlaceUniversidad: true
            ),
            (
                Nombre: "POSTULANTE",
                Permisos: new[]
                {
                    CatalogoPermisos.RegistroDecisionesConsultarPropio
                },
                AccesoFinancieroODecision: false,
                EsEnlaceUniversidad: false
            ),
            (
                Nombre: "BECARIO",
                Permisos: new[]
                {
                    CatalogoPermisos.RegistroDecisionesConsultarPropio
                },
                AccesoFinancieroODecision: false,
                EsEnlaceUniversidad: false
            )
        };

        var existentes = await db.TiposUsuario
            .AsNoTracking()
            .Select(t => t.NombreNormalizado)
            .ToListAsync(ct);

        var setExistentes = new HashSet<string>(existentes, StringComparer.OrdinalIgnoreCase);
        var agregados = 0;

        foreach (var def in definiciones)
        {
            if (setExistentes.Contains(def.Nombre))
            {
                continue;
            }

            var tipo = TipoUsuario.Crear(
                def.Nombre,
                def.Permisos,
                accesoFinancieroODecision: def.AccesoFinancieroODecision,
                esEnlaceUniversidad: def.EsEnlaceUniversidad,
                esPredefinido: true);

            await db.TiposUsuario.AddAsync(tipo, ct);
            agregados++;
        }

        if (agregados > 0)
        {
            await db.SaveChangesAsync(ct);
        }

        return agregados;
    }
}
