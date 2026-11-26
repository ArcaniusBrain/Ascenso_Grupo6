"use client";

import { useState, type FormEvent } from "react";
import { FileBarChart2, Plus, RefreshCw } from "lucide-react";
import { FranjaEstado } from "@/components/layout/FranjaEstado";
import { Boton } from "@/components/ui/Boton";
import { Campo } from "@/components/ui/Campo";
import { CampoSelect } from "@/components/ui/CampoSelect";
import { Estado } from "@/components/ui/Estado";
import { Aviso } from "@/components/ui/Aviso";
import { Modal } from "@/components/ui/Modal";
import { FilaDato, SeccionPanel } from "@/components/ui/SeccionPanel";
import { ErrorApiAscenso, llamarApiBff } from "@/lib/bff/client";
import { formatearFechaCorta, formatearMoneda } from "@/lib/formato";
import type { UsuarioSesionPublico } from "@/lib/auth/session";
import type { DonanteResumenDto, PeriodoResumenDto } from "@/lib/contratos/s2";
import type { SolicitudReporteImpactoDto } from "@/lib/contratos/s6";

export function ReportesImpactoVista({
  usuario,
  reportesIniciales,
  donantes,
  periodos,
  periodoActual,
  errorCarga,
}: {
  usuario: UsuarioSesionPublico;
  reportesIniciales: SolicitudReporteImpactoDto[];
  donantes: DonanteResumenDto[];
  periodos: PeriodoResumenDto[];
  periodoActual: string | null;
  errorCarga?: string | null;
}) {
  const [reportes, setReportes] = useState(reportesIniciales);
  const [seleccionado, setSeleccionado] = useState<SolicitudReporteImpactoDto | null>(
    reportesIniciales[0] ?? null
  );

  const [modalGenerar, setModalGenerar] = useState(false);
  const [donanteId, setDonanteId] = useState(donantes[0]?.id ?? "");
  const [codigoPeriodo, setCodigoPeriodo] = useState(
    periodoActual ?? periodos[0]?.codigo ?? ""
  );

  const [enviando, setEnviando] = useState(false);
  const [refrescando, setRefrescando] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [avisoEstado, setAvisoEstado] = useState<string | null>(null);

  async function consultarReporte(reporteId: string) {
    setRefrescando(true);
    try {
      const actualizado = await llamarApiBff<SolicitudReporteImpactoDto>(
        `/api/v1/reportes/impacto/${reporteId}`
      );
      setSeleccionado(actualizado);
      setReportes((prev) =>
        prev.some((r) => r.reporteId === actualizado.reporteId)
          ? prev.map((r) => (r.reporteId === actualizado.reporteId ? actualizado : r))
          : [actualizado, ...prev]
      );
      return actualizado;
    } finally {
      setRefrescando(false);
    }
  }

  async function solicitarReporte(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrorMsg(null);
    setAvisoEstado(null);
    setEnviando(true);
    try {
      const creado = await llamarApiBff<SolicitudReporteImpactoDto>("/api/v1/reportes/impacto", {
        method: "POST",
        body: { donanteId, codigoPeriodo: codigoPeriodo.trim() },
      });
      setModalGenerar(false);
      setReportes((prev) => [creado, ...prev]);
      setSeleccionado(creado);
      setAvisoEstado(creado.mensajeEstado ?? "El reporte se está preparando.");

      // Sondeo breve no bloqueante para reflejar el paso de PREPARANDO -> LISTO del worker en S6
      for (let intento = 0; intento < 6; intento++) {
        await new Promise((res) => setTimeout(res, 500));
        const estadoActual = await consultarReporte(creado.reporteId);
        if (estadoActual.estado !== "PREPARANDO") {
          setAvisoEstado(null);
          break;
        }
      }
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso
          ? err.message
          : "No se pudo solicitar el reporte de impacto."
      );
    } finally {
      setEnviando(false);
    }
  }

  function tonoPorEstado(estado: string): "exito" | "acento" | "peligro" {
    if (estado === "LISTO") return "exito";
    if (estado === "FALLIDO") return "peligro";
    return "acento";
  }

  return (
    <FranjaEstado
      tono={seleccionado?.estado === "LISTO" ? "logrado" : "proceso"}
      usuario={usuario}
      periodoActual={periodoActual}
      titulo="Reportes de impacto"
      estadoBadge={<Estado texto="Anonimización por diseño (RF-33)" tono="acento" />}
      subtitulo="Genera reportes asíncronos por donante y periodo. Nunca se exponen datos socioeconómicos; los becarios individuales solo aparecen con consentimiento explícito."
      acciones={
        <Boton
          variante="primarioSobreColor"
          icono={<Plus className="size-4" aria-hidden="true" />}
          onClick={() => {
            setErrorMsg(null);
            setModalGenerar(true);
          }}
        >
          Generar reporte
        </Boton>
      }
    >
      <div className="flex flex-col gap-8">
        {errorCarga ? <Aviso tono="alerta">{errorCarga}</Aviso> : null}
        {errorMsg ? <Aviso tono="peligro">{errorMsg}</Aviso> : null}
        {avisoEstado ? <Aviso tono="info">{avisoEstado}</Aviso> : null}

        <div className="grid grid-cols-1 gap-16 lg:grid-cols-[360px_1fr]">
          <SeccionPanel titulo="Solicitudes">
            {reportes.length === 0 ? (
              <p className="py-6 text-[14px] text-[var(--color-text-secondary)]">
                Todavía no hay reportes generados en S6.
              </p>
            ) : (
              <div className="divide-y divide-[var(--color-border-default)]">
                {reportes.map((r) => {
                  const activo = seleccionado?.reporteId === r.reporteId;
                  return (
                    <button
                      key={r.reporteId}
                      type="button"
                      onClick={() => consultarReporte(r.reporteId)}
                      className={`ascenso-focus flex w-full cursor-pointer flex-col gap-1.5 py-4 text-left ${
                        activo
                          ? "-mx-3 rounded-[var(--radius-md)] bg-[var(--color-bg-accent-soft)] px-3"
                          : "hover:bg-[var(--color-bg-muted)]"
                      }`}
                    >
                      <div className="flex items-center justify-between gap-2">
                        <span className="text-[14px] font-semibold text-[var(--color-text-primary)]">
                          {r.nombreDonante}
                        </span>
                        <Estado texto={r.estado} tono={tonoPorEstado(r.estado)} />
                      </div>
                      <span className="text-[12px] text-[var(--color-text-muted)]">
                        Periodo {r.codigoPeriodo} · {formatearFechaCorta(r.solicitadoEnUtc)}
                      </span>
                    </button>
                  );
                })}
              </div>
            )}
          </SeccionPanel>

          <div className="flex flex-col gap-8">
            {!seleccionado ? (
              <SeccionPanel titulo="Vista del reporte">
                <div className="flex items-center gap-3 py-8 text-[14px] text-[var(--color-text-secondary)]">
                  <FileBarChart2 className="size-5 text-[var(--color-icon-muted)]" aria-hidden="true" />
                  <span>Selecciona un reporte o genera uno nuevo para visualizar el impacto.</span>
                </div>
              </SeccionPanel>
            ) : seleccionado.estado === "PREPARANDO" ? (
              <SeccionPanel
                titulo={`${seleccionado.nombreDonante} · ${seleccionado.codigoPeriodo}`}
                accion={
                  <Boton
                    variante="secundario"
                    icono={<RefreshCw className="size-4" aria-hidden="true" />}
                    cargando={refrescando}
                    onClick={() => consultarReporte(seleccionado.reporteId)}
                  >
                    Actualizar estado
                  </Boton>
                }
              >
                <div className="py-6">
                  <Aviso tono="info" titulo="Preparación asíncrona en curso">
                    {seleccionado.mensajeEstado ?? "El reporte se está preparando."}
                  </Aviso>
                </div>
              </SeccionPanel>
            ) : seleccionado.estado === "FALLIDO" ? (
              <SeccionPanel titulo={`${seleccionado.nombreDonante} · ${seleccionado.codigoPeriodo}`}>
                <div className="py-6">
                  <Aviso tono="peligro" titulo="No se pudo completar el reporte">
                    {seleccionado.mensajeEstado ?? "Ocurrió un error al preparar el reporte."}
                  </Aviso>
                </div>
              </SeccionPanel>
            ) : seleccionado.contenido ? (
              <>
                <SeccionPanel
                  titulo={`${seleccionado.contenido.nombreDonante} · ${seleccionado.contenido.codigoPeriodo}`}
                  accion={<Estado texto="Anonimizado y listo" tono="exito" />}
                >
                  <div className="grid grid-cols-1 gap-4 py-5 sm:grid-cols-3">
                    <div className="rounded-[var(--radius-md)] border border-[var(--color-border-default)] p-4">
                      <p className="text-[12px] text-[var(--color-text-muted)]">Becarios únicos</p>
                      <p className="font-display text-[32px] font-bold text-[var(--color-text-primary)]">
                        {seleccionado.contenido.totalBecariosUnicos}
                      </p>
                    </div>
                    <div className="rounded-[var(--radius-md)] border border-[var(--color-border-default)] p-4">
                      <p className="text-[12px] text-[var(--color-text-muted)]">
                        Aporte del donante
                      </p>
                      <p className="font-display text-[32px] font-bold text-[var(--color-text-primary)]">
                        {formatearMoneda(
                          seleccionado.contenido.totalAportadoDonante,
                          seleccionado.contenido.moneda
                        )}
                      </p>
                    </div>
                    <div className="rounded-[var(--radius-md)] border border-[var(--color-border-default)] p-4">
                      <p className="text-[12px] text-[var(--color-text-muted)]">
                        Desembolsado confirmado
                      </p>
                      <p className="font-display text-[32px] font-bold text-[var(--color-text-success)]">
                        {formatearMoneda(
                          seleccionado.contenido.totalDesembolsadoConfirmado,
                          seleccionado.contenido.moneda
                        )}
                      </p>
                    </div>
                  </div>
                </SeccionPanel>

                {seleccionado.contenido.programas.map((prog) => (
                  <SeccionPanel key={prog.programaId} titulo={prog.nombrePrograma}>
                    <div className="flex flex-col gap-6 py-4">
                      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                        <div>
                          <p className="mb-2 text-[13px] font-semibold text-[var(--color-text-primary)]">
                            Perfil general agregado ({prog.numeroBecarios} becarios)
                          </p>
                          <FilaDato
                            etiqueta="Universidades"
                            valor={
                              prog.perfilGeneral.porUniversidad
                                .map((i) => `${i.etiqueta} (${i.cantidad})`)
                                .join(", ") || "—"
                            }
                            conBorde
                          />
                          <FilaDato
                            etiqueta="Carreras"
                            valor={
                              prog.perfilGeneral.porCarrera
                                .map((i) => `${i.etiqueta} (${i.cantidad})`)
                                .join(", ") || "—"
                            }
                            conBorde
                          />
                        </div>

                        <div>
                          <p className="mb-2 text-[13px] font-semibold text-[var(--color-text-primary)]">
                            Resultados académicos agregados
                          </p>
                          {prog.resultadosAcademicos.mensajeSinDatos ? (
                            <p className="py-2 text-[13px] text-[var(--color-text-secondary)]">
                              {prog.resultadosAcademicos.mensajeSinDatos}
                            </p>
                          ) : (
                            <>
                              <FilaDato
                                etiqueta="Promedio general"
                                valor={
                                  prog.resultadosAcademicos.promedioGeneral !== null
                                    ? String(prog.resultadosAcademicos.promedioGeneral)
                                    : "—"
                                }
                                conBorde
                              />
                              <FilaDato
                                etiqueta="Materias aprobadas / reprobadas"
                                valor={`${prog.resultadosAcademicos.totalMateriasAprobadas} / ${prog.resultadosAcademicos.totalMateriasReprobadas}`}
                                conBorde
                              />
                            </>
                          )}
                        </div>
                      </div>

                      <div>
                        <p className="mb-2 text-[13px] font-semibold text-[var(--color-text-primary)]">
                          Becarios presentados (solo con consentimiento explícito)
                        </p>
                        {prog.becariosPresentados.length === 0 ? (
                          <p className="text-[13px] text-[var(--color-text-muted)]">
                            Ningún becario ha otorgado consentimiento individual para este donante;
                            todos permanecen anonimizados en las cifras agregadas.
                          </p>
                        ) : (
                          <div className="divide-y divide-[var(--color-border-default)] rounded-[var(--radius-md)] border border-[var(--color-border-default)] px-4">
                            {prog.becariosPresentados.map((b) => (
                              <div
                                key={`${b.cedula}-${b.nombreCompleto}`}
                                className="flex flex-col justify-between gap-2 py-3 text-[13px] sm:flex-row sm:items-center"
                              >
                                <div>
                                  <p className="font-semibold text-[var(--color-text-primary)]">
                                    {b.nombreCompleto}{" "}
                                    <span className="font-normal text-[var(--color-text-muted)]">
                                      ({b.cedula})
                                    </span>
                                  </p>
                                  <p className="text-[var(--color-text-secondary)]">
                                    {b.universidad} · {b.carrera}
                                  </p>
                                </div>
                                <Estado
                                  texto={
                                    b.promedioAcademico !== null
                                      ? `Promedio ${b.promedioAcademico}`
                                      : "Sin promedio aún"
                                  }
                                  tono="exito"
                                />
                              </div>
                            ))}
                          </div>
                        )}
                      </div>
                    </div>
                  </SeccionPanel>
                ))}
              </>
            ) : null}
          </div>
        </div>
      </div>

      <Modal
        abierto={modalGenerar}
        onCerrar={() => setModalGenerar(false)}
        titulo="Solicitar reporte de impacto"
        descripcion="Se genera en segundo plano sin bloquear la operación diaria (HU-33.1)."
      >
        <form onSubmit={solicitarReporte} className="flex flex-col gap-4">
          {donantes.length > 0 ? (
            <CampoSelect
              etiqueta="Donante"
              value={donanteId}
              onChange={(e) => setDonanteId(e.target.value)}
              opciones={donantes.map((d) => ({ valor: d.id, etiqueta: d.nombre }))}
              required
            />
          ) : (
            <Campo
              etiqueta="ID del donante"
              placeholder="UUID del donante"
              value={donanteId}
              onChange={(e) => setDonanteId(e.target.value)}
              required
            />
          )}

          {periodos.length > 0 ? (
            <CampoSelect
              etiqueta="Periodo"
              value={codigoPeriodo}
              onChange={(e) => setCodigoPeriodo(e.target.value)}
              opciones={periodos.map((p) => ({ valor: p.codigo, etiqueta: p.codigo }))}
              required
            />
          ) : (
            <Campo
              etiqueta="Periodo"
              placeholder="2026-1"
              value={codigoPeriodo}
              onChange={(e) => setCodigoPeriodo(e.target.value)}
              required
            />
          )}

          <div className="mt-2 flex justify-end gap-3">
            <Boton variante="secundario" onClick={() => setModalGenerar(false)}>
              Cancelar
            </Boton>
            <Boton type="submit" variante="primario" cargando={enviando}>
              Solicitar reporte
            </Boton>
          </div>
        </form>
      </Modal>
    </FranjaEstado>
  );
}
