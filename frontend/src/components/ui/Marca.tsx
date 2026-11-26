export function MarcaIsotipo({ className = "" }: { className?: string }) {
  return (
    <span
      aria-hidden="true"
      className={`inline-flex size-8 shrink-0 items-end justify-center rounded-[var(--radius-sm)] bg-[var(--color-bg-brand)] py-[7px] ${className}`}
    >
      <span className="size-[6px] bg-white" />
      <span className="h-[12px] w-[6px] bg-white" />
      <span className="h-[18px] w-[6px] bg-white" />
    </span>
  );
}

export function MarcaLogo({
  variante = "oscuro",
  className = "",
}: {
  variante?: "oscuro" | "claro";
  className?: string;
}) {
  const colorTexto =
    variante === "oscuro"
      ? "text-[var(--color-text-on-brand)]"
      : "text-[var(--color-text-primary)]";

  return (
    <span className={`inline-flex items-center gap-3 ${className}`}>
      <MarcaIsotipo />
      <span
        className={`font-display text-[20px] font-semibold leading-[26px] tracking-[-0.01em] ${colorTexto}`}
      >
        Ascenso
      </span>
    </span>
  );
}
