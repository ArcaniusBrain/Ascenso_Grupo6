# Guía de Integración 06 — Frontend Next.js App Router y BFF (ADR-28)

Esta guía explica cómo funciona la base compartida de `frontend/` y cómo cada integrante del Grupo 6 agrega sus pantallas sin romper la sesión, la seguridad ni la identidad visual de Figma.

## 1. Arquitectura implementada (ADR-28)

```text
Navegador (React)
   │  Cookie HttpOnly sellada (ascenso_session, AES-256-GCM)
   ▼
Next.js App Router (BFF ligero)
   ├── /api/auth/*        → Flujo Login + 2FA (RF-37) contra S1
   ├── /api/bff/api/v1/*  → Proxy autenticado (inyecta Bearer + X-Actor-*)
   └── Bloqueo estricto   → 403 inmediato ante /internal/* o traversal (..)
   ▼
API Gateway YARP (ASCENSO_GATEWAY_URL)
   o microservicios locales (ASCENSO_S1_URL .. ASCENSO_S6_URL)
```

- **Nunca** hay tokens en `localStorage` ni `sessionStorage`.
- **Nunca** se devuelve `accessToken` al navegador.
- **Nunca** hay direcciones `localhost` hardcodeadas en código fuente; todo viene de `frontend/.env.local` (ver `frontend/.env.example`).

## 2. Dónde agregar una nueva pantalla

1. Crea tu ruta dentro de `frontend/src/app/(app)/<modulo>/page.tsx` (hereda automáticamente la validación server-side de sesión de `src/app/(app)/layout.tsx`).
2. Envuelve la vista con `FranjaEstado` (`src/components/layout/FranjaEstado.tsx`), eligiendo el tono según la escalera de avance de Figma:
   - `"inicio"` (`#0e2140` azul oscuro): inicio, catálogo, configuración.
   - `"proceso"` (`#0e5563` turquesa): evaluación, verificación, seguimiento en curso.
   - `"logrado"` (`#0e573f` verde): beca activa, programa con presupuesto, desembolso pagado.
3. Reutiliza los componentes base de `src/components/ui/`:
   - `Boton` (`primario`, `secundario`, `terciario`, `peligro`, `primarioSobreColor`, `secundarioSobreColor`)
   - `Campo` (etiqueta, ayuda, error)
   - `Estado` (`neutro`, `info`, `acento`, `exito`, `alerta`, `peligro`)
   - `Aviso` (`info`, `alerta`, `peligro`, `exito`)
   - `CasillasCodigo` (6 dígitos — reutilizable para **2FA** y para el **modal OTP de operaciones sensibles `ADR-10`**)
   - `SeccionPanel` y `FilaDato`
4. Si necesitas registrar una nueva sección en el menú superior, agrégala en `frontend/src/lib/navegacion.ts` indicando `rolesPermitidos`.

## 3. Cómo consumir tu microservicio desde una pantalla

### Opción A — Desde un Server Component (carga inicial autenticada)

```ts
import { llamarBackendDesdeBff } from "@/lib/bff/proxy";

const resp = await llamarBackendDesdeBff({ ruta: "/api/v1/programas" });
if (resp.ok) {
  const programas = JSON.parse(resp.bodyText);
}
```

### Opción B — Desde un Client Component (formularios, filtros, mutaciones)

```ts
import { llamarApiBff, ErrorApiAscenso } from "@/lib/bff/client";

try {
  const creado = await llamarApiBff("/api/v1/programas", {
    method: "POST",
    body: { nombre: "Beca Ingeniería" },
  });
} catch (err) {
  if (err instanceof ErrorApiAscenso) {
    // err.message contiene el `detail` de ProblemDetails devuelto por el backend
  }
}
```

## 4. Verificación obligatoria antes de abrir PR

```bash
cd frontend
npm run lint
npm run typecheck
npm test
npm run build
```

Además, recuerda incluir el cambio correspondiente en `openspec/` (ADR-23) y validar con `openspec validate --all --strict --no-interactive`.
