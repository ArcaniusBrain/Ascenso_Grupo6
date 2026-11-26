## Purpose

Establecer el registro de donantes (organizaciones y personas sin cuenta de acceso) y el ledger inmutable de aportes por programa y periodo en ASCENSO (`RF-13`: `HU-13.1` y `HU-13.2`), alimentando el presupuesto total de cada programa y el historial financiero interno con filtro por periodo.

## ADDED Requirements

### Requirement: Registro de donantes organización o persona sin acceso a la plataforma (HU-13.1)

El personal interno autorizado SHALL poder registrar donantes como organización o persona con sus datos de contacto obligatorios. El registro MUST NOT crear ninguna cuenta ni rol de acceso a la plataforma y MUST bloquear a roles sin permiso.

#### Scenario: Registrar organización y persona donante sin crear cuenta (HU-13.1 Escenarios 1, 2 y 4)
- **GIVEN** que "Empresa Alfa" y "Juan Pérez" no están registrados como donantes
- **WHEN** el coordinador registra a "Empresa Alfa" como organización con correo y teléfono, y a "Juan Pérez" como persona con correo
- **THEN** ambos aparecen en la lista de donantes con su tipo correspondiente y sin aportes iniciales
- **AND** no se crea ninguna cuenta ni rol de acceso a la plataforma para ellos

#### Scenario: Falta información de contacto del donante (HU-13.1 Escenario 3)
- **GIVEN** que el coordinador completó el nombre y el tipo del donante
- **WHEN** intenta guardar sin proporcionar datos de contacto
- **THEN** el sistema no registra al donante y muestra "Indica los datos de contacto del donante."

#### Scenario: Roles sin permiso no pueden registrar donantes (HU-13.1 Escenario 5)
- **GIVEN** que Pedro tiene el rol becario
- **WHEN** intenta registrar un donante
- **THEN** el sistema le niega el acceso sin crear registros

### Requirement: Registro append-only de aportes por donante, programa y periodo (HU-13.2)

El personal interno autorizado SHALL poder registrar aportes mayores que cero de un donante a un programa en un periodo académico registrado sin sobrescribir ni perder aportes previos, reflejando la suma en el presupuesto total del programa.

#### Scenario: Un donante aporta a varios programas y periodos sin sobrescribir historial (HU-13.2 Escenarios 1 y 4)
- **GIVEN** que existen los programas "Becas Ingeniería" y "Becas Arte" y los periodos "2026-1" y "2026-2"
- **WHEN** el coordinador registra aportes de "Empresa Alfa" de 25000 en "2026-1" y 30000 en "2026-2" a "Becas Ingeniería", más 10000 en "2026-2" a "Becas Arte"
- **THEN** el historial de "Empresa Alfa" conserva los tres aportes por separado con su programa, periodo y monto
- **AND** el presupuesto total de cada programa suma los aportes recibidos

#### Scenario: Validaciones al registrar aportes sin programas, sin periodo o con monto no positivo (HU-13.2 Escenarios 2, 3 y 7)
- **GIVEN** que existe el donante "Empresa Alfa"
- **WHEN** se intenta registrar un aporte sin programas registrados en el sistema, sin indicar periodo o con monto 0 o negativo
- **THEN** el sistema rechaza la operación con el mensaje literal correspondiente ("No hay programas registrados.", "Indica el periodo del aporte." o "El monto del aporte debe ser mayor que cero.")

### Requirement: Consulta protegida del historial de aportes con filtro por periodo (HU-13.2)

Solo el personal interno financiero/gestor autorizado SHALL poder consultar el historial de aportes de un donante y filtrarlo por periodo académico. Postulantes y becarios MUST recibir acceso denegado, y la ausencia de aportes MUST informarse explícitamente.

#### Scenario: Filtrar historial por periodo con aportes y por periodo sin aportes (HU-13.2 Escenarios 5, 6 y 9)
- **GIVEN** que "Empresa Alfa" tiene aportes en "2026-1" y "2026-2", "2025-1" existe sin aportes y "Juan Pérez" no tiene aportes
- **WHEN** el coordinador consulta el historial de "Juan Pérez", luego filtra a "Empresa Alfa" por "2026-1" y luego por "2025-1"
- **THEN** para "Juan Pérez" muestra "Este donante no tiene aportes registrados."
- **AND** para "Empresa Alfa" en "2026-1" devuelve solo los aportes de "2026-1"
- **AND** para "Empresa Alfa" en "2025-1" muestra "No hay aportes de este donante en el periodo 2025-1."

#### Scenario: Postulante o becario no puede consultar el historial de aportes (HU-13.2 Escenario 8)
- **GIVEN** que "Empresa Alfa" tiene aportes registrados
- **WHEN** un postulante o becario intenta consultar el historial de aportes de "Empresa Alfa"
- **THEN** el sistema niega el acceso a la información financiera interna
