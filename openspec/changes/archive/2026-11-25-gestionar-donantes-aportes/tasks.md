# Tasks — `gestionar-donantes-aportes` (RF-13 · S2)

## 1. Contratos y Domain (`Ascenso.Contracts` + `Ascenso.S2.CatalogoFondos.Domain`)

- [x] **1.1** Añadir `AporteRegistradoV1` en `building-blocks/Ascenso.Contracts/Eventos/EventosCatalogoFondosV1.cs`.
  - *Verificación:* `Ascenso.Contracts` compila sin advertencias y sin romper eventos existentes.
- [x] **1.2** Implementar `TipoDonante`, value object `ContactoDonante`, entidad append-only `Aporte`, eventos `DonanteRegistradoEvento` y `AporteRegistradoEvento`, raíz `Donante : RaizDeAgregado<Guid>` (`Version` explícita `ADR-25`) e `IDonanteRepository`; incorporar `Donante` en `ArquitecturaCatalogoFondosTests`.
  - *Verificación:* pruebas unitarias en `Ascenso.S2.CatalogoFondos.UnitTests` cubren todas las reglas y mensajes literales de `HU-13.1` y `HU-13.2`, y `ArquitecturaCatalogoFondosTests` aprueba.

## 2. Application (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Application`)

- [x] **2.1** Implementar `PoliticaAutorizacionDonantes` y comandos/queries `MediatR 12.5.0`: `RegistrarDonanteCommand`, `RegistrarAporteCommand` (publica `AporteRegistrado.v1` en Outbox atómicamente), `ListarDonantesQuery`, `ConsultarDonanteDetalleQuery` y `ConsultarHistorialAportesDonanteQuery` (con filtro por `codigoPeriodo`).
  - *Verificación:* pruebas cubren permisos (`HU-13.1 E5`, `HU-13.2 E8`), ausencia de programas (`HU-13.2 E3`), periodo obligatorio (`HU-13.2 E2`), monto `<= 0` (`HU-13.2 E7`) y mensajes sin aportes (`HU-13.2 E6, E9`).

## 3. Infrastructure (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Infrastructure`)

- [x] **3.1** Ampliar `CatalogoFondosDbContext` (`s2_catalogo_fondos`) con tablas `donantes` y `aportes` (índice por `(donante_id, codigo_periodo)`), implementar `DonanteRepositoryEfCore` y reemplazar `ConsultorDonantesYAportesPorDefecto` por `ConsultorDonantesYAportesEfCore` en `DependencyInjection.cs`.
  - *Verificación:* pruebas de integración SQLite real validan ledger append-only, publicación de `AporteRegistrado.v1` en `outbox_messages` y actualización automática del presupuesto total en `GET /api/v1/programas/{programaId}` (`HU-11.2 E1/E2/E13`).

## 4. Api y Contratos (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Api` + `contracts/`)

- [x] **4.1** Exponer los endpoints REST en `Program.cs` (`GET/POST /api/v1/donantes`, `GET /api/v1/donantes/{id}`, `GET/POST /api/v1/donantes/{id}/aportes` y `POST /api/v1/programas/{programaId}/aportes`) con `ProblemDetails` RFC 9457.
  - *Verificación:* pruebas de integración HTTP (`WebApplicationFactory`) cubren `HU-13.1` (E1–E5), `HU-13.2` (E1–E9) y la integración viva con `RF-11`.
- [x] **4.2** Actualizar `contracts/openapi.yaml` bajo tag `S2`, registrar `C18` en `docs/arquitectura/pendientes-v1.3.md` y actualizar `docs/guias-integracion/04-s2-catalogo-fondos-calendario.md` y `docs/guias-integracion/README.md`.
  - *Verificación:* contratos y guías consistentes con `Program.cs`.
- [x] **4.3** Ejecutar verificación integral (`dotnet build Ascenso.sln --configuration Release`, `dotnet test Ascenso.sln --configuration Release` y `openspec validate --all --strict --no-interactive`).
  - *Verificación:* 0 advertencias, 0 errores, 100 % de pruebas en verde y OpenSpec válido en modo estricto.
