import type { Metadata } from "next";
import { redirect } from "next/navigation";
import { leerSesionServidor } from "@/lib/auth/cookies";
import { extraerUsuarioPublico } from "@/lib/auth/session";
import { llamarBackendDesdeBff } from "@/lib/bff/proxy";
import { cargarListaPeriodosServidor } from "@/lib/bff/periodo";
import { ProgramasVista } from "@/components/programas/ProgramasVista";
import type { ProgramaResumenDto } from "@/lib/contratos/s2";

export const metadata: Metadata = {
  title: "Programas",
};

export default async function ProgramasPage() {
  const sesion = await leerSesionServidor();
  if (!sesion) {
    redirect("/login");
  }

  const [respProgramas, listaPeriodos] = await Promise.all([
    llamarBackendDesdeBff({ ruta: "/api/v1/programas" }),
    cargarListaPeriodosServidor(),
  ]);

  let programas: ProgramaResumenDto[] = [];
  let errorCarga: string | null = null;

  if (respProgramas.ok && respProgramas.bodyText) {
    programas = JSON.parse(respProgramas.bodyText) as ProgramaResumenDto[];
  } else {
    try {
      const p = JSON.parse(respProgramas.bodyText) as { detail?: string };
      errorCarga = p.detail ?? "No se pudo consultar el catálogo de programas.";
    } catch {
      errorCarga = "No se pudo consultar el catálogo de programas.";
    }
  }

  return (
    <ProgramasVista
      usuario={extraerUsuarioPublico(sesion)}
      programasIniciales={programas}
      periodos={listaPeriodos?.periodos ?? []}
      periodoActual={listaPeriodos?.periodoActual ?? null}
      errorCarga={errorCarga}
    />
  );
}
