import type { ReactNode } from "react";
import { MarcaLogo } from "@/components/ui/Marca";

export interface PeldanoEscalera {
  etiqueta: string;
  colorCss: string;
  alturaClase: string;
}

export const ESCALERA_LOGIN: PeldanoEscalera[] = [
  { etiqueta: "Postulas", colorCss: "bg-[var(--color-bg-brand)]", alturaClase: "h-[120px]" },
  { etiqueta: "Te evaluamos", colorCss: "bg-[var(--color-bg-accent)]", alturaClase: "h-[240px]" },
  { etiqueta: "Recibes tu beca", colorCss: "bg-[var(--color-bg-success)]", alturaClase: "h-[360px]" },
];

export const ESCALERA_2FA: PeldanoEscalera[] = [
  { etiqueta: "Tu contraseña", colorCss: "bg-[var(--color-bg-brand)]", alturaClase: "h-[120px]" },
  { etiqueta: "Tu código", colorCss: "bg-[var(--color-bg-accent)]", alturaClase: "h-[240px]" },
];

/**
 * Marco split-screen oficial de las pantallas de acceso (Figma 01 · Iniciar sesión y 02 · Verificación 2FA).
 */
export function PantallaAccesoMarco({
  tituloMarca,
  descripcionMarca,
  peldanos,
  children,
}: {
  tituloMarca: ReactNode;
  descripcionMarca: string;
  peldanos: PeldanoEscalera[];
  children: ReactNode;
}) {
  return (
    <div className="flex min-h-screen w-full flex-col bg-[var(--color-bg-surface)] lg:flex-row">
      <aside className="flex w-full flex-col justify-between bg-[var(--color-bg-brand-strong)] pt-10 lg:w-[58%] lg:max-w-[880px] lg:pt-12">
        <div className="flex flex-col gap-12 px-8 pb-10 lg:gap-16 lg:px-16 lg:pb-8">
          <MarcaLogo variante="oscuro" />

          <div className="flex flex-col gap-6">
            <h1 className="font-display text-[48px] font-bold leading-[50px] tracking-[-0.03em] text-[var(--color-text-on-brand)] xl:text-[80px] xl:leading-[78px]">
              {tituloMarca}
            </h1>
            <p className="font-display max-w-[520px] text-[18px] font-semibold leading-[26px] tracking-[-0.01em] text-[var(--color-text-on-brand-muted)] lg:text-[20px]">
              {descripcionMarca}
            </p>
          </div>
        </div>

        <div aria-hidden="true" className="hidden w-full items-end lg:flex">
          {peldanos.map((peldano, idx) => (
            <div key={peldano.etiqueta} className="flex min-w-0 flex-1 flex-col gap-4">
              <div className={idx === 0 ? "pl-16 pr-6" : "px-6"}>
                <p className="font-display text-[20px] font-semibold leading-[26px] tracking-[-0.01em] text-[var(--color-text-on-brand)] whitespace-nowrap">
                  {peldano.etiqueta}
                </p>
              </div>
              <div className={`w-full ${peldano.colorCss} ${peldano.alturaClase}`} />
            </div>
          ))}
        </div>
      </aside>

      <main className="flex flex-1 flex-col items-center justify-center px-6 py-12 lg:px-12">
        <div className="w-full max-w-[400px]">{children}</div>
      </main>
    </div>
  );
}
