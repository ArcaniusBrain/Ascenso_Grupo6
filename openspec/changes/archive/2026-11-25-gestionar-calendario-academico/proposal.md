# Proposal

## Why

El calendario académico institucional (**RF-38**: **HU-38.1** y **HU-38.2**) es la primera entidad obligatoria del servicio **`S2 Catálogo y Fondos`** (`services/Ascenso.S2.CatalogoFondos/`): programas (`RF-11`), ciclos de postulación (`RF-02`), convenios/costos de matrícula (`RF-14`) y renovaciones (`RF-26`) se ligan a un `CodigoPeriodo` registrado en `CalendarioAcademico` (`Anexo A · HU-38.1`).

Construir este primer vertical slice de `S2` desbloquea a Daniel (ciclos en `S3`), a Sebas (renovaciones en `S4`), a Leonel (costos vigentes `C2` en `S4`) y el resto de agregados de `S2` (`UniversidadAliada`, `Programa`, `Donante`, `PresupuestoPrograma`).

Este cambio materializa:
- **HU-38.1** (*Registrar y consultar los periodos académicos*): secuencia de periodos con código único no vacío, consulta de periodos anteriores diferenciados del actual, fechas de inicio de inscripciones por universidad y periodo en UTC-4 (`C4`, una sola por universidad y periodo), y restricción de escritura exclusiva a Dirección (`R16`, `RF-37`).
- **HU-38.2** (*Marcar el periodo actual sin interrumpir renovaciones*): invariante de **un único periodo actual a la vez** incluso bajo solicitudes simultáneas (`Version` explícita `ADR-25`), idempotencia al marcar el mismo periodo ya actual, no cierre de disponibilidades universitarias al cambiar de periodo (`C4`) y publicación atómica por Outbox de **`PeriodoMarcadoActual.v1`** para que `S4` ejecute en segundo plano `Beca.IniciarNuevoPeriodo()` sin interrumpir renovaciones en curso (`DDD-08`, `R38`).
- **ADRs aplicados:** `ADR-01/02/04` (Clean Architecture 4 capas + test de arquitectura Clase 09), `ADR-05` (schema aislado `s2_catalogo_fondos`), `ADR-12/13` (Outbox/Inbox y `RegistroDecision` append-only), `ADR-16/25` (`Version` explícita), `ADR-17/R20` (`IRelojNegocio` UTC-4) y `ADR-27` (`MediatR 12.5.0`).

## What Changes

- **Segundo microservicio Clean Architecture (`services/Ascenso.S2.CatalogoFondos/`):** proyectos `Domain`, `Application`, `Infrastructure`, `Api` y `tests/{Unit,Integration,Architecture}` incorporados a `Ascenso.sln`.
- **Dominio S2 (`Ascenso.S2.CatalogoFondos.Domain`):** raíz `CalendarioAcademico` (con entidad interna `PeriodoAcademico`, enum `EstadoPeriodo`, eventos `PeriodoAbiertoEvento` y `PeriodoMarcadoActualEvento`, `Version` explícita), registro de fecha de inicio de inscripciones por universidad y periodo (`InicioInscripcionesPeriodo`), e `ICalendarioAcademicoRepository`.
- **Aplicación S2 (`Ascenso.S2.CatalogoFondos.Application`):** comandos y queries `MediatR 12.5.0` delgados para abrir/cerrar periodos, registrar fecha de inicio de inscripciones por universidad y periodo, marcar periodo actual (emitiendo `PeriodoMarcadoActualV1` vía Outbox) y consultar la secuencia y el detalle de cada periodo.
- **Infraestructura S2 (`Ascenso.S2.CatalogoFondos.Infrastructure`):** `CatalogoFondosDbContext` sobre schema aislado `s2_catalogo_fondos`, `OutboxMessage`/`InboxMessage`, `RegistroDecision` + `AppendOnlyRegistroDecisionInterceptor`, repositorios EF Core e idempotencia por `operationId`.
- **API y contratos (`Ascenso.S2.CatalogoFondos.Api` y `contracts/openapi.yaml`):** implementa `POST /api/v1/periodos/actual` (ya definido en `openapi.yaml`) y añade las rutas públicas complementarias de `HU-38.1` (`GET /api/v1/periodos`, `POST /api/v1/periodos`, `GET /api/v1/periodos/{codigoPeriodo}`, `POST /api/v1/periodos/{codigoPeriodo}/cerrar`, `POST /api/v1/periodos/{codigoPeriodo}/inicios-inscripciones`).
- **Pruebas completas:** unitarias de dominio, integración HTTP + SQLite real (concurrencia real de dos cambios de periodo simultáneos, persistencia atómica de `PeriodoMarcadoActualV1` en Outbox, permisos solo Dirección) y pruebas de arquitectura (`S2.Domain` libre de infraestructura).

## Capabilities

### New Capabilities

- `calendario-academico`: registro, consulta, cierre, fechas de inicio de inscripciones por universidad y marcado atómico del periodo académico actual (`RF-38`: `HU-38.1` y `HU-38.2`).

### Modified Capabilities

Ninguna.

## Impact

- **Servicios afectados:** crea `services/Ascenso.S2.CatalogoFondos/` (dueño de `RF-38`, `RF-14`, `RF-11`, `RF-13`, `RF-12`).
- **No modifica** `Ascenso.SharedKernel` ni `Ascenso.Contracts` (consume `CodigoPeriodo`, `RaizDeAgregado`, `IRelojNegocio`, `PeriodoMarcadoActualV1` ya entregados).
- **Toca archivos compartidos del repo:** `Ascenso.sln` y `contracts/openapi.yaml` (rutas públicas de `HU-38.1` bajo tag `S2`).
- **Fuera de alcance en este PR:** la reacción por lote sobre becas (`Beca.IniciarNuevoPeriodo()`) que pertenece al consumidor en `S4` (Leonel/Sebas) al recibir `PeriodoMarcadoActual.v1`, y el agregado completo `UniversidadAliada` (`RF-14`, que entra en el siguiente cambio de `S2`).
