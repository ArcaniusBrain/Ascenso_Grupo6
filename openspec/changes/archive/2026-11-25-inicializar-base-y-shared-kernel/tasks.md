# Tasks

Detalle arquitectónico en `design.md`. Todas las tareas pertenecen a la base común (Semana 1, Paso 1 del equipo).

## 1. Solución, paquetes y contratos raíz

- [x] 1.1 Crear `Ascenso.sln`, `Directory.Build.props` (`net10.0`, nullable, warnings as errors) y `Directory.Packages.props` con versiones fijadas (ADR-24); verificar con `dotnet --info` y compilación limpia
- [x] 1.2 Copiar los contratos vigentes desde `docs/arquitectura/v1.3/03_CONTRATOS/` a `contracts/` y actualizar `contracts/README.md` y `openspec/config.yaml` para señalar `contracts/` como fuente activa; verificar que `openapi.yaml` y `openapi-interno.yaml` coinciden

## 2. Domain / `building-blocks/Ascenso.SharedKernel`

- [x] 2.1 Crear proyecto `Ascenso.SharedKernel` (sin dependencias de infraestructura) con `Entidad<TId>`, `RaizDeAgregado<TId>` (`Version: int` + `IncrementarVersion()`, DDD-07/ADR-25), `IEventoDeDominio`, `IManejador<TEvento>` y `ExcepcionDeDominio`; verificar con pruebas unitarias de igualdad por `Id`, registro de eventos e incremento explícito de `Version`
- [x] 2.2 Implementar los 9 Value Objects del Shared Kernel (DDD §5): `Dinero`, `Cedula`, `Contacto`, `VentanaFechas`, `FechaLimite`, `Promedio`, `Justificacion`, `ReferenciaArchivo`, `CodigoPeriodo` más `IRelojNegocio` (UTC-4, R20, ADR-17); verificar con pruebas unitarias cubriendo bordes (monedas distintas, montos negativos, 23:59:59 UTC-4 vs 00:00:00 del día siguiente)
- [x] 2.3 Configurar `MediatR` `12.5.0` (Apache 2.0, fijado en `Directory.Packages.props`, ADR-27) y `MediatR.Contracts` `2.0.1` en `Ascenso.SharedKernel` (`IEventoDeDominio : INotification`), junto con las interfaces/enums de OTP (`IServicioOtp`, `TipoOperacionSensible`, `CanalCodigo`, DDD-10); verificar publicación de eventos con `MediatR` y regla automática contra `MediatR >= 13.0.0` en pruebas unitarias

## 3. Contracts / `building-blocks/Ascenso.Contracts`

- [x] 3.1 Crear proyecto `Ascenso.Contracts` con `IntegrationEventEnvelope<TPayload>` y los registros `v1` de los eventos prioritarios del catálogo (incluyendo `UsuarioRegistradoV1`, `OfertaEmitidaV1`, `BecaRevocadaV1`, `PeriodoMarcadoActualV1`, `CostoMatriculaVigenteV1`); verificar serialización JSON ida y vuelta en pruebas unitarias sin exponer PII prohibida

## 4. Infrastructure base y despliegue local

- [x] 4.1 Crear proyecto `building-blocks/Ascenso.Messaging.Outbox` (separado del Shared Kernel) con `OutboxMessage`, `InboxMessage` (clave única `(ConsumerName, EventId)`) y configuraciones EF Core base reutilizables; verificar configuración del modelo en pruebas unitarias
- [x] 4.2 Crear `deploy/docker-compose.yml`, `deploy/.env.example` y `deploy/postgres/init-schemas.sql` con los 6 schemas (`s1_identidad`..`s6_comunicaciones`) y roles aislados sin acceso cruzado, más MongoDB, RabbitMQ, Keycloak y Mailpit; verificar sintaxis de compose y ausencia de secretos reales

## 5. Pruebas de arquitectura, CI y cierre

- [x] 5.1 Crear proyecto `building-blocks/tests/Ascenso.BuildingBlocks.Tests` con pruebas unitarias de `SharedKernel`, `Contracts` y `Outbox`, más prueba de arquitectura que falle si `Ascenso.SharedKernel` o `Ascenso.Contracts` referencian EF Core, MassTransit, ASP.NET Core, MongoDB o Firebase; verificar con `dotnet test`
- [x] 5.2 Agregar `.github/workflows/dotnet.yml` ejecutando `dotnet restore`, `dotnet build` y `dotnet test` en PRs y `main`; actualizar `building-blocks/README.md` y `deploy/README.md`
- [x] 5.3 Ejecutar `dotnet test` y `openspec validate --all --strict`; verificar cero errores antes de archivar

## Workflow follow-up

- Abrir Pull Request borrador `chore/inicializar-base-y-shared-kernel` solo con esta propuesta para revisión rápida del equipo (toca `SharedKernel` y `Contracts`).
- Tras aprobar el diseño, implementar las tareas, pasar `dotnet test` y archivar con `openspec archive inicializar-base-y-shared-kernel -y` dentro del mismo PR.
