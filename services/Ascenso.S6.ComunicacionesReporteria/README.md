# S6 · Comunicaciones y Reportería (`Ascenso.S6.ComunicacionesReporteria`)

**RF:** RF-23, 29 a 34, 36, 40 (schema aislado PostgreSQL: `s6_comunicaciones`).

Base de 7 proyectos en `Ascenso.sln` con **`RF-33`** (`ConsentimientoDonante` irrevocable `R36/A6`, capa única `ServicioAnonimizacionImpacto` y generación asíncrona de reportes de impacto `HU-33.1`/`HU-33.2`), `Outbox`/`Inbox` y `RegistroDecision` append-only listos para los demás RF de comunicaciones y reportería del equipo.

Guía completa de integración: [`docs/guias-integracion/05-s6-comunicaciones-reporteria.md`](../../docs/guias-integracion/05-s6-comunicaciones-reporteria.md).
