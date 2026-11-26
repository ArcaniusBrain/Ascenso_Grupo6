import { AlertCircle, CheckCircle2, Info, TriangleAlert } from "lucide-react";
import type { ReactNode } from "react";

export type TonoAviso = "info" | "alerta" | "peligro" | "exito";

const ESTILOS_AVISO: Record<
  TonoAviso,
  { contenedor: string; icono: string; Icono: typeof Info }
> = {
  info: {
    contenedor:
      "border-[var(--azul-200)] bg-[var(--color-bg-brand-soft)] text-[var(--color-text-brand)]",
    icono: "text-[var(--color-icon-brand)]",
    Icono: Info,
  },
  alerta: {
    contenedor:
      "border-[#f3d49b] bg-[var(--color-bg-warning-soft)] text-[var(--color-text-warning)]",
    icono: "text-[var(--color-icon-warning)]",
    Icono: TriangleAlert,
  },
  peligro: {
    contenedor:
      "border-[#f2c2bc] bg-[var(--color-bg-danger-soft)] text-[var(--color-text-danger)]",
    icono: "text-[var(--color-icon-danger)]",
    Icono: AlertCircle,
  },
  exito: {
    contenedor:
      "border-[var(--verde-200)] bg-[var(--color-bg-success-soft)] text-[var(--color-text-success)]",
    icono: "text-[var(--color-icon-success)]",
    Icono: CheckCircle2,
  },
};

export function Aviso({
  tono = "info",
  titulo,
  children,
  className = "",
}: {
  tono?: TonoAviso;
  titulo?: string;
  children: ReactNode;
  className?: string;
}) {
  const { contenedor, icono, Icono } = ESTILOS_AVISO[tono];

  return (
    <div
      role={tono === "peligro" || tono === "alerta" ? "alert" : "status"}
      className={`flex items-start gap-3 rounded-[var(--radius-md)] border px-4 py-3 text-[14px] leading-[22px] ${contenedor} ${className}`}
    >
      <Icono className={`mt-0.5 size-5 shrink-0 ${icono}`} aria-hidden="true" />
      <div className="min-w-0 flex-1">
        {titulo ? <p className="font-semibold">{titulo}</p> : null}
        <div>{children}</div>
      </div>
    </div>
  );
}
