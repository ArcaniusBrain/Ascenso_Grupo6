# Tasks — `gestionar-presupuesto-cupos` (RF-12 · S2)

## 1. Contratos y Domain (`Ascenso.Contracts` + `Ascenso.S2.CatalogoFondos.Domain`)

- [x] **1.1** Añadir `CupoLiberadoV1` en `building-blocks/Ascenso.Contracts/Eventos/EventosCatalogoFondosV1.cs`.
  - *Verificación:* `Ascenso.Contracts` compila sin advertencias y sin romper contratos existentes.
- [x] **1.2** Implementar `TipoReservaPresupuesto`, `EstadoReservaPresupuesto`, `MotivoLiberacionReserva`, entidad `CupoCiclo`, entidad `ReservaPresupuesto`, eventos de dominio, raíz `PresupuestoPrograma : RaizDeAgregado<Guid>` (`Version` explícita `ADR-25`) e `IPresupuestoProgramaRepository`; incorporar `PresupuestoPrograma` en `ArquitecturaCatalogoFondosTests`.
  - *Verificación:* pruebas unitarias en `Ascenso.S2.CatalogoFondos.UnitTests` cubren fórmula de 4 términos, igualdad exacta, cupos totales y por ciclo, `Inactiva` no libera, mensajes literales de `HU-12.1`/`HU-12.2` y `ArquitecturaCatalogoFondosTests` aprueba.

## 2. Application (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Application`)

- [x] **2.1** Implementar `PoliticaAutorizacionPresupuesto`, `IConsultorVinculoReservaS4`, `ISincronizadorPresupuestoPrograma`, comandos/queries `MediatR 12.5.0`: `ConsultarPresupuestoProgramaQuery`, `ConsultarDisponibilidadInternaQuery`, `ReservarOfertaInternaCommand`, `ReservarRenovacionInternaCommand`, `LiberarReservaInternaCommand`, `VincularReservaCommand`, `ConfirmarReservaCommand`, `RegistrarCupoCicloCommand`, `LiberarBecaRevocadaCommand` y `ConciliarReservasHuerfanasCommand` (`ADR-26`).
  - *Verificación:* handlers cubren idempotencia por `operationId`, unicidad por clave de negocio, `R32/A2` (duración completa), `R31/A1` (renovación al solicitar), Outbox `CupoLiberado.v1` y fail closed de `ADR-26`.
- [x] **2.2** Integrar sincronización idempotente de `PresupuestoPrograma` desde `CrearProgramaHandler` (`RF-11`) y `RegistrarAporteHandler` (`RF-13`).
  - *Verificación:* crear programa y registrar aportes actualizan `PresupuestoPrograma` sin romper pruebas previas de `RF-11` ni `RF-13`.

## 3. Infrastructure (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Infrastructure`)

- [x] **3.1** Ampliar `CatalogoFondosDbContext` (`s2_catalogo_fondos`) con tablas `presupuestos_programa`, `cupos_ciclo` y `reservas_presupuesto` (índices únicos por `programa_id`, `operation_id` y clave activa de negocio), implementar `PresupuestoProgramaRepositoryEfCore`, `ValidadorScopeInternoFondos` (OAuth2 `client_credentials`) y `ConsultorVinculoReservaS4Http` (timeout 3s, máx. 2 reintentos idempotentes, circuit breaker y fail closed).
  - *Verificación:* pruebas de integración SQLite real validan concurrencia paralela en último cupo (`HU-12.1 E6`), frontera privada `401`/`403` y conciliación `ADR-26`.

## 4. Api y Contratos (`services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Api` + `contracts/`)

- [x] **4.1** Exponer en `Program.cs` la ruta pública `GET /api/v1/programas/{programaId}/presupuesto` y las 4 rutas privadas `/internal/v1/fondos/reservas`, `/internal/v1/fondos/reservas/renovaciones`, `/internal/v1/fondos/reservas/{reservaId}/liberar` y `/internal/v1/fondos/{programaId}/disponibilidad` con `ProblemDetails` RFC 9457.
  - *Verificación:* pruebas de integración HTTP (`WebApplicationFactory`) cubren `HU-12.1` (E1–E12), `HU-12.2` (E1–E5), concurrencia en disputa del último cupo, seguridad interservicios (`401`/`403`), Outbox `CupoLiberado.v1` y conciliación `ADR-26`.
- [x] **4.2** Actualizar `contracts/openapi.yaml` (ruta pública `GET /api/v1/programas/{programaId}/presupuesto`), registrar `C19` en `docs/arquitectura/pendientes-v1.3.md` y actualizar `docs/guias-integracion/04-s2-catalogo-fondos-calendario.md` y `docs/guias-integracion/README.md`.
  - *Verificación:* contratos públicos/privados y guías 100 % consistentes con `Program.cs`.
- [x] **4.3** Ejecutar verificación integral (`dotnet build Ascenso.sln --configuration Release`, `dotnet test Ascenso.sln --configuration Release` y `openspec validate --all --strict --no-interactive`).
  - *Verificación:* 0 advertencias, 0 errores, 100 % de pruebas en verde y OpenSpec válido en modo estricto.
