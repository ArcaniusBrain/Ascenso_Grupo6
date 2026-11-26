import { describe, expect, it } from "vitest";
import {
  abrirPayload,
  enmascararCorreo,
  extraerUsuarioPublico,
  normalizarCanalCodigo,
  sellarPayload,
  sesionEstaVigente,
  type SesionSellada,
} from "@/lib/auth/session";

const SECRETO_PRUEBA = "secreto-prueba-ascenso-2026-con-mas-de-treinta-y-dos-bytes";

function crearSesionEjemplo(expiraEnUtc: string): SesionSellada {
  return {
    usuarioId: "11111111-1111-1111-1111-111111111111",
    correo: "coordinador@fundacionascenso.org",
    nombreCompleto: "Luis Herrera",
    roles: ["COORDINADOR"],
    permisos: ["programas.ver", "presupuesto.ver"],
    segundoFactorVerificado: true,
    selloSeguridad: "22222222-2222-2222-2222-222222222222",
    accessToken: "jwt-secreto-que-nunca-debe-salir-al-navegador",
    expiraEnUtc,
  };
}

describe("Sesión BFF sellada con AES-256-GCM (ADR-28)", () => {
  it("sella y abre una sesión íntegra y nunca expone accessToken en el perfil público", () => {
    const futura = new Date(Date.now() + 60 * 60 * 1000).toISOString();
    const sesion = crearSesionEjemplo(futura);

    const tokenSellado = sellarPayload(sesion, SECRETO_PRUEBA);
    expect(tokenSellado).not.toContain(sesion.accessToken);

    const abierta = abrirPayload<SesionSellada>(tokenSellado, SECRETO_PRUEBA);
    expect(abierta).toEqual(sesion);
    expect(sesionEstaVigente(abierta)).toBe(true);

    const publico = extraerUsuarioPublico(sesion);
    expect("accessToken" in publico).toBe(false);
    expect("selloSeguridad" in publico).toBe(false);
    expect(publico.nombreCompleto).toBe("Luis Herrera");
  });

  it("rechaza una cookie manipulada o firmada con otro secreto", () => {
    const futura = new Date(Date.now() + 60 * 60 * 1000).toISOString();
    const sellada = sellarPayload(crearSesionEjemplo(futura), SECRETO_PRUEBA);

    const alterada = `${sellada.slice(0, -2)}AA`;
    expect(abrirPayload(alterada, SECRETO_PRUEBA)).toBeNull();
    expect(abrirPayload(sellada, "otro-secreto-completamente-distinto-32-bytes")).toBeNull();
  });

  it("detecta sesiones expiradas y enmascara correos correctamente", () => {
    const vencida = new Date(Date.now() - 5000).toISOString();
    expect(sesionEstaVigente({ expiraEnUtc: vencida })).toBe(false);
    expect(enmascararCorreo("coordinador@fundacionascenso.org")).toBe("c•••••@fundacionascenso.org");
    expect(normalizarCanalCodigo(2)).toBe("SMS");
    expect(normalizarCanalCodigo("CORREO")).toBe("CORREO");
  });
});
