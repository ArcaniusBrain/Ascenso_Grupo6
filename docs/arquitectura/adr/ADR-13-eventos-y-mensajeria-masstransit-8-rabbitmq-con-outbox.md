# ADR-13 · Eventos y mensajería: MassTransit 8 + RabbitMQ con Outbox

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Propuesta

## Contexto

La lámina del profesor propone como opcional MassTransit con RabbitMQ para mensajería y trabajos en segundo plano (RabbitMQ es el broker y MassTransit la librería que lo usa: no son alternativas entre sí). Los RNF piden que las operaciones pesadas no bloqueen la plataforma.

## Decisión

- **Dos tipos de eventos.** Los de dominio viven dentro del servicio y se despachan en proceso con el mediador. Los de integración van entre servicios por RabbitMQ; sus contratos viven en Ascenso.Contracts, versionados y sin datos sensibles innecesarios.
- **Outbox transaccional** de MassTransit sobre EF Core, para que ningún mensaje se pierda ni se publique sin que el cambio se haya guardado.
- **Consumidores idempotentes:** cada servicio registra los mensajes ya procesados.
- **Reintentos** con espera creciente y cola de errores para revisión.
- **Envío masivo con control de tasa** (HU-29.2): concurrencia limitada por consumidor y trabajadores separados del camino transaccional.
- El catálogo inicial de eventos de integración es la tabla 16.2 del DDD (OfertaEmitida, OfertaAceptada, BecaRevocada, PagoConfirmado, PeriodoMarcadoActual, etc.). En la fase de contratos se completa con productor y consumidores.

## Alternativas descartadas

- RabbitMQ sin MassTransit: obliga a programar a mano reintentos, Outbox y serialización.
- MassTransit 9: tiene licencia comercial (ADR-24).

## Fuentes

Lámina «Decisiones Arquitectónicas»; clase del 15-oct; DDD v3.2, sección 16.2 y D-6; Anexo A de HU-29.1 y HU-29.2.
