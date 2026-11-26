export function Avatar({
  iniciales,
  className = "",
}: {
  iniciales: string;
  className?: string;
}) {
  return (
    <span
      aria-hidden="true"
      className={`inline-flex size-9 shrink-0 items-center justify-center rounded-[var(--radius-full)] bg-[var(--color-bg-accent-soft)] text-[13px] font-medium leading-[18px] text-[var(--color-text-accent)] ${className}`}
    >
      {iniciales}
    </span>
  );
}
