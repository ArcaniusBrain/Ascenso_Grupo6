import type { ReactNode } from "react";

/**
 * Superficie editorial estándar de ASCENSO: título H1 (30px Bricolage Grotesque)
 * sobre borde inferior de 2px (#c3ccd6), sin sombras innecesarias.
 */
export function SeccionPanel({
  titulo,
  accion,
  children,
  className = "",
}: {
  titulo: string;
  accion?: ReactNode;
  children: ReactNode;
  className?: string;
}) {
  return (
    <section className={`flex flex-col ${className}`}>
      <header className="flex items-end justify-between gap-4 border-b-2 border-[var(--color-border-strong)] pb-4">
        <h2 className="font-display text-[30px] font-semibold leading-[36px] tracking-[-0.015em] text-[var(--color-text-primary)]">
          {titulo}
        </h2>
        {accion ? <div className="shrink-0">{accion}</div> : null}
      </header>
      <div className="pt-2">{children}</div>
    </section>
  );
}

export function FilaDato({
  etiqueta,
  valor,
  conBorde = false,
}: {
  etiqueta: string;
  valor: ReactNode;
  conBorde?: boolean;
}) {
  return (
    <div
      className={`flex items-center justify-between gap-4 py-2 text-[14px] leading-[22px] ${
        conBorde ? "border-b border-[var(--color-border-default)] py-4" : ""
      }`}
    >
      <span className="text-[var(--color-text-secondary)]">{etiqueta}</span>
      <span className="text-right font-semibold text-[var(--color-text-primary)]">{valor}</span>
    </div>
  );
}
