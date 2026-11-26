import type { Metadata } from "next";
import { redirect } from "next/navigation";
import { leerSesionServidor } from "@/lib/auth/cookies";
import { extraerUsuarioPublico } from "@/lib/auth/session";
import { llamarBackendDesdeBff } from "@/lib/bff/proxy";
import { cargarListaPeriodosServidor } from "@/lib/bff/periodo";
import { UsuariosVista } from "@/components/usuarios/UsuariosVista";
import type { TipoUsuarioDto, UsuarioDto } from "@/lib/contratos/s1";

export const metadata: Metadata = {
  title: "Usuarios y roles",
};

export default async function UsuariosPage() {
  const sesion = await leerSesionServidor();
  if (!sesion) {
    redirect("/login");
  }

  const [respUsuarios, respTipos, listaPeriodos] = await Promise.all([
    llamarBackendDesdeBff({ ruta: "/api/v1/usuarios" }),
    llamarBackendDesdeBff({ ruta: "/api/v1/tipos-usuario" }),
    cargarListaPeriodosServidor(),
  ]);

  let usuarios: UsuarioDto[] = [];
  let tipos: TipoUsuarioDto[] = [];
  let errorCarga: string | null = null;

  if (respUsuarios.ok && respUsuarios.bodyText) {
    usuarios = JSON.parse(respUsuarios.bodyText) as UsuarioDto[];
  } else {
    try {
      const p = JSON.parse(respUsuarios.bodyText) as { detail?: string };
      errorCarga = p.detail ?? "No se pudo consultar el directorio de usuarios (S1).";
    } catch {
      errorCarga = "No se pudo consultar el directorio de usuarios (S1).";
    }
  }

  if (respTipos.ok && respTipos.bodyText) {
    tipos = JSON.parse(respTipos.bodyText) as TipoUsuarioDto[];
  }

  return (
    <UsuariosVista
      usuario={extraerUsuarioPublico(sesion)}
      usuariosIniciales={usuarios}
      tiposIniciales={tipos}
      periodoActual={listaPeriodos?.periodoActual ?? null}
      errorCarga={errorCarga}
    />
  );
}
