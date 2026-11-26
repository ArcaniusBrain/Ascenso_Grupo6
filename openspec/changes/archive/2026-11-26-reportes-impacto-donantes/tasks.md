# Tasks — `reportes-impacto-donantes` (RF-33 · S6)

## 1. Esqueleto S6 y Domain (`services/Ascenso.S6.ComunicacionesReporteria/`)

- [x] **1.1** Crear los 7 proyectos de `Ascenso.S6.ComunicacionesReporteria` (`Domain`, `Application`, `Infrastructure`, `Api`, `UnitTests`, `IntegrationTests`, `ArchitectureTests`) y agregarlos a `Ascenso.sln`.
  - *Verificación:* `Ascenso.sln` compila con 0 advertencias y `ArquitecturaComunicacionesTests` garantiza aislamiento limpio de `Domain` y `Application`.
- [x] **1.2** Implementar `ConsentimientoDonante : RaizDeAgregado<Guid>` (`ADR-25`, `R36/A6`), `IConsentimientoDonanteRepository`, modelos de entrada/salida y la capa única `ServicioAnonimizacionImpacto` (`Anexo A HU-33.1`).
  - *Verificación:* pruebas unitarias validan irreversibilidad/idempotencia de consentimiento, ausencia estructural de datos socioeconómicos, exclusión de desembolsos fallidos y mensajes literales.

## 2. Application (`Ascenso.S6.ComunicacionesReporteria.Application`)

- [x] **2.1** Implementar `PoliticaAutorizacionImpacto`, puertos de proyecciones (`IRepositorioProyeccionImpacto`, `ISolicitudReporteImpactoRepository`, `IColaProcesamientoReportesImpacto`), comandos/queries `MediatR 12.5.0` para `HU-33.2` (`RegistrarConsentimientoDonanteCommand`, `ListarMisConsentimientosQuery`) y `HU-33.1` (`SolicitarReporteImpactoCommand`, `ProcesarReporteImpactoCommand`, `ConsultarReporteImpactoQuery`, `ListarReportesImpactoQuery`).
  - *Verificación:* pruebas cubren todos los flujos alternos (`HU-33.1 E2, E7, E8, E11, E12` y `HU-33.2 E3, E4`).

## 3. Infrastructure (`Ascenso.S6.ComunicacionesReporteria.Infrastructure`)

- [x] **3.1** Implementar `ComunicacionesDbContext` (schema `s6_comunicaciones` solo en Npgsql, `Outbox`/`Inbox`, `RegistroDecision` append-only, tablas `consentimientos_donante`, `solicitudes_reporte_impacto` y proyecciones locales de calendario/aportes/becarios/desembolsos/reportes académicos), repositorios EF Core y worker en segundo plano (`Channel<Guid>` + `BackgroundService`).
  - *Verificación:* pruebas de integración SQLite real validan persistencia, unicidad `(becario_id, donante_id)` y procesamiento asíncrono no bloqueante.

## 4. Api, Contratos y Guías

- [x] **4.1** Exponer los endpoints REST en `Ascenso.S6.ComunicacionesReporteria.Api/Program.cs` (`POST /api/v1/consentimientos-donantes`, `GET /api/v1/consentimientos-donantes/mios`, `POST /api/v1/reportes/impacto`, `GET /api/v1/reportes/impacto/{reporteId}`, `GET /api/v1/reportes/impacto`) con `ProblemDetails` RFC 9457.
  - *Verificación:* pruebas de integración HTTP (`WebApplicationFactory`) cubren `HU-33.1` (E1–E12) y `HU-33.2` (E1–E4), incluyendo verificación explícita de que ningún campo socioeconómico aparece en la respuesta JSON.
- [x] **4.2** Actualizar `contracts/openapi.yaml` bajo tag `S6`, registrar `C20` en `docs/arquitectura/pendientes-v1.3.md`, crear `docs/guias-integracion/05-s6-comunicaciones-reporteria.md` y actualizar `docs/guias-integracion/README.md`.
  - *Verificación:* contratos y guías consistentes con `Program.cs`.
- [x] **4.3** Ejecutar verificación integral (`dotnet build Ascenso.sln --configuration Release`, `dotnet test Ascenso.sln --configuration Release` y `openspec validate --all --strict --no-interactive`).
  - *Verificación:* 0 advertencias, 0 errores, 100 % de pruebas en verde y OpenSpec válido en modo estricto.
