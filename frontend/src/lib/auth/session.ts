import { createCipheriv, createDecipheriv, createHash, randomBytes } from "node:crypto";

/**
 * Datos públicos de la sesión que pueden exponerse al cliente (Server/Client Components).
 * Nunca incluye accessToken ni refreshToken (ADR-28).
 */
export interface UsuarioSesionPublico {
  usuarioId: string;
  correo: string;
  nombreCompleto: string;
  roles: string[];
  permisos: string[];
  segundoFactorVerificado: boolean;
  programasAsignados?: string[];
  expiraEnUtc: string;
}

/**
 * Carga útil sellada dentro de la cookie HttpOnly `ascenso_session`.
 */
export interface SesionSellada extends UsuarioSesionPublico {
  accessToken: string;
  selloSeguridad: string;
}

/**
 * Estado temporal sellado en cookie HttpOnly `ascenso_2fa_pending` entre Paso 1 (Login)
 * y Paso 2 (Verificación en dos pasos, RF-37).
 */
export interface Desafio2FaPendiente {
  desafioId: string;
  correo: string;
  correoEnmascarado: string;
  canal: "CORREO" | "SMS";
  expiraEnUtc: string;
  usuarioId?: string;
}

const ALGORITMO = "aes-256-gcm";
const IV_BYTES = 12;
const AUTH_TAG_BYTES = 16;

function derivarClave(secreto: string): Buffer {
  return createHash("sha256").update(secreto, "utf8").digest();
}

export function sellarPayload<T>(payload: T, secreto: string): string {
  const clave = derivarClave(secreto);
  const iv = randomBytes(IV_BYTES);
  const cipher = createCipheriv(ALGORITMO, clave, iv);
  const json = Buffer.from(JSON.stringify(payload), "utf8");
  const cifrado = Buffer.concat([cipher.update(json), cipher.final()]);
  const tag = cipher.getAuthTag();

  return Buffer.concat([iv, tag, cifrado]).toString("base64url");
}

export function abrirPayload<T>(tokenSellado: string, secreto: string): T | null {
  if (!tokenSellado || tokenSellado.length < 32) {
    return null;
  }

  try {
    const raw = Buffer.from(tokenSellado, "base64url");
    if (raw.length <= IV_BYTES + AUTH_TAG_BYTES) {
      return null;
    }

    const iv = raw.subarray(0, IV_BYTES);
    const tag = raw.subarray(IV_BYTES, IV_BYTES + AUTH_TAG_BYTES);
    const cifrado = raw.subarray(IV_BYTES + AUTH_TAG_BYTES);

    const clave = derivarClave(secreto);
    const decipher = createDecipheriv(ALGORITMO, clave, iv);
    decipher.setAuthTag(tag);

    const plano = Buffer.concat([decipher.update(cifrado), decipher.final()]).toString("utf8");
    return JSON.parse(plano) as T;
  } catch {
    return null;
  }
}

export function sesionEstaVigente(
  sesion: { expiraEnUtc?: string } | null,
  ahoraMs: number = Date.now()
): boolean {
  if (!sesion?.expiraEnUtc) {
    return false;
  }
  const expiraMs = Date.parse(sesion.expiraEnUtc);
  return Number.isFinite(expiraMs) && expiraMs > ahoraMs;
}

export function extraerUsuarioPublico(sesion: SesionSellada): UsuarioSesionPublico {
  return {
    usuarioId: sesion.usuarioId,
    correo: sesion.correo,
    nombreCompleto: sesion.nombreCompleto,
    roles: sesion.roles,
    permisos: sesion.permisos,
    segundoFactorVerificado: sesion.segundoFactorVerificado,
    programasAsignados: sesion.programasAsignados,
    expiraEnUtc: sesion.expiraEnUtc,
  };
}

export function enmascararCorreo(correo: string): string {
  const limpio = correo.trim();
  const indice = limpio.indexOf("@");
  if (indice <= 1) {
    return limpio;
  }
  const local = limpio.slice(0, indice);
  const dominio = limpio.slice(indice);
  return `${local[0]}•••••${dominio}`;
}

export function normalizarCanalCodigo(canal: unknown): "CORREO" | "SMS" {
  if (canal === 2 || String(canal).toUpperCase() === "SMS") {
    return "SMS";
  }
  return "CORREO";
}
