## Purpose

Establecer la gestión de universidades aliadas, convenios, carreras elegibles, costos de matrícula históricos por periodo y fechas de inicio de inscripciones por universidad en ASCENSO (`RF-14`: `HU-14.1`, `HU-14.2` y `HU-14.3`), publicando el costo de matrícula vigente como referencia para el tope de becas y exponiendo el catálogo de universidades y carreras a postulantes y programas.

## ADDED Requirements

### Requirement: Registro de universidad aliada, convenio y contacto designado (HU-14.1)

El personal interno autorizado de la Fundación SHALL poder registrar universidades aliadas con su nombre, términos del convenio, vía de intercambio académico (`API` o `MANUAL`) y contacto designado. El enlace de universidad MUST ser bloqueado al intentar registrar o modificar convenios.

#### Scenario: Registrar una universidad aliada con convenio y contacto designado (HU-14.1 Escenarios 1, 3, 9 y 10)
- **GIVEN** que la universidad "UCAB" no está registrada en el catálogo
- **WHEN** personal autorizado registra a "UCAB" con sus términos del convenio, vía de intercambio académico "API" y a "Ana Ruiz" como contacto designado
- **THEN** "UCAB" aparece en la lista de universidades aliadas con sus términos, su contacto designado y vía "API"
- **AND** queda disponible para los postulantes y para restringir programas

#### Scenario: Convenio con registro manual de información académica (HU-14.1 Escenario 10)
- **GIVEN** que la universidad "UNIMET" no está registrada
- **WHEN** personal autorizado registra a "UNIMET" con sus términos del convenio, su contacto designado y vía de intercambio académico "MANUAL"
- **THEN** "UNIMET" queda registrada indicando origen "MANUAL" para el seguimiento académico

#### Scenario: Falta el contacto designado al registrar universidad (HU-14.1 Escenario 2)
- **GIVEN** que el usuario llenó el nombre y los términos del convenio de una universidad
- **WHEN** intenta guardar sin proporcionar el contacto designado
- **THEN** el sistema no registra la universidad y muestra "Indica el contacto designado de la universidad."

#### Scenario: Faltan los términos del convenio al registrar universidad (HU-14.1 Escenario 8)
- **GIVEN** que el usuario llenó el nombre de "UCAB" y su contacto designado
- **WHEN** intenta guardar sin indicar los términos del convenio
- **THEN** el sistema no registra la universidad y muestra "Indica los términos del convenio."

#### Scenario: El enlace de universidad no puede modificar el convenio de su universidad (HU-14.1 Escenario 7)
- **GIVEN** que "Ana Ruiz" tiene el rol de enlace de universidad para "UCAB"
- **WHEN** intenta registrar o modificar los términos del convenio de "UCAB"
- **THEN** el sistema le niega el acceso sin alterar el convenio registrado

### Requirement: Gestión de carreras elegibles aisladas por universidad (HU-14.1)

Cada universidad aliada SHALL mantener su propia lista independiente de carreras elegibles. El sistema MUST rechazar nombres vacíos e informar explícitamente cuando una universidad todavía no tenga carreras registradas.

#### Scenario: Cada universidad conserva su propia lista independiente de carreras (HU-14.1 Escenario 4)
- **GIVEN** que "UCAB" y "UNIMET" no tienen carreras elegibles registradas
- **WHEN** personal autorizado agrega "Ingeniería Informática" a "UCAB"
- **THEN** "Ingeniería Informática" aparece en las carreras elegibles de "UCAB" y no aparece en las de "UNIMET"

#### Scenario: Nombre de carrera vacío es rechazado (HU-14.1 Escenario 5)
- **GIVEN** que personal autorizado agrega una carrera elegible a "UCAB"
- **WHEN** intenta guardar sin nombre de carrera o solo con espacios
- **THEN** el sistema rechaza la operación y muestra "Indica el nombre de la carrera."

#### Scenario: Universidad sin carreras elegibles informa ausencia (HU-14.1 Escenario 6)
- **GIVEN** que "UNIMET" no tiene carreras elegibles registradas
- **WHEN** se consulta el detalle de "UNIMET"
- **THEN** el sistema informa "Esta universidad no tiene carreras elegibles registradas."

### Requirement: Registro histórico append-only de costos de matrícula por periodo (HU-14.2)

El sistema SHALL registrar costos de matrícula mayores que cero por universidad y periodo académico registrado sin sobrescribir ni borrar valores previos, manteniendo como vigente el registro más reciente y notificando el cambio de costo vigente.

#### Scenario: Registrar costo de matrícula para un nuevo periodo (HU-14.2 Escenario 1)
- **GIVEN** que "UCAB" tiene un costo de 900 para el periodo "2026-1" y existe el periodo "2026-2"
- **WHEN** personal autorizado registra 1000 para "UCAB" en "2026-2"
- **THEN** "UCAB" muestra 900 como costo vigente de "2026-1" y 1000 como costo vigente de "2026-2"

#### Scenario: Corregir el costo de un periodo conserva el valor anterior consultable (HU-14.2 Escenario 2)
- **GIVEN** que "UCAB" tiene un costo de 1000 registrado para el periodo "2026-2"
- **WHEN** personal autorizado registra una corrección de 1050 para "UCAB" en "2026-2"
- **THEN** "UCAB" muestra 1050 como costo vigente de "2026-2"
- **AND** el historial de "2026-2" conserva consultable el valor anterior de 1000

#### Scenario: Registrar costo sin periodos académicos registrados es rechazado (HU-14.2 Escenario 3)
- **GIVEN** que no existen periodos académicos registrados en el calendario
- **WHEN** personal autorizado intenta registrar un costo de matrícula para "UCAB"
- **THEN** el sistema rechaza la operación y muestra "No hay periodos académicos registrados."

#### Scenario: Costo de matrícula en cero o negativo es rechazado (HU-14.2 Escenario 4)
- **GIVEN** que existe el periodo "2026-2" y la universidad "UCAB"
- **WHEN** personal autorizado intenta registrar un costo de 0 o negativo para "2026-2"
- **THEN** el sistema no guarda el registro y muestra "El costo de matrícula debe ser mayor que cero."

#### Scenario: Consultar costo de un periodo registrado sin costo cargado (HU-14.2 Escenario 5)
- **GIVEN** que el periodo "2026-2" está registrado pero "UCAB" no tiene costo cargado para ese periodo
- **WHEN** se consulta el costo de matrícula de "UCAB" en "2026-2"
- **THEN** el sistema devuelve una respuesta válida con el mensaje "Sin costo registrado para este periodo"

### Requirement: Fechas de inicio de inscripciones asociadas a universidad aliada y periodo en UTC-4 (HU-14.3)

El sistema SHALL permitir registrar y consultar la fecha de inicio de inscripciones de cada universidad aliada para un periodo académico registrado evaluándola en UTC-4, aislando cada universidad respecto de las demás e informando cuando no haya periodos o no exista fecha cargada.

#### Scenario: Registrar y consultar fecha de inicio de inscripciones de una universidad (HU-14.3 Escenarios 1 y 2)
- **GIVEN** que "UCAB" y "UNIMET" son universidades aliadas registradas y el periodo "2026-2" existe
- **WHEN** se registra 15/09/2026 para "UCAB" y 01/10/2026 para "UNIMET" en "2026-2"
- **THEN** al llegar las 00:00 UTC-4 del 15/09/2026 las inscripciones de "UCAB" figuran iniciadas mientras las de "UNIMET" siguen pendientes

#### Scenario: Registrar inicio de inscripciones sin periodos registrados es rechazado (HU-14.3 Escenario 4)
- **GIVEN** que no existen periodos académicos registrados en el calendario
- **WHEN** se intenta registrar una fecha de inicio de inscripciones para "UCAB"
- **THEN** el sistema rechaza la operación y muestra "No hay periodos académicos registrados."

#### Scenario: Falta la fecha de inicio de inscripciones es rechazada (HU-14.3 Escenario 5)
- **GIVEN** que "UCAB" y el periodo "2026-2" están registrados
- **WHEN** se intenta guardar sin proporcionar una fecha de inicio de inscripciones
- **THEN** el sistema no guarda y muestra "Indica la fecha de inicio de inscripciones."

#### Scenario: Consultar fecha de inicio de inscripciones de un periodo sin fecha cargada (HU-14.3 Escenario 6)
- **GIVEN** que el periodo "2026-2" está registrado pero "UCAB" no tiene fecha cargada para ese periodo
- **WHEN** se consulta el inicio de inscripciones de "UCAB" en "2026-2"
- **THEN** el sistema informa "Sin fecha de inicio de inscripciones registrada para este periodo"
