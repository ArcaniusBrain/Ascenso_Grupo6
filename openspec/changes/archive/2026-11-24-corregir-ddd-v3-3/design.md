# Design

## Context

Ver proposal.md. La v1.3 declara el DDD v3.2 como fuente y no lo modifica; su adenda pide actualizar solo la ubicación de S2/S4 en §3.1 y el Context Map.

## Goals / Non-Goals

**Goals:**
- Que el DDD v3.3 y la v1.3 no se contradigan.
- Que cada regla nueva cite su fuente (HU, R, respuesta de la Ronda 5).

**Non-Goals:**
- Rediseñar agregados o fronteras: la partición S1–S6 de la v1.3 no cambia.
- Resolver preguntas del cliente sin su respuesta.

## Decisions

- **Archivo nuevo `DDD_v3.3.md`; la v3.2 se conserva**, porque la v1.3 y los PDF entregados la citan.
- **Versión de la raíz explícita**, incrementada por la raíz en cualquier operación del agregado (también cuando solo cambia una entidad interna). Coincide con la v1.3 §7.1 («versión por raíz») y la completa.
- **Un solo disparador para *Inactiva***: el cliente confirmó la fecha de cierre (R38), así que se conserva el proceso programado de la v1.3 §9 y el DDD retira la transición de `IniciarNuevoPeriodo()`.
- **R12 con estado `Aplazado` del desembolso programado**: `Programado → Aplazado` al suspender si la fecha cae dentro de la suspensión; `Aplazado → Programado` al reanudar.
- **`IServicioOtp` con la firma de la v1.3** (`usuarioId`, operación, `recursoId`, `challengeId`), para que dominio y arquitectura usen el mismo contrato.
- **Registro de cuenta (flujo 16.4f):** S1 crea el usuario en Keycloak (fuente de verdad de la identidad y la cédula única) y publica `UsuarioRegistrado`; S3 crea el `Postulante` al consumirlo, de forma idempotente.

## Risks / Trade-offs

- [El cliente responde distinto sobre *Inactiva* o la pausa] → Cada decisión queda en un solo lugar del DDD; cambiarla es una edición pequeña.
- [Cambios en el Shared Kernel] → Pull request revisado por todo el equipo.

## Open Questions

- B5 (rol exacto que aprueba la migración) y la clasificación del estado de matrícula (C10), que define el equipo.
