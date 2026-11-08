# Área 3 — Programas, Fondos y Otorgamiento de Becas

> Parte de las [Historias de Usuario · Plan B · versión final](README.md). Reglas técnicas en el [Anexo A](anexo-a-reglas-tecnicas.md); preguntas abiertas en el [Anexo B](anexo-b-preguntas-abiertas.md).

## RF-11 — Registro y configuración de programas de becas · *Debe tener*

### HU-11.1 Crear un programa de becas con su monto por periodo

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-11 | Debe tener | HU-38.1 | HU-11.1, HU-11.3 |

#### Narrativa

- Como coordinador de programa
- Quiero crear un programa de becas con su nombre, cupo de becarios y duración, y definir el monto fijo de la beca por periodo, la modalidad de pago y si el programa es "completo" o "parcial"
- Para empezar a gestionar ese programa en la plataforma y que los desembolsos de sus becarios se programen según ese monto y esa modalidad

#### Reglas de negocio

- Un programa se crea con nombre, cupo de becarios y duración (RF-11).
- Los programas se ligan a los periodos académicos registrados (RF-38; ver HU-38.1).
- Todas las becas son un monto fijo por periodo: las coberturas total y parcial se eliminan como forma de calcular el monto.
- El monto de una beca nueva lo fija el programa (por ejemplo, 2.000 cubren ese semestre).
- En una renovación el monto se asigna a cada estudiante, no lo fija el programa (ver HU-26.x).
- El programa fija la modalidad de pago de sus becas (RF-19: mensual, por semestre o pago directo a la universidad).
- El calendario y los montos de los desembolsos dependen del monto por periodo y de la modalidad de pago del programa (RF-19; ver HU-19.1).
- Cada programa se marca como "completo" o "parcial". Esa etiqueta se usa solo para la regla de no doble beneficio (ver HU-01.2, HU-16.1, HU-18.1) y no interviene en el cálculo del monto.
- El cupo de becarios del programa es el total de becarios del programa; el cupo de una convocatoria cuenta solo los becarios nuevos de ese ciclo (ver HU-02.1 y HU-12.1).
- Donantes y presupuesto, universidades y carreras, y condiciones de renovación se configuran en HU-11.2 (RF-11).
- El cupo de becarios, la duración y el monto de la beca por periodo son cantidades mayores que cero.
- Crear programas es una función del personal interno con esa responsabilidad; un evaluador o un becario no tiene acceso a ella (RF-37: acceso limitado a lo que corresponde a su función).

#### Flujo de la HU

1. El coordinador elige "Crear programa".
2. Ingresa el nombre, el cupo de becarios y la duración.
3. Elige, entre los periodos registrados, el periodo académico al que se liga el programa.
4. El sistema valida que estén todos los datos y que las cantidades sean mayores que cero, y guarda el programa, que aparece en la lista de programas.
5. El coordinador abre la configuración del programa e ingresa el monto de la beca por periodo.
6. Elige la modalidad de pago de las becas del programa.
7. Indica si el programa es "completo" o "parcial".
8. El sistema valida y guarda.
9. El programa muestra su monto por periodo, su modalidad de pago y su etiqueta, y queda listo para completar su configuración (HU-11.2).

#### Flujos alternos y de error

- FA-1 (paso 4) Falta un dato: el sistema no guarda el programa y señala el campo faltante.
- FA-2 (paso 3) No hay periodos registrados: el sistema no permite crear el programa y muestra "No hay periodos académicos registrados. Pide a Dirección que los registre."
- FA-3 (paso 5) Monto de la beca sin valor: el sistema no guarda y muestra "Indica el monto de la beca por periodo."
- FA-4 (paso 7) Sin etiqueta elegida: el sistema no guarda y muestra "Elige si el programa es completo o parcial."
- FA-5 (paso 4) Cupo en cero o negativo: el sistema no guarda y muestra "El cupo de becarios debe ser mayor que cero."
- FA-6 (paso 4) Duración en cero o negativa: el sistema no guarda y muestra "La duración del programa debe ser mayor que cero."
- FA-7 (paso 5) Monto de la beca en cero o negativo: el sistema no guarda y muestra "El monto de la beca por periodo debe ser mayor que cero."
- FA-8 (paso 1) Rol sin permiso: un evaluador o un becario que intenta crear un programa no accede a la función.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-11.1).*

#### Criterios de aceptación (11 escenarios)

##### Escenario 1: Creación de un programa con su monto por periodo

- **DADO QUE** el periodo 2026-2 está registrado
- **CUANDO** el coordinador crea "Becas Ingeniería" con cupo de 20 becarios y duración de 8 periodos, ligado a 2026-2, ingresa un monto de beca de 2.000 por periodo y guarda
- **ENTONCES** "Becas Ingeniería" aparece en la lista de programas con esos datos y muestra "Monto de la beca: 2.000 por periodo"

##### Escenario 2: Falta el cupo

- **DADO QUE** el coordinador llenó el nombre y la duración
- **CUANDO** guarda sin indicar el cupo de becarios
- **ENTONCES** el sistema no crea el programa y muestra "Indica el cupo de becarios."

##### Escenario 3: Sin periodos registrados

- **DADO QUE** no hay periodos académicos registrados
- **CUANDO** el coordinador elige "Crear programa"
- **ENTONCES** el sistema muestra "No hay periodos académicos registrados. Pide a Dirección que los registre." y no permite continuar

##### Escenario 4: Monto de la beca sin valor

- **DADO QUE** el coordinador configura "Becas Arte"
- **CUANDO** guarda sin ingresar el monto de la beca por periodo
- **ENTONCES** el sistema no guarda y muestra "Indica el monto de la beca por periodo."

##### Escenario 5: Cupo de becarios en cero

- **DADO QUE** el coordinador está creando "Becas Arte"
- **CUANDO** ingresa 0 como cupo de becarios y guarda
- **ENTONCES** el sistema no crea el programa y muestra "El cupo de becarios debe ser mayor que cero."

##### Escenario 6: Monto de la beca en cero

- **DADO QUE** el coordinador configura "Becas Arte"
- **CUANDO** ingresa 0 como monto de la beca por periodo y guarda
- **ENTONCES** el sistema no guarda y muestra "El monto de la beca por periodo debe ser mayor que cero."

##### Escenario 7: Un evaluador no puede crear programas

- **DADO QUE** Marta tiene el rol de evaluadora
- **CUANDO** intenta abrir "Crear programa"
- **ENTONCES** el sistema le niega el acceso a esa función

##### Escenario 8: Duración del programa en cero

- **DADO QUE** el coordinador está creando "Becas Arte"
- **CUANDO** ingresa 0 como duración del programa y guarda
- **ENTONCES** el sistema no crea el programa y muestra "La duración del programa debe ser mayor que cero."

##### Escenario 9: Programa marcado como "completo"

- **DADO QUE** el coordinador configura "Becas Ingeniería"
- **CUANDO** indica que el programa es "completo" y guarda
- **ENTONCES** el programa queda marcado como "completo", esa etiqueta queda disponible para la regla de no doble beneficio (HU-01.2) y el monto de la beca por periodo no cambia

##### Escenario 10: Sin etiqueta de programa completo o parcial

- **DADO QUE** el coordinador configura "Becas Arte"
- **CUANDO** guarda sin indicar si el programa es completo o parcial
- **ENTONCES** el sistema no guarda y muestra "Elige si el programa es completo o parcial."

##### Escenario 11: Modalidad de pago fijada por el programa

- **DADO QUE** el coordinador configura "Becas Arte"
- **CUANDO** elige "pago directo a la universidad" como modalidad de pago y guarda
- **ENTONCES** el programa muestra esa modalidad de pago y es la que usa la programación de desembolsos de sus becarios (HU-19.1)

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Se crea con nombre, cupo y duración | Regla | 1, 2 | CUBIERTA |
| Se liga a un periodo académico registrado | Regla | 1, 3 | CUBIERTA |
| Todas las becas son un monto fijo por periodo; el de una beca nueva lo fija el programa | Regla | 1, 4 | CUBIERTA |
| El monto de una renovación se asigna a cada estudiante | Regla | — | DELEGADA (ver HU-26.x) |
| El programa fija la modalidad de pago | Regla | 11 | CUBIERTA (captura y visualización; ver pregunta abierta sobre opciones y obligatoriedad) |
| Etiqueta "completo" o "parcial" del programa | Regla | 9, 10 | CUBIERTA |
| Aplicación de la regla de no doble beneficio con esa etiqueta | Regla | — | DELEGADA (ver HU-01.2, HU-16.1, HU-18.1) |
| Cupo del programa = becarios totales; cupo de convocatoria = nuevos del ciclo | Regla | 1 | CUBIERTA (el cupo por ciclo, DELEGADO a HU-02.1; ver HU-12.1) |
| Cupo de becarios > 0 | Regla | 5 | CUBIERTA |
| Duración > 0 | Regla | 8 | CUBIERTA |
| Monto de la beca por periodo > 0 | Regla | 6 | CUBIERTA |
| Solo personal interno crea programas (RF-37) | Regla | 7 | CUBIERTA |
| Cálculo de desembolsos según el monto por periodo y la modalidad de pago | Regla | — | DELEGADA (ver HU-19.1) |
| Donantes, universidades/carreras, renovación | Regla | — | DELEGADA (ver HU-11.2) |
| 2FA de inicio de sesión | Técnica | — | DELEGADA (autenticación transversal, fuera de esta área) |
| FA-1 Falta un dato | FA | 2 | CUBIERTA |
| FA-2 Sin periodos registrados | FA | 3 | CUBIERTA |
| FA-3 Monto de la beca sin valor | FA | 4 | CUBIERTA |
| FA-4 Sin etiqueta completo/parcial | FA | 10 | CUBIERTA |
| FA-5 Cupo en cero o negativo | FA | 5 | CUBIERTA |
| FA-6 Duración en cero o negativa | FA | 8 | CUBIERTA |
| FA-7 Monto de la beca en cero o negativo | FA | 6 | CUBIERTA |
| FA-8 Rol sin permiso | FA | 7 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-11.1).*

### HU-11.2 Configurar los donantes, la elegibilidad y la renovación del programa

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-11 | Debe tener | HU-11.1, HU-13.2, HU-14.1 | HU-11.2, HU-11.4, HU-11.5 |

#### Narrativa

- Como coordinador de programa
- Quiero asociar sus donantes al programa y ver su presupuesto total junto con el aporte de cada donante por separado, restringir el programa a universidades o carreras específicas y registrar sus condiciones de permanencia y de renovación
- Para saber con qué fondos cuenta el programa y de quién provienen, dirigir la beca a la población que el programa quiere atender y que la oferta informe esas condiciones y el seguimiento académico las evalúe

#### Reglas de negocio

- El presupuesto total del programa es la suma de los aportes de sus donantes; no se ingresa a mano (S8).
- Un programa puede tener más de un donante; el aporte de cada uno se registra por separado (RF-11). Los aportes se registran en HU-13.2.
- Un mismo donante se asocia una sola vez al programa; sus aportes sucesivos se suman en su propia línea (RF-11, RF-13).
- El total otorgado no puede superar el presupuesto asignado; se controla en HU-12.1 (RF-11).
- Un programa puede restringir la elegibilidad a universidades o carreras específicas (RF-11, RF-14).
- **Decisión del cliente:** los criterios generales de elegibilidad de un programa los define el coordinador de programa; "curador", mencionado en una versión anterior del PRD, fue un error de nombre de ese mismo rol. Esta HU los asume correctamente desde la Fase 1.
- La restricción es opcional: si no se define, el programa no limita universidades ni carreras (RF-14, "puede restringir").
- Solo se eligen universidades aliadas registradas y carreras registradas como elegibles en esa universidad (RF-14; ver HU-14.1).
- La verificación automática de elegibilidad aplica la restricción de universidades y carreras del programa (ver HU-04.1).
- El postulante debe estar matriculado o admitido en una universidad aliada (RF-01; ver HU-01.1).
- Cada programa define sus condiciones de renovación (RF-11).
- El monto de una renovación se asigna a cada estudiante: el programa no fija el monto de las renovaciones en sus condiciones de renovación (ver HU-26.x).
- Las condiciones de permanencia incluyen promedio mínimo, horas de servicio si aplica y reportes exigidos (RF-15).
- El promedio mínimo y las horas de servicio son cantidades numéricas no negativas.
- La duración máxima de una pausa es configurable por programa (RF-27; ver HU-27.1).
- Una pausa que vence sin reactivarse deja la beca *Inactiva* y sin ingresos (ver HU-27.2).
- Estas condiciones se incluyen en la oferta (RF-15; ver HU-15.1) y se evalúan al recibir el reporte académico (RF-25; ver HU-25.1).
- La aprobación de la renovación depende del cumplimiento académico y del presupuesto disponible (RF-26; ver HU-26.2). La ventana de renovación se define en HU-26.1 y la política de consecuencias en HU-25.1.
- **Confidencialidad:** no aplica a esta historia; la restricción de universidad/carrera y las condiciones de permanencia no son datos socioeconómicos de un postulante (RNF de confidencialidad).
- **Sin definir en el PRD:** qué contienen las "condiciones de renovación" de RF-11 además de las condiciones de permanencia. Esta HU las registra como una descripción del programa.

#### Flujo de la HU

1. El coordinador abre la configuración del programa.
2. Asocia uno o más donantes registrados (HU-13.1).
3. El sistema muestra cada donante con su aporte al programa en una línea separada (HU-13.2) y el presupuesto total del programa como la suma de esos aportes (S8).
4. El coordinador elige una o más universidades aliadas y, para cada universidad, puede elegir carreras entre sus carreras elegibles.
5. El sistema guarda la restricción y el programa muestra las universidades y carreras a las que está restringido.
6. El coordinador ingresa el promedio mínimo e indica si aplican horas de servicio (y cuántas) y los reportes exigidos.
7. Indica la duración máxima de una pausa (RF-27).
8. Describe las condiciones de renovación.
9. El sistema valida y guarda.

#### Flujos alternos y de error

- FA-1 (paso 2) No hay donantes registrados: el sistema muestra "No hay donantes registrados. Registra uno primero." (HU-13.1).
- FA-2 (paso 3) Donante sin aportes: el donante aparece con la leyenda "Sin aportes registrados".
- FA-3 (paso 4) Sin restricción: si el coordinador no elige universidades ni carreras y guarda, el programa muestra "Sin restricción de universidad ni carrera".
- FA-4 (paso 4) Universidad sin carreras elegibles: la lista de carreras aparece vacía con "UNIMET no tiene carreras elegibles registradas."; la restricción puede quedar solo a nivel de universidad.
- FA-5 (paso 6) No aplican horas de servicio: la condición queda como "No aplica" y el programa muestra "Horas de servicio: no aplica".
- FA-6 (paso 6) Promedio mínimo negativo: el sistema no guarda y muestra "El promedio mínimo no puede ser negativo."
- FA-7 (paso 6) Horas de servicio negativas: el sistema no guarda y muestra "Las horas de servicio no pueden ser negativas."
- FA-8 (paso 2) Donante ya asociado: el sistema no duplica la asociación y muestra "Este donante ya está asociado al programa."

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-11.2).*

#### Criterios de aceptación (14 escenarios)

##### Escenario 1: Programa con dos donantes

- **DADO QUE** "Becas Ingeniería" tiene aportes registrados de "Empresa Alfa" (30.000) y "Juan Pérez" (20.000)
- **CUANDO** el coordinador abre el programa
- **ENTONCES** ve a cada donante con su aporte en una línea separada y un presupuesto total de 50.000 (S8)

##### Escenario 2: Un nuevo aporte actualiza el presupuesto total

- **DADO QUE** "Becas Ingeniería" tiene un presupuesto total de 50.000
- **CUANDO** se registra un aporte de 10.000 de "Empresa Alfa" al programa (HU-13.2)
- **ENTONCES** el presupuesto total pasa a 60.000 y el nuevo aporte aparece en la línea de "Empresa Alfa"

##### Escenario 3: Sin donantes registrados

- **DADO QUE** no hay donantes registrados
- **CUANDO** el coordinador intenta asociar un donante
- **ENTONCES** el sistema muestra "No hay donantes registrados. Registra uno primero."

##### Escenario 4: Restricción a una universidad y una carrera elegible

- **DADO QUE** UCAB está registrada con la carrera elegible "Ingeniería Informática" y "Medicina" no está registrada como carrera elegible de UCAB
- **CUANDO** el coordinador elige UCAB, abre la lista de carreras, restringe "Becas Ingeniería" a "Ingeniería Informática" y guarda
- **ENTONCES** "Medicina" no aparece entre las opciones y el programa muestra "Elegibles: UCAB — Ingeniería Informática"

##### Escenario 5: Sin universidades aliadas

- **DADO QUE** no hay universidades aliadas registradas
- **CUANDO** el coordinador intenta restringir el programa
- **ENTONCES** el sistema muestra "No hay universidades aliadas registradas."

##### Escenario 6: Registro de condiciones

- **DADO QUE** "Becas Ingeniería" no tiene condiciones registradas
- **CUANDO** el coordinador ingresa promedio mínimo 15, 40 horas de servicio, "reporte académico por periodo", una duración máxima de pausa de 4 meses y una descripción de sus condiciones de renovación, y guarda
- **ENTONCES** el programa muestra esas tres condiciones de permanencia, la duración máxima de pausa de 4 meses y sus condiciones de renovación

##### Escenario 7: Promedio inválido

- **DADO QUE** el coordinador escribe "quince" en el promedio mínimo
- **CUANDO** guarda
- **ENTONCES** el sistema no guarda y muestra "El promedio mínimo debe ser un número."

##### Escenario 8: Promedio mínimo negativo

- **DADO QUE** el coordinador configura las condiciones de "Becas Ingeniería"
- **CUANDO** ingresa -5 como promedio mínimo y guarda
- **ENTONCES** el sistema no guarda y muestra "El promedio mínimo no puede ser negativo."

##### Escenario 9: Horas de servicio negativas

- **DADO QUE** el coordinador indica que el programa exige horas de servicio
- **CUANDO** ingresa -10 como cantidad de horas y guarda
- **ENTONCES** el sistema no guarda y muestra "Las horas de servicio no pueden ser negativas."

##### Escenario 10: Donante duplicado en el mismo programa

- **DADO QUE** "Empresa Alfa" ya está asociada a "Becas Ingeniería"
- **CUANDO** el coordinador intenta asociarla de nuevo al mismo programa
- **ENTONCES** el sistema no duplica la asociación y muestra "Este donante ya está asociado al programa."

##### Escenario 11: Sin restricción de universidad ni carrera

- **DADO QUE** el coordinador configura "Becas Arte" y no elige ninguna universidad ni carrera
- **CUANDO** guarda la configuración de elegibilidad
- **ENTONCES** el programa muestra "Sin restricción de universidad ni carrera"

##### Escenario 12: Universidad sin carreras elegibles al restringir un programa

- **DADO QUE** UNIMET está registrada como universidad aliada y no tiene carreras elegibles registradas
- **CUANDO** el coordinador elige UNIMET para restringir "Becas Ingeniería" y abre la lista de carreras
- **ENTONCES** la lista aparece vacía con "UNIMET no tiene carreras elegibles registradas." y la restricción puede guardarse solo a nivel de universidad

##### Escenario 13: Donante sin aportes al programa

- **DADO QUE** "Juan Pérez" está asociado a "Becas Ingeniería" sin aportes registrados a ese programa
- **CUANDO** el coordinador abre el programa
- **ENTONCES** "Juan Pérez" aparece en la lista de donantes del programa con la leyenda "Sin aportes registrados"

##### Escenario 14: El programa no exige horas de servicio

- **DADO QUE** el coordinador configura las condiciones de permanencia de "Becas Arte"
- **CUANDO** indica que el programa no exige horas de servicio y guarda
- **ENTONCES** el programa muestra "Horas de servicio: no aplica"

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Presupuesto total = suma de aportes de donantes (S8) | Regla | 1, 2 | CUBIERTA |
| Varios donantes, aporte de cada uno en línea separada | Regla | 1 | CUBIERTA |
| Un mismo donante se asocia una sola vez; sus aportes se suman en su línea | Regla | 2, 10 | CUBIERTA |
| Restricción a universidades/carreras específicas | Regla | 4 | CUBIERTA |
| La restricción es opcional (si no se define, no limita) | Regla | 11 | CUBIERTA |
| Solo universidades aliadas y carreras registradas como elegibles | Regla | 4, 5, 12 | CUBIERTA |
| Condiciones de permanencia: promedio, horas, reportes | Regla | 6, 14 | CUBIERTA |
| Promedio mínimo y horas de servicio no negativos | Regla | 7, 8, 9 | CUBIERTA |
| Duración máxima de pausa configurable por programa (RF-27) | Regla | 6 | CUBIERTA (captura y visualización; ver pregunta abierta sobre su validación) |
| Pausa vencida sin reactivar: la beca queda *Inactiva* y sin ingresos | Regla | — | DELEGADA (ver HU-27.2) |
| Condiciones de renovación propias por programa | Regla | 6 | CUBIERTA |
| El monto de una renovación se asigna a cada estudiante, no lo fija el programa | Regla | — | DELEGADA (ver HU-26.x) |
| Total otorgado no supera el presupuesto asignado | Regla | — | DELEGADA (ver HU-12.1) |
| Condiciones incluidas en la oferta / evaluadas en el reporte académico | Regla | — | DELEGADA (ver HU-15.1, HU-25.1) |
| La solicitud de pausa no puede superar la duración máxima | Regla | — | DELEGADA (ver HU-27.1) |
| Aprobación de renovación según cumplimiento y presupuesto | Regla | — | DELEGADA (ver HU-26.1, HU-26.2) |
| Postulante matriculado o admitido en universidad aliada | Regla | — | DELEGADA (ver HU-01.1) |
| Verificación automática aplica la restricción del programa | Regla | — | DELEGADA (ver HU-04.1) |
| 2FA de inicio de sesión | Técnica | — | DELEGADA (autenticación transversal, fuera de esta área) |
| Contenido exacto de "condiciones de renovación" | Regla | — | NO PROBABLE (sin definir en el PRD; pregunta abierta) |
| FA-1 No hay donantes registrados | FA | 3 | CUBIERTA |
| FA-2 Donante sin aportes | FA | 13 | CUBIERTA |
| FA-3 Sin restricción | FA | 11 | CUBIERTA |
| FA-4 Universidad sin carreras elegibles | FA | 12 | CUBIERTA |
| FA-5 No aplican horas de servicio | FA | 14 | CUBIERTA |
| FA-6 Promedio mínimo negativo | FA | 8 | CUBIERTA |
| FA-7 Horas de servicio negativas | FA | 9 | CUBIERTA |
| FA-8 Donante ya asociado | FA | 10 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-11.2).*

## RF-12 — Gestión de presupuesto y disponibilidad de cupos · *Debe tener*

### HU-12.1 Consultar el presupuesto y el cupo disponibles y verificarlos antes de otorgar

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-12 | Debe tener | HU-11.2 | HU-12.1, HU-12.2 |

#### Narrativa

- Como coordinador de programa
- Quiero ver en cualquier momento cuánto presupuesto y cupo le queda a un programa, y que el sistema los verifique antes de confirmar un otorgamiento
- Para decidir otorgamientos con datos al día y no comprometer más dinero ni más becas de los que tiene el programa

#### Reglas de negocio

- La vista está disponible en cualquier momento (RF-12).
- La vista de presupuesto y cupo también la consulta el administrador financiero, cuya función es registrar y controlar los desembolsos sin exceder el presupuesto aprobado (RF-12, RF-23; Actores). La decisión de otorgar es del coordinador (Actores).
- El presupuesto y el cupo de un programa son información interna de la Fundación: un postulante o un becario no accede a esa vista (RF-37: acceso limitado a lo que corresponde a su función).
- El presupuesto total del programa es la suma de los aportes de sus donantes (S8; ver HU-11.2).
- El total otorgado cuenta contra el presupuesto asignado y no puede superarlo (RF-11).
- El monto de una beca nueva es el monto fijo por periodo que fija su programa (ver HU-11.1 y HU-15.1); no se calcula a partir de una cobertura total o parcial.
- Otorgar una nueva beca verifica que haya presupuesto y cupo suficientes antes de confirmarse (RF-12). La verificación la define esta HU y se dispara al confirmar la oferta de beca (ver HU-15.1).
- Un monto de beca exactamente igual al presupuesto disponible sí alcanza: la verificación compara "no superar", no "dejar margen" (RF-11, RF-12).
- Dos otorgamientos al mismo tiempo no pueden dejar el programa por encima de su presupuesto ni de su cupo (RNF de consistencia bajo concurrencia).
- Lo comprometido en renovaciones pendientes del ciclo actual cuenta como no disponible, no solo lo desembolsado (RF-12; ver HU-12.2).
- Lo comprometido en ofertas pendientes también cuenta como no disponible: una oferta pendiente resta presupuesto hasta que el proceso se rechace definitivamente.
- Una oferta pendiente ocupa cupo y presupuesto: si se rechaza o expira, el cupo y el presupuesto vuelven a estar disponibles (RF-16; ver HU-15.2 y HU-16.2).
- El cupo que muestra esta vista es el del programa, es decir, el total de becarios del programa; el cupo de una convocatoria cuenta solo los becarios nuevos de ese ciclo y se gestiona en HU-02.1.
- Una beca revocada devuelve su presupuesto y cupo al programa (RF-22; ver HU-22.2).
- Una beca *Inactiva* no libera su cupo ni su presupuesto: a diferencia de una revocada, los mantiene ocupados mientras esté en ese estado, porque desde *Inactiva* puede volver a *Activa* o pasar a *Concluida* (ver CONTEXTO COMÚN, tabla de estados).
- Si el presupuesto no alcanza para lo que sugiere el ranking, el caso se resuelve como en HU-10.1 (RF-10).

#### Flujo de la HU

1. El coordinador o el administrador financiero abre un programa y elige "Presupuesto y cupo".
2. El sistema muestra el presupuesto total, el total otorgado, lo comprometido en ofertas pendientes, lo comprometido en renovaciones pendientes y el presupuesto disponible.
3. El sistema muestra el cupo total, el cupo ocupado y el cupo disponible.
4. Las cifras reflejan las operaciones registradas hasta ese momento.
5. Cuando el coordinador confirma el otorgamiento a un postulante seleccionado (HU-15.1), el sistema verifica que quede al menos un cupo disponible.
6. El sistema verifica que el presupuesto disponible cubra el monto de la beca.
7. Si ambas verificaciones pasan, confirma el otorgamiento y actualiza el disponible dentro de la misma operación.
8. Si alguna falla, bloquea el otorgamiento y muestra qué falta.

#### Flujos alternos y de error

- FA-1 (paso 2) Programa sin aportes: el presupuesto total y el disponible son 0, y el sistema muestra "Este programa no tiene aportes registrados." (S8; ver HU-13.2).
- FA-2 (paso 5) Sin cupo: el sistema muestra "El programa no tiene cupo disponible."
- FA-3 (paso 6) Presupuesto insuficiente: el sistema muestra el disponible y el monto requerido.
- FA-4 (paso 7) Otorgamientos simultáneos: si dos coordinadores confirman a la vez y solo alcanza para uno, se confirma uno y el otro recibe el mensaje de FA-2 o FA-3.
- FA-5 (paso 1) Rol sin permiso: un postulante o un becario que intenta consultar el presupuesto o el cupo de un programa no accede a esa información.
- FA-6 (paso 6) Monto igual al disponible: la verificación pasa, el otorgamiento se confirma y el presupuesto disponible queda en 0.
- FA-7 (paso 5) Último cupo: el otorgamiento se confirma, el cupo disponible pasa a 0 y un segundo otorgamiento recibe el mensaje de FA-2 hasta que se libere cupo.
- FA-8 (paso 2) Oferta pendiente: su monto aparece en "Comprometido en ofertas pendientes" y se descuenta del presupuesto disponible hasta que el proceso se rechace definitivamente.
- FA-9 (pasos 2 y 3) Beca *Inactiva*: su cupo y su presupuesto siguen contando como ocupados, igual que una beca Activa; no se suman al disponible mientras la beca esté en ese estado.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-12.1).*

#### Criterios de aceptación (12 escenarios)

##### Escenario 1: Programa con becas otorgadas

- **DADO QUE** "Becas Ingeniería" tiene presupuesto de 50.000, becas otorgadas por 30.000, ninguna oferta pendiente, ninguna renovación pendiente y 12 de 20 cupos ocupados
- **CUANDO** el administrador financiero consulta el programa
- **ENTONCES** ve presupuesto disponible de 20.000 y cupo disponible de 8

##### Escenario 2: Oferta rechazada libera cupo y presupuesto

- **DADO QUE** "Becas Ingeniería" tiene cupo disponible de 0 y una oferta pendiente de 2.000 descontada del presupuesto disponible
- **CUANDO** el postulante rechaza la oferta (HU-16.2) y el coordinador vuelve a consultar
- **ENTONCES** ve cupo disponible de 1 y los 2.000 de vuelta en el presupuesto disponible

##### Escenario 3: Programa sin aportes

- **DADO QUE** "Becas Arte" no tiene aportes registrados
- **CUANDO** el coordinador consulta el programa
- **ENTONCES** ve presupuesto total y disponible de 0 y el mensaje "Este programa no tiene aportes registrados."

##### Escenario 4: Presupuesto y cupo suficientes

- **DADO QUE** "Becas Ingeniería" tiene 15.000 y 3 cupos disponibles
- **CUANDO** el coordinador confirma una beca de 1.200
- **ENTONCES** el otorgamiento se confirma y el programa queda con 13.800 y 2 cupos disponibles

##### Escenario 5: Presupuesto insuficiente

- **DADO QUE** "Becas Ingeniería" tiene 800 disponibles
- **CUANDO** el coordinador confirma una beca de 1.200
- **ENTONCES** el sistema no confirma y muestra "Presupuesto insuficiente: disponible 800, la beca requiere 1.200."

##### Escenario 6: Último cupo en disputa

- **DADO QUE** "Becas Ingeniería" tiene 1 cupo disponible
- **CUANDO** dos coordinadores confirman al mismo tiempo becas para postulantes distintos
- **ENTONCES** solo un otorgamiento se confirma, el otro recibe "El programa no tiene cupo disponible." y el cupo ocupado no supera el cupo del programa

##### Escenario 7: Un becario no consulta el presupuesto del programa

- **DADO QUE** María es becaria de "Becas Ingeniería"
- **CUANDO** intenta consultar el presupuesto o el cupo disponible del programa
- **ENTONCES** el sistema le niega el acceso a esa información

##### Escenario 8: Monto exactamente igual al disponible

- **DADO QUE** "Becas Ingeniería" tiene 1.200 de presupuesto disponible
- **CUANDO** el coordinador confirma una beca de exactamente 1.200
- **ENTONCES** el otorgamiento se confirma y el presupuesto disponible queda en 0

##### Escenario 9: Último cupo exacto

- **DADO QUE** "Becas Ingeniería" tiene 1 cupo disponible y presupuesto suficiente
- **CUANDO** el coordinador confirma un otorgamiento
- **ENTONCES** el otorgamiento se confirma, el cupo disponible pasa a 0 y un segundo intento de otorgamiento muestra "El programa no tiene cupo disponible."

##### Escenario 10: Una oferta pendiente resta presupuesto disponible

- **DADO QUE** "Becas Ingeniería" tiene un presupuesto total de 50.000, becas otorgadas por 30.000, ninguna renovación pendiente y una oferta pendiente de 2.000
- **CUANDO** el administrador financiero consulta el programa
- **ENTONCES** ve 2.000 en "Comprometido en ofertas pendientes" y un presupuesto disponible de 18.000

##### Escenario 11: Cupo del programa y cupo de la convocatoria

- **DADO QUE** "Becas Ingeniería" tiene un cupo de 20 becarios en total, 12 cupos ocupados y una convocatoria abierta con cupo de 5 becarios nuevos para el ciclo
- **CUANDO** el coordinador consulta "Presupuesto y cupo" del programa
- **ENTONCES** ve el cupo disponible del programa en 8, y el cupo de 5 nuevos del ciclo se gestiona en la convocatoria (HU-02.1) sin sustituir al del programa

##### Escenario 12: Una beca Inactiva no libera cupo ni presupuesto

- **DADO QUE** "Becas Ingeniería" tiene un cupo de 20 becarios, 12 cupos ocupados (de los cuales 1 corresponde a una beca *Inactiva* por pausa vencida) y ninguna oferta ni renovación pendiente
- **CUANDO** el coordinador consulta "Presupuesto y cupo" del programa
- **ENTONCES** el cupo disponible es 8 (el cupo de la beca *Inactiva* sigue contando como ocupado) y el presupuesto que esa beca ya tenía comprometido tampoco se suma al disponible

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Vista de presupuesto y cupo disponible en cualquier momento | Regla | 1, 3 | CUBIERTA |
| Las cifras reflejan las operaciones registradas hasta ese momento | Regla | 2, 4 | CUBIERTA |
| La consulta también la hace el administrador financiero; la decisión de otorgar es del coordinador | Regla | 1, 10 (administrador), 4–6, 8, 9 (coordinador) | CUBIERTA |
| Total otorgado no supera el presupuesto asignado | Regla | 5, 8 | CUBIERTA |
| Verificación de presupuesto y cupo antes de confirmar un otorgamiento | Regla | 4, 5, 6, 8, 9 | CUBIERTA |
| Monto igual al disponible sí alcanza | Regla | 8 | CUBIERTA |
| Dos otorgamientos simultáneos no exceden presupuesto ni cupo | Regla | 6 | CUBIERTA |
| Una oferta pendiente ocupa cupo y resta presupuesto; el rechazo o la expiración los libera | Regla | 2, 10 | CUBIERTA |
| Cupo del programa = becarios totales; cupo de convocatoria = nuevos del ciclo | Regla | 11 | CUBIERTA (el cupo por ciclo, DELEGADO a HU-02.1) |
| El monto de una beca nueva es el monto por periodo del programa | Regla | — | DELEGADA (ver HU-11.1, HU-15.1) |
| Información interna; postulante o becario no accede (RF-37) | Regla | 7 | CUBIERTA |
| Presupuesto total = suma de aportes (S8) | Regla | — | DELEGADA (ver HU-11.2, HU-13.2) |
| Comprometido en renovaciones pendientes cuenta como no disponible | Regla | — | DELEGADA (ver HU-12.2) |
| Beca revocada devuelve presupuesto y cupo | Regla | — | DELEGADA (ver HU-22.2) |
| Presupuesto insuficiente para el ranking se resuelve en HU-10.1 | Regla | — | DELEGADA (ver HU-10.1) |
| 2FA de inicio de sesión | Técnica | — | DELEGADA (autenticación transversal, fuera de esta área) |
| Una beca *Inactiva* no libera su cupo ni su presupuesto; los mantiene ocupados | Regla | 12 | CUBIERTA |
| FA-1 Programa sin aportes | FA | 3 | CUBIERTA |
| FA-2 Sin cupo | FA | 6, 9 | CUBIERTA |
| FA-3 Presupuesto insuficiente | FA | 5 | CUBIERTA |
| FA-4 Otorgamientos simultáneos | FA | 6 | CUBIERTA |
| FA-5 Rol sin permiso | FA | 7 | CUBIERTA |
| FA-6 Monto igual al disponible | FA | 8 | CUBIERTA |
| FA-7 Último cupo | FA | 9 | CUBIERTA |
| FA-8 Oferta pendiente | FA | 10 | CUBIERTA |
| FA-9 Beca *Inactiva* cuenta como ocupada | FA | 12 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-12.1).*

### HU-12.2 Contar las renovaciones pendientes como no disponibles

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-12 | Debe tener | HU-12.1, HU-26.1 | HU-12.3 |

#### Narrativa

- Como administrador financiero
- Quiero que el presupuesto comprometido en renovaciones pendientes se refleje como no disponible en la consulta de presupuesto del programa
- Para no otorgar nuevas becas con dinero que ya está comprometido

#### Reglas de negocio

- El presupuesto comprometido en renovaciones pendientes del ciclo actual cuenta como no disponible, no solo lo ya desembolsado (RF-12).
- Una renovación está pendiente desde que el becario la solicita (HU-26.1) hasta que se decide (HU-26.2).
- El monto de una renovación se asigna a cada estudiante ("después se le puede asignar más dinero"); no se deriva de un tipo de cobertura ni del monto que el programa fija para las becas nuevas (ver HU-11.1 y HU-26.x).
- Lo comprometido en ofertas pendientes cuenta como no disponible por la misma razón y comparte el mismo punto de bloqueo (ver HU-12.1).
- El evento que dispara la regla es la solicitud del becario (HU-26.1); el administrador financiero observa el resultado en la consulta de HU-12.1.
- El monto agregado comprometido por las renovaciones pendientes de un programa es información interna: un becario solo ve el estado de su propia solicitud (RF-37; RNF de confidencialidad).
- **Sin definir en el PRD:** en qué momento queda fijado el monto asignado a la renovación de cada estudiante. Los escenarios parten de ese monto ya asignado.

#### Flujo de la HU

1. Un becario solicita la renovación de su beca (HU-26.1).
2. El sistema cuenta el monto comprometido de esa renovación como no disponible en su programa.
3. La consulta de presupuesto (HU-12.1) muestra ese monto en "Comprometido en renovaciones pendientes".
4. La verificación de otorgamiento (HU-12.1) usa el disponible ya descontado.

#### Flujos alternos y de error

- FA-1 (paso 2) Renovación rechazada: al rechazarse (HU-26.2) deja de estar pendiente y su monto vuelve al disponible.
- FA-2 (paso 3) Consulta de un becario: el becario ve el estado de su propia renovación, no el monto agregado comprometido por el programa.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-12.2).*

#### Criterios de aceptación (5 escenarios)

##### Escenario 1: Renovación pendiente reduce el disponible

- **DADO QUE** "Becas Ingeniería" tiene 20.000 disponibles
- **CUANDO** un becario solicita una renovación con 5.000 comprometidos
- **ENTONCES** la consulta muestra 5.000 en "Comprometido en renovaciones pendientes" y 15.000 disponibles

##### Escenario 2: El otorgamiento respeta lo comprometido

- **DADO QUE** a "Becas Ingeniería" le quedan 6.000 de presupuesto sin otorgar, de los que 5.000 están comprometidos en renovaciones pendientes, y no hay ofertas pendientes
- **CUANDO** el coordinador confirma una beca nueva de 1.200
- **ENTONCES** el sistema no confirma y muestra "Presupuesto insuficiente: disponible 1.000, la beca requiere 1.200."

##### Escenario 3: Renovación rechazada

- **DADO QUE** "Becas Ingeniería" tiene 15.000 disponibles y una renovación pendiente de 5.000
- **CUANDO** el coordinador rechaza esa renovación (HU-26.2)
- **ENTONCES** la consulta muestra 20.000 disponibles

##### Escenario 4: Sin renovaciones pendientes

- **DADO QUE** "Becas Arte" no tiene renovaciones pendientes
- **CUANDO** el administrador financiero consulta el programa
- **ENTONCES** ve 0 en "Comprometido en renovaciones pendientes"

##### Escenario 5: Un becario no ve el agregado de renovaciones del programa

- **DADO QUE** Pedro tiene una renovación pendiente en "Becas Ingeniería" y otros becarios también tienen renovaciones pendientes en el mismo programa
- **CUANDO** Pedro consulta el estado de su renovación
- **ENTONCES** el sistema le muestra solo el estado de su propia solicitud, no el monto total comprometido por todas las renovaciones pendientes del programa

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| El comprometido en renovaciones pendientes cuenta como no disponible | Regla | 1, 2 | CUBIERTA |
| Sin renovaciones pendientes, el comprometido es 0 | Regla | 4 | CUBIERTA |
| Una renovación está pendiente desde que se solicita hasta que se decide | Regla | 1, 3 | CUBIERTA |
| La verificación de otorgamiento usa el disponible ya descontado | Regla | 2 | CUBIERTA |
| El monto agregado es información interna; el becario ve solo su solicitud | Regla | 5 | CUBIERTA |
| El monto de una renovación se asigna a cada estudiante | Regla | 1, 2, 3 (parten del monto ya asignado) | CUBIERTA en su efecto presupuestario; la asignación en sí, DELEGADA (ver HU-26.x) |
| Lo comprometido en ofertas pendientes cuenta como no disponible | Regla | — | DELEGADA (ver HU-12.1) |
| Evento disparador: la solicitud del becario | Regla | — | DELEGADA (ver HU-26.1) |
| Decisión de la renovación (aprobación o rechazo) | Regla | — | DELEGADA (ver HU-26.2) |
| 2FA de inicio de sesión | Técnica | — | DELEGADA (autenticación transversal, fuera de esta área) |
| En qué momento queda fijado el monto asignado a la renovación | Regla | — | NO PROBABLE (sin definir; pregunta abierta, bloqueante) |
| Dos solicitudes simultáneas que exceden el disponible | Regla | — | NO PROBABLE (sin definir en el PRD; pregunta abierta) |
| FA-1 Renovación rechazada libera el monto | FA | 3 | CUBIERTA |
| FA-2 Consulta de un becario (solo su propio estado) | FA | 5 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-12.2).*

## RF-13 — Alta y gestión de donantes o patrocinadores · *Debe tener*

### HU-13.1 Registrar un donante

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-13 | Debe tener | — | HU-13.1 |

#### Narrativa

- Como coordinador de programa
- Quiero registrar a un donante con sus datos de contacto
- Para vincularlo luego a los programas que financia

#### Reglas de negocio

- Un donante puede ser una organización o una persona (RF-13), por ejemplo una empresa o un egresado (Actores).
- Se registran sus datos de contacto (RF-13).
- El donante no usa la plataforma: el equipo de la Fundación le comparte los reportes (Fuera de alcance: portal de donantes).
- Los aportes y los programas que financia se registran en HU-13.2.
- Registrar donantes es una función del personal interno con esa responsabilidad; un evaluador o un becario no tiene acceso a ella (RF-37).
- Sin exigencias de Firebase, OTP ni concurrencia relevantes.

#### Flujo de la HU

1. El coordinador elige "Registrar donante".
2. Indica si es organización o persona.
3. Ingresa el nombre y los datos de contacto.
4. El sistema valida y guarda.
5. El donante aparece en la lista de donantes, sin aportes.

#### Flujos alternos y de error

- FA-1 (paso 4) Faltan datos de contacto: el sistema no guarda y muestra "Indica los datos de contacto del donante."
- FA-2 (paso 1) Rol sin permiso: un evaluador o un becario que intenta registrar un donante no accede a la función.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-13.1).*

#### Criterios de aceptación (5 escenarios)

##### Escenario 1: Registro de una organización

- **DADO QUE** "Empresa Alfa" no está registrada
- **CUANDO** el coordinador la registra como organización con correo y teléfono de contacto
- **ENTONCES** "Empresa Alfa" aparece en la lista de donantes como organización, sin aportes

##### Escenario 2: Registro de una persona

- **DADO QUE** el egresado Juan Pérez quiere financiar becas
- **CUANDO** el coordinador lo registra como persona con su correo
- **ENTONCES** "Juan Pérez" aparece en la lista de donantes como persona

##### Escenario 3: Faltan datos de contacto

- **DADO QUE** el coordinador llenó solo el nombre
- **CUANDO** guarda
- **ENTONCES** el sistema no registra al donante y muestra "Indica los datos de contacto del donante."

##### Escenario 4: El donante no recibe acceso

- **DADO QUE** el coordinador registró a "Empresa Alfa"
- **CUANDO** termina el registro
- **ENTONCES** no se crea ninguna cuenta de acceso a la plataforma para "Empresa Alfa"

##### Escenario 5: Un becario no registra donantes

- **DADO QUE** Pedro tiene el rol de becario
- **CUANDO** intenta abrir "Registrar donante"
- **ENTONCES** el sistema le niega el acceso a esa función

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| El donante es una organización o una persona | Regla | 1, 2 | CUBIERTA |
| Se registran sus datos de contacto | Regla | 1, 2, 3 | CUBIERTA |
| El donante no usa la plataforma (no se crea cuenta de acceso) | Regla | 4 | CUBIERTA |
| El donante nuevo aparece sin aportes | Regla | 1 | CUBIERTA |
| Solo personal interno registra donantes (RF-37) | Regla | 5 | CUBIERTA |
| Aportes y programas financiados | Regla | — | DELEGADA (ver HU-13.2) |
| 2FA de inicio de sesión | Técnica | — | DELEGADA (autenticación transversal, fuera de esta área) |
| FA-1 Faltan datos de contacto | FA | 3 | CUBIERTA |
| FA-2 Rol sin permiso | FA | 5 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-13.1).*

### HU-13.2 Registrar los aportes de un donante y consultar su historial

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-13 | Debe tener | HU-11.1, HU-13.1 | HU-13.2, HU-13.3 |

#### Narrativa

- Como coordinador de programa
- Quiero registrar el aporte de un donante a un programa en un periodo y consultar su historial de aportes por periodo
- Para saber qué financia cada donante y contar con el historial de sus aportes para fines legales y de reportería

#### Reglas de negocio

- Un donante puede financiar varios programas; un programa puede tener varios donantes (RF-13).
- El aporte de cada donante a un programa se registra por separado (RF-11).
- Cada aporte se registra por periodo, para conservar el historial de aportes por donante y por periodo con fines legales o de reportería (RF-13).
- Los periodos son los del calendario académico (RF-38; ver HU-38.1).
- Cada aporte registrado suma al presupuesto total del programa (S8; ver HU-11.2).
- El historial muestra a qué programas financia el donante (RF-13).
- Ningún aporte registrado se pierde del historial (RF-13).
- El monto de un aporte es una cantidad mayor que cero.
- El historial de aportes también lo consulta Dirección, que supervisa el conjunto de programas (RF-13; Actores).
- Los aportes de un donante son información financiera interna de la Fundación: un postulante o un becario no accede al historial (RF-37).

#### Flujo de la HU

1. El coordinador abre un donante y elige "Registrar aporte".
2. Elige el programa y el periodo.
3. Ingresa el monto.
4. El sistema valida y guarda.
5. El aporte aparece en el historial del donante y en la lista de donantes del programa (HU-11.2).
6. El coordinador o un miembro de Dirección abre el donante y elige "Historial de aportes".
7. El sistema muestra cada aporte con su periodo, programa y monto.
8. El usuario puede filtrar por periodo; el sistema muestra solo los aportes del periodo elegido.

#### Flujos alternos y de error

- FA-1 (paso 4) Falta un dato: el sistema no guarda y señala el campo faltante.
- FA-2 (paso 2) No hay programas registrados: el sistema muestra "No hay programas registrados."
- FA-3 (paso 7) Donante sin aportes: el sistema muestra "Este donante no tiene aportes registrados."
- FA-4 (paso 8) Periodo sin aportes: el sistema muestra "No hay aportes de este donante en el periodo 2025-1."
- FA-5 (paso 4) Monto en cero o negativo: el sistema no lo registra y muestra "El monto del aporte debe ser mayor que cero."
- FA-6 (paso 6) Rol sin permiso: un postulante o un becario que intenta consultar el historial de aportes de un donante no accede a esa información.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-13.2).*

#### Criterios de aceptación (9 escenarios)

##### Escenario 1: Un donante financia dos programas

- **DADO QUE** "Empresa Alfa" tiene un aporte de 30.000 a "Becas Ingeniería" en 2026-2
- **CUANDO** el coordinador registra un aporte de 10.000 de "Empresa Alfa" a "Becas Arte" en 2026-2
- **ENTONCES** el historial de "Empresa Alfa" muestra los dos aportes, cada uno con su programa

##### Escenario 2: Falta el periodo

- **DADO QUE** el coordinador eligió programa y monto
- **CUANDO** guarda sin elegir el periodo
- **ENTONCES** el sistema no registra el aporte y muestra "Indica el periodo del aporte."

##### Escenario 3: Sin programas

- **DADO QUE** no hay programas registrados
- **CUANDO** el coordinador intenta registrar un aporte
- **ENTONCES** el sistema muestra "No hay programas registrados."

##### Escenario 4: Historial en varios periodos

- **DADO QUE** "Empresa Alfa" aportó 25.000 en 2026-1 y 30.000 en 2026-2 a "Becas Ingeniería"
- **CUANDO** un miembro de Dirección abre su historial
- **ENTONCES** ve ambos aportes, cada uno con su periodo, programa y monto

##### Escenario 5: Filtro por un periodo anterior

- **DADO QUE** "Empresa Alfa" aportó 25.000 en 2026-1 y 30.000 en 2026-2
- **CUANDO** el coordinador abre el historial de "Empresa Alfa" y filtra por 2026-1
- **ENTONCES** ve solo el aporte de 25.000 de 2026-1

##### Escenario 6: Donante sin aportes

- **DADO QUE** "Juan Pérez" no tiene aportes registrados
- **CUANDO** el coordinador abre su historial
- **ENTONCES** el sistema muestra "Este donante no tiene aportes registrados."

##### Escenario 7: Aporte en cero

- **DADO QUE** el coordinador registra un aporte de "Empresa Alfa" a "Becas Ingeniería"
- **CUANDO** ingresa 0 como monto y guarda
- **ENTONCES** el sistema no registra el aporte y muestra "El monto del aporte debe ser mayor que cero."

##### Escenario 8: Un postulante no accede al historial de un donante

- **DADO QUE** "Empresa Alfa" tiene aportes registrados
- **CUANDO** un postulante intenta consultar el historial de aportes de "Empresa Alfa"
- **ENTONCES** el sistema le niega el acceso a esa información

##### Escenario 9: Filtro por un periodo sin aportes

- **DADO QUE** el periodo 2025-1 está registrado y "Empresa Alfa" aportó 25.000 en 2026-1 y 30.000 en 2026-2 a "Becas Ingeniería"
- **CUANDO** el coordinador abre el historial de "Empresa Alfa" y filtra por 2025-1
- **ENTONCES** el sistema muestra "No hay aportes de este donante en el periodo 2025-1."

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Un donante financia varios programas | Regla | 1 | CUBIERTA |
| Un programa tiene varios donantes | Regla | — | DELEGADA (ver HU-11.2, Escenario 1) |
| El aporte de cada donante a un programa se registra por separado | Regla | 1 | CUBIERTA |
| Cada aporte se registra por periodo | Regla | 2, 4, 5 | CUBIERTA |
| El historial muestra a qué programas financia el donante | Regla | 1, 4 | CUBIERTA |
| Ningún aporte registrado se pierde del historial | Regla | 4 | CUBIERTA |
| Filtro por periodo | Regla | 5, 9 | CUBIERTA |
| Monto del aporte > 0 | Regla | 7 | CUBIERTA |
| El historial también lo consulta Dirección | Regla | 4 | CUBIERTA |
| Información financiera interna; postulante o becario no accede (RF-37) | Regla | 8 | CUBIERTA |
| Periodos del calendario académico (RF-38) | Regla | — | DELEGADA (ver HU-38.1) |
| Cada aporte suma al presupuesto total del programa (S8) | Regla | — | DELEGADA (ver HU-11.2, HU-12.1) |
| 2FA de inicio de sesión | Técnica | — | DELEGADA (autenticación transversal, fuera de esta área) |
| Registrar un aporte contra un periodo no vigente | Regla | — | NO PROBABLE (sin definir en el PRD; pregunta abierta) |
| FA-1 Falta un dato | FA | 2 | CUBIERTA |
| FA-2 No hay programas registrados | FA | 3 | CUBIERTA |
| FA-3 Donante sin aportes | FA | 6 | CUBIERTA |
| FA-4 Periodo sin aportes | FA | 9 | CUBIERTA |
| FA-5 Monto en cero o negativo | FA | 7 | CUBIERTA |
| FA-6 Rol sin permiso | FA | 8 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-13.2).*

## RF-14 — Convenios con universidades aliadas · *Debe tener*

### HU-14.1 Registrar una universidad aliada con su convenio y sus carreras elegibles

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-14 | Debe tener | — | HU-14.1, HU-14.2 |

#### Narrativa

- Como coordinador de programa
- Quiero registrar una universidad aliada con los términos de su convenio, su contacto designado y sus carreras elegibles
- Para que sus estudiantes puedan postular, la Fundación sepa con quién coordinar y los programas puedan restringirse a esas carreras

#### Reglas de negocio

- De cada universidad aliada se registran los términos del convenio, el contacto designado y sus carreras elegibles (RF-14).
- El contacto designado es el enlace que certifica la matrícula y el rendimiento académico de los becarios (Actores: Enlace de universidad aliada).
- Registrar universidades aliadas y los términos de su convenio es una función de la Fundación (RF-14); el enlace de universidad, cuya función es certificar matrícula y rendimiento, no modifica los términos del convenio de su propia universidad (RF-37; Actores).
- El convenio registra el origen de la información académica de esa universidad: por **API desde la universidad** o por **registro manual** de una persona que revisa las notas enviadas por el estudiante. El cliente amplía el alcance del PRD, que dejaba fuera la integración automática, y la considera imprescindible (RF-24; ver HU-24.1, HU-24.2 y la historia de integración nueva de Área 5).
- Cada universidad aliada tiene sus propias fechas: su fecha de inicio de inscripciones por periodo se registra en HU-14.3 y, al iniciar sus inscripciones, se cierran las disponibilidades para esa universidad.
- El postulante debe estar matriculado o admitido en una universidad aliada (RF-01; ver HU-01.1).
- Un programa puede restringir la elegibilidad a carreras específicas, elegidas entre las carreras elegibles de cada universidad (RF-14; ver HU-11.2).
- El costo de matrícula se registra en HU-14.2.
- Sin exigencias de Firebase (el convenio se registra como términos en texto, no como documento adjunto), OTP ni concurrencia relevantes.

#### Flujo de la HU

1. El coordinador elige "Registrar universidad aliada".
2. Ingresa el nombre de la universidad y los términos del convenio.
3. Indica si el convenio incluye intercambio formal de datos académicos y, si lo incluye, por qué vía llega la información: API de la universidad o registro manual.
4. Ingresa los datos del contacto designado.
5. El sistema valida y guarda; la universidad queda disponible para los postulantes (HU-01.1) y para restringir programas (HU-11.2).
6. El coordinador abre la universidad aliada, elige "Agregar carrera elegible" e ingresa el nombre de la carrera.
7. El sistema valida y guarda.
8. La carrera aparece en la lista de carreras elegibles de esa universidad.

#### Flujos alternos y de error

- FA-1 (paso 5) Falta el contacto designado: el sistema no guarda y muestra "Indica el contacto designado de la universidad."
- FA-2 (paso 5) Faltan los términos del convenio: el sistema no guarda y muestra "Indica los términos del convenio."
- FA-3 (paso 7) Nombre vacío: el sistema no guarda y muestra "Indica el nombre de la carrera."
- FA-4 (paso 6) Universidad sin carreras: la lista muestra "Esta universidad no tiene carreras elegibles registradas."
- FA-5 (paso 2) Enlace de universidad: el enlace designado que intenta registrar o modificar los términos del convenio de su propia universidad no accede a esa función.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-14.1).*

#### Criterios de aceptación (10 escenarios)

##### Escenario 1: Registro de una universidad

- **DADO QUE** UCAB no está registrada
- **CUANDO** el coordinador la registra con los términos del convenio y a Ana Ruiz como contacto designado
- **ENTONCES** UCAB aparece en la lista de universidades aliadas con esos datos

##### Escenario 2: Falta el contacto designado

- **DADO QUE** el coordinador llenó el nombre y los términos del convenio
- **CUANDO** guarda sin contacto designado
- **ENTONCES** el sistema no registra la universidad y muestra "Indica el contacto designado de la universidad."

##### Escenario 3: Disponible para postulantes

- **DADO QUE** el coordinador registró UNIMET como universidad aliada
- **CUANDO** un postulante registra sus datos académicos (HU-01.1)
- **ENTONCES** UNIMET aparece entre las universidades que puede elegir

##### Escenario 4: Cada universidad tiene su propia lista de carreras

- **DADO QUE** UCAB y UNIMET no tienen carreras elegibles
- **CUANDO** el coordinador agrega "Ingeniería Informática" a UCAB
- **ENTONCES** "Ingeniería Informática" aparece en las carreras elegibles de UCAB y no aparece en las de UNIMET hasta que se registre también en UNIMET

##### Escenario 5: Nombre vacío

- **DADO QUE** el coordinador eligió "Agregar carrera elegible"
- **CUANDO** guarda sin nombre
- **ENTONCES** el sistema muestra "Indica el nombre de la carrera."

##### Escenario 6: Universidad sin carreras

- **DADO QUE** UNIMET no tiene carreras elegibles registradas
- **CUANDO** el coordinador abre la universidad
- **ENTONCES** ve "Esta universidad no tiene carreras elegibles registradas."

##### Escenario 7: El enlace de universidad no modifica su propio convenio

- **DADO QUE** Ana Ruiz es el contacto designado (enlace) de UCAB
- **CUANDO** intenta modificar los términos del convenio de UCAB desde su cuenta
- **ENTONCES** el sistema le niega el acceso a esa función

##### Escenario 8: Faltan los términos del convenio

- **DADO QUE** el coordinador llenó el nombre de UCAB y el contacto designado
- **CUANDO** guarda sin indicar los términos del convenio
- **ENTONCES** el sistema no registra la universidad y muestra "Indica los términos del convenio."

##### Escenario 9: Convenio con intercambio de datos académicos por API

- **DADO QUE** el coordinador registra a UCAB con sus términos de convenio y a Ana Ruiz como contacto designado
- **CUANDO** indica que el convenio incluye intercambio formal de datos académicos por API de la universidad y guarda
- **ENTONCES** UCAB queda registrada y su convenio muestra que la información académica llega por API desde la universidad

##### Escenario 10: Convenio con registro manual de la información académica

- **DADO QUE** el coordinador registra a UNIMET con sus términos de convenio y su contacto designado
- **CUANDO** indica que la información académica de UNIMET llega por registro manual de las notas que envía el estudiante y guarda
- **ENTONCES** UNIMET queda registrada con ese origen, que es el que usa el seguimiento académico para decidir de dónde viene el reporte (HU-24.2)

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Se registran los términos del convenio | Regla | 1, 8 | CUBIERTA |
| Se registra el contacto designado | Regla | 1, 2 | CUBIERTA |
| Se registran las carreras elegibles, propias de cada universidad | Regla | 4, 5, 6 | CUBIERTA |
| Se registra el origen de la información académica del convenio: API o registro manual | Regla | 9, 10 | CUBIERTA |
| La universidad registrada queda disponible para los postulantes | Regla | 3 | CUBIERTA |
| Solo la Fundación registra; el enlace no modifica el convenio de su universidad | Regla | 7 | CUBIERTA |
| El contacto designado certifica matrícula y rendimiento | Regla | — | DELEGADA (ver HU-24.1, HU-24.2) |
| El origen registrado decide de dónde viene el reporte académico | Regla | — | DELEGADA (ver HU-24.2 y la historia de integración nueva de Área 5) |
| Fecha de inicio de inscripciones de la universidad por periodo | Regla | — | DELEGADA (ver HU-14.3) |
| Validación de que el postulante esté matriculado o admitido | Regla | — | DELEGADA (ver HU-01.1) |
| Restricción de un programa a carreras elegibles de la universidad | Regla | — | DELEGADA (ver HU-11.2) |
| Costo de referencia de matrícula | Regla | — | DELEGADA (ver HU-14.2) |
| 2FA de inicio de sesión del coordinador | Técnica | — | DELEGADA (autenticación transversal, fuera de esta área) |
| 2FA del enlace de universidad | Técnica | — | DELEGADA (autenticación transversal, fuera de esta área) |
| El enlace actualiza sus propios datos de contacto | Regla | — | NO PROBABLE (sin definir en el PRD; pregunta abierta) |
| Quién crea la cuenta del enlace de universidad | Regla | — | NO PROBABLE (la respuesta 3 lo deja pendiente; pregunta abierta) |
| FA-1 Falta el contacto designado | FA | 2 | CUBIERTA |
| FA-2 Faltan los términos del convenio | FA | 8 | CUBIERTA |
| FA-3 Nombre de carrera vacío | FA | 5 | CUBIERTA |
| FA-4 Universidad sin carreras | FA | 6 | CUBIERTA |
| FA-5 El enlace intenta modificar el convenio | FA | 7 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-14.1).*

### HU-14.2 Registrar el costo de matrícula por periodo sin perder los anteriores

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-14 | Debe tener | HU-14.1, HU-38.1 | HU-14.3 |

#### Narrativa

- Como coordinador de programa
- Quiero registrar el costo de referencia de la matrícula de cada universidad por periodo
- Para tener el costo al día sin perder los valores que ya se usaron

#### Reglas de negocio

- Cada universidad aliada tiene un costo de referencia de matrícula (RF-14).
- El costo cambia cada periodo, por eso se registra por periodo académico (RF-14, RF-38).
- Actualizar el costo no borra los valores anteriores; un cálculo ya hecho conserva el valor que usó (RF-14).
- El costo de matrícula es una cantidad mayor que cero.
- Un periodo registrado puede no tener todavía costo cargado para una universidad; esa ausencia se muestra como tal, no como un valor ni como un error de periodo inexistente (RF-14).
- El costo de matrícula ya no interviene en el cálculo del monto de una beca: todas las becas son un monto fijo por periodo y las coberturas total y parcial se eliminan (ver HU-11.1).
- **Decisión del cliente:** el uso del costo de referencia es servir de tope a la suma de las becas de una persona en un periodo, dentro de la regla de no doble beneficio (ver HU-01.2): esa suma no puede superar el costo de matrícula de su universidad en ese periodo. Ese tope se aplica **a la vez** con la otra regla de no doble beneficio (no tener dos becas activas de programas "completos" simultáneas), no en su lugar.

#### Flujo de la HU

1. El coordinador abre una universidad aliada y elige "Costo de matrícula".
2. Elige un periodo registrado e ingresa el costo.
3. El sistema valida que el costo sea mayor que cero y lo guarda para ese periodo como una nueva entrada.
4. El sistema conserva los costos de los periodos anteriores y las entradas anteriores del mismo periodo.
5. La universidad muestra el costo vigente de cada periodo.

#### Flujos alternos y de error

- FA-1 (paso 3) Corrección de un costo ya registrado: el sistema guarda el nuevo valor como vigente y conserva el valor anterior para consulta.
- FA-2 (paso 2) No hay periodos registrados: el sistema muestra "No hay periodos académicos registrados."
- FA-3 (paso 3) Costo en cero o negativo: el sistema no guarda y muestra "El costo de matrícula debe ser mayor que cero."
- FA-4 (paso 5) Periodo registrado sin costo cargado: el sistema muestra "Sin costo registrado para este periodo".

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-14.2).*

#### Criterios de aceptación (5 escenarios)

##### Escenario 1: Costo de un nuevo periodo

- **DADO QUE** UCAB tiene un costo de 900 para 2026-1
- **CUANDO** el coordinador registra 1.000 para 2026-2
- **ENTONCES** UCAB muestra 900 para 2026-1 y 1.000 para 2026-2

##### Escenario 2: La corrección no borra el valor anterior

- **DADO QUE** UCAB tiene un costo de 1.000 para 2026-2
- **CUANDO** el coordinador lo corrige a 1.050
- **ENTONCES** UCAB muestra 1.050 como costo vigente de 2026-2 y el valor anterior de 1.000 sigue consultable

##### Escenario 3: Sin periodos registrados

- **DADO QUE** no hay periodos académicos registrados
- **CUANDO** el coordinador intenta registrar un costo
- **ENTONCES** el sistema muestra "No hay periodos académicos registrados."

##### Escenario 4: Costo de matrícula en cero

- **DADO QUE** el coordinador registra el costo de matrícula de UCAB para 2026-2
- **CUANDO** ingresa 0 como costo y guarda
- **ENTONCES** el sistema no guarda y muestra "El costo de matrícula debe ser mayor que cero."

##### Escenario 5: Periodo registrado sin costo cargado

- **DADO QUE** el periodo 2026-2 está registrado pero UCAB no tiene costo de matrícula cargado para ese periodo
- **CUANDO** el coordinador consulta el costo de matrícula de UCAB en 2026-2
- **ENTONCES** el sistema muestra "Sin costo registrado para este periodo" en vez de un valor o de un error de periodo inexistente

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Cada universidad aliada tiene un costo de referencia de matrícula | Regla | 1 | CUBIERTA |
| El costo se registra por periodo académico | Regla | 1, 3 | CUBIERTA |
| Actualizar el costo no borra los valores anteriores | Regla | 1, 2 | CUBIERTA |
| El costo vigente de un periodo es el de registro más reciente | Regla | 2 | CUBIERTA |
| Costo de matrícula > 0 | Regla | 4 | CUBIERTA |
| Un periodo sin costo cargado se muestra como tal, no como valor ni como error | Regla | 5 | CUBIERTA |
| El costo no interviene en el monto de la beca (monto fijo por periodo) | Regla | — | DELEGADA (ver HU-11.1) |
| Uso del costo como tope de la suma de becas de una persona en un periodo, aplicado a la vez con la regla de los programas "completos" | Regla | — | DELEGADA (ver HU-01.2, HU-16.1, HU-18.1) |
| Periodos del calendario académico (RF-38) | Regla | — | DELEGADA (ver HU-38.1) |
| 2FA de inicio de sesión | Técnica | — | DELEGADA (autenticación transversal, fuera de esta área) |
| FA-1 Corrección de un costo ya registrado | FA | 2 | CUBIERTA |
| FA-2 No hay periodos registrados | FA | 3 | CUBIERTA |
| FA-3 Costo en cero o negativo | FA | 4 | CUBIERTA |
| FA-4 Periodo registrado sin costo cargado | FA | 5 | CUBIERTA |

### HU-14.3 Registrar la fecha de inicio de inscripciones de cada universidad por periodo

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-14 | Debe tener | HU-14.1, HU-38.1 | — (historia nueva, sin equivalente en v2.0) |

#### Narrativa

- Como coordinador de programa
- Quiero registrar, para cada universidad aliada y cada periodo, la fecha en que esa universidad inicia sus inscripciones
- Para que las disponibilidades de esa universidad se cierren cuando empiezan sus inscripciones y su lista de espera termine en esa fecha
- Todo el contenido de esta historia se origina en C4.

#### Reglas de negocio

- Se abre un periodo de becas común (por ejemplo, verano) y, dentro de él, cada universidad tiene sus propias fechas.
- Se registra la fecha de inicio de inscripciones por universidad y periodo.
- Cuando una universidad inicia sus inscripciones, se cierran las disponibilidades para esa universidad, no para las demás (ver HU-02.1, HU-02.2 y HU-38.1).
- La lista de espera termina, para cada universidad, al iniciar su periodo de inscripción (ver HU-10.2).
- Las fechas de los periodos las define cada universidad; la plataforma las registra.
- Las fechas se interpretan en UTC-4, y el día de cierre de un plazo se incluye hasta las 23:59.
- Solo se registran fechas de universidades aliadas registradas (HU-14.1) y de periodos académicos registrados (HU-38.1).
- Registrar estas fechas es una función del personal interno con esa responsabilidad (RF-37); la respuesta 16 confirma que Dirección es quien abre y cierra los periodos académicos, pero sigue por confirmar si el registro de esta fecha por universidad y periodo corresponde al coordinador de programa (como asume esta historia) o a Dirección (ver Preguntas abiertas).

#### Flujo de la HU

1. El coordinador abre una universidad aliada y elige "Fechas de inscripción".
2. Elige un periodo registrado e ingresa la fecha de inicio de inscripciones de esa universidad.
3. El sistema valida y guarda la fecha para esa universidad y ese periodo.
4. La universidad muestra su fecha de inicio de inscripciones de cada periodo.
5. Al llegar esa fecha, el sistema cierra las disponibilidades de esa universidad para ese periodo y termina su lista de espera (HU-02.2, HU-10.2).

#### Flujos alternos y de error

- FA-1 (paso 2) No hay periodos registrados: el sistema muestra "No hay periodos académicos registrados."
- FA-2 (paso 3) Falta la fecha: el sistema no guarda y muestra "Indica la fecha de inicio de inscripciones."
- FA-3 (paso 4) Universidad y periodo sin fecha cargada: el sistema muestra "Sin fecha de inicio de inscripciones registrada para este periodo".
- FA-4 (paso 5) Otras universidades del mismo periodo: el cierre de disponibilidades afecta solo a la universidad que inició sus inscripciones; las demás siguen abiertas hasta su propia fecha.

*Reglas técnicas de esta historia: ver [Anexo A](anexo-a-reglas-tecnicas.md) (HU-14.3).*

#### Criterios de aceptación (6 escenarios)

##### Escenario 1: Fecha de inicio de inscripciones de una universidad

- **DADO QUE** UCAB está registrada como universidad aliada y el periodo 2026-2 está registrado
- **CUANDO** el coordinador registra el 15/09/2026 como fecha de inicio de inscripciones de UCAB para 2026-2 y guarda
- **ENTONCES** UCAB muestra el 15/09/2026 como fecha de inicio de inscripciones de 2026-2

##### Escenario 2: El cierre de disponibilidades afecta solo a esa universidad

- **DADO QUE** en el periodo 2026-2 UCAB inicia sus inscripciones el 15/09/2026 y UNIMET el 01/10/2026
- **CUANDO** llega el 15/09/2026 (UTC-4)
- **ENTONCES** se cierran las disponibilidades de UCAB para 2026-2 y las de UNIMET siguen abiertas

##### Escenario 3: La lista de espera de la universidad termina al iniciar sus inscripciones

- **DADO QUE** hay postulantes de UCAB en lista de espera para 2026-2 y UCAB inicia sus inscripciones el 15/09/2026
- **CUANDO** llega el 15/09/2026 (UTC-4)
- **ENTONCES** la lista de espera de UCAB para 2026-2 termina (HU-10.2) y la de las demás universidades no cambia

##### Escenario 4: Sin periodos registrados

- **DADO QUE** no hay periodos académicos registrados
- **CUANDO** el coordinador intenta registrar una fecha de inicio de inscripciones
- **ENTONCES** el sistema muestra "No hay periodos académicos registrados."

##### Escenario 5: Falta la fecha

- **DADO QUE** el coordinador eligió UCAB y el periodo 2026-2
- **CUANDO** guarda sin indicar la fecha de inicio de inscripciones
- **ENTONCES** el sistema no guarda y muestra "Indica la fecha de inicio de inscripciones."

##### Escenario 6: Periodo registrado sin fecha cargada

- **DADO QUE** el periodo 2026-2 está registrado y UCAB no tiene fecha de inicio de inscripciones cargada para ese periodo
- **CUANDO** el coordinador consulta las fechas de inscripción de UCAB en 2026-2
- **ENTONCES** el sistema muestra "Sin fecha de inicio de inscripciones registrada para este periodo"

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Se registra la fecha de inicio de inscripciones por universidad y periodo | Regla | 1, 6 | CUBIERTA |
| Las fechas las define cada universidad; la plataforma las registra | Regla | 1, 2 | CUBIERTA |
| Al iniciar sus inscripciones se cierran las disponibilidades de esa universidad | Regla | 2 | CUBIERTA (el cierre en sí, DELEGADO a HU-02.2) |
| La lista de espera termina, por universidad, al iniciar su periodo de inscripción | Regla | 3 | CUBIERTA (el fin de la lista en sí, DELEGADO a HU-10.2) |
| Solo universidades aliadas y periodos registrados | Regla | 1, 4 | CUBIERTA |
| Las fechas se interpretan en UTC-4 | Regla | 2, 3 | CUBIERTA (zona horaria; el momento exacto del cierre sigue abierto) |
| Periodo de becas común sobre el que corren las fechas por universidad | Regla | — | DELEGADA (ver HU-38.1, HU-38.2) |
| 2FA de inicio de sesión | Técnica | — | DELEGADA (autenticación transversal, fuera de esta área) |
| ¿La fecha puede corregirse y se conservan los valores anteriores? | Regla | — | NO PROBABLE (sin definir; pregunta abierta) |
| FA-1 No hay periodos registrados | FA | 4 | CUBIERTA |
| FA-2 Falta la fecha | FA | 5 | CUBIERTA |
| FA-3 Universidad y periodo sin fecha cargada | FA | 6 | CUBIERTA |
| FA-4 Otras universidades del mismo periodo | FA | 2 | CUBIERTA |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-14.3).*

## RF-15 — Emisión de carta de otorgamiento · *Debe tener*

### HU-15.1 Generar la oferta formal de beca y comunicarla con su fecha límite

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-15 | Debe tener | HU-09.2, HU-10.1, HU-12.1 | HU-15.1, HU-15.2 |

#### Narrativa

- Como coordinador de programa
- Quiero generar la oferta formal de beca para un postulante seleccionado y que le llegue con su fecha límite de aceptación
- Para comunicarle por escrito el monto, la duración y las condiciones de su beca, y que decida a tiempo si la acepta

#### Reglas de negocio

- Al seleccionar a un postulante se genera y se le comunica una oferta formal con el monto, la duración y las condiciones de la beca (RF-15).
- La oferta incluye las condiciones de permanencia: promedio mínimo, horas de servicio si aplica y reportes exigidos (RF-15; definidas en HU-11.2).
- Antes de confirmarse se verifican presupuesto y cupo (RF-12; ver HU-12.1). Esta HU no reimplementa esa lógica: invoca el servicio de disponibilidad de Área 3A.
- El monto de la beca es un **monto fijo por periodo**; las coberturas total y parcial dejan de usarse para calcularlo.
- En una beca nueva el monto lo fija el **programa**, así que la oferta lo toma de la configuración del programa (HU-11.1) y no se define oferta por oferta. Los montos de los periodos siguientes se asignan a cada estudiante en la renovación (RF-26).
- La verificación de disponibilidad distingue el **cupo del programa** (becarios totales) del **cupo de la convocatoria** (nuevos del ciclo): una oferta nueva consume cupo de la convocatoria del ciclo y cuenta en el total del programa. El cálculo sigue siendo de HU-12.1.
- Una oferta pendiente de aceptación **resta presupuesto disponible** y lo mantiene comprometido hasta que ese proceso se rechace definitivamente (rechazo del postulante o expiración).
- El periodo de becas es común, pero las **disponibilidades se cierran por universidad** cuando esa universidad inicia sus inscripciones; la fecha de inicio de inscripciones por universidad y periodo se registra en HU-14.x/HU-02.x y el cierre lo aplica el servicio de disponibilidad (HU-12.1). Esta HU no lo reimplementa.
- La carta se guarda en la cuenta de Firebase de la Fundación, sin crear un almacenamiento paralelo (Restricción Técnica del Cliente). Es la única historia del área que persiste un documento.
- El compromiso de presupuesto y el guardado de la carta en Firebase deben comportarse como una sola unidad atómica de negocio: si falla el guardado externo, el compromiso de presupuesto se revierte (ver Escenario 4). El orden (comprometer y luego guardar con compensación, o guardar y luego comprometer) debe decidirse una sola vez para todo el proyecto.
- El postulante debe aceptar la oferta antes de una fecha límite; si no, la oferta expira (RF-15; ver HU-15.2). La fecha límite es configurable.
- Las fechas límite se interpretan en **UTC-4** e **incluyen el día de cierre hasta las 23:59**.
- La notificación indica qué ocurrió y qué acción se espera; el canal mínimo es el correo electrónico (RF-29; ver HU-29.1).
- Mientras la oferta está pendiente, la postulación sigue *En evaluación* (S3).
- La aprobación queda registrada con quién, cuándo y por qué, de forma permanente y sin edición retroactiva (RF-35; ver HU-35.1).
- Solo el coordinador responsable del programa puede generar ofertas en él; el personal interno accede únicamente a lo que corresponde a su función (RF-37).
- El coordinador tiene acceso a información de decisión, por lo que su inicio de sesión exige verificación en dos pasos (Restricción Técnica del Cliente).
- La generación de la oferta **no** exige código de un solo uso (OTP): no desembolsa dinero, no revoca una beca ni modifica datos de pago (Restricción Técnica del Cliente; ver RF-20 y RF-22).
- Una postulación no puede tener dos ofertas pendientes a la vez; mientras una oferta está sin responder no se genera otra para la misma postulación.
- Una oferta solo se genera sobre una postulación *En evaluación* que quedó dentro del cupo (HU-09.2) o por decisión de HU-10.1. Las reglas de estados y de no salto de etapas son las transversales del PRD (ver HU-04.1).
- **Sin definir en el PRD:** a qué nivel se configura la fecha límite de aceptación (por programa, por ciclo o por oferta individual) ni quién la calcula. El instante de vencimiento sí está definido: 23:59 (UTC-4) del día límite.

#### Flujo de la HU

1. El coordinador abre una postulación *En evaluación* que quedó dentro del cupo en el ranking (HU-09.2) o por decisión de HU-10.1, y elige "Generar oferta".
2. Revisa el monto fijo por periodo que define el programa, la duración y las condiciones de permanencia del programa, y la fecha límite de aceptación.
3. Confirma; el sistema verifica presupuesto y cupo (HU-12.1).
4. El sistema genera la carta de otorgamiento y la guarda en Firebase.
5. El sistema registra quién generó la oferta, cuándo y sobre qué decisión (HU-35.1).
6. La oferta queda pendiente, la postulación sigue *En evaluación* y su monto queda comprometido en el presupuesto del programa.
7. El sistema notifica al postulante por correo que tiene una oferta y la fecha límite para responder (HU-29.1).
8. El postulante abre la oferta en la plataforma.
9. Ve el monto, la duración, las condiciones de permanencia, la fecha límite y la carta.
10. Puede aceptar (HU-16.1) o rechazar (HU-16.2).

#### Flujos alternos y de error

- **FA-1 (paso 2) Programa sin condiciones de permanencia:** el sistema no genera la oferta y muestra "El programa no tiene condiciones de permanencia definidas."
- **FA-2 (paso 3) Presupuesto o cupo insuficiente:** el sistema no genera la oferta (HU-12.1).
- **FA-3 (paso 4) La carta no se pudo guardar:** el sistema no emite la oferta y muestra "No se pudo guardar la carta de otorgamiento. Intenta de nuevo."
- **FA-4 (paso 8) Oferta expirada:** el postulante ve "Esta oferta expiró el 15/10/2026." y no tiene opciones para responder.
- **FA-5 (paso 1) Rol sin permiso:** un becario, postulante o evaluador intenta generar una oferta; el sistema le niega el acceso a esa función (RF-37).
- **FA-6 (paso 1) Coordinador de otro programa:** un coordinador que no coordina el programa intenta generar una oferta en él; el sistema bloquea el acceso y responde "No tienes permiso para gestionar este programa."
- **FA-7 (paso 1) Postulación en un estado que no admite oferta:** si la postulación no está *En evaluación* (p. ej. *No elegible*, *Rechazada* o *Retirada*), el sistema no permite generar la oferta y muestra su estado actual. Las transiciones y el no salto de etapas se rigen por la regla transversal de estados (ver HU-04.1).
- **FA-8 (paso 1) Oferta duplicada:** la postulación ya tiene una oferta pendiente sin responder; el sistema no genera una segunda y muestra "Esta postulación ya tiene una oferta pendiente."
- **FA-9 (paso 3) Presupuesto comprometido por otra oferta pendiente:** el disponible ya descuenta las ofertas pendientes, así que la generación se bloquea aunque ese monto todavía no se haya desembolsado (HU-12.1).

#### Criterios de aceptación (13 escenarios)

##### Escenario 1: Oferta generada

- **DADO QUE** María fue seleccionada en "Becas Ingeniería", el programa tiene presupuesto, cupo y condiciones de permanencia, y define un monto fijo de 1.200 por periodo
- **CUANDO** el coordinador genera su oferta con una duración de 8 periodos
- **ENTONCES** la carta queda guardada en Firebase con el monto fijo del periodo, la duración y las condiciones de permanencia, y la postulación de María sigue *En evaluación*

##### Escenario 2: Programa sin condiciones de permanencia

- **DADO QUE** "Becas Arte" no tiene condiciones de permanencia definidas
- **CUANDO** el coordinador intenta generar una oferta
- **ENTONCES** el sistema muestra "El programa no tiene condiciones de permanencia definidas."

##### Escenario 3: Presupuesto insuficiente

- **DADO QUE** "Becas Ingeniería" tiene 800 disponibles y la oferta de María requiere 1.200
- **CUANDO** el coordinador confirma la generación de la oferta
- **ENTONCES** no se genera la carta, no se guarda ningún documento en Firebase, no queda ningún registro de aprobación y el sistema muestra el motivo de indisponibilidad que devuelve el servicio de HU-12.1

##### Escenario 4: Falla al guardar la carta

- **DADO QUE** Firebase no responde
- **CUANDO** el coordinador genera la oferta de María
- **ENTONCES** la oferta no se emite, el sistema muestra "No se pudo guardar la carta de otorgamiento. Intenta de nuevo." y el presupuesto y el cupo disponibles no cambian

##### Escenario 5: El postulante recibe la oferta

- **DADO QUE** el coordinador generó la oferta de María con fecha límite 15/10/2026
- **CUANDO** María abre el correo y entra a la oferta
- **ENTONCES** ve el monto, la duración, las condiciones de permanencia, la fecha límite 15/10/2026 y las opciones "Aceptar" y "Rechazar"

##### Escenario 6: Oferta expirada

- **DADO QUE** la fecha límite de la oferta de María fue el 15/10/2026
- **CUANDO** María la abre el 16/10/2026
- **ENTONCES** ve "Esta oferta expiró el 15/10/2026." y no puede aceptarla ni rechazarla

##### Escenario 7: Coordinador de otro programa

- **DADO QUE** Carlos coordina "Becas Salud" y no coordina "Becas Ingeniería"
- **CUANDO** intenta generar una oferta para una postulación de "Becas Ingeniería"
- **ENTONCES** el sistema bloquea el acceso y responde "No tienes permiso para gestionar este programa."

##### Escenario 8: Oferta duplicada mientras una ya está pendiente

- **DADO QUE** María ya tiene una oferta pendiente de "Becas Ingeniería" con fecha límite 15/10/2026
- **CUANDO** el coordinador intenta generar una segunda oferta para la misma postulación
- **ENTONCES** el sistema no genera la segunda oferta y muestra "Esta postulación ya tiene una oferta pendiente."

##### Escenario 9: La generación queda registrada

- **DADO QUE** el coordinador Carlos genera la oferta de María en "Becas Ingeniería"
- **CUANDO** la oferta queda emitida
- **ENTONCES** el historial de la postulación registra que Carlos la generó, la fecha y la justificación, y ese registro no puede editarse ni eliminarse después

##### Escenario 10: Rol sin permiso intenta generar una oferta

- **DADO QUE** Luis tiene el rol de becario y no es coordinador de "Becas Ingeniería"
- **CUANDO** intenta generar una oferta para una postulación de ese programa
- **ENTONCES** el sistema le niega el acceso a esa función

##### Escenario 11: Postulación en un estado que no admite oferta

- **DADO QUE** la postulación de Pedro en "Becas Ingeniería" está *Retirada*
- **CUANDO** el coordinador intenta generar una oferta sobre ella
- **ENTONCES** el sistema no genera la oferta y muestra el estado actual de la postulación, *Retirada*

##### Escenario 12: Generar la oferta no exige código de un solo uso

- **DADO QUE** María fue seleccionada en "Becas Ingeniería" y el programa tiene presupuesto, cupo y condiciones de permanencia
- **CUANDO** el coordinador confirma la generación de su oferta
- **ENTONCES** la oferta queda emitida sin que el sistema solicite ningún código de verificación de un solo uso (OTP)

##### Escenario 13: La oferta pendiente mantiene su monto comprometido

- **DADO QUE** "Becas Ingeniería" tiene presupuesto para una sola beca más y la oferta de María, de 1.200, está pendiente de respuesta
- **CUANDO** el coordinador intenta generar una oferta de 1.200 para Pedro
- **ENTONCES** el sistema no la genera, porque el monto de la oferta pendiente de María sigue comprometido hasta que ese proceso se rechace definitivamente

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Al seleccionar al postulante se genera y comunica la oferta con monto, duración y condiciones (RF-15) | Regla | 1, 5 | CUBIERTA |
| La oferta incluye las condiciones de permanencia | Regla | 1, 5 (ausencia: 2) | CUBIERTA |
| Monto fijo por periodo, fijado por el programa en una beca nueva | Regla | 1 | CUBIERTA |
| Verificación de presupuesto y cupo antes de confirmar | Regla | 3 (efecto local) | DELEGADA (HU-12.1); efecto local CUBIERTO |
| Cupo del programa (becarios totales) vs. cupo de la convocatoria (nuevos del ciclo) | Regla | — | DELEGADA (HU-12.1) |
| La oferta pendiente resta presupuesto hasta el rechazo definitivo (FA-9) | Regla / FA | 13 | CUBIERTA |
| Cierre de disponibilidades por universidad al iniciar sus inscripciones | Regla | — | DELEGADA (HU-02.x, HU-12.1, HU-14.x) |
| La carta se guarda en Firebase (única historia del área que persiste un documento) | Regla | 1, 4 | CUBIERTA |
| Atomicidad: compromiso de presupuesto + guardado en Firebase | Regla | 4 | CUBIERTA |
| El postulante debe aceptar antes de la fecha límite; si no, expira (mecanismo) | Regla | — | DELEGADA (HU-15.2) |
| Vencimiento a las 23:59 (UTC-4) del día límite | Regla | — | DELEGADA (HU-15.2, Escenario 1) |
| FA-4 Oferta expirada al abrirla | FA | 6 | CUBIERTA |
| Notificación al postulante (contenido y canal) | Regla | 5 (recepción) | DELEGADA (HU-29.1) |
| Mientras la oferta está pendiente la postulación sigue *En evaluación* | Regla | 1 | CUBIERTA |
| Aprobación registrada con quién, cuándo y por qué, sin edición retroactiva (RF-35) | Regla | 9 | CUBIERTA |
| Solo el coordinador responsable del programa genera ofertas (RF-37) | Regla | 7, 10 | CUBIERTA |
| FA-5 Rol sin permiso (becario, postulante, evaluador) | FA | 10 | CUBIERTA |
| FA-6 Coordinador de otro programa | FA | 7 | CUBIERTA |
| 2FA de login del coordinador | Regla | — | DELEGADA (HU-37.1/HU-37.2, fuera del área) |
| La generación de la oferta no exige OTP | Regla | 12 | CUBIERTA |
| No puede haber dos ofertas pendientes a la vez (FA-8) | Regla / FA | 8 | CUBIERTA |
| FA-7 Postulación en un estado que no admite oferta | FA | 11 | CUBIERTA |
| FA-2 Presupuesto o cupo insuficiente | FA | 3 | CUBIERTA |
| FA-3 La carta no se pudo guardar | FA | 4 | CUBIERTA |
| FA-1 Programa sin condiciones de permanencia | FA | 2 | CUBIERTA |
| Nivel de configuración de la fecha límite de aceptación | Regla | — | NO PROBABLE (sin definir en el PRD; pregunta abierta 1) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-15.1).*

### HU-15.2 Expirar una oferta sin respuesta y considerar al siguiente

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-15 | Debe tener | HU-10.2, HU-15.1 | HU-15.3 |

#### Narrativa

- Como coordinador de programa
- Quiero que el sistema expire automáticamente una oferta no respondida en su fecha límite y me muestre al siguiente postulante de la lista de espera
- Para no perder un cupo disponible por falta de seguimiento manual

#### Reglas de negocio

- Si el postulante no acepta antes de la fecha límite, la oferta expira (RF-15).
- La fecha límite vence a las **23:59 (UTC-4) del día de cierre**: ese día se incluye completo.
- Al expirar la oferta, la postulación pasa a *Retirada* (S3).
- El cupo vuelve a estar disponible considerando la lista de espera (RF-16).
- Al expirar, el monto que la oferta tenía comprometido deja de restar presupuesto: el proceso quedó rechazado definitivamente (HU-12.1).
- Se considera al siguiente de la lista de espera (RF-15). La lista de espera es una marca sobre las postulaciones elegibles no otorgadas (S2; ver HU-10.2).
- La lista de espera **termina, para cada universidad, al iniciar el periodo de inscripciones de esa universidad**: desde ese momento se cierran las disponibilidades para esa universidad y sus postulantes en espera ya no se consideran en ese periodo. Las fechas por universidad y periodo las aporta HU-14.x/HU-02.x.
- La oferta al siguiente se genera con HU-15.1, que verifica presupuesto y cupo (HU-12.1). El sistema sugiere al candidato; la decisión de generar la oferta sigue siendo humana.
- La expiración la dispara el paso del tiempo, no una acción de usuario: el cambio de estado debe ser condicional sobre el estado "pendiente", de modo que la expiración y una respuesta del postulante casi simultánea nunca se apliquen ambas (RNF de consistencia bajo concurrencia).
- La transición a *Retirada* por vencimiento también debe quedar registrada con quién, cuándo y por qué; el actor registrado es el sistema, no una persona (RF-35; ver HU-35.1).
- No aplican OTP ni verificación en dos pasos en el momento de la expiración: no la ejecuta una persona autenticada. El coordinador solo interviene después, al generar la siguiente oferta (HU-15.1).
- **Sin definir en el PRD:** el mecanismo de expiración (tarea programada que recorre las ofertas vencidas, o evaluación perezosa al primer acceso posterior a la fecha límite). Los escenarios describen el efecto, no el mecanismo.

#### Flujo de la HU

1. Pasan las 23:59 (UTC-4) de la fecha límite sin que el postulante haya respondido.
2. El sistema marca la oferta como expirada y pasa la postulación a *Retirada*.
3. El sistema registra la expiración con la fecha y el sistema como actor (HU-35.1).
4. El sistema libera el cupo y el presupuesto que la oferta tenía comprometido (HU-12.1).
5. El sistema muestra al coordinador el siguiente postulante de la lista de espera del ciclo cuya universidad no haya iniciado aún su periodo de inscripciones (HU-10.2).
6. El coordinador genera la oferta para ese postulante (HU-15.1).

#### Flujos alternos y de error

- **FA-1 (paso 5) Lista de espera vacía:** el sistema muestra "No hay postulantes en lista de espera para este ciclo." y el cupo queda disponible.
- **FA-2 (paso 1) Respuesta a tiempo:** si el postulante aceptó o rechazó antes de la fecha límite, la oferta no expira.
- **FA-3 (paso 1) Respuesta simultánea a la expiración:** si el postulante responde en el mismo instante en que corre la expiración, solo una de las dos operaciones se aplica sobre la oferta; nunca ambas.
- **FA-4 (paso 5) El siguiente de la lista ya tiene una beca activa de un programa marcado como "completo":** el PRD y las respuestas no definen si el sistema debe omitirlo o proponerlo igual y dejar que el bloqueo ocurra al aceptar (regla de no doble beneficio, HU-01.2; verificación en HU-16.1). Queda como pregunta abierta.
- **FA-5 (paso 5) La lista de espera de la universidad del candidato ya cerró:** su universidad inició sus inscripciones, por lo que el sistema no lo propone y lo indica; el cupo ya no se puede ofertar a postulantes de esa universidad en ese periodo.

#### Criterios de aceptación (7 escenarios)

##### Escenario 1: La oferta expira

- **DADO QUE** María tiene una oferta de "Becas Ingeniería" con fecha límite 15/10/2026 sin responder
- **CUANDO** pasan las 23:59 (UTC-4) del 15/10/2026
- **ENTONCES** la oferta queda expirada, la postulación de María pasa a *Retirada* y el cupo disponible aumenta en 1

##### Escenario 2: Se considera al siguiente

- **DADO QUE** la oferta de María expiró y Pedro es el siguiente en la lista de espera
- **CUANDO** el coordinador abre el programa
- **ENTONCES** ve a Pedro como el siguiente postulante a considerar, con la opción "Generar oferta"

##### Escenario 3: Lista de espera vacía

- **DADO QUE** la oferta de María expiró y la lista de espera está vacía
- **CUANDO** el coordinador abre el programa
- **ENTONCES** ve "No hay postulantes en lista de espera para este ciclo." y el cupo sigue disponible

##### Escenario 4: Aceptación a tiempo

- **DADO QUE** María aceptó su oferta el 14/10/2026 (HU-16.1)
- **CUANDO** pasan las 23:59 (UTC-4) del 15/10/2026
- **ENTONCES** la oferta no expira y su postulación no pasa a *Retirada*

##### Escenario 5: Carrera entre aceptación y expiración

- **DADO QUE** la oferta de María está pendiente y llega su fecha límite
- **CUANDO** María confirma "Aceptar" en el mismo instante en que se ejecuta la expiración
- **ENTONCES** se aplica una sola de las dos operaciones: o la oferta queda aceptada y la postulación *Otorgada*, o queda expirada y la postulación *Retirada*, nunca ambas ni un estado contradictorio

##### Escenario 6: La expiración queda registrada con el sistema como actor

- **DADO QUE** la oferta de María expiró el 15/10/2026 sin respuesta
- **CUANDO** el coordinador consulta el historial de la postulación
- **ENTONCES** ve el paso a *Retirada* con su fecha, el motivo "oferta expirada" y el sistema como responsable de la transición

##### Escenario 7: La lista de espera de la universidad ya cerró

- **DADO QUE** la oferta de María expiró y el único postulante en la lista de espera estudia en una universidad que ya inició su periodo de inscripciones
- **CUANDO** el coordinador abre el programa
- **ENTONCES** el sistema no le propone a ese postulante e indica que la lista de espera de esa universidad terminó al iniciar sus inscripciones

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Si el postulante no acepta antes de la fecha límite, la oferta expira (RF-15) | Regla | 1 | CUBIERTA |
| Vencimiento a las 23:59 (UTC-4) del día de cierre | Regla | 1, 4 | CUBIERTA |
| Al expirar, la postulación pasa a *Retirada* | Regla | 1 | CUBIERTA |
| El cupo vuelve a estar disponible (RF-16) | Regla | 1, 3 | CUBIERTA |
| Al expirar se libera el presupuesto comprometido por la oferta | Regla | 1 (efecto sobre el disponible) | DELEGADA (HU-12.1, HU-15.1/Escenario 13) |
| Se considera al siguiente de la lista de espera; la decisión de ofertar es humana | Regla | 2 | CUBIERTA |
| La lista de espera termina por universidad al iniciar sus inscripciones (FA-5) | Regla / FA | 7 | CUBIERTA |
| FA-1 Lista de espera vacía | FA | 3 | CUBIERTA |
| FA-2 Respuesta a tiempo (no expira) | FA | 4 | CUBIERTA |
| Expiración condicional sobre el estado "pendiente" (consistencia bajo concurrencia) | Regla | 5 | CUBIERTA |
| FA-3 Respuesta simultánea a la expiración | FA | 5 | CUBIERTA |
| Transición a *Retirada* registrada con el sistema como actor (RF-35) | Regla | 6 | CUBIERTA |
| No aplican OTP ni 2FA en la expiración (no la ejecuta una persona autenticada) | Regla | 1, 6 (el actor registrado es el sistema) | CUBIERTA |
| La oferta al siguiente la genera HU-15.1 verificando presupuesto y cupo | Regla | 2 (sugerencia del candidato) | DELEGADA (HU-15.1, HU-12.1) |
| FA-4 El siguiente de la lista ya tiene una beca activa de un programa "completo" | FA | — | NO PROBABLE (sin definir; pregunta abierta 2) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-15.2).*

## RF-16 — Aceptación o rechazo formal de la beca · *Debe tener*

### HU-16.1 Aceptar la oferta y convertirme en becario

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-16 | Debe tener | HU-15.1 | HU-16.1 |

#### Narrativa

- Como postulante seleccionado
- Quiero aceptar formalmente la oferta de beca
- Para convertirme en becario del programa

#### Reglas de negocio

- Al aceptar, la persona pasa a ser becario del programa y periodo correspondiente (RF-16).
- La beca queda *Otorgada* y la postulación pasa de *En evaluación* a *Otorgada* (S3; regla transversal de estados).
- Solo se acepta antes de la fecha límite (RF-15), que vence a las 23:59 (UTC-4) del día de cierre (ver HU-15.2).
- Una persona **puede tener más de una beca activa a la vez**, siempre dentro de la regla de no doble beneficio.
- Cada programa está marcado como **"completo"** o **"parcial"**; esa marca se usa solo para la regla de no doble beneficio y no para calcular el monto (configuración en HU-11.1).
- No se permiten dos becas activas y simultáneas de programas marcados como **"completos"** en programas distintos (regla transversal del PRD, con la terminología de C2; ver HU-01.2).
- La **suma de las becas** de una persona en un mismo periodo **no puede superar el costo de la matrícula de su universidad en ese periodo** (dato que aporta HU-14.2); no es un ejemplo, es el tope.
- **Decisión del cliente:** las dos reglas anteriores se aplican **a la vez**, no una u otra: la aceptación se bloquea si se incumple cualquiera de las dos (dos becas activas de programas "completos" distintos, o suma de becas por encima del costo de la matrícula del periodo).
- El monto que entra en esa suma es el monto fijo del periodo de cada beca.
- Cuentan como activas las becas *Otorgada*, *Activa*, *Suspendida* y *Renovada* (S6); se consultan en el historial (HU-18.1). El nuevo estado *Inactiva* se incorpora a la tabla de estados; **confirmado que no es un estado final** (desde *Inactiva* la beca puede volver a *Activa* o pasar a *Concluida*, decisión del coordinador de programa). **Decisión del cliente:** una beca *Inactiva* **no cuenta como beca activa** para esta regla.
- La verificación de no doble beneficio y la escritura de la aceptación deben ocurrir de forma consistente aunque dos aceptaciones lleguen casi al mismo tiempo en programas distintos (RNF de consistencia bajo concurrencia).
- El cambio de la beca a *Otorgada* queda justificado y registrado (regla transversal; ver HU-35.1).
- Si el programa exige compromiso, el siguiente paso es firmarlo (RF-17; ver HU-17.2).
- Si el programa no exige compromiso (HU-17.1), al aceptar la oferta la beca pasa de *Otorgada* a *Activa*. Si lo exige, sigue *Otorgada* hasta la firma (S10; ver HU-17.2). La transición *Otorgada* → *Activa* es la misma en ambos caminos y debe comportarse igual, incluido su registro de trazabilidad.
- Una persona puede tener varias postulaciones a la vez y volver a postular después de un rechazo, por lo que la verificación de no doble beneficio se ejecuta en cada aceptación y no al postular.
- Solo el titular de la oferta puede responderla: ni otro postulante ni el coordinador o un administrador pueden aceptar en su nombre (RF-37).
- Aceptar la oferta **no** exige código de un solo uso (OTP) ni verificación en dos pasos: el postulante no está en la lista de roles con segundo factor obligatorio y la acción no desembolsa dinero ni revoca una beca (Restricción Técnica del Cliente).
- La aceptación no se duplica: una oferta ya aceptada no genera un segundo otorgamiento.

#### Flujo de la HU

1. El postulante abre su oferta y elige "Aceptar".
2. El sistema verifica que la oferta no haya expirado y siga pendiente.
3. El sistema verifica la regla de no doble beneficio consultando el historial (HU-18.1; regla de HU-01.2): comprueba la marca "completo"/"parcial" de los programas y que la suma de los montos del periodo no supere el límite de la persona.
4. El sistema registra la aceptación: la beca queda *Otorgada*, la postulación pasa a *Otorgada* y la persona queda como becario del programa y periodo (HU-35.1).
5. Si el programa no exige compromiso, el sistema pasa la beca a *Activa* (S10). Si lo exige, la beca sigue *Otorgada* y el sistema indica que el siguiente paso es firmar el compromiso (HU-17.2).

#### Flujos alternos y de error

- **FA-1 (paso 2) Oferta expirada:** el sistema no acepta y muestra "Esta oferta expiró el 15/10/2026. No se puede aceptar."
- **FA-2 (paso 3) Dos programas marcados como "completos":** la persona ya tiene una beca activa de un programa "completo" y la oferta es de otro programa "completo"; el sistema no acepta y muestra el motivo.
- **FA-3 (paso 1) Oferta de otra persona:** un postulante intenta aceptar una oferta que no es suya; el sistema le niega el acceso.
- **FA-4 (paso 1) Aceptación en nombre del postulante:** un coordinador o administrador intenta marcar la oferta como aceptada por el postulante; el sistema le niega el acceso.
- **FA-5 (paso 2) Oferta ya respondida:** la oferta ya fue aceptada; el sistema no duplica el otorgamiento y muestra que ya fue aceptada.
- **FA-6 (paso 2) La postulación ya no está *En evaluación*:** la postulación salió de ese estado por otra vía (p. ej. quedó *Retirada*); el sistema no permite aceptar y muestra "Esta oferta ya no está disponible."
- **FA-7 (paso 3) La suma de las becas supera el límite del periodo:** aceptar haría que la suma de las becas de la persona en ese periodo superara su límite; el sistema no acepta y muestra el motivo.

#### Criterios de aceptación (14 escenarios)

##### Escenario 1: Aceptación a tiempo

- **DADO QUE** María tiene una oferta vigente de "Becas Ingeniería" 2026-2, que exige compromiso, y no tiene otras becas
- **CUANDO** elige "Aceptar"
- **ENTONCES** su beca queda *Otorgada*, su postulación pasa a *Otorgada*, aparece como becaria de "Becas Ingeniería" en 2026-2 y ve como siguiente paso firmar el compromiso

##### Escenario 2: Programa sin compromiso

- **DADO QUE** Luis tiene una oferta vigente de "Becas Salud" 2026-2, que no exige compromiso, y no tiene otras becas
- **CUANDO** elige "Aceptar"
- **ENTONCES** su postulación pasa a *Otorgada* y su beca pasa de *Otorgada* a *Activa* (S10)

##### Escenario 3: Ya tiene una beca activa de un programa marcado como completo

- **DADO QUE** María tiene una beca *Activa* en "Becas Salud", programa marcado como "completo", y la oferta de "Becas Ingeniería" es de un programa también marcado como "completo"
- **CUANDO** elige "Aceptar"
- **ENTONCES** el sistema no la acepta, no se crea ninguna beca en "Becas Ingeniería" y le muestra como motivo que ya tiene una beca activa de un programa "completo" en otro programa (el texto exacto del mensaje no se fija aquí: lo define HU-01.2, dueña de la regla)

##### Escenario 4: Una beca suspendida también cuenta

- **DADO QUE** la beca de María en "Becas Salud", programa marcado como "completo", está *Suspendida*
- **CUANDO** intenta aceptar otra oferta de un programa marcado como "completo"
- **ENTONCES** el sistema bloquea la aceptación con el mismo motivo del Escenario 3

##### Escenario 5: Solo el titular puede aceptar su oferta

- **DADO QUE** la oferta pendiente de "Becas Ingeniería" es de María
- **CUANDO** Pedro intenta aceptarla desde su propia cuenta
- **ENTONCES** el sistema le niega el acceso; no es su oferta

##### Escenario 6: El coordinador no acepta en nombre del postulante

- **DADO QUE** María tiene una oferta vigente de "Becas Ingeniería"
- **CUANDO** el coordinador del programa intenta marcarla como aceptada en su nombre
- **ENTONCES** el sistema le niega el acceso; solo María puede aceptar su propia oferta

##### Escenario 7: Doble aceptación

- **DADO QUE** María ya aceptó su oferta de "Becas Ingeniería" y su beca quedó *Otorgada*
- **CUANDO** vuelve a confirmar "Aceptar" sobre la misma oferta
- **ENTONCES** el sistema no genera un segundo registro de aceptación y muestra que la oferta ya fue aceptada

##### Escenario 8: Oferta expirada no se puede aceptar

- **DADO QUE** la oferta de María en "Becas Ingeniería" venció el 15/10/2026 sin respuesta
- **CUANDO** intenta aceptarla el 16/10/2026
- **ENTONCES** el sistema no la acepta y muestra "Esta oferta expiró el 15/10/2026. No se puede aceptar."

##### Escenario 9: La postulación ya no está En evaluación

- **DADO QUE** la postulación de María en "Becas Ingeniería" pasó a *Retirada* por otra vía antes de que ella respondiera su oferta
- **CUANDO** María intenta aceptar esa oferta
- **ENTONCES** el sistema no la acepta y muestra "Esta oferta ya no está disponible."

##### Escenario 10: Dos aceptaciones de programas completos casi simultáneas

- **DADO QUE** María tiene dos ofertas vigentes de programas marcados como "completos", una de "Becas Ingeniería" y otra de "Becas Salud"
- **CUANDO** las acepta casi en el mismo instante
- **ENTONCES** solo una de las dos becas queda *Otorgada* y la otra se bloquea con el motivo de no doble beneficio; nunca quedan las dos otorgadas ni la persona con dos becas activas de programas "completos"

##### Escenario 11: La aceptación queda registrada

- **DADO QUE** María aceptó su oferta de "Becas Ingeniería" 2026-2 y su beca quedó *Otorgada*
- **CUANDO** el coordinador consulta el historial de la postulación
- **ENTONCES** ve la aceptación con quién la hizo, la fecha y el motivo del cambio a *Otorgada*, y ese registro no puede editarse ni eliminarse después

##### Escenario 12: La suma de las becas supera el límite del periodo

- **DADO QUE** María tiene una beca *Activa* en "Becas Arte", programa marcado como "parcial", y aceptar la oferta de "Becas Ingeniería" haría que la suma de las dos becas en 2026-2 superara el límite definido para ella en ese periodo
- **CUANDO** elige "Aceptar"
- **ENTONCES** el sistema no la acepta, no se crea ninguna beca en "Becas Ingeniería" y le muestra como motivo que la suma de sus becas superaría ese límite

##### Escenario 13: Dos becas simultáneas dentro del límite

- **DADO QUE** María tiene una beca *Activa* en "Becas Arte", programa marcado como "parcial", y la suma de esa beca y la de la oferta de "Becas Ingeniería" en 2026-2 no supera el límite definido para ella
- **CUANDO** elige "Aceptar"
- **ENTONCES** el sistema acepta la oferta, su beca de "Becas Ingeniería" queda *Otorgada* y conserva su beca *Activa* de "Becas Arte"

##### Escenario 14: Una beca Inactiva no cuenta para el doble beneficio

- **DADO QUE** la beca de María en "Becas Salud", programa marcado como "completo", quedó *Inactiva* porque no se renovó, y la oferta de "Becas Ingeniería" es de otro programa también marcado como "completo"
- **CUANDO** elige "Aceptar"
- **ENTONCES** el sistema acepta la oferta y su beca de "Becas Ingeniería" queda *Otorgada*, porque la beca *Inactiva* de "Becas Salud" no cuenta como beca activa

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Al aceptar, la persona pasa a ser becario del programa y periodo (RF-16) | Regla | 1, 2 | CUBIERTA |
| La beca queda *Otorgada* y la postulación pasa a *Otorgada* | Regla | 1, 2 | CUBIERTA |
| Solo se acepta antes de la fecha límite (RF-15) / FA-1 Oferta expirada | Regla / FA | 8 | CUBIERTA |
| No doble beneficio: dos becas activas de programas marcados como "completos" | Regla | 3, 4, 10 | CUBIERTA |
| Tope de la suma de las becas de una persona en el periodo = costo de matrícula (FA-7) | Regla / FA | 12 | CUBIERTA; el valor del tope lo provee HU-14.2 |
| Las dos reglas de no doble beneficio se aplican a la vez | Regla | 3, 4, 10, 12 | CUBIERTA (cada regla probada por separado) |
| Dos becas simultáneas permitidas dentro del tope | Regla | 13 | CUBIERTA |
| *Inactiva* no es estado final (puede volver a *Activa* o pasar a *Concluida*, decisión del coordinador de programa) | Regla | — | DELEGADA (tabla de estados, CONTEXTO COMÚN) |
| Catálogo de estados que cuentan como activa (Otorgada/Activa/Suspendida/Renovada) | Regla | 3 (Activa), 4 (Suspendida) | DELEGADA (HU-01.2, HU-18.1) |
| *Inactiva* no cuenta como beca activa para el doble beneficio | Regla | 14 | CUBIERTA |
| Consistencia bajo concurrencia en la verificación de no doble beneficio | Regla | 10 | CUBIERTA |
| Registro justificado y trazable del cambio a *Otorgada* (RF-35) | Regla | 11 | CUBIERTA |
| Transición *Otorgada* → *Activa* si el programa no exige compromiso (S10) | Regla | 2 | CUBIERTA |
| Solo el titular puede aceptar su oferta (RF-37) / FA-3 | Regla / FA | 5 | CUBIERTA |
| FA-4 Aceptación en nombre del postulante | FA | 6 | CUBIERTA |
| FA-5 Oferta ya respondida (doble aceptación) | FA | 7 | CUBIERTA |
| FA-6 La postulación ya no está *En evaluación* | FA | 9 | CUBIERTA |
| Varias postulaciones y repostulación tras rechazo | Regla | — | DELEGADA (Área 1: RF-01, RF-02) |
| Si el programa exige compromiso, el siguiente paso es la firma | Regla | 1 (indicación del paso) | DELEGADA (HU-17.2) |
| Aceptar no exige OTP ni 2FA | Regla | — | NO PROBABLE (en disputa: preguntas abiertas 1 y 2; no se fija comportamiento) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-16.1).*

### HU-16.2 Rechazar la oferta

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-16 | Debe tener | HU-15.1 | HU-16.2 |

#### Narrativa

- Como postulante seleccionado
- Quiero rechazar formalmente la oferta de beca
- Para dejar el cupo libre si no voy a usar la beca

#### Reglas de negocio

- El postulante puede rechazar formalmente la oferta (RF-16).
- Al rechazarla, la postulación pasa a *Retirada* (S3).
- El cupo vuelve a estar disponible considerando la lista de espera (RF-16; S2); se considera al siguiente como en HU-15.2, reutilizando el mismo mecanismo, sin duplicarlo. Eso incluye el cierre de la lista de espera de cada universidad al iniciar sus inscripciones (HU-15.2, FA-5).
- Al rechazar, el monto que la oferta tenía comprometido deja de restar presupuesto: el proceso quedó rechazado definitivamente (HU-12.1).
- Solo se responde una oferta que no ha expirado (RF-15); el vencimiento ocurre a las 23:59 (UTC-4) del día límite (ver HU-15.2).
- Rechazar la oferta no impide volver a postular al mismo programa en un ciclo posterior ni a otros programas; la repostulación se rige por Área 1 (RF-01, RF-02).
- El rechazo es una decisión sobre la postulación y queda registrado con quién, cuándo y por qué (RF-35; ver HU-35.1).
- Solo el titular de la oferta puede rechazarla (RF-37).
- Una oferta ya aceptada no puede rechazarse después, y un rechazo repetido no produce un segundo retiro.
- No aplican OTP ni verificación en dos pasos (mismas razones que HU-16.1).

#### Flujo de la HU

1. El postulante abre su oferta y elige "Rechazar".
2. El sistema pide confirmar el rechazo.
3. El postulante confirma.
4. El sistema pasa la postulación a *Retirada*, registra el rechazo (HU-35.1) y libera el cupo y el presupuesto comprometido (HU-12.1).
5. El sistema muestra al coordinador el siguiente de la lista de espera (HU-15.2, pasos 5 y 6).

#### Flujos alternos y de error

- **FA-1 (paso 3) No confirma:** la oferta sigue pendiente.
- **FA-2 (paso 1) Oferta expirada:** el sistema no permite rechazarla (ya expiró, HU-15.2).
- **FA-3 (paso 1) Oferta de otra persona:** alguien distinto al titular intenta rechazarla; el sistema le niega el acceso.
- **FA-4 (paso 1) Oferta ya aceptada:** el sistema no permite el rechazo y muestra "Esta oferta ya fue aceptada."
- **FA-5 (paso 1) Rechazo repetido:** la oferta ya fue rechazada; el sistema muestra su estado actual sin registrar un segundo retiro.

#### Criterios de aceptación (8 escenarios)

##### Escenario 1: Rechazo de la oferta

- **DADO QUE** María tiene una oferta vigente de "Becas Ingeniería"
- **CUANDO** elige "Rechazar" y confirma
- **ENTONCES** su postulación pasa a *Retirada* y el cupo disponible del programa aumenta en 1

##### Escenario 2: Se considera al siguiente de la lista

- **DADO QUE** Pedro es el siguiente en la lista de espera de "Becas Ingeniería"
- **CUANDO** María rechaza su oferta
- **ENTONCES** el coordinador ve a Pedro como el siguiente postulante a considerar

##### Escenario 3: No confirma el rechazo

- **DADO QUE** María eligió "Rechazar"
- **CUANDO** cancela en la confirmación
- **ENTONCES** la oferta sigue pendiente y la postulación sigue *En evaluación*

##### Escenario 4: Oferta expirada

- **DADO QUE** la oferta de María venció el 15/10/2026
- **CUANDO** la abre el 16/10/2026
- **ENTONCES** no tiene la opción "Rechazar" y ve "Esta oferta expiró el 15/10/2026."

##### Escenario 5: Solo el titular puede rechazar su oferta

- **DADO QUE** la oferta pendiente es de María
- **CUANDO** Pedro intenta rechazarla desde su propia cuenta
- **ENTONCES** el sistema le niega el acceso

##### Escenario 6: Rechazar una oferta ya aceptada

- **DADO QUE** María ya aceptó su oferta de "Becas Ingeniería"
- **CUANDO** intenta rechazarla después
- **ENTONCES** el sistema no lo permite y muestra "Esta oferta ya fue aceptada."

##### Escenario 7: El rechazo queda registrado

- **DADO QUE** María rechazó y confirmó el rechazo de su oferta de "Becas Ingeniería" el 12/10/2026
- **CUANDO** el coordinador consulta el historial de la postulación
- **ENTONCES** ve que María la rechazó, con la fecha 12/10/2026 y el motivo del paso a *Retirada*, y ese registro no puede editarse ni eliminarse después

##### Escenario 8: Rechazo repetido

- **DADO QUE** María ya rechazó su oferta de "Becas Ingeniería" y su postulación está *Retirada*
- **CUANDO** vuelve a confirmar "Rechazar" sobre la misma oferta
- **ENTONCES** el sistema no registra un segundo retiro y muestra que la oferta ya fue rechazada

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| El postulante puede rechazar formalmente la oferta (RF-16) | Regla | 1 | CUBIERTA |
| Al rechazar, la postulación pasa a *Retirada* | Regla | 1 | CUBIERTA |
| El cupo vuelve a estar disponible y se considera al siguiente (reutiliza HU-15.2) | Regla | 1, 2 | CUBIERTA |
| Cierre de la lista de espera por universidad al iniciar sus inscripciones | Regla | — | DELEGADA (HU-15.2, Escenario 7) |
| Al rechazar se libera el presupuesto comprometido por la oferta | Regla | 1 (efecto sobre el disponible) | DELEGADA (HU-12.1, HU-15.1/Escenario 13) |
| Solo se responde una oferta no expirada (RF-15) / FA-2 | Regla / FA | 4 | CUBIERTA |
| Rechazar no impide volver a postular | Regla | — | DELEGADA (Área 1: RF-01, RF-02) |
| Rechazo registrado con quién, cuándo y por qué (RF-35) | Regla | 7 | CUBIERTA |
| Solo el titular puede rechazar (RF-37) / FA-3 | Regla / FA | 5 | CUBIERTA |
| FA-4 Una oferta ya aceptada no puede rechazarse | FA | 6 | CUBIERTA |
| FA-5 Rechazo repetido | FA | 8 | CUBIERTA |
| FA-1 No confirma el rechazo | FA | 3 | CUBIERTA |
| No aplican OTP ni 2FA | Regla | — | NO PROBABLE (misma disputa que HU-16.1, preguntas abiertas 1 y 2 de esa historia) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-16.2).*

## RF-17 — Firma de compromiso del becario · *Debe tener*

### HU-17.1 Configurar el compromiso que exige un programa

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-17 | Debe tener | HU-11.1 | HU-17.1 |

#### Narrativa

- Como coordinador de programa
- Quiero indicar si el programa exige un compromiso firmado y cuáles son sus condiciones
- Para que los becarios conozcan sus obligaciones antes de recibir desembolsos

#### Reglas de negocio

- Algunos programas exigen un compromiso firmado con las obligaciones del becario antes de iniciar los desembolsos (RF-17).
- Las condiciones del compromiso pueden variar según el programa (RF-17).
- Si el programa lo exige, los desembolsos no inician sin el compromiso registrado (RF-17; ver HU-17.2).
- Las condiciones del compromiso son distintas de las condiciones de permanencia del programa (RF-15, HU-11.2): son dos configuraciones separadas y no deben mezclarse en un mismo dato.
- Solo el coordinador responsable del programa configura su compromiso; evaluadores, becarios y el equipo financiero no tienen acceso a esta función (RF-37).
- El coordinador accede con verificación en dos pasos (Restricción Técnica del Cliente). No aplican OTP ni Firebase.
- **Sin definir en el PRD:** qué ocurre con los becarios que ya firmaron si el coordinador edita las condiciones o quita "Exige compromiso" después. Los escenarios no fijan si el cambio aplica retroactivamente, si se conserva lo firmado o si la edición se bloquea.

#### Flujo de la HU

1. El coordinador abre la configuración del programa.
2. Marca "Exige compromiso".
3. Escribe las condiciones del compromiso.
4. El sistema valida y guarda.
5. El programa muestra que exige compromiso y sus condiciones.

#### Flujos alternos y de error

- **FA-1 (paso 2) El programa no exige compromiso:** el coordinador deja la opción sin marcar; sus becarios no tienen paso de firma.
- **FA-2 (paso 4) Sin condiciones:** el sistema no guarda y muestra "Escribe las condiciones del compromiso."
- **FA-3 (paso 1) Rol sin permiso:** un evaluador, un becario o el equipo financiero intenta configurar el compromiso; el sistema le niega el acceso (RF-37).
- **FA-4 (paso 1) Coordinador de otro programa:** un coordinador que no coordina el programa intenta configurar su compromiso; el sistema bloquea el acceso y responde "No tienes permiso para gestionar este programa."
- **FA-5 (paso 4) Edición con becarios ya comprometidos:** el coordinador modifica las condiciones o quita "Exige compromiso" cuando ya hay becarios que firmaron o tienen firma pendiente. El PRD no define el efecto sobre esos becarios; queda como pregunta abierta y los criterios no fijan un comportamiento.

#### Criterios de aceptación (6 escenarios)

##### Escenario 1: Programa que exige compromiso

- **DADO QUE** "Becas Ingeniería" no exige compromiso
- **CUANDO** el coordinador marca "Exige compromiso", escribe "Cumplir 40 horas de servicio por periodo" y guarda
- **ENTONCES** el programa muestra que exige compromiso con esa condición

##### Escenario 2: Sin condiciones

- **DADO QUE** el coordinador marcó "Exige compromiso"
- **CUANDO** guarda sin escribir condiciones
- **ENTONCES** el sistema no guarda y muestra "Escribe las condiciones del compromiso."

##### Escenario 3: Condiciones distintas por programa

- **DADO QUE** "Becas Ingeniería" y "Becas Arte" exigen compromiso
- **CUANDO** el coordinador consulta ambos programas
- **ENTONCES** cada uno muestra sus propias condiciones

##### Escenario 4: Programa sin compromiso

- **DADO QUE** "Becas Salud" no exige compromiso
- **CUANDO** un becario de ese programa acepta su oferta (HU-16.1)
- **ENTONCES** no se le pide firmar ningún compromiso

##### Escenario 5: Finanzas no configura el compromiso

- **DADO QUE** Ana tiene el rol de administradora financiera
- **CUANDO** intenta configurar si "Becas Ingeniería" exige compromiso
- **ENTONCES** el sistema le niega el acceso a esa función

##### Escenario 6: Coordinador de otro programa

- **DADO QUE** Carlos no coordina "Becas Ingeniería"
- **CUANDO** intenta configurar su compromiso
- **ENTONCES** el sistema bloquea el acceso y responde "No tienes permiso para gestionar este programa."

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Algunos programas exigen compromiso firmado (RF-17) | Regla | 1 | CUBIERTA |
| Las condiciones del compromiso varían según el programa | Regla | 3 | CUBIERTA |
| Si el programa lo exige, los desembolsos no inician sin el compromiso | Regla | — | DELEGADA (HU-17.2, HU-20.1) |
| Las condiciones del compromiso son un dato distinto de las de permanencia | Regla | 1, 3 (se configuran y se muestran por separado) | CUBIERTA |
| FA-1 El programa no exige compromiso (efecto sobre el becario) | FA | 4 | CUBIERTA |
| FA-2 Sin condiciones no se guarda | FA | 2 | CUBIERTA |
| Solo el coordinador responsable configura; otros roles sin acceso (RF-37) / FA-3 | Regla / FA | 5 (representativo: finanzas), 6 | CUBIERTA |
| FA-4 Coordinador de otro programa | FA | 6 | CUBIERTA |
| 2FA de login del coordinador | Regla | — | DELEGADA (HU-37.1/HU-37.2, fuera del área) |
| FA-5 Edición con becarios ya comprometidos | FA | — | NO PROBABLE (sin definir en el PRD; preguntas abiertas 1 y 2) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-17.1).*

### HU-17.2 Firmar mi compromiso de becario

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-17 | Debe tener | HU-16.1, HU-17.1 | HU-17.2 |

#### Narrativa

- Como becario
- Quiero firmar el compromiso que exige mi programa
- Para que puedan iniciar mis desembolsos

#### Reglas de negocio

- La firma es una aceptación electrónica en la plataforma (S7); no requiere un servicio externo de firma digital.
- El becario firma las condiciones que define su programa (RF-17; ver HU-17.1).
- Si el programa exige compromiso, los desembolsos no pueden iniciar sin ese compromiso registrado (RF-17). La validación al momento de desembolsar vive en Área 4 (HU-20.1); esta HU garantiza que el estado "pendiente/firmado" quede consultable de forma confiable.
- Al registrarse la firma, la beca pasa de *Otorgada* a *Activa* (S10). Es la misma transición que produce HU-16.1 cuando el programa no exige compromiso y debe comportarse igual, incluido su registro.
- El compromiso queda registrado con la persona y la fecha, de forma permanente y sin edición retroactiva (RF-35, RNF de trazabilidad).
- Solo el propio becario puede firmar su compromiso: ni otro becario ni el coordinador pueden hacerlo en su nombre (RF-37).
- Una firma ya registrada no se duplica ni repite la transición de estado.
- Solo se firma mientras la beca está *Otorgada*; si ya salió de ese estado, no procede la firma (regla transversal de estados).
- Firmar el compromiso **no** exige código de un solo uso (OTP) ni verificación en dos pasos: el becario no está en la lista de roles con segundo factor obligatorio y la acción no mueve dinero (Restricción Técnica del Cliente).
- **Sin definir en el PRD:** si la firma debe conservar evidencia adicional (por ejemplo, la versión exacta de las condiciones vigentes al firmar, para el caso en que HU-17.1 las edite después) más allá de persona y fecha.

#### Flujo de la HU

1. Tras aceptar la oferta (HU-16.1), el becario ve el compromiso pendiente de firma.
2. Lee las condiciones del compromiso de su programa.
3. Elige "Acepto y firmo".
4. El sistema registra el compromiso con la persona y la fecha, y pasa la beca de *Otorgada* a *Activa* (S10; registro en HU-35.1).
5. Los desembolsos del becario ya pueden iniciar (HU-19.1, HU-20.1).

#### Flujos alternos y de error

- **FA-1 (paso 3) No firma todavía:** el compromiso sigue pendiente y los desembolsos no pueden iniciar.
- **FA-2 (paso 1) El programa no exige compromiso:** no hay paso de firma.
- **FA-3 (paso 1) Compromiso de otro becario:** un becario intenta firmar el compromiso de otra persona; el sistema le niega el acceso.
- **FA-4 (paso 1) Firma en nombre del becario:** un coordinador intenta firmar por el becario; el sistema le niega el acceso.
- **FA-5 (paso 3) Firma repetida:** el compromiso ya está firmado; el sistema lo muestra así, sin registrar una segunda firma ni repetir la transición de estado.
- **FA-6 (paso 1) La beca ya no está *Otorgada*:** la beca está *Activa*, *Suspendida* o *Revocada*; el sistema no permite la firma y muestra el estado actual de la beca (regla transversal de estados).

#### Criterios de aceptación (8 escenarios)

##### Escenario 1: Firma del compromiso

- **DADO QUE** María es becaria de "Becas Ingeniería", que exige compromiso
- **CUANDO** lee las condiciones y elige "Acepto y firmo"
- **ENTONCES** el compromiso queda registrado con su nombre y la fecha, deja de aparecer como pendiente y su beca pasa de *Otorgada* a *Activa*

##### Escenario 2: Compromiso pendiente mientras no se firma

- **DADO QUE** María es becaria de "Becas Ingeniería", que exige compromiso, y aún no lo ha firmado
- **CUANDO** consulta el estado de su beca
- **ENTONCES** ve el compromiso como pendiente de firma y su beca sigue *Otorgada* (el rechazo del desembolso con ese estado lo verifica HU-20.1)

##### Escenario 3: Programa sin compromiso

- **DADO QUE** Luis es becario de "Becas Salud", que no exige compromiso
- **CUANDO** el administrador financiero registra su primer desembolso
- **ENTONCES** el sistema no pide ningún compromiso

##### Escenario 4: Solo el titular firma su compromiso

- **DADO QUE** el compromiso pendiente es de María
- **CUANDO** Luis intenta firmarlo desde su propia cuenta
- **ENTONCES** el sistema le niega el acceso

##### Escenario 5: Doble firma

- **DADO QUE** María ya firmó el compromiso de "Becas Ingeniería" y su beca está *Activa*
- **CUANDO** intenta firmar el mismo compromiso otra vez
- **ENTONCES** el sistema no registra una segunda firma y muestra que el compromiso ya está firmado

##### Escenario 6: La beca ya no está Otorgada

- **DADO QUE** la beca de María fue revocada antes de que firmara el compromiso
- **CUANDO** intenta firmar el compromiso pendiente
- **ENTONCES** el sistema no lo permite y muestra el estado actual de su beca

##### Escenario 7: El coordinador no firma en nombre del becario

- **DADO QUE** el compromiso pendiente de "Becas Ingeniería" es de María
- **CUANDO** el coordinador del programa intenta firmarlo en su nombre
- **ENTONCES** el sistema le niega el acceso; solo María puede firmar su propio compromiso

##### Escenario 8: La firma queda registrada de forma permanente

- **DADO QUE** María firmó el compromiso de "Becas Ingeniería" el 20/10/2026
- **CUANDO** el coordinador consulta el historial de su beca
- **ENTONCES** ve la firma registrada con el nombre de María y la fecha 20/10/2026, y ese registro no puede editarse ni eliminarse después

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| La firma es una aceptación electrónica en la plataforma (S7) | Regla | 1 | CUBIERTA |
| El becario firma las condiciones que define su programa | Regla | 1 | CUBIERTA |
| Si el programa exige compromiso, los desembolsos no inician sin registro | Regla | 2 (estado consultable como pendiente) | DELEGADA (HU-20.1); efecto local CUBIERTO |
| Al firmar, la beca pasa de *Otorgada* a *Activa* (misma transición que HU-16.1) | Regla | 1 | CUBIERTA |
| Compromiso registrado con persona y fecha, permanente y sin edición retroactiva (RF-35) | Regla | 8 | CUBIERTA |
| Solo el propio becario firma (RF-37) / FA-3 | Regla / FA | 4 | CUBIERTA |
| FA-4 Firma en nombre del becario (coordinador) | FA | 7 | CUBIERTA |
| FA-5 Firma repetida | FA | 5 | CUBIERTA |
| FA-6 La beca ya no está *Otorgada* | FA | 6 | CUBIERTA |
| FA-2 El programa no exige compromiso (no hay paso de firma) | FA | 3 | CUBIERTA |
| FA-1 No firma todavía (el compromiso sigue pendiente) | FA | 2 | CUBIERTA |
| No exige OTP ni 2FA | Regla | — | NO PROBABLE (misma disputa que HU-16.1; pregunta abierta 3 de esta historia) |
| Evidencia adicional de la versión de las condiciones firmadas | Regla | — | NO PROBABLE (sin definir en el PRD; pregunta abierta 1) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-17.2).*

## RF-18 — Historial de becas por postulante o becario · *Debe tener*

### HU-18.1 Consultar el historial de becas de una persona por programa o periodo

| Requerimiento | Prioridad | Depende de | Integra (v2.0) |
|---|---|---|---|
| RF-18 | Debe tener | HU-16.1 | HU-18.1, HU-18.2 |

#### Narrativa

- Como coordinador de programa
- Quiero consultar todas las becas que ha recibido una persona, en cualquier programa y periodo, y también el historial por programa o por periodo
- Para conocer su trayectoria, verificar la regla de no doble beneficio y ver todas las becas de un programa o de un periodo sin buscar persona por persona

#### Reglas de negocio

- Existe un historial completo de todas las becas de una persona, en cualquier programa y periodo (RF-18).
- El historial se puede consultar por persona, por programa o por periodo (RF-18).
- La búsqueda por persona se hace por su **número de cédula**, que es la identificación única de una persona.
- El historial sirve para verificar la regla de no doble beneficio (RF-18, RF-01; regla en HU-01.2). Cuentan como becas activas las *Otorgada*, *Activa*, *Suspendida* y *Renovada* (S6); **confirmado que *Inactiva* no cuenta como activa**. La definición de "cuenta como activa" debe ser una sola, compartida con la verificación de HU-16.1.
- El historial incluye becas en cualquier estado, también *Concluida*, *Revocada* e *Inactiva*.
- Cada beca muestra programa, periodo, **monto fijo del periodo**, la **marca "completo"/"parcial" de su programa** y estado, porque la regla de no doble beneficio distingue los programas marcados como "completos" y necesita los montos del periodo para verificar el tope de la suma.
- El historial no expone información socioeconómica del postulante (ingreso familiar declarado, composición familiar): esa información solo es visible para quien la necesita para evaluar o decidir (RNF de confidencialidad).
- Los periodos anteriores siguen consultables (RF-38; ver HU-38.1).
- Esta consulta no es solo de reportería: HU-16.1 la invoca en cada aceptación de oferta para verificar el no doble beneficio, por lo que la búsqueda por persona debe responder como parte de un flujo transaccional y no solo como pantalla administrativa.
- El acceso está limitado según la función de cada rol (RF-37): un becario solo ve su propio historial y no accede a la búsqueda agregada por programa o periodo.
- El coordinador que consulta accede con verificación en dos pasos (Restricción Técnica del Cliente). No aplican OTP ni Firebase. Esta HU es de solo lectura: no produce registros de trazabilidad, pero depende de que las historias del ciclo de vida de la beca hayan registrado sus transiciones.
- **Sin definir en el PRD:** qué otros roles (dirección, equipo financiero, evaluadores del comité) necesitan acceso directo a este historial consolidado. Los escenarios cubren al coordinador y el límite del becario, sin fijar el alcance de los demás roles (la pregunta 2 del cliente sigue pendiente).

#### Flujo de la HU

1. El coordinador abre el historial de becas y busca a la persona por su número de cédula.
2. El sistema muestra todas sus becas, con programa, periodo, monto del periodo, marca "completo"/"parcial" del programa y estado.
3. El sistema señala cuáles cuentan como activas (S6).
4. El coordinador puede abrir la línea de tiempo de una beca (HU-28.1).
5. En lugar de una persona, el coordinador puede elegir un programa o un periodo.
6. El sistema muestra las becas que corresponden, con persona, programa, periodo y estado.
7. El coordinador puede quitar el filtro y volver a la búsqueda por persona.

#### Flujos alternos y de error

- **FA-1 (paso 2) Persona sin becas:** el sistema muestra "María Gómez no tiene becas registradas."
- **FA-2 (paso 1) Persona no encontrada:** el sistema muestra "No se encontró a una persona con la identificación V-12345678."
- **FA-3 (paso 6) Sin resultados:** el sistema muestra "No hay becas para el periodo 2025-1." o "No hay becas en el programa Becas Arte."
- **FA-4 (paso 1) Becario consultando a otra persona:** el sistema le niega el acceso; un becario solo ve su propio historial (RF-37).
- **FA-5 (paso 5) Becario o postulante en la búsqueda agregada:** el sistema le niega el acceso a la vista por programa o por periodo (RF-37).

#### Criterios de aceptación (13 escenarios)

##### Escenario 1: Becas en varios programas y periodos, con señal de beca activa

- **DADO QUE** María tuvo una beca *Concluida* en "Becas Arte" 2025-2 y tiene una beca *Suspendida* en "Becas Ingeniería" 2026-2
- **CUANDO** el coordinador consulta su historial
- **ENTONCES** ve ambas becas con programa, periodo, monto del periodo, marca "completo"/"parcial" del programa y estado, y la beca *Suspendida* aparece señalada como activa y la *Concluida* no

##### Escenario 2: Persona sin becas

- **DADO QUE** María Gómez es postulante y nunca tuvo una beca
- **CUANDO** el coordinador consulta su historial
- **ENTONCES** el sistema muestra "María Gómez no tiene becas registradas."

##### Escenario 3: Persona no encontrada

- **DADO QUE** no existe ninguna persona con la identificación V-12345678
- **CUANDO** el coordinador la busca
- **ENTONCES** el sistema muestra "No se encontró a una persona con la identificación V-12345678."

##### Escenario 4: Consulta por programa

- **DADO QUE** "Becas Ingeniería" tiene becas de María y de Pedro
- **CUANDO** el coordinador consulta el historial por "Becas Ingeniería"
- **ENTONCES** ve las becas de María y de Pedro con su periodo y estado

##### Escenario 5: Consulta por un periodo anterior

- **DADO QUE** el periodo actual es 2026-2 y en 2026-1 hubo becas en "Becas Ingeniería" y "Becas Arte"
- **CUANDO** el coordinador consulta el historial por 2026-1
- **ENTONCES** ve las becas de ambos programas en ese periodo y ninguna de otro periodo

##### Escenario 6: Periodo sin becas

- **DADO QUE** no hubo becas en 2025-1
- **CUANDO** el coordinador consulta el historial por 2025-1
- **ENTONCES** el sistema muestra "No hay becas para el periodo 2025-1."

##### Escenario 7: El historial no expone datos socioeconómicos

- **DADO QUE** el coordinador consulta el historial de becas de María
- **CUANDO** el sistema muestra sus becas con programa, periodo, monto del periodo, marca del programa y estado
- **ENTONCES** no se muestra su ingreso familiar declarado ni su composición familiar en esa vista

##### Escenario 8: Un becario no consulta el historial de otra persona

- **DADO QUE** Luis es becario de "Becas Salud"
- **CUANDO** intenta consultar el historial de becas de María
- **ENTONCES** el sistema le niega el acceso; solo puede ver su propio historial

##### Escenario 9: Un becario no usa la búsqueda por programa

- **DADO QUE** Luis tiene el rol de becario
- **CUANDO** intenta consultar el historial agregado de "Becas Ingeniería" por programa
- **ENTONCES** el sistema le niega el acceso a esa vista

##### Escenario 10: El historial incluye becas revocadas

- **DADO QUE** Pedro tuvo una beca *Revocada* en "Becas Arte" 2025-2
- **CUANDO** el coordinador consulta su historial
- **ENTONCES** ve esa beca con su programa, periodo, monto del periodo, marca del programa y estado *Revocada*, y no aparece señalada como activa

##### Escenario 11: Programa sin becas registradas

- **DADO QUE** "Becas Música" no tiene ninguna beca registrada
- **CUANDO** el coordinador consulta el historial por "Becas Música"
- **ENTONCES** el sistema muestra "No hay becas en el programa Becas Música."

##### Escenario 12: Un postulante no usa la búsqueda por programa

- **DADO QUE** María tiene el rol de postulante
- **CUANDO** intenta consultar el historial agregado de "Becas Ingeniería" por programa
- **ENTONCES** el sistema le niega el acceso a esa vista

##### Escenario 13: El historial incluye becas inactivas, sin señal de activa

- **DADO QUE** la beca de Luis en "Becas Salud" 2025-2 quedó *Inactiva* porque no se renovó
- **CUANDO** el coordinador consulta su historial
- **ENTONCES** ve esa beca con su programa, periodo, monto del periodo, marca del programa y estado *Inactiva*, y no aparece señalada como activa, porque *Inactiva* no cuenta como beca activa

#### Matriz de cobertura

| Elemento | Tipo | Escenario(s) | Estado |
|:--- |:--- |:--- |:--- |
| Historial completo de las becas de una persona, en cualquier programa y periodo (RF-18) | Regla | 1 | CUBIERTA |
| Consultable por persona, por programa o por periodo | Regla | 1 (persona), 4 (programa), 5 (periodo) | CUBIERTA |
| La búsqueda por persona usa el número de cédula | Regla | 1, 3 | CUBIERTA |
| Catálogo de estados que cuentan como activos (Otorgada/Activa/Suspendida/Renovada) | Regla | 1 (Suspendida), 10 (contraste con *Revocada*) | DELEGADA (HU-01.2) |
| El historial incluye becas en cualquier estado, también *Concluida*, *Revocada* e *Inactiva* | Regla | 1 (Concluida), 10 (Revocada), 13 (Inactiva) | CUBIERTA |
| *Inactiva* no es estado final (puede volver a *Activa* o pasar a *Concluida*, decisión del coordinador de programa) | Regla | 13 | DELEGADA (tabla de estados, CONTEXTO COMÚN); esta HU solo consulta el estado vigente |
| *Inactiva* no cuenta como beca activa para el doble beneficio | Regla | 13 | CUBIERTA |
| Cada beca muestra programa, periodo, monto del periodo, marca "completo"/"parcial" y estado | Regla | 1, 10, 13 | CUBIERTA |
| No expone información socioeconómica (RNF de confidencialidad) | Regla | 7 | CUBIERTA |
| Los periodos anteriores siguen consultables (RF-38) | Regla | 5 | CUBIERTA |
| La consulta se invoca transaccionalmente desde HU-16.1, no solo para reportería | Regla | — | DELEGADA (HU-16.1, Escenarios 3, 4, 10, 12 y 13) |
| FA-1 Persona sin becas | FA | 2 | CUBIERTA |
| FA-2 Persona no encontrada | FA | 3 | CUBIERTA |
| FA-3 Sin resultados por periodo | FA | 6 | CUBIERTA |
| FA-3 Sin resultados por programa | FA | 11 | CUBIERTA |
| FA-4 Becario consultando a otra persona (RF-37) | FA | 8 | CUBIERTA |
| FA-5 Becario en la búsqueda agregada | FA | 9 | CUBIERTA |
| FA-5 Postulante en la búsqueda agregada | FA | 12 | CUBIERTA |
| 2FA de login del coordinador | Regla | — | DELEGADA (HU-37.1/HU-37.2, fuera del área) |
| Alcance de acceso de dirección, finanzas y evaluadores | Regla | — | NO PROBABLE (sin definir; preguntas abiertas 1 y 2; pregunta 2 del cliente pendiente) |

*Preguntas abiertas de esta historia: ver [Anexo B](anexo-b-preguntas-abiertas.md) (HU-18.1).*
