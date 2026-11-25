# Proposal

## Why

El repositorio ya cuenta con integración continua de `.NET 10` (`.github/workflows/dotnet.yml`), validación de OpenSpec (`.github/workflows/openspec.yml`) y protección de rama `main` en GitHub, pero faltan verificaciones automáticas a nivel de Pull Request que impidan la subida accidental de secretos o artefactos locales, hagan cumplir la obligatoriedad de OpenSpec cuando se modifica código (`ADR-23`), protejan la restricción de licencia de `MediatR` (`ADR-27`) y destaquen de forma visible cualquier cambio sobre zonas críticas compartidas (`building-blocks/`, `contracts/`, `deploy/postgres/init-schemas.sql` y `.github/`).

## What Changes

- Crear `.github/workflows/pr-guardrails.yml` con el check obligatorio **`Guardrails de PR`** para Pull Requests hacia `main` (archivos prohibidos y secretos, OpenSpec obligatorio al tocar código/contratos, validación de `MediatR < 13.0.0`, resumen de archivos críticos compartidos y verificación de descripción/referencias del PR).
- Enriquecer `.github/pull_request_template.md` con puntos clave de `ADR-25`, `ADR-28` (Next.js BFF / Figma) y una lista rápida para el revisor antes de aprobar.

## Capabilities

### New Capabilities

Ninguna (`skip_specs: true`: cambio exclusivo de CI y gobernanza de Pull Requests).

### Modified Capabilities

Ninguna.

## Impact

- **Servicios afectados:** ninguno en código de producto; aplica transversalmente a todos los PRs hacia `main` (`S1`–`S6`, `building-blocks/`, `contracts/`, `frontend/`, `gateway/`, `deploy/`).
- **No toca** `Ascenso.SharedKernel` ni `Ascenso.Contracts`.
- **Acción posterior en GitHub Settings:** agregar el nuevo check `Guardrails de PR` a la regla *Require status checks to pass* del ruleset de `main` tras la primera ejecución del PR.
