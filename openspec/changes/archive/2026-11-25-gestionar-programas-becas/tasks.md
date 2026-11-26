# Tasks — `gestionar-programas-becas` (RF-11 · S2)

## 1. Domain (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Domain`)

- [x] **1.1** Implementar `ModalidadPago`, `AlcancePrograma`, value object `CondicionesPermanencia`, entidades internas `RestriccionElegibilidad` y `AsociacionDonantePrograma`, evento `ProgramaCreadoEvento`, raíz `Programa : RaizDeAgregado<Guid>` (`Version` explícita `ADR-25`) e `IProgramaRepository`.
  - *Verificación:* pruebas unitarias en `Ascenso.S2.CatalogoFondos.UnitTests` cubren todas las reglas y mensajes literales de `HU-11.1` (E1–E11) y `HU-11.2` (E4–E14).
- [x] **1.2** Incorporar `Programa` en `ArquitecturaCatalogoFondosTests`.
  - *Verificación:* `S2.Domain` permanece libre de referencias externas y `ArquitecturaCatalogoFondosTests` aprueba.

## 2. Application (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Application`)

- [x] **2.1** Implementar `PoliticaAutorizacionProgramas`, puerto `IConsultorDonantesYAportes` y los comandos/queries `MediatR 12.5.0`: `CrearProgramaCommand`, `ActualizarConfiguracionBaseProgramaCommand`, `AsociarDonanteAProgramaCommand`, `ConfigurarElegibilidadProgramaCommand`, `ConfigurarCondicionesProgramaCommand`, `ListarProgramasQuery` y `ConsultarProgramaDetalleQuery`.
  - *Verificación:* pruebas verifican permisos (`HU-11.1 E7`), validación contra `CalendarioAcademico` (`HU-11.1 E3`), validación contra `UniversidadAliada` (`HU-11.2 E4, E5, E11, E12`), suma de aportes por donante (`HU-11.2 E1, E2, E3, E10, E13`) y promedio no numérico (`HU-11.2 E7`).

## 3. Infrastructure (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Infrastructure`)

- [x] **3.1** Ampliar `CatalogoFondosDbContext` (`s2_catalogo_fondos`) con tablas `programas`, `programa_restricciones_elegibilidad` y `programa_donantes`, implementar `ProgramaRepositoryEfCore`, `ConsultorDonantesYAportesPorDefecto` y actualizar `ConsultorElementosPeriodoEfCore` (contando programas reales ligados a cada periodo) en `DependencyInjection.cs`.
  - *Verificación:* pruebas de integración SQLite real validan persistencia completa de `Programa`, restricciones estructuradas, donantes únicos y conteo en `GET /api/v1/periodos/{codigoPeriodo}`.

## 4. Api y Contratos (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Api` + `contracts/`)

- [x] **4.1** Exponer los endpoints REST en `Ascenso.S2.CatalogoFondos.Api/Program.cs` (`/api/v1/programas`, `/configuracion-base`, `/donantes`, `/elegibilidad`, `/condiciones`) con `ProblemDetails` RFC 9457.
  - *Verificación:* pruebas de integración HTTP (`WebApplicationFactory`) cubren `HU-11.1` (E1–E11) y `HU-11.2` (E1–E14), manteniendo en verde todas las pruebas previas de `RF-38` y `RF-14`.
- [x] **4.2** Actualizar `contracts/openapi.yaml` bajo tag `S2`, cerrar las decisiones Anexo B (`C17`) en `docs/arquitectura/pendientes-v1.3.md` y actualizar la guía del equipo `docs/guias-integracion/04-s2-catalogo-fondos-calendario.md`.
  - *Verificación:* contratos y guías consistentes con `Program.cs`.
- [x] **4.3** Ejecutar verificación integral (`dotnet build Ascenso.sln --configuration Release`, `dotnet test Ascenso.sln --configuration Release` y `openspec validate --all --strict --no-interactive`).
  - *Verificación:* 0 advertencias, 0 errores, 100 % de pruebas en verde y OpenSpec válido en modo estricto.
