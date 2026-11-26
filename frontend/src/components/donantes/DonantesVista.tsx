"use client";

import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import { Plus } from "lucide-react";
import { FranjaEstado } from "@/components/layout/FranjaEstado";
import { Boton } from "@/components/ui/Boton";
import { Campo } from "@/components/ui/Campo";
import { CampoSelect } from "@/components/ui/CampoSelect";
import { Estado } from "@/components/ui/Estado";
import { Aviso } from "@/components/ui/Aviso";
import { Modal } from "@/components/ui/Modal";
import { FilaDato, SeccionPanel } from "@/components/ui/SeccionPanel";
import { ErrorApiAscenso, llamarApiBff } from "@/lib/bff/client";
import {
  etiquetaTipoDonante,
  formatearFechaCorta,
  formatearMoneda,
} from "@/lib/formato";
import type { UsuarioSesionPublico } from "@/lib/auth/session";
import type {
  DonanteResumenDto,
  HistorialAportesDonanteDto,
  PeriodoResumenDto,
  ProgramaResumenDto,
  TipoDonante,
} from "@/lib/contratos/s2";

export function DonantesVista({
  usuario,
  donantesIniciales,
  programas,
  periodos,
  periodoActual,
  errorCarga,
}: {
  usuario: UsuarioSesionPublico;
  donantesIniciales: DonanteResumenDto[];
  programas: ProgramaResumenDto[];
  periodos: PeriodoResumenDto[];
  periodoActual: string | null;
  errorCarga?: string | null;
}) {
  const router = useRouter();
  const [donantes, setDonantes] = useState(donantesIniciales);
  const [donanteSeleccionadoId, setDonanteSeleccionadoId] = useState<string | null>(null);
  const [filtroPeriodo, setFiltroPeriodo] = useState("");
  const [historial, setHistorial] = useState<HistorialAportesDonanteDto | null>(null);
  const [cargandoHistorial, setCargandoHistorial] = useState(false);

  // Modal nuevo donante
  const [modalNuevo, setModalNuevo] = useState(false);
  const [nombre, setNombre] = useState("");
  const [tipo, setTipo] = useState<TipoDonante>("ORGANIZACION");
  const [correo, setCorreo] = useState("");
  const [telefono, setTelefono] = useState("");

  // Modal nuevo aporte desde donante
  const [modalAporte, setModalAporte] = useState(false);
  const [aporteProgramaId, setAporteProgramaId] = useState(programas[0]?.id ?? "");
  const [aporteCantidad, setAporteCantidad] = useState("");
  const [aportePeriodo, setAportePeriodo] = useState(
    periodoActual ?? periodos[0]?.codigo ?? ""
  );

  const [guardando, setGuardando] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [exitoMsg, setExitoMsg] = useState<string | null>(null);

  const totalAcumulado = donantes.reduce((acc, d) => acc + d.totalAportado, 0);
  const totalAportes = donantes.reduce((acc, d) => acc + d.cantidadAportes, 0);

  async function cargarHistorial(donanteId: string, periodoFiltro: string) {
    setDonanteSeleccionadoId(donanteId);
    setFiltroPeriodo(periodoFiltro);
    setCargandoHistorial(true);
    setErrorMsg(null);
    try {
      const qs = periodoFiltro ? `?codigoPeriodo=${encodeURIComponent(periodoFiltro)}` : "";
      const data = await llamarApiBff<HistorialAportesDonanteDto>(
        `/api/v1/donantes/${donanteId}/aportes${qs}`
      );
      setHistorial(data);
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo consultar el historial."
      );
    } finally {
      setCargandoHistorial(false);
    }
  }

  async function registrarDonante(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrorMsg(null);
    setGuardando(true);
    try {
      const creado = await llamarApiBff<DonanteResumenDto>("/api/v1/donantes", {
        method: "POST",
        body: {
          nombre: nombre.trim(),
          tipo,
          contacto: {
            correo: correo.trim() || null,
            telefono: telefono.trim() || null,
          },
        },
      });
      setDonantes((prev) => [...prev, creado]);
      setModalNuevo(false);
      setNombre("");
      setCorreo("");
      setTelefono("");
      setExitoMsg(`Donante «${creado.nombre}» registrado (sin cuenta de acceso al sistema).`);
      router.refresh();
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo registrar el donante."
      );
    } finally {
      setGuardando(false);
    }
  }

  async function registrarAporte(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!donanteSeleccionadoId) return;
    setErrorMsg(null);
    setGuardando(true);
    try {
      const prog = programas.find((p) => p.id === aporteProgramaId);
      await llamarApiBff(`/api/v1/donantes/${donanteSeleccionadoId}/aportes`, {
        method: "POST",
        body: {
          programaId: aporteProgramaId,
          cantidad: Number(aporteCantidad),
          moneda: prog?.moneda ?? "USD",
          codigoPeriodo: aportePeriodo,
        },
      });

      const listaActualizada = await llamarApiBff<DonanteResumenDto[]>("/api/v1/donantes");
      setDonantes(listaActualizada);
      await cargarHistorial(donanteSeleccionadoId, filtroPeriodo);
      setModalAporte(false);
      setAporteCantidad("");
      setExitoMsg("Aporte registrado correctamente en el ledger inmutable.");
      router.refresh();
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo registrar el aporte."
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
      titulo="Donantes y aportes"
      estadoBadge={<Estado texto={`${donantes.length} donante(s)`} tono="info" />}
      subtitulo="Registra organizaciones o personas donantes y consulta su historial inmutable de aportes por periodo (RF-13)."
      acciones={
        <Boton
          variante="primarioSobreColor"
          icono={<Plus className="size-4" aria-hidden="true" />}
          onClick={() => {
            setErrorMsg(null);
            setModalNuevo(true);
          }}
        >
          Registrar donante
        </Boton>
      }
      cifras={
        <div className="flex flex-wrap items-end gap-12">
          <div className="flex flex-col gap-1">
            <span className="text-[13px] font-medium text-[var(--color-text-on-brand-muted)]">
              Total aportado
            </span>
            <span className="font-display text-[44px] font-bold leading-[48px] tracking-[-0.02em] text-white">
              {formatearMoneda(totalAcumulado, "USD")}
            </span>
          </div>
          <div className="flex flex-col gap-1">
            <span className="text-[13px] font-medium text-[var(--color-text-on-brand-muted)]">
              Aportes registrados
            </span>
            <span className="font-display text-[44px] font-bold leading-[48px] tracking-[-0.02em] text-white">
              {totalAportes}
            </span>
          </div>
        </div>
      }
    >
      <div className="flex flex-col gap-8">
        {errorCarga ? <Aviso tono="alerta">{errorCarga}</Aviso> : null}
        {errorMsg ? <Aviso tono="peligro">{errorMsg}</Aviso> : null}
        {exitoMsg ? <Aviso tono="exito">{exitoMsg}</Aviso> : null}

        <div className="grid grid-cols-1 gap-16 lg:grid-cols-[1fr_420px]">
          <SeccionPanel titulo="Directorio de donantes">
            {donantes.length === 0 ? (
              <p className="py-8 text-[14px] text-[var(--color-text-secondary)]">
                Aún no hay donantes registrados.
              </p>
            ) : (
              <div className="divide-y divide-[var(--color-border-default)]">
                {donantes.map((d) => {
                  const activo = d.id === donanteSeleccionadoId;
                  return (
                    <div
                      key={d.id}
                      className={`flex flex-col justify-between gap-3 py-4 sm:flex-row sm:items-center ${
                        activo ? "bg-[var(--color-bg-brand-soft)]/60 -mx-3 rounded-[var(--radius-md)] px-3" : ""
                      }`}
                    >
                      <div className="min-w-0 flex-1">
                        <div className="flex flex-wrap items-center gap-2">
                          <span className="text-[15px] font-semibold text-[var(--color-text-primary)]">
                            {d.nombre}
                          </span>
                          <Estado
                            texto={etiquetaTipoDonante(d.tipo)}
                            tono={d.tipo === "ORGANIZACION" ? "info" : "acento"}
                          />
                        </div>
                        <p className="mt-1 text-[13px] text-[var(--color-text-secondary)]">
                          {[d.contacto.correo, d.contacto.telefono].filter(Boolean).join(" · ") ||
                            "Sin datos de contacto"}
                        </p>
                      </div>

                      <div className="flex items-center gap-5">
                        <div className="text-right">
                          <p className="text-[14px] font-semibold text-[var(--color-text-primary)]">
                            {formatearMoneda(d.totalAportado, "USD")}
                          </p>
                          <p className="text-[12px] text-[var(--color-text-muted)]">
                            {d.cantidadAportes} aporte(s)
                          </p>
                        </div>
                        <Boton
                          variante="secundario"
                          onClick={() => cargarHistorial(d.id, "")}
                          className="h-[38px] px-3 text-[13px]"
                        >
                          Ver aportes
                        </Boton>
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </SeccionPanel>

          <aside>
            <SeccionPanel
              titulo="Historial de aportes"
              accion={
                donanteSeleccionadoId ? (
                  <Boton
                    variante="terciario"
                    onClick={() => {
                      setErrorMsg(null);
                      setModalAporte(true);
                    }}
                  >
                    + Aporte
                  </Boton>
                ) : null
              }
            >
              {!donanteSeleccionadoId ? (
                <p className="py-6 text-[14px] text-[var(--color-text-secondary)]">
                  Selecciona «Ver aportes» en un donante para revisar su historial y registrar
                  nuevos aportes.
                </p>
              ) : cargandoHistorial ? (
                <p className="py-6 text-[14px] text-[var(--color-text-muted)]">
                  Cargando historial...
                </p>
              ) : historial ? (
                <div className="flex flex-col gap-4 py-4">
                  <div className="flex items-end gap-3">
                    <CampoSelect
                      etiqueta="Filtrar por periodo"
                      value={filtroPeriodo}
                      onChange={(e) => cargarHistorial(historial.donanteId, e.target.value)}
                      opciones={[
                        { valor: "", etiqueta: "Todos los periodos" },
                        ...periodos.map((per) => ({ valor: per.codigo, etiqueta: per.codigo })),
                      ]}
                    />
                  </div>

                  <FilaDato
                    etiqueta="Donante"
                    valor={historial.nombreDonante}
                    conBorde
                  />
                  <FilaDato
                    etiqueta="Total en el filtro"
                    valor={formatearMoneda(historial.totalAportado, "USD")}
                    conBorde
                  />

                  {historial.aportes.length === 0 ? (
                    <p className="py-3 text-[13px] text-[var(--color-text-secondary)]">
                      {historial.mensajeSinAportes ?? "No hay aportes para este filtro."}
                    </p>
                  ) : (
                    <div className="divide-y divide-[var(--color-border-default)]">
                      {historial.aportes.map((ap) => (
                        <div
                          key={ap.id}
                          className="flex items-center justify-between gap-3 py-3 text-[13px]"
                        >
                          <div className="min-w-0">
                            <p className="font-semibold text-[var(--color-text-primary)]">
                              {ap.nombrePrograma}
                            </p>
                            <p className="text-[12px] text-[var(--color-text-muted)]">
                              Periodo {ap.codigoPeriodo} · {formatearFechaCorta(ap.registradoEnUtc)}
                            </p>
                          </div>
                          <span className="shrink-0 font-semibold text-[var(--color-text-primary)]">
                            {formatearMoneda(ap.monto, ap.moneda)}
                          </span>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              ) : null}
            </SeccionPanel>
          </aside>
        </div>
      </div>

      {/* Modal: Registrar donante */}
      <Modal
        abierto={modalNuevo}
        onCerrar={() => setModalNuevo(false)}
        titulo="Registrar donante"
        descripcion="Los donantes se registran solo con fines de financiamiento y reportería; no se crea cuenta de usuario (RF-13)."
      >
        <form onSubmit={registrarDonante} className="flex flex-col gap-4">
          <Campo
            etiqueta="Nombre completo o razón social"
            placeholder="Ej. Fundación Horizonte"
            value={nombre}
            onChange={(e) => setNombre(e.target.value)}
            required
          />
          <CampoSelect
            etiqueta="Tipo de donante"
            value={tipo}
            onChange={(e) => setTipo(e.target.value as TipoDonante)}
            opciones={[
              { valor: "ORGANIZACION", etiqueta: "Organización" },
              { valor: "PERSONA", etiqueta: "Persona" },
            ]}
          />
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Campo
              etiqueta="Correo de contacto"
              type="email"
              placeholder="contacto@organizacion.org"
              value={correo}
              onChange={(e) => setCorreo(e.target.value)}
            />
            <Campo
              etiqueta="Teléfono de contacto"
              placeholder="+58 412 000 0000"
              value={telefono}
              onChange={(e) => setTelefono(e.target.value)}
            />
          </div>
          <div className="mt-2 flex justify-end gap-3">
            <Boton variante="secundario" onClick={() => setModalNuevo(false)}>
              Cancelar
            </Boton>
            <Boton type="submit" variante="primario" cargando={guardando}>
              Guardar donante
            </Boton>
          </div>
        </form>
      </Modal>

      {/* Modal: Registrar aporte desde donante */}
      <Modal
        abierto={modalAporte}
        onCerrar={() => setModalAporte(false)}
        titulo="Registrar aporte"
        descripcion="Se registra como asiento inmutable y actualiza automáticamente el presupuesto del programa."
      >
        <form onSubmit={registrarAporte} className="flex flex-col gap-4">
          {programas.length === 0 ? (
            <Aviso tono="alerta">Crea primero un programa antes de registrar un aporte.</Aviso>
          ) : (
            <CampoSelect
              etiqueta="Programa destino"
              value={aporteProgramaId}
              onChange={(e) => setAporteProgramaId(e.target.value)}
              opciones={programas.map((p) => ({
                valor: p.id,
                etiqueta: `${p.nombre} (${p.moneda})`,
              }))}
              required
            />
          )}

          <div className="grid grid-cols-2 gap-4">
            <Campo
              etiqueta="Cantidad"
              type="number"
              min={1}
              step="0.01"
              value={aporteCantidad}
              onChange={(e) => setAporteCantidad(e.target.value)}
              required
            />
            {periodos.length > 0 ? (
              <CampoSelect
                etiqueta="Periodo"
                value={aportePeriodo}
                onChange={(e) => setAportePeriodo(e.target.value)}
                opciones={periodos.map((per) => ({ valor: per.codigo, etiqueta: per.codigo }))}
                required
              />
            ) : (
              <Campo
                etiqueta="Periodo"
                value={aportePeriodo}
                onChange={(e) => setAportePeriodo(e.target.value)}
                required
              />
            )}
          </div>

          <div className="mt-2 flex justify-end gap-3">
            <Boton variante="secundario" onClick={() => setModalAporte(false)}>
              Cancelar
            </Boton>
            <Boton
              type="submit"
              variante="primario"
              cargando={guardando}
              disabled={programas.length === 0}
            >
              Registrar aporte
            </Boton>
          </div>
        </form>
      </Modal>
    </FranjaEstado>
  );
}
