/**
 * Formateadores visuales de presentación (sin reglas de negocio ni cálculos transaccionales).
 */

export function formatearMoneda(monto: number, moneda: string = "USD"): string {
  const simbolo = moneda.toUpperCase() === "USD" ? "$" : moneda.toUpperCase();
  const tieneDecimales = Math.abs(monto % 1) > 0.0001;
  const numero = new Intl.NumberFormat("es-VE", {
    minimumFractionDigits: tieneDecimales ? 2 : 0,
    maximumFractionDigits: 2,
  }).format(monto);

  return `${simbolo} ${numero}`;
}

export function formatearFechaCorta(iso: string): string {
  const fecha = new Date(iso);
  if (Number.isNaN(fecha.getTime())) {
    return iso;
  }
  return new Intl.DateTimeFormat("es-VE", {
    day: "2-digit",
    month: "short",
    year: "numeric",
  }).format(fecha);
}

export function etiquetaModalidadPago(modalidad: string): string {
  switch (modalidad.toUpperCase()) {
    case "MENSUAL":
      return "Pago mensual";
    case "SEMESTRAL":
      return "Pago semestral";
    case "DIRECTO_UNIVERSIDAD":
      return "Pago directo a universidad";
    default:
      return modalidad;
  }
}

export function etiquetaAlcance(alcance: string): string {
  return alcance.toUpperCase() === "COMPLETO" ? "Programa completo" : "Programa parcial";
}

export function etiquetaTipoDonante(tipo: string): string {
  return tipo.toUpperCase() === "ORGANIZACION" ? "Organización" : "Persona";
}

export function calcularBecasEstimadasDisponibles(
  presupuestoDisponible: number,
  montoPorPeriodo: number,
  duracionPeriodos: number
): number {
  const costoTotalBeca = montoPorPeriodo * duracionPeriodos;
  if (costoTotalBeca <= 0 || presupuestoDisponible <= 0) {
    return 0;
  }
  return Math.floor(presupuestoDisponible / costoTotalBeca);
}
