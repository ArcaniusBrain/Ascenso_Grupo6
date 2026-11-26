# Especificación: Frontend Universidades Aliadas y Calendario Académico (RF-14 y RF-38)

## Purpose

Implementar en `frontend/` las pantallas de administración de universidades aliadas (`/universidades`, RF-14) y calendario académico (`/calendario`, RF-38) conectadas a `S2` exclusivamente mediante el BFF (`ADR-28`).

## Requirements

### Requirement: Gestion de universidades aliadas, carreras, costos e inscripciones (RF-14)

El frontend MUST permitir registrar universidades aliadas, editar convenios y vía de datos académicos (`API`/`MANUAL`), agregar carreras elegibles, registrar costos de matrícula append-only con visualización de historial de versiones por periodo y registrar fechas de inicio de inscripciones (UTC-4).

#### Scenario: Consulta de costo de matricula muestra version vigente e historial

- **WHEN** el usuario selecciona una universidad y un periodo en `/universidades`
- **THEN** la interfaz consulta `/api/v1/universidades/{id}/costos-matricula/{codigoPeriodo}` y `/api/v1/universidades/{id}/inicios-inscripciones/{codigoPeriodo}` vía BFF
- **AND** presenta el monto vigente, su número de versión y el historial append-only sin sobrescribir registros anteriores

### Requirement: Gestion de periodos academicos y marcado de periodo actual (RF-38)

El frontend MUST permitir abrir periodos académicos, cerrarlos cuando corresponda, consultar su detalle y marcar el periodo actual enviando un `operationId` idempotente a `POST /api/v1/periodos/actual`.

#### Scenario: Marcado de periodo actual actualiza estado y encabezado global

- **WHEN** un usuario autorizado marca un periodo abierto como actual desde `/calendario`
- **THEN** el frontend invoca `/api/bff/api/v1/periodos/actual` con `codigoPeriodo` y `operationId` único
- **AND** recarga el calendario mostrando el badge de periodo actual y actualizando la barra de navegación
