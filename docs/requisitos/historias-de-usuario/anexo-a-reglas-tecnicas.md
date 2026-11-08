# Anexo A — Reglas técnicas

Reglas que la revisión de arquitectura marcó como técnicas. Son decisiones de diseño propuestas por el equipo, no requisitos escritos del PRD: deben confirmarse en el diseño técnico. Se comprueban con pruebas técnicas (concurrencia, carga, seguridad, revisión de configuración), no con los escenarios funcionales de las historias.

## HU-01.1 Crear mi perfil de postulante y postular a una convocatoria abierta

- **Técnica — la copia del perfil es un dato propio de la postulación:** el modelo debe guardar los valores usados al postular junto a la postulación (no una referencia al perfil vivo), porque de esa copia dependen la verificación automática (HU-04.1), la evaluación (RF-08/RF-09) y la trazabilidad (RF-35).

## HU-01.2 Impedir postular con una beca activa en el mismo periodo

- **Técnica — entradas del tope de la suma:** la capa 3 necesita dos datos que esta HU no produce: el monto por periodo de cada beca activa de la persona y el de la beca a la que postula (HU-11.1), y el límite del periodo, cuyo valor propuesto es el costo de matrícula de referencia de su universidad (HU-14.2). El límite debe leerse del valor vigente para ese periodo, no del último registrado.

## HU-03.2 Rechazar un documento con motivo y permitir que se vuelva a subir

- **Técnica — la comparación de la fecha límite usa el reloj del servidor en UTC-4**, igual que la ventana de postulación (HU-02.2), y conviene resolverla con la misma función de dominio para que el criterio de inclusión del día límite no quede implementado dos veces.

## HU-06.2 Asignar los evaluadores de cada postulación

- **Técnica — el acceso no se cachea:** el acceso del evaluador a una postulación se calcula en el momento de abrirla, cruzando la asignación vigente con los conflictos declarados por ese evaluador contra ese postulante (HU-39.1). No se persiste como permiso otorgado, de modo que el retiro de acceso por un conflicto posterior es consecuencia automática de la consulta y no requiere ningún evento ni proceso diferido (RNF de confidencialidad).
- **Técnica — consistencia bajo concurrencia:** la verificación de "al menos dos evaluadores o excepción autorizada" se hace dentro de la misma transacción que confirma la asignación, releyendo el conteo justo antes de confirmar, para que dos asignaciones simultáneas sobre la misma postulación no dejen el mínimo mal evaluado (RNF de consistencia bajo concurrencia).

## HU-07.1 Definir la rúbrica del programa con pesos que sumen 100 %

- **Técnica — autorización en el servidor:** las restricciones de rol (finanzas no modifica rúbricas; coordinador solo sus programas) se verifican en el backend, no solo ocultando la opción en la interfaz (RF-37).
- **Técnica — modelo extensible:** los criterios se modelan como una colección por programa (nombre, peso, rango mínimo, rango máximo), no como columnas fijas, porque cada programa define los suyos y la Fundación pasará de 12 a más de 25 programas (RNF de escalabilidad de uso).
- **Técnica — aritmética exacta:** la suma de pesos se valida con un tipo decimal exacto, no con punto flotante, para no rechazar sumas que sí son 100 % (RF-07).

## HU-07.2 Bloquear la rúbrica al comenzar la evaluación del ciclo

- **Técnica — un único punto de bloqueo transaccional:** la comprobación de "¿la rúbrica todavía se puede editar?" y la escritura de una puntuación deben compartir el mismo punto de bloqueo transaccional. Si se evalúan por separado, una edición de la rúbrica y el primer registro de puntuación pueden entrelazarse y dejar evaluadores del mismo ciclo puntuando contra versiones distintas de la rúbrica (RNF de consistencia bajo concurrencia).

## HU-08.1 Puntuar, comentar y enviar mi evaluación

- **Técnica — modelo de datos:** una evaluación por par (evaluador, postulación), con estado borrador o enviada, y una puntuación por criterio. Al estar cada evaluación en su propio registro, el envío simultáneo de dos evaluadores no comparte estado mutable y no requiere candados adicionales en esta HU: el riesgo real de concurrencia vive en el disparador de consolidación (HU-09.1) (RNF de consistencia bajo concurrencia).
- **Técnica — validación contra la rúbrica vigente:** la comprobación de "todos los criterios puntuados" se revalida en el servidor contra la rúbrica vigente al momento de enviar, no contra la que el formulario cargó al abrir (RF-08; ver HU-07.2).

## HU-08.2 Ocultar las puntuaciones de otros evaluadores hasta el cierre

- **Técnica — filtrado en el servidor:** el ocultamiento se aplica en la capa de servidor o API. La respuesta de la postulación no incluye las puntuaciones de otros evaluadores mientras el periodo está abierto; no basta con ocultarlas en la interfaz, porque eso es evadible inspeccionando la respuesta de red (RNF de confidencialidad).
- **Técnica — acceso directo:** la restricción se aplica por igual al acceso directo por enlace o identificador de la evaluación de otro evaluador, no solo a la navegación por la interfaz (RNF de confidencialidad).
- **Técnica — un único reloj:** la comparación "¿el periodo sigue abierto?" se resuelve en el servidor con la hora en UTC-4, no con la hora local del navegador de quien consulta, para que dos evaluadores en zonas distintas vean lo mismo.

## HU-09.1 Calcular la puntuación consolidada y marcar las diferencias grandes

- **Técnica — el umbral vive en el programa:** el umbral de diferencia grande es un atributo del programa, no una constante global, porque distintos programas pueden usar escalas de puntuación distintas (RF-09; mismo patrón extensible que la rúbrica de HU-07.1).
- **Técnica — disparador idempotente:** la consolidación se dispara al final de cada envío de evaluación, verificando dentro de la misma transacción si ya se cumple la condición de "todos enviaron", y la función de consolidación es idempotente. Así, dos envíos casi simultáneos no dejan la postulación sin consolidar ni producen un resultado distinto si el cálculo corre más de una vez (RNF de consistencia bajo concurrencia; es el ejemplo que da el propio PRD para este RNF).

## HU-09.2 Generar el ranking respetando cupo y presupuesto

- **Técnica — una sola fuente de verdad de disponibilidad:** el cupo de la convocatoria y el presupuesto disponible —ya descontadas las ofertas pendientes— se consultan a HU-12.1 en el momento de generar el ranking. No se replica ni se recalcula en Área 2, para que el ranking no marque como "dentro del cupo" a alguien que al momento de otorgar (HU-15.1) ya no cabe (RNF de consistencia bajo concurrencia).
- **Técnica — foto del momento revalidada:** el ranking es una foto del momento en que se genera. Si el presupuesto disponible cambia en paralelo, la selección se revalida al confirmarse (ver HU-10.1), no se asume vigente la foto (RNF de consistencia bajo concurrencia).
- **Técnica — acción puntual:** generar el ranking es una acción puntual a pedido del coordinador, no una vista recalculada de forma continua; una consulta ordenada e indexada por puntuación consolidada basta y no requiere procesamiento en segundo plano (RNF de escalabilidad de uso).

## HU-10.1 Resolver un empate o un caso límite con justificación

- **Técnica — autorización combinada:** la verificación de permiso resuelve en un solo paso dos fuentes de rol distintas: ser coordinador de ese programa (rol sobre el programa) o ser la jefatura de ese comité (atributo dentro de la composición del comité, HU-06.1). No son dos funcionalidades separadas que llegan al mismo botón (RF-10, RF-37).
- **Técnica — revalidación en la confirmación:** el cupo de la convocatoria y el presupuesto disponible —con las ofertas pendientes descontadas— se releen dentro de la misma transacción que confirma la decisión, no se usan los valores mostrados al abrir el caso (RNF de consistencia bajo concurrencia; fuente única de disponibilidad en HU-12.1).

## HU-10.2 Dejar en lista de espera a los elegibles no otorgados

- **Técnica — la marca no es un estado:** se implementa como atributos adicionales de la postulación (marca de lista de espera y puesto), no como un valor nuevo del conjunto de estados de la postulación, para no alterar la máquina de estados del PRD (S2; transiciones en HU-04.1).
- **Técnica — el puesto se fija al confirmar:** el puesto se persiste en el momento en que se confirma la lista de espera y no se recalcula en cada consulta. Si se recalculara en vivo desde el ranking, un cambio posterior no relacionado podría reordenar la lista sin que nadie tomara esa decisión (RNF de trazabilidad, "sin edición retroactiva silenciosa").
- **Técnica — cierre por fecha, no manual:** la espera ya no es indefinida. El sistema necesita conocer, para cada postulación en espera, la fecha de inicio de inscripciones de la universidad de ese postulante en ese periodo, y dar por terminada la marca al llegar esa fecha, sin que nadie la cierre a mano.

## HU-11.1 Crear un programa de becas con su monto por periodo

- **Técnica — 2FA de inicio de sesión:** el coordinador accede con segundo factor además de su contraseña (Restricciones Técnicas del Cliente).
- **Técnica — monto obligatorio y etiqueta como enum cerrado de dos valores:** el monto por periodo es un campo obligatorio del programa, y "completo"/"parcial" se modela como un valor único de dos posibles, independiente del monto, no como un indicador que pueda quedar vacío o marcado dos veces. El enum de tres coberturas desaparece.
- **Técnica — dependencia bloqueante de Área 7:** sin el módulo de periodos académicos (HU-38.1) esta historia no puede probarse de punta a punta, aunque el CRUD sea simple.
- **Técnica — contrato hacia adelante:** el monto por periodo y la modalidad de pago son los datos que HU-19.1 (Área 4) traduce a un calendario de desembolsos y que HU-09.2/HU-15.1 usan como monto de la beca; la etiqueta "completo"/"parcial" es el dato que consume la regla de no doble beneficio (HU-01.2). Su esquema debe cerrarse antes de construir el formulario.

## HU-11.2 Configurar los donantes, la elegibilidad y la renovación del programa

- **Técnica — 2FA de inicio de sesión:** aplica al coordinador.
- **Técnica — tres sub-modelos, no una tabla ancha:** (a) la relación programa–donante–aporte vive en HU-13.2 ya versionada por periodo; (b) la restricción es una relación de dos niveles `programa–universidad–carrera`, donde la carrera vacía representa "restringido solo a nivel de universidad" (ver FA-4); (c) las condiciones de permanencia son campos estructurados del programa, no texto libre (salvo la descripción de renovación).
- **Técnica — no modelar la configuración como JSON suelto:** HU-04.1 consulta la restricción de universidad/carrera por cada postulación y HU-25.1 evalúa el promedio mínimo por cada reporte académico; ambas necesitan consultar estos datos de forma estructurada.
- **Técnica — consistencia con HU-12.1:** el presupuesto total que esta historia muestra y el disponible que calcula HU-12.1 leen la misma cifra; la decisión de calcularla al vuelo o materializarla debe tomarse una sola vez para ambas.

## HU-12.1 Consultar el presupuesto y el cupo disponibles y verificarlos antes de otorgar

- **Técnica — 2FA de inicio de sesión:** aplica tanto al coordinador como al administrador financiero (ambos están en la lista de "acceso a información financiera o de decisión" de las Restricciones Técnicas del Cliente).
- **Técnica — la verificación debe ser atómica:** `disponible = SUM(aportes) − SUM(becas otorgadas) − SUM(comprometido en ofertas pendientes) − SUM(comprometido en renovaciones pendientes)` se lee de cuatro fuentes. Un "leer–decidir–escribir" en pasos separados es insuficiente; verificar y comprometer deben ocurrir en la misma transacción, con bloqueo o versionado de la fila del programa.
- **Técnica — un único servicio transaccional, no lógica embebida en la pantalla de otorgamiento:** la misma pieza debe exponer la verificación y la liberación (rechazo o expiración de oferta en HU-15.2/HU-16.2, revocación en HU-22.2), y la consumen Área 2 (HU-09.2, HU-10.1), Área 3B y Área 4 sin reimplementarla.
- **Técnica — la fórmula debe prever desde el inicio sus cuatro términos** (lo comprometido en ofertas pendientes y lo comprometido en renovaciones pendientes, HU-12.2), no agregarlos después como parche.
- **Técnica — disponibilidad en periodos críticos:** esta verificación se invoca en cada otorgamiento durante las ventanas de evaluación, las más cargadas del sistema; el diseño de bloqueo elegido tiene impacto directo en la duración del lock bajo carga concurrente (RNF de disponibilidad en periodos críticos).

## HU-12.2 Contar las renovaciones pendientes como no disponibles

- **Técnica — 2FA de inicio de sesión:** aplica al administrador financiero.
- **Técnica — extensión de la fórmula de HU-12.1, no un cálculo aparte:** este monto es uno de los términos del disponible (junto con lo comprometido en ofertas pendientes) y comparte el mismo punto de bloqueo transaccional. Una renovación que pasa de pendiente a rechazada (liberando su monto) y un otorgamiento que verifica disponible al mismo tiempo deben pasar por el mismo candado, no por candados independientes (RNF de consistencia bajo concurrencia).
- **Técnica — recomendación de trazabilidad:** el monto comprometido debería fijarse como *snapshot* al momento de solicitar la renovación (HU-26.1) y no recalcularse mientras está pendiente; un recálculo en vivo movería silenciosamente el presupuesto comprometido de una renovación que nadie volvió a tocar (RNF de trazabilidad: sin edición retroactiva silenciosa). C1 refuerza esta recomendación: al asignarse el monto a cada estudiante, deja de existir una fórmula de la que recalcularlo. El momento exacto en que se fija sigue abierto (ver Preguntas abiertas).
- **Técnica — dependencia cruzada bloqueante:** el contrato de datos de "monto comprometido de una renovación pendiente" lo define HU-26.1 (Área 5), no esta área; la historia no puede cerrarse sin coordinar con quien la construya.

## HU-13.1 Registrar un donante

- **Técnica — 2FA de inicio de sesión:** aplica al coordinador que registra, no al donante, que no tiene acceso.
- **Técnica — el tipo es un atributo, no dos entidades:** organización y persona se modelan como un atributo del donante, porque HU-13.2 y HU-11.2 los tratan exactamente igual y ninguna regla posterior distingue por tipo.
- **Técnica — no crear cuenta ni rol al registrar un donante**, ni siquiera inactivo (consistente con "Fuera de alcance: portal de donantes").
- **Técnica — sin dependencias:** junto con HU-14.1, es la historia del área que puede construirse primero, sin esperar al calendario académico de Área 7.

## HU-13.2 Registrar los aportes de un donante y consultar su historial

- **Técnica — 2FA de inicio de sesión:** aplica al coordinador y a Dirección.
- **Técnica — ledger append-only:** el modelo correcto es una fila inmutable por aporte (`donante, programa, periodo, monto, fecha de registro`), nunca una fila mutable de "aporte actual por donante y programa" que se sobrescriba; el Escenario 4 exige que dos aportes de distintos periodos del mismo donante al mismo programa coexistan y se consulten ambos.
- **Técnica — fuente de verdad del presupuesto:** esta tabla es la fuente de la que dependen el presupuesto total de cada programa (S8) y todo el cálculo de HU-12.1; la decisión de leerla con una suma en cada consulta o de materializar el total debe acordarse una sola vez junto con HU-12.1, no por separado en cada historia.
- **Técnica — trazabilidad:** "ningún aporte registrado se pierde del historial" es el RNF de trazabilidad del PRD ("sin posibilidad de edición retroactiva silenciosa") aplicado a datos financieros del programa.
- **Técnica — índice por `(donante, periodo)`** para el filtro del paso 8; sin riesgo de desempeño previsible a la escala del PRD (de 12 a 25+ programas).
- **Técnica — dependencia bloqueante de Área 7:** cada aporte se registra contra un periodo ya definido (HU-38.1).

## HU-14.1 Registrar una universidad aliada con su convenio y sus carreras elegibles

- **Técnica — 2FA de inicio de sesión:** aplica al coordinador.
- **Técnica — 2FA del enlace de universidad:** al enlace designado también se le exige verificación en dos pasos además de su contraseña; queda por definir quién crea su cuenta (ver Preguntas abiertas).
- **Técnica — subentidad propia:** las carreras elegibles se modelan como tabla hija de la universidad, aisladas por universidad (ver Escenario 4), no como un catálogo global compartido.
- **Técnica — contrato hacia adelante del origen de los datos académicos:** el dato del convenio deja de ser un indicador de sí/no y pasa a identificar la vía (API o registro manual); es lo que HU-24.2 (Área 5) usará para decidir el origen del reporte académico. Su nombre y sus valores deben fijarse ahora, porque otra área lo leerá sin poder renegociar su significado.
- **Técnica — sin dependencias:** junto con HU-13.1, es punto de partida del área. Es dependencia de entrada de HU-01.1 (Área 1), HU-11.2, HU-14.3 y HU-24.2 (Área 5).

## HU-14.2 Registrar el costo de matrícula por periodo sin perder los anteriores

- **Técnica — 2FA de inicio de sesión:** aplica al coordinador.
- **Técnica — una fila por corrección, no una fila por periodo:** el Escenario 2 exige que una corrección dentro del *mismo* periodo (2026-2: 1.000 → 1.050) conserve el valor anterior consultable. El modelo es `costo_matricula(universidad, periodo, costo, fecha de registro)` **sin** clave única sobre `(universidad, periodo)`, y el "costo vigente" se resuelve como el de fecha de registro más reciente. Nunca un `UPDATE` sobre la fila existente: con una sola fila mutable por `(universidad, periodo)`, el escenario de aceptación falla en la segunda corrección.
- **Técnica — mismo patrón append-only que HU-13.2:** ambas historias comparten criterio de implementación (filas inmutables) aunque vivan en tablas distintas; es el RNF de trazabilidad del PRD ("sin edición retroactiva silenciosa").
- **Técnica — dependencia bloqueante de Área 7:** requiere HU-14.1 (universidad) y HU-38.1 (periodos académicos).

## HU-14.3 Registrar la fecha de inicio de inscripciones de cada universidad por periodo

- **Técnica — 2FA de inicio de sesión:** aplica a quien registra la fecha, que accede a información de decisión (Restricciones Técnicas del Cliente).
- **Técnica — contrato hacia adelante:** esta fecha es el dato que leen el cierre de disponibilidades por universidad (HU-02.2) y el fin de la lista de espera por universidad (HU-10.2); su clave es `(universidad, periodo)` y su semántica debe fijarse con Área 1 y Área 2 antes de construirla.
- **Técnica — mismo patrón append-only de HU-14.2:** si la fecha se corrige, conviene conservar el valor anterior por el RNF de trazabilidad ("sin edición retroactiva silenciosa"); el cliente no definió si la fecha puede corregirse una vez iniciadas las inscripciones (ver Preguntas abiertas).
- **Técnica — dependencia bloqueante de Área 7:** sin los periodos académicos (HU-38.1) la historia no puede probarse de punta a punta.

## HU-19.1 Generar el calendario de desembolsos de un becario sin superar lo aprobado

- **Técnica — consistencia bajo concurrencia:** la suma de lo programado y su comparación con lo aprobado se evalúan dentro de una misma transacción sobre el agregado Beca, con control de versión (bloqueo optimista), para que dos ediciones simultáneas no dejen el calendario con datos contradictorios (RNF de consistencia bajo concurrencia).
- **Técnica — estacionalidad:** el arranque de calendarios se concentra al cierre de ciclo, por lo que el diseño transaccional debe soportar ráfagas de escritura sobre un mismo conjunto de programas sin degradar el resto de la plataforma (RNF de disponibilidad en periodos críticos).

## HU-20.1 Registrar y confirmar un desembolso con un código de un solo uso

- **Técnica — servicio único de verificación:** la emisión y validación del OTP se implementa una sola vez como servicio transversal reutilizable por toda acción sensible (confirmar desembolso, resolver un fallido, registrar o modificar datos de pago, revocar una beca), con marca de "usado" por código, vigencia de 15 minutos, contador de 3 intentos con espera de 1 hora, reenvío y elección de canal (correo o SMS) por usuario. Al estar la Fundación ya sobre Firebase, debe evaluarse Firebase Authentication (MFA) antes de incorporar un proveedor externo, en coherencia con la restricción de no crear infraestructura paralela.
- **Técnica — consistencia bajo concurrencia:** la validación del estado de la beca, del presupuesto del programa y de si el desembolso ya fue confirmado, y su escritura, ocurren en una misma transacción con control de versión sobre los agregados Beca y Programa (presupuesto/cupo). No basta un *check-then-act* (RNF de consistencia bajo concurrencia).
- **Técnica — trazabilidad:** el registro y la confirmación se asientan en el historial de decisiones append-only, no como campos mutables de la beca (RNF de trazabilidad; RF-35, ver HU-35.1).
- **Técnica — procesamiento en segundo plano:** la notificación al becario se envía por el mismo mecanismo asíncrono de RF-29, fuera de la transacción de confirmación (RNF de procesamiento en segundo plano; ver HU-29.1).

## HU-20.2 Resolver un desembolso fallido o devuelto

- **Técnica — servicio único de verificación:** la resolución reutiliza el mismo servicio de OTP de HU-20.1, no una implementación propia.
- **Técnica — trazabilidad append-only:** marca y resolución se asientan como eventos inmutables del historial de la beca, no sobrescribiendo campos del desembolso (RF-35; ver HU-35.1).

## HU-20.3 Registrar y modificar los datos de pago del becario con un código de un solo uso

- **Técnica — protección del dato:** la cuenta bancaria se cifra en reposo y se muestra enmascarada en la interfaz (solo los últimos dígitos), tanto en la consulta como en el historial del cambio (RNF de confidencialidad y protección de datos personales).
- **Técnica — servicio único de verificación:** reutiliza el mismo servicio de OTP de HU-20.1 y HU-20.2.
- **Técnica — auditoría de lectura:** se registra también quién consulta los datos de pago, no solo quién los registra o modifica (RNF de protección de datos personales).

## HU-21.1 Suspender una beca con motivo y fecha de revisión, y notificar al becario

- **Técnica — consistencia bajo concurrencia:** el cambio a *Suspendida* y la pausa de los desembolsos programados ocurren en una misma transacción sobre el agregado Beca, con control de versión, de modo que una confirmación de desembolso simultánea no pueda quedar pagada sobre una beca ya suspendida (RNF de consistencia bajo concurrencia; ver HU-20.1).
- **Técnica — trazabilidad append-only:** la decisión se asienta como evento inmutable, del que la beca solo deriva su estado actual (RF-35; ver HU-35.1).
- **Técnica — procesamiento en segundo plano:** la notificación se envía por el mecanismo asíncrono de RF-29, fuera de la transacción del cambio de estado (RNF de procesamiento en segundo plano).

## HU-21.2 Reanudar una beca suspendida

- **Técnica — consistencia bajo concurrencia:** el cambio a *Activa* y la reactivación de los desembolsos programados ocurren en una misma transacción sobre el agregado Beca, con control de versión (RNF de consistencia bajo concurrencia).
- **Técnica — trazabilidad append-only:** la reanudación se asienta como evento inmutable, sin sobrescribir el registro de la suspensión previa (RF-35; ver HU-35.1).

## HU-21.3 Avisar al coordinador cuando llega la fecha de revisión de una suspensión

- **Técnica — proceso programado idempotente:** la vigilancia de las fechas de revisión se ejecuta como proceso en segundo plano y registra el envío por suspensión y fecha, de modo que el aviso no se duplique si el proceso se repite ni se pierda si falla (RNF de procesamiento en segundo plano; RNF de consistencia bajo concurrencia).

## HU-22.1 Revocar una beca con motivo y código de un solo uso

- **Técnica — consistencia bajo concurrencia:** la validación del código, el cambio de estado y la liberación de presupuesto y cupo (HU-22.2) se ejecutan de forma atómica, con control de versión sobre los agregados Beca y Programa, para que una confirmación de desembolso simultánea no produzca datos contradictorios en ninguno de los dos órdenes posibles (RNF de consistencia bajo concurrencia; ver HU-20.1).
- **Técnica — servicio único de verificación:** reutiliza el mismo servicio de OTP de HU-20.1, HU-20.2 y HU-20.3.
- **Técnica — trazabilidad append-only:** la revocación y los desembolsos confirmados antes de ella quedan como eventos inmutables, con su orden real de ocurrencia (RF-35; ver HU-35.1).

## HU-22.2 Liberar el presupuesto y el cupo de una beca revocada

- **Técnica — consistencia bajo concurrencia:** la liberación se aplica en la misma transacción que la revocación (HU-22.1), con control de versión sobre el agregado Programa (presupuesto/cupo), de modo que dos liberaciones concurrentes o una liberación concurrente con la confirmación de otro desembolso no se pierdan ni se dupliquen (RNF de consistencia bajo concurrencia).
- **Técnica — escalabilidad:** el disponible se mantiene como agregado incremental, sin recalcular todo el histórico de desembolsos en cada consulta (RNF de escalabilidad de uso).

## HU-23.1 Consultar y exportar el estado financiero de un programa

- **Técnica — aislamiento de la reportería:** las lecturas agregadas se sirven desde una vista derivada o agregados incrementales, fuera del camino transaccional, para no degradar postulaciones, evaluaciones ni desembolsos, y para sostener el crecimiento de 12 a más de 25 programas (RF-23; RNF de escalabilidad de uso).
- **Técnica — consistencia consulta/exportación:** el archivo se genera a partir del mismo snapshot ya mostrado en pantalla, no de un recálculo al momento de exportar, para cumplir la exigencia de cifras idénticas (RF-23).
- **Técnica — procesamiento en segundo plano:** la generación de un archivo con gran volumen de movimientos se ejecuta de forma asíncrona, sin bloquear ni ralentizar el uso del resto de la plataforma (RNF de procesamiento en segundo plano).
- **Técnica — almacenamiento:** si el archivo exportado se persiste en lugar de descargarse al vuelo, se aloja en la misma cuenta de Firebase que ya usa la Fundación, sin crear un almacenamiento paralelo (Restricción del cliente).

## HU-24.1 Entregar el reporte académico del periodo, con alerta de entrega tardía

- **Técnica — trazabilidad:** el reporte y su fecha de entrega se escriben en el registro append-only único de eventos de beca (RF-35; ver HU-35.1), sin posibilidad de edición ni eliminación retroactiva. El historial no puede modelarse como un campo mutable.
- **Técnica — autorización:** el filtro "solo mi propia beca" se aplica en la consulta de datos, no solo en la interfaz.
- **Técnica — fecha límite consultable:** la `fecha límite` por programa y periodo se modela como dato consultable de forma independiente del flujo de entrega, porque la leerá también el proceso programado de recordatorios previos al vencimiento (RF-30, Área 6). Esta HU solo compara fechas en el momento del envío y no requiere ese proceso.
- **Técnica — huso horario:** la comparación entre fecha de entrega y fecha límite se hace con una misma referencia de fecha/hora en UTC-4, no con la hora local del dispositivo del becario.
- **Técnica — accesibilidad:** la entrega debe poder completarse con conectividad limitada (RNF de accesibilidad).

## HU-24.2 Registrar el reporte académico desde la universidad

- **Técnica — autorización por universidad:** el enlace de universidad aliada no está cubierto por el supuesto S1 (usuarios internos). Debe tratarse como un tipo de usuario propio del módulo de permisos (RF-37, Área 7), con autorización a nivel de datos filtrando por universidad en la consulta, no solo en la interfaz, para evitar fuga de información entre universidades.
- **Técnica — 2FA del enlace:** el acceso del enlace de universidad aliada exige verificación en dos pasos (Restricciones Técnicas del Cliente); es una precondición que resuelve el Área 7, no esta HU.
- **Técnica — trazabilidad por autor:** el reporte se escribe en el registro append-only (RF-35; ver HU-35.1) conservando autor y fecha; ningún usuario puede editarlo ni eliminarlo después, tampoco otro enlace de la misma universidad.
- **Técnica — dependencia de dominio:** el sistema debe poder consultar si el convenio de la universidad incluye "intercambio de datos académicos" (RF-14; ver HU-14.1) antes de habilitar este registro.

## HU-24.3 Recibir por API el reporte académico que envía la universidad

- **Técnica — trazabilidad:** el reporte recibido se escribe en el registro append-only único de eventos de beca (RF-35; ver HU-35.1), conservando el origen y la fecha de recepción; un envío posterior no edita ni elimina un reporte ya registrado.
- **Técnica — autorización de la universidad emisora:** la universidad que envía es un consumidor de API con identidad propia, distinta de la del enlace que entra a la plataforma; su autorización se filtra por universidad en la consulta y la escritura de datos, no solo en la interfaz (RF-37, Área 7). El mecanismo concreto de autenticación de la API queda por definir con el cliente (ver Preguntas abiertas).
- **Técnica — procesamiento en segundo plano:** la recepción de lotes de reportes de una universidad debe procesarse de forma asíncrona, paginada e idempotente, sin bloquear el uso normal de la plataforma (RNF de procesamiento en segundo plano y de escalabilidad).
- **Técnica — dependencia de terceros:** la plataforma no puede asumir que la universidad envía a tiempo ni de forma completa (Riesgo "Dependencia de terceros" del PRD); por eso las otras dos vías de entrada siguen disponibles y la alerta de entrega tardía se mantiene.

## HU-25.1 Definir la política de consecuencias y evaluar el cumplimiento

- **Técnica — consistencia bajo concurrencia:** un mismo reporte académico no puede quedar con dos evaluaciones contradictorias; la escritura de la evaluación usa el mismo mecanismo transaccional (bloqueo optimista o transacción atómica) definido para el agregado Beca en las Áreas 3 y 4, no uno propio (RNF de consistencia bajo concurrencia).
- **Técnica — autorización:** el alcance por programa se aplica en la consulta de datos, no solo en la interfaz.
- **Técnica — trazabilidad:** la evaluación se escribe en el registro append-only único de eventos de beca (RF-35; ver HU-35.1), que consume HU-28.1.

## HU-25.2 Aplicar la consecuencia por incumplimiento e informarla al becario

- **Técnica — OTP:** se reutiliza el servicio de verificación de un solo uso ya exigido en el Área 4 (HU-22.1); no se diseña uno nuevo aquí. Hasta que el código se valide, la beca no cambia de estado.
- **Técnica — reglas del código de un solo uso:** el código se envía por el canal que elige el usuario, correo o SMS; vence a los **15 minutos** de enviado; admite **3 intentos fallidos** y, después, el sistema no acepta nuevos intentos durante **1 hora**. Las reglas y los mensajes los fija HU-22.1 (Área 4); esta HU las reutiliza sin redefinirlas.
- **Técnica — 2FA:** el acceso del coordinador exige verificación en dos pasos (Restricciones Técnicas del Cliente); es una precondición resuelta en el Área 7, no en esta HU.
- **Técnica — consistencia bajo concurrencia:** aplicar la consecuencia escribe sobre el agregado Beca con el mismo mecanismo transaccional del Área 4, para que no se produzcan datos contradictorios si al mismo tiempo se actúa sobre esa beca (RNF de consistencia bajo concurrencia).
- **Técnica — trazabilidad:** la consecuencia y su justificación se escriben en el registro append-only único de eventos de beca (RF-35; ver HU-35.1), que consume HU-28.1.

## HU-26.1 Definir la ventana de renovación y solicitar la renovación dentro de ella

- **Técnica — reserva de presupuesto:** la reserva por solicitud pendiente se calcula sobre el agregado Programa con la misma fórmula y el mismo control transaccional de HU-12.1/HU-12.2 (`total − desembolsado − comprometido`), no con un cálculo propio de esta área.
- **Técnica — disponibilidad en periodos críticos:** la ventana de renovación concentra las solicitudes en pocos días (RNF de disponibilidad en periodos críticos; Riesgo de estacionalidad del PRD); el diseño debe sostener ese pico.
- **Técnica — autorización:** el becario solo ve y usa la ventana de renovación del programa de su propia beca; el filtro se aplica en la consulta de datos.
- **Técnica — huso horario:** la comparación entre el momento de la solicitud y las fechas de la ventana usa una misma referencia de fecha/hora en UTC-4, no la hora local del dispositivo del becario.

## HU-26.2 Evaluar la renovación y resolver las becas no renovadas

- **Técnica — consistencia bajo concurrencia:** la aprobación compite por el saldo disponible del agregado Programa; debe usar el mismo mecanismo transaccional (bloqueo optimista o transacción atómica) definido para `Programa.presupuesto` en las Áreas 3 y 4, de modo que dos aprobaciones simultáneas del mismo programa no lo lleven a déficit (RNF de consistencia bajo concurrencia).
- **Técnica — procesamiento en segundo plano e idempotencia:** la resolución de las becas sin solicitud al cerrar la ventana, y la transición de periodo de S11/HU-38.2, son un fan-out sobre un volumen grande de becas, cada una con su propio registro de auditoría. Debe ejecutarse como trabajo asíncrono paginado, reanudable e idempotente (verificando el estado vigente antes de escribir, sin volver a marcar *Inactiva* una beca que ya lo está ni duplicar su auditoría), nunca acoplado de forma síncrona a la acción administrativa que lo dispara (RNF de procesamiento en segundo plano y de escalabilidad).
- **Técnica — disponibilidad en periodos críticos:** el cierre de la ventana concentra la carga en pocos días (RNF de disponibilidad en periodos críticos).
- **Técnica — trazabilidad:** cada decisión se escribe en el registro append-only único de eventos de beca (RF-35; ver HU-35.1), que consume HU-28.1.

## HU-27.1 Solicitar y resolver una pausa de la beca

- **Técnica — estado vigente al ejecutar desembolsos:** el proceso que ejecuta los desembolsos programados (HU-19.1, Área 4) debe tratar "beca *Activa* al momento de ejecutar" como precondición de cada ítem del área, no como un filtro aplicado una sola vez al generar el calendario; de lo contrario podría pagar una beca ya *Suspendida*.
- **Técnica — consistencia bajo concurrencia:** aprobar la pausa escribe sobre el agregado Beca con el mismo mecanismo transaccional del Área 4, para no producir datos contradictorios con un desembolso que se confirme al mismo tiempo (RNF de consistencia bajo concurrencia).
- **Técnica — 2FA:** el acceso del coordinador exige verificación en dos pasos (Restricciones Técnicas del Cliente), resuelto en el Área 7.
- **Técnica — trazabilidad:** la solicitud y la decisión se escriben en el registro append-only único de eventos de beca (RF-35; ver HU-35.1), que consume HU-28.1.

## HU-27.2 Reactivar la beca tras verificar el estatus académico

- **Técnica — estado vigente al ejecutar desembolsos:** la reanudación no reprograma el calendario; el proceso de desembolsos del Área 4 valida el estado vigente de la beca en cada corrida (ver HU-27.1).
- **Técnica — trazabilidad:** la verificación y la decisión se escriben en el registro append-only único de eventos de beca (RF-35; ver HU-35.1), que consume HU-28.1.
- **Técnica — confidencialidad:** la información académica del becario solo es visible para quien la necesita para decidir (RNF de confidencialidad), con el filtro aplicado en la consulta de datos.

## HU-28.1 Ver la línea de tiempo completa de una beca

- **Técnica — fuente única de eventos:** la línea de tiempo lee el registro append-only único de "evento de beca" de RF-35/HU-35.1, filtrado por beca y ordenado por fecha, con un campo de tipo de evento y su payload. No hace un *join* en tiempo de lectura sobre las tablas de origen de cada área. Toda HU que cambie el estado de una beca o registre algo relevante sobre ella debe escribir en ese registro; la decisión debe acordarse con el Área 6 **antes** de implementar HU-24.1, HU-24.3, HU-25.2, HU-26.2 y HU-27.1/27.2, que son sus productoras.
- **Técnica — orden cronológico consistente:** todas las productoras deben usar la misma fuente de fecha/hora, para que el orden de la línea de tiempo no dependa de relojes distintos; esa fuente es la referencia en UTC-4 con la que se evalúan las fechas límite.
- **Técnica — escalabilidad:** la lectura debe paginarse y no recorrer todo el histórico de la plataforma en cada consulta (RNF de escalabilidad; crecimiento de 12 a más de 25 programas).
- **Técnica — autorización:** el alcance de datos (beca propia frente a beca de un programa del que el coordinador es responsable) se aplica en la consulta, no solo en la interfaz.

## HU-29.1 Recibir los hitos de mi postulación o beca por correo y por los canales del programa

- **Técnica — evento de dominio, no llamada directa:** las notificaciones se disparan por suscripción al registro append-only de eventos de cambio de estado (RF-35; ver HU-35.1), no por una llamada al módulo de notificaciones escrita dentro de cada HU de las Áreas 1 a 5. Sin esto, cada hito nuevo de otra área puede quedar sin notificar silenciosamente. Esta decisión debe tomarse antes de implementar las HU productoras de hitos (HU-01.1, HU-15.1, HU-20.1, entre otras). Los avisos que no nacen de un cambio de estado sino de la llegada de una fecha (HU-21.3, HU-27.2) no entran por este camino: los dispara el proceso programado que ya usa HU-30.1.
- **Técnica — abstracción de canal:** el correo es un canal mínimo fijo y los canales adicionales se modelan detrás de una interfaz extensible, para no rediseñar el módulo cuando se defina el primer canal adicional real. Esta abstracción la reutilizan HU-29.2, HU-30.1 y HU-31.1. El SMS ya es un canal real de la plataforma, porque el código de verificación se envía por correo o por SMS a elección del usuario, de modo que la interfaz debe poder usarlo.
- **Técnica — configuración por programa:** los canales activos son configuración del programa y viven junto al resto de su configuración (RF-11, Área 3), no en una tabla paralela del módulo de notificaciones.
- **Técnica — registro de estado de envío:** cada envío queda registrado por destinatario con su resultado (entregado o fallido). Sin ese registro no se distingue "no se le avisó por un fallo técnico" de "se le avisó y no entró a revisar" (RNF de trazabilidad).
- **Técnica — idempotencia:** el registro de envío es por (persona, hito), de modo que reprocesar el mismo hito no genera una segunda notificación (RNF de consistencia bajo concurrencia). Mismo mecanismo que usa HU-29.2.
- **Técnica — contenido mínimo necesario:** el mensaje que sale del perímetro del sistema dice qué ocurrió y la acción esperada en términos genéricos; el detalle sensible (ingreso declarado, composición familiar) solo es accesible dentro de la plataforma tras autenticarse, porque un correo queda almacenado y reenviable sin control del sistema (RNF de confidencialidad y de protección de datos personales). **El motivo de una suspensión queda fuera de esa lista:** el cliente confirma que el correo de suspensión sí lo lleva (ver HU-21.1).
- **Técnica — autorización en los datos:** el filtro "solo los canales de los programas que coordino" se aplica en la consulta de datos, no solo en la interfaz (RF-37).

## HU-29.2 Enviar notificaciones a todo un ciclo sin ralentizar la plataforma

- **Técnica — cola con control de tasa:** el *fan-out* de miles de destinatarios no es solo asíncrono. Cualquier proveedor de correo transaccional impone un límite de tasa de envío, así que el envío se procesa en una cola con *throttling* configurable por canal y proveedor, y reintentos con espera creciente. Un reintento sin control de tasa provoca rechazos del proveedor o notificaciones duplicadas.
- **Técnica — idempotencia por destinatario:** el estado de envío se persiste por (persona, notificación), no por lote. Si el trabajo se interrumpe a mitad, al reiniciarse no reenvía a quienes ya recibieron su notificación. Es el mismo registro de estado de envío de HU-29.1.
- **Técnica — aislamiento de recursos:** los procesos de envío masivo corren en trabajadores dedicados, separados del camino transaccional de postulación, evaluación y desembolso. Es la infraestructura de cola compartida con el *fan-out* de conclusión de becas (Área 5, HU-26.2) y el proceso de desembolsos (Área 4); esta HU la reutiliza, no crea una paralela.
- **Técnica — disponibilidad en periodos críticos:** los hitos masivos (resultado de evaluación de todo un ciclo) coinciden con las ventanas cortas de alta carga que el PRD identifica como críticas, lo que hace el aislamiento anterior más necesario, no menos (RNF de disponibilidad en periodos críticos; Riesgos del PRD).
- **Técnica — escalabilidad:** la selección de destinatarios del ciclo se resuelve con filtros indexados y por lotes, sin recorrer el histórico completo, para sostener el crecimiento de 12 a más de 25 programas (RNF de escalabilidad de uso).

## HU-30.1 Configurar y recibir recordatorios antes de un vencimiento

- **Técnica — fecha límite consultable:** el proceso de recordatorios no reacciona a un evento: compara la fecha actual contra fechas límite de otras áreas. Cada fecha límite (documento rechazado, RF-03/Área 1; ventana de renovación, RF-26/Área 5; reporte académico, RF-24/Área 5) debe modelarse como dato consultable de forma independiente, no implícito en la validación de su propia pantalla, o el proceso no tiene de dónde leerla. Dependencia ya señalada en la revisión del Área 5 y confirmada allí al actualizar HU-24.1.
- **Técnica — estado de cumplimiento con varias fuentes:** el dato que el proceso consulta para saber si el plazo del reporte académico está cumplido es el reporte del periodo, sin importar por cuál de las tres vías entró (HU-24.1, HU-24.2, HU-24.3); no puede ser "el becario entregó".
- **Técnica — deduplicación:** debe existir registro de "recordatorio ya enviado para (persona, plazo, momento de anticipación)". Si el programador de tareas se reinicia o corre dos veces el mismo día, la persona no recibe el mismo recordatorio dos veces.
- **Técnica — reutilización de infraestructura:** el proceso corre sobre la misma cola y los mismos trabajadores en segundo plano de HU-29.2, con el registro de estado de envío por destinatario de HU-29.1; no se construye un mecanismo propio. Es también el proceso programado que dispara los avisos por fecha dirigidos a usuarios internos (HU-21.3, HU-27.2).
- **Técnica — escalabilidad del recorrido:** la búsqueda de personas con un plazo pendiente se resuelve con índices por fecha de vencimiento y por estado de cumplimiento. Un recorrido completo de tablas sin filtro indexado se degrada con el crecimiento previsto (RNF de escalabilidad de uso).
- **Técnica — contenido mínimo necesario:** el recordatorio indica el plazo, la fecha de vencimiento y la acción esperada, sin incluir datos sensibles del caso en el cuerpo del mensaje (RNF de confidencialidad; ver HU-29.1).

## HU-31.1 Avisar al comité de nuevas asignaciones y del cierre de la evaluación

- **Técnica — reutilización del canal:** el aviso y el recordatorio salen por la abstracción de canal de HU-29.1 y quedan en su registro de estado de envío, aunque el PRD no fije un canal mínimo para evaluadores. No se construye un mecanismo de envío propio para esta HU.
- **Técnica — deduplicación:** el recordatorio de cierre se registra por (evaluador, ciclo, momento de anticipación), con el mismo mecanismo de HU-30.1, para que un reinicio del proceso no lo envíe dos veces.
- **Técnica — no reimplementar el conflicto de interés:** esta HU confía en que la asignación de HU-06.2 ya excluyó a los evaluadores con conflicto declarado y ya resolvió el reemplazo; no vuelve a evaluar esa regla ni elige al sustituto al enviar el aviso.
- **Técnica — contenido mínimo necesario:** el aviso identifica el programa y la postulación asignada sin incluir información socioeconómica del postulante en el cuerpo del mensaje; el expediente se consulta dentro de la plataforma tras autenticarse (RNF de confidencialidad).
- **Técnica — autorización en los datos:** la lista de asignaciones y los recordatorios se filtran por evaluador y ciclo en la consulta de datos, no solo en la interfaz (RF-37).

## HU-32.1 Enviar una consulta sobre mi postulación o beca

- **Técnica — autorización en los datos:** la lista de postulaciones y becas que se ofrece para asociar, y la lista de consultas propias, se filtran por persona en la consulta de datos, no solo en la interfaz (RNF de protección de datos personales).
- **Técnica — contenido como texto:** el mensaje es texto libre que entra por el navegador. Se almacena y se muestra como texto, sin interpretar ni ejecutar marcado ni guiones, tanto para el autor como para el coordinador que lo lee en HU-32.2.
- **Técnica — autor y fecha:** cada mensaje de la conversación conserva su autor y su fecha, de modo que el historial de la consulta quede íntegro para el seguimiento del equipo (RNF de trazabilidad).
- **Técnica — la consulta no es un documento de postulación:** los mensajes viven en la base de datos principal del sistema y no en el almacenamiento Firebase de documentos, que el PRD reserva para documentos de postulantes, cartas y comprobantes (Restricciones Técnicas del Cliente).

## HU-32.2 Responder y dar seguimiento a una consulta

- **Técnica — fuente de verdad única para el estado:** el contenido de los mensajes y el estado autoritativo de la consulta (abierta, respondida, cerrada) viven en la base de datos principal del sistema. Si se aprovecha la cuenta Firebase existente para el transporte en tiempo real, debe limitarse a notificar que hay un cambio, no a almacenar la consulta ni su estado; de lo contrario se crea un almacenamiento paralelo de facto, que es justamente lo que prohíbe la restricción técnica del cliente.
- **Técnica — mecanismo de tiempo real compartido:** el canal de empuje al navegador que requiere esta HU es el mismo que necesitarán el panel de indicadores (RF-40) y el reporte consolidado (RF-34); conviene decidirlo una sola vez, antes de comenzar el desarrollo de esta HU, y no mantener dos soluciones distintas.
- **Técnica — control de concurrencia:** el cambio de estado de la consulta se protege con bloqueo optimista o equivalente sobre el registro de la consulta, de forma que ninguna respuesta se pierda silenciosamente ante escrituras simultáneas.
- **Técnica — autorización en los datos:** el filtro por programa se aplica en la consulta de datos, no solo en la interfaz; un coordinador no puede abrir ni responder por identificador una consulta de un programa que no coordina (RF-37).
- **Técnica — historial íntegro:** los mensajes y los cambios de estado de la consulta se conservan completos con autor y fecha. No se trata de una decisión sobre la beca en el sentido de RF-35, por lo que no se le impone aquí el carácter estrictamente inmutable de ese registro.

## HU-33.1 Generar el reporte de impacto de un donante con datos agregados o anonimizados

- **Técnica — confidencialidad por diseño:** el filtrado y la anonimización de datos socioeconómicos se implementan en una única capa de servicio de agregación/anonimización, que es el único camino permitido para producir vistas dirigidas al donante. No puede quedar en la interfaz ni en lógica ad-hoc del reporte, porque cualquier canal nuevo (p. ej. la exportación de HU-36.1) reintroduciría la fuga (RNF de confidencialidad y de protección de datos personales).
- **Técnica — procesamiento en segundo plano:** el reporte es una operación pesada de agregación sobre programas, aportes, desembolsos y reportes académicos; no debe bloquear ni ralentizar el uso normal del resto de la plataforma mientras se procesa (RNF de procesamiento en segundo plano, que nombra explícitamente los "reportes grandes"). La ruta de lectura del reporte debe separarse de la ruta transaccional.
- **Técnica — almacenamiento:** si el reporte se materializa como archivo descargable o se conserva una copia, ese archivo se aloja en la misma cuenta Firebase que ya usa la Fundación, sin crear un almacenamiento paralelo (Restricciones Técnicas del Cliente).
- **Técnica — autenticación:** el coordinador accede a información de decisión y presupuesto, por lo que su inicio de sesión exige un segundo factor además de la contraseña (Restricciones Técnicas del Cliente, Verificación en dos pasos). Esto es 2FA de sesión, no un OTP por acción. El segundo factor llega por el canal que elige el propio usuario, correo o SMS; el código vence a los 15 minutos y, tras 3 intentos fallidos, la persona debe esperar 1 hora. Esas reglas se definen en la HU de autenticación y aquí solo se referencian.
- **Técnica — sin OTP:** generar este reporte no mueve dinero ni cambia el estado legal de una beca, por lo que no requiere código de verificación de un solo uso (el PRD lo exige solo para RF-20, RF-22 y el cambio de datos de pago).

## HU-33.2 Registrar el consentimiento para ser presentado a mi donante

- **Técnica — consentimiento vigente único:** el registro de la aceptación de un becario es uno y vigente; confirmar de nuevo no crea un segundo registro con otra fecha.
- **Técnica — nueva entidad de dominio:** el consentimiento del becario hacia su donante no está cubierto por ninguna entidad existente del modelo; debe modelarse explícitamente antes de construir HU-33.1.
- **Técnica — sin OTP:** registrar el consentimiento no mueve dinero ni cambia el estado legal de la beca, por lo que no requiere código de verificación de un solo uso.

## HU-34.1 Consultar el reporte consolidado institucional por periodo, universidad y carrera

- **Técnica — ruta de lectura separada:** es el requisito de mayor riesgo del área. Calcular el reporte con agregaciones en vivo sobre las mismas tablas que reciben postulaciones y desembolsos es inviable a mediano plazo y coincide con las ventanas de mayor carga (apertura de convocatoria, cierre de evaluación), donde el PRD también exige disponibilidad. La ruta de lectura analítica debe separarse de la transaccional (vista materializada, réplica de lectura, almacén de reportes o snapshot recalculado por un trabajo en segundo plano). FA-1 ya es compatible con ese patrón asíncrono: no debe implementarse como consulta síncrona por simplicidad.
- **Técnica — "activo" es estado derivado:** la condición de becario activo se calcula desde la máquina de estados de la beca (reglas transversales del PRD), no se duplica en una marca independiente que pueda desincronizarse. El cálculo debe contemplar el estado *Inactiva* incorporado por C3, incluida su reversibilidad: *Inactiva* no es un estado final y la beca puede volver a *Activa* o pasar a *Concluida*.
- **Técnica — validación por carga:** el cumplimiento de "no afectar la operación diaria" se verifica con una prueba de carga y concurrencia, no solo con revisión funcional; el Escenario 3 define el comportamiento esperado, pero su ejecución requiere un arnés distinto al de los escenarios de datos.
- **Técnica — autenticación:** Dirección tiene acceso a información financiera y de decisión, por lo que su inicio de sesión exige un segundo factor además de la contraseña (Restricciones Técnicas del Cliente). Es 2FA de sesión, no un OTP por acción. El segundo factor llega por el canal que elige el propio usuario, correo o SMS; el código vence a los 15 minutos y, tras 3 intentos fallidos, la persona debe esperar 1 hora. Esas reglas se definen en la HU de autenticación y aquí solo se referencian.
- **Técnica — sin OTP:** consultar este reporte no requiere código de verificación de un solo uso.

## HU-35.1 Registrar cada decisión con quién, cuándo y por qué, sin edición posterior

- **Técnica — atomicidad:** el cambio de estado del dominio y la escritura del registro de auditoría son una sola unidad atómica; ocurren ambas o ninguna. Se resuelve con una transacción (o escritura por lotes transaccional, si el almacén es Firestore) que incluya el cambio de estado más su registro. Nunca con una escritura de auditoría "best effort" posterior al hecho.
- **Técnica — inmutabilidad real:** el carácter append-only se refuerza a nivel de permisos de datos, sin operaciones de actualización ni borrado concedidas sobre esa colección o tabla. Ocultar el botón de editar en la interfaz no satisface la regla, porque un rol con acceso directo a la base de datos o a una API genérica la violaría.
- **Técnica — consistencia bajo concurrencia:** dos acciones simultáneas sobre el mismo caso (por ejemplo, revocar una beca mientras finanzas confirma un desembolso) no deben dejar registros contradictorios ni aplicar un cambio de estado sin su registro correspondiente (RNF de consistencia bajo concurrencia).
- **Técnica — dependencia de identidad:** el "quién" de cada registro exige que la gestión de usuarios y roles ya exista y esté integrada con el inicio de sesión antes de que cualquier módulo produzca registros válidos (RF-37, ver HU-37.1).
- **Técnica — secuenciación:** este registro es una pieza de infraestructura compartida por las Áreas 2, 4 y 5. Debe construirse antes de las historias que cambian estado (suspender, revocar, renovar, aprobar); dejarlo para después obliga a reescribirlas para que pasen por él.
- **Técnica — modelo a futuro:** los campos del registro deben definirse contemplando la exportación de HU-36.1, porque no es posible reconstruir retroactivamente datos que no se guardaron.
- **Técnica — sin OTP:** el registro de la decisión en sí no exige código de un solo uso. El OTP lo exigen las acciones de RF-20 y RF-22 en sus propias HU; aquí no se añade un segundo factor por acción.

## HU-35.2 Consultar el registro de decisiones

- **Técnica — solo lectura a nivel de datos:** esta consulta usa un acceso de solo lectura sobre el registro append-only de HU-35.1; no se le conceden permisos de actualización ni de borrado.
- **Técnica — autenticación:** Dirección accede a información de decisión, por lo que su inicio de sesión exige el segundo factor (Restricciones Técnicas del Cliente). Es 2FA de sesión, no un OTP por acción. El segundo factor llega por el canal que elige el propio usuario, correo o SMS; el código vence a los 15 minutos y, tras 3 intentos fallidos, la persona debe esperar 1 hora. Esas reglas se definen en la HU de autenticación y aquí solo se referencian.

## HU-36.1 Exportar el uso de los fondos y el registro de decisiones para una auditoría

- **Técnica — un solo camino de anonimización:** la exportación debe usar la misma capa de servicio de agregación y anonimización que HU-33.1 cuando el destinatario no tenga derecho a datos individuales. Un canal de exportación propio que lea campos socioeconómicos crudos reintroduciría la fuga que RF-33 y el RNF de confidencialidad cierran.
- **Técnica — procesamiento en segundo plano:** una exportación de "todos los programas" en una fundación que pasa de 12 a más de 25 programas es una operación pesada; se procesa en segundo plano para no bloquear ni ralentizar el resto de la plataforma (RNF de procesamiento en segundo plano, que nombra los "reportes grandes").
- **Técnica — almacenamiento:** si el archivo generado se conserva o se vuelve a entregar, se aloja en la misma cuenta Firebase que ya usa la Fundación, sin crear un almacenamiento paralelo (Restricciones Técnicas del Cliente).
- **Técnica — solo lectura:** la exportación accede al registro de decisiones en modo lectura, sin permisos de actualización ni de borrado (ver la inmutabilidad de HU-35.1).
- **Técnica — autenticación:** quien exporta accede a información financiera y de decisión, por lo que su inicio de sesión exige el segundo factor (Restricciones Técnicas del Cliente). Es 2FA de sesión, no un OTP por acción. El segundo factor llega por el canal que elige el propio usuario, correo o SMS; el código vence a los 15 minutos y, tras 3 intentos fallidos, la persona debe esperar 1 hora. Esas reglas se definen en la HU de autenticación y aquí solo se referencian.
- **Técnica — sin OTP:** exportar no mueve dinero ni cambia el estado legal de una beca, por lo que no requiere código de verificación de un solo uso.
- **Técnica — modelo de datos anticipado:** aunque RF-36 es *Podría tener* y puede posponerse, el modelo del registro de decisiones (RF-35) debe diseñarse desde el principio contemplando esta exportación, para no remodelarlo después.

## HU-37.1 Crear usuarios internos y tipos de usuario con acceso limitado a su función

- **Técnica:** los roles y tipos de usuario se modelan como **datos configurables** (catálogo de tipos + matriz de permisos), no como un enum o `switch` en el código; agregar un tipo nuevo no debe exigir cambios de código ni un nuevo despliegue (RF-37 "puede necesitar agregar nuevos tipos con el tiempo").
- **Técnica:** la autorización se aplica en el **backend**, con máscara a nivel de campo en la respuesta del sistema; ocultar el campo solo en la pantalla no satisface la confidencialidad, porque una consulta directa al servicio devolvería el dato (RNF de confidencialidad y de protección de datos personales).
- **Técnica:** el motor de autorización debe poder evaluar **condiciones de estado de la entidad**, no solo pares rol→permiso: la ocultación de puntuaciones ajenas depende de si el periodo de revisión está abierto o cerrado (S9, RF-08).
- **Técnica:** el punto de aplicación de permisos es único y centralizado; es el mismo que resuelve la restricción por conflicto de interés (ver HU-39.1), para que ninguna pantalla nueva quede sin la restricción.
- **Técnica:** el catálogo de permisos del sistema no está enumerado de forma completa en el PRD; se ensambla a partir de las reglas de acceso de cada área (RF-08, RF-32, RF-37, RNF de confidencialidad) y debe mantenerse como artefacto único para que dos áreas no nombren distinto el mismo permiso.

## HU-37.2 Iniciar sesión con verificación en dos pasos

- **Técnica:** el 2FA de inicio de sesión y el **código de un solo uso (OTP) por operación sensible** son dos flujos independientes. Haber verificado el segundo factor al entrar **no** sustituye el OTP que se exige antes de confirmar un desembolso (ver HU-20.1), revocar una beca (ver HU-22.1) o modificar datos de pago de un becario (Restricción del cliente, RF-20, RF-22). Pueden compartir la infraestructura de envío del código, pero tienen disparadores y alcance distintos.
- **Técnica:** el segundo factor debe funcionar para personas externas a la Fundación (evaluadores externos del comité, enlace de universidad aliada), que no tienen credenciales ni infraestructura institucional.
- **Técnica:** el sistema resuelve qué roles y tipos exigen el segundo factor a partir de la configuración de permisos de HU-37.1, no de una lista fija en el código.

## HU-38.1 Registrar y consultar los periodos académicos

- **Técnica:** el Periodo académico es una entidad propia y referencia obligatoria de Programa (RF-11), Ciclo de postulación (RF-02) y Renovación (RF-26); debe existir antes de modelar esas tres, aunque el catálogo del PRD coloque RF-38 al final.
- **Técnica:** los periodos no se destruyen ni se sobrescriben al avanzar la secuencia: lo ligado a un periodo anterior debe seguir resolviéndose sobre ese periodo (RF-38, RNF de trazabilidad; igual que el costo de matrícula de referencia de RF-14 conserva los valores ya usados).
- **Técnica:** la fecha de inicio de inscripciones es un dato de la relación **universidad–periodo**, no del periodo ni de la universidad por separado: un mismo periodo tiene tantas fechas como universidades aliadas, y una universidad tiene una fecha distinta en cada periodo.

## HU-38.2 Marcar el periodo actual sin interrumpir renovaciones

- **Técnica:** cada cambio de estado se registra de forma **individual por beca**, no como un registro agregado del área, y el histórico no puede editarse ni eliminarse después (RF-35, RNF de trazabilidad).
- **Técnica:** el cambio de periodo es un *fan-out* potencialmente grande (25+ programas, cientos de becarios). Se resuelve fijando primero un corte de qué becas califican y aplicando luego los cambios de forma transaccional por beca e idempotente ante reintentos, de modo que una interrupción no deje el cambio parcialmente aplicado (RNF de escalabilidad de uso).
- **Técnica:** si el volumen lo justifica, el *fan-out* se ejecuta como proceso en segundo plano reutilizando la infraestructura de las Áreas 4/5/6, sin bloquear la acción de Dirección ni el resto de la plataforma (RNF de procesamiento en segundo plano).
- **Técnica:** ante dos solicitudes casi simultáneas de cambio de periodo actual, el resultado final debe ser un único periodo actual, sin estados intermedios con dos periodos actuales ni con ninguno (RF-38, RNF de consistencia bajo concurrencia).

## HU-39.1 Declarar un conflicto de interés y restringir mi participación en ese caso

- **Técnica:** la restricción se aplica en el **mismo punto de autorización** que decide quién puede puntuar o decidir (el motor centralizado de HU-37.1), no como una validación adicional dispersa por pantalla; debe poder evaluar la condición "tiene conflicto declarado con este postulante" sobre la entidad, no solo el rol.
- **Técnica:** la opción de declarar el conflicto se ofrece desde que la postulación le es asignada o visible a la persona, antes de puntuar o decidir, para que la declaración pueda hacerse sin haber participado en la evaluación (regla transversal).
- **Técnica:** esta historia no se diseña aislada: se co-diseña con HU-06.2 (asignación de comité y autorización de excepción), porque ambas resuelven el mismo filtro de participación.

## HU-40.1 Ver los indicadores clave actualizados sin recargar la página

- **Técnica:** los indicadores se mantienen como **agregados actualizados por eventos** (vista materializada o contadores), no como consultas directas sobre las tablas transaccionales en cada apertura o actualización del panel; lo contrario degrada exactamente lo que el RNF prohíbe degradar al crecer de 12 a 25+ programas.
- **Técnica:** los incrementos de los contadores son **atómicos** en la base de datos, no lectura-cálculo-escritura desde la aplicación; dos eventos simultáneos deben sumar ambos y no perder uno (RNF de consistencia bajo concurrencia).
- **Técnica:** el panel consume el mismo bus de eventos de dominio de RF-35/RF-29 y el mismo canal de *push* al navegador de HU-32.2, y comparte el mecanismo de agregados con HU-34.1 (RF-34 tiene la misma exigencia textual de no afectar la operación diaria). No se construyen dos infraestructuras de agregación en tiempo real para el mismo problema.
- **Técnica:** si el canal en tiempo real se interrumpe, el panel reconcilia su estado con el valor real del servidor al reconectar, en vez de conservar un valor obsoleto sin aviso (RNF de actualizaciones en tiempo real).
