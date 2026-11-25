# Building blocks

Código compartido por todos los servicios (arquitectura v1.3 §12, DDD v3.3 §5 y `ADR_CORRECTIVOS_V1.3.md`).

| Proyecto | Contiene | Regla |
|---|---|---|
| [`Ascenso.SharedKernel/`](Ascenso.SharedKernel/) | Tipos base DDD (`Entidad`, `RaizDeAgregado` con `Version` explícita, `IEventoDeDominio`, `IManejador`, `ExcepcionDeDominio`), los 9 value objects comunes (`Dinero`, `Cedula`, `Contacto`, `VentanaFechas`, `FechaLimite`, `Promedio`, `Justificacion`, `ReferenciaArchivo`, `CodigoPeriodo`), `IRelojNegocio` (UTC-4, R20) e `IServicioOtp` + `TipoOperacionSensible` | **Dominio puro:** sin EF Core, ASP.NET, MassTransit, MongoDB ni Firebase (solo `MediatR.Contracts` `2.0.1`). Todo cambio se acuerda entre todos. |
| [`Ascenso.Contracts/`](Ascenso.Contracts/) | `IntegrationEventEnvelope<T>` y eventos de integración versionados (`UsuarioRegistradoV1`, `OfertaEmitidaV1`, `BecaRevocadaV1`, etc.) | Sin dependencias de `SharedKernel` ni infraestructura. Nunca expone entidades de dominio ni PII prohibida. |
| [`Ascenso.Messaging.Outbox/`](Ascenso.Messaging.Outbox/) | Entidades técnicas `OutboxMessage` e `InboxMessage` (unicidad `(ConsumerName, EventId)`) y extensiones EF Core (`ConfigurarOutboxEInbox`, `ConfigurarVersionExplicita`) | Referenciado **solo por `*.Infrastructure`**, nunca por `*.Domain`. |
| [`tests/Ascenso.BuildingBlocks.Tests/`](tests/Ascenso.BuildingBlocks.Tests/) | Pruebas unitarias de los 9 VOs, concurrencia por `Version`, publicación con `MediatR 12.5.0`, serialización de contratos y **pruebas de arquitectura** (pureza de dependencias y bloqueo de `MediatR >= 13.0.0`) | Se ejecuta automáticamente en CI (`.github/workflows/dotnet.yml`). |
