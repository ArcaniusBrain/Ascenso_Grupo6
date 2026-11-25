# identidad-acceso Specification

## Purpose
Establecer la gestión de usuarios internos y tipos de usuario configurables con control de acceso aplicado en el servidor (`HU-37.1`), junto con el inicio de sesión mediante verificación en dos pasos por correo o SMS (`HU-37.2`), garantizando que cada rol acceda únicamente a lo que corresponde a su función y protegiendo la información financiera y de decisión de ASCENSO (`RF-37`).

## Requirements

### Requirement: Gestión de tipos de usuario configurables por Dirección (HU-37.1)

Solo Dirección SHALL poder crear tipos de usuario nuevos con un nombre único no vacío y un conjunto de permisos configurables sin requerir cambios de código. Si un tipo incluye permisos sobre información financiera o de decisión, el sistema MUST marcarlo automáticamente como sujeto a verificación en dos pasos.

#### Scenario: Crear un tipo de usuario nuevo (HU-37.1 Escenario 3)
- **GIVEN** que la Fundación necesita un rol "Asistente de comunicaciones"
- **WHEN** Dirección lo crea con permiso para responder consultas
- **THEN** el tipo queda creado con sus permisos configurados
- **AND** aparece disponible como opción para asignarlo al crear usuarios

#### Scenario: Nombre de tipo de usuario duplicado es rechazado (HU-37.1 Escenario 7)
- **GIVEN** que ya existe el tipo de usuario "Auditor interno"
- **WHEN** Dirección intenta crear otro tipo con el mismo nombre "Auditor interno"
- **THEN** el sistema no crea el tipo
- **AND** muestra "El tipo de usuario Auditor interno ya existe."

#### Scenario: Tipo de usuario sin nombre es rechazado (HU-37.1 Escenario 9)
- **GIVEN** que Dirección abre la gestión de tipos de usuario
- **WHEN** intenta crear un tipo dejando el nombre vacío
- **THEN** el sistema no crea el tipo
- **AND** muestra "Escribe el nombre del tipo de usuario."

#### Scenario: Tipo nuevo con acceso financiero queda marcado para verificación en dos pasos (HU-37.1 Escenario 11)
- **GIVEN** que Dirección está creando el tipo "Auditor interno" con permiso para ver información financiera
- **WHEN** guarda el tipo de usuario
- **THEN** el tipo queda creado
- **AND** su configuración indica que exige verificación en dos pasos

### Requirement: Alta de usuarios internos y asignación exclusiva de roles por Dirección (HU-37.1)

Solo Dirección SHALL poder registrar usuarios internos o evaluadores externos con su número de cédula único, datos de contacto y al menos un rol asignado. Ningún otro rol MUST poder crear usuarios ni autoasignarse un rol distinto del suyo.

#### Scenario: Crear un coordinador y un evaluador externo (HU-37.1 Escenario 1)
- **GIVEN** que Laura trabaja en la Fundación y el profesor Ramírez es evaluador externo que formará parte de un comité
- **WHEN** Dirección registra a Laura con el rol coordinador y al profesor Ramírez con el rol evaluador
- **THEN** el usuario de Laura queda creado con el rol coordinador y el del profesor Ramírez queda creado con el rol evaluador habilitado para integrarse a un comité

#### Scenario: Solo Dirección puede crear usuarios internos (HU-37.1 Escenario 2)
- **GIVEN** que Laura tiene el rol coordinador
- **WHEN** intenta crear un usuario interno
- **THEN** el sistema le niega la operación y no crea ningún usuario

#### Scenario: Auto-escalamiento de permisos bloqueado (HU-37.1 Escenario 8)
- **GIVEN** que Laura tiene el rol coordinador
- **WHEN** intenta asignarse a sí misma el rol dirección
- **THEN** el sistema bloquea la operación
- **AND** el rol de Laura sigue siendo coordinador

#### Scenario: Usuario sin rol asignado es rechazado (HU-37.1 Escenario 10)
- **GIVEN** que Dirección registra los datos de identificación y contacto de Carlos como nuevo usuario interno
- **WHEN** intenta guardarlo sin asignarle un rol
- **THEN** el sistema no crea el usuario
- **AND** muestra "Asigna un rol al usuario."

### Requirement: Control de acceso por función, estado y confidencialidad en el servidor (HU-37.1)

El sistema MUST aplicar en el servidor las restricciones de función, estado del proceso y confidencialidad de datos socioeconómicos, impidiendo acciones fuera de rol y omitiendo datos restringidos directamente en las respuestas entregadas al cliente.

#### Scenario: Evaluador no ve puntuaciones de otros evaluadores antes del cierre (HU-37.1 Escenario 4)
- **GIVEN** que Luis y Ana evalúan la postulación de María y el periodo de revisión aún no ha cerrado
- **WHEN** Luis consulta la evaluación de esa postulación
- **THEN** recibe únicamente sus propias puntuaciones y no recibe las puntuaciones de Ana

#### Scenario: Finanzas no puede modificar rúbricas de evaluación (HU-37.1 Escenario 5)
- **GIVEN** que Pablo es administrador financiero
- **WHEN** intenta modificar la rúbrica de "Becas Ingeniería"
- **THEN** el sistema bloquea la acción con el mensaje "Tu rol no permite modificar rúbricas de evaluación."
- **AND** la rúbrica permanece sin cambios

#### Scenario: Finanzas no recibe datos socioeconómicos en la respuesta del servidor (HU-37.1 Escenario 6)
- **GIVEN** que Pablo es administrador financiero
- **WHEN** consulta la postulación de María
- **THEN** no ve el ingreso familiar ni la composición familiar de María
- **AND** esos datos tampoco se incluyen en la respuesta que entrega el servidor

### Requirement: Verificación en dos pasos obligatoria para roles privilegiados y externos (HU-37.2)

Toda persona con acceso a información financiera o de decisión (coordinador, finanzas, dirección, comité evaluador, tipos nuevos con ese acceso) y todo enlace de universidad aliada MUST completar un segundo factor además de su contraseña para abrir sesión. La sesión SHALL NOT abrirse nunca con la contraseña sola ni al fallar el envío del código.

#### Scenario: Acceso exitoso con los dos pasos (HU-37.2 Escenario 1)
- **GIVEN** que Laura es coordinadora con su cuenta activa
- **WHEN** ingresa su contraseña correcta y luego un código de segundo factor válido
- **THEN** el sistema abre su sesión con los permisos de su rol

#### Scenario: Segundo factor incorrecto rechaza la apertura de sesión (HU-37.2 Escenario 2)
- **GIVEN** que Pablo es administrador financiero e ingresó su contraseña correcta
- **WHEN** ingresa un código de segundo factor incorrecto
- **THEN** el sistema no abre la sesión
- **AND** muestra "El segundo factor no es válido."

#### Scenario: Evaluador externo recibe el segundo factor por correo (HU-37.2 Escenario 3)
- **GIVEN** que el profesor Ramírez es evaluador externo
- **WHEN** ingresa su contraseña correcta
- **THEN** el sistema le exige el segundo factor antes de abrir la sesión
- **AND** le entrega el código por correo electrónico

#### Scenario: Tipo de usuario nuevo con acceso financiero exige segundo factor (HU-37.2 Escenario 4)
- **GIVEN** que Dirección creó el tipo "Auditor interno" con acceso a información financiera
- **WHEN** una persona asignada a ese tipo ingresa su contraseña correcta
- **THEN** el sistema le exige el segundo factor antes de abrir la sesión

#### Scenario: Falla en el canal de envío no permite atajo (HU-37.2 Escenario 5)
- **GIVEN** que Pablo ingresó su contraseña correcta
- **WHEN** el sistema no logra entregar el código del segundo factor por un fallo del canal de envío
- **THEN** el sistema no abre la sesión
- **AND** informa que no pudo entregar el código permitiendo solicitarlo de nuevo

#### Scenario: Contraseña incorrecta no habilita el segundo paso (HU-37.2 Escenario 6)
- **GIVEN** que Pablo es administrador financiero
- **WHEN** ingresa una contraseña incorrecta
- **THEN** el sistema no solicita ni envía el segundo factor
- **AND** no abre la sesión

#### Scenario: La sesión no se abre solo con la contraseña (HU-37.2 Escenario 7)
- **GIVEN** que Laura es coordinadora, ingresó su contraseña correcta y el sistema le está pidiendo el segundo factor
- **WHEN** intenta continuar o acceder a la sesión sin validar el segundo factor
- **THEN** el sistema no abre la sesión y continúa exigiendo el segundo factor

#### Scenario: Enlace de universidad aliada verifica en dos pasos (HU-37.2 Escenario 11)
- **GIVEN** que Andrea es el enlace de una universidad aliada y tiene su cuenta activa
- **WHEN** ingresa su contraseña correcta
- **THEN** el sistema le exige el segundo factor antes de abrir la sesión

### Requirement: Canal elegido, vigencia, bloqueo por intentos y recuperación del segundo factor (HU-37.2)

El código del segundo factor SHALL enviarse por el canal elegido por la persona (correo o SMS, salvo evaluadores externos que siempre reciben por correo), MUST vencer exactamente a los 15 minutos de su emisión, MUST bloquear nuevos intentos durante 1 hora tras 3 intentos fallidos, y SHALL permitir la recuperación por correo cuando la persona pierda su medio habitual.

#### Scenario: Envío por el canal elegido por la persona usuaria (HU-37.2 Escenario 8)
- **GIVEN** que Laura es coordinadora y eligió recibir el código por SMS
- **WHEN** ingresa su contraseña correcta
- **THEN** el sistema envía el código del segundo factor por SMS y no por correo

#### Scenario: Código vencido después de 15 minutos es rechazado (HU-37.2 Escenario 9)
- **GIVEN** que Pablo recibió su código de segundo factor hace más de 15 minutos
- **WHEN** ingresa ese código
- **THEN** el sistema no abre la sesión
- **AND** permite solicitar un código nuevo

#### Scenario: Tres intentos fallidos bloquean durante una hora (HU-37.2 Escenario 10)
- **GIVEN** que Pablo ya ingresó dos códigos de segundo factor incorrectos
- **WHEN** ingresa un tercer código incorrecto
- **THEN** el sistema no abre la sesión
- **AND** rechaza cualquier nuevo intento hasta que transcurra 1 hora

#### Scenario: Recuperación por correo al perder el medio del segundo factor (HU-37.2 Escenario 12)
- **GIVEN** que Laura perdió el teléfono donde recibía el código por SMS
- **WHEN** solicita recuperar su segundo factor
- **THEN** el sistema inicia la recuperación enviando el código al correo registrado de Laura

#### Scenario: Código ya usado y código reemplazado por reenvío son rechazados (HU-37.2)
- **GIVEN** que Laura ya validó un código de segundo factor o solicitó un reenvío de código
- **WHEN** intenta reutilizar el código ya consumido o el código anterior al reenvío aunque aún esté dentro de los 15 minutos
- **THEN** el sistema rechaza el código y no abre ninguna sesión adicional
