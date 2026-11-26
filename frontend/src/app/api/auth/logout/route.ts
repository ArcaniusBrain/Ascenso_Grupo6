import { NextResponse } from "next/server";
import { limpiarSesionServidor } from "@/lib/auth/cookies";

export async function POST() {
  await limpiarSesionServidor();
  return NextResponse.json({ cerrada: true });
}
