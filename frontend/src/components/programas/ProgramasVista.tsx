"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import { ArrowRight, Plus } from "lucide-react";
import { FranjaEstado } from "@/components/layout/FranjaEstado";
import { Boton } from "@/components/ui/Boton";
import { Campo } from "@/components/ui/Campo";
import { CampoSelect } from "@/components/ui/CampoSelect";
import { Estado } from "@/components/ui/Estado";
import { Aviso } from "@/components/ui/Aviso";
import { Modal } from "@/components/ui/Modal";
import { SeccionPanel } from "@/components/ui/SeccionPanel";
import { ErrorApiAscenso, llamarApiBff } from "@/lib/bff/client";
import {
  etiquetaAlcance,
  etiquetaModalidadPago,
  formatearMoneda,
} from "@/lib/formato";
import type { UsuarioSesionPublico } from "@/lib/auth/session";
import type {
  AlcancePrograma,
  ModalidadPago,
  PeriodoResumenDto,
  ProgramaResumenDto,
} from "@/lib/contratos/s2";

export function ProgramasVista({
  usuario,
  programasIniciales,
  periodos,
  periodoActual,
  errorCarga,
}: {
  usuario: UsuarioSesionPublico;
  programasIniciales: ProgramaResumenDto[];
  periodos: PeriodoResumenDto[];
  periodoActual: string | null;
  errorCarga?: string | null;
}) {
  const router = useRouter();
  const [programas, setProgramas] = useState(programasIniciales);
  const [modalAbierto, setModalAbierto] = useState(false);

  const [nombre, setNombre] = useState("");
  const [periodoInicio, setPeriodoInicio] = useState(
    periodoActual ?? periodos[0]?.codigo ?? ""
  );
  const [cupoBecarios, setCupoBecarios] = useState("20");
  const [duracionPeriodos, setDuracionPeriodos] = useState("8");
  const [montoPorPeriodo, setMontoPorPeriodo] = useState("2000");
  const [moneda, setMoneda] = useState("USD");
  const [modalidadPago, setModalidadPago] = useState<ModalidadPago>("SEMESTRAL");
  const [alcance, setAlcance] = useState<AlcancePrograma>("COMPLETO");
  const [permiteAcumulacion, setPermiteAcumulacion] = useState(false);

  const [guardando, setGuardando] = useState(false);
  const [errorForm, setErrorForm] = useState<string | null>(null);

  const puedeCrear = usuario.roles.some((r) =>
    ["DIRECCION", "COORDINADOR", "ADMINISTRADOR"].includes(r.toUpperCase())
  );

  function cambiarAlcance(nuevo: AlcancePrograma) {
    setAlcance(nuevo);
    if (nuevo === "COMPLETO") {
      setPermiteAcumulacion(false);
    }
  }

  async function crearPrograma(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrorForm(null);
    setGuardando(true);
    try {
      const creado = await llamarApiBff<ProgramaResumenDto>("/api/v1/programas", {
        method: "POST",
        body: {
          nombre: nombre.trim(),
          periodoInicio: periodoInicio.trim(),
          cupoBecarios: Number(cupoBecarios),
          duracionPeriodos: Number(duracionPeriodos),
          montoPorPeriodo: Number(montoPorPeriodo),
          moneda: moneda.trim().toUpperCase(),
          modalidadPago,
          alcance,
          permiteAcumulacion: alcance === "COMPLETO" ? false : permiteAcumulacion,
        },
      });

      setProgramas((prev) => [...prev, creado]);
      setModalAbierto(false);
      setNombre("");
      router.push(`/programas/${creado.id}`);
      router.refresh();
    } catch (err) {
      setErrorForm(
        err instanceof ErrorApiAscenso
          ? err.message
          : "No se pudo crear el programa. Verifica los datos ingresados."
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
      titulo="Programas de becas"
      estadoBadge={<Estado texto={`${programas.length} programa(s)`} tono="info" />}
      subtitulo="Configura los programas, su alcance, presupuesto, aportes de donantes y condiciones de permanencia."
      acciones={
        puedeCrear ? (
          <Boton
            variante="primarioSobreColor"
            icono={<Plus className="size-4" aria-hidden="true" />}
            onClick={() => {
              setErrorForm(null);
              setModalAbierto(true);
            }}
          >
            Nuevo programa
          </Boton>
        ) : null
      }
    >
      <div className="flex flex-col gap-8">
        {errorCarga ? <Aviso tono="alerta">{errorCarga}</Aviso> : null}

        <SeccionPanel titulo="Catálogo de programas">
          {programas.length === 0 ? (
            <div className="py-10 text-[14px] text-[var(--color-text-secondary)]">
              Todavía no hay programas registrados en S2.
              {puedeCrear ? " Usa «Nuevo programa» para crear el primero." : ""}
            </div>
          ) : (
            <div className="divide-y divide-[var(--color-border-default)]">
              {programas.map((p) => (
                <div
                  key={p.id}
                  className="flex flex-col justify-between gap-4 py-5 lg:flex-row lg:items-center"
                >
                  <div className="min-w-0 flex-1">
                    <div className="flex flex-wrap items-center gap-3">
                      <Link
                        href={`/programas/${p.id}`}
                        className="font-display text-[20px] font-semibold leading-[26px] text-[var(--color-text-primary)] hover:text-[var(--color-text-brand)]"
                      >
                        {p.nombre}
                      </Link>
                      <Estado
                        texto={etiquetaAlcance(p.alcance)}
                        tono={p.alcance === "COMPLETO" ? "exito" : "acento"}
                      />
                      {p.permiteAcumulacion ? (
                        <Estado texto="Acumulable" tono="info" />
                      ) : null}
                    </div>
                    <p className="mt-1 text-[14px] leading-[22px] text-[var(--color-text-secondary)]">
                      {formatearMoneda(p.montoPorPeriodo, p.moneda)} por periodo ·{" "}
                      {etiquetaModalidadPago(p.modalidadPago)} · Duración: {p.duracionPeriodos}{" "}
                      periodos · Inicio {p.periodoInicio}
                    </p>
                  </div>

                  <div className="flex items-center gap-6">
                    <div className="text-right">
                      <p className="text-[12px] leading-[18px] text-[var(--color-text-muted)]">
                        Cupo total
                      </p>
                      <p className="text-[14px] font-semibold text-[var(--color-text-primary)]">
                        {p.cupoBecarios} becarios
                      </p>
                    </div>

                    <Link
                      href={`/programas/${p.id}`}
                      className="ascenso-focus inline-flex items-center gap-1.5 rounded-[var(--radius-md)] border border-[var(--color-border-strong)] px-4 py-2.5 text-[14px] font-semibold text-[var(--color-text-primary)] transition-colors hover:bg-[var(--color-bg-subtle)]"
                    >
                      <span>Presupuesto y detalle</span>
                      <ArrowRight className="size-4" aria-hidden="true" />
                    </Link>
                  </div>
                </div>
              ))}
            </div>
          )}
        </SeccionPanel>
      </div>

      <Modal
        abierto={modalAbierto}
        onCerrar={() => setModalAbierto(false)}
        titulo="Crear programa de becas"
        descripcion="Define los parámetros base del programa (RF-11). Los programas completos no admiten acumulación."
      >
        <form onSubmit={crearPrograma} className="flex flex-col gap-4">
          {errorForm ? <Aviso tono="peligro">{errorForm}</Aviso> : null}

          <Campo
            etiqueta="Nombre del programa"
            placeholder="Ej. Becas Ingeniería"
            value={nombre}
            onChange={(e) => setNombre(e.target.value)}
            required
          />

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            {periodos.length > 0 ? (
              <CampoSelect
                etiqueta="Periodo de inicio"
                value={periodoInicio}
                onChange={(e) => setPeriodoInicio(e.target.value)}
                opciones={periodos.map((per) => ({
                  valor: per.codigo,
                  etiqueta: `${per.codigo}${per.esActual ? " (Actual)" : ""}`,
                }))}
                required
              />
            ) : (
              <Campo
                etiqueta="Periodo de inicio"
                placeholder="2026-1"
                value={periodoInicio}
                onChange={(e) => setPeriodoInicio(e.target.value)}
                required
              />
            )}

            <Campo
              etiqueta="Cupo de becarios"
              type="number"
              min={1}
              value={cupoBecarios}
              onChange={(e) => setCupoBecarios(e.target.value)}
              required
            />
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <Campo
              etiqueta="Duración (periodos)"
              type="number"
              min={1}
              value={duracionPeriodos}
              onChange={(e) => setDuracionPeriodos(e.target.value)}
              required
            />
            <Campo
              etiqueta="Monto por periodo"
              type="number"
              min={1}
              step="0.01"
              value={montoPorPeriodo}
              onChange={(e) => setMontoPorPeriodo(e.target.value)}
              required
            />
            <Campo
              etiqueta="Moneda"
              maxLength={3}
              value={moneda}
              onChange={(e) => setMoneda(e.target.value)}
              required
            />
          </div>

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <CampoSelect
              etiqueta="Modalidad de pago"
              value={modalidadPago}
              onChange={(e) => setModalidadPago(e.target.value as ModalidadPago)}
              opciones={[
                { valor: "SEMESTRAL", etiqueta: "Semestral" },
                { valor: "MENSUAL", etiqueta: "Mensual" },
                { valor: "DIRECTO_UNIVERSIDAD", etiqueta: "Directo a universidad" },
              ]}
            />
            <CampoSelect
              etiqueta="Alcance"
              value={alcance}
              onChange={(e) => cambiarAlcance(e.target.value as AlcancePrograma)}
              opciones={[
                { valor: "COMPLETO", etiqueta: "Completo (cubre matrícula total)" },
                { valor: "PARCIAL", etiqueta: "Parcial (aporte complementario)" },
              ]}
            />
          </div>

          <label className="flex items-start gap-3 rounded-[var(--radius-md)] border border-[var(--color-border-default)] bg-[var(--color-bg-muted)] p-3 text-[13px] leading-[20px] text-[var(--color-text-secondary)]">
            <input
              type="checkbox"
              checked={alcance === "COMPLETO" ? false : permiteAcumulacion}
              disabled={alcance === "COMPLETO"}
              onChange={(e) => setPermiteAcumulacion(e.target.checked)}
              className="mt-1 size-4 accent-[var(--color-bg-brand)]"
            />
            <span>
              <strong className="text-[var(--color-text-primary)]">
                Permite acumulación con otras becas parciales
              </strong>
              <br />
              {alcance === "COMPLETO"
                ? "Deshabilitado automáticamente: un programa completo no permite acumulación (RF-11)."
                : "Permite que el becario combine este beneficio con otro programa parcial compatible."}
            </span>
          </label>

          <div className="mt-2 flex justify-end gap-3">
            <Boton variante="secundario" onClick={() => setModalAbierto(false)}>
              Cancelar
            </Boton>
            <Boton type="submit" variante="primario" cargando={guardando}>
              Crear programa
            </Boton>
          </div>
        </form>
      </Modal>
    </FranjaEstado>
  );
}
