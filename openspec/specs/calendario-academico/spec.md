# calendario-academico Specification

## Purpose
Establecer la gestión del calendario académico institucional en ASCENSO (`RF-38`: `HU-38.1` y `HU-38.2`), permitiendo a Dirección definir la secuencia de periodos académicos, registrar las fechas de inicio de inscripciones de cada universidad aliada por periodo en UTC-4, consultar periodos anteriores sin pérdida histórica y marcar un único periodo como actual de forma consistente bajo concurrencia.

## Requirements

### Requirement: Registro de secuencia de periodos académicos por Dirección (HU-38.1)

Solo Dirección SHALL poder registrar nuevos periodos académicos con un código no vacío y único en la secuencia. Quien no tenga rol de Dirección MUST recibir bloqueo sin alterar la secuencia.

#### Scenario: Registrar un periodo académico nuevo (HU-38.1 Escenario 1)
- **GIVEN** que la secuencia tiene registrado el periodo "2026-1"
- **WHEN** Dirección registra el periodo "2026-2"
- **THEN** la secuencia incluye "2026-1" y "2026-2" y "2026-2" queda abierto para ligarse a programas, ciclos y renovaciones

#### Scenario: Periodo repetido es rechazado (HU-38.1 Escenario 2)
- **GIVEN** que ya existe el periodo "2026-2" en la secuencia
- **WHEN** Dirección intenta registrar nuevamente "2026-2"
- **THEN** el sistema no lo registra y muestra "El periodo 2026-2 ya existe."

#### Scenario: Nombre de periodo vacío o solo con espacios es rechazado (HU-38.1 Escenario 7)
- **GIVEN** que Dirección está registrando un nuevo periodo académico
- **WHEN** envía el nombre vacío o compuesto solo por espacios
- **THEN** el sistema no lo registra y muestra "Escribe el nombre del periodo académico."

#### Scenario: Solo Dirección puede registrar periodos (HU-38.1 Escenario 8)
- **GIVEN** que Laura tiene el rol coordinador
- **WHEN** intenta registrar el periodo "2027-1"
- **THEN** el sistema bloquea la operación y la secuencia de periodos permanece igual

### Requirement: Consulta del calendario y de periodos anteriores (HU-38.1)

El personal interno autorizado SHALL poder consultar la secuencia de periodos y el detalle de periodos anteriores diferenciándolos claramente del periodo actual. Si no existen periodos registrados, si el periodo no existe o si no tiene registros ligados, el sistema MUST informarlo con claridad.

#### Scenario: Calendario sin periodos registrados (HU-38.1 Escenario 3)
- **GIVEN** que no existen periodos académicos registrados
- **WHEN** Dirección consulta el calendario académico
- **THEN** el sistema devuelve una lista vacía con el mensaje "Aún no hay periodos académicos registrados."

#### Scenario: Consultar un periodo anterior diferenciado del actual (HU-38.1 Escenarios 4 y 6)
- **GIVEN** que "2026-2" es el periodo actual y "2026-1" existe como periodo anterior
- **WHEN** el coordinador consulta el periodo "2026-1"
- **THEN** el sistema devuelve el detalle de "2026-1" identificándolo como periodo anterior y no como actual

#### Scenario: Periodo sin registros ligados informa ausencia de elementos (HU-38.1 Escenario 5)
- **GIVEN** que el periodo "2025-2" existe y no tiene programas, ciclos ni renovaciones ligados
- **WHEN** el coordinador consulta el periodo "2025-2"
- **THEN** el sistema informa "El periodo 2025-2 no tiene programas, ciclos ni renovaciones."

#### Scenario: Consultar un periodo inexistente (HU-38.1 Escenario 9)
- **GIVEN** que el periodo "2024-1" no existe en la secuencia
- **WHEN** el coordinador intenta consultar "2024-1"
- **THEN** el sistema indica que ese periodo no está registrado

### Requirement: Fechas de inicio de inscripciones por universidad y periodo en UTC-4 (HU-38.1)

Dirección SHALL poder registrar, dentro de un periodo académico existente, una única fecha de inicio de inscripciones por universidad aliada interpretada en UTC-4. No MUST aceptarse una fecha vacía ni duplicarse una fecha ya registrada para la misma universidad y periodo.

#### Scenario: Registrar fecha de inicio de inscripciones de una universidad (HU-38.1 Escenarios 10 y 11)
- **GIVEN** que el periodo "2026-2" está registrado y existen las universidades aliadas Universidad Metropolitana y Universidad Central
- **WHEN** Dirección registra el 15/09/2026 para la Universidad Metropolitana y el 01/10/2026 para la Universidad Central en "2026-2"
- **THEN** el periodo "2026-2" muestra ambas fechas asociadas cada una a su universidad correspondiente

#### Scenario: Fecha de inicio de inscripciones vacía es rechazada (HU-38.1 Escenario 12)
- **GIVEN** que Dirección está registrando el inicio de inscripciones de la Universidad Central para "2026-2"
- **WHEN** intenta guardar sin proporcionar una fecha
- **THEN** el sistema rechaza la operación y la Universidad Central permanece sin fecha registrada para "2026-2"

#### Scenario: Una sola fecha de inicio de inscripciones por universidad y periodo (HU-38.1 Escenario 13)
- **GIVEN** que la Universidad Metropolitana ya tiene registrada la fecha 15/09/2026 para "2026-2"
- **WHEN** Dirección intenta registrar otra fecha distinta para la misma universidad en "2026-2"
- **THEN** el sistema conserva la fecha ya registrada y no crea un segundo registro para esa universidad y periodo

### Requirement: Marcado único y consistente del periodo académico actual (HU-38.2)

Solo Dirección SHALL poder marcar cuál es el periodo académico actual. El sistema MUST garantizar que exista como máximo un único periodo actual a la vez incluso ante solicitudes simultáneas, sin duplicar cambios al repetir el periodo ya actual y notificando el cambio de forma asíncrona sin cerrar disponibilidades universitarias.

#### Scenario: Cambiar el periodo actual deja un único periodo vigente (HU-38.2 Escenarios 1 y 3)
- **GIVEN** que "2026-1" es el periodo actual y "2026-2" está registrado y abierto
- **WHEN** Dirección marca "2026-2" como periodo actual
- **THEN** "2026-2" queda como el único periodo actual y "2026-1" deja de serlo

#### Scenario: Solo Dirección puede marcar el periodo actual (HU-38.2 Escenario 5)
- **GIVEN** que Laura tiene el rol coordinador
- **WHEN** intenta marcar "2026-2" como periodo actual
- **THEN** el sistema bloquea la operación y el periodo actual permanece sin cambios

#### Scenario: Dos solicitudes simultáneas de cambio de periodo dejan un único periodo actual (HU-38.2 Escenario 6)
- **GIVEN** que "2026-1" es el periodo actual y existen "2026-2" y "2027-1"
- **WHEN** dos personas de Dirección intentan marcar casi al mismo tiempo "2026-2" y "2027-1" como periodo actual
- **THEN** exactamente una operación se confirma y la otra es rechazada por conflicto de concurrencia
- **AND** en el calendario queda un único periodo actual sin estados intermedios

#### Scenario: Marcar como actual el periodo que ya es actual no produce cambios duplicados (HU-38.2 Escenario 8)
- **GIVEN** que "2026-2" ya es el periodo actual
- **WHEN** Dirección solicita marcar "2026-2" como actual nuevamente
- **THEN** no se producen cambios ni eventos duplicados y "2026-2" continúa siendo el único periodo actual

#### Scenario: Marcar el periodo actual no cierra las disponibilidades universitarias (HU-38.2 Escenario 11)
- **GIVEN** que "2026-1" es el periodo actual y la Universidad Metropolitana aún no alcanza su fecha de inicio de inscripciones en "2026-2"
- **WHEN** Dirección marca "2026-2" como periodo actual
- **THEN** la fecha de inicio de inscripciones de la Universidad Metropolitana permanece intacta y sus disponibilidades no se cierran por el cambio de periodo
