## ASCENSO — División de requerimientos por integrante

N ov 21, 2026 · @Sebastian Perez

## Criterio de división

Cada integrante toma 8 RF agrupados por bounded context: es dueño de sus agregados y se comunica con los demás solo por eventos o por los contratos ya definidos en el kit de arquitectura v1.3.

- Punto de partida: el reparto sugerido en el DDD v3.2 (§2.2), ajustado a los dueños de la arquitectura v1.3. Cambian de mano cuatro RF de reportería (RF-33, RF-34, RF-36 y RF-40) para que cada reporte quede con quien produce sus datos.

- Un agregado, un dueño. Las dos excepciones vienen del propio DDD: Postulacion la comparten Postulación y Evaluación (Partnership), y Beca la comparten quien la crea y paga y quien gestiona sus cambios de estado.

- Las HU van con su RF padre, aunque tres de ellas viven en S2 (HU-07.1, HU-17.1 y HU- 25.1). Se acuerdan con el dueño de S2, como se indica en Puntos de contacto.

- S6 (Comunicaciones y Reportería) se reparte por consumidor: Daniel monta la base de envío de mensajes, y cada reporte o aviso lo construye quien produce los eventos que lo alimentan.

## Resumen

Los 40 RF quedan en cinco bloques de 8, sin repetir ninguno. Cada bloque tiene un compañero con el que comparte un agregado o un servicio, y con él conviene trabajar más cerca.

| Integrante Bloque RF Servicios Compañero más relacionado Jesús Catálogo, Programas 11, 12, S1, S2 y S6 (impacto Leonel: reservas y y Fondos, Identidad y 13, 14, del donante) desembolsos Trazabilidad 33, 35, contra el 37, 38 presupuesto Daniel Postulación y 01, 02, S3 y S6 (avisos, Diego: comparten Comunicaciones 03, 04, recordatorios y el agregado 05, 29, consultas) Postulacion en S3 |
| --- |
| 30, 32 |


| Integrante Bloque |   | RF | Servicios | Compañero más |
| --- | --- | --- | --- | --- |
|   |   |   |   | relacionado |
| Diego | Evaluación y | 06, 07, | S3 y S6 (aviso al | Daniel: comparten |
|   | Selección | 08, 09, | comité y panel) | el agregado |
|   |   | 10, 31, |   | Postulacion en S3 |
|   |   | 39, 40 |   |   |
| Leonel | Oferta, compromiso, | 15, 16, | S4 y S6 (estado | Sebas: comparten |
|   | desembolsos y estado | 17, 18, | financiero y | el agregado Beca |
|   | financiero | 19, 20, | exportación para | en S4 |
|   |   | 23, 36 | auditoría) |   |
| Sebas | Ciclo de vida de la | 21, 22, | S4, S5 y S6 (reporte | Leonel: comparten |
|   | beca y Seguimiento | 24, 25, | institucional) | el agregado Beca |
|   | Académico | 26, 27, |   | en S4 |
|   |   | 28, 34 |   |   |

## Jesús — Catálogo, Programas y Fondos, Identidad y Trazabilidad

Jesús construye la base que los demás consumen: usuarios y permisos, programas, presupuesto, universidades, periodos y el registro de decisiones. Trabaja más cerca de Leonel, porque cada oferta y cada desembolso reservan, confirman o liberan presupuesto en S2.

| RF | Requisito | HU | Servicio |
| --- | --- | --- | --- |
| RF-11 Configuración de |   | HU-11.1 Crear un programa de becas con | S2 |
|   | programas | su monto por periodo · HU-11.2 Configurar |   |
|   |   | los donantes, la elegibilidad y la |   |
|   |   | renovación del programa |   |
| RF- | Presupuesto y cupos | HU-12.1 Consultar el presupuesto y el | S2 |
| 12 |   | cupo disponibles y verificarlos antes de |   |
|   |   | otorgar · HU-12.2 Contar las renovaciones |   |
|   |   | pendientes como no disponibles |   |
| RF- | Donantes y aportes | HU-13.1 Registrar un donante · HU-13.2 | S2 |
| 13 |   | Registrar los aportes de un donante y |   |
|   |   | consultar su historial |   |


| RF | Requisito | HU | Servicio |
| --- | --- | --- | --- |
| RF- | Universidades y | HU-14.1 Registrar una universidad aliada | S2 |
| 14 | convenios | con su convenio y sus carreras elegibles · |   |
|   |   | HU-14.2 Registrar el costo de matrícula |   |
|   |   | por periodo sin perder los anteriores · HU- |   |
|   |   | 14.3 Registrar la fecha de inicio de |   |
|   |   | inscripciones de cada universidad por |   |
|   |   | periodo |   |
| RF- | Reporte de impacto | HU-33.1 Generar el reporte de impacto de | S6 |
| 33 |   | un donante con datos agregados o |   |
|   |   | anonimizados · HU-33.2 Registrar el |   |
|   |   | consentimiento para ser presentado a mi |   |
|   |   | donante |   |
| RF- | Registro de decisiones | HU-35.1 Registrar cada decisión con | Transversal |
| 35 |   | quién, cuándo y por qué, sin edición |   |
|   |   | posterior · HU-35.2 Consultar el registro |   |
|   |   | de decisiones |   |
| RF- | Usuarios y permisos | HU-37.1 Crear usuarios internos y tipos de | S1 |
| 37 |   | usuario con acceso limitado a su función · |   |
|   |   | HU-37.2 Iniciar sesión con verificación en |   |
|   |   | dos pasos |   |
| RF- | Calendario académico | HU-38.1 Registrar y consultar los periodos | S2 |
| 38 |   | académicos · HU-38.2 Marcar el periodo |   |
|   |   | actual sin interrumpir renovaciones |   |

## Parte del DDD v3.2

- Secciones: §7 Catálogo Institucional, §8 Programas y Fondos, §13 Identidad y Acceso, §14 (Trazabilidad y Reportería e Impacto), §15.7 Estados del periodo académico, §15.13 Estados del usuario y §16.4 Flujos de consistencia (reservas).

- Agregados: CalendarioAcademico , UniversidadAliada , Programa , Donante , PresupuestoPrograma , Usuario , TipoUsuario , ClienteApiUniversidad y ConsentimientoDonante , más el registro inmutable RegistroDecision .

- Eventos que publica: PeriodoAbierto , ProgramaCreado , AporteRegistrado , CupoLiberado , PeriodoMarcadoActual y CostoMatriculaVigente .

- Lo que entrega a todos: autenticación con 2FA, OTP de operación, permisos por tipo de usuario y el componente para registrar decisiones.

Coordina con: Leonel (reserva, confirmación y desembolsos contra PresupuestoPrograma ) y Sebas (liberación por revocación y reservas de renovación).


## Daniel — Postulación y Comunicaciones

Daniel lleva al postulante desde la convocatoria hasta la verificación de elegibilidad, y monta la base de S6 para enviar avisos, recordatorios y consultas. Trabaja más cerca de Diego: los dos escriben sobre el agregado Postulacion dentro de S3.

| RF | Requisito | HU | Servicio |
| --- | --- | --- | --- |
| RF-01 Registro de postulantes |   | HU-01.1 Crear mi perfil de postulante y | S3 |
|   |   | postular a una convocatoria abierta · HU- |   |
|   |   | 01.2 Impedir postular con una beca activa en |   |
|   |   | el mismo periodo |   |
| RF- | Publicación de | HU-02.1 Configurar el ciclo de postulación: | S3 |
| 02 | convocatorias | fechas, cupo, criterios y documentos · HU- |   |
|   |   | 02.2 Publicar la convocatoria y aceptar |   |
|   |   | postulaciones solo dentro de su ventana |   |
| RF- | Carga de documentos | HU-03.1 Cargar mis documentos de soporte | S3 |
| 03 |   | y ver los que faltan · HU-03.2 Rechazar un |   |
|   |   | documento con motivo y permitir que se |   |
|   |   | vuelva a subir |   |
| RF- | Verificación de | HU-04.1 Verificar automáticamente la | S3 |
| 04 | elegibilidad | elegibilidad básica y mostrar el motivo · HU- |   |
|   |   | 04.2 Anular un resultado de no elegible en |   |
|   |   | un caso límite |   |
| RF- | Retiro voluntario | HU-05.1 Retirar mi postulación antes de que | S3 |
| 05 |   | empiece la evaluación |   |
| RF- | Notificaciones | HU-29.1 Recibir los hitos de mi postulación o | S6 |
| 29 | automáticas | beca por correo y por los canales del |   |
|   |   | programa · HU-29.2 Enviar notificaciones a |   |
|   |   | todo un ciclo sin ralentizar la plataforma |   |
| RF- | Recordatorios | HU-30.1 Configurar y recibir recordatorios | S6 |
| 30 |   | antes de un vencimiento |   |
| RF- | Mensajería | HU-32.1 Enviar una consulta sobre mi | S6 |
| 32 |   | postulación o beca · HU-32.2 Responder y |   |
|   |   | dar seguimiento a una consulta |   |

Parte del DDD v3.2


- Secciones: §9 Postulación (9a Ciclo de postulación y postulante, 9b La postulación), §14 (Comunicaciones), §15.1 Estados de la postulación, §15.6 Estados de la consulta y §15.8 Estados del documento cargado.

- Agregados: CicloPostulacion , Postulante , Postulacion (compartido con Diego), Consulta y RegistroEnvio .

- Servicios de dominio: PoliticaVentanaPostulacion y VerificadorElegibilidad (patrón Specification).

- Eventos que publica: PostulacionRecibida , DocumentosCompletados , PostulacionVerificada , CicloPublicado y PeriodoEvaluacionAbierto .

- Eventos que consume: los de todos los servicios para notificar (RF-29), sin duplicar envíos gracias a RegistroEnvio .

Coordina con: Diego (estados y métodos de Postulacion ), Leonel (HU-01.2 consulta BeneficiosPersona en S4) y Jesús (programa, ciclo y criterios de S2).

## Diego — Evaluación y Selección

Diego lleva el contexto Core del sistema: comité, rúbrica, puntuación, consolidación, ranking, empates y lista de espera, más el panel de dirección en tiempo real. Trabaja más cerca de Daniel, porque los dos escriben sobre Postulacion en S3, y su salida

( PostulacionSeleccionada.v1 ) es la entrada de Leonel.

| RF | Requisito | HU | Servicio |
| --- | --- | --- | --- |
| RF- | Comité de evaluación | HU-06.1 Conformar el comité de un | S3 |
| 06 |   | programa · HU-06.2 Asignar los evaluadores |   |
|   |   | de cada postulación |   |
| RF- | Rúbrica de evaluación | HU-07.1 Definir la rúbrica del programa con | S3 |
| 07 |   | pesos que sumen 100 % (vive en S2) · HU- |   |
|   |   | 07.2 Bloquear la rúbrica al comenzar la |   |
|   |   | evaluación del ciclo |   |
| RF- | Puntuaciones y | HU-08.1 Puntuar, comentar y enviar mi | S3 |
| 08 | comentarios | evaluación · HU-08.2 Ocultar las |   |
|   |   | puntuaciones de otros evaluadores hasta el |   |
|   |   | cierre |   |
| RF- | Consolidación y ranking HU-09.1 Calcular la puntuación consolidada |   | S3 |
| 09 |   | y marcar las diferencias grandes · HU-09.2 |   |
|   |   | Generar el ranking respetando cupo y |   |
|   |   | presupuesto |   |


| RF | Requisito | HU | Servicio |
| --- | --- | --- | --- |
| RF-10 Empates y casos límite |   | HU-10.1 Resolver un empate o un caso límite | S3 |
|   |   | con justificación · HU-10.2 Dejar en lista de |   |
|   |   | espera a los elegibles no otorgados |   |
| RF-31 Notificaciones al comité HU-31.1 Avisar al comité de nuevas |   |   | S6 |
|   |   | asignaciones y del cierre de la evaluación |   |
| RF- | Conflictos de interés | HU-39.1 Declarar un conflicto de interés y | S3 |
| 39 |   | restringir mi participación en ese caso |   |
| RF- | Indicadores en tiempo | HU-40.1 Ver los indicadores clave | S6 |
| 40 | real | actualizados sin recargar la página |   |

## Parte del DDD v3.2

- Secciones: §10 Evaluación y Selección, §15.9 Estados de la asignación de evaluador, §15.10 Estados de la evaluación y §18.2 Modelos de lectura (ranking y panel).

- Agregados: Comite , Evaluacion , ConflictoInteres y la parte de evaluación de Postulacion (compartido con Daniel).

- Servicio de dominio: ConsolidadorPuntuaciones (patrón Strategy por método de consolidación).

- Eventos que publica: ConflictoDeclarado , EvaluacionEnviada , ( SugerenciaListaEspera.v1 ). PostulacionConsolidada , ListaEsperaConfirmada y el de integración PostulacionSeleccionada.v1 ; también la sugerencia de lista de espera

- Eventos que consume: PeriodoEvaluacionAbierto (Daniel), OfertaRechazada y OfertaExpirada (Leonel) para el siguiente de la lista; para el panel, postulaciones, desembolsos, revocaciones e IncumplimientoDetectado .

Coordina con: Daniel ( Postulacion ), Leonel (de la selección a la oferta y vuelta a la lista de espera) y Jesús (rúbrica en Programa , disponibilidad de cupo y presupuesto).

## Leonel — Oferta, compromiso, desembolsos y estado financiero

Leonel convierte una postulación seleccionada en becario y mueve el dinero: oferta, aceptación sin doble beneficio, compromiso, calendario y confirmación de desembolsos, y los dos reportes que salen de esas cifras. Trabaja más cerca de Sebas, porque los dos escriben sobre el agregado Beca dentro de S4.


| RF | Requisito | HU | Servicio |
| --- | --- | --- | --- |
| RF- | Emisión de ofertas | HU-15.1 Generar la oferta formal de | S4 |
| 15 |   | beca y comunicarla con su fecha |   |
|   |   | límite · HU-15.2 Expirar una oferta sin |   |
|   |   | respuesta y considerar al siguiente |   |
| RF- | Aceptación/rechazo de ofertas | HU-16.1 Aceptar la oferta y | S4 |
| 16 |   | convertirme en becario · HU-16.2 |   |
|   |   | Rechazar la oferta |   |
| RF- | Compromiso del becario | HU-17.1 Configurar el compromiso | S4 |
| 17 |   | que exige un programa (vive en S2) · |   |
|   |   | HU-17.2 Firmar mi compromiso de |   |
|   |   | becario |   |
| RF- | Historial de becas | HU-18.1 Consultar el historial de | S4 |
| 18 |   | becas de una persona por programa |   |
|   |   | o periodo |   |
| RF- | Calendario de desembolsos | HU-19.1 Generar el calendario de | S4 |
| 19 |   | desembolsos de un becario sin |   |
|   |   | superar lo aprobado |   |
| RF- | Registro y confirmación de | HU-20.1 Registrar y confirmar un | S4 |
| 20 | desembolsos | desembolso con un código de un |   |
|   |   | solo uso · HU-20.2 Resolver un |   |
|   |   | desembolso fallido o devuelto · HU- |   |
|   |   | 20.3 Registrar y modificar los datos |   |
|   |   | de pago del becario con un código de |   |
|   |   | un solo uso |   |
| RF- | Reporte financiero por programa HU-23.1 Consultar y exportar el |   | S6 |
| 23 |   | estado financiero de un programa |   |
| RF- | Exportación para auditoría | HU-36.1 Exportar el uso de los fondos | S6 |
| 36 |   | y el registro de decisiones para una |   |
|   |   | auditoría |   |

## Parte del DDD v3.2

- Secciones: §11a Oferta, no doble beneficio, datos de pago y ventana de renovación (sin la ventana); §11b Beca, en lo que toca a compromiso y calendario de desembolsos; §15.3 Estados de la oferta, §15.4 Estados del desembolso, §15.5 Estados del pago y §17 Fábricas ( BecaFactory ).


- Agregados: Oferta , BeneficiosPersona , DatosPago y Beca (compartido con Sebas).

- Servicio de dominio: ServicioAceptacionOferta , que acepta la oferta y registra el beneficio en la misma transacción (D-3).

- Eventos que publica: OfertaEmitida , OfertaAceptada , OfertaRechazada , OfertaExpirada , CompromisoFirmado , BecaActivada , PagoConfirmado , PagoFallido y PagoDevuelto .

- Eventos que consume: PostulacionSeleccionada.v1 (Diego) y CostoMatriculaVigente.v1 (Jesús).

Coordina con: Sebas ( Beca y su máquina de estados), Jesús (reservar, confirmar y registrar desembolsos en PresupuestoPrograma ; OTP de S1) y Diego (de la selección a la oferta).

## Sebas — Ciclo de vida de la beca y Seguimiento Académico

Sebas gestiona la beca ya otorgada: suspensión, revocación, renovación y pausa, el seguimiento académico que dispara esas decisiones, la línea de tiempo de cada beca y el reporte institucional. Trabaja más cerca de Leonel, porque los dos escriben sobre el agregado Beca dentro de S4.

| RF | Requisito | HU | Servicio |
| --- | --- | --- | --- |
| RF-21 Suspensión temporal |   | HU-21.1 Suspender una beca con motivo y | S4 |
|   |   | fecha de revisión, y notificar al becario · HU- |   |
|   |   | 21.2 Reanudar una beca suspendida · HU- |   |
|   |   | 21.3 Avisar al coordinador cuando llega la |   |
|   |   | fecha de revisión de una suspensión |   |
| RF- | Revocación | HU-22.1 Revocar una beca con motivo y | S4 |
| 22 |   | código de un solo uso · HU-22.2 Liberar el |   |
|   |   | presupuesto y el cupo de una beca |   |
|   |   | revocada |   |
| RF- | Reportes académicos | HU-24.1 Entregar el reporte académico del | S5 |
| 24 |   | periodo, con alerta de entrega tardía · HU- |   |
|   |   | 24.2 Registrar el reporte académico desde |   |
|   |   | la universidad · HU-24.3 Recibir por API el |   |
|   |   | reporte académico que envía la universidad |   |


| RF | Requisito | HU | Servicio |
| --- | --- | --- | --- |
| RF- | Condiciones de | HU-25.1 Definir la política de consecuencias | S5 |
| 25 | permanencia | y evaluar el cumplimiento (vive en S2) · HU- |   |
|   |   | 25.2 Aplicar la consecuencia por |   |
|   |   | incumplimiento e informarla al becario |   |
| RF- | Renovación | HU-26.1 Definir la ventana de renovación y | S4 |
| 26 |   | solicitar la renovación dentro de ella · HU- |   |
|   |   | 26.2 Evaluar la renovación y resolver las |   |
|   |   | becas no renovadas |   |
| RF- | Pausa o aplazamiento | HU-27.1 Solicitar y resolver una pausa de la | S4 |
| 27 |   | beca · HU-27.2 Reactivar la beca tras |   |
|   |   | verificar el estatus académico |   |
| RF- | Historial del becario | HU-28.1 Ver la línea de tiempo completa de | S4 |
| 28 |   | una beca |   |
| RF- | Reporte institucional | HU-34.1 Consultar el reporte consolidado | S6 |
| 34 |   | institucional por periodo, universidad y |   |
|   |   | carrera |   |

## Parte del DDD v3.2

- Secciones: §11b Beca, en lo que toca a suspensiones, renovaciones y pausa; §11a (solo VentanaRenovacion ); §12 Seguimiento Académico; §3b Integraciones externas (capa anticorrupción con las universidades); §15.2 Estados de la beca, §15.11 Estados de la solicitud de renovación y §15.12 Estados de la solicitud de pausa.

- Agregados: Beca (compartido con Leonel), VentanaRenovacion , ReporteAcademico , PlazoReporte y EvaluacionCumplimiento .

- Eventos que publica: BecaSuspendida , BecaReanudada , BecaRevocada , BecaInactivada , BecaRenovada , BecaConcluida , RenovacionSolicitada , RenovacionAprobada , RenovacionRechazada , ReporteAcademicoRegistrado e IncumplimientoDetectado .

- Eventos que consume: PeriodoMarcadoActual (Jesús), para iniciar el nuevo periodo de cada beca.

Coordina con: Leonel ( Beca y su máquina de estados), Jesús (liberar presupuesto al revocar, reservar al renovar, calendario académico y cliente API de cada universidad) y Diego (becarios en riesgo en el panel).


## Puntos de contacto entre bloques

Para trabajar en paralelo, hay que acordar al inicio estos once contratos. Con ellos fijos, cada uno simula con dobles de prueba lo que no es suyo y no espera a nadie. Los cuatro primeros son los que más bloquean.

| # | Contrato | Entre | RF que lo | Qué acordar |
| --- | --- | --- | --- | --- |
|   |   |   | usan |   |
| 1 | Agregado Beca y su máquina de | Leonel y | 17, 19, 20, | Estados y transiciones |
|   | estados | Sebas | 21, 22, 26, | (§15.2), qué métodos |
|   |   |   | 27, 28 | agrega cada uno y una |
|   |   |   |   | sola configuración de |
|   |   |   |   | Stateless. |
| 2 | Agregado Postulacion y sus | Daniel y | 01–05, 06– | Estados (§15.1) y qué |
|   | estados | Diego | 10, 39 | métodos son de |
|   |   |   |   | postulación y cuáles |
|   |   |   |   | de evaluación. |
| 3 | Reservas de presupuesto | Jesús | 09, 12, 15, | Reservar, confirmar y |
|   | ( IFondosPrivados , S4 y S3 hacia | con | 16, 22, 26 | liberar, con |
|   | S2) | Leonel, |   | idempotencia por |
|   |   | Sebas y |   | operationId (D-1). |
|   |   | Diego |   |   |
| 4 | Autenticación, permisos, OTP y | Jesús | 35, 37 y | Tipos de usuario, |
|   | registro de decisiones | con | toda | alcances, desafío OTP |
|   |   | todos | operación | ( ChallengeResponse ) |
|   |   |   | sensible | y formato de |
|   |   |   |   | RegistroDecision . |
| 5 | PostulacionSeleccionada.v1 , | Diego y | 09, 10, 15, | Contenido del evento |
|   | OfertaRechazada y | Leonel | 16 | y clave |
|   | OfertaExpirada |   |   | (postulacionId, |
|   |   |   |   | cicloId) . |
| 6 | Consulta de beca activa | Leonel | 01 (HU- | Respuesta síncrona y |
|   | ( IConsultaBeneficiosPersona ) | hacia | 01.2), 18 | autoritativa sobre si la |
|   |   | Daniel |   | persona tiene beca |
|   |   |   |   | activa en el periodo. |


| # | Contrato | Entre | RF que lo | Qué acordar |
| --- | --- | --- | --- | --- |
|   |   |   | usan |   |
| 7 | Envío de avisos | Daniel | 21 (HU- | Plantilla por evento y |
|   | ( RegistroEnvio ) | con | 21.3), 29, | regla contra |
|   |   | todos los | 30, 31 | duplicados; los demás |
|   |   | que |   | solo publican eventos. |
|   |   | notifican |   |   |
| 8 | HU que viven en S2: HU-07.1, HU- | Jesús | 07, 17, 25 | El dueño del RF define |
|   | 17.1 y HU-25.1 | con |   | la política y Jesús la |
|   |   | Diego, |   | agrega a Programa . |
|   |   | Leonel y |   |   |
|   |   | Sebas |   |   |
| 9 Modelos de lectura en MongoDB |   | Jesús, | 23, 33, 34, | Una colección por |
|   | (S6) | Diego, | 36, 40 | reporte y la misma |
|   |   | Leonel y |   | convención de |
|   |   | Sebas |   | proyección y nombres. |
| 10 | PeriodoMarcadoActual | Jesús | 26, 38 | Iniciar el nuevo periodo |
|   |   | hacia |   | de cada beca en |
|   |   | Sebas |   | segundo plano (HU- |
|   |   |   |   | 38.2). |
| 11 | IncumplimientoDetectado | Sebas | 25, 40 | Datos mínimos para |
|   |   | hacia |   | mostrar becarios en |
|   |   | Diego |   | riesgo en el panel. |

## Qué se puede programar ya

Los 40 RF se pueden empezar hoy (24-nov-2026): 15 sin depender de nadie y 25 simulando una pieza de otro compañero. Ninguno espera una decisión para arrancar. La tabla responde tres preguntas por separado:

- Arranque: si el RF se programa sin tocar nada de otro, o si usa una pieza de otro compañero. En el segundo caso también se empieza hoy: la pieza del otro se simula en las pruebas hasta que exista.

- Pieza de otro que se simula: cuál es y de quién.

- Punto pendiente que queda aparte: la única parte del RF que todavía no se programa, porque falta una decisión. El resto del RF no la espera.

MongoDB queda como base de lectura por decisión del equipo (24-nov-2026).


Todo el código de dominio arranca después del paso 1 (Shared Kernel, Contracts, Outbox y registro de decisiones), porque las raíces heredan de esa base. Las pantallas del frontend esperan el ADR de la C7. Cuando un RF quede terminado, cámbienlo a Listo en la columna Arranque.

Regla para lo que falta definir. Cada punto abierto (paso a Inactiva, pago al reanudar R12, desembolso aplazado, rechazo de la oferta, firma del OTP y registro de cuenta) se deja detrás de una política propia o como una transición sin configurar en la máquina de estados de Beca , con su prueba marcada como pendiente. Cuando se decida, se cambia esa pieza y no el RF entero.

| Integrante RF | Arranque | Pieza de otro | Punto |
| --- | --- | --- | --- |
|   |   | que se simula | pendiente que |
|   |   |   | queda aparte |
| Jesús RF-35 Registro de | Ya, sin depender de nadie | Ninguna | Ninguno |
| decisiones |   |   |   |
| Jesús RF-37 Usuarios y | Ya, sin depender de nadie | Ninguna | Ninguno |
| permisos |   |   |   |
| Jesús RF-11 Configuración | Ya, sin depender de nadie | Ninguna | Ninguno |
| de programas |   |   |   |
| Jesús RF-13 Donantes y | Ya, sin depender de nadie | Ninguna | Ninguno |
| aportes |   |   |   |
| Jesús | RF-14 Universidades Ya, sin depender de nadie | Ninguna | Ninguno |
| y convenios |   |   |   |
| Jesús RF-38 Calendario | Ya, sin depender de nadie | Ninguna | Ninguno |
| académico |   |   |   |
| Jesús | RF-12 Presupuesto y Ya, simulando lo de otro | La consulta a S4 | Cómo se |
| cupos |   | con la que el | calcula el |
|   |   | reconciliador | monto a |
|   |   | verifica si la | reservar (A2 |
|   |   | oferta existe | frente a HU- |
|   |   | (Leonel) | 23.1) y el plazo |
|   |   |   | de conciliación |
|   |   |   | (C5) |
| Jesús RF-33 Reporte de | Ya, simulando lo de otro | Los eventos de | Ninguno |
| impacto |   | becas y pagos |   |
|   |   | de S4 (Leonel y |   |
|   |   | Sebas) y los |   |
|   |   | reportes |   |
|   |   | académicos de |   |
|   |   | S5 (Sebas) |   |
| Daniel RF-03 Carga de | Ya, sin depender de nadie | Ninguna | Ninguno |
| documentos |   |   |   |


| Integrante RF | Arranque | Pieza de otro | Punto |
| --- | --- | --- | --- |
|   |   | que se simula | pendiente que |
|   |   |   | queda aparte |
| Daniel RF-05 Retiro | Ya, sin depender de nadie | Ninguna | Ninguno |
| voluntario |   |   |   |
| Daniel RF-01 Registro de | Ya, simulando lo de otro | La consulta de | Si una beca |
| postulantes |   | beca activa de | Inactiva cuenta |
|   |   | S4 (Leonel) | como beca |
|   |   |   | activa (DDD |
|   |   |   | v3.3) |
| Daniel RF-02 Publicación | Ya, simulando lo de otro | El programa y | Ninguno |
| de convocatorias |   | su cupo en S2 |   |
|   |   | (Jesús) |   |
| Daniel RF-04 Verificación | Ya, simulando lo de otro | Los criterios del | Ninguno |
| de elegibilidad |   | programa en S2 |   |
|   |   | (Jesús) y la |   |
|   |   | fuente |   |
|   |   | académica |   |
|   |   | externa |   |
| Daniel RF-29 | Ya, simulando lo de otro | Los eventos de | Ninguno |
| Notificaciones |   | los demás |   |
| automáticas |   | servicios |   |
|   |   | (todos) |   |
| Daniel | RF-30 Recordatorios Ya, simulando lo de otro | Las fechas de | Ninguno |
|   |   | vencimiento de |   |
|   |   | S2, S4 y S5 |   |
|   |   | (Jesús, Leonel y |   |
|   |   | Sebas) |   |
| Daniel RF-32 Mensajería | Ya, simulando lo de otro | Las becas de S4 | Confirmar |
|   |   | a las que se | SignalR como |
|   |   | asocia una | canal de tiempo |
|   |   | consulta | real |
|   |   | (Leonel) |   |
| Diego RF-06 Comité de | Ya, sin depender de nadie | Ninguna | Ninguno |
| evaluación |   |   |   |
| Diego | RF-08 Puntuaciones Ya, sin depender de nadie | Ninguna | Ninguno |
| y comentarios |   |   |   |
| Diego RF-39 Conflictos de | Ya, sin depender de nadie | Ninguna | Ninguno |
| interés |   |   |   |
| Diego RF-07 Rúbrica de | Ya, simulando lo de otro | La rúbrica del | Ninguno |
| evaluación |   | programa en S2 |   |
|   |   | (Jesús) |   |


| Integrante RF | Arranque | Pieza de otro | Punto |
| --- | --- | --- | --- |
|   |   | que se simula | pendiente que |
|   |   |   | queda aparte |
| Diego | RF-09 Consolidación Ya, simulando lo de otro | La | Ninguno |
| y ranking |   | disponibilidad |   |
|   |   | de cupo y |   |
|   |   | presupuesto de |   |
|   |   | S2 (Jesús) |   |
| Diego RF-10 Empates y | Ya, simulando lo de otro | Los eventos de | Ninguno |
| casos límite |   | oferta |   |
|   |   | rechazada y |   |
|   |   | expirada de S4 |   |
|   |   | (Leonel) |   |
| Diego | RF-31 Notificaciones Ya, simulando lo de otro | La base de | Ninguno |
| al comité |   | envío de avisos |   |
|   |   | (Daniel) |   |
| Diego RF-40 Indicadores | Ya, simulando lo de otro | Los eventos de | Confirmar |
| en tiempo real |   | los demás | SignalR como |
|   |   | servicios | canal de tiempo |
|   |   | (todos) | real |
| Leonel RF-18 Historial de | Ya, sin depender de nadie | Ninguna | Confirmar que |
| becas |   |   | el dueño es S4 |
| Leonel RF-15 Emisión de | Ya, simulando lo de otro | La reserva de | Ninguno. El |
| ofertas |   | presupuesto de | monto a |
|   |   | S2 (Jesús) y la | reservar lo |
|   |   | postulación | calcula S2: es el |
|   |   | seleccionada | pendiente de |
|   |   | de S3 (Diego) | RF-12, no de |
|   |   |   | este RF |
| Leonel RF-16 | Ya, simulando lo de otro | El costo de | El rechazo de la |
|   | Aceptación/rechazo | matrícula | oferta, HU-16.2 |
| de ofertas |   | vigente de S2, | (DDD v3.3) |
|   |   | que usa la regla |   |
|   |   | C2 (Jesús) |   |
| Leonel RF-17 Compromiso | Ya, simulando lo de otro | El compromiso | Ninguno |
| del becario |   | que exige el |   |
|   |   | programa en S2 |   |
|   |   | (Jesús) |   |
| Leonel RF-19 Calendario de | Ya, simulando lo de otro | El monto y la | El desembolso |
| desembolsos |   | modalidad de | aplazado (DDD |
|   |   | pago del | v3.3) |
|   |   | programa en S2 |   |
|   |   | (Jesús) |   |


| Integrante RF | Arranque | Pieza de otro | Punto |
| --- | --- | --- | --- |
|   |   | que se simula | pendiente que |
|   |   |   | queda aparte |
| Leonel RF-20 Registro y | Ya, simulando lo de otro | La validación | La firma del |
| confirmación de |   | del OTP de S1 | OTP, el registro |
| desembolsos |   | (Jesús) | de cuenta de |
|   |   |   | HU-20.3 y el |
|   |   |   | desembolso |
|   |   |   | aplazado (DDD |
|   |   |   | v3.3) |
| Leonel RF-23 Reporte | Ya, simulando lo de otro | Los eventos de | Ninguno. El |
| financiero |   | presupuesto de | reporte lee el |
|   |   | S2 (Jesús) | comprometido |
|   |   |   | que calcula S2 |
| Leonel RF-36 Exportación | Ya, simulando lo de otro | Los registros | Ninguno |
| para auditoría |   | de decisiones |   |
|   |   | de los demás |   |
|   |   | servicios |   |
|   |   | (todos) |   |
| Sebas RF-21 Suspensión | Ya, sin depender de nadie | Ninguna | Si al reanudar |
| temporal |   |   | se paga lo |
|   |   |   | pendiente |
|   |   |   | (R12) |
| Sebas RF-27 Pausa o | Ya, sin depender de nadie | Ninguna | El vencimiento |
| aplazamiento |   |   | a Inactiva y si |
|   |   |   | R12 aplica a la |
|   |   |   | pausa |
|   |   |   | (preguntas al |
|   |   |   | cliente) |
| Sebas RF-28 Historial del | Ya, sin depender de nadie | Ninguna | Ninguno |
| becario |   |   |   |
| Sebas RF-22 Revocación | Ya, simulando lo de otro | La validación | Los ajustes de |
|   |   | del OTP de S1 | revocación que |
|   |   | (Jesús) | traiga el DDD |
|   |   |   | v3.3 |
| Sebas RF-24 Reportes | Ya, simulando lo de otro | El calendario | El catálogo de |
| académicos |   | académico de | estados de |
|   |   | S2 (Jesús) y las | matrícula |
|   |   | becas activas |   |
|   |   | de S4 (Leonel) |   |
| Sebas RF-25 Condiciones | Ya, simulando lo de otro | La política de | Ninguno |
| de permanencia |   | consecuencias |   |
|   |   | del programa |   |
|   |   | en S2 (Jesús) |   |


| Integrante RF | Arranque | Pieza de otro | Punto |
| --- | --- | --- | --- |
|   |   | que se simula | pendiente que |
|   |   |   | queda aparte |
| Sebas RF-26 Renovación | Ya, simulando lo de otro | La reserva de | Cuándo pasa a |
|   |   | presupuesto y | Inactiva la beca |
|   |   | la configuración | no renovada |
|   |   | de renovación | (pregunta al |
|   |   | de S2 (Jesús) | cliente) |
| Sebas RF-34 Reporte | Ya, simulando lo de otro | Los eventos de | Ninguno |
| institucional |   | postulaciones |   |
|   |   | de S3 (Daniel y |   |
|   |   | Diego), los de |   |
|   |   | becas y pagos |   |
|   |   | de S4 (Leonel) |   |
|   |   | y el catálogo de |   |
|   |   | S2 (Jesús) |   |

## Orden de arranque

Los cinco programan desde la semana 1. Jesús arma la base del paso 1 y S2; Daniel y Diego empiezan S3; Leonel y Sebas empiezan S4 y S5. Como Daniel y Diego comparten `Postulacion` y Leonel y Sebas comparten `Beca`, cada pareja acuerda primero los estados de su agregado. Los puntos pendientes del DDD v3.3 los cierra el equipo en paralelo.

| Integrante Semana 1 (24 al 30-nov) |   | Semana 2 (1 al 7-dic) |
| --- | --- | --- |
| Jesús | Paso 1: Shared Kernel con la versión | S2 con RF-11, 13, 14 y 38; HU-12.1 y las |
|   | de raíz decidida en C3, Contracts, | pruebas de concurrencia de |
|   | Outbox e Inbox, registro de | PresupuestoPrograma . |
|   | decisiones (RF-35) y la prueba de |   |
|   | Keycloak (RF-37). |   |
| Daniel | Acordar con Diego los estados de | S3 con RF-02, 03, 04 y 05 contra la |
|   | Postulacion ; prueba de Firebase | base del paso 1; la base de envío de |
|   | Storage. | RF-29. |
| Diego | Acordar con Daniel los estados de | RF-07 y HU-09.1; HU-10.1. |
|   | Postulacion ; dominio de RF-06, 08 |   |
|   | y 39 con pruebas unitarias. |   |
| Leonel | Acordar con Sebas los estados de | RF-15 y RF-20 con la reserva y el OTP |
|   | Beca . Programar RF-16 (aceptación, | simulados; RF-17; RF-23. |
|   | BeneficiosPersona y regla C2), RF- |   |
|   | 18 y RF-19. |   |


| Integrante Semana 1 (24 al 30-nov) |   | Semana 2 (1 al 7-dic) |
| --- | --- | --- |
| Sebas | Acordar con Leonel los estados de | RF-22, RF-25 y RF-26 con el OTP y la |
|   | Beca . Programar RF-24 en S5, y RF- | reserva simulados; RF-28. |
|   | 21 y RF-27 en S4. |   |

El código de dominio de la semana 1 de Diego puede empezar el mismo día en que Jesús publique las clases base del Shared Kernel; no necesita el resto del paso 1.

## Supuestos y decisiones a confirmar

- Nombres por bloque: se asignaron sin conocer las preferencias de cada uno. Si dos personas intercambian bloques completos, la división sigue valiendo igual.

- RF-18 en S4, como quedó en la arquitectura v1.3. Si el equipo lo pasa a S6, Leonel lo sigue llevando como modelo de lectura.

- HU-07.1, HU-17.1 y HU-25.1 viven en S2, aunque sus RF son de Diego, Leonel y Sebas. La propuesta es que cada dueño defina la política y Jesús la agregue a Programa . La alternativa es que Jesús las implemente completas.

- Base de S6: Daniel monta la estructura del servicio y el envío de avisos. Los reportes y el panel son proyecciones independientes de quien los construye.

- DDD §2.2: este reparto difiere del sugerido ahí en cuatro RF (33, 34, 36 y 40). Si el equipo lo adopta, hay que actualizar esa tabla para que el DDD y la arquitectura digan lo mismo.
