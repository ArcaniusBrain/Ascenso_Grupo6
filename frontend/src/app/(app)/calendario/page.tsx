import type { Metadata } from "next";
import { redirect } from "next/navigation";
import { leerSesionServidor } from "@/lib/auth/cookies";
import { extraerUsuarioPublico } from "@/lib/auth/session";
import { cargarListaPeriodosServidor } from "@/lib/bff/periodo";
import { CalendarioVista } from "@/components/calendario/CalendarioVista";

export const metadata: Metadata = {
  title: "Calendario académico",
};

export default async function CalendarioPage() {
  const sesion = await leerSesionServidor();
  if (!sesion) {
    redirect("/login");
  }

  const calendario = await cargarListaPeriodosServidor();

  return (
    <CalendarioVista
      usuario={extraerUsuarioPublico(sesion)}
      calendarioInicial={calendario}
      errorCarga={
        calendario === null ? "No se pudo consultar el calendario académico desde S2." : null
      }
    />
  );
}
