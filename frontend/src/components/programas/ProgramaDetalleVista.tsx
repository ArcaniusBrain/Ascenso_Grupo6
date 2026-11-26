"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import { ArrowLeft, Plus, SlidersHorizontal } from "lucide-react";
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
  calcularBecasEstimadasDisponibles,
  etiquetaAlcance,
  etiquetaModalidadPago,
  formatearMoneda,
} from "@/lib/formato";
import type { UsuarioSesionPublico } from "@/lib/auth/session";
import type {
  AlcancePrograma,
  DonanteResumenDto,
  ModalidadPago,
  PeriodoResumenDto,
  PresupuestoProgramaDto,
  ProgramaDetalleDto,
  UniversidadResumenDto,
} from "@/lib/contratos/s2";

/**
 * Vista de detalle de programa, presupuesto y cupos fiel a Figma `08 · Coordinador — Programa: presupuesto y cupo`
 * (cubre RF-11, RF-12 y registro de aporte de RF-13).
 */
export function ProgramaDetalleVista({
  usuario,
  programaInicial,
  presupuestoInicial,
  donantesCatalogo,
  universidadesCatalogo,
  periodos,
  periodoActual,
}: {
  usuario: UsuarioSesionPublico;
  programaInicial: ProgramaDetalleDto;
  presupuestoInicial: PresupuestoProgramaDto | null;
  donantesCatalogo: DonanteResumenDto[];
  universidadesCatalogo: UniversidadResumenDto[];
  periodos: PeriodoResumenDto[];
  periodoActual: string | null;
}) {
  const router = useRouter();
  const [programa, setPrograma] = useState(programaInicial);
  const [presupuesto, setPresupuesto] = useState(presupuestoInicial);
  const [mensajeOk, setMensajeOk] = useState<string | null>(null);

  // Modales
  const [modalAporte, setModalAporte] = useState(false);
  const [modalEditar, setModalEditar] = useState(false);
  const [modalAsociarDonante, setModalAsociarDonante] = useState(false);
  const [modalCondiciones, setModalCondiciones] = useState(false);
  const [modalElegibilidad, setModalElegibilidad] = useState(false);

  // Estado formulario aporte
  const [aporteDonanteId, setAporteDonanteId] = useState(donantesCatalogo[0]?.id ?? "");
  const [aporteCantidad, setAporteCantidad] = useState("");
  const [aportePeriodo, setAportePeriodo] = useState(
    periodoActual ?? periodos[0]?.codigo ?? programa.periodoInicio
  );

  // Estado formulario edición base
  const [editNombre, setEditNombre] = useState(programa.nombre);
  const [editMonto, setEditMonto] = useState(String(programa.montoPorPeriodo));
  const [editMoneda, setEditMoneda] = useState(programa.moneda);
  const [editModalidad, setEditModalidad] = useState<ModalidadPago>(
    (programa.modalidadPago as ModalidadPago) || "SEMESTRAL"
  );
  const [editAlcance, setEditAlcance] = useState<AlcancePrograma>(
    (programa.alcance as AlcancePrograma) || "COMPLETO"
  );
  const [editAcumulacion, setEditAcumulacion] = useState(programa.permiteAcumulacion);

  // Estado asociar donante
  const [asociarDonanteId, setAsociarDonanteId] = useState(donantesCatalogo[0]?.id ?? "");

  // Estado condiciones
  const [condPromedio, setCondPromedio] = useState(
    programa.condiciones ? String(programa.condiciones.promedioMinimo) : "15"
  );
  const [condExigeHoras, setCondExigeHoras] = useState(
    programa.condiciones?.exigeHorasServicio ?? false
  );
  const [condHoras, setCondHoras] = useState(
    programa.condiciones?.horasServicio ? String(programa.condiciones.horasServicio) : "20"
  );
  const [condReportes, setCondReportes] = useState(
    programa.condiciones?.reportesExigidos.join(", ") ?? "Notas del periodo, Constancia de matrícula"
  );
  const [condPausaMeses, setCondPausaMeses] = useState(
    programa.condiciones ? String(programa.condiciones.duracionMaxPausaMeses) : "6"
  );
  const [condRenovacion, setCondRenovacion] = useState(
    programa.condiciones?.descripcionRenovacion ??
      "Mantener el promedio mínimo y presentar recaudos antes del inicio del nuevo periodo."
  );

  // Estado elegibilidad
  const [elegUniId, setElegUniId] = useState(universidadesCatalogo[0]?.id ?? "");
  const [elegCarrerasTexto, setElegCarrerasTexto] = useState("");

  const [guardando, setGuardando] = useState(false);
  const [errorModal, setErrorModal] = useState<string | null>(null);

  async function refrescarDatos() {
    const [det, pres] = await Promise.all([
      llamarApiBff<ProgramaDetalleDto>(`/api/v1/programas/${programa.id}`),
      llamarApiBff<PresupuestoProgramaDto>(`/api/v1/programas/${programa.id}/presupuesto`),
    ]);
    setPrograma(det);
    setPresupuesto(pres);
    router.refresh();
  }

  async function registrarAporte(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrorModal(null);
    setGuardando(true);
    try {
      await llamarApiBff(`/api/v1/programas/${programa.id}/aportes`, {
        method: "POST",
        body: {
          donanteId: aporteDonanteId,
          cantidad: Number(aporteCantidad),
          moneda: programa.moneda,
          codigoPeriodo: aportePeriodo,
        },
      });
      await refrescarDatos();
      setModalAporte(false);
      setAporteCantidad("");
      setMensajeOk("Aporte registrado y presupuesto recalculado correctamente.");
    } catch (err) {
      setErrorModal(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo registrar el aporte."
      );
    } finally {
      setGuardando(false);
    }
  }

  async function guardarEdicion(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrorModal(null);
    setGuardando(true);
    try {
      await llamarApiBff(`/api/v1/programas/${programa.id}/configuracion-base`, {
        method: "PUT",
        body: {
          nombre: editNombre.trim(),
          montoPorPeriodo: Number(editMonto),
          moneda: editMoneda.trim().toUpperCase(),
          modalidadPago: editModalidad,
          alcance: editAlcance,
          permiteAcumulacion: editAlcance === "COMPLETO" ? false : editAcumulacion,
        },
      });
      await refrescarDatos();
      setModalEditar(false);
      setMensajeOk("Configuración base del programa actualizada.");
    } catch (err) {
      setErrorModal(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo actualizar el programa."
      );
    } finally {
      setGuardando(false);
    }
  }

  async function asociarDonante(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrorModal(null);
    setGuardando(true);
    try {
      await llamarApiBff(`/api/v1/programas/${programa.id}/donantes`, {
        method: "POST",
        body: { donanteId: asociarDonanteId },
      });
      await refrescarDatos();
      setModalAsociarDonante(false);
      setMensajeOk("Donante asociado al programa.");
    } catch (err) {
      setErrorModal(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo asociar el donante."
      );
    } finally {
      setGuardando(false);
    }
  }

  async function guardarCondiciones(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrorModal(null);
    setGuardando(true);
    try {
      const reportes = condReportes
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean);

      await llamarApiBff(`/api/v1/programas/${programa.id}/condiciones`, {
        method: "PUT",
        body: {
          promedioMinimo: condPromedio.trim(),
          exigeHorasServicio: condExigeHoras,
          horasServicio: condExigeHoras ? Number(condHoras) : null,
          reportesExigidos: reportes,
          duracionMaxPausaMeses: Number(condPausaMeses),
          descripcionRenovacion: condRenovacion.trim(),
        },
      });
      await refrescarDatos();
      setModalCondiciones(false);
      setMensajeOk("Condiciones de permanencia guardadas.");
    } catch (err) {
      setErrorModal(
        err instanceof ErrorApiAscenso ? err.message : "No se pudieron guardar las condiciones."
      );
    } finally {
      setGuardando(false);
    }
  }

  async function agregarRestriccionElegibilidad(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrorModal(null);
    setGuardando(true);
    try {
      const carreras = elegCarrerasTexto
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean);

      const existentes = programa.restricciones
        .filter((r) => r.universidadId !== elegUniId)
        .map((r) => ({
          universidadId: r.universidadId,
          carreras: r.carreras.length > 0 ? r.carreras : null,
        }));

      const nuevas = elegUniId
        ? [...existentes, { universidadId: elegUniId, carreras: carreras.length > 0 ? carreras : null }]
        : [];

      await llamarApiBff(`/api/v1/programas/${programa.id}/elegibilidad`, {
        method: "PUT",
        body: { restricciones: nuevas },
      });
      await refrescarDatos();
      setModalElegibilidad(false);
      setElegCarrerasTexto("");
      setMensajeOk("Restricciones de elegibilidad actualizadas.");
    } catch (err) {
      setErrorModal(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo actualizar la elegibilidad."
      );
    } finally {
      setGuardando(false);
    }
  }

  const p = presupuesto;
  const moneda = p?.moneda ?? programa.moneda;
  const aportesTotal = p?.presupuestoTotal ?? programa.presupuestoTotal;
  const otorgado = p?.totalOtorgado ?? 0;
  const ofertas = p?.comprometidoOfertasPendientes ?? 0;
  const renovaciones = p?.comprometidoRenovacionesPendientes ?? 0;
  const disponible = p?.presupuestoDisponible ?? aportesTotal;
  const becasAlcanzan = calcularBecasEstimadasDisponibles(
    disponible,
    programa.montoPorPeriodo,
    programa.duracionPeriodos
  );

  return (
    <FranjaEstado
      tono={aportesTotal > 0 ? "logrado" : "inicio"}
      usuario={usuario}
      periodoActual={periodoActual}
      titulo={programa.nombre}
      estadoBadge={
        <Estado
          texto={etiquetaAlcance(programa.alcance)}
          tono={programa.alcance === "COMPLETO" ? "exito" : "acento"}
        />
      }
      subtitulo={`Monto fijo: ${formatearMoneda(programa.montoPorPeriodo, programa.moneda)} por periodo. ${etiquetaModalidadPago(programa.modalidadPago)}. Duración: ${programa.duracionPeriodos} periodos`}
      acciones={
        <>
          <Boton
            variante="secundarioSobreColor"
            icono={<SlidersHorizontal className="size-4" aria-hidden="true" />}
            onClick={() => {
              setErrorModal(null);
              setModalEditar(true);
            }}
          >
            Editar programa
          </Boton>
          <Boton
            variante="primarioSobreColor"
            icono={<Plus className="size-4" aria-hidden="true" />}
            onClick={() => {
              setErrorModal(null);
              setModalAporte(true);
            }}
          >
            Registrar aporte
          </Boton>
        </>
      }
      cifras={
        <div className="flex flex-wrap items-end gap-x-6 gap-y-6">
          <div className="flex flex-col gap-1">
            <span className="text-[13px] font-medium leading-[18px] text-[var(--color-text-on-brand-muted)]">
              Aportes recibidos
            </span>
            <span className="font-display text-[36px] font-bold leading-[42px] tracking-[-0.02em] text-white xl:text-[44px] xl:leading-[48px]">
              {formatearMoneda(aportesTotal, moneda)}
            </span>
            <span className="text-[12px] leading-[18px] text-[var(--color-text-on-brand-muted)]">
              {programa.donantes.length > 0
                ? `suma de ${programa.donantes.length} donante(s)`
                : "sin aportes registrados aún"}
            </span>
          </div>

          <span
            aria-hidden="true"
            className="font-display pb-5 text-[36px] font-bold leading-[44px] text-[var(--color-text-on-brand-muted)]"
          >
            −
          </span>

          <div className="flex flex-col gap-1">
            <span className="text-[13px] font-medium leading-[18px] text-[var(--color-text-on-brand-muted)]">
              Otorgado
            </span>
            <span className="font-display text-[36px] font-bold leading-[42px] tracking-[-0.02em] text-white xl:text-[44px] xl:leading-[48px]">
              {formatearMoneda(otorgado, moneda)}
            </span>
            <span className="text-[12px] leading-[18px] text-[var(--color-text-on-brand-muted)]">
              becas activas / inactivas
            </span>
          </div>

          <span
            aria-hidden="true"
            className="font-display pb-5 text-[36px] font-bold leading-[44px] text-[var(--color-text-on-brand-muted)]"
          >
            −
          </span>

          <div className="flex flex-col gap-1">
            <span className="text-[13px] font-medium leading-[18px] text-[var(--color-text-on-brand-muted)]">
              Ofertas pendientes
            </span>
            <span className="font-display text-[36px] font-bold leading-[42px] tracking-[-0.02em] text-white xl:text-[44px] xl:leading-[48px]">
              {formatearMoneda(ofertas, moneda)}
            </span>
            <span className="text-[12px] leading-[18px] text-[var(--color-text-on-brand-muted)]">
              reservas activas
            </span>
          </div>

          <span
            aria-hidden="true"
            className="font-display pb-5 text-[36px] font-bold leading-[44px] text-[var(--color-text-on-brand-muted)]"
          >
            −
          </span>

          <div className="flex flex-col gap-1">
            <span className="text-[13px] font-medium leading-[18px] text-[var(--color-text-on-brand-muted)]">
              Renovaciones pendientes
            </span>
            <span className="font-display text-[36px] font-bold leading-[42px] tracking-[-0.02em] text-white xl:text-[44px] xl:leading-[48px]">
              {formatearMoneda(renovaciones, moneda)}
            </span>
            <span className="text-[12px] leading-[18px] text-[var(--color-text-on-brand-muted)]">
              renovaciones en curso
            </span>
          </div>

          <span
            aria-hidden="true"
            className="font-display pb-5 text-[36px] font-bold leading-[44px] text-[var(--color-text-on-brand-muted)]"
          >
            =
          </span>

          <div className="flex flex-col gap-1">
            <span className="text-[13px] font-medium leading-[18px] text-[var(--color-text-on-brand-muted)]">
              Disponible
            </span>
            <span className="font-display text-[52px] font-bold leading-[54px] tracking-[-0.03em] text-white xl:text-[72px] xl:leading-[72px]">
              {formatearMoneda(disponible, moneda)}
            </span>
            <span className="text-[12px] leading-[18px] text-[var(--color-text-on-brand-muted)]">
              {p?.mensajeSinAportes ??
                (becasAlcanzan > 0
                  ? `alcanza para ${becasAlcanzan} beca(s) nueva(s)`
                  : "sin margen para nuevas becas completas")}
            </span>
          </div>
        </div>
      }
    >
      <div className="flex flex-col gap-10">
        <div className="flex items-center justify-between gap-4">
          <Link
            href="/programas"
            className="ascenso-focus inline-flex items-center gap-2 rounded-[var(--radius-sm)] text-[14px] font-semibold text-[var(--color-text-brand)] hover:underline"
          >
            <ArrowLeft className="size-4" aria-hidden="true" />
            <span>Volver a programas</span>
          </Link>
        </div>

        {mensajeOk ? <Aviso tono="exito">{mensajeOk}</Aviso> : null}

        <div className="grid grid-cols-1 gap-16 lg:grid-cols-[1fr_360px]">
          <div className="flex flex-col gap-12">
            <SeccionPanel
              titulo="Aportes por donante"
              accion={
                <Boton
                  variante="terciario"
                  onClick={() => {
                    setErrorModal(null);
                    setModalAsociarDonante(true);
                  }}
                >
                  Asociar donante
                </Boton>
              }
            >
              {programa.donantes.length === 0 ? (
                <p className="py-6 text-[14px] text-[var(--color-text-secondary)]">
                  Este programa aún no tiene donantes asociados ni aportes registrados.
                </p>
              ) : (
                <div className="divide-y divide-[var(--color-border-default)]">
                  {programa.donantes.map((d) => (
                    <div
                      key={d.donanteId}
                      className="flex items-center justify-between gap-4 py-4 text-[14px] leading-[22px]"
                    >
                      <span className="min-w-0 flex-1 font-semibold text-[var(--color-text-primary)]">
                        {d.nombreDonante}
                      </span>
                      <span className="text-[var(--color-text-secondary)]">
                        {d.leyendaSinAportes ?? "Aportes registrados"}
                      </span>
                      <span className="w-[130px] text-right font-semibold text-[var(--color-text-primary)]">
                        {formatearMoneda(d.totalAportado, d.moneda)}
                      </span>
                    </div>
                  ))}
                </div>
              )}
            </SeccionPanel>

            <SeccionPanel
              titulo="Elegibilidad"
              accion={
                <Boton
                  variante="terciario"
                  onClick={() => {
                    setErrorModal(null);
                    setModalElegibilidad(true);
                  }}
                >
                  Configurar elegibilidad
                </Boton>
              }
            >
              {programa.restricciones.length === 0 ? (
                <p className="py-5 text-[14px] text-[var(--color-text-secondary)]">
                  {programa.leyendaElegibilidad ??
                    "Abierto a todas las universidades aliadas y carreras elegibles."}
                </p>
              ) : (
                <div className="divide-y divide-[var(--color-border-default)]">
                  {programa.restricciones.map((r) => (
                    <div key={r.universidadId} className="py-4">
                      <p className="text-[14px] font-semibold text-[var(--color-text-primary)]">
                        {r.nombreUniversidad}
                      </p>
                      <p className="mt-1 text-[13px] text-[var(--color-text-secondary)]">
                        {r.carreras.length > 0
                          ? `Carreras restringidas: ${r.carreras.join(", ")}`
                          : (r.avisoSinCarreras ?? "Todas las carreras de esta universidad")}
                      </p>
                    </div>
                  ))}
                </div>
              )}
            </SeccionPanel>

            <SeccionPanel
              titulo="Condiciones de permanencia"
              accion={
                <Boton
                  variante="terciario"
                  onClick={() => {
                    setErrorModal(null);
                    setModalCondiciones(true);
                  }}
                >
                  Configurar condiciones
                </Boton>
              }
            >
              {!programa.condiciones ? (
                <p className="py-5 text-[14px] text-[var(--color-text-secondary)]">
                  Aún no se han definido las condiciones de permanencia y renovación.
                </p>
              ) : (
                <div className="py-2">
                  <FilaDato
                    etiqueta="Promedio mínimo exigido"
                    valor={String(programa.condiciones.promedioMinimo)}
                  />
                  <FilaDato
                    etiqueta="Horas de servicio"
                    valor={programa.condiciones.leyendaHorasServicio}
                  />
                  <FilaDato
                    etiqueta="Duración máxima de pausa"
                    valor={`${programa.condiciones.duracionMaxPausaMeses} meses`}
                  />
                  <FilaDato
                    etiqueta="Reportes exigidos"
                    valor={programa.condiciones.reportesExigidos.join(", ") || "—"}
                  />
                  <p className="mt-3 text-[13px] leading-[20px] text-[var(--color-text-secondary)]">
                    <strong className="text-[var(--color-text-primary)]">Renovación: </strong>
                    {programa.condiciones.descripcionRenovacion}
                  </p>
                </div>
              )}
            </SeccionPanel>
          </div>

          <aside className="flex flex-col gap-10">
            <SeccionPanel titulo="Cupos">
              <div className="py-2">
                <FilaDato
                  etiqueta="Cupo del programa"
                  valor={`${p?.cupoOcupado ?? 0} de ${p?.cupoTotal ?? programa.cupoBecarios} becarios`}
                />
                <FilaDato
                  etiqueta="Cupos disponibles"
                  valor={`${p?.cupoDisponible ?? programa.cupoBecarios} nuevos`}
                />
                <FilaDato
                  etiqueta="Desembolsado acumulado"
                  valor={formatearMoneda(p?.desembolsado ?? 0, moneda)}
                />
                <FilaDato
                  etiqueta="Acumulación"
                  valor={programa.permiteAcumulacion ? "Permitida" : "No permitida"}
                />
              </div>
              <p className="pt-2 text-[12px] leading-[18px] text-[var(--color-text-muted)]">
                Una beca Inactiva mantiene ocupados su cupo y su presupuesto; una beca revocada los
                libera.
              </p>
            </SeccionPanel>
          </aside>
        </div>
      </div>

      {/* Modal: Registrar aporte (RF-13 + RF-12) */}
      <Modal
        abierto={modalAporte}
        onCerrar={() => setModalAporte(false)}
        titulo="Registrar aporte al programa"
        descripcion="Los aportes se registran de forma inmutable (append-only) y actualizan el presupuesto disponible."
      >
        <form onSubmit={registrarAporte} className="flex flex-col gap-4">
          {errorModal ? <Aviso tono="peligro">{errorModal}</Aviso> : null}

          {donantesCatalogo.length === 0 ? (
            <Aviso tono="alerta">
              No hay donantes registrados todavía. Registra primero un donante en la sección{" "}
              <Link href="/donantes" className="underline">
                Donantes
              </Link>
              .
            </Aviso>
          ) : (
            <CampoSelect
              etiqueta="Donante"
              value={aporteDonanteId}
              onChange={(e) => setAporteDonanteId(e.target.value)}
              opciones={donantesCatalogo.map((d) => ({ valor: d.id, etiqueta: d.nombre }))}
              required
            />
          )}

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Campo
              etiqueta={`Cantidad (${programa.moneda})`}
              type="number"
              min={1}
              step="0.01"
              value={aporteCantidad}
              onChange={(e) => setAporteCantidad(e.target.value)}
              required
            />
            {periodos.length > 0 ? (
              <CampoSelect
                etiqueta="Periodo del aporte"
                value={aportePeriodo}
                onChange={(e) => setAportePeriodo(e.target.value)}
                opciones={periodos.map((per) => ({ valor: per.codigo, etiqueta: per.codigo }))}
                required
              />
            ) : (
              <Campo
                etiqueta="Periodo del aporte"
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
              disabled={donantesCatalogo.length === 0}
            >
              Guardar aporte
            </Boton>
          </div>
        </form>
      </Modal>

      {/* Modal: Editar configuración base */}
      <Modal
        abierto={modalEditar}
        onCerrar={() => setModalEditar(false)}
        titulo="Editar configuración base"
      >
        <form onSubmit={guardarEdicion} className="flex flex-col gap-4">
          {errorModal ? <Aviso tono="peligro">{errorModal}</Aviso> : null}
          <Campo
            etiqueta="Nombre"
            value={editNombre}
            onChange={(e) => setEditNombre(e.target.value)}
            required
          />
          <div className="grid grid-cols-2 gap-4">
            <Campo
              etiqueta="Monto por periodo"
              type="number"
              min={1}
              step="0.01"
              value={editMonto}
              onChange={(e) => setEditMonto(e.target.value)}
              required
            />
            <Campo
              etiqueta="Moneda"
              maxLength={3}
              value={editMoneda}
              onChange={(e) => setEditMoneda(e.target.value)}
              required
            />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <CampoSelect
              etiqueta="Modalidad de pago"
              value={editModalidad}
              onChange={(e) => setEditModalidad(e.target.value as ModalidadPago)}
              opciones={[
                { valor: "SEMESTRAL", etiqueta: "Semestral" },
                { valor: "MENSUAL", etiqueta: "Mensual" },
                { valor: "DIRECTO_UNIVERSIDAD", etiqueta: "Directo a universidad" },
              ]}
            />
            <CampoSelect
              etiqueta="Alcance"
              value={editAlcance}
              onChange={(e) => {
                const val = e.target.value as AlcancePrograma;
                setEditAlcance(val);
                if (val === "COMPLETO") setEditAcumulacion(false);
              }}
              opciones={[
                { valor: "COMPLETO", etiqueta: "Completo" },
                { valor: "PARCIAL", etiqueta: "Parcial" },
              ]}
            />
          </div>
          <label className="flex items-center gap-2 text-[13px] text-[var(--color-text-secondary)]">
            <input
              type="checkbox"
              checked={editAlcance === "COMPLETO" ? false : editAcumulacion}
              disabled={editAlcance === "COMPLETO"}
              onChange={(e) => setEditAcumulacion(e.target.checked)}
            />
            <span>Permitir acumulación con otros programas parciales</span>
          </label>
          <div className="mt-2 flex justify-end gap-3">
            <Boton variante="secundario" onClick={() => setModalEditar(false)}>
              Cancelar
            </Boton>
            <Boton type="submit" variante="primario" cargando={guardando}>
              Guardar cambios
            </Boton>
          </div>
        </form>
      </Modal>

      {/* Modal: Asociar donante */}
      <Modal
        abierto={modalAsociarDonante}
        onCerrar={() => setModalAsociarDonante(false)}
        titulo="Asociar donante al programa"
      >
        <form onSubmit={asociarDonante} className="flex flex-col gap-4">
          {errorModal ? <Aviso tono="peligro">{errorModal}</Aviso> : null}
          <CampoSelect
            etiqueta="Donante"
            value={asociarDonanteId}
            onChange={(e) => setAsociarDonanteId(e.target.value)}
            opciones={donantesCatalogo.map((d) => ({ valor: d.id, etiqueta: d.nombre }))}
            required
          />
          <div className="mt-2 flex justify-end gap-3">
            <Boton variante="secundario" onClick={() => setModalAsociarDonante(false)}>
              Cancelar
            </Boton>
            <Boton
              type="submit"
              variante="primario"
              cargando={guardando}
              disabled={donantesCatalogo.length === 0}
            >
              Asociar
            </Boton>
          </div>
        </form>
      </Modal>

      {/* Modal: Configurar condiciones de permanencia */}
      <Modal
        abierto={modalCondiciones}
        onCerrar={() => setModalCondiciones(false)}
        titulo="Condiciones de permanencia"
      >
        <form onSubmit={guardarCondiciones} className="flex flex-col gap-4">
          {errorModal ? <Aviso tono="peligro">{errorModal}</Aviso> : null}
          <div className="grid grid-cols-2 gap-4">
            <Campo
              etiqueta="Promedio mínimo"
              value={condPromedio}
              onChange={(e) => setCondPromedio(e.target.value)}
              required
            />
            <Campo
              etiqueta="Pausa máxima (meses)"
              type="number"
              min={1}
              value={condPausaMeses}
              onChange={(e) => setCondPausaMeses(e.target.value)}
              required
            />
          </div>
          <label className="flex items-center gap-2 text-[13px] text-[var(--color-text-secondary)]">
            <input
              type="checkbox"
              checked={condExigeHoras}
              onChange={(e) => setCondExigeHoras(e.target.checked)}
            />
            <span>Exige horas de servicio comunitario</span>
          </label>
          {condExigeHoras ? (
            <Campo
              etiqueta="Cantidad de horas exigidas"
              type="number"
              min={1}
              value={condHoras}
              onChange={(e) => setCondHoras(e.target.value)}
              required
            />
          ) : null}
          <Campo
            etiqueta="Reportes exigidos (separados por coma)"
            value={condReportes}
            onChange={(e) => setCondReportes(e.target.value)}
          />
          <Campo
            etiqueta="Descripción de renovación"
            value={condRenovacion}
            onChange={(e) => setCondRenovacion(e.target.value)}
            required
          />
          <div className="mt-2 flex justify-end gap-3">
            <Boton variante="secundario" onClick={() => setModalCondiciones(false)}>
              Cancelar
            </Boton>
            <Boton type="submit" variante="primario" cargando={guardando}>
              Guardar condiciones
            </Boton>
          </div>
        </form>
      </Modal>

      {/* Modal: Configurar elegibilidad */}
      <Modal
        abierto={modalElegibilidad}
        onCerrar={() => setModalElegibilidad(false)}
        titulo="Agregar restricción de elegibilidad"
        descripcion="Restringe el programa a universidades aliadas y, opcionalmente, a carreras específicas."
      >
        <form onSubmit={agregarRestriccionElegibilidad} className="flex flex-col gap-4">
          {errorModal ? <Aviso tono="peligro">{errorModal}</Aviso> : null}
          {universidadesCatalogo.length === 0 ? (
            <Aviso tono="alerta">
              No hay universidades aliadas registradas todavía.
            </Aviso>
          ) : (
            <CampoSelect
              etiqueta="Universidad aliada"
              value={elegUniId}
              onChange={(e) => setElegUniId(e.target.value)}
              opciones={universidadesCatalogo.map((u) => ({ valor: u.id, etiqueta: u.nombre }))}
            />
          )}
          <Campo
            etiqueta="Carreras específicas (opcional, separadas por coma)"
            placeholder="Dejar vacío para incluir todas las carreras elegibles"
            value={elegCarrerasTexto}
            onChange={(e) => setElegCarrerasTexto(e.target.value)}
          />
          <div className="mt-2 flex justify-end gap-3">
            <Boton variante="secundario" onClick={() => setModalElegibilidad(false)}>
              Cancelar
            </Boton>
            <Boton
              type="submit"
              variante="primario"
              cargando={guardando}
              disabled={universidadesCatalogo.length === 0}
            >
              Guardar elegibilidad
            </Boton>
          </div>
        </form>
      </Modal>
    </FranjaEstado>
  );
}
