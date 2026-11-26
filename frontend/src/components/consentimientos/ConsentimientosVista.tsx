"use client";

import { useState, type FormEvent } from "react";
import { ShieldCheck } from "lucide-react";
import { FranjaEstado } from "@/components/layout/FranjaEstado";
import { Boton } from "@/components/ui/Boton";
import { Campo } from "@/components/ui/Campo";
import { Estado } from "@/components/ui/Estado";
import { Aviso } from "@/components/ui/Aviso";
import { SeccionPanel } from "@/components/ui/SeccionPanel";
import { ErrorApiAscenso, llamarApiBff } from "@/lib/bff/client";
import { formatearFechaCorta } from "@/lib/formato";
import type { UsuarioSesionPublico } from "@/lib/auth/session";
import type {
  ConsentimientoDonanteDto,
  ConsentimientoRegistroResultadoDto,
} from "@/lib/contratos/s6";

export function ConsentimientosVista({
  usuario,
  consentimientosIniciales,
  periodoActual,
  errorCarga,
}: {
  usuario: UsuarioSesionPublico;
  consentimientosIniciales: ConsentimientoDonanteDto[];
  periodoActual: string | null;
  errorCarga?: string | null;
}) {
  const [lista, setLista] = useState(consentimientosIniciales);
  const [donanteId, setDonanteId] = useState("");
  const [confirmaIrrevocable, setConfirmaIrrevocable] = useState(false);
  const [guardando, setGuardando] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [resultadoMsg, setResultadoMsg] = useState<string | null>(null);

  async function otorgarConsentimiento(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrorMsg(null);
    setResultadoMsg(null);

    if (!confirmaIrrevocable) {
      setErrorMsg("Debes confirmar que comprendes que el consentimiento es irrevocable.");
      return;
    }

    setGuardando(true);
    try {
      const resp = await llamarApiBff<ConsentimientoRegistroResultadoDto>(
        "/api/v1/consentimientos-donantes",
        {
          method: "POST",
          body: {
            donanteId: donanteId.trim(),
            aceptaPresentacionIndividual: true,
          },
        }
      );

      if (resp.consentimiento) {
        const nuevo = resp.consentimiento;
        setLista((prev) =>
          prev.some((c) => c.id === nuevo.id) ? prev : [nuevo, ...prev]
        );
      }
      setDonanteId("");
      setConfirmaIrrevocable(false);
      setResultadoMsg(
        resp.mensaje ??
          "Consentimiento registrado correctamente. Esta autorización es irrevocable (R36/A6)."
      );
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso
          ? err.message
          : "No se pudo registrar tu consentimiento."
      );
    } finally {
      setGuardando(false);
    }
  }

  return (
    <FranjaEstado
      tono="logrado"
      usuario={usuario}
      periodoActual={periodoActual}
      titulo="Consentimiento a donantes"
      estadoBadge={<Estado texto="Privacidad protegida (R36/A6)" tono="exito" />}
      subtitulo="Decide por cada donante si autorizas que se muestre tu perfil académico individual en los reportes de impacto. Tus datos socioeconómicos nunca se comparten."
    >
      <div className="flex flex-col gap-8">
        {errorCarga ? <Aviso tono="alerta">{errorCarga}</Aviso> : null}
        {errorMsg ? <Aviso tono="peligro">{errorMsg}</Aviso> : null}
        {resultadoMsg ? <Aviso tono="exito">{resultadoMsg}</Aviso> : null}

        <div className="grid grid-cols-1 gap-16 lg:grid-cols-[1fr_420px]">
          <SeccionPanel titulo="Mis consentimientos vigentes">
            {lista.length === 0 ? (
              <p className="py-6 text-[14px] text-[var(--color-text-secondary)]">
                No has otorgado consentimientos individuales todavía. Mientras no autorices a un
                donante, solo aparecerás en cifras agregadas anónimas.
              </p>
            ) : (
              <div className="divide-y divide-[var(--color-border-default)]">
                {lista.map((c) => (
                  <div
                    key={c.id}
                    className="flex flex-col justify-between gap-2 py-4 sm:flex-row sm:items-center"
                  >
                    <div>
                      <p className="text-[14px] font-semibold text-[var(--color-text-primary)]">
                        Donante {c.donanteId}
                      </p>
                      <p className="text-[12px] text-[var(--color-text-muted)]">
                        Autorizado el {formatearFechaCorta(c.aceptadoEnUtc)}
                      </p>
                    </div>
                    <Estado texto="Irrevocable y vigente" tono="exito" />
                  </div>
                ))}
              </div>
            )}
          </SeccionPanel>

          <aside>
            <SeccionPanel titulo="Autorizar presentación individual">
              <form onSubmit={otorgarConsentimiento} className="flex flex-col gap-4 py-4">
                <Aviso tono="info" titulo="Qué se comparte y qué nunca se comparte">
                  Si aceptas, el donante verá únicamente tu nombre, cédula, universidad, carrera,
                  programa y promedio académico. Nunca se mostrarán tus ingresos ni composición
                  familiar.
                </Aviso>

                <Campo
                  etiqueta="Identificador del donante"
                  placeholder="UUID del donante"
                  value={donanteId}
                  onChange={(e) => setDonanteId(e.target.value)}
                  required
                />

                <label className="flex items-start gap-3 rounded-[var(--radius-md)] border border-[var(--color-border-default)] bg-[var(--color-bg-warning-soft)] p-3 text-[13px] leading-[20px] text-[var(--color-text-warning)]">
                  <input
                    type="checkbox"
                    checked={confirmaIrrevocable}
                    onChange={(e) => setConfirmaIrrevocable(e.target.checked)}
                    className="mt-1 size-4"
                  />
                  <span>
                    Entiendo que este consentimiento es <strong>irrevocable</strong> una vez
                    confirmado (regla R36/A6).
                  </span>
                </label>

                <Boton
                  type="submit"
                  variante="primario"
                  anchoCompleto
                  cargando={guardando}
                  icono={<ShieldCheck className="size-4" aria-hidden="true" />}
                >
                  Confirmar consentimiento
                </Boton>
              </form>
            </SeccionPanel>
          </aside>
        </div>
      </div>
    </FranjaEstado>
  );
}
