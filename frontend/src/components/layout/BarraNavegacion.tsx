"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { Bell, LogOut } from "lucide-react";
import { useState } from "react";
import { MarcaLogo } from "@/components/ui/Marca";
import { Avatar } from "@/components/ui/Avatar";
import { filtrarNavegacionPorRoles, obtenerIniciales } from "@/lib/navegacion";
import type { UsuarioSesionPublico } from "@/lib/auth/session";

export interface BarraNavegacionProps {
  usuario: UsuarioSesionPublico;
  periodoActual?: string | null;
}

export function BarraNavegacion({ usuario, periodoActual }: BarraNavegacionProps) {
  const pathname = usePathname();
  const router = useRouter();
  const [cerrando, setCerrando] = useState(false);

  const secciones = filtrarNavegacionPorRoles(usuario.roles);
  const iniciales = obtenerIniciales(usuario.nombreCompleto);

  async function cerrarSesion() {
    setCerrando(true);
    try {
      await fetch("/api/auth/logout", { method: "POST", credentials: "same-origin" });
      router.replace("/login");
      router.refresh();
    } finally {
      setCerrando(false);
    }
  }

  return (
    <header className="flex w-full flex-wrap items-center justify-between gap-6">
      <div className="flex flex-wrap items-center gap-10">
        <Link href="/inicio" className="ascenso-focus rounded-[var(--radius-sm)]">
          <MarcaLogo variante="oscuro" />
        </Link>

        <nav aria-label="Navegación principal" className="flex flex-wrap items-center gap-7 pt-2">
          {secciones.map((item) => {
            const activo =
              pathname === item.href ||
              (item.href !== "/inicio" && pathname.startsWith(`${item.href}/`));

            return (
              <Link
                key={item.id}
                href={item.href}
                className="ascenso-focus group flex flex-col items-center gap-2 rounded-[var(--radius-sm)]"
              >
                <span
                  className={`text-[13px] font-medium leading-[18px] whitespace-nowrap transition-colors ${
                    activo
                      ? "text-[var(--color-text-on-brand)]"
                      : "text-[var(--color-text-on-brand-muted)] group-hover:text-[var(--color-text-on-brand)]"
                  }`}
                >
                  {item.etiqueta}
                </span>
                <span
                  aria-hidden="true"
                  className={`h-[2px] w-full transition-colors ${
                    activo ? "bg-[var(--color-bg-surface)]" : "bg-transparent"
                  }`}
                />
              </Link>
            );
          })}
        </nav>
      </div>

      <div className="flex items-center gap-4">
        {periodoActual ? (
          <span className="text-[12px] leading-[18px] text-[var(--color-text-on-brand-muted)] whitespace-nowrap">
            Periodo {periodoActual}
          </span>
        ) : null}

        <button
          type="button"
          aria-label="Notificaciones"
          title="Notificaciones"
          className="ascenso-focus inline-flex size-9 cursor-pointer items-center justify-center rounded-full text-[var(--color-icon-on-brand)] transition-colors hover:bg-white/10"
        >
          <Bell className="size-5" aria-hidden="true" />
        </button>

        <div className="flex items-center gap-3">
          <Avatar iniciales={iniciales} />
          <span className="hidden text-[14px] font-semibold leading-[22px] text-[var(--color-text-on-brand)] sm:inline whitespace-nowrap">
            {usuario.nombreCompleto}
          </span>
        </div>

        <button
          type="button"
          onClick={cerrarSesion}
          disabled={cerrando}
          title="Cerrar sesión"
          aria-label="Cerrar sesión"
          className="ascenso-focus inline-flex size-9 cursor-pointer items-center justify-center rounded-full text-[var(--color-text-on-brand-muted)] transition-colors hover:bg-white/10 hover:text-white disabled:opacity-50"
        >
          <LogOut className="size-4" aria-hidden="true" />
        </button>
      </div>
    </header>
  );
}
