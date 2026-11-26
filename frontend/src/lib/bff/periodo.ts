import { llamarBackendDesdeBff } from "@/lib/bff/proxy";
import type { ListaPeriodosDto } from "@/lib/contratos/s2";

/**
 * Consulta no bloqueante del listado de periodos desde S2 para mostrar el periodo actual
 * en la barra de navegación y poblar selectores en las vistas de gestión.
 */
export async function cargarListaPeriodosServidor(): Promise<ListaPeriodosDto | null> {
  const resp = await llamarBackendDesdeBff({ ruta: "/api/v1/periodos" });
  if (!resp.ok || !resp.bodyText) {
    return null;
  }

  try {
    return JSON.parse(resp.bodyText) as ListaPeriodosDto;
  } catch {
    return null;
  }
}
