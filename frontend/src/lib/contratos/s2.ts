/**
 * Contratos TypeScript alineados 1-a-1 con los DTOs de Ascenso.S2.CatalogoFondos.
 */

export interface PeriodoResumenDto {
  codigo: string;
  estado: "ABIERTO" | "CERRADO" | string;
  esActual: boolean;
  registradoEnUtc: string;
}

export interface ListaPeriodosDto {
  calendarioId: string;
  version: number;
  periodoActual: string | null;
  mensaje: string | null;
  periodos: PeriodoResumenDto[];
}

export interface InicioInscripcionesDto {
  id: string;
  universidadId: string;
  nombreUniversidad: string;
  codigoPeriodo: string;
  fechaInicio: string;
  inscripcionesIniciadas: boolean;
}

export interface PeriodoDetalleDto {
  codigo: string;
  estado: string;
  esActual: boolean;
  esPeriodoAnterior: boolean;
  registradoEnUtc: string;
  cantidadElementosLigados: number;
  mensajeSinElementos: string | null;
  iniciosInscripciones: InicioInscripcionesDto[];
}

export interface DonanteAporteResumenDto {
  donanteId: string;
  nombreDonante: string;
  totalAportado: number;
  moneda: string;
  leyendaSinAportes: string | null;
}

export interface RestriccionElegibilidadDto {
  universidadId: string;
  nombreUniversidad: string;
  carreras: string[];
  avisoSinCarreras: string | null;
}

export interface CondicionesPermanenciaDto {
  promedioMinimo: number;
  exigeHorasServicio: boolean;
  horasServicio: number | null;
  leyendaHorasServicio: string;
  reportesExigidos: string[];
  duracionMaxPausaMeses: number;
  descripcionRenovacion: string;
}

export type ModalidadPago = "MENSUAL" | "SEMESTRAL" | "DIRECTO_UNIVERSIDAD";
export type AlcancePrograma = "COMPLETO" | "PARCIAL";
export type TipoDonante = "ORGANIZACION" | "PERSONA";

export interface ProgramaResumenDto {
  id: string;
  nombre: string;
  periodoInicio: string;
  cupoBecarios: number;
  duracionPeriodos: number;
  montoPorPeriodo: number;
  moneda: string;
  modalidadPago: ModalidadPago | string;
  alcance: AlcancePrograma | string;
  permiteAcumulacion: boolean;
  version: number;
}

export interface ProgramaDetalleDto {
  id: string;
  nombre: string;
  periodoInicio: string;
  cupoBecarios: number;
  duracionPeriodos: number;
  montoPorPeriodo: number;
  moneda: string;
  modalidadPago: ModalidadPago | string;
  alcance: AlcancePrograma | string;
  permiteAcumulacion: boolean;
  presupuestoTotal: number;
  donantes: DonanteAporteResumenDto[];
  leyendaElegibilidad: string | null;
  restricciones: RestriccionElegibilidadDto[];
  condiciones: CondicionesPermanenciaDto | null;
  version: number;
}

export interface PresupuestoProgramaDto {
  programaId: string;
  nombrePrograma: string;
  moneda: string;
  presupuestoTotal: number;
  totalOtorgado: number;
  comprometidoOfertasPendientes: number;
  comprometidoRenovacionesPendientes: number;
  presupuestoDisponible: number;
  desembolsado: number;
  cupoTotal: number;
  cupoOcupado: number;
  cupoDisponible: number;
  mensajeSinAportes: string | null;
  version: number;
}

export interface ContactoDonanteDto {
  correo: string | null;
  telefono: string | null;
}

export interface DonanteResumenDto {
  id: string;
  nombre: string;
  tipo: TipoDonante | string;
  contacto: ContactoDonanteDto;
  cantidadAportes: number;
  totalAportado: number;
  version: number;
}

export interface AporteDto {
  id: string;
  donanteId: string;
  nombreDonante: string;
  programaId: string;
  nombrePrograma: string;
  codigoPeriodo: string;
  monto: number;
  moneda: string;
  registradoEnUtc: string;
}

export interface HistorialAportesDonanteDto {
  donanteId: string;
  nombreDonante: string;
  tipoDonante: string;
  codigoPeriodoFiltro: string | null;
  totalAportado: number;
  mensajeSinAportes: string | null;
  aportes: AporteDto[];
}

export interface ContactoDesignadoDto {
  nombreCompleto: string;
  correo: string | null;
  telefono: string | null;
}

export interface CarreraElegibleDto {
  id: string;
  nombre: string;
}

export interface CostoMatriculaRegistroDto {
  id: string;
  codigoPeriodo: string;
  monto: number;
  moneda: string;
  versionRegistro: number;
  registradoEnUtc: string;
}

export interface UniversidadResumenDto {
  id: string;
  nombre: string;
  terminosConvenio: string;
  viaDatosAcademicos: "API" | "MANUAL" | string;
  contactoDesignado: ContactoDesignadoDto;
  version: number;
}

export interface UniversidadDetalleDto extends UniversidadResumenDto {
  mensajeSinCarreras: string | null;
  carreras: CarreraElegibleDto[];
  costosVigentes: CostoMatriculaRegistroDto[];
}

export interface CostoMatriculaPeriodoDto {
  universidadId: string;
  codigoPeriodo: string;
  costoVigente: CostoMatriculaRegistroDto | null;
  mensaje: string | null;
  historial: CostoMatriculaRegistroDto[];
}

export interface InicioInscripcionesUniversidadDto {
  universidadId: string;
  codigoPeriodo: string;
  fechaInicio: string | null;
  inscripcionesIniciadas: boolean;
  mensaje: string | null;
}
