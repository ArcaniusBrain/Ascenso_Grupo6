/**
 * Contratos TypeScript alineados con Ascenso.S1.Identidad (RF-37).
 */

export interface TipoUsuarioDto {
  id: string;
  nombre: string;
  permisos: string[];
  accesoFinancieroODecision: boolean;
  esEnlaceUniversidad: boolean;
  exigeSegundoFactor: boolean;
  esPredefinido: boolean;
}

export interface UsuarioDto {
  id: string;
  cedula: string;
  nombreCompleto: string;
  correo: string;
  telefono: string;
  canalPreferido: number | string;
  estado: number | string;
  esEvaluadorExterno: boolean;
  universidadId: string | null;
  tipoUsuarioIds: string[];
  nombresRoles: string[];
  requiereSegundoFactor: boolean;
}

export function etiquetaEstadoUsuario(estado: number | string): string {
  if (estado === 1 || String(estado).toUpperCase() === "ACTIVO") {
    return "Activo";
  }
  if (estado === 2 || String(estado).toUpperCase() === "INACTIVO") {
    return "Inactivo";
  }
  if (estado === 3 || String(estado).toUpperCase() === "BLOQUEADO") {
    return "Bloqueado";
  }
  return String(estado);
}

export function etiquetaCanalPreferido(canal: number | string): string {
  if (canal === 2 || String(canal).toUpperCase() === "SMS") {
    return "SMS";
  }
  return "Correo";
}
