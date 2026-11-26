# 05 · `S6` Comunicaciones y Reportería — Base compartida y Reportes de Impacto (`RF-33`)

El microservicio **`services/Ascenso.S6.ComunicacionesReporteria/`** (schema `s6_comunicaciones`) ya cuenta con su estructura completa de 7 proyectos en `Ascenso.sln` (`Domain`, `Application`, `Infrastructure`, `Api`, `UnitTests`, `IntegrationTests`, `ArchitectureTests`), `Outbox`/`Inbox`, `RegistroDecision` append-only y la implementación completa de **`RF-33`** (`HU-33.1` y `HU-33.2`).

---

## 1. Contratos públicos disponibles (`contracts/openapi.yaml`, tag `S6`)

| Método y ruta | Quién puede llamarlo | Qué hace |
|---|---|---|
| `POST /api/v1/consentimientos-donantes` | **Solo el propio `BECARIO`** *(cualquier otro rol o intento por otro becario → `403`)* | Registra el consentimiento **irrevocable** (`R36/A6`) para ser presentado individualmente a un donante específico `(BecarioId, DonanteId)`. Confirmar dos veces conserva el único registro vigente con su fecha original (`HU-33.2 E4`). Sin OTP. |
| `GET /api/v1/consentimientos-donantes/mios` | **Solo `BECARIO`** | Lista los consentimientos vigentes del becario autenticado. |
| `POST /api/v1/reportes/impacto` | **`DIRECCION`** (global) y **`COORDINADOR`** (acotado a sus programas si informa alcance) | Valida periodo en el calendario proyectado (`404` si no existe) y aportes del donante en ese periodo (`422` si no tiene), encola la generación en segundo plano (`Channel<Guid>` + `ProcesadorReportesImpactoWorker`) y responde **`202 Accepted`** con `"El reporte se está preparando."` sin bloquear la operación diaria (`HU-33.1 E9, E12`). |
| `GET /api/v1/reportes/impacto/{reporteId}` | **`DIRECCION`, `COORDINADOR` y `FINANZAS`** *(bloqueado `403` a `EVALUADOR`, `ENLACE_UNIVERSIDAD`, `BECARIO`, `POSTULANTE`)* | Devuelve estado `PREPARANDO` (`"El reporte se está preparando."`) o `LISTO` con el snapshot conservado y anonimizado del reporte. |
| `GET /api/v1/reportes/impacto?donanteId=&codigoPeriodo=` | **`DIRECCION`, `COORDINADOR` y `FINANZAS`** | Lista los reportes de impacto generados y conservados para re-entrega. |

---

## 2. Garantías de confidencialidad y capa única de anonimización (`Anexo A HU-33.1`, `C20`)

Toda salida dirigida a donantes (y futuras exportaciones anonimizadas como `RF-36`) pasa obligatoriamente por **`ServicioAnonimizacionImpacto`** (`Ascenso.S6.ComunicacionesReporteria.Domain/Servicios/ServicioAnonimizacionImpacto.cs`):

1. **Imposibilidad estructural de fuga socioeconómica:** los modelos de salida **no tienen campos** para ingreso familiar ni composición familiar (jamás se exponen, incluso si el becario otorgó consentimiento).
2. **Becarios sin consentimiento:** solo participan en cifras agregadas (`NumeroBecarios`, distribución por universidad/carrera y promedio agregado); nunca se expone nombre ni cédula (`HU-33.1 E4, E5`).
3. **Becarios con consentimiento vigente para ese donante (`HU-33.2`, `C20`):** aparecen en `BecariosPresentados` con nombre completo, cédula, universidad, carrera, programa y rendimiento académico general.
4. **Ejecución presupuestaria exacta (`HU-33.1 E10`):** contabiliza exclusivamente desembolsos en estado `CONFIRMADO` (excluye `FALLIDO`, `DEVUELTO` y `PROGRAMADO`), sin exponer aportes ni nombres de otros donantes del mismo programa.
5. **Resultados académicos agnósticos al origen:** agrega reportes académicos de origen `API` y `MANUAL` por igual; si el programa no tiene reportes en el periodo devuelve `"Aún no hay datos académicos para este periodo."` (`HU-33.1 E11`).

---

## 3. Cómo enchufar el resto de `S6` (para Daniel, Diego, Leonel y Sebas)

`Ascenso.S6.ComunicacionesReporteria` queda listo para recibir sobre la misma base y `ComunicacionesDbContext` (`s6_comunicaciones`):
- **Daniel (`RF-29`, `RF-30`, `RF-32`):** agregar agregados `Consulta` y `RegistroEnvio` en `Domain`, sus handlers en `Application` y sus tablas en `ComunicacionesDbContext`.
- **Diego (`RF-31`, `RF-34`):** agregar avisos al comité y proyección de reporte consolidado institucional detrás de puertos CQRS.
- **Leonel (`RF-23`, `RF-36`) y Sebas (`RF-40`):** reutilizar `ServicioAnonimizacionImpacto` para exportaciones anonimizadas, sumar proyecciones financieras, panel de indicadores y hub SignalR en `S6.Api`.
- **Alimentación de proyecciones CQRS (Outbox → RabbitMQ → Inbox, `ADR-13`):**
  - `Ascenso.Messaging.Outbox` provee `AddOutboxRabbitMqDispatcher<TDbContext>(configuration, producerName)` (ya conectado en `S1`, `S2` y `S6`, y reutilizable por `S3`, `S4` y `S5` en una sola línea).
  - `S6` incluye `ProyectorEventosImpacto` (idempotente mediante `inbox_messages`, consumidor `s6.proyeccion-impacto`) y `ConsumidorEventosImpactoRabbitMqWorker`, suscrito al exchange topic durable `ascenso.eventos` para `PeriodoMarcadoActual.v1`, `AporteRegistrado.v1` y `PagoConfirmado.v1`.
  - Para activarlo en local con Docker (`deploy/docker-compose.yml`), define `RabbitMq__Host`, `RabbitMq__UserName` y `RabbitMq__Password` en variables de entorno; si `RabbitMq:Host` queda vacío, los workers permanecen inactivos sin afectar pruebas locales.
