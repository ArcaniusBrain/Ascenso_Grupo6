import type { ButtonHTMLAttributes, ReactNode } from "react";

export type VarianteBoton =
  | "primario"
  | "secundario"
  | "terciario"
  | "peligro"
  | "primarioSobreColor"
  | "secundarioSobreColor";

export interface BotonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variante?: VarianteBoton;
  anchoCompleto?: boolean;
  cargando?: boolean;
  icono?: ReactNode;
}

const ESTILOS_VARIANTE: Record<VarianteBoton, string> = {
  primario:
    "rounded-[var(--radius-md)] bg-[var(--color-bg-brand)] text-[var(--color-text-on-brand)] hover:bg-[var(--color-bg-brand-hover)] disabled:bg-[var(--color-bg-disabled)] disabled:text-[var(--color-text-disabled)]",
  secundario:
    "rounded-[var(--radius-md)] border border-[var(--color-border-strong)] bg-[var(--color-bg-surface)] text-[var(--color-text-primary)] hover:bg-[var(--color-bg-subtle)] disabled:border-[var(--color-border-default)] disabled:bg-[var(--color-bg-muted)] disabled:text-[var(--color-text-disabled)]",
  terciario:
    "rounded-[var(--radius-md)] bg-transparent text-[var(--color-text-brand)] hover:bg-[var(--color-bg-brand-soft)] disabled:text-[var(--color-text-disabled)] disabled:hover:bg-transparent",
  peligro:
    "rounded-[var(--radius-md)] bg-[var(--color-bg-danger)] text-[var(--color-text-on-brand)] hover:bg-[var(--color-bg-danger-hover)] disabled:bg-[var(--color-bg-disabled)] disabled:text-[var(--color-text-disabled)]",
  primarioSobreColor:
    "rounded-[var(--radius-full)] bg-[var(--color-bg-surface)] px-5 text-[var(--color-text-primary)] hover:bg-[var(--color-bg-subtle)] disabled:opacity-60",
  secundarioSobreColor:
    "rounded-[var(--radius-full)] border border-[var(--color-icon-on-brand)] bg-transparent px-5 text-[var(--color-text-on-brand)] hover:bg-white/10 disabled:opacity-60",
};

export function Boton({
  variante = "primario",
  anchoCompleto = false,
  cargando = false,
  icono,
  className = "",
  disabled,
  type = "button",
  children,
  ...rest
}: BotonProps) {
  return (
    <button
      type={type}
      disabled={disabled || cargando}
      className={`ascenso-focus inline-flex h-[44px] cursor-pointer items-center justify-center gap-2 px-4 text-[14px] font-semibold leading-[20px] transition-colors disabled:cursor-not-allowed ${
        ESTILOS_VARIANTE[variante]
      } ${anchoCompleto ? "w-full" : ""} ${className}`}
      {...rest}
    >
      {icono ? <span className="inline-flex shrink-0 items-center">{icono}</span> : null}
      <span>{cargando ? "Procesando..." : children}</span>
    </button>
  );
}
