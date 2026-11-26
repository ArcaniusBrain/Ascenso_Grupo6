import type { Metadata } from "next";
import { redirect } from "next/navigation";
import { leerSesionServidor } from "@/lib/auth/cookies";
import { extraerUsuarioPublico } from "@/lib/auth/session";
import { llamarBackendDesdeBff } from "@/lib/bff/proxy";
import { cargarListaPeriodosServidor } from "@/lib/bff/periodo";
import { ReportesImpactoVista } from "@/components/reportes/ReportesImpactoVista";
import type { DonanteResumenDto } from "@/lib/contratos/s2";
import type { SolicitudReporteImpactoDto } from "@/lib/contratos/s6";

export const metadata: Metadata = {
  title: "Reportes de impacto",
};

export default async function ReportesImpactoPage() {
  const sesion = await leerSesionServidor();
  if (!sesion) {
    redirect("/login");
  }

  const [respReportes, respDonantes, listaPeriodos] = await Promise.all([
    llamarBackendDesdeBff({ ruta: "/api/v1/reportes/impacto" }),
    llamarBackendDesdeBff({ ruta: "/api/v1/donantes" }),
    cargarListaPeriodosServidor(),
  ]);

  let reportes: SolicitudReporteImpactoDto[] = [];
  let errorCarga: string | null = null;
  if (respReportes.ok && respReportes.bodyText) {
    reportes = JSON.parse(respReportes.bodyText) as SolicitudReporteImpactoDto[];
  } else {
    errorCarga = "No se pudo consultar el historial de reportes de impacto en S6.";
  }

  const donantes =
    respDonantes.ok && respDonantes.bodyText
      ? (JSON.parse(respDonantes.bodyText) as DonanteResumenDto[])
      : [];

  return (
    <ReportesImpactoVista
      usuario={extraerUsuarioPublico(sesion)}
      reportesIniciales={reportes}
      donantes={donantes}
      periodos={listaPeriodos?.periodos ?? []}
      periodoActual={listaPeriodos?.periodoActual ?? null}
      errorCarga={errorCarga}
    />
  );
}
