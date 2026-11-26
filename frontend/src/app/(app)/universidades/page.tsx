import type { Metadata } from "next";
import { redirect } from "next/navigation";
import { leerSesionServidor } from "@/lib/auth/cookies";
import { extraerUsuarioPublico } from "@/lib/auth/session";
import { llamarBackendDesdeBff } from "@/lib/bff/proxy";
import { cargarListaPeriodosServidor } from "@/lib/bff/periodo";
import { UniversidadesVista } from "@/components/universidades/UniversidadesVista";
import type { UniversidadResumenDto } from "@/lib/contratos/s2";

export const metadata: Metadata = {
  title: "Universidades aliadas",
};

export default async function UniversidadesPage() {
  const sesion = await leerSesionServidor();
  if (!sesion) {
    redirect("/login");
  }

  const [respUnis, listaPeriodos] = await Promise.all([
    llamarBackendDesdeBff({ ruta: "/api/v1/universidades" }),
    cargarListaPeriodosServidor(),
  ]);

  let universidades: UniversidadResumenDto[] = [];
  let errorCarga: string | null = null;
  if (respUnis.ok && respUnis.bodyText) {
    universidades = JSON.parse(respUnis.bodyText) as UniversidadResumenDto[];
  } else {
    errorCarga = "No se pudo consultar el catálogo de universidades aliadas.";
  }

  return (
    <UniversidadesVista
      usuario={extraerUsuarioPublico(sesion)}
      universidadesIniciales={universidades}
      periodos={listaPeriodos?.periodos ?? []}
      periodoActual={listaPeriodos?.periodoActual ?? null}
      errorCarga={errorCarga}
    />
  );
}
