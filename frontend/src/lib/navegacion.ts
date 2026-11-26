export interface ItemNavegacion {
  id: string;
  etiqueta: string;
  href: string;
  rolesPermitidos: string[];
  descripcion: string;
  rf: string;
}

/**
 * Mapa único de secciones de la plataforma (alineado con la barra de navegación del Figma).
 * Cada integrante del equipo agrega su página en src/app/(app)/<ruta>/page.tsx.
 */
export const SECCIONES_PLATAFORMA: ItemNavegacion[] = [
  {
    id: "inicio",
    etiqueta: "Inicio",
    href: "/inicio",
    rolesPermitidos: ["*"],
    descripcion: "Resumen de tu espacio de trabajo y accesos rápidos según tu rol.",
    rf: "Base",
  },
  {
    id: "programas",
    etiqueta: "Programas",
    href: "/programas",
    rolesPermitidos: ["DIRECCION", "COORDINADOR", "FINANZAS", "ADMINISTRADOR"],
    descripcion: "Configuración de programas de becas, elegibilidad, presupuesto y cupos.",
    rf: "RF-11 / RF-12",
  },
  {
    id: "donantes",
    etiqueta: "Donantes",
    href: "/donantes",
    rolesPermitidos: ["DIRECCION", "COORDINADOR", "FINANZAS", "ADMINISTRADOR"],
    descripcion: "Registro de donantes, aportes y reportes de impacto anonimizados.",
    rf: "RF-13 / RF-33",
  },
  {
    id: "universidades",
    etiqueta: "Universidades",
    href: "/universidades",
    rolesPermitidos: ["DIRECCION", "COORDINADOR", "ADMINISTRADOR"],
    descripcion: "Universidades aliadas, convenios, carreras elegibles y costos de matrícula.",
    rf: "RF-14",
  },
  {
    id: "calendario",
    etiqueta: "Calendario",
    href: "/calendario",
    rolesPermitidos: ["DIRECCION", "COORDINADOR", "ADMINISTRADOR"],
    descripcion: "Periodos académicos, periodo actual e inicios de inscripciones.",
    rf: "RF-38",
  },
  {
    id: "usuarios",
    etiqueta: "Usuarios y roles",
    href: "/usuarios",
    rolesPermitidos: ["DIRECCION", "ADMINISTRADOR"],
    descripcion: "Tipos de usuario, permisos, cuentas internas y seguridad 2FA.",
    rf: "RF-37",
  },
  {
    id: "convocatorias",
    etiqueta: "Convocatorias",
    href: "/convocatorias",
    rolesPermitidos: ["POSTULANTE", "COORDINADOR", "DIRECCION", "COMITE"],
    descripcion: "Ciclos y convocatorias de becas disponibles.",
    rf: "RF-01 / RF-15",
  },
  {
    id: "becas",
    etiqueta: "Becas",
    href: "/becas",
    rolesPermitidos: ["BECARIO", "COORDINADOR", "DIRECCION", "FINANZAS"],
    descripcion: "Gestión de becas activas, desembolsos y consentimiento a donantes.",
    rf: "RF-17 / RF-33",
  },
];

export function filtrarNavegacionPorRoles(rolesUsuario: readonly string[]): ItemNavegacion[] {
  const normalizados = new Set(rolesUsuario.map((r) => r.trim().toUpperCase()));

  return SECCIONES_PLATAFORMA.filter((item) => {
    if (item.rolesPermitidos.includes("*")) {
      return true;
    }
    return item.rolesPermitidos.some((rol) => normalizados.has(rol.toUpperCase()));
  });
}

export function obtenerIniciales(nombreCompleto: string): string {
  const partes = nombreCompleto
    .trim()
    .split(/\s+/)
    .filter(Boolean);

  if (partes.length === 0) {
    return "AS";
  }
  if (partes.length === 1) {
    return partes[0].slice(0, 2).toUpperCase();
  }
  return `${partes[0][0]}${partes[partes.length - 1][0]}`.toUpperCase();
}
