import type { Metadata } from "next";
import { redirect } from "next/navigation";
import { leerSesionServidor } from "@/lib/auth/cookies";
import {
  ESCALERA_LOGIN,
  PantallaAccesoMarco,
} from "@/components/auth/PantallaAccesoMarco";
import { FormularioLogin } from "@/components/auth/FormularioLogin";

export const metadata: Metadata = {
  title: "Iniciar sesión",
};

export default async function LoginPage({
  searchParams,
}: {
  searchParams: Promise<{ siguiente?: string }>;
}) {
  const sesion = await leerSesionServidor();
  const params = await searchParams;
  const destino =
    params.siguiente && params.siguiente.startsWith("/") && !params.siguiente.startsWith("//")
      ? params.siguiente
      : "/inicio";

  if (sesion) {
    redirect(destino);
  }

  return (
    <PantallaAccesoMarco
      tituloMarca="Becas que impulsan tu carrera"
      descripcionMarca="Postula, sigue el estado de tu solicitud y gestiona tu beca en un solo lugar."
      peldanos={ESCALERA_LOGIN}
    >
      <FormularioLogin destinoInicial={destino} />
    </PantallaAccesoMarco>
  );
}
