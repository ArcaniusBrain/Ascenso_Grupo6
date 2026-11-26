## Purpose

Establecer el agregado `PresupuestoPrograma` como único guardián transaccional de presupuesto y cupos en `S2` (`RF-12`: `HU-12.1` y `HU-12.2`), exponiendo la consulta pública de presupuesto y cupo disponible, las fronteras privadas interservicios de reserva/liberación/disponibilidad (`openapi-interno.yaml`, `R31/A1`, `R32/A2`) y la conciliación segura `S2 → S4` (`ADR-26`).

## ADDED Requirements

### Requirement: Consulta protegida de presupuesto y cupo disponibles del programa (HU-12.1 y HU-12.2)

El personal interno autorizado (`DIRECCION`, `COORDINADOR` y `FINANZAS`) SHALL poder consultar en cualquier momento el presupuesto total, otorgado, comprometido en ofertas y renovaciones pendientes, disponible y cupos de un programa. El sistema MUST mantener ocupados el cupo y presupuesto de becas `Inactiva` y bloquear (`403`) a roles sin permiso.

#### Scenario: Consulta con becas otorgadas, ofertas pendientes, renovaciones pendientes y beca Inactiva (HU-12.1 Escenarios 1, 10, 11, 12 y HU-12.2 Escenarios 1, 4)
- **GIVEN** que un programa tiene aportes totales de 50.000, cupo total de 20 becarios, becas otorgadas por 30.000 (incluyendo 1 beca `Inactiva` que conserva su cupo y presupuesto), una oferta pendiente de 2.000 y una renovación pendiente de 5.000
- **WHEN** el administrador financiero o el coordinador consulta el presupuesto y cupo del programa
- **THEN** el sistema muestra presupuesto total 50.000, otorgado 30.000, comprometido en ofertas pendientes 2.000, comprometido en renovaciones pendientes 5.000 y disponible 13.000
- **AND** mantiene contados como ocupados el cupo de las becas otorgadas (incluida la `Inactiva`) y el de las ofertas pendientes, sin sustituir el cupo del programa por el cupo de una convocatoria

#### Scenario: Programa sin aportes registrados (HU-12.1 Escenario 3)
- **GIVEN** que "Becas Arte" no tiene aportes registrados
- **WHEN** el coordinador o el administrador financiero consulta el presupuesto del programa
- **THEN** el sistema muestra presupuesto total 0, disponible 0 y el mensaje literal "Este programa no tiene aportes registrados."

#### Scenario: Roles sin permiso no consultan presupuesto ni agregado de renovaciones (HU-12.1 Escenario 7 y HU-12.2 Escenario 5)
- **GIVEN** que el programa tiene presupuesto y renovaciones pendientes registrados
- **WHEN** un becario o un postulante intenta consultar el presupuesto o el cupo del programa
- **THEN** el sistema le niega el acceso (`403 Forbidden`) a la información financiera interna

### Requirement: Verificación y reserva atómica de presupuesto y cupo para ofertas nuevas y renovaciones (HU-12.1, HU-12.2, R31/A1, R32/A2)

La frontera privada de `S2` SHALL verificar y comprometer presupuesto y cupo en una única operación transaccional versionada (`ADR-25`) protegida por scopes OAuth2 `client_credentials` (`fondos.reservar`, `fondos.consultar`), garantizando que ninguna combinación concurrente de reservas deje disponible o cupo en negativo. Un monto exactamente igual al disponible MUST aceptarse dejando disponible en 0.

#### Scenario: Reserva autoritativa de oferta nueva con monto exacto, monto insuficiente o último cupo en disputa (HU-12.1 Escenarios 4, 5, 6, 8 y 9)
- **GIVEN** que un programa tiene presupuesto y cupo configurados
- **WHEN** `S4` solicita reservar una oferta nueva por `/internal/v1/fondos/reservas` con scope `fondos.reservar`
- **THEN** `S2` calcula el monto por toda la duración de la beca (`MontoPorPeriodo * DuracionPeriodos`), descuenta presupuesto y 1 cupo atómicamente si alcanza (incluyendo igualdad exacta dejando disponible en 0)
- **AND** si no hay cupo responde con "El programa no tiene cupo disponible."
- **AND** si no alcanza el presupuesto responde con "Presupuesto insuficiente: disponible {disponible}, la beca requiere {requerido}."
- **AND** ante dos reservas simultáneas por el último cupo, exactamente una confirma y la otra es rechazada sin superar el cupo ni dejar saldo negativo

#### Scenario: Reserva de renovación pendiente reduce el disponible y bloquea nuevos otorgamientos sobre ese monto (HU-12.2 Escenarios 1 y 2)
- **GIVEN** que "Becas Ingeniería" tiene 6.000 sin otorgar y ninguna oferta pendiente
- **WHEN** `S4` registra una reserva de renovación pendiente de 5.000 por `/internal/v1/fondos/reservas/renovaciones` y luego intenta reservar una oferta nueva de 1.200
- **THEN** la consulta muestra 5.000 en comprometido en renovaciones pendientes y 1.000 disponibles
- **AND** el intento de oferta nueva es rechazado con "Presupuesto insuficiente: disponible 1.000, la beca requiere 1.200."

#### Scenario: Idempotencia, clave única de negocio y seguridad de frontera privada
- **GIVEN** que `S2` expone las rutas `/internal/v1/fondos/...`
- **WHEN** un token de usuario final o un cliente sin el scope requerido invoca una ruta interna, o `S4` reintenta una reserva con el mismo `operationId` o el mismo par `(postulacionId, cicloId)`
- **THEN** el token de usuario final o sin scope recibe `403 Forbidden` (y sin token `401 Unauthorized`)
- **AND** el reintento con el mismo `operationId` devuelve la misma reserva sin descontar dos veces, y no se permiten dos reservas activas para el mismo par `(postulacionId, cicloId)` ni `(becaId, codigoPeriodo)`

### Requirement: Liberación idempotente de reservas y conciliación segura S2 → S4 (HU-12.1, HU-12.2, ADR-26)

El sistema SHALL liberar presupuesto y cupo de forma idempotente ante rechazo, expiración, compensación o revocación de beca (emitiendo `CupoLiberado.v1` vía Outbox cuando se libera cupo), y MUST verificar obligatoriamente el estado real en `S4` antes de liberar una reserva sin vínculo que superó el plazo de gracia de 30 minutos (`ADR-26`).

#### Scenario: Liberación por rechazo de oferta, rechazo de renovación y compensación repetida (HU-12.1 Escenario 2 y HU-12.2 Escenario 3)
- **GIVEN** que existe una reserva de oferta pendiente de 2.000 ocupando el último cupo y una reserva de renovación pendiente de 5.000
- **WHEN** `S4` solicita liberar la oferta por rechazo (`RECHAZO`) y la renovación por rechazo (`RECHAZO`), incluso repitiendo la llamada de liberación
- **THEN** el cupo disponible y los montos vuelven al disponible una sola vez sin doble liberación
- **AND** se persiste el evento `CupoLiberado.v1` en `outbox_messages` al liberar el cupo

#### Scenario: Conciliación segura S2 → S4 falla cerrada ante indisponibilidad y vincula si el recurso existe (ADR-26)
- **GIVEN** que existen reservas en estado `RESERVADA` con más de 30 minutos de antigüedad y otras dentro del plazo de gracia
- **WHEN** el reconciliador ejecuta su ciclo consultando `GET /internal/v1/reservas/{reservaId}/vinculo` en `S4`
- **THEN** ignora las reservas con menos de 30 minutos
- **AND** si `S4` responde `existeVinculo = true`, vincula la reserva sin liberarla
- **AND** si `S4` responde `existeVinculo = false`, libera la reserva de forma segura
- **AND** si `S4` no responde o devuelve error (`5xx`/timeout), **no libera** la reserva y la deja para el próximo ciclo
