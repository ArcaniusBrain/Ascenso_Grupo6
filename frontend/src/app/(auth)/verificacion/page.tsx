import type { Metadata } from "next";
import { redirect } from "next/navigation";
import {
  leerDesafio2FaPendiente,
  leerSesionServidor,
} from "@/lib/auth/cookies";
import {
  ESCALERA_2FA,
  PantallaAccesoMarco,
} from "@/components/auth/PantallaAccesoMarco";
import { FormularioVerificacion2Fa } from "@/components/auth/FormularioVerificacion2Fa";

export const metadata: Metadata = {
  title: "Verificación en dos pasos",
};

export default async function Verificacion2FaPage({
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

  const pendiente = await leerDesafio2FaPendiente();
  if (!pendiente) {
    redirect("/login");
  }

  return (
    <PantallaAccesoMarco
      tituloMarca={
        <>
          Confirma que
          <br />
          eres tú
        </>
      }
      descripcionMarca="El personal de la Fundación, el comité y los enlaces de universidad confirman su identidad con un segundo paso."
      peldanos={ESCALERA_2FA}
    >
      <FormularioVerificacion2Fa
        correoEnmascarado={pendiente.correoEnmascarado}
        canalInicial={pendiente.canal}
        destinoInicial={destino}
      />
    </PantallaAccesoMarco>
  );
}
