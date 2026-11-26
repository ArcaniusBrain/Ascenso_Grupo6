"use client";

import { useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { Shield } from "lucide-react";
import { Boton } from "@/components/ui/Boton";
import { Campo } from "@/components/ui/Campo";
import { Aviso } from "@/components/ui/Aviso";
import { ErrorApiAscenso, llamarApiBff } from "@/lib/bff/client";

interface RespuestaLoginBff {
  requiereSegundoFactor: boolean;
  correoEnmascarado?: string;
  canal?: "CORREO" | "SMS";
}

export function FormularioLogin({ destinoInicial = "/inicio" }: { destinoInicial?: string }) {
  const router = useRouter();
  const [correo, setCorreo] = useState("");
  const [contrasena, setContrasena] = useState("");
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [avisoRecuperacion, setAvisoRecuperacion] = useState<string | null>(null);

  async function manejarEnvio(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);
    setAvisoRecuperacion(null);

    if (!correo.trim() || !contrasena) {
      setError("Ingresa tu correo electrónico y contraseña.");
      return;
    }

    setCargando(true);
    try {
      const resultado = await llamarApiBff<RespuestaLoginBff>("/api/auth/login", {
        method: "POST",
        body: { correo: correo.trim(), contrasena },
      });

      if (resultado.requiereSegundoFactor) {
        const params = new URLSearchParams();
        if (destinoInicial && destinoInicial !== "/inicio") {
          params.set("siguiente", destinoInicial);
        }
        const query = params.toString();
        router.push(query ? `/verificacion?${query}` : "/verificacion");
        return;
      }

      router.replace(destinoInicial || "/inicio");
      router.refresh();
    } catch (err) {
      if (err instanceof ErrorApiAscenso) {
        setError(err.message);
      } else {
        setError("No se pudo iniciar sesión en este momento. Intenta nuevamente.");
      }
    } finally {
      setCargando(false);
    }
  }

  return (
    <form onSubmit={manejarEnvio} noValidate className="flex flex-col gap-8">
      <div className="flex flex-col gap-2">
        <h2 className="font-display text-[44px] font-bold leading-[48px] tracking-[-0.02em] text-[var(--color-text-primary)]">
          Iniciar sesión
        </h2>
        <p className="text-[14px] leading-[22px] text-[var(--color-text-secondary)]">
          Ingresa con los datos de tu cuenta.
        </p>
      </div>

      {error ? <Aviso tono="peligro">{error}</Aviso> : null}
      {avisoRecuperacion ? <Aviso tono="info">{avisoRecuperacion}</Aviso> : null}

      <div className="flex flex-col items-end gap-4">
        <Campo
          etiqueta="Correo electrónico"
          type="email"
          autoComplete="email"
          placeholder="nombre.apellido@correo.com"
          value={correo}
          onChange={(e) => setCorreo(e.target.value)}
          disabled={cargando}
          required
        />

        <Campo
          etiqueta="Contraseña"
          type="password"
          autoComplete="current-password"
          placeholder="••••••••••"
          value={contrasena}
          onChange={(e) => setContrasena(e.target.value)}
          disabled={cargando}
          required
        />

        <Boton
          variante="terciario"
          onClick={() =>
            setAvisoRecuperacion(
              "Para restablecer tu contraseña, comunícate con la administración de la Fundación o utiliza el enlace de recuperación configurado en Keycloak."
            )
          }
          className="h-auto px-2 py-1"
        >
          ¿Olvidaste tu contraseña?
        </Boton>
      </div>

      <Boton type="submit" variante="primario" anchoCompleto cargando={cargando}>
        Ingresar
      </Boton>

      <div className="flex flex-col gap-3">
        <p className="text-[14px] leading-[22px] text-[var(--color-text-secondary)]">
          ¿Eres estudiante y no tienes cuenta?
        </p>
        <Boton
          variante="secundario"
          anchoCompleto
          onClick={() =>
            setAvisoRecuperacion(
              "El registro de postulantes (S1 + S3) se habilitará al integrarse el módulo de postulación."
            )
          }
        >
          Crear cuenta de postulante
        </Boton>
      </div>

      <div className="flex items-start gap-2">
        <Shield
          className="mt-0.5 size-4 shrink-0 text-[var(--color-icon-muted)]"
          aria-hidden="true"
        />
        <p className="text-[12px] leading-[18px] text-[var(--color-text-muted)]">
          El personal de la Fundación, el comité y los enlaces de universidad confirman su identidad
          con un segundo paso.
        </p>
      </div>
    </form>
  );
}
