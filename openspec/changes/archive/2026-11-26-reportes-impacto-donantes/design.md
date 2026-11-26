# Design — `reportes-impacto-donantes` (RF-33 · S6)

## 1. Arquitectura de `Ascenso.S6.ComunicacionesReporteria` (`s6_comunicaciones`)

- **Proyectos en `Ascenso.sln` (7 proyectos estándar `ADR-04`):**
  - `Ascenso.S6.ComunicacionesReporteria.Domain` (cero dependencias externas, verificado por `ArchitectureTests`)
  - `Ascenso.S6.ComunicacionesReporteria.Application` (`MediatR 12.5.0`, `ADR-27`)
  - `Ascenso.S6.ComunicacionesReporteria.Infrastructure` (EF Core `ComunicacionesDbContext` con schema `s6_comunicaciones` solo en `Npgsql`, `Outbox`/`Inbox`, `RegistroDecision` append-only, cola asíncrona en segundo plano)
  - `Ascenso.S6.ComunicacionesReporteria.Api` (Minimal API + `ProblemDetails` RFC 9457)
  - `tests/Ascenso.S6.ComunicacionesReporteria.UnitTests`
  - `tests/Ascenso.S6.ComunicacionesReporteria.IntegrationTests`
  - `tests/Ascenso.S6.ComunicacionesReporteria.ArchitectureTests`

## 2. Modelo de dominio y capa única de anonimización (`DDD_v3.3.md` §14)

- **Raíz de agregado `ConsentimientoDonante : RaizDeAgregado<Guid>` (`ADR-25`, `R36/A6`):**
  - `Id`, `BecarioId`, `DonanteId`, `AceptadoEnUtc`, `Version` explícita.
  - Índice único `(becario_id, donante_id)`; **irrevocable** (no existe operación de retiro).
  - `CrearOAceptar(...)` valida `BecarioId != Empty` y `DonanteId != Empty`; si el becario vuelve a confirmar un consentimiento existente, el handler es idempotente y devuelve el registro original sin modificar `AceptadoEnUtc` (`HU-33.2 E4`).
- **Capa única centralizada `ServicioAnonimizacionImpacto` (`Domain/Servicios/`):**
  - Recibe las proyecciones crudas de `S6` (aportes del donante en el periodo, becas del programa, desembolsos con estado, reportes académicos con origen `API`/`MANUAL` y conjunto de `BecarioId` con consentimiento vigente para ese `DonanteId`).
  - **Invariantes de confidencialidad por diseño:**
    - Los DTOs de salida (`ReporteImpactoContenidoDto`, `ProgramaImpactoDto`, `PerfilGeneralAgregadoDto`, `BecarioPresentadoDto`) **no tienen propiedades** para ingreso familiar ni composición familiar (imposibilidad estructural de fuga socioeconómica).
    - Filtra desembolsos estrictamente a `EstadoDesembolsoProyectado.CONFIRMADO` (`HU-33.1 E10`).
    - Si no hay reportes académicos para el programa en el periodo, establece `MensajeSinDatosAcademicos = "Aún no hay datos académicos para este periodo."` (`HU-33.1 E11`).
    - Para becarios sin consentimiento nunca se exponen nombre, cédula ni datos individuales (`HU-33.1 E4, E5`). Incluye en `BecariosPresentados` solo a los becarios presentes en el conjunto de consentimientos vigentes del donante, mostrando únicamente nombre completo, cédula, universidad, carrera, programa y rendimiento académico general (nunca datos socioeconómicos, `C20`).
- **Procesamiento asíncrono en segundo plano (`HU-33.1 E9, E12`):**
  - Entidad persistida `SolicitudReporteImpacto` (`Id`, `DonanteId`, `NombreDonante`, `CodigoPeriodo`, `SolicitadoPorUsuarioId`, `Estado: PREPARANDO | LISTO | FALLIDO`, `MensajeEstado`, `SolicitadoEnUtc`, `CompletadoEnUtc`, `ContenidoJson`).
  - `POST /api/v1/reportes/impacto` valida permisos, existencia del periodo en la proyección del calendario (`HU-33.1 E8`) y existencia de aportes del donante en ese periodo dentro del alcance del actor (`HU-33.1 E2`), persiste `SolicitudReporteImpacto` en estado `PREPARANDO`, encola en `IColaProcesamientoReportesImpacto` (`Channel<Guid>` + `BackgroundService` en runtime) y responde `202 Accepted` con `"El reporte se está preparando."` sin bloquear la operación diaria.

## 3. Contratos públicos (`contracts/openapi.yaml`, tag `S6`)

- `POST /api/v1/consentimientos-donantes` — solo `BECARIO` para sí mismo (idempotente, `200`/`201`).
- `GET /api/v1/consentimientos-donantes/mios` — solo `BECARIO` (lista sus consentimientos vigentes).
- `POST /api/v1/reportes/impacto` — `DIRECCION` (global) y `COORDINADOR` (acotado a sus programas si informa alcance); responde `202 Accepted` con `SolicitudReporteImpactoDto`.
- `GET /api/v1/reportes/impacto/{reporteId}` — `DIRECCION`, `COORDINADOR` y `FINANZAS`; devuelve estado `"PREPARANDO"` (`"El reporte se está preparando."`) o `"LISTO"` con el contenido anonimizado completo.
- `GET /api/v1/reportes/impacto` — honra ruta existente en `contracts/openapi.yaml` (lista/consulta reportes por `donanteId` y `codigoPeriodo` para personal autorizado).

## 4. Decisiones del Anexo B (`HU-33.1`) — confirmadas (`C20`)

1. **Perfil general y datos de becarios con consentimiento (`Anexo B · HU-33.1 #2 y #4`):** el perfil general agregado muestra distribución por universidad y carrera + conteo de becarios (sin nombres ni cédulas para becarios sin consentimiento); un becario con consentimiento vigente muestra únicamente nombre completo, cédula, universidad, carrera, programa y rendimiento académico general, **nunca** ingreso familiar ni composición familiar.
2. **Programa con varios donantes (`Anexo B · HU-33.1 #3`):** el reporte muestra el aporte propio del donante consultado junto con la ejecución presupuestaria (desembolsos confirmados) e impacto agregado del programa, sin exponer nombres ni montos de otros donantes.
3. **Permisos y conservación del reporte (`Anexo B · HU-33.1 #5 y #7`):** `DIRECCION` genera/consulta reportes consolidados de cualquier donante; `COORDINADOR` genera reportes acotados a los programas que coordina; `FINANZAS` puede consultar reportes generados; `EVALUADOR`, `ENLACE_UNIVERSIDAD`, `BECARIO` y `POSTULANTE` reciben `403`. Los reportes generados se conservan en `s6_comunicaciones` para re-entrega.
4. **Persistencia CQRS inicial de `S6`:** `ConsentimientoDonante`, proyecciones de lectura de impacto y snapshots de reportes se persisten en PostgreSQL EF Core (`s6_comunicaciones`) detrás de puertos `IRepositorioProyeccionImpacto` listos para adaptador MongoDB en despliegue.
5. **Base de `Ascenso.S6.ComunicacionesReporteria`:** crear los 7 proyectos estándar en `Ascenso.sln` + guía `docs/guias-integracion/05-s6-comunicaciones-reporteria.md` lista para que el equipo enchufe `RF-23`, `RF-29–32`, `RF-34`, `RF-36` y `RF-40`.
