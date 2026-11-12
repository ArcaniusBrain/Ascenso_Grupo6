# Proposal

## Why

La revisión del modelo de dominio (DDD v3.2) y de la propuesta de arquitectura (ADR v0.1) encontró errores que permitirían romper reglas del cliente: dos operaciones concurrentes sobre la misma beca sin conflicto detectado (ADR-16), una reserva de presupuesto liberada aunque su oferta exista (ADR-03, regla transversal 8), el desembolso aplazado por suspensión sin modelar (R12) y un OTP reutilizable entre operaciones (R4–R6, ADR-10). Además, los dos documentos se contradicen en varios puntos (disparador de Activa → Inactiva, motor de la base de lectura, proveedor de identidad). Hay que corregirlos antes de escribir código, porque el código se escribirá a partir de ellos.

La lista completa, con ubicación exacta y motivo de cada cambio, está en `docs/cambios/cambios_DDD_v3.3_y_ADR_v0.2.md`.

## What Changes

- Nueva versión del modelo de dominio, `docs/dominio/DDD_v3.3.md`, con los cambios DDD-01 a DDD-15:
  - incorpora los cambios que ya pedía la sección 5 del ADR (base de lectura PostgreSQL, Keycloak, Stateless, mapeo de contextos a servicios, API de entrada de universidades);
  - versión explícita en la raíz que sube con cualquier cambio del agregado (**bloqueante**);
  - un solo disparador para Activa → Inactiva;
  - estado `Aplazado` del desembolso (R12);
  - OTP vinculado a operación y recurso;
  - flujo de registro entre S1 y S3;
  - bitácora de lectura y cifrado de datos de pago;
  - Context Map sin Trazabilidad como intermediaria;
  - referencias internas corregidas y totales actualizados.
- Nueva versión de la arquitectura (ADR v0.2) con los cambios ADR-B01 a ADR-B16, editando los archivos de `docs/arquitectura/adr/`:
  - **ADR-16**: versión explícita en lugar de `xmin` (**bloqueante**);
  - **ADR-03**: liberación segura de reservas sin vincular, con consulta S2 → S4 (**bloqueante**);
  - ADR-14, ADR-08, ADR-02, ADR-09, ADR-10, ADR-06, ADR-17, ADR-12 ajustados;
  - nuevos ADR-25 (observabilidad y operación), ADR-26 (frontend) y ADR-27 (despliegue de la demostración);
  - MediatR como dependencia opcional (ADR-04, ADR-24).
- Diagramas y página de enlaces regenerados desde el DDD v3.3 con `tools/mermaid/generar_diagramas.py`.
- Dos preguntas nuevas para el cliente agregadas al Anexo B (disparador de Inactiva; R12 en la pausa).

## Capabilities

### New Capabilities

Ninguna. Es un cambio de documentación de diseño: no cambia el comportamiento especificado del sistema (`skip_specs: true`).

### Modified Capabilities

Ninguna.

## Impact

- Documentos: `docs/dominio/`, `docs/arquitectura/`, `docs/requisitos/historias-de-usuario/anexo-b-preguntas-abiertas.md`.
- Servicios: ninguno todavía (no hay código). Los cambios afectan cómo se construirán S1, S2, S3 y S4.
- Contratos y Shared Kernel: cambian en diseño la firma de `IServicioOtp`, el tipo base `RaizDeAgregado` y el estado del desembolso; por eso requiere revisión de todo el equipo.
- Pendiente de terceros: las preguntas al profesor de la sección 6 del ADR y las dos preguntas nuevas al cliente.
