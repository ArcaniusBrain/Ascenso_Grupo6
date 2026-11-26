import { obtenerConfiguracionBff } from "@/lib/config/env";
import { leerSesionServidor } from "@/lib/auth/cookies";
import { resolverDestinoBackend } from "@/lib/bff/router";
import type { SesionSellada } from "@/lib/auth/session";

export interface OpcionesLlamadaBackend {
  method?: string;
  ruta: string;
  queryString?: string;
  body?: unknown;
  requiereAutenticacion?: boolean;
  sesionExplicita?: SesionSellada | null;
}

export interface RespuestaBackendBff {
  status: number;
  ok: boolean;
  contentType: string;
  bodyText: string;
}

export function crearProblemJson(
  status: number,
  title: string,
  detail: string,
  code: string
): Response {
  return new Response(
    JSON.stringify({
      status,
      title,
      detail,
      code,
    }),
    {
      status,
      headers: {
        "Content-Type": "application/problem+json; charset=utf-8",
      },
    }
  );
}

/**
 * Ejecuta una llamada desde el servidor de Next.js hacia YARP o el microservicio configurado,
 * inyectando la identidad de sesión desde la cookie HttpOnly (ADR-28).
 */
export async function llamarBackendDesdeBff(
  opciones: OpcionesLlamadaBackend
): Promise<RespuestaBackendBff> {
  const config = obtenerConfiguracionBff();
  const destino = resolverDestinoBackend(
    opciones.ruta,
    opciones.queryString ?? "",
    config
  );

  if (!destino.ok) {
    return {
      status: destino.status,
      ok: false,
      contentType: "application/problem+json; charset=utf-8",
      bodyText: JSON.stringify({
        status: destino.status,
        title: "Solicitud bloqueada por el BFF",
        detail: destino.detalle,
        code: destino.codigo,
      }),
    };
  }

  const requiereAuth = opciones.requiereAutenticacion ?? true;
  const sesion =
    opciones.sesionExplicita !== undefined
      ? opciones.sesionExplicita
      : requiereAuth
        ? await leerSesionServidor()
        : null;

  if (requiereAuth && !sesion) {
    return {
      status: 401,
      ok: false,
      contentType: "application/problem+json; charset=utf-8",
      bodyText: JSON.stringify({
        status: 401,
        title: "No autenticado",
        detail: "Tu sesión expiró o no has iniciado sesión.",
        code: "NO_AUTENTICADO",
      }),
    };
  }

  const headers = new Headers();
  headers.set("Accept", "application/json, application/problem+json");

  if (sesion) {
    headers.set("Authorization", `Bearer ${sesion.accessToken}`);
    headers.set("X-Actor-Usuario-Id", sesion.usuarioId);
    if (sesion.roles.length > 0) {
      headers.set("X-Actor-Rol", sesion.roles[0]);
    }
    if (sesion.programasAsignados && sesion.programasAsignados.length > 0) {
      headers.set("X-Actor-Programas", sesion.programasAsignados.join(","));
    }
  }

  const method = (opciones.method ?? "GET").toUpperCase();
  let payload: string | undefined;
  if (opciones.body !== undefined && method !== "GET" && method !== "HEAD") {
    headers.set("Content-Type", "application/json; charset=utf-8");
    payload = typeof opciones.body === "string" ? opciones.body : JSON.stringify(opciones.body);
  }

  try {
    const response = await fetch(destino.urlDestino, {
      method,
      headers,
      body: payload,
      cache: "no-store",
      signal: AbortSignal.timeout(10000),
    });

    const bodyText = await response.text();
    const contentType =
      response.headers.get("content-type") ?? "application/json; charset=utf-8";

    return {
      status: response.status,
      ok: response.ok,
      contentType,
      bodyText,
    };
  } catch {
    return {
      status: 503,
      ok: false,
      contentType: "application/problem+json; charset=utf-8",
      bodyText: JSON.stringify({
        status: 503,
        title: "Servicio temporalmente no disponible",
        detail: "No se pudo conectar con el servicio de ASCENSO. Intenta nuevamente en unos momentos.",
        code: "BACKEND_NO_DISPONIBLE",
      }),
    };
  }
}
