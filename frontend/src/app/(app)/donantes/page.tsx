import type { Metadata } from "next";
import { redirect } from "next/navigation";
import { leerSesionServidor } from "@/lib/auth/cookies";
import { extraerUsuarioPublico } from "@/lib/auth/session";
import { llamarBackendDesdeBff } from "@/lib/bff/proxy";
import { cargarListaPeriodosServidor } from "@/lib/bff/periodo";
import { DonantesVista } from "@/components/donantes/DonantesVista";
import type { DonanteResumenDto, ProgramaResumenDto } from "@/lib/contratos/s2";

export const metadata: Metadata = {
  title: "Donantes",
};

export default async function DonantesPage() {
  const sesion = await leerSesionServidor();
  if (!sesion) {
    redirect("/login");
  }

  const [respDonantes, respProgramas, listaPeriodos] = await Promise.all([
    llamarBackendDesdeBff({ ruta: "/api/v1/donantes" }),
    llamarBackendDesdeBff({ ruta: "/api/v1/programas" }),
    cargarListaPeriodosServidor(),
  ]);

  let donantes: DonanteResumenDto[] = [];
  let errorCarga: string | null = null;
  if (respDonantes.ok && respDonantes.bodyText) {
    donantes = JSON.parse(respDonantes.bodyText) as DonanteResumenDto[];
  } else {
    errorCarga = "No se pudo consultar el directorio de donantes.";
  }

  const programas =
    respProgramas.ok && respProgramas.bodyText
      ? (JSON.parse(respProgramas.bodyText) as ProgramaResumenDto[])
      : [];

  return (
    <DonantesVista
      usuario={extraerUsuarioPublico(sesion)}
      donantesIniciales={donantes}
      programas={programas}
      periodos={listaPeriodos?.periodos ?? []}
      periodoActual={listaPeriodos?.periodoActual ?? null}
      errorCarga={errorCarga}
    />
  );
}
