# Anexo B — Preguntas abiertas al cliente

Preguntas generales que siguen pendientes con el cliente:

| # | Pregunta | Historias afectadas |
| :--- | :--- | :--- |
| 2 | ¿Qué puede ver y hacer cada rol? Hace falta la tabla completa de permisos de coordinador, evaluador, administrador financiero, Dirección y enlace de universidad. | HU-37.1 y los escenarios de permisos de todas las áreas |
| 15 | ¿Cómo se calcula el total de cada evaluador a partir de los pesos y los rangos de los criterios de la rúbrica? | HU-08.1, HU-09.1, HU-09.2 |
| 25 | ¿Qué formato y qué nivel de detalle exige una auditoría externa? Conviene saberlo antes de construir el registro de decisiones. | HU-35.1, HU-36.1 |
| 3 | ¿Quién crea la cuenta del enlace de universidad aliada? | HU-24.2, HU-37.1 |
| 4d | Al reenviar un código de verificación, ¿se invalida el anterior? | HU-20.1, HU-20.3, HU-22.1, HU-37.2 |
| 7 | ¿Qué campos forman los datos de pago? Propuesta: titular, cédula del titular, banco, tipo de cuenta, número de cuenta y, si aplica, pago móvil. | HU-20.3 |
| 12 | Los desembolsos con fecha durante una pausa, ¿también se pagan al reactivar la beca, como en la suspensión? | HU-27.1, HU-27.2 |
| 13 | ¿Quién registra a mano el promedio cuando no llega por API? | HU-04.1, HU-24.2 |
| 17 | Las puntuaciones que un evaluador registró antes de declarar un conflicto de interés, ¿se descartan? | HU-06.2, HU-39.1 |
| 18c | Cuando llega la fecha de revisión de una suspensión, ¿también se avisa al becario? | HU-21.3 |
| 22 | Si el postulante actualiza su perfil mientras el periodo sigue abierto, ¿cambia la postulación ya enviada? | HU-01.1, HU-04.1 |
| 29b | ¿Se pueden resolver los desembolsos fallidos de una beca *Inactiva*? | HU-20.2 |
| S4 | Validar el supuesto S4: una beca *Renovada* vuelve a *Activa* al iniciar el nuevo periodo. | HU-26.2, HU-38.2 |

Preguntas abiertas de cada historia. Ninguna se ha resuelto con un valor inventado: mientras no haya respuesta, los escenarios evitan esos puntos.

## HU-01.1 Crear mi perfil de postulante y postular a una convocatoria abierta

- ¿Qué datos de acceso usa la cuenta (correo, usuario, otro)? La deduplicación ya está resuelta con la cédula, pero el dato con el que se inicia sesión sigue sin definirse.
- ¿La actualización del perfil mientras el periodo sigue abierto cambia la postulación ya enviada, o esta conserva siempre la copia original? La respuesta lo deja **por confirmar**; hasta entonces esta HU no fija ningún recálculo ni reevaluación (relacionado con HU-04.1).
- ¿Qué rango y formato son válidos para el ingreso familiar declarado (¿admite cero?, ¿hay máximo?, ¿moneda o unidad de referencia?) y para la composición familiar? El PRD exige los campos pero no define su validación, por lo que esta HU no fija ninguna.
- ¿Puede un mismo postulante tener dos postulaciones **al mismo programa y periodo**? ¿Un reintento por pérdida de conexión debe ser idempotente o puede crear una segunda postulación? La respuesta confirma varias postulaciones y volver a postular tras un rechazo, pero no cubre el caso de dos postulaciones al mismo ciclo.
- ¿Una postulación *Retirada* o *No elegible* en un ciclo bloquea volver a postular al mismo programa en un ciclo futuro? La respuesta cubre expresamente el rechazo, no estos dos estados.
- ¿Se divide esta historia para planeación (cuenta+perfil / postular) o se mantiene como historia contenedora por ser un solo formulario en la interfaz? Decisión de alcance y estimación, no de negocio.
- Los intentos de acceso indebido (FA-8, FA-9), ¿deben generar una alerta activa al equipo, o basta con negar el acceso y dejar el registro de trazabilidad?
- ¿Debe el sistema mostrar un mensaje explicativo cuando la universidad del postulante no es aliada, o basta con que no aparezca en la lista? FA-3 describe el caso sin mensaje y el Escenario 4 sí lo fija; conviene cerrar la diferencia con el cliente. *(Pregunta abierta de la Fase 2.)*

## HU-01.2 Impedir postular con una beca activa en el mismo periodo

- ¿La regla de los programas "completos" prevalece siempre sobre el flag "permite acumulación" del programa? Así se especifica aquí, por continuidad con la lectura del PRD, pero la respuesta no lo dice y debe confirmarse.
- ¿Cuál es el mensaje exacto que ve el postulante cuando el bloqueo proviene de la etiqueta "completo" o del tope a la suma, y no de la regla de acumulación del programa? No está definido, por lo que los Escenarios 6 y 11 no fijan un mensaje.
- ¿Qué hace el sistema cuando no hay límite registrado para la universidad del postulante en ese periodo: bloquea, deja pasar, o aplica solo la capa 2?
- ¿Dónde y quién configura el flag "permite acumulación" de un programa? La etiqueta "completo"/"parcial" ya la captura HU-11.1, pero el flag de acumulación sigue sin aparecer en el modelo de programa de RF-11.
- ¿El bloqueo considera también becas de la persona en el mismo programa, o solo en programas distintos? La regla transversal habla de "programas distintos" y RF-01 de "un programa"; la combinación no está explicitada.

## HU-02.1 Configurar el ciclo de postulación: fechas, cupo, criterios y documentos

- ¿Un cupo ya lleno debe bloquear nuevas postulaciones o solo el otorgamiento posterior (RF-09/RF-10)? La relación entre el cupo del ciclo y el del programa ya está resuelta (nuevos del ciclo frente a becarios totales), pero el efecto de agotar el cupo sobre la postulación sigue sin definirse.
- ¿El cupo del ciclo se reparte por universidad, o es un cupo único del que solo se cierra la entrada de las universidades que ya iniciaron sus inscripciones? C4 habla de "cerrar las disponibilidades" de una universidad sin precisar si hay una disponibilidad por universidad.
- ¿Cuáles son los criterios generales de elegibilidad aplicables a todos los programas, y el umbral de ingreso es un máximo o un mínimo? La respuesta confirma que los define el **coordinador de programa**, pero no dice cuál es esa lista ni el sentido del umbral.
- ¿Qué valores de cupo son válidos? El PRD exige indicar el cupo pero no define un mínimo, por lo que esta HU no fija que cero o un negativo sean inválidos.
- ¿Puede un ciclo pedir dos documentos requeridos con el mismo nombre, o deben ser únicos? El PRD solo dice que cada programa puede pedir un conjunto distinto; sin definición, el cálculo de "postulación completa" (HU-03.1) queda ambiguo.
- ¿Los documentos requeridos y los criterios de un ciclo pueden reutilizarse de un ciclo anterior del mismo programa?
- ¿Se divide esta historia (ventana+cupo / criterios / documentos) para planeación, o se mantiene unificada? Decisión de alcance y estimación.
- ¿Qué ve el postulante en una convocatoria que no tiene criterios de elegibilidad adicionales: una leyenda explícita, la sección vacía, o la sección no se muestra? El PRD no lo define y el Escenario 5 ya no fija ningún texto. Si la respuesta es una leyenda, su escenario corresponde a HU-02.2, dueña de la vista de la convocatoria publicada. *(Pregunta abierta de la Fase 2.)*

## HU-02.2 Publicar la convocatoria y aceptar postulaciones solo dentro de su ventana

- ¿La ventana abre a las 00:00 UTC-4 del día de apertura? La respuesta fija el huso (UTC-4) y la inclusión del día de cierre hasta las 23:59, pero no dice nada del instante de apertura.
- ¿Qué mensaje ve un postulante cuya universidad ya inició sus inscripciones? No puede ser el de cierre de la convocatoria, porque la ventana común sigue abierta; el mensaje no está definido y el Escenario 12 no lo fija.
- ¿Qué ocurre si la universidad del postulante no tiene registrada su fecha de inicio de inscripciones para ese periodo: se aplica solo la ventana común o se bloquea la postulación?
- ¿Qué pasa con las postulaciones **ya recibidas** de una universidad cuando esa universidad inicia sus inscripciones: siguen su curso normal hasta el otorgamiento, o también se cierran? C4 habla de cerrar "las disponibilidades", sin precisar el efecto sobre lo ya recibido.
- ¿La convocatoria publicada debe mostrar al postulante la fecha de inicio de inscripciones de su universidad, además de las fechas del ciclo? El Escenario 1 enumera lo que se muestra y no la incluye.
- ¿Una convocatoria publicada puede despublicarse o modificar sus fechas una vez abierta, y qué ocurre con las postulaciones ya recibidas?
- ¿Un cupo ya agotado debe cerrar la ventana anticipadamente, o la ventana depende solo de las fechas? (Relacionado con la pregunta de cupo de HU-02.1 y con RF-09/RF-10.)

## HU-03.1 Cargar mis documentos de soporte y ver los que faltan

- ¿Qué tipos de archivo y qué tamaño máximo se aceptan por documento? El PRD no lo define.
- ¿Qué ocurre si llegan dos cargas casi simultáneas para el mismo documento requerido: gana la última, se conservan ambas versiones, o se rechaza la segunda? El PRD no lo define y el comportamiento no se fija aquí.
- ¿El postulante puede reemplazar por su cuenta un documento ya cargado que no ha sido rechazado, y hasta cuándo?
- ¿Las credenciales de la cuenta de Firebase existente de la Fundación estarán disponibles antes del inicio del desarrollo de esta HU? Es una dependencia operativa externa con impacto en el calendario.

## HU-03.2 Rechazar un documento con motivo y permitir que se vuelva a subir

- ¿Quién del equipo de la Fundación puede rechazar un documento: solo el coordinador de programa, también el comité evaluador, o un futuro rol de verificación documental? El PRD solo dice "el equipo"; esta HU asume el coordinador sin que el PRD lo exija.
- ¿Quién fija la fecha límite de resubida y coincide con el cierre del ciclo?
- ¿Qué ocurre con la postulación si la fecha límite vence sin resubida: cambia de estado automáticamente o requiere acción manual? Determina si se necesita un proceso en segundo plano y debe resolverse antes de estimar la historia.
- ¿Es válido rechazar un documento de una postulación que ya está *En verificación* o *En evaluación*, y qué pasa con ella en ese caso?
- ¿Se puede rechazar un documento más de una vez? Si sí, ¿cada nuevo rechazo fija una nueva fecha límite y con qué criterio? El PRD no limita el número de rechazos ni define el cálculo.

## HU-04.1 Verificar automáticamente la elegibilidad básica y mostrar el motivo

- ¿Cuál es la lista completa de "reglas básicas de elegibilidad" que debe aplicar la verificación automática? La respuesta confirma que los criterios generales los define el **coordinador de programa**, pero no dice cuál es esa lista.
- ¿Quién hace el **registro manual** del promedio cuando la universidad no entrega la información por API? La respuesta lo deja **por confirmar**.
- ¿Qué hace la verificación cuando la universidad todavía no ha entregado el promedio (ni por API ni manualmente) y el ciclo exige promedio mínimo: espera, deja la postulación en *En verificación*, o la resuelve como *No elegible*?
- ¿El umbral de ingreso es un máximo o un mínimo?
- Cuando se incumplen varias reglas a la vez, ¿el motivo mostrado debe listar todas las reglas incumplidas o basta con la primera detectada? El PRD solo exige que el postulante "pueda ver el motivo", por lo que esta HU no fija el comportamiento.
- Si el postulante corrige su perfil mientras su postulación está *No elegible*, ¿se re-verifica automáticamente, queda fija, o debe postular de nuevo? La postulación trabaja sobre una copia del perfil, pero si una actualización posterior afecta a la postulación ya enviada sigue **por confirmar**.
- ¿La verificación automática puede ejecutarse en segundo plano con un retardo perceptible para el postulante, o debe ser inmediata al cargar el último documento obligatorio? Con el promedio proveniente de un sistema externo, el retardo puede no depender solo de la plataforma.

## HU-04.2 Anular un resultado de no elegible en un caso límite

- ¿Dirección de la Fundación también puede anular un resultado *No elegible*, dado que el PRD la describe como quien "resuelve excepciones que el coordinador no puede decidir solo", o esa facultad es exclusiva del coordinador de programa? Sin definición, un caso límite sin acuerdo del coordinador no tiene vía de escalamiento en esta HU. **Depende de la pregunta 2 (qué puede ver y hacer cada rol), que sigue pendiente de respuesta.**
- ¿Existe un límite de tiempo para anular un resultado (p. ej. antes de que cierre el periodo de evaluación del ciclo)?
- ¿Estará disponible el módulo de trazabilidad (HU-35.1) antes de desarrollar esta HU? Es la dependencia más fuerte del área y pertenece al Área 6.

## HU-05.1 Retirar mi postulación antes de que empiece la evaluación

- ¿Qué mensaje corresponde a un intento de retiro sobre una postulación *No elegible*, *Otorgada*, *Rechazada* o ya *Retirada*? El PRD define el límite solo en términos de "antes de que comience la evaluación" y no cubre estos estados, por lo que esta HU no fija un mensaje para ellos. El caso *No elegible* es particularmente ambiguo: nunca llegó a evaluación.
- ¿Un retiro puede revertirse si la ventana del ciclo sigue abierta? La respuesta confirma que se puede volver a postular tras un rechazo y tener varias postulaciones, pero no dice si una postulación *Retirada* se puede reactivar ni si el mismo ciclo admite una nueva postulación después de un retiro (ver la pregunta equivalente en HU-01.1).
- Los intentos de retiro sobre la postulación de otra persona (FA-4), ¿deben generar una alerta al equipo además del registro de trazabilidad?

## HU-06.1 Conformar el comité de un programa

- ¿Puede modificarse un comité ya conformado —agregar o quitar evaluadores, cambiar la jefatura— fuera del caso de pérdida de acceso por conflicto de interés (que ya cubre HU-06.2)? El PRD no lo define. Mientras no se defina, esta HU cubre solo la conformación inicial y no se fija ninguna regla sobre el reemplazo de la jefatura ni sobre el retiro de miembros.
- Si la modificación se permite, ¿qué ocurre con las postulaciones ya asignadas a un evaluador que sale del comité a mitad de ciclo?

## HU-06.2 Asignar los evaluadores de cada postulación

- ¿Qué ocurre con las puntuaciones ya registradas o enviadas por un evaluador que declara un conflicto de interés después de la asignación: se conservan para trazabilidad pero se excluyen de la consolidación, o se descartan? La respuesta 17 resolvió el reemplazo del evaluador pero dejó este punto explícitamente por confirmar. Afecta el modelo de datos de HU-08.1 y el cálculo de HU-09.1. El RNF de trazabilidad ("sin edición retroactiva silenciosa") sugiere conservarlas.
- Observación de alcance (INVEST, sin decisión tomada): esta HU reúne ahora cinco comportamientos (mínimo de dos evaluadores, excepción autorizada, bloqueo por conflicto previo, retiro de acceso por conflicto posterior y reasignación del lugar a otro evaluador del comité). Queda a decisión del equipo si se divide en "asignar respetando el mínimo y los conflictos" + "autorizar la excepción", y si el cuarto y el quinto comportamiento se reubican como historia derivada de RF-39. No se divide aquí para no alterar los IDs del plan.

## HU-07.1 Definir la rúbrica del programa con pesos que sumen 100 %

- ¿Con qué precisión se expresan los pesos de los criterios: enteros, o con decimales y cuántos? El PRD solo exige que sumen 100 %, por lo que no se fija aquí ninguna unidad mínima ni se incluye un escenario con pesos decimales.
- ¿Existe un número mínimo o máximo de criterios por rúbrica? El PRD no lo define.

## HU-07.2 Bloquear la rúbrica al comenzar la evaluación del ciclo

- ¿El bloqueo de la rúbrica es por ciclo o por programa completo cuando hay más de un ciclo abierto a la vez?

## HU-08.1 Puntuar, comentar y enviar mi evaluación

- ¿Puede un evaluador modificar o retractar una evaluación ya enviada mientras el periodo de revisión sigue abierto, y quién lo autoriza? El PRD no lo define, por lo que no se fija aquí ninguna regla ni escenario de bloqueo o reapertura. Si se permite, hay que definir si dispara un recálculo de la consolidación de HU-09.1.
- ¿Qué ocurre si una postulación asignada deja de estar *En evaluación* mientras un evaluador la tiene en borrador? El PRD impide el retiro voluntario una vez iniciada la evaluación (RF-05; ver HU-05.1), así que el caso solo puede llegar por otra vía no descrita en el PRD.

## HU-08.2 Ocultar las puntuaciones de otros evaluadores hasta el cierre

- Sigue abierto quién puede mover la fecha del periodo de revisión una vez fijada por **Dirección**.
- ¿"Cierre del periodo de revisión" es el mismo evento que "todos los evaluadores asignados enviaron su evaluación" (que dispara la consolidación en HU-09.1), o una acción manual separada que alguien ejecuta explícitamente aunque falten envíos? Define si basta un indicador en la postulación o el ciclo, o si se necesita una entidad y un permiso nuevos.
- ¿Puede el coordinador del programa, u otro rol interno, ver las puntuaciones individuales de un evaluador antes del cierre del periodo de revisión? RF-08 solo restringe la visibilidad "para otros evaluadores" y RF-37 no lo resuelve para el coordinador, por lo que no se fija aquí ninguna regla ni escenario al respecto.

## HU-09.1 Calcular la puntuación consolidada y marcar las diferencias grandes

- ¿Cómo se calcula el total ponderado de un evaluador a partir de sus puntuaciones por criterio, con pesos y rangos distintos por criterio? Es el dato de entrada de toda la consolidación y de la comparación de diferencias; debe resolverse junto con HU-07.1 y HU-08.1. **El cliente la mantiene pendiente** (respuesta 15).
- ¿El umbral de diferencia grande es inclusivo o exclusivo en su límite (una diferencia exactamente igual al umbral, ¿se marca?).
- ¿En qué consiste la revisión de desempate y cómo se mide la diferencia cuando hay más de dos evaluadores?
- Si una evaluación ya enviada pudiera corregirse (ver HU-08.1), ¿se recalcula la consolidación ya producida y queda registro de ambos valores?
- ¿Admite el registro de auditoría (HU-35.1) entradas cuyo autor es el sistema y que no tienen justificación? HU-35.1 fija como regla que "una decisión sin justificación no se aplica", pero la consolidación es un cálculo automático sin justificación humana. El Escenario 8 de esta HU y la regla de trazabilidad dependen de que ese registro acepte ese tipo de entrada; hay que resolverlo con el dueño de HU-35.1 antes de construir.

## HU-09.2 Generar el ranking respetando cupo y presupuesto

- ¿Puede el ranking regenerarse varias veces en el mismo ciclo y queda registro de cada generación, o la última sustituye a la anterior?

## HU-10.1 Resolver un empate o un caso límite con justificación

- ¿Debe el sistema impedir que el coordinador o la jefatura del comité resuelvan un empate o caso límite si tienen un conflicto de interés declarado (HU-39.1) con alguno de los postulantes en disputa? La respuesta 17 resolvió que el **evaluador** con conflicto no evalúa y que su lugar pasa a otro evaluador del comité, pero no dice si ese bloqueo alcanza a esta decisión ni quién decide en lugar de quien está bloqueado. **Prioridad alta de validación:** si la respuesta es sí, hay que agregar una regla, un flujo alterno y un escenario de bloqueo análogos a los de HU-06.2.
- ¿Qué ocurre si el coordinador y la jefatura del comité resuelven el mismo caso con decisiones distintas? El PRD no establece prelación entre ambos roles.

## HU-10.2 Dejar en lista de espera a los elegibles no otorgados

- ¿Qué hecho hace pasar al estado *Rechazada* una postulación que estaba en lista de espera? La respuesta del cliente cierra la espera al iniciar las inscripciones de la universidad, pero no dice si en ese momento la postulación cambia de estado o se queda *En evaluación*; ninguna HU usa hoy *Rechazada*.
- ¿La espera termina al comenzar el día de inicio de inscripciones de la universidad o al final de ese día? La regla de fechas límite (UTC-4, día de cierre incluido hasta las 23:59) está pensada para fechas de cierre, y aquí el hito es una fecha de inicio.
- ¿Debe el postulante en lista de espera ver su propia posición, o solo recibir la notificación cuando se libere un cupo (RF-29; ver HU-29.1)? El PRD no lo define, por lo que no se fija aquí ninguna regla sobre la visibilidad de su propio puesto.
- ¿Cómo se numera el puesto en la lista de espera: desde 1 entre los postulantes en espera, o heredando la posición del ranking? El Escenario 1 habla de "los puestos 4 y 5" del ranking y el Escenario 2 muestra la marca "Lista de espera: puesto 1" para el primero de esa lista. Ambos escenarios son correctos por separado, pero el dato visible depende de esta definición y la necesita HU-15.2 / HU-16.2 para considerar "al siguiente" cuando se libera un cupo.

## HU-11.1 Crear un programa de becas con su monto por periodo

- ¿Un programa puede existir sin periodo académico asignable (para pre-configurarlo con anticipación), o siempre debe haber al menos un periodo registrado antes de crear cualquier programa? Las historias asumen hoy lo segundo (FA-2).
- ¿Un programa puede crearse contra un periodo académico que ya no es el "actual" (periodo pasado), o solo sobre el periodo vigente? El PRD no lo prohíbe explícitamente.
- ¿El nombre debe ser único en la plataforma para los programas, o se permiten homónimos?
- ¿Un coordinador puede configurar un programa que no coordina? Depende de la pregunta 2 (permisos por rol), que sigue pendiente.
- ¿Qué opciones admite exactamente la modalidad de pago que fija el programa (RF-19 menciona mensual, por semestre y pago directo a la universidad) y es obligatoria al crear el programa? La respuesta solo dice que la fija el programa.

## HU-11.2 Configurar los donantes, la elegibilidad y la renovación del programa

- ¿Qué valores admite la duración máxima de una pausa (RF-27): solo valores positivos, puede registrarse como "sin límite", o puede ser cero con el significado de "este programa no permite pausas"? ¿Qué mensaje muestra el sistema con un valor inválido (por ejemplo, negativo)? *(La unidad —meses— ya queda resuelta por consistencia con HU-27.1.)*
- ¿Qué debe contener "condiciones de renovación" (RF-11) más allá de las condiciones de permanencia ya cubiertas? ¿Texto libre descriptivo o reglas evaluables automáticamente (p. ej. "requiere 2 periodos cursados")? Define si ese campo necesita estructura propia.
- ¿Qué ocurre si el coordinador edita la restricción de universidades o carreras de un programa cuando ya existen postulaciones "En evaluación" bajo la restricción anterior: la nueva restricción aplica retroactivamente o solo a las postulaciones recibidas después del cambio?
- ¿Un coordinador puede configurar un programa que no coordina? Depende de la pregunta 2 (permisos por rol), que sigue pendiente.
- **(Decisión arquitectónica, una sola vez)** ¿El "disponible" de presupuesto y cupo se expone como cálculo en vivo sobre aportes/otorgamientos/renovaciones, o se materializa como saldo por programa actualizado en cada operación? Debe decidirse una vez para que Área 2, Área 3B y Área 4 lo consuman igual (ver HU-12.1).
- **(Decisión de equipo, no del cliente)** El PO recomienda dividir esta HU en tres (donantes y presupuesto, elegibilidad, condiciones). Se mantiene unida para conservar los IDs y el orden del plan.

## HU-12.1 Consultar el presupuesto y el cupo disponibles y verificarlos antes de otorgar

- **(Decisión arquitectónica, una sola vez)** ¿El "disponible" de presupuesto y cupo se expone como cálculo en vivo sobre aportes/otorgamientos/ofertas/renovaciones, o se materializa como saldo por programa actualizado en cada operación? Debe decidirse una vez para que Área 2, Área 3B y Área 4 lo consuman igual, en vez de que cada área implemente su propia verificación.
- ¿El administrador financiero solo consulta el presupuesto y el cupo, o también puede intervenir bloqueando o aprobando un otorgamiento? El PRD solo le atribuye "registrar y controlar los desembolsos". Depende de la pregunta 2 (permisos por rol), que sigue pendiente.
- ¿Qué cuenta como "rechazo definitivo" del proceso para liberar el presupuesto que una oferta pendiente mantiene comprometido: solo el rechazo del postulante (HU-16.2), también la expiración de la oferta (HU-15.2), o algún otro cierre del caso? Las historias asumen hoy que el rechazo y la expiración liberan por igual, por consistencia con RF-16.
- **(Decisión de equipo, no del cliente)** El PO recomienda dividir esta HU en dos (consulta y verificación). Se mantiene unida para conservar los IDs y el orden del plan.

## HU-12.2 Contar las renovaciones pendientes como no disponibles

- **(Bloqueante)** ¿En qué momento queda fijado el monto comprometido de una renovación: al solicitarla (HU-26.1), cuando el coordinador le asigna el monto al estudiante, o se recalcula mientras está pendiente? C1 resuelve que el monto se asigna a cada estudiante y descarta el recálculo por tipo de cobertura, pero no dice cuándo se fija. Sin esto, RF-12 no se verifica de extremo a extremo y HU-12.2 no puede darse por "Lista".
- ¿Existe un plazo máximo para que una renovación quede "pendiente" antes de liberar el presupuesto que bloquea, para evitar que quede comprometido indefinidamente por una solicitud sin resolver?
- Si dos becarios solicitan renovación casi al mismo tiempo y la suma de sus montos comprometidos excede el disponible, ¿el sistema bloquea la segunda solicitud al crearla, o acepta ambas y deja que HU-26.2 resuelva cuál se aprueba? ¿Cómo se muestra el disponible en ese caso? Mientras no se defina, no hay escenario de aceptación para este caso.
- **(Decisión arquitectónica, una sola vez)** Cálculo en vivo del disponible o saldo materializado por programa (ver HU-12.1).

## HU-13.1 Registrar un donante

- ¿Quién da de alta y mantiene a los donantes: el coordinador de programa (como asume esta historia) o Dirección, dado que son relaciones institucionales transversales a varios programas y Dirección "aprueba configuraciones" según el PRD?
- ¿El nombre debe ser único en la plataforma para los donantes, o se permiten homónimos?
- ¿Un coordinador puede registrar o mantener donantes de programas que no coordina? Depende de la pregunta 2 (permisos por rol), que sigue pendiente.

## HU-13.2 Registrar los aportes de un donante y consultar su historial

- ¿Un aporte puede registrarse contra un periodo académico que ya no es el "actual" (periodo pasado), o solo sobre el periodo vigente? El PRD no lo prohíbe explícitamente.
- ¿Quién da de alta y mantiene a los donantes y sus aportes: el coordinador de programa (como asume esta historia) o Dirección?
- **(Decisión arquitectónica, una sola vez)** Cálculo en vivo del presupuesto total y del disponible, o saldo materializado por programa (ver HU-12.1).

## HU-14.1 Registrar una universidad aliada con su convenio y sus carreras elegibles

- ¿Puede el enlace de una universidad aliada actualizar sus propios datos de contacto, aunque no los términos del convenio (que esta historia reserva al coordinador)?
- ¿Quién crea la cuenta del enlace de universidad, a la que ahora se le exige verificación en dos pasos: el coordinador al registrar el contacto designado, Dirección, o la propia universidad? La respuesta 3 resuelve el segundo factor pero deja esto pendiente.
- ¿Quién hace el registro manual de las notas que envía el estudiante cuando el convenio no tiene API? La respuesta 13 lo deja por confirmar.
- ¿Quién da de alta y mantiene las universidades aliadas: el coordinador de programa (como asume esta historia) o Dirección, dado que son relaciones institucionales transversales a varios programas?
- ¿El nombre debe ser único en la plataforma para las universidades aliadas, o se permiten homónimos?
- ¿Un coordinador puede registrar o modificar universidades aliadas que usan programas que no coordina? Depende de la pregunta 2 (permisos por rol), que sigue pendiente.

## HU-14.3 Registrar la fecha de inicio de inscripciones de cada universidad por periodo

- ¿El cierre de disponibilidades ocurre al comenzar el día de inicio de inscripciones (00:00 UTC-4) o al terminarlo (23:59), como el día de cierre de un plazo?
- La respuesta 16 confirma que Dirección abre y cierra los periodos académicos (antes, "los responsables de la plataforma"), pero no dice si también es quien registra la fecha de inicio de inscripciones por universidad y periodo. ¿Quién la registra: el coordinador de programa (como asume esta historia) o Dirección?
- ¿Puede corregirse una fecha ya registrada, deben conservarse los valores anteriores (como con el costo de matrícula de HU-14.2) y puede corregirse después de que las inscripciones ya iniciaron?
- ¿Qué ocurre con una universidad aliada que no tiene fecha de inicio de inscripciones registrada en un periodo: sus disponibilidades siguen abiertas hasta el cierre del periodo de becas común?

## HU-15.1 Generar la oferta formal de beca y comunicarla con su fecha límite

1. ¿A qué nivel se configura la fecha límite de aceptación (por programa, por ciclo o por oferta) y se calcula automáticamente o la define el coordinador caso por caso? El instante de vencimiento ya no es parte de la duda: es 23:59 (UTC-4) del día límite.
2. ¿Es obligatorio que todo programa tenga condiciones de permanencia definidas antes de poder otorgar becas, o esa validación ya ocurre en la configuración del programa (HU-11.2, Área 3A)? Si ya es obligatoria allí, FA-1 queda redundante.
3. ¿El compromiso de presupuesto de una oferta pendiente cubre solo el monto del periodo o toda la duración de la beca? R11 confirma que la oferta pendiente resta presupuesto y C1 fija el monto por periodo dejando los montos siguientes para la renovación, pero no se define el alcance del compromiso.
4. ¿Debe impedirse generar dos ofertas pendientes simultáneas de programas marcados como "completos" para el mismo postulante, o el bloqueo ocurre solo al aceptar la segunda (regla de no doble beneficio, HU-01.2)?

## HU-15.2 Expirar una oferta sin respuesta y considerar al siguiente

1. ¿La expiración debe ejecutarse mediante una tarea programada que recorra periódicamente las ofertas pendientes, o basta evaluarla de forma perezosa la próxima vez que alguien consulte la oferta o el programa? La segunda opción puede dejar cupo sin liberar si nadie vuelve a consultar. La decisión afecta también a RF-26 y RF-30.
2. Si el siguiente de la lista de espera ya tiene una beca activa de un programa marcado como "completo", ¿el sistema debe omitirlo automáticamente o generarle la oferta igual y dejar que el bloqueo ocurra al aceptar (HU-16.1)?
3. ¿Existe una expiración manual de una oferta antes de su fecha límite? De existir, ¿qué rol puede ejecutarla? El PRD solo describe la expiración por vencimiento.

## HU-16.1 Aceptar la oferta y convertirme en becario

1. ¿Aceptar una oferta requiere el código de verificación de un solo uso (OTP), dado que cambia el estado legal de la beca, aunque el PRD solo liste como ejemplos desembolsar (RF-20), revocar (RF-22) y modificar datos de pago? R6 confirma que a un becario sí se le puede exigir un código (primer registro de datos de pago), pero ninguna respuesta dice si la aceptación lo exige.
2. ¿El segundo factor de autenticación de inicio de sesión debe extenderse al postulante/becario? R3 lo extiende al enlace de universidad, no al postulante ni al becario, y la pregunta 2 (permisos por rol) sigue pendiente.
3. La regla de no doble beneficio se evalúa "en programas distintos": ¿la verificación debe excluir las becas del mismo programa cuando ese programa permite acumulación (RF-01)?
4. ¿Cuál es el texto exacto del mensaje que ve el postulante cuando se le bloquea la aceptación por la regla de no doble beneficio? Debe definirse en HU-01.2 (que ya lo tiene como pregunta abierta) y aplicarse igual aquí, ahora con la terminología de programa "completo"; hasta entonces los Escenarios 3, 4, 10 y 12 verifican el bloqueo y el motivo, no la redacción.

## HU-16.2 Rechazar la oferta

- Ninguna propia. Las de la oferta (mecanismo de expiración, siguiente de la lista de espera) están en HU-15.2; las de OTP y segundo factor para el postulante, en HU-16.1.

## HU-17.1 Configurar el compromiso que exige un programa

1. ¿Qué pasa si el coordinador modifica las condiciones de un compromiso después de que algunos becarios ya lo firmaron con la versión anterior? ¿Deben volver a firmar, se conserva la versión que firmaron o se bloquea la edición mientras haya becarios con compromiso vigente? Relevante para el RNF de trazabilidad ("sin edición retroactiva silenciosa").
2. Si el coordinador quita "Exige compromiso" cuando hay becarios con firma pendiente, ¿el cambio aplica a esos becarios o solo a las ofertas generadas después del cambio?

## HU-17.2 Firmar mi compromiso de becario

1. ¿La firma electrónica (S7) requiere evidencia adicional —por ejemplo, registrar la versión exacta de las condiciones vigentes al firmar, para el caso en que HU-17.1 las edite después— con fines de auditoría externa (RF-36), o basta con persona y fecha?
2. Si las condiciones del compromiso cambian después de la firma, ¿el becario debe volver a firmar? (misma pregunta abierta que HU-17.1).
3. ¿Firmar el compromiso requiere OTP, dado que la beca pasa de *Otorgada* a *Activa*? (misma pregunta abierta que HU-16.1).

## HU-18.1 Consultar el historial de becas de una persona por programa o periodo

1. ¿Dirección o el equipo financiero necesitan acceso directo a este historial consolidado, o lo reciben únicamente a través de los reportes de RF-34 y RF-23? Si lo necesitan de forma directa, falta una historia equivalente con ese actor o una extensión de permisos. (Depende de la pregunta 2 del cliente, pendiente.)
2. ¿Un evaluador del comité necesita consultar alguna vez el historial de becas de un postulante, y en ese caso solo el de las postulaciones que tiene asignadas, o esa consulta es exclusiva de coordinador, finanzas y dirección? (Depende de la pregunta 2 del cliente, pendiente.)
3. La historia reúne tres modos de búsqueda (persona, programa, periodo). ¿Se mantiene como una sola HU o se divide en "historial por persona" y "historial por programa/periodo" si el equipo la considera demasiado grande para una iteración?

## HU-19.1 Generar el calendario de desembolsos de un becario sin superar lo aprobado

1. ¿Debe el sistema validar que la fecha de cada desembolso caiga dentro de un periodo académico definido (RF-38) y, en particular, dentro del periodo para el que la oferta aprobó el monto? Con C4, ese periodo es el de la universidad del becario, con sus propias fechas.
2. ¿Existe un monto mínimo válido por desembolso programado (por ejemplo, rechazar cero o negativos)? El PRD no fija ninguna validación de formato sobre el monto.
3. Ante una edición concurrente del mismo calendario, ¿el sistema bloquea por completo al segundo administrador o fusiona cambios no conflictivos (desembolsos de meses distintos)? El PRD exige consistencia pero no una estrategia de resolución.
4. ¿El administrador financiero gestiona los calendarios de todos los programas o solo de los que tenga asignados? (depende de la pregunta 2 del cliente, aún pendiente).

## HU-20.1 Registrar y confirmar un desembolso con un código de un solo uso

1. **Reenvío del código.** El reenvío se permite, pero sigue por confirmar si invalida el código anterior y si hay un límite de reenvíos o una espera mínima entre ellos.
2. **Destinatario frente a la modalidad del calendario.** ¿Puede un desembolso registrarse con un destinatario distinto al definido en la modalidad que fija el programa (por ejemplo, un pago excepcional a la universidad dentro de una modalidad mensual al becario), o la coincidencia es obligatoria? (ver HU-19.1).
3. **Periodo del desembolso.** ¿La fecha registrada determina a qué periodo se imputa el control de "no superar lo aprobado", y qué ocurre si cae fuera del periodo aprobado en la oferta?

## HU-20.2 Resolver un desembolso fallido o devuelto

1. **Formas de resolver un fallido o devuelto además de repagar.** ¿Existe la opción de anular el desembolso y devolver el monto al disponible del programa?
2. **Reenvío del código.** Se permite reenviar, pero sigue por confirmar si el reenvío invalida el código anterior y si hay límite o espera entre reenvíos (igual que en HU-20.1).
3. ¿Un desembolso ya resuelto puede volver a marcarse como fallido si el segundo intento de pago también falla, y cómo queda la secuencia en el historial?
4. **Desembolso fallido de una beca que quedó *Inactiva*.Confirmado:** *Inactiva* no es un estado final como *Revocada* o *Concluida*; la beca puede volver a *Activa* o pasar a *Concluida*. Sigue sin definirse si, mientras está *Inactiva*, puede resolverse con un nuevo pago el desembolso ya vencido de un periodo anterior, o si hay que esperar a que cambie de estado.

## HU-20.3 Registrar y modificar los datos de pago del becario con un código de un solo uso

1. **Auditoría de lectura.** El Escenario 6 prueba una decisión técnica del equipo, no una exigencia escrita del PRD: el PRD exige trazabilidad de "toda acción relevante" y decisiones (RF-35), no de las consultas. ¿La Fundación requiere auditar quién consulta los datos de pago y por cuánto tiempo se conserva ese registro? Si la respuesta es no, el Escenario 6 se retira.
2. **Campos de los datos de pago.** El cliente responde "los necesarios". ¿Confirma la lista propuesta —titular de la cuenta, cédula del titular, banco, tipo de cuenta, número de cuenta y, si aplica, datos de pago móvil (teléfono y banco)— y cuáles son obligatorios?
3. **Segundo factor del becario.** Al pasar la operación al becario, ¿exige la Fundación que el becario también verifique su identidad en dos pasos al iniciar sesión, o el código de un solo uso de esta operación es suficiente? La política de verificación en dos pasos del PRD nombra a los roles internos (coordinadores, finanzas, dirección, comité), no al becario.
4. **Reenvío del código.** Se permite reenviar, pero sigue por confirmar si invalida el código anterior y si hay límite o espera entre reenvíos (igual que en HU-20.1).

## HU-21.1 Suspender una beca con motivo y fecha de revisión, y notificar al becario

1. ¿Debe el sistema rechazar una fecha de revisión anterior a hoy? El PRD exige que exista una fecha de revisión o la marca de indefinida, pero no define su validez temporal. El "hoy" se evalúa en UTC-4, con el día completo hasta las 23:59.
2. **Destinatarios del aviso de la fecha de revisión.** El sistema avisa al coordinador; sigue por confirmar si también debe avisar al becario (ver HU-21.3).
3. ¿Puede la dirección de la Fundación suspender una beca de un programa que no coordina, como resolución de excepción? (depende de la pregunta 2 del cliente, aún pendiente; ver HU-37.1).

## HU-21.2 Reanudar una beca suspendida

1. **Reanudación antes de la fecha de revisión.** ¿Puede el coordinador reanudar antes de que llegue la fecha de revisión registrada, o esa fecha actúa como bloqueo mínimo? El PRD no define ninguno de los dos comportamientos, y el aviso de R18c no lo resuelve, por lo que ningún escenario lo fija.
2. ¿Existe un límite de suspensiones y reanudaciones sucesivas de una misma beca antes de escalar a revocación o a una decisión de dirección?
3. **Mecanismo de la reactivación desde *Inactiva*.Confirmado:** *Inactiva* no es un estado final; la beca puede volver a *Activa* o pasar a *Concluida*, que es el **coordinador de programa** quien decide ese cambio, y que lo decide cuando el becario sube los datos que le faltan. Sigue sin definirse si esta HU (reanudar una *Suspendida*) también cubre esa reactivación desde *Inactiva* o si es un flujo distinto de HU-27.2, y si FA-3 debe nombrar ese estado.
4. **Desembolsos vencidos durante una pausa.** R12 resuelve el caso de la suspensión; sigue por confirmar si la misma regla aplica a los desembolsos cuya fecha cayó dentro de una pausa (RF-27, HU-27.2).

## HU-21.3 Avisar al coordinador cuando llega la fecha de revisión de una suspensión

1. **¿El aviso llega también al becario?** El cliente confirma el aviso al coordinador y deja esa parte por confirmar.
2. **Momento del envío dentro del día.** R20 fija la zona (UTC-4) y que el día se cuenta completo, pero no a qué hora de ese día se dispara el aviso.
3. **Reiteración del aviso.** Si el coordinador no actúa, ¿el aviso se repite (y con qué periodicidad) o se envía una sola vez? ¿Debe existir también un recordatorio previo a la fecha, como en RF-30?
4. **Nueva fecha de revisión.** ¿Puede el coordinador, tras el aviso, dejar la beca *Suspendida* con una nueva fecha de revisión? Hoy una beca ya *Suspendida* no se vuelve a suspender (HU-21.1 FA-5) y el PRD no define una actualización de esa fecha.

## HU-22.1 Revocar una beca con motivo y código de un solo uso

1. **Alcance de "cualquier estado activo" para la revocación directa por incumplimiento grave.** ¿Qué estados abarca esa ruta directa además de *Activa*? **Decisión del cliente:** una beca *Inactiva* también se puede revocar; esta HU la trata como *Suspendida*, sin exigir la condición de incumplimiento grave. Sigue sin definirse si, además, *Inactiva* debería contar como "estado activo" para la revocación directa por incumplimiento grave, igual que *Activa*.
2. **Clasificación del motivo.** El incumplimiento de las condiciones de permanencia ya cuenta como incumplimiento grave; sigue sin definirse si el retiro de la universidad o la información falsa también lo son, o si requieren suspender primero.
3. **Reenvío del código.** Se permite reenviar, pero sigue por confirmar si invalida el código anterior y si hay límite o espera entre reenvíos; aquí es más crítico por ser una operación irreversible.
4. ¿Puede la dirección de la Fundación revocar una beca de un programa que no coordina, como resolución de excepción? (depende de la pregunta 2 del cliente, aún pendiente; ver HU-37.1).

## HU-22.2 Liberar el presupuesto y el cupo de una beca revocada

1. ¿El cupo liberado se ofrece automáticamente a la lista de espera (S2, RF-10, RF-16) o queda disponible para una decisión del coordinador? Esta HU solo lo deja disponible. Con C4, la lista de espera de una universidad termina al iniciar su periodo de inscripciones, así que el ofrecimiento automático solo tendría sentido antes de esa fecha.

## HU-23.1 Consultar y exportar el estado financiero de un programa

1. **Umbral de rendimiento durante una exportación de gran volumen.** ¿Qué tiempo de respuesta o grado de degradación máximo se considera aceptable en el resto de la plataforma mientras se genera el archivo? El PRD exige que el procesamiento en segundo plano no bloquee ni ralentice el resto de la plataforma, pero no fija un umbral medible, por lo que el Escenario 8 solo puede verificar la asincronía y no el rendimiento.
2. **Asignación de programas al rol financiero.** ¿El administrador financiero consulta el estado de todos los programas o solo de los que tenga asignados? (depende de la pregunta 2 del cliente, aún pendiente).
3. **Formato y persistencia del archivo exportado.** ¿Qué formato espera el donante y el archivo se conserva en la plataforma (y por tanto en el Firebase de la Fundación) o se descarga al vuelo?
4. ¿El reporte por programa debe desglosar aportes por donante cuando un programa tiene varios (RF-11, RF-13), o basta el total del programa?

## HU-24.1 Entregar el reporte académico del periodo, con alerta de entrega tardía

- ¿Puede el becario corregir o reenviar su reporte del mismo periodo después de enviarlo, o la entrega es definitiva? El PRD no lo define, por lo que esta HU no fija ninguna de las dos conductas.
- ¿Puede entregar reporte académico un becario cuya beca no está *Activa* (por ejemplo *Suspendida* por pausa, al reactivar la cual se exige verificar el estatus académico — HU-27.2)? El PRD no restringe la entrega a un estado de beca concreto.
- ¿Quién registra manualmente las notas que envía el estudiante (R13, "por confirmar")? Si es un rol de la Fundación distinto del enlace de universidad, hará falta una historia propia para ese flujo; mientras no se confirme, esta HU no fija el actor.

## HU-24.2 Registrar el reporte académico desde la universidad

- ¿Quién crea la cuenta del enlace de universidad aliada en la plataforma? El cliente ya confirmó que a este actor se le exige verificación en dos pasos (R3), pero no quién da el alta de su cuenta; sigue bloqueando las historias de permisos de RF-37 para este actor.
- Si el becario (HU-24.1), el enlace (HU-24.2) y la API de la universidad (HU-24.3) registran reportes distintos para el mismo periodo y la misma beca, ¿cuál prevalece, coexisten todos o el segundo se bloquea? El PRD no resuelve la prioridad entre las fuentes y C5 añade una tercera, por lo que esta HU no fija ninguna conducta.

## HU-24.3 Recibir por API el reporte académico que envía la universidad

- ¿Con qué mecanismo se autentica y autoriza el sistema académico de cada universidad (credenciales, certificados, quién las emite y las revoca)? C5 fija que la vía debe existir, no cómo se asegura.
- ¿Qué formato y qué frecuencia de envío se acuerdan con cada universidad (envío por becario o por lote, periodicidad, reintentos)?
- ¿Puede la universidad enviar reportes de personas que no son becarias activas de la Fundación (por ejemplo, una beca *Suspendida* o ya terminada), y qué debe hacer el sistema con ellos?
- Si la API y otra fuente (becario o enlace) registran reportes distintos para el mismo becario y periodo, ¿cuál prevalece? Es la misma pregunta abierta de HU-24.2, ampliada por esta vía.
- ¿Quién registra manualmente las notas que envía el estudiante, cuando la universidad no envía por API (R13, "por confirmar")? Mientras no se confirme el actor, esa variante no se modela como historia propia (ver HU-24.1).

## HU-25.1 Definir la política de consecuencias y evaluar el cumplimiento

- ¿Puede modificarse la política de consecuencias de un programa una vez que ya se aplicó a evaluaciones en curso? El PRD sí lo prohíbe para la rúbrica de evaluación una vez iniciado el ciclo (RF-07), pero no declara un principio equivalente aquí.
- Si se retira una consecuencia de la política mientras hay evaluaciones "No cumple" pendientes de resolver con esa consecuencia, ¿qué ocurre con esos casos pendientes? (ver HU-25.2).
- ¿Cómo está estructurada la política de consecuencias: por condición incumplida, por gravedad o por reincidencia?

## HU-25.2 Aplicar la consecuencia por incumplimiento e informarla al becario

- ¿Puede aplicarse más de una consecuencia sobre el mismo incumplimiento, o la primera es definitiva? RF-25 habla de "una consecuencia" sin resolver el caso de una segunda aplicación.
- ¿Qué parte del detalle de una decisión ve el becario? La justificación interna del coordinador podría contener información no dirigida a él, lo que tensiona con el RNF de confidencialidad (ver HU-28.1). Sigue dependiendo de la pregunta 2 (permisos por rol), que continúa pendiente.
- ¿El reenvío del código de un solo uso invalida el código anterior? El cliente confirma que el reenvío existe, pero deja ese punto "por confirmar" (R4d); mientras no se defina, esta HU no fija la conducta.
- ¿El aviso de la fecha de revisión de una suspensión llega también al becario, además del coordinador? El cliente confirma el aviso al coordinador y deja el resto "por confirmar" (R18c).

## HU-26.1 Definir la ventana de renovación y solicitar la renovación dentro de ella

- ¿Puede un becario tener más de una solicitud de renovación para el mismo periodo, o la segunda debe bloquearse y mostrarse la existente? El PRD no lo define, por lo que esta HU no fija ninguna conducta.
- Si cada universidad define las fechas de su periodo (C4) y un programa tiene becarios de varias universidades, ¿la ventana de renovación sigue siendo una sola por programa y periodo, o debe fijarse por universidad para cerrar antes del inicio de cada periodo? C4 resuelve de quién son las fechas, no cómo convive esa variedad con una ventana única del programa.
- ¿Con qué monto se compromete el presupuesto de una solicitud de renovación pendiente, si el monto de la renovación se asigna a cada estudiante al resolverla (C1): con el del periodo anterior o con uno indicado al solicitar?

## HU-26.2 Evaluar la renovación y resolver las becas no renovadas

- ¿En qué consiste la "excepción definida por el programa": dónde se configura, qué casos cubre y qué ocurre después con la beca exceptuada?
- ¿En qué momento queda *Inactiva* la beca de quien no solicitó la renovación: al cerrar la ventana o al iniciar el nuevo periodo? Hoy el cambio depende de que el coordinador lo confirme.
- ¿Es intencional que las becas sin solicitud y las de renovación rechazada queden *Inactiva* en momentos distintos (confirmación del coordinador frente a S11, cambio de periodo)? Ya no afecta la liberación de presupuesto y cupo, porque una beca *Inactiva* no los libera en ningún caso; sigue abierta la asimetría del momento exacto de la transición.
- ¿Quién asigna el monto de la renovación a cada estudiante y en qué momento (el coordinador al aprobar, como asume esta HU, u otro rol antes de abrir la ventana)? C1 fija que el monto se asigna a cada estudiante, no quién lo hace.
- Cuando alguien intenta solicitar la renovación de una beca *Inactiva* (Escenario 11), el sistema usa la vía de HU-26.1 FA-4 ("Solo puedes solicitar la renovación con una beca activa."), no la de postular de nuevo: al no ser *Inactiva* un estado final, esa vía queda reservada a *Concluida* y *Revocada*. El texto exacto que muestra esta HU sigue sin fijarse.

## HU-27.1 Solicitar y resolver una pausa de la beca

- Ninguna propia. El vencimiento de la pausa ya está resuelto (la beca queda *Inactiva*,, y mantiene ocupados su presupuesto y su cupo mientras lo esté) y lo que queda por confirmar sobre la pausa es solo los desembolsos cuya fecha venció durante ella (); el requisito y quién decide el paso de esa beca *Inactiva* a *Activa* o a *Concluida* ya están confirmados (el becario sube los datos que le falten y decide el coordinador de programa,) y se registran en HU-27.2 y HU-26.2.

## HU-27.2 Reactivar la beca tras verificar el estatus académico

- ¿Qué pasa con los desembolsos cuya fecha venció durante la pausa? El cliente fija que los desembolsos con fecha durante una **suspensión** se pagan al reanudar, pero deja "por confirmar" si eso aplica también a la pausa (R12), por lo que esta HU no fija la conducta.
- ¿Existe un flujo de reactivación para las becas *Suspendida* por un motivo distinto a "Pausa" (por ejemplo, incumplimiento académico), o desde ese estado solo se puede pasar a *Revocada*? La regla transversal permite *Suspendida* → *Activa*, pero el PRD no define quién lo decide ni con qué requisitos.
- ¿El aviso de la fecha de revisión llega también al becario, además del coordinador? El cliente lo deja "por confirmar" (R18c); la pregunta está registrada en HU-25.2 y aplica igual a la pausa.

## HU-28.1 Ver la línea de tiempo completa de una beca

- ¿El detalle de cada evento en la línea de tiempo del becario incluye las justificaciones internas que escribió el coordinador (por ejemplo, "Constancia médica recibida"), o solo la información dirigida al becario (condición incumplida, consecuencia, siguiente paso) como muestra HU-25.2? Afecta directamente al RNF de confidencialidad, por lo que esta HU no fija qué campos del detalle ve cada rol.
- ¿Tienen acceso a la línea de tiempo de una beca el enlace de universidad aliada, el administrador financiero y la dirección? Y si lo tienen, ¿ven los montos de desembolso o solo los eventos propios de su función (RF-23, RF-34)? Sigue dependiendo de la pregunta 2 (qué puede ver y hacer cada rol), que el cliente mantiene pendiente.

## HU-29.1 Recibir los hitos de mi postulación o beca por correo y por los canales del programa

- ¿Qué canales adicionales al correo se van a ofrecer para los hitos (SMS, push, mensajería)? RF-29 solo dice que pueden agregarse según el programa; que el SMS exista para los códigos no significa que se ofrezca como canal de notificación.
- Al desactivarse un canal adicional, ¿se avisa a quienes lo tenían activo de que dejarán de recibir notificaciones por ese medio, o simplemente dejan de llegar?
- ¿Quién además del coordinador del programa puede configurar los canales de notificación? Por ejemplo, la dirección en programas sin coordinador asignado. RF-37 habla de acceso por función pero no asigna esta pantalla a un rol concreto.
- ¿Un envío fallido se reintenta automáticamente, y con qué política, o queda para resolución manual de soporte? El PRD exige dejar rastro pero no define el reintento, por lo que esta HU solo fija el registro del resultado.
- ¿Puede el postulante o becario elegir por qué canales adicionales quiere recibir las notificaciones, o la elección es exclusivamente del programa? Existe un precedente a favor de la elección de la persona: el canal del código de verificación lo elige el usuario.
- ¿Hace falta un criterio de aceptación propio para el contenido mínimo necesario del mensaje —que un correo no incluya el ingreso declarado ni la composición familiar— o se verifica solo en pruebas de confidencialidad? El caso que se usaba de ejemplo, el motivo de la suspensión, ya está resuelto en sentido contrario: el correo sí lo incluye.
- ¿Por qué canal recibe un usuario interno los avisos automáticos del módulo (correo de trabajo, aviso dentro de la plataforma, ambos)? El canal mínimo de RF-29 está fijado para postulantes y becarios.

## HU-29.2 Enviar notificaciones a todo un ciclo sin ralentizar la plataforma

- ¿Qué tiempo de respuesta se considera aceptable para el resto de la plataforma mientras corre un envío masivo? RF-29 exige "no ralentizar" sin fijar un umbral medible.
- ¿Cuántos reintentos y con qué espaciado se hacen sobre un envío fallido antes de darlo por definitivamente no entregado, y quién ve esa lista?
- ¿Hay un volumen máximo de destinatarios por ciclo que la Fundación espere manejar? El PRD menciona "potencialmente miles" y un crecimiento a más de 25 programas, sin una cifra objetivo.

## HU-30.1 Configurar y recibir recordatorios antes de un vencimiento

- ¿La anticipación configurada es un valor único por programa para los tres tipos de plazo (documentos, renovación, reporte académico), o cada tipo puede tener la suya?
- ¿Existe una fecha límite de carga de los documentos obligatorios distinta del cierre de la convocatoria (RF-02), que ahora ocurre por universidad al iniciar sus inscripciones, o el recordatorio de documentos solo aplica a la fecha de reposición de un documento rechazado (RF-03)? Afecta directamente al alcance del Escenario 6.
- ¿A qué hora del día se envía el recordatorio dentro de su momento de anticipación? El instante de vencimiento del plazo ya está definido —23:59 (UTC-4) del día límite—, pero no el momento del envío: un recordatorio a 2 días del 20/02/2027 puede salir el 18/02 a las 00:00 o más tarde ese día.
- ¿Quién además del coordinador del programa puede configurar la anticipación de los recordatorios? Por ejemplo, la dirección en programas sin coordinador asignado.
- ¿Debe recibir recordatorio quien tiene el plazo pendiente pero su beca está *Suspendida* (incluida la pausa de RF-27)? El PRD no excluye ni incluye ese estado, que no es final.

## HU-31.1 Avisar al comité de nuevas asignaciones y del cierre de la evaluación

- ¿Por qué canal se avisa y se recuerda al evaluador? RF-31 no lo indica y el canal mínimo de RF-29 está definido para postulantes y becarios. Del evaluador externo se sabe que la plataforma lo alcanza por correo para el segundo factor, lo que lo hace el candidato natural, pero el cliente no lo dijo del aviso ni del recordatorio.
- ¿Quién configura la anticipación del recordatorio de cierre del periodo de evaluación, y con qué valor? ¿Es la misma configuración de recordatorios de RF-30 (HU-30.1) o una propia del comité? El cliente confirmó que es Dirección quien abre y cierra los periodos, no el comité ni el evaluador, pero no dijo quién configura esta anticipación.
- ¿Un evaluador que ya envió todas sus evaluaciones del ciclo sigue recibiendo el recordatorio de cierre?
- ¿Se avisa al evaluador cuando se le quita una asignación o cuando se lo retira del comité, o simplemente deja de verla en su lista? El caso del conflicto de interés ya tiene una parte resuelta: la postulación pasa a otro evaluador del comité, pero el cliente no dice si se avisa al que la pierde.

## HU-32.1 Enviar una consulta sobre mi postulación o beca

- ¿Se permite enviar una consulta asociada a una postulación ya cerrada (*Retirada*, *Rechazada*, *No elegible*) o a una beca en estado final (*Concluida*, *Revocada*)? RF-32 no lo restringe ni lo habilita.
- ¿Hay un límite de longitud o de formato para el mensaje de una consulta, y puede adjuntarse un archivo? RF-32 no lo define; si se admitieran adjuntos, habría que decidir si van al almacenamiento Firebase de documentos.
- ¿Puede el postulante o becario editar o eliminar una consulta ya enviada?
- ¿Las consultas generan notificación al coordinador por los canales de RF-29, o solo aparecen en su lista de consultas (HU-32.2)?
- ¿Cuál es el texto exacto del aviso cuando se intenta enviar una consulta vacía? HU-32.2 sí fija "Escribe una respuesta antes de enviarla." para la respuesta vacía, mientras que aquí FA-2 solo dice "pide escribir el mensaje". Conviene definir el texto para que la validación sea verificable palabra por palabra, como las demás.

## HU-32.2 Responder y dar seguimiento a una consulta

- ¿Puede el postulante o becario cerrar su propia consulta?
- ¿Una consulta respondida o cerrada vuelve a abierta cuando la persona escribe de nuevo, o se abre una consulta nueva?
- ¿Qué debe ver el segundo coordinador cuando otro acaba de responder o cerrar la misma consulta: un aviso de que el caso cambió, o se acepta su respuesta sin más? El PRD exige que no haya datos contradictorios, pero no define la conducta visible.
- ¿Además del coordinador del programa, quién del equipo de la Fundación puede responder o cerrar consultas (dirección, finanzas, un rol de soporte)? RF-32 dice "el equipo de la Fundación" sin precisar el rol.
- ¿El historial de una consulta debe conservarse como registro no editable ni eliminable, al modo de RF-35, o admite depuración? El PRD no lo mandata para mensajería.

## HU-33.1 Generar el reporte de impacto de un donante con datos agregados o anonimizados

1. ¿Cada cuánto se emite el reporte de impacto de un donante?
2. ¿Qué datos componen el "perfil general" de los becarios en el reporte?
3. Si un programa tiene varios donantes, ¿el reporte de uno muestra las cifras de todo el programa o solo la porción correspondiente a su aporte? De esto depende si el reporte debe además impedir inferir el monto aportado por los otros donantes del mismo programa.
4. ¿Qué datos del becario se muestran cuando aceptó ser presentado de forma individual (incluyen datos socioeconómicos)?
5. ¿Quién genera el reporte cuando un donante financia programas de coordinadores distintos: cada coordinador genera su parte, o solo Dirección ve el consolidado completo? Si la respuesta es Dirección, el actor de esta historia cambia y conviene separar el caso multi-coordinador.
6. ¿La verificación en dos pasos se valida una vez por sesión al iniciar sesión, o debe revalidarse al entrar a cada pantalla con datos financieros o de decisión?
7. ¿Debe conservarse una copia del reporte generado para re-entregarlo al donante, o basta con generarlo y compartirlo en el momento?

## HU-33.2 Registrar el consentimiento para ser presentado a mi donante

1. ¿Puede el becario retirar su aceptación de ser presentado de forma individual? Si puede, ¿desde cuándo aplica: solo a reportes futuros, o también obliga a corregir un reporte ya entregado al donante?
2. ¿La aceptación vale para todos los donantes del becario o se registra por donante o por programa? La respuesta cambia la cardinalidad de la entidad de consentimiento, por lo que conviene resolverla antes de modelar.

## HU-34.1 Consultar el reporte consolidado institucional por periodo, universidad y carrera

1. ¿"Becario activo" en este reporte usa la definición amplia de S6 (Otorgada + Activa + Suspendida + Renovada) o la estricta que usan hoy los escenarios (solo *Activa*)? Son dos definiciones homónimas con alcance distinto dentro del mismo plan y cambian la cifra que ve Dirección. La respuesta debe darse sobre el S6 ya reformulado por la nueva regla de no doble beneficio y con el estado *Inactiva* incorporado a la tabla de estados.
2. ¿El "total de postulantes" cuenta personas (deduplicadas por número de cédula) o postulaciones? La distinción ya no es teórica: una persona puede enviar varias postulaciones y volver a postular tras un rechazo.
3. ¿Cómo debe desglosarse por universidad y por carrera un presupuesto que se asigna por programa: prorrateado, o solo se desglosan los conteos de postulantes y becarios?
4. ¿Qué tamaño mínimo debe tener un grupo en un desglose (por carrera, universidad o periodo) para que la cifra agregada no permita identificar a una persona específica?
5. ¿"Becarios activos" cuenta personas o becas activas? Una persona puede tener dos becas simultáneas dentro del límite de la regla de no doble beneficio, así que las dos cifras pueden diferir en el mismo periodo.
6. Si el postulante actualiza su perfil mientras el periodo sigue abierto, ¿esa actualización cambia la postulación ya enviada y, con ella, el desglose por universidad y carrera del reporte? El cliente dejó este punto por confirmar.

## HU-35.1 Registrar cada decisión con quién, cuándo y por qué, sin edición posterior

1. ¿La trazabilidad se modela como esta HU única y transversal, invocada por cada historia de decisión, o como un criterio de aceptación repetido dentro de cada una? Cambia cómo se estima y se secuencia el trabajo.
2. Cuando quien declaró el conflicto de interés no es un miembro del comité sino el coordinador del programa o el administrador financiero, ¿quién queda habilitado para tomar la decisión en su lugar? Para el comité ya está resuelto: pasa a otro evaluador del comité.
3. ¿Prefiere el cliente reencuadrar la narrativa al actor que realmente decide (coordinador, comité o administrador financiero), manteniendo a Dirección como beneficiaria de la trazabilidad?
4. El paso de una beca a *Inactiva* no lo decide una persona: ¿debe generar un registro en la auditoría de decisiones y, si sí, con qué autor y con qué justificación? **Cerrada en parte:** *Inactiva* no es un estado final; la beca puede volver a *Activa* o pasar a *Concluida*, que sigue existiendo como estado final, y esa transición de salida la decide el coordinador de programa cuando el becario sube los datos que le falten —. Sigue abierto solo con qué autoría y justificación se registraría cualquiera de los dos cambios (la entrada a *Inactiva* o la salida).

## HU-35.2 Consultar el registro de decisiones

1. Además de Dirección, ¿puede el coordinador de programa consultar el registro de decisiones de los casos de su propio programa, o el administrador financiero los de su ámbito? Si la respuesta es sí, hace falta una historia hermana acotada a ese alcance antes de cerrar el backlog de RF-35.
2. ¿El becario o el postulante puede consultar las decisiones registradas sobre su propio caso, más allá de la línea de tiempo de RF-28?

## HU-36.1 Exportar el uso de los fondos y el registro de decisiones para una auditoría

1. ¿Qué formato debe tener la exportación y existe un contenido o una estructura que el ente regulador o el auditor exija? Debe conocerse antes de fijar el modelo del registro de decisiones, porque condiciona qué campos capturar desde el día uno: no se reconstruye retroactivamente lo que no se guardó.
2. ¿La exportación del uso de los fondos puede iniciarla también el administrador financiero, o es exclusiva de Dirección?
3. ¿Conviene dividir esta historia en dos entregables independientes —uso de los fondos (estado financiero y desembolsos) y registro de decisiones—? Tienen fuentes de datos y dueños distintos y podrían estimarse y liberarse por separado. Si se divide, confirmar si ambas conservan la prioridad *Podría tener*.
4. ¿Debe conservarse una copia del archivo exportado para re-entregarlo, o basta con generarlo y descargarlo en el momento?
5. ¿Qué nivel de detalle individual puede contener la exportación dirigida a un ente regulador o auditor externo: solo datos agregados o anonimizados, como el reporte al donante de RF-33, o la auditoría exige identificar a cada becario y sus montos? Es distinta de la pregunta de formato ya registrada: de esta depende si la exportación pasa por la capa de anonimización de HU-33.1 o necesita un camino propio con control de acceso reforzado, y hoy bloquea el único criterio de aceptación que no se pudo escribir en todo el área.

## HU-37.1 Crear usuarios internos y tipos de usuario con acceso limitado a su función

- El PRD no define la matriz completa de permisos de los cuatro roles base (coordinador, evaluador, finanzas, dirección): solo da tres ejemplos puntuales (RF-08, RF-37 y el RNF de confidencialidad). Se requiere la matriz para construir y probar el paso 8 del flujo.
- El PRD no define si un usuario o un tipo de usuario puede eliminarse o desactivarse (offboarding de personal, fin de participación de un evaluador externo), ni qué pasa con sus asignaciones, evaluaciones o decisiones ya registradas.
- El PRD no define si un tipo de usuario en uso puede eliminarse, ni qué ocurre con los usuarios que lo tienen asignado.
- El PRD no define si el cambio de permisos de un rol ya asignado aplica de inmediato a las sesiones abiertas o solo desde el siguiente inicio de sesión.
- El PRD no define si un tipo de usuario **sin** acceso a información financiera ni de decisión queda exento del segundo factor, o si este se exige a todo usuario interno.
- El PRD no define si un evaluador puede consultar postulaciones de programas en cuyo comité no participa.
- No se define quién crea la cuenta del enlace de universidad aliada, que no es personal de la Fundación y sí requiere verificación en dos pasos.
- **Nota de planificación (PO):** el PO propone dividir esta historia en gestión de tipos de usuario y alta de usuarios, y trasladar el enforcement de los Escenarios 4, 5 y 6 a las HU de dominio (HU-08.2, rúbricas de RF-07, confidencialidad). Aquí se conservan porque son consideraciones textuales de RF-37; la división requiere aprobación del equipo por implicar renumeración de IDs.

## HU-37.2 Iniciar sesión con verificación en dos pasos

- ¿Debe rechazarse un código del segundo factor **ya usado** antes de que venzan sus 15 minutos? Las respuestas fijan la vigencia, pero no el uso único.
- ¿El reenvío del código **invalida el anterior**? La respuesta 4d autoriza el reenvío y deja este punto por confirmar.
- Nota de seguridad de la respuesta 4e: si el correo sirve a la vez para recibir el segundo factor y para recuperarlo, quien controle el correo controla el segundo factor. Falta que el cliente acepte ese riesgo o defina otra verificación en la recuperación.

## HU-38.1 Registrar y consultar los periodos académicos

- El PRD habla de una "secuencia" de periodos pero no define cómo se determina su orden (orden de registro o criterio cronológico independiente del nombre). No está claro qué ocurre si Dirección registra "2025-2" después de "2026-1": ¿se reordena la secuencia o queda al final?
- El PRD no define si la consulta de periodos anteriores está abierta a todo el personal interno o solo a ciertos roles (RF-37 exige acceso limitado por función).
- El PRD no define si un periodo con programas, ciclos o renovaciones ligados puede editarse o eliminarse, ni qué pasaría con esas referencias históricas.
- Las respuestas fijan la zona UTC-4 y que el día de cierre llega hasta las 23:59, pero no a qué hora del día registrado inician las inscripciones de una universidad y se cierran sus disponibilidades.

## HU-38.2 Marcar el periodo actual sin interrumpir renovaciones

- El PRD no define qué ocurre si un coordinador decide una renovación (HU-26.2) en el mismo instante en que Dirección marca el nuevo periodo como actual, sobre la misma beca: ¿se bloquea la decisión, se aplica un orden determinista, o se acepta como caso no crítico? El RNF de consistencia bajo concurrencia exige que no haya datos contradictorios, pero no fija el criterio de resolución.
- No se define si las becas cuyo becario no solicitó la renovación a tiempo (RF-26) quedan *Inactiva* en este mismo disparador de cambio de periodo o en otro proceso, ni quién lo decide.
- El PRD no define si es válido marcar como actual un periodo anterior al vigente (retroceder), ni qué pasa con las becas y renovaciones que ya avanzaron bajo el periodo que se reemplaza.
- El PRD no liga el OTP de un solo uso a "cualquier cambio de estatus de beca", sino a desembolsar, revocar o modificar datos de pago. Conviene confirmar con el cliente si el cambio masivo de estatus de esta historia debe tratarse como cambio de estado legal sujeto a ese mismo control.
- La transición *Renovada* → *Activa* se apoya en el supuesto S4, que no está en la tabla de transiciones explícitas del PRD. Toda la mecánica de la historia depende de ese supuesto: debe validarse con el cliente antes de implementarla.
- El PRD no define qué recibe o ve la persona cuya solicitud de cambio de periodo actual no se aplicó cuando dos solicitudes compiten (mensaje en pantalla, reintento automático, aviso posterior, o nada). Mientras no se defina, el Escenario 6 solo verifica el estado final.

## HU-39.1 Declarar un conflicto de interés y restringir mi participación en ese caso

- El PRD no define si una declaración de conflicto de interés puede retirarse (un conflicto declarado por error bloquearía al evaluador de forma permanente).
- El PRD no define qué pasa con las puntuaciones que el evaluador envió antes de declarar el conflicto: ¿se anulan, se marcan o se mantienen en el ranking de RF-09? De la respuesta depende si el modelo de evaluación de RF-08 necesita un estado "anulada por conflicto". La respuesta 17 deja este punto explícitamente sin resolver.
- La respuesta 17 no define si la reasignación del lugar del evaluador es automática o la ejecuta el coordinador, ni cómo se elige entre varios evaluadores del comité disponibles.
- El PRD no define quién toma las decisiones sobre la postulación cuando el conflicto lo declara el coordinador del programa o la jefatura del comité (S12); si nadie queda con autoridad, se requiere un mecanismo de escalamiento que hoy no existe en ninguna historia.
- El PRD no define si "participar en la evaluación" incluye el solo hecho de abrir o ver la postulación, o únicamente puntuar o decidir. El flujo actual asume la segunda interpretación; es una decisión de negocio que debe confirmarse.
- La cabecera indica que esta historia integra HU-39.1 y HU-39.2 de la v2.0, pero no incorpora ningún mecanismo de retiro de la declaración. Confirmar si la fusión perdió contenido de la v2.0 o si HU-39.2 trataba otro tema.

## HU-40.1 Ver los indicadores clave actualizados sin recargar la página

- El PRD no define qué postulaciones cuentan como "postulantes activos" de un ciclo, cómo se calcula la "tasa de aprobación" ni qué hace que un becario esté "en riesgo de incumplimiento". Los escenarios solo verifican que el panel muestra esos indicadores; el valor y su actualización se verifican con el indicador de presupuesto ejecutado frente a disponible, el único con definición operacional completa.
- Mientras esas tres fórmulas no se definan, ¿el primer incremento entrega solo el indicador financiero o se construyen los cuatro con marcadores de posición? Afecta el alcance del sprint donde se planifique esta historia, y hoy la historia completa solo es estimable para el indicador financiero.
- El PRD no define cómo se agregan los indicadores para evitar que un grupo pequeño de becarios permita inferir información socioeconómica individual (RNF de confidencialidad, RF-33).
- El PRD no define si algún rol distinto de dirección puede consultar el panel, ni con qué alcance (por ejemplo, un coordinador limitado a sus programas).
