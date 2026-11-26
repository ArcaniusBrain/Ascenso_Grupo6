import { cookies } from "next/headers";
import { obtenerConfiguracionBff } from "@/lib/config/env";
import {
  abrirPayload,
  sellarPayload,
  sesionEstaVigente,
  type Desafio2FaPendiente,
  type SesionSellada,
} from "@/lib/auth/session";

export const COOKIE_SESION = "ascenso_session";
export const COOKIE_2FA_PENDIENTE = "ascenso_2fa_pending";

function calcularMaxAgeSegundos(expiraEnUtc: string, fallbackSegundos: number): number {
  const diff = Math.floor((Date.parse(expiraEnUtc) - Date.now()) / 1000);
  if (!Number.isFinite(diff) || diff <= 0) {
    return fallbackSegundos;
  }
  return diff;
}

export async function leerSesionServidor(): Promise<SesionSellada | null> {
  const config = obtenerConfiguracionBff();
  const store = await cookies();
  const valor = store.get(COOKIE_SESION)?.value;
  if (!valor) {
    return null;
  }

  const sesion = abrirPayload<SesionSellada>(valor, config.sessionSecret);
  if (!sesion || !sesionEstaVigente(sesion)) {
    return null;
  }

  return sesion;
}

export async function guardarSesionServidor(sesion: SesionSellada): Promise<void> {
  const config = obtenerConfiguracionBff();
  const store = await cookies();
  const sellada = sellarPayload(sesion, config.sessionSecret);

  store.set(COOKIE_SESION, sellada, {
    httpOnly: true,
    secure: config.esProduccion,
    sameSite: "lax",
    path: "/",
    maxAge: calcularMaxAgeSegundos(sesion.expiraEnUtc, 3600),
  });
}

export async function limpiarSesionServidor(): Promise<void> {
  const store = await cookies();
  store.delete(COOKIE_SESION);
  store.delete(COOKIE_2FA_PENDIENTE);
}

export async function guardarDesafio2FaPendiente(desafio: Desafio2FaPendiente): Promise<void> {
  const config = obtenerConfiguracionBff();
  const store = await cookies();
  const sellado = sellarPayload(desafio, config.sessionSecret);

  store.set(COOKIE_2FA_PENDIENTE, sellado, {
    httpOnly: true,
    secure: config.esProduccion,
    sameSite: "lax",
    path: "/",
    maxAge: calcularMaxAgeSegundos(desafio.expiraEnUtc, 900),
  });
}

export async function leerDesafio2FaPendiente(): Promise<Desafio2FaPendiente | null> {
  const config = obtenerConfiguracionBff();
  const store = await cookies();
  const valor = store.get(COOKIE_2FA_PENDIENTE)?.value;
  if (!valor) {
    return null;
  }

  const desafio = abrirPayload<Desafio2FaPendiente>(valor, config.sessionSecret);
  if (!desafio || !sesionEstaVigente(desafio)) {
    return null;
  }

  return desafio;
}

export async function limpiarDesafio2FaPendiente(): Promise<void> {
  const store = await cookies();
  store.delete(COOKIE_2FA_PENDIENTE);
}
