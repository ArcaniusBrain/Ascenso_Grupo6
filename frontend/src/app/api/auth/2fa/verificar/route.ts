import { NextResponse } from "next/server";
import { llamarBackendDesdeBff, crearProblemJson } from "@/lib/bff/proxy";
import {
  guardarSesionServidor,
  leerDesafio2FaPendiente,
  limpiarDesafio2FaPendiente,
} from "@/lib/auth/cookies";
import { extraerUsuarioPublico, type SesionSellada } from "@/lib/auth/session";

interface VerificarBody {
  codigo?: string;
}

export async function POST(request: Request) {
  const pendiente = await leerDesafio2FaPendiente();
  if (!pendiente) {
    return crearProblemJson(
      401,
      "Verificación vencida",
      "No hay una verificación pendiente o el código expiró. Vuelve a iniciar sesión.",
      "DESAFIO_2FA_NO_DISPONIBLE"
    );
  }

  let body: VerificarBody;
  try {
    body = (await request.json()) as VerificarBody;
  } catch {
    return crearProblemJson(400, "Solicitud inválida", "El cuerpo debe ser JSON válido.", "JSON_INVALIDO");
  }

  const codigo = (body.codigo ?? "").replace(/\D/g, "");
  if (codigo.length !== 6) {
    return crearProblemJson(
      422,
      "Código incompleto",
      "Ingresa los 6 dígitos del código de verificación.",
      "CODIGO_INCOMPLETO"
    );
  }

  const resp = await llamarBackendDesdeBff({
    method: "POST",
    ruta: "/api/v1/auth/2fa/verificar",
    body: {
      desafioId: pendiente.desafioId,
      codigo,
    },
    requiereAutenticacion: false,
  });

  if (!resp.ok) {
    return new Response(resp.bodyText, {
      status: resp.status,
      headers: { "Content-Type": resp.contentType },
    });
  }

  const sesion = JSON.parse(resp.bodyText) as SesionSellada;
  await limpiarDesafio2FaPendiente();
  await guardarSesionServidor(sesion);

  return NextResponse.json({
    usuario: extraerUsuarioPublico(sesion),
  });
}
