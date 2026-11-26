# Proposal — `gestionar-presupuesto-cupos` (RF-12 · S2)

## 1. Problema y objetivo

En ASCENSO, cada programa de becas dispone de un presupuesto financiado exclusivamente por los aportes de sus donantes (`S8`) y un cupo total de becarios (`RF-11`). Antes de emitir una oferta nueva (`RF-15`) o registrar una solicitud de renovación (`RF-26`), el sistema debe garantizar de forma **atómica y consistente bajo concurrencia** que el total otorgado más lo comprometido en ofertas y renovaciones pendientes nunca supere el presupuesto disponible ni el cupo del programa (ni el cupo del ciclo cuando esté configurado).

Este cambio implementa **`RF-12`** (`HU-12.1` y `HU-12.2`), las fronteras privadas interservicios de `S2` definidas en `contracts/openapi-interno.yaml` (`S4 → S2` y `S3 → S2`) y el reconciliador seguro `S2 → S4` (`ADR-26`), cerrando el último requerimiento pendiente de `services/Ascenso.S2.CatalogoFondos/`.

## 2. Alcance

- **Agregado raíz `PresupuestoPrograma` (`DDD_v3.3.md` §8, `ADR-25`):**
  - Único guardián transaccional de dinero (`Aportes`, `Otorgado`, `OfertasPendientes`, `RenovacionesPendientes`, `Desembolsado`) y cupos (`CupoBecarios`, `CuposOcupados` y sub-entidad `CupoCiclo`) con `Version` explícita para concurrencia optimista.
  - Entidad interna `ReservaPresupuesto` con clave de negocio única (`(postulacionId, cicloId)` para ofertas nuevas; `(becaId, codigoPeriodo)` para renovaciones) e idempotencia por `operationId`.
  - Fórmula autoritativa única: `Disponible = Aportes - Otorgado - OfertasPendientes - RenovacionesPendientes >= 0` y `CupoDisponible = CupoBecarios - CuposOcupados >= 0`.
  - Sincronización idempotente con creación de programas (`RF-11`) y registro de aportes (`RF-13`).
- **API pública (`contracts/openapi.yaml`, tag `S2`):**
  - `GET /api/v1/programas/{programaId}/presupuesto` (Dirección, Coordinador y Finanzas; bloqueado `403` para Becario, Postulante, Evaluador y Enlace universitario).
- **Frontera privada interservicios (`contracts/openapi-interno.yaml`, nunca expuesta por YARP):**
  - `POST /internal/v1/fondos/reservas` (scope `fondos.reservar`, monto calculado en S2 por toda la duración de la beca `MontoPorPeriodo * DuracionPeriodos`, `R32/A2`).
  - `POST /internal/v1/fondos/reservas/renovaciones` (scope `fondos.reservar`, monto fijado al enviar la solicitud de renovación, `R31/A1`).
  - `POST /internal/v1/fondos/reservas/{reservaId}/liberar` (scope `fondos.liberar`, compensación/rechazo/expiración/revocación idempotente).
  - `GET /internal/v1/fondos/{programaId}/disponibilidad` (scope `fondos.consultar`, consulta autoritativa para `S3`).
  - Validación estricta de tokens OAuth2 `client_credentials` por scope (tokens de usuario final reciben `403`).
- **Conciliación segura `S2 → S4` (`ADR-26`):**
  - Reconciliador de reservas en estado `RESERVADA` con más de 30 minutos de antigüedad que consulta `GET /internal/v1/reservas/{reservaId}/vinculo` (scope `ofertas.consultar-vinculo`), falla cerrada si S4 no responde, vincula si `existeVinculo = true` y solo libera si `existeVinculo = false`.
- **Eventos de integración (`Ascenso.Contracts`):**
  - Registro aditivo `CupoLiberadoV1` en `EventosCatalogoFondosV1.cs`, publicado vía Outbox al liberar reservas de oferta o becas revocadas.

## 3. Fuera de alcance

- Emisión, aceptación o rechazo de ofertas y flujo de renovaciones en `S4` (propiedad de `Ascenso.S4.Becas`).
- Generación del ranking y gestión de lista de espera en `S3` (propiedad de `Ascenso.S3.PostulacionEvaluacion`).
- Job recurrente Hangfire en producción (el servicio de conciliación queda implementado, inyectable y probado de extremo a extremo; el host de jobs se conecta en infraestructura de despliegue).
