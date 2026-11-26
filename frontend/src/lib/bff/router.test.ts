import { describe, expect, it } from "vitest";
import { resolverDestinoBackend } from "@/lib/bff/router";
import { filtrarNavegacionPorRoles, obtenerIniciales } from "@/lib/navegacion";
import type { ConfiguracionBff } from "@/lib/config/env";

const CONFIG_SERVICIOS: ConfiguracionBff = {
  sessionSecret: "secreto-prueba-ascenso-2026-con-mas-de-treinta-y-dos-bytes",
  gatewayUrl: null,
  esProduccion: false,
  servicios: {
    S1: "http://s1.interno:5101",
    S2: "http://s2.interno:5102",
    S3: "http://s3.interno:5103",
    S4: "http://s4.interno:5104",
    S5: "http://s5.interno:5105",
    S6: "http://s6.interno:5106",
  },
};

describe("Enrutamiento BFF y protección de fronteras privadas (ADR-28)", () => {
  it("bloquea siempre rutas privadas /internal/* y path traversal con 403", () => {
    const r1 = resolverDestinoBackend("/internal/v1/fondos/reservas", "", CONFIG_SERVICIOS);
    expect(r1.ok).toBe(false);
    if (!r1.ok) {
      expect(r1.status).toBe(403);
      expect(r1.codigo).toBe("FRONTERA_PRIVADA_BLOQUEADA");
    }

    const r2 = resolverDestinoBackend("/api/v1/programas/../../internal/v1/otp/validar-y-consumir", "", CONFIG_SERVICIOS);
    expect(r2.ok).toBe(false);
    if (!r2.ok) {
      expect(r2.status).toBe(403);
    }
  });

  it("enruta por YARP cuando ASCENSO_GATEWAY_URL está definido", () => {
    const conGateway: ConfiguracionBff = {
      ...CONFIG_SERVICIOS,
      gatewayUrl: "https://api.ascenso.org",
    };

    const res = resolverDestinoBackend("/api/v1/programas", "?activo=true", conGateway);
    expect(res).toEqual({
      ok: true,
      servicio: "GATEWAY",
      urlDestino: "https://api.ascenso.org/api/v1/programas?activo=true",
    });
  });

  it("enruta por microservicio configurado cuando no hay gateway en desarrollo local", () => {
    const s2 = resolverDestinoBackend("/api/v1/periodos/actual", "", CONFIG_SERVICIOS);
    expect(s2).toEqual({
      ok: true,
      servicio: "S2",
      urlDestino: "http://s2.interno:5102/api/v1/periodos/actual",
    });

    const s6 = resolverDestinoBackend("/api/v1/reportes/impacto", "", CONFIG_SERVICIOS);
    expect(s6).toEqual({
      ok: true,
      servicio: "S6",
      urlDestino: "http://s6.interno:5106/api/v1/reportes/impacto",
    });
  });

  it("filtra la navegación según el rol del usuario", () => {
    const navBecario = filtrarNavegacionPorRoles(["BECARIO"]).map((x) => x.id);
    expect(navBecario).toEqual(["inicio", "becas"]);

    const navDir = filtrarNavegacionPorRoles(["DIRECCION"]).map((x) => x.id);
    expect(navDir).toContain("programas");
    expect(navDir).toContain("usuarios");
    expect(navDir).toContain("calendario");
    expect(obtenerIniciales("Luis Herrera")).toBe("LH");
  });
});
