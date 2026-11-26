# Proposal

## Why

Tras fusionar los servicios **S1** y **S2** en `main`, la revisión de integración del incremento previo de **RF-18 (`HU-18.1`)** en **S4 (`Ascenso.S4.Becas`)** identificó seis correcciones necesarias para quedar alineado con **RF-37**, **ADR-02**, **ADR-05** (schema aislado `s4_becas` en `deploy/postgres/init-schemas.sql`), **ADR-12**, **ADR-27** y OpenAPI 3.1: resolver conflictos en `Ascenso.sln`, usar el schema `s4_becas`, habilitar PostgreSQL y autenticación/autorización en runtime, permitir acceso al rol **Dirección**, filtrar la consulta por periodo directamente en SQL y ajustar sintaxis de `contracts/openapi.yaml` junto con el orden de archivos de rama.

## What Changes

- Resolver `Ascenso.sln` partiendo de la versión limpia de `main` (con `S1` y `S2`) y re-registrando los 7 proyectos de `S4`.
- Cambiar el schema de S4 de `"s4"` a `"s4_becas"` en `BecaDbContext` y la migración inicial `20261125183000_InicialS4Becas`, aplicando `HasDefaultSchema("s4_becas")` únicamente cuando el proveedor de base de datos sea Npgsql.
- Configurar el arranque en runtime de `Ascenso.S4.Becas.Api` leyendo `ConnectionStrings:BecasDb` con `UseNpgsql` (siguiendo el patrón de S2) y registrando `AddAuthentication()` y `AddAuthorization()`.
- Habilitar el rol `DIRECCION` en `RolHistorialBecas`, `PoliticaAccesoHistorialBecas` y `HistorialBecasController` (dejando `FINANZAS` pendiente de definición del cliente / Anexo B #1).
- Optimizar `ConsultaHistorialBecasEfCore.ListarPorPeriodoAsync` para filtrar en SQL mediante `Where(b => b.Tramos.Any(t => t.Periodo == codigo))` en lugar de materializar todas las becas en memoria.
- Actualizar `nullable: true` a sintaxis OpenAPI 3.1 (`type: ['string', 'null']`) en `contracts/openapi.yaml` y mantener `services/Ascenso.S4.Becas/INTEGRACION_EQUIPO.md` limpio en esta rama sin mezclar `docs/ASCENSO — División de requerimientos por integrante.md`.

## Capabilities

### New Capabilities
<!-- Ninguna capacidad nueva; se modifica únicamente la capacidad existente becas. -->

### Modified Capabilities
- `becas`: Habilitar la consulta de historial de becas por persona, programa y periodo para el rol Dirección (`HU-18.1`, `RF-37`).

## Impact

- **Servicios afectados:** únicamente **S4** (`services/Ascenso.S4.Becas/`). No se modifican `S1`, `S2`, `S3`, `S5` ni `S6`.
- **Bloques compartidos:** **no** se modifican `Ascenso.SharedKernel`, `Ascenso.Contracts` ni `Ascenso.Messaging.Outbox`.
- **Solución y contratos:** `Ascenso.sln` y `contracts/openapi.yaml`.
