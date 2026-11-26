import Link from "next/link";
import { redirect } from "next/navigation";
import { ArrowRight } from "lucide-react";
import { leerSesionServidor } from "@/lib/auth/cookies";
import { extraerUsuarioPublico } from "@/lib/auth/session";
import { filtrarNavegacionPorRoles } from "@/lib/navegacion";
import { FranjaEstado } from "@/components/layout/FranjaEstado";
import { Estado } from "@/components/ui/Estado";
import { FilaDato, SeccionPanel } from "@/components/ui/SeccionPanel";

export default async function InicioPage() {
  const sesion = await leerSesionServidor();
  if (!sesion) {
    redirect("/login");
  }

  const usuario = extraerUsuarioPublico(sesion);
  const secciones = filtrarNavegacionPorRoles(usuario.roles).filter((s) => s.id !== "inicio");
  const rolPrincipal = usuario.roles[0] ?? "Usuario";

  return (
    <FranjaEstado
      tono="inicio"
      usuario={usuario}
      titulo={`Hola, ${usuario.nombreCompleto.split(" ")[0]}`}
      estadoBadge={
        <Estado
          texto={usuario.segundoFactorVerificado ? "Sesión verificada (2FA)" : rolPrincipal}
          tono={usuario.segundoFactorVerificado ? "exito" : "info"}
        />
      }
      subtitulo={`Rol activo: ${usuario.roles.join(", ") || "Sin rol asignado"} · ${usuario.correo}`}
    >
      <div className="grid grid-cols-1 gap-16 lg:grid-cols-[1fr_360px]">
        <SeccionPanel titulo="Módulos habilitados">
          {secciones.length === 0 ? (
            <p className="py-6 text-[14px] text-[var(--color-text-secondary)]">
              No hay módulos habilitados para tu rol actual.
            </p>
          ) : (
            <div className="divide-y divide-[var(--color-border-default)]">
              {secciones.map((modulo) => (
                <div
                  key={modulo.id}
                  className="flex flex-col justify-between gap-3 py-4 sm:flex-row sm:items-center"
                >
                  <div className="min-w-0 flex-1">
                    <div className="flex items-center gap-2">
                      <p className="text-[14px] font-semibold leading-[22px] text-[var(--color-text-primary)]">
                        {modulo.etiqueta}
                      </p>
                      <Estado texto={modulo.rf} tono="neutro" />
                    </div>
                    <p className="text-[14px] leading-[22px] text-[var(--color-text-secondary)]">
                      {modulo.descripcion}
                    </p>
                  </div>
                  <Link
                    href={modulo.href}
                    className="ascenso-focus inline-flex items-center gap-1.5 rounded-[var(--radius-md)] px-3 py-2 text-[14px] font-semibold text-[var(--color-text-brand)] transition-colors hover:bg-[var(--color-bg-brand-soft)]"
                  >
                    <span>Abrir</span>
                    <ArrowRight className="size-4" aria-hidden="true" />
                  </Link>
                </div>
              ))}
            </div>
          )}
        </SeccionPanel>

        <SeccionPanel titulo="Tu sesión">
          <div className="py-2">
            <FilaDato etiqueta="Correo" valor={usuario.correo} />
            <FilaDato etiqueta="Roles" valor={usuario.roles.join(", ") || "—"} />
            <FilaDato
              etiqueta="Segundo factor (2FA)"
              valor={usuario.segundoFactorVerificado ? "Verificado" : "No requerido"}
            />
            <FilaDato
              etiqueta="Permisos activos"
              valor={`${usuario.permisos.length} permiso(s)`}
            />
          </div>
          <p className="pt-2 text-[12px] leading-[18px] text-[var(--color-text-muted)]">
            Tu sesión se resguarda en cookies HttpOnly gestionadas por el servidor (ADR-28).
          </p>
        </SeccionPanel>
      </div>
    </FranjaEstado>
  );
}
