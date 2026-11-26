# 04 · `S2` Catálogo y Fondos — Calendario (`RF-38`), Universidades (`RF-14`) y Programas (`RF-11`)

El microservicio **`services/Ascenso.S2.CatalogoFondos/`** (schema `s2_catalogo_fondos`) incluye actualmente **`CalendarioAcademico`** (`RF-38`), **`UniversidadAliada`** (`RF-14`) y **`Programa`** (`RF-11`).

---

## 1. Contratos públicos disponibles (`contracts/openapi.yaml`, tag `S2`)

### A. Calendario académico (`RF-38`: `HU-38.1` y `HU-38.2`)

| Método y ruta | Quién puede llamarlo | Qué hace |
|---|---|---|
| `GET /api/v1/periodos` | Personal interno (`DIRECCION`, `COORDINADOR`, `FINANZAS`, `EVALUADOR`) | Devuelve la secuencia ordenada **cronológicamente ascendente** (`2025-2` antes que `2026-1`). Si no hay periodos, devuelve `mensaje: "Aún no hay periodos académicos registrados."`. |
| `POST /api/v1/periodos` | **Solo `DIRECCION`** | Registra y abre un periodo nuevo (`ABIERTO`). Rechaza vacío (`422`) y duplicado (`409`). |
| `GET /api/v1/periodos/{codigoPeriodo}` | Personal interno | Devuelve el detalle del periodo, `esActual`, `esPeriodoAnterior`, `iniciosInscripciones` por universidad y `mensajeSinElementos` si no tiene elementos ligados (`HU-38.1 E5`). Devuelve `404` si no existe. |
| `POST /api/v1/periodos/{codigoPeriodo}/cerrar` | **Solo `DIRECCION`** | Cierra un periodo (`ABIERTO → CERRADO`). No permite cerrar el periodo que actualmente es el vigente (`422`). |
| `POST /api/v1/periodos/{codigoPeriodo}/inicios-inscripciones` | **Solo `DIRECCION`** | Registra la **única fecha de inicio de inscripciones** de una universidad en ese periodo (`DateOnly`, interpretada desde las **`00:00` UTC-4**, `R20`, `C4`, `C15`). Duplicado devuelve `409` conservando la fecha original. |
| `POST /api/v1/periodos/actual` | **Solo `DIRECCION`** *(solo 2FA de sesión, sin OTP)* | Marca un periodo `ABIERTO` como el **único periodo actual**, persiste atómicamente `PeriodoMarcadoActual.v1` en `outbox_messages` e idempotencia por `operationId`. |

### B. Universidades aliadas, convenios, costos e inscripciones (`RF-14`: `HU-14.1`, `HU-14.2`, `HU-14.3`)

| Método y ruta | Quién puede llamarlo | Qué hace |
|---|---|---|
| `GET /api/v1/universidades` | Cualquier usuario autenticado (incluidos `POSTULANTE` y `BECARIO`) | Lista las universidades aliadas registradas para selección en `HU-01.1` y configuración de programas. |
| `POST /api/v1/universidades` | **`DIRECCION` y `COORDINADOR`** *(bloqueado a `ENLACE_UNIVERSIDAD`)* | Registra universidad con nombre único (insensible a mayúsculas/espacios, `409` si se repite), términos del convenio, vía académica (`API` \| `MANUAL`) y contacto designado. |
| `GET /api/v1/universidades/{universidadId}` | Cualquier usuario autenticado | Devuelve convenio, contacto, carreras elegibles (o `"Esta universidad no tiene carreras elegibles registradas."`) y costos vigentes por periodo. |
| `PUT /api/v1/universidades/{universidadId}/convenio` | **`DIRECCION` y `COORDINADOR`** *(bloqueado a `ENLACE_UNIVERSIDAD`)* | Actualiza convenio, vía, contacto y permite corregir mayúsculas/minúsculas del nombre sin auto-colisión (`C16`). |
| `POST /api/v1/universidades/{universidadId}/carreras` | **`DIRECCION` y `COORDINADOR`** | Agrega una carrera elegible aislada a esa universidad (`HU-14.1 E4`). |
| `POST /api/v1/universidades/{universidadId}/costos-matricula` | **`DIRECCION` y `COORDINADOR`** | Registra o corrige un costo `> 0` (moneda ISO-3, `USD` por defecto) en modo **append-only** (`VersionRegistro++`, sin sobrescribir historial) y publica **`CostoMatriculaVigente.v1`** en `outbox_messages`. |
| `GET /api/v1/universidades/{universidadId}/costos-matricula/{codigoPeriodo}` | Cualquier usuario autenticado | Devuelve costo vigente + historial completo del periodo, o `200 OK` con `"Sin costo registrado para este periodo"` (`HU-14.2 E5`). |
| `POST /api/v1/universidades/{universidadId}/inicios-inscripciones` | **`DIRECCION` y `COORDINADOR`** (`C16`) | Registra fecha única de inicio de inscripciones en UTC-4 (`00:00`) para un periodo registrado (`HU-14.3`). |
| `GET /api/v1/universidades/{universidadId}/inicios-inscripciones/{codigoPeriodo}` | Cualquier usuario autenticado | Devuelve fecha e indicador `inscripcionesIniciadas` (UTC-4), o `200 OK` con `"Sin fecha de inicio de inscripciones registrada para este periodo"` (`inscripcionesIniciadas = false`). |

### C. Programas de becas (`RF-11`: `HU-11.1` y `HU-11.2`)

| Método y ruta | Quién puede llamarlo | Qué hace |
|---|---|---|
| `GET /api/v1/programas` | Cualquier usuario autenticado | Lista los programas registrados con cupo, duración, monto fijo por periodo, modalidad, alcance y `permiteAcumulacion`. |
| `POST /api/v1/programas` | **`DIRECCION` y `COORDINADOR`** | Crea un programa ligado a un periodo `ABIERTO` con nombre único, cupo `> 0`, duración `> 0`, monto `> 0`, modalidad (`MENSUAL` \| `SEMESTRAL` \| `DIRECTO_UNIVERSIDAD`), alcance (`COMPLETO` \| `PARCIAL`) y `permiteAcumulacion` (forzado a `false` si `COMPLETO`, `C17`). |
| `GET /api/v1/programas/{programaId}` | Cualquier usuario autenticado | Detalle completo: presupuesto total (`S8`), donantes con aportes separados (o `"Sin aportes registrados"`), restricciones universidad/carrera (o `"Sin restricción de universidad ni carrera"`) y condiciones estructuradas de permanencia/pausa/renovación. |
| `PUT /api/v1/programas/{programaId}/configuracion-base` | **`DIRECCION` y `COORDINADOR` del programa** | Actualiza nombre (con corrección de mayúsculas/minúsculas), monto fijo, modalidad, alcance y acumulación. |
| `POST /api/v1/programas/{programaId}/donantes` | **`DIRECCION` y `COORDINADOR` del programa** | Asocia un donante registrado sin duplicados (`409` con `"Este donante ya está asociado al programa."`). |
| `PUT /api/v1/programas/{programaId}/elegibilidad` | **`DIRECCION` y `COORDINADOR` del programa** | Configura restricciones opcionales validadas contra `UniversidadAliada` y sus `CarrerasElegibles` (sin retroactividad sobre postulaciones ya enviadas, `C17`). |
| `PUT /api/v1/programas/{programaId}/condiciones` | **`DIRECCION` y `COORDINADOR` del programa** | Registra promedio mínimo `>= 0`, horas de servicio `>= 0` (u opción `"Horas de servicio: no aplica"`), reportes exigidos, duración máxima de pausa (`>= 0`, `0` = sin pausas) y descripción de renovación. |

### Comportamientos críticos garantizados por `S2`
- **Concurrencia (`HU-38.2 E6`, `ADR-25`):** si dos usuarios de Dirección marcan dos periodos distintos simultáneamente, exactamente uno confirma (`200 OK`) y el otro recibe **`409 Conflict`** (`ProblemDetails` con `code: "CONCURRENCIA_CONFLICTO"`). Nunca quedan dos periodos actuales.
- **Idempotencia (`HU-38.2 E8`):**
  - Reenviar el mismo `operationId` devuelve la misma `OperacionConfirmada` sin volver a escribir en Outbox.
  - Marcar como actual el periodo que **ya** es actual devuelve `200 OK` con `estado: "SIN_CAMBIOS"`, no incrementa `Version` y no emite evento duplicado.
- **No cierra disponibilidades universitarias (`HU-38.2 E11`):** cambiar el periodo actual nunca altera ni cierra las fechas de inicio de inscripciones de las universidades.

---

## 2. Eventos de integración que publica `S2` (para `S4` Leonel/Sebas y `S6`)

### A. `PeriodoMarcadoActual.v1` (`RF-38`)
Cuando Dirección cambia el periodo actual, `S2` guarda en `s2_catalogo_fondos.outbox_messages`:

- **`EventType`:** `"PeriodoMarcadoActual.v1"` (`SchemaVersion = 1`)
- **Contrato C# (`Ascenso.Contracts.Eventos.PeriodoMarcadoActualV1`):**
  ```csharp
  public sealed record PeriodoMarcadoActualV1(
      Guid CalendarioId,
      string CodigoPeriodo,
      string? CodigoPeriodoAnterior);
  ```

**Qué debe hacer el consumidor en `S4` (Inbox idempotente por `EventId`):**
1. Registrar `(ConsumerName, EventId)` en `s4_becas.inbox_messages` (`ADR-13`).
2. Ejecutar por lotes idempotentes **`Beca.IniciarNuevoPeriodo(CodigoPeriodo)`** en segundo plano:
   - Activar las becas que estaban en estado `Renovada` para el nuevo `CodigoPeriodo`.
   - **NO interrumpir** solicitudes de renovación todavía en curso (`HU-38.2 E2/E4`).
   - **NO pasar becas `Activa → Inactiva`** al cambiar de periodo: esa transición tiene como **único disparador** el job de Hangfire en la fecha de cierre de ventana de renovación (`C2`, `DDD-08`, `R38`).

### B. `CostoMatriculaVigente.v1` (`RF-14`, `HU-14.2`, `C2`)
Cada vez que Dirección o Coordinador registra o corrige un costo de matrícula para `(UniversidadId, CodigoPeriodo)`, `S2` persiste el historial append-only y guarda en `s2_catalogo_fondos.outbox_messages`:

- **`EventType`:** `"CostoMatriculaVigente.v1"` (`SchemaVersion = 1`)
- **Contrato C# (`Ascenso.Contracts.Eventos.CostoMatriculaVigenteV1`):**
  ```csharp
  public sealed record CostoMatriculaVigenteV1(
      Guid UniversidadId,
      string CodigoPeriodo,
      decimal Monto,
      string Moneda,
      int VersionRegistro,
      DateTimeOffset VigenteDesdeUtc);
  ```

**Qué debe hacer el consumidor en `S4`:**
- Proyectar la versión más alta (`VersionRegistro`) por `(UniversidadId, CodigoPeriodo)` en su tabla local para aplicar el **tope de la suma de becas por periodo** en la regla de no doble beneficio (`HU-01.2`, `HU-16.1`, `C2`).

---

## 3. Cómo se conecta con `S3` (Daniel y Diego), `S5` y próximos incrementos de `S2`

- **`S3` (Postulación y ciclos):** consulta `GET /api/v1/universidades` y `GET /api/v1/universidades/{id}` para validar universidades aliadas y carreras elegibles (`HU-01.1`), y consulta `GET /api/v1/universidades/{id}/inicios-inscripciones/{periodo}` para evaluar el cierre de disponibilidades/lista de espera por universidad a las `00:00` UTC-4 (`HU-02.2`, `HU-10.2`).
- **`S5` (Seguimiento académico):** lee `viaDatosAcademicos` (`API` | `MANUAL`) del convenio para saber por qué vía llega el reporte académico de cada universidad (`HU-24.2`, `HU-24.3`).
- **Siguientes incrementos planificados dentro de `services/Ascenso.S2.CatalogoFondos/`** (en este mismo servicio y schema `s2_catalogo_fondos`):
  1. `RF-13` (`Donante` y `Aporte` — conecta directo con `IConsultorDonantesYAportes`).
  2. `RF-12` (`PresupuestoPrograma`, reservas/liberaciones atómicas `S4 → S2`, `S3 → S2` y conciliación `S2 → S4` `ADR-26`).
