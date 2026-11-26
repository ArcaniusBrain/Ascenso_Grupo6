import { redirect } from "next/navigation";
import { leerSesionServidor } from "@/lib/auth/cookies";

export default async function HomePage() {
  const sesion = await leerSesionServidor();
  redirect(sesion ? "/inicio" : "/login");
}
