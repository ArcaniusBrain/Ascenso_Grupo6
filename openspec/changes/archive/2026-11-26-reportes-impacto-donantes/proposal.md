# Proposal — `reportes-impacto-donantes` (RF-33 · S6)

## Why

Los donantes de la Fundación necesitan recibir reportes periódicos del impacto de sus aportes (número de becarios, perfil general, ejecución presupuestaria con desembolsos confirmados y resultados académicos agregados) en minutos y sin comprometer la confidencialidad de los datos socioeconómicos individuales (`RF-33`, `HU-33.1`), mostrando de forma individual únicamente a los becarios que hayan registrado explícitamente su consentimiento irrevocable para ese donante (`HU-33.2`, `R36/A6`).

Además, `services/Ascenso.S6.ComunicacionesReporteria/` aún no tiene esqueleto en `Ascenso.sln`; este cambio crea la base limpia de `S6` (schema `s6_comunicaciones`) sobre la que también se apoyarán los demás RF de comunicaciones y reportería del equipo.

## What Changes

- **Base del microservicio `Ascenso.S6.ComunicacionesReporteria` (`s6_comunicaciones`):** 7 proyectos estándar (`Domain`, `Application`, `Infrastructure`, `Api`, `UnitTests`, `IntegrationTests`, `ArchitectureTests`) integrados en `Ascenso.sln`, con `Outbox`/`Inbox` y `RegistroDecision` append-only (`ADR-04`, `ADR-05`, `ADR-12`, `ADR-25`, `ADR-27`).
- **Agregado raíz `ConsentimientoDonante : RaizDeAgregado<Guid>` (`DDD_v3.3.md` §14, `HU-33.2`, `R36/A6`):**
  - Uno por `(BecarioId, DonanteId)`, irrevocable una vez aceptado, idempotente ante confirmaciones repetidas (conserva la fecha original) y registrable exclusivamente por el propio becario.
- **Capa única centralizada de agregación y anonimización (`ServicioAnonimizacionImpacto`, `Anexo A HU-33.1`):**
  - Garantiza por diseño que jamás se expongan ingresos familiares, composición familiar ni identificaciones en reportes dirigidos al donante.
  - Incluye en ejecución presupuestaria únicamente desembolsos confirmados (`CONFIRMADO`, excluyendo fallidos/devueltos/programados) y muestra `"Aún no hay datos académicos para este periodo."` cuando no hay reportes académicos.
  - Presenta individualmente (solo datos no socioeconómicos) únicamente a becarios con `ConsentimientoDonante` vigente para ese donante.
- **Procesamiento asíncrono en segundo plano (`HU-33.1 E9, E12`):**
  - Solicitud `POST /api/v1/reportes/impacto` que encola la generación sobre proyecciones locales de `S6` y responde inmediatamente `"El reporte se está preparando."` (`202 Accepted`) sin bloquear la operación diaria, más consulta de estado/contenido en `GET /api/v1/reportes/impacto/{reporteId}` y listado/consulta en `GET /api/v1/reportes/impacto`.
- **Consentimientos API (`HU-33.2`):**
  - `POST /api/v1/consentimientos-donantes` (solo el propio becario, idempotente) y `GET /api/v1/consentimientos-donantes/mios`.
- **Guía de integración para el equipo:** nueva guía `docs/guias-integracion/05-s6-comunicaciones-reporteria.md` y actualización de `docs/guias-integracion/README.md`.
