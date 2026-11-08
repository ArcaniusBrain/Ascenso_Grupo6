# Historias de Usuario · Plan B · versión final

> Versión del 14-oct-2026. El PDF entregable está en [`../ASCENSO_Historias_de_Usuario_PlanB_final.pdf`](../ASCENSO_Historias_de_Usuario_PlanB_final.pdf). **Cuando el DDD o la arquitectura difieran de estas historias, mandan las historias.**

## Contenido

- [Área 1 — Postulación y Documentación](area-1-postulacion-y-documentacion.md)
- [Área 2 — Evaluación y Comité](area-2-evaluacion-y-comite.md)
- [Área 3 — Programas, Fondos y Otorgamiento de Becas](area-3-programas-fondos-y-otorgamiento-de-becas.md)
- [Área 4 — Desembolsos y Pagos](area-4-desembolsos-y-pagos.md)
- [Área 5 — Seguimiento Académico y Renovación](area-5-seguimiento-academico-y-renovacion.md)
- [Área 6 — Comunicaciones, Reportería y Cumplimiento](area-6-comunicaciones-reporteria-y-cumplimiento.md)
- [Área 7 — Administración General](area-7-administracion-general.md)
- [Anexo A — Reglas técnicas](anexo-a-reglas-tecnicas.md)
- [Anexo B — Preguntas abiertas al cliente](anexo-b-preguntas-abiertas.md)
- [Anexo C — Correspondencia con la versión 2.0](anexo-c-correspondencia-v2.md)

## Presentación

Este documento contiene la versión final de las historias de usuario del **Plan B** de la plataforma ASCENSO, de la Fundación Ascenso, para la gestión de becas y ayudas estudiantiles.

Parte del *Plan B (preliminar)* del 28 de septiembre de 2026, que traducía los 40 requerimientos funcionales del PRD ASCENSO en 69 historias. Sobre esa base se hicieron tres fases de trabajo:

| Fase | Qué se hizo | Resultado |
| :--- | :--- | :--- |
| 1. Revisión de contenido | Cada historia se revisó desde cuatro roles: Product Owner (valor y criterio INVEST), QA (escenarios y casos borde), Arquitecto (viabilidad y restricciones técnicas) y Scrum Master (consolidación). | Narrativa, reglas, flujos y flujos alternos corregidos y consistentes entre historias. |
| 2. Criterios de aceptación | Se verificó que cada regla de negocio y cada flujo alterno tenga al menos un escenario que lo pruebe, y que ningún escenario fije valores que el PRD no define. | Una matriz de cobertura por historia, sin reglas sin cubrir. |
| 3. Respuestas del cliente | Se aplicaron las respuestas del cliente a las preguntas abiertas (cuatro rondas). Cinco de ellas modifican el PRD. | Reglas concretas donde antes había huecos, y cuatro historias nuevas. |

**En cifras:** 73 historias de usuario para los 40 requerimientos funcionales del PRD, con 714 escenarios de aceptación. Por prioridad: 30 RF *Debe tener* (60 HU), 7 RF *Debería tener* (10 HU) y 3 RF *Podría tener* (3 HU). Respecto al Plan B preliminar (69 HU) hay cuatro historias nuevas: HU-14.3, HU-20.3, HU-21.3 y HU-24.3.

## Cómo leer este documento

- **Orden e identificadores.** Las historias siguen la numeración del PRD. El ID **HU-RR.n** indica el requerimiento (RR) y el orden dentro de él (n). Por ejemplo, HU-20.2 es la segunda historia de RF-20. La numeración es propia de este plan y no coincide con la de la versión 2.0.
- **Prioridad.** Cada historia hereda la prioridad que el PRD da a su requerimiento: *Debe tener*, *Debería tener* o *Podría tener*.
- **Integra (v2.0).** Indica qué historias de la versión 2.0 reúne cada historia. El Anexo C ubica cualquier historia de la versión 2.0 en este plan.
- **Plantilla de cada historia:**
    - **Narrativa** *Como / Quiero / Para*.
    - **Reglas de negocio**, cada una con su fuente: un RF, un RNF, una restricción del cliente, un supuesto (S1–S12) o una decisión del cliente (C1–C5, R1–R30).
    - **Flujo** paso a paso y **flujos alternos y de error** (FA).
    - **Criterios de aceptación** en Gherkin (*DADO QUE / CUANDO / ENTONCES*).
    - **Matriz de cobertura:** relaciona cada regla y cada flujo alterno con los escenarios que lo prueban.
- **Estados de la matriz de cobertura:**
    - **CUBIERTA:** al menos un escenario de la historia lo prueba.
    - **DELEGADA:** la regla pertenece a otra historia, que es la que la prueba.
    - **NO PROBABLE / PENDIENTE DE DEFINICIÓN:** depende de algo que aún no está definido. No lleva escenario y figura en las preguntas abiertas.
    - **NO VERIFICABLE EN CRITERIOS:** es una garantía técnica que se comprueba con pruebas técnicas, no con un escenario funcional.
- **"Sin definir en el PRD".** Cuando ni el PRD ni el cliente definen algo que la historia necesita, la regla lo dice así, los escenarios evitan ese punto y la duda pasa al Anexo B. No se inventan reglas ni valores.
- **Reglas técnicas.** Las reglas marcadas como técnicas en la revisión de arquitectura están en el Anexo A, para que las historias describan solo lo que se comprueba desde fuera del sistema.
- **Preguntas abiertas.** Están reunidas en el Anexo B, por historia, junto con las preguntas generales que siguen pendientes con el cliente.

## Decisiones del cliente

Las respuestas del cliente **prevalecen sobre el PRD** cuando lo contradicen o lo completan. Las historias las citan con su código.

### Cambios al PRD (C1–C5)

| Código | Decisión | Requerimientos afectados |
| :--- | :--- | :--- |
| **C1** | **Monto fijo por periodo.** Se eliminan las coberturas total y parcial como forma de calcular el monto: toda beca es un monto fijo que cubre un periodo (por ejemplo, 2.000 $ por semestre). En una beca nueva el monto lo fija el programa; en una renovación se asigna a cada estudiante. | RF-11, RF-12, RF-15, RF-19, RF-22, RF-23, RF-26 |
| **C2** | **No doble beneficio.** Cada programa se marca como "completo" o "parcial". Se aplican a la vez dos reglas: una persona no puede tener dos becas de programas "completos" activas al mismo tiempo, y la suma de sus becas en un periodo no puede superar el costo de la matrícula de su universidad en ese periodo. | RF-01, RF-14, RF-16, RF-18 |
| **C3** | **Nuevo estado *Inactiva*.** Lo toman la beca no renovada y la beca cuya pausa vence sin reactivarse. No es final: desde *Inactiva* la beca puede volver a *Activa*, pasar a *Concluida* o ser revocada. | RF-19, RF-22, RF-26, RF-27, RF-28, RF-38 |
| **C4** | **Periodos por universidad.** Hay un periodo de becas común, y las disponibilidades se cierran para cada universidad cuando esta inicia sus inscripciones. Se registra esa fecha por universidad y periodo. | RF-02, RF-10, RF-14, RF-38 |
| **C5** | **Integración con las universidades.** El promedio y la información académica pueden llegar por API desde la universidad o por registro manual. Se amplía el alcance del PRD, que dejaba fuera la integración automática. | RF-04, RF-24 |

### Respuestas individuales (R1–R30)

| Código | Tema | Respuesta |
| :--- | :--- | :--- |
| R1 | Identificación única | Número de cédula. |
| R3 | Enlace de universidad | Se le exige verificación en dos pasos. |
| R4a–R4f | Códigos de verificación | El usuario elige correo o SMS; el código vale 15 minutos; tras 3 intentos fallidos hay una espera de 1 hora; se puede reenviar; el segundo factor se recupera por correo; los evaluadores externos lo reciben por correo. |
| R5, R6 | Datos de pago | Solo el becario los registra y modifica, también la primera vez y siempre con código. El administrador financiero solo los consulta. |
| R9 | Modalidad de pago | La fija el programa. |
| R10 | Cupos | El cupo del programa es el total de becarios que sostiene; el de la convocatoria, los becarios nuevos de ese ciclo. |
| R11 | Ofertas pendientes | Restan presupuesto hasta que el proceso se rechace definitivamente. |
| R12 | Suspensión | Los desembolsos con fecha durante una suspensión se pagan al reanudar. |
| R13 | Promedio del postulante | Llega por API o lo registra una persona que revisa las notas enviadas (C5). |
| R14 | Criterios de elegibilidad | Los define el coordinador de programa. |
| R16 | Periodos | Los abre y cierra Dirección. La apertura del periodo de evaluación bloquea la rúbrica. |
| R17 | Conflicto de interés | El evaluador con conflicto no evalúa; otro miembro del comité ocupa su lugar. |
| R18a–R18d | Vencimientos | La beca no renovada y la pausa vencida pasan a *Inactiva* (C3). Al llegar la fecha de revisión de una suspensión se avisa al coordinador. La lista de espera termina cuando la universidad inicia sus inscripciones. |
| R19 | Incumplimiento de permanencia | Cuenta como incumplimiento grave y permite revocar directamente. |
| R20 | Fechas límite | Se evalúan en UTC-4 e incluyen el día de cierre hasta las 23:59. |
| R21 | Fechas de los periodos | Las define cada universidad (C4). |
| R22 | Perfil y postulación | La postulación guarda una copia del perfil al momento de postular; el perfil puede actualizarse mientras el periodo siga abierto. |
| R23 | Varias postulaciones | Se permiten, y se puede volver a postular tras un rechazo. |
| R24 | Varias becas | Se permiten, dentro de la regla C2. |
| R26 | Correo de suspensión | Incluye el motivo. |
| R27 | *Inactiva* y presupuesto | Una beca *Inactiva* mantiene ocupados su cupo y su presupuesto. |
| R28 | Salida de *Inactiva* | El becario sube los datos que le falten y el coordinador de programa decide el paso a *Activa* o a *Concluida*. |
| R29 | Revocar una beca *Inactiva* | Sí se puede. |
| R30 | *Inactiva*, recordatorios y consultas | El becario sigue recibiendo recordatorios y puede enviar consultas. |

Las preguntas que el cliente aún no ha respondido están en el Anexo B.

## Actores

| Actor | Qué le importa y qué hace en la plataforma |
| :--- | :--- |
| Postulante | Entender si califica, qué documentos le faltan y en qué estado está su postulación. Se identifica por su número de cédula (R1). |
| Becario o becaria | Ver el estado de sus desembolsos, cumplir sus condiciones de permanencia y solicitar la renovación a tiempo. Registra y modifica sus propios datos de pago (R5, R6). |
| Coordinador de programa | Definir los criterios de elegibilidad (R14), gestionar el ciclo de postulación, decidir los casos límite y las transiciones de la beca, incluida la salida de *Inactiva* (R28). |
| Miembro del comité evaluador | Revisar y puntuar las postulaciones asignadas con criterios claros, evitando conflictos de interés. Si declara un conflicto, otro miembro del comité ocupa su lugar (R17). Uno de los miembros puede tener la jefatura del comité (RF-10; S12). |
| Administrador financiero | Registrar y controlar los desembolsos sin exceder el presupuesto aprobado. Consulta los datos de pago, pero no los modifica (R5). |
| Enlace de universidad aliada | Certificar la matrícula y el rendimiento académico de los becarios. Inicia sesión con verificación en dos pasos (R3). |
| Sistema académico de la universidad | Actor no humano: envía por API los reportes académicos de los becarios (C5; HU-24.3). |
| Donante o patrocinador | Saber cómo se usan sus fondos y qué impacto tienen. No usa la plataforma: recibe los reportes. |
| Dirección de la Fundación | Supervisar los programas, aprobar configuraciones y resolver excepciones. Crea los usuarios internos (S1) y abre y cierra los periodos (R16). |

## Estados

| Entidad | Estados | Transiciones |
| :--- | :--- | :--- |
| Postulación | *Recibida*, *En verificación*, *En evaluación*, *No elegible*, *Otorgada*, *Rechazada*, *Retirada* | Una sola a la vez y sin saltar etapas: de *Recibida* no se pasa a *Otorgada* sin evaluación. La postulación guarda una copia del perfil al momento de postular (R22). |
| Beca | *Otorgada*, *Activa*, *Suspendida*, *Renovada*, *Inactiva*, *Concluida*, *Revocada* | Ver la tabla siguiente. *Concluida* y *Revocada* son finales. |

| Desde | Hacia | Cuándo |
| :--- | :--- | :--- |
| *Otorgada* | *Activa* | Al firmarse el compromiso o, si el programa no lo exige, al aceptarse la oferta (S10). |
| *Activa* | *Suspendida* | Suspensión con motivo y fecha de revisión, o pausa aprobada (S5). |
| *Suspendida* | *Activa* | Reanudación, o reactivación tras una pausa con verificación del estatus académico. |
| *Suspendida* (pausa) | *Inactiva* | La pausa vence sin que la beca se reactive (C3, R18b). |
| *Suspendida* | *Revocada* | Revocación con motivo y código de un solo uso. |
| *Activa* | *Renovada* | Renovación aprobada. |
| *Renovada* | *Activa* | Al iniciar el nuevo periodo (S4). |
| *Activa* | *Inactiva* | Renovación rechazada o no solicitada a tiempo (C3, R18a). |
| *Inactiva* | *Activa* o *Concluida* | Lo decide el coordinador de programa cuando el becario sube los datos que le falten (R28). |
| *Inactiva* | *Revocada* | Se puede revocar (R29). |
| *Activa* | *Concluida* | Fin de la beca. |
| *Activa* | *Revocada* | Directamente, por incumplimiento grave. Incumplir una condición de permanencia cuenta como incumplimiento grave (R19). |

Una beca *Inactiva* no recibe dinero en el periodo siguiente, **mantiene ocupados su cupo y su presupuesto** (R27) y **no cuenta como beca activa** para la regla de no doble beneficio. Su becario sigue recibiendo recordatorios y puede enviar consultas (R30).

## Reglas transversales

1. **Monto de la beca (C1).** Toda beca es un monto fijo por periodo. En una beca nueva lo fija el programa; en una renovación se asigna a cada estudiante. La modalidad de pago la fija el programa (R9).
2. **No doble beneficio (RF-01, C2).** Se aplican tres reglas a la vez: (a) no se puede postular a un programa teniendo ya una beca activa en el mismo periodo, salvo que ese programa permita acumulación; (b) no se pueden tener dos becas de programas marcados "completo" activas al mismo tiempo; (c) la suma de las becas de una persona en un periodo no puede superar el costo de su matrícula en ese periodo (HU-14.2). Cuentan como beca activa las becas *Otorgada*, *Activa*, *Suspendida* y *Renovada* (S6); una beca *Inactiva* no cuenta.
3. **Identidad (R1).** Una persona se identifica de forma única por su número de cédula.
4. **Verificación en dos pasos (restricción del cliente, R3).** La usan coordinadores, administradores financieros, Dirección, miembros del comité y enlaces de universidad.
5. **Códigos de un solo uso (restricción del cliente, R4).** Se exigen para confirmar un desembolso, revocar una beca y registrar o modificar los datos de pago. El usuario elige si recibe el código por correo o por SMS; el código vale 15 minutos; tras 3 intentos fallidos hay una espera de 1 hora, y se puede reenviar.
6. **Fechas límite (R20).** Se evalúan en UTC-4 e incluyen el día de cierre hasta las 23:59.
7. **Periodos (C4, R16).** Dirección abre y cierra los periodos. El periodo de becas es común, pero las disponibilidades de cada universidad se cierran cuando esa universidad inicia sus inscripciones (HU-14.3). En ese mismo momento termina, para esa universidad, la lista de espera.
8. **Presupuesto disponible (RF-12, R11, R27).** Se calcula como los aportes, menos lo otorgado, menos lo comprometido en ofertas pendientes, menos lo comprometido en renovaciones pendientes. Una beca *Inactiva* mantiene su presupuesto y su cupo; una beca revocada los libera.
9. **Alcance (C5).** La integración con los sistemas académicos de las universidades forma parte del alcance.

## Supuestos

Solo donde ni el PRD ni el cliente definen un punto que las historias necesitan para encajar. Deben validarse con el cliente.

| # | Supuesto | Historias |
| :--- | :--- | :--- |
| S1 | Dirección crea los usuarios internos y asigna sus roles, incluidas las cuentas de los evaluadores externos del comité. Falta definir quién crea la cuenta del enlace de universidad. | HU-06.1, HU-37.1 |
| S2 | La lista de espera es una marca sobre la postulación elegible no otorgada; no es un estado nuevo. Termina, para cada universidad, al iniciar sus inscripciones (R18d). | HU-10.2, HU-15.2, HU-16.2 |
| S3 | Mientras hay una oferta pendiente, la postulación sigue *En evaluación*. Si la acepta, pasa a *Otorgada*; si la rechaza o la oferta expira, pasa a *Retirada*. | HU-15.1, HU-15.2, HU-16.1, HU-16.2 |
| S4 | Una beca *Renovada* vuelve a *Activa* al iniciar el nuevo periodo. | HU-26.2, HU-38.2 |
| S5 | Una beca en pausa (RF-27) está en estado *Suspendida*, con el motivo "Pausa". Si la pausa vence sin reactivarse, pasa a *Inactiva* (C3). | HU-27.1, HU-27.2 |
| S6 | "Beca activa", en la regla de no doble beneficio, incluye las becas *Otorgada*, *Activa*, *Suspendida* y *Renovada*. No incluye *Inactiva*. | HU-01.2, HU-16.1, HU-18.1 |
| S7 | La firma del compromiso es una aceptación electrónica en la plataforma. | HU-17.2 |
| S8 | El presupuesto total de un programa es la suma de los aportes de sus donantes. | HU-11.2, HU-12.1, HU-13.2 |
| S9 | Las puntuaciones de otros evaluadores se ocultan hasta el cierre del periodo de revisión (RF-08). Prevalece sobre la frase general de RF-37 por ser más específica. | HU-08.2, HU-37.1 |
| S10 | La beca pasa de *Otorgada* a *Activa* al firmarse el compromiso (HU-17.2) o, si el programa no lo exige, al aceptarse la oferta (HU-16.1). | HU-16.1, HU-17.2, HU-19.1 |
| S11 | *Reemplazado por C3.* Si la renovación se rechaza o no se solicita, la beca pasa de *Activa* a *Inactiva*, y ya no a *Concluida*. | HU-26.2, HU-38.2 |
| S12 | El coordinador designa la jefatura del comité al conformarlo. | HU-06.1, HU-10.1 |

## Índice de requerimientos e historias

| RF | Requerimiento | Prioridad | Historias |
| :--- | :--- | :--- | :--- |
| RF-01 | Registro de postulantes | Debe tener | [HU-01.1](area-1-postulacion-y-documentacion.md#hu-011-crear-mi-perfil-de-postulante-y-postular-a-una-convocatoria-abierta), [HU-01.2](area-1-postulacion-y-documentacion.md#hu-012-impedir-postular-con-una-beca-activa-en-el-mismo-periodo) |
| RF-02 | Publicación de convocatorias | Debe tener | [HU-02.1](area-1-postulacion-y-documentacion.md#hu-021-configurar-el-ciclo-de-postulación-fechas-cupo-criterios-y-documentos), [HU-02.2](area-1-postulacion-y-documentacion.md#hu-022-publicar-la-convocatoria-y-aceptar-postulaciones-solo-dentro-de-su-ventana) |
| RF-03 | Carga de documentos de soporte | Debe tener | [HU-03.1](area-1-postulacion-y-documentacion.md#hu-031-cargar-mis-documentos-de-soporte-y-ver-los-que-faltan), [HU-03.2](area-1-postulacion-y-documentacion.md#hu-032-rechazar-un-documento-con-motivo-y-permitir-que-se-vuelva-a-subir) |
| RF-04 | Verificación automática de elegibilidad básica | Debe tener | [HU-04.1](area-1-postulacion-y-documentacion.md#hu-041-verificar-automáticamente-la-elegibilidad-básica-y-mostrar-el-motivo), [HU-04.2](area-1-postulacion-y-documentacion.md#hu-042-anular-un-resultado-de-no-elegible-en-un-caso-límite) |
| RF-05 | Retiro voluntario de una postulación | Debería tener | [HU-05.1](area-1-postulacion-y-documentacion.md#hu-051-retirar-mi-postulación-antes-de-que-empiece-la-evaluación) |
| RF-06 | Conformación de comités evaluadores por programa | Debe tener | [HU-06.1](area-2-evaluacion-y-comite.md#hu-061-conformar-el-comité-de-un-programa), [HU-06.2](area-2-evaluacion-y-comite.md#hu-062-asignar-los-evaluadores-de-cada-postulación) |
| RF-07 | Rúbrica de evaluación configurable por programa | Debe tener | [HU-07.1](area-2-evaluacion-y-comite.md#hu-071-definir-la-rúbrica-del-programa-con-pesos-que-sumen-100-), [HU-07.2](area-2-evaluacion-y-comite.md#hu-072-bloquear-la-rúbrica-al-comenzar-la-evaluación-del-ciclo) |
| RF-08 | Registro de puntuaciones y comentarios de evaluadores | Debe tener | [HU-08.1](area-2-evaluacion-y-comite.md#hu-081-puntuar-comentar-y-enviar-mi-evaluación), [HU-08.2](area-2-evaluacion-y-comite.md#hu-082-ocultar-las-puntuaciones-de-otros-evaluadores-hasta-el-cierre) |
| RF-09 | Consolidación y ranking de postulantes | Debe tener | [HU-09.1](area-2-evaluacion-y-comite.md#hu-091-calcular-la-puntuación-consolidada-y-marcar-las-diferencias-grandes), [HU-09.2](area-2-evaluacion-y-comite.md#hu-092-generar-el-ranking-respetando-cupo-y-presupuesto) |
| RF-10 | Resolución de empates y casos límite | Debe tener | [HU-10.1](area-2-evaluacion-y-comite.md#hu-101-resolver-un-empate-o-un-caso-límite-con-justificación), [HU-10.2](area-2-evaluacion-y-comite.md#hu-102-dejar-en-lista-de-espera-a-los-elegibles-no-otorgados) |
| RF-11 | Registro y configuración de programas de becas | Debe tener | [HU-11.1](area-3-programas-fondos-y-otorgamiento-de-becas.md#hu-111-crear-un-programa-de-becas-con-su-monto-por-periodo), [HU-11.2](area-3-programas-fondos-y-otorgamiento-de-becas.md#hu-112-configurar-los-donantes-la-elegibilidad-y-la-renovación-del-programa) |
| RF-12 | Gestión de presupuesto y disponibilidad de cupos | Debe tener | [HU-12.1](area-3-programas-fondos-y-otorgamiento-de-becas.md#hu-121-consultar-el-presupuesto-y-el-cupo-disponibles-y-verificarlos-antes-de-otorgar), [HU-12.2](area-3-programas-fondos-y-otorgamiento-de-becas.md#hu-122-contar-las-renovaciones-pendientes-como-no-disponibles) |
| RF-13 | Alta y gestión de donantes o patrocinadores | Debe tener | [HU-13.1](area-3-programas-fondos-y-otorgamiento-de-becas.md#hu-131-registrar-un-donante), [HU-13.2](area-3-programas-fondos-y-otorgamiento-de-becas.md#hu-132-registrar-los-aportes-de-un-donante-y-consultar-su-historial) |
| RF-14 | Convenios con universidades aliadas | Debe tener | [HU-14.1](area-3-programas-fondos-y-otorgamiento-de-becas.md#hu-141-registrar-una-universidad-aliada-con-su-convenio-y-sus-carreras-elegibles), [HU-14.2](area-3-programas-fondos-y-otorgamiento-de-becas.md#hu-142-registrar-el-costo-de-matrícula-por-periodo-sin-perder-los-anteriores), [HU-14.3](area-3-programas-fondos-y-otorgamiento-de-becas.md#hu-143-registrar-la-fecha-de-inicio-de-inscripciones-de-cada-universidad-por-periodo) |
| RF-15 | Emisión de carta de otorgamiento | Debe tener | [HU-15.1](area-3-programas-fondos-y-otorgamiento-de-becas.md#hu-151-generar-la-oferta-formal-de-beca-y-comunicarla-con-su-fecha-límite), [HU-15.2](area-3-programas-fondos-y-otorgamiento-de-becas.md#hu-152-expirar-una-oferta-sin-respuesta-y-considerar-al-siguiente) |
| RF-16 | Aceptación o rechazo formal de la beca | Debe tener | [HU-16.1](area-3-programas-fondos-y-otorgamiento-de-becas.md#hu-161-aceptar-la-oferta-y-convertirme-en-becario), [HU-16.2](area-3-programas-fondos-y-otorgamiento-de-becas.md#hu-162-rechazar-la-oferta) |
| RF-17 | Firma de compromiso del becario | Debe tener | [HU-17.1](area-3-programas-fondos-y-otorgamiento-de-becas.md#hu-171-configurar-el-compromiso-que-exige-un-programa), [HU-17.2](area-3-programas-fondos-y-otorgamiento-de-becas.md#hu-172-firmar-mi-compromiso-de-becario) |
| RF-18 | Historial de becas por postulante o becario | Debe tener | [HU-18.1](area-3-programas-fondos-y-otorgamiento-de-becas.md#hu-181-consultar-el-historial-de-becas-de-una-persona-por-programa-o-periodo) |
| RF-19 | Programación de desembolsos por becario | Debe tener | [HU-19.1](area-4-desembolsos-y-pagos.md#hu-191-generar-el-calendario-de-desembolsos-de-un-becario-sin-superar-lo-aprobado) |
| RF-20 | Registro y confirmación de desembolsos realizados | Debe tener | [HU-20.1](area-4-desembolsos-y-pagos.md#hu-201-registrar-y-confirmar-un-desembolso-con-un-código-de-un-solo-uso), [HU-20.2](area-4-desembolsos-y-pagos.md#hu-202-resolver-un-desembolso-fallido-o-devuelto), [HU-20.3](area-4-desembolsos-y-pagos.md#hu-203-registrar-y-modificar-los-datos-de-pago-del-becario-con-un-código-de-un-solo-uso) |
| RF-21 | Suspensión temporal de desembolsos | Debe tener | [HU-21.1](area-4-desembolsos-y-pagos.md#hu-211-suspender-una-beca-con-motivo-y-fecha-de-revisión-y-notificar-al-becario), [HU-21.2](area-4-desembolsos-y-pagos.md#hu-212-reanudar-una-beca-suspendida), [HU-21.3](area-4-desembolsos-y-pagos.md#hu-213-avisar-al-coordinador-cuando-llega-la-fecha-de-revisión-de-una-suspensión) |
| RF-22 | Revocación de una beca | Debe tener | [HU-22.1](area-4-desembolsos-y-pagos.md#hu-221-revocar-una-beca-con-motivo-y-código-de-un-solo-uso), [HU-22.2](area-4-desembolsos-y-pagos.md#hu-222-liberar-el-presupuesto-y-el-cupo-de-una-beca-revocada) |
| RF-23 | Reporte de estado financiero por programa | Debería tener | [HU-23.1](area-4-desembolsos-y-pagos.md#hu-231-consultar-y-exportar-el-estado-financiero-de-un-programa) |
| RF-24 | Registro de reportes académicos periódicos | Debe tener | [HU-24.1](area-5-seguimiento-academico-y-renovacion.md#hu-241-entregar-el-reporte-académico-del-periodo-con-alerta-de-entrega-tardía), [HU-24.2](area-5-seguimiento-academico-y-renovacion.md#hu-242-registrar-el-reporte-académico-desde-la-universidad), [HU-24.3](area-5-seguimiento-academico-y-renovacion.md#hu-243-recibir-por-api-el-reporte-académico-que-envía-la-universidad) |
| RF-25 | Evaluación de cumplimiento de condiciones de permanencia | Debe tener | [HU-25.1](area-5-seguimiento-academico-y-renovacion.md#hu-251-definir-la-política-de-consecuencias-y-evaluar-el-cumplimiento), [HU-25.2](area-5-seguimiento-academico-y-renovacion.md#hu-252-aplicar-la-consecuencia-por-incumplimiento-e-informarla-al-becario) |
| RF-26 | Solicitud y evaluación de renovación | Debe tener | [HU-26.1](area-5-seguimiento-academico-y-renovacion.md#hu-261-definir-la-ventana-de-renovación-y-solicitar-la-renovación-dentro-de-ella), [HU-26.2](area-5-seguimiento-academico-y-renovacion.md#hu-262-evaluar-la-renovación-y-resolver-las-becas-no-renovadas) |
| RF-27 | Solicitud de pausa o aplazamiento | Debería tener | [HU-27.1](area-5-seguimiento-academico-y-renovacion.md#hu-271-solicitar-y-resolver-una-pausa-de-la-beca), [HU-27.2](area-5-seguimiento-academico-y-renovacion.md#hu-272-reactivar-la-beca-tras-verificar-el-estatus-académico) |
| RF-28 | Historial completo del becario | Debe tener | [HU-28.1](area-5-seguimiento-academico-y-renovacion.md#hu-281-ver-la-línea-de-tiempo-completa-de-una-beca) |
| RF-29 | Notificaciones automáticas de hitos clave | Debería tener | [HU-29.1](area-6-comunicaciones-reporteria-y-cumplimiento.md#hu-291-recibir-los-hitos-de-mi-postulación-o-beca-por-correo-y-por-los-canales-del-programa), [HU-29.2](area-6-comunicaciones-reporteria-y-cumplimiento.md#hu-292-enviar-notificaciones-a-todo-un-ciclo-sin-ralentizar-la-plataforma) |
| RF-30 | Recordatorios antes de un vencimiento | Debería tener | [HU-30.1](area-6-comunicaciones-reporteria-y-cumplimiento.md#hu-301-configurar-y-recibir-recordatorios-antes-de-un-vencimiento) |
| RF-31 | Comunicación al comité evaluador | Podría tener | [HU-31.1](area-6-comunicaciones-reporteria-y-cumplimiento.md#hu-311-avisar-al-comité-de-nuevas-asignaciones-y-del-cierre-de-la-evaluación) |
| RF-32 | Mensajería de postulantes y becarios hacia la Fundación | Debería tener | [HU-32.1](area-6-comunicaciones-reporteria-y-cumplimiento.md#hu-321-enviar-una-consulta-sobre-mi-postulación-o-beca), [HU-32.2](area-6-comunicaciones-reporteria-y-cumplimiento.md#hu-322-responder-y-dar-seguimiento-a-una-consulta) |
| RF-33 | Reporte de impacto por programa o donante | Debe tener | [HU-33.1](area-6-comunicaciones-reporteria-y-cumplimiento.md#hu-331-generar-el-reporte-de-impacto-de-un-donante-con-datos-agregados-o-anonimizados), [HU-33.2](area-6-comunicaciones-reporteria-y-cumplimiento.md#hu-332-registrar-el-consentimiento-para-ser-presentado-a-mi-donante) |
| RF-34 | Reporte consolidado institucional | Debe tener | [HU-34.1](area-6-comunicaciones-reporteria-y-cumplimiento.md#hu-341-consultar-el-reporte-consolidado-institucional-por-periodo-universidad-y-carrera) |
| RF-35 | Trazabilidad y auditoría de decisiones | Debe tener | [HU-35.1](area-6-comunicaciones-reporteria-y-cumplimiento.md#hu-351-registrar-cada-decisión-con-quién-cuándo-y-por-qué-sin-edición-posterior), [HU-35.2](area-6-comunicaciones-reporteria-y-cumplimiento.md#hu-352-consultar-el-registro-de-decisiones) |
| RF-36 | Exportación de información para auditorías externas | Podría tener | [HU-36.1](area-6-comunicaciones-reporteria-y-cumplimiento.md#hu-361-exportar-el-uso-de-los-fondos-y-el-registro-de-decisiones-para-una-auditoría) |
| RF-37 | Gestión de usuarios internos y sus permisos | Debe tener | [HU-37.1](area-7-administracion-general.md#hu-371-crear-usuarios-internos-y-tipos-de-usuario-con-acceso-limitado-a-su-función), [HU-37.2](area-7-administracion-general.md#hu-372-iniciar-sesión-con-verificación-en-dos-pasos) |
| RF-38 | Gestión del calendario académico institucional | Debe tener | [HU-38.1](area-7-administracion-general.md#hu-381-registrar-y-consultar-los-periodos-académicos), [HU-38.2](area-7-administracion-general.md#hu-382-marcar-el-periodo-actual-sin-interrumpir-renovaciones) |
| RF-39 | Registro de conflictos de interés | Debería tener | [HU-39.1](area-7-administracion-general.md#hu-391-declarar-un-conflicto-de-interés-y-restringir-mi-participación-en-ese-caso) |
| RF-40 | Panel de indicadores para dirección | Podría tener | [HU-40.1](area-7-administracion-general.md#hu-401-ver-los-indicadores-clave-actualizados-sin-recargar-la-página) |

Las historias HU-14.3, HU-20.3, HU-21.3 y HU-24.3 son nuevas respecto al Plan B preliminar. Por ellas, RF-14, RF-20, RF-21 y RF-24 tienen tres historias.
