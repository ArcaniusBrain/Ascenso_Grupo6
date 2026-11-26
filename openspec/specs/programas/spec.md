# programas Specification

## Purpose
Establecer la creación y configuración de los programas de becas en ASCENSO (`RF-11`: `HU-11.1` y `HU-11.2`), fijando su periodo ligado, cupo total de becarios, duración, monto fijo por periodo, modalidad de pago, etiqueta completo/parcial, donantes asociados, restricciones de universidad y carrera, y condiciones de permanencia, pausa y renovación.

## Requirements

### Requirement: Creación de programa de becas con monto fijo, modalidad y alcance (HU-11.1)

El personal interno autorizado SHALL poder crear un programa de becas ligado a un periodo académico registrado, con nombre, cupo de becarios y duración mayores que cero, monto fijo por periodo mayor que cero, modalidad de pago y etiqueta "completo" o "parcial". Quien no tenga permiso MUST recibir acceso denegado.

#### Scenario: Crear programa con monto fijo por periodo, modalidad y etiqueta (HU-11.1 Escenarios 1, 9 y 11)
- **GIVEN** que el periodo "2026-2" está registrado y abierto
- **WHEN** el coordinador crea "Becas Ingeniería" con cupo de 20 becarios, duración de 8 periodos, ligado a "2026-2", monto de 2000 por periodo, modalidad "pago directo a la universidad" y etiqueta "completo"
- **THEN** "Becas Ingeniería" queda registrada con esos datos, etiqueta "completo" disponible para no doble beneficio y monto intacto de 2000 por periodo

#### Scenario: Sin periodos académicos registrados no permite crear programas (HU-11.1 Escenario 3)
- **GIVEN** que no existen periodos académicos registrados en el calendario
- **WHEN** el coordinador intenta crear un programa de becas
- **THEN** el sistema bloquea la operación y muestra "No hay periodos académicos registrados. Pide a Dirección que los registre."

#### Scenario: Validaciones de campos faltantes o cantidades no positivas (HU-11.1 Escenarios 2, 4, 5, 6, 8 y 10)
- **GIVEN** que el periodo "2026-2" está registrado
- **WHEN** el coordinador intenta guardar un programa sin cupo, con cupo 0, con duración 0, sin monto, con monto 0 o sin etiqueta completo/parcial
- **THEN** el sistema rechaza la operación con el mensaje literal correspondiente ("Indica el cupo de becarios.", "El cupo de becarios debe ser mayor que cero.", "La duración del programa debe ser mayor que cero.", "Indica el monto de la beca por periodo.", "El monto de la beca por periodo debe ser mayor que cero." o "Elige si el programa es completo o parcial.")

#### Scenario: Roles sin permiso no pueden crear ni configurar programas (HU-11.1 Escenario 7)
- **GIVEN** que Marta tiene el rol evaluador
- **WHEN** intenta crear un programa de becas
- **THEN** el sistema le niega el acceso sin registrar ningún programa

### Requirement: Asociación de donantes y visualización de presupuesto del programa (HU-11.2)

Un programa SHALL poder asociar múltiples donantes registrados sin duplicados, mostrando el aporte de cada donante por separado y el presupuesto total como suma de esos aportes. Si no existen donantes registrados o un donante no tiene aportes, el sistema MUST informarlo explícitamente.

#### Scenario: Asociar donante y rechazar duplicado o ausencia de donantes (HU-11.2 Escenarios 3 y 10)
- **GIVEN** que el programa "Becas Ingeniería" está registrado
- **WHEN** se intenta asociar un donante sin haber donantes registrados en la plataforma
- **THEN** el sistema muestra "No hay donantes registrados. Registra uno primero."
- **AND** cuando "Empresa Alfa" ya está asociada e intentan asociarla nuevamente, muestra "Este donante ya está asociado al programa."

#### Scenario: Consultar presupuesto total y aportes por donante (HU-11.2 Escenarios 1, 2 y 13)
- **GIVEN** que "Becas Ingeniería" tiene asociados a "Empresa Alfa" (aportes por 30000) y "Juan Pérez" (sin aportes o con 20000)
- **WHEN** el coordinador consulta el programa
- **THEN** el sistema muestra cada donante en su línea separada con su aporte (o "Sin aportes registrados" si no tiene aportes) y el presupuesto total igual a la suma de los aportes

### Requirement: Restricción opcional de elegibilidad por universidad aliada y carreras (HU-11.2)

Un programa SHALL poder restringirse opcionalmente a universidades aliadas registradas y a carreras registradas como elegibles en cada universidad. Si no se define restricción o una universidad no tiene carreras elegibles, el sistema MUST informarlo claramente.

#### Scenario: Restringir programa a universidad aliada y carrera elegible (HU-11.2 Escenario 4)
- **GIVEN** que "UCAB" está registrada con la carrera elegible "Ingeniería Informática" y no tiene "Medicina"
- **WHEN** el coordinador restringe "Becas Ingeniería" a "UCAB" y "Ingeniería Informática"
- **THEN** el programa guarda esa restricción y rechaza carreras no registradas como elegibles en "UCAB"

#### Scenario: Sin universidades aliadas, sin restricción o universidad sin carreras (HU-11.2 Escenarios 5, 11 y 12)
- **GIVEN** que el coordinador configura la elegibilidad de un programa
- **WHEN** guarda sin elegir universidades ni carreras
- **THEN** el programa muestra "Sin restricción de universidad ni carrera"
- **AND** si intenta restringir sin universidades aliadas registradas muestra "No hay universidades aliadas registradas."
- **AND** si restringe a "UNIMET" que no tiene carreras elegibles, informa "UNIMET no tiene carreras elegibles registradas." y permite restringir solo a nivel de universidad

### Requirement: Condiciones estructuradas de permanencia, pausa máxima y renovación (HU-11.2)

Cada programa SHALL registrar sus condiciones estructuradas de permanencia (promedio mínimo no negativo, horas de servicio no negativas u opción "no aplica", reportes exigidos), duración máxima de pausa en meses y descripción de condiciones de renovación.

#### Scenario: Registrar condiciones de permanencia, pausa y renovación válidas (HU-11.2 Escenarios 6 y 14)
- **GIVEN** que existe el programa "Becas Ingeniería"
- **WHEN** el coordinador registra promedio mínimo 15, 40 horas de servicio, reportes exigidos, duración máxima de pausa de 4 meses y condiciones de renovación
- **THEN** el programa guarda y muestra esas condiciones estructuradas
- **AND** cuando indica que un programa no exige horas de servicio, muestra "Horas de servicio: no aplica"

#### Scenario: Rechazar promedio inválido o negativo y horas de servicio negativas (HU-11.2 Escenarios 7, 8 y 9)
- **GIVEN** que el coordinador configura las condiciones de permanencia de un programa
- **WHEN** ingresa un promedio no numérico, un promedio negativo (-5) o horas de servicio negativas (-10)
- **THEN** el sistema no guarda y muestra "El promedio mínimo debe ser un número.", "El promedio mínimo no puede ser negativo." o "Las horas de servicio no pueden ser negativas." respectivamente
