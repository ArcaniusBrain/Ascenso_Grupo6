/**
 * Contratos TypeScript alineados con Ascenso.S6.ComunicacionesReporteria (RF-33).
 */

export interface ConsentimientoDonanteDto {
  id: string;
  becarioId: string;
  donanteId: string;
  aceptadoEnUtc: string;
  irrevocable: boolean;
}

export interface ConsentimientoRegistroResultadoDto {
  registrado: boolean;
  yaExistia: boolean;
  consentimiento: ConsentimientoDonanteDto | null;
  mensaje: string | null;
}

export interface DistribucionAgregadaItem {
  etiqueta: string;
  cantidad: number;
}

export interface PerfilGeneralAgregado {
  totalBecarios: number;
  porUniversidad: DistribucionAgregadaItem[];
  porCarrera: DistribucionAgregadaItem[];
}

export interface EjecucionPresupuestariaImpacto {
  aporteDonanteEnPeriodo: number;
  desembolsadoConfirmado: number;
  moneda: string;
}

export interface ResultadosAcademicosAgregados {
  cantidadReportes: number;
  promedioGeneral: number | null;
  totalMateriasAprobadas: number;
  totalMateriasReprobadas: number;
  mensajeSinDatos: string | null;
}

export interface BecarioPresentadoImpacto {
  nombreCompleto: string;
  cedula: string;
  universidad: string;
  carrera: string;
  programa: string;
  promedioAcademico: number | null;
}

export interface ProgramaImpactoReporte {
  programaId: string;
  nombrePrograma: string;
  numeroBecarios: number;
  perfilGeneral: PerfilGeneralAgregado;
  ejecucionPresupuestaria: EjecucionPresupuestariaImpacto;
  resultadosAcademicos: ResultadosAcademicosAgregados;
  becariosPresentados: BecarioPresentadoImpacto[];
}

export interface ReporteImpactoContenido {
  donanteId: string;
  nombreDonante: string;
  codigoPeriodo: string;
  generadoEnUtc: string;
  totalBecariosUnicos: number;
  totalAportadoDonante: number;
  totalDesembolsadoConfirmado: number;
  moneda: string;
  programas: ProgramaImpactoReporte[];
}

export interface SolicitudReporteImpactoDto {
  reporteId: string;
  donanteId: string;
  nombreDonante: string;
  codigoPeriodo: string;
  estado: "PREPARANDO" | "LISTO" | "FALLIDO" | string;
  mensajeEstado: string | null;
  solicitadoEnUtc: string;
  completadoEnUtc: string | null;
  contenido: ReporteImpactoContenido | null;
}
