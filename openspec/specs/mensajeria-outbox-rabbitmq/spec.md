# Especificación: Mensajería Outbox -> RabbitMQ y Consumidores Inbox (ADR-13)

## Purpose

Implementar el despacho transaccional Outbox hacia RabbitMQ reutilizable en `Ascenso.Messaging.Outbox` (conectado en `S1`, `S2` y `S6`) y el consumidor idempotente con `InboxMessage` en `S6` para alimentar automáticamente las proyecciones CQRS de reportes de impacto (`RF-33`).

## Requirements

### Requirement: Despacho reutilizable de Outbox hacia RabbitMQ sin acoplar pruebas locales

`Ascenso.Messaging.Outbox` MUST proveer un despachador configurable (`OutboxDispatcherWorker<TDbContext>`) que lea `outbox_messages` pendientes en orden cronológico, publique mensajes persistentes al exchange topic `ascenso.eventos` usando `EventType` como routing key, marque `ProcessedAtUtc` al confirmar y registre `Attempts` y `LastError` ante fallo del broker sin detener el servicio. Si `RabbitMq:Host` está vacío, el worker MUST permanecer inactivo.

#### Scenario: Despacho exitoso marca ProcessedAtUtc y evita republicacion

- **WHEN** existen mensajes pendientes (`ProcessedAtUtc == null`) en `outbox_messages`
- **THEN** el despachador los publica en orden por `OccurredAtUtc`
- **AND** actualiza `ProcessedAtUtc` para que ciclos posteriores no vuelvan a publicarlos

#### Scenario: Fallo del broker preserva el mensaje para reintento

- **WHEN** la publicación en RabbitMQ falla por indisponibilidad temporal
- **THEN** el despachador incrementa `Attempts`, guarda el detalle en `LastError` y mantiene `ProcessedAtUtc == null` para reintentarlo en el siguiente ciclo

### Requirement: Consumo idempotente con Inbox en S6 para proyecciones CQRS de impacto

`S6` MUST consumir los eventos `PeriodoMarcadoActual.v1`, `AporteRegistrado.v1` y `PagoConfirmado.v1` desde RabbitMQ actualizando sus tablas locales `proyeccion_*` y registrando `InboxMessage` (`ConsumerName = "s6.proyeccion-impacto"`) en la misma transacción para garantizar entrega efectivamente única (ADR-13).

#### Scenario: Reentrega duplicada del mismo EventId no duplica aportes ni desembolsos

- **WHEN** `ProyectorEventosImpacto` recibe dos veces un evento `AporteRegistrado.v1` con el mismo `EventId`
- **THEN** la primera ejecución inserta la proyección y el registro en `inbox_messages`
- **AND** la segunda ejecución retorna `DUPLICADO_OMITIDO` sin alterar las tablas de proyección
