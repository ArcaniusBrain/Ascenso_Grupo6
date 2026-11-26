import type { ReactNode } from "react";
import { BarraNavegacion } from "@/components/layout/BarraNavegacion";
import type { UsuarioSesionPublico } from "@/lib/auth/session";

export type TonoFranja = "inicio" | "proceso" | "logrado";

const FONDO_FRANJA: Record<TonoFranja, string> = {
  inicio: "bg-[var(--color-bg-brand-strong)]",
  proceso: "bg-[var(--color-bg-accent-strong)]",
  logrado: "bg-[var(--color-bg-success-strong)]",
};

/**
 * Franja de estado full-width del sistema visual ASCENSO:
 * contiene la barra de navegación superior, la portada de la vista y, opcionalmente,
 * la franja de cifras/KPIs siguiendo la escalera de color (azul -> turquesa -> verde).
 */
export function FranjaEstado({
  tono = "inicio",
  usuario,
  periodoActual,
  titulo,
  subtitulo,
  estadoBadge,
  acciones,
  cifras,
  children,
}: {
  tono?: TonoFranja;
  usuario: UsuarioSesionPublico;
  periodoActual?: string | null;
  titulo: string;
  subtitulo?: ReactNode;
  estadoBadge?: ReactNode;
  acciones?: ReactNode;
  cifras?: ReactNode;
  children?: ReactNode;
}) {
  return (
    <div className="flex min-h-screen flex-col bg-[var(--color-bg-surface)]">
      <div className={`${FONDO_FRANJA[tono]} w-full px-6 pt-6 lg:px-16`}>
        <div className="mx-auto max-w-[1312px]">
          <BarraNavegacion usuario={usuario} periodoActual={periodoActual} />

          <div className="flex flex-col justify-between gap-6 pb-12 pt-14 lg:flex-row lg:items-end lg:gap-8">
            <div className="flex min-w-0 flex-1 flex-col gap-4">
              <h1 className="font-display text-[44px] font-bold leading-[48px] tracking-[-0.03em] text-[var(--color-text-on-brand)] lg:text-[64px] lg:leading-[64px]">
                {titulo}
              </h1>
              {estadoBadge || subtitulo ? (
                <div className="flex flex-wrap items-center gap-3">
                  {estadoBadge}
                  {subtitulo ? (
                    <p className="text-[14px] leading-[22px] text-[var(--color-text-on-brand-muted)]">
                      {subtitulo}
                    </p>
                  ) : null}
                </div>
              ) : null}
            </div>
            {acciones ? (
              <div className="flex shrink-0 flex-wrap items-center gap-3">{acciones}</div>
            ) : null}
          </div>

          {cifras ? <div className="pb-12">{cifras}</div> : null}
        </div>
      </div>

      {children ? (
        <main className="mx-auto w-full max-w-[1440px] flex-1 px-6 py-12 lg:px-16">
          {children}
        </main>
      ) : null}
    </div>
  );
}
