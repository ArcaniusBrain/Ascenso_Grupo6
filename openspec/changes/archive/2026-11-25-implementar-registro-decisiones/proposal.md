# Proposal

## Why

La trazabilidad inmutable de decisiones (**RF-35**: **HU-35.1** y **HU-35.2**) es una pieza de infraestructura de dominio y persistencia transversal que, según el Anexo A (`HU-35.1`, regla de secuenciación), debe construirse **antes** de implementar las historias que cambian el estado de postulaciones, becas, pagos o presupuestos en `S1`–`S5`. Dejarla para después obligaría a reescribir todos los casos de uso de los cinco integrantes del equipo.

Este cambio materializa:
- **HU-35.1** (*Registrar cada decisión con quién, cuándo y por qué, sin edición posterior*): exige registro obligatorio con autor, fecha, tipo, sujeto y justificación; atomicidad real con el cambio de estado; consistencia bajo concurrencia (incluyendo carreras como revocación frente a confirmación de desembolso); control de autorización por función (**RF-37**); e inmutabilidad real (ningún rol, incluida Dirección, puede editar ni eliminar un registro). Además cierra la regla de registro de entrada automática a *Inactiva* (actor sistema con justificación automática) y salida de *Inactiva* (coordinador con justificación obligatoria).
- **HU-35.2** (*Consultar el registro de decisiones*): exige consulta de solo lectura ordenada cronológicamente, mensaje explícito cuando un caso aún no tiene decisiones, garantía de que consultar no altera ningún registro y autorización por función y alcance (**RF-37**): Dirección (global), Coordinador de programa (sus programas asignados), Administrador Financiero (solo decisiones de ámbito financiero, sin deliberaciones socioeconómicas ni de comité) y Postulante/Becario (registro completo de su propio caso, denegando siempre casos ajenos).
- **Decisiones del cliente y arquitectura:** **C3 / R18 / R28 / R38** (*Inactiva* y su revocación/salida), **R20** (reloj de negocio en UTC-4), **DDD v3.3 §14, §18, D-6 y DDD-14** (`RegistroDecision` no es un agregado independiente; Trazabilidad no intermedia eventos), **ADR-05** (aislamiento de schemas PostgreSQL), **ADR-12** (auditoría + Outbox en commit local atómico), **ADR-13** (Outbox/Inbox), **ADR-16 y ADR-25** (`Version` explícita en la raíz de agregado) y preparación del modelo de datos para la exportación interna nominada futura de **RF-36 / HU-36.1** (Anexo A).

## What Changes

- **Dominio transversal puro (`building-blocks/Ascenso.SharedKernel/Trazabilidad/`):** modelo inmutable `RegistroDecision` (sin métodos de mutación ni borrado, incluyendo `ProgramaId` y `PropietarioUsuarioId` para autorización por ámbito sin cruzar schemas), value objects `SujetoRef` y `ActorDecision`, enumeraciones exactas del DDD v3.3 (`TipoSujeto` con 5 valores y `TipoDecision` con 38 valores), excepción de inmutabilidad `ExcepcionInmutabilidadRegistro`, política pura de autorización para registro/consulta (`Dirección`, `Coordinador`, `Finanzas`, `Postulante/Becario`) y puertos de solo lectura (`IConsultaRegistroDecisiones`, filtros y DTO proyectado).
- **Contrato de integración (`building-blocks/Ascenso.Contracts/` y `contracts/`):** nuevo evento versionado `DecisionRegistradaV1` para alimentar de forma asíncrona e idempotente las proyecciones de solo lectura en S6 (`registro_decisiones` y `linea_tiempo_beca`) sin cruzar schemas de PostgreSQL; actualización de `contracts/EVENTOS_E_INTERFACES.md`, `docs/arquitectura/v1.3/04_MATRICES/CATALOGO_EVENTOS_INTEGRACION.csv`, `contracts/openapi.yaml` (parámetros de consulta de `GET /api/v1/registro-decisiones`) y `docs/arquitectura/pendientes-v1.3.md`.
- **Persistencia atómica y defensa append-only (`building-blocks/Ascenso.Messaging.Outbox/` y `deploy/postgres/init-schemas.sql`):** mapeo EF Core de la tabla `registro_decisiones` en el schema propio de cada servicio, interceptor `AppendOnlyRegistroDecisionInterceptor` que bloquea `UPDATE`/`DELETE` a nivel de EF Core, helper transaccional atómico para confirmar en una sola transacción `Agregado (Version++) + RegistroDecision + OutboxMessage`, lector `AsNoTracking` de solo lectura con filtros de alcance en datos y función/trigger SQL reutilizable para endurecer `append-only` en PostgreSQL.
- **Pruebas automatizadas (`building-blocks/tests/Ascenso.BuildingBlocks.Tests/`):** pruebas unitarias de dominio, pruebas de integración transaccional real (SQLite `:memory:` con transacciones, triggers `BEFORE UPDATE/DELETE`, rollback atómico y concurrencia optimista por `Version`) cubriendo todos los escenarios de `HU-35.1` y `HU-35.2` (incluidos Inactiva y alcances de Coordinador, Finanzas y Becario), y pruebas de arquitectura (pureza de `Ascenso.SharedKernel` y ausencia de PII prohibida en `DecisionRegistradaV1`).

## Capabilities

### New Capabilities

- `registro-decisiones`: registro inmutable (append-only) y atómico de toda decisión relevante con autor, fecha, tipo, sujeto y justificación (`HU-35.1`), junto con su consulta de solo lectura ordenada por fecha y sujeta a autorización por función y alcance (`HU-35.2`).

### Modified Capabilities

Ninguna.

## Impact

- **Servicios afectados:** capacidad transversal consumida por `S1`, `S2`, `S3`, `S4`, `S5` (escritura atómica en su schema local y publicación por Outbox) y `S6` (proyección de lectura `registro_decisiones` / `linea_tiempo_beca` y consulta `GET /api/v1/registro-decisiones`). No crea proyectos vacíos de microservicios (`ARQUITECTURA_V1.0.md` §12); entrega el bloque transversal probado para que cada servicio lo incorpore en su propio vertical slice.
- **Toca `building-blocks/Ascenso.SharedKernel` y `building-blocks/Ascenso.Contracts`:** requiere revisión y acuerdo de todo el equipo antes de fusionar en `main`.
- **Toca contratos activos y registro de pendientes:** `contracts/EVENTOS_E_INTERFACES.md`, `contracts/openapi.yaml`, `docs/arquitectura/v1.3/04_MATRICES/CATALOGO_EVENTOS_INTEGRACION.csv` y `docs/arquitectura/pendientes-v1.3.md`.
- **Fuera de alcance en este PR:** la exportación interna nominada (**RF-36 / HU-36.1**, *Podría tener*, aunque el modelo captura los campos necesarios para no remodelar luego), el despliegue del controlador HTTP y proyector MongoDB de `S6` (se cablearán al levantar `Ascenso.S6.ComunicacionesReporteria`) y las validaciones propias de cada máquina de estados u OTP (que viven en sus respectivas HU).
