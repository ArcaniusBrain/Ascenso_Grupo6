"use client";

import { useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Clock } from "lucide-react";
import { Boton } from "@/components/ui/Boton";
import { CasillasCodigo } from "@/components/ui/CasillasCodigo";
import { Aviso } from "@/components/ui/Aviso";
import { ErrorApiAscenso, llamarApiBff } from "@/lib/bff/client";

export interface FormularioVerificacion2FaProps {
  correoEnmascarado: string;
  canalInicial: "CORREO" | "SMS";
  destinoInicial?: string;
}

export function FormularioVerificacion2Fa({
  correoEnmascarado,
  canalInicial,
  destinoInicial = "/inicio",
}: FormularioVerificacion2FaProps) {
  const router = useRouter();
  const [codigo, setCodigo] = useState("");
  const [canal, setCanal] = useState<"CORREO" | "SMS">(canalInicial);
  const [verificando, setVerificando] = useState(false);
  const [reenviando, setReenviando] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);

  async function manejarVerificacion(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);
    setInfo(null);

    if (codigo.replace(/\D/g, "").length !== 6) {
      setError("Ingresa los 6 dígitos del código de verificación.");
      return;
    }

    setVerificando(true);
    try {
      await llamarApiBff("/api/auth/2fa/verificar", {
        method: "POST",
        body: { codigo },
      });
      router.replace(destinoInicial || "/inicio");
      router.refresh();
    } catch (err) {
      if (err instanceof ErrorApiAscenso) {
        setError(err.message);
      } else {
        setError("No se pudo verificar el código. Intenta nuevamente.");
      }
    } finally {
      setVerificando(false);
    }
  }

  async function reenviarCodigo() {
    setError(null);
    setInfo(null);
    setReenviando(true);
    try {
      const resp = await llamarApiBff<{ canal: "CORREO" | "SMS"; mensaje?: string }>(
        "/api/auth/2fa/reenviar",
        { method: "POST" }
      );
      setCanal(resp.canal);
      setCodigo("");
      setInfo(resp.mensaje ?? "Enviamos un nuevo código de 6 dígitos.");
    } catch (err) {
      if (err instanceof ErrorApiAscenso) {
        setError(err.message);
      } else {
        setError("No se pudo reenviar el código en este momento.");
      }
    } finally {
      setReenviando(false);
    }
  }

  const descripcionDestino =
    canal === "SMS"
      ? "Enviamos un código de 6 dígitos por SMS a tu teléfono registrado. El código vale 15 minutos."
      : `Enviamos un código de 6 dígitos a ${correoEnmascarado}. El código vale 15 minutos.`;

  return (
    <form onSubmit={manejarVerificacion} noValidate className="flex flex-col gap-8">
      <div className="flex flex-col gap-2">
        <h2 className="font-display text-[44px] font-bold leading-[48px] tracking-[-0.02em] text-[var(--color-text-primary)]">
          Verificación
          <br />
          en dos pasos
        </h2>
        <p className="text-[14px] leading-[22px] text-[var(--color-text-secondary)]">
          {descripcionDestino}
        </p>
      </div>

      {error ? <Aviso tono="peligro">{error}</Aviso> : null}
      {info ? <Aviso tono="exito">{info}</Aviso> : null}

      <CasillasCodigo
        valor={codigo}
        onChange={setCodigo}
        disabled={verificando || reenviando}
      />

      <Boton type="submit" variante="primario" anchoCompleto cargando={verificando}>
        Verificar e ingresar
      </Boton>

      <div className="flex flex-wrap items-center justify-center gap-2">
        <Boton
          variante="terciario"
          onClick={reenviarCodigo}
          disabled={verificando || reenviando}
        >
          {reenviando ? "Reenviando..." : "Reenviar código"}
        </Boton>
        <Link
          href="/login"
          className="ascenso-focus inline-flex h-[44px] items-center justify-center rounded-[var(--radius-md)] px-4 text-[14px] font-semibold leading-[20px] text-[var(--color-text-brand)] transition-colors hover:bg-[var(--color-bg-brand-soft)]"
        >
          Volver a iniciar sesión
        </Link>
      </div>

      <div className="flex items-start gap-2">
        <Clock
          className="mt-0.5 size-4 shrink-0 text-[var(--color-icon-muted)]"
          aria-hidden="true"
        />
        <p className="text-[12px] leading-[18px] text-[var(--color-text-muted)]">
          Tras 3 intentos fallidos deberás esperar 1 hora para volver a intentarlo.
        </p>
      </div>
    </form>
  );
}
