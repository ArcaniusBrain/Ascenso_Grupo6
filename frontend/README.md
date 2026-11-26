# Frontend ASCENSO (Next.js App Router + BFF · ADR-28)

Aplicación web de la plataforma **ASCENSO** construida con **Next.js App Router**, **React 19**, **TypeScript**, **Tailwind CSS v4** y **npm**.

## Requisitos previos

- Node.js 22+ y `npm`.

## Arranque rápido en local

```bash
cd frontend
cp .env.example .env.local
npm ci
npm run dev
```

Abre `http://localhost:3000`.

## Configuración de entorno (`.env.local`)

- `ASCENSO_SESSION_SECRET`: clave aleatoria de al menos 32 caracteres para cifrar (AES-256-GCM) las cookies `HttpOnly` de sesión.
- `ASCENSO_GATEWAY_URL`: URL del API Gateway (YARP). Si se define, todas las rutas `/api/v1/*` salen por YARP.
- `ASCENSO_S1_URL` .. `ASCENSO_S6_URL`: overrides opcionales por microservicio para desarrollo local sin YARP (solo aplican cuando `ASCENSO_GATEWAY_URL` está vacío).

> Nunca subas `.env.local` al repositorio (está bloqueado por `.gitignore` y por `pr-guardrails.yml`).

## Reglas obligatorias para todo el equipo (ADR-28)

1. **Cero tokens en el navegador:** prohibido usar `localStorage` o `sessionStorage` para JWTs. La sesión vive sellada en la cookie `HttpOnly` `ascenso_session`.
2. **Siempre a través del BFF:**
   - Desde **Server Components**: usa `llamarBackendDesdeBff(...)` en `src/lib/bff/proxy.ts`.
   - Desde **Client Components**: usa `llamarApiBff("/api/v1/...")` en `src/lib/bff/client.ts` (pasa por `/api/bff/[...path]`).
3. **Nunca rutas `/internal/*`:** el BFF bloquea automáticamente con HTTP `403` cualquier intento de acceder a fronteras privadas entre servicios.
4. **Cero reglas de negocio o cálculos financieros en el frontend:** los montos, cupos, estados y validaciones autoritativas vienen siempre del backend.
5. **Reutiliza el sistema visual:** importa los componentes de `src/components/ui/` y el shell `FranjaEstado` de `src/components/layout/`.

## Comandos de verificación

```bash
npm run lint       # ESLint
npm run typecheck  # TypeScript estricto
npm test           # Pruebas unitarias Vitest (sesión AES-256-GCM + proxy BFF)
npm run build      # Build de producción Next.js
```

Consulta la guía completa del equipo en [`docs/guias-integracion/06-frontend-nextjs-bff.md`](../docs/guias-integracion/06-frontend-nextjs-bff.md).
