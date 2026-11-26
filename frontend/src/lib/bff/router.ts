import type { CodigoServicioBackend, ConfiguracionBff } from "@/lib/config/env";

export type ResultadoResolucionDestino =
  | { ok: true; urlDestino: string; servicio: CodigoServicioBackend | "GATEWAY" }
  | { ok: false; status: number; codigo: string; detalle: string };

const PREFIJOS_SERVICIOS: Array<{ prefijo: string; servicio: CodigoServicioBackend }> = [
  // S1 · Identidad y Acceso
  { prefijo: "/api/v1/auth", servicio: "S1" },
  { prefijo: "/api/v1/usuarios", servicio: "S1" },
  { prefijo: "/api/v1/tipos-usuario", servicio: "S1" },
  { prefijo: "/api/v1/otp", servicio: "S1" },

  // S2 · Catálogo, Fondos y Calendario
  { prefijo: "/api/v1/periodos", servicio: "S2" },
  { prefijo: "/api/v1/universidades", servicio: "S2" },
  { prefijo: "/api/v1/programas", servicio: "S2" },
  { prefijo: "/api/v1/donantes", servicio: "S2" },

  // S3 · Postulación y Evaluación
  { prefijo: "/api/v1/convocatorias", servicio: "S3" },
  { prefijo: "/api/v1/postulantes", servicio: "S3" },
  { prefijo: "/api/v1/postulaciones", servicio: "S3" },
  { prefijo: "/api/v1/evaluaciones", servicio: "S3" },
  { prefijo: "/api/v1/rubricas", servicio: "S3" },

  // S4 · Becas, Ofertas y Desembolsos
  { prefijo: "/api/v1/ofertas", servicio: "S4" },
  { prefijo: "/api/v1/becas", servicio: "S4" },
  { prefijo: "/api/v1/desembolsos", servicio: "S4" },
  { prefijo: "/api/v1/pagos", servicio: "S4" },

  // S5 · Seguimiento Académico
  { prefijo: "/api/v1/seguimiento", servicio: "S5" },
  { prefijo: "/api/v1/reportes-academicos", servicio: "S5" },
  { prefijo: "/api/v1/renovaciones", servicio: "S5" },
  { prefijo: "/api/v1/pausas", servicio: "S5" },

  // S6 · Comunicaciones y Reportería
  { prefijo: "/api/v1/consentimientos-donantes", servicio: "S6" },
  { prefijo: "/api/v1/reportes", servicio: "S6" },
  { prefijo: "/api/v1/notificaciones", servicio: "S6" },
  { prefijo: "/api/v1/consultas", servicio: "S6" },
  { prefijo: "/api/v1/indicadores", servicio: "S6" },
];

/**
 * Valida que la ruta solicitada por el navegador sea estrictamente pública (/api/v1/*)
 * y nunca acceda a fronteras privadas (/internal/*) ni realice traversal.
 */
export function resolverDestinoBackend(
  rutaSolicitada: string,
  queryString: string,
  config: ConfiguracionBff
): ResultadoResolucionDestino {
  const rutaNormalizada = rutaSolicitada.startsWith("/") ? rutaSolicitada : `/${rutaSolicitada}`;
  const rutaDecodificada = decodeURIComponent(rutaNormalizada);

  if (
    rutaDecodificada.includes("..") ||
    rutaDecodificada.includes("\\") ||
    /(^|\/)internal(\/|$)/i.test(rutaDecodificada)
  ) {
    return {
      ok: false,
      status: 403,
      codigo: "FRONTERA_PRIVADA_BLOQUEADA",
      detalle: "El frontend no puede invocar rutas privadas entre servicios.",
    };
  }

  if (!rutaNormalizada.startsWith("/api/v1/")) {
    return {
      ok: false,
      status: 403,
      codigo: "RUTA_BACKEND_NO_PERMITIDA",
      detalle: "Solo se permiten llamadas a contratos públicos bajo /api/v1/.",
    };
  }

  const sufijoQuery = queryString ? (queryString.startsWith("?") ? queryString : `?${queryString}`) : "";

  if (config.gatewayUrl) {
    return {
      ok: true,
      servicio: "GATEWAY",
      urlDestino: `${config.gatewayUrl}${rutaNormalizada}${sufijoQuery}`,
    };
  }

  const coincidencia = PREFIJOS_SERVICIOS.find(
    (entry) =>
      rutaNormalizada === entry.prefijo || rutaNormalizada.startsWith(`${entry.prefijo}/`)
  );

  if (!coincidencia) {
    return {
      ok: false,
      status: 502,
      codigo: "SERVICIO_NO_ENCONTRADO",
      detalle: `No existe enrutamiento configurado para la ruta ${rutaNormalizada}.`,
    };
  }

  const baseServicio = config.servicios[coincidencia.servicio];
  if (!baseServicio) {
    return {
      ok: false,
      status: 503,
      codigo: "SERVICIO_NO_CONFIGURADO",
      detalle: `Configura ASCENSO_GATEWAY_URL o ASCENSO_${coincidencia.servicio}_URL en el entorno del frontend.`,
    };
  }

  return {
    ok: true,
    servicio: coincidencia.servicio,
    urlDestino: `${baseServicio}${rutaNormalizada}${sufijoQuery}`,
  };
}
