import { useId, type SelectHTMLAttributes } from "react";

export interface OpcionSelect {
  valor: string;
  etiqueta: string;
}

export interface CampoSelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  etiqueta: string;
  opciones: OpcionSelect[];
  ayuda?: string;
  error?: string;
  placeholder?: string;
}

export function CampoSelect({
  etiqueta,
  opciones,
  ayuda,
  error,
  placeholder,
  id,
  className = "",
  disabled,
  ...rest
}: CampoSelectProps) {
  const autoId = useId();
  const selectId = id ?? autoId;
  const hayError = Boolean(error);

  return (
    <div className={`flex w-full flex-col gap-2 ${className}`}>
      <label
        htmlFor={selectId}
        className="text-[13px] font-medium leading-[18px] text-[var(--color-text-secondary)]"
      >
        {etiqueta}
      </label>
      <select
        id={selectId}
        disabled={disabled}
        aria-invalid={hayError}
        className={`ascenso-focus w-full rounded-[var(--radius-md)] border bg-[var(--color-bg-surface)] px-3 py-3 text-[14px] leading-[22px] text-[var(--color-text-primary)] transition-colors ${
          hayError
            ? "border-[var(--color-bg-danger)]"
            : "border-[var(--color-border-strong)] hover:border-[var(--gris-500)] focus:border-[var(--color-border-brand)]"
        } disabled:cursor-not-allowed disabled:border-[var(--color-border-default)] disabled:bg-[var(--color-bg-muted)] disabled:text-[var(--color-text-muted)]`}
        {...rest}
      >
        {placeholder ? <option value="">{placeholder}</option> : null}
        {opciones.map((op) => (
          <option key={op.valor} value={op.valor}>
            {op.etiqueta}
          </option>
        ))}
      </select>
      {hayError ? (
        <p className="text-[12px] leading-[18px] text-[var(--color-text-danger)]">{error}</p>
      ) : ayuda ? (
        <p className="text-[12px] leading-[18px] text-[var(--color-text-muted)]">{ayuda}</p>
      ) : null}
    </div>
  );
}
