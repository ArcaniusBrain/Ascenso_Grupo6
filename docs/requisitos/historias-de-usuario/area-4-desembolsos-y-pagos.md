# Área 4 — Desembolsos y Pagos

> Parte de las [Historias de Usuario · Plan B · versión final](README.md). Reglas técnicas en el [Anexo A](anexo-a-reglas-tecnicas.md); preguntas abiertas en el [Anexo B](anexo-b-preguntas-abiertas.md).

## RF-19 — Programación de desembolsos por becario · *Debe tener*

### HU-19.1 Generar el calendario de desembolsos de un becario sin superar lo aprobado

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-19 | Debe tener | HU-11.1, HU-16.1, HU-17.2, HU-38.1, HU-37.1 | HU-19.1, HU-19.2 |

#### Narrativa

- Como administrador financiero
- Quiero definir el calendario de desembolsos de cada becario sin que lo programado en un periodo supere lo aprobado en su oferta
- Para saber cuándo, cuánto y a quién pagar, sin pagar más de lo que se le otorgó

#### Reglas de negocio

- Para cada becario activo se define su calendario de desembolsos: mensual, por semestre o pago directo a la universidad (RF-19). El calendario se define para becas *Activa*: la beca pasa de *Otorgada* a *Activa* al firmarse el compromiso (HU-17.2) o, si el programa no lo exige, al aceptarse la oferta (HU-16.1) (S10).
- Todas las becas son un **monto fijo por periodo**; no existen las coberturas total ni parcial como forma de calcular el monto. En una beca nueva el monto lo fija el programa; en una renovación el monto está asignado a cada estudiante (RF-19; ver HU-11.1 y HU-26.x).
- La modalidad de pago (mensual, por semestre o pago directo a la universidad) la fija el programa; no se elige por becario (RF-19; ver HU-11.1).
- El total desembolsado a un becario en un periodo no puede superar lo aprobado en su oferta; por eso, el total programado para un periodo tampoco puede superarlo (RF-19).
- Si el programa exige compromiso, los desembolsos no inician sin él (RF-17; ver HU-17.2).
- En el pago directo a la universidad, el destinatario es la universidad del becario (RF-19, RF-20; ver HU-14.1).
- Una beca *Suspendida* tiene sus desembolsos en pausa (RF-21; ver HU-21.1) y una beca *Revocada* o *Concluida* no recibe más desembolsos (RF-22, regla transversal de estados finales; ver HU-22.2): en esos estados el calendario no se define ni se modifica.
- Una beca *Inactiva* no recibe dinero en el periodo siguiente: no se le define ni se modifica el calendario de desembolsos de ese periodo (C3; ver HU-26.2 y HU-27.2).
- El becario puede ver el estado de **sus** desembolsos, y solo los suyos (Actores: Becario; RNF de confidencialidad y de protección de datos personales).
- Solo el personal con función financiera define o edita el calendario; el alcance de cada rol se resuelve en la gestión de usuarios y permisos (RF-37; ver HU-37.1). El acceso de estos roles exige además verificación en dos pasos al iniciar sesión (Restricción del cliente; ver Área 7).
- Los periodos académicos a los que se liga el calendario ("2026-2") existen como configuración institucional, y sus fechas las define cada universidad: el periodo aplicable a un becario es el de su universidad (RF-38, C4; ver HU-38.1).

#### Flujo de la HU

1. El administrador financiero abre el becario y elige "Calendario de desembolsos".
2. El sistema verifica que la beca esté *Activa* y muestra el monto fijo del periodo y lo aprobado en la oferta.
3. El sistema muestra la modalidad fijada por el programa: mensual, por semestre o pago directo a la universidad.
4. Registra la fecha y el monto de cada desembolso del periodo.
5. Al guardar o editar el calendario, el sistema suma —en una sola transacción— lo programado para cada periodo y lo compara con lo aprobado en la oferta para ese periodo.
6. Si no lo supera, guarda el calendario; si lo supera, no guarda y muestra ambos montos.
7. El calendario queda visible para el becario titular de la beca.

#### Flujos alternos y de error

- FA-1 (paso 2) Programa sin monto del periodo ni modalidad definidos: el sistema no permite continuar y muestra "El programa no tiene monto ni modalidad de pago definidos." (HU-11.1).
- FA-2 (paso 4) Monto del periodo: el sistema muestra el monto fijo del periodo —fijado por el programa en una beca nueva, o el asignado al estudiante en una renovación— como total disponible para repartir entre los desembolsos de ese periodo.
- FA-3 (paso 1) Beca todavía *Otorgada*: el sistema no permite definir el calendario y muestra "La beca de este becario aún no está Activa."
- FA-4 (paso 6) Edición que supera lo aprobado: el sistema rechaza el cambio y conserva el calendario anterior.
- FA-5 (paso 1) Beca *Suspendida*: el sistema no permite definir ni editar el calendario y muestra "La beca está suspendida. Sus desembolsos están en pausa."
- FA-6 (paso 1) Beca *Revocada*, *Concluida* o *Inactiva*: el sistema no permite definir ni editar el calendario y muestra el estado de la beca como motivo (RF-22; C3).
- FA-7 (paso 1) Becario consultando una beca ajena: el sistema niega el acceso; un becario solo abre su propio calendario.
- FA-8 (paso 5) Edición simultánea del mismo calendario: el sistema conserva una sola versión del calendario, no aplica dos ediciones contradictorias y avisa al segundo administrador de que los datos cambiaron antes de que reintente.
- FA-9 (paso 1) Usuario sin función financiera: el sistema no ofrece la opción "Calendario de desembolsos" (RF-37; ver HU-37.1).

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-19.1).*

#### Criterios de aceptación (13 escenarios)

##### Escenario 1: Calendario mensual con el monto del periodo

- **DADO QUE** Luis es becario activo de "Becas Arte", cuyo programa fija un monto de 1.800 para el periodo 2026-2 y la modalidad mensual
- **CUANDO** el administrador registra las seis fechas mensuales del periodo 2026-2 con 300 en cada una
- **ENTONCES** el calendario muestra cada desembolso de 300 en su fecha, con Luis como destinatario, y el total programado de 1.800 coincide con el monto del periodo

##### Escenario 2: Pago directo a la universidad igual a lo aprobado

- **DADO QUE** María es becaria activa de "Becas Ingeniería", estudia en UCAB y su oferta aprueba 1.200 para 2026-2
- **CUANDO** el administrador elige pago directo a la universidad y registra un desembolso de 1.200 para 2026-2
- **ENTONCES** el calendario se guarda y muestra ese desembolso con UCAB como destinataria

##### Escenario 3: Beca todavía Otorgada

- **DADO QUE** la beca de María en "Becas Ingeniería" sigue *Otorgada* porque no ha firmado el compromiso que exige el programa
- **CUANDO** el administrador abre su calendario de desembolsos
- **ENTONCES** el sistema no permite definirlo y muestra "La beca de este becario aún no está Activa."

##### Escenario 4: El becario ve su calendario

- **DADO QUE** el administrador guardó el calendario de Luis
- **CUANDO** Luis consulta sus desembolsos
- **ENTONCES** ve las fechas y montos programados

##### Escenario 5: Supera lo aprobado

- **DADO QUE** la oferta de María aprueba 1.200 para 2026-2
- **CUANDO** el administrador programa 1.500 para 2026-2
- **ENTONCES** el sistema no guarda y muestra "El total programado para 2026-2 (1.500) supera lo aprobado en la oferta (1.200)."

##### Escenario 6: Edición que supera lo aprobado

- **DADO QUE** el calendario de María tiene 1.200 programados para 2026-2, igual a lo aprobado
- **CUANDO** el administrador agrega un desembolso de 100 en 2026-2
- **ENTONCES** el sistema rechaza el cambio y el calendario sigue con 1.200

##### Escenario 7: Beca suspendida no permite editar el calendario

- **DADO QUE** la beca de Pedro está *Suspendida*
- **CUANDO** el administrador financiero intenta editar su calendario de desembolsos
- **ENTONCES** el sistema no aplica el cambio y muestra "La beca está suspendida. Sus desembolsos están en pausa."

##### Escenario 8: Beca revocada no permite editar el calendario

- **DADO QUE** la beca de Juan está *Revocada*
- **CUANDO** el administrador financiero intenta definir o editar su calendario
- **ENTONCES** el sistema no lo permite y muestra que la beca está *Revocada* y no recibe más desembolsos

##### Escenario 9: Un becario no ve el calendario de otro becario

- **DADO QUE** Luis y María son becarios distintos del mismo programa
- **CUANDO** Luis intenta consultar el calendario de desembolsos de María
- **ENTONCES** el sistema le niega el acceso y Luis solo ve su propio calendario

##### Escenario 10: Programa sin monto ni modalidad definidos

- **DADO QUE** el programa "Becas Nuevas" no tiene configurados el monto del periodo ni la modalidad de pago
- **CUANDO** el administrador financiero abre el calendario de desembolsos de un becario de ese programa
- **ENTONCES** el sistema no permite continuar y muestra "El programa no tiene monto ni modalidad de pago definidos."

##### Escenario 11: Beca concluida no permite editar el calendario

- **DADO QUE** la beca de Ana está *Concluida*
- **CUANDO** el administrador financiero intenta definir o editar su calendario de desembolsos
- **ENTONCES** el sistema no lo permite y muestra que la beca está *Concluida* y no recibe más desembolsos

##### Escenario 12: Edición simultánea del mismo calendario

- **DADO QUE** dos administradores financieros abren al mismo tiempo el calendario de María, con 1.200 programados para 2026-2
- **CUANDO** ambos guardan cambios distintos casi al mismo tiempo
- **ENTONCES** el calendario queda con una sola versión, no se aplican dos ediciones contradictorias y el segundo administrador ve el aviso de que los datos cambiaron antes de reintentar

##### Escenario 13: Beca inactiva no recibe calendario del periodo siguiente

- **DADO QUE** la beca de Rosa quedó *Inactiva* porque no se renovó para 2027-1
- **CUANDO** el administrador financiero intenta definir su calendario de desembolsos de 2027-1
- **ENTONCES** el sistema no lo permite y muestra que la beca está *Inactiva* y no recibe dinero en ese periodo

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Calendario solo se define/edita con beca *Activa* | Regla | 1, 2, 3, 7, 8, 11, 13 | CUBIERTA |
| Monto fijo por periodo (programa en beca nueva, estudiante en renovación) y modalidad fijada por el programa (FA-2) | Regla / FA-2 | 1, 2, 10 | CUBIERTA |
| Total programado por periodo no supera lo aprobado en la oferta | Regla | 5, 6 | CUBIERTA |
| Edición que supera lo aprobado conserva el calendario anterior (FA-4) | Regla / FA-4 | 6 | CUBIERTA |
| Pago directo a la universidad → destinatario es la universidad | Regla | 2 | CUBIERTA |
| Beca *Suspendida* → calendario no se define ni edita (FA-5) | Regla / FA-5 | 7 | CUBIERTA |
| Beca *Revocada* → calendario no se define ni edita (FA-6) | Regla / FA-6 | 8 | CUBIERTA |
| Beca *Concluida* → calendario no se define ni edita (FA-6) | Regla / FA-6 | 11 | CUBIERTA |
| Beca *Inactiva* → no recibe dinero en el periodo siguiente, el calendario no se define ni edita (FA-6) | Regla / FA-6 | 13 | CUBIERTA |
| Becario ve solo su propio calendario (FA-7) | Regla / FA-7 | 4, 9 | CUBIERTA |
| FA-1: Programa sin monto del periodo ni modalidad definidos | FA | 10 | CUBIERTA |
| FA-3: Beca aún *Otorgada* | FA | 3 | CUBIERTA |
| FA-8: Edición simultánea (consistencia bajo concurrencia) | FA / Técnica | 12 | CUBIERTA |
| Si el programa exige compromiso, los desembolsos no inician sin él | Regla | 3 (contexto) | DELEGADA (HU-20.1 Esc. 8; HU-17.2 delega aquí el rechazo del desembolso) |
| Solo personal financiero define/edita el calendario (FA-9) | Regla / FA | — | DELEGADA (HU-37.1) |
| Verificación en dos pasos del rol financiero | Restricción | — | DELEGADA (Área 7) |
| Periodos académicos existen como configuración institucional, con fechas por universidad | Regla | — | DELEGADA (HU-38.1, HU-38.2) |
| Técnica — estacionalidad / ráfagas de escritura | Técnica | — | NO VERIFICABLE por escenario (prueba de carga; sin umbral en el PRD) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-19.1).*

## RF-20 — Registro y confirmación de desembolsos realizados · *Debe tener*

### HU-20.1 Registrar y confirmar un desembolso con un código de un solo uso

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-20 | Debe tener | HU-19.1, HU-17.2, HU-29.1, HU-35.1, HU-37.1 | HU-20.1, HU-20.2 |

#### Narrativa

- Como administrador financiero
- Quiero registrar un desembolso realizado con su fecha, monto, método y destinatario, y confirmarlo con un código de un solo uso
- Para llevar el control de lo que efectivamente se pagó y que ningún pago se confirme sin verificar mi identidad ni sin respetar el presupuesto

#### Reglas de negocio

- El equipo financiero registra cuándo se hizo un desembolso: fecha, monto, método y destinatario (RF-20).
- Se distingue si el pago fue directo al becario o a su universidad (RF-20).
- La plataforma solo registra el desembolso; la transferencia se hace por los canales financieros actuales de la Fundación (Fuera de alcance: procesamiento de pagos).
- Confirmar un desembolso requiere un código de verificación de un solo uso (OTP) enviado en el momento (Restricción del cliente; RF-20).
- Cada código sirve una sola vez (Restricción del cliente).
- El código se envía por correo o por SMS, según el canal que elija el propio usuario (Restricción del cliente).
- El código tiene una vigencia de 15 minutos desde su envío; pasado ese tiempo expira y no confirma nada.
- Se permiten 3 intentos fallidos de código; al tercero, la operación queda bloqueada y el usuario debe esperar 1 hora antes de volver a intentarla.
- El usuario puede solicitar el reenvío del código.
- Si el programa exige compromiso y no está registrado, el desembolso no se registra (RF-17; ver HU-17.2).
- Lo desembolsado a un becario en un periodo no supera lo aprobado en su oferta, que es el monto fijo de ese periodo (RF-19; C1).
- Ningún desembolso puede exceder el presupuesto aprobado del programa (regla transversal).
- No se confirma un desembolso de una beca *Revocada*, ni de una beca *Suspendida* mientras sus desembolsos están en pausa (RF-21, RF-22; RNF de consistencia bajo concurrencia; ver HU-21.1 y HU-22.1).
- No se confirman desembolsos del periodo siguiente de una beca *Inactiva*: esa beca no recibe dinero en ese periodo (C3; ver HU-19.1, HU-26.2 y HU-27.2).
- Lo desembolsado antes de la revocación no se reclama automáticamente (RF-22).
- Un mismo desembolso programado se confirma una sola vez (RF-20; RNF de consistencia bajo concurrencia).
- El becario ve el estado de **sus** desembolsos, y solo los suyos (Actores: Becario; RNF de confidencialidad).
- Solo el personal con función financiera registra y confirma desembolsos; el alcance de cada rol se resuelve en RF-37 (ver HU-37.1), y su acceso exige verificación en dos pasos al iniciar sesión (Restricción del cliente; ver Área 7).

#### Flujo de la HU

1. El administrador abre el calendario del becario y elige un desembolso programado.
2. Ingresa la fecha, el monto, el método y el destinatario (becario o universidad).
3. El sistema valida los datos, el compromiso y lo aprobado para el periodo.
4. El sistema guarda el desembolso como registrado, pendiente de confirmar.
5. El administrador elige "Confirmar" y el sistema envía en ese momento un código de un solo uso por el canal que el administrador tiene elegido (correo o SMS), con 15 minutos de vigencia.
6. El administrador ingresa el código.
7. El sistema valida, en una misma transacción, el código y su vigencia, el presupuesto del programa, el estado de la beca y que el desembolso no esté ya confirmado.
8. El sistema confirma el desembolso y lo considera pagado, salvo que luego se marque fallido o devuelto (HU-20.2).
9. El becario recibe la notificación de desembolso realizado (HU-29.1).

#### Flujos alternos y de error

- FA-1 (paso 3) Falta un dato: el sistema no guarda y señala el campo faltante; si falta el método, muestra "Indica el método del desembolso."
- FA-2 (paso 3) Supera lo aprobado: el sistema no guarda y muestra lo aprobado y lo ya desembolsado en el periodo.
- FA-3 (paso 3) Compromiso no firmado: el sistema no guarda y muestra "El becario no ha firmado el compromiso que exige el programa."
- FA-4 (paso 7) Código incorrecto, ya usado o expirado: el sistema no confirma y muestra "El código no es válido. No se confirmó el desembolso."
- FA-5 (paso 7) Excede el presupuesto, o beca *Revocada*, *Suspendida* o *Inactiva*: el sistema no confirma y muestra el motivo o el estado de la beca.
- FA-6 (paso 7) Desembolso ya confirmado: el sistema no vuelve a confirmarlo ni genera un segundo pago y muestra "Este desembolso ya fue confirmado."
- FA-7 (paso 1) Usuario sin función financiera: el sistema no ofrece registrar ni confirmar desembolsos (RF-37; ver HU-37.1).
- FA-8 (consulta del becario) Acceso a desembolsos ajenos: el becario solo ve los suyos; el sistema niega cualquier acceso a los de otro becario.
- FA-9 (paso 6) Tercer intento fallido: el sistema bloquea la confirmación de ese desembolso y no acepta nuevos códigos hasta que pase 1 hora.
- FA-10 (paso 6) Reenvío del código: el administrador puede solicitar un código nuevo por su canal elegido.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-20.1).*

#### Criterios de aceptación (14 escenarios)

##### Escenario 1: Pago directo al becario

- **DADO QUE** Luis tiene programado un desembolso de 300 el 01/10/2026
- **CUANDO** el administrador registra fecha 01/10/2026, monto 300, método "transferencia bancaria" y destinatario "becario"
- **ENTONCES** el desembolso queda registrado, pendiente de confirmar, con Luis como destinatario

##### Escenario 2: Pago a la universidad confirmado con código válido

- **DADO QUE** María tiene programado un pago directo a UCAB y "Becas Ingeniería" tiene presupuesto
- **CUANDO** el administrador lo registra con destinatario "universidad" y lo confirma con el código que recibió
- **ENTONCES** el desembolso queda confirmado y pagado, con UCAB como destinataria, y se distingue del pago directo al becario

##### Escenario 3: Supera lo aprobado

- **DADO QUE** la oferta de María aprueba 1.200 para 2026-2 y ya se le desembolsaron 1.200
- **CUANDO** el administrador registra otro desembolso de 100 en 2026-2
- **ENTONCES** el sistema no lo registra y muestra "Este desembolso supera lo aprobado en la oferta para 2026-2 (1.200)."

##### Escenario 4: Código reutilizado

- **DADO QUE** el administrador ya usó el código 482913 para confirmar otro desembolso
- **CUANDO** ingresa 482913 para confirmar el desembolso de Luis
- **ENTONCES** el sistema no confirma y muestra "El código no es válido. No se confirmó el desembolso."

##### Escenario 5: Excede el presupuesto

- **DADO QUE** "Becas Arte" tiene presupuesto de 50.000 y ya desembolsó 49.500
- **CUANDO** el administrador confirma un desembolso de 800 con un código válido
- **ENTONCES** el sistema no confirma y muestra "El desembolso excede el presupuesto aprobado del programa."

##### Escenario 6: Revocación al mismo tiempo

- **DADO QUE** el coordinador revoca la beca de Luis (HU-22.1) mientras el administrador confirma un desembolso de esa beca
- **CUANDO** la revocación queda registrada primero
- **ENTONCES** el desembolso no se confirma, el administrador ve "La beca fue revocada. No se puede confirmar el desembolso." y la beca no queda a la vez *Revocada* y con ese desembolso pagado

##### Escenario 7: Confirmación doble simultánea

- **DADO QUE** dos administradores financieros abren el mismo desembolso pendiente de confirmar
- **CUANDO** ambos ingresan un código válido casi al mismo tiempo
- **ENTONCES** solo la primera confirmación deja el desembolso pagado y la segunda se rechaza con "Este desembolso ya fue confirmado.", sin generar un doble pago

##### Escenario 8: Compromiso exigido y no firmado

- **DADO QUE** "Becas Ingeniería" exige compromiso firmado y María no lo ha firmado (HU-17.2)
- **CUANDO** el administrador intenta registrar un desembolso de su beca
- **ENTONCES** el sistema no lo registra y muestra "El becario no ha firmado el compromiso que exige el programa."

##### Escenario 9: Un becario no ve los desembolsos de otro

- **DADO QUE** Luis y María son becarios del mismo programa
- **CUANDO** Luis consulta la lista de sus desembolsos
- **ENTONCES** solo ve los propios, nunca los de María

##### Escenario 10: Falta el método del desembolso

- **DADO QUE** el administrador financiero está registrando un desembolso para Luis
- **CUANDO** guarda sin indicar el método del desembolso
- **ENTONCES** el sistema no guarda y muestra "Indica el método del desembolso."

##### Escenario 11: Beca suspendida impide confirmar el desembolso

- **DADO QUE** la beca de Pedro está *Suspendida* y tiene un desembolso pendiente de confirmar
- **CUANDO** el administrador financiero intenta confirmarlo con un código válido
- **ENTONCES** el sistema no confirma el desembolso y muestra "La beca está suspendida. Sus desembolsos están en pausa."

##### Escenario 12: Código expirado

- **DADO QUE** el administrador financiero recibió el código para confirmar el desembolso de Luis y han pasado 16 minutos desde su envío
- **CUANDO** ingresa ese código
- **ENTONCES** el sistema no confirma el desembolso y muestra "El código no es válido. No se confirmó el desembolso."

##### Escenario 13: Tercer intento fallido del código

- **DADO QUE** el administrador financiero ya erró dos veces el código del desembolso de Luis
- **CUANDO** ingresa un tercer código incorrecto
- **ENTONCES** el sistema no confirma el desembolso, bloquea la confirmación y no acepta nuevos códigos hasta que pase 1 hora

##### Escenario 14: Beca inactiva impide confirmar el desembolso

- **DADO QUE** la beca de Rosa quedó *Inactiva* para 2027-1 y tiene un desembolso de ese periodo pendiente de confirmar
- **CUANDO** el administrador financiero intenta confirmarlo con un código válido
- **ENTONCES** el sistema no confirma el desembolso y muestra que la beca está *Inactiva* y no recibe dinero en ese periodo

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Registro de fecha, monto, método y destinatario | Regla | 1, 2 | CUBIERTA |
| Se distingue pago directo al becario vs. a la universidad | Regla | 1, 2 | CUBIERTA |
| Confirmar requiere OTP enviado en el momento | Regla | 2, 4, 7 | CUBIERTA |
| Cada código sirve una sola vez | Regla | 4 | CUBIERTA |
| Vigencia del código: 15 minutos | Regla | 12 | CUBIERTA |
| 3 intentos fallidos y después espera de 1 hora | Regla | 13 | CUBIERTA |
| Canal del código: correo o SMS, a elección del usuario | Regla | — | DELEGADA (Área 7: elección y gestión del canal del usuario) |
| Reenvío del código permitido (FA-10) | Regla / FA-10 | — | CUBIERTA solo en que se permite; su efecto sobre el código anterior sigue por confirmar (ver Preguntas abiertas) |
| Solo se registra el pago; la transferencia va por los canales de la Fundación | Alcance | 1, 2 (el estado es "registrado"/"pagado", sin ejecución bancaria) | CUBIERTA |
| Si falta el compromiso exigido, no se registra el desembolso (FA-3) | Regla / FA-3 | 8 | CUBIERTA |
| No supera lo aprobado en la oferta para el periodo (FA-2) | Regla / FA-2 | 3 | CUBIERTA |
| No excede el presupuesto del programa (FA-5) | Regla / FA-5 | 5 | CUBIERTA |
| Beca *Revocada* bloquea la confirmación (FA-5) | Regla / FA-5 | 6 | CUBIERTA |
| Beca *Suspendida* bloquea la confirmación (FA-5) | Regla / FA-5 | 11 | CUBIERTA |
| Beca *Inactiva* bloquea la confirmación del periodo siguiente (FA-5) | Regla / FA-5 | 14 | CUBIERTA |
| Un desembolso programado se confirma una sola vez, sin doble pago (FA-6) | Regla / FA-6 | 7 | CUBIERTA |
| Lo desembolsado antes de la revocación no se reclama | Regla | — | DELEGADA (HU-22.2 Esc. 2; HU-22.1 Esc. 6) |
| Becario ve solo sus propios desembolsos (FA-8) | Regla / FA-8 | 9 | CUBIERTA |
| FA-1: Falta un dato obligatorio | FA | 10 | CUBIERTA |
| FA-4: Código incorrecto, ya usado o expirado | FA | 4, 12 | CUBIERTA |
| FA-9: Tercer intento fallido | FA | 13 | CUBIERTA |
| Solo personal financiero registra y confirma (FA-7) | Regla / FA | — | DELEGADA (HU-37.1) |
| Verificación en dos pasos del rol financiero | Restricción | — | DELEGADA (Área 7) |
| Técnica — trazabilidad append-only del registro y la confirmación | Técnica | 6 (orden real de eventos) | DELEGADA (HU-35.1) |
| Técnica — notificación asíncrona al becario | Técnica | — | DELEGADA (HU-29.1) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-20.1).*

### HU-20.2 Resolver un desembolso fallido o devuelto

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-20 | Debe tener | HU-19.1, HU-20.1, HU-35.1, HU-37.1 | HU-20.3, HU-20.4 |

#### Narrativa

- Como administrador financiero
- Quiero marcar un desembolso como fallido o devuelto y registrar cómo se resolvió, confirmando el nuevo pago con un código de un solo uso
- Para que ningún desembolso fallido o devuelto se cuente como pagado mientras no se resuelva

#### Reglas de negocio

- Un desembolso fallido o devuelto queda marcado y debe resolverse antes de considerarse pagado; mientras esté marcado, no se considera pagado (RF-20).
- Si la resolución implica volver a pagar, se confirma con código de un solo uso, como todo desembolso (Restricción del cliente; ver HU-20.1).
- Cada código sirve una sola vez (Restricción del cliente).
- El código llega por el canal elegido por el usuario (correo o SMS), vale 15 minutos y admite 3 intentos fallidos antes de una espera de 1 hora; se puede solicitar su reenvío (mismos parámetros del servicio único de verificación; ver HU-20.1).
- El nuevo pago de una resolución está sujeto a los mismos límites que cualquier desembolso: no supera lo aprobado en la oferta para el periodo (RF-19) ni el presupuesto aprobado del programa (regla transversal).
- Un desembolso ya marcado como fallido o devuelto no se vuelve a marcar: la marca original conserva quién y cuándo la hizo (RF-20; RNF de trazabilidad).
- No se genera un nuevo pago sobre una beca *Revocada* o *Concluida*: la revocación es definitiva y esos estados son finales (RF-22; regla transversal).
- La marca queda registrada con quién y cuándo la hizo, y la resolución, con quién, cuándo y cómo se resolvió (RNF de trazabilidad; RF-35, ver HU-35.1).
- El becario ve que su desembolso está fallido o devuelto y no pagado (Actores: Becario; RF-20).
- Solo el personal con función financiera marca y resuelve desembolsos (RF-37; ver HU-37.1), con verificación en dos pasos al iniciar sesión (Restricción del cliente; ver Área 7).
- **Sin definir en el PRD:** qué formas de "resolver" un desembolso fallido o devuelto existen además de volver a pagarlo (por ejemplo, anularlo y devolver el monto al disponible del programa). Esta HU cubre solo el nuevo pago; la duda pasa a Preguntas abiertas.
- **Decisión del cliente:** *Inactiva* no es un estado final; la beca puede volver a *Activa* o pasar a *Concluida*. Sigue por confirmar si, mientras la beca está *Inactiva*, puede resolverse con un nuevo pago un desembolso ya vencido de un periodo anterior, o si hay que esperar a que la beca cambie de estado; la duda pasa a Preguntas abiertas.

#### Flujo de la HU

1. El administrador abre un desembolso confirmado, elige "Marcar como fallido o devuelto" e indica si fue fallido o devuelto.
2. El sistema guarda la marca con quién y cuándo; el desembolso aparece como fallido o devuelto, no pagado, también para el becario.
3. Para resolverlo, el administrador abre el desembolso marcado, elige "Resolver" y describe cómo se resolvió.
4. Registra la nueva fecha, método y destinatario del pago.
5. El sistema valida el estado de la beca y lo aprobado para el periodo.
6. El administrador confirma el pago con un código de un solo uso, enviado por su canal y válido 15 minutos (HU-20.1).
7. El sistema quita la marca y considera pagado el desembolso; la resolución queda registrada.

#### Flujos alternos y de error

- FA-1 (paso 1) Sin indicar el tipo: el sistema no guarda y muestra "Indica si el desembolso fue fallido o devuelto."
- FA-2 (paso 3) Sin descripción: el sistema no guarda y muestra "Describe cómo se resolvió el desembolso."
- FA-3 (paso 6) Código incorrecto, ya usado o expirado: el sistema muestra "El código no es válido. No se confirmó el desembolso." y el desembolso sigue marcado como fallido o devuelto.
- FA-4 (paso 1) Doble marca: si el desembolso ya está marcado, el sistema no duplica la marca y muestra "Este desembolso ya está marcado como fallido o devuelto."
- FA-5 (paso 5) Resolución que supera lo aprobado: el sistema no registra el nuevo pago y muestra lo aprobado para el periodo.
- FA-6 (paso 3) Beca *Revocada* o *Concluida*: el sistema no permite resolver el desembolso y muestra el estado de la beca.
- FA-7 (paso 1) Usuario sin función financiera: el sistema no ofrece "Marcar como fallido o devuelto" ni "Resolver" (RF-37; ver HU-37.1).
- FA-8 (paso 6) Tercer intento fallido del código: el sistema bloquea la confirmación de la resolución hasta que pase 1 hora, y el desembolso sigue marcado.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-20.2).*

#### Criterios de aceptación (9 escenarios)

##### Escenario 1: Desembolso fallido

- **DADO QUE** el desembolso de 300 a Luis del 01/10/2026 está confirmado
- **CUANDO** el administrador lo marca como fallido
- **ENTONCES** el desembolso aparece como fallido, no pagado, con el nombre del administrador y la fecha de la marca, y Luis también lo ve como fallido al consultar sus desembolsos

##### Escenario 2: Resolución confirmada

- **DADO QUE** el desembolso de 300 a Luis está marcado como fallido
- **CUANDO** el administrador describe "Se corrigió la cuenta y se transfirió de nuevo", registra el nuevo pago y lo confirma con un código válido
- **ENTONCES** el desembolso aparece como pagado y la resolución queda registrada con su descripción, autor y fecha

##### Escenario 3: Sin descripción

- **DADO QUE** el administrador eligió "Resolver"
- **CUANDO** guarda sin describir la resolución
- **ENTONCES** el sistema muestra "Describe cómo se resolvió el desembolso."

##### Escenario 4: Doble marca de fallido

- **DADO QUE** el desembolso de Luis ya está marcado como fallido
- **CUANDO** el administrador intenta marcarlo como fallido otra vez
- **ENTONCES** el sistema muestra "Este desembolso ya está marcado como fallido o devuelto." y conserva el autor y la fecha de la marca original

##### Escenario 5: Resolución que supera lo aprobado

- **DADO QUE** la oferta de María aprueba 1.200 para 2026-2, ya se le desembolsaron 1.200 y uno de esos desembolsos está marcado como fallido
- **CUANDO** el administrador lo resuelve registrando un pago adicional que suma más de 1.200 para ese periodo
- **ENTONCES** el sistema no lo registra y muestra "Este desembolso supera lo aprobado en la oferta para 2026-2 (1.200)."

##### Escenario 6: Resolución sobre una beca revocada

- **DADO QUE** la beca de Juan está *Revocada* y tenía un desembolso marcado como devuelto
- **CUANDO** el administrador intenta resolverlo con un nuevo pago
- **ENTONCES** el sistema no lo permite y muestra "La beca fue revocada. No se puede confirmar el desembolso."

##### Escenario 7: Falta indicar el tipo de marca

- **DADO QUE** el administrador financiero abre el desembolso confirmado de 300 a Luis del 01/10/2026
- **CUANDO** elige "Marcar como fallido o devuelto" y guarda sin indicar si fue fallido o devuelto
- **ENTONCES** el sistema no guarda y muestra "Indica si el desembolso fue fallido o devuelto."

##### Escenario 8: Beca concluida no permite resolver el desembolso

- **DADO QUE** la beca de Ana está *Concluida* y tenía un desembolso marcado como fallido
- **CUANDO** el administrador financiero intenta resolverlo con un nuevo pago
- **ENTONCES** el sistema no lo permite y muestra que la beca está *Concluida*

##### Escenario 9: Código inválido al confirmar la resolución

- **DADO QUE** el desembolso de 300 a Luis está marcado como fallido y el administrador registró el nuevo pago
- **CUANDO** ingresa un código que no coincide con el enviado
- **ENTONCES** el sistema muestra "El código no es válido. No se confirmó el desembolso." y el desembolso sigue marcado como fallido y no pagado

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Desembolso fallido/devuelto no cuenta como pagado hasta resolverse | Regla | 1, 9 | CUBIERTA |
| Resolución con nuevo pago requiere OTP | Regla | 2, 9 | CUBIERTA |
| Nuevo pago no supera lo aprobado en la oferta del periodo (FA-5) | Regla / FA-5 | 5 | CUBIERTA |
| Nuevo pago no excede el presupuesto del programa | Regla | — | DELEGADA (HU-20.1 Esc. 5) |
| Un desembolso ya marcado no se vuelve a marcar (conserva autor/fecha original) (FA-4) | Regla / FA-4 | 4 | CUBIERTA |
| No se genera nuevo pago sobre beca *Revocada* (FA-6) | Regla / FA-6 | 6 | CUBIERTA |
| No se genera nuevo pago sobre beca *Concluida* (FA-6) | Regla / FA-6 | 8 | CUBIERTA |
| Marca y resolución registradas con quién, cuándo y cómo | Regla | 1, 2, 4 | CUBIERTA |
| Becario ve su desembolso como fallido/devuelto y no pagado | Regla | 1 | CUBIERTA |
| FA-1: Sin indicar si fue fallido o devuelto | FA | 7 | CUBIERTA |
| FA-2: Sin descripción de la resolución | FA | 3 | CUBIERTA |
| FA-3: Código inválido, ya usado o expirado al confirmar la resolución (la marca persiste) | FA | 9 | CUBIERTA |
| Cada código sirve una sola vez | Regla | — | DELEGADA (HU-20.1 Esc. 4) |
| Vigencia de 15 minutos, 3 intentos con espera de 1 hora y canal del código (FA-8) | Regla / FA-8 | — | DELEGADA (HU-20.1 Esc. 12 y 13) |
| Solo personal financiero marca y resuelve (FA-7) | Regla / FA | — | DELEGADA (HU-37.1) |
| Verificación en dos pasos del rol financiero | Restricción | — | DELEGADA (Área 7) |
| Técnica — trazabilidad append-only de marca y resolución | Técnica | 4 (la marca original se conserva) | DELEGADA (HU-35.1) |
| Formas de "resolver" un desembolso además de repagarlo | Sin definir | — | NO PROBABLE (P7 del área) |
| Resolución de un desembolso de una beca que quedó *Inactiva* | Confirmado que *Inactiva* no es estado final; resolución durante ese estado sin definir | — | NO PROBABLE (ver Preguntas abiertas) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-20.2).*

### HU-20.3 Registrar y modificar los datos de pago del becario con un código de un solo uso

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-20 | Debe tener | HU-20.1, HU-35.1, HU-37.1 | HU-20.5 |

#### Narrativa

- Como becaria o becario
- Quiero registrar mis datos de pago la primera vez y modificarlos cuando cambien, confirmando la operación con un código de un solo uso enviado en el momento
- Para que el dinero de mi beca llegue a la cuenta correcta, sin que el cambio quede sin verificar ni sin registrar

#### Reglas de negocio

- Los datos de pago de un becario los registra y modifica **solo el propio becario**; el administrador financiero no los modifica, aunque sí puede consultarlos para pagar (Restricción del cliente).
- El primer registro de los datos de pago lo hace el becario y exige también el código de un solo uso.
- Registrar o modificar los datos de pago requiere un código de verificación de un solo uso (OTP) enviado en el momento a quien hace la operación (Restricción del cliente).
- Cada código sirve una sola vez (Restricción del cliente).
- El código llega por el canal que elija el usuario (correo o SMS), vale 15 minutos y admite 3 intentos fallidos antes de una espera de 1 hora; se puede solicitar su reenvío (mismos parámetros del servicio único de verificación; ver HU-20.1).
- El registro y el cambio de los datos de pago quedan registrados con quién y cuándo, sin edición retroactiva silenciosa (RNF de trazabilidad; RF-35, ver HU-35.1).
- No se modifican los datos de pago de una beca *Revocada* o *Concluida*: no habrá más desembolsos sobre ella (RF-22; regla transversal de estados finales).
- Un becario solo accede a sus propios datos de pago: no consulta ni modifica los de otro becario (RNF de confidencialidad y de protección de datos personales).
- Los datos de pago son información financiera sensible: su acceso está restringido a quien los necesita para pagar (RNF de confidencialidad y de protección de datos personales; RF-37, ver HU-37.1).
- El acceso del rol financiero que consulta los datos de pago exige verificación en dos pasos al iniciar sesión (Restricción del cliente; ver Área 7).
- **Sin definir en el PRD:** qué campos forman los datos de pago del becario. El cliente responde "los necesarios" y la lista propuesta por el equipo (titular de la cuenta, cédula del titular, banco, tipo de cuenta, número de cuenta y, si aplica, datos de pago móvil) sigue por confirmar; la duda pasa a Preguntas abiertas.

#### Flujo de la HU

1. El becario abre sus datos de pago: si no tiene ninguno registrado elige "Registrar"; si ya los tiene, elige "Modificar".
2. Ingresa los datos.
3. El sistema verifica que su beca no esté *Revocada* ni *Concluida*.
4. El sistema envía un código de un solo uso en ese momento, por el canal elegido por el becario y válido 15 minutos, y el becario lo ingresa.
5. El sistema valida el código y guarda los datos.
6. La operación queda registrada con quién y cuándo, y los datos se muestran enmascarados.

#### Flujos alternos y de error

- FA-1 (paso 5) Código incorrecto, ya usado o expirado: el sistema no guarda y muestra "El código no es válido. No se modificaron los datos de pago."
- FA-2 (paso 4) Cancela: los datos de pago no cambian.
- FA-3 (paso 3) Beca *Revocada* o *Concluida*: el sistema no permite registrar ni modificar los datos de pago y muestra el estado de la beca.
- FA-4 (paso 1) Usuario distinto del becario titular, incluido el administrador financiero: el sistema no ofrece "Registrar" ni "Modificar"; el personal con función financiera solo consulta los datos para pagar (RF-37; ver HU-37.1).
- FA-5 (paso 5) Tercer intento fallido del código: el sistema bloquea la operación y no acepta nuevos códigos hasta que pase 1 hora; los datos de pago no cambian.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-20.3).*

#### Criterios de aceptación (10 escenarios)

##### Escenario 1: El becario cambia sus datos de pago

- **DADO QUE** Luis, becario activo, tiene registrada la cuenta bancaria terminada en 1234
- **CUANDO** registra la cuenta terminada en 5678 e ingresa el código que recibió
- **ENTONCES** sus datos de pago muestran la cuenta terminada en 5678 y el cambio queda registrado con el nombre de Luis y la fecha

##### Escenario 2: Código incorrecto

- **DADO QUE** Luis ingresó nuevos datos de pago
- **CUANDO** ingresa un código incorrecto
- **ENTONCES** el sistema muestra "El código no es válido. No se modificaron los datos de pago." y conserva la cuenta terminada en 1234

##### Escenario 3: Código reutilizado

- **DADO QUE** Luis ya usó el código 715204 en un cambio anterior
- **CUANDO** lo ingresa para un nuevo cambio de sus datos de pago
- **ENTONCES** el sistema rechaza el código y los datos de pago no cambian

##### Escenario 4: Beca revocada

- **DADO QUE** la beca de Juan está *Revocada*
- **CUANDO** Juan intenta modificar sus datos de pago
- **ENTONCES** el sistema no lo permite y muestra que la beca está *Revocada* y no recibirá más desembolsos

##### Escenario 5: Beca concluida impide modificar datos de pago

- **DADO QUE** la beca de Ana está *Concluida*
- **CUANDO** Ana intenta modificar sus datos de pago
- **ENTONCES** el sistema no lo permite y muestra que la beca está *Concluida* y no recibirá más desembolsos

##### Escenario 6: La consulta de los datos de pago queda registrada

- **DADO QUE** Luis tiene registrada la cuenta terminada en 5678
- **CUANDO** el administrador financiero abre sus datos de pago sin modificarlos
- **ENTONCES** la cuenta se muestra enmascarada, con los últimos dígitos 5678, y el historial de los datos de pago de Luis incluye una entrada de consulta con el nombre del administrador y la fecha

##### Escenario 7: Cancelación de la modificación

- **DADO QUE** Luis ingresó una nueva cuenta de pago
- **CUANDO** cancela antes de ingresar el código de un solo uso
- **ENTONCES** sus datos de pago siguen mostrando la cuenta terminada en 1234

##### Escenario 8: Primer registro de los datos de pago

- **DADO QUE** Luis acaba de ser becario de "Becas Arte" y no tiene datos de pago registrados
- **CUANDO** registra su cuenta bancaria terminada en 1234 e ingresa el código que recibió
- **ENTONCES** sus datos de pago quedan registrados, se muestran enmascarados con los últimos dígitos 1234 y el registro queda asentado con su nombre y la fecha

##### Escenario 9: El administrador financiero no modifica los datos de pago

- **DADO QUE** Luis tiene registrada la cuenta terminada en 1234
- **CUANDO** el administrador financiero abre sus datos de pago
- **ENTONCES** solo puede consultarlos enmascarados, no se le ofrece "Registrar" ni "Modificar", y la cuenta de Luis sigue terminada en 1234

##### Escenario 10: Un becario no accede a los datos de pago de otro

- **DADO QUE** Luis y María son becarios del mismo programa
- **CUANDO** Luis intenta abrir o modificar los datos de pago de María
- **ENTONCES** el sistema le niega el acceso y los datos de pago de María no cambian

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Solo el becario registra y modifica sus datos de pago; el administrador financiero solo consulta (FA-4) | Regla / FA-4 | 1, 9 | CUBIERTA |
| El primer registro lo hace el becario y exige código | Regla | 8 | CUBIERTA |
| Registrar o modificar datos de pago requiere OTP enviado en el momento | Regla | 1, 2, 8 | CUBIERTA |
| Cada código sirve una sola vez | Regla | 3 | CUBIERTA |
| Vigencia de 15 minutos, 3 intentos con espera de 1 hora y canal del código (FA-5) | Regla / FA-5 | — | DELEGADA (HU-20.1 Esc. 12 y 13) |
| Operación registrada con quién y cuándo, sin edición retroactiva silenciosa | Regla | 1, 8 | CUBIERTA |
| No se modifican datos de pago de beca *Revocada* (FA-3) | Regla / FA-3 | 4 | CUBIERTA |
| No se modifican datos de pago de beca *Concluida* (FA-3) | Regla / FA-3 | 5 | CUBIERTA |
| Un becario no accede ni modifica los datos de pago de otro | Regla | 10 | CUBIERTA |
| Acceso a datos de pago restringido a quien los necesita | Regla | 9 | CUBIERTA en lo observable; el alcance por rol, DELEGADO (HU-37.1) |
| Técnica — cuenta cifrada en reposo y mostrada enmascarada | Técnica | 1, 6, 8, 9 (enmascaramiento; el cifrado en reposo no es observable por el usuario) | CUBIERTA en lo observable |
| Técnica — auditoría de lectura de los datos de pago | Técnica | 6 | CUBIERTA |
| FA-1: Código incorrecto, ya usado o expirado | FA | 2, 3 | CUBIERTA |
| FA-2: Cancela la operación | FA | 7 | CUBIERTA |
| Verificación en dos pasos del rol financiero que consulta | Restricción | — | DELEGADA (Área 7) |
| Campos que componen los datos de pago | Por confirmar | — | NO PROBABLE (lista propuesta sin confirmar; ver Preguntas abiertas) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-20.3).*

## RF-21 — Suspensión temporal de desembolsos · *Debe tener*

### HU-21.1 Suspender una beca con motivo y fecha de revisión, y notificar al becario

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-21 | Debe tener | HU-19.1, HU-29.1, HU-35.1, HU-37.1 | HU-21.1, HU-21.2 |

#### Narrativa

- Como coordinador de programa
- Quiero suspender una beca registrando el motivo, lo que el becario debe resolver y una fecha de revisión, o marcarla como indefinida, y que el becario reciba el aviso
- Para pausar los desembolsos de un becario sin cancelar la beca por completo, y que el becario sepa qué hacer para que se reanuden

#### Reglas de negocio

- La suspensión pausa los desembolsos del becario sin cancelar la beca (RF-21).
- El motivo de la suspensión queda registrado (RF-21).
- El becario es notificado con el motivo de la suspensión y lo que necesita resolver para reanudar; por eso el coordinador registra ambos (RF-21).
- El correo de suspensión **sí incluye el motivo**: el cliente lo confirma expresamente, de modo que el motivo viaja en el mensaje y no solo dentro de la plataforma (afecta la regla de contenido de HU-29.1).
- Los desembolsos cuya fecha programada cae dentro de la suspensión no se pierden: se pagan al reanudar la beca (ver HU-21.2).
- Toda suspensión tiene una fecha de revisión o queda marcada como indefinida hasta resolución manual (RF-21).
- Al llegar la fecha de revisión de una suspensión, el sistema avisa al coordinador responsable; la beca no cambia de estado por sí sola (ver HU-21.3).
- Las fechas se manejan en UTC-4 y el día indicado se cuenta completo, hasta las 23:59: la fecha de revisión no se considera alcanzada antes de ese momento.
- Solo se permite la transición *Activa* → *Suspendida* (regla transversal). Una beca ya *Suspendida* no se vuelve a suspender: su motivo y su fecha de revisión originales se conservan (regla transversal de un solo estado a la vez; RNF de trazabilidad).
- El cambio de estado queda justificado y registrado con quién, cuándo y por qué (regla transversal; RF-35, ver HU-35.1).
- La suspensión es un hito que se notifica de forma automática; el canal mínimo es el correo electrónico (RF-29; ver HU-29.1).
- La notificación indica qué ocurrió y qué acción se espera del becario (RF-29).
- La notificación llega solo al becario afectado; ningún otro becario ve datos de esa suspensión (RNF de confidencialidad).
- La suspensión la ejecuta el coordinador responsable del programa de esa beca; el alcance por rol y por programa se resuelve en RF-37 (ver HU-37.1), con verificación en dos pasos al iniciar sesión (Restricción del cliente; ver Área 7).

#### Flujo de la HU

1. El coordinador abre una beca *Activa* de su programa y elige "Suspender".
2. Escribe el motivo y lo que el becario debe resolver para reanudar.
3. Indica una fecha de revisión o marca la suspensión como indefinida.
4. Confirma la suspensión.
5. El sistema cambia la beca a *Suspendida*, registra la decisión (HU-35.1) y pausa sus desembolsos programados (HU-19.1), todo en una misma transacción.
6. El sistema genera la notificación con el motivo, lo que el becario debe resolver y la fecha de revisión o la marca de indefinida.
7. El sistema la envía al correo del becario, con el motivo incluido, y por los canales adicionales del programa, si los hay (HU-29.1).
8. El becario ve la misma información en el detalle de su beca.

#### Flujos alternos y de error

- FA-1 (paso 2) Sin motivo: el sistema no permite confirmar y muestra "Debes indicar el motivo de la suspensión."
- FA-2 (paso 3) Sin fecha ni indefinida: el sistema no permite confirmar y muestra "Indica una fecha de revisión o marca la suspensión como indefinida."
- FA-3 (paso 1) Beca en otro estado: si la beca no está *Activa*, el sistema no ofrece "Suspender".
- FA-4 (paso 6) Suspensión indefinida: la notificación indica que la suspensión no tiene fecha de revisión y se resolverá de forma manual.
- FA-5 (paso 1) Beca ya *Suspendida*: la opción "Suspender" no está disponible; el sistema remite a reanudar (HU-21.2) o a revocar (HU-22.1).
- FA-6 (paso 5) Confirmación de desembolso simultánea: si la suspensión se registra primero, el desembolso no se confirma y el administrador financiero ve que la beca fue suspendida.
- FA-7 (paso 1) Usuario sin función de coordinación del programa: el sistema no ofrece "Suspender" (RF-37; ver HU-37.1).

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-21.1).*

#### Criterios de aceptación (9 escenarios)

##### Escenario 1: Suspensión con fecha de revisión y aviso al becario

- **DADO QUE** la beca de Pedro está *Activa*
- **CUANDO** el coordinador la suspende con el motivo "Constancia de matrícula pendiente", lo que debe resolver "Entregar la constancia de matrícula" y la fecha de revisión 15/11/2026
- **ENTONCES** la beca queda *Suspendida*, con motivo, fecha de revisión, autor y fecha de la decisión registrados; Pedro recibe por correo el aviso de que su beca quedó *Suspendida*, **con el motivo "Constancia de matrícula pendiente"** y la acción que se espera de él, y ve la misma información y la fecha de revisión 15/11/2026 en el detalle de su beca

##### Escenario 2: Falta la fecha de revisión

- **DADO QUE** el coordinador está suspendiendo la beca *Activa* de Pedro con un motivo
- **CUANDO** confirma sin fecha de revisión y sin marcarla como indefinida
- **ENTONCES** la beca sigue *Activa* y el sistema muestra "Indica una fecha de revisión o marca la suspensión como indefinida."

##### Escenario 3: Desembolsos en pausa

- **DADO QUE** la beca de Pedro está *Suspendida* y tiene un desembolso programado para el 30/10/2026
- **CUANDO** el administrador financiero intenta registrar ese desembolso
- **ENTONCES** el sistema no lo permite y muestra "La beca está suspendida. Sus desembolsos están en pausa."

##### Escenario 4: Beca que no está activa

- **DADO QUE** la beca de Ana está *Otorgada* y aún no está *Activa*
- **CUANDO** el coordinador consulta sus acciones
- **ENTONCES** la opción "Suspender" no está disponible

##### Escenario 5: Suspensión indefinida

- **DADO QUE** la suspensión de la beca de Pedro se marcó como indefinida
- **CUANDO** Pedro recibe la notificación
- **ENTONCES** el mensaje indica que no hay fecha de revisión y que la suspensión se resolverá de forma manual

##### Escenario 6: Solo el becario afectado

- **DADO QUE** se suspendió la beca de Pedro
- **CUANDO** Lucía, becaria del mismo programa, revisa su correo y su beca
- **ENTONCES** Lucía no recibe ningún aviso ni ve datos de la suspensión de Pedro

##### Escenario 7: Suspensión simultánea con la confirmación de un desembolso

- **DADO QUE** el administrador financiero está confirmando con un código válido un desembolso de la beca *Activa* de Pedro
- **CUANDO** el coordinador registra la suspensión de esa misma beca antes de que la confirmación se complete
- **ENTONCES** el desembolso no se confirma, el administrador ve "La beca está suspendida. Sus desembolsos están en pausa." y la beca no queda a la vez *Suspendida* y con ese desembolso pagado

##### Escenario 8: Beca ya suspendida

- **DADO QUE** la beca de Pedro ya está *Suspendida* con el motivo "Constancia de matrícula pendiente"
- **CUANDO** el coordinador consulta sus acciones
- **ENTONCES** la opción "Suspender" no está disponible y el motivo y la fecha de revisión originales se conservan

##### Escenario 9: Falta el motivo de la suspensión

- **DADO QUE** el coordinador está suspendiendo la beca *Activa* de Pedro
- **CUANDO** confirma sin escribir el motivo
- **ENTONCES** el sistema no permite confirmar y muestra "Debes indicar el motivo de la suspensión."

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| La suspensión pausa los desembolsos sin cancelar la beca | Regla | 3, 7 | CUBIERTA |
| El motivo queda registrado | Regla | 1, 8 | CUBIERTA |
| El becario es notificado y conoce el motivo y lo que debe resolver | Regla | 1 | CUBIERTA |
| El correo de suspensión incluye el motivo | Regla | 1 | CUBIERTA |
| Los desembolsos vencidos durante la suspensión se pagan al reanudar | Regla | — | DELEGADA (HU-21.2 Esc. 7) |
| Fecha de revisión o marca de indefinida (FA-2, FA-4) | Regla / FA | 1, 2, 5 | CUBIERTA |
| Al llegar la fecha de revisión el sistema avisa al coordinador | Regla | — | DELEGADA (HU-21.3) |
| Fechas en UTC-4, con el día completo hasta las 23:59 | Regla | — | DELEGADA (HU-21.3 Esc. 3; regla transversal del CONTEXTO COMÚN) |
| Solo transición *Activa* → *Suspendida*; una ya suspendida no se vuelve a suspender (FA-5) | Regla / FA-5 | 4, 8 | CUBIERTA |
| Cambio de estado registrado con quién, cuándo y por qué | Regla | 1 | CUBIERTA |
| Notificación automática, canal mínimo correo | Regla | 1, 6 | CUBIERTA |
| La notificación indica qué ocurrió y qué se espera del becario | Regla | 1, 5 | CUBIERTA |
| La notificación llega solo al becario afectado | Regla | 6 | CUBIERTA |
| FA-1: Sin motivo | FA | 9 | CUBIERTA |
| FA-3: Beca en un estado distinto a *Activa* | FA | 4 | CUBIERTA |
| FA-6 / Técnica: consistencia bajo concurrencia frente a una confirmación de desembolso | Técnica / FA-6 | 7 | CUBIERTA |
| Solo el coordinador del programa suspende (FA-7) | Regla / FA | — | DELEGADA (HU-37.1) |
| Verificación en dos pasos del coordinador | Restricción | — | DELEGADA (Área 7) |
| Técnica — notificación asíncrona fuera de la transacción | Técnica | — | DELEGADA (HU-29.1) |
| Técnica — trazabilidad append-only de la decisión | Técnica | 8 (la decisión original se conserva) | DELEGADA (HU-35.1) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-21.1).*

### HU-21.2 Reanudar una beca suspendida

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-21 | Debe tener | HU-21.1, HU-35.1, HU-37.1 | HU-21.3 |

#### Narrativa

- Como coordinador de programa
- Quiero reanudar una beca suspendida cuando el becario resuelve el motivo
- Para que vuelva a recibir sus desembolsos

#### Reglas de negocio

- Se permite la transición *Suspendida* → *Activa* (regla transversal).
- Ninguna beca puede reactivarse después de estar *Concluida* o *Revocada*; si la persona quiere continuar, debe postular de nuevo como un caso nuevo (regla transversal).
- Todo cambio de estatus de una beca queda justificado y registrado (regla transversal; ver HU-35.1).
- Al reanudar, los desembolsos del calendario del becario dejan de estar en pausa (RF-21; ver HU-19.1).
- Los desembolsos cuya fecha programada venció durante la suspensión no se pierden ni se reprograman: se pagan al reanudar la beca.
- Una beca *Suspendida* con el motivo "Pausa" se reactiva por HU-27.2, que exige verificar de nuevo el estatus académico (RF-27, S5).
- Si una pausa vence sin que se reactive la beca, esta queda *Inactiva* y sin ingresos (C3; ver HU-27.2).
- Para que una beca *Inactiva* pase a *Activa* o a *Concluida*, el becario debe subir los datos que le falten; con eso el coordinador de programa decide ese paso (C3; ver HU-27.2).
- La reanudación la ejecuta el coordinador responsable del programa de esa beca (RF-37; ver HU-37.1), con verificación en dos pasos al iniciar sesión (Restricción del cliente; ver Área 7).

#### Flujo de la HU

1. El coordinador abre una beca *Suspendida* de su programa y elige "Reanudar".
2. Escribe la justificación de la reanudación.
3. Confirma.
4. El sistema cambia la beca a *Activa* y registra la decisión (HU-35.1).
5. Los desembolsos del calendario vuelven a poder registrarse, incluidos los que vencieron durante la suspensión (HU-20.1).

#### Flujos alternos y de error

- FA-1 (paso 2) Sin justificación: el sistema no permite confirmar y muestra "Debes justificar la reanudación."
- FA-2 (paso 1) Suspensión por pausa: el sistema no ofrece "Reanudar" y remite a la reactivación tras pausa (HU-27.2).
- FA-3 (paso 1) Beca en estado final (*Revocada* o *Concluida*): el sistema no ofrece "Reanudar" (regla transversal).
- FA-4 (paso 1) Usuario sin función de coordinación del programa: el sistema no ofrece "Reanudar" (RF-37; ver HU-37.1).

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-21.2).*

#### Criterios de aceptación (7 escenarios)

##### Escenario 1: Reanudación con justificación

- **DADO QUE** la beca de Pedro está *Suspendida* por "Constancia de matrícula pendiente"
- **CUANDO** el coordinador la reanuda con la justificación "Constancia entregada el 10/11/2026"
- **ENTONCES** la beca queda *Activa*, la decisión queda registrada con autor, fecha y justificación, y su siguiente desembolso programado puede registrarse

##### Escenario 2: Sin justificación

- **DADO QUE** la beca de Pedro está *Suspendida*
- **CUANDO** el coordinador confirma "Reanudar" sin justificación
- **ENTONCES** la beca sigue *Suspendida* y el sistema muestra "Debes justificar la reanudación."

##### Escenario 3: Beca en pausa

- **DADO QUE** la beca de Lucía está *Suspendida* con el motivo "Pausa"
- **CUANDO** el coordinador consulta sus acciones
- **ENTONCES** la opción "Reanudar" no está disponible y el sistema indica que debe usarse la reactivación tras pausa

##### Escenario 4: Beca revocada

- **DADO QUE** la beca de Juan está *Revocada*
- **CUANDO** el coordinador consulta sus acciones
- **ENTONCES** la opción "Reanudar" no está disponible

##### Escenario 5: Beca concluida

- **DADO QUE** la beca de Ana está *Concluida*
- **CUANDO** el coordinador consulta sus acciones
- **ENTONCES** la opción "Reanudar" no está disponible y el sistema indica que una beca concluida no se reactiva

##### Escenario 6: Reanudación simultánea por dos coordinadores

- **DADO QUE** la beca de Pedro está *Suspendida* y dos coordinadores del mismo programa abren la opción "Reanudar" casi al mismo tiempo
- **CUANDO** ambos confirman la reanudación con una justificación
- **ENTONCES** la beca queda *Activa* con una sola reanudación registrada y el segundo coordinador ve que la beca ya no está *Suspendida*

##### Escenario 7: Desembolso vencido durante la suspensión

- **DADO QUE** la beca de Pedro estuvo *Suspendida* desde el 01/10/2026 y tenía un desembolso de 300 programado para el 30/10/2026, que no se pudo registrar
- **CUANDO** el coordinador la reanuda el 10/11/2026 con una justificación
- **ENTONCES** ese desembolso del 30/10/2026 deja de estar en pausa y el administrador financiero puede registrarlo y confirmarlo

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Transición *Suspendida* → *Activa* permitida | Regla | 1 | CUBIERTA |
| No se reactiva una beca *Revocada* ni *Concluida* (FA-3) | Regla / FA-3 | 4, 5 | CUBIERTA |
| Cambio de estado registrado con autor, fecha y justificación | Regla | 1 | CUBIERTA |
| Al reanudar, los desembolsos dejan de estar en pausa | Regla | 1 | CUBIERTA |
| Los desembolsos vencidos durante la suspensión se pagan al reanudar | Regla | 7 | CUBIERTA |
| Suspensión por "Pausa" remite a la reactivación con verificación académica (FA-2) | Regla / FA-2 | 3 | CUBIERTA en la remisión; la reactivación en sí, DELEGADA (HU-27.2) |
| Pausa vencida sin reactivar → beca *Inactiva* y sin ingresos | Regla | — | DELEGADA (HU-27.2, HU-26.2) |
| FA-1: Sin justificación | FA | 2 | CUBIERTA |
| Técnica — consistencia bajo concurrencia al reanudar | Técnica | 6 | CUBIERTA |
| Técnica — trazabilidad append-only (no sobrescribe la suspensión previa) | Técnica | 1 | DELEGADA (HU-35.1) |
| Solo el coordinador del programa reanuda (FA-4) | Regla / FA | — | DELEGADA (HU-37.1) |
| Verificación en dos pasos del coordinador | Restricción | — | DELEGADA (Área 7) |
| Reanudación antes de la fecha de revisión | Sin definir | — | NO PROBABLE (P11 del área) |
| Reactivación de una beca *Inactiva* (transición *Inactiva* → *Activa* confirmada, decidida por el coordinador de programa cuando el becario sube los datos que le falten) | Confirmado | — | NO PROBABLE: ningún escenario de esta HU cubre el flujo desde *Inactiva*, porque no está definido si lo ejecuta esta HU o un flujo distinto (ver Preguntas abiertas) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-21.2).*

### HU-21.3 Avisar al coordinador cuando llega la fecha de revisión de una suspensión

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-21 | Debe tener | HU-21.1, HU-21.2, HU-29.1, HU-37.1 | — (historia nueva de la Fase 3) |

#### Narrativa

- Como coordinador de programa
- Quiero que el sistema me avise cuando llega la fecha de revisión de una suspensión de mi programa
- Para decidir a tiempo si reanudo la beca o la revoco, sin que el caso se quede pausado porque nadie lo recordó

#### Reglas de negocio

- Al llegar la fecha de revisión registrada en una suspensión, el sistema avisa al coordinador responsable del programa de esa beca.
- El aviso no cambia por sí mismo el estado de la beca: sigue *Suspendida* hasta que alguien decida reanudarla (HU-21.2) o revocarla (HU-22.1), porque todo cambio de estatus exige una decisión justificada y registrada (RF-21; regla transversal; RF-35).
- Una suspensión marcada como indefinida no tiene fecha de revisión y no genera este aviso (RF-21).
- Si la beca ya salió de *Suspendida* antes de esa fecha, el aviso no se envía: no hay nada que revisar.
- La fecha de revisión se evalúa en UTC-4 y el día se cuenta completo, hasta las 23:59; el aviso no se dispara antes de que ese día comience.
- El aviso es una notificación automática: se envía por el mecanismo asíncrono de notificaciones, con el correo como canal mínimo (RF-29; ver HU-29.1).
- El aviso indica qué ocurrió (llegó la fecha de revisión de una suspensión) y qué acción se espera del coordinador (RF-29).
- El aviso llega al coordinador responsable del programa de esa beca; el alcance por rol y por programa se resuelve en RF-37 (ver HU-37.1).

#### Flujo de la HU

1. El coordinador suspende una beca con una fecha de revisión (HU-21.1).
2. El sistema mantiene esa fecha en vigilancia.
3. Al llegar el día de la fecha de revisión en UTC-4, el sistema comprueba que la beca siga *Suspendida*.
4. Genera el aviso con el becario, el programa, el motivo de la suspensión y la fecha de revisión, y lo envía al coordinador responsable por el mecanismo asíncrono de notificaciones (HU-29.1).
5. Registra que el aviso de esa suspensión ya se envió, para no duplicarlo.
6. El coordinador abre la beca desde el aviso y decide reanudarla (HU-21.2) o revocarla (HU-22.1).

#### Flujos alternos y de error

- FA-1 (paso 2) Suspensión indefinida: no hay fecha de revisión y el sistema no programa ningún aviso.
- FA-2 (paso 3) Beca ya reanudada o revocada antes de la fecha: el sistema no envía el aviso.
- FA-3 (paso 4) Fallo en el envío: el reintento y el tratamiento del fallo los gestiona el mecanismo de notificaciones (HU-29.1), sin que el aviso se pierda.
- FA-4 (paso 6) El coordinador no actúa tras el aviso: la beca sigue *Suspendida*, con sus desembolsos en pausa, hasta una decisión manual.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-21.3).*

#### Criterios de aceptación (6 escenarios)

##### Escenario 1: Aviso al llegar la fecha de revisión

- **DADO QUE** la beca de Pedro está *Suspendida* con el motivo "Constancia de matrícula pendiente" y fecha de revisión 15/11/2026
- **CUANDO** llega el 15/11/2026 y la beca sigue *Suspendida*
- **ENTONCES** el coordinador responsable del programa recibe un aviso con el becario, el motivo y la fecha de revisión, y la indicación de revisar el caso

##### Escenario 2: El aviso no cambia el estado de la beca

- **DADO QUE** el coordinador recibió el aviso de la fecha de revisión de la beca de Pedro
- **CUANDO** no registra ninguna decisión
- **ENTONCES** la beca sigue *Suspendida*, con sus desembolsos en pausa, y no se registra ningún cambio de estado

##### Escenario 3: Suspensión indefinida sin aviso

- **DADO QUE** la suspensión de la beca de Lucía se marcó como indefinida, sin fecha de revisión
- **CUANDO** pasan los meses sin que se resuelva
- **ENTONCES** el sistema no envía ningún aviso de fecha de revisión por esa beca

##### Escenario 4: Beca reanudada antes de la fecha de revisión

- **DADO QUE** la beca de Pedro tenía fecha de revisión 15/11/2026 y el coordinador la reanudó el 10/11/2026
- **CUANDO** llega el 15/11/2026
- **ENTONCES** el sistema no envía el aviso, porque la beca ya no está *Suspendida*

##### Escenario 5: El día de la fecha de revisión se cuenta en UTC-4

- **DADO QUE** la beca de Pedro está *Suspendida* con fecha de revisión 15/11/2026
- **CUANDO** son las 23:00 del 14/11/2026 en UTC-4
- **ENTONCES** el coordinador todavía no ha recibido el aviso, y lo recibe dentro del día 15/11/2026 en UTC-4

##### Escenario 6: El aviso llega solo al coordinador del programa

- **DADO QUE** la beca *Suspendida* de Pedro pertenece a "Becas Arte" y llegó su fecha de revisión
- **CUANDO** se envía el aviso
- **ENTONCES** lo recibe el coordinador responsable de "Becas Arte" y no los coordinadores de otros programas

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Al llegar la fecha de revisión el sistema avisa al coordinador | Regla | 1 | CUBIERTA |
| El aviso no cambia el estado de la beca | Regla | 2 | CUBIERTA |
| Suspensión indefinida no genera aviso (FA-1) | Regla / FA-1 | 3 | CUBIERTA |
| Beca que ya salió de *Suspendida* no genera aviso (FA-2) | Regla / FA-2 | 4 | CUBIERTA |
| Fecha evaluada en UTC-4, con el día completo hasta las 23:59 | Regla | 5 | CUBIERTA |
| El aviso llega al coordinador responsable del programa | Regla | 6 | CUBIERTA; el alcance por rol, DELEGADO (HU-37.1) |
| El aviso indica qué ocurrió y qué acción se espera | Regla (RF-29) | 1 | CUBIERTA |
| Canal mínimo correo y envío automático | Regla (RF-29) | — | DELEGADA (HU-29.1) |
| FA-3: Fallo de envío y reintento | FA | — | DELEGADA (HU-29.1) |
| FA-4: El coordinador no actúa tras el aviso | FA | 2 | CUBIERTA |
| Técnica — proceso programado idempotente (un aviso por suspensión y fecha) | Técnica | — | NO VERIFICABLE por escenario de negocio (prueba del proceso programado) |
| Aviso también al becario | Por confirmar | — | NO PROBABLE (ver Preguntas abiertas) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-21.3).*

## RF-22 — Revocación de una beca · *Debe tener*

### HU-22.1 Revocar una beca con motivo y código de un solo uso

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-22 | Debe tener | HU-35.1, HU-37.1 | HU-22.1 |

#### Narrativa

- Como coordinador de programa
- Quiero revocar una beca documentando el motivo y confirmando con un código de un solo uso
- Para cancelarla de forma definitiva cuando hay retiro de la universidad, información falsa o incumplimiento grave

#### Reglas de negocio

- La revocación cancela la beca de forma definitiva; son ejemplos de motivo el retiro de la universidad, la información falsa y el incumplimiento grave (RF-22).
- El motivo queda documentado (RF-22) y la decisión queda registrada con quién, cuándo y por qué (regla transversal; RF-35, ver HU-35.1).
- Incumplir las condiciones de permanencia de la beca cuenta como incumplimiento grave y habilita la revocación directa (RF-25; ver HU-25.1).
- Revocar requiere ingresar un código de verificación de un solo uso (OTP) enviado en el momento (Restricción del cliente; RF-22).
- Cada código sirve una sola vez (Restricción del cliente).
- El código llega por el canal elegido por el usuario (correo o SMS), vale 15 minutos y admite 3 intentos fallidos antes de una espera de 1 hora; se puede solicitar su reenvío (mismos parámetros del servicio único de verificación; ver HU-20.1).
- Transiciones permitidas hacia *Revocada*: *Suspendida* → *Revocada* y, por incumplimiento grave, cualquier estado activo → *Revocada* directamente (regla transversal).
- Una beca *Inactiva* también se puede revocar: se suma a *Suspendida* como transición directa, sin la condición de incumplimiento grave que exige la beca *Activa* (RF-22).
- Una beca *Concluida* o *Revocada* no se reactiva ni vuelve a cambiar de estado; si la persona quiere continuar, postula de nuevo como un caso nuevo (regla transversal). Por eso una beca en estado final no se revoca de nuevo y su decisión original se conserva intacta (RNF de trazabilidad).
- Los montos ya desembolsados antes de la revocación no se reclaman automáticamente (RF-22; ver HU-22.2).
- La revocación la ejecuta el coordinador responsable del programa de esa beca (RF-37; ver HU-37.1), con verificación en dos pasos al iniciar sesión (Restricción del cliente; ver Área 7).
- **Sin definir en el PRD:** qué estados abarca "cualquier estado activo" para la revocación directa (esta HU solo la permite desde *Activa*) y si el retiro de la universidad o la información falsa cuentan como incumplimiento grave. El incumplimiento de las condiciones de permanencia sí cuenta; para el resto, el coordinador indica en cada caso si la revocación se debe a un incumplimiento grave, y la duda pasa a Preguntas abiertas.

#### Flujo de la HU

1. El coordinador abre una beca *Activa*, *Suspendida* o *Inactiva* de su programa y elige "Revocar".
2. Escribe el motivo de la revocación e indica si se debe a un incumplimiento grave.
3. El sistema envía un código de un solo uso al coordinador, por su canal elegido y válido 15 minutos.
4. El coordinador ingresa el código y confirma.
5. El sistema valida el código y su vigencia, cambia la beca a *Revocada* y registra la decisión (HU-35.1).
6. El sistema libera el presupuesto y el cupo (HU-22.2).

#### Flujos alternos y de error

- FA-1 (paso 5) Código incorrecto, ya usado o expirado: la beca no cambia de estado y el sistema muestra "El código no es válido. Solicita uno nuevo."
- FA-2 (paso 2) Sin motivo: el sistema no envía el código y muestra "Debes documentar el motivo de la revocación."
- FA-3 (paso 2) Estado que no admite la revocación: si la beca no está *Activa*, *Suspendida* ni *Inactiva*, el sistema no ofrece "Revocar". Si está *Activa* y no se indicó incumplimiento grave, el sistema no envía el código y muestra "Una beca activa solo se revoca directamente por incumplimiento grave. Suspéndela primero."
- FA-4 (paso 1) Beca ya *Revocada* o *Concluida*: la opción "Revocar" no está disponible y la decisión original se conserva.
- FA-5 (paso 5) Desembolso confirmado inmediatamente antes: la revocación se registra, el desembolso ya confirmado permanece pagado y ambos eventos quedan en el historial con su orden real.
- FA-6 (paso 1) Usuario sin función de coordinación del programa: el sistema no ofrece "Revocar" (RF-37; ver HU-37.1).
- FA-7 (paso 4) Tercer intento fallido del código: el sistema bloquea la revocación y no acepta nuevos códigos hasta que pase 1 hora; la beca no cambia de estado.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-22.1).*

#### Criterios de aceptación (11 escenarios)

##### Escenario 1: Revocación de una beca suspendida

- **DADO QUE** la beca de Juan está *Suspendida*
- **CUANDO** el coordinador la revoca con el motivo "Retiro de la universidad" e ingresa el código de un solo uso recibido
- **ENTONCES** la beca queda *Revocada* y la decisión queda registrada con autor, fecha y motivo

##### Escenario 2: Revocación directa por incumplimiento grave

- **DADO QUE** la beca de Juan está *Activa*
- **CUANDO** el coordinador la revoca con el motivo "Incumplimiento grave de las condiciones de la beca", indica que se debe a un incumplimiento grave e ingresa un código válido
- **ENTONCES** la beca pasa de *Activa* a *Revocada* sin pasar por *Suspendida*

##### Escenario 3: Código inválido

- **DADO QUE** el coordinador está revocando la beca *Suspendida* de Juan
- **CUANDO** ingresa un código que no coincide con el enviado
- **ENTONCES** la beca sigue *Suspendida* y el sistema muestra "El código no es válido. Solicita uno nuevo."

##### Escenario 4: Beca activa sin incumplimiento grave

- **DADO QUE** la beca de Juan está *Activa*
- **CUANDO** el coordinador intenta revocarla con el motivo "Retiro de la universidad" sin indicar que se debe a un incumplimiento grave
- **ENTONCES** el sistema no envía el código, la beca sigue *Activa* y muestra "Una beca activa solo se revoca directamente por incumplimiento grave. Suspéndela primero."

##### Escenario 5: Beca ya revocada

- **DADO QUE** la beca de Juan ya está *Revocada*
- **CUANDO** el coordinador consulta sus acciones
- **ENTONCES** la opción "Revocar" no está disponible y el motivo, el autor y la fecha de la revocación original se conservan

##### Escenario 6: Desembolso confirmado inmediatamente antes de la revocación

- **DADO QUE** el administrador financiero confirmó con un código válido un desembolso de la beca *Activa* de Luis
- **CUANDO** el coordinador revoca esa misma beca justo después de que la confirmación quedó registrada
- **ENTONCES** la revocación se registra, el desembolso confirmado permanece como pagado y no se reclama automáticamente, y ambos eventos quedan en el historial con el orden real en que ocurrieron

##### Escenario 7: Falta el motivo de la revocación

- **DADO QUE** el coordinador está revocando la beca *Suspendida* de Juan
- **CUANDO** intenta confirmar sin documentar el motivo
- **ENTONCES** el sistema no envía el código y muestra "Debes documentar el motivo de la revocación."

##### Escenario 8: Beca en un estado que no admite revocación

- **DADO QUE** la beca de Ana está *Otorgada* y aún no está *Activa*
- **CUANDO** el coordinador consulta sus acciones
- **ENTONCES** la opción "Revocar" no está disponible

##### Escenario 9: Beca concluida no se revoca

- **DADO QUE** la beca de Ana está *Concluida*
- **CUANDO** el coordinador consulta sus acciones
- **ENTONCES** la opción "Revocar" no está disponible y no se registra ninguna decisión de revocación para esa beca

##### Escenario 10: Revocación directa por incumplir las condiciones de permanencia

- **DADO QUE** la beca *Activa* de Juan no cumple las condiciones de permanencia de su programa, según la evaluación del seguimiento académico (HU-25.1)
- **CUANDO** el coordinador la revoca indicando ese incumplimiento de permanencia como incumplimiento grave e ingresa un código válido
- **ENTONCES** la beca pasa de *Activa* a *Revocada* sin pasar por *Suspendida*, con el motivo documentado

##### Escenario 11: Revocación de una beca *Inactiva

- **DADO QUE** la beca de Rosa en "Becas Arte" quedó *Inactiva* porque no se renovó para 2027-1
- **CUANDO** el coordinador la revoca con el motivo "La becaria no continuará en el programa" e ingresa un código de un solo uso válido
- **ENTONCES** la beca pasa de *Inactiva* a *Revocada* y la decisión queda registrada con autor, fecha y motivo

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| La revocación cancela la beca de forma definitiva | Regla | 1, 2 | CUBIERTA |
| El motivo queda documentado y la decisión registrada con quién, cuándo y por qué | Regla | 1, 5 | CUBIERTA |
| Revocar requiere OTP enviado en el momento | Regla | 1, 2, 3 | CUBIERTA |
| Vigencia de 15 minutos, 3 intentos con espera de 1 hora y canal del código (FA-7) | Regla / FA-7 | — | DELEGADA (HU-20.1 Esc. 12 y 13) |
| Transición *Suspendida* → *Revocada* | Regla | 1 | CUBIERTA |
| *Activa* → *Revocada* directa solo por incumplimiento grave (FA-3) | Regla / FA-3 | 2, 4 | CUBIERTA |
| Incumplir las condiciones de permanencia es incumplimiento grave y habilita la revocación directa | Regla | 10 | CUBIERTA |
| Beca *Revocada* no se revoca de nuevo; conserva la decisión original (FA-4) | Regla / FA-4 | 5 | CUBIERTA |
| Beca *Concluida* no se revoca (FA-4) | Regla / FA-4 | 9 | CUBIERTA |
| FA-3: Estado que no admite revocación (ni *Activa* ni *Suspendida*) | FA | 8 | CUBIERTA (alcance sujeto a P12, ahora reducida) |
| Montos ya desembolsados antes de la revocación no se reclaman | Regla | 6 | CUBIERTA |
| FA-1: Código incorrecto, ya usado o expirado | FA | 3 | CUBIERTA |
| FA-2: Sin motivo | FA | 7 | CUBIERTA |
| FA-5 / Técnica: revocación y desembolso confirmado casi al mismo tiempo | Técnica / FA-5 | 6 | CUBIERTA |
| Liberación de presupuesto y cupo en la misma transacción | Regla / Técnica | — | DELEGADA (HU-22.2) |
| Cada código sirve una sola vez | Regla | — | DELEGADA (HU-20.1 Esc. 4) |
| Solo el coordinador del programa revoca (FA-6) | Regla / FA | — | DELEGADA (HU-37.1) |
| Verificación en dos pasos del coordinador | Restricción | — | DELEGADA (Área 7) |
| Técnica — trazabilidad append-only con el orden real de los eventos | Técnica | 6 | CUBIERTA |
| Revocación de una beca *Inactiva* permitida (FA-3) | Regla / FA-3 | 11 | CUBIERTA |
| Alcance de "cualquier estado activo" y clasificación como incumplimiento grave del retiro o la información falsa | Sin definir | — | NO PROBABLE (P12 del área, reducida por R19) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-22.1).*

### HU-22.2 Liberar el presupuesto y el cupo de una beca revocada

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-22 | Debe tener | HU-12.1, HU-22.1 | HU-22.2 |

#### Narrativa

- Como administrador financiero
- Quiero ver el presupuesto y el cupo de un programa actualizados en cuanto se revoca una beca, con lo ya desembolsado intacto
- Para decidir, sobre cifras reales, si el programa puede comprometer ese cupo y ese monto liberados

#### Reglas de negocio

- El presupuesto y el cupo liberados por la revocación vuelven a estar disponibles en el programa (RF-22).
- Lo que se libera es la parte no desembolsada del monto fijo del periodo aprobado en la oferta de esa beca; no existen coberturas total ni parcial que calcular (RF-22; C1).
- El cupo del programa cuenta sus becarios totales, mientras que el cupo de una convocatoria cuenta los nuevos del ciclo: el cupo liberado por una revocación vuelve al cupo del programa (RF-11, RF-12).
- Los montos ya desembolsados antes de la revocación no se reclaman automáticamente (RF-22).
- La revocación es definitiva: la beca no recibe más desembolsos (RF-22; ver HU-20.1).
- La disponibilidad de presupuesto y cupo debe poder consultarse en cualquier momento y reflejar el estado real (RF-12; ver HU-12.1).
- El uso del cupo liberado para otorgar una nueva beca se verifica y decide en el otorgamiento (RF-12; ver HU-12.1 y Área 3), no en esta HU.
- Una beca *Inactiva* no libera su cupo ni su presupuesto: a diferencia de la revocación, los mantiene ocupados mientras esté *Inactiva*, porque puede volver a *Activa* (RF-22; C3).

#### Flujo de la HU

1. El coordinador revoca la beca (HU-22.1).
2. El sistema toma el monto del periodo aprobado en la oferta de la beca y lo ya desembolsado.
3. El sistema devuelve al disponible del programa la parte aún no desembolsada y libera un cupo del programa, en la misma transacción de la revocación.
4. El sistema retira del calendario los desembolsos pendientes de esa beca.
5. El administrador financiero ve el presupuesto y el cupo actualizados (HU-12.1) y decide sobre esa disponibilidad.

#### Flujos alternos y de error

- FA-1 (paso 3) Beca sin desembolsos: se libera todo el monto del periodo aprobado en la oferta y un cupo.
- FA-2 (paso 3) Dos revocaciones del mismo programa casi al mismo tiempo: ambas liberaciones se aplican de forma consistente, sin perder ni duplicar ninguna.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-22.2).*

#### Criterios de aceptación (5 escenarios)

##### Escenario 1: Liberación de lo no desembolsado

- **DADO QUE** la beca de Juan tiene 3.000 aprobados en su oferta para el periodo en curso, de los cuales se desembolsaron 1.000, y el programa tiene 5.000 disponibles y 0 cupos libres
- **CUANDO** se revoca la beca
- **ENTONCES** el programa muestra 7.000 disponibles y 1 cupo libre

##### Escenario 2: Lo desembolsado no se reclama

- **DADO QUE** la beca revocada de Juan tuvo 1.000 desembolsados
- **CUANDO** el administrador financiero consulta los movimientos de la beca
- **ENTONCES** los 1.000 siguen registrados como desembolsados y no se genera ningún cobro al becario

##### Escenario 3: Sin más desembolsos

- **DADO QUE** la beca de Juan fue revocada y tenía un desembolso programado para el 30/11/2026
- **CUANDO** el administrador financiero consulta el calendario del programa
- **ENTONCES** ese desembolso ya no aparece como pendiente

##### Escenario 4: Revocaciones simultáneas en el mismo programa

- **DADO QUE** "Becas Ingeniería" tiene 10.000 disponibles y dos becas *Activas* con 2.000 aprobados cada una, sin desembolsos
- **CUANDO** se revocan ambas becas casi al mismo tiempo
- **ENTONCES** el programa termina con exactamente 14.000 disponibles y 2 cupos liberados, sin perder ni duplicar ninguna de las dos liberaciones

##### Escenario 5: Beca *Inactiva* no libera presupuesto ni cupo

- **DADO QUE** la beca de Rosa en "Becas Arte" quedó *Inactiva* porque no se renovó para 2027-1, con 2.000 aprobados para ese periodo sin desembolsar, y el programa tiene 5.000 disponibles y 0 cupos libres
- **CUANDO** el administrador financiero consulta el presupuesto y el cupo del programa
- **ENTONCES** el programa sigue con 5.000 disponibles y 0 cupos libres: el monto y el cupo de la beca de Rosa permanecen ocupados mientras esté *Inactiva*

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| El presupuesto y el cupo liberados vuelven a estar disponibles | Regla | 1, 4 | CUBIERTA |
| Lo liberado es la parte no desembolsada del monto del periodo | Regla | 1 | CUBIERTA |
| El cupo liberado vuelve al cupo del programa (becarios totales), no al de la convocatoria | Regla | 1, 4 | CUBIERTA |
| Los montos ya desembolsados no se reclaman | Regla | 2 | CUBIERTA |
| La beca revocada no recibe más desembolsos | Regla | 3 | CUBIERTA |
| FA-1: Beca sin desembolsos, se libera todo el monto del periodo y un cupo | FA | 4 | CUBIERTA |
| FA-2 / Técnica: dos revocaciones casi simultáneas en el mismo programa | FA / Técnica | 4 | CUBIERTA |
| La liberación ocurre en la misma transacción que la revocación | Técnica | 1, 4 (resultado observable) | CUBIERTA en lo observable |
| Disponibilidad consultable en cualquier momento y refleja el estado real | Regla | — | DELEGADA (HU-12.1) |
| Uso del cupo liberado para otorgar una nueva beca | Regla | — | DELEGADA (HU-12.1 / Área 3) |
| Técnica — disponible como agregado incremental (escalabilidad) | Técnica | — | NO VERIFICABLE por escenario (decisión de implementación) |
| Solo personal financiero o de dirección consulta la disponibilidad | Regla | — | DELEGADA (HU-37.1) |
| Destino del cupo liberado (lista de espera o decisión del coordinador) | Sin definir | — | NO PROBABLE (P20 del área, acotada por C4) |
| Una beca *Inactiva* no libera presupuesto ni cupo, los mantiene ocupados | Regla | 5 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-22.2).*

## RF-23 — Reporte de estado financiero por programa · *Debería tener*

### HU-23.1 Consultar y exportar el estado financiero de un programa

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-23 | Debería tener | HU-12.1, HU-12.2, HU-20.1, HU-20.2, HU-22.2, HU-37.1 | HU-23.1, HU-23.2 |

#### Narrativa

- Como administrador financiero
- Quiero ver por programa el presupuesto comprometido, desembolsado y disponible, y exportarlo
- Para conocer el estado real de los fondos de cada programa y compartirlo con el donante que financia ese programa

#### Reglas de negocio

- La consulta muestra, por programa, el presupuesto comprometido, el desembolsado y el disponible (RF-23).
- Refleja el estado real considerando becas pendientes, suspendidas y revocadas (RF-23).
- Las ofertas pendientes restan presupuesto —cuentan como comprometido— hasta que el proceso se rechace definitivamente; al rechazarse, ese monto vuelve a disponible (RF-23; ver HU-15.1 y HU-16.1).
- El comprometido de una beca es el monto fijo aprobado para el periodo, no un cálculo de cobertura total o parcial ni la suma de toda la duración de la beca (RF-23; C1).
- El presupuesto comprometido en renovaciones pendientes del ciclo actual cuenta como no disponible; en una renovación el monto es el asignado a ese estudiante (RF-12, C1; ver HU-12.2).
- Una beca suspendida no está cancelada: lo aprobado en su oferta sigue comprometido (RF-21).
- En una beca revocada, lo no desembolsado vuelve a disponible y lo ya desembolsado se mantiene como desembolsado (RF-22; ver HU-22.2).
- Una beca *Inactiva* no recibe dinero en el periodo siguiente, por lo que no genera nuevos desembolsos en el reporte (C3; ver HU-19.1).
- A diferencia de una beca revocada, el monto no desembolsado de una beca *Inactiva* sigue figurando como comprometido, no pasa a disponible, mientras dure ese estado, porque no libera su presupuesto ni su cupo (RF-22, RF-23; ver HU-22.2).
- Un desembolso fallido o devuelto no cuenta como pagado hasta resolverse (RF-20; ver HU-20.2).
- El estado financiero de un programa se puede compartir o exportar para reportar a un donante (RF-23).
- El archivo exportado muestra las mismas cifras que la consulta (RF-23).
- El donante no usa la plataforma: el equipo de la Fundación le comparte los reportes directamente (Alcance, fuera de alcance: portal de donantes).
- El reporte es de ejecución presupuestaria agregada; no expone información socioeconómica individual de los becarios (RNF de confidencialidad; RF-33).
- La consulta está limitada al personal con función financiera o de dirección (RF-37; ver HU-37.1), con verificación en dos pasos al iniciar sesión (Restricción del cliente; ver Área 7).

#### Flujo de la HU

1. El administrador financiero entra a "Estado financiero" y elige un programa.
2. El sistema muestra el presupuesto total, el comprometido, el desembolsado y el disponible.
3. El sistema muestra el desglose por estado de las becas (activas, suspendidas y revocadas), lo comprometido en ofertas pendientes y lo comprometido en renovaciones pendientes (HU-12.2).
4. El administrador financiero revisa las cifras.
5. Elige "Exportar".
6. El sistema genera, a partir de las mismas cifras consultadas, un archivo con el presupuesto total, comprometido, desembolsado y disponible del programa.
7. El administrador financiero descarga el archivo y lo comparte con el donante por los medios de la Fundación.

#### Flujos alternos y de error

- FA-1 (paso 2) Programa sin becas: el comprometido y el desembolsado aparecen en cero y el disponible es igual al presupuesto total.
- FA-2 (paso 6) Programa sin movimientos: el archivo se genera con comprometido y desembolsado en cero.
- FA-3 (paso 6) Exportación de gran volumen: el sistema genera el archivo en segundo plano y avisa al administrador cuando está disponible, sin afectar al resto de la plataforma.
- FA-4 (paso 1) Usuario sin función financiera ni de dirección: el sistema no ofrece "Estado financiero" (RF-37; ver HU-37.1).
- FA-5 (paso 2) Oferta rechazada definitivamente: el monto de esa oferta deja de figurar como comprometido y vuelve a disponible.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-23.1).*

#### Criterios de aceptación (12 escenarios)

##### Escenario 1: Consulta del estado financiero

- **DADO QUE** "Becas Ingeniería" tiene 100.000 de presupuesto y becas con desembolsos confirmados
- **CUANDO** el administrador financiero consulta su estado financiero
- **ENTONCES** ve el presupuesto comprometido, el desembolsado y el disponible del programa

##### Escenario 2: Beca revocada

- **DADO QUE** una beca de "Becas Ingeniería" con 3.000 aprobados y 1.000 desembolsados fue revocada
- **CUANDO** el administrador financiero consulta el estado financiero
- **ENTONCES** los 1.000 figuran como desembolsados y los 2.000 restantes ya no figuran como comprometidos sino como disponibles

##### Escenario 3: Beca suspendida

- **DADO QUE** una beca de "Becas Ingeniería" con 3.000 aprobados está *Suspendida*
- **CUANDO** el administrador financiero consulta el estado financiero
- **ENTONCES** la parte no desembolsada de esos 3.000 sigue figurando como comprometida

##### Escenario 4: Desembolso fallido

- **DADO QUE** un desembolso de 500 está marcado como fallido y no se ha resuelto
- **CUANDO** el administrador financiero consulta el estado financiero
- **ENTONCES** esos 500 no se suman al desembolsado

##### Escenario 5: Exportación con las mismas cifras de la consulta

- **DADO QUE** la consulta del estado financiero de "Becas Ingeniería" muestra 40.000 desembolsados
- **CUANDO** el administrador financiero elige "Exportar"
- **ENTONCES** descarga un archivo con el presupuesto total, comprometido, desembolsado y disponible del programa, que muestra 40.000 desembolsados

##### Escenario 6: Programa sin movimientos

- **DADO QUE** "Becas Salud" no tiene becas otorgadas
- **CUANDO** el administrador financiero exporta su estado financiero
- **ENTONCES** el archivo muestra comprometido y desembolsado en cero y el disponible igual al presupuesto total

##### Escenario 7: Renovación pendiente comprometida

- **DADO QUE** "Becas Ingeniería" tiene una renovación pendiente del ciclo actual con 3.000 aprobados (HU-12.2)
- **CUANDO** el administrador financiero consulta el estado financiero
- **ENTONCES** esos 3.000 figuran como comprometidos y no como disponibles

##### Escenario 8: Exportación en segundo plano

- **DADO QUE** el administrador financiero exporta el estado financiero de un programa con miles de movimientos
- **CUANDO** la generación del archivo tarda varios segundos
- **ENTONCES** el sistema no bloquea su pantalla: la exportación queda marcada como en curso, el administrador puede seguir usando la plataforma y recibe el aviso cuando el archivo está disponible

##### Escenario 9: El reporte financiero no muestra datos socioeconómicos individuales

- **DADO QUE** "Becas Ingeniería" tiene becarios con ingreso familiar declarado en su perfil
- **CUANDO** el administrador financiero consulta o exporta el estado financiero del programa
- **ENTONCES** el reporte muestra solo cifras agregadas de presupuesto y no incluye el ingreso ni otro dato socioeconómico individual de ningún becario

##### Escenario 10: Oferta pendiente de respuesta comprometida

- **DADO QUE** "Becas Ingeniería" emitió una oferta de 2.000 para el periodo en curso y el postulante todavía no la ha aceptado ni rechazado
- **CUANDO** el administrador financiero consulta el estado financiero
- **ENTONCES** esos 2.000 figuran como comprometidos y no como disponibles

##### Escenario 11: Oferta rechazada definitivamente libera el comprometido

- **DADO QUE** esa oferta de 2.000 de "Becas Ingeniería" figuraba como comprometida
- **CUANDO** el proceso termina con un rechazo definitivo
- **ENTONCES** esos 2.000 dejan de figurar como comprometidos y vuelven a figurar como disponibles

##### Escenario 12: Beca *Inactiva* mantiene su monto comprometido

- **DADO QUE** una beca de "Becas Ingeniería" con 3.000 aprobados y 1.000 desembolsados quedó *Inactiva* porque no se renovó para el periodo siguiente
- **CUANDO** el administrador financiero consulta el estado financiero
- **ENTONCES** los 1.000 figuran como desembolsados y los 2.000 restantes siguen figurando como comprometidos, no como disponibles

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Muestra comprometido, desembolsado y disponible por programa | Regla | 1 | CUBIERTA |
| Refleja el estado real considerando becas pendientes, suspendidas y revocadas | Regla | 2, 3, 4, 7, 10 | CUBIERTA |
| Las ofertas pendientes cuentan como comprometidas hasta el rechazo definitivo (FA-5) | Regla / FA-5 | 10, 11 | CUBIERTA |
| El comprometido por beca es el monto fijo del periodo | Regla | 2, 3, 7, 10 | CUBIERTA |
| Lo comprometido en renovaciones pendientes del ciclo actual no cuenta como disponible | Regla | 7 | CUBIERTA |
| Beca *Suspendida*: lo no desembolsado sigue comprometido | Regla | 3 | CUBIERTA |
| Beca *Revocada*: lo no desembolsado pasa a disponible, lo desembolsado se mantiene | Regla | 2 | CUBIERTA |
| Desembolso fallido o devuelto no cuenta como pagado | Regla | 4 | CUBIERTA |
| Puede compartirse o exportarse para el donante | Regla | 5 | CUBIERTA |
| El archivo exportado muestra las mismas cifras que la consulta | Regla | 5 | CUBIERTA |
| El reporte es agregado y no expone información socioeconómica individual | Regla (RNF confidencialidad; RF-33) | 9 | CUBIERTA |
| FA-1: Programa sin becas (comprometido y desembolsado en cero) | FA | 6 | CUBIERTA |
| FA-2: Programa sin movimientos en la exportación | FA | 6 | CUBIERTA |
| FA-3 / Técnica: exportación de gran volumen en segundo plano, con aviso | FA / Técnica | 8 | CUBIERTA en lo observable (asincronía y aviso) |
| El donante no usa la plataforma; el equipo le comparte el reporte | Alcance | 5 (descarga y comparte por fuera) | CUBIERTA |
| Solo personal financiero o de dirección consulta (FA-4) | Regla / FA | — | DELEGADA (HU-37.1) |
| Verificación en dos pasos del rol financiero o de dirección | Restricción | — | DELEGADA (Área 7) |
| Técnica — aislamiento de la reportería: no degradar las operaciones diarias | Técnica | — | NO VERIFICABLE por escenario (sin umbral en el PRD; ver pregunta abierta) |
| Técnica — archivo persistido en el Firebase de la Fundación | Técnica | — | NO PROBABLE (depende de P19: formato y persistencia) |
| El comprometido de una beca *Inactiva* sigue comprometido, no pasa a disponible | Regla | 12 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-23.1).*
