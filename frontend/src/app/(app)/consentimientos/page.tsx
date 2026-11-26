import type { Metadata } from "next";
import { redirect } from "next/navigation";
import { leerSesionServidor } from "@/lib/auth/cookies";
import { extraerUsuarioPublico } from "@/lib/auth/session";
import { llamarBackendDesdeBff } from "@/lib/bff/proxy";
import { cargarListaPeriodosServidor } from "@/lib/bff/periodo";
import { ConsentimientosVista } from "@/components/consentimientos/ConsentimientosVista";
import type { ConsentimientoDonanteDto } from "@/lib/contratos/s6";

export const metadata: Metadata = {
  title: "Consentimiento a donantes",
};

export default async function ConsentimientosPage() {
  const sesion = await leerSesionServidor();
  if (!sesion) {
    redirect("/login");
  }

  const [respLista, listaPeriodos] = await Promise.all([
    llamarBackendDesdeBff({ ruta: "/api/v1/consentimientos-donantes/mios" }),
    cargarListaPeriodosServidor(),
  ]);

  let consentimientos: ConsentimientoDonanteDto[] = [];
  let errorCarga: string | null = null;
  if (respLista.ok && respLista.bodyText) {
    consentimientos = JSON.parse(respLista.bodyText) as ConsentimientoDonanteDto[];
  } else {
    errorCarga = "No se pudieron consultar tus consentimientos en S6.";
  }

  return (
    <ConsentimientosVista
      usuario={extraerUsuarioPublico(sesion)}
      consentimientosIniciales={consentimientos}
      periodoActual={listaPeriodos?.periodoActual ?? null}
      errorCarga={errorCarga}
    />
  );
}
