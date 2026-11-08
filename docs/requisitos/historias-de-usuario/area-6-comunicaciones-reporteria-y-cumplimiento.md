# Área 6 — Comunicaciones, Reportería y Cumplimiento

> Parte de las [Historias de Usuario · Plan B · versión final](README.md). Reglas técnicas en el [Anexo A](anexo-a-reglas-tecnicas.md); preguntas abiertas en el [Anexo B](anexo-b-preguntas-abiertas.md).

## RF-29 — Notificaciones automáticas de hitos clave · *Debería tener*

### HU-29.1 Recibir los hitos de mi postulación o beca por correo y por los canales del programa

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-29 | Debería tener | HU-01.1 | HU-29.1, HU-29.3 |

#### Narrativa

- Como postulante o becario
- Quiero recibir un correo automático en los momentos clave de mi postulación o beca, y también por los canales adicionales que el coordinador active en mi programa
- Para saber qué ocurrió y qué debo hacer sin tener que entrar a revisar, por los medios que el programa necesite

> La configuración de canales (pasos 1–3) la ejecuta el **coordinador de programa**; la recepción (pasos 4–7) la vive el **postulante o becario**. Son dos actores beneficiarios de la misma historia; ver propuesta de split en las notas de consolidación.
> El módulo que define esta historia transporta además avisos automáticos cuyo destinatario es un usuario interno (HU-21.3, HU-27.2): esos avisos reutilizan su mecanismo, pero su narrativa y sus criterios pertenecen a la historia que los origina.

#### Reglas de negocio

- Son hitos clave, entre otros: postulación recibida, documentos faltantes, resultado de evaluación, oferta para aceptar, desembolso realizado y suspensión (RF-29).
- Cada notificación indica qué ocurrió y qué acción, si alguna, se espera de la persona (RF-29).
- La notificación es automática (RF-29).
- El canal mínimo es el correo electrónico; se pueden agregar otros canales según el programa (RF-29).
- El correo no se puede desactivar porque es el canal mínimo (RF-29).
- Los canales activos son los del programa de la postulación o beca a la que corresponde el hito; la configuración de un programa no afecta a los demás (RF-29).
- Solo el coordinador del programa configura los canales de notificación de ese programa; cualquier otro rol tiene el acceso limitado a lo que corresponde a su función (RF-37).
- La notificación de una suspensión **incluye el motivo** de la suspensión y lo que el becario debe resolver para reanudar (RF-21; ver HU-21.1).
- Cuando la notificación comunica una fecha límite, la expresa en **UTC-4** e incluye el día de cierre **hasta las 23:59**, que es el instante en que vence el plazo (ver HU-15.2, HU-30.1).
- Una persona puede tener más de una beca a la vez, dentro del límite de la regla de no doble beneficio (ver HU-01.2, HU-14.2); cada hito se notifica por los canales del programa al que pertenece esa beca o postulación.
- El módulo entrega también avisos automáticos dirigidos a un usuario interno cuando otra historia lo exige: el aviso al coordinador al llegar la fecha de revisión de una suspensión (HU-21.3) y el del vencimiento de la duración aprobada de una pausa (HU-27.2). Esos avisos reutilizan la abstracción de canal, el registro de estado de envío y la idempotencia de esta HU; su regla de negocio y sus criterios viven en la historia que los origina.
- **Esta área no requiere OTP ni almacenamiento Firebase de documentos:** ninguna HU de RF-29 a RF-32 mueve dinero, cambia el estado legal de una beca ni gestiona documentos de postulantes. Las dos restricciones técnicas del cliente que aplican al Área 4 no se extienden aquí. Lo que sí toca a esta HU es el **envío** del código: viaja por correo o por SMS, a elección del usuario, por el canal de salida que define esta historia, aunque el flujo de verificación lo gobiernen las Áreas 4 y 7.
- **Sin definir en el PRD:** qué canales adicionales existen. RF-29 solo dice que pueden agregarse según el programa; el cliente confirma que el SMS existe en la plataforma para los códigos, pero no dice que se ofrezca como canal adicional de notificación. Los escenarios usan un canal adicional cualquiera.
- **Sin definir en el PRD:** por qué canal llega un aviso cuyo destinatario es un usuario interno. El canal mínimo de RF-29 está definido para postulantes y becarios, y el cliente no fijó el canal del coordinador.

#### Flujo de la HU

1. El coordinador abre la configuración de notificaciones de su programa.
2. El sistema muestra el correo como canal siempre activo y los canales adicionales disponibles.
3. El coordinador activa uno o más canales adicionales y guarda.
4. Ocurre un hito de la postulación o beca, por ejemplo la postulación queda *Recibida* (HU-01.1).
5. El sistema genera la notificación con lo que ocurrió y la acción esperada, si la hay; cuando el hito es una suspensión, incluye el motivo y lo que el becario debe resolver (HU-21.1).
6. El sistema la envía al correo de la persona y por los canales adicionales activados en su programa, si los hay, y registra el resultado de cada envío.
7. La persona lee la notificación y, si corresponde, entra a la plataforma a realizar la acción.

#### Flujos alternos y de error

- FA-1 (paso 3) Intento de desactivar el correo: el sistema no lo permite y muestra "El correo electrónico es el canal mínimo y no se puede desactivar."
- FA-2 (paso 3) Desactivar un canal adicional: las notificaciones siguientes salen solo por los canales que siguen activos.
- FA-3 (paso 5) Hito sin acción esperada: la notificación indica que no se requiere ninguna acción.
- FA-4 (paso 1) Rol sin permiso sobre el programa: el sistema bloquea el acceso y responde "No tienes permiso para gestionar este programa."
- FA-5 (paso 6) Falla el envío por un canal: el envío queda registrado como fallido y disponible para seguimiento; la notificación no se reporta como entregada y el hito sigue visible en la plataforma.
- FA-6 (paso 5) Hito ya notificado: si el mismo hito se reprocesa, el sistema no genera una segunda notificación para esa persona.
- FA-7 (paso 4) Hito cuyo destinatario es un usuario interno: la notificación se genera y se envía por este mismo mecanismo, con su propio registro de envío, y no se envía al postulante o becario salvo que la HU que la origina lo indique (HU-21.3, HU-27.2).

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-29.1).*

#### Criterios de aceptación (13 escenarios)

##### Escenario 1: Hito con acción esperada

- **DADO QUE** María recibió una oferta de beca con fecha límite 10/01/2027 (HU-15.1)
- **CUANDO** se emite la oferta
- **ENTONCES** María recibe un correo que dice que tiene una oferta de beca y que debe aceptarla o rechazarla hasta las 23:59 (UTC-4) del 10/01/2027

##### Escenario 2: Hito sin acción esperada

- **DADO QUE** se confirmó un desembolso de 500 a la beca de Pedro (HU-20.1)
- **CUANDO** se registra el desembolso
- **ENTONCES** Pedro recibe un correo con el desembolso realizado y la indicación de que no se requiere ninguna acción

##### Escenario 3: Postulación recibida

- **DADO QUE** María envió su postulación a "Becas Ingeniería 2026-2"
- **CUANDO** la postulación queda *Recibida*
- **ENTONCES** María recibe un correo que confirma la recepción

##### Escenario 4: Canal adicional activado

- **DADO QUE** "Becas Ingeniería" solo notifica por correo
- **CUANDO** el coordinador activa un canal adicional y guarda
- **ENTONCES** las siguientes notificaciones del programa salen por correo y por ese canal

##### Escenario 5: El correo no se desactiva

- **DADO QUE** el coordinador está en la configuración de notificaciones de "Becas Ingeniería"
- **CUANDO** intenta desactivar el correo
- **ENTONCES** el sistema no lo permite y muestra "El correo electrónico es el canal mínimo y no se puede desactivar."

##### Escenario 6: Otro programa no cambia

- **DADO QUE** el coordinador activó un canal adicional en "Becas Ingeniería" y "Becas Salud" solo tiene el correo activo
- **CUANDO** se notifica un hito de "Becas Salud"
- **ENTONCES** la notificación sale solo por correo y no por el canal adicional de "Becas Ingeniería"

##### Escenario 7: Coordinador de otro programa no configura canales

- **DADO QUE** Carla coordina "Becas Salud" y no coordina "Becas Ingeniería"
- **CUANDO** intenta abrir la configuración de notificaciones de "Becas Ingeniería"
- **ENTONCES** el sistema bloquea el acceso y responde "No tienes permiso para gestionar este programa."

##### Escenario 8: Falla el envío del correo

- **DADO QUE** la postulación de María queda *Recibida*
- **CUANDO** el servidor de correo rechaza el mensaje
- **ENTONCES** el envío queda registrado como fallido y disponible para seguimiento, y la notificación no se reporta como entregada

##### Escenario 9: Un hito no se notifica dos veces

- **DADO QUE** la postulación de María ya generó su notificación de *Recibida*
- **CUANDO** el mismo hito se reprocesa
- **ENTONCES** María no recibe una segunda notificación de ese hito

##### Escenario 10: Desactivar un canal adicional

- **DADO QUE** "Becas Ingeniería" tiene activado el correo y un canal adicional
- **CUANDO** el coordinador desactiva ese canal adicional y guarda
- **ENTONCES** las siguientes notificaciones del programa salen solo por correo

##### Escenario 11: Notificación de suspensión con el motivo

- **DADO QUE** la beca de Pedro queda *Suspendida* con el motivo "Constancia de matrícula pendiente" y lo que debe resolver "Entregar la constancia de matrícula" (HU-21.1)
- **CUANDO** se registra la suspensión
- **ENTONCES** Pedro recibe un correo que dice que su beca está *Suspendida*, con el motivo "Constancia de matrícula pendiente" y "Entregar la constancia de matrícula"

##### Escenario 12: Una persona con dos becas recibe cada hito por los canales de su programa

- **DADO QUE** Lucía tiene una beca en "Becas Ingeniería", que notifica por correo y por un canal adicional, y otra beca en "Becas Salud", que solo notifica por correo
- **CUANDO** se confirma un desembolso de su beca de "Becas Salud"
- **ENTONCES** Lucía recibe esa notificación solo por correo y no por el canal adicional de "Becas Ingeniería"

##### Escenario 13: Aviso dirigido a un usuario interno

- **DADO QUE** llega la fecha de revisión de la suspensión de la beca de Pedro, que pertenece a "Becas Ingeniería" (HU-21.3)
- **CUANDO** el sistema genera ese aviso
- **ENTONCES** el aviso se envía al coordinador de "Becas Ingeniería" por este mismo mecanismo, su resultado de envío queda registrado y el proceso no lo envía una segunda vez si se repite

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Hitos clave: postulación recibida, documentos faltantes, resultado de evaluación, oferta, desembolso, suspensión | Regla | 1, 2, 3, 11 | CUBIERTA (muestra representativa: oferta, desembolso, postulación recibida y suspensión; el mecanismo es el mismo para los demás hitos, que no tienen escenario propio) |
| Cada notificación indica qué ocurrió y la acción esperada, si la hay | Regla | 1, 2 | CUBIERTA |
| La notificación es automática | Regla | 1, 2, 3 | CUBIERTA |
| Canal mínimo correo; canales adicionales según programa | Regla | 1, 2, 3, 4, 10 | CUBIERTA |
| El correo no se puede desactivar | Regla / FA-1 | 5 | CUBIERTA |
| Canales activos son los del programa del hito; la config. de un programa no afecta a otros | Regla | 6, 12 | CUBIERTA |
| Solo el coordinador del programa configura los canales (RF-37) | Regla / FA-4 | 7 | CUBIERTA |
| La notificación de suspensión incluye el motivo y lo que hay que resolver | Regla | 11 | CUBIERTA |
| Las fechas límite se comunican en UTC-4 con el día de cierre hasta las 23:59 | Regla | 1 | CUBIERTA |
| Varias becas de la misma persona: cada hito usa los canales de su programa | Regla | 12 | CUBIERTA |
| Avisos cuyo destinatario es un usuario interno reutilizan canal, registro de envío e idempotencia | Regla / FA-7 | 13 | CUBIERTA; la regla de negocio del aviso, DELEGADA (HU-21.3, HU-27.2) |
| FA-2 Desactivar un canal adicional | FA | 10 | CUBIERTA |
| FA-3 Hito sin acción esperada | FA | 2 | CUBIERTA |
| FA-5 Falla el envío por un canal | FA | 8 | CUBIERTA |
| FA-6 Hito ya notificado (idempotencia) | FA | 9 | CUBIERTA |
| Técnica — registro de estado de envío por destinatario (entregado o fallido) | Técnica | 8, 13 | CUBIERTA |
| Técnica — idempotencia por (persona, hito) | Técnica | 9, 13 | CUBIERTA |
| Técnica — autorización en los datos, no solo en la interfaz | Técnica | 7 | CUBIERTA (parcial: el escenario verifica el bloqueo, no la capa donde se aplica) |
| Técnica — contenido mínimo necesario: el mensaje no lleva datos sensibles, con el motivo de la suspensión como excepción confirmada | Técnica / RNF | 11 (lo que el mensaje sí lleva) | CUBIERTA PARCIAL: ningún escenario comprueba la ausencia de ingreso declarado ni de composición familiar |
| Técnica — disparo por evento de dominio, no por llamada directa; los avisos por fecha entran por el proceso programado | Técnica | — | SIN ESCENARIO (técnica) |
| Técnica — el SMS debe caber en la abstracción de canal, porque el código viaja por correo o SMS a elección del usuario | Técnica | — | SIN ESCENARIO (técnica); el flujo del código, DELEGADO (Áreas 4 y 7) |
| Qué canales adicionales existen | Sin definir en el PRD | — | NO PROBABLE |
| Por qué canal llega un aviso a un usuario interno | Sin definir en el PRD | — | NO PROBABLE |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-29.1).*

### HU-29.2 Enviar notificaciones a todo un ciclo sin ralentizar la plataforma

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-29 | Debería tener | HU-29.1 | HU-29.2 |

#### Narrativa

- Como coordinador de programa
- Quiero que las notificaciones de un hito que afecta a todo un ciclo se envíen en segundo plano
- Para que la plataforma siga funcionando con normalidad para todos los usuarios mientras se envían

#### Reglas de negocio

- Enviar notificaciones a todo un ciclo, potencialmente miles de personas a la vez, no debe ralentizar la plataforma para el resto de los usuarios (RF-29).
- Las operaciones pesadas, como las notificaciones masivas, no bloquean ni ralentizan el uso normal de la plataforma mientras se ejecutan (RNF de procesamiento en segundo plano).
- Cada persona del ciclo recibe su notificación con lo que ocurrió y la acción esperada (RF-29; ver HU-29.1).
- Un reproceso del mismo hito no vuelve a notificar a quien ya recibió su notificación (RNF de consistencia bajo concurrencia).
- Si una parte del área falla, el hito no queda comunicado a esas personas: los envíos fallidos quedan identificados y separados de los entregados (RNF de trazabilidad).
- **Sin definir en el PRD:** un umbral medible de "no ralentizar" (tiempo de respuesta aceptable). Los escenarios verifican solo que ninguna operación espera a que termine el envío.

#### Flujo de la HU

1. Ocurre un hito que afecta a todas las personas de un ciclo, por ejemplo el resultado de evaluación del ciclo.
2. El sistema genera una notificación para cada persona del ciclo.
3. El sistema encola los envíos y los procesa en segundo plano, registrando el resultado por destinatario.
4. El coordinador y el resto de los usuarios siguen trabajando sin esperar a que termine el envío.
5. Cada persona recibe su notificación (HU-29.1).

#### Flujos alternos y de error

- FA-1 (paso 3) Ciclo sin destinatarios: el sistema no genera envíos.
- FA-2 (paso 3) Falla parcial del área: el sistema identifica los envíos fallidos y los reintenta sin reenviar a los ya entregados.
- FA-3 (paso 1) Hito disparado de nuevo: el sistema no vuelve a notificar a quien ya tiene su notificación registrada como entregada para ese hito.
- FA-4 (paso 3) Interrupción del proceso de envío: al reanudarse continúa por los destinatarios pendientes, no desde el inicio del área.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-29.2).*

#### Criterios de aceptación (7 escenarios)

##### Escenario 1: Envío a todo el ciclo

- **DADO QUE** el ciclo "Becas Ingeniería 2026-2" tiene 3.000 postulantes
- **CUANDO** ocurre el hito de resultado de evaluación del ciclo
- **ENTONCES** cada uno de los 3.000 postulantes recibe su notificación con su resultado

##### Escenario 2: El coordinador no espera

- **DADO QUE** se disparó el envío masivo del ciclo "Becas Ingeniería 2026-2"
- **CUANDO** el coordinador continúa con otra tarea en la plataforma
- **ENTONCES** puede hacerlo sin esperar a que termine el envío

##### Escenario 3: Otros usuarios no se ven afectados

- **DADO QUE** el envío masivo del ciclo "Becas Ingeniería 2026-2" está en proceso
- **CUANDO** un postulante de otro programa envía su postulación y un evaluador envía su evaluación
- **ENTONCES** ambas operaciones se completan sin esperar a que termine el envío

##### Escenario 4: Falla parcial del área

- **DADO QUE** el envío masivo a los 3.000 postulantes de "Becas Ingeniería 2026-2" está en curso y 50 envíos fallan
- **CUANDO** el sistema reintenta el envío
- **ENTONCES** reintenta solo esos 50 y no reenvía a los 2.950 que ya recibieron su notificación

##### Escenario 5: Disparo duplicado del mismo hito

- **DADO QUE** el resultado de evaluación de "Becas Ingeniería 2026-2" ya disparó el envío masivo
- **CUANDO** el mismo hito se dispara otra vez por un reproceso
- **ENTONCES** cada postulante recibe su notificación una sola vez

##### Escenario 6: Ciclo sin destinatarios

- **DADO QUE** el ciclo "Becas Salud 2026-2" no tiene postulantes registrados
- **CUANDO** ocurre el hito de resultado de evaluación del ciclo
- **ENTONCES** el sistema no genera ningún envío

##### Escenario 7: Interrupción y reanudación del envío masivo

- **DADO QUE** el envío masivo a los 3.000 postulantes de "Becas Ingeniería 2026-2" se interrumpe después de notificar a 1.000 de ellos
- **CUANDO** el proceso se reanuda
- **ENTONCES** continúa notificando a los 2.000 postulantes pendientes, sin reenviar a los 1.000 que ya recibieron su notificación

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Envío masivo (miles de personas) no ralentiza la plataforma | Regla | 2, 3 | CUBIERTA |
| Operaciones pesadas no bloquean el uso normal del resto | Regla | 2, 3 | CUBIERTA |
| Cada persona del ciclo recibe su notificación | Regla | 1 | CUBIERTA (el contenido del mensaje lo verifica HU-29.1) |
| Reproceso del hito no vuelve a notificar a quien ya recibió | Regla | 5, 7 | CUBIERTA |
| Fallo parcial: los envíos fallidos quedan identificados y separados de los entregados | Regla | 4 | CUBIERTA |
| FA-1 Ciclo sin destinatarios | FA | 6 | CUBIERTA |
| FA-2 Falla parcial del área: reintenta sin reenviar a los entregados | FA | 4 | CUBIERTA |
| FA-3 Hito disparado de nuevo | FA | 5 | CUBIERTA |
| FA-4 Interrupción del proceso: al reanudarse continúa por los pendientes | FA | 7 | CUBIERTA |
| Técnica — idempotencia por (persona, notificación), no por lote | Técnica | 4, 7 | CUBIERTA |
| Técnica — cola con control de tasa y reintentos con espera creciente | Técnica | — | SIN ESCENARIO (técnica; el número y espaciado de reintentos no está definido en el PRD) |
| Técnica — aislamiento de recursos y escalabilidad de la selección de destinatarios | Técnica | 3 | CUBIERTA (parcial: se verifica el efecto, no el mecanismo) |
| Umbral medible de "no ralentizar" | Sin definir en el PRD | — | NO PROBABLE |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-29.2).*

## RF-30 — Recordatorios antes de un vencimiento · *Debería tener*

### HU-30.1 Configurar y recibir recordatorios antes de un vencimiento

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-30 | Debería tener | HU-29.1 | HU-30.1, HU-30.2 |

#### Narrativa

- Como postulante o becario
- Quiero recibir un recordatorio, en los momentos de anticipación que configura el coordinador de mi programa, antes de que venza un plazo que me toca cumplir
- Para no perder una fecha límite por olvido y tener tiempo suficiente para cumplir

> Igual que en HU-29.1, la configuración (pasos 1–3) es del **coordinador de programa** y la recepción (pasos 4–7) del **postulante o becario**; ver propuesta de split en las notas de consolidación.

#### Reglas de negocio

- El sistema recuerda proactivamente los plazos próximos de documentos, solicitud de renovación y reporte académico, antes de que venzan y no solo después (RF-30).
- El momento del recordatorio es configurable por el programa y los recordatorios salen en los momentos configurados (RF-30); el PRD da como ejemplo 7 y 2 días antes.
- Los plazos con fecha límite que el PRD define y que esta HU recuerda son: la fecha límite para volver a subir un documento rechazado (RF-03; ver HU-03.2), la ventana de solicitud de renovación (RF-26; ver HU-26.1) y la fecha límite del reporte académico del periodo (RF-24; ver HU-24.1).
- Las fechas límite se interpretan en **UTC-4** e incluyen el día de cierre **hasta las 23:59**: el plazo sigue pendiente durante todo ese día y el recordatorio comunica la fecha con ese instante de vencimiento. La anticipación en días se cuenta desde ese día de vencimiento.
- Los recordatorios usan los canales de notificación del programa, con el correo como mínimo (RF-29; ver HU-29.1).
- El recordatorio se dirige a quien tiene pendiente el plazo (RF-30); quien ya lo cumplió no lo recibe.
- El reporte académico del periodo puede llegar por tres vías: la entrega del propio becario (HU-24.1), el registro manual del enlace de la universidad (HU-24.2) y el envío por API de la universidad (HU-24.3). Como el recordatorio se dirige a quien tiene el plazo pendiente, si el reporte del periodo ya llegó por cualquiera de las tres vías ese plazo está cumplido y el becario no recibe el recordatorio.
- Un recordatorio es válido solo mientras la postulación o la beca sigue en el proceso: una postulación *Retirada*, *No elegible* o *Rechazada* y una beca *Concluida* o *Revocada* ya no tienen ese plazo pendiente, porque esos estados son finales para el caso (Reglas de Negocio Transversales; ver HU-05.1, HU-22.1).
- El cliente añade el estado *Inactiva* para la beca no renovada y para la pausa vencida sin reactivar (ver HU-26.2, HU-27.2). **Decisión del cliente:** *Inactiva* no es un estado final: desde *Inactiva* la beca puede volver a *Activa* o pasar a *Concluida*, que sigue siendo un estado final. Por eso esta HU no la añade a la lista de estados finales que cancelan un plazo pendiente. **Decisión del cliente:** mientras dura ese estado, la beca conserva sus plazos pendientes y el becario sigue recibiendo los recordatorios correspondientes, incluido el de la ventana de renovación del periodo siguiente. El paso de *Inactiva* a *Activa* o a *Concluida* lo decide el coordinador de programa, a partir de que el becario sube los datos que le faltaban; esta HU no modela esa decisión, solo que no cancela el plazo mientras tanto.
- Los momentos de anticipación se expresan en días enteros mayores que cero antes del vencimiento; cualquier otro valor no es un momento de anticipación válido (RF-30).
- Solo el coordinador del programa configura los recordatorios de ese programa (RF-37).
- **Sin definir en el PRD:** si la anticipación configurada (por ejemplo, 7 y 2 días) es un valor único por programa que aplica igual a los tres tipos de plazo, o si cada tipo puede tener la suya. RF-30 solo dice que "el momento del recordatorio debe ser configurable". Los escenarios usan un único par de valores por programa sin afirmar que sea la única opción posible.
- **Sin definir en el PRD:** si la carga original de los documentos obligatorios tiene una fecha límite propia, distinta del cierre de la convocatoria (RF-02; ver HU-02.2), que ahora ocurre por universidad al iniciar sus inscripciones (ver HU-02.2, HU-14.3). RF-03 solo fija fecha límite para volver a subir un documento rechazado, así que los escenarios de documentos usan esa fecha y no inventan un plazo de carga independiente.

#### Flujo de la HU

1. El coordinador abre la configuración de recordatorios de su programa.
2. Indica uno o más momentos de anticipación, en días antes del vencimiento, y guarda.
3. El sistema registra la configuración y la usa para los recordatorios del programa.
4. El sistema identifica los plazos próximos a partir de sus fechas límite registradas: documento rechazado por volver a subir (HU-03.2), ventana de renovación (HU-26.1) y reporte académico (HU-24.1).
5. En cada momento configurado, identifica a las personas que aún no cumplen ese plazo —el del reporte académico cuenta como cumplido por cualquiera de sus tres vías (HU-24.1, HU-24.2, HU-24.3)— y cuyo caso sigue vigente.
6. El sistema les envía un recordatorio con el plazo, la fecha de vencimiento —con su límite a las 23:59 (UTC-4) de ese día— y la acción esperada, y registra el envío para no repetirlo.
7. La persona entra a la plataforma y cumple el plazo.

#### Flujos alternos y de error

- FA-1 (paso 2) Anticipación no válida: si un valor no es un número entero de días mayor que cero, el sistema no guarda y muestra "Indica cuántos días antes del vencimiento se envía el recordatorio."
- FA-2 (paso 5) Plazo ya cumplido: la persona no recibe el recordatorio. En el reporte académico, el plazo está cumplido también cuando lo registró el enlace de la universidad (HU-24.2) o cuando llegó por API (HU-24.3).
- FA-3 (paso 1) Rol sin permiso sobre el programa: el sistema bloquea el acceso y responde "No tienes permiso para gestionar este programa."
- FA-4 (paso 5) Caso que ya no está vigente: si la postulación quedó *Retirada*, *No elegible* o *Rechazada*, o la beca quedó *Concluida* o *Revocada* antes del momento configurado, el recordatorio de ese plazo no se envía.
- FA-5 (paso 6) Recordatorio ya enviado: si el proceso corre de nuevo para el mismo momento de anticipación, la persona no recibe un segundo recordatorio del mismo plazo.
- FA-6 (paso 6) Falla el envío: el envío queda registrado como fallido y la fecha de vencimiento sigue visible para la persona en la plataforma (ver HU-29.1).
- FA-7 (paso 5) Plazo ya vencido: si el proceso se ejecuta después de las 23:59 (UTC-4) del día de vencimiento, ese plazo ya no está pendiente y no se envía ningún recordatorio por él.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-30.1).*

#### Criterios de aceptación (16 escenarios)

##### Escenario 1: Dos recordatorios

- **DADO QUE** "Becas Ingeniería" no tiene recordatorios configurados
- **CUANDO** el coordinador configura 7 y 2 días antes y guarda
- **ENTONCES** la configuración queda registrada con los dos momentos

##### Escenario 2: Valor no válido

- **DADO QUE** el coordinador está configurando los recordatorios
- **CUANDO** indica 0 días
- **ENTONCES** el sistema no guarda y muestra "Indica cuántos días antes del vencimiento se envía el recordatorio."

##### Escenario 3: Cambio de configuración

- **DADO QUE** "Becas Ingeniería" tiene recordatorios a 7 y 2 días
- **CUANDO** el coordinador los cambia a 10 días y guarda
- **ENTONCES** los siguientes recordatorios del programa se envían 10 días antes del vencimiento

##### Escenario 4: Recordatorio de reporte académico solo a quien no lo entregó

- **DADO QUE** el programa tiene recordatorios a 7 y 2 días, la fecha límite de reportes de 2026-2 es el 20/02/2027, Pedro no ha entregado su reporte y Lucía entregó el suyo el 10/02/2027
- **CUANDO** llega el 13/02/2027
- **ENTONCES** Pedro recibe un recordatorio con el plazo "Reporte académico 2026-2", la fecha 20/02/2027 hasta las 23:59 (UTC-4) y la acción "Entregar tu reporte académico", y Lucía no recibe el recordatorio del reporte académico

##### Escenario 5: Recordatorio de renovación

- **DADO QUE** la ventana de renovación cierra el 15/12/2026, el programa tiene recordatorios a 2 días y la beca de Lucía está *Activa* sin solicitud
- **CUANDO** llega el 13/12/2026
- **ENTONCES** Lucía recibe un recordatorio para solicitar la renovación hasta las 23:59 (UTC-4) del 15/12/2026

##### Escenario 6: Documento rechazado pendiente de volver a subir

- **DADO QUE** a María le rechazaron la constancia de ingresos con fecha límite 15/11/2026 para volver a subirla (HU-03.2) y el programa tiene recordatorios a 7 y 2 días
- **CUANDO** llega el 08/11/2026
- **ENTONCES** María recibe un recordatorio para volver a subir la constancia de ingresos hasta las 23:59 (UTC-4) del 15/11/2026

##### Escenario 7: Valor negativo

- **DADO QUE** el coordinador está configurando los recordatorios
- **CUANDO** indica -3 días
- **ENTONCES** el sistema no guarda y muestra "Indica cuántos días antes del vencimiento se envía el recordatorio."

##### Escenario 8: Postulación retirada no recibe recordatorio

- **DADO QUE** María tenía pendiente volver a subir la constancia de ingresos pero retiró su postulación (HU-05.1) antes del momento configurado
- **CUANDO** llega el momento del recordatorio
- **ENTONCES** María no recibe el recordatorio de ese documento

##### Escenario 9: Beca revocada no recibe recordatorio de renovación

- **DADO QUE** la beca de Pedro quedó *Revocada* (HU-22.1) antes del momento configurado del recordatorio de renovación
- **CUANDO** llega ese momento
- **ENTONCES** Pedro no recibe el recordatorio de renovación de esa beca

##### Escenario 10: Coordinador de otro programa no configura recordatorios

- **DADO QUE** Carla coordina "Becas Salud" y no coordina "Becas Ingeniería"
- **CUANDO** intenta abrir la configuración de recordatorios de "Becas Ingeniería"
- **ENTONCES** el sistema bloquea el acceso y responde "No tienes permiso para gestionar este programa."

##### Escenario 11: Recordatorio no se envía dos veces

- **DADO QUE** Pedro ya recibió el recordatorio de "Reporte académico 2026-2" en el momento de 7 días antes del vencimiento
- **CUANDO** el proceso de recordatorios corre de nuevo para ese mismo momento de anticipación
- **ENTONCES** Pedro no recibe un segundo recordatorio de ese plazo

##### Escenario 12: Falla el envío del recordatorio

- **DADO QUE** llega el momento del recordatorio de María para volver a subir la constancia de ingresos (fecha límite 15/11/2026)
- **CUANDO** el servidor de correo rechaza el mensaje
- **ENTONCES** el envío queda registrado como fallido y la fecha límite 15/11/2026 sigue visible para María en la plataforma

##### Escenario 13: El reporte que llegó por API no genera recordatorio al becario

- **DADO QUE** la fecha límite de reportes de 2026-2 es el 20/02/2027, el programa tiene recordatorios a 7 y 2 días y el reporte de Lucía llegó por API desde su universidad el 05/02/2027 (HU-24.3)
- **CUANDO** llega el 13/02/2027
- **ENTONCES** Lucía no recibe el recordatorio del reporte académico, porque ese plazo ya está cumplido

##### Escenario 14: El plazo sigue pendiente durante todo el día de vencimiento

- **DADO QUE** la fecha límite del reporte académico 2026-2 es el 20/02/2027, el programa tiene recordatorios a 7 y 2 días y Pedro, que ya recibió los dos, aún no ha entregado
- **CUANDO** llega el 20/02/2027 a las 20:00 (UTC-4)
- **ENTONCES** el plazo de Pedro sigue pendiente hasta las 23:59 (UTC-4) de ese mismo día y no se le envía ningún recordatorio adicional de ese plazo

##### Escenario 15: Beca Inactiva sigue recibiendo el recordatorio de reporte académico

- **DADO QUE** la beca de Pedro quedó *Inactiva* por no renovar a tiempo (HU-26.2), la fecha límite del reporte académico 2026-2 es el 20/02/2027, el programa tiene recordatorios a 7 y 2 días y Pedro no ha entregado su reporte
- **CUANDO** llega el 13/02/2027
- **ENTONCES** Pedro recibe el recordatorio, porque *Inactiva* no es un estado final y no cancela ese plazo pendiente

##### Escenario 16: Beca Inactiva recibe el recordatorio de la ventana de renovación del periodo siguiente

- **DADO QUE** la beca de Lucía quedó *Inactiva* por vencer su pausa sin reactivarse (HU-27.2), la ventana de renovación del periodo 2027-1 cierra el 15/06/2027 y el programa tiene recordatorios a 7 y 2 días
- **CUANDO** llega el 13/06/2027
- **ENTONCES** Lucía recibe el recordatorio para solicitar la renovación hasta las 23:59 (UTC-4) del 15/06/2027, porque *Inactiva* no es un estado final

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| El sistema recuerda proactivamente los plazos antes del vencimiento | Regla | 4, 5, 6 | CUBIERTA |
| Momento del recordatorio configurable por programa | Regla | 1, 3 | CUBIERTA |
| Plazos recordados: documento rechazado (HU-03.2), ventana de renovación (HU-26.1), reporte académico (HU-24.1) | Regla | 6, 5, 4 | CUBIERTA (uno por tipo de plazo) |
| Fechas límite en UTC-4, con el día de vencimiento incluido hasta las 23:59 | Regla | 4, 5, 6, 14 | CUBIERTA |
| El recordatorio indica el plazo, la fecha de vencimiento y la acción esperada | Regla / Técnica | 4, 5, 6 | CUBIERTA |
| Los recordatorios usan los canales del programa, correo como mínimo | Regla | — | DELEGADA (HU-29.1) |
| El recordatorio se dirige a quien tiene pendiente el plazo; quien cumplió no lo recibe | Regla / FA-2 | 4, 13 | CUBIERTA |
| El reporte académico cuenta como cumplido por cualquiera de sus tres vías | Regla / FA-2 | 13 | CUBIERTA |
| Recordatorio válido solo mientras el caso sigue en proceso (estados finales excluidos) | Regla / FA-4 | 8, 9 | CUBIERTA |
| Una beca *Inactiva* (no final; puede volver a *Activa* o pasar a *Concluida*) conserva plazos pendientes y recibe recordatorios mientras dura ese estado, incluido el de la ventana de renovación siguiente | Regla | 15, 16 | CUBIERTA |
| Anticipación en días enteros > 0; otro valor no es válido | Regla / FA-1 | 2, 7 | CUBIERTA |
| Solo el coordinador del programa configura los recordatorios (RF-37) | Regla / FA-3 | 10 | CUBIERTA |
| FA-5 Recordatorio ya enviado (deduplicación) | FA | 11 | CUBIERTA |
| FA-6 Falla el envío del recordatorio | FA | 12 | CUBIERTA |
| FA-7 Plazo ya vencido: no se envía recordatorio | FA | 14 (resultado complementario: el plazo sigue vigente hasta las 23:59) | CUBIERTA PARCIAL |
| Técnica — fecha límite consultable de forma independiente | Técnica | 4, 5, 6 | CUBIERTA (parcial: los escenarios parten de fechas límite registradas; el modelo de datos se verifica en diseño) |
| Técnica — estado de cumplimiento con varias fuentes, no "el becario entregó" | Técnica | 13 | CUBIERTA |
| Técnica — reutilización de la cola y del registro de envío de HU-29.1/HU-29.2; es el mismo proceso programado de los avisos por fecha | Técnica | — | SIN ESCENARIO (técnica) |
| Técnica — escalabilidad del recorrido con índices | Técnica | — | SIN ESCENARIO (técnica) |
| Técnica — contenido mínimo necesario del recordatorio | Técnica / RNF | 4 | CUBIERTA (parcial: el escenario fija el contenido esperado, no la ausencia de datos sensibles) |
| Anticipación única por programa o una por tipo de plazo | Sin definir en el PRD | — | NO PROBABLE |
| Fecha límite propia de la carga original de documentos, con el cierre de convocatoria ahora por universidad | Sin definir en el PRD | — | NO PROBABLE |
| Hora del día en que sale el recordatorio en su momento de anticipación | Sin definir en el PRD | — | NO PROBABLE |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-30.1).*

## RF-31 — Comunicación al comité evaluador · *Podría tener*

### HU-31.1 Avisar al comité de nuevas asignaciones y del cierre de la evaluación

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-31 | Podría tener | HU-06.2 | HU-31.1, HU-31.2 |

#### Narrativa

- Como miembro del comité evaluador
- Quiero recibir un aviso cuando se me asigna una nueva postulación y un recordatorio antes de que cierre el periodo de evaluación
- Para empezar a evaluarla sin tener que revisar la plataforma a cada rato y enviar mis evaluaciones a tiempo

#### Reglas de negocio

- Los evaluadores son notificados cuando se les asignan nuevas postulaciones y recordados antes de que cierre el periodo de evaluación (RF-31).
- Solo recibe el aviso el evaluador asignado a esa postulación (RF-31).
- Un evaluador con conflicto de interés declarado no puede revisar esa postulación; por eso no se le asigna ni se le avisa (RF-06, RF-39; ver HU-06.2). Cuando el conflicto se declara después de la asignación, pierde el acceso a esa postulación y su lugar pasa a otro evaluador del comité: el aviso de asignación lo recibe ese evaluador sustituto, cuando el coordinador lo asigna (HU-06.2).
- Tras un reemplazo por conflicto de interés, los destinatarios del recordatorio de cierre son los evaluadores con asignaciones vigentes en el ciclo, no los originales (HU-06.2).
- El recordatorio corresponde al ciclo en el que el evaluador tiene postulaciones asignadas (RF-31); quien no tiene asignaciones en ese ciclo no lo recibe.
- Quien ya no forma parte del comité del programa no recibe avisos ni recordatorios de ese ciclo, porque ya no puede evaluar en él (RF-06; ver HU-06.1).
- El inicio y el cierre de los periodos los ejecuta **Dirección**; no los ejecuta el comité ni el evaluador. La fecha de cierre del periodo de evaluación que comunica el recordatorio es la de ese periodo (ver HU-08.2).
- La fecha de cierre se interpreta en **UTC-4** e incluye el día de cierre **hasta las 23:59**; el recordatorio la comunica con ese instante de vencimiento.
- Si falla el envío, la plataforma es la red de seguridad: la asignación se mantiene y la fecha de cierre sigue consultable por el evaluador (RF-31).
- **Sin definir en el PRD:** por qué canal se avisa al evaluador. RF-31 no lo indica y el correo como canal mínimo de RF-29 está dirigido a postulantes y becarios. La plataforma ya alcanza por correo al evaluador externo, porque ese es el canal de su segundo factor (ver HU-06.1, HU-06.2), pero esa respuesta se refiere al código de verificación y no fija el canal del aviso ni del recordatorio.
- **Sin definir en el PRD:** quién configura la anticipación del recordatorio de cierre del periodo de evaluación y qué valor tiene, y si el evaluador que ya envió todas sus evaluaciones también recibe el recordatorio. El cliente confirmó que es Dirección quien fija y cierra la fecha del periodo (ver regla de negocio anterior), pero no dijo si también configura esta anticipación. Los escenarios parten de una fecha de cierre y una anticipación ya configuradas, y usan evaluadores con evaluaciones sin enviar.

#### Flujo de la HU

1. El coordinador asigna una postulación a uno o más evaluadores (HU-06.2).
2. El sistema envía un aviso a cada evaluador asignado y registra el resultado del envío.
3. El aviso indica el programa y la postulación asignada.
4. El evaluador abre la postulación desde su lista de asignaciones (HU-08.1).
5. El ciclo del programa tiene una fecha de cierre del periodo de evaluación, que inicia y cierra **Dirección**.
6. Al llegar la anticipación configurada, el sistema identifica a los evaluadores del comité con postulaciones asignadas vigentes en ese ciclo.
7. El sistema envía un recordatorio a cada uno y registra el envío para no repetirlo.
8. El recordatorio indica el programa y la fecha de cierre de la evaluación, hasta las 23:59 (UTC-4) de ese día.

#### Flujos alternos y de error

- FA-1 (paso 1) Evaluador con conflicto: la asignación se bloquea (HU-06.2) y no se envía aviso. Si el conflicto se declara después de la asignación, el evaluador deja de tener esa postulación y el aviso se envía al evaluador del comité que toma su lugar, cuando el coordinador lo asigna.
- FA-2 (paso 2) Falla el envío del aviso: el envío queda registrado como fallido, la asignación se mantiene y la postulación aparece en la lista de asignaciones del evaluador.
- FA-3 (paso 6) Evaluador en varios programas: recibe un recordatorio por cada ciclo, cada uno con su fecha de cierre.
- FA-4 (paso 7) Falla el envío del recordatorio: el envío queda registrado como fallido, la fecha de cierre no cambia y sigue visible para el evaluador en la plataforma.
- FA-5 (paso 6) Evaluador retirado del comité: si dejó de ser miembro del comité del programa después de la asignación, no entra en la lista de destinatarios del recordatorio de ese ciclo.
- FA-6 (paso 7) Recordatorio ya enviado: si el proceso corre de nuevo para el mismo momento de anticipación, el evaluador no recibe un segundo recordatorio de ese ciclo.
- FA-7 (paso 6) Evaluador reemplazado por conflicto de interés: no entra en los destinatarios del recordatorio por las asignaciones que perdió; sí por las que conserve en ese ciclo.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-31.1).*

#### Criterios de aceptación (13 escenarios)

##### Escenario 1: Aviso al asignar

- **DADO QUE** Luis es miembro del comité de "Becas Ingeniería 2026-2"
- **CUANDO** el coordinador le asigna la postulación de María Pérez
- **ENTONCES** Luis recibe un aviso que identifica el programa "Becas Ingeniería 2026-2" y la postulación de María Pérez

##### Escenario 2: Cada evaluador asignado recibe su aviso

- **DADO QUE** Luis y Ana son miembros del comité de "Becas Ingeniería 2026-2"
- **CUANDO** el coordinador asigna la postulación de María Pérez a ambos
- **ENTONCES** Luis y Ana reciben cada uno su aviso

##### Escenario 3: Sin aviso para quien no fue asignado

- **DADO QUE** Pedro es miembro del mismo comité pero no fue asignado a la postulación de María Pérez
- **CUANDO** el coordinador asigna esa postulación a Luis y Ana
- **ENTONCES** Pedro no recibe ningún aviso sobre esa postulación

##### Escenario 4: Recordatorio antes del cierre

- **DADO QUE** la evaluación de "Becas Ingeniería 2026-2" cierra el 20-oct-2026, la anticipación configurada es 2 días y Luis tiene postulaciones asignadas sin evaluar en ese ciclo
- **CUANDO** llega el 18-oct-2026
- **ENTONCES** Luis recibe un recordatorio que indica el programa "Becas Ingeniería 2026-2" y la fecha de cierre 20-oct-2026 hasta las 23:59 (UTC-4)

##### Escenario 5: Sin asignaciones, sin recordatorio

- **DADO QUE** Pedro es miembro del comité de "Becas Ingeniería 2026-2" pero no tiene postulaciones asignadas en ese ciclo
- **CUANDO** llega el momento del recordatorio
- **ENTONCES** Pedro no recibe recordatorio de ese ciclo

##### Escenario 6: Un recordatorio por ciclo

- **DADO QUE** Luis tiene asignaciones sin evaluar en "Becas Ingeniería 2026-2" (cierre 20-oct-2026) y en "Becas Salud 2026-2" (cierre 27-oct-2026)
- **CUANDO** llega el momento de cada recordatorio
- **ENTONCES** Luis recibe dos recordatorios, cada uno con su programa y su fecha de cierre

##### Escenario 7: Evaluador retirado del comité

- **DADO QUE** Luis tenía asignada la postulación de María Pérez en "Becas Ingeniería 2026-2" y el coordinador lo retiró del comité antes del momento del recordatorio
- **CUANDO** llega el momento del recordatorio de cierre
- **ENTONCES** Luis no recibe el recordatorio de ese ciclo

##### Escenario 8: Aislamiento entre ciclos del mismo programa

- **DADO QUE** Luis tuvo asignaciones en el ciclo 2026-1 de "Becas Ingeniería" y no tiene ninguna en el ciclo 2026-2
- **CUANDO** se envían los recordatorios del ciclo 2026-2
- **ENTONCES** Luis no recibe recordatorio de ese ciclo

##### Escenario 9: Falla el envío del aviso de asignación

- **DADO QUE** el coordinador asigna la postulación de María Pérez a Luis en "Becas Ingeniería 2026-2"
- **CUANDO** el envío del aviso a Luis falla
- **ENTONCES** el envío queda registrado como fallido, la asignación se mantiene y la postulación aparece en la lista de postulaciones asignadas de Luis (HU-08.1)

##### Escenario 10: Falla el envío del recordatorio de cierre

- **DADO QUE** llega el momento del recordatorio de cierre para Luis en "Becas Ingeniería 2026-2" (cierre 20-oct-2026)
- **CUANDO** el envío del recordatorio falla
- **ENTONCES** el envío queda registrado como fallido, la fecha de cierre sigue siendo 20-oct-2026 y sigue visible para Luis en la plataforma

##### Escenario 11: El recordatorio de cierre no se envía dos veces

- **DADO QUE** Luis ya recibió el recordatorio de cierre de "Becas Ingeniería 2026-2" para el momento de anticipación configurado
- **CUANDO** el proceso de recordatorios corre de nuevo para ese mismo momento
- **ENTONCES** Luis no recibe un segundo recordatorio de ese ciclo

##### Escenario 12: El aviso va al evaluador que toma el lugar del que declaró conflicto

- **DADO QUE** Luis tenía asignada la postulación de María Pérez en "Becas Ingeniería 2026-2" y declaró un conflicto de interés con ella, por lo que la postulación quedó pendiente de reasignación (HU-06.2)
- **CUANDO** el coordinador asigna a Elena, miembro del mismo comité sin conflicto declarado, en lugar de Luis
- **ENTONCES** Elena recibe el aviso con el programa "Becas Ingeniería 2026-2" y la postulación de María Pérez, y Luis no recibe ningún aviso sobre esa postulación

##### Escenario 13: El evaluador reemplazado no recibe el recordatorio del ciclo

- **DADO QUE** la única postulación asignada a Luis en "Becas Ingeniería 2026-2" pasó a Elena tras el conflicto de interés de Luis, y Elena no ha enviado su evaluación
- **CUANDO** llega el momento del recordatorio de cierre del ciclo
- **ENTONCES** Elena recibe el recordatorio y Luis no, porque ya no tiene asignaciones vigentes en ese ciclo

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Evaluadores notificados al asignarse postulaciones y recordados antes del cierre | Regla | 1, 4 | CUBIERTA |
| Solo recibe el aviso el evaluador asignado a esa postulación | Regla | 2, 3 | CUBIERTA |
| Evaluador con conflicto de interés: no se le asigna ni se le avisa | Regla / FA-1 | — | DELEGADA (HU-06.2) |
| El aviso de la reasignación va al evaluador del comité que toma su lugar | Regla / FA-1 | 12 | CUBIERTA |
| Tras un reemplazo, el recordatorio va a quien tiene asignaciones vigentes | Regla / FA-7 | 13 | CUBIERTA |
| El recordatorio corresponde al ciclo con asignaciones; quien no tiene asignaciones no lo recibe | Regla | 5, 8, 13 | CUBIERTA |
| Quien ya no es miembro del comité no recibe avisos ni recordatorios de ese ciclo | Regla / FA-5 | 7 | CUBIERTA |
| El inicio y el cierre de los periodos los ejecuta Dirección | Regla | 4 (parte de: la fecha de cierre existe y se comunica) | CUBIERTA PARCIAL — el escenario no verifica que sea Dirección quien fija la fecha |
| La fecha de cierre se comunica en UTC-4 con el día incluido hasta las 23:59 | Regla | 4 | CUBIERTA |
| Si falla el envío, la asignación y la fecha de cierre siguen consultables en la plataforma | Regla | 9, 10 | CUBIERTA |
| FA-2 Falla el envío del aviso de asignación | FA | 9 | CUBIERTA |
| FA-3 Evaluador en varios programas: un recordatorio por ciclo | FA | 6 | CUBIERTA |
| FA-4 Falla el envío del recordatorio de cierre | FA | 10 | CUBIERTA |
| FA-6 Recordatorio ya enviado (deduplicación) | FA | 11 | CUBIERTA |
| Técnica — reutilización de la abstracción de canal y del registro de envío de HU-29.1 | Técnica | 9, 10 | CUBIERTA (parcial: se verifica el registro del resultado, no la reutilización del componente) |
| Técnica — contenido del aviso: programa y postulación, sin información socioeconómica | Técnica / RNF | 1, 4, 12 | CUBIERTA (parcial: se verifica lo que el aviso contiene, no la ausencia del dato sensible) |
| Técnica — autorización en los datos (filtro por evaluador y ciclo) | Técnica | 3, 5, 8 | CUBIERTA |
| Técnica — no reimplementar el conflicto de interés ni la elección del sustituto | Técnica | — | DELEGADA (HU-06.2) |
| Canal de aviso y recordatorio al evaluador; el correo confirmado para el evaluador externo es el del segundo factor | Sin definir en el PRD | — | NO PROBABLE |
| Quién configura la anticipación del recordatorio de cierre y qué valor tiene; si el evaluador que ya envió todo recibe el recordatorio | Sin definir en el PRD | — | NO PROBABLE |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-31.1).*

## RF-32 — Mensajería de postulantes y becarios hacia la Fundación · *Debería tener*

### HU-32.1 Enviar una consulta sobre mi postulación o beca

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-32 | Debería tener | HU-01.1 | HU-32.1 |

#### Narrativa

- Como postulante o becario
- Quiero enviar una consulta a la Fundación sobre mi postulación o mi beca
- Para resolver mis dudas sin depender del correo electrónico

#### Reglas de negocio

- Postulantes y becarios pueden hacer una consulta relacionada con su postulación o beca (RF-32).
- Cada consulta queda asociada a la postulación o beca específica (RF-32).
- La consulta tiene un estado para el seguimiento del equipo: abierta, respondida o cerrada (RF-32). Al enviarse queda abierta.
- Una persona solo puede asociar su consulta a sus propias postulaciones y becas (RNF de protección de datos personales; RF-37).
- Una misma persona puede tener varias postulaciones a la vez, volver a postular después de un rechazo y tener dos becas simultáneas dentro de la regla de no doble beneficio; el sistema le ofrece todas las suyas para asociar la consulta y no limita la elección a una sola.
- Una beca *Inactiva* no es un estado final —puede volver a *Activa* o pasar a *Concluida*— y el becario sigue pudiendo enviar consultas asociadas a ella mientras dura ese estado.
- La respuesta y el seguimiento se cubren en HU-32.2.

#### Flujo de la HU

1. El postulante o becario abre la opción "Nueva consulta".
2. Elige, entre sus propias postulaciones y becas, aquella a la que se refiere.
3. Escribe su mensaje y lo envía.
4. El sistema registra la consulta asociada a esa postulación o beca, con estado abierta, su autor y su fecha.
5. La consulta aparece en su lista de consultas.

#### Flujos alternos y de error

- FA-1 (paso 2) Sin postulación ni beca: el sistema no permite crear la consulta y muestra "Para enviar una consulta necesitas tener una postulación o una beca."
- FA-2 (paso 3) Mensaje vacío: el sistema no envía la consulta y pide escribir el mensaje.
- FA-3 (paso 2) Sin elegir a qué asociarla: el sistema no la envía y muestra "Elige la postulación o beca a la que se refiere tu consulta."
- FA-4 (paso 2) Postulación o beca de otra persona: no aparece entre las opciones que el sistema ofrece.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-32.1).*

#### Criterios de aceptación (8 escenarios)

##### Escenario 1: Consulta asociada a una postulación

- **DADO QUE** María tiene una postulación *En evaluación* en "Becas Ingeniería 2026-2"
- **CUANDO** envía la consulta "¿Cuándo sabré el resultado?" asociada a esa postulación
- **ENTONCES** la consulta queda abierta, asociada a su postulación, y aparece en su lista de consultas

##### Escenario 2: Consulta sin asociar

- **DADO QUE** María escribe una consulta
- **CUANDO** intenta enviarla sin elegir una postulación o beca
- **ENTONCES** el sistema no la envía y muestra "Elige la postulación o beca a la que se refiere tu consulta."

##### Escenario 3: Sin postulación ni beca

- **DADO QUE** Jorge tiene cuenta pero ninguna postulación ni beca
- **CUANDO** abre "Nueva consulta"
- **ENTONCES** el sistema muestra "Para enviar una consulta necesitas tener una postulación o una beca."

##### Escenario 4: Solo mis postulaciones y becas

- **DADO QUE** María tiene una postulación y Jorge tiene otra
- **CUANDO** María elige a qué asociar su consulta
- **ENTONCES** solo ve sus propias postulaciones y becas

##### Escenario 5: El mensaje se guarda y se muestra como texto

- **DADO QUE** María escribe una consulta cuyo texto incluye una etiqueta `<script>`
- **CUANDO** la envía
- **ENTONCES** el sistema guarda y muestra el contenido como texto, sin interpretar ni ejecutar esa etiqueta, tanto para María como para el coordinador

##### Escenario 6: Mensaje vacío

- **DADO QUE** María eligió su postulación en "Becas Ingeniería 2026-2" para su consulta
- **CUANDO** intenta enviarla sin escribir ningún mensaje
- **ENTONCES** el sistema no envía la consulta, no queda ninguna consulta nueva en su lista y se le pide escribir el mensaje

##### Escenario 7: Varias postulaciones y becas para elegir

- **DADO QUE** Lucía tiene dos becas activas en programas distintos y una postulación nueva en un tercer programa
- **CUANDO** abre "Nueva consulta" y elige a qué asociarla
- **ENTONCES** el sistema le ofrece las tres y la consulta queda asociada solo a la que eligió

##### Escenario 8: Consulta sobre una beca Inactiva

- **DADO QUE** la beca de Pedro está *Inactiva* porque no renovó a tiempo (HU-26.2)
- **CUANDO** Pedro envía la consulta "¿Qué datos me faltan para reactivar mi beca?" asociada a esa beca
- **ENTONCES** la consulta queda abierta, asociada a su beca *Inactiva*, y aparece en su lista de consultas

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Postulantes y becarios pueden enviar una consulta sobre su postulación o beca | Regla | 1 | CUBIERTA |
| Cada consulta queda asociada a la postulación o beca específica | Regla | 1, 2, 7 | CUBIERTA |
| La consulta queda abierta al enviarse | Regla | 1 | CUBIERTA |
| Varias postulaciones y hasta dos becas de la misma persona entre las opciones | Regla | 7 | CUBIERTA |
| Respuesta, cambio de estado y seguimiento | Regla | — | DELEGADA (HU-32.2) |
| Una persona solo asocia su consulta a sus propias postulaciones y becas | Regla / FA-4 | 4 | CUBIERTA |
| Técnica — el mensaje se guarda y se muestra como texto, sin interpretar marcado ni guiones | Técnica | 5 | CUBIERTA |
| Técnica — autorización en los datos (filtro por persona en la consulta de datos) | Técnica | 4 | CUBIERTA (parcial: se verifica lo que la persona ve, no la capa donde se filtra) |
| Técnica — cada mensaje conserva autor y fecha | Técnica / RNF | — | SIN ESCENARIO (técnica) |
| Técnica — los mensajes no van al almacenamiento Firebase de documentos | Técnica | — | SIN ESCENARIO (técnica) |
| FA-1 Sin postulación ni beca | FA | 3 | CUBIERTA |
| FA-2 Mensaje vacío | FA | 6 | CUBIERTA |
| FA-3 Sin elegir a qué asociarla | FA | 2 | CUBIERTA |
| El becario de una beca *Inactiva* (no final) puede enviar una consulta asociada a ella | Regla | 8 | CUBIERTA |
| Consulta sobre postulación o beca en estado final (*Concluida*, *Revocada*) | Sin definir en el PRD | — | NO PROBABLE |
| Límite de longitud o formato del mensaje, adjuntos | Sin definir en el PRD | — | NO PROBABLE |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-32.1).*

### HU-32.2 Responder y dar seguimiento a una consulta

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-32 | Debería tener | HU-32.1 | HU-32.2 |

#### Narrativa

- Como coordinador de programa
- Quiero responder las consultas de mis programas y cambiar su estado
- Para dar seguimiento a lo que preguntan postulantes y becarios

#### Reglas de negocio

- El equipo de la Fundación puede responder cada consulta y darle seguimiento (RF-32).
- Cada consulta tiene un estado: abierta, respondida o cerrada (RF-32).
- Los mensajes se ven casi al instante por ambas partes, sin recargar la página (RF-32; RNF de actualizaciones en tiempo real).
- El coordinador solo ve y atiende las consultas asociadas a postulaciones y becas de los programas que coordina (RF-37).
- Dos coordinadores actuando al mismo tiempo sobre la misma consulta no pueden dejarla con datos contradictorios ni perder una respuesta ya enviada (RNF de consistencia bajo concurrencia).
- **Sin definir en el PRD:** si el postulante o becario también puede cerrar una consulta, y si una consulta respondida o cerrada vuelve a abierta cuando la persona escribe de nuevo. Los escenarios usan solo al coordinador.

#### Flujo de la HU

1. El coordinador ve la lista de consultas de sus programas, con su estado y la postulación o beca asociada.
2. Abre una consulta abierta y lee el mensaje.
3. Escribe y envía la respuesta.
4. La consulta pasa a respondida y la respuesta queda en la conversación con su autor y su fecha.
5. El postulante o becario ve la respuesta sin recargar la página.
6. Cuando el caso está resuelto, el coordinador marca la consulta como cerrada.

#### Flujos alternos y de error

- FA-1 (paso 1) Sin consultas: el sistema muestra que no hay consultas pendientes.
- FA-2 (paso 3) Respuesta vacía: el sistema no la envía y pide escribir la respuesta.
- FA-3 (paso 1) Consulta de un programa que no coordina: no aparece en su lista y, si intenta abrirla directamente, el sistema bloquea el acceso y responde "No tienes permiso para gestionar este programa."
- FA-4 (paso 3) Respuestas simultáneas de dos coordinadores del mismo programa: ambas respuestas quedan registradas en la conversación y la consulta queda en un único estado, sin perder ninguna de las dos.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-32.2).*

#### Criterios de aceptación (8 escenarios)

##### Escenario 1: Responder una consulta

- **DADO QUE** la consulta de María sobre su postulación a "Becas Ingeniería 2026-2" está abierta
- **CUANDO** el coordinador envía una respuesta
- **ENTONCES** la consulta pasa a respondida y la respuesta queda en la conversación

##### Escenario 2: Ver la respuesta sin recargar

- **DADO QUE** María tiene abierta la pantalla de su consulta
- **CUANDO** el coordinador envía la respuesta
- **ENTONCES** María ve la respuesta sin recargar la página

##### Escenario 3: Cerrar una consulta

- **DADO QUE** la consulta de María está respondida
- **CUANDO** el coordinador la marca como cerrada
- **ENTONCES** la consulta aparece como cerrada para el coordinador y para María

##### Escenario 4: Respuesta vacía

- **DADO QUE** el coordinador abre una consulta abierta
- **CUANDO** pulsa "Enviar" sin escribir nada
- **ENTONCES** el sistema no envía la respuesta, la consulta sigue abierta y se muestra "Escribe una respuesta antes de enviarla."

##### Escenario 5: Coordinador de otro programa no responde

- **DADO QUE** la consulta de María está asociada a su postulación en "Becas Ingeniería" y Carla coordina "Becas Salud"
- **CUANDO** Carla intenta abrir y responder esa consulta
- **ENTONCES** el sistema bloquea el acceso y responde "No tienes permiso para gestionar este programa."

##### Escenario 6: Coordinador de otro programa no la ve en su lista

- **DADO QUE** la consulta de María está asociada a "Becas Ingeniería" y Carla coordina "Becas Salud"
- **CUANDO** Carla abre su lista de consultas
- **ENTONCES** no ve la consulta de María

##### Escenario 7: Dos respuestas simultáneas no se pierden

- **DADO QUE** dos coordinadores de "Becas Ingeniería" tienen abierta la misma consulta de María, que está abierta
- **CUANDO** ambos envían su respuesta casi al mismo tiempo
- **ENTONCES** las dos respuestas quedan registradas en la conversación con su autor, y la consulta queda en un único estado, sin datos contradictorios

##### Escenario 8: Sin consultas pendientes

- **DADO QUE** Carla coordina "Becas Salud" y ningún postulante o becario de ese programa ha enviado una consulta
- **CUANDO** Carla abre su lista de consultas
- **ENTONCES** el sistema muestra que no hay consultas pendientes

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| El equipo de la Fundación puede responder cada consulta y darle seguimiento | Regla | 1, 3 | CUBIERTA |
| Estado: abierta, respondida, cerrada | Regla | 1, 3, 4 | CUBIERTA |
| Los mensajes se ven casi al instante, sin recargar la página | Regla | 2 | CUBIERTA |
| El coordinador solo ve y atiende consultas de los programas que coordina (RF-37) | Regla / FA-3 | 5, 6 | CUBIERTA |
| Dos coordinadores a la vez no dejan datos contradictorios ni pierden una respuesta | Regla / FA-4 | 7 | CUBIERTA |
| FA-1 Sin consultas: el sistema muestra que no hay consultas pendientes | FA | 8 | CUBIERTA |
| FA-2 Respuesta vacía | FA | 4 | CUBIERTA |
| Técnica — fuente de verdad única del estado en la base de datos principal; Firebase solo como transporte | Técnica | — | SIN ESCENARIO (técnica) |
| Técnica — control de concurrencia (bloqueo optimista o equivalente) | Técnica | 7 | CUBIERTA (se verifica el efecto; el mecanismo queda a diseño) |
| Técnica — historial íntegro con autor y fecha | Técnica / RNF | 7 | CUBIERTA (parcial: el autor se verifica en 7; la fecha no tiene escenario) |
| Técnica — mecanismo de tiempo real compartido con RF-34 y RF-40 | Técnica | — | SIN ESCENARIO (técnica, decisión de arquitectura) |
| Si el postulante o becario puede cerrar su consulta; si una consulta respondida o cerrada se reabre al escribir de nuevo | Sin definir en el PRD | — | NO PROBABLE |
| Qué ve el segundo coordinador cuando otro acaba de responder | Sin definir en el PRD | — | NO PROBABLE |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-32.2).*

## RF-33 — Reporte de impacto por programa o donante · *Debe tener*

### HU-33.1 Generar el reporte de impacto de un donante con datos agregados o anonimizados

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-33 | Debe tener | HU-13.2, HU-20.1, HU-25.1, HU-33.2, HU-38.1, HU-37.1 | HU-33.1, HU-33.2 |

#### Narrativa

- Como coordinador de programa
- Quiero generar el reporte de impacto de un donante y que solo muestre datos agregados o anonimizados
- Para mostrarle cómo se usó su aporte en minutos y no en días, sin exponer información sensible de los becarios

#### Reglas de negocio

- Cada donante recibe un reporte periódico sobre el uso de su aporte (RF-33).
- El reporte incluye número de becarios, perfil general, ejecución presupuestaria y resultados académicos agregados (RF-33).
- El reporte cubre los programas que financia el donante según sus aportes registrados (RF-13, ver HU-13.2).
- El donante no usa la plataforma: el equipo de la Fundación le comparte el reporte (PRD, Fuera de alcance).
- El reporte no expone información sensible individual, como el ingreso exacto de una familia; solo datos agregados o anonimizados (RF-33).
- La excepción es el becario que aceptó explícitamente ser presentado de forma individual a su donante (RF-33, ver HU-33.2).
- Los donantes aceptan reportes agregados o anonimizados como parte de su acuerdo (Supuestos del PRD).
- La información socioeconómica solo es visible para quien evalúa o decide (RNF de confidencialidad).
- El personal interno accede solo a lo que corresponde a su función (RF-37, ver HU-37.1); generar el reporte de impacto de un donante queda limitado a ese alcance.
- El periodo del reporte es uno de los periodos del calendario académico institucional (RF-38, ver HU-38.1).
- El desembolsado que alimenta la ejecución presupuestaria solo incluye desembolsos confirmados (RF-20, ver HU-20.1 y HU-20.2).
- Los resultados académicos agregados se alimentan de la información académica de los becarios, que puede llegar por **API desde la universidad** o por **registro manual** de la persona que revisa las notas enviadas por el estudiante; el reporte usa esa información sin depender de su origen (ver HU-24.1 y la historia nueva de integración del Área 5).
- Sin definir en el PRD: cada cuánto se emite el reporte, qué datos forman el "perfil general" de los becarios y, si un programa tiene varios donantes, si el reporte muestra las cifras de todo el programa o solo la parte del aporte de ese donante.
- Sin definir en el PRD: qué datos del becario se muestran cuando aceptó ser presentado de forma individual (por ejemplo, si incluyen datos socioeconómicos).
- Sin definir en el PRD: quién genera el reporte cuando el donante financia programas de coordinadores distintos. Los escenarios usan un donante cuyos programas coordina quien genera el reporte.

#### Flujo de la HU

1. El coordinador, con su sesión verificada en dos pasos, abre la generación del reporte de impacto.
2. El coordinador elige un donante y el periodo del reporte entre los periodos del calendario académico (HU-38.1).
3. El sistema verifica que el usuario tiene la función que le corresponde para generar ese reporte (HU-37.1).
4. El sistema identifica los programas que ese donante financia en el periodo (HU-13.2).
5. El sistema reúne número de becarios, perfil general, ejecución presupuestaria con desembolsos confirmados (HU-20.1) y resultados académicos agregados (HU-25.1), con independencia de que la información académica haya llegado por API o por registro manual, sin competir con la operación diaria.
6. El sistema revisa qué becarios del reporte registraron su aceptación (HU-33.2).
7. Para los becarios sin aceptación, el sistema solo usa datos agregados o anonimizados.
8. El sistema muestra el reporte sin información sensible individual.
9. El coordinador lo obtiene para compartirlo con el donante.

#### Flujos alternos y de error

- FA-1 (paso 2) Periodo inexistente en el calendario académico: el sistema no genera el reporte e indica que ese periodo no existe.
- FA-2 (paso 3) Usuario sin esa función: el sistema niega el acceso y no genera el reporte.
- FA-3 (paso 4) Donante sin aportes en el periodo: el sistema no genera el reporte e informa el motivo.
- FA-4 (paso 5) Reporte en preparación: el sistema indica que el reporte se está preparando y el coordinador puede seguir usando la plataforma.
- FA-5 (paso 5) Sin reportes académicos en el periodo: la sección de resultados académicos indica que aún no hay datos.
- FA-6 (paso 6) Ningún becario aceptó: todo el reporte se presenta agregado o anonimizado.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-33.1).*

#### Criterios de aceptación (12 escenarios)

##### Escenario 1: Reporte de un donante

- **DADO QUE** "Fundación Horizonte" aporta a "Becas Ingeniería" en 2026-1, programa con 10 becarios y 4.000 USD desembolsados y confirmados
- **CUANDO** el coordinador genera el reporte de impacto de "Fundación Horizonte" para 2026-1
- **ENTONCES** el reporte muestra el número de becarios, el perfil general, la ejecución presupuestaria y los resultados académicos agregados de "Becas Ingeniería"

##### Escenario 2: Donante sin aportes en el periodo

- **DADO QUE** "Grupo Norte" no tiene aportes registrados en 2026-1
- **CUANDO** el coordinador genera su reporte para 2026-1
- **ENTONCES** el sistema no genera el reporte y muestra "Este donante no tiene aportes registrados en el periodo 2026-1."

##### Escenario 3: Varios programas del mismo donante

- **DADO QUE** "Fundación Horizonte" aporta a "Becas Ingeniería" y a "Becas Salud" en 2026-1, y el coordinador coordina ambos programas
- **CUANDO** el coordinador genera su reporte para 2026-1
- **ENTONCES** el reporte incluye los dos programas

##### Escenario 4: Sin datos individuales

- **DADO QUE** ningún becario de "Becas Ingeniería" registró su aceptación
- **CUANDO** el coordinador genera el reporte de impacto de su donante
- **ENTONCES** el reporte no muestra nombres, identificaciones ni el ingreso familiar de ningún becario

##### Escenario 5: Ingreso solo agregado

- **DADO QUE** los becarios de "Becas Ingeniería" declararon ingresos familiares distintos
- **CUANDO** el coordinador genera el reporte de impacto
- **ENTONCES** el reporte no muestra el ingreso exacto de ninguna familia

##### Escenario 6: Becario que aceptó ser presentado

- **DADO QUE** Carlos registró su aceptación para ser presentado a su donante y Ana no
- **CUANDO** el coordinador genera el reporte de impacto
- **ENTONCES** el reporte presenta a Carlos de forma individual y a Ana solo dentro de los datos agregados

##### Escenario 7: Rol sin esa función no genera el reporte

- **DADO QUE** Luis es miembro del comité evaluador de "Becas Ingeniería"
- **CUANDO** intenta generar el reporte de impacto de "Fundación Horizonte"
- **ENTONCES** el sistema le niega el acceso y no genera el reporte, consistente con RF-37

##### Escenario 8: Periodo que no existe en el calendario académico

- **DADO QUE** el periodo "2027-1" no está registrado en el calendario académico (HU-38.1)
- **CUANDO** el coordinador intenta generar el reporte de impacto para "2027-1"
- **ENTONCES** el sistema muestra "El periodo 2027-1 no existe en el calendario académico." y no genera el reporte

##### Escenario 9: La operación diaria no se detiene

- **DADO QUE** el coordinador está generando el reporte de impacto de un donante
- **CUANDO** en ese momento un postulante envía su postulación y finanzas confirma un desembolso
- **ENTONCES** ambas operaciones se completan sin esperar a que termine el reporte

##### Escenario 10: Desembolso fallido no cuenta en el reporte de impacto

- **DADO QUE** "Becas Ingeniería" tiene 4.000 USD desembolsados y confirmados para "Fundación Horizonte" en 2026-1, además de 500 USD marcados como fallidos y no resueltos (HU-20.2)
- **CUANDO** el coordinador genera el reporte de impacto de "Fundación Horizonte" para 2026-1
- **ENTONCES** la ejecución presupuestaria del reporte muestra 4.000 USD, sin incluir los 500 USD fallidos

##### Escenario 11: Sin reportes académicos en el periodo

- **DADO QUE** "Becas Ingeniería" no tiene reportes académicos registrados para 2026-1
- **CUANDO** el coordinador genera el reporte de impacto de "Fundación Horizonte" para 2026-1
- **ENTONCES** la sección de resultados académicos del reporte muestra "Aún no hay datos académicos para este periodo."

##### Escenario 12: Reporte en preparación

- **DADO QUE** el coordinador solicitó el reporte de impacto de "Fundación Horizonte" para 2026-1 y el sistema todavía no terminó de reunir la información
- **CUANDO** el coordinador abre la pantalla de postulaciones de "Becas Ingeniería"
- **ENTONCES** esa pantalla se muestra sin esperar a que termine el reporte, y el reporte sigue indicando "El reporte se está preparando."

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Reporte con número de becarios, perfil general, ejecución presupuestaria y resultados académicos agregados (RF-33) | Regla | 1 | CUBIERTA; el origen de la información académica (API o registro manual) se delega a HU-24.1 y a la historia de integración |
| Reporte cubre los programas que el donante financia según sus aportes (RF-13, ver HU-13.2) | Regla | 1, 3 | CUBIERTA (fuente de datos delegada a HU-13.2) |
| Donante sin aportes en el periodo: no se genera el reporte (FA-3) | FA | 2 | CUBIERTA |
| No expone información sensible individual; solo agregado/anonimizado, salvo excepción (RF-33 + RNF confidencialidad) | Regla | 4, 5 | CUBIERTA |
| Excepción: becario que aceptó explícitamente se presenta de forma individual (RF-33) | Regla | 6 | CUBIERTA (el registro del consentimiento en sí es HU-33.2) |
| Acceso limitado a la función que corresponde (RF-37, ver HU-37.1) | Regla / FA-2 | 7 | CUBIERTA aquí; la definición de funciones se delega a HU-37.1 |
| Periodo del reporte = uno de los periodos del calendario académico (RF-38, ver HU-38.1) | Regla / FA-1 | 8 | CUBIERTA aquí; la lista válida de periodos se delega a HU-38.1 |
| El reporte no detiene la operación diaria (RNF segundo plano) | Regla | 9, 12 | CUBIERTA |
| Ejecución presupuestaria solo incluye desembolsos confirmados, excluye fallidos/devueltos (RF-20, ver HU-20.1 y HU-20.2) | Regla | 10 | CUBIERTA |
| FA-5 Sin reportes académicos en el periodo | FA | 11 | CUBIERTA |
| FA-4 Reporte en preparación: el coordinador puede seguir usando la plataforma | FA | 12 | CUBIERTA |
| FA-6 Ningún becario aceptó: todo el reporte se presenta agregado/anonimizado | FA | 4 | CUBIERTA |
| Anonimización en una única capa de servicio; archivo en la cuenta Firebase existente; 2FA de sesión con canal elegido por el usuario, vigencia de 15 minutos y espera de 1 hora tras 3 intentos fallidos | Técnica | — | NO PROBABLE por escenario funcional (se verifica en revisión técnica y de configuración) |
| Periodicidad del reporte, contenido del "perfil general", alcance del reporte cuando el programa tiene varios donantes | Sin definir en el PRD | — | NO PROBABLE (preguntas abiertas 1, 2, 3 de la historia) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-33.1).*

### HU-33.2 Registrar el consentimiento para ser presentado a mi donante

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-33 | Debe tener | HU-33.1 | HU-33.3 |

#### Narrativa

- Como becario
- Quiero registrar si acepto ser presentado de forma individual a mi donante
- Para decidir yo si mis datos aparecen fuera de las cifras agregadas

#### Reglas de negocio

- Un becario solo se presenta de forma individual a su donante si lo aceptó explícitamente (RF-33).
- Sin esa aceptación, el becario solo aparece en datos agregados o anonimizados (RF-33, ver HU-33.1).
- La aceptación la da el propio becario (RF-33: "el becario haya aceptado explícitamente"); ningún otro rol la registra en su nombre.
- El manejo de esta información sensible cumple buenas prácticas de protección de datos personales (RNF de protección de datos personales).
- La aceptación registrada queda con su fecha y es el único insumo válido para que el camino dirigido al donante pueda mostrar datos individuales (RF-33, ver la capa de anonimización de HU-33.1).
- Sin definir en el PRD: si el becario puede retirar su aceptación y desde cuándo aplica el retiro.
- Sin definir en el PRD: si la aceptación vale para todos los donantes del becario o se registra por donante o programa. Los escenarios usan un becario con un solo donante.

#### Flujo de la HU

1. El becario abre la opción de presentación a su donante.
2. El sistema explica que, si acepta, puede aparecer de forma individual en el reporte de impacto de su donante.
3. El becario marca su aceptación y la confirma.
4. El sistema registra la aceptación con su fecha, como único registro vigente del becario.
5. Los siguientes reportes de impacto lo tienen en cuenta (HU-33.1).

#### Flujos alternos y de error

- FA-1 (paso 3) No marca la aceptación: no se registra nada y el becario sigue apareciendo solo en datos agregados.
- FA-2 (paso 3) Otro rol intenta registrar la aceptación por el becario: el sistema no ofrece esa opción, porque la aceptación solo la registra el becario.
- FA-3 (paso 4) El becario confirma de nuevo una aceptación ya registrada: el sistema conserva un único registro vigente, sin duplicarlo.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-33.2).*

#### Criterios de aceptación (4 escenarios)

##### Escenario 1: Aceptar ser presentado

- **DADO QUE** Carlos es becario de "Becas Ingeniería" y no ha registrado su aceptación
- **CUANDO** marca y confirma la aceptación
- **ENTONCES** el sistema registra su aceptación con la fecha y el siguiente reporte de impacto puede presentarlo de forma individual

##### Escenario 2: Sin aceptación explícita

- **DADO QUE** Ana es becaria y nunca abrió la opción de presentación
- **CUANDO** el coordinador genera el reporte de impacto
- **ENTONCES** Ana solo aparece dentro de los datos agregados

##### Escenario 3: Otro no puede aceptar por el becario

- **DADO QUE** un coordinador consulta la beca de Ana
- **CUANDO** busca registrar la aceptación en nombre de Ana
- **ENTONCES** el sistema no ofrece esa opción, porque la aceptación solo la registra el becario

##### Escenario 4: Confirmar dos veces no duplica el registro

- **DADO QUE** Carlos ya registró su aceptación con fecha 01-oct-2026
- **CUANDO** vuelve a abrir la opción y confirma de nuevo
- **ENTONCES** el sistema conserva un único registro vigente de su aceptación, sin duplicarlo

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| El becario se presenta de forma individual solo si aceptó explícitamente (RF-33) | Regla | 1 | CUBIERTA |
| Sin aceptación, el becario solo aparece en datos agregados (RF-33, ver HU-33.1) | Regla / FA-1 | 2 | CUBIERTA |
| Solo el propio becario registra su aceptación; ningún otro rol lo hace en su nombre (RF-33) | Regla / FA-2 | 3 | CUBIERTA |
| El registro de aceptación es único y vigente; confirmar de nuevo no crea un segundo registro (Técnica) | Regla / FA-3 | 4 | CUBIERTA |
| La aceptación queda con su fecha y es el único insumo válido para mostrar datos individuales (RF-33) | Regla | 1, 4 | CUBIERTA |
| Manejo de esta información bajo buenas prácticas de protección de datos (RNF) | Regla | — | NO PROBABLE (no añade comportamiento propio verificable más allá del control de acceso de HU-37.1) |
| Nueva entidad de dominio del consentimiento (Técnica) | Técnica | — | NO PROBABLE por escenario funcional (se verifica en el modelo de dominio) |
| Retiro de la aceptación | Sin definir en el PRD | — | NO PROBABLE (pregunta abierta 1 de la historia) |
| Cardinalidad del consentimiento (por becario, por donante o por programa) | Sin definir en el PRD | — | NO PROBABLE (pregunta abierta 2 de la historia) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-33.2).*

## RF-34 — Reporte consolidado institucional · *Debe tener*

### HU-34.1 Consultar el reporte consolidado institucional por periodo, universidad y carrera

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-34 | Debe tener | HU-12.1, HU-14.1, HU-20.1, HU-20.2, HU-37.1, HU-38.1 | HU-34.1, HU-34.2 |

#### Narrativa

- Como dirección de la Fundación
- Quiero ver una vista general de todos los programas y desglosarla por periodo, universidad y carrera
- Para supervisar el conjunto de la Fundación sin armar hojas de cálculo y ver dónde se concentran postulantes, becarios y fondos

#### Reglas de negocio

- El reporte muestra, para todos los programas, el total de postulantes, los becarios activos, el presupuesto gestionado y el presupuesto desembolsado (RF-34).
- El reporte consolidado se puede ver por periodo, universidad y carrera (RF-34).
- Los periodos son los del calendario académico (RF-38, ver HU-38.1); las universidades y carreras, las de los convenios (RF-14, ver HU-14.1).
- El periodo de becas es común a todas las universidades y, además, cada universidad tiene sus propias fechas, incluida la de inicio de sus inscripciones, que se registra por universidad y periodo; el desglose de este reporte sigue usando los periodos del calendario académico institucional y esas fechas por universidad se registran en HU-38.1 y HU-14.1, no aquí.
- Los periodos anteriores siguen siendo consultables, aunque solo uno sea el "actual" (RF-38, ver HU-38.1).
- Una beca que no se renueva, o que estaba en pausa y vence sin reactivarse, queda *Inactiva* y no recibe dinero en el siguiente periodo; una beca *Inactiva* no cuenta como becario activo en este reporte, ni bajo la lectura amplia de S6 ni bajo la estricta. **Decisión del cliente:** *Inactiva* no es un estado final: la beca puede volver a *Activa* o pasar a *Concluida*, que sigue siendo estado final; si vuelve a *Activa*, vuelve a contar como becario activo en el periodo en que esté Activa.
- Una persona puede tener dos becas a la vez, dentro del límite de la regla de no doble beneficio (C2), por lo que la cifra de becarios activos puede no coincidir con el número de becas activas; los escenarios cuentan personas distintas y la definición que debe usar el reporte sigue como pregunta abierta.
- Una persona puede enviar varias postulaciones y volver a postular después de un rechazo, y su identificación única es el número de cédula, con la que se deduplica si el reporte cuenta personas; por eso el total de postulantes y el número de postulaciones pueden diferir.
- El desglose por universidad y por carrera usa los datos de la copia que la postulación guardó al enviarse, no el perfil actual del postulante, porque al postular se toma una copia del perfil.
- Consultar el reporte o su desglose no afecta el rendimiento de las postulaciones, evaluaciones y desembolsos, aunque crezca el volumen de datos (RF-34).
- Un reporte grande no bloquea ni ralentiza el uso normal del resto de la plataforma (RNF de procesamiento en segundo plano).
- El desembolsado solo incluye desembolsos confirmados (ver HU-20.1 y HU-20.2).
- La vista general de todos los programas corresponde a la función de Dirección; el resto del personal interno accede solo a lo que corresponde a su función (RF-34, RF-37, ver HU-37.1).
- La plataforma sostiene el crecimiento de 12 a más de 25 programas sin degradar la experiencia (RNF de escalabilidad de uso).
- Sin definir en el PRD: qué estados de beca cuentan como "becario activo". El Glosario dice "con una beca activa" y S6 define "beca activa" solo para la regla de no doble beneficio (Otorgada, Activa, Suspendida y Renovada). Los escenarios usan la definición estricta, solo becas *Activa*; la discrepancia entre ambas lecturas está en Preguntas abiertas porque cambia la cifra que ve Dirección. La comparación debe rehacerse sobre el S6 actualizado, porque la regla de no doble beneficio que lo sostiene se reformula (programas "completo" o "parcial" y tope de suma de becas) y porque el nuevo estado *Inactiva* se suma a la tabla de estados.
- Sin definir en el PRD: si el "total de postulantes" cuenta personas o postulaciones. Los escenarios usan postulantes con una sola postulación. La distinción es material, porque una persona puede presentar varias postulaciones y volver a postular tras un rechazo.
- Sin definir en el PRD: cómo se desglosa por universidad y por carrera el presupuesto gestionado, que se asigna por programa (RF-11). Los escenarios desglosan postulantes y becarios.
- Sin definir en el PRD: el tamaño mínimo de grupo a partir del cual una cifra agregada de un desglose podría identificar a una persona.

#### Flujo de la HU

1. Dirección, con su sesión verificada en dos pasos, abre el reporte consolidado institucional.
2. El sistema verifica que el usuario tiene la función que le corresponde (HU-37.1).
3. El sistema prepara el reporte sin detener la operación diaria.
4. El sistema muestra total de postulantes, becarios activos, presupuesto gestionado y presupuesto desembolsado de todos los programas.
5. Dirección elige desglosar por periodo, por universidad o por carrera, entre los periodos del calendario académico (HU-38.1) y las universidades y carreras de los convenios (HU-14.1).
6. Elige el valor del desglose (por ejemplo, el periodo 2026-1).
7. El sistema muestra las cifras del reporte para ese valor, tomando la universidad y la carrera de la copia que guardó cada postulación al enviarse.

#### Flujos alternos y de error

- FA-1 (paso 2) Usuario sin esa función: el sistema niega el acceso y no muestra el reporte.
- FA-2 (paso 3) Reporte en preparación: el sistema indica que el reporte se está preparando y Dirección puede seguir usando la plataforma.
- FA-3 (paso 4) Sin programas registrados: el reporte muestra los totales en 0.
- FA-4 (paso 6) Periodo inexistente en el calendario académico: el sistema lo indica y no muestra cifras de otro periodo.
- FA-5 (paso 7) Sin datos para el valor elegido: el sistema indica que no hay datos para ese periodo, universidad o carrera.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-34.1).*

#### Criterios de aceptación (12 escenarios)

##### Escenario 1: Vista general

- **DADO QUE** hay 2 programas con 120 postulantes, cada uno con una sola postulación, 30 becas de 30 personas distintas, todas Activa, 50.000 USD de presupuesto y 12.000 USD desembolsados y confirmados
- **CUANDO** Dirección abre el reporte consolidado
- **ENTONCES** ve 120 postulantes, 30 becarios activos, 50.000 USD gestionados y 12.000 USD desembolsados

##### Escenario 2: Desembolso fallido no cuenta

- **DADO QUE** un desembolso de 500 USD está marcado como fallido y no está resuelto (HU-20.2)
- **CUANDO** Dirección abre el reporte consolidado
- **ENTONCES** esos 500 USD no suman al presupuesto desembolsado

##### Escenario 3: La operación diaria no se detiene

- **DADO QUE** Dirección está generando el reporte consolidado
- **CUANDO** en ese momento un postulante envía su postulación y finanzas confirma un desembolso
- **ENTONCES** ambas operaciones se completan sin esperar a que termine el reporte

##### Escenario 4: Desglose por periodo

- **DADO QUE** hubo 80 postulantes en 2026-1 y 120 en 2026-2
- **CUANDO** Dirección desglosa el reporte por el periodo 2026-1
- **ENTONCES** ve 80 postulantes

##### Escenario 5: Desglose por universidad

- **DADO QUE** hay 20 becas Activa de 20 estudiantes distintos de UCAB y 10 de otra universidad aliada, y ninguna beca en otro estado
- **CUANDO** Dirección desglosa por la universidad UCAB
- **ENTONCES** ve 20 becarios activos

##### Escenario 6: Desglose por carrera sin datos

- **DADO QUE** ninguna postulación es de la carrera "Arquitectura"
- **CUANDO** Dirección desglosa por esa carrera
- **ENTONCES** el sistema muestra "No hay datos para la carrera Arquitectura."

##### Escenario 7: Rol sin esa función no abre el reporte consolidado

- **DADO QUE** Carla es coordinadora de "Becas Salud"
- **CUANDO** intenta abrir el reporte consolidado institucional
- **ENTONCES** el sistema le niega el acceso, consistente con RF-37

##### Escenario 8: Desglose por un periodo que no existe en el calendario académico

- **DADO QUE** el periodo "2028-1" no está registrado en el calendario académico (HU-38.1)
- **CUANDO** Dirección desglosa el reporte consolidado por "2028-1"
- **ENTONCES** el sistema muestra "El periodo 2028-1 no existe en el calendario académico." y no muestra cifras

##### Escenario 9: Sin programas registrados

- **DADO QUE** la Fundación no tiene ningún programa registrado
- **CUANDO** Dirección abre el reporte consolidado
- **ENTONCES** ve 0 postulantes, 0 becarios activos, 0 USD gestionados y 0 USD desembolsados

##### Escenario 10: Reporte en preparación

- **DADO QUE** Dirección abrió el reporte consolidado institucional y el sistema todavía no terminó de reunir la información
- **CUANDO** Dirección abre la pantalla de programas
- **ENTONCES** esa pantalla se muestra sin esperar a que termine el reporte, y el reporte sigue indicando "El reporte se está preparando."

##### Escenario 11: Una beca Inactiva no cuenta como becario activo

- **DADO QUE** en 2026-2 hay 30 becas Activa de 30 personas distintas y 5 becas que quedaron *Inactiva* porque no se renovaron para ese periodo
- **CUANDO** Dirección abre el reporte consolidado de 2026-2
- **ENTONCES** ve 30 becarios activos y las 5 becas *Inactiva* no suman a esa cifra

##### Escenario 12: Una beca que vuelve de Inactiva a Activa cuenta de nuevo como becario activo

- **DADO QUE** en 2026-2 hay 25 becas Activa de 25 personas distintas y una beca que estuvo *Inactiva* en 2026-1 volvió a *Activa* para 2026-2
- **CUANDO** Dirección abre el reporte consolidado de 2026-2
- **ENTONCES** ve 26 becarios activos, incluida la beca que volvió de *Inactiva* a *Activa*

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Reporte muestra total de postulantes, becarios activos, presupuesto gestionado y desembolsado de todos los programas (RF-34) | Regla | 1 | CUBIERTA |
| Desglose por periodo, universidad y carrera (RF-34) | Regla | 4, 5, 6 | CUBIERTA |
| Periodos = calendario académico; los periodos anteriores siguen consultables (RF-38, ver HU-38.1) | Regla / FA-4 | 4, 8 | CUBIERTA aquí; la lista válida de periodos se delega a HU-38.1 |
| Universidades/carreras del desglose = las de los convenios (RF-14, ver HU-14.1) | Regla | 5, 6 | CUBIERTA aquí; la lista válida de valores se delega a HU-14.1 |
| Periodo de becas común y fechas propias de cada universidad, incluida la de inicio de inscripciones | Regla | — | DELEGADA a HU-38.1 y HU-14.1: registrar esas fechas no es comportamiento de este reporte |
| Una beca *Inactiva* no cuenta como becario activo ni recibe dinero en el periodo siguiente | Regla | 11 | CUBIERTA |
| *Inactiva* no es estado final: si la beca vuelve a *Activa*, vuelve a contar como becario activo | Regla | 12 | CUBIERTA |
| Consultar el reporte no afecta el rendimiento de postulaciones, evaluaciones y desembolsos (RF-34) | Regla | 3 | CUBIERTA |
| Un reporte grande no bloquea ni ralentiza el resto de la plataforma (RNF segundo plano) / FA-2 | Regla / FA-2 | 10 | CUBIERTA |
| El presupuesto desembolsado solo incluye desembolsos confirmados (ver HU-20.1 y HU-20.2) | Regla | 2 | CUBIERTA |
| Acceso limitado a la función de Dirección (RF-34, RF-37, ver HU-37.1) / FA-1 | Regla / FA-1 | 7 | CUBIERTA aquí; la definición de funciones se delega a HU-37.1 |
| FA-3 Sin programas registrados: los totales se muestran en 0 | FA | 9 | CUBIERTA |
| FA-5 Sin datos para el valor del desglose elegido | FA | 6 | CUBIERTA |
| El desglose usa la copia de universidad y carrera que guardó la postulación, no el perfil actual | Regla | — | **DIFERIDA**: el escenario de contraste exige saber si una actualización del perfil cambia la postulación ya enviada, que el cliente dejó por confirmar (pregunta abierta 6) |
| "Becario activo" como estado derivado de la máquina de estados (Técnica) | Técnica | 1, 5, 11, 12 | CUBIERTA en la cifra observable, incluido el nuevo estado *Inactiva* y su reversibilidad hacia *Activa*; que no se duplique en una marca independiente se verifica en el modelo |
| Ruta de lectura analítica separada de la transaccional (Técnica) | Técnica | 3, 10 | CUBIERTA en el comportamiento; la implementación y la prueba de carga se verifican aparte |
| La plataforma sostiene el crecimiento de 12 a más de 25 programas (RNF escalabilidad) | Regla | — | NO PROBABLE por escenario funcional (requiere prueba de carga y concurrencia) |
| Definición de "becario activo" (S6 amplia vs. estricta, con S6 en reformulación por C2 y *Inactiva* añadida por C3), si "becarios activos" cuenta personas o becas, "total de postulantes" (personas vs. postulaciones), desglose del presupuesto por universidad/carrera, umbral mínimo de grupo | Sin definir en el PRD | — | NO PROBABLE (preguntas abiertas 1 a 6 de la historia) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-34.1).*

## RF-35 — Trazabilidad y auditoría de decisiones · *Debe tener*

### HU-35.1 Registrar cada decisión con quién, cuándo y por qué, sin edición posterior

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-35 | Debe tener | HU-37.1; invocada por toda HU de decisión: HU-21.1, HU-22.1, HU-26.2, HU-04.1, HU-10.1 y cualquier otra que cambie el estatus de una postulación o beca | HU-35.1, HU-35.2 |

> > **Nota de alcance (consolidación):** esta HU no es una pantalla que Dirección opere. Es un comportamiento transversal invocado desde cada historia de decisión: Dirección es la beneficiaria de la trazabilidad (Objetivo 4 del PRD) y quien dispara la acción es el coordinador, el comité o finanzas, como muestran el flujo y los escenarios. Se mantiene el actor original porque el PRD no asigna la autoría de este requerimiento; si el cliente prefiere reencuadrarla al actor que decide, está planteado en Preguntas abiertas.

#### Narrativa

- Como dirección de la Fundación
- Quiero que cada decisión sobre un postulante o becario quede registrada con quién la tomó, cuándo y por qué, y que nadie pueda editar ni eliminar ese registro
- Para garantizar la trazabilidad completa de las decisiones y que el historial sea confiable ante cualquier revisión

#### Reglas de negocio

- Toda decisión que afecte a un postulante o becario (aprobación, rechazo, suspensión, revocación, renovación) queda registrada con quién la tomó, cuándo y por qué (RF-35).
- Todo cambio de estatus de una beca queda justificado y registrado (regla transversal). A los estatus del PRD (otorgada, suspendida, revocada, renovada, concluida) se suma el paso a *Inactiva* de una beca que no se renueva o de una pausa que vence sin reactivarse. **Decisión del cliente:** *Inactiva* no es un estado final: desde *Inactiva* la beca puede volver a *Activa* o pasar a *Concluida*, que sigue siendo un estado final.
- Toda acción relevante sobre una postulación o beca queda registrada de forma permanente (RNF de trazabilidad).
- Una decisión sin justificación no se aplica (RF-35, regla transversal).
- El registro de decisiones no puede editarse ni eliminarse después de registrado (RF-35).
- Esta restricción aplica a todos los roles, incluida Dirección (RF-35).
- No hay edición retroactiva silenciosa de ninguna acción relevante (RNF de trazabilidad).
- Solo quien tiene poder de decisión sobre el caso según su función registra la decisión; el resto del personal y los actores externos no (RF-35, RF-37, ver HU-37.1).
- La justificación no habilita una transición de estado prohibida: la decisión solo se aplica si la transición es una de las permitidas por las reglas transversales del PRD. La validación de la transición vive en la HU de cada decisión (ver HU-04.1 para los saltos de etapa de la postulación); esta HU no la duplica, exige la justificación y el registro.
- El incumplimiento de las condiciones de permanencia cuenta como incumplimiento grave y, por tanto, habilita la revocación directa desde cualquier estado activo; la validación de esa transición sigue viviendo en la HU de la decisión (ver HU-22.1 y HU-25.1) y aquí solo se exigen la justificación y el registro.
- **Decisión del cliente:** una beca *Inactiva* también se puede revocar, aunque *Inactiva* no cuente como estado activo (C3); esa decisión se registra con la misma justificación y trazabilidad que cualquier otra revocación. La validación de esa transición sigue viviendo en la HU de la decisión (ver HU-22.1) y aquí solo se exigen la justificación y el registro.
- Quien declaró un conflicto de interés con el postulante o becario no participa en la decisión sobre ese caso (regla transversal, RF-39, ver HU-39.1). Esta HU no implementa esa validación: la referencia. Cuando quien queda apartado es un miembro del comité, su lugar pasa a otro evaluador del comité, que es quien queda habilitado para decidir y para firmar el registro.
- El motivo que se registra en una suspensión es el mismo que se comunica al becario en su correo de suspensión; el envío de esa notificación vive en su propia HU (RF-21, RF-29), aquí solo se registra.
- El paso de una beca a *Inactiva* ocurre sin que una persona decida (no renovación en plazo o pausa vencida), de modo que no tiene autor ni justificación escrita como el resto de las decisiones. Con qué autoría y con qué justificación se registra ese cambio queda como pregunta abierta. **Decisión del cliente:** la transición de salida de *Inactiva* (a *Activa* o a *Concluida*) la decide el coordinador de programa —. **Decisión del cliente:** el requisito para salir de *Inactiva* es que el becario suba los datos que le falten; con eso el coordinador de programa decide el paso a *Activa* o a *Concluida* —. Sigue abierto con qué autoría y justificación se registraría esa decisión, tanto la entrada a *Inactiva* como la salida.

#### Flujo de la HU

1. Un usuario interno con poder de decisión sobre el caso toma una decisión desde su propia HU (por ejemplo, HU-21.1 suspender, HU-22.1 revocar, HU-26.2 renovar).
2. El sistema le pide la justificación.
3. El usuario escribe la justificación y confirma.
4. El sistema aplica la decisión y registra, en la misma unidad atómica, quién la tomó, cuándo, qué decisión fue, sobre qué postulación o beca y el porqué.
5. Un usuario interno consulta una decisión registrada (HU-35.2).
6. El sistema la muestra solo para lectura.
7. Si el usuario intenta modificarla o borrarla, el sistema lo impide.
8. Una decisión posterior sobre el mismo caso crea un registro nuevo; el anterior se conserva.

#### Flujos alternos y de error

- FA-1 (paso 1) Usuario sin poder de decisión sobre el caso: el sistema le niega la acción y no se registra ninguna decisión.
- FA-2 (paso 1) Transición de estado no permitida: la HU de la decisión rechaza la operación antes de pedir la justificación y el caso queda igual (reglas transversales; ver HU-04.1 para la postulación).
- FA-3 (paso 1) Conflicto de interés declarado sobre el caso: el sistema le impide decidir (ver HU-39.1) y, en el comité, la decisión queda en manos de otro evaluador del mismo comité.
- FA-4 (paso 3) Sin justificación: el sistema no aplica la decisión y pide la justificación.
- FA-5 (paso 4) No se puede guardar el registro: el sistema no aplica la decisión e informa el error.
- FA-6 (paso 4) Dos decisiones simultáneas sobre el mismo caso: el sistema resuelve una sola a la vez, cada una con su registro, y no deja un cambio de estado sin registro ni registros contradictorios.
- FA-7 (paso 7) Intento de edición o eliminación: el sistema lo rechaza y el registro queda igual.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-35.1).*

#### Criterios de aceptación (8 escenarios)

##### Escenario 1: Suspensión registrada

- **DADO QUE** la beca de Carlos está Activa
- **CUANDO** la coordinadora Laura la suspende el 05-oct-2026 con la justificación "No entregó constancia de matrícula"
- **ENTONCES** el registro guarda: Laura, 05-oct-2026, suspensión, beca de Carlos y "No entregó constancia de matrícula"

##### Escenario 2: Decisión sin justificación

- **DADO QUE** Laura va a revocar la beca de Carlos
- **CUANDO** confirma sin escribir la justificación
- **ENTONCES** la beca no cambia de estado y el sistema muestra "Escribe la justificación de esta decisión."

##### Escenario 3: Falla al registrar

- **DADO QUE** Laura aprueba la renovación de la beca de Ana con justificación
- **CUANDO** el sistema no logra guardar el registro de la decisión
- **ENTONCES** la beca de Ana no cambia de estado y el sistema muestra "No se pudo registrar la decisión. No se aplicó ningún cambio."

##### Escenario 4: Registro solo de lectura

- **DADO QUE** existe el registro de la suspensión de la beca de Carlos
- **CUANDO** un miembro de Dirección lo abre
- **ENTONCES** ve la decisión sin opción para editarla ni eliminarla

##### Escenario 5: Intento de modificar

- **DADO QUE** existe el registro de la suspensión de la beca de Carlos
- **CUANDO** alguien intenta cambiar su justificación o borrarlo
- **ENTONCES** el sistema lo rechaza, muestra "El registro de decisiones no se puede modificar ni eliminar." y el registro queda igual

##### Escenario 6: Nueva decisión, nuevo registro

- **DADO QUE** la beca de Carlos fue suspendida y se registró esa decisión
- **CUANDO** la coordinadora la reanuda (HU-21.2)
- **ENTONCES** se crea un registro nuevo y el de la suspensión sigue igual

##### Escenario 7: Rol sin poder de decisión no puede registrar una decisión

- **DADO QUE** Jorge es el enlace de la universidad aliada de Carlos
- **CUANDO** intenta registrar una decisión de suspensión sobre la beca de Carlos
- **ENTONCES** el sistema le niega el acceso, no se aplica ningún cambio y no se registra ninguna decisión, consistente con RF-37

##### Escenario 8: Dos decisiones simultáneas sobre el mismo caso

- **DADO QUE** la beca de Carlos está Activa
- **CUANDO** la coordinadora Laura la revoca con la justificación "Información falsa en el ingreso familiar declarado" al mismo tiempo que finanzas confirma un desembolso de esa beca
- **ENTONCES** el sistema resuelve las dos operaciones una a la vez, la beca queda en un único estado final, cada operación aplicada queda con su propio registro y ningún cambio de estado queda sin registro

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Toda decisión queda registrada: quién, cuándo, qué decisión, sobre qué caso y por qué (RF-35) | Regla | 1 | CUBIERTA |
| Decisión sin justificación no se aplica (RF-35) / FA-4 | Regla / FA-4 | 2 | CUBIERTA |
| Registro no editable ni eliminable, para ningún rol incluida Dirección; sin edición retroactiva silenciosa (RF-35, RNF trazabilidad) / FA-7 | Regla / FA-7 | 4, 5 | CUBIERTA |
| Solo quien tiene poder de decisión sobre el caso registra la decisión (RF-35, RF-37) / FA-1 | Regla / FA-1 | 7 | CUBIERTA aquí; la definición de funciones se delega a HU-37.1 |
| La justificación no habilita una transición de estado prohibida / FA-2 | Regla / FA-2 | — | DELEGADA a HU-04.1 y a la HU de cada decisión (las reglas transversales de la máquina de estados se prueban allí) |
| Incumplir las condiciones de permanencia es incumplimiento grave y habilita revocar directamente | Regla | — | DELEGADA a HU-22.1 y HU-25.1: aquí solo se exigen la justificación y el registro de esa decisión |
| Una beca *Inactiva* también se puede revocar | Regla | — | DELEGADA a HU-22.1: la validación de esa transición se prueba allí, aquí solo se exigen la justificación y el registro |
| Quien declaró conflicto de interés con el caso no decide; en el comité, su lugar pasa a otro evaluador del comité / FA-3 | Regla / FA-3 | — | DELEGADA a HU-39.1 |
| Registro del paso automático a *Inactiva* (beca no renovada o pausa vencida) y de su salida (a *Activa* o a *Concluida*, cuando el becario sube los datos que le falten) | Regla | — | **DIFERIDA**: el requisito de salida ya está resuelto (), pero no se puede fijar el resultado esperado del registro sin saber con qué autoría y justificación se anota un cambio que ninguna persona decide directamente (pregunta abierta 4) |
| El motivo registrado en una suspensión es el que se comunica al becario | Regla | 1 | CUBIERTA en el registro del motivo; el envío del correo se delega a la HU de notificación |
| Atomicidad: el cambio de estado y el registro son una sola unidad (Técnica) / FA-5 | Regla / FA-5 | 3 | CUBIERTA |
| Inmutabilidad a nivel de permisos de datos, no solo de interfaz (Técnica) | Técnica | 4, 5 | CUBIERTA en el comportamiento observable; el permiso append-only del almacén se verifica con revisión técnica, no por escenario de interfaz |
| Consistencia bajo concurrencia: dos decisiones simultáneas no dejan registros contradictorios ni cambios de estado sin registro (RNF) / FA-6 | Regla / FA-6 | 8 | CUBIERTA |
| Una decisión posterior crea un registro nuevo; el anterior se conserva | Regla | 6 | CUBIERTA |
| El registro de la decisión en sí no exige OTP (Técnica) | Técnica | 1, 8 | CUBIERTA por ausencia: ningún escenario introduce un código de un solo uso en esta HU |
| Secuenciación como infraestructura compartida y modelo de datos anticipado para HU-36.1 (Técnica) | Técnica | — | NO PROBABLE por escenario funcional (decisión de plan y de modelo) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-35.1).*

### HU-35.2 Consultar el registro de decisiones

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-35 | Debe tener | HU-35.1, HU-37.1 | HU-35.3 |

#### Narrativa

- Como dirección de la Fundación
- Quiero consultar las decisiones registradas sobre un postulante o becario
- Para saber quién decidió qué, cuándo y por qué

#### Reglas de negocio

- Cada decisión registrada muestra quién la tomó, cuándo y por qué (RF-35).
- La consulta no permite modificar el registro (RF-35, ver HU-35.1).
- El acceso al registro queda limitado a lo que corresponde a la función de cada usuario (RF-37, ver HU-37.1); una persona no consulta las decisiones de un caso que no es el suyo ni está bajo su responsabilidad.
- La información socioeconómica que pudiera aparecer en una justificación sigue siendo visible solo para quien la necesita para evaluar o decidir (RNF de confidencialidad).
- Sin definir en el PRD: si además de Dirección otro rol interno (por ejemplo, el coordinador sobre los casos de su programa, o el administrador financiero) puede consultar el registro de decisiones. RF-37 solo habla de "acceso limitado a lo que corresponde a su función", sin precisar si la auditoría de decisiones forma parte de esa función. Los escenarios cubren a Dirección y el caso claro de un becario ajeno al caso.

#### Flujo de la HU

1. Dirección, con su sesión verificada en dos pasos, busca la postulación o beca de una persona.
2. Abre su registro de decisiones.
3. El sistema verifica que el usuario tiene la función que le corresponde para consultar ese caso (HU-37.1).
4. El sistema muestra las decisiones en orden de fecha, con tipo, quién, cuándo y por qué, solo para lectura.

#### Flujos alternos y de error

- FA-1 (paso 3) Usuario sin esa función o ajeno al caso: el sistema le niega el acceso y no muestra ninguna decisión.
- FA-2 (paso 4) Sin decisiones: el sistema indica que ese caso aún no tiene decisiones registradas.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-35.2).*

#### Criterios de aceptación (4 escenarios)

##### Escenario 1: Ver las decisiones de una beca

- **DADO QUE** la beca de Carlos fue suspendida el 05-oct-2026 y reanudada el 20-oct-2026
- **CUANDO** Dirección abre el registro de decisiones de esa beca
- **ENTONCES** ve las dos decisiones en orden de fecha, cada una con quién la tomó, cuándo y por qué

##### Escenario 2: Caso sin decisiones

- **DADO QUE** la postulación de Jorge está Recibida y no tiene decisiones
- **CUANDO** Dirección abre su registro de decisiones
- **ENTONCES** el sistema muestra "Este caso aún no tiene decisiones registradas."

##### Escenario 3: Consulta sin cambios

- **DADO QUE** Dirección consulta el registro de la beca de Carlos
- **CUANDO** termina la consulta
- **ENTONCES** el registro queda igual que antes de consultarlo

##### Escenario 4: Un becario no puede ver el registro de otro becario

- **DADO QUE** Ana es becaria de "Becas Ingeniería"
- **CUANDO** intenta consultar el registro de decisiones de la beca de Carlos
- **ENTONCES** el sistema le niega el acceso

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Cada decisión consultada muestra quién la tomó, cuándo y por qué (RF-35, ver HU-35.1) | Regla | 1 | CUBIERTA |
| Las decisiones se muestran en orden de fecha (flujo paso 4) | Regla | 1 | CUBIERTA |
| La consulta no permite modificar el registro (RF-35) | Regla | 3 | CUBIERTA |
| Acceso limitado a lo que corresponde a la función del usuario (RF-37, ver HU-37.1) / FA-1 | Regla / FA-1 | 4 | CUBIERTA aquí para el caso ajeno al caso; qué roles internos quedan habilitados depende de la pregunta abierta 1 de la historia |
| Información socioeconómica en una justificación visible solo para quien la necesita (RNF confidencialidad) | Regla | 4 | CUBIERTA por el mismo control de acceso; no agrega un comportamiento propio distinto |
| FA-2 Caso sin decisiones registradas | FA | 2 | CUBIERTA |
| Acceso de solo lectura a nivel de datos y 2FA de sesión, con canal elegido por el usuario, vigencia de 15 minutos y espera de 1 hora tras 3 intentos fallidos (Técnica) | Técnica | 3 | CUBIERTA en el comportamiento observable; los permisos del almacén se verifican con revisión técnica |
| Si el coordinador o el administrador financiero pueden consultar el registro de su ámbito; si el postulante o becario ve el de su propio caso | Sin definir en el PRD | — | NO PROBABLE (preguntas abiertas 1 y 2 de la historia) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-35.2).*

## RF-36 — Exportación de información para auditorías externas · *Podría tener*

### HU-36.1 Exportar el uso de los fondos y el registro de decisiones para una auditoría

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-36 | Podría tener | HU-20.1, HU-23.1, HU-35.2, HU-37.1 | HU-36.1, HU-36.2 |

#### Narrativa

- Como dirección de la Fundación
- Quiero exportar la información sobre el uso de los fondos y el registro de decisiones
- Para entregarla a entes reguladores o auditorías externas y respaldar ante ellos la trazabilidad de las decisiones

#### Reglas de negocio

- La Fundación, como organización sin fines de lucro, genera información para entes reguladores o auditorías externas sobre el uso de los fondos (RF-36).
- Las cifras de cada programa son las del estado financiero: comprometido, desembolsado y disponible (RF-23, ver HU-23.1).
- Todas las becas son un monto fijo por periodo: ya no existen las coberturas total ni parcial como forma de calcular el monto. El monto de una beca nueva lo fija el programa y el de una renovación se asigna a cada estudiante; la exportación refleja esos montos tal como los calcula el estado financiero (ver HU-23.1 y HU-19.1).
- El presupuesto comprometido incluye las ofertas pendientes de aceptación, que siguen restando presupuesto hasta que el proceso se rechace definitivamente; la composición de la cifra vive en HU-23.1 y la exportación la toma de allí.
- El estado financiero exportado refleja el estado real considerando becas pendientes, suspendidas y revocadas (RF-23) y también las que quedaron *Inactiva*, que no reciben dinero en el periodo siguiente. **Decisión del cliente:** una beca *Inactiva* no libera su cupo ni su presupuesto: los mantiene ocupados mientras esté *Inactiva*, porque puede volver a *Activa*.
- Los desembolsos incluyen fecha, monto, método y destinatario (RF-20, ver HU-20.1).
- El método de cada desembolso exportado es el de la modalidad de pago que fija el programa.
- Cada decisión incluye quién la tomó, cuándo y por qué (RF-35, ver HU-35.1).
- Exportar no modifica ni elimina el registro de decisiones (RF-35, ver HU-35.1).
- El acceso a la exportación queda limitado a lo que corresponde a la función de cada usuario (RF-37, ver HU-37.1).
- La exportación no expone información sensible individual de los becarios más allá de lo que el PRD permite para cada destinatario (RNF de confidencialidad y de protección de datos personales).
- Sin definir en el PRD: el formato de la exportación y si los entes reguladores o auditores exigen un contenido o una estructura específicos. Esta HU exporta el estado financiero, los desembolsos y el registro de decisiones.
- Sin definir en el PRD: si la exportación del uso de los fondos puede iniciarla también el administrador financiero, que es quien registra y controla los desembolsos. Los escenarios usan a Dirección.

#### Flujo de la HU

1. Dirección, con su sesión verificada en dos pasos, abre la opción de exportación para auditoría y elige qué exportar: el uso de los fondos o el registro de decisiones.
2. El sistema verifica que el usuario tiene la función que le corresponde (HU-37.1).
3. Elige un programa o todos los programas.
4. Si exporta el uso de los fondos, el sistema reúne el estado financiero de cada programa (HU-23.1) y sus desembolsos registrados (HU-20.1).
5. Si exporta el registro de decisiones, el sistema reúne las decisiones registradas con su tipo, caso, quién, cuándo y por qué, sin modificarlas.
6. El sistema genera la exportación en segundo plano y Dirección la descarga.

#### Flujos alternos y de error

- FA-1 (paso 2) Usuario sin esa función: el sistema niega la exportación y no genera ningún archivo.
- FA-2 (paso 3) Programa inexistente o identificador inválido: el sistema lo informa y no genera un archivo vacío ni parcial.
- FA-3 (paso 4) Programa sin desembolsos: la exportación incluye su estado financiero e indica que no tiene desembolsos.
- FA-4 (paso 5) Sin decisiones: el sistema indica que no hay decisiones registradas para exportar.
- FA-5 (paso 6) Exportación en preparación: el sistema indica que la exportación se está generando y Dirección puede seguir usando la plataforma.
- FA-6 (paso 6) Falla la generación: el sistema informa el error y no entrega un archivo incompleto.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-36.1).*

#### Criterios de aceptación (9 escenarios)

##### Escenario 1: Exportar el uso de fondos de un programa

- **DADO QUE** "Becas Ingeniería" tiene 20.000 USD comprometidos, 8.000 USD desembolsados y 12.000 USD disponibles
- **CUANDO** Dirección exporta el uso de fondos de ese programa
- **ENTONCES** la exportación muestra esas tres cifras y cada desembolso con fecha, monto, método y destinatario

##### Escenario 2: Programa sin desembolsos

- **DADO QUE** "Becas Salud" aún no tiene desembolsos
- **CUANDO** Dirección exporta su uso de fondos
- **ENTONCES** la exportación incluye su estado financiero e indica "Sin desembolsos registrados"

##### Escenario 3: Falla al exportar

- **DADO QUE** Dirección pide la exportación del uso de fondos de todos los programas
- **CUANDO** el sistema no logra generarla
- **ENTONCES** no se descarga ningún archivo y se muestra "No se pudo generar la exportación. Intenta de nuevo."

##### Escenario 4: Exportar decisiones

- **DADO QUE** "Becas Ingeniería" tiene 3 decisiones registradas
- **CUANDO** Dirección exporta su registro de decisiones
- **ENTONCES** la exportación muestra las 3 decisiones con tipo, caso, quién, cuándo y por qué

##### Escenario 5: Sin decisiones

- **DADO QUE** "Becas Salud" no tiene decisiones registradas
- **CUANDO** Dirección intenta exportar su registro de decisiones
- **ENTONCES** el sistema muestra "No hay decisiones registradas para exportar."

##### Escenario 6: El registro no cambia

- **DADO QUE** Dirección exportó el registro de decisiones de "Becas Ingeniería"
- **CUANDO** consulta el registro después (HU-35.2)
- **ENTONCES** las decisiones siguen iguales

##### Escenario 7: Programa inexistente

- **DADO QUE** Dirección elige exportar el uso de fondos de un programa con un identificador que no existe
- **CUANDO** confirma la exportación
- **ENTONCES** el sistema muestra "El programa indicado no existe." y no genera ningún archivo

##### Escenario 8: Rol sin esa función no puede exportar

- **DADO QUE** Laura es coordinadora de "Becas Ingeniería" y no tiene la función de Dirección
- **CUANDO** intenta exportar el uso de fondos de "Becas Ingeniería" para auditoría
- **ENTONCES** el sistema le niega la exportación y no genera ningún archivo, consistente con RF-37

##### Escenario 9: Exportación en preparación

- **DADO QUE** Dirección solicitó la exportación del registro de decisiones de "Becas Ingeniería" y el sistema todavía no terminó de generar el archivo
- **CUANDO** Dirección abre la pantalla de programas
- **ENTONCES** esa pantalla se muestra sin esperar a que termine la exportación, y la exportación sigue indicando "La exportación se está generando."

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Genera información sobre el uso de los fondos para entes reguladores o auditorías externas (RF-36) | Regla | 1 | CUBIERTA |
| Las cifras del programa son las del estado financiero: comprometido, desembolsado, disponible (RF-23, ver HU-23.1) | Regla | 1 | CUBIERTA (cifras fuente delegadas a HU-23.1, incluidas las ofertas pendientes dentro del comprometido y los montos fijos por periodo) |
| El estado financiero exportado también refleja las becas *Inactiva*, que no reciben dinero en el periodo siguiente y no liberan su cupo ni su presupuesto | Regla | — | DELEGADA a HU-23.1: la composición de la cifra se prueba allí, no en el canal de exportación |
| Los desembolsos exportados incluyen fecha, monto, método y destinatario (RF-20, ver HU-20.1) | Regla | 1 | CUBIERTA; el método corresponde a la modalidad de pago que fija el programa, delegada a HU-19.1 |
| Cada decisión exportada incluye quién, cuándo y por qué (RF-35, ver HU-35.1) | Regla | 4 | CUBIERTA |
| Exportar no modifica ni elimina el registro de decisiones (RF-35) | Regla | 6 | CUBIERTA |
| Acceso limitado a la función correspondiente (RF-37, ver HU-37.1) / FA-1 | Regla / FA-1 | 8 | CUBIERTA aquí; la definición de funciones se delega a HU-37.1 |
| FA-2 Programa inexistente: no se genera archivo vacío ni parcial | FA | 7 | CUBIERTA |
| FA-3 Programa sin desembolsos | FA | 2 | CUBIERTA |
| FA-4 Sin decisiones para exportar | FA | 5 | CUBIERTA |
| FA-5 Exportación en preparación: Dirección puede seguir usando la plataforma (RNF segundo plano) | FA / Regla | 9 | CUBIERTA |
| FA-6 Falla la generación: no se entrega un archivo incompleto | FA | 3 | CUBIERTA |
| La exportación no expone información sensible individual más allá de lo permitido; misma capa de anonimización que HU-33.1 | Regla | — | **DIFERIDA**: el PRD no define a qué nivel de detalle individual tiene derecho un ente regulador, así que no se puede fijar el resultado esperado sin inventarlo. Bloqueada por la pregunta abierta nueva de esta historia |
| Archivo en la cuenta Firebase existente, solo lectura sobre el registro, 2FA de sesión con canal elegido por el usuario, vigencia de 15 minutos y espera de 1 hora tras 3 intentos fallidos, sin OTP (Técnica) | Técnica | 6, 9 | CUBIERTA parcialmente en el comportamiento; el almacenamiento y los permisos se verifican con revisión técnica |
| Formato exigido por el ente regulador o auditor; si el administrador financiero puede exportar; división de la historia | Sin definir en el PRD | — | NO PROBABLE (preguntas abiertas 1, 2 y 3 de la historia) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-36.1).*
