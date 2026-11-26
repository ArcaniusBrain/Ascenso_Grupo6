/**
 * Configuración de entorno exclusivamente del lado del servidor (Next.js BFF - ADR-28).
 * Nunca importa ni hardcodea direcciones locales; toda URL proviene de variables de entorno.
 */

export type CodigoServicioBackend = "S1" | "S2" | "S3" | "S4" | "S5" | "S6";

export interface ConfiguracionBff {
  sessionSecret: string;
  gatewayUrl: string | null;
  servicios: Record<CodigoServicioBackend, string | null>;
  esProduccion: boolean;
}

function limpiarUrl(valor: string | undefined): string | null {
  const limpio = valor?.trim();
  if (!limpio) {
    return null;
  }
  return limpio.replace(/\/+$/, "");
}

export function obtenerConfiguracionBff(
  env: NodeJS.ProcessEnv = process.env
): ConfiguracionBff {
  const secret = env.ASCENSO_SESSION_SECRET?.trim() ?? "";
  const esProduccion = env.NODE_ENV === "production";

  if (secret.length < 32) {
    if (esProduccion) {
      throw new Error(
        "ASCENSO_SESSION_SECRET es obligatorio en producción y debe tener al menos 32 caracteres."
      );
    }
  }

  return {
    sessionSecret:
      secret.length >= 32
        ? secret
        : "desarrollo-local-ascenso-cambiar-en-env-local-32bytes",
    gatewayUrl: limpiarUrl(env.ASCENSO_GATEWAY_URL),
    servicios: {
      S1: limpiarUrl(env.ASCENSO_S1_URL),
      S2: limpiarUrl(env.ASCENSO_S2_URL),
      S3: limpiarUrl(env.ASCENSO_S3_URL),
      S4: limpiarUrl(env.ASCENSO_S4_URL),
      S5: limpiarUrl(env.ASCENSO_S5_URL),
      S6: limpiarUrl(env.ASCENSO_S6_URL),
    },
    esProduccion,
  };
}
