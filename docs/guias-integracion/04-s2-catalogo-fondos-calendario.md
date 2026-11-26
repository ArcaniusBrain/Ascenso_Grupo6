# 04 · `S2` Catálogo y Fondos — Calendario Académico (`RF-38`, `HU-38.1` y `HU-38.2`)

El microservicio **`services/Ascenso.S2.CatalogoFondos/`** (schema `s2_catalogo_fondos`) ya está operativo en `main` con su primer agregado institucional: **`CalendarioAcademico`** + **`InicioInscripcionesPeriodo`**.

---

## 1. Contrato público disponible (`contracts/openapi.yaml`, tag `S2`)

| Método y ruta | Quién puede llamarlo | Qué hace |
|---|---|---|
| `GET /api/v1/periodos` | Personal interno (`DIRECCION`, `COORDINADOR`, `FINANZAS`, `EVALUADOR`) | Devuelve la secuencia ordenada **cronológicamente ascendente** (`2025-2` antes que `2026-1`). Si no hay periodos, devuelve `mensaje: "Aún no hay periodos académicos registrados."`. |
| `POST /api/v1/periodos` | **Solo `DIRECCION`** | Registra y abre un periodo nuevo (`ABIERTO`). Rechaza vacío (`422`) y duplicado (`409`). |
| `GET /api/v1/periodos/{codigoPeriodo}` | Personal interno | Devuelve el detalle del periodo, `esActual`, `esPeriodoAnterior`, `iniciosInscripciones` por universidad y `mensajeSinElementos` si no tiene elementos ligados (`HU-38.1 E5`). Devuelve `404` si no existe. |
| `POST /api/v1/periodos/{codigoPeriodo}/cerrar` | **Solo `DIRECCION`** | Cierra un periodo (`ABIERTO → CERRADO`). No permite cerrar el periodo que actualmente es el vigente (`422`). |
| `POST /api/v1/periodos/{codigoPeriodo}/inicios-inscripciones` | **Solo `DIRECCION`** | Registra la **única fecha de inicio de inscripciones** de una universidad en ese periodo (`DateOnly`, interpretada desde las **`00:00` UTC-4**, `R20`, `C4`, `C15`). Duplicado devuelve `409` conservando la fecha original. |
| `POST /api/v1/periodos/actual` | **Solo `DIRECCION`** *(solo 2FA de sesión, sin OTP)* | Marca un periodo `ABIERTO` como el **único periodo actual**, persiste atómicamente `PeriodoMarcadoActual.v1` en `outbox_messages` e idempotencia por `operationId`. |

### Comportamientos críticos garantizados por `S2`
- **Concurrencia (`HU-38.2 E6`, `ADR-25`):** si dos usuarios de Dirección marcan dos periodos distintos simultáneamente, exactamente uno confirma (`200 OK`) y el otro recibe **`409 Conflict`** (`ProblemDetails` con `code: "CONCURRENCIA_CONFLICTO"`). Nunca quedan dos periodos actuales.
- **Idempotencia (`HU-38.2 E8`):**
  - Reenviar el mismo `operationId` devuelve la misma `OperacionConfirmada` sin volver a escribir en Outbox.
  - Marcar como actual el periodo que **ya** es actual devuelve `200 OK` con `estado: "SIN_CAMBIOS"`, no incrementa `Version` y no emite evento duplicado.
- **No cierra disponibilidades universitarias (`HU-38.2 E11`):** cambiar el periodo actual nunca altera ni cierra las fechas de inicio de inscripciones de las universidades.

---

## 2. Cómo se conecta con `S4` (Leonel y Sebas) y `S6`

Cuando Dirección cambia el periodo actual, `S2` guarda en `s2_catalogo_fondos.outbox_messages` el evento:

- **`EventType`:** `"PeriodoMarcadoActual.v1"` (`SchemaVersion = 1`)
- **Contrato C# (`Ascenso.Contracts.Eventos.PeriodoMarcadoActualV1`):**
  ```csharp
  public sealed record PeriodoMarcadoActualV1(
      Guid CalendarioId,
      string CodigoPeriodo,
      string? CodigoPeriodoAnterior);
  ```

### Qué debe hacer el consumidor en `S4` (Inbox idempotente por `EventId`):
1. Registrar `(ConsumerName, EventId)` en `s4_becas.inbox_messages` (`ADR-13`).
2. Ejecutar por lotes idempotentes **`Beca.IniciarNuevoPeriodo(CodigoPeriodo)`** en segundo plano:
   - Activar las becas que estaban en estado `Renovada` para el nuevo `CodigoPeriodo`.
   - **NO interrumpir** solicitudes de renovación todavía en curso (`HU-38.2 E2/E4`).
   - **NO pasar becas `Activa → Inactiva`** al cambiar de periodo: esa transición tiene como **único disparador** el job de Hangfire en la fecha de cierre de ventana de renovación (`C2`, `DDD-08`, `R38`).

---

## 3. Cómo se conecta con `S3` (Daniel y Diego) y próximos incrementos de `S2`

- **`S3` (Ciclos de postulación):** todo `CicloPostulacion` se liga a un `CodigoPeriodo` registrado en `S2` (`Ascenso.SharedKernel.ValueObjects.CodigoPeriodo`). Las fechas de inicio de inscripciones por universidad (`InicioInscripcionesPeriodo`) se evalúan a las `00:00` UTC-4 (`IRelojNegocio`).
- **Siguientes incrementos planificados dentro de `services/Ascenso.S2.CatalogoFondos/`** (en este mismo servicio y schema `s2_catalogo_fondos`):
  1. `RF-14` (`UniversidadAliada`, convenios y `CostoMatriculaVigenteV1` hacia `S4`).
  2. `RF-11` (`Programa` y reglas configurables de renovación/cumplimiento/postulación).
  3. `RF-13` (`Donante` y `Aporte`).
  4. `RF-12` (`PresupuestoPrograma`, reservas/liberaciones atómicas `S4 → S2`, `S3 → S2` y conciliación `S2 → S4` `ADR-26`).
