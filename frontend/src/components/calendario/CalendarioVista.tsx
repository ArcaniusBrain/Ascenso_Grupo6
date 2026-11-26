"use client";

import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import { CalendarCheck2, Plus } from "lucide-react";
import { FranjaEstado } from "@/components/layout/FranjaEstado";
import { Boton } from "@/components/ui/Boton";
import { Campo } from "@/components/ui/Campo";
import { Estado } from "@/components/ui/Estado";
import { Aviso } from "@/components/ui/Aviso";
import { Modal } from "@/components/ui/Modal";
import { FilaDato, SeccionPanel } from "@/components/ui/SeccionPanel";
import { ErrorApiAscenso, llamarApiBff } from "@/lib/bff/client";
import { formatearFechaCorta } from "@/lib/formato";
import type { UsuarioSesionPublico } from "@/lib/auth/session";
import type { ListaPeriodosDto, PeriodoDetalleDto } from "@/lib/contratos/s2";

export function CalendarioVista({
  usuario,
  calendarioInicial,
  errorCarga,
}: {
  usuario: UsuarioSesionPublico;
  calendarioInicial: ListaPeriodosDto | null;
  errorCarga?: string | null;
}) {
  const router = useRouter();
  const [calendario, setCalendario] = useState(calendarioInicial);
  const [detalle, setDetalle] = useState<PeriodoDetalleDto | null>(null);
  const [cargandoDetalle, setCargandoDetalle] = useState(false);

  const [modalAbrir, setModalAbrir] = useState(false);
  const [codigoNuevo, setCodigoNuevo] = useState("");
  const [procesando, setProcesando] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [exitoMsg, setExitoMsg] = useState<string | null>(null);

  async function recargarCalendario() {
    const lista = await llamarApiBff<ListaPeriodosDto>("/api/v1/periodos");
    setCalendario(lista);
    router.refresh();
  }

  async function verDetalle(codigo: string) {
    setCargandoDetalle(true);
    setErrorMsg(null);
    try {
      const det = await llamarApiBff<PeriodoDetalleDto>(
        `/api/v1/periodos/${encodeURIComponent(codigo)}`
      );
      setDetalle(det);
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo consultar el periodo."
      );
    } finally {
      setCargandoDetalle(false);
    }
  }

  async function abrirPeriodo(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrorMsg(null);
    setProcesando(true);
    try {
      await llamarApiBff("/api/v1/periodos", {
        method: "POST",
        body: { codigoPeriodo: codigoNuevo.trim() },
      });
      setModalAbrir(false);
      setCodigoNuevo("");
      setExitoMsg(`Periodo ${codigoNuevo.trim()} abierto correctamente.`);
      await recargarCalendario();
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo abrir el periodo."
      );
    } finally {
      setProcesando(false);
    }
  }

  async function marcarComoActual(codigo: string) {
    setErrorMsg(null);
    setExitoMsg(null);
    setProcesando(true);
    try {
      await llamarApiBff("/api/v1/periodos/actual", {
        method: "POST",
        body: {
          codigoPeriodo: codigo,
          operationId: crypto.randomUUID(),
        },
      });
      setExitoMsg(
        `Periodo ${codigo} marcado como actual. Evento PeriodoMarcadoActual.v1 emitido al Outbox.`
      );
      await recargarCalendario();
      await verDetalle(codigo);
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso
          ? err.message
          : "No se pudo marcar el periodo como actual."
      );
    } finally {
      setProcesando(false);
    }
  }

  async function cerrarPeriodo(codigo: string) {
    setErrorMsg(null);
    setExitoMsg(null);
    setProcesando(true);
    try {
      await llamarApiBff(`/api/v1/periodos/${encodeURIComponent(codigo)}/cerrar`, {
        method: "POST",
      });
      setExitoMsg(`Periodo ${codigo} cerrado.`);
      await recargarCalendario();
      await verDetalle(codigo);
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo cerrar el periodo."
      );
    } finally {
      setProcesando(false);
    }
  }

  const periodos = calendario?.periodos ?? [];

  return (
    <FranjaEstado
      tono={calendario?.periodoActual ? "logrado" : "inicio"}
      usuario={usuario}
      periodoActual={calendario?.periodoActual ?? null}
      titulo="Calendario académico"
      estadoBadge={
        calendario?.periodoActual ? (
          <Estado texto={`Periodo actual: ${calendario.periodoActual}`} tono="exito" />
        ) : (
          <Estado texto="Sin periodo actual" tono="alerta" />
        )
      }
      subtitulo="Abre o cierra periodos académicos y define el periodo actual que gobierna los flujos de toda la plataforma (RF-38)."
      acciones={
        <Boton
          variante="primarioSobreColor"
          icono={<Plus className="size-4" aria-hidden="true" />}
          onClick={() => {
            setErrorMsg(null);
            setModalAbrir(true);
          }}
        >
          Abrir periodo
        </Boton>
      }
    >
      <div className="flex flex-col gap-8">
        {errorCarga ? <Aviso tono="alerta">{errorCarga}</Aviso> : null}
        {errorMsg ? <Aviso tono="peligro">{errorMsg}</Aviso> : null}
        {exitoMsg ? <Aviso tono="exito">{exitoMsg}</Aviso> : null}

        <div className="grid grid-cols-1 gap-16 lg:grid-cols-[1fr_420px]">
          <SeccionPanel titulo="Periodos registrados">
            {periodos.length === 0 ? (
              <p className="py-8 text-[14px] text-[var(--color-text-secondary)]">
                {calendario?.mensaje ?? "Aún no hay periodos académicos registrados."}
              </p>
            ) : (
              <div className="divide-y divide-[var(--color-border-default)]">
                {periodos.map((p) => (
                  <div
                    key={p.codigo}
                    className="flex flex-col justify-between gap-4 py-4 sm:flex-row sm:items-center"
                  >
                    <div className="flex flex-wrap items-center gap-3">
                      <span className="font-display text-[22px] font-semibold text-[var(--color-text-primary)]">
                        {p.codigo}
                      </span>
                      <Estado
                        texto={p.estado === "ABIERTO" ? "Abierto" : "Cerrado"}
                        tono={p.estado === "ABIERTO" ? "info" : "neutro"}
                      />
                      {p.esActual ? <Estado texto="Actual" tono="exito" /> : null}
                      <span className="text-[12px] text-[var(--color-text-muted)]">
                        {formatearFechaCorta(p.registradoEnUtc)}
                      </span>
                    </div>

                    <div className="flex flex-wrap items-center gap-2">
                      {!p.esActual && p.estado === "ABIERTO" ? (
                        <Boton
                          variante="terciario"
                          icono={<CalendarCheck2 className="size-4" aria-hidden="true" />}
                          disabled={procesando}
                          onClick={() => marcarComoActual(p.codigo)}
                          className="h-[38px] px-3 text-[13px]"
                        >
                          Marcar actual
                        </Boton>
                      ) : null}
                      {p.estado === "ABIERTO" && !p.esActual ? (
                        <Boton
                          variante="secundario"
                          disabled={procesando}
                          onClick={() => cerrarPeriodo(p.codigo)}
                          className="h-[38px] px-3 text-[13px]"
                        >
                          Cerrar
                        </Boton>
                      ) : null}
                      <Boton
                        variante="secundario"
                        onClick={() => verDetalle(p.codigo)}
                        className="h-[38px] px-3 text-[13px]"
                      >
                        Detalle
                      </Boton>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </SeccionPanel>

          <aside>
            <SeccionPanel titulo="Detalle del periodo">
              {!detalle ? (
                <p className="py-6 text-[14px] text-[var(--color-text-secondary)]">
                  Selecciona «Detalle» en un periodo para revisar sus elementos ligados y las
                  fechas de inicio de inscripciones por universidad.
                </p>
              ) : cargandoDetalle ? (
                <p className="py-6 text-[14px] text-[var(--color-text-muted)]">Cargando...</p>
              ) : (
                <div className="flex flex-col gap-4 py-3">
                  <FilaDato etiqueta="Código" valor={detalle.codigo} conBorde />
                  <FilaDato etiqueta="Estado" valor={detalle.estado} conBorde />
                  <FilaDato
                    etiqueta="Elementos ligados"
                    valor={`${detalle.cantidadElementosLigados}`}
                    conBorde
                  />
                  {detalle.mensajeSinElementos ? (
                    <p className="text-[13px] text-[var(--color-text-muted)]">
                      {detalle.mensajeSinElementos}
                    </p>
                  ) : null}

                  <div className="pt-2">
                    <h3 className="font-display text-[16px] font-semibold text-[var(--color-text-primary)]">
                      Inicios de inscripciones
                    </h3>
                    {detalle.iniciosInscripciones.length === 0 ? (
                      <p className="mt-2 text-[13px] text-[var(--color-text-secondary)]">
                        Sin fechas registradas en este periodo.
                      </p>
                    ) : (
                      <div className="mt-2 divide-y divide-[var(--color-border-default)]">
                        {detalle.iniciosInscripciones.map((ini) => (
                          <div
                            key={ini.id}
                            className="flex items-center justify-between gap-2 py-2.5 text-[13px]"
                          >
                            <span className="font-medium text-[var(--color-text-primary)]">
                              {ini.nombreUniversidad}
                            </span>
                            <span className="text-[var(--color-text-secondary)]">
                              {ini.fechaInicio}
                            </span>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                </div>
              )}
            </SeccionPanel>
          </aside>
        </div>
      </div>

      <Modal
        abierto={modalAbrir}
        onCerrar={() => setModalAbrir(false)}
        titulo="Abrir nuevo periodo académico"
        descripcion="Usa el formato estándar AAAA-N (por ejemplo 2026-1 o 2026-2)."
      >
        <form onSubmit={abrirPeriodo} className="flex flex-col gap-4">
          <Campo
            etiqueta="Código de periodo"
            placeholder="2026-2"
            value={codigoNuevo}
            onChange={(e) => setCodigoNuevo(e.target.value)}
            required
          />
          <div className="flex justify-end gap-3">
            <Boton variante="secundario" onClick={() => setModalAbrir(false)}>
              Cancelar
            </Boton>
            <Boton type="submit" variante="primario" cargando={procesando}>
              Abrir periodo
            </Boton>
          </div>
        </form>
      </Modal>
    </FranjaEstado>
  );
}
