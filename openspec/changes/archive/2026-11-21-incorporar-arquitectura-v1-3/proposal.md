# Proposal

## Why

El equipo cerró la arquitectura v1.3 (20-nov-2026), que reemplaza la propuesta v0.1 de 24 ADR que estaba en el repositorio. Varias piezas del repositorio contradecían la v1.3: la base de lectura (PostgreSQL en vez de MongoDB), la lista de llamadas síncronas (faltaba S3 → S4 para HU-01.2), la estructura de carpetas del código (`src/` en vez de `services/` y `building-blocks/`, §12) y el cambio `actualizar-ddd-v3-3-y-adr-v0-2`, cuya mitad de arquitectura quedó superada. Los asistentes leen `openspec/config.yaml`, así que hoy guiarían con decisiones viejas.

## What Changes

- La arquitectura v1.3 entra al repositorio en `docs/arquitectura/v1.3/`; la v0.1 pasa a `docs/arquitectura/historico/v0.1/`.
- La adenda DDD v3.3 (Programas y Fondos en S2, Becas en S4) pasa a `docs/dominio/DDD_v3.3_adenda_S2_S4.md`. El DDD v3.2 no cambia.
- La estructura de código sigue la v1.3 §12: `services/Ascenso.S1…S6`, `building-blocks/`, `gateway/`, `contracts/`. Se elimina `src/`.
- `openspec/config.yaml` se alinea con la v1.3: fuentes, MongoDB, lista cerrada de llamadas síncronas, Outbox/Inbox, estructura de carpetas y regla de actualizar contratos.
- Se retira el cambio `actualizar-ddd-v3-3-y-adr-v0-2` (sin tareas hechas) y se crea `corregir-ddd-v3-3` solo con lo que la v1.3 deja abierto en el dominio.
- `docs/arquitectura/pendientes-v1.3.md` reúne las preguntas y decisiones que siguen abiertas.
- README, CONTRIBUTING, guías, `CODEOWNERS`, `deploy/` y `frontend/` se actualizan a la v1.3.

## Capabilities

### New Capabilities

Ninguna. Es un cambio de documentación y estructura del repositorio (`skip_specs: true`).

### Modified Capabilities

Ninguna.

## Impact

- Documentos: `docs/arquitectura/`, `docs/dominio/`, `docs/guia-*.md`, `README.md`, `CONTRIBUTING.md`.
- Estructura: `services/`, `building-blocks/`, `gateway/`, `contracts/` (solo README; todavía sin código).
- OpenSpec: `openspec/config.yaml` y los cambios en curso.
- Servicios: ninguno con código todavía; afecta a cómo se construirán S1–S6.
