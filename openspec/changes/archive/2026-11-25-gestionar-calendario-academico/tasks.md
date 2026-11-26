# Tasks — `gestionar-calendario-academico` (RF-38 · S2)

## 1. Domain (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Domain`)

- [x] **1.1** Crear la estructura Clean Architecture de `services/Ascenso.S2.CatalogoFondos/` (4 proyectos de servicio + 3 proyectos de pruebas) e incorporarla a `Ascenso.sln`.
  - *Verificación:* `dotnet build Ascenso.sln --configuration Release` compila con 0 errores y 0 advertencias.
- [x] **1.2** Implementar `CalendarioAcademico` (`RaizDeAgregado<Guid>` con `Version` explícita `ADR-25`), entidad interna `PeriodoAcademico` (`EstadoPeriodo`: `ABIERTO`/`CERRADO`), eventos `PeriodoAbiertoEvento` y `PeriodoMarcadoActualEvento`, y entidad `InicioInscripcionesPeriodo` (`FechaInicio` UTC-4, unicidad por `(UniversidadId, CodigoPeriodo)`).
  - *Verificación:* pruebas unitarias en `Ascenso.S2.CatalogoFondos.UnitTests` cubren `HU-38.1` (E1, E2, E7, E10–E13) y `HU-38.2` (E1, E3, E8, E11).
- [x] **1.3** Definir `ICalendarioAcademicoRepository`, `IInicioInscripcionesRepository` e `IUnidadTrabajoCatalogo` en Domain.
  - *Verificación:* `S2.Domain` compila sin referencias externas y las pruebas de arquitectura aprueban.

## 2. Application (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Application`)

- [x] **2.1** Implementar los comandos y queries `MediatR 12.5.0` delgados: `AbrirPeriodoCommand`, `CerrarPeriodoCommand`, `MarcarPeriodoActualCommand` (guarda `PeriodoMarcadoActualV1` en Outbox atómicamente e idempotente por `operationId`), `RegistrarInicioInscripcionesCommand`, `ListarPeriodosQuery` y `ConsultarPeriodoDetalleQuery`.
  - *Verificación:* pruebas verifican autorización exclusiva de Dirección (`HU-38.1 E8`, `HU-38.2 E5`), mensajes literales (`HU-38.1 E2, E3, E5, E7`), idempotencia (`HU-38.2 E8`) y emisión de `PeriodoMarcadoActualV1`.

## 3. Infrastructure (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Infrastructure`)

- [x] **3.1** Implementar `CatalogoFondosDbContext` en schema `s2_catalogo_fondos` (`ConfigurarOutboxEInbox()`, `ConfigurarRegistroDecisiones()`, `ConfigurarVersionExplicita()`, `AppendOnlyRegistroDecisionInterceptor`), mapeos EF Core y repositorios EF Core + `DependencyInjection.cs`.
  - *Verificación:* pruebas de integración SQLite real validan persistencia atómica de `CalendarioAcademico + OutboxMessage (PeriodoMarcadoActualV1)`, unicidad de `(UniversidadId, CodigoPeriodo)` y concurrencia optimista ante dos llamadas simultáneas de `MarcarActual` (`HU-38.2 Escenario 6`).
- [x] **3.2** Implementar la suite obligatoria `Ascenso.S2.CatalogoFondos.ArchitectureTests` (Clase 09, láminas 31 y 39): `S2.Domain` sin infraestructura ni capas exteriores; `S2.Application` sin `Infrastructure` ni `Api`; raíces con `Version` explícita.
  - *Verificación:* `Ascenso.S2.CatalogoFondos.ArchitectureTests` aprueba al 100 %.

## 4. Api y Contratos (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Api` + `contracts/`)

- [x] **4.1** Implementar `Ascenso.S2.CatalogoFondos.Api` (`Program.cs`, `ProblemDetails` RFC 9457, `/health`, `GET/POST /api/v1/periodos`, `GET /api/v1/periodos/{codigoPeriodo}`, `POST /api/v1/periodos/{codigoPeriodo}/cerrar`, `POST /api/v1/periodos/{codigoPeriodo}/inicios-inscripciones` y `POST /api/v1/periodos/actual`).
  - *Verificación:* pruebas de integración HTTP (`WebApplicationFactory`) cubren todos los escenarios de `HU-38.1` (E1–E13) y `HU-38.2` (E1, E3, E5, E6, E8, E11).
- [x] **4.2** Actualizar `contracts/openapi.yaml` añadiendo las rutas complementarias de `HU-38.1` bajo tag `S2` sin alterar `POST /api/v1/periodos/actual`.
  - *Verificación:* diff revisado y consistente con `Program.cs`.
- [x] **4.3** Ejecutar verificación integral (`dotnet build Ascenso.sln --configuration Release`, `dotnet test Ascenso.sln --configuration Release` y `openspec validate --all --strict --no-interactive`).
  - *Verificación:* 0 advertencias, 0 errores, 100 % de pruebas en verde (building-blocks + S1 + S2) y OpenSpec válido en modo estricto.
