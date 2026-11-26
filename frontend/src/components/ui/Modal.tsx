"use client";

import { useEffect, type ReactNode } from "react";
import { X } from "lucide-react";

/**
 * Modal editorial de ASCENSO (único componente con elevación junto a las vistas de acceso,
 * siguiendo el principio "Una superficie por idea" de Figma 01 · Fundamentos).
 */
export function Modal({
  abierto,
  onCerrar,
  titulo,
  descripcion,
  children,
  ancho = "max-w-[540px]",
}: {
  abierto: boolean;
  onCerrar: () => void;
  titulo: string;
  descripcion?: string;
  children: ReactNode;
  ancho?: string;
}) {
  useEffect(() => {
    if (!abierto) {
      return;
    }

    function manejarEscape(e: KeyboardEvent) {
      if (e.key === "Escape") {
        onCerrar();
      }
    }

    window.addEventListener("keydown", manejarEscape);
    return () => window.removeEventListener("keydown", manejarEscape);
  }, [abierto, onCerrar]);

  if (!abierto) {
    return null;
  }

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="modal-ascenso-titulo"
      className="fixed inset-0 z-50 flex items-center justify-center bg-[#0e2140]/50 p-4"
      onClick={(e) => {
        if (e.target === e.currentTarget) {
          onCerrar();
        }
      }}
    >
      <div
        className={`flex max-h-[90vh] w-full flex-col overflow-hidden rounded-[var(--radius-md)] border border-[var(--color-border-default)] bg-[var(--color-bg-surface)] shadow-xl ${ancho}`}
      >
        <div className="flex items-start justify-between gap-4 border-b border-[var(--color-border-default)] px-6 py-5">
          <div className="min-w-0">
            <h2
              id="modal-ascenso-titulo"
              className="font-display text-[22px] font-semibold leading-[28px] tracking-[-0.015em] text-[var(--color-text-primary)]"
            >
              {titulo}
            </h2>
            {descripcion ? (
              <p className="mt-1 text-[14px] leading-[20px] text-[var(--color-text-secondary)]">
                {descripcion}
              </p>
            ) : null}
          </div>
          <button
            type="button"
            onClick={onCerrar}
            aria-label="Cerrar"
            className="ascenso-focus inline-flex size-9 shrink-0 cursor-pointer items-center justify-center rounded-[var(--radius-sm)] text-[var(--color-text-secondary)] transition-colors hover:bg-[var(--color-bg-subtle)] hover:text-[var(--color-text-primary)]"
          >
            <X className="size-5" aria-hidden="true" />
          </button>
        </div>

        <div className="overflow-y-auto px-6 py-5">{children}</div>
      </div>
    </div>
  );
}
