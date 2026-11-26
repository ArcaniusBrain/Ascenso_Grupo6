"use client";

import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import { Plus, ShieldCheck, Trash2, UserCog } from "lucide-react";
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
  etiquetaCanalPreferido,
  etiquetaEstadoUsuario,
  type TipoUsuarioDto,
  type UsuarioDto,
} from "@/lib/contratos/s1";
import type { UsuarioSesionPublico } from "@/lib/auth/session";

export function UsuariosVista({
  usuario,
  usuariosIniciales,
  tiposIniciales,
  periodoActual,
  errorCarga,
}: {
  usuario: UsuarioSesionPublico;
  usuariosIniciales: UsuarioDto[];
  tiposIniciales: TipoUsuarioDto[];
  periodoActual: string | null;
  errorCarga?: string | null;
}) {
  const router = useRouter();
  const [usuarios, setUsuarios] = useState(usuariosIniciales);
  const [tipos, setTipos] = useState(tiposIniciales);

  // Modales
  const [modalTipo, setModalTipo] = useState(false);
  const [modalEditarPermisos, setModalEditarPermisos] = useState<TipoUsuarioDto | null>(null);
  const [modalUsuario, setModalUsuario] = useState(false);
  const [modalRoles, setModalRoles] = useState<UsuarioDto | null>(null);

  // Campos tipo de usuario
  const [tNombre, setTNombre] = useState("");
  const [tPermisos, setTPermisos] = useState("");
  const [tFinanciero, setTFinanciero] = useState(false);
  const [tEnlace, setTEnlace] = useState(false);

  // Campos nuevo usuario interno
  const [uCedula, setUCedula] = useState("");
  const [uNombre, setUNombre] = useState("");
  const [uCorreo, setUCorreo] = useState("");
  const [uTelefono, setUTelefono] = useState("");
  const [uCanal, setUCanal] = useState<"CORREO" | "SMS">("CORREO");
  const [uTiposSeleccionados, setUTiposSeleccionados] = useState<string[]>([]);
  const [uEsExterno, setUEsExterno] = useState(false);
  const [uClaveTemp, setUClaveTemp] = useState("");

  // Campos cambio de roles
  const [rolesSeleccionados, setRolesSeleccionados] = useState<string[]>([]);

  const [guardando, setGuardando] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [exitoMsg, setExitoMsg] = useState<string | null>(null);

  async function recargarTodo() {
    const [listaUsuarios, listaTipos] = await Promise.all([
      llamarApiBff<UsuarioDto[]>("/api/v1/usuarios"),
      llamarApiBff<TipoUsuarioDto[]>("/api/v1/tipos-usuario"),
    ]);
    setUsuarios(listaUsuarios);
    setTipos(listaTipos);
    router.refresh();
  }

  function alternarId(lista: string[], id: string): string[] {
    return lista.includes(id) ? lista.filter((x) => x !== id) : [...lista, id];
  }

  async function crearTipoUsuario(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrorMsg(null);
    setGuardando(true);
    try {
      const permisos = tPermisos
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean);

      await llamarApiBff<TipoUsuarioDto>("/api/v1/tipos-usuario", {
        method: "POST",
        body: {
          nombre: tNombre.trim(),
          permisos,
          accesoFinancieroODecision: tFinanciero,
          esEnlaceUniversidad: tEnlace,
        },
      });
      setModalTipo(false);
      setTNombre("");
      setTPermisos("");
      setTFinanciero(false);
      setTEnlace(false);
      setExitoMsg("Tipo de usuario creado correctamente.");
      await recargarTodo();
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo crear el tipo de usuario."
      );
    } finally {
      setGuardando(false);
    }
  }

  async function guardarPermisosTipo(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!modalEditarPermisos) return;
    setErrorMsg(null);
    setGuardando(true);
    try {
      const permisos = tPermisos
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean);

      await llamarApiBff(`/api/v1/tipos-usuario/${modalEditarPermisos.id}/permisos`, {
        method: "PUT",
        body: {
          permisos,
          accesoFinancieroODecision: tFinanciero,
        },
      });
      setModalEditarPermisos(null);
      setExitoMsg("Permisos del tipo de usuario actualizados.");
      await recargarTodo();
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso ? err.message : "No se pudieron actualizar los permisos."
      );
    } finally {
      setGuardando(false);
    }
  }

  async function eliminarTipo(tipoId: string) {
    setErrorMsg(null);
    setGuardando(true);
    try {
      await llamarApiBff(`/api/v1/tipos-usuario/${tipoId}`, { method: "DELETE" });
      setExitoMsg("Tipo de usuario eliminado.");
      await recargarTodo();
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo eliminar el tipo de usuario."
      );
    } finally {
      setGuardando(false);
    }
  }

  async function crearUsuarioInterno(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrorMsg(null);
    setGuardando(true);
    try {
      await llamarApiBff<UsuarioDto>("/api/v1/usuarios", {
        method: "POST",
        body: {
          cedula: uCedula.trim(),
          nombreCompleto: uNombre.trim(),
          correo: uCorreo.trim(),
          telefono: uTelefono.trim(),
          tipoUsuarioIds: uTiposSeleccionados,
          canalPreferido: uCanal === "SMS" ? 2 : 1,
          esEvaluadorExterno: uEsExterno,
          contrasenaTemporal: uClaveTemp.trim() || null,
        },
      });
      setModalUsuario(false);
      setUCedula("");
      setUNombre("");
      setUCorreo("");
      setUTelefono("");
      setUClaveTemp("");
      setUTiposSeleccionados([]);
      setExitoMsg("Usuario interno creado y sincronizado con identidad (S1).");
      await recargarTodo();
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso ? err.message : "No se pudo crear el usuario interno."
      );
    } finally {
      setGuardando(false);
    }
  }

  async function guardarRolesUsuario(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!modalRoles) return;
    setErrorMsg(null);
    setGuardando(true);
    try {
      await llamarApiBff(`/api/v1/usuarios/${modalRoles.id}/roles`, {
        method: "PUT",
        body: { tipoUsuarioIds: rolesSeleccionados },
      });
      setModalRoles(null);
      setExitoMsg("Roles del usuario actualizados.");
      await recargarTodo();
    } catch (err) {
      setErrorMsg(
        err instanceof ErrorApiAscenso ? err.message : "No se pudieron actualizar los roles."
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
      titulo="Usuarios y roles"
      estadoBadge={
        <Estado
          texto={`${usuarios.length} usuario(s) · ${tipos.length} tipo(s)`}
          tono="info"
        />
      }
      subtitulo="Configura tipos de usuario, permisos, cuentas internas y políticas de segundo factor obligatorio (RF-37)."
      acciones={
        <>
          <Boton
            variante="secundarioSobreColor"
            icono={<ShieldCheck className="size-4" aria-hidden="true" />}
            onClick={() => {
              setErrorMsg(null);
              setTNombre("");
              setTPermisos("");
              setTFinanciero(false);
              setTEnlace(false);
              setModalTipo(true);
            }}
          >
            Nuevo tipo de usuario
          </Boton>
          <Boton
            variante="primarioSobreColor"
            icono={<Plus className="size-4" aria-hidden="true" />}
            onClick={() => {
              setErrorMsg(null);
              setModalUsuario(true);
            }}
          >
            Crear usuario interno
          </Boton>
        </>
      }
    >
      <div className="flex flex-col gap-10">
        {errorCarga ? <Aviso tono="alerta">{errorCarga}</Aviso> : null}
        {errorMsg ? <Aviso tono="peligro">{errorMsg}</Aviso> : null}
        {exitoMsg ? <Aviso tono="exito">{exitoMsg}</Aviso> : null}

        <div className="grid grid-cols-1 gap-16 lg:grid-cols-[1fr_420px]">
          <SeccionPanel titulo="Usuarios registrados">
            {usuarios.length === 0 ? (
              <p className="py-8 text-[14px] text-[var(--color-text-secondary)]">
                No hay usuarios registrados en S1 todavía.
              </p>
            ) : (
              <div className="divide-y divide-[var(--color-border-default)]">
                {usuarios.map((u) => (
                  <div
                    key={u.id}
                    className="flex flex-col justify-between gap-3 py-4 sm:flex-row sm:items-center"
                  >
                    <div className="min-w-0 flex-1">
                      <div className="flex flex-wrap items-center gap-2">
                        <span className="text-[15px] font-semibold text-[var(--color-text-primary)]">
                          {u.nombreCompleto}
                        </span>
                        <Estado
                          texto={etiquetaEstadoUsuario(u.estado)}
                          tono={etiquetaEstadoUsuario(u.estado) === "Activo" ? "exito" : "neutro"}
                        />
                        {u.requiereSegundoFactor ? (
                          <Estado texto="2FA obligatorio" tono="acento" />
                        ) : null}
                      </div>
                      <p className="mt-1 text-[13px] text-[var(--color-text-secondary)]">
                        {u.cedula} · {u.correo} · {u.telefono} · Canal{" "}
                        {etiquetaCanalPreferido(u.canalPreferido)}
                      </p>
                      <div className="mt-2 flex flex-wrap gap-1.5">
                        {u.nombresRoles.map((rol) => (
                          <Estado key={rol} texto={rol} tono="info" />
                        ))}
                      </div>
                    </div>

                    <Boton
                      variante="secundario"
                      icono={<UserCog className="size-4" aria-hidden="true" />}
                      onClick={() => {
                        setErrorMsg(null);
                        setRolesSeleccionados(u.tipoUsuarioIds);
                        setModalRoles(u);
                      }}
                      className="h-[38px] px-3 text-[13px]"
                    >
                      Roles
                    </Boton>
                  </div>
                ))}
              </div>
            )}
          </SeccionPanel>

          <aside>
            <SeccionPanel titulo="Tipos de usuario">
              {tipos.length === 0 ? (
                <p className="py-6 text-[14px] text-[var(--color-text-secondary)]">
                  Sin tipos de usuario configurados.
                </p>
              ) : (
                <div className="divide-y divide-[var(--color-border-default)]">
                  {tipos.map((t) => (
                    <div key={t.id} className="flex flex-col gap-2 py-4">
                      <div className="flex items-center justify-between gap-2">
                        <span className="text-[15px] font-semibold text-[var(--color-text-primary)]">
                          {t.nombre}
                        </span>
                        <div className="flex items-center gap-1">
                          {t.exigeSegundoFactor ? (
                            <Estado texto="2FA" tono="acento" />
                          ) : null}
                          {t.esPredefinido ? <Estado texto="Predefinido" tono="neutro" /> : null}
                        </div>
                      </div>
                      <p className="text-[12px] text-[var(--color-text-secondary)]">
                        Permisos: {t.permisos.join(", ") || "Sin permisos"}
                      </p>
                      <div className="flex items-center gap-2 pt-1">
                        <Boton
                          variante="terciario"
                          onClick={() => {
                            setErrorMsg(null);
                            setTPermisos(t.permisos.join(", "));
                            setTFinanciero(t.accesoFinancieroODecision);
                            setModalEditarPermisos(t);
                          }}
                          className="h-[34px] px-2 text-[13px]"
                        >
                          Editar permisos
                        </Boton>
                        {!t.esPredefinido ? (
                          <Boton
                            variante="terciario"
                            icono={<Trash2 className="size-3.5" aria-hidden="true" />}
                            onClick={() => eliminarTipo(t.id)}
                            disabled={guardando}
                            className="h-[34px] px-2 text-[13px] text-[var(--color-text-danger)] hover:bg-[var(--color-bg-danger-soft)]"
                          >
                            Eliminar
                          </Boton>
                        ) : null}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </SeccionPanel>
          </aside>
        </div>
      </div>

      {/* Modal: Nuevo tipo de usuario */}
      <Modal
        abierto={modalTipo}
        onCerrar={() => setModalTipo(false)}
        titulo="Crear tipo de usuario"
      >
        <form onSubmit={crearTipoUsuario} className="flex flex-col gap-4">
          <Campo
            etiqueta="Nombre del tipo de usuario"
            placeholder="Ej. Auditor interno"
            value={tNombre}
            onChange={(e) => setTNombre(e.target.value)}
            required
          />
          <Campo
            etiqueta="Permisos (separados por coma)"
            placeholder="programas.ver, reportes.impacto.ver"
            value={tPermisos}
            onChange={(e) => setTPermisos(e.target.value)}
            required
          />
          <label className="flex items-center gap-2 text-[13px] text-[var(--color-text-secondary)]">
            <input
              type="checkbox"
              checked={tFinanciero}
              onChange={(e) => setTFinanciero(e.target.checked)}
            />
            <span>Acceso financiero o de decisión (activa 2FA obligatorio)</span>
          </label>
          <label className="flex items-center gap-2 text-[13px] text-[var(--color-text-secondary)]">
            <input
              type="checkbox"
              checked={tEnlace}
              onChange={(e) => setTEnlace(e.target.checked)}
            />
            <span>Es enlace universitario (activa 2FA obligatorio)</span>
          </label>
          <div className="mt-2 flex justify-end gap-3">
            <Boton variante="secundario" onClick={() => setModalTipo(false)}>
              Cancelar
            </Boton>
            <Boton type="submit" variante="primario" cargando={guardando}>
              Crear tipo
            </Boton>
          </div>
        </form>
      </Modal>

      {/* Modal: Editar permisos */}
      <Modal
        abierto={Boolean(modalEditarPermisos)}
        onCerrar={() => setModalEditarPermisos(null)}
        titulo={`Permisos · ${modalEditarPermisos?.nombre ?? ""}`}
      >
        <form onSubmit={guardarPermisosTipo} className="flex flex-col gap-4">
          <Campo
            etiqueta="Permisos (separados por coma)"
            value={tPermisos}
            onChange={(e) => setTPermisos(e.target.value)}
            required
          />
          <label className="flex items-center gap-2 text-[13px] text-[var(--color-text-secondary)]">
            <input
              type="checkbox"
              checked={tFinanciero}
              onChange={(e) => setTFinanciero(e.target.checked)}
            />
            <span>Acceso financiero o de decisión</span>
          </label>
          <div className="mt-2 flex justify-end gap-3">
            <Boton variante="secundario" onClick={() => setModalEditarPermisos(null)}>
              Cancelar
            </Boton>
            <Boton type="submit" variante="primario" cargando={guardando}>
              Guardar permisos
            </Boton>
          </div>
        </form>
      </Modal>

      {/* Modal: Crear usuario interno */}
      <Modal
        abierto={modalUsuario}
        onCerrar={() => setModalUsuario(false)}
        titulo="Crear usuario interno"
        descripcion="Valida cédula y correo únicos en S1 y sincroniza credenciales en Keycloak."
      >
        <form onSubmit={crearUsuarioInterno} className="flex flex-col gap-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Campo
              etiqueta="Cédula"
              placeholder="V-25123456"
              value={uCedula}
              onChange={(e) => setUCedula(e.target.value)}
              required
            />
            <Campo
              etiqueta="Nombre completo"
              placeholder="Ana Gómez"
              value={uNombre}
              onChange={(e) => setUNombre(e.target.value)}
              required
            />
          </div>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Campo
              etiqueta="Correo electrónico"
              type="email"
              value={uCorreo}
              onChange={(e) => setUCorreo(e.target.value)}
              required
            />
            <Campo
              etiqueta="Teléfono"
              placeholder="+584121234567"
              value={uTelefono}
              onChange={(e) => setUTelefono(e.target.value)}
              required
            />
          </div>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <CampoSelect
              etiqueta="Canal preferido 2FA"
              value={uCanal}
              onChange={(e) => setUCanal(e.target.value as "CORREO" | "SMS")}
              opciones={[
                { valor: "CORREO", etiqueta: "Correo electrónico" },
                { valor: "SMS", etiqueta: "SMS" },
              ]}
            />
            <Campo
              etiqueta="Contraseña temporal (opcional)"
              type="password"
              value={uClaveTemp}
              onChange={(e) => setUClaveTemp(e.target.value)}
            />
          </div>

          <fieldset className="flex flex-col gap-2">
            <legend className="text-[13px] font-medium text-[var(--color-text-secondary)]">
              Roles asignados
            </legend>
            <div className="flex flex-wrap gap-2">
              {tipos.map((t) => (
                <label
                  key={t.id}
                  className="inline-flex items-center gap-2 rounded-[var(--radius-md)] border border-[var(--color-border-default)] px-3 py-2 text-[13px]"
                >
                  <input
                    type="checkbox"
                    checked={uTiposSeleccionados.includes(t.id)}
                    onChange={() =>
                      setUTiposSeleccionados((prev) => alternarId(prev, t.id))
                    }
                  />
                  <span>{t.nombre}</span>
                </label>
              ))}
            </div>
          </fieldset>

          <label className="flex items-center gap-2 text-[13px] text-[var(--color-text-secondary)]">
            <input
              type="checkbox"
              checked={uEsExterno}
              onChange={(e) => setUEsExterno(e.target.checked)}
            />
            <span>Evaluador externo</span>
          </label>

          <div className="mt-2 flex justify-end gap-3">
            <Boton variante="secundario" onClick={() => setModalUsuario(false)}>
              Cancelar
            </Boton>
            <Boton type="submit" variante="primario" cargando={guardando}>
              Crear usuario
            </Boton>
          </div>
        </form>
      </Modal>

      {/* Modal: Cambiar roles */}
      <Modal
        abierto={Boolean(modalRoles)}
        onCerrar={() => setModalRoles(null)}
        titulo={`Cambiar roles · ${modalRoles?.nombreCompleto ?? ""}`}
        descripcion="S1 bloquea el auto-escalamiento de privilegios y registra cada cambio en auditoría."
      >
        <form onSubmit={guardarRolesUsuario} className="flex flex-col gap-4">
          <div className="flex flex-col gap-2">
            {tipos.map((t) => (
              <label
                key={t.id}
                className="flex items-center justify-between gap-2 rounded-[var(--radius-md)] border border-[var(--color-border-default)] px-3 py-2.5 text-[14px]"
              >
                <span className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    checked={rolesSeleccionados.includes(t.id)}
                    onChange={() =>
                      setRolesSeleccionados((prev) => alternarId(prev, t.id))
                    }
                  />
                  <span className="font-medium text-[var(--color-text-primary)]">{t.nombre}</span>
                </span>
                {t.exigeSegundoFactor ? <Estado texto="2FA" tono="acento" /> : null}
              </label>
            ))}
          </div>
          <div className="mt-2 flex justify-end gap-3">
            <Boton variante="secundario" onClick={() => setModalRoles(null)}>
              Cancelar
            </Boton>
            <Boton type="submit" variante="primario" cargando={guardando}>
              Guardar roles
            </Boton>
          </div>
        </form>
      </Modal>
    </FranjaEstado>
  );
}
