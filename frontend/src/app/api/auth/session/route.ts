import { NextResponse } from "next/server";
import { leerSesionServidor } from "@/lib/auth/cookies";
import { extraerUsuarioPublico } from "@/lib/auth/session";

export async function GET() {
  const sesion = await leerSesionServidor();
  if (!sesion) {
    return NextResponse.json({ autenticado: false, usuario: null });
  }

  return NextResponse.json({
    autenticado: true,
    usuario: extraerUsuarioPublico(sesion),
  });
}
