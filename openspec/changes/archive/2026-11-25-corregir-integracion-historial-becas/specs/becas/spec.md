# Spec Delta

## MODIFIED Requirements

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
