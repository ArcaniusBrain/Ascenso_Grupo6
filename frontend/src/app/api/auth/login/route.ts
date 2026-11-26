import { NextResponse } from "next/server";
import { llamarBackendDesdeBff, crearProblemJson } from "@/lib/bff/proxy";
import {
  guardarDesafio2FaPendiente,
  guardarSesionServidor,
  limpiarDesafio2FaPendiente,
} from "@/lib/auth/cookies";
import {
  enmascararCorreo,
  extraerUsuarioPublico,
  normalizarCanalCodigo,
  type SesionSellada,
} from "@/lib/auth/session";

interface LoginBody {
  correo?: string;
  contrasena?: string;
}

interface RespuestaS1Paso1 {
  requiereSegundoFactor?: boolean;
  desafioId?: string | null;
  canalEnviado?: string | number | null;
  expiraEnUtc?: string | null;
  sesion?: SesionSellada | null;
  mensaje?: string | null;
}

export async function POST(request: Request) {
  let body: LoginBody;
  try {
    body = (await request.json()) as LoginBody;
  } catch {
    return crearProblemJson(400, "Solicitud inválida", "El cuerpo debe ser JSON válido.", "JSON_INVALIDO");
  }

  const correo = body.correo?.trim() ?? "";
  const contrasena = body.contrasena ?? "";

  if (!correo || !contrasena) {
    return crearProblemJson(
      422,
      "Datos incompletos",
      "Ingresa tu correo electrónico y contraseña.",
      "CREDENCIALES_INCOMPLETAS"
    );
  }

  const resp = await llamarBackendDesdeBff({
    method: "POST",
    ruta: "/api/v1/auth/login",
    body: { correo, contrasena },
    requiereAutenticacion: false,
  });

  if (!resp.ok) {
    return new Response(resp.bodyText, {
      status: resp.status,
      headers: { "Content-Type": resp.contentType },
    });
  }

  const data = JSON.parse(resp.bodyText) as RespuestaS1Paso1;

  if (data.requiereSegundoFactor && data.desafioId) {
    const canal = normalizarCanalCodigo(data.canalEnviado);
    const expiraEnUtc =
      data.expiraEnUtc ?? new Date(Date.now() + 15 * 60 * 1000).toISOString();
    const correoEnmascarado = enmascararCorreo(correo);

    await guardarDesafio2FaPendiente({
      desafioId: data.desafioId,
      correo,
      correoEnmascarado,
      canal,
      expiraEnUtc,
    });

    return NextResponse.json(
      {
        requiereSegundoFactor: true,
        canal,
        correoEnmascarado,
        expiraEnUtc,
        mensaje: data.mensaje ?? null,
      },
      { status: 202 }
    );
  }

  if (data.sesion) {
    await limpiarDesafio2FaPendiente();
    await guardarSesionServidor(data.sesion);
    return NextResponse.json({
      requiereSegundoFactor: false,
      usuario: extraerUsuarioPublico(data.sesion),
    });
  }

  return crearProblemJson(
    502,
    "Respuesta inesperada de identidad",
    "El servicio de identidad no devolvió una sesión ni un desafío de verificación.",
    "RESPUESTA_IDENTIDAD_INVALIDA"
  );
}
