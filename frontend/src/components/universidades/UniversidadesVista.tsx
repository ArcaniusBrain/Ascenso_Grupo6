"use client";

import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import { Plus, SlidersHorizontal } from "lucide-react";
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
import type {
  CostoMatriculaPeriodoDto,
  InicioInscripcionesUniversidadDto,
  PeriodoResumenDto,
  UniversidadDetalleDto,
  UniversidadResumenDto,
} from "@/lib/contratos/s2";

export function UniversidadesVista({
  usuario,
  universidadesIniciales,
  periodos,
  periodoActual,
  errorCarga,
}: {
  usuario: UsuarioSesionPublico;
  universidadesIniciales: UniversidadResumenDto[];
  periodos: PeriodoResumenDto[];
  periodoActual: string | null;
  errorCarga?: string | null;
}) {
  const router = useRouter();
  const [universidades, setUniversidades] = useState(universidadesIniciales);
  const [detalle, setDetalle] = useState<UniversidadDetalleDto | null>(null);
  const [periodoConsulta, setPeriodoConsulta] = useState(
    periodoActual ?? periodos[0]?.codigo ?? ""
  );
  const [costoPeriodo, setCostoPeriodo] = useState<CostoMatriculaPeriodoDto | null>(null);
  const [inicioPeriodo, setInicioPeriodo] = useState<InicioInscripcionesUniversidadDto | null>(null);
  const [cargandoDetalle, setCargandoDetalle] = useState(false);

  // Modales
  const [modalNueva, setModalNueva] = useState(false);
  const [modalEditar, setModalEditar] = useState(false);
  const [modalCarrera, setModalCarrera] = useState(false);
  const [modalCosto, setModalCosto] = useState(false);
  const [modalInicio, setModalInicio] = useState(false);

  // Campos universidad
  const [uNombre, setUNombre] = useState("");
  const [uTerminos, setUTerminos] = useState("");
  const [uVia, setUVia] = useState<"API" | "MANUAL">("API");
  const [uContactoNombre, setUContactoNombre] = useState("");
  const [uContactoCorreo, setUContactoCorreo] = useState("");
  const [uContactoTelefono, setUContactoTelefono] = useState("");

  // Campos carrera / costo / inicio inscripciones
  const [carreraNombre, setCarreraNombre] = useState("");
  const [costoMonto, setCostoMonto] = useState("");
  const [costoMoneda, setCostoMoneda] = useState("USD");
  const [costoCodPeriodo, setCostoCodPeriodo] = useState(
    periodoActual ?? periodos[0]?.codigo ?? ""
  );
  const [fechaInicio, setFechaInicio] = useState("");
  const [inicioCodPeriodo, setInicioCodPeriodo] = useState(
    periodoActual ?? periodos[0]?.codigo ?? ""
  );

  const [guardando, setGuardando] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [exitoMsg, setExitoMsg] = useState<string | null>(null);

  async function cargarDetalleUniversidad(universidadId: string, codPeriodo: string) {
    setCargandoDetalle(true);
    setErrorMsg(null);
    try {
      const det = await llamarApiBff<UniversidadDetalleDto>(
        `/api/v1/universidades/${universidadId}`
      );
      setDetalle(det);

      if (codPeriodo) {
        const [cos, ini] = await Promise.all([
          llamarApiBff<CostoMatriculaPeriodoDto>(
            `/api/v1/universidades/${universidadId}/costos-matricula/${encodeURIComponent(codPeriodo)}`
          ).catch(() => null),
          llamarApiBff<InicioInscripcionesUniversidadDto>(
            `/api/v1/universidades/${universidadId}/inicios-inscripciones/${encodeURIComponent(codPeriodo)}`
          ).catch(() => null),
        ]);
        setCostoPeriodo(cos);
        setInicioPeriodo(ini);
      } else {
        setCostoPeriodo(null);
        setInicioPeriodo(null);
      }
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo consultar la universidad."
      );
    } finally {
      setCargandoDetalle(false);
    }
  }

  function abrirModalNueva() {
    setErrorMsg(null);
    setUNombre("");
    setUTerminos("");
    setUVia("API");
    setUContactoNombre("");
    setUContactoCorreo("");
    setUContactoTelefono("");
    setModalNueva(true);
  }

  function abrirModalEditar() {
    if (!detalle) return;
    setErrorMsg(null);
    setUNombre(detalle.nombre);
    setUTerminos(detalle.terminosConvenio);
    setUVia(detalle.viaDatosAcademicos === "MANUAL" ? "MANUAL" : "API");
    setUContactoNombre(detalle.contactoDesignado.nombreCompleto);
    setUContactoCorreo(detalle.contactoDesignado.correo ?? "");
    setUContactoTelefono(detalle.contactoDesignado.telefono ?? "");
    setModalEditar(true);
  }

  async function registrarUniversidad(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrorMsg(null);
    setGuardando(true);
    try {
      const creada = await llamarApiBff<UniversidadResumenDto>("/api/v1/universidades", {
        method: "POST",
        body: {
          nombre: uNombre.trim(),
          terminosConvenio: uTerminos.trim(),
          viaDatosAcademicos: uVia,
          contactoDesignado: {
            nombreCompleto: uContactoNombre.trim(),
            correo: uContactoCorreo.trim() || null,
            telefono: uContactoTelefono.trim() || null,
          },
        },
      });
      setUniversidades((prev) => [...prev, creada]);
      setModalNueva(false);
      setExitoMsg(`Universidad «${creada.nombre}» registrada.`);
      await cargarDetalleUniversidad(creada.id, periodoConsulta);
      router.refresh();
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo registrar la universidad."
      );
    } finally {
      setGuardando(false);
    }
  }

  async function actualizarConvenio(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!detalle) return;
    setErrorMsg(null);
    setGuardando(true);
    try {
      const act = await llamarApiBff<UniversidadResumenDto>(
        `/api/v1/universidades/${detalle.id}/convenio`,
        {
          method: "PUT",
          body: {
            nombre: uNombre.trim(),
            terminosConvenio: uTerminos.trim(),
            viaDatosAcademicos: uVia,
            contactoDesignado: {
              nombreCompleto: uContactoNombre.trim(),
              correo: uContactoCorreo.trim() || null,
              telefono: uContactoTelefono.trim() || null,
            },
          },
        }
      );
      setUniversidades((prev) => prev.map((u) => (u.id === act.id ? act : u)));
      setModalEditar(false);
      setExitoMsg("Convenio actualizado.");
      await cargarDetalleUniversidad(act.id, periodoConsulta);
      router.refresh();
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo actualizar el convenio."
      );
    } finally {
      setGuardando(false);
    }
  }

  async function agregarCarrera(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!detalle) return;
    setErrorMsg(null);
    setGuardando(true);
    try {
      await llamarApiBff(`/api/v1/universidades/${detalle.id}/carreras`, {
        method: "POST",
        body: { nombre: carreraNombre.trim() },
      });
      setCarreraNombre("");
      setModalCarrera(false);
      setExitoMsg("Carrera elegible agregada.");
      await cargarDetalleUniversidad(detalle.id, periodoConsulta);
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo agregar la carrera."
      );
    } finally {
      setGuardando(false);
    }
  }

  async function registrarCosto(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!detalle) return;
    setErrorMsg(null);
    setGuardando(true);
    try {
      await llamarApiBff(`/api/v1/universidades/${detalle.id}/costos-matricula`, {
        method: "POST",
        body: {
          codigoPeriodo: costoCodPeriodo.trim(),
          monto: Number(costoMonto),
          moneda: costoMoneda.trim().toUpperCase(),
        },
      });
      setCostoMonto("");
      setModalCosto(false);
      setPeriodoConsulta(costoCodPeriodo.trim());
      setExitoMsg("Costo de matrícula registrado en el historial append-only.");
      await cargarDetalleUniversidad(detalle.id, costoCodPeriodo.trim());
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo registrar el costo."
      );
    } finally {
      setGuardando(false);
    }
  }

  async function registrarInicioInscripciones(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!detalle) return;
    setErrorMsg(null);
    setGuardando(true);
    try {
      await llamarApiBff(`/api/v1/universidades/${detalle.id}/inicios-inscripciones`, {
        method: "POST",
        body: {
          codigoPeriodo: inicioCodPeriodo.trim(),
          fechaInicio,
        },
      });
      setModalInicio(false);
      setPeriodoConsulta(inicioCodPeriodo.trim());
      setExitoMsg("Fecha de inicio de inscripciones registrada (UTC-4).");
      await cargarDetalleUniversidad(detalle.id, inicioCodPeriodo.trim());
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso
          ? err.message
          : "No se pudo registrar la fecha de inicio de inscripciones."
      );
    } finally {
      setGuardando(false);
    }
  }

  return (
    <FranjaEstado
      tono="inicio"
      usuario={usuario}
      periodoActual={periodoActual}
      titulo="Universidades aliadas"
      estadoBadge={<Estado texto={`${universidades.length} convenio(s)`} tono="info" />}
      subtitulo="Administra convenios, vías de recepción académica, carreras elegibles, costos de matrícula históricos e inicios de inscripciones (RF-14)."
      acciones={
        <Boton
          variante="primarioSobreColor"
          icono={<Plus className="size-4" aria-hidden="true" />}
          onClick={abrirModalNueva}
        >
          Registrar universidad
        </Boton>
      }
    >
      <div className="flex flex-col gap-8">
        {errorCarga ? <Aviso tono="alerta">{errorCarga}</Aviso> : null}
        {errorMsg ? <Aviso tono="peligro">{errorMsg}</Aviso> : null}
        {exitoMsg ? <Aviso tono="exito">{exitoMsg}</Aviso> : null}

        <div className="grid grid-cols-1 gap-16 lg:grid-cols-[380px_1fr]">
          <SeccionPanel titulo="Convenios">
            {universidades.length === 0 ? (
              <p className="py-6 text-[14px] text-[var(--color-text-secondary)]">
                Aún no hay universidades aliadas registradas.
              </p>
            ) : (
              <div className="divide-y divide-[var(--color-border-default)]">
                {universidades.map((u) => {
                  const seleccionada = detalle?.id === u.id;
                  return (
                    <button
                      key={u.id}
                      type="button"
                      onClick={() => cargarDetalleUniversidad(u.id, periodoConsulta)}
                      className={`ascenso-focus flex w-full cursor-pointer flex-col items-start gap-1 py-4 text-left transition-colors ${
                        seleccionada
                          ? "-mx-3 rounded-[var(--radius-md)] bg-[var(--color-bg-brand-soft)] px-3"
                          : "hover:bg-[var(--color-bg-muted)]"
                      }`}
                    >
                      <div className="flex w-full items-center justify-between gap-2">
                        <span className="text-[15px] font-semibold text-[var(--color-text-primary)]">
                          {u.nombre}
                        </span>
                        <Estado
                          texto={u.viaDatosAcademicos === "API" ? "API" : "Manual"}
                          tono={u.viaDatosAcademicos === "API" ? "acento" : "neutro"}
                        />
                      </div>
                      <span className="text-[12px] text-[var(--color-text-muted)]">
                        Contacto: {u.contactoDesignado.nombreCompleto}
                      </span>
                    </button>
                  );
                })}
              </div>
            )}
          </SeccionPanel>

          <div className="flex flex-col gap-10">
            {!detalle ? (
              <SeccionPanel titulo="Detalle del convenio">
                <p className="py-6 text-[14px] text-[var(--color-text-secondary)]">
                  Selecciona una universidad de la lista para ver sus carreras, historial de
                  costos de matrícula y fecha de inicio de inscripciones.
                </p>
              </SeccionPanel>
            ) : cargandoDetalle ? (
              <p className="py-6 text-[14px] text-[var(--color-text-muted)]">Cargando...</p>
            ) : (
              <>
                <SeccionPanel
                  titulo={detalle.nombre}
                  accion={
                    <Boton
                      variante="secundario"
                      icono={<SlidersHorizontal className="size-4" aria-hidden="true" />}
                      onClick={abrirModalEditar}
                    >
                      Editar convenio
                    </Boton>
                  }
                >
                  <div className="py-2">
                    <FilaDato
                      etiqueta="Vía de datos académicos"
                      valor={detalle.viaDatosAcademicos === "API" ? "Integración API" : "Carga manual"}
                      conBorde
                    />
                    <FilaDato
                      etiqueta="Contacto designado"
                      valor={detalle.contactoDesignado.nombreCompleto}
                      conBorde
                    />
                    <FilaDato
                      etiqueta="Correo / Teléfono"
                      valor={
                        [detalle.contactoDesignado.correo, detalle.contactoDesignado.telefono]
                          .filter(Boolean)
                          .join(" · ") || "—"
                      }
                      conBorde
                    />
                    <p className="pt-3 text-[13px] leading-[20px] text-[var(--color-text-secondary)]">
                      <strong className="text-[var(--color-text-primary)]">Términos: </strong>
                      {detalle.terminosConvenio}
                    </p>
                  </div>
                </SeccionPanel>

                <SeccionPanel
                  titulo="Carreras elegibles"
                  accion={
                    <Boton variante="terciario" onClick={() => setModalCarrera(true)}>
                      + Agregar carrera
                    </Boton>
                  }
                >
                  {detalle.carreras.length === 0 ? (
                    <p className="py-4 text-[14px] text-[var(--color-text-secondary)]">
                      {detalle.mensajeSinCarreras ?? "No hay carreras registradas todavía."}
                    </p>
                  ) : (
                    <div className="flex flex-wrap gap-2 py-4">
                      {detalle.carreras.map((c) => (
                        <Estado key={c.id} texto={c.nombre} tono="info" />
                      ))}
                    </div>
                  )}
                </SeccionPanel>

                <SeccionPanel
                  titulo="Matrícula e inscripciones por periodo"
                  accion={
                    <div className="flex gap-2">
                      <Boton variante="terciario" onClick={() => setModalCosto(true)}>
                        + Costo matrícula
                      </Boton>
                      <Boton variante="terciario" onClick={() => setModalInicio(true)}>
                        + Fecha inscripciones
                      </Boton>
                    </div>
                  }
                >
                  <div className="flex flex-col gap-5 py-4">
                    <div className="max-w-[260px]">
                      {periodos.length > 0 ? (
                        <CampoSelect
                          etiqueta="Periodo consultado"
                          value={periodoConsulta}
                          onChange={(e) => {
                            setPeriodoConsulta(e.target.value);
                            cargarDetalleUniversidad(detalle.id, e.target.value);
                          }}
                          opciones={periodos.map((p) => ({
                            valor: p.codigo,
                            etiqueta: `${p.codigo}${p.esActual ? " (Actual)" : ""}`,
                          }))}
                        />
                      ) : (
                        <Campo
                          etiqueta="Periodo consultado"
                          placeholder="2026-1"
                          value={periodoConsulta}
                          onChange={(e) => setPeriodoConsulta(e.target.value)}
                        />
                      )}
                    </div>

                    <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                      <div className="rounded-[var(--radius-md)] border border-[var(--color-border-default)] p-4">
                        <p className="text-[12px] font-medium text-[var(--color-text-muted)]">
                          Costo de matrícula vigente
                        </p>
                        {costoPeriodo?.costoVigente ? (
                          <>
                            <p className="font-display mt-1 text-[28px] font-bold text-[var(--color-text-primary)]">
                              {formatearMoneda(
                                costoPeriodo.costoVigente.monto,
                                costoPeriodo.costoVigente.moneda
                              )}
                            </p>
                            <p className="text-[12px] text-[var(--color-text-secondary)]">
                              Versión v{costoPeriodo.costoVigente.versionRegistro} ·{" "}
                              {formatearFechaCorta(costoPeriodo.costoVigente.registradoEnUtc)}
                            </p>
                            {costoPeriodo.historial.length > 1 ? (
                              <div className="mt-3 border-t border-[var(--color-border-default)] pt-2">
                                <p className="text-[11px] font-semibold text-[var(--color-text-muted)]">
                                  Historial append-only ({costoPeriodo.historial.length} versiones)
                                </p>
                                {costoPeriodo.historial.map((h) => (
                                  <p
                                    key={h.id}
                                    className="text-[12px] text-[var(--color-text-secondary)]"
                                  >
                                    v{h.versionRegistro}: {formatearMoneda(h.monto, h.moneda)} (
                                    {formatearFechaCorta(h.registradoEnUtc)})
                                  </p>
                                ))}
                              </div>
                            ) : null}
                          </>
                        ) : (
                          <p className="mt-2 text-[13px] text-[var(--color-text-secondary)]">
                            {costoPeriodo?.mensaje ?? "Sin costo registrado para este periodo."}
                          </p>
                        )}
                      </div>

                      <div className="rounded-[var(--radius-md)] border border-[var(--color-border-default)] p-4">
                        <p className="text-[12px] font-medium text-[var(--color-text-muted)]">
                          Inicio de inscripciones (HU-14.3)
                        </p>
                        {inicioPeriodo?.fechaInicio ? (
                          <>
                            <p className="font-display mt-1 text-[28px] font-bold text-[var(--color-text-primary)]">
                              {inicioPeriodo.fechaInicio}
                            </p>
                            <div className="mt-2">
                              <Estado
                                texto={
                                  inicioPeriodo.inscripcionesIniciadas
                                    ? "Inscripciones iniciadas"
                                    : "Próxima apertura"
                                }
                                tono={inicioPeriodo.inscripcionesIniciadas ? "exito" : "info"}
                              />
                            </div>
                          </>
                        ) : (
                          <p className="mt-2 text-[13px] text-[var(--color-text-secondary)]">
                            {inicioPeriodo?.mensaje ??
                              "Sin fecha de inicio de inscripciones registrada para este periodo."}
                          </p>
                        )}
                      </div>
                    </div>
                  </div>
                </SeccionPanel>
              </>
            )}
          </div>
        </div>
      </div>

      {/* Modal crear / editar universidad */}
      <Modal
        abierto={modalNueva || modalEditar}
        onCerrar={() => {
          setModalNueva(false);
          setModalEditar(false);
        }}
        titulo={modalEditar ? "Editar convenio universitario" : "Registrar universidad aliada"}
      >
        <form
          onSubmit={modalEditar ? actualizarConvenio : registrarUniversidad}
          className="flex flex-col gap-4"
        >
          <Campo
            etiqueta="Nombre de la universidad"
            placeholder="Ej. Universidad Católica Andrés Bello"
            value={uNombre}
            onChange={(e) => setUNombre(e.target.value)}
            required
          />
          <Campo
            etiqueta="Términos del convenio"
            placeholder="Resumen de acuerdos, descuentos y entregas académicas"
            value={uTerminos}
            onChange={(e) => setUTerminos(e.target.value)}
            required
          />
          <CampoSelect
            etiqueta="Vía de recepción de datos académicos"
            value={uVia}
            onChange={(e) => setUVia(e.target.value as "API" | "MANUAL")}
            opciones={[
              { valor: "API", etiqueta: "API (integración directa)" },
              { valor: "MANUAL", etiqueta: "Carga manual por enlace" },
            ]}
          />
          <Campo
            etiqueta="Contacto designado"
            placeholder="Nombre y apellido del enlace universitario"
            value={uContactoNombre}
            onChange={(e) => setUContactoNombre(e.target.value)}
            required
          />
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Campo
              etiqueta="Correo del contacto"
              type="email"
              value={uContactoCorreo}
              onChange={(e) => setUContactoCorreo(e.target.value)}
            />
            <Campo
              etiqueta="Teléfono del contacto"
              value={uContactoTelefono}
              onChange={(e) => setUContactoTelefono(e.target.value)}
            />
          </div>
          <div className="mt-2 flex justify-end gap-3">
            <Boton
              variante="secundario"
              onClick={() => {
                setModalNueva(false);
                setModalEditar(false);
              }}
            >
              Cancelar
            </Boton>
            <Boton type="submit" variante="primario" cargando={guardando}>
              Guardar
            </Boton>
          </div>
        </form>
      </Modal>

      {/* Modal agregar carrera */}
      <Modal
        abierto={modalCarrera}
        onCerrar={() => setModalCarrera(false)}
        titulo="Agregar carrera elegible"
      >
        <form onSubmit={agregarCarrera} className="flex flex-col gap-4">
          <Campo
            etiqueta="Nombre de la carrera"
            placeholder="Ej. Ingeniería Informática"
            value={carreraNombre}
            onChange={(e) => setCarreraNombre(e.target.value)}
            required
          />
          <div className="flex justify-end gap-3">
            <Boton variante="secundario" onClick={() => setModalCarrera(false)}>
              Cancelar
            </Boton>
            <Boton type="submit" variante="primario" cargando={guardando}>
              Agregar
            </Boton>
          </div>
        </form>
      </Modal>

      {/* Modal costo de matrícula */}
      <Modal
        abierto={modalCosto}
        onCerrar={() => setModalCosto(false)}
        titulo="Registrar costo de matrícula"
        descripcion="Cada corrección en el mismo periodo crea una nueva versión conservando el historial (HU-14.2)."
      >
        <form onSubmit={registrarCosto} className="flex flex-col gap-4">
          {periodos.length > 0 ? (
            <CampoSelect
              etiqueta="Periodo"
              value={costoCodPeriodo}
              onChange={(e) => setCostoCodPeriodo(e.target.value)}
              opciones={periodos.map((p) => ({ valor: p.codigo, etiqueta: p.codigo }))}
              required
            />
          ) : (
            <Campo
              etiqueta="Periodo"
              placeholder="2026-1"
              value={costoCodPeriodo}
              onChange={(e) => setCostoCodPeriodo(e.target.value)}
              required
            />
          )}
          <div className="grid grid-cols-2 gap-4">
            <Campo
              etiqueta="Monto"
              type="number"
              min={1}
              step="0.01"
              value={costoMonto}
              onChange={(e) => setCostoMonto(e.target.value)}
              required
            />
            <Campo
              etiqueta="Moneda"
              maxLength={3}
              value={costoMoneda}
              onChange={(e) => setCostoMoneda(e.target.value)}
              required
            />
          </div>
          <div className="flex justify-end gap-3">
            <Boton variante="secundario" onClick={() => setModalCosto(false)}>
              Cancelar
            </Boton>
            <Boton type="submit" variante="primario" cargando={guardando}>
              Registrar costo
            </Boton>
          </div>
        </form>
      </Modal>

      {/* Modal inicio de inscripciones */}
      <Modal
        abierto={modalInicio}
        onCerrar={() => setModalInicio(false)}
        titulo="Registrar inicio de inscripciones"
        descripcion="Fecha evaluada en hora de Caracas (UTC-4) para alertas de desembolso (HU-14.3)."
      >
        <form onSubmit={registrarInicioInscripciones} className="flex flex-col gap-4">
          {periodos.length > 0 ? (
            <CampoSelect
              etiqueta="Periodo"
              value={inicioCodPeriodo}
              onChange={(e) => setInicioCodPeriodo(e.target.value)}
              opciones={periodos.map((p) => ({ valor: p.codigo, etiqueta: p.codigo }))}
              required
            />
          ) : (
            <Campo
              etiqueta="Periodo"
              placeholder="2026-1"
              value={inicioCodPeriodo}
              onChange={(e) => setInicioCodPeriodo(e.target.value)}
              required
            />
          )}
          <Campo
            etiqueta="Fecha de inicio"
            type="date"
            value={fechaInicio}
            onChange={(e) => setFechaInicio(e.target.value)}
            required
          />
          <div className="flex justify-end gap-3">
            <Boton variante="secundario" onClick={() => setModalInicio(false)}>
              Cancelar
            </Boton>
            <Boton type="submit" variante="primario" cargando={guardando}>
              Guardar fecha
            </Boton>
          </div>
        </form>
      </Modal>
    </FranjaEstado>
  );
}
