# Proposal

## Why

La arquitectura v1.3 deja el DDD v3.2 intacto y solo propone una adenda de ubicación (S2/S4). Pero hay reglas de dominio que ningún documento resuelve o en las que el DDD y la v1.3 se contradicen, y el código se escribirá a partir de ellos:

- La versión de la raíz no sube cuando solo cambia una entidad interna: un pago y una revocación simultáneos sobre la misma beca no chocan (RNF de concurrencia, ADR-16).
- Activa → *Inactiva* tiene dos disparadores distintos: `IniciarNuevoPeriodo()` en el DDD (15.2) y un proceso de Hangfire al cerrar la ventana en la v1.3 (§9).
- El desembolso con fecha dentro de una suspensión se paga al reanudar (R12), pero no está modelado.
- El OTP de la v1.3 (§8.2) se ata a usuario, operación, recurso y desafío; la interfaz `IServicioOtp` del DDD solo recibe usuario y código.
- El registro de una cuenta cruza S1 (Keycloak) y S3 (Postulante) con cédula única (R1) y no hay flujo.
- La bitácora de lectura de datos de pago y el cifrado de la cuenta (v1.3 §8.3) no tienen lugar en el modelo.

## What Changes

- Nueva versión `docs/dominio/DDD_v3.3.md` (la v3.2 se conserva), que incorpora la adenda S2/S4 en §3.1 y el Context Map, y corrige los puntos de "Why".
- Se registran en el DDD las respuestas del cliente de la Ronda 5 que cambien reglas (estados de Beca y Postulación, reservas, matrícula).
- Referencias internas rotas ("14.2", "16.4b"), totales de 6.3 y control de versiones.
- Diagramas y enlaces regenerados con `tools/mermaid/generar_diagramas.py`.

El detalle de cada punto (ubicación exacta en el DDD y motivo) está en `docs/cambios/cambios_DDD_v3.3_y_ADR_v0.2.md`, IDs DDD-07 a DDD-15.

## Capabilities

### New Capabilities

Ninguna. Corrige el documento de diseño, no el comportamiento especificado (`skip_specs: true`).

### Modified Capabilities

Ninguna.

## Impact

- Documentos: `docs/dominio/`. En B1, B2 y B6 de la Ronda 5 el equipo mantiene la v1.3, así que la arquitectura no cambia.
- Shared Kernel en diseño: `RaizDeAgregado.Version` e `IServicioOtp`. Requiere revisión de todo el equipo.
- Usa las respuestas del cliente de la Ronda 5 (R31–R44), registradas en `docs/requisitos/respuestas_cliente.md`.
