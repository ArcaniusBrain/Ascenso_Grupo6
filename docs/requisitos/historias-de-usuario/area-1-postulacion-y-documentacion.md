# Área 1 — Postulación y Documentación

> Parte de las [Historias de Usuario · Plan B · versión final](README.md). Reglas técnicas en el [Anexo A](anexo-a-reglas-tecnicas.md); preguntas abiertas en el [Anexo B](anexo-b-preguntas-abiertas.md).

## RF-01 — Registro de postulantes · *Debe tener*

### HU-01.1 Crear mi perfil de postulante y postular a una convocatoria abierta

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-01 | Debe tener | HU-02.2, HU-14.1, HU-37.1 | HU-01.1, HU-01.2, HU-01.3, HU-01.4 |

#### Narrativa

- Como postulante
- Quiero crear mi cuenta, registrar mis datos personales, académicos y socioeconómicos, guardarlos y retomarlos después, y enviar mi postulación a un programa con convocatoria abierta
- Para que la Fundación considere mi postulación y evalúe mi elegibilidad aunque mi conexión sea limitada, sin exponer mis datos socioeconómicos a quien no los necesita

#### Reglas de negocio

- El postulante crea su propio perfil en la plataforma (RF-01).
- Campos mínimos de los datos personales y académicos: identificación, contacto, universidad, carrera y periodo académico actual. Campos mínimos de los datos socioeconómicos: ingreso familiar declarado y composición familiar (RF-01).
- La identificación única de una persona es su **número de cédula**. No puede existir más de una cuenta con la misma cédula: es la clave de deduplicación de negocio del modelo de usuarios.
- El postulante debe estar matriculado o admitido en una universidad aliada (RF-01). Elige su universidad entre las aliadas registradas (HU-14.1) e indica si está matriculado o admitido.
- El postulante postula con su perfil completo: datos personales y académicos, y socioeconómicos (RF-01).
- Un mismo postulante puede tener **varias postulaciones** a convocatorias distintas a la vez, y puede **volver a postular después de un rechazo**.
- Al postular, la postulación guarda una **copia de los datos del perfil en ese momento**; esa copia es la que se verifica y se evalúa, no el perfil en vivo. El postulante puede seguir actualizando su perfil mientras el periodo siga abierto. Si esa actualización modifica o no una postulación ya enviada está **por confirmar** (ver *Preguntas abiertas*).
- Al enviarse, la postulación queda *Recibida* (regla transversal de estados).
- La ventana de postulación se controla en HU-02.2, el no doble beneficio en HU-01.2 y las transiciones de estado posteriores en HU-04.1. Esta HU no reimplementa esas reglas: las invoca en el paso 7 del flujo.
- Los documentos se cargan sobre la postulación ya creada (HU-03.1).
- **Accesibilidad (RNF):** un postulante con conectividad limitada debe poder completar su postulación sin fricción innecesaria: guarda lo que lleva y lo retoma después sin perder lo guardado. Técnicamente implica guardado parcial por sección con tolerancia a conexión intermitente (reintentos que no pierden lo ya guardado) y un estado de completitud **calculado por sección**, no una bandera global; el modelo de datos no puede exigir todos los campos obligatorios desde el alta del perfil.
- **Confidencialidad (RNF):** los datos socioeconómicos solo los ve el propio postulante y quienes los necesitan para **evaluar o decidir sobre esa postulación específica**. No es un permiso por rol estático: es rol + asignación al caso. Técnicamente exige autorización a nivel de campo (*field-level authorization*), porque el mismo registro de postulación es visible para varios roles con distinto detalle. Los permisos por rol están en HU-37.1.
- **Protección de datos personales (RNF):** `ingreso_familiar_declarado` y `composicion_familiar` son datos sensibles; se recomienda cifrado en reposo o aislamiento de esquema/colección (el PRD lo exige como "buenas prácticas", sin detallar el mecanismo). La cédula es también un dato personal identificatorio y queda bajo el mismo tratamiento.
- **Verificación en dos pasos (restricción del cliente):** se exige solo a quien tiene acceso a información financiera o de decisión (coordinadores, finanzas, dirección, comité). El rol postulante **no** está en ese grupo y nunca debe recibir el reto de segundo factor. El mecanismo de login con 2FA vive en HU-37.2.
- **Firebase (restricción del cliente):** aplica a los **archivos** de la postulación (HU-03.1/HU-03.2). Los datos estructurados del perfil y de la postulación no deben forzarse a ese almacenamiento solo por consistencia de stack.
- **Disponibilidad en periodos críticos (RNF):** junto con HU-03.1 y HU-04.1, esta HU es el camino crítico que concentra el mayor tráfico concurrente justo antes del cierre de cada ciclo (riesgo "Estacionalidad alta" del PRD).
- **Sin definir en el PRD:** qué datos de acceso pide la cuenta (correo, usuario, otro). La clave de deduplicación de negocio ya no está abierta: es el número de cédula, y de ella se apoyan el bloqueo de doble beneficio (HU-01.2) y el conteo de postulantes de la reportería (RF-33/RF-34).

#### Flujo de la HU

1. El postulante entra a la plataforma, elige "Crear cuenta" e ingresa sus datos de acceso.
2. El sistema crea la cuenta con el rol de postulante. El postulante inicia sesión, sin segundo factor, y ve su perfil pendiente de completar.
3. Registra su número de cédula como identificación y su contacto. El sistema verifica que no exista otra cuenta con esa cédula. Elige su universidad aliada, indica si está matriculado o admitido, y registra su carrera y su periodo académico actual.
4. En la sección socioeconómica de su perfil, registra su ingreso familiar declarado y su composición familiar.
5. Guarda cada sección. El sistema conserva lo registrado aunque falten campos y marca la sección como completa cuando todos sus campos mínimos tienen valor.
6. El postulante abre una convocatoria publicada (HU-02.2) y pulsa "Postular".
7. El sistema verifica que su perfil esté completo y aplica las validaciones de HU-02.2 (ventana) y HU-01.2 (no doble beneficio).
8. El sistema crea la postulación en estado *Recibida*, guarda en ella una copia de los datos del perfil en ese momento y le muestra los documentos que debe cargar (HU-03.1).
9. El sistema muestra los datos socioeconómicos del postulante solo a quien evalúa o decide sobre esa postulación.

#### Flujos alternos y de error

- **FA-1 (paso 1) Datos de acceso incompletos:** el sistema no crea la cuenta, indica qué falta y muestra "Completa tus datos de acceso para crear tu cuenta."
- **FA-2 (paso 2) Datos de acceso incorrectos:** el sistema no da acceso y muestra "Los datos de acceso no son correctos."
- **FA-3 (paso 3) Universidad no aliada:** la universidad no aparece en la lista y el postulante no puede completar la sección.
- **FA-4 (paso 5) Falta un campo mínimo:** el sistema guarda lo registrado, deja la sección incompleta e indica qué falta, por ejemplo "Falta completar: carrera." o "Falta completar: composición familiar."
- **FA-5 (paso 6) Sin convocatorias publicadas:** el sistema muestra "No hay convocatorias publicadas en este momento." (caso delegado a HU-02.2, Escenario 3.)
- **FA-6 (paso 7) Fuera de la ventana de postulación:** el sistema no crea la postulación y aplica el bloqueo y los mensajes de HU-02.2 (convocatoria aún no abierta / ya cerrada / universidad que ya inició sus inscripciones). Esta HU no define su propio mensaje.
- **FA-7 (paso 7) Beca activa en el mismo periodo:** el sistema no crea la postulación y aplica el bloqueo y el mensaje de HU-01.2.
- **FA-8 (paso 9) Acceso horizontal entre postulantes:** un postulante intenta abrir la postulación o el perfil de otro; el sistema niega el acceso sin exponer ningún dato del titular.
- **FA-9 (paso 9) Rol sin necesidad de conocer:** un evaluador no asignado a esa postulación, o un coordinador de otro programa, la abre; el sistema no muestra los datos socioeconómicos y el intento queda registrado para trazabilidad.
- **FA-10 (inicio de sesión, control negativo) Segundo factor no aplicable:** el rol postulante nunca recibe el reto de 2FA.
- **FA-11 (paso 3) Cédula ya registrada en otra cuenta:** el sistema no guarda la sección y muestra "Ya existe una cuenta registrada con esta cédula."

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-01.1).*

#### Criterios de aceptación (17 escenarios)

##### Escenario 1: Cuenta creada

- **DADO QUE** María no tiene cuenta en la plataforma
- **CUANDO** completa sus datos de acceso y confirma
- **ENTONCES** el sistema crea su cuenta de postulante y le muestra su perfil pendiente de completar

##### Escenario 2: Perfil completo

- **DADO QUE** María está matriculada en "Universidad A", que es universidad aliada
- **CUANDO** registra identificación, contacto, "Universidad A", condición "Matriculada", carrera "Ingeniería Civil" y periodo "2026-2", un ingreso familiar de 400 y una composición familiar de 4 personas, y guarda
- **ENTONCES** el sistema guarda sus datos y marca como completas la sección de datos personales y académicos y la sección socioeconómica

##### Escenario 3: Guardar y retomar

- **DADO QUE** María guardó su identificación y su contacto y perdió la conexión
- **CUANDO** vuelve a entrar a su perfil
- **ENTONCES** encuentra esos datos guardados y continúa desde ahí

##### Escenario 4: Universidad no aliada

- **DADO QUE** "Universidad Z" no es universidad aliada
- **CUANDO** María busca su universidad
- **ENTONCES** "Universidad Z" no aparece y el sistema muestra "Solo pueden postular estudiantes matriculados o admitidos en una universidad aliada."

##### Escenario 5: Datos socioeconómicos solo para quien evalúa o decide

- **DADO QUE** Ana es evaluadora asignada a la postulación de María (HU-06.2) y Pedro es administrador financiero
- **CUANDO** Ana abre esa postulación y Pedro consulta el perfil de María
- **ENTONCES** Ana ve el ingreso familiar y la composición familiar de María, y Pedro no los ve

##### Escenario 6: Postulación recibida

- **DADO QUE** María tiene su perfil completo y "Becas Ingeniería 2026-2" está publicada y abierta
- **CUANDO** pulsa "Postular"
- **ENTONCES** el sistema crea su postulación en estado *Recibida* y le muestra los documentos requeridos

##### Escenario 7: Perfil incompleto

- **DADO QUE** María no registró su ingreso familiar
- **CUANDO** pulsa "Postular" en "Becas Ingeniería 2026-2"
- **ENTONCES** el sistema no crea la postulación y muestra "Completa tu perfil antes de postular. Falta: ingreso familiar declarado."

##### Escenario 8: Un postulante no accede a la postulación de otro

- **DADO QUE** María y Carlos son postulantes distintos, cada uno con su propia postulación
- **CUANDO** María intenta acceder a la postulación de Carlos
- **ENTONCES** el sistema le niega el acceso y no expone ningún dato personal, académico ni socioeconómico de Carlos

##### Escenario 9: Evaluador no asignado no ve los datos socioeconómicos

- **DADO QUE** Jorge es miembro del comité evaluador pero no está asignado a la postulación de María
- **CUANDO** Jorge intenta abrir el perfil o la postulación de María
- **ENTONCES** el sistema no le muestra el ingreso familiar ni la composición familiar de María, y el intento queda registrado para trazabilidad

##### Escenario 10: El postulante no recibe el reto de segundo factor

- **DADO QUE** María inicia sesión con sus datos de acceso correctos
- **CUANDO** el sistema valida sus credenciales
- **ENTONCES** el acceso se concede sin solicitar un segundo factor, porque el rol postulante no tiene acceso a información financiera ni de decisión

##### Escenario 11: Datos de acceso incompletos

- **DADO QUE** María no tiene cuenta en la plataforma
- **CUANDO** intenta confirmar la creación de su cuenta sin completar todos sus datos de acceso
- **ENTONCES** el sistema no crea la cuenta y muestra "Completa tus datos de acceso para crear tu cuenta."

##### Escenario 12: Datos de acceso incorrectos

- **DADO QUE** María ya tiene una cuenta creada en la plataforma
- **CUANDO** intenta iniciar sesión con datos de acceso incorrectos
- **ENTONCES** el sistema no le da acceso y muestra "Los datos de acceso no son correctos."

##### Escenario 13: Falta un campo mínimo al guardar una sección

- **DADO QUE** María está completando la sección de datos personales y académicos de su perfil
- **CUANDO** guarda la sección sin haber registrado su carrera
- **ENTONCES** el sistema conserva los demás datos ingresados, deja la sección incompleta y muestra "Falta completar: carrera."

##### Escenario 14: Una cédula ya registrada no admite una segunda cuenta

- **DADO QUE** María ya tiene una cuenta con la cédula "V-12345678" y creó una segunda cuenta con otros datos de acceso
- **CUANDO** registra en esa segunda cuenta la cédula "V-12345678" y guarda
- **ENTONCES** el sistema no guarda la sección y muestra "Ya existe una cuenta registrada con esta cédula."

##### Escenario 15: Varias postulaciones a la vez

- **DADO QUE** María tiene su perfil completo y "Becas Ingeniería 2026-2" y "Becas Salud 2026-2" están publicadas y abiertas
- **CUANDO** postula a las dos
- **ENTONCES** el sistema crea las dos postulaciones en estado *Recibida* y ambas aparecen en su lista de postulaciones

##### Escenario 16: Volver a postular después de un rechazo

- **DADO QUE** la postulación de María a "Becas Ingeniería 2026-1" quedó *Rechazada* y el ciclo 2026-2 de ese programa está publicado y abierto
- **CUANDO** María postula al ciclo 2026-2
- **ENTONCES** el sistema crea la nueva postulación en estado *Recibida*

##### Escenario 17: La postulación guarda una copia del perfil

- **DADO QUE** María postuló a "Becas Ingeniería 2026-2" con un ingreso familiar declarado de 400 y después actualiza su perfil a 500 con el periodo todavía abierto
- **CUANDO** Ana, evaluadora asignada, abre esa postulación
- **ENTONCES** ve el ingreso familiar de 400 con el que María postuló, y el perfil de María queda actualizado en 500

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| El postulante crea su propio perfil (RF-01) | Regla | 1, 2 | CUBIERTA |
| Campos mínimos personales/académicos | Regla | 2, 13 | CUBIERTA |
| Campos mínimos socioeconómicos | Regla | 2, 7 | CUBIERTA |
| Identificación única de la persona = número de cédula; una sola cuenta por cédula | Regla | 14 | CUBIERTA |
| Debe estar matriculado/admitido en universidad aliada | Regla | 2, 4 | CUBIERTA |
| Universidad elegida entre las aliadas registradas | Regla | 4 | CUBIERTA (el registro de aliadas es HU-14.1) |
| Postula con perfil completo | Regla | 6, 7 | CUBIERTA |
| Varias postulaciones a la vez y nueva postulación tras un rechazo | Regla | 15, 16 | CUBIERTA |
| La postulación guarda una copia del perfil; el perfil se sigue actualizando con el periodo abierto | Regla | 17 | CUBIERTA (el efecto sobre una postulación ya enviada está por confirmar) |
| Al enviarse, la postulación queda *Recibida* | Regla | 6 | CUBIERTA |
| Completitud calculada por sección, no bandera global | Regla (RNF) | 2, 13 | CUBIERTA |
| Ventana de postulación (incluido el cierre por universidad) | Regla (FA-6) | — | DELEGADA (HU-02.2) |
| No doble beneficio | Regla (FA-7) | — | DELEGADA (HU-01.2) |
| Transiciones posteriores de la postulación | Regla | — | DELEGADA (HU-04.1) |
| Documentos sobre la postulación ya creada | Regla | — | DELEGADA (HU-03.1) |
| Accesibilidad: guardado parcial y tolerancia a conexión intermitente | Regla (RNF) | 3 | CUBIERTA |
| Confidencialidad: datos socioeconómicos solo para quien evalúa/decide | Regla (RNF) | 5, 9 | CUBIERTA |
| Protección de datos personales: cifrado/aislamiento de datos sensibles | Regla (RNF) | — | NO PROBABLE (recomendación técnica; su efecto observable lo cubren 5, 8, 9) |
| 2FA no aplica al rol postulante | Regla (restricción cliente) | 10 | CUBIERTA (mecanismo en HU-37.2) |
| Firebase solo para archivos | Regla (restricción cliente) | — | DELEGADA (HU-03.1/HU-03.2) |
| Disponibilidad en periodos críticos | Regla (RNF) | — | NO PROBABLE |
| La copia del perfil es un dato propio de la postulación | Regla (técnica) | 17 | CUBIERTA (efecto observable) |
| FA-1 Datos de acceso incompletos | FA | 11 | CUBIERTA |
| FA-2 Datos de acceso incorrectos | FA | 12 | CUBIERTA |
| FA-3 Universidad no aliada | FA | 4 | CUBIERTA |
| FA-4 Falta un campo mínimo al guardar sección | FA | 13 | CUBIERTA |
| FA-5 Sin convocatorias publicadas | FA | — | DELEGADA (HU-02.2, Escenario 3) |
| FA-6 Fuera de la ventana de postulación | FA | — | DELEGADA (HU-02.2, Escenarios 5, 6, 11 y 12) |
| FA-7 Beca activa en el mismo periodo | FA | — | DELEGADA (HU-01.2, Escenario 2) |
| FA-8 Acceso horizontal entre postulantes | FA | 8 | CUBIERTA |
| FA-9 Rol sin necesidad de conocer (evaluador no asignado) | FA | 9 | CUBIERTA |
| FA-10 Segundo factor no aplicable | FA | 10 | CUBIERTA |
| FA-11 Cédula ya registrada en otra cuenta | FA | 14 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-01.1).*

### HU-01.2 Impedir postular con una beca activa en el mismo periodo

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-01 | Debe tener | HU-01.1, HU-18.1, HU-11.1, HU-14.2 | HU-01.5 |

#### Narrativa

- Como coordinador de programa
- Quiero que el sistema impida postular a quien ya tiene una beca activa en el mismo periodo cuando el programa no permite acumulación, que nunca permita dos becas de programas "completos" simultáneas y que la suma de las becas de una persona en un periodo no supere su límite
- Para que ninguna postulación otorgada infrinja la regla de no doble beneficio de la Fundación y el postulante entienda por qué fue bloqueado

#### Reglas de negocio

- **Capa 1 (nivel programa):** no puede postular a un programa si ya tiene una beca activa en el mismo periodo, **salvo que ese programa permita acumulación** (RF-01).
- Una persona **sí puede tener dos becas a la vez**, siempre dentro de la regla de no doble beneficio de las capas 2 y 3.
- **Capa 2 (nivel Fundación, etiqueta del programa):** cada programa se marca como **"completo"** o **"parcial"**. Esa etiqueta se usa **solo** para esta regla y no interviene en el cálculo del monto. Un postulante no puede tener dos becas de programas "completos" activas y simultáneas. La etiqueta se configura en el programa (HU-11.1).
- **Capa 3 (nivel Fundación, tope de la suma):** la suma de las becas de una persona en un periodo no puede superar el costo de la matrícula de su universidad en ese periodo (HU-14.2); ese costo es el límite, no un ejemplo.
- **Decisión del cliente:** las capas 2 y 3 se aplican **a la vez**: el sistema bloquea la postulación si cualquiera de las dos lo exige, y la capa 3 usa siempre el costo de la matrícula de su universidad en ese periodo como límite.
- Cuentan como beca activa las becas *Otorgada*, *Activa*, *Suspendida* y *Renovada* (S6). El catálogo de estados "activos" para este efecto debe estar centralizado en un único lugar del dominio, no replicado en cada consulta.
- Las becas *Concluida*, *Revocada* e *Inactiva*, y las postulaciones *Rechazada*, *Retirada* o *No elegible*, no cuentan como beca activa (S6). **Decisión del cliente:** una beca *Inactiva* no cuenta como beca activa para esta regla ni para el tope de la suma.
- Existe un estado de beca *Inactiva* (beca no renovada, o pausa vencida sin reactivar; no recibe dinero en el periodo siguiente). **Decisión del cliente:** *Inactiva* no es un estado final: desde ahí la beca puede volver a *Activa* o pasar a *Concluida*, que sigue siendo un estado final, y esa transición la decide el **coordinador de programa** cuando el becario sube los datos que le falten.
- El filtro es **por periodo académico**: una beca activa en otro periodo no bloquea (RF-01, "en el mismo periodo").
- El flag "permite acumulación", la etiqueta "completo"/"parcial" y el monto fijo de la beca por periodo son configuración del programa (RF-11; ver HU-11.1). Las coberturas total y parcial desaparecen como forma de calcular el monto, por lo que esta regla ya no se evalúa sobre el tipo de cobertura.
- La consulta de becas por persona se hace por su número de cédula, identificación única de una persona (ver HU-01.1 y HU-18.1).
- **Consistencia bajo concurrencia (RNF):** esta HU valida al momento de postular, contra las becas existentes. No cierra por sí sola el riesgo de doble beneficio: la misma regla debe revalidarse en el momento de otorgar o aceptar la oferta (HU-16.1), dentro de una transacción con bloqueo a nivel de base de datos, no solo con una validación en la capa de aplicación. Queda registrado como **control doble (postulación + otorgamiento)** para que no se pierda en el backlog de HU-16.1.
- La consulta de becas por persona y periodo se apoya en el historial de becas (RF-18; ver HU-18.1).

#### Flujo de la HU

1. El postulante pulsa "Postular" en una convocatoria abierta (HU-01.1).
2. El sistema consulta sus becas en el periodo de la convocatoria (HU-18.1), considerando como activas las *Otorgada*, *Activa*, *Suspendida* y *Renovada*.
3. Si no tiene una beca activa en ese periodo, la postulación continúa.
4. Si tiene una beca activa y el programa **no** permite acumulación, el sistema bloquea la postulación y muestra el motivo.
5. Si tiene una beca activa y el programa **sí** permite acumulación, el sistema evalúa además la etiqueta "completo"/"parcial" de los programas involucrados.
6. Si la beca activa y el programa al que postula corresponden ambos a programas "completos", el sistema bloquea la postulación y registra el motivo.
7. El sistema suma los montos por periodo de las becas activas de la persona en ese periodo y el monto de la beca del programa al que postula, y compara el resultado con el límite del periodo (HU-14.2). Si lo supera, bloquea la postulación y registra el motivo.
8. En cualquier otro caso, la postulación continúa.

#### Flujos alternos y de error

- **FA-1 (paso 5) Programa con acumulación sin conflicto de etiqueta ni de tope:** la postulación continúa aunque tenga una beca activa.
- **FA-2 (paso 2) Beca *Otorgada* o *Renovada* en el mismo periodo:** cuentan igual que *Activa* y *Suspendida* (S6); bloquean salvo acumulación.
- **FA-3 (paso 3) Beca activa en un periodo distinto:** no bloquea; la postulación continúa.
- **FA-4 (paso 3) Beca en estado no activo (*Concluida*, *Revocada*, *Inactiva*) o postulación previa *Rechazada*, *Retirada* o *No elegible*:** no bloquea; la postulación continúa.
- **FA-5 (pasos 2-7) Otorgamiento simultáneo:** si una beca se otorga en el mismo instante en que se evalúa esta regla para otra postulación, el resultado debe ser consistente: una de las dos operaciones se resuelve primero y la otra ve el estado ya definido, nunca un estado contradictorio.
- **FA-6 (paso 7) Suma de becas por encima del límite del periodo:** el sistema no crea la postulación y registra el motivo. El mensaje que ve el postulante no está definido (ver *Preguntas abiertas*).
- **FA-7 (paso 7) Límite del periodo no registrado para su universidad:** ni el PRD ni las respuestas definen qué hace el sistema; esta HU no fija comportamiento (ver *Preguntas abiertas*).

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-01.2).*

#### Criterios de aceptación (13 escenarios)

##### Escenario 1: Sin beca activa

- **DADO QUE** María no tiene becas en el periodo 2026-2
- **CUANDO** postula a "Becas Ingeniería 2026-2"
- **ENTONCES** su postulación queda *Recibida*

##### Escenario 2: Beca activa en el mismo periodo

- **DADO QUE** María tiene una beca *Activa* en el periodo 2026-2 y "Becas Salud 2026-2" no permite acumulación
- **CUANDO** postula a "Becas Salud 2026-2"
- **ENTONCES** el sistema no crea la postulación y muestra "Ya tienes una beca activa en el periodo 2026-2. Este programa no permite acumular becas."

##### Escenario 3: Beca suspendida también cuenta

- **DADO QUE** la beca de María en 2026-2 está *Suspendida*
- **CUANDO** postula a otro programa sin acumulación en 2026-2
- **ENTONCES** el sistema bloquea la postulación con el mismo mensaje

##### Escenario 4: Una beca *Otorgada* también cuenta

- **DADO QUE** María tiene una beca *Otorgada*, aún no aceptada, en el periodo 2026-2, y "Becas Salud 2026-2" no permite acumulación
- **CUANDO** postula a "Becas Salud 2026-2"
- **ENTONCES** el sistema no crea la postulación y muestra "Ya tienes una beca activa en el periodo 2026-2. Este programa no permite acumular becas."

##### Escenario 5: Beca activa en otro periodo no bloquea

- **DADO QUE** María tiene una beca *Activa* en el periodo 2026-1 y ninguna beca en el periodo 2026-2
- **CUANDO** postula a "Becas Ingeniería 2026-2"
- **ENTONCES** su postulación queda *Recibida*

##### Escenario 6: Dos programas "completos" no se acumulan

- **DADO QUE** María tiene una beca *Activa* en el periodo 2026-2 de un programa marcado como "completo" y "Becas Deportivas 2026-2" permite acumulación pero también está marcado como "completo"
- **CUANDO** María postula a "Becas Deportivas 2026-2"
- **ENTONCES** el sistema no crea la postulación y registra que se aplicó la regla de no dos becas de programas "completos" activas y simultáneas, sin que "permite acumulación" la anule

##### Escenario 7: Una beca *Renovada* también cuenta

- **DADO QUE** María tiene una beca *Renovada* en el periodo 2026-2 y "Becas Salud 2026-2" no permite acumulación
- **CUANDO** postula a "Becas Salud 2026-2"
- **ENTONCES** el sistema no crea la postulación y muestra "Ya tienes una beca activa en el periodo 2026-2. Este programa no permite acumular becas."

##### Escenario 8: Programa con acumulación, sin conflicto de etiqueta

- **DADO QUE** María tiene una beca *Activa* en el periodo 2026-2 de un programa marcado como "parcial", "Becas Deportivas 2026-2" permite acumulación y también está marcado como "parcial", y la suma de las dos becas no supera el límite del periodo
- **CUANDO** María postula a "Becas Deportivas 2026-2"
- **ENTONCES** su postulación continúa y queda *Recibida*

##### Escenario 9: Beca revocada no bloquea una nueva postulación

- **DADO QUE** la beca de Carlos en el periodo 2026-2 está *Revocada*
- **CUANDO** Carlos postula a "Becas Salud 2026-2", que no permite acumulación
- **ENTONCES** su postulación continúa y queda *Recibida*

##### Escenario 10: Postulación previa retirada no bloquea

- **DADO QUE** la postulación previa de Carlos a "Becas Ingeniería 2026-1" quedó *Retirada* y Carlos no tiene ninguna beca en el periodo 2026-2
- **CUANDO** Carlos postula a "Becas Salud 2026-2"
- **ENTONCES** su postulación continúa y queda *Recibida*

##### Escenario 11: La suma de las becas supera el límite del periodo

- **DADO QUE** el límite de la suma de las becas de María en el periodo 2026-2 es 2.500, su beca *Activa* en ese periodo es de 2.000 y "Becas Deportivas 2026-2" permite acumulación, está marcado como "parcial" y su beca es de 1.000
- **CUANDO** María postula a "Becas Deportivas 2026-2"
- **ENTONCES** el sistema no crea la postulación y registra que se aplicó el tope a la suma de las becas del periodo

##### Escenario 12: Dos becas a la vez dentro del límite

- **DADO QUE** el límite de la suma de las becas de María en el periodo 2026-2 es 2.500, su beca *Activa* en ese periodo es de 2.000 y "Becas Deportivas 2026-2" permite acumulación, está marcado como "parcial" y su beca es de 400
- **CUANDO** María postula a "Becas Deportivas 2026-2"
- **ENTONCES** su postulación continúa y queda *Recibida*

##### Escenario 13: Beca *Inactiva* no bloquea una nueva postulación

- **DADO QUE** la beca de Carlos en el periodo 2026-2 está *Inactiva*
- **CUANDO** Carlos postula a "Becas Salud 2026-2", que no permite acumulación
- **ENTONCES** su postulación continúa y queda *Recibida*

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Capa 1 — bloqueo por programa sin acumulación | Regla | 2, 3, 4, 7 | CUBIERTA |
| Capa 2 — no dos becas de programas "completos" simultáneas, sin excepción por acumulación | Regla | 6 | CUBIERTA |
| Capa 3 — la suma de las becas del periodo no supera el límite | Regla | 11, 12 | CUBIERTA |
| Dos becas a la vez permitidas dentro de la regla de no doble beneficio | Regla | 8, 12 | CUBIERTA |
| Becas activas = *Otorgada*, *Activa*, *Suspendida*, *Renovada* | Regla | 2, 3, 4, 7 | CUBIERTA |
| Becas/postulaciones no activas no cuentan (*Concluida*, *Revocada*, *Rechazada*, *Retirada*, *No elegible*) | Regla | 9, 10 | CUBIERTA |
| Estado *Inactiva* no cuenta como beca activa | Regla | 13 | CUBIERTA (confirmado; *Inactiva* no es un estado final y su paso a *Activa* o *Concluida* lo decide el coordinador de programa cuando el becario sube los datos que le falten) |
| Filtro por periodo académico | Regla | 5 | CUBIERTA |
| Acumulación permitida sin conflicto de etiqueta ni de tope: no bloquea | Regla | 8, 12 | CUBIERTA |
| Configuración de "permite acumulación", etiqueta "completo"/"parcial" y monto por periodo | Regla | — | DELEGADA (HU-11.1) |
| Límite del periodo (costo de la matrícula de su universidad, confirmado) | Regla | — | DELEGADA (HU-14.2) |
| Consulta de becas por cédula | Regla | — | DELEGADA (HU-01.1, HU-18.1) |
| Control doble: revalidación transaccional al otorgar/aceptar | Regla (RNF concurrencia) | — | DELEGADA (HU-16.1) |
| Consulta de becas vía historial | Regla | — | DELEGADA (HU-18.1) |
| FA-1 Programa con acumulación sin conflicto de etiqueta ni de tope | FA | 8, 12 | CUBIERTA |
| FA-2 Beca *Otorgada* o *Renovada* cuenta igual que *Activa*/*Suspendida* | FA | 4, 7 | CUBIERTA |
| FA-3 Beca activa en periodo distinto no bloquea | FA | 5 | CUBIERTA |
| FA-4 Beca no activa (incl. *Inactiva*) o postulación previa no bloquea | FA | 9, 10, 13 | CUBIERTA |
| FA-5 Otorgamiento simultáneo (concurrencia) | FA | — | DELEGADA (HU-16.1) |
| FA-6 Suma por encima del límite del periodo | FA | 11 | CUBIERTA |
| FA-7 Límite del periodo no registrado | FA | — | PENDIENTE (comportamiento sin definir; ver pregunta abierta) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-01.2).*

## RF-02 — Publicación de convocatorias · *Debe tener*

### HU-02.1 Configurar el ciclo de postulación: fechas, cupo, criterios y documentos

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-02 | Debe tener | HU-11.1, HU-38.1, HU-37.1, HU-14.3 | HU-02.1, HU-02.2, HU-02.3 |

#### Narrativa

- Como coordinador de programa
- Quiero crear el ciclo de postulación de un programa con su fecha de apertura, su fecha de cierre y su cupo disponible, y definir sus criterios de elegibilidad adicionales y los documentos que pide, indicando cuáles son obligatorios
- Para preparar la convocatoria que luego se publicará, de modo que solo califiquen los postulantes que cumplen lo que exige el programa y que sepan qué deben adjuntar

#### Reglas de negocio

- Cada ciclo tiene una fecha de apertura y una fecha de cierre, que forman la ventana de postulación (RF-02). Por eso la fecha de cierre es **posterior** a la de apertura: fechas iguales o invertidas no son válidas.
- Las fechas del ciclo se registran e interpretan en **UTC-4**, y el día de cierre se incluye **hasta las 23:59**. La regla de la ventana sigue siendo de HU-02.2.
- El ciclo indica el cupo disponible (RF-02).
- El cupo del ciclo cuenta los **becarios nuevos de ese ciclo**; el cupo de becarios del programa (RF-11; ver HU-11.1) es el **total de becarios del programa**. Son dos cantidades distintas, no dos formas de medir la misma disponibilidad.
- El ciclo se liga a un periodo académico (RF-38; ver HU-38.1), que debe existir previamente.
- El ciclo abre un **periodo de becas común** a todas las universidades aliadas. Además, cada universidad tiene su propia fecha de inicio de inscripciones en ese periodo, que se registra por universidad y periodo en el convenio de la universidad (HU-14.3), no en el ciclo. El efecto de esa fecha —cerrar las disponibilidades de esa universidad— se aplica en HU-02.2.
- El ciclo no es visible para los postulantes hasta que se publica (HU-02.2).
- Cada programa puede definir criterios adicionales a los generales: promedio mínimo, carrera específica y umbral de ingreso (RF-02).
- **Decisión del cliente:** los criterios generales de elegibilidad los define el **coordinador de programa**.
- Los criterios adicionales son opcionales: un ciclo sin ellos solo aplica los generales (RF-02, "puede definir").
- La verificación automática usa estos criterios (HU-04.1), con el promedio que la universidad entrega por API o por registro manual (HU-04.1).
- Cada programa puede pedir un conjunto distinto de documentos, por ejemplo notas, constancia de ingresos, identificación o cartas de recomendación (RF-03).
- Cada documento se marca como obligatorio o no. La postulación no está completa hasta tener todos los obligatorios (RF-03; ver HU-03.1).
- **Permisos (RF-37; ver HU-37.1):** configurar el ciclo corresponde al coordinador **del programa** en cuestión. El control es por rol **y** por asignación coordinador↔programa, y debe reforzarse en el backend, no solo ocultando opciones en la interfaz.
- **Restricción técnica — extensibilidad del modelo de criterios:** los criterios de elegibilidad adicionales deben modelarse de forma extensible (p. ej. `criterio_ciclo(tipo, operador, valor)`), no como columnas fijas. La Fundación planea pasar de 12 a más de 25 programas y es previsible que la lista de criterios configurables crezca; un modelo rígido obligaría a migraciones de esquema recurrentes y a tocar código en vez de configuración (ver HU-04.1).
- **Restricción técnica — cupo del ciclo y cupo del programa:** ya no son dos fuentes de verdad de la misma disponibilidad. El cupo del programa mide becarios totales y el del ciclo, becarios nuevos; el disponible en tiempo real (RF-12) debe calcularse con esa distinción y reflejarse en HU-12.1 antes de implementarla.
- No aplican las restricciones de OTP ni de Firebase: son metadatos de configuración, no archivos ni movimientos de dinero.
- **Disponibilidad en periodos críticos (RNF):** la configuración ocurre antes de abrir el ciclo, fuera de la ventana de alto tráfico; no es un punto crítico de carga.
- **Sin definir en el PRD:** cuáles son los criterios generales y si el umbral de ingreso es un máximo o un mínimo.

#### Flujo de la HU

1. El coordinador abre un programa que coordina (HU-11.1) y elige "Nuevo ciclo".
2. Indica el periodo académico, la fecha de apertura, la fecha de cierre y el cupo disponible de becarios nuevos del ciclo, y guarda.
3. El sistema valida los datos y crea el ciclo sin publicarlo, con una ventana común a todas las universidades aliadas.
4. En el ciclo, elige "Criterios de elegibilidad" y activa los criterios adicionales que exige el programa.
5. Indica el valor de cada criterio activado y guarda. El sistema asocia los criterios al ciclo.
6. En el ciclo, elige "Documentos requeridos" y agrega un documento con su nombre.
7. Lo marca como obligatorio o no.
8. Repite para cada documento y guarda. El sistema asocia la lista al ciclo.

#### Flujos alternos y de error

- **FA-1 (paso 3) Falta un dato:** el sistema no crea el ciclo e indica qué dato falta. Si falta el cupo, muestra "Indica el cupo disponible del ciclo."
- **FA-2 (paso 3) Fechas invertidas o iguales:** el sistema no crea el ciclo y muestra "La fecha de cierre debe ser posterior a la fecha de apertura."
- **FA-3 (paso 5) Criterio sin valor:** el sistema no guarda e indica qué criterio no tiene valor, por ejemplo "Indica el valor de 'Promedio mínimo'."
- **FA-4 (paso 6) Documento sin nombre:** el sistema no lo agrega y muestra "Indica el nombre del documento."
- **FA-5 (paso 1, bloqueo por rol) Usuario sin permiso de coordinador:** el sistema no le ofrece crear ni modificar ciclos (RF-37; HU-37.1).
- **FA-6 (paso 1, bloqueo por asignación) Coordinador de otro programa:** el sistema le niega el acceso a la configuración del ciclo de un programa que no coordina.
- **FA-7 (paso 2) Periodo académico inexistente:** el sistema no crea el ciclo; el periodo debe existir previamente (HU-38.1).

#### Criterios de aceptación (14 escenarios)

##### Escenario 1: Ciclo creado

- **DADO QUE** Carlos coordina "Becas Ingeniería"
- **CUANDO** crea un ciclo para el periodo 2026-2 con apertura el 1 de octubre de 2026, cierre el 31 de octubre de 2026 y cupo de 20 becarios nuevos
- **ENTONCES** el sistema crea el ciclo sin publicar, con esas fechas y ese cupo de becarios nuevos del ciclo

##### Escenario 2: Cierre anterior a la apertura

- **DADO QUE** Carlos está creando un ciclo
- **CUANDO** indica apertura el 31 de octubre de 2026 y cierre el 1 de octubre de 2026
- **ENTONCES** el sistema no crea el ciclo y muestra "La fecha de cierre debe ser posterior a la fecha de apertura."

##### Escenario 3: Ciclo no publicado

- **DADO QUE** Carlos creó el ciclo 2026-2 de "Becas Ingeniería" y no lo ha publicado
- **CUANDO** un postulante consulta las convocatorias
- **ENTONCES** ese ciclo no aparece

##### Escenario 4: Criterios definidos

- **DADO QUE** Carlos abre el ciclo 2026-2 de "Becas Ingeniería"
- **CUANDO** activa "Promedio mínimo" con valor 14 y "Carrera específica" con "Ingeniería Civil", y guarda
- **ENTONCES** el sistema asocia ambos criterios al ciclo

##### Escenario 5: Ciclo sin criterios adicionales

- **DADO QUE** Carlos abre los criterios de elegibilidad del ciclo 2026-2 de "Becas Ingeniería"
- **CUANDO** guarda sin activar ningún criterio adicional
- **ENTONCES** el sistema guarda el ciclo sin criterios adicionales asociados

##### Escenario 6: Documentos requeridos definidos

- **DADO QUE** Carlos abre el ciclo 2026-2 de "Becas Ingeniería"
- **CUANDO** agrega "Identificación" como obligatorio y "Carta de recomendación" como no obligatorio, y guarda
- **ENTONCES** el ciclo queda con esos dos documentos y su marca de obligatorio

##### Escenario 7: Conjuntos distintos por programa

- **DADO QUE** "Becas Ingeniería" pide "Notas" y "Becas Salud" no lo pide
- **CUANDO** un postulante consulta cada convocatoria
- **ENTONCES** "Notas" aparece solo en la de "Becas Ingeniería"

##### Escenario 8: Fechas de apertura y cierre iguales

- **DADO QUE** Carlos está creando un ciclo
- **CUANDO** indica apertura el 1 de octubre de 2026 y cierre también el 1 de octubre de 2026
- **ENTONCES** el sistema no crea el ciclo y muestra "La fecha de cierre debe ser posterior a la fecha de apertura."

##### Escenario 9: Coordinador de otro programa no configura un ciclo ajeno

- **DADO QUE** Luis coordina "Becas Artes" y no "Becas Ingeniería"
- **CUANDO** intenta abrir la configuración del ciclo de "Becas Ingeniería"
- **ENTONCES** el sistema le niega el acceso y el ciclo no se modifica

##### Escenario 10: Falta el cupo disponible

- **DADO QUE** Carlos está creando un ciclo para "Becas Ingeniería" con apertura el 1 de octubre de 2026 y cierre el 31 de octubre de 2026
- **CUANDO** guarda sin indicar el cupo disponible
- **ENTONCES** el sistema no crea el ciclo y muestra "Indica el cupo disponible del ciclo."

##### Escenario 11: Usuario sin rol de coordinador no configura ciclos

- **DADO QUE** Ana es miembro del comité evaluador de "Becas Ingeniería" y no tiene rol de coordinador
- **CUANDO** intenta crear un ciclo para ese programa, incluso invocando la operación directamente
- **ENTONCES** el sistema le niega la operación y no se crea ningún ciclo

##### Escenario 12: Periodo académico inexistente

- **DADO QUE** el periodo "2027-1" todavía no existe en el calendario académico (HU-38.1)
- **CUANDO** Carlos intenta crear un ciclo de "Becas Ingeniería" para el periodo "2027-1"
- **ENTONCES** el sistema no crea el ciclo

##### Escenario 13: El cupo del ciclo cuenta becarios nuevos, no el total del programa

- **DADO QUE** "Becas Ingeniería" tiene un cupo de 20 becarios en total (HU-11.1) y ya tiene 15 becarios activos
- **CUANDO** Carlos crea el ciclo 2026-2 con un cupo de 5
- **ENTONCES** el ciclo queda con un cupo de 5 becarios nuevos de ese ciclo y el cupo de becarios del programa sigue siendo 20 en total

##### Escenario 14: El ciclo abre una ventana común y no registra fechas por universidad

- **DADO QUE** "Universidad A" inicia sus inscripciones del periodo 2026-2 el 10 de octubre de 2026 y "Universidad B" el 25 de octubre de 2026, registradas en sus convenios (HU-14.3)
- **CUANDO** Carlos crea el ciclo 2026-2 de "Becas Ingeniería" con apertura el 1 de octubre y cierre el 31 de octubre de 2026
- **ENTONCES** el ciclo queda con una sola ventana común del 1 al 31 de octubre, sin fechas propias por universidad, y esas fechas se siguen leyendo de los convenios

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Fecha de cierre posterior a la de apertura | Regla | 1, 2, 8 | CUBIERTA |
| Fechas del ciclo en UTC-4, día de cierre incluido hasta las 23:59 | Regla | — | DELEGADA (HU-02.2, Escenarios 10 y 11) |
| El ciclo indica el cupo disponible | Regla | 1, 10 | CUBIERTA |
| Cupo del ciclo = becarios nuevos; cupo del programa = becarios totales | Regla | 1, 13 | CUBIERTA |
| El ciclo se liga a un periodo académico que debe existir previamente | Regla | 1, 12 | CUBIERTA |
| Ventana común del ciclo; fechas de inscripción por universidad fuera del ciclo | Regla | 14 | CUBIERTA (el registro de la fecha es HU-14.3 y su efecto, HU-02.2) |
| Ciclo no visible hasta publicarse | Regla | 3 | CUBIERTA |
| Criterios adicionales opcionales | Regla | 4, 5 | CUBIERTA |
| Rol que define los criterios generales | Regla | — | PENDIENTE (confirmado que es el coordinador de programa; esta HU no implementa esa configuración, solo la referencia) |
| Documentos: conjunto distinto por programa, marca obligatorio/no | Regla | 6, 7 | CUBIERTA |
| Postulación no completa sin documentos obligatorios | Regla | — | DELEGADA (HU-03.1) |
| Permisos: solo coordinador del programa (rol **y** asignación) | Regla | 9, 11 | CUBIERTA |
| Extensibilidad del modelo de criterios | Regla (técnica) | — | NO PROBABLE |
| Cupo del ciclo y cupo del programa como magnitudes distintas en el disponible | Regla (técnica) | — | NO PROBABLE (efecto en HU-12.1) |
| Disponibilidad en periodos críticos (config. fuera de ventana de carga) | Regla (RNF) | — | NO PROBABLE |
| FA-1 Falta un dato (p. ej. cupo) | FA | 10 | CUBIERTA |
| FA-2 Fechas invertidas o iguales | FA | 2, 8 | CUBIERTA |
| FA-3 Criterio sin valor | FA | — | PENDIENTE (mensaje ya definido en FA-3; siguiente iteración) |
| FA-4 Documento sin nombre | FA | — | PENDIENTE (mensaje ya definido en FA-4; siguiente iteración) |
| FA-5 Usuario sin permiso de coordinador | FA | 11 | CUBIERTA |
| FA-6 Coordinador de otro programa | FA | 9 | CUBIERTA |
| FA-7 Periodo académico inexistente | FA | 12 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-02.1).*

### HU-02.2 Publicar la convocatoria y aceptar postulaciones solo dentro de su ventana

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-02 | Debe tener | HU-02.1, HU-37.1, HU-37.2, HU-14.3 | HU-02.4, HU-02.5 |

#### Narrativa

- Como coordinador de programa
- Quiero publicar la convocatoria de un ciclo para que los postulantes consulten sus fechas, criterios, documentos y cupo, y que el sistema solo acepte postulaciones dentro de su ventana y mientras la universidad del postulante no haya iniciado sus inscripciones
- Para que cada estudiante sepa si califica y qué debe presentar, y no envíe una postulación que la Fundación no aceptará

#### Reglas de negocio

- La Fundación publica el ciclo de postulación de cada programa con fechas, criterios de elegibilidad, documentos requeridos y cupo disponible (RF-02).
- Un ciclo sin publicar no es visible para los postulantes (RF-02).
- No se aceptan postulaciones antes de la fecha de apertura ni después de la fecha de cierre (RF-02). **Esta HU es la dueña de la regla de la ventana**; HU-01.1 la invoca en el momento del envío.
- Las fechas límite se interpretan en **UTC-4** y el **día de cierre se incluye hasta las 23:59**: una postulación enviada el día de cierre hasta las 23:59 UTC-4 se acepta; a partir de las 00:00 UTC-4 del día siguiente, no.
- El periodo de becas es **común** a todas las universidades aliadas, pero **las disponibilidades de una universidad se cierran cuando esa universidad inicia sus inscripciones**: desde su fecha de inicio de inscripciones, un estudiante de esa universidad ya no puede postular a ese ciclo, aunque la ventana común siga abierta. La fecha de inicio de inscripciones se registra por universidad y periodo en HU-14.3; la universidad del postulante viene de su perfil (HU-01.1).
- El cierre por universidad no adelanta ni modifica el cierre común del ciclo para las demás universidades.
- **Permisos (RF-37; ver HU-37.1):** publicar un ciclo corresponde al coordinador del programa; ningún otro rol puede publicarlo.
- **Verificación en dos pasos (restricción del cliente; ver HU-37.2):** el coordinador tiene acceso de decisión, por lo que su inicio de sesión exige un segundo factor además de la contraseña. El mecanismo y sus controles negativos (código inválido) se especifican y prueban en HU-37.2; esta HU los asume cumplidos.
- **Restricción técnica — una sola regla de dominio:** la validación de ventana se evalúa en dos puntos (catálogo de convocatorias y envío de la postulación, HU-01.1). Debe implementarse como una única función de dominio reutilizada, no duplicada, para que una corrección de huso horario o del criterio de inclusión del día de cierre no quede arreglada en un solo lugar. Esa misma función resuelve ahora los dos límites: la ventana común del ciclo y el cierre por universidad.
- **Restricción técnica — reloj y huso del servidor:** la comparación de fechas se hace siempre con la hora del servidor en UTC-4, nunca con la del cliente, para que un dispositivo desincronizado o en otro huso no produzca comportamientos inconsistentes cerca del límite.
- **Consistencia bajo concurrencia (RNF):** la verificación de la ventana y la creación de la postulación deben resolverse de forma atómica; el resultado debe ser siempre uno de los dos desenlaces posibles, nunca una postulación parcialmente creada.
- **Disponibilidad en periodos críticos (RNF):** el cierre de la ventana es el momento de mayor carga del ciclo. La consulta "¿está abierta la convocatoria?" debe ser barata (cacheable o indexada por fecha), no una validación costosa repetida en cada intento. Con el cierre por universidad, esa consulta pasa a depender también de la universidad del postulante: la clave de caché debe incluirla.

#### Flujo de la HU

1. El coordinador revisa el ciclo con sus fechas, cupo, criterios y documentos (HU-02.1).
2. Pulsa "Publicar".
3. El sistema publica la convocatoria.
4. El postulante consulta la lista de convocatorias publicadas.
5. Abre una y ve las fechas, los criterios, los documentos (obligatorios y no obligatorios) y el cupo disponible.
6. El sistema compara la fecha y hora del servidor en UTC-4 con la apertura y el cierre del ciclo, y con la fecha de inicio de inscripciones de la universidad del postulante en ese periodo (HU-14.3).
7. Si está dentro de la ventana y la universidad del postulante no ha iniciado sus inscripciones, el postulante puede postular (HU-01.1).
8. Si aún no abre, el sistema muestra la fecha de apertura y no permite postular.
9. Si ya cerró, el sistema no permite postular.
10. Si la universidad del postulante ya inició sus inscripciones, el sistema no permite postular aunque la ventana común siga abierta.

#### Flujos alternos y de error

- **FA-1 (paso 4) Sin convocatorias publicadas:** el sistema muestra "No hay convocatorias publicadas en este momento."
- **FA-2 (paso 7) La ventana cierra durante el llenado:** si el postulante envía después del cierre, el sistema no acepta la postulación y muestra el mensaje de cierre.
- **FA-3 (paso 2, bloqueo por rol) Usuario sin permiso de coordinador:** el sistema no le ofrece la opción "Publicar" (RF-37; HU-37.1).
- **FA-4 (paso 7, concurrencia) Cierre simultáneo al envío:** el sistema resuelve la verificación de ventana y la creación de la postulación como una sola operación atómica; el resultado es uno de los dos desenlaces, nunca un estado intermedio.
- **FA-5 (paso 10) La universidad del postulante ya inició sus inscripciones:** el sistema no acepta la postulación. El mensaje que ve el postulante no está definido (ver *Preguntas abiertas*).
- **FA-6 (paso 6) La universidad del postulante no tiene registrada su fecha de inicio de inscripciones para ese periodo:** ni el PRD ni las respuestas definen si se aplica solo la ventana común o se bloquea; esta HU no fija comportamiento (ver *Preguntas abiertas*).

#### Criterios de aceptación (13 escenarios)

##### Escenario 1: Convocatoria publicada y consultada

- **DADO QUE** el ciclo 2026-2 de "Becas Ingeniería" tiene fechas, cupo, criterios y documentos definidos
- **CUANDO** Carlos pulsa "Publicar" y María abre la convocatoria
- **ENTONCES** la convocatoria aparece en la lista de convocatorias publicadas y María ve la fecha de apertura, la fecha de cierre, los criterios de elegibilidad, los documentos requeridos con su marca de obligatorio y el cupo disponible

##### Escenario 2: Ciclo sin publicar

- **DADO QUE** el ciclo 2026-2 de "Becas Salud" no está publicado
- **CUANDO** María consulta las convocatorias
- **ENTONCES** "Becas Salud 2026-2" no aparece

##### Escenario 3: Sin convocatorias

- **DADO QUE** no hay convocatorias publicadas
- **CUANDO** María consulta las convocatorias
- **ENTONCES** el sistema muestra "No hay convocatorias publicadas en este momento."

##### Escenario 4: Dentro de la ventana

- **DADO QUE** "Becas Ingeniería 2026-2" abre el 1 de octubre y cierra el 31 de octubre de 2026, y hoy es 15 de octubre de 2026
- **CUANDO** María postula
- **ENTONCES** la postulación continúa (HU-01.1)

##### Escenario 5: Antes de la apertura

- **DADO QUE** hoy es 20 de septiembre de 2026
- **CUANDO** María intenta postular a "Becas Ingeniería 2026-2"
- **ENTONCES** el sistema no acepta la postulación y muestra "Esta convocatoria abre el 1 de octubre de 2026. Aún no puedes postular."

##### Escenario 6: Después del cierre

- **DADO QUE** hoy es 5 de noviembre de 2026
- **CUANDO** María intenta postular a "Becas Ingeniería 2026-2"
- **ENTONCES** el sistema no acepta la postulación y muestra "Esta convocatoria cerró el 31 de octubre de 2026. Ya no se aceptan postulaciones."

##### Escenario 7: Solo el coordinador publica la convocatoria

- **DADO QUE** Ana es evaluadora del comité de "Becas Ingeniería"
- **CUANDO** intenta publicar el ciclo 2026-2
- **ENTONCES** el sistema no le ofrece la opción "Publicar" y el ciclo sigue sin publicar

##### Escenario 8: Cierre de la ventana simultáneo al envío

- **DADO QUE** el envío de la postulación de María y el cierre de la ventana de "Becas Ingeniería 2026-2" ocurren en el mismo instante
- **CUANDO** el sistema procesa la verificación de la ventana y la creación de la postulación
- **ENTONCES** el resultado es uno solo de los dos posibles —postulación creada en *Recibida*, o rechazo con el mensaje de cierre— y nunca queda una postulación parcialmente creada

##### Escenario 9: El reloj del dispositivo del postulante no determina la ventana

- **DADO QUE** "Becas Ingeniería 2026-2" cerró el 31 de octubre de 2026, la fecha del servidor es el 2 de noviembre de 2026 y el reloj del dispositivo de María marca el 20 de octubre de 2026
- **CUANDO** María intenta postular
- **ENTONCES** el sistema no acepta la postulación y muestra "Esta convocatoria cerró el 31 de octubre de 2026. Ya no se aceptan postulaciones."

##### Escenario 10: El día de cierre se acepta hasta las 23:59

- **DADO QUE** "Becas Ingeniería 2026-2" cierra el 31 de octubre de 2026 y la hora del servidor es el 31 de octubre de 2026 a las 23:45 UTC-4
- **CUANDO** María envía su postulación
- **ENTONCES** el sistema la acepta y la postulación queda *Recibida*

##### Escenario 11: Pasada la medianoche del día de cierre ya no se acepta

- **DADO QUE** "Becas Ingeniería 2026-2" cierra el 31 de octubre de 2026 y la hora del servidor es el 1 de noviembre de 2026 a las 00:10 UTC-4
- **CUANDO** María envía su postulación
- **ENTONCES** el sistema no acepta la postulación y muestra "Esta convocatoria cerró el 31 de octubre de 2026. Ya no se aceptan postulaciones."

##### Escenario 12: Universidad que ya inició sus inscripciones

- **DADO QUE** "Becas Ingeniería 2026-2" cierra el 31 de octubre de 2026, "Universidad A" inicia sus inscripciones del periodo 2026-2 el 10 de octubre de 2026 (HU-14.3), María estudia en "Universidad A" y hoy es 15 de octubre de 2026
- **CUANDO** María intenta postular
- **ENTONCES** el sistema no acepta la postulación, porque las disponibilidades de "Universidad A" se cerraron el 10 de octubre de 2026

##### Escenario 13: Universidad que aún no inicia sus inscripciones

- **DADO QUE** "Becas Ingeniería 2026-2" cierra el 31 de octubre de 2026, "Universidad B" inicia sus inscripciones del periodo 2026-2 el 25 de octubre de 2026 (HU-14.3), Luis estudia en "Universidad B" y hoy es 15 de octubre de 2026
- **CUANDO** Luis postula
- **ENTONCES** la postulación continúa (HU-01.1) y queda *Recibida*

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Publicar el ciclo con fechas, criterios, documentos y cupo | Regla | 1 | CUBIERTA |
| Ciclo sin publicar no es visible | Regla | 2 | CUBIERTA |
| No se aceptan postulaciones fuera de la ventana (dueña de la regla) | Regla | 4, 5, 6, 9 | CUBIERTA |
| Día de cierre incluido hasta las 23:59 en UTC-4 | Regla | 10, 11 | CUBIERTA |
| Cierre de disponibilidades al iniciar las inscripciones de la universidad | Regla | 12, 13 | CUBIERTA (la fecha la registra HU-14.3) |
| El cierre por universidad no afecta a las demás universidades | Regla | 13 | CUBIERTA |
| Permisos: solo el coordinador publica | Regla | 7 | CUBIERTA |
| 2FA del coordinador | Regla (restricción cliente) | — | DELEGADA (HU-37.2) |
| Una sola función de dominio reutilizada (ventana común + cierre por universidad) | Regla (técnica) | — | NO PROBABLE |
| Reloj del servidor en UTC-4, nunca el del cliente | Regla (técnica con efecto observable) | 9, 10, 11 | CUBIERTA |
| Consistencia bajo concurrencia (verificación + creación atómica) | Regla (RNF) | 8 | CUBIERTA |
| Disponibilidad: consulta de ventana barata/cacheable por universidad | Regla (RNF) | — | NO PROBABLE |
| FA-1 Sin convocatorias publicadas | FA | 3 | CUBIERTA |
| FA-2 La ventana cierra durante el llenado | FA | 6, 11 | CUBIERTA |
| FA-3 Bloqueo por rol al publicar | FA | 7 | CUBIERTA |
| FA-4 Cierre simultáneo al envío (concurrencia) | FA | 8 | CUBIERTA |
| FA-5 Universidad que ya inició sus inscripciones | FA | 12 | CUBIERTA (sin mensaje fijado) |
| FA-6 Universidad sin fecha de inicio de inscripciones registrada | FA | — | PENDIENTE (comportamiento sin definir; ver pregunta abierta) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-02.2).*

## RF-03 — Carga de documentos de soporte · *Debe tener*

### HU-03.1 Cargar mis documentos de soporte y ver los que faltan

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-03 | Debe tener | HU-01.1, HU-02.1 | HU-03.1, HU-03.2 |

#### Narrativa

- Como postulante
- Quiero adjuntar a mi postulación los documentos que pide el programa y ver cuáles ya cargué y cuáles me faltan
- Para que mi postulación pueda quedar completa y ser evaluada, y saber si ya lo está

#### Reglas de negocio

- El postulante adjunta los documentos que pide el programa (RF-03; lista definida en HU-02.1).
- Una postulación no está completa para evaluación hasta que todos los documentos obligatorios estén cargados (RF-03).
- Los documentos no obligatorios no impiden completar la postulación (RF-03).
- Un documento obligatorio rechazado no cuenta como cargado hasta que se vuelve a subir (RF-03; ver HU-03.2).
- La postulación completa pasa a la verificación automática (HU-04.1), que es la dueña de la transición de estado.
- **Firebase (restricción del cliente) — cumplimiento literal:** los archivos binarios se guardan en la cuenta de Firebase que ya usa la Fundación; no se crea ningún almacenamiento de archivos paralelo. Esta restricción aplica a los **archivos**, no obliga a que el resto del dominio (postulaciones, estados, metadatos del documento) viva en Firebase. La decisión de dónde vive el núcleo de dominio —base relacional con la referencia al archivo en Firebase Storage, o todo el dominio en Firestore— debe tomarse **una sola vez como arquitectura base**, no historia por historia.
- **Dependencia operativa:** se requieren las credenciales de acceso a la cuenta de Firebase **ya existente** de la Fundación. Es una gestión con el cliente, no una decisión de diseño, y es un riesgo de calendario para esta HU y HU-03.2.
- **Accesibilidad (RNF):** la carga debe tolerar conexión intermitente. Firebase Storage soporta cargas reanudables de forma nativa; conviene aprovecharlo en vez de construir un mecanismo de reintento propio.
- **Confidencialidad (RNF):** los documentos (p. ej. constancia de ingresos, identificación) son tan sensibles como los datos socioeconómicos del perfil. El control de quién puede **descargar** un archivo debe replicar la misma regla de "solo el titular y quien evalúa o decide sobre esa postulación" (HU-01.1), y no basarse únicamente en los permisos por defecto del almacenamiento.
- **Disponibilidad en periodos críticos (RNF):** junto con HU-01.1 y HU-04.1, esta HU concentra el tráfico de las horas previas al cierre de cada ciclo.

#### Flujo de la HU

1. El postulante abre su postulación.
2. El sistema lista los documentos requeridos con su marca de obligatorio y su situación: cargado, faltante o rechazado con su motivo.
3. El postulante elige un documento requerido y adjunta el archivo.
4. El sistema guarda el archivo en la cuenta de Firebase de la Fundación, lo asocia a ese documento de su postulación y lo muestra como cargado.
5. El sistema indica si la postulación está completa o cuántos documentos obligatorios faltan.
6. Cuando se carga el último obligatorio, la postulación queda completa y sigue a la verificación (HU-04.1).

#### Flujos alternos y de error

- **FA-1 (paso 2) Ningún documento cargado:** todos los documentos aparecen como faltantes.
- **FA-2 (paso 4) Falla al guardar:** el documento no queda cargado y el sistema permite reintentar.
- **FA-3 (paso 1, bloqueo) Acceso horizontal entre postulantes:** un postulante no puede ver ni cargar documentos de la postulación de otro; el sistema niega el acceso sin exponer ningún archivo.
- **FA-4 (paso 2, bloqueo) Rol sin asignación al caso:** un evaluador no asignado a esa postulación no puede ver ni descargar sus documentos; el intento queda registrado para trazabilidad.

#### Criterios de aceptación (9 escenarios)

##### Escenario 1: Documento cargado en la cuenta de la Fundación

- **DADO QUE** la postulación de María pide "Constancia de ingresos"
- **CUANDO** adjunta el archivo de su constancia y luego el coordinador abre ese documento
- **ENTONCES** el sistema muestra "Constancia de ingresos" como cargado y el archivo se obtiene de la cuenta de Firebase de la Fundación

##### Escenario 2: Falla al guardar

- **DADO QUE** María adjunta su "Identificación"
- **CUANDO** el archivo no se puede guardar
- **ENTONCES** el documento sigue como faltante y el sistema muestra "No pudimos guardar el documento. Intenta de nuevo."

##### Escenario 3: Falta un obligatorio

- **DADO QUE** María cargó "Identificación" y le falta "Constancia de ingresos", ambos obligatorios
- **CUANDO** abre su postulación
- **ENTONCES** ve "Tu postulación está incompleta. Falta 1 documento obligatorio: Constancia de ingresos."

##### Escenario 4: Completa sin los no obligatorios

- **DADO QUE** María cargó todos los obligatorios y no cargó "Carta de recomendación", que no es obligatoria
- **CUANDO** abre su postulación
- **ENTONCES** ve "Tu postulación está completa." y la postulación sigue a la verificación

##### Escenario 5: Documento rechazado

- **DADO QUE** la "Identificación" de María fue rechazada con el motivo "Documento ilegible"
- **CUANDO** abre su postulación
- **ENTONCES** ve "Identificación" como rechazado con su motivo y la postulación como incompleta

##### Escenario 6: Sin documentos cargados

- **DADO QUE** María acaba de postular y no ha cargado documentos
- **CUANDO** abre su postulación
- **ENTONCES** ve todos los documentos requeridos como faltantes

##### Escenario 7: Un postulante no accede a los documentos de otro

- **DADO QUE** María y Carlos tienen postulaciones distintas, cada uno con sus propios documentos cargados
- **CUANDO** María intenta acceder a los documentos de la postulación de Carlos
- **ENTONCES** el sistema le niega el acceso y no expone ningún archivo de Carlos

##### Escenario 8: Evaluador no asignado no ve los documentos cargados

- **DADO QUE** Jorge es miembro del comité evaluador pero no está asignado a la postulación de María
- **CUANDO** Jorge intenta abrir la lista de documentos de María
- **ENTONCES** el sistema no le muestra los documentos ni permite descargarlos, y el intento queda registrado para trazabilidad

##### Escenario 9: La carga tolera una interrupción de conexión

- **DADO QUE** María está cargando el archivo de su "Constancia de ingresos" y la conexión se interrumpe durante la carga
- **CUANDO** la conexión se restablece y María continúa
- **ENTONCES** el sistema completa la carga sin que María tenga que volver a adjuntar el archivo desde cero, y el documento queda cargado

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| El postulante adjunta los documentos que pide el programa | Regla | 1 | CUBIERTA |
| Postulación no completa sin todos los obligatorios | Regla | 3 | CUBIERTA |
| Documentos no obligatorios no impiden completar | Regla | 4 | CUBIERTA |
| Documento rechazado no cuenta como cargado | Regla | 5 | CUBIERTA |
| Al completarse el último obligatorio, sigue a verificación | Regla | 4 | CUBIERTA (la transición es de HU-04.1) |
| Archivos en la cuenta de Firebase de la Fundación | Regla (restricción cliente) | 1 | CUBIERTA |
| Dependencia operativa: credenciales de Firebase | Riesgo operativo | — | NO PROBABLE |
| Accesibilidad: tolerar conexión intermitente en la carga | Regla (RNF) | 9 | CUBIERTA |
| Confidencialidad en la descarga (titular y quien evalúa/decide) | Regla (RNF) | 7, 8 | CUBIERTA |
| Disponibilidad en periodos críticos | Regla (RNF) | — | NO PROBABLE |
| FA-1 Ningún documento cargado | FA | 6 | CUBIERTA |
| FA-2 Falla al guardar | FA | 2 | CUBIERTA |
| FA-3 Acceso horizontal entre postulantes | FA | 7 | CUBIERTA |
| FA-4 Rol sin asignación al caso | FA | 8 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-03.1).*

### HU-03.2 Rechazar un documento con motivo y permitir que se vuelva a subir

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-03 | Debe tener | HU-03.1, HU-37.1 | HU-03.3, HU-03.4 |

#### Narrativa

- Como coordinador de programa
- Quiero rechazar un documento cargado indicando el motivo, y que el postulante pueda volver a subirlo antes de la fecha límite
- Para que el postulante sepa qué corregir y complete su postulación después de corregirlo

#### Reglas de negocio

- Un documento rechazado por el equipo debe indicar el motivo (RF-03). El motivo es obligatorio.
- El postulante puede volver a subir un documento rechazado antes de una fecha límite (RF-03). La fecha límite es configurable.
- La fecha límite se interpreta en **UTC-4** y el **día límite se incluye hasta las 23:59**: una resubida hecha el día de la fecha límite hasta las 23:59 UTC-4 se acepta; a partir de las 00:00 UTC-4 del día siguiente, no.
- El nuevo archivo se guarda en la cuenta de Firebase de la Fundación, versionando o reemplazando en esa misma cuenta (restricción del cliente; ver HU-03.1).
- La notificación de documentos faltantes está en HU-29.1.
- **Permisos (RF-37; ver HU-37.1):** el rechazo es una acción del equipo de la Fundación sobre una postulación **de su programa**. El postulante no puede ejecutarla, y un coordinador de otro programa tampoco. El control debe reforzarse en el backend, no solo en la interfaz.
- **Trazabilidad (RNF y RF-35):** el motivo del rechazo y la fecha límite quedan registrados de forma permanente; si un documento se rechaza más de una vez, cada rechazo debe quedar registrado sin sobrescribir el anterior ni borrar rastro.
- **Sin definir en el PRD:quién** del equipo de la Fundación rechaza un documento. El PRD solo dice "el equipo". Esta HU atribuye la acción al coordinador de programa como alcance mínimo verificable; si el área de evaluación determina que el comité también revisa documentos, la HU debe ampliarse a ese actor. Ver *Preguntas abiertas*.
- **Sin definir en el PRD:** en qué momento revisa el equipo los documentos. La verificación automática empieza al cargarse el último obligatorio (HU-03.1, HU-04.1), y el PRD no dice si se puede rechazar un documento de una postulación que ya pasó a *En verificación* o *En evaluación*, ni qué pasa con ella. Los escenarios usan postulaciones *Recibida*.
- **Sin definir en el PRD:** quién fija la fecha límite, si coincide con el cierre del ciclo y qué pasa con la postulación si vence sin resubida. *Nota técnica:* esta definición cambia la arquitectura de la historia. Si vencida la fecha límite la postulación debe cambiar de estado automáticamente, se necesita un proceso en segundo plano que barra las fechas límite vencidas —extensión del mecanismo que el RNF de "procesamiento en segundo plano" ya exige para notificaciones masivas y reportes grandes—; si queda igual hasta que alguien la revise manualmente, no se necesita ningún job. **Debe resolverse con el cliente antes de estimar esta historia.**

#### Flujo de la HU

1. El coordinador del programa abre un documento cargado de una postulación de su programa.
2. Elige "Rechazar".
3. Escribe el motivo.
4. Confirma. El sistema marca el documento como rechazado, con su motivo y la fecha límite para volver a subirlo, y registra el rechazo de forma permanente.
5. El postulante ve el documento rechazado con su motivo y su fecha límite en su lista de documentos (HU-03.1).
6. Adjunta un nuevo archivo.
7. El sistema verifica, con la hora del servidor en UTC-4, que la fecha límite no haya vencido; el día límite cuenta hasta las 23:59.
8. El sistema guarda el archivo en la cuenta de Firebase de la Fundación y el documento vuelve a quedar cargado.
9. Si era el último obligatorio pendiente, la postulación queda completa (HU-03.1).

#### Flujos alternos y de error

- **FA-1 (paso 3) Sin motivo:** el sistema no registra el rechazo y muestra "Indica el motivo del rechazo."
- **FA-2 (paso 7) Fecha límite vencida (a partir de las 00:00 UTC-4 del día siguiente al día límite):** el sistema no acepta el archivo y el documento sigue rechazado.
- **FA-3 (paso 2, bloqueo por rol) Postulante intentando rechazar su propio documento:** la acción no está disponible para su rol y el sistema la niega también si se invoca directamente.
- **FA-4 (paso 1, bloqueo por asignación) Coordinador de otro programa:** el sistema le niega rechazar documentos de una postulación que no corresponde a su programa.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-03.2).*

#### Criterios de aceptación (11 escenarios)

##### Escenario 1: Documento rechazado

- **DADO QUE** María cargó su "Identificación" y su postulación sigue *Recibida* porque le falta otro documento obligatorio
- **CUANDO** Carlos la rechaza con el motivo "Documento ilegible"
- **ENTONCES** el documento queda rechazado con ese motivo

##### Escenario 2: Rechazo sin motivo

- **DADO QUE** Carlos eligió "Rechazar" sobre la "Identificación" de María
- **CUANDO** confirma sin escribir el motivo
- **ENTONCES** el sistema no registra el rechazo y muestra "Indica el motivo del rechazo."

##### Escenario 3: El postulante ve el motivo

- **DADO QUE** Carlos rechazó la "Identificación" de María con el motivo "Documento ilegible"
- **CUANDO** María abre su postulación
- **ENTONCES** ve "Identificación" como rechazado, el motivo "Documento ilegible" y la fecha límite para volver a subirlo

##### Escenario 4: Resubida a tiempo

- **DADO QUE** la "Identificación" de María está rechazada con fecha límite el 20 de octubre de 2026, y hoy es 18 de octubre de 2026
- **CUANDO** María sube un nuevo archivo
- **ENTONCES** "Identificación" vuelve a quedar cargado

##### Escenario 5: Fecha límite vencida

- **DADO QUE** la fecha límite de la "Identificación" rechazada de María era el 20 de octubre de 2026, y hoy es 22 de octubre de 2026
- **CUANDO** María intenta subir un nuevo archivo
- **ENTONCES** el sistema no lo acepta y muestra "La fecha límite para volver a subir este documento venció el 20 de octubre de 2026."

##### Escenario 6: La resubida completa la postulación

- **DADO QUE** la "Identificación" rechazada era el único documento obligatorio pendiente de María
- **CUANDO** María la vuelve a subir a tiempo
- **ENTONCES** su postulación queda completa y sigue a la verificación (HU-04.1)

##### Escenario 7: El postulante no puede rechazar su propio documento

- **DADO QUE** María ve su documento "Identificación" como cargado
- **CUANDO** intenta ejecutar la acción "Rechazar" sobre él, incluso invocándola directamente
- **ENTONCES** el sistema le niega la operación y el documento sigue cargado

##### Escenario 8: Coordinador de otro programa no rechaza un documento ajeno

- **DADO QUE** Luis coordina "Becas Artes" y María postuló a "Becas Ingeniería"
- **CUANDO** Luis intenta rechazar un documento de la postulación de María
- **ENTONCES** el sistema le niega la operación y el documento sigue cargado

##### Escenario 9: El registro del rechazo queda permanente

- **DADO QUE** Carlos rechazó la "Identificación" de María el 15 de octubre de 2026 con el motivo "Documento ilegible"
- **CUANDO** Carlos consulta el historial de esa postulación
- **ENTONCES** ve el rechazo registrado con Carlos como autor, la fecha 15 de octubre de 2026 y el motivo "Documento ilegible", sin opción para editarlo ni eliminarlo (HU-35.1)

##### Escenario 10: Resubida el mismo día de la fecha límite

- **DADO QUE** la "Identificación" de María está rechazada con fecha límite el 20 de octubre de 2026 y la hora del servidor es el 20 de octubre de 2026 a las 23:30 UTC-4
- **CUANDO** María sube un nuevo archivo
- **ENTONCES** el sistema lo acepta e "Identificación" vuelve a quedar cargado

##### Escenario 11: Resubida pasada la medianoche del día límite

- **DADO QUE** la "Identificación" de María está rechazada con fecha límite el 20 de octubre de 2026 y la hora del servidor es el 21 de octubre de 2026 a las 00:10 UTC-4
- **CUANDO** María intenta subir un nuevo archivo
- **ENTONCES** el sistema no lo acepta y muestra "La fecha límite para volver a subir este documento venció el 20 de octubre de 2026."

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| El rechazo debe indicar motivo (obligatorio) | Regla | 1, 2 | CUBIERTA |
| El postulante ve el motivo y la fecha límite | Regla | 3 | CUBIERTA |
| Resubida antes de la fecha límite configurable | Regla | 4, 5 | CUBIERTA |
| Fecha límite en UTC-4, día límite incluido hasta las 23:59 | Regla | 10, 11 | CUBIERTA |
| La resubida puede completar la postulación | Regla | 6 | CUBIERTA |
| El nuevo archivo se guarda en la cuenta de Firebase de la Fundación | Regla (restricción cliente) | — | DELEGADA (HU-03.1, Escenario 1) |
| Notificación de documentos faltantes | Regla | — | DELEGADA (HU-29.1) |
| Permisos: solo el coordinador del programa rechaza | Regla | 7, 8 | CUBIERTA |
| Trazabilidad: el rechazo queda registrado de forma permanente | Regla (RNF y RF-35) | 9 | CUBIERTA |
| Misma función de dominio para la comparación de fechas que HU-02.2 | Regla (técnica) | — | NO PROBABLE |
| Rechazos múltiples no sobrescriben el anterior | Regla (RNF) | — | PENDIENTE (depende de la pregunta abierta sobre rechazar más de una vez y el cálculo de la nueva fecha límite) |
| FA-1 Rechazo sin motivo | FA | 2 | CUBIERTA |
| FA-2 Fecha límite vencida | FA | 5, 11 | CUBIERTA |
| FA-3 El postulante no puede rechazar su propio documento | FA | 7 | CUBIERTA |
| FA-4 Coordinador de otro programa no rechaza | FA | 8 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-03.2).*

## RF-04 — Verificación automática de elegibilidad básica · *Debe tener*

### HU-04.1 Verificar automáticamente la elegibilidad básica y mostrar el motivo

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-04 | Debe tener | HU-02.1, HU-03.1, HU-35.1, HU-14.1 | HU-04.1, HU-04.2 |

#### Narrativa

- Como coordinador de programa
- Quiero que cada postulación completa pase por una verificación automática de las reglas básicas de elegibilidad, y que el postulante pueda ver por qué su postulación quedó *No elegible*
- Para que solo las elegibles lleguen al comité, sin ocupar su tiempo con las que no cumplen, y que el postulante entienda por qué no califica

#### Reglas de negocio

- Antes de entrar a evaluación, cada postulación pasa por una verificación automática de las reglas básicas de elegibilidad (RF-04).
- Se verifican los criterios de la convocatoria (HU-02.1), la restricción de universidades y carreras del programa (RF-11, RF-14; ver HU-11.2) y que el postulante esté matriculado o admitido en una universidad aliada (RF-01).
- La verificación se hace sobre la **copia del perfil guardada al postular** (HU-01.1), no sobre el perfil en vivo.
- **Origen del promedio y de la información académica:** llegan de la universidad, por **API desde sus sistemas** o por **registro manual** de una persona que revisa las notas enviadas por el estudiante. El cliente amplía el alcance del PRD —que dejaba fuera la integración automática con los sistemas académicos— y la considera imprescindible. La vía que usa cada universidad se registra en su convenio (HU-14.1) y la historia de integración corresponde a RF-24 (Área 5).
- El criterio "promedio mínimo" de la convocatoria (HU-02.1) se evalúa con ese promedio, cualquiera sea su vía de entrada.
- **Transiciones (esta HU es la dueña de la máquina de estados de la postulación en esta etapa):** al quedar completa (HU-03.1), la postulación pasa de *Recibida* a *En verificación*. Si cumple, pasa a *En evaluación*. Si no cumple, pasa a *No elegible* con el motivo (RF-04; regla transversal de estados).
- Ninguna postulación salta etapas: no pasa a *En evaluación* sin verificación ni a *Otorgada* sin evaluación (regla transversal). Las demás historias del área no reimplementan este control: lo invocan.
- Las postulaciones *No elegible* no se asignan al comité (RF-04; ver HU-06.2).
- El postulante debe poder ver el motivo del rechazo en esta etapa (RF-04).
- Cada postulante ve solo el motivo de sus propias postulaciones (RNF de protección de datos personales).
- La notificación del resultado está en HU-29.1.
- **Trazabilidad (RF-35; ver HU-35.1):** aunque la decisión es automática, el resultado *No elegible* afecta al postulante, por lo que debe quedar registrado con el mismo mecanismo de auditoría permanente que las decisiones humanas —qué lo determinó, cuándo y por qué—, en lugar de un registro ad hoc paralelo.
- **Protección de datos personales (RNF):** la redacción del motivo no debe filtrar datos socioeconómicos de otros postulantes ni criterios internos sensibles del programa.
- **Restricción técnica — motor de reglas genérico:** los criterios a verificar son configurables por ciclo y deben evaluarse recorriendo la lista de criterios activados con su operador y valor (ver HU-02.1), no con lógica condicional fija por criterio; de lo contrario cada criterio nuevo que la Fundación agregue al crecer de 12 a más de 25 programas implica tocar código en vez de configuración.
- **Restricción técnica — el promedio es un dato externo:** la verificación depende de un dato que no produce la plataforma. El modelo debe registrar, junto al promedio, su vía de entrada (API o registro manual), su fecha y su origen, porque de eso depende la trazabilidad del resultado *No elegible* (RF-35) y porque las dos vías tienen latencias muy distintas.
- **Disponibilidad en periodos críticos (RNF):** la verificación se dispara por evento, al completarse el último documento obligatorio, lo que produce ráfagas de verificaciones simultáneas justo antes del cierre de cada ciclo (riesgo "Estacionalidad alta"). Recomendación: no ejecutarla de forma síncrona bloqueando la respuesta de la carga del documento; encolarla y resolverla en segundo plano (mismo mecanismo del RNF de procesamiento en segundo plano).
- **Sin definir en el PRD:** la lista completa de reglas básicas y si el umbral de ingreso es un máximo o un mínimo. **Decisión del cliente:** los criterios generales los define el **coordinador de programa** (ver HU-02.1). Ver *Preguntas abiertas*.

#### Flujo de la HU

1. La postulación queda completa (HU-03.1).
2. El sistema la pasa a *En verificación*.
3. El sistema compara la copia del perfil guardada al postular con las reglas de elegibilidad de la convocatoria, recorriendo los criterios activados del ciclo, y usa el promedio recibido de la universidad (por API o por registro manual) para el criterio de promedio mínimo.
4. Si cumple, el sistema la pasa a *En evaluación*.
5. Si no cumple, el sistema la pasa a *No elegible* y registra el motivo.
6. El sistema registra el resultado de la verificación de forma permanente en el historial de la postulación (HU-35.1).
7. El postulante abre su postulación y el sistema muestra su estado.
8. Si está *No elegible*, el sistema muestra el motivo registrado en la verificación.

#### Flujos alternos y de error

- **FA-1 (paso 1) Postulación incompleta:** sigue *Recibida* y no se verifica.
- **FA-2 (paso 8) Otro estado:** el sistema muestra el estado sin motivo de no elegibilidad.
- **FA-3 (pasos 2-5) Intento de salto de etapa:** cualquier intento de mover la postulación a un estado que no sea el inmediato siguiente es rechazado y la postulación conserva su estado.
- **FA-4 (paso 7) Postulación de otra persona:** el sistema no muestra la postulación ni su motivo a quien no es su titular ni tiene asignación sobre ella.
- **FA-5 (paso 3) Sin promedio disponible:** si la universidad no ha entregado el promedio por ninguna de las dos vías y el ciclo exige promedio mínimo, ni el PRD ni las respuestas definen qué hace la verificación; esta HU no fija comportamiento (ver *Preguntas abiertas*).

#### Criterios de aceptación (10 escenarios)

##### Escenario 1: Cumple las reglas

- **DADO QUE** "Becas Ingeniería 2026-2" exige la carrera "Ingeniería Civil" y la postulación completa de María indica "Ingeniería Civil"
- **CUANDO** el sistema la verifica
- **ENTONCES** la postulación pasa de *En verificación* a *En evaluación*

##### Escenario 2: No cumple una regla y el postulante ve el motivo

- **DADO QUE** "Becas Ingeniería 2026-2" exige la carrera "Ingeniería Civil" y la postulación completa de Luis indica "Derecho"
- **CUANDO** el sistema la verifica y Luis abre su postulación
- **ENTONCES** la postulación pasa a *No elegible* y Luis ve ese estado con el motivo "La carrera Derecho no está entre las carreras elegibles de esta convocatoria."

##### Escenario 3: Postulación incompleta

- **DADO QUE** a la postulación de María le falta un documento obligatorio
- **CUANDO** el sistema revisa las postulaciones pendientes de verificación
- **ENTONCES** la postulación de María sigue *Recibida*

##### Escenario 4: No se salta la evaluación

- **DADO QUE** la postulación de María está *Recibida*
- **CUANDO** se intenta pasarla a *Otorgada*
- **ENTONCES** el sistema lo impide y la postulación sigue *Recibida*

##### Escenario 5: Postulación elegible

- **DADO QUE** la postulación de María está *En evaluación*
- **CUANDO** María abre su postulación
- **ENTONCES** ve el estado *En evaluación* y ningún motivo de no elegibilidad

##### Escenario 6: Postulación de otra persona

- **DADO QUE** la postulación de Luis está *No elegible*
- **CUANDO** María intenta ver esa postulación
- **ENTONCES** el sistema no se la muestra

##### Escenario 7: El resultado de la verificación queda registrado

- **DADO QUE** la postulación de Luis quedó *No elegible* en la verificación automática
- **CUANDO** Carlos consulta el historial de esa postulación
- **ENTONCES** ve registrado el resultado de la verificación con su fecha y su motivo, y ese registro no se puede editar ni eliminar (HU-35.1)

##### Escenario 8: No se salta la verificación

- **DADO QUE** la postulación de María está *Recibida*
- **CUANDO** se intenta pasarla a *En evaluación* sin que haya pasado por la verificación
- **ENTONCES** el sistema lo impide y la postulación sigue *Recibida*

##### Escenario 9: El promedio llega por API de la universidad

- **DADO QUE** "Becas Ingeniería 2026-2" exige un promedio mínimo de 14, "Universidad A" entrega la información académica por API (HU-14.1) y el promedio de María recibido por esa API es 16
- **CUANDO** el sistema verifica su postulación completa
- **ENTONCES** la postulación pasa a *En evaluación* y queda registrado que el promedio usado llegó por la API de "Universidad A"

##### Escenario 10: El promedio se registra manualmente

- **DADO QUE** "Becas Ingeniería 2026-2" exige un promedio mínimo de 14, "Universidad B" no entrega información académica por API (HU-14.1) y el promedio de Luis, registrado manualmente a partir de las notas que envió, es 12
- **CUANDO** el sistema verifica su postulación completa
- **ENTONCES** la postulación pasa a *No elegible* con el motivo del promedio mínimo y queda registrado que el promedio usado se registró manualmente

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Verificación automática antes de evaluación | Regla | 1, 2 | CUBIERTA |
| La verificación usa la copia del perfil guardada al postular | Regla | — | DELEGADA (HU-01.1, Escenario 17) |
| Origen del promedio: API de la universidad o registro manual | Regla | 9, 10 | CUBIERTA (el convenio que fija la vía es HU-14.1; la integración, RF-24) |
| Evaluación del criterio "promedio mínimo" con ese promedio | Regla | 9, 10 | CUBIERTA |
| Transiciones *Recibida→En verificación→En evaluación/No elegible* | Regla | 1, 2, 3, 4, 8 | CUBIERTA |
| Ninguna postulación salta etapas | Regla | 4, 8 | CUBIERTA |
| *No elegible* no se asigna al comité | Regla | — | DELEGADA (HU-06.2) |
| El postulante ve el motivo de su propia postulación | Regla | 2, 5, 6 | CUBIERTA |
| Notificación del resultado | Regla | — | DELEGADA (HU-29.1) |
| Trazabilidad permanente del resultado (RF-35) | Regla | 7 | CUBIERTA |
| El motivo no filtra datos sensibles de otros ni criterios internos | Regla (RNF) | 2, 6 | CUBIERTA |
| Motor de reglas genérico y configurable | Regla (técnica) | — | NO PROBABLE |
| El promedio se registra con su vía de entrada, fecha y origen | Regla (técnica) | 9, 10 | CUBIERTA (efecto observable en el registro) |
| Disponibilidad: verificación en segundo plano | Regla (RNF) | — | NO PROBABLE |
| FA-1 Postulación incompleta sigue *Recibida* | FA | 3 | CUBIERTA |
| FA-2 Otro estado, sin motivo | FA | 5 | CUBIERTA |
| FA-3 Intento de salto de etapa | FA | 4, 8 | CUBIERTA |
| FA-4 Postulación de otra persona no se muestra | FA | 6 | CUBIERTA |
| FA-5 Sin promedio disponible | FA | — | PENDIENTE (comportamiento sin definir; ver pregunta abierta) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-04.1).*

### HU-04.2 Anular un resultado de no elegible en un caso límite

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-04 | Debe tener | HU-04.1, HU-35.1, HU-37.1 | HU-04.3 |

#### Narrativa

- Como coordinador de programa
- Quiero anular el resultado *No elegible* de una postulación de mi programa en un caso límite, con justificación
- Para que un caso que la verificación automática no resuelve bien llegue a evaluación

#### Reglas de negocio

- El coordinador de programa puede anular el resultado de la verificación en casos límite (RF-04).
- La anulación es una decisión que afecta al postulante: queda registrada con quién la tomó, cuándo y por qué (RF-35; ver HU-35.1). La justificación es obligatoria.
- Al anularse, la postulación pasa de *No elegible* a *En evaluación* (RF-04; regla transversal de estados). La anulación **solo tiene sentido desde *No elegible***: sobre una postulación en cualquier otro estado forzaría una transición inválida y debe bloquearse (regla transversal de estados; control aplicado en HU-04.1).
- **Permisos (RF-37; ver HU-37.1):** la anulación corresponde al coordinador **del programa** de esa postulación. Ni el comité evaluador ni un coordinador de otro programa pueden anularla. El control debe reforzarse a nivel de autorización en el backend, no solo ocultando el botón en la interfaz.
- **Trazabilidad estricta (RNF y RF-35):** el registro de la anulación no admite edición ni borrado posterior ("sin posibilidad de edición retroactiva silenciosa"). Técnicamente, una tabla de auditoría *append-only* (sin UPDATE ni DELETE a nivel de permisos de base de datos), no solo una convención de aplicación.
- **Dependencia de arquitectura crítica:** esta historia está bloqueada por el módulo de trazabilidad y auditoría de RF-35 (HU-35.1), que se define en el Área 6 y no se construye en esta área. La anulación *es* una decisión que debe trazarse con el mismo mecanismo que todas las demás. **Recomendación al equipo: tratar el módulo de auditoría como infraestructura base temprana**, no como una historia más del Área 6, para no bloquear el Área 1.

#### Flujo de la HU

1. El coordinador del programa abre una postulación *No elegible* de su programa y ve su motivo.
2. Elige "Anular resultado".
3. Escribe la justificación.
4. Confirma.
5. El sistema verifica que la postulación siga en *No elegible*, registra la decisión con su autor, su fecha y su justificación (HU-35.1) y pasa la postulación a *En evaluación*.

#### Flujos alternos y de error

- **FA-1 (paso 3) Sin justificación:** el sistema no anula el resultado y muestra "Indica la justificación para anular este resultado."
- **FA-2 (paso 1, bloqueo por estado) La postulación no está en *No elegible*:** la opción "Anular resultado" no está disponible y el sistema rechaza la operación si se invoca directamente. Cubre también la **doble anulación**: una postulación ya anulada está *En evaluación* y no puede anularse de nuevo.
- **FA-3 (paso 1, bloqueo por rol) Miembro del comité evaluador:** no tiene la opción "Anular resultado".
- **FA-4 (paso 1, bloqueo por asignación) Coordinador de otro programa:** el sistema le niega anular el resultado de una postulación que no corresponde a su programa.

#### Criterios de aceptación (7 escenarios)

##### Escenario 1: Resultado anulado

- **DADO QUE** la postulación de Luis está *No elegible*
- **CUANDO** Carlos anula el resultado con la justificación "Cambio de carrera aprobado por su universidad"
- **ENTONCES** la postulación pasa a *En evaluación* y la decisión queda registrada con Carlos, la fecha y la justificación

##### Escenario 2: Sin justificación

- **DADO QUE** Carlos eligió "Anular resultado" en la postulación de Luis
- **CUANDO** confirma sin escribir la justificación
- **ENTONCES** el sistema no anula el resultado y muestra "Indica la justificación para anular este resultado."

##### Escenario 3: Solo el coordinador anula

- **DADO QUE** Ana es miembro del comité evaluador
- **CUANDO** abre una postulación *No elegible*
- **ENTONCES** no tiene la opción "Anular resultado"

##### Escenario 4: No se anula una postulación que resultó elegible

- **DADO QUE** la postulación de Luis está *En evaluación* porque la verificación automática la encontró elegible
- **CUANDO** Carlos intenta "Anular resultado" sobre ella
- **ENTONCES** el sistema no permite la operación y la postulación sigue *En evaluación*

##### Escenario 5: Coordinador de otro programa no anula un resultado ajeno

- **DADO QUE** Luis postuló a "Becas Ingeniería" y su postulación está *No elegible*, y Ana coordina "Becas Salud"
- **CUANDO** Ana intenta anular ese resultado
- **ENTONCES** el sistema le niega la operación y la postulación sigue *No elegible*

##### Escenario 6: No se anula dos veces el mismo resultado

- **DADO QUE** Carlos ya anuló el resultado *No elegible* de la postulación de Luis y esta quedó *En evaluación*
- **CUANDO** Carlos intenta "Anular resultado" sobre ella de nuevo
- **ENTONCES** el sistema no permite la operación y la postulación sigue *En evaluación*

##### Escenario 7: El registro de la anulación no puede editarse ni eliminarse

- **DADO QUE** Carlos anuló el 12 de octubre de 2026 el resultado *No elegible* de la postulación de Luis con la justificación "Cambio de carrera aprobado por su universidad"
- **CUANDO** Carlos consulta el historial de esa postulación
- **ENTONCES** ve la anulación registrada con Carlos como autor, la fecha 12 de octubre de 2026 y esa justificación, sin opción para editarla ni eliminarla (HU-35.1)

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| El coordinador del programa puede anular el resultado | Regla | 1 | CUBIERTA |
| Justificación obligatoria y decisión registrada (quién, cuándo, por qué) | Regla | 1, 2 | CUBIERTA |
| Al anularse, la postulación pasa de *No elegible* a *En evaluación* | Regla | 1 | CUBIERTA |
| Transición solo válida desde *No elegible* | Regla | 4 | CUBIERTA |
| No hay doble anulación | Regla | 6 | CUBIERTA |
| Permisos: solo coordinador del programa; no comité, no otro coordinador | Regla | 3, 5 | CUBIERTA |
| Trazabilidad estricta: registro *append-only*, no editable ni eliminable | Regla (RNF y RF-35) | 7 | CUBIERTA (mecanismo en HU-35.1) |
| Dependencia del módulo de auditoría (HU-35.1) | Riesgo de arquitectura | — | NO PROBABLE |
| FA-1 Sin justificación | FA | 2 | CUBIERTA |
| FA-2 Bloqueo por estado / doble anulación | FA | 4, 6 | CUBIERTA |
| FA-3 Bloqueo por rol (comité evaluador) | FA | 3 | CUBIERTA |
| FA-4 Bloqueo por asignación (otro coordinador) | FA | 5 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-04.2).*

## RF-05 — Retiro voluntario de una postulación · *Debería tener*

### HU-05.1 Retirar mi postulación antes de que empiece la evaluación

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-05 | Debería tener | HU-01.1, HU-35.1 | HU-05.1, HU-05.2 |

#### Narrativa

- Como postulante
- Quiero retirar mi postulación antes de que empiece su evaluación, y que el retiro se conserve en el historial
- Para dejar de participar en la convocatoria si ya no me interesa, y que la Fundación tenga en cuenta el retiro en la reportería

#### Reglas de negocio

- El postulante puede retirar su postulación antes de que comience la evaluación; no después (RF-05).
- Se puede retirar en *Recibida* o *En verificación*; desde *En evaluación* ya no (RF-05 aplicado a los estados de la regla transversal).
- Al retirarla, la postulación pasa a *Retirada* (regla transversal de estados).
- El retiro queda en el historial para efectos de reportería (RF-05).
- Una postulación retirada no se elimina: conserva su estado *Retirada* y la fecha del retiro (RF-05).
- Los reportes que usan este historial están en HU-33.1 y HU-34.1.
- Solo el titular retira su propia postulación (RNF de confidencialidad y protección de datos personales; permisos en HU-37.1).
- **Trazabilidad (RNF y RF-35; ver HU-35.1):** el retiro y su fecha persisten de forma permanente, sin eliminar el registro de la postulación ni permitir su edición retroactiva.
- **Consistencia bajo concurrencia (RNF):** si el retiro se solicita en el mismo instante en que la postulación pasa a *En evaluación* (HU-04.1 o el inicio de la evaluación del Área 2), la validación del estado y la escritura del nuevo estado deben ocurrir dentro de una sola operación atómica con bloqueo, no como "leer, decidir en la aplicación y luego escribir". El resultado debe ser uno solo: retiro aceptado o retiro bloqueado.
- **Dependencia indirecta:** requiere que el inicio de la evaluación (Área 2, RF-06 en adelante) tenga un punto de transición de estado claro y atómico; sin ese límite bien definido, esta HU no puede validarse de forma fiable frente a condiciones de carrera.

#### Flujo de la HU

1. El postulante abre su propia postulación.
2. Elige "Retirar postulación".
3. El sistema pide confirmar.
4. El postulante confirma.
5. El sistema verifica el estado y, en una sola operación atómica, pasa la postulación a *Retirada*.
6. El sistema registra el retiro con su fecha en el historial de postulaciones del ciclo.
7. El coordinador consulta las postulaciones del ciclo y ve las retiradas con su estado *Retirada* y su fecha de retiro.

#### Flujos alternos y de error

- **FA-1 (paso 5) Evaluación iniciada:** el sistema no permite el retiro.
- **FA-2 (paso 4) El postulante cancela:** la postulación no cambia.
- **FA-3 (paso 7) Sin retiros:** el sistema indica que no hay postulaciones retiradas en el ciclo.
- **FA-4 (paso 1, bloqueo) Acceso horizontal entre postulantes:** un postulante no puede retirar la postulación de otro; el sistema niega la operación sin exponer datos del titular.
- **FA-5 (paso 5, bloqueo) Postulación en un estado que ya no admite retiro (*No elegible*, *Otorgada*, *Rechazada* o *Retirada*):** el sistema no la retira y conserva su estado. El mensaje que corresponde a cada uno de estos estados está pendiente de definición (ver *Preguntas abiertas*); no se reutiliza el de "ya está en evaluación".
- **FA-6 (paso 5, concurrencia) Retiro simultáneo al paso a *En evaluación*:** el resultado es uno solo —retiro aceptado o retiro bloqueado—, nunca ambos ni un estado intermedio.

#### Criterios de aceptación (10 escenarios)

##### Escenario 1: Retiro antes de la evaluación

- **DADO QUE** la postulación de María está *Recibida*
- **CUANDO** María la retira y confirma
- **ENTONCES** la postulación pasa a *Retirada*

##### Escenario 2: Evaluación ya iniciada

- **DADO QUE** la postulación de María está *En evaluación*
- **CUANDO** María intenta retirarla
- **ENTONCES** el sistema no la retira y muestra "Tu postulación ya está en evaluación. No se puede retirar."

##### Escenario 3: Retiro cancelado

- **DADO QUE** la postulación de María está *En verificación*
- **CUANDO** María elige "Retirar postulación" y cancela la confirmación
- **ENTONCES** la postulación sigue *En verificación*

##### Escenario 4: Retiro en el historial

- **DADO QUE** María retiró su postulación a "Becas Ingeniería 2026-2" el 10 de octubre de 2026
- **CUANDO** Carlos consulta las postulaciones de ese ciclo
- **ENTONCES** ve la postulación de María como *Retirada* con fecha 10 de octubre de 2026

##### Escenario 5: El retiro no se pierde con el tiempo

- **DADO QUE** María retiró su postulación en el ciclo 2026-1
- **CUANDO** Carlos consulta ese ciclo durante el periodo 2026-2
- **ENTONCES** la postulación de María sigue apareciendo como *Retirada*

##### Escenario 6: Ciclo sin retiros

- **DADO QUE** nadie retiró su postulación en "Becas Salud 2026-2"
- **CUANDO** Carlos consulta las postulaciones retiradas de ese ciclo
- **ENTONCES** el sistema muestra "No hay postulaciones retiradas en este ciclo."

##### Escenario 7: Un postulante no retira la postulación de otro

- **DADO QUE** María y Carlos son postulantes distintos, cada uno con su propia postulación *Recibida*
- **CUANDO** María intenta retirar la postulación de Carlos
- **ENTONCES** el sistema le niega la operación y la postulación de Carlos sigue *Recibida*

##### Escenario 8: Retiro simultáneo al paso a *En evaluación

- **DADO QUE** la postulación de María está *En verificación* y su retiro y su paso a *En evaluación* se solicitan en el mismo instante
- **CUANDO** el sistema procesa ambas operaciones
- **ENTONCES** la postulación queda en uno solo de los dos estados —*Retirada* o *En evaluación*—, de forma consistente y sin estados intermedios ni contradictorios

##### Escenario 9: Retiro exitoso desde *En verificación

- **DADO QUE** la postulación de María está *En verificación*
- **CUANDO** María la retira y confirma
- **ENTONCES** la postulación pasa a *Retirada*

##### Escenario 10: Bloqueo de retiro en un estado que ya no lo admite

- **DADO QUE** la postulación de María está *Otorgada*
- **CUANDO** María intenta retirarla
- **ENTONCES** el sistema no la retira y la postulación conserva su estado *Otorgada*

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Retirar antes de que comience la evaluación, no después | Regla | 1, 2, 9 | CUBIERTA |
| Se puede retirar desde *Recibida* o *En verificación*; no desde *En evaluación* | Regla | 1, 2, 9 | CUBIERTA |
| Al retirarse, pasa a *Retirada* | Regla | 1, 9 | CUBIERTA |
| Estados que ya no admiten retiro (*No elegible*, *Otorgada*, *Rechazada*, *Retirada*) | Regla | 10 | CUBIERTA |
| El retiro queda en el historial, no se elimina, conserva estado y fecha | Regla | 4, 5 | CUBIERTA |
| Reportes que usan este historial | Regla | — | DELEGADA (HU-33.1, HU-34.1) |
| Solo el titular retira su propia postulación | Regla (RNF) | 7 | CUBIERTA |
| Consistencia bajo concurrencia (operación atómica) | Regla (RNF) | 8 | CUBIERTA |
| Dependencia indirecta: punto de transición claro en el inicio de evaluación (Área 2) | Riesgo de arquitectura | — | NO PROBABLE |
| FA-1 Evaluación iniciada | FA | 2 | CUBIERTA |
| FA-2 El postulante cancela | FA | 3 | CUBIERTA |
| FA-3 Sin retiros en el ciclo | FA | 6 | CUBIERTA |
| FA-4 Acceso horizontal entre postulantes | FA | 7 | CUBIERTA |
| FA-5 Estado que ya no admite retiro | FA | 10 | CUBIERTA |
| FA-6 Concurrencia (retiro simultáneo al paso a evaluación) | FA | 8 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-05.1).*
