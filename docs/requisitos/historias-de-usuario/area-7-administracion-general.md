# Área 7 — Administración General

> Parte de las [Historias de Usuario · Plan B · versión final](README.md). Reglas técnicas en el [Anexo A](anexo-a-reglas-tecnicas.md); preguntas abiertas en el [Anexo B](anexo-b-preguntas-abiertas.md).

## RF-37 — Gestión de usuarios internos y sus permisos · *Debe tener*

### HU-37.1 Crear usuarios internos y tipos de usuario con acceso limitado a su función

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-37 | Debe tener | HU-08.2 (visibilidad de puntuaciones) | HU-37.1, HU-37.3, HU-37.4 |

#### Narrativa

- Como dirección de la Fundación
- Quiero crear usuarios internos con un rol, crear nuevos tipos de usuario con sus permisos y que cada rol acceda solo a lo que corresponde a su función
- Para adaptar la plataforma a nuevas funciones del equipo y proteger la información y la independencia de la evaluación

#### Reglas de negocio

- El personal de la Fundación (coordinadores, evaluadores, finanzas, dirección) tiene acceso limitado a lo que corresponde a su función (RF-37).
- Los evaluadores pueden ser internos o externos (RF-06).
- Dirección crea los usuarios internos y los tipos de usuario, y asigna los roles (S1). Ningún otro rol puede crear usuarios ni asignarse un rol distinto del suyo (RF-37, S1).
- La Fundación puede agregar nuevos tipos de usuario interno con el tiempo (RF-37).
- Si un nuevo tipo accede a información financiera o de decisión, requiere verificación en dos pasos (Restricción del cliente, ver HU-37.2).
- Un evaluador no ve las puntuaciones de otros evaluadores hasta el cierre del periodo de revisión (RF-37, RF-08, S9; ver HU-08.2).
- El equipo financiero no puede modificar rúbricas de evaluación (RF-37; la gestión de rúbricas pertenece a RF-07).
- La información socioeconómica solo es visible para quien la necesita para evaluar o decidir (RNF de confidencialidad).
- La identificación única de una persona en la plataforma es su número de cédula, también para los usuarios internos.
- El enlace de universidad aliada accede a la plataforma con verificación en dos pasos, igual que el personal con acceso a información financiera o de decisión (ver HU-37.2).
- **Nota de planificación (Arquitecto):** esta historia es prerrequisito funcional de toda historia que distinga "quién puede hacer qué" (RF-06, RF-11, RF-20); debe construirse al inicio del plan, aunque el catálogo del PRD la coloque al final.

#### Flujo de la HU

1. Si la Fundación necesita un nuevo tipo de usuario interno, Dirección abre la gestión de tipos de usuario, escribe el nombre del nuevo tipo y elige sus permisos.
2. El sistema verifica que el nombre no esté repetido y crea el tipo, que queda disponible para asignarlo a usuarios.
3. Si el tipo incluye permisos sobre información financiera o de decisión, el sistema lo marca como sujeto a verificación en dos pasos (HU-37.2).
4. Dirección abre la gestión de usuarios internos y registra los datos de identificación —número de cédula— y contacto de la persona.
5. Le asigna un rol: coordinador, evaluador, finanzas o dirección, o un tipo de usuario nuevo.
6. El sistema crea el usuario con ese rol.
7. La persona inicia sesión (HU-37.2).
8. El sistema carga los permisos de su rol y muestra solo las funciones y datos que su rol permite, aplicando la restricción en la respuesta del servidor, no solo en la pantalla.
9. Si intenta una acción que su rol no permite, el sistema la bloquea.

#### Flujos alternos y de error

- FA-1 (paso 1) Tipo sin nombre: el sistema no crea el tipo y muestra "Escribe el nombre del tipo de usuario."
- FA-2 (paso 2) Nombre de tipo repetido: el sistema no crea el tipo y muestra "El tipo de usuario Auditor interno ya existe."
- FA-3 (paso 5) Usuario sin rol: el sistema no crea el usuario y muestra "Asigna un rol al usuario."
- FA-4 (paso 5) Quien no es Dirección intenta crear un usuario o asignarse un rol distinto del suyo: el sistema lo bloquea y no crea ni modifica ningún usuario.
- FA-5 (paso 9) Acción no permitida: el sistema la bloquea y no cambia ningún dato.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-37.1).*

#### Criterios de aceptación (11 escenarios)

##### Escenario 1: Crear un coordinador y un evaluador externo

- **DADO QUE** Laura trabaja en la Fundación y el profesor Ramírez no es personal de la Fundación y formará parte de un comité
- **CUANDO** Dirección registra a Laura con el rol coordinador y al profesor Ramírez con el rol evaluador
- **ENTONCES** el usuario de Laura queda creado con el rol coordinador y el del profesor Ramírez con el rol evaluador, y este puede incluirse en un comité (HU-06.1)

##### Escenario 2: Solo Dirección crea usuarios

- **DADO QUE** Laura tiene el rol coordinador
- **CUANDO** intenta crear un usuario interno
- **ENTONCES** el sistema no le permite hacerlo

##### Escenario 3: Crear un tipo nuevo

- **DADO QUE** la Fundación necesita un rol "Asistente de comunicaciones"
- **CUANDO** Dirección lo crea con permiso para responder consultas (HU-32.2)
- **ENTONCES** el tipo queda creado y aparece como opción al crear usuarios

##### Escenario 4: Evaluador sin puntuaciones ajenas

- **DADO QUE** Luis y Ana evalúan la postulación de María y el periodo de revisión no ha cerrado
- **CUANDO** Luis abre esa postulación
- **ENTONCES** ve sus propias puntuaciones y no ve las de Ana (HU-08.2)

##### Escenario 5: Finanzas no modifica rúbricas

- **DADO QUE** Pablo es administrador financiero
- **CUANDO** intenta modificar la rúbrica de "Becas Ingeniería"
- **ENTONCES** el sistema lo bloquea, muestra "Tu rol no permite modificar rúbricas de evaluación." y la rúbrica queda igual

##### Escenario 6: Finanzas no ve datos socioeconómicos

- **DADO QUE** Pablo es administrador financiero
- **CUANDO** consulta la postulación de María
- **ENTONCES** no ve su ingreso familiar ni su composición familiar, y esos datos tampoco vienen en la respuesta que el sistema le entrega

##### Escenario 7: Nombre de tipo de usuario duplicado

- **DADO QUE** ya existe el tipo de usuario "Auditor interno"
- **CUANDO** Dirección intenta crear otro tipo con el mismo nombre
- **ENTONCES** el sistema no lo crea y muestra "El tipo de usuario Auditor interno ya existe."

##### Escenario 8: Auto-escalamiento de permisos bloqueado

- **DADO QUE** Laura tiene el rol coordinador
- **CUANDO** intenta asignarse a sí misma el rol dirección
- **ENTONCES** el sistema lo bloquea y el rol de Laura sigue siendo coordinador

##### Escenario 9: Tipo de usuario sin nombre

- **DADO QUE** Dirección abre la gestión de tipos de usuario
- **CUANDO** intenta crear un tipo dejando el nombre vacío
- **ENTONCES** el sistema no crea el tipo y muestra "Escribe el nombre del tipo de usuario."

##### Escenario 10: Usuario sin rol asignado

- **DADO QUE** Dirección registra los datos de identificación y contacto de Carlos como nuevo usuario interno
- **CUANDO** intenta guardarlo sin asignarle un rol
- **ENTONCES** el sistema no crea el usuario y muestra "Asigna un rol al usuario."

##### Escenario 11: Tipo nuevo con acceso financiero queda marcado para verificación en dos pasos

- **DADO QUE** Dirección está creando el tipo "Auditor interno" con permiso para ver información financiera
- **CUANDO** guarda el tipo
- **ENTONCES** el tipo queda creado y su configuración muestra que exige verificación en dos pasos (HU-37.2)

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Personal de la Fundación con acceso limitado a su función (RF-37) | Regla | 2, 4, 5, 6, 8 | CUBIERTA |
| Evaluadores internos o externos (RF-06) | Regla | 1 | CUBIERTA |
| Solo Dirección crea usuarios y tipos, y asigna roles; nadie se autoasigna un rol distinto (S1) | Regla | 2, 8 | CUBIERTA |
| La Fundación puede agregar nuevos tipos de usuario con el tiempo (RF-37) | Regla | 3, 11 | CUBIERTA |
| Tipo nuevo con acceso financiero o de decisión queda sujeto a verificación en dos pasos | Regla (flujo, paso 3) | 11 | CUBIERTA (el efecto en el login, en HU-37.2 Esc. 4) |
| Evaluador no ve puntuaciones de otros hasta el cierre del periodo | Regla (RF-37, RF-08) | 4 | CUBIERTA (compartida con HU-08.2, Esc. 2) |
| Finanzas no modifica rúbricas de evaluación | Regla (RF-37) | 5 | CUBIERTA (compartida con HU-07.1, Esc. 5) |
| Información socioeconómica visible solo para quien la necesita (RNF confidencialidad) | Regla | 6 | CUBIERTA |
| Autorización aplicada en el servidor, no solo en la pantalla | Regla técnica | 6 | CUBIERTA |
| Agregar un tipo nuevo no exige cambios de código ni despliegue | Regla técnica | — | NO VERIFICABLE EN CRITERIOS (revisión de diseño) |
| Identificación de la persona por número de cédula | Regla | — | DELEGADA (HU-01.1, identificación única de la persona) |
| El enlace de universidad aliada accede con verificación en dos pasos | Regla | — | CUBIERTA en HU-37.2 (Esc. 11); quién crea su cuenta: pregunta abierta |
| FA-1 Tipo sin nombre | FA | 9 | CUBIERTA |
| FA-2 Nombre de tipo repetido | FA | 7 | CUBIERTA |
| FA-3 Usuario sin rol | FA | 10 | CUBIERTA |
| FA-4 Quien no es Dirección crea un usuario o se autoasigna otro rol | FA | 2, 8 | CUBIERTA |
| FA-5 Acción no permitida: el sistema la bloquea y no cambia ningún dato | FA | 2, 5, 8 | CUBIERTA |
| Matriz completa de permisos de los cuatro roles base (paso 8 del flujo) | Regla | — | PENDIENTE DE DEFINICIÓN (pregunta 2 del cliente, sigue pendiente) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-37.1).*

### HU-37.2 Iniciar sesión con verificación en dos pasos

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-37 | Debe tener | HU-37.1 | HU-37.2 |

#### Narrativa

- Como coordinador de programa, administrador financiero, miembro del comité evaluador, enlace de universidad aliada o dirección de la Fundación
- Quiero verificar mi identidad con un segundo factor además de mi contraseña al iniciar sesión
- Para proteger la información financiera y las decisiones de la Fundación

#### Reglas de negocio

- Cualquier persona con acceso a información financiera o de decisión (coordinadores, finanzas, dirección, comité) verifica su identidad con un segundo factor además de su contraseña al iniciar sesión (Restricción del cliente).
- Un tipo de usuario nuevo con ese acceso también requiere el segundo factor (Restricción del cliente, ver HU-37.1).
- La sesión no se abre con la contraseña sola en ninguna circunstancia (Restricción del cliente).
- El enlace de universidad aliada también verifica su identidad con un segundo factor al iniciar sesión.
- El segundo factor es un código que el sistema entrega por **correo o SMS**, y el canal lo elige la persona usuaria.
- El código del segundo factor **vence a los 15 minutos** de enviarse.
- Se permiten **3 intentos fallidos** del código; después, la persona debe **esperar 1 hora** antes de volver a intentarlo.
- La persona puede pedir el **reenvío** del código.
- Si la persona pierde el medio donde recibe el segundo factor, la **recuperación se hace por correo**.
- A los **evaluadores externos** el código se les entrega **por correo**.
- El **primer registro** de los datos de pago lo hace el becario y también exige el código de un solo uso; el administrador financiero no modifica los datos de pago de un becario.

#### Flujo de la HU

1. La persona ingresa su usuario y contraseña.
2. El sistema valida la contraseña.
3. El sistema envía el código del segundo factor al canal que la persona eligió —correo o SMS— y lo pide.
4. La persona lo ingresa antes de que venza, dentro de los 15 minutos siguientes al envío.
5. El sistema valida el segundo factor.
6. Si es correcto, el sistema abre la sesión con los permisos de su rol (HU-37.1).

#### Flujos alternos y de error

- FA-1 (paso 2) Contraseña incorrecta: el sistema no pide el segundo factor y no abre la sesión.
- FA-2 (paso 5) Segundo factor incorrecto: el sistema no abre la sesión y muestra "El segundo factor no es válido."
- FA-3 (paso 3) El sistema no logra entregar el segundo factor (falla el canal de envío): la sesión no se abre; el sistema informa que no pudo entregar el código y permite solicitarlo de nuevo.
- FA-4 (paso 6) La persona ya verificó su segundo factor y luego intenta confirmar una operación sensible: el sistema le exige además el código de un solo uso de esa operación (HU-20.1, HU-22.1).
- FA-5 (paso 4) Código vencido: pasaron más de 15 minutos desde el envío; el sistema no abre la sesión y permite solicitar un código nuevo.
- FA-6 (paso 5) Tercer intento fallido: el sistema no abre la sesión y no admite un nuevo intento hasta que pase 1 hora.
- FA-7 (paso 3) La persona ya no tiene acceso al medio donde recibe el código: el sistema ofrece la recuperación por correo.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-37.2).*

#### Criterios de aceptación (12 escenarios)

##### Escenario 1: Acceso con los dos pasos

- **DADO QUE** Laura es coordinadora
- **CUANDO** ingresa su contraseña correcta y su segundo factor correcto
- **ENTONCES** el sistema abre su sesión

##### Escenario 2: Segundo factor incorrecto

- **DADO QUE** Pablo es administrador financiero e ingresó su contraseña correcta
- **CUANDO** ingresa un segundo factor incorrecto
- **ENTONCES** el sistema no abre la sesión y muestra "El segundo factor no es válido."

##### Escenario 3: También el comité

- **DADO QUE** el profesor Ramírez es evaluador externo
- **CUANDO** ingresa su contraseña correcta
- **ENTONCES** el sistema le pide el segundo factor antes de abrir la sesión y le entrega el código por correo

##### Escenario 4: Tipo de usuario nuevo con acceso financiero

- **DADO QUE** Dirección creó el tipo "Auditor interno" con acceso a información financiera (HU-37.1)
- **CUANDO** una persona con ese tipo inicia sesión
- **ENTONCES** el sistema le pide el segundo factor

##### Escenario 5: No existe atajo al segundo paso

- **DADO QUE** Pablo ingresó su contraseña correcta
- **CUANDO** el sistema no logra enviarle el segundo factor porque falla el canal de envío
- **ENTONCES** el sistema no abre la sesión y le permite solicitar el código de nuevo

##### Escenario 6: Contraseña incorrecta no habilita el segundo paso

- **DADO QUE** Pablo es administrador financiero
- **CUANDO** ingresa una contraseña incorrecta
- **ENTONCES** el sistema no le pide el segundo factor y no abre la sesión

##### Escenario 7: La sesión no se abre solo con la contraseña

- **DADO QUE** Laura es coordinadora, ingresó su contraseña correcta y el sistema le está pidiendo el segundo factor
- **CUANDO** intenta continuar sin ingresar el segundo factor
- **ENTONCES** el sistema no abre la sesión y sigue pidiendo el segundo factor

##### Escenario 8: El canal del código lo elige la persona usuaria

- **DADO QUE** Laura es coordinadora y eligió recibir el código por SMS
- **CUANDO** ingresa su contraseña correcta
- **ENTONCES** el sistema le envía el código del segundo factor por SMS y no por correo

##### Escenario 9: Código vencido a los 15 minutos

- **DADO QUE** Pablo recibió su código del segundo factor hace más de 15 minutos
- **CUANDO** lo ingresa
- **ENTONCES** el sistema no abre la sesión y le permite solicitar un código nuevo

##### Escenario 10: Tres intentos fallidos y espera de una hora

- **DADO QUE** Pablo ya ingresó dos códigos del segundo factor incorrectos
- **CUANDO** ingresa un tercer código incorrecto
- **ENTONCES** el sistema no abre la sesión y no admite un nuevo intento hasta que pase 1 hora

##### Escenario 11: El enlace de universidad aliada verifica en dos pasos

- **DADO QUE** Andrea es el enlace de la universidad aliada y tiene su cuenta creada
- **CUANDO** ingresa su contraseña correcta
- **ENTONCES** el sistema le pide el segundo factor antes de abrir la sesión

##### Escenario 12: Recuperación por correo al perder el medio del segundo factor

- **DADO QUE** Laura perdió el teléfono donde recibía el código por SMS
- **CUANDO** pide recuperar su segundo factor
- **ENTONCES** el sistema le permite recuperarlo por correo

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Persona con acceso financiero o de decisión verifica 2FA al iniciar sesión (Restricción del cliente) | Regla | 1, 2, 3 | CUBIERTA |
| Tipo de usuario nuevo con ese acceso también requiere 2FA (ver HU-37.1) | Regla | 4 | CUBIERTA |
| La sesión no se abre con la contraseña sola en ninguna circunstancia (Restricción del cliente) | Regla | 5, 6, 7 | CUBIERTA |
| El segundo factor funciona para personas externas a la Fundación | Regla técnica | 3, 11 | CUBIERTA |
| Qué roles y tipos exigen el segundo factor se resuelve desde la configuración de permisos, no de una lista fija | Regla técnica | 4 | CUBIERTA |
| El enlace de universidad aliada verifica en dos pasos | Regla | 11 | CUBIERTA (quién crea su cuenta: pregunta abierta de HU-37.1) |
| Canal del código: correo o SMS, a elección de la persona usuaria | Regla | 8 | CUBIERTA |
| Vigencia del código: 15 minutos | Regla | 9 | CUBIERTA |
| 3 intentos fallidos y espera de 1 hora | Regla | 10 | CUBIERTA |
| Reenvío del código permitido | Regla | 5 | CUBIERTA (si el reenvío invalida el anterior: pregunta abierta) |
| Recuperación por correo al perder el medio | Regla | 12 | CUBIERTA |
| Canal del código para evaluadores externos: correo | Regla | 3 | CUBIERTA |
| FA-1 Contraseña incorrecta: no pide el segundo factor y no abre la sesión | FA | 6 | CUBIERTA |
| FA-2 Segundo factor incorrecto | FA | 2 | CUBIERTA |
| FA-3 Falla el canal de envío del segundo factor | FA | 5 | CUBIERTA |
| FA-4 2FA ya verificado y luego operación sensible exige su propio OTP | FA | — | DELEGADA (HU-20.1, HU-22.1) |
| FA-5 Código vencido | FA | 9 | CUBIERTA |
| FA-6 Tercer intento fallido y espera de 1 hora | FA | 10 | CUBIERTA |
| FA-7 Pérdida del medio del segundo factor | FA | 12 | CUBIERTA |
| Primer registro de datos de pago: lo hace el becario y exige código de un solo uso | Regla | — | DELEGADA (datos de pago del becario, RF-20) |
| Rechazo de un código del segundo factor ya usado | Regla | — | PENDIENTE DE DEFINICIÓN (pregunta abierta de la historia) |
| Efecto del reenvío sobre el código anterior | Regla | — | PENDIENTE DE DEFINICIÓN (pregunta abierta de la historia) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-37.2).*

## RF-38 — Gestión del calendario académico institucional · *Debe tener*

### HU-38.1 Registrar y consultar los periodos académicos

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-38 | Debe tener | HU-37.1 (roles y permisos), HU-14.1 (universidades aliadas) | HU-38.1, HU-38.3 |

#### Narrativa

- Como dirección de la Fundación, quiero registrar la secuencia de periodos académicos y, para cada periodo, la fecha en que cada universidad aliada inicia sus inscripciones; y como persona del equipo interno autorizada, quiero consultar los periodos anteriores y lo que tienen ligado
- Quiero que los periodos anteriores sigan siendo consultables aunque no sean el actual
- Para ligar a ellos los programas, los ciclos y las renovaciones, operar sobre el periodo correcto y poder revisar los casos ya pasados

#### Reglas de negocio

- La Fundación define la secuencia de periodos académicos, por ejemplo "2026-1" y "2026-2" (RF-38).
- Los programas, los ciclos y las renovaciones se ligan a un periodo (RF-38).
- Los periodos anteriores siguen siendo consultables aunque no sean el actual (RF-38).
- El registro de periodos lo hace Dirección; los demás roles consultan (RF-38, RF-37 de acceso limitado por función).
- Un periodo necesita un nombre para registrarse, y su nombre no puede repetirse en la secuencia (RF-38: la secuencia identifica cada periodo, por ejemplo "2026-1", "2026-2").
- El periodo de becas es **común** a la Fundación (por ejemplo, el periodo de verano). Dentro de ese periodo, cada universidad aliada tiene su propia fecha de inicio de inscripciones, que se registra **por universidad y periodo**.
- Las fechas de cada periodo las define cada universidad.
- Cuando una universidad inicia sus inscripciones, se cierran las disponibilidades para esa universidad y termina su lista de espera; la fecha registrada en esta historia es la que dispara ese cierre (ver HU-02.2, HU-10.2, HU-14.2).
- El inicio y el cierre de los periodos los ejecuta **Dirección**.
- Las fechas se interpretan en **UTC-4** y el día de cierre se incluye **hasta las 23:59**.

#### Flujo de la HU

1. Dirección abre el calendario académico.
2. Ve la secuencia de periodos registrados.
3. Registra un nuevo periodo con su nombre.
4. El sistema verifica que el nombre no esté vacío ni repetido y lo agrega a la secuencia.
5. El periodo queda disponible para ligar programas, ciclos y renovaciones.
6. Una persona del equipo interno autorizada abre el calendario académico y elige un periodo anterior.
7. El sistema muestra los programas, ciclos y renovaciones ligados a ese periodo, identificándolo como periodo anterior y no como actual, junto con la fecha de inicio de inscripciones de cada universidad aliada en ese periodo.
8. Para un periodo registrado, Dirección registra la fecha de inicio de inscripciones de cada universidad aliada, según las fechas que define cada universidad.
9. El sistema guarda cada fecha asociada a esa universidad y ese periodo, interpretada en UTC-4.
10. Al llegar esa fecha, se cierran las disponibilidades de esa universidad y termina su lista de espera (ver HU-02.2, HU-10.2, HU-14.2).

#### Flujos alternos y de error

- FA-1 (paso 2) Sin periodos: el sistema muestra "Aún no hay periodos académicos registrados."
- FA-2 (paso 3) Nombre vacío o solo con espacios: el sistema no lo registra y muestra "Escribe el nombre del periodo académico."
- FA-3 (paso 4) Nombre repetido: el sistema no lo registra y muestra "El periodo 2026-2 ya existe."
- FA-4 (paso 3) Quien no es Dirección intenta registrar un periodo: el sistema lo bloquea y la secuencia no cambia.
- FA-5 (paso 6) Periodo que no existe en la secuencia: el sistema indica que ese periodo no está registrado.
- FA-6 (paso 7) Periodo sin registros ligados: el sistema indica que ese periodo no tiene programas, ciclos ni renovaciones.
- FA-7 (paso 8) Fecha de inicio de inscripciones vacía: el sistema no la registra, lo indica, y esa universidad queda sin fecha para ese periodo.
- FA-8 (paso 8) La universidad ya tiene fecha de inicio de inscripciones registrada en ese periodo: el sistema muestra la fecha registrada y no crea una segunda para la misma universidad y periodo.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-38.1).*

#### Criterios de aceptación (13 escenarios)

##### Escenario 1: Registrar un periodo

- **DADO QUE** la secuencia tiene el periodo "2026-1"
- **CUANDO** Dirección registra "2026-2"
- **ENTONCES** la secuencia muestra "2026-1" y "2026-2" y "2026-2" puede ligarse a un ciclo

##### Escenario 2: Periodo repetido

- **DADO QUE** ya existe el periodo "2026-2"
- **CUANDO** Dirección intenta registrar otro "2026-2"
- **ENTONCES** el sistema no lo registra y muestra "El periodo 2026-2 ya existe."

##### Escenario 3: Calendario vacío

- **DADO QUE** no hay periodos registrados
- **CUANDO** Dirección abre el calendario académico
- **ENTONCES** el sistema muestra "Aún no hay periodos académicos registrados."

##### Escenario 4: Consultar un periodo anterior

- **DADO QUE** "2026-2" es el periodo actual y en "2026-1" hubo el ciclo "Becas Ingeniería 2026-1" y la renovación de la beca de Carlos
- **CUANDO** el coordinador consulta "2026-1"
- **ENTONCES** ve el ciclo "Becas Ingeniería 2026-1" y la renovación de la beca de Carlos

##### Escenario 5: Periodo sin registros

- **DADO QUE** el periodo "2025-2" no tiene programas, ciclos ni renovaciones
- **CUANDO** el coordinador lo consulta
- **ENTONCES** el sistema muestra "El periodo 2025-2 no tiene programas, ciclos ni renovaciones."

##### Escenario 6: El periodo anterior no aparece como actual

- **DADO QUE** "2026-2" es el periodo actual
- **CUANDO** el coordinador consulta "2026-1"
- **ENTONCES** "2026-1" se muestra como periodo anterior, no como actual

##### Escenario 7: Nombre de periodo vacío

- **DADO QUE** Dirección está registrando un nuevo periodo
- **CUANDO** deja el nombre vacío o solo con espacios
- **ENTONCES** el sistema no lo registra y muestra "Escribe el nombre del periodo académico."

##### Escenario 8: Solo Dirección registra periodos

- **DADO QUE** Laura tiene el rol coordinador
- **CUANDO** intenta registrar el periodo "2027-1"
- **ENTONCES** el sistema lo bloquea y la secuencia de periodos queda igual

##### Escenario 9: Consultar un periodo que no existe en la secuencia

- **DADO QUE** la secuencia de periodos no incluye "2024-1"
- **CUANDO** el coordinador intenta consultar el periodo "2024-1"
- **ENTONCES** el sistema indica que ese periodo no está registrado

##### Escenario 10: Registrar la fecha de inicio de inscripciones de una universidad

- **DADO QUE** el periodo "2026-2" está registrado y la Universidad Metropolitana es universidad aliada
- **CUANDO** Dirección registra el 15/09/2026 como fecha de inicio de inscripciones de la Universidad Metropolitana para "2026-2"
- **ENTONCES** el calendario muestra esa fecha asociada a la Universidad Metropolitana y al periodo "2026-2"

##### Escenario 11: Cada universidad tiene su propia fecha en el mismo periodo

- **DADO QUE** en el periodo "2026-2" la Universidad Metropolitana inicia sus inscripciones el 15/09/2026 y la Universidad Central el 01/10/2026
- **CUANDO** el coordinador consulta "2026-2"
- **ENTONCES** ve las dos fechas de inicio de inscripciones, una por universidad, dentro del mismo periodo "2026-2"

##### Escenario 12: Fecha de inicio de inscripciones vacía

- **DADO QUE** Dirección está registrando la fecha de inicio de inscripciones de la Universidad Central para "2026-2"
- **CUANDO** deja la fecha vacía
- **ENTONCES** el sistema no la registra, lo indica, y la Universidad Central sigue sin fecha de inicio de inscripciones para "2026-2"

##### Escenario 13: Una sola fecha por universidad y periodo

- **DADO QUE** la Universidad Metropolitana ya tiene registrado el 15/09/2026 como inicio de inscripciones para "2026-2"
- **CUANDO** Dirección intenta registrar otra fecha de inicio de inscripciones para la Universidad Metropolitana en "2026-2"
- **ENTONCES** el sistema muestra la fecha ya registrada y no crea una segunda fecha para esa universidad en ese periodo

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| La Fundación define la secuencia de periodos académicos (RF-38) | Regla | 1 | CUBIERTA |
| Programas, ciclos y renovaciones se ligan a un periodo (RF-38) | Regla | 1, 4 | CUBIERTA |
| Los periodos anteriores siguen siendo consultables (RF-38) | Regla | 4, 6 | CUBIERTA |
| El registro lo hace Dirección; los demás roles consultan (RF-38, RF-37) | Regla | 1, 4, 8 | CUBIERTA |
| Un periodo necesita nombre no vacío y no repetido (RF-38) | Regla | 2, 7 | CUBIERTA |
| Lo ligado a un periodo anterior sigue resolviéndose sobre ese periodo (RNF trazabilidad) | Regla técnica | 4, 6 | CUBIERTA |
| Periodo común con fecha de inicio de inscripciones por universidad y periodo | Regla | 10, 11 | CUBIERTA |
| Las fechas del periodo las define cada universidad | Regla | 11 | CUBIERTA |
| Una sola fecha de inicio de inscripciones por universidad y periodo | Regla técnica | 13 | CUBIERTA |
| Al iniciar las inscripciones de una universidad se cierran sus disponibilidades y termina su lista de espera | Regla | — | DELEGADA (HU-02.2, HU-10.2, HU-14.2) |
| Interpretación de fechas en UTC-4, con el día de cierre hasta las 23:59 | Regla | — | DELEGADA (HU-02.1, ventana del ciclo de postulación); la hora exacta del inicio de inscripciones es pregunta abierta |
| Inicio y cierre de periodos a cargo de Dirección | Regla | 1, 8 | CUBIERTA |
| El bloqueo de la rúbrica lo dispara la apertura del periodo de evaluación, no el cierre de este periodo académico | Regla | — | DELEGADA (HU-07.2) |
| FA-1 Sin periodos registrados | FA | 3 | CUBIERTA |
| FA-2 Nombre vacío o solo con espacios | FA | 7 | CUBIERTA |
| FA-3 Nombre repetido | FA | 2 | CUBIERTA |
| FA-4 Quien no es Dirección intenta registrar un periodo | FA | 8 | CUBIERTA |
| FA-5 Periodo que no existe en la secuencia | FA | 9 | CUBIERTA |
| FA-6 Periodo sin registros ligados | FA | 5 | CUBIERTA |
| FA-7 Fecha de inicio de inscripciones vacía | FA | 12 | CUBIERTA |
| FA-8 Universidad con fecha ya registrada en ese periodo | FA | 13 | CUBIERTA |
| Criterio de orden de la secuencia; edición o borrado de un periodo con referencias | Regla | — | PENDIENTE DE DEFINICIÓN (preguntas abiertas de la historia) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-38.1).*

### HU-38.2 Marcar el periodo actual sin interrumpir renovaciones

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-38 | Debe tener | HU-26.2, HU-35.1, HU-38.1 | HU-38.2 |

#### Narrativa

- Como dirección de la Fundación
- Quiero marcar cuál es el periodo actual
- Para que la operación use el periodo correcto sin afectar lo que sigue en curso

#### Reglas de negocio

- Solo un periodo puede ser el "actual" a la vez (RF-38).
- Pasar a un nuevo periodo no interrumpe las renovaciones del periodo anterior que sigan en curso (RF-38).
- Marcar el periodo actual lo hace Dirección (RF-38, RF-37 de acceso limitado por función).
- El inicio y el cierre de los periodos los ejecuta **Dirección**.
- Al marcarse como actual el nuevo periodo, las becas *Renovada* para ese periodo vuelven a *Activa* (S4).
- Al marcarse como actual el nuevo periodo, las becas cuya renovación para ese periodo se rechazó pasan de *Activa* a ***Inactiva*** y no reciben dinero en el siguiente periodo (ver HU-26.2).
- Una beca en pausa que vence sin reactivarse queda *Inactiva* por su propio disparador —el vencimiento de la pausa, ver HU-27.2—, no por marcarse un nuevo periodo actual.
- El cambio de periodo solo mueve esas dos poblaciones de becas. Una beca *Suspendida* —incluida la pausa de RF-27 con motivo "Pausa" (S5)— no cambia de estado por marcarse un nuevo periodo actual; las transiciones permitidas de una beca son las del PRD más el estado *Inactiva* que incorpora C3, con sus transiciones de salida **confirmadas**: *Inactiva* → *Activa* e *Inactiva* → *Concluida* (que sigue siendo estado final); **confirmado también** que una beca *Inactiva* puede revocarse directamente (*Inactiva* → *Revocada*), igual que cualquier estado activo (ver HU-04.1 para el control de saltos de estado).
- El periodo actual es **común** a la Fundación; el cierre de disponibilidades y el fin de la lista de espera ocurren **por universidad**, al llegar la fecha en que esa universidad inicia sus inscripciones (HU-38.1), y no dependen de marcar el periodo actual.
- Estos cambios de estado quedan justificados y registrados (regla transversal; ver HU-35.1).
- Las becas cuyo becario no solicitó la renovación a tiempo quedan *Inactiva* según la regla de RF-26 actualizada por C3 (ver HU-26.2); esta historia no define ese caso.

#### Flujo de la HU

1. Dirección abre el calendario académico.
2. Marca un periodo como actual.
3. El sistema quita la marca al periodo que era actual y deja un único periodo marcado como actual.
4. El sistema fija el corte de becas que califican para el cambio: las *Renovada* para el nuevo periodo y las que tienen renovación rechazada para ese periodo.
5. El sistema pasa a *Activa* las becas *Renovada* para el nuevo periodo (S4) y a *Inactiva* las becas cuya renovación para ese periodo se rechazó, y registra cada cambio por beca con su justificación (HU-35.1).
6. Las becas que no están en el corte, incluidas las *Suspendida*, conservan su estado.
7. Las renovaciones del periodo anterior que siguen en curso continúan su trámite (HU-26.2).

#### Flujos alternos y de error

- FA-1 (paso 2) Periodo ya actual: el sistema no hace cambios.
- FA-2 (paso 2) Quien no es Dirección intenta marcar el periodo actual: el sistema lo bloquea y el periodo actual no cambia.
- FA-3 (paso 3) Dos solicitudes de cambio de periodo actual casi simultáneas: el sistema deja un único periodo actual de forma consistente, sin perder silenciosamente ninguno de los dos intentos.
- FA-4 (paso 5) El *fan-out* se interrumpe a la mitad: el sistema puede reanudarlo sin duplicar cambios ni registros, y ninguna beca queda en un estado intermedio.
- FA-5 (paso 4) Sin becas *Renovada* ni renovaciones rechazadas para el nuevo periodo: el periodo queda marcado como actual y no se genera ningún registro de cambio de estado.
- FA-6 (paso 2) Marcar el periodo actual no cierra las disponibilidades de ninguna universidad: ese cierre lo dispara la fecha de inicio de inscripciones de cada universidad (HU-38.1).

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-38.2).*

#### Criterios de aceptación (11 escenarios)

##### Escenario 1: Cambiar el periodo actual

- **DADO QUE** "2026-1" es el periodo actual
- **CUANDO** Dirección marca "2026-2" como actual
- **ENTONCES** "2026-2" es el único periodo actual y "2026-1" deja de serlo

##### Escenario 2: Renovación en curso no se interrumpe

- **DADO QUE** Carlos solicitó la renovación de su beca en "2026-1" y aún no se decide
- **CUANDO** Dirección marca "2026-2" como actual
- **ENTONCES** la solicitud de Carlos sigue pendiente y el coordinador puede decidirla (HU-26.2)

##### Escenario 3: Nunca dos periodos actuales

- **DADO QUE** "2026-2" es el periodo actual
- **CUANDO** Dirección consulta el calendario después de cualquier cambio
- **ENTONCES** solo un periodo aparece marcado como actual

##### Escenario 4: Becas renovadas y rechazadas al cambiar de periodo

- **DADO QUE** "2026-2" es el periodo actual, la beca de Pedro está *Renovada* para "2027-1" y la renovación de Lucía para "2027-1" fue rechazada
- **CUANDO** Dirección marca "2027-1" como actual
- **ENTONCES** la beca de Pedro pasa a *Activa*, la de Lucía pasa a *Inactiva* y no recibe dinero en "2027-1", y cada cambio queda registrado por separado con quién lo originó, cuándo y por qué (HU-35.1)

##### Escenario 5: Solo Dirección marca el periodo actual

- **DADO QUE** Laura tiene el rol coordinador
- **CUANDO** intenta marcar "2026-2" como periodo actual
- **ENTONCES** el sistema lo bloquea y el periodo actual no cambia

##### Escenario 6: Dos cambios de periodo actual casi simultáneos

- **DADO QUE** "2026-1" es el periodo actual
- **CUANDO** dos personas de Dirección intentan marcar, casi al mismo tiempo, "2026-2" y "2027-1" como actual
- **ENTONCES** al terminar ambas solicitudes un único periodo aparece marcado como actual —ni dos ni ninguno— y los cambios de estado de becas corresponden solo al periodo que quedó actual

##### Escenario 7: Beca en pausa no cambia con la transición de periodo

- **DADO QUE** la beca de Ana está *Suspendida* por pausa (S5), su pausa no ha vencido y no tiene renovación asociada al nuevo periodo
- **CUANDO** Dirección marca "2027-1" como actual
- **ENTONCES** la beca de Ana permanece *Suspendida*, sin pasar a *Activa* ni a *Inactiva*

##### Escenario 8: Marcar como actual un periodo que ya es el actual

- **DADO QUE** "2026-2" es el periodo actual
- **CUANDO** Dirección intenta marcar "2026-2" como actual
- **ENTONCES** el sistema no hace ningún cambio y "2026-2" sigue siendo el único periodo actual

##### Escenario 9: Cambio de periodo sin becas que califiquen

- **DADO QUE** no hay becas *Renovada* ni renovaciones rechazadas para "2027-1"
- **CUANDO** Dirección marca "2027-1" como actual
- **ENTONCES** "2027-1" queda marcado como actual y el sistema no genera ningún registro de cambio de estado de beca

##### Escenario 10: Reanudar un cambio de periodo interrumpido

- **DADO QUE** el cambio a "2027-1" como periodo actual se interrumpió después de aplicar la transición a la beca de Pedro pero antes de aplicarla a la de Lucía
- **CUANDO** el sistema reanuda el proceso
- **ENTONCES** la beca de Pedro no se actualiza una segunda vez ni genera un registro adicional, y la beca de Lucía pasa a *Inactiva* con un único registro

##### Escenario 11: El cambio de periodo actual no cierra las disponibilidades de una universidad

- **DADO QUE** "2026-1" es el periodo actual y la Universidad Metropolitana aún no llega a su fecha de inicio de inscripciones para "2026-2" (HU-38.1)
- **CUANDO** Dirección marca "2026-2" como actual
- **ENTONCES** las disponibilidades de la Universidad Metropolitana siguen abiertas y solo se cerrarán al llegar esa fecha

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Solo un periodo puede ser el "actual" a la vez (RF-38) | Regla | 1, 3, 6 | CUBIERTA |
| Pasar a un nuevo periodo no interrumpe renovaciones en curso del anterior (RF-38) | Regla | 2 | CUBIERTA |
| Marcar el periodo actual lo hace Dirección (RF-38, RF-37) | Regla | 1, 5 | CUBIERTA |
| Inicio y cierre de periodos a cargo de Dirección | Regla | 1, 5 | CUBIERTA |
| Becas *Renovada* para el nuevo periodo pasan a *Activa* (S4) | Regla | 4 | CUBIERTA (sujeta a validación de S4 con el cliente) |
| Becas con renovación rechazada para el nuevo periodo pasan a *Inactiva* y no reciben dinero | Regla | 4, 10 | CUBIERTA |
| Pausa vencida sin reactivar queda *Inactiva* por su propio disparador | Regla | 7 | DELEGADA (HU-27.2); aquí solo se verifica que el cambio de periodo no la mueve |
| El cambio solo mueve esas dos poblaciones; una beca *Suspendida* (incluida la pausa S5) no cambia | Regla | 7, 9 | CUBIERTA |
| El periodo actual es común; el cierre de disponibilidades es por universidad | Regla | 11 | CUBIERTA (el cierre en sí, en HU-02.2, HU-10.2 y HU-14.2) |
| Cada cambio de estado queda justificado y registrado, de forma individual por beca | Regla / Técnica | 4, 10 | CUBIERTA (el detalle no editable del histórico, en HU-35.1) |
| *Fan-out* transaccional por beca e idempotente ante reintentos | Regla técnica | 10 | CUBIERTA |
| FA-1 Periodo ya actual: el sistema no hace cambios | FA | 8 | CUBIERTA |
| FA-2 Quien no es Dirección intenta marcar el periodo actual | FA | 5 | CUBIERTA |
| FA-3 Dos solicitudes de cambio casi simultáneas | FA | 6 | CUBIERTA (el aviso a la solicitud no aplicada: pregunta abierta) |
| FA-4 El *fan-out* se interrumpe a la mitad: reanudable sin duplicar | FA | 10 | CUBIERTA |
| FA-5 Sin becas *Renovada* ni renovaciones rechazadas para el nuevo periodo | FA | 9 | CUBIERTA |
| FA-6 Marcar el periodo actual no cierra disponibilidades de universidades | FA | 11 | CUBIERTA |
| Becas sin solicitud de renovación a tiempo (quedan *Inactiva*) | Regla | — | DELEGADA (HU-26.2) |
| Renovación decidida en el mismo instante que el cambio de periodo | Regla | — | PENDIENTE DE DEFINICIÓN (pregunta abierta de la historia) |
| *Inactiva* no es estado final; puede volver a *Activa* o pasar a *Concluida* (que sigue siendo final) cuando el becario sube los datos que le faltan y, con eso, el coordinador de programa decide el paso; también puede revocarse directamente | Regla | — | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-38.2).*

## RF-39 — Registro de conflictos de interés · *Debería tener*

### HU-39.1 Declarar un conflicto de interés y restringir mi participación en ese caso

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-39 | Debería tener | HU-01.1, HU-06.2 | HU-39.1, HU-39.2 |

#### Narrativa

- Como miembro del comité evaluador o coordinador de programa
- Quiero declarar un conflicto de interés con un postulante específico y que el sistema restrinja mi participación en ese caso
- Para no participar en un caso en el que no puedo ser imparcial y que la decisión sobre ese postulante no dependa de mí

#### Reglas de negocio

- Cualquier persona del equipo (evaluador, coordinador) puede declarar un conflicto de interés con un postulante específico (RF-39). Los roles sin poder de evaluación ni de decisión sobre la postulación no tienen esa acción disponible (RF-39, RF-37).
- Quien tiene poder de decisión sobre una postulación declara el conflicto antes de participar en su evaluación (regla transversal).
- El conflicto se declara **con un postulante**, no con una postulación puntual (RF-39), por lo que la restricción alcanza también a postulaciones posteriores de esa misma persona.
- La declaración queda registrada de forma permanente y no puede editarse ni eliminarse después (RNF de trazabilidad; alimenta el mismo histórico de RF-35, ver HU-35.1).
- La declaración de conflicto restringe la participación de esa persona en ese caso (RF-39).
- Un evaluador no puede revisar una postulación si declaró un conflicto con esa persona (RF-06, ver HU-06.2).
- El evaluador con conflicto declarado no puede evaluar y **su lugar pasa a otro evaluador del comité**.
- Cada postulación recibe al menos dos evaluaciones independientes, salvo excepción autorizada por el coordinador (RF-06, ver HU-06.2).

#### Flujo de la HU

1. El evaluador o coordinador recibe o abre la postulación, con la opción "Declarar conflicto de interés" disponible desde ese momento y antes de puntuar o decidir.
2. Elige "Declarar conflicto de interés".
3. Confirma la declaración.
4. El sistema registra de forma permanente quién declaró, con qué postulante y cuándo.
5. Si es evaluador, el sistema le impide puntuar o comentar las postulaciones de ese postulante.
6. Si es coordinador, el sistema le impide tomar decisiones sobre las postulaciones de ese postulante.
7. Si quien declaró es evaluador, su lugar pasa a otro evaluador del comité; si no queda otro evaluador del comité disponible y la postulación se queda con menos de dos evaluadores, el sistema lo indica al coordinador para asignar otro evaluador o autorizar la excepción (HU-06.2).

#### Flujos alternos y de error

- FA-1 (paso 2) Conflicto ya declarado: el sistema muestra la declaración existente y no crea otra.
- FA-2 (paso 5) Evaluador ya asignado: pierde el acceso para puntuar esa postulación.
- FA-3 (paso 2) Un rol sin poder de evaluación ni de decisión sobre la postulación: el sistema no le ofrece la acción de declarar conflicto de interés.
- FA-4 (paso 7) El coordinador del programa es quien declaró el conflicto: el sistema lo restringe igual y marca el caso como pendiente de resolución de autoridad (ver Preguntas abiertas).
- FA-5 (paso 7) No queda ningún evaluador del comité sin conflicto con ese postulante: el sistema lo indica al coordinador y el caso queda pendiente de asignar otro evaluador o de autorizar la excepción (HU-06.2).

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-39.1).*

#### Criterios de aceptación (10 escenarios)

##### Escenario 1: Evaluador declara un conflicto

- **DADO QUE** Luis es miembro del comité de "Becas Ingeniería 2026-2" y María Pérez es su sobrina
- **CUANDO** Luis declara un conflicto de interés con María Pérez
- **ENTONCES** el sistema registra la declaración con Luis, María Pérez y la fecha

##### Escenario 2: Coordinador declara un conflicto

- **DADO QUE** Laura coordina "Becas Ingeniería" y conoce a Jorge Díaz
- **CUANDO** declara un conflicto de interés con Jorge Díaz
- **ENTONCES** el sistema registra la declaración con Laura, Jorge Díaz y la fecha

##### Escenario 3: Declaración repetida

- **DADO QUE** Luis ya declaró un conflicto con María Pérez
- **CUANDO** intenta declararlo otra vez
- **ENTONCES** el sistema no crea otra declaración y muestra "Ya declaraste un conflicto de interés con este postulante."

##### Escenario 4: Evaluador restringido

- **DADO QUE** Luis declaró un conflicto con María Pérez
- **CUANDO** intenta puntuar la postulación de María
- **ENTONCES** el sistema lo impide y muestra "Declaraste un conflicto de interés con este postulante. No puedes participar en su evaluación."

##### Escenario 5: Coordinador restringido

- **DADO QUE** Laura declaró un conflicto con Jorge Díaz
- **CUANDO** intenta anular el resultado No elegible de Jorge (HU-04.2)
- **ENTONCES** el sistema lo impide y muestra "Declaraste un conflicto de interés con este postulante. No puedes decidir sobre su postulación."

##### Escenario 6: El lugar del evaluador con conflicto pasa a otro evaluador del comité

- **DADO QUE** la postulación de María tenía asignados a Luis y Ana, y el comité de "Becas Ingeniería 2026-2" tiene además a Rosa, sin conflicto con María
- **CUANDO** Luis declara un conflicto con María
- **ENTONCES** el lugar de Luis pasa a Rosa, la postulación vuelve a tener dos evaluadores asignados y el coordinador queda informado del reemplazo (HU-06.2)

##### Escenario 7: Finanzas no tiene la opción de declarar conflicto de interés

- **DADO QUE** Pablo es administrador financiero
- **CUANDO** busca la opción de declarar un conflicto de interés sobre un postulante
- **ENTONCES** el sistema no le ofrece esa acción, porque no tiene poder de decisión ni de evaluación sobre la postulación (RF-39)

##### Escenario 8: El conflicto persiste si el postulante vuelve a postular

- **DADO QUE** Luis declaró un conflicto de interés con María Pérez en el ciclo "Becas Ingeniería 2026-2"
- **CUANDO** María vuelve a postular en el ciclo "Becas Ingeniería 2027-1" y Luis es asignado como evaluador
- **ENTONCES** el sistema le impide puntuar la nueva postulación de María y muestra "Declaraste un conflicto de interés con este postulante. No puedes participar en su evaluación."

##### Escenario 9: La opción de declarar está disponible antes de puntuar

- **DADO QUE** a Luis se le asignó la postulación de María Pérez y aún no ha registrado ninguna puntuación en ella
- **CUANDO** abre esa postulación
- **ENTONCES** ve disponible la opción "Declarar conflicto de interés"

##### Escenario 10: No queda otro evaluador del comité sin conflicto

- **DADO QUE** el comité de "Becas Ingeniería 2027-1" solo tiene a Luis y Ana, y Ana ya declaró un conflicto con María Pérez
- **CUANDO** Luis declara también un conflicto con María Pérez
- **ENTONCES** la postulación de María queda sin evaluadores habilitados y el sistema lo indica al coordinador para asignar otro evaluador o autorizar la excepción (HU-06.2)

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Evaluador o coordinador declara conflicto con un postulante; roles sin poder de evaluación o decisión no tienen la acción (RF-39, RF-37) | Regla | 1, 2, 7 | CUBIERTA |
| La opción de declarar está disponible desde que la postulación es asignada o visible, antes de puntuar o decidir (regla transversal) | Regla | 9 | CUBIERTA |
| El conflicto se declara con el postulante y alcanza postulaciones posteriores de esa persona (RF-39) | Regla | 8 | CUBIERTA |
| Registro permanente de quién declaró, con qué postulante y cuándo | Regla | 1, 2 | CUBIERTA |
| La declaración no puede editarse ni eliminarse (mismo histórico de RF-35) | Regla | — | DELEGADA (HU-35.1) |
| La declaración restringe la participación de esa persona en ese caso (RF-39) | Regla | 4, 5, 8 | CUBIERTA |
| Un evaluador no puede revisar una postulación si declaró conflicto con esa persona (RF-06) | Regla | 4, 8 | CUBIERTA (compartida con HU-06.2, Esc. 6; la asignación bloqueada, en HU-06.2, Esc. 5) |
| El lugar del evaluador con conflicto pasa a otro evaluador del comité | Regla | 6 | CUBIERTA (la ejecución de la asignación, compartida con HU-06.2) |
| Cada postulación recibe al menos dos evaluaciones independientes, salvo excepción autorizada (RF-06) | Regla | 6, 10 | CUBIERTA en su efecto; la asignación y la excepción, DELEGADAS (HU-06.2) |
| La restricción se aplica en el mismo punto de autorización que decide quién puntúa o decide | Regla técnica | 4, 5, 8 | CUBIERTA |
| FA-1 Conflicto ya declarado | FA | 3 | CUBIERTA |
| FA-2 Evaluador asignado pierde acceso para puntuar | FA | 4 | CUBIERTA |
| FA-3 Rol sin poder de evaluación ni de decisión: no se ofrece la acción | FA | 7 | CUBIERTA |
| FA-4 El coordinador que declaró el conflicto queda igualmente restringido | FA | 5 | CUBIERTA |
| FA-4 Marca del caso como pendiente de resolución de autoridad | FA | — | PENDIENTE DE DEFINICIÓN (no existe mecanismo de escalamiento definido; pregunta abierta de la historia) |
| FA-5 Sin otro evaluador del comité sin conflicto | FA | 10 | CUBIERTA |
| Retiro de una declaración; puntuaciones previas al conflicto | Regla | — | PENDIENTE DE DEFINICIÓN (preguntas abiertas de la historia) |
| Forma de la reasignación del lugar del evaluador (automática o por el coordinador) | Regla | — | PENDIENTE DE DEFINICIÓN (pregunta abierta de la historia) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-39.1).*

## RF-40 — Panel de indicadores para dirección · *Podría tener*

### HU-40.1 Ver los indicadores clave actualizados sin recargar la página

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-40 | Podría tener | HU-16.1, HU-20.1, HU-22.1, HU-22.2, HU-23.1, HU-34.1 | HU-40.1, HU-40.2 |

#### Narrativa

- Como dirección de la Fundación
- Quiero una vista resumida de los indicadores clave que se actualice sola cuando ocurre un evento
- Para conocer el estado de la Fundación de un vistazo y ver siempre la situación actual sin recargar la página

#### Reglas de negocio

- El panel muestra postulantes activos por ciclo, tasa de aprobación, presupuesto ejecutado frente a disponible y becarios en riesgo de incumplimiento (RF-40).
- Los indicadores se actualizan a medida que ocurren los eventos (nueva postulación, desembolso, cambio de estatus), sin recargar la página (RF-40).
- El panel refleja los cambios al instante (RNF de actualizaciones en tiempo real).
- Consultar el panel no compite por recursos con las operaciones transaccionales del día a día (RF-40).
- El panel es la vista de dirección; el acceso está limitado a lo que corresponde a cada función (RF-40, RF-37).
- El indicador de presupuesto disponible cuenta como no disponible el presupuesto comprometido en renovaciones pendientes del ciclo actual, no solo lo ya desembolsado (RF-12, ver HU-12.2); y refleja el presupuesto y cupo que se liberan al revocar una beca (RF-22, ver HU-22.1 y HU-22.2).
- Las **ofertas pendientes** de aceptación también cuentan como presupuesto no disponible, hasta que el proceso se rechace definitivamente (ver HU-15.1, HU-16.1).
- El panel refleja el paso de una beca a ***Inactiva*** como cualquier otro cambio de estatus (C3). **Decisión del cliente:** una beca *Inactiva* **no** libera su presupuesto ni su cupo del programa; los mantiene ocupados mientras esté *Inactiva*, a diferencia de la revocada.
- No se expone información sensible individual en los indicadores: solo datos agregados (RNF de confidencialidad, RF-33).

#### Flujo de la HU

1. Dirección abre el panel de indicadores.
2. El sistema muestra los cuatro indicadores a partir de los agregados ya calculados.
3. Las operaciones diarias siguen su curso mientras el panel está abierto, sin esperar al panel.
4. Ocurre un evento: nueva postulación, desembolso, cambio de estatus —incluido el paso a *Inactiva*— o una oferta emitida o rechazada definitivamente.
5. El sistema actualiza el agregado afectado de forma atómica y lo empuja al panel.
6. Dirección ve el nuevo valor sin recargar la página.

#### Flujos alternos y de error

- FA-1 (paso 2) Sin ciclos abiertos: el indicador de postulantes activos por ciclo indica que no hay ciclos.
- FA-2 (paso 4) Varios eventos seguidos o simultáneos: el panel refleja todos los eventos y ninguno se pierde.
- FA-3 (paso 1) Un rol distinto de dirección intenta abrir el panel: el sistema le niega el acceso y no muestra ningún indicador.
- FA-4 (paso 5) Se interrumpe el canal en tiempo real: al restablecerse, el panel reconcilia sus valores con los del servidor y no deja un valor obsoleto sin aviso.
- FA-5 (paso 4) Una oferta pendiente se rechaza definitivamente: el presupuesto comprometido en ella vuelve a contar como disponible en el panel.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-40.1).*

#### Criterios de aceptación (14 escenarios)

##### Escenario 1: Panel con los cuatro indicadores

- **DADO QUE** hay ciclos, postulaciones, desembolsos y becarios registrados
- **CUANDO** Dirección abre el panel
- **ENTONCES** ve postulantes activos por ciclo, tasa de aprobación, presupuesto ejecutado frente a disponible y becarios en riesgo de incumplimiento

##### Escenario 2: Presupuesto ejecutado frente a disponible

- **DADO QUE** los programas tienen 12.000 USD desembolsados y 38.000 USD disponibles (HU-23.1)
- **CUANDO** Dirección abre el panel
- **ENTONCES** el indicador muestra 12.000 USD ejecutados frente a 38.000 USD disponibles

##### Escenario 3: La operación diaria no se detiene

- **DADO QUE** Dirección tiene el panel abierto
- **CUANDO** finanzas confirma un desembolso y un evaluador envía su evaluación
- **ENTONCES** ambas operaciones se completan sin esperar al panel

##### Escenario 4: Cambio de estatus de una beca

- **DADO QUE** Dirección tiene el panel abierto y muestra 38.000 USD disponibles
- **CUANDO** el coordinador revoca una beca con 2.000 USD aprobados aún no desembolsados (HU-22.1, HU-22.2)
- **ENTONCES** el panel muestra 40.000 USD disponibles sin recargar la página

##### Escenario 5: Desembolso confirmado

- **DADO QUE** el panel muestra 12.000 USD ejecutados
- **CUANDO** finanzas confirma un desembolso de 500 USD (HU-20.1)
- **ENTONCES** el panel muestra 12.500 USD ejecutados sin recargar la página

##### Escenario 6: Eventos simultáneos

- **DADO QUE** el panel muestra 12.000 USD ejecutados
- **CUANDO** dos administradores financieros confirman al mismo tiempo un desembolso de 500 USD cada uno
- **ENTONCES** el panel muestra 13.000 USD ejecutados sin recargar la página

##### Escenario 7: Solo Dirección accede al panel

- **DADO QUE** Laura tiene el rol coordinador
- **CUANDO** intenta abrir el panel de indicadores
- **ENTONCES** el sistema le niega el acceso

##### Escenario 8: Presupuesto comprometido en renovaciones pendientes se refleja como no disponible

- **DADO QUE** "Becas Ingeniería" tiene 50.000 USD de presupuesto, 12.000 USD desembolsados y 10.000 USD comprometidos en renovaciones pendientes de decisión del ciclo actual (HU-12.2)
- **CUANDO** Dirección abre el panel
- **ENTONCES** el indicador muestra 12.000 USD ejecutados y 28.000 USD disponibles, sin contar como disponibles los 10.000 USD comprometidos

##### Escenario 9: Reconexión tras perder la actualización en tiempo real

- **DADO QUE** el panel de Dirección perdió la conexión en tiempo real mientras ocurrían desembolsos y cambios de estatus
- **CUANDO** la conexión se restablece
- **ENTONCES** el panel muestra los valores reales al momento de la reconexión, no los que tenía antes de perder la conexión

##### Escenario 10: Panel sin ciclos abiertos

- **DADO QUE** no hay ciclos de postulación abiertos
- **CUANDO** Dirección abre el panel
- **ENTONCES** el indicador de postulantes activos por ciclo indica que no hay ciclos

##### Escenario 11: El panel no muestra datos socioeconómicos individuales

- **DADO QUE** "Becas Ingeniería" tiene becarios con ingreso familiar declarado en su perfil
- **CUANDO** Dirección abre el panel
- **ENTONCES** los indicadores muestran solo valores agregados y no muestran el ingreso familiar ni la composición familiar de ningún becario

##### Escenario 12: Las ofertas pendientes de aceptación no cuentan como disponibles

- **DADO QUE** "Becas Ingeniería" tiene 50.000 USD de presupuesto, 12.000 USD desembolsados, 10.000 USD comprometidos en renovaciones pendientes y una oferta de 2.000 USD pendiente de aceptación (HU-15.1)
- **CUANDO** Dirección abre el panel
- **ENTONCES** el indicador muestra 12.000 USD ejecutados y 26.000 USD disponibles, sin contar como disponibles los 10.000 USD comprometidos ni los 2.000 USD de la oferta pendiente

##### Escenario 13: La oferta rechazada definitivamente vuelve a estar disponible

- **DADO QUE** el panel muestra 26.000 USD disponibles de "Becas Ingeniería", con una oferta de 2.000 USD pendiente de aceptación
- **CUANDO** esa oferta se rechaza definitivamente (HU-16.1)
- **ENTONCES** el panel muestra 28.000 USD disponibles sin recargar la página

##### Escenario 14: Una beca *Inactiva* no libera presupuesto ni cupo

- **DADO QUE** el panel muestra 28.000 USD disponibles de "Becas Ingeniería" y la beca de Marta, de 2.000 USD, está *Activa* dentro de ese presupuesto
- **CUANDO** la beca de Marta pasa a *Inactiva* por renovación rechazada (HU-38.2)
- **ENTONCES** el panel sigue mostrando 28.000 USD disponibles, sin sumar los 2.000 USD de Marta como liberados, a diferencia de cuando se revoca una beca (Escenario 4)

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| El panel muestra los cuatro indicadores (RF-40) | Regla | 1 | CUBIERTA (el valor de tres de ellos depende de su fórmula: pregunta abierta) |
| Los indicadores se actualizan por evento, sin recargar la página (RF-40) | Regla | 4, 5, 6, 13 | CUBIERTA |
| El panel refleja los cambios al instante (RNF tiempo real) | Regla | 5, 6 | CUBIERTA |
| Consultar el panel no compite por recursos con la operación diaria (RF-40) | Regla | 3 | CUBIERTA |
| Acceso limitado a dirección (RF-40, RF-37) | Regla | 7 | CUBIERTA |
| Presupuesto disponible excluye lo comprometido en renovaciones pendientes (RF-12) | Regla | 8 | CUBIERTA (compartida con HU-12.2 y HU-23.1, Esc. 7) |
| Las ofertas pendientes cuentan como no disponibles hasta el rechazo definitivo | Regla | 12, 13 | CUBIERTA (compartida con HU-12.2 y HU-23.1) |
| Presupuesto y cupo liberados al revocar una beca (RF-22) | Regla | 4 | CUBIERTA (el cupo liberado, en HU-22.2) |
| El panel refleja el paso a *Inactiva* como cambio de estatus, sin liberar presupuesto ni cupo | Regla | 14 | CUBIERTA |
| Agregados actualizados por eventos, no consultas transaccionales por apertura | Regla técnica | 3 | CUBIERTA |
| Incrementos atómicos: dos eventos simultáneos suman ambos | Regla técnica | 6 | CUBIERTA |
| Reconciliación del panel al reconectar | Regla técnica | 9 | CUBIERTA |
| No se expone información sensible individual; solo datos agregados (RNF confidencialidad, RF-33) | Regla | 11 | CUBIERTA |
| Agregación que evita inferir el dato individual con grupos pequeños de becarios | Regla | — | PENDIENTE DE DEFINICIÓN (pregunta abierta de la historia) |
| FA-1 Sin ciclos abiertos | FA | 10 | CUBIERTA |
| FA-2 Varios eventos seguidos o simultáneos: ninguno se pierde | FA | 6 | CUBIERTA |
| FA-3 Rol distinto de dirección intenta abrir el panel | FA | 7 | CUBIERTA |
| FA-4 Se interrumpe el canal en tiempo real; reconcilia al reconectar | FA | 9 | CUBIERTA |
| FA-5 Oferta pendiente rechazada definitivamente | FA | 13 | CUBIERTA |
| Fórmulas de postulantes activos, tasa de aprobación y riesgo de incumplimiento | Regla | — | PENDIENTE DE DEFINICIÓN (preguntas abiertas de la historia) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-40.1).*
