# Área 5 — Seguimiento Académico y Renovación

> Parte de las [Historias de Usuario · Plan B · versión final](README.md). Reglas técnicas en el [Anexo A](anexo-a-reglas-tecnicas.md); preguntas abiertas en el [Anexo B](anexo-b-preguntas-abiertas.md).

## RF-24 — Registro de reportes académicos periódicos · *Debe tener*

### HU-24.1 Entregar el reporte académico del periodo, con alerta de entrega tardía

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-24 | Debe tener | HU-16.1, HU-38.1 | HU-24.1, HU-24.2 |

#### Narrativa

- Como becario
- Quiero entregar cada periodo mi información de rendimiento académico, y que una entrega posterior a la fecha límite se registre igual generando una alerta para el coordinador
- Para demostrar que sigo cumpliendo las condiciones de mi beca, y que el coordinador detecte a tiempo las entregas tardías

#### Reglas de negocio

- Cada periodo, el becario entrega su promedio, materias aprobadas, materias reprobadas y estado de matrícula (RF-24).
- El reporte queda ligado a un periodo académico registrado (RF-38; ver HU-38.1).
- Existe una fecha límite de entrega del reporte académico por periodo (RF-24); la registra el coordinador del programa.
- Las fechas límite se evalúan en la zona horaria **UTC-4** y el día de cierre se incluye **hasta las 23:59**: una entrega hasta las 23:59 del día límite no es tardía.
- La información académica del becario puede llegar por tres vías: la entrega del propio becario (esta HU), el registro manual del enlace de la universidad (HU-24.2) y la integración por API con la universidad (HU-24.3).
- La información académica también puede registrarse manualmente por una persona que revisa las notas que envía el estudiante; quién cumple ese rol está por confirmar, por lo que esta HU no fija ese actor ni su flujo (ver Preguntas abiertas).
- Una entrega posterior a la fecha límite genera una alerta para el coordinador (RF-24).
- La entrega tardía se registra igual: la alerta no impide recibir el reporte (RF-24).
- La fecha límite y la alerta también aplican cuando el reporte lo registra el enlace de la universidad (RF-24; ver HU-24.2) o cuando llega por API (HU-24.3).
- Un becario solo entrega y consulta el reporte de su propia beca (RF-24; RNF de protección de datos personales).

#### Flujo de la HU

1. El coordinador registra la fecha límite de entrega de reportes del programa para un periodo académico registrado (HU-38.1).
2. El becario entra a su beca y elige "Entregar reporte académico".
3. Selecciona el periodo académico del reporte entre los periodos registrados.
4. Registra su promedio, materias aprobadas, materias reprobadas y estado de matrícula.
5. Envía el reporte.
6. El sistema lo registra con su fecha de entrega y lo deja disponible para evaluar el cumplimiento (HU-25.1).
7. El sistema compara la fecha de entrega con la fecha límite del periodo, en UTC-4 y considerando el día de cierre hasta las 23:59.
8. Si la entrega es posterior a la fecha límite, el reporte queda registrado igual y el sistema genera una alerta de entrega tardía para el coordinador del programa.

#### Flujos alternos y de error

- FA-1 (paso 5) Datos incompletos: el sistema no registra el reporte y muestra "Completa promedio, materias aprobadas, materias reprobadas y estado de matrícula."
- FA-2 (paso 7) Entrega a tiempo: el reporte se registra sin alerta; se considera a tiempo toda entrega hecha hasta las 23:59 (UTC-4) del día límite.
- FA-3 (paso 3) Periodo no registrado: el sistema no ofrece periodos académicos que no estén registrados (HU-38.1).
- FA-4 (paso 2) Beca de otra persona: el sistema solo ofrece la entrega sobre las becas del propio becario.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-24.1).*

#### Criterios de aceptación (8 escenarios)

##### Escenario 1: Entrega completa a tiempo

- **DADO QUE** Pedro tiene una beca *Activa* en el periodo 2026-2 y la fecha límite de reportes de 2026-2 es el 20/02/2027
- **CUANDO** entrega el 15/02/2027 su reporte con promedio 16,5, 6 materias aprobadas, 0 reprobadas y estado de matrícula "Inscrito"
- **ENTONCES** el reporte queda registrado para 2026-2 con la fecha de entrega y no se genera alerta

##### Escenario 2: Falta un dato

- **DADO QUE** Pedro está llenando su reporte de 2026-2
- **CUANDO** lo envía sin el estado de matrícula
- **ENTONCES** el sistema no lo registra y muestra "Completa promedio, materias aprobadas, materias reprobadas y estado de matrícula."

##### Escenario 3: Entrega tardía

- **DADO QUE** la fecha límite de reportes de 2026-2 es el 20/02/2027
- **CUANDO** Pedro entrega su reporte el 25/02/2027
- **ENTONCES** el reporte se registra y el coordinador ve la alerta "Reporte académico de Pedro Gómez entregado tarde (2026-2)"

##### Escenario 4: Solo su propia beca

- **DADO QUE** Pedro y Lucía son becarios del mismo programa
- **CUANDO** Pedro entra a "Entregar reporte académico"
- **ENTONCES** el sistema solo le ofrece la beca de Pedro y la beca de Lucía no aparece entre las opciones

##### Escenario 5: Entrega en la última hora de la fecha límite

- **DADO QUE** la fecha límite de reportes de 2026-2 es el 20/02/2027
- **CUANDO** Pedro entrega su reporte el 20/02/2027 a las 23:59 (UTC-4)
- **ENTONCES** el reporte se registra y no se genera alerta de entrega tardía, porque el día de cierre se incluye hasta las 23:59

##### Escenario 6: Periodo no registrado

- **DADO QUE** el periodo 2027-1 todavía no está registrado (HU-38.1)
- **CUANDO** Pedro abre "Entregar reporte académico"
- **ENTONCES** el sistema no ofrece 2027-1 como periodo disponible

##### Escenario 7: Registro de la fecha límite por el coordinador

- **DADO QUE** el periodo 2026-2 está registrado y "Becas Ingeniería" no tiene fecha límite de reportes para 2026-2
- **CUANDO** el coordinador registra el 20/02/2027 como fecha límite de entrega de reportes de 2026-2
- **ENTONCES** la fecha límite queda registrada para 2026-2 y disponible para comparar las entregas de los becarios del programa

##### Escenario 8: Entrega pasada la medianoche del día límite

- **DADO QUE** la fecha límite de reportes de 2026-2 es el 20/02/2027
- **CUANDO** Pedro entrega su reporte el 21/02/2027 a las 00:05 (UTC-4)
- **ENTONCES** el reporte se registra y se genera la alerta de entrega tardía para el coordinador, porque la entrega es posterior a las 23:59 del día de cierre

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Campos del reporte: promedio, materias aprobadas, materias reprobadas, estado de matrícula | Regla | 1, 2 | CUBIERTA |
| Reporte ligado a un periodo académico registrado | Regla | 1, 6 | CUBIERTA |
| Fecha límite de entrega por periodo, registrada por el coordinador | Regla | 7 | CUBIERTA |
| Fecha límite en UTC-4, con el día de cierre incluido hasta las 23:59 | Regla | 5, 8 | CUBIERTA |
| Entrega tardía genera alerta para el coordinador | Regla | 3, 8 | CUBIERTA |
| Entrega tardía se registra igual (no se bloquea) | Regla | 3, 8 | CUBIERTA |
| Frontera: entrega hasta las 23:59 del día límite no es tardía; a las 00:05 del día siguiente sí | Regla (frontera) | 5, 8 | CUBIERTA |
| Fecha límite y alerta también aplican cuando lo registra la universidad | Regla | — | DELEGADA (HU-24.2, Escenario 4) |
| Fecha límite y alerta también aplican cuando el reporte llega por API | Regla | — | DELEGADA (HU-24.3) |
| Tres vías de origen de la información académica (becario, enlace, API) | Regla | 1 (vía del becario) | CUBIERTA / DELEGADA (HU-24.2, HU-24.3) |
| Registro manual por una persona que revisa las notas que envía el estudiante | Regla | — | NO PROBABLE todavía: el actor está por confirmar (R13) |
| Becario solo entrega y consulta el reporte de su propia beca | Regla | 4 | CUBIERTA |
| Reporte append-only, sin edición ni eliminación retroactiva | Técnica | — | DELEGADA (HU-35.1) |
| Fecha límite consultable de forma independiente del flujo de entrega | Técnica | 7 (parcial) | DELEGADA (RF-30, Área 6) |
| Entrega posible con conectividad limitada | Técnica (RNF) | — | NO PROBABLE como criterio funcional |
| FA-1: Datos incompletos | FA | 2 | CUBIERTA |
| FA-2: Entrega a tiempo (sin alerta) | FA | 1, 5 | CUBIERTA |
| FA-3: Periodo no registrado | FA | 6 | CUBIERTA |
| FA-4: Beca de otra persona | FA | 4 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-24.1).*

### HU-24.2 Registrar el reporte académico desde la universidad

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-24 | Debe tener | HU-14.1, HU-24.1 | HU-24.3 |

#### Narrativa

- Como enlace de universidad aliada
- Quiero registrar el reporte académico de los becarios de mi universidad
- Para que la información de rendimiento venga certificada directamente por la universidad

#### Reglas de negocio

- Si hay un convenio formal de intercambio de datos con la universidad, la información académica puede venir directamente de ella en vez del becario (RF-24).
- El registro manual del enlace es una de las vías válidas de entrada de la información académica; la otra vía desde la universidad es el envío por API (HU-24.3), que no sustituye a esta.
- El enlace certifica la matrícula y el rendimiento de los becarios de su universidad, y solo de ella (Actores; RNF de protección de datos personales).
- El reporte contiene los mismos datos que el del becario: promedio, materias aprobadas, materias reprobadas y estado de matrícula (RF-24; ver HU-24.1).
- El reporte indica que su origen es la universidad (RF-24; RNF de trazabilidad).
- La fecha límite del periodo y la alerta de entrega tardía también aplican al reporte que registra la universidad (RF-24; ver HU-24.1), evaluadas en UTC-4 con el día de cierre incluido hasta las 23:59.

#### Flujo de la HU

1. El enlace entra a la lista de becarios de su universidad.
2. Elige un becario y el periodo académico.
3. Registra promedio, materias aprobadas, materias reprobadas y estado de matrícula.
4. Envía el reporte.
5. El sistema lo registra indicando que proviene de la universidad, con su autor y su fecha de entrega (HU-24.1).
6. El sistema compara la fecha de entrega con la fecha límite del periodo (UTC-4, día de cierre hasta las 23:59) y, si es posterior, genera la alerta de entrega tardía para el coordinador del programa.

#### Flujos alternos y de error

- FA-1 (paso 1) Convenio sin intercambio de datos: el sistema no ofrece la opción y muestra "El convenio con esta universidad no incluye intercambio de datos académicos."
- FA-2 (paso 4) Datos incompletos: el sistema no registra el reporte y muestra "Completa promedio, materias aprobadas, materias reprobadas y estado de matrícula."
- FA-3 (paso 1) Becario de otra universidad: el becario no aparece en la lista del enlace.
- FA-4 (paso 5) Reporte ya registrado: el registro existente conserva su autor y su fecha; ningún enlace puede editarlo ni eliminarlo (RNF de trazabilidad).

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-24.2).*

#### Criterios de aceptación (6 escenarios)

##### Escenario 1: Registro desde la universidad

- **DADO QUE** la Universidad del Norte tiene un convenio con intercambio de datos y Lucía es becaria de esa universidad
- **CUANDO** el enlace registra el reporte de Lucía para 2026-2
- **ENTONCES** el reporte queda registrado para 2026-2 con origen "Universidad del Norte"

##### Escenario 2: Convenio sin intercambio de datos

- **DADO QUE** el convenio con la Universidad del Sur no incluye intercambio de datos
- **CUANDO** su enlace intenta registrar un reporte académico
- **ENTONCES** el sistema no lo permite y muestra "El convenio con esta universidad no incluye intercambio de datos académicos."

##### Escenario 3: Becario de otra universidad

- **DADO QUE** Pedro es becario de la Universidad del Sur
- **CUANDO** el enlace de la Universidad del Norte busca a Pedro
- **ENTONCES** Pedro no aparece en su lista y no puede registrar su reporte

##### Escenario 4: Registro tardío desde la universidad

- **DADO QUE** la fecha límite de reportes de 2026-2 es el 20/02/2027
- **CUANDO** el enlace de la universidad registra el reporte de Lucía el 22/02/2027
- **ENTONCES** el reporte se registra y se genera la alerta de entrega tardía

##### Escenario 5: Aislamiento entre enlaces de la misma universidad

- **DADO QUE** el enlace A de la Universidad del Norte registró el reporte de Lucía para 2026-2
- **CUANDO** el enlace B, también de la Universidad del Norte, intenta modificarlo o eliminarlo
- **ENTONCES** no existe ninguna opción para editarlo ni eliminarlo y el registro conserva como autor al enlace A

##### Escenario 6: Datos incompletos desde la universidad

- **DADO QUE** el enlace de la Universidad del Norte está registrando el reporte de Lucía para 2026-2
- **CUANDO** lo envía sin indicar las materias reprobadas
- **ENTONCES** el sistema no lo registra y muestra "Completa promedio, materias aprobadas, materias reprobadas y estado de matrícula."

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| La universidad registra el reporte si su convenio incluye intercambio de datos | Regla | 1 | CUBIERTA |
| El enlace certifica solo a becarios de su propia universidad | Regla | 3 | CUBIERTA |
| Mismos campos que el reporte del becario (promedio, materias, estado de matrícula) | Regla | 1, 6 | CUBIERTA |
| El reporte indica que su origen es la universidad | Regla | 1 | CUBIERTA |
| Fecha límite y alerta de entrega tardía de HU-24.1 también aplican aquí | Regla | 4 | CUBIERTA |
| Fecha límite en UTC-4, con el día de cierre hasta las 23:59 | Regla | — | DELEGADA (HU-24.1, Escenarios 5 y 8) |
| El registro manual del enlace coexiste con el envío por API de la universidad | Regla | — | DELEGADA (HU-24.3) |
| Acceso del enlace con verificación en dos pasos | Técnica | — | DELEGADA (RF-37, Área 7) |
| Ningún enlace puede editar ni eliminar el reporte ya registrado, que conserva su autor | Técnica | 5 | CUBIERTA |
| Autorización por universidad aplicada en la consulta de datos | Técnica | 3 (observable) | CUBIERTA / DELEGADA (RF-37, Área 7) |
| FA-1: Convenio sin intercambio de datos | FA | 2 | CUBIERTA |
| FA-2: Datos incompletos | FA | 6 | CUBIERTA |
| FA-3: Becario de otra universidad | FA | 3 | CUBIERTA |
| FA-4: Reporte ya registrado (no editable) | FA | 5 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-24.2).*

### HU-24.3 Recibir por API el reporte académico que envía la universidad

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-24 | Debe tener | HU-14.1, HU-24.1, HU-38.1 | — (historia nueva) |

#### Narrativa

- Como universidad aliada con convenio de intercambio de datos académicos, a través de mi sistema académico
- Quiero enviar por API los reportes académicos de los becarios de la Fundación matriculados en mi universidad
- Para que su promedio e información académica lleguen a la plataforma sin que nadie los transcriba a mano

#### Reglas de negocio

- El promedio y la información académica pueden llegar por API desde las universidades; el cliente confirma que esto amplía el alcance del PRD (que dejaba fuera la integración automática) y que es imprescindible.
- El envío por API solo se habilita para una universidad cuyo convenio incluya "intercambio de datos académicos" (RF-14; ver HU-14.1).
- La API solo acepta reportes de becarios matriculados en la universidad que envía los datos; el aislamiento entre universidades es el mismo de HU-24.2 (RNF de protección de datos personales).
- El reporte recibido contiene los mismos datos que el del becario: promedio, materias aprobadas, materias reprobadas y estado de matrícula (RF-24; ver HU-24.1).
- El reporte queda ligado a un periodo académico registrado (RF-38; ver HU-38.1).
- El reporte registrado indica que su origen es la universidad y que llegó por la vía de integración, de modo que se distinga del que registra el enlace a mano (RF-24; RNF de trazabilidad).
- La fecha límite del periodo y la alerta de entrega tardía de HU-24.1 también aplican a los reportes recibidos por API, evaluadas en UTC-4 con el día de cierre incluido hasta las 23:59.
- Esta vía no sustituye al registro manual del enlace (HU-24.2) ni a la entrega del becario (HU-24.1): las tres coexisten.
- El reporte recibido queda disponible para evaluar el cumplimiento de las condiciones de permanencia (RF-25; ver HU-25.1).

#### Flujo de la HU

1. La Fundación habilita el envío por API para una universidad cuyo convenio incluye intercambio de datos académicos (HU-14.1).
2. El sistema académico de la universidad envía uno o varios reportes académicos, cada uno identificando al becario y el periodo académico.
3. El sistema verifica que el convenio habilite esta vía, que cada becario pertenezca a esa universidad, que el periodo esté registrado (HU-38.1) y que estén todos los campos del reporte.
4. El sistema registra cada reporte válido con su origen (la universidad, por la vía de integración) y su fecha de recepción.
5. El sistema compara la fecha de recepción con la fecha límite del periodo (UTC-4, día de cierre hasta las 23:59) y, si es posterior, genera la alerta de entrega tardía para el coordinador del programa (HU-24.1).
6. El sistema deja cada reporte disponible para la evaluación de cumplimiento del coordinador (HU-25.1).
7. El sistema devuelve a la universidad el resultado del envío, indicando qué reportes se registraron y qué reportes se rechazaron, con su motivo.

#### Flujos alternos y de error

- FA-1 (paso 3) Convenio sin intercambio de datos: el sistema no registra ningún reporte de esa universidad por esta vía y el envío se rechaza (regla de HU-24.2, FA-1).
- FA-2 (paso 3) Becario de otra universidad: el sistema rechaza ese reporte y no lo registra.
- FA-3 (paso 3) Datos incompletos: el sistema no registra el reporte al que le falta algún campo obligatorio y sí registra los demás del mismo envío.
- FA-4 (paso 3) Periodo no registrado: el sistema rechaza el reporte cuyo periodo académico no está registrado (HU-38.1).
- FA-5 (paso 4) Reporte ya registrado para ese becario y periodo: el registro existente conserva su origen, su autor y su fecha, y el nuevo envío no lo edita ni lo elimina (RNF de trazabilidad; ver HU-35.1). Qué fuente prevalece sigue sin definirse (ver Preguntas abiertas).
- FA-6 (paso 5) Recepción a tiempo: el reporte se registra sin alerta.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-24.3).*

#### Criterios de aceptación (7 escenarios)

##### Escenario 1: Recepción por API con convenio habilitado

- **DADO QUE** la Universidad del Norte tiene un convenio con intercambio de datos académicos y el envío por API habilitado, y Lucía es becaria de esa universidad
- **CUANDO** su sistema académico envía el reporte de Lucía para 2026-2 con promedio, materias aprobadas, materias reprobadas y estado de matrícula
- **ENTONCES** el reporte queda registrado para 2026-2 con origen la Universidad del Norte, indicando que llegó por la vía de integración, y queda disponible para la evaluación de cumplimiento

##### Escenario 2: Convenio sin intercambio de datos

- **DADO QUE** el convenio con la Universidad del Sur no incluye intercambio de datos académicos
- **CUANDO** su sistema académico envía un reporte por API
- **ENTONCES** el sistema rechaza el envío, no registra ningún reporte y devuelve el motivo del rechazo

##### Escenario 3: Becario de otra universidad

- **DADO QUE** Pedro es becario de la Universidad del Sur
- **CUANDO** la Universidad del Norte envía por API un reporte de Pedro
- **ENTONCES** el sistema rechaza ese reporte, no lo registra y devuelve el motivo del rechazo

##### Escenario 4: Envío con un reporte incompleto entre varios válidos

- **DADO QUE** la Universidad del Norte envía en un mismo lote los reportes de Lucía y de Ana para 2026-2, y el de Ana no indica las materias reprobadas
- **CUANDO** el sistema procesa el envío
- **ENTONCES** registra el reporte de Lucía, no registra el de Ana y devuelve el motivo del rechazo del reporte de Ana

##### Escenario 5: Recepción tardía por API

- **DADO QUE** la fecha límite de reportes de 2026-2 es el 20/02/2027
- **CUANDO** la Universidad del Norte envía el reporte de Lucía el 22/02/2027
- **ENTONCES** el reporte se registra y el coordinador ve la alerta de entrega tardía de HU-24.1

##### Escenario 6: Periodo no registrado

- **DADO QUE** el periodo 2027-1 todavía no está registrado (HU-38.1)
- **CUANDO** la Universidad del Norte envía por API un reporte para 2027-1
- **ENTONCES** el sistema rechaza ese reporte y no lo registra

##### Escenario 7: Reenvío del mismo reporte

- **DADO QUE** el reporte de Lucía para 2026-2 ya está registrado con origen la Universidad del Norte y fecha de recepción 15/02/2027
- **CUANDO** la universidad envía de nuevo un reporte de Lucía para 2026-2
- **ENTONCES** el registro existente conserva su origen y su fecha de recepción, y no existe ninguna opción de editarlo ni eliminarlo

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| El promedio y la información académica pueden llegar por API desde la universidad | Regla | 1 | CUBIERTA |
| Solo con convenio que incluya intercambio de datos académicos | Regla / FA-1 | 2 | CUBIERTA |
| Solo becarios de la universidad que envía | Regla / FA-2 | 3 | CUBIERTA |
| Mismos campos que el reporte del becario | Regla / FA-3 | 1, 4 | CUBIERTA |
| Reporte ligado a un periodo académico registrado | Regla / FA-4 | 1, 6 | CUBIERTA |
| El reporte indica su origen y la vía por la que llegó | Regla | 1 | CUBIERTA |
| Fecha límite y alerta de entrega tardía (UTC-4, día de cierre hasta las 23:59) | Regla | 5 | CUBIERTA |
| Recepción a tiempo, sin alerta | FA-6 | 1 | CUBIERTA |
| Append-only: un reenvío no edita ni elimina el reporte registrado | Técnica / FA-5 | 7 | CUBIERTA |
| El reporte recibido alimenta la evaluación de cumplimiento | Regla | 1 (parcial) | DELEGADA (HU-25.1) |
| Coexistencia de las tres vías de entrada | Regla | — | DELEGADA (HU-24.1, HU-24.2) |
| Autenticación y autorización de la universidad emisora | Técnica | 3 (aislamiento observable) | CUBIERTA / DELEGADA (RF-37, Área 7) |
| Procesamiento asíncrono, paginado e idempotente del área | Técnica (RNF) | — | NO PROBABLE como criterio funcional (diseño) |
| Prevalencia entre reportes de fuentes distintas para el mismo periodo | Sin definir | — | NO PROBABLE |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-24.3).*

## RF-25 — Evaluación de cumplimiento de condiciones de permanencia · *Debe tener*

### HU-25.1 Definir la política de consecuencias y evaluar el cumplimiento

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-25 | Debe tener | HU-11.2, HU-24.1 | HU-25.1, HU-25.2 |

#### Narrativa

- Como coordinador de programa
- Quiero registrar qué consecuencias contempla la política de mi programa y evaluar, al recibir cada reporte académico, si el becario sigue cumpliendo las condiciones de permanencia
- Para decidir si el becario conserva la beca en las condiciones pactadas y que los incumplimientos se traten según la política del programa

#### Reglas de negocio

- Tras recibir el reporte académico se evalúa si el becario sigue cumpliendo las condiciones para conservar la beca (RF-25).
- La evaluación se hace sobre el reporte, sin importar por cuál de las tres vías llegó: entrega del becario (HU-24.1), registro del enlace (HU-24.2) o envío por API de la universidad (HU-24.3).
- Las condiciones son las de permanencia definidas para el programa e informadas en la oferta (RF-11, RF-15; ver HU-11.2).
- Las consecuencias posibles son llamado de atención, suspensión o revocación (RF-25), y se aplican según la política del programa (RF-25).
- El resultado de la evaluación es insumo de la renovación (RF-26; ver HU-26.2).
- El incumplimiento de una condición de permanencia es incumplimiento grave, por lo que habilita la revocación directa; la consecuencia concreta la aplica HU-25.2.
- La evaluación queda registrada de forma permanente con autor y fecha (RNF de trazabilidad; RF-35, ver HU-35.1).
- Cada coordinador define y evalúa solo en los programas de los que es responsable (RF-37; RNF de confidencialidad).
- La información socioeconómica y académica del becario solo es visible para quien la necesita para evaluar o decidir (RNF de confidencialidad).
- **Sin definir en el PRD:** cómo está estructurada la política de consecuencias (por condición, por gravedad o por reincidencia). Esta HU solo registra qué consecuencias contempla el programa y la descripción de su política.

#### Flujo de la HU

1. El coordinador abre la configuración de uno de sus programas y elige "Política de consecuencias"; el sistema muestra las condiciones de permanencia del programa (HU-11.2).
2. Marca qué consecuencias contempla la política del programa (llamado de atención, suspensión, revocación), escribe la descripción de la política y la guarda.
3. El sistema registra la política y la usa al aplicar consecuencias (HU-25.2).
4. Llega un reporte académico (HU-24.1, HU-24.2 o HU-24.3) y el sistema lo pone en la lista de pendientes de evaluación del coordinador del programa.
5. El coordinador abre el reporte; el sistema lo muestra junto a las condiciones de permanencia del programa.
6. Marca cada condición como cumplida o no cumplida.
7. El sistema registra el resultado con autor y fecha.
8. Si alguna condición no se cumple, el sistema lo deja listo para aplicar la consecuencia (HU-25.2).

#### Flujos alternos y de error

- FA-1 (paso 2) Ninguna consecuencia marcada: el sistema no guarda y muestra "Marca al menos una consecuencia de la política del programa."
- FA-2 (paso 1) Programa sin condiciones de permanencia: el sistema muestra "Este programa no tiene condiciones de permanencia definidas." y remite a HU-11.2.
- FA-3 (paso 6) Condición sin marcar: el sistema no registra la evaluación y muestra "Indica si se cumple cada condición de permanencia."
- FA-4 (pasos 1 y 5) Programa ajeno: el sistema no muestra ni permite modificar la política ni las evaluaciones de un programa del que el coordinador no es responsable.
- FA-5 (paso 7) Evaluación concurrente: si el reporte ya recibió una evaluación mientras esta estaba en curso, el sistema conserva una sola evaluación válida y rechaza el segundo intento.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-25.1).*

#### Criterios de aceptación (11 escenarios)

##### Escenario 1: Política registrada

- **DADO QUE** "Becas Ingeniería" tiene condiciones de permanencia definidas
- **CUANDO** el coordinador marca "Llamado de atención" y "Suspensión", escribe la descripción de la política y guarda
- **ENTONCES** la política queda registrada para el programa con esas dos consecuencias

##### Escenario 2: Ninguna consecuencia marcada

- **DADO QUE** el coordinador está definiendo la política de "Becas Ingeniería"
- **CUANDO** guarda sin marcar ninguna consecuencia
- **ENTONCES** el sistema no guarda y muestra "Marca al menos una consecuencia de la política del programa."

##### Escenario 3: Becario que cumple

- **DADO QUE** Pedro entregó su reporte de 2026-2 con promedio 16,5 y la condición es "Promedio mínimo 14"
- **CUANDO** el coordinador marca la condición como cumplida
- **ENTONCES** la evaluación queda registrada como "Cumple" y no se abre ninguna consecuencia

##### Escenario 4: Becario que no cumple

- **DADO QUE** Lucía entregó su reporte de 2026-2 con promedio 12
- **CUANDO** el coordinador marca "Promedio mínimo 14" como no cumplida
- **ENTONCES** la evaluación queda registrada como "No cumple" y queda pendiente aplicar la consecuencia

##### Escenario 5: Evaluación incompleta

- **DADO QUE** el programa tiene dos condiciones de permanencia
- **CUANDO** el coordinador marca solo una y guarda
- **ENTONCES** el sistema no registra la evaluación y muestra "Indica si se cumple cada condición de permanencia."

##### Escenario 6: Sin reportes pendientes

- **DADO QUE** no hay reportes académicos sin evaluar en "Becas Ingeniería"
- **CUANDO** el coordinador abre la lista de pendientes de evaluación
- **ENTONCES** el sistema muestra "No hay reportes académicos pendientes de evaluar."

##### Escenario 7: Coordinador de otro programa

- **DADO QUE** Carla es coordinadora de "Becas Salud" y no de "Becas Ingeniería"
- **CUANDO** intenta abrir la política de consecuencias de "Becas Ingeniería"
- **ENTONCES** el sistema no le permite verla ni modificarla

##### Escenario 8: Evaluación concurrente del mismo reporte

- **DADO QUE** el reporte académico de 2026-2 de Pedro está pendiente de evaluación
- **CUANDO** dos sesiones intentan registrar evaluaciones distintas de ese mismo reporte casi al mismo tiempo
- **ENTONCES** el sistema registra una sola evaluación válida para ese reporte y rechaza el segundo intento

##### Escenario 9: Programa sin condiciones de permanencia

- **DADO QUE** "Becas Ingeniería" no tiene condiciones de permanencia definidas
- **CUANDO** el coordinador abre "Política de consecuencias" de ese programa
- **ENTONCES** el sistema muestra "Este programa no tiene condiciones de permanencia definidas." y remite a la configuración del programa (HU-11.2)

##### Escenario 10: Evaluaciones pendientes de un programa ajeno

- **DADO QUE** Carla es coordinadora de "Becas Salud" y no de "Becas Ingeniería"
- **CUANDO** intenta abrir la lista de evaluaciones pendientes de "Becas Ingeniería"
- **ENTONCES** el sistema no se la muestra y Carla no ve los reportes académicos ni las condiciones de permanencia de ese programa

##### Escenario 11: Evaluación de un reporte recibido por API

- **DADO QUE** el reporte de 2026-2 de Lucía llegó por API desde la Universidad del Norte (HU-24.3) y "Becas Ingeniería" tiene la condición "Promedio mínimo 14"
- **CUANDO** el coordinador abre la lista de pendientes de evaluación de su programa
- **ENTONCES** el reporte de Lucía aparece en la lista y puede evaluarse igual que uno entregado por el becario

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Tras recibir el reporte se evalúa si el becario cumple las condiciones | Regla | 3, 4 | CUBIERTA |
| La evaluación es igual para las tres vías de entrada del reporte | Regla | 11 | CUBIERTA |
| Las condiciones son las de permanencia definidas en el programa | Regla | 3, 4 | CUBIERTA (fuente delegada a HU-11.2) |
| Consecuencias posibles según la política del programa (llamado, suspensión, revocación) | Regla | 1 | CUBIERTA |
| Lista de reportes pendientes de evaluación del coordinador (paso 4 del flujo) | Flujo | 6, 11 | CUBIERTA |
| El resultado de la evaluación es insumo de la renovación | Regla | — | DELEGADA (HU-26.2, Escenarios 1 y 2) |
| El incumplimiento de una condición de permanencia es incumplimiento grave | Regla | — | DELEGADA (HU-25.2, Escenario 12) |
| Evaluación registrada permanentemente con autor y fecha | Técnica | — | DELEGADA (HU-35.1) |
| Cada coordinador define y evalúa solo en sus propios programas (política) | Regla / FA-4 | 7 | CUBIERTA |
| Cada coordinador define y evalúa solo en sus propios programas (evaluaciones pendientes) | Regla / FA-4 | 10 | CUBIERTA |
| Info socioeconómica y académica visible solo para quien la necesita para decidir | Regla (RNF) | 10 | CUBIERTA |
| Consistencia bajo concurrencia al registrar la evaluación | Técnica / FA-5 | 8 | CUBIERTA |
| FA-1: Ninguna consecuencia marcada | FA | 2 | CUBIERTA |
| FA-2: Programa sin condiciones de permanencia | FA | 9 | CUBIERTA |
| FA-3: Condición sin marcar | FA | 5 | CUBIERTA |
| Estructura de la política de consecuencias (por condición, gravedad o reincidencia) | Sin definir en el PRD | — | NO PROBABLE |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-25.1).*

### HU-25.2 Aplicar la consecuencia por incumplimiento e informarla al becario

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-25 | Debe tener | HU-25.1 | HU-25.3, HU-25.4 |

#### Narrativa

- Como coordinador de programa
- Quiero aplicar la consecuencia que fija la política del programa cuando un becario no cumple una condición, y que el becario vea con claridad qué condición no cumplió y cuál es el siguiente paso
- Para tratar los incumplimientos de forma consistente y documentada, y que el becario sepa qué hacer para conservar su beca

#### Reglas de negocio

- Si el becario no cumple las condiciones, se activa una consecuencia según la política del programa: llamado de atención, suspensión o revocación (RF-25; ver HU-25.1).
- El sistema solo ofrece las consecuencias que la política vigente del programa contempla (RF-25; ver HU-25.1).
- La suspensión sigue las reglas de HU-21.1: *Activa* → *Suspendida*, con motivo y con fecha de revisión o marca de indefinida (RF-21).
- La revocación sigue las reglas de HU-22.1: motivo documentado, código de verificación de un solo uso (OTP) y transiciones permitidas (RF-22; Restricciones Técnicas del Cliente). Esta HU no redefine esas reglas, las reutiliza.
- El incumplimiento de una condición de permanencia cuenta como **incumplimiento grave**: si la política del programa contempla la revocación, el coordinador puede revocar directamente una beca *Activa* sin suspenderla primero (regla transversal de incumplimiento grave; ver HU-22.1).
- El coordinador registra el siguiente paso, y el becario puede ver con claridad qué condición no cumplió y cuál es ese siguiente paso (RF-25).
- Toda decisión queda registrada con quién, cuándo y por qué (RF-35; ver HU-35.1).
- Si la consecuencia es una revocación, la beca no se reactiva; para continuar, la persona debe postular de nuevo (regla transversal), lo que la plataforma permite incluso después de una decisión desfavorable previa.
- El becario ve solo la información de su propia beca (RNF de protección de datos personales).
- La decisión se comunica al becario por el módulo de notificaciones (RF-29; ver HU-29.1).
- La notificación de una suspensión al becario incluye el motivo de la suspensión (ver HU-21.1, HU-29.1).
- Cuando llega la fecha de revisión de una suspensión, el sistema avisa al coordinador del programa (ver HU-21.1, HU-29.1).

#### Flujo de la HU

1. El coordinador abre una evaluación con resultado "No cumple" (HU-25.1).
2. El sistema muestra la condición incumplida y las consecuencias que contempla la política vigente del programa (HU-25.1).
3. El coordinador escribe el siguiente paso para el becario.
4. Elige y aplica una de las consecuencias que contempla la política: llamado de atención, suspensión (HU-21.1) o revocación (HU-22.1, con código de un solo uso). La revocación puede aplicarse directamente sobre una beca *Activa*, porque el incumplimiento de una condición de permanencia es incumplimiento grave.
5. El sistema registra la consecuencia en la beca y la decisión con autor, fecha y motivo (HU-35.1), la comunica al becario (HU-29.1) —incluyendo el motivo cuando la consecuencia es una suspensión— y la deja visible en su beca.
6. El becario entra al detalle de su beca.
7. El sistema le muestra la condición incumplida, el periodo del reporte, la consecuencia aplicada y el siguiente paso.
8. Si la consecuencia fue una suspensión con fecha de revisión, al llegar esa fecha el sistema avisa al coordinador del programa (HU-21.1, HU-29.1).

#### Flujos alternos y de error

- FA-1 (paso 3) Sin siguiente paso: el sistema no aplica la consecuencia y muestra "Indica el siguiente paso para el becario."
- FA-2 (paso 4) Revocación sin código válido: la beca no cambia de estado (HU-22.1). El código vence a los 15 minutos y, tras 3 intentos fallidos, el sistema no acepta nuevos intentos durante 1 hora.
- FA-3 (paso 4) Consecuencia no contemplada: el sistema no ofrece las consecuencias que la política vigente del programa no contempla, incluso si las contemplaba cuando se registró la evaluación.
- FA-4 (paso 7) Sin incumplimientos: el sistema muestra que el becario cumple las condiciones de permanencia evaluadas.
- FA-5 (paso 7) Beca revocada: el sistema indica que la beca no puede reactivarse y que, para continuar, debe postular de nuevo.
- FA-6 (paso 4) Transición no permitida: si el estado de la beca cambió y la transición requerida por la consecuencia ya no es válida, el sistema no la aplica y conserva el estado vigente (reglas de transición de HU-21.1 y HU-22.1).

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-25.2).*

#### Criterios de aceptación (12 escenarios)

##### Escenario 1: Llamado de atención

- **DADO QUE** la política de "Becas Ingeniería" contempla "Llamado de atención" y Pedro no cumplió "Entrega del reporte académico"
- **CUANDO** el coordinador aplica el llamado de atención con el siguiente paso "Entregar el reporte a tiempo el próximo periodo"
- **ENTONCES** el llamado de atención queda registrado en la beca, que sigue *Activa*

##### Escenario 2: Suspensión

- **DADO QUE** la política de "Becas Ingeniería" contempla "Suspensión" y la beca *Activa* de Lucía no cumplió "Promedio mínimo 14"
- **CUANDO** el coordinador aplica la suspensión con el siguiente paso "Presentar plan de recuperación académica" y fecha de revisión 15/03/2027
- **ENTONCES** la beca queda *Suspendida* con ese motivo y la decisión queda registrada

##### Escenario 3: Falta el siguiente paso

- **DADO QUE** el coordinador está aplicando la consecuencia a Lucía
- **CUANDO** confirma sin escribir el siguiente paso
- **ENTONCES** el sistema no aplica la consecuencia y muestra "Indica el siguiente paso para el becario."

##### Escenario 4: Condición incumplida visible

- **DADO QUE** a Lucía se le aplicó una suspensión por no cumplir "Promedio mínimo 14" en 2026-2
- **CUANDO** Lucía abre el detalle de su beca
- **ENTONCES** ve "Promedio mínimo 14", el periodo 2026-2, la consecuencia "Suspensión" y el siguiente paso "Presentar plan de recuperación académica"

##### Escenario 5: Becario que cumple

- **DADO QUE** la evaluación de Pedro en 2026-2 fue "Cumple"
- **CUANDO** Pedro abre el detalle de su beca
- **ENTONCES** ve que cumple las condiciones de permanencia de 2026-2 y ningún incumplimiento

##### Escenario 6: Solo su propia información

- **DADO QUE** a Lucía se le aplicó una consecuencia
- **CUANDO** Pedro, becario del mismo programa, consulta su beca
- **ENTONCES** no ve información del incumplimiento de Lucía

##### Escenario 7: Revocación con código de un solo uso incorrecto

- **DADO QUE** el coordinador está aplicando una revocación a la beca *Suspendida* de Lucía
- **CUANDO** ingresa un código de verificación de un solo uso incorrecto
- **ENTONCES** la beca sigue *Suspendida* y el sistema muestra "El código no es válido. Solicita uno nuevo." (mensaje definido en HU-22.1)

##### Escenario 8: Consecuencia retirada de la política antes de aplicarla

- **DADO QUE** la evaluación "No cumple" de Lucía quedó pendiente de aplicar consecuencia y la política de "Becas Ingeniería" contemplaba "Revocación"
- **CUANDO** el coordinador quita "Revocación" de la política antes de resolver el caso de Lucía
- **ENTONCES** el sistema ya no ofrece "Revocación" como consecuencia disponible para el caso de Lucía

##### Escenario 9: Detalle de una beca revocada

- **DADO QUE** la beca de Lucía fue revocada como consecuencia de un incumplimiento
- **CUANDO** Lucía abre el detalle de su beca
- **ENTONCES** el sistema indica que la beca no puede reactivarse y que, para continuar, debe postular de nuevo

##### Escenario 10: Transición ya no válida al aplicar la consecuencia

- **DADO QUE** la evaluación "No cumple" de Lucía está pendiente de aplicar la consecuencia de suspensión y, mientras el coordinador decide, la beca de Lucía queda *Revocada* por otro motivo
- **CUANDO** el coordinador intenta aplicar la suspensión
- **ENTONCES** el sistema no la aplica y la beca conserva el estado *Revocada*, porque la transición a *Suspendida* solo parte de *Activa*

##### Escenario 11: Revocación directa de una beca *Activa* por incumplimiento de permanencia

- **DADO QUE** la política de "Becas Ingeniería" contempla "Revocación" y la beca *Activa* de Lucía no cumplió "Promedio mínimo 14"
- **CUANDO** el coordinador aplica la revocación con el motivo documentado y un código de un solo uso válido, sin suspender antes la beca
- **ENTONCES** la beca queda *Revocada* y la decisión queda registrada, porque el incumplimiento de una condición de permanencia es incumplimiento grave

##### Escenario 12: Revocación con código de un solo uso vencido

- **DADO QUE** el coordinador está aplicando una revocación a la beca *Activa* de Lucía y el código de un solo uso se le envió hace 20 minutos
- **CUANDO** ingresa ese código
- **ENTONCES** la beca sigue *Activa* y la revocación no se aplica, porque el código vence a los 15 minutos de enviado

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Consecuencia activada según la política del programa (llamado, suspensión, revocación) | Regla | 1, 2, 11 | CUBIERTA |
| Solo se ofrecen las consecuencias que la política vigente contempla | Regla | 8 | CUBIERTA |
| La suspensión sigue las reglas de HU-21.1 (motivo, fecha de revisión) | Regla | 2 (integración) | DELEGADA (HU-21.1) |
| La revocación sigue las reglas de HU-22.1 (motivo, OTP, transiciones) | Regla | 7, 10, 11, 12 (integración) | DELEGADA (HU-22.1) |
| El incumplimiento de permanencia es grave: revocación directa desde *Activa* | Regla | 11 | CUBIERTA |
| Hasta que el código de un solo uso se valide, la beca no cambia de estado | Técnica / FA-2 | 7, 12 | CUBIERTA |
| Vigencia del código de un solo uso: 15 minutos | Regla | 12 | CUBIERTA |
| 3 intentos fallidos y después espera de 1 hora | Regla | — | DELEGADA (HU-22.1, dueña del servicio OTP) |
| Canal del código elegido por el usuario (correo o SMS) | Regla | — | DELEGADA (HU-22.1, Área 7) |
| El coordinador registra el siguiente paso y el becario lo ve junto a la condición incumplida | Regla | 4 | CUBIERTA |
| Toda decisión registrada con quién, cuándo y por qué | Técnica | — | DELEGADA (HU-35.1) |
| Si la consecuencia es revocación, la beca no se reactiva; debe postularse de nuevo | Regla / FA-5 | 9 | CUBIERTA |
| El becario ve solo la información de su propia beca | Regla | 6 | CUBIERTA |
| La decisión se comunica por el módulo de notificaciones | Regla | — | DELEGADA (HU-29.1) |
| La notificación de la suspensión incluye el motivo | Regla | — | DELEGADA (HU-29.1, HU-21.1) |
| Aviso al coordinador al llegar la fecha de revisión de la suspensión | Regla | — | DELEGADA (HU-21.1, HU-29.1) |
| FA-1: Sin siguiente paso | FA | 3 | CUBIERTA |
| FA-3: Consecuencia retirada de la política antes de aplicarla | FA | 8 | CUBIERTA |
| FA-4: Sin incumplimientos (el becario cumple) | FA | 5 | CUBIERTA |
| FA-6: Transición ya no permitida porque el estado de la beca cambió | FA | 10 | CUBIERTA |
| Qué parte de la justificación interna ve el becario | Sin definir en el PRD | — | NO PROBABLE |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-25.2).*

## RF-26 — Solicitud y evaluación de renovación · *Debe tener*

### HU-26.1 Definir la ventana de renovación y solicitar la renovación dentro de ella

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-26 | Debe tener | HU-11.2, HU-38.1 | HU-26.1, HU-26.2 |

#### Narrativa

- Como becario
- Quiero solicitar la renovación de mi beca dentro de la ventana que define el coordinador antes de cada nuevo periodo académico
- Para seguir recibiendo la beca el siguiente periodo, sabiendo cuándo debo pedirla

#### Reglas de negocio

- La renovación es una solicitud de un becario activo para continuar recibiendo la beca el siguiente periodo; no es automática y existe una ventana de tiempo para solicitarla (RF-26; Glosario).
- La ventana se abre antes de cada nuevo periodo académico (RF-26) y se liga a ese periodo (RF-38; ver HU-38.1).
- Solo se puede solicitar la renovación dentro de la ventana definida para el programa del becario (RF-26).
- La ventana se evalúa en la zona horaria **UTC-4**, y el día de cierre se incluye **hasta las 23:59**.
- Las fechas de cada periodo académico las define cada universidad, y se registra la fecha de inicio de inscripciones por universidad y periodo (ver HU-38.1, HU-14.x). La ventana de renovación, en cambio, la define el programa para todos sus becarios.
- Las condiciones de renovación son las del programa (RF-11; ver HU-11.2).
- El monto de una beca es un monto fijo por periodo; en una renovación, ese monto se asigna a cada estudiante y puede ser mayor al del periodo anterior. Ya no existen coberturas total ni parcial como forma de calcular el monto.
- Solo una beca *Activa* puede pasar a *Renovada* (regla transversal), por lo que solo con una beca *Activa* se puede solicitar la renovación.
- Una solicitud pendiente compromete presupuesto que cuenta como no disponible, no solo lo ya desembolsado (RF-12; ver HU-12.2). Ese compromiso se mantiene hasta que la solicitud se aprueba o se rechaza definitivamente.
- La solicitud queda registrada con su fecha (RNF de trazabilidad; ver HU-35.1).

#### Flujo de la HU

1. El coordinador abre su programa y elige "Ventana de renovación".
2. Selecciona el periodo académico al que se renueva (HU-38.1).
3. Indica la fecha de apertura y la fecha de cierre de la ventana, y guarda.
4. El sistema registra la ventana y la muestra a los becarios de ese programa.
5. El becario entra a su beca, ve la ventana de renovación vigente de su programa y elige "Solicitar renovación".
6. El sistema verifica que la ventana esté abierta —en UTC-4, con el día de cierre incluido hasta las 23:59— y que la beca esté *Activa*.
7. El sistema registra la solicitud como pendiente, con su fecha, y compromete el presupuesto correspondiente (HU-12.2), que sigue comprometido hasta que la solicitud se resuelva.
8. El coordinador la recibe para evaluarla (HU-26.2).

#### Flujos alternos y de error

- FA-1 (paso 3) Fechas incoherentes: si la fecha de cierre es anterior a la de apertura, el sistema no guarda y muestra "La fecha de cierre debe ser posterior a la de apertura."
- FA-2 (paso 2) Ventana ya definida: el sistema muestra la ventana existente para ese programa y periodo.
- FA-3 (paso 6) Fuera de la ventana: el sistema no registra la solicitud y muestra "La ventana de renovación para 2027-1 no está abierta." Se considera dentro de la ventana toda solicitud hecha hasta las 23:59 (UTC-4) del día de cierre.
- FA-4 (paso 6) Beca no activa: el sistema no registra la solicitud y muestra "Solo puedes solicitar la renovación con una beca activa."
- FA-5 (paso 5) Programa ajeno: el sistema solo ofrece al becario la ventana de renovación del programa de su beca.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-26.1).*

#### Criterios de aceptación (11 escenarios)

##### Escenario 1: Ventana definida

- **DADO QUE** el periodo 2027-1 está registrado
- **CUANDO** el coordinador de "Becas Ingeniería" define la ventana del 01/12/2026 al 15/12/2026 para 2027-1
- **ENTONCES** la ventana queda registrada y los becarios del programa la ven

##### Escenario 2: Fechas incoherentes

- **DADO QUE** el coordinador está definiendo la ventana para 2027-1
- **CUANDO** indica apertura el 15/12/2026 y cierre el 01/12/2026
- **ENTONCES** el sistema no guarda y muestra "La fecha de cierre debe ser posterior a la de apertura."

##### Escenario 3: Programa sin ventana

- **DADO QUE** "Becas Salud" no tiene ventana de renovación para 2027-1
- **CUANDO** un becario de "Becas Salud" consulta la renovación
- **ENTONCES** el sistema indica que aún no hay una ventana de renovación definida

##### Escenario 4: Solicitud dentro de la ventana

- **DADO QUE** la ventana de "Becas Ingeniería" para 2027-1 va del 01/12/2026 al 15/12/2026 y la beca de Pedro está *Activa*
- **CUANDO** Pedro solicita la renovación el 05/12/2026
- **ENTONCES** la solicitud queda registrada como pendiente

##### Escenario 5: Fuera de la ventana

- **DADO QUE** la ventana para 2027-1 cerró el 15/12/2026
- **CUANDO** Pedro intenta solicitar la renovación el 18/12/2026
- **ENTONCES** el sistema no registra la solicitud y muestra "La ventana de renovación para 2027-1 no está abierta."

##### Escenario 6: Beca suspendida

- **DADO QUE** la beca de Lucía está *Suspendida* y la ventana está abierta
- **CUANDO** Lucía intenta solicitar la renovación
- **ENTONCES** el sistema no registra la solicitud y muestra "Solo puedes solicitar la renovación con una beca activa."

##### Escenario 7: Solicitud en la última hora del día de cierre

- **DADO QUE** la ventana de "Becas Ingeniería" para 2027-1 cierra el 15/12/2026 y la beca de Pedro está *Activa*
- **CUANDO** Pedro solicita la renovación el 15/12/2026 a las 23:59 (UTC-4)
- **ENTONCES** la solicitud queda registrada como pendiente, porque el día de cierre se incluye hasta las 23:59

##### Escenario 8: Solicitud antes de la apertura

- **DADO QUE** la ventana de "Becas Ingeniería" para 2027-1 abre el 01/12/2026
- **CUANDO** Pedro intenta solicitar la renovación el 28/11/2026
- **ENTONCES** el sistema no registra la solicitud y muestra "La ventana de renovación para 2027-1 no está abierta."

##### Escenario 9: Becario de otro programa

- **DADO QUE** Lucía es becaria de "Becas Salud" y la ventana abierta es la de "Becas Ingeniería"
- **CUANDO** Lucía abre la renovación de su beca
- **ENTONCES** el sistema no le ofrece la ventana de "Becas Ingeniería" y no registra ninguna solicitud de renovación en ese programa

##### Escenario 10: Ventana ya definida para el programa y el periodo

- **DADO QUE** "Becas Ingeniería" ya tiene definida la ventana de renovación del 01/12/2026 al 15/12/2026 para 2027-1
- **CUANDO** el coordinador vuelve a abrir "Ventana de renovación" para ese programa y ese periodo
- **ENTONCES** el sistema le muestra la ventana ya definida (01/12/2026 al 15/12/2026) en vez de pedir una nueva

##### Escenario 11: Solicitud pasada la medianoche del día de cierre

- **DADO QUE** la ventana de "Becas Ingeniería" para 2027-1 cierra el 15/12/2026 y la beca de Pedro está *Activa*
- **CUANDO** Pedro intenta solicitar la renovación el 16/12/2026 a las 00:05 (UTC-4)
- **ENTONCES** el sistema no registra la solicitud y muestra "La ventana de renovación para 2027-1 no está abierta.", porque la solicitud es posterior a las 23:59 del día de cierre

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| La ventana se abre antes del nuevo periodo y queda ligada a ese periodo registrado | Regla | 1 | CUBIERTA |
| Solo se puede solicitar la renovación dentro de la ventana del propio programa | Regla | 4, 5, 7, 8, 9, 11 | CUBIERTA |
| Fronteras de la ventana en UTC-4 (hasta las 23:59 del día de cierre) | Regla (frontera) | 7, 8, 11 | CUBIERTA |
| Fechas de los periodos definidas por cada universidad, con su inicio de inscripciones registrado | Regla | — | DELEGADA (HU-38.1, HU-14.x) |
| Las condiciones de renovación son las del programa | Regla | — | DELEGADA (HU-11.2) |
| Monto fijo por periodo, asignado a cada estudiante en la renovación | Regla | — | DELEGADA (HU-26.2, Escenario 15) |
| Solo una beca *Activa* puede solicitar la renovación | Regla / FA-4 | 6 | CUBIERTA |
| Una solicitud pendiente compromete presupuesto hasta que se resuelve | Regla | — | DELEGADA (HU-12.2; liberación en HU-26.2, Escenario 13) |
| La solicitud queda registrada con su fecha | Técnica | 4 (parcial) | DELEGADA (HU-35.1) |
| Ventana no definida todavía para el programa (consulta del becario) | Flujo | 3 | CUBIERTA |
| FA-1: Fechas incoherentes (cierre antes que apertura) | FA | 2 | CUBIERTA |
| FA-2: Ventana ya definida (muestra la existente) | FA | 10 | CUBIERTA |
| FA-3: Fuera de la ventana | FA | 5, 8, 11 | CUBIERTA |
| FA-5: Programa ajeno | FA | 9 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-26.1).*

### HU-26.2 Evaluar la renovación y resolver las becas no renovadas

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-26 | Debe tener | HU-12.1, HU-25.1, HU-26.1 | HU-26.3, HU-26.4 |

#### Narrativa

- Como coordinador de programa
- Quiero aprobar o rechazar las solicitudes de renovación según el cumplimiento académico y el presupuesto disponible, y resolver las becas de quienes no la solicitaron dentro de la ventana
- Para renovar solo las becas que cumplen y que el programa puede financiar, sin que ningún caso quede abierto por olvido

#### Reglas de negocio

- La aprobación depende del cumplimiento académico (RF-25; ver HU-25.1) y de que el programa aún tenga presupuesto disponible (RF-26; ver HU-12.1).
- El monto de la renovación es un **monto fijo para el periodo que se renueva** y se asigna a cada estudiante; puede ser mayor al del periodo anterior. Ya no se calcula como cobertura total ni parcial. El presupuesto disponible se verifica contra ese monto asignado.
- Al aprobar, la beca pasa de *Activa* a *Renovada* (regla transversal); solo una beca *Activa* admite esa transición.
- Una beca *Renovada* vuelve a *Activa* al iniciar el nuevo periodo (S4).
- Una beca **no renovada queda *Inactiva*** y no recibe dinero en el siguiente periodo. Esto aplica tanto a la beca cuya renovación se rechaza como a la del becario que no la solicitó dentro de la ventana, salvo excepción definida por el programa (RF-26).
- Si la renovación se rechaza, la beca sigue *Activa* durante el periodo en curso y queda *Inactiva* cuando se marca como actual el siguiente periodo (S11; ver HU-38.2).
- Pasar a un nuevo periodo no interrumpe las renovaciones del periodo anterior que sigan en curso (RF-38; ver HU-38.2).
- El monto de una renovación pendiente ya cuenta como no disponible (RF-12; ver HU-12.2); al verificar el presupuesto no se descuenta dos veces. Al rechazarse definitivamente la solicitud, ese monto vuelve a estar disponible.
- La decisión de renovación y el paso a *Inactiva* quedan justificados y registrados con quién, cuándo y por qué (regla transversal; RF-35, ver HU-35.1).
- Una beca *Concluida* o *Revocada* no se reactiva; para continuar, la persona postula de nuevo (regla transversal), lo que la plataforma permite tras un rechazo previo.
- *Inactiva* **no es un estado final**: desde *Inactiva* la beca puede volver a *Activa* o pasar a *Concluida*, que sigue siendo un estado final, o puede ser revocada (*Inactiva* → *Revocada*). El paso de *Inactiva* a *Activa* o a *Concluida* lo decide el **coordinador de programa**, cuando el becario **sube los datos que le falten**; con eso el coordinador resuelve cuál de los dos pasos aplica. **Confirmado**. Esta HU no fija el flujo de esa reactivación, que no le es propio (ver Preguntas abiertas). Una beca *Inactiva* **no cuenta como beca activa** para la regla de no doble beneficio (C2; ver HU-01.2, HU-16.1, HU-18.1).
- Una beca que queda *Inactiva* **no libera** su presupuesto ni su cupo del programa: los **mantiene ocupados** mientras esté *Inactiva*, porque puede volver a *Activa* (RF-12; ver HU-12.1). **Confirmado**. El cupo que mantiene ocupado es el del programa —becarios totales—, no el de una convocatoria, que cuenta solo los nuevos del ciclo.
- Cada coordinador resuelve solo las renovaciones de los programas de los que es responsable (RF-37).
- La decisión se comunica al becario por el módulo de notificaciones (RF-29; ver HU-29.1).
- **Sin definir en el PRD:** en qué consiste la "excepción definida por el programa" (dónde se configura, qué casos cubre y qué pasa después con la beca exceptuada), y en qué momento la beca de quien no solicitó la renovación queda *Inactiva*. Por eso esta HU no fija un disparador automático: el cambio lo confirma el coordinador caso por caso y la beca exceptuada solo queda registrada como tal.

#### Flujo de la HU

1. El coordinador abre una solicitud de renovación pendiente de uno de sus programas (HU-26.1).
2. El sistema muestra el resultado de la última evaluación de cumplimiento (HU-25.1) y el presupuesto disponible del programa (HU-12.1).
3. El coordinador aprueba o rechaza, indica el monto asignado al estudiante para el periodo que se renueva cuando aprueba, y escribe la justificación.
4. Si aprueba, el sistema verifica que la beca siga *Activa* y que haya presupuesto disponible para ese monto, y la cambia a *Renovada*. Si rechaza, libera el presupuesto comprometido por la solicitud y la beca sigue *Activa* hasta que se marca como actual el siguiente periodo, momento en el que queda *Inactiva* (S11, HU-38.2).
5. El sistema registra la decisión (HU-35.1) y la comunica al becario (HU-29.1).
6. Cuando la ventana de renovación del programa cierra (HU-26.1), el sistema muestra al coordinador las becas *Activa* del programa sin solicitud de renovación.
7. Para cada beca, el coordinador confirma que la beca no se renueva o registra que aplica la excepción definida por el programa, con justificación.
8. El sistema marca como *Inactiva* las becas confirmadas, registra la decisión con la justificación "No solicitó la renovación dentro de la ventana" y **mantiene ocupados** su presupuesto y su cupo del programa, porque la beca puede volver a *Activa* (HU-12.1). Las becas con excepción siguen *Activa*.

#### Flujos alternos y de error

- FA-1 (paso 3) Sin cumplimiento: si la evaluación es "No cumple" o no existe, el sistema no permite aprobar y muestra "El becario no tiene una evaluación de cumplimiento favorable."
- FA-2 (paso 3) Sin presupuesto: si el programa no tiene presupuesto disponible para el monto asignado a la renovación, sin descontar dos veces lo ya reservado para esta misma solicitud (HU-12.2), el sistema no permite aprobar y muestra "El programa no tiene presupuesto disponible para renovar."
- FA-3 (paso 3) Rechazo: la beca no pasa a *Renovada*, sigue *Activa*, el rechazo queda registrado con autor, fecha y justificación y el presupuesto comprometido se libera. La beca queda *Inactiva* al marcarse como actual el siguiente periodo (S11).
- FA-4 (paso 7) Excepción sin justificación: el sistema no la registra y muestra "Justifica la excepción."
- FA-5 (paso 8) Beca ya *Inactiva*: si la persona intenta después solicitar la renovación de una beca *Inactiva*, el sistema no lo permite porque la beca no está *Activa*. Como *Inactiva* no es un estado final, no aplica la indicación de postular de nuevo (regla transversal reservada a *Concluida* y *Revocada*); el mensaje exacto que se muestra sigue por confirmar. El paso a *Activa* o a *Concluida* lo decide el coordinador de programa cuando el becario sube los datos que le falten; el flujo para hacerlo no es propio de esta HU (ver Preguntas abiertas).
- FA-6 (paso 4) Beca que ya no está *Activa*: si la beca fue suspendida o revocada mientras la solicitud estaba pendiente, el sistema no permite aprobar la renovación porque la transición a *Renovada* solo parte de *Activa*.
- FA-7 (paso 4) Aprobaciones concurrentes: si dos aprobaciones del mismo programa compiten por un saldo que solo alcanza para una, el sistema confirma una sola y la otra no se aprueba, mostrando "El programa no tiene presupuesto disponible para renovar."
- FA-8 (pasos 1 y 7) Programa ajeno: el sistema no muestra ni permite resolver solicitudes ni becas sin solicitud de un programa del que el coordinador no es responsable.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-26.2).*

#### Criterios de aceptación (15 escenarios)

##### Escenario 1: Renovación aprobada

- **DADO QUE** Pedro solicitó la renovación, su evaluación de 2026-2 es "Cumple" y "Becas Ingeniería" tiene presupuesto disponible
- **CUANDO** el coordinador aprueba con la justificación "Cumple condiciones y hay fondos"
- **ENTONCES** la beca de Pedro pasa de *Activa* a *Renovada* y la decisión queda registrada

##### Escenario 2: Incumplimiento académico

- **DADO QUE** Lucía solicitó la renovación y su evaluación de 2026-2 es "No cumple"
- **CUANDO** el coordinador intenta aprobar
- **ENTONCES** el sistema no lo permite y muestra "El becario no tiene una evaluación de cumplimiento favorable."

##### Escenario 3: Programa sin presupuesto

- **DADO QUE** Pedro cumple las condiciones y el presupuesto de "Becas Ingeniería" está comprometido por completo en otras becas y renovaciones, sin cubrir la de Pedro
- **CUANDO** el coordinador intenta aprobar su renovación
- **ENTONCES** el sistema no lo permite y muestra "El programa no tiene presupuesto disponible para renovar."

##### Escenario 4: La beca no renovada queda *Inactiva

- **DADO QUE** la ventana para 2027-1 cerró y la beca *Activa* de Ana no tiene solicitud de renovación
- **CUANDO** el coordinador confirma que esa beca no se renueva
- **ENTONCES** la beca queda *Inactiva*, no recibe dinero en 2027-1 y la decisión queda registrada con la justificación "No solicitó la renovación dentro de la ventana"

##### Escenario 5: Excepción del programa

- **DADO QUE** la beca *Activa* de Ana no tiene solicitud y el programa define una excepción que el coordinador considera aplicable
- **CUANDO** el coordinador registra la excepción con su justificación
- **ENTONCES** la beca sigue *Activa* y la excepción queda registrada

##### Escenario 6: Sin casos pendientes

- **DADO QUE** todos los becarios activos de "Becas Ingeniería" solicitaron la renovación
- **CUANDO** cierra la ventana
- **ENTONCES** el sistema muestra "Todas las becas activas del programa solicitaron la renovación."

##### Escenario 7: Presupuesto exactamente suficiente

- **DADO QUE** el presupuesto disponible de "Becas Ingeniería" es exactamente igual al monto de la renovación de Pedro y su evaluación es "Cumple"
- **CUANDO** el coordinador aprueba su renovación
- **ENTONCES** la beca pasa a *Renovada* y el presupuesto disponible del programa queda en cero

##### Escenario 8: Beca revocada mientras la solicitud está pendiente

- **DADO QUE** Lucía tiene una solicitud de renovación pendiente y su beca es revocada antes de resolverla (HU-22.1)
- **CUANDO** el coordinador intenta aprobar esa renovación
- **ENTONCES** el sistema no lo permite porque la beca ya no está *Activa*

##### Escenario 9: Coordinador de otro programa

- **DADO QUE** Carla es coordinadora de "Becas Salud"
- **CUANDO** intenta aprobar o rechazar una solicitud de renovación de "Becas Ingeniería"
- **ENTONCES** el sistema no le permite hacerlo

##### Escenario 10: Aprobaciones concurrentes por el mismo saldo

- **DADO QUE** el presupuesto disponible de "Becas Ingeniería" solo alcanza para una renovación, y Pedro y Lucía tienen solicitudes pendientes que cumplen las condiciones
- **CUANDO** dos sesiones aprueban casi al mismo tiempo la renovación de Pedro y la de Lucía
- **ENTONCES** el sistema confirma una sola renovación y la otra no se aprueba, mostrando "El programa no tiene presupuesto disponible para renovar."

##### Escenario 11: Intento de renovar una beca ya *Inactiva

- **DADO QUE** la beca de Ana en "Becas Ingeniería" ya está *Inactiva* por no haber solicitado la renovación a tiempo
- **CUANDO** Ana intenta solicitar la renovación de esa beca
- **ENTONCES** el sistema no registra la solicitud, porque solo una beca *Activa* puede solicitar la renovación

##### Escenario 12: Excepción registrada sin justificación

- **DADO QUE** la beca *Activa* de Ana no tiene solicitud de renovación al cerrar la ventana de 2027-1
- **CUANDO** el coordinador marca que aplica la excepción del programa sin escribir una justificación
- **ENTONCES** el sistema no registra la excepción y muestra "Justifica la excepción."

##### Escenario 13: Renovación rechazada

- **DADO QUE** Lucía solicitó la renovación para 2027-1 y su beca está *Activa*
- **CUANDO** el coordinador rechaza la solicitud con la justificación "El becario concluye su carrera en 2026-2"
- **ENTONCES** la beca de Lucía no pasa a *Renovada*, sigue *Activa*, el rechazo queda registrado con autor, fecha y justificación y el presupuesto comprometido por esa solicitud vuelve a estar disponible en el programa

##### Escenario 14: Becas sin solicitud de un programa ajeno

- **DADO QUE** Carla es coordinadora de "Becas Salud" y la ventana de renovación de "Becas Ingeniería" para 2027-1 ya cerró
- **CUANDO** Carla intenta abrir las becas sin solicitud de renovación de "Becas Ingeniería"
- **ENTONCES** el sistema no se las muestra y no le permite confirmar que no se renuevan ni registrar excepciones

##### Escenario 15: Monto asignado al estudiante al renovar

- **DADO QUE** la beca de Pedro en 2026-2 fue de 2.000 $ para ese periodo, su evaluación es "Cumple" y "Becas Ingeniería" tiene 2.500 $ disponibles
- **CUANDO** el coordinador aprueba su renovación asignándole 2.500 $ para 2027-1
- **ENTONCES** la beca pasa a *Renovada* con el monto de 2.500 $ para 2027-1 y el presupuesto disponible del programa queda en cero

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| La aprobación depende del cumplimiento académico y del presupuesto disponible | Regla | 1, 2, 3 | CUBIERTA |
| Monto fijo por periodo asignado a cada estudiante al renovar, verificado contra el presupuesto | Regla | 15 | CUBIERTA |
| Al aprobar, la beca pasa de *Activa* a *Renovada* | Regla | 1, 15 | CUBIERTA |
| Una beca *Renovada* vuelve a *Activa* al iniciar el nuevo periodo | Regla | — | DELEGADA (HU-38.2) |
| La beca no renovada queda *Inactiva* y no recibe dinero el siguiente periodo | Regla | 4 | CUBIERTA |
| Si se rechaza, la beca sigue *Activa* y el rechazo queda justificado y registrado | Regla / FA-3 | 13 | CUBIERTA |
| El rechazo definitivo libera el presupuesto comprometido por la solicitud | Regla | 13 | CUBIERTA |
| La beca con renovación rechazada queda *Inactiva* al iniciar el siguiente periodo | Regla | — | DELEGADA (HU-38.2, S11) |
| El monto de una renovación pendiente ya cuenta como no disponible, sin doble descuento | Regla | 7 | CUBIERTA |
| Si no se solicita a tiempo, la beca queda *Inactiva* salvo excepción del programa | Regla | 4, 5 | CUBIERTA |
| Sin becas pendientes al cerrar la ventana | Flujo | 6 | CUBIERTA |
| Decisión y cambio de estado registrados con quién, cuándo y por qué | Técnica | 4, 13 (parcial) | DELEGADA (HU-35.1) |
| Una beca *Inactiva* no puede solicitar la renovación porque no está *Activa* | Regla / FA-5 | 11 | CUBIERTA |
| *Inactiva* no es un estado final: puede volver a *Activa*, pasar a *Concluida* o ser revocada | Regla | — | NO PROBABLE: confirmado que decide el coordinador de programa cuando el becario sube los datos que le falten, pero el flujo de esa reactivación no es propio de esta HU |
| Una beca *Inactiva* no libera su presupuesto ni su cupo del programa: los mantiene ocupados | Regla | — | DELEGADA (HU-12.1) |
| Cada coordinador resuelve solo las renovaciones de sus propios programas | Regla | 9, 14 | CUBIERTA |
| Fan-out asíncrono, paginado e idempotente del cierre de ventana | Técnica | — | NO PROBABLE como criterio funcional (diseño) |
| FA-1: Sin cumplimiento académico favorable | FA | 2 | CUBIERTA |
| FA-2: Sin presupuesto disponible | FA | 3 | CUBIERTA |
| FA-4: Excepción registrada sin justificación | FA | 12 | CUBIERTA |
| FA-6: Beca que ya no está *Activa* al resolver la solicitud | FA | 8 | CUBIERTA |
| FA-7: Aprobaciones concurrentes compitiendo por el mismo saldo | FA | 10 | CUBIERTA |
| FA-8: Programa ajeno — resolver solicitudes (paso 1) | FA | 9 | CUBIERTA |
| FA-8: Programa ajeno — confirmar becas sin solicitud o excepciones (paso 7) | FA | 14 | CUBIERTA |
| En qué consiste la "excepción definida por el programa" | Sin definir en el PRD | — | NO PROBABLE |
| Momento exacto en que la beca sin solicitud queda *Inactiva* | Sin definir en el PRD | — | NO PROBABLE |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-26.2).*

## RF-27 — Solicitud de pausa o aplazamiento · *Debería tener*

### HU-27.1 Solicitar y resolver una pausa de la beca

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-27 | Debería tener | HU-16.1, HU-19.1 | HU-27.1, HU-27.2 |

#### Narrativa

- Como becario
- Quiero solicitar una pausa temporal de mi beca por licencia médica o retiro temporal autorizado por mi universidad, y que el coordinador la apruebe o la rechace
- Para no perder la beca por completo mientras no puedo estudiar: durante la pausa se detienen los desembolsos sin concluir la beca

#### Reglas de negocio

- Los motivos de pausa son licencia médica o retiro temporal autorizado por la universidad (RF-27).
- La duración máxima de una pausa es configurable por programa y se registra en su configuración; la solicitud no puede superarla (RF-27; ver HU-11.2).
- La pausa deja la beca en *Suspendida* con el motivo "Pausa": al aprobarla, la beca pasa de *Activa* a *Suspendida*, y solo una beca *Activa* puede pasar a *Suspendida* (S5; regla transversal).
- Durante la pausa se detienen los desembolsos, pero la beca no se considera concluida (RF-27).
- Si la duración aprobada de la pausa vence sin que la beca se reactive, la beca queda *Inactiva* y sin ingresos (ver HU-27.2, dueña de ese flujo).
- El cambio de estado queda justificado y registrado con quién, cuándo y por qué (regla transversal; RF-35, ver HU-35.1).
- La decisión se comunica al becario por el módulo de notificaciones (RF-29; ver HU-29.1); cuando la pausa se aprueba y la beca queda *Suspendida*, la notificación incluye el motivo de la suspensión.

#### Flujo de la HU

1. El becario entra a su beca y elige "Solicitar pausa".
2. Selecciona el motivo: licencia médica o retiro temporal autorizado por su universidad.
3. Indica la duración solicitada y la explicación.
4. El sistema verifica que se haya seleccionado un motivo válido, que la duración no supere la máxima del programa y que la beca esté *Activa*.
5. El sistema registra la solicitud como pendiente y la envía al coordinador.
6. El coordinador abre la solicitud pendiente y revisa el motivo y la duración solicitada.
7. Aprueba o rechaza, con justificación.
8. Si aprueba, el sistema cambia la beca a *Suspendida* con el motivo "Pausa" y la duración aprobada, y detiene sus desembolsos programados (HU-19.1).
9. El sistema registra la decisión (HU-35.1) y la comunica al becario (HU-29.1), incluyendo el motivo cuando la beca queda *Suspendida*.

#### Flujos alternos y de error

- FA-1 (paso 4) Duración mayor a la máxima: el sistema no registra la solicitud y muestra "La pausa no puede superar la duración máxima del programa (4 meses)."
- FA-2 (paso 4) Beca no activa: el sistema no registra la solicitud y muestra "Solo puedes pedir una pausa con una beca activa."
- FA-3 (paso 7) Rechazo: la beca sigue *Activa* y sus desembolsos continúan.
- FA-4 (paso 7) Sin justificación: el sistema no registra la decisión y muestra "Justifica la decisión sobre la pausa."
- FA-5 (paso 4) Sin motivo: el sistema no registra la solicitud y muestra "Selecciona el motivo de la pausa."
- FA-6 (paso 8) Beca que ya no está *Activa*: si la beca cambió de estado mientras la solicitud estaba pendiente, el sistema no aplica la pausa porque la transición a *Suspendida* solo parte de *Activa*.
- FA-7 (posterior al paso 8) Pausa vencida sin reactivar: la beca queda *Inactiva* y sin ingresos (HU-27.2).

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-27.1).*

#### Criterios de aceptación (11 escenarios)

##### Escenario 1: Solicitud válida

- **DADO QUE** la beca de Pedro está *Activa* y la duración máxima de pausa de su programa es 4 meses
- **CUANDO** solicita una pausa de 3 meses por licencia médica
- **ENTONCES** la solicitud queda pendiente y la beca sigue *Activa* hasta que se apruebe

##### Escenario 2: Supera la duración máxima

- **DADO QUE** la duración máxima de pausa del programa es 4 meses
- **CUANDO** Pedro solicita una pausa de 6 meses
- **ENTONCES** el sistema no registra la solicitud y muestra "La pausa no puede superar la duración máxima del programa (4 meses)."

##### Escenario 3: Beca no activa

- **DADO QUE** la beca de Lucía está *Suspendida*
- **CUANDO** Lucía intenta solicitar una pausa
- **ENTONCES** el sistema no registra la solicitud y muestra "Solo puedes pedir una pausa con una beca activa."

##### Escenario 4: Pausa aprobada

- **DADO QUE** Pedro tiene una solicitud de pausa pendiente de 3 meses por licencia médica
- **CUANDO** el coordinador la aprueba con la justificación "Constancia médica recibida"
- **ENTONCES** la beca queda *Suspendida* con el motivo "Pausa" y la decisión queda registrada

##### Escenario 5: Desembolsos detenidos, beca no concluida

- **DADO QUE** la beca de Pedro está *Suspendida* con el motivo "Pausa"
- **CUANDO** el administrador financiero intenta registrar un desembolso programado de Pedro
- **ENTONCES** el sistema no lo permite y la beca no figura como *Concluida*

##### Escenario 6: Pausa rechazada

- **DADO QUE** Pedro tiene una solicitud de pausa pendiente
- **CUANDO** el coordinador la rechaza con la justificación "No presenta autorización de la universidad"
- **ENTONCES** la beca sigue *Activa* y el rechazo queda registrado

##### Escenario 7: Duración exactamente igual a la máxima

- **DADO QUE** la duración máxima de pausa del programa es 4 meses y la beca de Pedro está *Activa*
- **CUANDO** Pedro solicita una pausa de exactamente 4 meses
- **ENTONCES** la solicitud queda pendiente, porque la duración no supera la máxima del programa

##### Escenario 8: Pausa sobre una beca ya en pausa

- **DADO QUE** la beca de Pedro está *Suspendida* con el motivo "Pausa"
- **CUANDO** Pedro intenta solicitar una nueva pausa
- **ENTONCES** el sistema no registra la solicitud y muestra "Solo puedes pedir una pausa con una beca activa."

##### Escenario 9: Motivo no seleccionado

- **DADO QUE** Pedro está llenando la solicitud de pausa
- **CUANDO** la envía sin seleccionar un motivo
- **ENTONCES** el sistema no la registra y muestra "Selecciona el motivo de la pausa."

##### Escenario 10: Decisión sin justificación

- **DADO QUE** el coordinador está resolviendo la solicitud de pausa pendiente de Pedro
- **CUANDO** confirma la aprobación sin escribir una justificación
- **ENTONCES** el sistema no registra la decisión, la beca sigue *Activa* y muestra "Justifica la decisión sobre la pausa."

##### Escenario 11: La beca deja de estar activa mientras la solicitud está pendiente

- **DADO QUE** Pedro tiene una solicitud de pausa pendiente y, antes de que el coordinador decida, su beca queda *Suspendida* por otro motivo
- **CUANDO** el coordinador intenta aprobar la pausa de Pedro
- **ENTONCES** el sistema no aplica la pausa y la beca conserva su estado vigente, porque la transición a *Suspendida* solo parte de una beca *Activa*

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Motivos de pausa: licencia médica o retiro temporal autorizado por la universidad | Regla | 1, 9 | CUBIERTA |
| Duración máxima configurable por programa; la solicitud no puede superarla | Regla / FA-1 | 2, 7 | CUBIERTA |
| Al aprobar, la beca pasa de *Activa* a *Suspendida* con el motivo "Pausa" | Regla | 4 | CUBIERTA |
| Durante la pausa se detienen los desembolsos y la beca no se considera concluida | Regla | 5 | CUBIERTA |
| Pausa vencida sin reactivar: la beca queda *Inactiva* y sin ingresos | Regla / FA-7 | — | DELEGADA (HU-27.2, Escenario 7) |
| Cambio de estado justificado y registrado | Técnica | 4, 6, 10 | CUBIERTA |
| La decisión se comunica por el módulo de notificaciones | Regla | — | DELEGADA (HU-29.1) |
| La notificación de la suspensión por pausa incluye el motivo | Regla | — | DELEGADA (HU-29.1) |
| Estado vigente de la beca validado en cada corrida de desembolsos | Técnica | 5 (observable) | CUBIERTA / DELEGADA (HU-19.1) |
| FA-2: Beca no activa al solicitar | FA | 3, 8 | CUBIERTA |
| FA-3: Rechazo (la beca sigue *Activa*) | FA | 6 | CUBIERTA |
| FA-4: Decisión sin justificación | FA | 10 | CUBIERTA |
| FA-5: Sin motivo seleccionado | FA | 9 | CUBIERTA |
| FA-6: Beca que ya no está *Activa* al momento de aprobar | FA | 11 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-27.1).*

### HU-27.2 Reactivar la beca tras verificar el estatus académico

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-27 | Debería tener | HU-24.1, HU-27.1 | HU-27.3 |

#### Narrativa

- Como coordinador de programa
- Quiero reactivar una beca en pausa después de verificar de nuevo el estatus académico del becario
- Para reanudar sus desembolsos solo si puede continuar sus estudios

#### Reglas de negocio

- Reactivar la beca tras una pausa requiere verificar de nuevo el estatus académico (RF-27).
- El estatus académico es la información de rendimiento del becario: promedio, materias aprobadas, etc. (Glosario; ver HU-24.1), que puede provenir de cualquiera de las tres vías de entrada (HU-24.1, HU-24.2, HU-24.3).
- La reactivación es la transición *Suspendida* (motivo "Pausa") → *Activa* (S5; regla transversal). Este flujo cubre solo las becas *Suspendida* con el motivo "Pausa".
- Si la duración aprobada de la pausa vence sin que la beca se reactive, la beca queda *Inactiva* y sin ingresos: no recibe dinero en el siguiente periodo.
- Cuando llega la fecha de revisión de la pausa —la del vencimiento de su duración aprobada—, el sistema avisa al coordinador del programa (RF-29; ver HU-29.1), para que pueda decidir antes de que la beca quede *Inactiva*.
- Una vez que la beca queda *Inactiva* por vencimiento de la pausa, este flujo de reactivación ya no aplica, porque solo cubre becas *Suspendida*: *Inactiva* no es un estado final y puede volver a *Activa*, pasar a *Concluida* o ser revocada, decisión que toma el **coordinador de programa** cuando el becario **sube los datos que le falten**; el flujo para hacerlo no es propio de esta HU (ver Preguntas abiertas y HU-26.2). Mientras esté *Inactiva*, la beca mantiene ocupados su presupuesto y su cupo del programa.
- El cambio de estado queda justificado y registrado con quién, cuándo y por qué (regla transversal; RF-35, ver HU-35.1).
- Los desembolsos con fecha posterior a la reactivación vuelven a poder registrarse (RF-27; ver HU-19.1).
- **Por confirmar:** qué pasa con los desembolsos cuya fecha venció durante la pausa. El cliente fija que los desembolsos con fecha durante una **suspensión** se pagan al reanudar, pero deja por confirmar si esa regla aplica también a la pausa; mientras no se confirme, esta HU no fija la conducta.

#### Flujo de la HU

1. El coordinador abre una beca *Suspendida* con el motivo "Pausa" de uno de sus programas y elige "Reactivar".
2. El sistema muestra la información académica más reciente del becario (HU-24.1, HU-24.2, HU-24.3).
3. El coordinador registra el resultado de la verificación del estatus académico y su justificación.
4. Si la verificación es favorable, el sistema cambia la beca a *Activa* y sus desembolsos con fecha posterior a la reactivación vuelven a poder registrarse.
5. El sistema registra la decisión (HU-35.1).
6. Cuando llega la fecha de vencimiento de la duración aprobada de la pausa, el sistema avisa al coordinador del programa (HU-29.1).
7. Si la pausa vence sin que la beca se reactive, el sistema la deja *Inactiva*, sin ingresos para el siguiente periodo, y registra el cambio de estado (HU-35.1).

#### Flujos alternos y de error

- FA-1 (paso 3) Sin verificación: el sistema no reactiva la beca y muestra "Debes verificar el estatus académico antes de reactivar."
- FA-2 (paso 4) Verificación no favorable: la beca sigue *Suspendida* y el resultado queda registrado.
- FA-3 (paso 1) Suspensión por otro motivo: el sistema no ofrece en este flujo las becas *Suspendida* cuyo motivo no es "Pausa".
- FA-4 (paso 1) Beca no suspendida: el sistema no ofrece en este flujo las becas que no están *Suspendida*, porque la única transición válida aquí es *Suspendida* → *Activa*.
- FA-5 (paso 7) Pausa vencida sin reactivar: la beca queda *Inactiva* y sin ingresos, y deja de aparecer en este flujo de reactivación.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-27.2).*

#### Criterios de aceptación (8 escenarios)

##### Escenario 1: Reactivación tras verificar

- **DADO QUE** la beca de Pedro está *Suspendida* por "Pausa" y hay un reporte académico reciente de Pedro
- **CUANDO** el coordinador registra la verificación como favorable con la justificación "Reinscrito en 2027-1"
- **ENTONCES** la beca queda *Activa* y la decisión queda registrada

##### Escenario 2: Sin verificación

- **DADO QUE** la beca de Pedro está *Suspendida* por "Pausa"
- **CUANDO** el coordinador intenta reactivarla sin registrar la verificación del estatus académico
- **ENTONCES** la beca sigue *Suspendida* y el sistema muestra "Debes verificar el estatus académico antes de reactivar."

##### Escenario 3: Verificación no favorable

- **DADO QUE** la beca de Lucía está *Suspendida* por "Pausa"
- **CUANDO** el coordinador registra la verificación como no favorable con la justificación "Sin matrícula vigente"
- **ENTONCES** la beca sigue *Suspendida* y el resultado queda registrado

##### Escenario 4: Suspensión por un motivo distinto a "Pausa"

- **DADO QUE** la beca de Lucía está *Suspendida* con un motivo distinto a "Pausa" (HU-25.2)
- **CUANDO** el coordinador abre este flujo de reactivación por pausa
- **ENTONCES** la beca de Lucía no aparece en él

##### Escenario 5: Beca que no está suspendida

- **DADO QUE** la beca de Pedro está *Activa*
- **CUANDO** el coordinador abre este flujo de reactivación por pausa
- **ENTONCES** la beca de Pedro no aparece en él, porque la única transición válida aquí es *Suspendida* → *Activa*

##### Escenario 6: Desembolso posterior a la reactivación

- **DADO QUE** la beca de Pedro fue reactivada el 01/03/2027 tras una pausa
- **CUANDO** el administrador financiero registra un desembolso programado de Pedro con fecha 05/03/2027
- **ENTONCES** ese desembolso puede registrarse, porque la beca está *Activa* desde la reactivación

##### Escenario 7: Pausa vencida sin reactivar

- **DADO QUE** la beca de Pedro está *Suspendida* por "Pausa" con una duración aprobada de 3 meses que vence el 01/03/2027 y nadie la reactiva
- **CUANDO** pasa esa fecha de vencimiento
- **ENTONCES** la beca queda *Inactiva*, no recibe ingresos en el periodo siguiente, el cambio de estado queda registrado y la beca deja de aparecer en el flujo de reactivación por pausa

##### Escenario 8: Aviso al coordinador al llegar la fecha de revisión de la pausa

- **DADO QUE** la pausa de la beca de Pedro tiene su duración aprobada vencida el 01/03/2027
- **CUANDO** llega esa fecha
- **ENTONCES** el coordinador del programa recibe el aviso de que debe resolver la pausa de Pedro

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Reactivar requiere verificar de nuevo el estatus académico | Regla | 1 | CUBIERTA |
| La reactivación es la transición *Suspendida* (motivo "Pausa") → *Activa* | Regla | 1 | CUBIERTA |
| El estatus académico puede provenir de cualquiera de las tres vías de entrada | Regla | 1 (parcial) | DELEGADA (HU-24.1, HU-24.2, HU-24.3) |
| Este flujo cubre solo las becas *Suspendida* con el motivo "Pausa" | Regla / FA-3 | 4 | CUBIERTA |
| Pausa vencida sin reactivar: la beca queda *Inactiva* y sin ingresos | Regla / FA-5 | 7 | CUBIERTA |
| Aviso al coordinador al llegar la fecha de revisión de la pausa | Regla | 8 | CUBIERTA / DELEGADA (HU-29.1) |
| Cambio de estado registrado con quién, cuándo y por qué | Técnica | 1, 3, 7 (parcial) | DELEGADA (HU-35.1) |
| Los desembolsos con fecha posterior a la reactivación vuelven a poder registrarse | Regla | 6 | CUBIERTA |
| Información académica visible solo para quien la necesita para decidir | Técnica (RNF) | — | DELEGADA (RF-37, Área 7) |
| FA-1: Sin verificación | FA | 2 | CUBIERTA |
| FA-2: Verificación no favorable | FA | 3 | CUBIERTA |
| FA-4: Beca no suspendida | FA | 5 | CUBIERTA |
| Desembolsos cuya fecha venció durante la pausa | Por confirmar | — | NO PROBABLE: pendiente de confirmación del cliente |
| *Inactiva* no es un estado final: permite volver a *Activa*, pasar a *Concluida* o ser revocada | Regla | — | NO PROBABLE: confirmado que decide el coordinador de programa cuando el becario sube los datos que le falten, pero el mecanismo no es propio de este flujo (ver HU-26.2) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-27.2).*

## RF-28 — Historial completo del becario · *Debe tener*

### HU-28.1 Ver la línea de tiempo completa de una beca

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-28 | Debe tener | HU-35.1 | HU-28.1, HU-28.2 |

#### Narrativa

- Como coordinador de programa
- Quiero ver la línea de tiempo completa de una beca, y que el becario vea la línea de tiempo de la suya
- Para conocer toda su historia sin reconstruirla desde varias fuentes, y que el becario sepa qué ha pasado con su beca y sus desembolsos

#### Reglas de negocio

- La línea de tiempo muestra el otorgamiento, los desembolsos, los reportes académicos, las renovaciones y las suspensiones de la beca; el becario puede ver la línea de tiempo de su propia beca (RF-28).
- La línea de tiempo también muestra el paso de la beca a *Inactiva*, con su fecha y su motivo (beca no renovada o pausa vencida sin reactivar), porque es un cambio de estatus que debe quedar justificado y registrado (regla transversal; ver HU-26.2 y HU-27.2).
- Cada evento indica su fecha, su tipo y su detalle (RF-28).
- Los eventos registrados no se editan ni se eliminan (RNF de trazabilidad; ver HU-35.1).
- Cada becario ve solo sus propias becas (RNF de protección de datos personales).
- Una persona puede tener más de una beca a la vez, dentro de la regla de no doble beneficio: se aplican a la vez el límite de becas simultáneas de programas "completos" y el tope de que la suma de sus becas en un periodo no supere el costo de la matrícula de su universidad en ese periodo (C2, confirmado; ver HU-01.2, HU-16.1, HU-18.1); por eso la línea de tiempo se consulta por beca y no por persona.
- La información sensible solo es visible para quien la necesita para evaluar o decidir (RNF de confidencialidad).

#### Flujo de la HU

1. El coordinador abre una beca de uno de sus programas y elige "Línea de tiempo".
2. El sistema muestra los eventos de la beca en orden cronológico.
3. Cada evento indica su fecha, su tipo y su detalle (por ejemplo, monto del desembolso, periodo del reporte, motivo de la suspensión, motivo del paso a *Inactiva*).
4. El coordinador abre un evento para ver su detalle completo.
5. El becario entra a su beca y elige "Línea de tiempo".
6. El sistema le muestra los eventos de su beca en orden cronológico, con fecha, tipo y detalle.
7. El becario abre un evento para ver su detalle.

#### Flujos alternos y de error

- FA-1 (paso 2) Beca recién otorgada: la línea de tiempo muestra solo el otorgamiento.
- FA-2 (paso 5) Varias becas: si la persona tiene más de una beca, elige primero la beca a consultar (ver HU-18.1).
- FA-3 (paso 5) Beca de otra persona: el sistema no muestra al becario la línea de tiempo de una beca que no es suya.
- FA-4 (paso 1) Programa ajeno: el sistema no muestra al coordinador las becas de un programa del que no es responsable.
- FA-5 (pasos 4 y 7) Evento no editable: no existe ninguna opción para editar ni eliminar un evento de la línea de tiempo.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-28.1).*

#### Criterios de aceptación (9 escenarios)

##### Escenario 1: Línea de tiempo con varios eventos

- **DADO QUE** la beca de Pedro tiene otorgamiento, tres desembolsos, un reporte académico, una suspensión y una renovación
- **CUANDO** el coordinador abre su línea de tiempo
- **ENTONCES** ve los siete eventos en orden cronológico, cada uno con fecha, tipo y detalle

##### Escenario 2: Beca recién otorgada

- **DADO QUE** la beca de Ana se otorgó hoy y no tiene más eventos
- **CUANDO** el coordinador abre su línea de tiempo
- **ENTONCES** ve solo el evento de otorgamiento

##### Escenario 3: Eventos no editables

- **DADO QUE** el coordinador ve la suspensión en la línea de tiempo de Pedro
- **CUANDO** busca modificarla o eliminarla desde la línea de tiempo
- **ENTONCES** no existe ninguna opción para editar o eliminar el evento

##### Escenario 4: Línea de tiempo propia

- **DADO QUE** la beca de Pedro tiene otorgamiento, dos desembolsos y una suspensión
- **CUANDO** Pedro abre su línea de tiempo
- **ENTONCES** ve los cuatro eventos en orden cronológico, con fecha, tipo y detalle

##### Escenario 5: Mismos eventos que el coordinador

- **DADO QUE** el coordinador ve cinco eventos en la línea de tiempo de la beca de Pedro
- **CUANDO** Pedro abre la línea de tiempo de esa beca
- **ENTONCES** ve los mismos cinco eventos, cada uno con su fecha y su tipo

##### Escenario 6: Beca de otra persona

- **DADO QUE** Pedro y Lucía son becarios del mismo programa
- **CUANDO** Pedro intenta abrir la línea de tiempo de la beca de Lucía
- **ENTONCES** el sistema no se la muestra

##### Escenario 7: Becario con más de una beca

- **DADO QUE** Pedro tiene una beca en "Becas Ingeniería" y otra en "Becas Salud"
- **CUANDO** Pedro elige "Línea de tiempo"
- **ENTONCES** el sistema le pide elegir primero cuál de sus dos becas quiere consultar

##### Escenario 8: Coordinador de un programa ajeno

- **DADO QUE** Carla es coordinadora de "Becas Salud" y no de "Becas Ingeniería"
- **CUANDO** intenta abrir la línea de tiempo de la beca de Pedro en "Becas Ingeniería"
- **ENTONCES** el sistema no le muestra la línea de tiempo y esa beca no aparece entre las que puede consultar

##### Escenario 9: Beca que quedó *Inactiva

- **DADO QUE** la beca de Ana quedó *Inactiva* al no renovarse para 2027-1 (HU-26.2)
- **CUANDO** el coordinador abre su línea de tiempo
- **ENTONCES** ve el evento del paso a *Inactiva* con su fecha y su motivo, además de los eventos anteriores de la beca

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| La línea de tiempo muestra otorgamiento, desembolsos, reportes, renovaciones y suspensiones | Regla | 1 | CUBIERTA |
| La línea de tiempo muestra el paso de la beca a *Inactiva*, con fecha y motivo | Regla | 9 | CUBIERTA |
| Cada evento indica su fecha, su tipo y su detalle | Regla | 1, 4, 9 | CUBIERTA |
| El becario ve la línea de tiempo de su propia beca, con los mismos eventos que el coordinador | Regla | 4, 5 | CUBIERTA |
| Los eventos registrados no se editan ni se eliminan | Regla / FA-5 | 3 | CUBIERTA |
| El becario ve solo sus propias becas | Regla / FA-3 | 6 | CUBIERTA |
| Una persona puede tener más de una beca a la vez | Regla | 7 | CUBIERTA |
| Fuente única append-only de eventos de beca, ordenada y paginada | Técnica | 1, 5 (observable) | CUBIERTA / DELEGADA (HU-35.1) |
| HU-24.3 también escribe eventos en la fuente única | Técnica | — | DELEGADA (HU-24.3, HU-35.1) |
| FA-1: Beca recién otorgada (solo el otorgamiento) | FA | 2 | CUBIERTA |
| FA-2: Varias becas (elegir primero cuál consultar) | FA | 7 | CUBIERTA |
| FA-4: Programa ajeno (coordinador) | FA | 8 | CUBIERTA |
| Qué campos del detalle ve el becario (justificaciones internas) | Sin definir en el PRD | — | NO PROBABLE |
| Acceso del enlace de universidad, finanzas y dirección a la línea de tiempo | Sin definir en el PRD | — | NO PROBABLE |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-28.1).*
