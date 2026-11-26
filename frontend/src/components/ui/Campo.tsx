import { useId, type InputHTMLAttributes } from "react";

export interface CampoProps extends InputHTMLAttributes<HTMLInputElement> {
  etiqueta: string;
  ayuda?: string;
  error?: string;
}

export function Campo({
  etiqueta,
  ayuda,
  error,
  id,
  className = "",
  disabled,
  ...rest
}: CampoProps) {
  const autoId = useId();
  const inputId = id ?? autoId;
  const hayError = Boolean(error);

  return (
    <div className={`flex w-full flex-col gap-2 ${className}`}>
      <label
        htmlFor={inputId}
        className="text-[13px] font-medium leading-[18px] text-[var(--color-text-secondary)]"
      >
        {etiqueta}
      </label>
      <input
        id={inputId}
        disabled={disabled}
        aria-invalid={hayError}
        aria-describedby={hayError ? `${inputId}-error` : ayuda ? `${inputId}-ayuda` : undefined}
        className={`ascenso-focus w-full rounded-[var(--radius-md)] border bg-[var(--color-bg-surface)] px-3 py-3 text-[14px] leading-[22px] text-[var(--color-text-primary)] placeholder:text-[var(--color-text-disabled)] transition-colors ${
          hayError
            ? "border-[var(--color-bg-danger)]"
            : "border-[var(--color-border-strong)] hover:border-[var(--gris-500)] focus:border-[var(--color-border-brand)]"
        } disabled:cursor-not-allowed disabled:border-[var(--color-border-default)] disabled:bg-[var(--color-bg-muted)] disabled:text-[var(--color-text-muted)]`}
        {...rest}
      />
      {hayError ? (
        <p id={`${inputId}-error`} className="text-[12px] leading-[18px] text-[var(--color-text-danger)]">
          {error}
        </p>
      ) : ayuda ? (
        <p id={`${inputId}-ayuda`} className="text-[12px] leading-[18px] text-[var(--color-text-muted)]">
          {ayuda}
        </p>
      ) : null}
    </div>
  );
}
