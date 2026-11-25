# Tasks — `implementar-registro-decisiones` (RF-35)

## 1. Domain (`building-blocks/Ascenso.SharedKernel`)

- [x] **1.1** Crear las enumeraciones exactas del DDD v3.3 §14 en `building-blocks/Ascenso.SharedKernel/Trazabilidad/`: `TipoSujeto` (5 valores) y `TipoDecision` (38 valores) con helper `TipoDecisionExtensions.EsAmbitoFinanciero()`.
  - *Verificación:* prueba unitaria `Catalogo_TipoSujeto_Y_TipoDecision_Coinciden_Con_DDD_v3_3` en verde.
- [x] **1.2** Crear los value objects inmutables `SujetoRef` y `ActorDecision` y las excepciones `ExcepcionInmutabilidadRegistro` (mensaje exacto `"El registro de decisiones no se puede modificar ni eliminar."`) y `ExcepcionAccesoDenegadoDecision`.
  - *Verificación:* pruebas unitarias en `RegistroDecisionDominioTests` en verde.
- [x] **1.3** Implementar el registro inmutable `RegistroDecision` (sin heredar de `RaizDeAgregado`, sin setters públicos ni métodos de mutación) con fábricas `Crear(...)` y `CrearAutomatica(...)`, conservando `ProgramaId`, `PropietarioUsuarioId`, `EstadoAnterior`, `EstadoNuevo`, `DatosJson`, `ServicioOrigen` y `CorrelationId`.
  - *Verificación:* pruebas unitarias de `HU-35.1` Escenarios 1, 2, 4/5, 6 y cierre de *Inactiva* en verde.
- [x] **1.4** Implementar `PoliticaAutorizacionDecision` y `ContextoSolicitanteDecision` para registro (`HU-35.1 Escenario 7`), prohibición absoluta de mutación/eliminación (`HU-35.1 Escenarios 4 y 5`) y alcances de consulta (`DIRECCION`, `COORDINADOR`, `FINANZAS`, `POSTULANTE_O_BECARIO`).
  - *Verificación:* pruebas unitarias y de integración en verde para los 5 perfiles.

## 2. Application (`Ascenso.SharedKernel` puertos de lectura + `Ascenso.Contracts`)

- [x] **2.1** Definir en `building-blocks/Ascenso.SharedKernel/Trazabilidad/` el puerto de solo lectura `IConsultaRegistroDecisiones`, el filtro `ConsultaRegistroDecisionesFiltro`, el DTO `DecisionProyectada` y `ResultadoConsultaDecisiones` (`"Este caso aún no tiene decisiones registradas."`).
  - *Verificación:* prueba de reflexión `HU35_2_Puerto_IConsultaRegistroDecisiones_Es_Estrictamente_Solo_Lectura` en verde.
- [x] **2.2** Crear `building-blocks/Ascenso.Contracts/Eventos/EventosTrazabilidadV1.cs` con `DecisionRegistradaV1` (versión `1`, con `ProgramaId` y `PropietarioUsuarioId` opacos, sin PII prohibida).
  - *Verificación:* prueba `Contrato_DecisionRegistradaV1_Serializa_En_Envelope_Y_Sin_Pii_Prohibida` en verde.

## 3. Infrastructure (`building-blocks/Ascenso.Messaging.Outbox` + `deploy/postgres`)

- [x] **3.1** Agregar `Microsoft.EntityFrameworkCore.Sqlite.Core` (`10.0.0`) y `SQLitePCLRaw.bundle_e_sqlite3` (`3.0.2`, sin vulnerabilidades NuGet Audit) en `Directory.Packages.props` para pruebas de integración y extender `OutboxModelBuilderExtensions` con `ConfigurarRegistroDecisiones()`.
  - *Verificación:* `dotnet restore` y `dotnet build Ascenso.sln --configuration Release` con 0 errores y 0 advertencias.
- [x] **3.2** Implementar `AppendOnlyRegistroDecisionInterceptor` (`SaveChangesInterceptor`) que lanza `ExcepcionInmutabilidadRegistro` ante `EntityState.Modified` o `EntityState.Deleted`.
  - *Verificación:* prueba `HU35_1_Escenarios4y5_Doble_Barrera_AppendOnly_Bloquea_Modificacion_Y_Eliminacion` (barrera 1 EF Core) en verde.
- [x] **3.3** Implementar `RegistroDecisionSqlAppendOnly` (SQL PostgreSQL + SQLite) y actualizar `deploy/postgres/init-schemas.sql` con `public.impedir_modificacion_registro_decisiones()`.
  - *Verificación:* pruebas `SqlAppendOnly_PostgreSql_Revoca_Update_Y_Delete_Y_Crea_Trigger` y barrera 2 SQL directo en SQLite en verde.
- [x] **3.4** Implementar `UnidadAtomicaDecisiones` para confirmar atómicamente `Agregado (Version++) + RegistroDecision + OutboxMessage (DecisionRegistradaV1)` y revertir ante fallo con `"No se pudo registrar la decisión. No se aplicó ningún cambio."`.
  - *Verificación:* pruebas SQLite reales `HU35_1_Escenarios1y6...`, `HU35_1_Escenario3...` (rollback atómico real) y `HU35_1_Escenario8...` (concurrencia optimista real) en verde.
- [x] **3.5** Implementar `ConsultorRegistroDecisionesEfCore` (`AsNoTracking()`) cumpliendo `IConsultaRegistroDecisiones`, orden cronológico y alcances por rol.
  - *Verificación:* prueba SQLite real `HU35_2_Escenarios_Completos_Orden_Vacio_SoloLectura_Y_Alcances_Por_Rol` en verde.
- [x] **3.6** Verificar pruebas de arquitectura en `Ascenso.BuildingBlocks.Tests` (`SharedKernel` sin infraestructura, `Contracts` sin `SharedKernel` ni infraestructura, `MediatR < 13.0.0`).
  - *Verificación:* `dotnet test Ascenso.sln --configuration Release` → **35/35 pruebas aprobadas**.

## 4. Api / Contratos y cierre del cambio

- [x] **4.1** Actualizar `contracts/EVENTOS_E_INTERFACES.md`, `docs/arquitectura/v1.3/04_MATRICES/CATALOGO_EVENTOS_INTEGRACION.csv` y `docs/arquitectura/pendientes-v1.3.md` incorporando `DecisionRegistrada.v1` y las decisiones cerradas `C11`, `C12` y `C13`.
  - *Verificación:* diff revisado y consistente entre código, contratos y pendientes.
- [x] **4.2** Actualizar `contracts/openapi.yaml` (`GET /api/v1/registro-decisiones` con query params) y `building-blocks/README.md`.
  - *Verificación:* parámetros alineados con `ConsultaRegistroDecisionesFiltro` y `ConsultaResponse`.
- [x] **4.3** Ejecutar validación final completa (`dotnet build`, `dotnet test` y `openspec validate --all --strict --no-interactive`).
  - *Verificación:* 0 errores, 0 advertencias, 35/35 pruebas aprobadas y OpenSpec válido en modo estricto.
