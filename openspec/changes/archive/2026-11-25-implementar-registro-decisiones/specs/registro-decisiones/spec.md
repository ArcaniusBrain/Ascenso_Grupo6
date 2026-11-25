## Purpose

Establecer el registro transversal e inmutable de decisiones relevantes de ASCENSO (`RF-35`: `HU-35.1` y `HU-35.2`), garantizando que toda decisión quede asentada de forma atómica junto con el cambio de estado con quién la tomó, cuándo, qué decisión fue, sobre qué caso y por qué, sin posibilidad de edición ni eliminación posterior, y que pueda consultarse en modo de solo lectura según la función y el alcance autorizado de cada usuario.

## ADDED Requirements

### Requirement: Registro obligatorio e integral de cada decisión (HU-35.1)

Toda decisión relevante que afecte a un postulante, postulación, beca, programa o datos de pago SHALL quedar registrada con el actor que la tomó, la fecha y hora de ocurrencia, el tipo de decisión, el caso sobre el que recae y la justificación. Cada decisión posterior sobre el mismo caso MUST crear un registro nuevo y conservar intactos todos los registros anteriores.

#### Scenario: Suspensión registrada con todos sus datos (HU-35.1 Escenario 1)
- **GIVEN** que la beca de Carlos está Activa
- **WHEN** la coordinadora Laura la suspende el 05-oct-2026 con la justificación "No entregó constancia de matrícula"
- **THEN** el registro guarda a Laura como autora, la fecha 05-oct-2026, el tipo de decisión suspensión, la beca de Carlos y la justificación "No entregó constancia de matrícula"

#### Scenario: Nueva decisión crea un registro nuevo sin alterar el anterior (HU-35.1 Escenario 6)
- **GIVEN** que la beca de Carlos fue suspendida y existe el registro de esa decisión
- **WHEN** la coordinadora registra la reanudación de la beca con su justificación
- **THEN** se crea un registro nuevo para la reanudación
- **AND** el registro previo de la suspensión permanece idéntico

#### Scenario: Entrada automática a Inactiva y salida decidida por coordinador (HU-35.1)
- **GIVEN** que el cierre de la ventana de renovación inactiva automáticamente una beca sin solicitud
- **WHEN** el sistema registra la inactivación automática y posteriormente el coordinador registra la salida de Inactiva con justificación escrita
- **THEN** el registro de inactivación queda firmado por el sistema con justificación automática no vacía
- **AND** el registro de salida de Inactiva queda firmado por el coordinador con su justificación obligatoria

### Requirement: Justificación obligatoria antes de aplicar una decisión (HU-35.1)

Una decisión tomada por una persona sin justificación MUST ser rechazada y SHALL NOT aplicarse ningún cambio de estado sobre el caso.

#### Scenario: Decisión sin justificación es rechazada (HU-35.1 Escenario 2)
- **GIVEN** que Laura va a revocar la beca de Carlos
- **WHEN** confirma la decisión sin escribir la justificación
- **THEN** la beca no cambia de estado y no se crea ningún registro de decisión
- **AND** el sistema responde con el mensaje "Escribe la justificación de esta decisión."

### Requirement: Atomicidad entre el cambio de estado y el registro de la decisión (HU-35.1)

El cambio de estado del caso y la escritura de su registro de decisión MUST ejecutarse como una única unidad atómica: ocurren ambos o no ocurre ninguno. Si falla la persistencia del registro, el cambio de estado SHALL revertirse por completo.

#### Scenario: Falla al registrar revierte el cambio de estado (HU-35.1 Escenario 3)
- **GIVEN** que Laura aprueba la renovación de la beca de Ana con justificación válida
- **WHEN** ocurre un fallo al guardar el registro de la decisión
- **THEN** la beca de Ana conserva su estado anterior sin cambios
- **AND** el sistema informa "No se pudo registrar la decisión. No se aplicó ningún cambio."

### Requirement: Inmutabilidad absoluta del registro de decisiones (HU-35.1)

Una vez guardado, ningún registro de decisión SHALL poder editarse ni eliminarse por ningún rol, incluida Dirección. La restricción MUST aplicarse en la capa de datos y de negocio, no únicamente ocultando acciones en la interfaz.

#### Scenario: Registro disponible únicamente para lectura para Dirección (HU-35.1 Escenario 4)
- **GIVEN** que existe el registro de la suspensión de la beca de Carlos
- **WHEN** un miembro de Dirección lo abre para revisión
- **THEN** obtiene la información de la decisión en modo exclusivamente de lectura, sin operaciones disponibles para editarla ni eliminarla

#### Scenario: Intento de modificar o eliminar un registro es rechazado (HU-35.1 Escenario 5)
- **GIVEN** que existe el registro de la suspensión de la beca de Carlos
- **WHEN** alguien intenta modificar su justificación o eliminar el registro
- **THEN** el sistema rechaza la operación con el mensaje "El registro de decisiones no se puede modificar ni eliminar."
- **AND** el registro original permanece exactamente igual

### Requirement: Autorización por función para registrar decisiones (HU-35.1)

Solo un usuario cuya función le otorga poder de decisión sobre el caso SHALL poder registrar una decisión. Los roles sin poder de decisión sobre el caso y los actores externos MUST recibir denegación de acceso sin alterar el caso ni crear registros.

#### Scenario: Rol sin poder de decisión no puede registrar una decisión (HU-35.1 Escenario 7)
- **GIVEN** que Jorge es el enlace de la universidad aliada de Carlos y no tiene poder de decisión sobre la beca
- **WHEN** intenta registrar una decisión de suspensión sobre la beca de Carlos
- **THEN** el sistema le niega el acceso
- **AND** la beca no cambia de estado y no se registra ninguna decisión

### Requirement: Consistencia bajo decisiones simultáneas sobre el mismo caso (HU-35.1)

Cuando dos operaciones con decisión compiten simultáneamente sobre el mismo caso, el sistema MUST serializar su resolución mediante control de concurrencia de modo que el caso quede en un único estado final coherente, toda operación aplicada cuente con su propio registro de decisión y nunca quede un cambio de estado sin registro ni un registro huérfano.

#### Scenario: Dos decisiones simultáneas sobre la misma beca (HU-35.1 Escenario 8)
- **GIVEN** que la beca de Carlos está Activa
- **WHEN** la coordinadora Laura intenta revocarla con la justificación "Información falsa en el ingreso familiar declarado" al mismo tiempo que finanzas intenta confirmar un desembolso de esa beca
- **THEN** el sistema resuelve las operaciones sin superposición conflictiva dejando la beca en un único estado final válido
- **AND** cada operación efectivamente aplicada queda con su propio registro de decisión
- **AND** ningún cambio de estado queda sin registro ni existe un registro cuya operación fue rechazada

### Requirement: Consulta cronológica y de solo lectura del registro de decisiones (HU-35.2)

Un usuario autorizado SHALL poder consultar las decisiones registradas dentro de su alcance en orden cronológico por fecha de ocurrencia, visualizando tipo, quién la tomó, cuándo y por qué, sin alterar el registro. Cuando el caso aún no tenga decisiones, el sistema MUST informarlo explícitamente.

#### Scenario: Ver las decisiones de una beca en orden de fecha (HU-35.2 Escenario 1)
- **GIVEN** que la beca de Carlos fue suspendida el 05-oct-2026 y reanudada el 20-oct-2026
- **WHEN** Dirección consulta el registro de decisiones de esa beca
- **THEN** recibe las dos decisiones ordenadas cronológicamente (primero la suspensión del 05-oct-2026 y luego la reanudación del 20-oct-2026)
- **AND** cada decisión incluye quién la tomó, cuándo, qué decisión fue y por qué

#### Scenario: Caso sin decisiones registradas (HU-35.2 Escenario 2)
- **GIVEN** que la postulación de Jorge está Recibida y aún no tiene decisiones registradas
- **WHEN** Dirección consulta el registro de decisiones de esa postulación
- **THEN** recibe una colección vacía acompañada del mensaje "Este caso aún no tiene decisiones registradas."

#### Scenario: Consultar el registro no lo modifica (HU-35.2 Escenario 3)
- **GIVEN** que Dirección consulta el registro de decisiones de la beca de Carlos
- **WHEN** finaliza la consulta
- **THEN** todos los registros de decisión de esa beca permanecen exactamente iguales a como estaban antes de la consulta

### Requirement: Restricción de acceso a la consulta según función y alcance (HU-35.2)

El acceso a la consulta del registro de decisiones MUST restringirse en la capa de datos según la función y el alcance autorizado del usuario (Dirección global, coordinador en sus programas, finanzas en ámbito financiero y postulante o becario en su propio caso). Cualquier intento fuera de alcance SHALL ser denegado sin devolver decisiones ni justificaciones ajenas.

#### Scenario: Un becario no puede consultar el registro de otro becario (HU-35.2 Escenario 4)
- **GIVEN** que Ana es becaria de "Becas Ingeniería" y no es titular del caso de Carlos
- **WHEN** intenta consultar el registro de decisiones de la beca de Carlos
- **THEN** el sistema le niega el acceso y no devuelve ninguna decisión

#### Scenario: Un becario consulta el registro completo de su propio caso (HU-35.2)
- **GIVEN** que Carlos tiene decisiones registradas sobre su propia beca
- **WHEN** Carlos consulta el registro de decisiones de su propia beca
- **THEN** recibe el registro completo de las decisiones de su caso en orden cronológico y en modo de solo lectura

#### Scenario: Coordinador consulta solo decisiones de los programas que coordina (HU-35.2)
- **GIVEN** que Laura coordina "Becas Ingeniería" y no coordina "Becas Salud"
- **WHEN** consulta decisiones de un caso de "Becas Ingeniería" y luego intenta consultar un caso de "Becas Salud"
- **THEN** recibe las decisiones de "Becas Ingeniería" en orden cronológico
- **AND** el acceso al caso de "Becas Salud" le es denegado

#### Scenario: Administrador financiero consulta solo decisiones de ámbito financiero (HU-35.2)
- **GIVEN** que una beca tiene una decisión de confirmación de pago y una decisión de suspensión con justificación socioeconómica
- **WHEN** el administrador financiero consulta el registro de decisiones de esa beca
- **THEN** recibe únicamente las decisiones de ámbito financiero
- **AND** no se exponen decisiones académicas, socioeconómicas ni de evaluación del comité
