"use client";

import { useRef, type ClipboardEvent, type KeyboardEvent } from "react";

export interface CasillasCodigoProps {
  valor: string;
  onChange: (nuevoValor: string) => void;
  longitud?: number;
  disabled?: boolean;
  ariaLabel?: string;
}

/**
 * 6 casillas de código (48×56px, Bricolage Grotesque SemiBold 20px) según Figma `02 · Componentes / Casilla de código`.
 * Reutilizable para verificación en dos pasos (RF-37) y operaciones sensibles con OTP (ADR-10).
 */
export function CasillasCodigo({
  valor,
  onChange,
  longitud = 6,
  disabled = false,
  ariaLabel = "Código de verificación",
}: CasillasCodigoProps) {
  const refs = useRef<Array<HTMLInputElement | null>>([]);
  const digitos = Array.from({ length: longitud }, (_, i) => valor[i] ?? "");

  function actualizarIndice(indice: number, caracter: string) {
    const limpio = caracter.replace(/\D/g, "");
    if (!limpio) {
      const copia = [...digitos];
      copia[indice] = "";
      onChange(copia.join(""));
      return;
    }

    const copia = [...digitos];
    for (let offset = 0; offset < limpio.length && indice + offset < longitud; offset++) {
      copia[indice + offset] = limpio[offset];
    }
    onChange(copia.join(""));

    const siguiente = Math.min(indice + limpio.length, longitud - 1);
    refs.current[siguiente]?.focus();
  }

  function manejarTecla(indice: number, e: KeyboardEvent<HTMLInputElement>) {
    if (e.key === "Backspace" && !digitos[indice] && indice > 0) {
      refs.current[indice - 1]?.focus();
    } else if (e.key === "ArrowLeft" && indice > 0) {
      refs.current[indice - 1]?.focus();
    } else if (e.key === "ArrowRight" && indice < longitud - 1) {
      refs.current[indice + 1]?.focus();
    }
  }

  function manejarPegado(e: ClipboardEvent<HTMLInputElement>) {
    const texto = e.clipboardData.getData("text").replace(/\D/g, "").slice(0, longitud);
    if (!texto) {
      return;
    }
    e.preventDefault();
    onChange(texto);
    const destino = Math.min(texto.length, longitud - 1);
    refs.current[destino]?.focus();
  }

  return (
    <div
      role="group"
      aria-label={ariaLabel}
      className="flex w-full items-center gap-2"
    >
      {digitos.map((digito, indice) => (
        <input
          key={indice}
          ref={(el) => {
            refs.current[indice] = el;
          }}
          type="text"
          inputMode="numeric"
          autoComplete={indice === 0 ? "one-time-code" : "off"}
          maxLength={longitud}
          disabled={disabled}
          value={digito}
          aria-label={`Dígito ${indice + 1} de ${longitud}`}
          onChange={(e) => actualizarIndice(indice, e.target.value)}
          onKeyDown={(e) => manejarTecla(indice, e)}
          onPaste={manejarPegado}
          className="ascenso-focus font-display h-[56px] min-w-0 flex-1 rounded-[var(--radius-md)] border border-[var(--color-border-strong)] bg-[var(--color-bg-surface)] text-center text-[20px] font-semibold leading-[26px] tracking-[-0.01em] text-[var(--color-text-primary)] transition-colors focus:border-2 focus:border-[var(--color-border-brand)] disabled:cursor-not-allowed disabled:bg-[var(--color-bg-muted)]"
        />
      ))}
    </div>
  );
}
