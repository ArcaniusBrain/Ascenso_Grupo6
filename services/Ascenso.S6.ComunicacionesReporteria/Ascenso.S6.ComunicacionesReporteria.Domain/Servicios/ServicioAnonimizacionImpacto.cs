using Ascenso.S6.ComunicacionesReporteria.Domain.Agregados;
using Ascenso.S6.ComunicacionesReporteria.Domain.Modelos;

namespace Ascenso.S6.ComunicacionesReporteria.Domain.Servicios;

/// <summary>
/// Capa única centralizada de agregación y anonimización para reportes dirigidos a donantes (Anexo A HU-33.1, C20).
/// Garantiza por diseño:
/// 1. Ningún dato socioeconómico (ingreso familiar, composición familiar) pasa jamás a la salida.
/// 2. Becarios sin ConsentimientoDonante vigente nunca exponen nombre ni cédula (solo cifras agregadas).
/// 3. Becarios con ConsentimientoDonante vigente para este donante se presentan con nombre, cédula, universidad, carrera, programa y promedio.
/// 4. Ejecución presupuestaria contabiliza únicamente desembolsos CONFIRMADO (excluye FALLIDO, DEVUELTO y PROGRAMADO).
/// 5. Reportes académicos de origen API y MANUAL se agregan por igual; sin reportes muestra el mensaje literal exigido.
/// </summary>
public static class ServicioAnonimizacionImpacto
{
    public static ReporteImpactoContenido ConstruirReporte(
        Guid donanteId,
        string nombreDonante,
        string codigoPeriodo,
        IReadOnlyList<AporteDonanteProyectado> aportesDonanteEnPeriodo,
        IReadOnlyList<BecarioProyectadoEntrada> becariosDeProgramas,
        IReadOnlyList<DesembolsoProyectadoEntrada> desembolsosEnPeriodo,
        IReadOnlyList<ReporteAcademicoProyectadoEntrada> reportesAcademicosEnPeriodo,
        IReadOnlySet<Guid> becariosConConsentimientoParaDonante,
        DateTimeOffset generadoEnUtc)
    {
        var gruposPorPrograma = aportesDonanteEnPeriodo
            .Where(a => a.DonanteId == donanteId)
            .GroupBy(a => a.ProgramaId)
            .OrderBy(g => g.First().NombrePrograma, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var monedaGlobal = gruposPorPrograma.FirstOrDefault()?.First().Moneda ?? "USD";
        var programasSalida = new List<ProgramaImpactoReporte>(gruposPorPrograma.Length);
        var becariosUnicos = new HashSet<Guid>();

        foreach (var grupo in gruposPorPrograma)
        {
            var programaId = grupo.Key;
            var nombrePrograma = grupo.First().NombrePrograma;
            var monedaPrograma = grupo.First().Moneda;
            var aporteDonante = grupo.Sum(x => x.MontoAportado);

            var becariosPrograma = becariosDeProgramas
                .Where(b => b.ProgramaId == programaId)
                .GroupBy(b => b.BecarioId)
                .Select(g => g.First())
                .ToArray();

            foreach (var b in becariosPrograma)
            {
                becariosUnicos.Add(b.BecarioId);
            }

            var porUniversidad = becariosPrograma
                .GroupBy(b => string.IsNullOrWhiteSpace(b.Universidad) ? "Sin especificar" : b.Universidad.Trim())
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .Select(g => new DistribucionAgregadaItem(g.Key, g.Count()))
                .ToArray();

            var porCarrera = becariosPrograma
                .GroupBy(b => string.IsNullOrWhiteSpace(b.Carrera) ? "Sin especificar" : b.Carrera.Trim())
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .Select(g => new DistribucionAgregadaItem(g.Key, g.Count()))
                .ToArray();

            var perfilGeneral = new PerfilGeneralAgregado(
                becariosPrograma.Length,
                porUniversidad,
                porCarrera);

            // HU-33.1 E10: únicamente desembolsos CONFIRMADO; excluye FALLIDO, DEVUELTO y PROGRAMADO.
            var desembolsadoConfirmado = desembolsosEnPeriodo
                .Where(d => d.ProgramaId == programaId
                            && string.Equals(d.CodigoPeriodo, codigoPeriodo, StringComparison.OrdinalIgnoreCase)
                            && d.Estado == EstadoDesembolsoProyectado.CONFIRMADO)
                .Sum(d => d.Monto);

            var ejecucion = new EjecucionPresupuestariaImpacto(
                aporteDonante,
                desembolsadoConfirmado,
                monedaPrograma);

            // HU-33.1 E11: agrega reportes académicos sin distinguir API vs MANUAL.
            var academicosPrograma = reportesAcademicosEnPeriodo
                .Where(r => r.ProgramaId == programaId
                            && string.Equals(r.CodigoPeriodo, codigoPeriodo, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            ResultadosAcademicosAgregados resultadosAcademicos;
            if (academicosPrograma.Length == 0)
            {
                resultadosAcademicos = new ResultadosAcademicosAgregados(
                    CantidadReportes: 0,
                    PromedioGeneral: null,
                    TotalMateriasAprobadas: 0,
                    TotalMateriasReprobadas: 0,
                    MensajeSinDatos: SolicitudReporteImpacto.MensajeSinDatosAcademicos);
            }
            else
            {
                var promedio = Math.Round(academicosPrograma.Average(r => r.Promedio), 2, MidpointRounding.AwayFromZero);
                resultadosAcademicos = new ResultadosAcademicosAgregados(
                    CantidadReportes: academicosPrograma.Length,
                    PromedioGeneral: promedio,
                    TotalMateriasAprobadas: academicosPrograma.Sum(r => r.MateriasAprobadas),
                    TotalMateriasReprobadas: academicosPrograma.Sum(r => r.MateriasReprobadas),
                    MensajeSinDatos: null);
            }

            // HU-33.1 E4, E5, E6 + HU-33.2 + C20:
            // Solo becarios con consentimiento vigente para ESTE donante; muestra cédula pero JAMÁS datos socioeconómicos.
            var promediosPorBecario = academicosPrograma
                .GroupBy(r => r.BecarioId)
                .ToDictionary(g => g.Key, g => Math.Round(g.Average(x => x.Promedio), 2, MidpointRounding.AwayFromZero));

            var presentados = becariosPrograma
                .Where(b => becariosConConsentimientoParaDonante.Contains(b.BecarioId))
                .OrderBy(b => b.NombreCompleto, StringComparer.OrdinalIgnoreCase)
                .Select(b => new BecarioPresentadoImpacto(
                    b.NombreCompleto,
                    b.Cedula,
                    b.Universidad,
                    b.Carrera,
                    nombrePrograma,
                    promediosPorBecario.TryGetValue(b.BecarioId, out var prom) ? prom : null))
                .ToArray();

            programasSalida.Add(new ProgramaImpactoReporte(
                programaId,
                nombrePrograma,
                becariosPrograma.Length,
                perfilGeneral,
                ejecucion,
                resultadosAcademicos,
                presentados));
        }

        return new ReporteImpactoContenido(
            donanteId,
            nombreDonante,
            codigoPeriodo,
            generadoEnUtc,
            becariosUnicos.Count,
            programasSalida.Sum(p => p.EjecucionPresupuestaria.AporteDonanteEnPeriodo),
            programasSalida.Sum(p => p.EjecucionPresupuestaria.DesembolsadoConfirmado),
            monedaGlobal,
            programasSalida);
    }
}
