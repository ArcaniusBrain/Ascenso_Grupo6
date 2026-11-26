# Design — `gestionar-calendario-academico` (RF-38 · S2)

## 1. Contexto, agregados DDD y ADRs aplicados

- **Historias de usuario:** `HU-38.1` (13 escenarios) y `HU-38.2` (11 escenarios, de los cuales E1, E3, E5, E6, E8 y E11 corresponden a la autoridad de `S2`, mientras E2, E4, E7, E9 y E10 corresponden a la reacción asíncrona sobre becas en `S4` al consumir `PeriodoMarcadoActual.v1`).
- **Modelo de dominio (`DDD_v3.3.md` §7 y §15.7):**
  - **Raíz de agregado `CalendarioAcademico : RaizDeAgregado<Guid>`** (instancia única institucional con `Version` explícita `ADR-25`):
    - Entidad interna `PeriodoAcademico`: identidad natural `Codigo` (`CodigoPeriodo` de `Ascenso.SharedKernel`), `Estado` (`EstadoPeriodo`: `ABIERTO` | `CERRADO`), `Actual` (`bool`), `RegistradoEnUtc` (`DateTimeOffset`).
    - Métodos:
      - `AbrirPeriodo(string? codigoTexto, DateTimeOffset ahoraUtc)` → rechaza vacío con `"Escribe el nombre del periodo académico."`, rechaza duplicado con `"El periodo {codigo} ya existe."`, incrementa `Version` y registra `PeriodoAbiertoEvento`.
      - `CerrarPeriodo(CodigoPeriodo codigo)` → cambia `ABIERTO → CERRADO` (rechaza cerrar el periodo que actualmente es el vigente si no se marcó otro antes), incrementa `Version`.
      - `MarcarActual(CodigoPeriodo codigo, DateTimeOffset ahoraUtc)` → devuelve `bool HuboCambio`: si ya es el actual devuelve `false` sin incrementar `Version` ni emitir evento (`HU-38.2 E8`); rechaza marcar un periodo `CERRADO` como actual; desmarca el anterior, marca el nuevo, incrementa `Version` y registra `PeriodoMarcadoActualEvento`.
      - `PeriodoActual()` → devuelve el `CodigoPeriodo?` actual.
  - **Entidad de fecha de inicio de inscripciones por universidad y periodo (`InicioInscripcionesPeriodo`):**
    - Almacena `(UniversidadId, NombreUniversidad, CodigoPeriodo, FechaInicio: DateOnly, RegistradoEnUtc)` con clave única `(UniversidadId, CodigoPeriodo)` (`HU-38.1 E10–E13`, `C4`).
    - Cuando se implemente el agregado `UniversidadAliada` en el siguiente cambio (`universidades-convenios`, `RF-14`) dentro del mismo servicio `S2`, `UniversidadAliada` consumirá/integrará esta misma persistencia del schema `s2_catalogo_fondos`.
  - **Eventos de dominio:** `PeriodoAbiertoEvento` y `PeriodoMarcadoActualEvento` (`IEventoDeDominio`).
- **ADRs aplicados:**
  - **ADR-01 / ADR-02 / ADR-04:** nuevo microservicio `services/Ascenso.S2.CatalogoFondos/` en 4 capas (`Domain`, `Application`, `Infrastructure`, `Api`) + 3 proyectos de pruebas (`UnitTests`, `IntegrationTests`, `ArchitectureTests` Clase 09).
  - **ADR-05:** `CatalogoFondosDbContext` en schema aislado `s2_catalogo_fondos`.
  - **ADR-12 / ADR-13 / D-6:** `MarcarActual` persiste en una única transacción local `CalendarioAcademico (Version++) + OutboxMessage (PeriodoMarcadoActualV1)` (con `InboxMessage` y `RegistroDecision` + `AppendOnlyRegistroDecisionInterceptor` ya registrados en `CatalogoFondosDbContext`).
  - **ADR-16 / ADR-25:** `Version` explícita en `CalendarioAcademico` garantiza que ante dos solicitudes simultáneas de `MarcarActual` (`HU-38.2 E6`), exactamente una haga commit y la segunda reciba `DbUpdateConcurrencyException` → HTTP `409 Conflict`.
  - **ADR-17 / R20:** uso de `IRelojNegocio` (UTC-4) para evaluar si la fecha de inicio de inscripciones de una universidad ya llegó.
  - **ADR-27:** `MediatR 12.5.0` en `Ascenso.S2.CatalogoFondos.Application`.

## 2. Eventos de integración

- **`PeriodoMarcadoActual.v1`** (`Ascenso.Contracts.Eventos.PeriodoMarcadoActualV1`, ya existente en `Ascenso.Contracts`, versión `1`):
  - **Productor:** `S2` (guardado en `OutboxMessage` en la misma transacción que `CalendarioAcademico.MarcarActual`).
  - **Consumidores:** `S4` (ejecuta en segundo plano y por lotes idempotentes `Beca.IniciarNuevoPeriodo()` para activar becas `Renovada` sin interrumpir renovaciones en curso, `DDD-08`, `R38`) y `S6` (actualiza proyecciones e indicadores).
  - No se añaden nuevos tipos a `Ascenso.Contracts`.

## 3. Contratos públicos de S2 (`contracts/openapi.yaml`)

- **Ya existente en `contracts/openapi.yaml`:**
  - `POST /api/v1/periodos/actual` (`MarcarPeriodoCommand { codigoPeriodo, operationId }` → `OperacionConfirmada`, solo Dirección).
- **Rutas complementarias de `HU-38.1` a añadir bajo tag `S2` en `contracts/openapi.yaml`:**
  - `GET /api/v1/periodos` (secuencia de periodos y mensaje vacío si no hay ninguno).
  - `POST /api/v1/periodos` (solo Dirección; abre un periodo nuevo en la secuencia).
  - `GET /api/v1/periodos/{codigoPeriodo}` (detalle de periodo actual o anterior, fechas de inicio de inscripciones por universidad y conteo de elementos ligados).
  - `POST /api/v1/periodos/{codigoPeriodo}/cerrar` (solo Dirección; cierra un periodo no actual).
  - `POST /api/v1/periodos/{codigoPeriodo}/inicios-inscripciones` (solo Dirección; registra fecha única de inicio de inscripciones para una universidad en ese periodo).

## 4. Decisiones cerradas del Anexo B (`HU-38.1` y `HU-38.2`, 25-nov-2026 — C15)

- **Anexo B · HU-38.1 #1 (Orden de la secuencia):** **Confirmado** — orden cronológico ascendente por código (`2025-2` antes que `2026-1`).
- **Anexo B · HU-38.1 #2 (Quién consulta periodos anteriores):** **Confirmado** — lectura habilitada para todo el personal interno autenticado (Dirección, Coordinador, Finanzas, Evaluador); toda escritura es exclusiva de Dirección.
- **Anexo B · HU-38.1 #3 (Edición/borrado de periodos):** **Confirmado** — los periodos nunca se eliminan ni renombran para preservar integridad histórica y trazabilidad (`RF-38`).
- **Anexo B · HU-38.1 #4 (Hora de inicio de inscripciones en UTC-4):** **Confirmado** — se interpreta desde las `00:00` UTC-4 del día registrado (coherente con `R20`).
- **Anexo B · HU-38.2 #3 (Marcar como actual un periodo cerrado o anterior):** **Confirmado** — exige que el periodo esté en estado `ABIERTO` (`DDD_v3.3` §15.7).
- **Anexo B · HU-38.2 #4 (OTP al marcar periodo actual):** **Confirmado** — no exige OTP; basta el 2FA de sesión de Dirección.
- **Anexo B · HU-38.2 #6 (Respuesta al segundo solicitante en carrera concurrente):** **Confirmado** — HTTP `409 Conflict` con `ProblemDetails` RFC 9457 informando la modificación concurrente.
