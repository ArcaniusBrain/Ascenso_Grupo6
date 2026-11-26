import { redirect } from "next/navigation";
import type { ReactNode } from "react";
import { leerSesionServidor } from "@/lib/auth/cookies";

/**
 * Layout protegido del lado del servidor para todas las rutas autenticadas bajo /(app).
 * Valida la firma criptográfica AES-256-GCM y la expiración de la cookie HttpOnly (ADR-28).
 */
export default async function AppAutenticadaLayout({ children }: { children: ReactNode }) {
  const sesion = await leerSesionServidor();
  if (!sesion) {
    redirect("/login");
  }

  return <>{children}</>;
}
