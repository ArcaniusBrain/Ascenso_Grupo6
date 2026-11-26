# Tasks — `gestionar-universidades-convenios` (RF-14 · S2)

## 1. Domain (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Domain`)

- [x] **1.1** Implementar `ViaDatosAcademicos` (`API` | `MANUAL`), value objects `ContactoDesignado` y `Convenio`, entidades internas `CarreraElegible` y `CostoMatricula` (append-only con `VersionRegistro`), eventos `UniversidadRegistradaEvento` y `CostoMatriculaRegistradoEvento`, y la raíz `UniversidadAliada : RaizDeAgregado<Guid>` (`Version` explícita `ADR-25`).
  - *Verificación:* pruebas unitarias en `Ascenso.S2.CatalogoFondos.UnitTests` cubren todos los mensajes literales y reglas de `HU-14.1` (E1, E2, E4, E5, E8–E10) y `HU-14.2` (E1, E2, E4, E5).
- [x] **1.2** Definir `IUniversidadAliadaRepository` en Domain y registrar `UniversidadAliada` en `ArquitecturaCatalogoFondosTests`.
  - *Verificación:* `S2.Domain` permanece libre de referencias externas y `ArquitecturaCatalogoFondosTests` aprueba.

## 2. Application (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Application`)

- [x] **2.1** Implementar `PoliticaAutorizacionUniversidades` (bloqueo explícito al enlace de universidad y roles no autorizados, lectura de catálogo habilitada para postulantes/becarios e internos) y los comandos/queries `MediatR 12.5.0`: `RegistrarUniversidadAliadaCommand`, `ActualizarConvenioUniversidadCommand`, `AgregarCarreraElegibleCommand`, `RegistrarCostoMatriculaCommand` (publica `CostoMatriculaVigenteV1` vía Outbox), `RegistrarInicioInscripcionesUniversidadCommand` (`HU-14.3`), `ListarUniversidadesQuery`, `ConsultarUniversidadDetalleQuery`, `ConsultarCostoMatriculaPeriodoQuery` y `ConsultarInicioInscripcionesUniversidadQuery`.
  - *Verificación:* pruebas verifican autorización (`HU-14.1 E7`), mensajes literales (`HU-14.1`, `HU-14.2`, `HU-14.3`) y publicación atómica de `CostoMatriculaVigenteV1`.

## 3. Infrastructure (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Infrastructure`)

- [x] **3.1** Ampliar `CatalogoFondosDbContext` (`s2_catalogo_fondos`) con mapeos EF Core para `universidades_aliadas`, `carreras_elegibles` y `costos_matricula` (sin clave única sobre `(universidad_id, periodo)` para preservar historial append-only `HU-14.2`), implementar `UniversidadAliadaRepositoryEfCore` y registrarlo en `DependencyInjection.cs`.
  - *Verificación:* pruebas de integración SQLite real validan persistencia multi-corrección de costos, aislamiento de carreras por universidad y persistencia atómica de `CostoMatriculaVigenteV1` en `outbox_messages`.

## 4. Api y Contratos (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Api` + `contracts/`)

- [x] **4.1** Exponer los endpoints REST en `Ascenso.S2.CatalogoFondos.Api/Program.cs` (`/api/v1/universidades`, `/convenio`, `/carreras`, `/costos-matricula`, `/inicios-inscripciones`) con `ProblemDetails` RFC 9457.
  - *Verificación:* pruebas de integración HTTP (`WebApplicationFactory`) cubren `HU-14.1` (E1–E10), `HU-14.2` (E1–E5) y `HU-14.3` (E1, E2, E4–E6), manteniendo intactas todas las pruebas previas de `RF-38`.
- [x] **4.2** Actualizar `contracts/openapi.yaml` bajo tag `S2` con las rutas y esquemas de `RF-14`, y registrar las decisiones cerradas del Anexo B en `docs/arquitectura/pendientes-v1.3.md`.
  - *Verificación:* contratos consistentes con `Program.cs`.
- [x] **4.3** Ejecutar verificación integral (`dotnet build Ascenso.sln --configuration Release`, `dotnet test Ascenso.sln --configuration Release` y `openspec validate --all --strict --no-interactive`).
  - *Verificación:* 0 advertencias, 0 errores, 100 % de pruebas en verde y OpenSpec válido en modo estricto.
