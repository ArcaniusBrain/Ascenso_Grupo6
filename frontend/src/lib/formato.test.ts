import { describe, expect, it } from "vitest";
import {
  calcularBecasEstimadasDisponibles,
  etiquetaAlcance,
  etiquetaModalidadPago,
  formatearMoneda,
} from "@/lib/formato";

describe("Formateadores visuales de ASCENSO", () => {
  it("formatea montos en dólares sin decimales innecesarios y conserva centavos cuando existen", () => {
    expect(formatearMoneda(60000, "USD")).toContain("$");
    expect(formatearMoneda(60000, "USD")).toContain("60");
    expect(formatearMoneda(1250.5, "USD")).toContain("50");
  });

  it("traduce enums de S2 a etiquetas legibles en español", () => {
    expect(etiquetaAlcance("COMPLETO")).toBe("Programa completo");
    expect(etiquetaAlcance("PARCIAL")).toBe("Programa parcial");
    expect(etiquetaModalidadPago("DIRECTO_UNIVERSIDAD")).toBe("Pago directo a universidad");
  });

  it("calcula el número entero de becas nuevas cubiertas con el saldo disponible", () => {
    expect(calcularBecasEstimadasDisponibles(6000, 500, 4)).toBe(3);
    expect(calcularBecasEstimadasDisponibles(0, 500, 4)).toBe(0);
    expect(calcularBecasEstimadasDisponibles(5000, 0, 4)).toBe(0);
  });
});
