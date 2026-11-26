import { NextResponse } from "next/server";
import { llamarBackendDesdeBff, crearProblemJson } from "@/lib/bff/proxy";
import {
  guardarDesafio2FaPendiente,
  leerDesafio2FaPendiente,
} from "@/lib/auth/cookies";
import { normalizarCanalCodigo } from "@/lib/auth/session";

interface RespuestaReenvio {
  desafioId?: string | null;
  canalEnviado?: string | number | null;
  expiraEnUtc?: string | null;
  mensaje?: string | null;
}

export async function POST() {
  const pendiente = await leerDesafio2FaPendiente();
  if (!pendiente) {
    return crearProblemJson(
      401,
      "Verificación vencida",
      "No hay una verificación activa. Vuelve a iniciar sesión.",
      "DESAFIO_2FA_NO_DISPONIBLE"
    );
  }

  // Usar recuperación por correo si no hay usuarioId explícito, o reenvío directo si está disponible.
  const resp = pendiente.usuarioId
    ? await llamarBackendDesdeBff({
        method: "POST",
        ruta: "/api/v1/auth/2fa/reenviar",
        body: { usuarioId: pendiente.usuarioId },
        requiereAutenticacion: false,
      })
    : await llamarBackendDesdeBff({
        method: "POST",
        ruta: "/api/v1/auth/2fa/recuperar",
        body: { correo: pendiente.correo },
        requiereAutenticacion: false,
      });

  if (!resp.ok) {
    return new Response(resp.bodyText, {
      status: resp.status,
      headers: { "Content-Type": resp.contentType },
    });
  }

  const data = JSON.parse(resp.bodyText) as RespuestaReenvio;
  const nuevoDesafioId = data.desafioId ?? pendiente.desafioId;
  const canal = normalizarCanalCodigo(data.canalEnviado ?? pendiente.canal);
  const expiraEnUtc =
    data.expiraEnUtc ?? new Date(Date.now() + 15 * 60 * 1000).toISOString();

  await guardarDesafio2FaPendiente({
    ...pendiente,
    desafioId: nuevoDesafioId,
    canal,
    expiraEnUtc,
  });

  return NextResponse.json(
    {
      reenviado: true,
      canal,
      correoEnmascarado: pendiente.correoEnmascarado,
      expiraEnUtc,
      mensaje: data.mensaje ?? "Enviamos un nuevo código de verificación.",
    },
    { status: 202 }
  );
}
