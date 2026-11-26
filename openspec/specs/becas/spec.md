# becas Specification

## Purpose

Gestionar la consulta del historial completo de becas en S4 (`RF-18`, `HU-18.1`), permitiendo consultar becas por cédula de persona, programa o periodo académico con clasificación consistente de beca activa (`S6`), confidencialidad socioeconómica y control de acceso por rol.

## Requirements

### Requirement: Consulta del historial completo de becas de una persona por cédula (HU-18.1)

El sistema SHALL permitir consultar el historial completo de becas de una persona mediante su número de cédula, en cualquier estado (incluidas *Concluida*, *Revocada* e *Inactiva*), mostrando programa, periodo, monto fijo del periodo, marca "completo" o "parcial" y estado, señalando como activas únicamente las becas en estado *Otorgada*, *Activa*, *Suspendida* o *Renovada* e informando explícitamente cuando la persona no exista o no tenga becas (HU-18.1).

#### Scenario: Becas en varios programas y periodos, con señal de beca activa
- **GIVEN** que María tuvo una beca *Concluida* en "Becas Arte" 2025-2 y tiene una beca *Suspendida* en "Becas Ingeniería" 2026-2
- **WHEN** el coordinador consulta su historial por cédula
- **THEN** ve ambas becas con programa, periodo, monto del periodo, marca "completo" o "parcial" del programa y estado
- **AND** la beca *Suspendida* aparece señalada como activa y la *Concluida* no

#### Scenario: Persona sin becas
- **GIVEN** que María Gómez es postulante y nunca tuvo una beca
- **WHEN** el coordinador consulta su historial por cédula
- **THEN** el sistema muestra "María Gómez no tiene becas registradas."

#### Scenario: Persona no encontrada
- **GIVEN** que no existe ninguna persona con la identificación V-12345678
- **WHEN** el coordinador la busca por su número de cédula
- **THEN** el sistema muestra "No se encontró a una persona con la identificación V-12345678."

#### Scenario: El historial incluye becas revocadas
- **GIVEN** que Pedro tuvo una beca *Revocada* en "Becas Arte" 2025-2
- **WHEN** el coordinador consulta su historial por cédula
- **THEN** ve esa beca con su programa, periodo, monto del periodo, marca del programa y estado *Revocada*
- **AND** la beca no aparece señalada como activa

#### Scenario: El historial incluye becas inactivas, sin señal de activa
- **GIVEN** que la beca de Luis en "Becas Salud" 2025-2 quedó *Inactiva* porque no se renovó
- **WHEN** el coordinador consulta su historial por cédula
- **THEN** ve esa beca con su programa, periodo, monto del periodo, marca del programa y estado *Inactiva*
- **AND** no aparece señalada como activa porque *Inactiva* no cuenta como beca activa

### Requirement: Consulta del historial de becas por programa o por periodo (HU-18.1)

El sistema SHALL permitir a usuarios autorizados consultar de forma agregada todas las becas registradas de un programa o de un periodo académico, incluidos periodos anteriores, mostrando persona, programa, periodo y estado, e informando explícitamente cuando no existan resultados (HU-18.1).

#### Scenario: Consulta por programa
- **GIVEN** que "Becas Ingeniería" tiene becas de María y de Pedro
- **WHEN** el coordinador consulta el historial por "Becas Ingeniería"
- **THEN** ve las becas de María y de Pedro con su periodo y estado

#### Scenario: Consulta por un periodo anterior
- **GIVEN** que el periodo actual es 2026-2 y en 2026-1 hubo becas en "Becas Ingeniería" y "Becas Arte"
- **WHEN** el coordinador consulta el historial por 2026-1
- **THEN** ve las becas de ambos programas en ese periodo y ninguna de otro periodo

#### Scenario: Periodo sin becas
- **GIVEN** que no hubo becas en 2025-1
- **WHEN** el coordinador consulta el historial por 2025-1
- **THEN** el sistema muestra "No hay becas para el periodo 2025-1."

#### Scenario: Programa sin becas registradas
- **GIVEN** que "Becas Música" no tiene ninguna beca registrada
- **WHEN** el coordinador consulta el historial por "Becas Música"
- **THEN** el sistema muestra "No hay becas en el programa Becas Música."

### Requirement: Confidencialidad de información socioeconómica en el historial de becas (HU-18.1)

El sistema SHALL omitir cualquier dato socioeconómico del postulante o becario, incluyendo el ingreso familiar declarado y la composición familiar, en todas las vistas y respuestas del historial de becas (HU-18.1).

#### Scenario: El historial no expone datos socioeconómicos
- **GIVEN** que el coordinador consulta el historial de becas de María
- **WHEN** el sistema muestra sus becas con programa, periodo, monto del periodo, marca del programa y estado
- **THEN** no se muestra su ingreso familiar declarado ni su composición familiar en esa vista

### Requirement: Control de acceso por función al historial de becas (HU-18.1)

El sistema MUST restringir la consulta del historial de becas según el rol del usuario: los roles Coordinador y Dirección pueden consultar por persona, programa o periodo; un becario solo puede consultar su propio historial por persona; y los roles de becario y postulante tienen denegado el acceso a historiales ajenos y a la búsqueda agregada (HU-18.1).

#### Scenario: Un becario no consulta el historial de otra persona
- **GIVEN** que Luis es becario de "Becas Salud"
- **WHEN** intenta consultar el historial de becas de María
- **THEN** el sistema le niega el acceso y solo le permite ver su propio historial

#### Scenario: Un becario no usa la búsqueda por programa
- **GIVEN** que Luis tiene el rol de becario
- **WHEN** intenta consultar el historial agregado de "Becas Ingeniería" por programa
- **THEN** el sistema le niega el acceso a esa vista

#### Scenario: Un postulante no usa la búsqueda por programa
- **GIVEN** que María tiene el rol de postulante
- **WHEN** intenta consultar el historial agregado de "Becas Ingeniería" por programa
- **THEN** el sistema le niega el acceso a esa vista

#### Scenario: Dirección consulta el historial por persona y por programa
- **GIVEN** que Elena tiene el rol de Dirección y existen becas registradas en "Becas Ingeniería"
- **WHEN** consulta el historial de becas de María por cédula o el historial agregado de "Becas Ingeniería"
- **THEN** el sistema le concede acceso y devuelve las becas correspondientes sin datos socioeconómicos
