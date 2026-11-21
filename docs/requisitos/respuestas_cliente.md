# Respuestas del cliente — Rondas 1 a 5

> Última actualización: 21/11/2026 (Ronda 5). Estas respuestas **prevalecen sobre el PRD** cuando lo contradicen o lo completan. Las marcadas "Por confirmar" son detalles menores pendientes.

## Cambios al PRD (decisiones de mayor impacto)

### C1. Modelo de monto de la beca
- Se eliminan las coberturas **total** y **parcial** como forma de calcular el monto. **Todas las becas son un monto fijo por periodo** (ej.: 2.000 $ cubren ese semestre).
- El monto tiene dos orígenes:
  - **Beca nueva:** el monto lo fija el **programa**.
  - **Renovación:** el monto se asigna **a cada estudiante** ("después se le puede asignar más dinero").
- Afecta: RF-11, RF-12, RF-15, RF-19, RF-22, RF-23, RF-26 (HU-11.1, HU-12.x, HU-15.1, HU-19.1, HU-22.2, HU-23.1, HU-26.x).

### C2. Regla de no doble beneficio
- Cada programa se marca como **"completo"** o **"parcial"** (etiqueta usada solo para esta regla, no para calcular el monto).
- La **suma de las becas** de una persona en un periodo **no puede superar un límite**, por ejemplo el costo de su matrícula (HU-14.2).
- **Confirmado:** se aplican **las dos reglas a la vez**: no puede tener dos becas de programas "completos" simultáneas **y** la suma de sus becas en el periodo no puede superar **el costo de la matrícula** de su universidad en ese periodo (HU-14.2).
- Afecta: HU-01.2, HU-16.1, HU-18.1, regla transversal y supuesto S6.

### C3. Nuevo estado de beca: *Inactiva*
- Una beca no renovada queda *Inactiva*: no recibe dinero en el siguiente periodo.
- Una beca en pausa que vence sin reactivarse queda *Inactiva* y sin ingresos.
- **Confirmado:** *Inactiva* **no es final**. Desde *Inactiva* la beca puede volver a *Activa* o pasar a *Concluida*. *Concluida* se mantiene como estado final.
- **Confirmado:** el paso de *Inactiva* a *Activa* o a *Concluida* lo decide el **coordinador de programa** (el cliente lo describió como "quienes gestionan las becas"). Los requisitos para cada paso siguen sin definir.
- **Confirmado:** una beca *Inactiva* **no cuenta como beca activa** para la regla de no doble beneficio (ajusta el supuesto S6).
- Afecta: tabla de estados, HU-26.2, HU-27.1, HU-27.2, HU-38.2, HU-28.1, reportes.

### C4. Periodos por universidad
- Se abre un **periodo de becas común** (ej.: verano) y, **cuando cada universidad inicia sus inscripciones, se cierran las disponibilidades para esa universidad**.
- Cada universidad tiene sus propias fechas; hay que registrar la fecha de inicio de inscripciones por universidad y periodo.
- La lista de espera termina, para cada universidad, al iniciar su periodo de inscripción.
- Afecta: RF-02, RF-10, RF-14, RF-38 (HU-02.1, HU-02.2, HU-10.2, HU-14.1, HU-14.2, HU-38.1, HU-38.2).

### C5. Integración con universidades (amplía el alcance)
- El promedio y la información académica pueden llegar por **API desde las universidades** o por **registro manual** de una persona que revisa las notas enviadas por el estudiante.
- El cliente confirma que **se amplía el alcance** del PRD (que dejaba fuera la integración automática) y que es imprescindible.
- Requiere al menos una historia nueva de integración. Afecta: HU-04.1, HU-24.1, HU-24.2.

## Respuestas por pregunta

| # | Pregunta | Respuesta | Estado |
| :-- | :-- | :-- | :-- |
| 1 | Identificación única de una persona | Número de cédula | Resuelta |
| 2 | Qué puede ver y hacer cada rol | Se consultará al cliente | Pendiente |
| 3 | Acceso del enlace de universidad | Se le exige verificación en dos pasos | Resuelta (falta: quién crea su cuenta) |
| 4a | Canal de los códigos | Correo o SMS; **elige el usuario** | Resuelta |
| 4b | Vigencia del código | 15 minutos | Resuelta |
| 4c | Intentos fallidos | 3; después, **espera de 1 hora** | Resuelta |
| 4d | Reenvío del código | Sí | Por confirmar: ¿invalida el anterior? |
| 4e | Pérdida del medio del segundo factor | Recuperación por correo | Resuelta (ver nota de seguridad) |
| 4f | Canal para evaluadores externos | Correo | Resuelta |
| 5 | ¿El administrador financiero modifica datos de pago? | No; solo el becario | Resuelta |
| 6 | Primer registro de datos de pago | Lo hace el becario y **exige código** | Resuelta |
| 7 | Campos de los datos de pago | "Los necesarios" | Por confirmar la lista propuesta |
| 8 | Monto de la beca | Ver C1 | Resuelta |
| 9 | Modalidad de pago | La fija el programa | Resuelta |
| 10 | Cupo de convocatoria vs. del programa | Programa = becarios totales; convocatoria = nuevos del ciclo | Resuelta |
| 11 | ¿Las ofertas pendientes restan presupuesto? | Sí, hasta que el proceso se rechace definitivamente | Resuelta |
| 12 | Desembolsos con fecha durante una suspensión | Se pagan al reanudar | Resuelta (¿aplica también a la pausa?) |
| 13 | Origen del promedio | API o registro manual (ver C5) | Por confirmar: quién lo registra a mano |
| 14 | Criterios generales de elegibilidad | Los define el **coordinador de programa** | Resuelta |
| 15 | Total ponderado del evaluador | Se consultará al cliente | Pendiente |
| 16 | Inicio y cierre de periodos | Los abre y cierra **Dirección**; la apertura del periodo de evaluación **bloquea la rúbrica** | Resuelta |
| 17 | Evaluador con conflicto de interés | No puede evaluar; lo sustituye otro **miembro del comité** | Resuelta (¿se descartan sus puntuaciones previas?) |
| 18a | Beca no renovada | Pasa a *Inactiva* (C3) | Resuelta |
| 18b | Pausa vencida sin reactivar | Pasa a *Inactiva* (C3) | Resuelta |
| 18c | Llega la fecha de revisión de una suspensión | El sistema avisa al coordinador | Resuelta (¿también al becario?) |
| 18d | Duración de la lista de espera | Hasta el inicio de inscripciones de la universidad (C4) | Resuelta |
| 19 | ¿Incumplir la permanencia es incumplimiento grave? | Sí (permite revocar directamente) | Resuelta |
| 20 | Fechas límite | UTC-4; el día de cierre **se incluye hasta las 23:59** | Resuelta |
| 21 | Fechas de los periodos | Las define cada universidad (C4) | Resuelta |
| 22 | ¿Perfil actual o copia al postular? | Copia al postular; el perfil se puede actualizar mientras el periodo siga abierto | Por confirmar: ¿la actualización cambia la postulación ya enviada? |
| 23 | Varias postulaciones / volver a postular tras rechazo | Sí a ambas | Resuelta |
| 24 | ¿Dos becas a la vez? | Sí, dentro de la regla C2 | Resuelta |
| 25 | Formato y detalle de la auditoría externa | No están seguros | Pendiente |
| 26 | ¿El correo de suspensión incluye el motivo? | Sí | Resuelta |

## Nota de seguridad (4e)
Si el correo sirve a la vez para recibir el segundo factor y para recuperarlo, quien controle el correo de un usuario controla también su segundo factor. Conviene que el cliente acepte ese riesgo o añadir otra verificación al recuperar.

## Propuesta de campos de datos de pago (7) — para confirmar
Titular de la cuenta, cédula del titular, banco, tipo de cuenta, número de cuenta y, si aplica, datos de pago móvil (teléfono y banco).

## Ronda 4 — Estado *Inactiva*

| # | Pregunta | Respuesta | Estado |
| :-- | :-- | :-- | :-- |
| 27 | ¿Una beca *Inactiva* libera su cupo y su presupuesto? | **No.** Los mantiene ocupados, porque puede volver a *Activa* | Resuelta |
| 28 | ¿Qué requisitos debe cumplir el becario para salir de *Inactiva*? | Debe **subir los datos que le falten**; con eso el coordinador decide el paso a *Activa* o a *Concluida* | Resuelta |
| 29 | ¿Se puede revocar una beca *Inactiva*? | **Sí** | Resuelta |
| 29b | ¿Se pueden resolver los desembolsos fallidos de una beca *Inactiva*? | Sin respuesta explícita | Por confirmar |
| 30 | ¿Un becario con beca *Inactiva* sigue recibiendo recordatorios y puede enviar consultas? | **Sí** | Resuelta (interpretación de la respuesta "y sí"; confirmar) |

## Ronda 5 — Preguntas posteriores a la arquitectura v1.3

Registrada el 21/11/2026. La columna «Pendiente» remite a [`docs/arquitectura/pendientes-v1.3.md`](../arquitectura/pendientes-v1.3.md).

| # | Pendiente | Pregunta | Respuesta | Estado |
| :-- | :-- | :-- | :-- | :-- |
| 31 | A1 · HU-12.2 | ¿Cuándo se fija el monto de una renovación? | **Al enviar la solicitud de renovación** | Resuelta |
| 32 | A2 · HU-15.1 | ¿La reserva de una oferta cubre un periodo o toda la beca? | **Toda la duración de la beca** | Resuelta |
| 33 | A3 · HU-22.1 | ¿Se puede revocar una beca *Otorgada* o *Renovada*? | **No.** Solo se revoca desde *Activa* o *Suspendida* (y desde *Inactiva*, R29) | Resuelta |
| 34 | A4 · HU-10.2 | ¿Qué lleva una postulación a *Rechazada*? | Distintos motivos, pero **siempre lo decide el encargado**; no hay rechazo automático | Resuelta |
| 35 | A5 · HU-24.2 | ¿Qué fuente prevalece entre la API de la universidad y el registro manual? | **La API, si la universidad la tiene.** No todas tendrán API; las demás usan el registro manual | Resuelta |
| 36 | A6 · HU-33.2 | ¿El consentimiento para aparecer en el reporte del donante es global, por donante o por programa? ¿Se puede retirar? | **Por donante** y **no se puede retirar** | Resuelta |
| 37 | A7 · RF-24 | ¿Qué valores tiene el estado de matrícula? | **Texto libre**; ASCENSO debe interpretarlo y clasificarlo correctamente | Resuelta (el equipo define la clasificación) |
| 38 | A8 · C3 | ¿Cuándo pasa a *Inactiva* una beca no renovada? | **En la fecha de cierre** de la ventana de renovación | Resuelta |
| 39 | B1 · HU-01.2 | Si no se puede comprobar si la persona ya tiene una beca activa, ¿se rechaza la postulación o queda pendiente? | **Queda pendiente** hasta poder comprobarlo | Respondida; **no adoptada** (ver nota) |
| 40 | B2 · HU-24.3 | ¿Con qué dato identifican las universidades al estudiante? | **Cédula** | Respondida; **no adoptada** (ver nota) |
| 41 | B3 · HU-25.2 | ¿Las consecuencias por incumplimiento son llamado de atención, suspensión y revocación? | **Sí, en ese orden** | Resuelta |
| 42 | B4 | Si el código de un solo uso se valida pero la operación falla, ¿debe pedirse otro código? | **Sí**; el usuario puede volver a intentarlo después con un código nuevo | Resuelta |
| 43 | B5 · ETL | ¿Quién aprueba la migración de las planillas? | **Los administradores de más alta jerarquía** | Por confirmar: nombre exacto del rol y fecha de congelamiento de las planillas |
| 44 | B6 · HU-32.1 | ¿Una consulta puede ser general? | **No: siempre es sobre una postulación** | Respondida; **no adoptada** (ver nota) |

**Nota sobre R39, R40 y R44.** El equipo decidió el 21/11/2026 mantener lo que define la arquitectura v1.3 en estos tres puntos, aunque el cliente respondió otra cosa:

- R39: si no se puede comprobar el doble beneficio, la postulación **se rechaza** («fail closed», v1.3 §3.2).
- R40: las universidades envían el **código de estudiante** y S5 lo traduce a la persona.
- R44: el contrato de consultas **no exige** indicar la postulación.

Es la única excepción a la regla de que estas respuestas prevalecen sobre los demás documentos.

Las demás preguntas abiertas de las historias siguen registradas en el [Anexo B](historias-de-usuario/anexo-b-preguntas-abiertas.md).
