# Área 2 — Evaluación y Comité

> Parte de las [Historias de Usuario · Plan B · versión final](README.md). Reglas técnicas en el [Anexo A](anexo-a-reglas-tecnicas.md); preguntas abiertas en el [Anexo B](anexo-b-preguntas-abiertas.md).

## RF-06 — Conformación de comités evaluadores por programa · *Debe tener*

### HU-06.1 Conformar el comité de un programa

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-06 | Debe tener | HU-11.1, HU-37.1 | HU-06.1 |

#### Narrativa

- Como coordinador de programa
- Quiero asignar a mi programa un comité de uno o más evaluadores, internos o externos, designando a uno de ellos como jefatura del comité
- Para que las postulaciones del programa tengan quién las revise y las puntúe

#### Reglas de negocio

- Cada programa puede tener un comité asignado, compuesto por uno o más evaluadores internos o externos (RF-06).
- El comité no puede quedar vacío: necesita al menos un evaluador (RF-06).
- Dirección crea las cuentas de los evaluadores internos y externos y les asigna su rol; el coordinador elige entre esos usuarios (S1; ver HU-37.1).
- Solo pueden agregarse al comité usuarios que ya existen con rol de evaluador (interno o externo). La restricción se valida también al agregar, no solo en el alta de usuarios (RF-06, RF-37; ver HU-37.1).
- Una misma persona no puede figurar dos veces en el mismo comité: el comité es un conjunto de evaluadores distintos (RF-06).
- Al conformar el comité, el coordinador designa a uno de sus miembros como jefatura del comité (S12). La jefatura puede tomar la decisión final de RF-10 (ver HU-10.1).
- La jefatura del comité es única y obligatoria: exactamente un miembro la tiene, nunca cero ni más de uno. Es un invariante del dato, no solo una validación de formulario, porque HU-10.1 necesita resolver sin ambigüedad quién es la jefatura en cualquier momento (S12; ver HU-10.1).
- Un coordinador solo gestiona el comité de los programas de los que es responsable (RF-37; ver HU-37.1). La autorización se verifica en el servidor, no ocultando la opción en la interfaz.
- **Restricciones técnicas del cliente:** coordinador y evaluadores tienen acceso a información de decisión, por lo que el inicio de sesión exige verificación en dos pasos (2FA). El segundo factor de los evaluadores externos se envía por correo. No aplica el código de un solo uso por acción (OTP), reservado a RF-20/RF-22, ni el almacenamiento Firebase, porque esta HU no gestiona archivos.
- **Trazabilidad:** el PRD no exige auditar la composición del comité bajo RF-35 (no es una decisión sobre un postulante o becario), pero el propio registro del comité conserva autor y fecha de su última modificación (RNF de trazabilidad).

#### Flujo de la HU

1. El coordinador abre su programa (HU-11.1) y elige "Comité evaluador".
2. El sistema verifica que el coordinador sea responsable de ese programa (HU-37.1).
3. Agrega uno o más evaluadores, internos o externos, entre los usuarios con rol de evaluador dados de alta por Dirección (S1).
4. Designa a uno de ellos como jefatura del comité (S12).
5. Guarda.
6. El sistema verifica que el comité tenga al menos un evaluador, sin duplicados, y con exactamente una jefatura designada, y lo asigna al programa.

#### Flujos alternos y de error

- FA-1 (paso 6) Comité vacío: el sistema no guarda el comité e informa que necesita al menos un evaluador.
- FA-2 (paso 3) Evaluador duplicado: el sistema no agrega un segundo registro de la misma persona e informa que ya forma parte del comité.
- FA-3 (paso 6) Comité sin jefatura designada: el sistema no guarda el comité y pide designar la jefatura.
- FA-4 (paso 2) Programa que no coordina: el sistema bloquea el acceso a la pantalla del comité.
- FA-5 (paso 1) Programa sin comité: el sistema informa que el programa aún no tiene comité evaluador y permite conformarlo.

#### Criterios de aceptación (9 escenarios)

##### Escenario 1: Comité conformado

- **DADO QUE** Ana es evaluadora interna y Luis es evaluador externo, ambos con acceso a la plataforma
- **CUANDO** Carlos los agrega al comité de "Becas Ingeniería", designa a Luis como jefatura del comité y guarda
- **ENTONCES** el comité de "Becas Ingeniería" queda con Ana y Luis, y Luis como jefatura del comité

##### Escenario 2: Comité sin evaluadores

- **DADO QUE** Carlos abre el comité de "Becas Salud"
- **CUANDO** guarda sin agregar evaluadores
- **ENTONCES** el sistema no guarda y muestra "El comité debe tener al menos un evaluador."

##### Escenario 3: Programa sin comité

- **DADO QUE** "Becas Salud" no tiene comité
- **CUANDO** Carlos abre "Comité evaluador" en ese programa
- **ENTONCES** el sistema muestra "Este programa aún no tiene comité evaluador."

##### Escenario 4: Comité sin jefatura designada

- **DADO QUE** Carlos agregó a Ana y a Luis al comité de "Becas Ingeniería"
- **CUANDO** guarda sin designar la jefatura del comité
- **ENTONCES** el sistema no guarda y muestra "Designa a un miembro del comité como jefatura."

##### Escenario 5: Evaluador duplicado

- **DADO QUE** Carlos ya agregó a Ana al comité de "Becas Ingeniería"
- **CUANDO** intenta agregarla nuevamente antes de guardar
- **ENTONCES** el sistema no agrega un segundo registro de Ana y muestra "Ana ya forma parte de este comité."

##### Escenario 6: Comité de un programa que no coordina

- **DADO QUE** Carlos coordina "Becas Ingeniería" y no coordina "Becas Salud"
- **CUANDO** intenta abrir "Comité evaluador" de "Becas Salud"
- **ENTONCES** el sistema no le muestra la pantalla y muestra "No tienes permiso para gestionar este programa."

##### Escenario 7: Usuario sin rol de evaluador

- **DADO QUE** Pablo es administrador financiero y no tiene rol de evaluador
- **CUANDO** Carlos intenta agregarlo al comité de "Becas Ingeniería"
- **ENTONCES** el sistema no lo agrega y muestra "Solo se pueden agregar usuarios con rol de evaluador."

##### Escenario 8: Más de una jefatura designada

- **DADO QUE** Carlos agregó a Ana y a Luis al comité de "Becas Ingeniería"
- **CUANDO** guarda el comité con Ana y Luis designados como jefatura
- **ENTONCES** el sistema no guarda y muestra "El comité solo puede tener una jefatura."

##### Escenario 9: Autor y fecha de la última modificación del comité

- **DADO QUE** Carlos guardó el comité de "Becas Ingeniería" el 10-oct-2026
- **CUANDO** consulta el comité de "Becas Ingeniería"
- **ENTONCES** el registro del comité muestra a Carlos y el 10-oct-2026 como autor y fecha de su última modificación

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Comité con uno o más evaluadores internos/externos | Regla (RF-06) | 1 | CUBIERTA |
| Comité no puede quedar vacío (≥1 evaluador) | Regla / FA-1 | 2 | CUBIERTA |
| Dirección crea las cuentas; el coordinador elige entre ellas | Regla (S1) | — | DELEGADA (HU-37.1, Escenario 1) |
| Solo se agregan usuarios con rol de evaluador, validado también al agregar | Regla (RF-06, RF-37) | 7 | CUBIERTA |
| No se puede duplicar a un evaluador en el comité | Regla / FA-2 | 5 | CUBIERTA |
| Se designa una jefatura al conformar el comité | Regla (S12) | 1 | CUBIERTA |
| Jefatura única y obligatoria (nunca 0 ni más de 1) | Regla / FA-3 | 1, 4, 8 | CUBIERTA |
| Coordinador solo gestiona el comité de programas que coordina | Regla / FA-4 | 6 | CUBIERTA |
| Programa sin comité | FA-5 | 3 | CUBIERTA |
| Trazabilidad: autor y fecha de la última modificación del comité | Regla técnica (RNF) | 9 | CUBIERTA |
| Autorización verificada en el servidor, no ocultando la opción | Regla técnica | 6 (resultado) | NO VERIFICABLE EN CRITERIOS (la vía de verificación es implementación) |
| 2FA en el login de coordinador y evaluadores; el segundo factor de los evaluadores externos llega por correo | Restricción del cliente | — | DELEGADA (HU-37.2, transversal) |
| ¿Puede modificarse un comité ya conformado? | — | — | PENDIENTE DE DEFINICIÓN |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-06.1).*

### HU-06.2 Asignar los evaluadores de cada postulación

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-06 | Debe tener | HU-04.1, HU-06.1, HU-39.1 | HU-06.2, HU-06.3, HU-06.4 |

#### Narrativa

- Como coordinador de programa
- Quiero asignar cada postulación elegible a al menos dos evaluadores del comité, autorizar que se evalúe con uno solo cuando no es posible asignar dos, impedir que la revise un evaluador que declaró un conflicto de interés con el postulante y poner su lugar en manos de otro evaluador del comité
- Para que cada postulación reciba evaluaciones independientes e imparciales sin detener su evaluación

#### Reglas de negocio

- Cada postulación recibe al menos dos evaluaciones independientes, salvo que el coordinador autorice una excepción (RF-06).
- Solo se asignan postulaciones *En evaluación*; las *No elegible* no ocupan tiempo del comité (RF-04). El control de las transiciones de estado de la postulación es de HU-04.1; aquí solo se filtra por el estado vigente.
- Solo se asignan miembros del comité del programa de esa postulación (RF-06; comité en HU-06.1).
- Solo el coordinador puede autorizar que una postulación no reciba dos evaluaciones (RF-06).
- La autorización queda registrada en la postulación con quién la autorizó y cuándo (RNF de trazabilidad; registro permanente en HU-35.1).
- Con la excepción, la consolidación usa la única evaluación asignada (HU-09.1).
- Un evaluador no puede revisar una postulación si declara un conflicto de interés con esa persona (RF-06).
- Toda persona con poder de decisión sobre una postulación declara el conflicto antes de participar en su evaluación (regla transversal; la declaración está en HU-39.1).
- Si el evaluador ya estaba asignado, pierde el acceso a esa postulación y **su lugar pasa a otro evaluador del comité**: el coordinador asigna un reemplazo entre los miembros del comité del programa que no tengan conflicto declarado con ese postulante. Mientras no haya reemplazo y queden menos de dos evaluadores, vuelve a regir la exigencia de al menos dos evaluadores o una excepción autorizada (RF-06).
- El reemplazo no es un miembro nuevo del comité: se elige entre los que ya lo integran (RF-06; comité en HU-06.1).
- **Confidencialidad:** quedar asignado sin conflicto habilita ver la postulación completa, incluida la información socioeconómica; esa visibilidad se revoca en el mismo instante en que se detecta el conflicto (RNF de confidencialidad).
- **Restricciones técnicas del cliente:** 2FA de login para coordinador y evaluadores; el segundo factor de los evaluadores externos se envía por correo. No aplica OTP por acción ni Firebase en esta HU.
- El aviso al evaluador está en HU-31.1.
- **Sin definir en el PRD:** qué pasa con las puntuaciones que el evaluador ya registró o envió antes de declarar el conflicto. La respuesta 17 deja ese detalle explícitamente por confirmar, así que los escenarios no lo cubren (ver Preguntas abiertas).

#### Flujo de la HU

1. El coordinador abre las postulaciones *En evaluación* del ciclo.
2. Elige una postulación.
3. Selecciona evaluadores del comité del programa.
4. El sistema revisa los conflictos declarados (HU-39.1). Si un evaluador tiene conflicto con ese postulante, el sistema no permite asignarlo.
5. Guarda. Si asignó un solo evaluador, elige "Autorizar excepción" y confirma.
6. El sistema verifica, dentro de la misma transacción, que haya al menos dos evaluadores o una excepción autorizada, y registra la asignación. Si hay excepción, la registra con quién la autorizó (HU-35.1).
7. Si un evaluador declara un conflicto después de la asignación, el sistema le quita el acceso a esa postulación y la deja pendiente de reasignación.
8. El coordinador asigna, en su lugar, a otro evaluador del comité sin conflicto con ese postulante, o autoriza la excepción.

#### Flujos alternos y de error

- FA-1 (paso 6) Menos de dos evaluadores sin excepción: el sistema no guarda la asignación.
- FA-2 (paso 1) Sin postulaciones en evaluación: el sistema muestra un mensaje de lista vacía.
- FA-3 (paso 5) El coordinador no confirma la excepción: el sistema no guarda la asignación.
- FA-4 (paso 7) Quedan menos de dos evaluadores: la postulación aparece como pendiente de reasignación hasta que el coordinador ponga a otro evaluador del comité en el lugar del que declaró el conflicto o autorice la excepción.
- FA-5 (paso 3) Evaluador que no pertenece al comité del programa: el sistema no lo asigna.
- FA-6 (paso 2) Postulación que no está *En evaluación*: el sistema no permite asignarle evaluadores.
- FA-7 (paso 8) Ningún otro miembro del comité puede tomar el lugar —todos los demás tienen conflicto declarado con ese postulante—: la postulación sigue pendiente de reasignación y la única vía para avanzar es la excepción autorizada del coordinador (RF-06).

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-06.2).*

#### Criterios de aceptación (11 escenarios)

##### Escenario 1: Dos evaluadores asignados sin conflicto

- **DADO QUE** la postulación de María está *En evaluación*, y Ana y Luis son del comité de "Becas Ingeniería" y no tienen conflictos declarados con María
- **CUANDO** Carlos asigna a Ana y a Luis y guarda
- **ENTONCES** la postulación queda asignada a ambos

##### Escenario 2: Un solo evaluador sin excepción

- **DADO QUE** la postulación de María está *En evaluación*
- **CUANDO** Carlos asigna solo a Ana y guarda sin autorizar una excepción
- **ENTONCES** el sistema no guarda y muestra "Cada postulación necesita al menos dos evaluadores. Asigna otro o autoriza una excepción."

##### Escenario 3: Excepción autorizada

- **DADO QUE** Carlos asignó solo a Ana a la postulación de María
- **CUANDO** autoriza la excepción y confirma
- **ENTONCES** la asignación queda guardada con Ana como única evaluadora y la excepción registrada a nombre de Carlos

##### Escenario 4: Solo el coordinador autoriza

- **DADO QUE** Ana es miembro del comité de "Becas Ingeniería"
- **CUANDO** intenta autorizar la excepción en la postulación de María
- **ENTONCES** el sistema no lo permite y muestra "Solo el coordinador del programa puede autorizar esta excepción."

##### Escenario 5: Asignación bloqueada por conflicto

- **DADO QUE** Luis declaró un conflicto de interés con María
- **CUANDO** Carlos intenta asignarlo a la postulación de María
- **ENTONCES** el sistema no lo asigna y muestra "Luis declaró un conflicto de interés con esta postulante y no puede evaluarla."

##### Escenario 6: Conflicto declarado después de la asignación

- **DADO QUE** Luis estaba asignado a la postulación de María y luego declaró un conflicto con ella
- **CUANDO** Luis intenta abrir esa postulación
- **ENTONCES** el sistema no se la muestra y muestra "Declaraste un conflicto de interés con este postulante. No puedes participar en su evaluación."

##### Escenario 7: Postulación no elegible y lista vacía

- **DADO QUE** ninguna postulación del ciclo está *En evaluación* y la postulación de Luis está *No elegible*
- **CUANDO** Carlos abre la asignación
- **ENTONCES** la postulación de Luis no aparece y el sistema muestra "No hay postulaciones en evaluación para asignar."

##### Escenario 8: Evaluador fuera del comité del programa

- **DADO QUE** Marta es evaluadora del comité de "Becas Salud" pero no de "Becas Ingeniería"
- **CUANDO** Carlos intenta asignarla a una postulación de "Becas Ingeniería"
- **ENTONCES** el sistema no la asigna y muestra "Marta no es miembro del comité de este programa."

##### Escenario 9: Postulación que ya no está en evaluación

- **DADO QUE** la postulación de Pedro está *Otorgada*
- **CUANDO** Carlos intenta asignarle evaluadores
- **ENTONCES** el sistema no lo permite y muestra "Esta postulación ya no está en evaluación."

##### Escenario 10: Postulación pendiente de reasignación tras un conflicto posterior

- **DADO QUE** Ana y Luis estaban asignados a la postulación de María sin excepción autorizada y Luis declaró un conflicto de interés con ella (HU-39.1)
- **CUANDO** Carlos abre la asignación de evaluadores del ciclo
- **ENTONCES** la postulación de María aparece con una sola evaluadora, Ana, y marcada "Pendiente de reasignación: asigna a otro evaluador del comité en lugar de Luis o autoriza una excepción."

##### Escenario 11: El lugar del evaluador con conflicto pasa a otro evaluador del comité

- **DADO QUE** la postulación de María quedó pendiente de reasignación con Ana como única evaluadora, y Elena es miembro del comité de "Becas Ingeniería" sin conflicto declarado con María
- **CUANDO** Carlos asigna a Elena en el lugar de Luis y guarda
- **ENTONCES** la postulación de María queda asignada a Ana y a Elena y deja de estar marcada como pendiente de reasignación

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Al menos dos evaluaciones independientes, salvo excepción autorizada | Regla (RF-06) | 1, 2, 3 | CUBIERTA |
| Solo se asignan postulaciones *En evaluación* | Regla / FA-6 | 7, 9 | CUBIERTA |
| Solo se asignan miembros del comité del programa | Regla / FA-5 | 8 | CUBIERTA |
| Solo el coordinador autoriza la excepción | Regla (RF-06) | 4 | CUBIERTA |
| La autorización se registra con quién la autorizó y cuándo | Regla (RNF) | 3 (quién) | CUBIERTA PARCIAL — el "cuándo" se verifica en HU-35.1 |
| Con excepción, la consolidación usa la única evaluación | Regla | — | DELEGADA (HU-09.1, Escenario 4) |
| Evaluador con conflicto declarado no puede ser asignado | Regla (RF-06) | 5 | CUBIERTA |
| Declaración del conflicto antes de participar | Regla transversal | — | DELEGADA (HU-39.1) |
| Conflicto declarado tras la asignación retira el acceso | Regla (RF-06) | 6 | CUBIERTA |
| El lugar del evaluador con conflicto pasa a otro evaluador del comité | Regla (R17) | 10, 11 | CUBIERTA |
| Si quedan menos de dos evaluadores, vuelve a regir el mínimo hasta la reasignación o la excepción | Regla / FA-4 | 10, 11 | CUBIERTA |
| Ningún otro miembro del comité puede tomar el lugar: solo queda la excepción autorizada | FA-7 | 2, 3 (resultado observable: sin dos evaluadores no se guarda sin excepción) | CUBIERTA PARCIAL |
| Sin postulaciones en evaluación (lista vacía) | FA-2 | 7 | CUBIERTA |
| El coordinador no confirma la excepción | FA-3 | 2 | CUBIERTA (mismo resultado observable: no se guarda la asignación) |
| El acceso del evaluador se calcula al abrir, no se cachea como permiso | Regla técnica | 6 (resultado) | NO VERIFICABLE EN CRITERIOS |
| Verificación transaccional del mínimo bajo asignaciones simultáneas | Regla técnica (RNF) | — | NO VERIFICABLE EN CRITERIOS (pruebas de concurrencia) |
| Confidencialidad: la asignación sin conflicto habilita ver la información socioeconómica | Regla (RNF) | 6 (revocación) | CUBIERTA PARCIAL — el alcance del dato se verifica en HU-37.1, Escenario 6 |
| 2FA en el login; segundo factor de evaluadores externos por correo | Restricción del cliente | — | DELEGADA (HU-37.2, transversal) |
| ¿Qué pasa con las puntuaciones previas al conflicto? | — | — | PENDIENTE DE DEFINICIÓN (la respuesta 17 lo deja por confirmar) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-06.2).*

## RF-07 — Rúbrica de evaluación configurable por programa · *Debe tener*

### HU-07.1 Definir la rúbrica del programa con pesos que sumen 100 %

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-07 | Debe tener | HU-11.1 | HU-07.1, HU-07.2 |

#### Narrativa

- Como coordinador de programa
- Quiero definir los criterios de evaluación de mi programa, con el peso y el rango de puntuación de cada uno, y que el sistema valide que los pesos sumen 100 %
- Para que el comité evalúe con criterios claros y la puntuación de cada postulación sea coherente

#### Reglas de negocio

- Cada programa define sus propios criterios de evaluación y el peso de cada uno, por ejemplo mérito académico, necesidad socioeconómica o ensayo (RF-07).
- Cada criterio tiene un rango de puntuación con un valor mínimo menor que el máximo (RF-08).
- Los pesos de todos los criterios del programa deben sumar 100 % (RF-07).
- El sistema no acepta la rúbrica mientras la suma no sea 100 % (RF-07).
- La rúbrica no se modifica una vez iniciada la evaluación del ciclo (ver HU-07.2).
- El equipo financiero no puede modificar rúbricas de evaluación (RF-37; ver HU-37.1).
- Un coordinador solo define la rúbrica de los programas de los que es responsable (RF-37; ver HU-37.1).
- **Restricciones técnicas del cliente:** 2FA de login para el coordinador. No aplica OTP por acción ni Firebase en esta HU.

#### Flujo de la HU

1. El coordinador abre su programa y elige "Rúbrica de evaluación".
2. El sistema verifica que el coordinador sea responsable de ese programa y que su rol pueda modificar rúbricas (HU-37.1).
3. Agrega un criterio con su nombre, su peso y su rango de puntuación (mínimo y máximo).
4. Repite para cada criterio.
5. El sistema muestra la suma actual de los pesos.
6. El coordinador guarda. El sistema valida los datos y la suma de pesos.
7. Si la suma es 100 %, el sistema acepta la rúbrica.
8. Si no, el sistema no la acepta e informa la suma actual.

#### Flujos alternos y de error

- FA-1 (paso 3) Rango inválido: el sistema no acepta el criterio.
- FA-2 (paso 3) Criterio sin nombre: el sistema no acepta el criterio.
- FA-3 (paso 8) Suma distinta de 100 %: el coordinador ajusta los pesos y vuelve a guardar.
- FA-4 (paso 3) Rango con mínimo igual al máximo: el sistema no acepta el criterio.
- FA-5 (paso 2) Rúbrica de un programa que no coordina: el sistema bloquea el acceso.
- FA-6 (paso 2) Rol sin permiso para modificar rúbricas: el sistema bloquea la modificación (ver HU-37.1).

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-07.1).*

#### Criterios de aceptación (8 escenarios)

##### Escenario 1: Rúbrica definida con pesos que suman 100 %

- **DADO QUE** Carlos abre la rúbrica de "Becas Ingeniería"
- **CUANDO** agrega "Mérito académico" con peso 40 %, "Necesidad socioeconómica" con peso 40 % y "Ensayo" con peso 20 %, todos con rango de 0 a 10, y guarda
- **ENTONCES** el sistema acepta la rúbrica y la guarda con esos tres criterios

##### Escenario 2: Rango inválido

- **DADO QUE** Carlos agrega el criterio "Ensayo"
- **CUANDO** indica un rango de 10 a 0
- **ENTONCES** el sistema no lo acepta y muestra "El valor mínimo del rango debe ser menor que el máximo."

##### Escenario 3: Suma menor

- **DADO QUE** los pesos de la rúbrica son 40 %, 30 % y 20 %
- **CUANDO** Carlos guarda
- **ENTONCES** el sistema no acepta la rúbrica y muestra "Los pesos suman 90 %. Deben sumar 100 %."

##### Escenario 4: Suma mayor

- **DADO QUE** los pesos de la rúbrica son 50 %, 40 % y 20 %
- **CUANDO** Carlos guarda
- **ENTONCES** el sistema no acepta la rúbrica y muestra "Los pesos suman 110 %. Deben sumar 100 %."

##### Escenario 5: Finanzas no modifica rúbricas

- **DADO QUE** Pablo es administrador financiero
- **CUANDO** intenta modificar la rúbrica de "Becas Ingeniería"
- **ENTONCES** el sistema no lo permite, muestra "Tu rol no permite modificar rúbricas de evaluación." y la rúbrica queda igual

##### Escenario 6: Rango con mínimo igual al máximo

- **DADO QUE** Carlos agrega el criterio "Entrevista"
- **CUANDO** indica un rango de 5 a 5
- **ENTONCES** el sistema no lo acepta y muestra "El valor mínimo del rango debe ser menor que el máximo."

##### Escenario 7: Rúbrica de un programa que no coordina

- **DADO QUE** Carlos coordina "Becas Ingeniería" y no coordina "Becas Salud"
- **CUANDO** intenta modificar la rúbrica de "Becas Salud"
- **ENTONCES** el sistema no le muestra la pantalla y muestra "No tienes permiso para gestionar este programa."

##### Escenario 8: Criterio sin nombre

- **DADO QUE** Carlos agrega un criterio sin indicar un nombre
- **CUANDO** intenta guardarlo con peso 20 % y rango de 0 a 10
- **ENTONCES** el sistema no lo acepta y muestra "El criterio necesita un nombre."

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| El programa define sus propios criterios y el peso de cada uno | Regla (RF-07) | 1 | CUBIERTA |
| Cada criterio tiene un rango con mínimo menor que el máximo | Regla / FA-1, FA-4 | 2, 6 | CUBIERTA |
| Los pesos deben sumar 100 %; si no, no se acepta la rúbrica | Regla / FA-3 | 1, 3, 4 | CUBIERTA |
| Criterio sin nombre | FA-2 | 8 | CUBIERTA |
| La rúbrica no se modifica una vez iniciada la evaluación | Regla (RF-07) | — | DELEGADA (HU-07.2) |
| Finanzas no puede modificar rúbricas | Regla (RF-37) / FA-6 | 5 | CUBIERTA |
| Coordinador solo define la rúbrica de programas que coordina | Regla / FA-5 | 7 | CUBIERTA |
| Autorización verificada en el servidor | Regla técnica | 5, 7 (resultado) | NO VERIFICABLE EN CRITERIOS |
| Modelo extensible de criterios por programa | Regla técnica | 1 (resultado) | NO VERIFICABLE EN CRITERIOS |
| Suma validada con aritmética decimal exacta | Regla técnica | — | NO VERIFICABLE EN CRITERIOS mientras no se defina la precisión de los pesos (pregunta abierta de la HU) |
| 2FA en el login del coordinador | Restricción del cliente | — | DELEGADA (HU-37.2, transversal) |
| ¿Precisión de los pesos y mínimo/máximo de criterios? | — | — | PENDIENTE DE DEFINICIÓN |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-07.1).*

### HU-07.2 Bloquear la rúbrica al comenzar la evaluación del ciclo

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-07 | Debe tener | HU-07.1, HU-08.1 | HU-07.3 |

#### Narrativa

- Como coordinador de programa
- Quiero que la rúbrica no se pueda modificar una vez que comenzó la evaluación del ciclo
- Para que todas las postulaciones del ciclo se evalúen con los mismos criterios

#### Reglas de negocio

- La rúbrica no puede modificarse una vez que la evaluación de un ciclo ya comenzó (RF-07).
- El bloqueo alcanza cualquier cambio de la rúbrica: agregar criterios, eliminarlos, cambiar sus pesos o sus rangos (RF-07).
- **Restricciones técnicas del cliente:** 2FA de login para el coordinador. No aplica OTP por acción ni Firebase en esta HU.
- **Decisión del cliente:** el hecho que marca el inicio de la evaluación del ciclo y bloquea la rúbrica es la apertura del periodo de evaluación, que ejecuta **Dirección**. Ya no es un punto "por confirmar": la respuesta 16 fija ese disparador explícitamente. Los escenarios 1 a 4 usan casos en que el inicio es claro (ninguna postulación del ciclo en evaluación, o un evaluador que ya registró puntuaciones); el Escenario 5 cubre el disparador exacto, con el periodo recién abierto y sin ninguna puntuación registrada.
- **Sin definir en el PRD:** si el bloqueo alcanza solo al ciclo en evaluación o al programa completo cuando corre más de un ciclo a la vez. Los escenarios cubren un ciclo a la vez.

#### Flujo de la HU

1. El coordinador abre la rúbrica del programa.
2. El sistema revisa si el periodo de evaluación del ciclo ya fue abierto por Dirección.
3. Si no comenzó, el coordinador puede modificarla (HU-07.1).
4. Si comenzó, el sistema la muestra solo para consulta.

#### Flujos alternos y de error

- FA-1 (paso 4) Intento de modificación: el sistema rechaza cualquier cambio de criterios, pesos o rangos.
- FA-2 (paso 4) Intento de eliminar un criterio: el sistema lo rechaza igual que agregarlo o modificarlo.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-07.2).*

#### Criterios de aceptación (5 escenarios)

##### Escenario 1: Antes de la evaluación

- **DADO QUE** ninguna postulación del ciclo 2026-2 de "Becas Ingeniería" está *En evaluación*
- **CUANDO** Carlos cambia el peso de "Ensayo"
- **ENTONCES** el sistema acepta el cambio (sujeto a HU-07.1)

##### Escenario 2: Evaluación iniciada

- **DADO QUE** Ana ya registró puntuaciones en una postulación del ciclo 2026-2 de "Becas Ingeniería"
- **CUANDO** Carlos intenta cambiar el peso de "Ensayo"
- **ENTONCES** el sistema no lo permite y muestra "La evaluación de este ciclo ya comenzó. La rúbrica no se puede modificar."

##### Escenario 3: Agregar un criterio tras el inicio

- **DADO QUE** la evaluación del ciclo 2026-2 de "Becas Ingeniería" ya comenzó
- **CUANDO** Carlos intenta agregar el criterio "Entrevista"
- **ENTONCES** el sistema no lo permite y la rúbrica sigue igual

##### Escenario 4: Eliminar un criterio tras el inicio

- **DADO QUE** Ana ya registró puntuaciones en una postulación del ciclo 2026-2 de "Becas Ingeniería"
- **CUANDO** Carlos intenta eliminar el criterio "Ensayo"
- **ENTONCES** el sistema no lo permite y muestra "La evaluación de este ciclo ya comenzó. La rúbrica no se puede modificar."

##### Escenario 5: Rúbrica bloqueada al abrir el periodo, sin puntuaciones registradas

- **DADO QUE** Dirección abrió el periodo de evaluación del ciclo 2026-2 de "Becas Ingeniería" y ningún evaluador registró puntuaciones todavía
- **CUANDO** Carlos intenta cambiar el peso de "Ensayo"
- **ENTONCES** el sistema no lo permite y muestra "La evaluación de este ciclo ya comenzó. La rúbrica no se puede modificar."

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Antes de comenzar la evaluación, la rúbrica se puede modificar | Regla (RF-07) | 1 | CUBIERTA |
| La rúbrica no se modifica una vez comenzada la evaluación del ciclo | Regla / FA-1 | 2 | CUBIERTA |
| El bloqueo alcanza cambiar pesos o rangos | Regla (RF-07) | 2 | CUBIERTA |
| El bloqueo alcanza agregar criterios | Regla / FA-1 | 3 | CUBIERTA |
| El bloqueo alcanza eliminar criterios | Regla / FA-2 | 4 | CUBIERTA |
| La apertura del periodo de evaluación por Dirección es el hecho que bloquea la rúbrica | Regla (R16) | 1, 2, 5 | CUBIERTA |
| Único punto de bloqueo transaccional entre edición y primera puntuación | Regla técnica (RNF) | — | NO VERIFICABLE EN CRITERIOS (pruebas de concurrencia) |
| 2FA en el login del coordinador | Restricción del cliente | — | DELEGADA (HU-37.2, transversal) |
| ¿El bloqueo es por ciclo o por programa completo? | — | — | PENDIENTE DE DEFINICIÓN |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-07.2).*

## RF-08 — Registro de puntuaciones y comentarios de evaluadores · *Debe tener*

### HU-08.1 Puntuar, comentar y enviar mi evaluación

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-08 | Debe tener | HU-06.2, HU-07.1 | HU-08.1, HU-08.2 |

#### Narrativa

- Como miembro del comité evaluador
- Quiero puntuar cada criterio de la rúbrica y comentar las postulaciones que tengo asignadas, y enviar mi evaluación cuando tenga todos los criterios puntuados
- Para dejar mi evaluación de cada postulante y que cuente en la puntuación consolidada de la postulación

#### Reglas de negocio

- El evaluador puntúa y comenta cada postulación que tiene asignada (RF-08).
- Cada puntuación respeta el rango definido para ese criterio, con los valores mínimo y máximo incluidos (RF-08; rango en HU-07.1).
- El evaluador puede guardar su avance antes de enviar la evaluación completa (RF-08).
- Un evaluador no puede enviar su evaluación sin puntuar todos los criterios de la rúbrica (RF-08).
- Las evaluaciones son independientes: la de un evaluador no modifica la de otro (RF-06).
- Dos evaluadores que califican la misma postulación al mismo tiempo no producen datos contradictorios (RNF de consistencia bajo concurrencia).
- El evaluador con conflicto de interés no accede a la postulación (HU-06.2).
- La consolidación empieza cuando todos los evaluadores asignados envían (HU-09.1).
- **Confidencialidad:** el evaluador necesita ver la información socioeconómica del postulante para puntuar criterios como necesidad socioeconómica; esa visibilidad se acota a las postulaciones donde está asignado y sin conflicto vigente (RNF de confidencialidad; ver HU-06.2).
- **Restricciones técnicas del cliente:** el evaluador tiene acceso a información de decisión, por lo que su inicio de sesión exige 2FA; el segundo factor de los evaluadores externos se envía por correo. No aplica OTP por acción. El PRD no pide adjuntos en los comentarios del evaluador; si se agregaran, deberían ir al mismo servicio de archivos que ya usa la Fundación (ver Restricciones Técnicas del Cliente y HU-03.1).
- El retiro de una postulación antes del inicio de la evaluación se rige por HU-05.1; esta HU no define qué ocurre si una postulación asignada cambia de estado.

#### Flujo de la HU

1. El evaluador abre su lista de postulaciones asignadas.
2. Abre una postulación. El sistema recalcula su acceso cruzando la asignación vigente con los conflictos declarados (HU-06.2).
3. El sistema muestra los criterios de la rúbrica con su rango.
4. El evaluador registra una puntuación por criterio y sus comentarios.
5. Guarda. El sistema valida los rangos y conserva el avance.
6. Revisa sus puntuaciones y comentarios y pulsa "Enviar evaluación".
7. El sistema verifica, contra la rúbrica vigente, que todos los criterios estén puntuados.
8. El sistema registra la evaluación como enviada.

#### Flujos alternos y de error

- FA-1 (paso 5) Puntuación fuera de rango: el sistema no la acepta.
- FA-2 (paso 1) Sin asignaciones: el sistema muestra un mensaje de lista vacía: "No tienes postulaciones asignadas."
- FA-3 (paso 7) Falta un criterio: el sistema no envía la evaluación e indica qué criterio falta.
- FA-4 (paso 2) Postulación no asignada o con conflicto declarado: el sistema no se la muestra (ver HU-06.2).

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-08.1).*

#### Criterios de aceptación (8 escenarios)

##### Escenario 1: Puntuación registrada

- **DADO QUE** Ana tiene asignada la postulación de María y "Ensayo" tiene rango de 0 a 10
- **CUANDO** registra 8 en "Ensayo" con el comentario "Ensayo claro y bien argumentado", y guarda
- **ENTONCES** el sistema guarda la puntuación y el comentario

##### Escenario 2: Fuera de rango

- **DADO QUE** "Ensayo" tiene rango de 0 a 10
- **CUANDO** Ana registra 12 en "Ensayo"
- **ENTONCES** el sistema no la acepta y muestra "La puntuación de 'Ensayo' debe estar entre 0 y 10."

##### Escenario 3: Postulación no asignada

- **DADO QUE** Ana no tiene asignada la postulación de Luis
- **CUANDO** intenta abrirla
- **ENTONCES** el sistema no se la muestra y muestra "No tienes asignada esta postulación."

##### Escenario 4: Evaluación enviada

- **DADO QUE** Ana puntuó los tres criterios de la rúbrica en la postulación de María
- **CUANDO** pulsa "Enviar evaluación"
- **ENTONCES** el sistema registra su evaluación como enviada

##### Escenario 5: Falta un criterio

- **DADO QUE** Ana no puntuó "Necesidad socioeconómica"
- **CUANDO** pulsa "Enviar evaluación"
- **ENTONCES** el sistema no la envía y muestra "Falta puntuar: Necesidad socioeconómica."

##### Escenario 6: Dos evaluadores al mismo tiempo

- **DADO QUE** Ana y Luis evalúan la postulación de María
- **CUANDO** ambos envían su evaluación al mismo tiempo
- **ENTONCES** el sistema guarda las dos evaluaciones completas, cada una con sus propias puntuaciones, sin que una altere la otra

##### Escenario 7: Puntuaciones en los límites exactos del rango

- **DADO QUE** "Ensayo" y "Mérito académico" tienen rango de 0 a 10
- **CUANDO** Ana registra 0 en "Ensayo" y 10 en "Mérito académico", y guarda
- **ENTONCES** el sistema acepta ambas puntuaciones, porque los valores mínimo y máximo del rango están incluidos

##### Escenario 8: Sin postulaciones asignadas

- **DADO QUE** Ana no tiene ninguna postulación asignada
- **CUANDO** abre su lista de postulaciones asignadas
- **ENTONCES** el sistema muestra "No tienes postulaciones asignadas."

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| El evaluador puntúa y comenta cada postulación asignada | Regla (RF-08) | 1 | CUBIERTA |
| La puntuación respeta el rango del criterio, límites incluidos | Regla / FA-1 | 2, 7 | CUBIERTA |
| El evaluador puede guardar su avance antes de enviar | Regla (RF-08) | 1 | CUBIERTA |
| No puede enviar sin puntuar todos los criterios | Regla / FA-3 | 4, 5 | CUBIERTA |
| Las evaluaciones de distintos evaluadores son independientes | Regla (RF-06) | 6 | CUBIERTA |
| Dos evaluadores califican a la vez sin datos contradictorios | Regla técnica (RNF) | 6 | CUBIERTA (el riesgo real de concurrencia está en el disparador de HU-09.1) |
| Sin postulaciones asignadas (lista vacía) | FA-2 | 8 | CUBIERTA |
| Postulación no asignada no se muestra | FA-4 | 3 | CUBIERTA |
| Evaluador con conflicto de interés no accede | Regla (RF-06) | — | DELEGADA (HU-06.2, Escenario 6) |
| Visibilidad de la información socioeconómica acotada a la asignación sin conflicto | Regla (RNF) | — | DELEGADA (HU-06.2 y HU-37.1, Escenario 6) |
| Validación de "todos los criterios puntuados" contra la rúbrica vigente al enviar | Regla técnica | 4, 5 (resultado) | NO VERIFICABLE EN CRITERIOS (el riesgo de rúbrica cambiante lo elimina el bloqueo de HU-07.2) |
| Modelo: una evaluación por par (evaluador, postulación), con borrador y enviada | Regla técnica | 1, 4, 6 (resultado) | NO VERIFICABLE EN CRITERIOS |
| 2FA en el login del evaluador; segundo factor de evaluadores externos por correo | Restricción del cliente | — | DELEGADA (HU-37.2, transversal) |
| ¿Puede modificarse una evaluación ya enviada? | — | — | PENDIENTE DE DEFINICIÓN |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-08.1).*

### HU-08.2 Ocultar las puntuaciones de otros evaluadores hasta el cierre

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-08 | Debe tener | HU-08.1 | HU-08.3 |

#### Narrativa

- Como miembro del comité evaluador
- Quiero no ver las puntuaciones de los demás evaluadores mientras el periodo de revisión está abierto
- Para evaluar sin influencias

#### Reglas de negocio

- Las puntuaciones de un evaluador no son visibles para otros evaluadores hasta que cierra el periodo de revisión (RF-08).
- Cada evaluador ve sus propias puntuaciones.
- Al cerrar el periodo de revisión, cada evaluador puede ver las puntuaciones de los demás evaluadores de esa postulación. RF-08 prevalece sobre la frase general de RF-37 (S9; ver HU-37.1).
- El inicio y el cierre de los periodos los ejecuta **Dirección**; no los ejecuta el comité ni el evaluador.
- Las fechas límite se interpretan en la zona horaria **UTC-4** y el día de cierre se incluye completo: el periodo de revisión sigue abierto hasta las **23:59** de su día de cierre, y las puntuaciones de los demás evaluadores siguen ocultas hasta ese momento.
- **Restricciones técnicas del cliente:** 2FA de login para los evaluadores; el segundo factor de los evaluadores externos se envía por correo. No aplica OTP por acción ni Firebase en esta HU.
- **Sin definir en el PRD:** el cliente confirmó que **Dirección** abre y cierra el periodo de revisión, pero sigue sin definir si ese cierre es el mismo evento que "todos los evaluadores enviaron" (que dispara la consolidación en HU-09.1) o una acción manual distinta. Ver Preguntas abiertas.

#### Flujo de la HU

1. El evaluador abre una postulación asignada.
2. El sistema muestra sus propias puntuaciones y comentarios.
3. Mientras el periodo de revisión esté abierto, el sistema no muestra las puntuaciones de los demás evaluadores, ni en la pantalla ni en la respuesta del servidor.
4. Cuando cierra el periodo de revisión —al pasar las 23:59 (UTC-4) de su día de cierre o cuando lo cierra **Dirección**—, el sistema muestra también las puntuaciones de los demás evaluadores (S9).

#### Flujos alternos y de error

- FA-1 (paso 3) Otro evaluador ya envió: sus puntuaciones siguen ocultas.
- FA-2 (paso 3) Acceso directo a la evaluación de otro evaluador por enlace o identificador: el sistema niega el acceso igual que desde la interfaz.
- FA-3 (paso 3) Consulta durante el día de cierre, antes de las 23:59 (UTC-4): el periodo sigue abierto y las puntuaciones de los demás siguen ocultas.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-08.2).*

#### Criterios de aceptación (7 escenarios)

##### Escenario 1: Ve sus propias puntuaciones

- **DADO QUE** Ana registró sus puntuaciones en la postulación de María
- **CUANDO** abre esa postulación
- **ENTONCES** ve sus puntuaciones y comentarios

##### Escenario 2: Puntuaciones de otro ocultas

- **DADO QUE** Luis registró puntuaciones en la postulación de María y el periodo de revisión está abierto
- **CUANDO** Ana abre esa postulación
- **ENTONCES** no ve las puntuaciones de Luis

##### Escenario 3: Oculta aunque el otro ya envió

- **DADO QUE** Luis envió su evaluación de la postulación de María y el periodo de revisión está abierto
- **CUANDO** Ana abre esa postulación
- **ENTONCES** no ve las puntuaciones de Luis

##### Escenario 4: Puntuaciones visibles tras el cierre

- **DADO QUE** Luis envió su evaluación de la postulación de María y el periodo de revisión ya cerró
- **CUANDO** Ana abre esa postulación
- **ENTONCES** ve las puntuaciones de Luis (S9)

##### Escenario 5: Acceso directo bloqueado

- **DADO QUE** Ana registró puntuaciones en la postulación de María y el periodo de revisión está abierto
- **CUANDO** Luis intenta acceder directamente, por enlace o identificador, a la evaluación registrada por Ana para esa postulación
- **ENTONCES** el sistema le niega el acceso igual que si lo intentara desde la interfaz

##### Escenario 6: El día de cierre cuenta completo

- **DADO QUE** el periodo de revisión del ciclo 2026-2 de "Becas Ingeniería" cierra el 15-oct-2026 y Luis ya envió su evaluación de la postulación de María
- **CUANDO** Ana abre esa postulación a las 22:00 (UTC-4) del 15-oct-2026
- **ENTONCES** no ve las puntuaciones de Luis, porque el periodo sigue abierto hasta las 23:59 (UTC-4) de ese día

##### Escenario 7: Visibles al pasar el día de cierre

- **DADO QUE** el periodo de revisión del ciclo 2026-2 de "Becas Ingeniería" cierra el 15-oct-2026 y Luis ya envió su evaluación de la postulación de María
- **CUANDO** Ana abre esa postulación a las 00:10 (UTC-4) del 16-oct-2026
- **ENTONCES** ve las puntuaciones de Luis, porque el periodo de revisión ya cerró

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Cada evaluador ve sus propias puntuaciones | Regla (RF-08) | 1 | CUBIERTA |
| Las puntuaciones de otro evaluador están ocultas mientras el periodo está abierto | Regla (RF-08) | 2 | CUBIERTA |
| Siguen ocultas aunque el otro evaluador ya envió | FA-1 | 3 | CUBIERTA |
| Al cerrar el periodo, se ven las puntuaciones de los demás | Regla (S9) | 4, 7 | CUBIERTA |
| Fechas límite en UTC-4, con el día de cierre incluido hasta las 23:59 | Regla (R20) / FA-3 | 6, 7 | CUBIERTA |
| La comparación de "periodo abierto" usa la hora del servidor en UTC-4 | Regla técnica | 6, 7 (resultado) | NO VERIFICABLE EN CRITERIOS |
| El inicio y el cierre de los periodos los ejecuta **Dirección** | Regla (R16) | 4 (resultado del cierre) | CUBIERTA PARCIAL — confirmado el rol (Dirección); sigue por confirmar si ese cierre coincide con la consolidación |
| El ocultamiento se aplica en el servidor, no solo en la interfaz | Regla técnica (RNF) | 5 | CUBIERTA |
| El acceso directo por enlace o identificador también se bloquea | FA-2 | 5 | CUBIERTA |
| 2FA en el login de los evaluadores; segundo factor de evaluadores externos por correo | Restricción del cliente | — | DELEGADA (HU-37.2, transversal) |
| ¿Es el cierre del periodo por Dirección el mismo evento que la consolidación? | — | — | PENDIENTE DE DEFINICIÓN (el ejecutor ya se sabe: Dirección; falta si coincide con la consolidación) |
| ¿Puede el coordinador ver puntuaciones individuales antes del cierre? | — | — | PENDIENTE DE DEFINICIÓN |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-08.2).*

## RF-09 — Consolidación y ranking de postulantes · *Debe tener*

### HU-09.1 Calcular la puntuación consolidada y marcar las diferencias grandes

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-09 | Debe tener | HU-08.1 | HU-09.1, HU-09.2 |

#### Narrativa

- Como coordinador de programa
- Quiero que el sistema calcule la puntuación consolidada de cada postulación cuando todos sus evaluadores terminen, y que marque las postulaciones con una diferencia grande entre evaluadores
- Para ordenar a los postulantes elegibles del programa y revisar las diferencias grandes antes de desempatar

#### Reglas de negocio

- Una vez que todos los evaluadores asignados terminan, el sistema calcula una puntuación consolidada (RF-09).
- El método de consolidación queda definido por el programa; es configurable, por ejemplo promedio de evaluadores (RF-09).
- Con la excepción de un solo evaluador (HU-06.2), se consolida con esa única evaluación (RF-09: "todos los evaluadores asignados").
- Si el lugar de un evaluador con conflicto pasó a otro evaluador del comité (HU-06.2), la consolidación espera el envío del reemplazo: "todos los evaluadores asignados" son los vigentes en ese momento, no los originales.
- Una diferencia grande entre evaluadores debería marcarse para una revisión de desempate (RF-09).
- Qué diferencia es "grande" es un umbral configurable; el PRD no da un valor.
- **Trazabilidad:** la consolidación es un cálculo automático, no una decisión humana, pero afecta a quién queda dentro del cupo; se registra en el mismo mecanismo de auditoría identificando al sistema como autor, en vez de un registro aparte para eventos automáticos (RNF de trazabilidad; ver HU-35.1 y el criterio ya aplicado en HU-04.1).
- **Restricciones técnicas del cliente:** 2FA de login para el coordinador. No aplica OTP por acción ni Firebase en esta HU.
- **Sin definir en el PRD:** cómo se combinan los pesos y rangos de los criterios en la puntuación total de cada evaluador. El cliente mantiene pendiente esa definición (respuesta 15), así que los escenarios siguen partiendo del total de cada evaluador. Ver Preguntas abiertas.
- **Sin definir en el PRD:** en qué consiste la revisión de desempate y cómo se mide la diferencia con más de dos evaluadores.
- **Sin definir en el PRD:** si el umbral de diferencia grande es inclusivo o exclusivo en su límite. Los escenarios usan diferencias claramente por encima o por debajo del umbral y no cubren el valor exacto.

#### Flujo de la HU

1. El coordinador define el método de consolidación del programa y el umbral de diferencia grande.
2. El último evaluador asignado envía su evaluación (HU-08.1).
3. El sistema verifica, en la misma transacción del envío, que todas las evaluaciones asignadas estén enviadas y calcula la puntuación consolidada con el método definido.
4. El sistema la muestra en la postulación.
5. El sistema compara las puntuaciones totales de los evaluadores de la postulación.
6. Si la diferencia supera el umbral configurado, marca la postulación para revisión de desempate.
7. El coordinador ve la marca en la postulación y en el ranking (HU-09.2).

#### Flujos alternos y de error

- FA-1 (paso 3) Faltan evaluaciones: el sistema no calcula y muestra cuántas faltan.
- FA-2 (paso 3) Método no definido: el sistema no calcula e indica que falta definir el método.
- FA-3 (paso 6) Diferencia dentro del umbral: el sistema no marca la postulación.
- FA-4 (paso 3) Doble disparo simultáneo del cálculo: la consolidación es idempotente y la postulación queda con un único resultado.
- FA-5 (paso 3) Un evaluador fue reemplazado por conflicto de interés y su sustituto aún no envía: el sistema no calcula y la postulación sigue pendiente de ese envío (HU-06.2).

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-09.1).*

#### Criterios de aceptación (9 escenarios)

##### Escenario 1: Consolidación por promedio

- **DADO QUE** "Becas Ingeniería" usa promedio de evaluadores y Ana dio un total de 80 y Luis de 90 a la postulación de María
- **CUANDO** Luis envía su evaluación, la última pendiente
- **ENTONCES** la postulación de María muestra una puntuación consolidada de 85

##### Escenario 2: Faltan evaluaciones

- **DADO QUE** Ana envió su evaluación y Luis no
- **CUANDO** Carlos consulta la postulación de María
- **ENTONCES** no hay puntuación consolidada y el sistema muestra "Pendiente: 1 de 2 evaluaciones enviadas."

##### Escenario 3: Método no definido

- **DADO QUE** "Becas Salud" no tiene método de consolidación definido
- **CUANDO** todos los evaluadores de una postulación envían su evaluación
- **ENTONCES** el sistema no calcula y muestra "Define el método de consolidación del programa."

##### Escenario 4: Un solo evaluador por excepción

- **DADO QUE** la postulación de Pedro tiene solo a Ana asignada por excepción (HU-06.2)
- **CUANDO** Ana envía su evaluación con un total de 75
- **ENTONCES** la postulación de Pedro muestra una puntuación consolidada de 75

##### Escenario 5: Diferencia grande

- **DADO QUE** el umbral de "Becas Ingeniería" es 20 y Ana dio 60 y Luis 90 a la postulación de María
- **CUANDO** el sistema calcula la puntuación consolidada
- **ENTONCES** la postulación queda marcada con "Diferencia grande entre evaluadores: revisar para desempate"

##### Escenario 6: Diferencia pequeña

- **DADO QUE** el umbral es 20 y Ana dio 80 y Luis 85 a la postulación de Pedro
- **CUANDO** el sistema calcula la puntuación consolidada
- **ENTONCES** la postulación no queda marcada

##### Escenario 7: Doble disparo del cálculo

- **DADO QUE** "Becas Ingeniería" usa promedio de evaluadores y Ana y Luis, los dos evaluadores asignados a la postulación de María, registraron totales de 80 y 90
- **CUANDO** ambos envían su evaluación casi al mismo tiempo
- **ENTONCES** la postulación de María queda con una única puntuación consolidada de 85, registrada una sola vez

##### Escenario 8: Autor del cálculo automático

- **DADO QUE** el sistema calculó la puntuación consolidada de la postulación de María el 10-oct-2026
- **CUANDO** Carlos consulta el historial de esa postulación
- **ENTONCES** el historial muestra el cálculo de la puntuación consolidada con el sistema como autor y el 10-oct-2026 como fecha

##### Escenario 9: Consolidación con el evaluador reemplazado

- **DADO QUE** Luis declaró un conflicto con María y Carlos asignó a Elena en su lugar (HU-06.2), y Ana ya envió su evaluación con un total de 80
- **CUANDO** Elena envía su evaluación con un total de 90 y "Becas Ingeniería" usa promedio de evaluadores
- **ENTONCES** la postulación de María muestra una puntuación consolidada de 85, calculada con las evaluaciones de Ana y Elena

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Al terminar todos los evaluadores asignados, se calcula la consolidada | Regla (RF-09) | 1 | CUBIERTA |
| El método de consolidación lo define el programa | Regla (RF-09) | 1 | CUBIERTA |
| Método no definido: no se calcula | FA-2 | 3 | CUBIERTA |
| Faltan evaluaciones: no se calcula | FA-1 | 2 | CUBIERTA |
| Con excepción de un solo evaluador, se consolida con esa evaluación | Regla (RF-09) | 4 | CUBIERTA |
| "Todos los evaluadores asignados" son los vigentes tras un reemplazo por conflicto | Regla (R17) / FA-5 | 9 | CUBIERTA |
| La diferencia grande se marca para revisión de desempate | Regla (RF-09) | 5 | CUBIERTA |
| Diferencia dentro del umbral: no se marca | FA-3 | 6 | CUBIERTA |
| El umbral es configurable por programa, sin valor fijado por el PRD | Regla técnica | 5, 6 (valor de prueba) | CUBIERTA |
| Disparador idempotente ante doble disparo | Regla técnica / FA-4 | 7 | CUBIERTA (el resultado; la mecánica va a pruebas de concurrencia) |
| La consolidación automática se registra con el sistema como autor | Regla (RNF de trazabilidad) | 8 | CUBIERTA |
| 2FA en el login del coordinador | Restricción del cliente | — | DELEGADA (HU-37.2, transversal) |
| ¿Cómo se calcula el total ponderado de cada evaluador? | — | — | PENDIENTE DE DEFINICIÓN (el cliente la mantiene pendiente, respuesta 15; los escenarios parten del total, no lo calculan) |
| ¿El umbral es inclusivo o exclusivo en su límite? | — | — | PENDIENTE DE DEFINICIÓN (ningún escenario usa el valor exacto) |
| ¿En qué consiste la revisión de desempate y cómo se mide con más de dos evaluadores? | — | — | PENDIENTE DE DEFINICIÓN |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-09.1).*

### HU-09.2 Generar el ranking respetando cupo y presupuesto

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-09 | Debe tener | HU-09.1, HU-11.1, HU-12.1 | HU-09.3 |

#### Narrativa

- Como coordinador de programa
- Quiero generar el ranking de los postulantes elegibles del ciclo, respetando el cupo y el presupuesto disponible
- Para saber a quiénes se les puede otorgar la beca

#### Reglas de negocio

- El sistema ordena a los postulantes elegibles del programa por su puntuación consolidada, de mayor a menor (RF-09).
- Solo entran las postulaciones *En evaluación* con puntuación consolidada (RF-09).
- El ranking respeta el cupo del programa según el presupuesto disponible (RF-09; disponibilidad en HU-12.1): se señalan como "dentro del cupo" los primeros del orden mientras alcancen el cupo y el presupuesto.
- El cupo que limita el ranking de un ciclo es el **cupo de la convocatoria** (los becarios nuevos de ese ciclo), distinto del **cupo del programa** (sus becarios totales).
- **Todas las becas son un monto fijo por periodo:** no existen las coberturas total ni parcial como forma de calcular el monto. En un ciclo de becas nuevas, el monto de cada beca es el monto fijo por periodo que fija el programa (RF-11; ver HU-11.1).
- Cuántas becas caben en el presupuesto disponible se obtiene comparando ese monto fijo por periodo con el presupuesto disponible del programa; no hace falta ninguna fórmula de cobertura.
- El presupuesto disponible que se consulta a HU-12.1 descuenta las **ofertas pendientes de respuesta**: una oferta ya emitida sigue restando presupuesto hasta que el proceso se rechace definitivamente.
- Las postulaciones marcadas por una diferencia grande entre evaluadores muestran su marca en el ranking (RF-09; ver HU-09.1).
- Un empate en el límite del cupo o un presupuesto que no alcanza se resuelve en HU-10.1. Los elegibles que quedan fuera van a lista de espera (HU-10.2).
- Generar el ranking no otorga becas ni cambia el estado de ninguna postulación; el otorgamiento sigue HU-12.1 y HU-15.1, y las transiciones de estado se rigen por HU-04.1.
- El ranking es información interna de evaluación y decisión: postulantes y becarios no acceden a él; cada postulante solo ve el estado de su propia postulación (RNF de confidencialidad, RF-37; ver HU-37.1).
- **Restricciones técnicas del cliente:** 2FA de login para el coordinador. No aplica OTP por acción ni Firebase en esta HU.

#### Flujo de la HU

1. El coordinador pide generar el ranking del ciclo.
2. El sistema ordena las postulaciones con puntuación consolidada de mayor a menor.
3. El sistema obtiene el cupo de la convocatoria del ciclo y el presupuesto disponible del programa, ya descontadas las ofertas pendientes de respuesta (HU-12.1).
4. El sistema calcula cuántas becas caben en ese presupuesto a partir del monto fijo por periodo del programa.
5. El sistema señala qué postulaciones quedan dentro y fuera del cupo, y muestra la marca de diferencia grande de las que la tienen (HU-09.1).
6. El sistema señala los casos para resolver en HU-10.1.

#### Flujos alternos y de error

- FA-1 (paso 2) Postulaciones sin consolidar: quedan fuera del ranking y se muestran como pendientes.
- FA-2 (paso 2) Sin postulaciones consolidadas: el sistema muestra un mensaje de lista vacía.
- FA-3 (paso 1) Postulante o becario intenta acceder al ranking: el sistema no le permite el acceso.
- FA-4 (paso 4) El programa no tiene definido su monto fijo por periodo: el sistema no puede determinar cuántas becas caben en el presupuesto e informa que falta definirlo (HU-11.1).

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-09.2).*

#### Criterios de aceptación (11 escenarios)

##### Escenario 1: Ranking dentro del cupo

- **DADO QUE** "Becas Ingeniería 2026-2" tiene cupo de 3, presupuesto suficiente para 3 becas y 5 postulaciones consolidadas
- **CUANDO** Carlos genera el ranking
- **ENTONCES** las 5 aparecen ordenadas de mayor a menor puntuación y las 3 primeras quedan "dentro del cupo"

##### Escenario 2: El presupuesto limita el cupo

- **DADO QUE** el ciclo tiene cupo de 3 pero el presupuesto disponible alcanza para 2 becas
- **CUANDO** Carlos genera el ranking
- **ENTONCES** solo las 2 primeras quedan "dentro del cupo" y el sistema señala "Presupuesto insuficiente para el cupo: requiere resolución (HU-10.1)"

##### Escenario 3: Postulación sin consolidar

- **DADO QUE** a la postulación de Pedro le falta una evaluación
- **CUANDO** Carlos genera el ranking
- **ENTONCES** la postulación de Pedro no aparece en el ranking y se lista como "Pendiente de consolidar"

##### Escenario 4: Sin postulaciones consolidadas

- **DADO QUE** ninguna postulación del ciclo tiene puntuación consolidada
- **CUANDO** Carlos genera el ranking
- **ENTONCES** el sistema muestra "No hay postulaciones con puntuación consolidada en este ciclo."

##### Escenario 5: Marca visible en el ranking

- **DADO QUE** la postulación de María está marcada por diferencia grande (HU-09.1)
- **CUANDO** Carlos genera el ranking del ciclo
- **ENTONCES** la postulación de María aparece con la marca

##### Escenario 6: El presupuesto alcanza exactamente para el cupo

- **DADO QUE** el ciclo tiene cupo de 3 y el presupuesto disponible alcanza exactamente para 3 becas
- **CUANDO** Carlos genera el ranking
- **ENTONCES** las 3 primeras quedan "dentro del cupo" y el sistema no señala el caso como presupuesto insuficiente

##### Escenario 7: Generar el ranking no cambia estados

- **DADO QUE** las 5 postulaciones consolidadas del ciclo están *En evaluación*
- **CUANDO** Carlos genera el ranking
- **ENTONCES** las 5 siguen *En evaluación*, incluidas las que quedaron "dentro del cupo"

##### Escenario 8: Un postulante no accede al ranking

- **DADO QUE** María es postulante del ciclo 2026-2 de "Becas Ingeniería"
- **CUANDO** intenta acceder al ranking del ciclo
- **ENTONCES** el sistema no le permite el acceso y María solo ve el estado de su propia postulación

##### Escenario 9: El monto fijo del programa determina cuántas becas caben

- **DADO QUE** "Becas Ingeniería" paga un monto fijo de 2.000 $ por periodo, la convocatoria 2026-2 tiene cupo de 4, su presupuesto disponible es de 6.000 $ y hay 5 postulaciones consolidadas
- **CUANDO** Carlos genera el ranking
- **ENTONCES** las 3 primeras quedan "dentro del cupo", porque 6.000 $ alcanzan para 3 becas de 2.000 $, y el sistema señala "Presupuesto insuficiente para el cupo: requiere resolución (HU-10.1)"

##### Escenario 10: Una oferta pendiente sigue restando presupuesto

- **DADO QUE** "Becas Ingeniería" paga un monto fijo de 2.000 $ por periodo, la convocatoria 2026-2 tiene cupo de 3 y, de los 6.000 $ no desembolsados del programa, 2.000 $ corresponden a una oferta ya emitida que el postulante todavía no respondió
- **CUANDO** Carlos genera el ranking
- **ENTONCES** el presupuesto disponible considerado es de 4.000 $ y solo las 2 primeras postulaciones quedan "dentro del cupo"

##### Escenario 11: El cupo del ciclo es el de la convocatoria, no el del programa

- **DADO QUE** "Becas Ingeniería" tiene un cupo de programa de 20 becarios totales, de los que 10 están ocupados por becarios vigentes, y la convocatoria 2026-2 tiene cupo de 3 becarios nuevos, con presupuesto suficiente para esas 3 becas y 5 postulaciones consolidadas
- **CUANDO** Carlos genera el ranking
- **ENTONCES** solo las 3 primeras quedan "dentro del cupo", según el cupo de la convocatoria, aunque el cupo total del programa admita más becarios

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Ordena a los elegibles por puntuación consolidada, de mayor a menor | Regla (RF-09) | 1 | CUBIERTA |
| Solo entran postulaciones *En evaluación* con puntuación consolidada | Regla / FA-1 | 3, 7 | CUBIERTA |
| El ranking respeta el cupo según el presupuesto disponible | Regla (RF-09) | 1, 2, 6 | CUBIERTA |
| El cupo del ciclo es el de la convocatoria, no el del programa | Regla (R10) | 11 | CUBIERTA |
| Todas las becas son un monto fijo por periodo, fijado por el programa en las becas nuevas | Regla (C1) | 9 | CUBIERTA |
| Cuántas becas caben = presupuesto disponible frente al monto fijo por periodo | Regla (C1) | 9 | CUBIERTA |
| Programa sin monto fijo definido | FA-4 | — | DELEGADA (HU-11.1: la definición del monto del programa) |
| Las ofertas pendientes de respuesta siguen restando presupuesto disponible | Regla (R11) | 10 | CUBIERTA |
| Las marcadas por diferencia grande muestran su marca en el ranking | Regla (RF-09) | 5 | CUBIERTA |
| Sin postulaciones consolidadas (lista vacía) | FA-2 | 4 | CUBIERTA |
| Generar el ranking no otorga becas ni cambia estados | Regla (RF-09, HU-04.1) | 7 | CUBIERTA |
| El ranking es información interna; postulantes y becarios no acceden | Regla / FA-3 | 8 | CUBIERTA |
| Empate o presupuesto insuficiente se resuelve aparte | Regla (RF-10) | 2, 9 (señalamiento) | DELEGADA (HU-10.1) |
| Los elegibles no otorgados van a lista de espera | Regla (RF-10) | — | DELEGADA (HU-10.2) |
| Cupo y presupuesto desde una única fuente de verdad, con las ofertas pendientes descontadas | Regla técnica | 1, 2, 6, 10 (resultado) | DELEGADA (HU-12.1) |
| El ranking es una foto del momento, revalidada al confirmar | Regla técnica (RNF) | — | DELEGADA (HU-10.1, Escenarios 5 y 6) |
| Acción puntual, sin procesamiento en segundo plano | Regla técnica | — | NO VERIFICABLE EN CRITERIOS |
| 2FA en el login del coordinador | Restricción del cliente | — | DELEGADA (HU-37.2, transversal) |
| ¿Puede regenerarse el ranking y queda registro de cada generación? | — | — | PENDIENTE DE DEFINICIÓN |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-09.2).*

## RF-10 — Resolución de empates y casos límite · *Debe tener*

### HU-10.1 Resolver un empate o un caso límite con justificación

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-10 | Debe tener | HU-06.1, HU-09.2, HU-35.1 | HU-10.1 |

#### Narrativa

- Como responsable de la decisión final de la selección del ciclo (el coordinador del programa o, cuando el coordinador lo designó al conformar el comité, su jefatura)
- Quiero decidir un empate en el límite del cupo o un caso en que el presupuesto no alcanza, dejando la justificación registrada
- Para cerrar la selección del ciclo con una decisión trazable

#### Reglas de negocio

- Hay un proceso definido de resolución cuando hay un empate en el límite del cupo o el presupuesto no alcanza para otorgar lo que sugiere el ranking (RF-10).
- La decisión final la toma el coordinador o la jefatura del comité (RF-10). Ningún otro miembro del comité puede resolver el caso.
- La jefatura del comité es el miembro que el coordinador designó al conformar el comité (S12; ver HU-06.1).
- La decisión se registra con su justificación, quién la tomó y cuándo (RF-10, RF-35; ver HU-35.1). La justificación es obligatoria.
- La selección no puede superar el cupo ni el presupuesto disponible (RF-09, RF-11).
- El cupo que no puede superarse en un ciclo es el de la **convocatoria** (becarios nuevos del ciclo), no el cupo total de becarios del programa.
- El presupuesto disponible con el que se compara la selección descuenta las **ofertas pendientes de respuesta**, que siguen restando presupuesto hasta que el proceso se rechace definitivamente.
- El costo de cada beca seleccionada es el **monto fijo por periodo** que fija el programa; no hay coberturas total ni parcial que traducir (RF-11; ver HU-11.1).
- Los seleccionados siguen al otorgamiento (HU-12.1, HU-15.1). Los elegibles no seleccionados van a lista de espera (HU-10.2). Esta decisión no otorga la beca ni cambia el estado de ninguna postulación (HU-04.1).
- Toda persona con poder de decisión sobre una postulación declara su conflicto de interés antes de participar en su evaluación (regla transversal del PRD; la declaración está en HU-39.1). El bloqueo del evaluador con conflicto declarado y el paso de su lugar a otro evaluador del comité están en HU-06.2.
- **Trazabilidad estricta:** el registro de la decisión es permanente y no editable ni eliminable después de registrado (RF-35; dependencia bloqueante con HU-35.1 — sin ese módulo esta HU no puede cumplir su criterio de aceptación sin un registro ad hoc que luego habría que migrar).
- **Restricciones técnicas del cliente:** 2FA de login para coordinador y jefatura del comité. **No aplica el código de un solo uso (OTP)**: esta decisión no mueve dinero ni cambia el estado legal de una beca; el OTP se exige al desembolsar (RF-20) y al revocar (RF-22). Se deja explícito porque es el punto de esta área semánticamente más cercano a una decisión sobre dinero. No aplica Firebase en esta HU.
- **Sin definir en el PRD:** si un conflicto de interés declarado con alguno de los postulantes en disputa impide a quien decide resolver el caso. La respuesta 17 resolvió el caso del **evaluador** (no puede evaluar y su lugar pasa a otro evaluador del comité) pero no dice si ese bloqueo alcanza a quien toma esta decisión ni quién decide en su lugar, así que no se fija ninguna regla aquí ni se cubre en los escenarios. Ver Preguntas abiertas.

#### Flujo de la HU

1. El sistema muestra el ranking con el caso señalado (HU-09.2).
2. Quien decide abre el caso y ve las postulaciones empatadas o el faltante de presupuesto.
3. El sistema verifica que quien abre el caso sea el coordinador del programa o la jefatura del comité (HU-06.1).
4. Selecciona las postulaciones que quedan dentro del cupo.
5. Escribe la justificación y confirma.
6. El sistema relee el cupo de la convocatoria y el presupuesto disponible, con las ofertas pendientes descontadas (HU-12.1), y verifica que la selección —valorada al monto fijo por periodo del programa— no los supere.
7. El sistema registra la decisión con quién la tomó, cuándo y su justificación (HU-35.1) y actualiza el ranking.

#### Flujos alternos y de error

- FA-1 (paso 5) Sin justificación: el sistema no registra la decisión.
- FA-2 (paso 6) La selección supera el cupo disponible: el sistema no la acepta, aunque la justificación esté completa.
- FA-3 (paso 6) La selección supera el presupuesto disponible: el sistema no la acepta, aunque la justificación esté completa.
- FA-4 (paso 3) Quien intenta resolver no es el coordinador ni la jefatura del comité: el sistema no lo permite.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-10.1).*

#### Criterios de aceptación (8 escenarios)

##### Escenario 1: Empate resuelto

- **DADO QUE** queda un lugar en el cupo y María y Pedro tienen 85 de puntuación consolidada
- **CUANDO** Carlos selecciona a María con la justificación "Mayor puntuación en necesidad socioeconómica" y confirma
- **ENTONCES** María queda "dentro del cupo", Pedro queda fuera y la decisión queda registrada con Carlos, la fecha y la justificación

##### Escenario 2: Sin justificación

- **DADO QUE** Carlos seleccionó a María en un empate
- **CUANDO** confirma sin escribir la justificación
- **ENTONCES** el sistema no registra la decisión y muestra "Escribe la justificación de esta decisión."

##### Escenario 3: Decide la jefatura del comité

- **DADO QUE** Carlos designó a Luis como jefatura del comité de "Becas Ingeniería" al conformarlo (HU-06.1)
- **CUANDO** resuelve el empate con una justificación
- **ENTONCES** el sistema registra la decisión a nombre de Luis

##### Escenario 4: Miembro sin jefatura

- **DADO QUE** Ana es miembro del comité sin la jefatura
- **CUANDO** intenta resolver el empate
- **ENTONCES** el sistema no lo permite y muestra "Solo el coordinador o la jefatura del comité pueden resolver este caso."

##### Escenario 5: Selección que excede el cupo

- **DADO QUE** el cupo disponible es 1 y Carlos selecciona a María y a Pedro para ese único lugar
- **CUANDO** escribe la justificación y confirma
- **ENTONCES** el sistema no acepta la selección y muestra "La selección supera el cupo disponible del programa."

##### Escenario 6: Selección que excede el presupuesto disponible

- **DADO QUE** el presupuesto disponible alcanza para una beca más y Carlos selecciona a dos postulantes
- **CUANDO** escribe la justificación y confirma
- **ENTONCES** el sistema no acepta la selección y muestra "La selección supera el presupuesto disponible del programa."

##### Escenario 7: Resolver el caso no cambia el estado de la postulación

- **DADO QUE** la postulación de María está *En evaluación* y Carlos la selecciona para resolver un empate con la justificación "Mayor puntuación en necesidad socioeconómica"
- **CUANDO** confirma la decisión
- **ENTONCES** la postulación de María sigue *En evaluación* hasta que se registre su otorgamiento (HU-12.1, HU-15.1)

##### Escenario 8: Una oferta pendiente mantiene reservado el presupuesto

- **DADO QUE** "Becas Ingeniería" paga un monto fijo de 2.000 $ por periodo, el programa tiene 4.000 $ no desembolsados y 2.000 $ de ellos corresponden a una oferta ya emitida que el postulante todavía no respondió, y Carlos selecciona a María y a Pedro
- **CUANDO** escribe la justificación y confirma
- **ENTONCES** el sistema no acepta la selección y muestra "La selección supera el presupuesto disponible del programa.", porque la oferta pendiente sigue restando presupuesto

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Existe un proceso definido de resolución del empate o del faltante de presupuesto | Regla (RF-10) | 1, 5, 6 | CUBIERTA |
| La decisión final la toma el coordinador o la jefatura del comité, nadie más | Regla / FA-4 | 3, 4 | CUBIERTA |
| La jefatura es el miembro designado al conformar el comité | Regla (S12) | 3 | CUBIERTA (la designación se verifica en HU-06.1) |
| La justificación es obligatoria | Regla / FA-1 | 2 | CUBIERTA |
| La decisión se registra con quién la tomó y cuándo | Regla (RF-10, RF-35) | 1, 3 | CUBIERTA |
| La selección no puede superar el cupo disponible | Regla / FA-2 | 5 | CUBIERTA |
| El cupo aplicable es el de la convocatoria, no el del programa | Regla (R10) | 5 | CUBIERTA PARCIAL — la distinción se verifica en HU-09.2, Escenario 11 |
| La selección no puede superar el presupuesto disponible | Regla / FA-3 | 6 | CUBIERTA |
| Las ofertas pendientes de respuesta siguen restando presupuesto disponible | Regla (R11) | 8 | CUBIERTA |
| El costo de cada beca es el monto fijo por periodo del programa | Regla (C1) | 8 | CUBIERTA |
| Resolver el caso no otorga la beca ni cambia el estado de la postulación | Regla (HU-04.1) | 7 | CUBIERTA |
| Autorización combinada (coordinador del programa o jefatura del comité) | Regla técnica | 3, 4 | CUBIERTA |
| Cupo y presupuesto releídos en la misma transacción que confirma | Regla técnica (RNF) | 5, 6, 8 (resultado) | CUBIERTA PARCIAL — la relectura concurrente va a pruebas de integración |
| Registro permanente y no editable | Regla (RF-35) | — | DELEGADA (HU-35.1, Escenarios 4 y siguientes) |
| No aplica OTP por acción (no mueve dinero ni cambia el estado legal de una beca) | Restricción del cliente | — | DELEGADA (el OTP se verifica en RF-20 y RF-22) |
| 2FA en el login de coordinador y jefatura | Restricción del cliente | — | DELEGADA (HU-37.2, transversal) |
| ¿Un conflicto de interés declarado bloquea a quien decide? | — | — | PENDIENTE DE DEFINICIÓN (prioridad alta; la respuesta 17 solo resuelve el caso del evaluador) |
| ¿Qué prelación hay entre coordinador y jefatura si ambos deciden distinto? | — | — | PENDIENTE DE DEFINICIÓN |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-10.1).*

### HU-10.2 Dejar en lista de espera a los elegibles no otorgados

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-10 | Debe tener | HU-10.1, HU-14.1, HU-38.1 | HU-10.2 |

#### Narrativa

- Como coordinador de programa
- Quiero dejar en lista de espera a los postulantes elegibles que no obtuvieron beca, hasta que inicien las inscripciones de su universidad
- Para considerarlos si se libera un cupo o presupuesto mientras su universidad todavía no inicia su periodo

#### Reglas de negocio

- Los postulantes elegibles no otorgados por falta de cupo quedan en lista de espera por si se libera disponibilidad (RF-10). El glosario incluye también la falta de presupuesto.
- La lista de espera es una marca sobre la postulación elegible no otorgada; no es un estado nuevo y la postulación no cambia de estado (S2).
- La lista sigue el orden del ranking (HU-09.2), porque al liberarse un cupo se considera "al siguiente" (RF-15; ver HU-15.2 y HU-16.2).
- Solo entran postulaciones elegibles: no las *No elegible* ni las *Retirada* (RF-10).
- **La lista de espera termina, para cada universidad, cuando inicia el periodo de inscripciones de esa universidad:** desde ese momento la postulación de un estudiante de esa universidad deja de estar en lista de espera y ya no puede recibir disponibilidad liberada de ese periodo.
- Cada universidad tiene sus propias fechas: la fecha de inicio de inscripciones se registra por universidad y periodo, y es el dato que cierra la espera. Ese registro vive fuera de esta área (RF-14, RF-38; ver HU-14.1 y HU-38.1).
- En un mismo ciclo, la espera puede terminar en fechas distintas para postulantes de universidades distintas: la marca se evalúa postulación por postulación, según la universidad del postulante.
- Las fechas de esta regla se interpretan en la zona horaria **UTC-4**, igual que todas las fechas límite de la plataforma.
- El orden y la identidad de los demás postulantes de la lista de espera no son visibles para postulantes ni becarios: es información interna de evaluación y decisión (RNF de confidencialidad, RF-37; ver HU-37.1).
- **Trazabilidad:** quién confirmó la lista de espera y cuándo se registra igual que las demás decisiones que afectan a un postulante, aunque RF-35 no la enumere explícitamente (RNF de trazabilidad; ver HU-35.1).
- **Restricciones técnicas del cliente:** 2FA de login para el coordinador. No aplica OTP por acción ni Firebase en esta HU.
- **Sin definir en el PRD:** qué hecho hace pasar una postulación al estado *Rechazada*; ninguna HU usa hoy ese estado, y la respuesta que cierra la espera (C4) no dice si el fin de la espera cambia el estado de la postulación. Ver Preguntas abiertas.

#### Flujo de la HU

1. El ranking del ciclo está resuelto (HU-09.2, HU-10.1).
2. El coordinador confirma la lista de espera.
3. El sistema marca en lista de espera a los elegibles que quedaron fuera del cupo, en el orden del ranking, fijando el puesto de cada uno, y excluye las postulaciones *No elegible* y *Retirada*.
4. El sistema registra quién confirmó la lista y cuándo (HU-35.1).
5. El coordinador consulta la lista de espera del ciclo.
6. Al llegar la fecha de inicio de inscripciones de una universidad (HU-14.1, HU-38.1), el sistema da por terminada la espera de las postulaciones de los estudiantes de esa universidad.

#### Flujos alternos y de error

- FA-1 (paso 3) Todos los elegibles caben en el cupo: la lista de espera queda vacía.
- FA-2 (paso 3) Postulaciones *No elegible* o *Retirada* en el ciclo: el sistema las excluye del marcado.
- FA-3 (paso 5) Un postulante o becario consulta la lista de espera del ciclo: el sistema no se la muestra.
- FA-4 (paso 6) Universidades con fechas distintas: la espera termina primero para las postulaciones de la universidad que inicia antes y sigue vigente para las de las demás.
- FA-5 (paso 6) Se libera disponibilidad después de que inició el periodo de inscripciones de una universidad: las postulaciones de esa universidad ya no están en lista de espera y no se consideran para ese periodo.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-10.2).*

#### Criterios de aceptación (9 escenarios)

##### Escenario 1: Lista de espera creada

- **DADO QUE** el ciclo tiene cupo de 3 y hay 5 postulaciones elegibles en el ranking
- **CUANDO** Carlos confirma la lista de espera
- **ENTONCES** las postulaciones en los puestos 4 y 5 quedan marcadas en lista de espera, en ese orden

##### Escenario 2: La postulación no cambia de estado

- **DADO QUE** la postulación de Pedro quedó en lista de espera
- **CUANDO** Carlos la consulta
- **ENTONCES** la postulación sigue *En evaluación* y muestra la marca "Lista de espera: puesto 1"

##### Escenario 3: Lista de espera vacía

- **DADO QUE** el ciclo tiene cupo de 5 y solo hay 4 postulaciones elegibles
- **CUANDO** Carlos consulta la lista de espera
- **ENTONCES** el sistema muestra "No hay postulantes en lista de espera en este ciclo."

##### Escenario 4: Postulaciones no elegibles y retiradas excluidas

- **DADO QUE** la postulación de Luis está *No elegible* y la de Pedro está *Retirada*, ambas del mismo ciclo
- **CUANDO** Carlos confirma la lista de espera
- **ENTONCES** ninguna de las dos queda marcada en lista de espera

##### Escenario 5: Un postulante no ve la lista de espera del ciclo

- **DADO QUE** María quedó en el puesto 1 y Pedro en el puesto 2 de la lista de espera del ciclo
- **CUANDO** Pedro intenta acceder a la lista de espera del ciclo
- **ENTONCES** el sistema no le permite el acceso y no le muestra la posición ni la identidad de María

##### Escenario 6: El puesto fijado no se recalcula

- **DADO QUE** Pedro quedó confirmado en el puesto 1 y María en el puesto 2 de la lista de espera del ciclo 2026-2 de "Becas Ingeniería"
- **CUANDO** la postulación de Luis, que estaba pendiente de consolidar, obtiene después una puntuación consolidada mayor que la de Pedro
- **ENTONCES** Pedro sigue en el puesto 1 y María en el puesto 2 de la lista de espera

##### Escenario 7: Autor y fecha de la confirmación de la lista de espera

- **DADO QUE** Carlos confirmó la lista de espera del ciclo 2026-2 de "Becas Ingeniería" el 10-oct-2026
- **CUANDO** consulta el registro de esa confirmación
- **ENTONCES** el registro muestra a Carlos como quien la confirmó y el 10-oct-2026 como fecha

##### Escenario 8: La espera termina al iniciar las inscripciones de la universidad

- **DADO QUE** Pedro estudia en la Universidad Central, cuyo periodo de inscripciones 2026-2 inicia el 20-oct-2026, y su postulación está en lista de espera del ciclo
- **CUANDO** llega el 20-oct-2026 (UTC-4)
- **ENTONCES** la postulación de Pedro deja de estar en lista de espera del ciclo y no se considera para la disponibilidad que se libere en ese periodo

##### Escenario 9: La espera sigue vigente para otra universidad

- **DADO QUE** Pedro estudia en la Universidad Central, que inicia sus inscripciones 2026-2 el 20-oct-2026, y María en la Universidad del Litoral, que las inicia el 05-nov-2026, y ambas postulaciones están en lista de espera del mismo ciclo
- **CUANDO** llega el 20-oct-2026 (UTC-4)
- **ENTONCES** la postulación de Pedro deja de estar en lista de espera y la de María sigue en lista de espera

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Los elegibles no otorgados por falta de cupo o presupuesto quedan en lista de espera | Regla (RF-10) | 1 | CUBIERTA |
| La lista de espera es una marca; la postulación no cambia de estado | Regla (S2) | 2 | CUBIERTA |
| La lista sigue el orden del ranking | Regla (RF-10, RF-15) | 1 | CUBIERTA |
| Solo entran elegibles: no *No elegible* ni *Retirada* | Regla / FA-2 | 4 | CUBIERTA |
| Todos los elegibles caben en el cupo: lista vacía | FA-1 | 3 | CUBIERTA |
| El orden y la identidad no son visibles para postulantes ni becarios | Regla / FA-3 | 5 | CUBIERTA |
| La marca no es un estado nuevo de la postulación | Regla técnica (S2) | 2 | CUBIERTA |
| El puesto se fija al confirmar y no se recalcula en cada consulta | Regla técnica (RNF) | 6 | CUBIERTA |
| Trazabilidad: quién confirmó la lista de espera y cuándo | Regla (RNF) | 7 | CUBIERTA |
| La espera termina, por universidad, al iniciar su periodo de inscripciones | Regla (C4, R18d) / FA-5 | 8 | CUBIERTA |
| Universidades con fechas distintas: la espera termina en momentos distintos | Regla (C4) / FA-4 | 9 | CUBIERTA |
| Cierre por fecha, sin acción manual, usando la fecha de inicio de inscripciones por universidad y periodo | Regla técnica | 8, 9 (resultado) | DELEGADA (el registro de la fecha es de HU-14.1 / HU-38.1) |
| Fechas interpretadas en UTC-4 | Regla (R20) | 8, 9 | CUBIERTA PARCIAL — la hora exacta del corte del día sigue por definir |
| 2FA en el login del coordinador | Restricción del cliente | — | DELEGADA (HU-37.2, transversal) |
| ¿Qué hace pasar una postulación a *Rechazada*? | — | — | PENDIENTE DE DEFINICIÓN |
| ¿La espera termina al comenzar el día de inicio de inscripciones o al final de ese día? | — | — | PENDIENTE DE DEFINICIÓN |
| ¿Ve el postulante su propia posición? | — | — | PENDIENTE DE DEFINICIÓN |
| ¿Cómo se numera el puesto: desde 1 en la lista o heredando la posición del ranking? | — | — | PENDIENTE DE DEFINICIÓN |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-10.2).*
