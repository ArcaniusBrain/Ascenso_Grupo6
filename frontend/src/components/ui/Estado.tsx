export type TonoEstado = "neutro" | "info" | "acento" | "exito" | "alerta" | "peligro";

const ESTILOS_ESTADO: Record<TonoEstado, { fondo: string; texto: string; punto: string }> = {
  neutro: {
    fondo: "bg-[var(--color-bg-subtle)]",
    texto: "text-[var(--color-text-secondary)]",
    punto: "bg-[var(--color-icon-muted)]",
  },
  info: {
    fondo: "bg-[var(--color-bg-brand-soft)]",
    texto: "text-[var(--color-text-brand)]",
    punto: "bg-[var(--color-icon-brand)]",
  },
  acento: {
    fondo: "bg-[var(--color-bg-accent-soft)]",
    texto: "text-[var(--color-text-accent)]",
    punto: "bg-[var(--color-icon-accent)]",
  },
  exito: {
    fondo: "bg-[var(--color-bg-success-soft)]",
    texto: "text-[var(--color-text-success)]",
    punto: "bg-[var(--color-icon-success)]",
  },
  alerta: {
    fondo: "bg-[var(--color-bg-warning-soft)]",
    texto: "text-[var(--color-text-warning)]",
    punto: "bg-[var(--color-icon-warning)]",
  },
  peligro: {
    fondo: "bg-[var(--color-bg-danger-soft)]",
    texto: "text-[var(--color-text-danger)]",
    punto: "bg-[var(--color-icon-danger)]",
  },
};

export function Estado({
  texto,
  tono = "neutro",
  className = "",
}: {
  texto: string;
  tono?: TonoEstado;
  className?: string;
}) {
  const estilo = ESTILOS_ESTADO[tono];
  return (
    <span
      className={`inline-flex items-center gap-2 rounded-[var(--radius-full)] py-1 pl-2 pr-3 text-[13px] font-medium leading-[18px] whitespace-nowrap ${estilo.fondo} ${estilo.texto} ${className}`}
    >
      <span aria-hidden="true" className={`size-[6px] rounded-full ${estilo.punto}`} />
      <span>{texto}</span>
    </span>
  );
}
