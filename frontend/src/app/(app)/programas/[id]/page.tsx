import type { Metadata } from "next";
import { notFound, redirect } from "next/navigation";
import { leerSesionServidor } from "@/lib/auth/cookies";
import { extraerUsuarioPublico } from "@/lib/auth/session";
import { llamarBackendDesdeBff } from "@/lib/bff/proxy";
import { cargarListaPeriodosServidor } from "@/lib/bff/periodo";
import { ProgramaDetalleVista } from "@/components/programas/ProgramaDetalleVista";
import type {
  DonanteResumenDto,
  PresupuestoProgramaDto,
  ProgramaDetalleDto,
  UniversidadResumenDto,
} from "@/lib/contratos/s2";

export const metadata: Metadata = {
  title: "Programa y presupuesto",
};

export default async function ProgramaDetallePage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const sesion = await leerSesionServidor();
  if (!sesion) {
    redirect("/login");
  }

  const { id } = await params;

  const [respDetalle, respPresupuesto, respDonantes, respUniversidades, listaPeriodos] =
    await Promise.all([
      llamarBackendDesdeBff({ ruta: `/api/v1/programas/${id}` }),
      llamarBackendDesdeBff({ ruta: `/api/v1/programas/${id}/presupuesto` }),
      llamarBackendDesdeBff({ ruta: "/api/v1/donantes" }),
      llamarBackendDesdeBff({ ruta: "/api/v1/universidades" }),
      cargarListaPeriodosServidor(),
    ]);

  if (respDetalle.status === 404 || !respDetalle.ok || !respDetalle.bodyText) {
    notFound();
  }

  const programa = JSON.parse(respDetalle.bodyText) as ProgramaDetalleDto;
  const presupuesto =
    respPresupuesto.ok && respPresupuesto.bodyText
      ? (JSON.parse(respPresupuesto.bodyText) as PresupuestoProgramaDto)
      : null;
  const donantes =
    respDonantes.ok && respDonantes.bodyText
      ? (JSON.parse(respDonantes.bodyText) as DonanteResumenDto[])
      : [];
  const universidades =
    respUniversidades.ok && respUniversidades.bodyText
      ? (JSON.parse(respUniversidades.bodyText) as UniversidadResumenDto[])
      : [];

  return (
    <ProgramaDetalleVista
      usuario={extraerUsuarioPublico(sesion)}
      programaInicial={programa}
      presupuestoInicial={presupuesto}
      donantesCatalogo={donantes}
      universidadesCatalogo={universidades}
      periodos={listaPeriodos?.periodos ?? []}
      periodoActual={listaPeriodos?.periodoActual ?? null}
    />
  );
}
