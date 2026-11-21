# Anexos técnicos — ASCENSO Arquitectura v1.3

**Generados directamente desde las matrices CSV; los CSV son fuente de trazabilidad.** DDD v3.2 preservado.

## A. Los 40 requerimientos funcionales — propietarios y colaboraciones

| RF | Requisito | Prioridad | Contexto_arquitectonico | Colaboraciones | Estado | Base |
| --- | --- | --- | --- | --- | --- | --- |
| RF-01 | Registro de postulantes | Debe tener | S3 | S1 identidad | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-02 | Publicación de convocatorias | Debe tener | S3 | S2 programa/ciclo | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-03 | Carga de documentos | Debe tener | S3 | Firebase Storage | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-04 | Verificación de elegibilidad | Debe tener | S3 | S2 criterios; IFuenteAcademicaS3 con adaptador académico propio (sin dependencia S5) | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-05 | Retiro voluntario | Debería tener | S3 | RegistroDecision | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-06 | Comité de evaluación | Debe tener | S3 | S1 permisos | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-07 | Rúbrica de evaluación | Debe tener | S3 | S2 rúbrica; S3 copia aplicada | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-08 | Puntuaciones y comentarios | Debe tener | S3 | S2 rubrica aplicada | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-09 | Consolidación y ranking | Debe tener | S3 | S3 ranking a pedido; S2 disponibilidad viva | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-10 | Empates y casos límite | Debe tener | S3 | S2 cupos | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-11 | Configuración de programas | Debe tener | S2 | S1 autenticación | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-12 | Presupuesto y cupos | Debe tener | S2 | S4 reserva/renovación | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-13 | Donantes y aportes | Debe tener | S2 | S6 reportería | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-14 | Universidades y convenios | Debe tener | S2 | S5 fuentes universidad | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-15 | Emisión de ofertas | Debe tener | S4 | S2 reserva | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-16 | Aceptación/rechazo de ofertas | Debe tener | S4 | S2 confirmación; S1 identidad | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-17 | Compromiso del becario | Debe tener | S4 | Firebase documentos | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-18 | Historial de becas | Debe tener | S4 | S6 proyección historial_becas por programa y periodo; S3 solo consume por IConsultaBeneficiosPersona (HU-01.2) | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-19 | Calendario de desembolsos | Debe tener | S4 | S2 presupuesto | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-20 | Registro y confirmación de desembolsos | Debe tener | S4 | S1 OTP; S2 fondos | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-21 | Suspensión temporal | Debe tener | S4 | S6 avisos | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-22 | Revocación | Debe tener | S4 | S2 liberación | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-23 | Reporte financiero por programa | Debería tener | S6 | S2 aportes y fondos | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-24 | Reportes académicos | Debe tener | S5 | S2 calendario; S1 enlace | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-25 | Condiciones de permanencia | Debe tener | S5 | S4 acciones de beca | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-26 | Renovación | Debe tener | S4 | S2 reservas | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-27 | Pausa o aplazamiento | Debería tener | S4 | S5 validación académica | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-28 | Historial del becario | Debe tener | S4 | S6 proyección para línea temporal, S4 eventos autoritativos | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-29 | Notificaciones automáticas | Debería tener | S6 | Todos eventos | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-30 | Recordatorios | Debería tener | S6 | S2/S4/S5 fechas | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-31 | Notificaciones al comité | Podría tener | S6 | S3 comité | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-32 | Mensajería | Debería tener | S6 | SignalR/servicio notificaciones | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-33 | Reporte de impacto | Debe tener | S6 | S6 ConsentimientoDonante; proyección anonimizada; S4 eventos de becas | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-34 | Reporte institucional | Debe tener | S6 | S1 autorización | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-35 | Registro de decisiones | Debe tener | Transversal (cada S + S6 lectura) | Cada servicio + Outbox | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-36 | Exportación para auditoría | Podría tener | S6 | Datos S6 anonimizados | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-37 | Usuarios y permisos | Debe tener | S1 | Todos validan permisos | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-38 | Calendario académico | Debe tener | S2 | S4 reacciona al cambio | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-39 | Conflictos de interés | Debería tener | S3 | S3 invalidación de asignación | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |
| RF-40 | Indicadores en tiempo real | Podría tener | S6 | Eventos de todos los servicios | Asignación propuesta — sin implementación | PRD + HU Plan B final + DDD v3.2 |

## B. Las 73 historias vigentes — matriz única

| HU | Título oficial | Página índice HU | RF padre | Servicio propietario | Colabora | Prioridad |
| --- | --- | --- | --- | --- | --- | --- |
| HU-01.1 | Crear mi perfil de postulante y postular a una convocatoria abierta | 14 | RF-01 | S3 | S1 identidad | Debe tener |
| HU-01.2 | Impedir postular con una beca activa en el mismo periodo | 20 | RF-01 | S3 | S4 consulta síncrona autoritativa de beca activa; eventos como cache no autorizativo | Debe tener |
| HU-02.1 | Configurar el ciclo de postulación: fechas, cupo, criterios y documentos | 25 | RF-02 | S3 | S2 programa/ciclo | Debe tener |
| HU-02.2 | Publicar la convocatoria y aceptar postulaciones solo dentro de su ventana | 30 | RF-02 | S3 | S2 programa/ciclo | Debe tener |
| HU-03.1 | Cargar mis documentos de soporte y ver los que faltan | 35 | RF-03 | S3 | Firebase Storage | Debe tener |
| HU-03.2 | Rechazar un documento con motivo y permitir que se vuelva a subir | 38 | RF-03 | S3 | Firebase Storage | Debe tener |
| HU-04.1 | Verificar automáticamente la elegibilidad básica y mostrar el motivo | 42 | RF-04 | S3 | S2 criterios; IFuenteAcademicaS3 con adaptador propio, sin llamada a S5 | Debe tener |
| HU-04.2 | Anular un resultado de no elegible en un caso límite | 46 | RF-04 | S3 | S2 criterios; IFuenteAcademicaS3 con adaptador propio, sin llamada a S5 | Debe tener |
| HU-05.1 | Retirar mi postulación antes de que empiece la evaluación | 49 | RF-05 | S3 | RegistroDecision | Debería tener |
| HU-06.1 | Conformar el comité de un programa | 52 | RF-06 | S3 | S1 permisos | Debe tener |
| HU-06.2 | Asignar los evaluadores de cada postulación | 55 | RF-06 | S3 | S1 permisos | Debe tener |
| HU-07.1 | Definir la rúbrica del programa con pesos que sumen 100 % | 59 | RF-07 | S2 | S3/S4/S5 consumen política, según caso | Debe tener |
| HU-07.2 | Bloquear la rúbrica al comenzar la evaluación del ciclo | 62 | RF-07 | S3 | S2 rúbrica; S3 copia aplicada | Debe tener |
| HU-08.1 | Puntuar, comentar y enviar mi evaluación | 64 | RF-08 | S3 | S2 rubrica aplicada | Debe tener |
| HU-08.2 | Ocultar las puntuaciones de otros evaluadores hasta el cierre | 67 | RF-08 | S3 | S2 rubrica aplicada | Debe tener |
| HU-09.1 | Calcular la puntuación consolidada y marcar las diferencias grandes | 70 | RF-09 | S3 | S2 disponibilidad | Debe tener |
| HU-09.2 | Generar el ranking respetando cupo y presupuesto | 73 | RF-09 | S3 | S2 disponibilidad autoritativa; genera ranking en S3 | Debe tener |
| HU-10.1 | Resolver un empate o un caso límite con justificación | 77 | RF-10 | S3 | S2 cupos | Debe tener |
| HU-10.2 | Dejar en lista de espera a los elegibles no otorgados | 81 | RF-10 | S3 | S3 lista de espera y sugerencia; S6 solo avisa | Debe tener |
| HU-11.1 | Crear un programa de becas con su monto por periodo | 85 | RF-11 | S2 | S1 autenticación | Debe tener |
| HU-11.2 | Configurar los donantes, la elegibilidad y la renovación del programa | 89 | RF-11 | S2 | S1 autenticación | Debe tener |
| HU-12.1 | Consultar el presupuesto y el cupo disponibles y verificarlos antes de otorgar | 94 | RF-12 | S2 | S4 reserva/renovación | Debe tener |
| HU-12.2 | Contar las renovaciones pendientes como no disponibles | 99 | RF-12 | S2 | S4 reserva/renovación | Debe tener |
| HU-13.1 | Registrar un donante | 102 | RF-13 | S2 | S6 reportería | Debe tener |
| HU-13.2 | Registrar los aportes de un donante y consultar su historial | 104 | RF-13 | S2 | S6 reportería | Debe tener |
| HU-14.1 | Registrar una universidad aliada con su convenio y sus carreras elegibles | 107 | RF-14 | S2 | S5 fuentes universidad | Debe tener |
| HU-14.2 | Registrar el costo de matrícula por periodo sin perder los anteriores | 111 | RF-14 | S2 | S5 fuentes universidad | Debe tener |
| HU-14.3 | Registrar la fecha de inicio de inscripciones de cada universidad por periodo | 113 | RF-14 | S2 | S5 fuentes universidad | Debe tener |
| HU-15.1 | Generar la oferta formal de beca y comunicarla con su fecha límite | 116 | RF-15 | S4 | S3 PostulacionSeleccionada.v1; S2 calcula reserva por programa/ciclo | Debe tener |
| HU-15.2 | Expirar una oferta sin respuesta y considerar al siguiente | 121 | RF-15 | S4 | S2 reserva | Debe tener |
| HU-16.1 | Aceptar la oferta y convertirme en becario | 124 | RF-16 | S4 | S2 confirmación; S1 identidad | Debe tener |
| HU-16.2 | Rechazar la oferta | 129 | RF-16 | S4 | S2 confirmación; S1 identidad | Debe tener |
| HU-17.1 | Configurar el compromiso que exige un programa | 132 | RF-17 | S2 | S3/S4/S5 consumen política, según caso | Debe tener |
| HU-17.2 | Firmar mi compromiso de becario | 135 | RF-17 | S4 | Firebase documentos | Debe tener |
| HU-18.1 | Consultar el historial de becas de una persona por programa o periodo | 138 | RF-18 | S4 | S6 proyección historial_becas por programa y periodo; S3 solo consume (HU-01.2) | Debe tener |
| HU-19.1 | Generar el calendario de desembolsos de un becario sin superar lo aprobado | 142 | RF-19 | S4 | S2 presupuesto | Debe tener |
| HU-20.1 | Registrar y confirmar un desembolso con un código de un solo uso | 147 | RF-20 | S4 | S1 OTP; S2 fondos | Debe tener |
| HU-20.2 | Resolver un desembolso fallido o devuelto | 152 | RF-20 | S4 | S1 OTP; S2 fondos | Debe tener |
| HU-20.3 | Registrar y modificar los datos de pago del becario con un código de un solo uso | 156 | RF-20 | S4 | S1 OTP; S2 fondos | Debe tener |
| HU-21.1 | Suspender una beca con motivo y fecha de revisión, y notificar al becario | 160 | RF-21 | S4 | S6 avisos | Debe tener |
| HU-21.2 | Reanudar una beca suspendida | 164 | RF-21 | S4 | S6 avisos | Debe tener |
| HU-21.3 | Avisar al coordinador cuando llega la fecha de revisión de una suspensión | 167 | RF-21 | S4 | S6 avisos | Debe tener |
| HU-22.1 | Revocar una beca con motivo y código de un solo uso | 170 | RF-22 | S4 | S2 liberación | Debe tener |
| HU-22.2 | Liberar el presupuesto y el cupo de una beca revocada | 174 | RF-22 | S4 | S2 liberación | Debe tener |
| HU-23.1 | Consultar y exportar el estado financiero de un programa | 177 | RF-23 | S6 | S2 aportes y fondos | Debería tener |
| HU-24.1 | Entregar el reporte académico del periodo, con alerta de entrega tardía | 181 | RF-24 | S5 | S2 calendario; S1 enlace | Debe tener |
| HU-24.2 | Registrar el reporte académico desde la universidad | 184 | RF-24 | S5 | S2 calendario; S1 enlace | Debe tener |
| HU-24.3 | Recibir por API el reporte académico que envía la universidad | 187 | RF-24 | S5 | S2 calendario; S1 enlace | Debe tener |
| HU-25.1 | Definir la política de consecuencias y evaluar el cumplimiento | 190 | RF-25 | S2 | S3/S4/S5 consumen política, según caso | Debe tener |
| HU-25.2 | Aplicar la consecuencia por incumplimiento e informarla al becario | 194 | RF-25 | S4 | S5 informa incumplimiento; S1 OTP | Debe tener |
| HU-26.1 | Definir la ventana de renovación y solicitar la renovación dentro de ella | 198 | RF-26 | S4 | S2 reservas | Debe tener |
| HU-26.2 | Evaluar la renovación y resolver las becas no renovadas | 202 | RF-26 | S4 | S2 reservas | Debe tener |
| HU-27.1 | Solicitar y resolver una pausa de la beca | 208 | RF-27 | S4 | S5 validación académica | Debería tener |
| HU-27.2 | Reactivar la beca tras verificar el estatus académico | 211 | RF-27 | S4 | S5 validación académica | Debería tener |
| HU-28.1 | Ver la línea de tiempo completa de una beca | 214 | RF-28 | S4 | RegistroDecision/consulta | Debe tener |
| HU-29.1 | Recibir los hitos de mi postulación o beca por correo y por los canales del programa | 217 | RF-29 | S6 | Todos eventos | Debería tener |
| HU-29.2 | Enviar notificaciones a todo un ciclo sin ralentizar la plataforma | 222 | RF-29 | S6 | Todos eventos | Debería tener |
| HU-30.1 | Configurar y recibir recordatorios antes de un vencimiento | 225 | RF-30 | S6 | S2/S4/S5 fechas | Debería tener |
| HU-31.1 | Avisar al comité de nuevas asignaciones y del cierre de la evaluación | 230 | RF-31 | S6 | S3 comité | Podría tener |
| HU-32.1 | Enviar una consulta sobre mi postulación o beca | 235 | RF-32 | S6 | SignalR/servicio notificaciones | Debería tener |
| HU-32.2 | Responder y dar seguimiento a una consulta | 238 | RF-32 | S6 | SignalR/servicio notificaciones | Debería tener |
| HU-33.1 | Generar el reporte de impacto de un donante con datos agregados o anonimizados | 241 | RF-33 | S6 | S6 ConsentimientoDonante; proyección anonimizada; S4 eventos de becas | Debe tener |
| HU-33.2 | Registrar el consentimiento para ser presentado a mi donante | 245 | RF-33 | S6 | S6 consentimiento y proyección anonimizada; no S4 consentimiento | Debe tener |
| HU-34.1 | Consultar el reporte consolidado institucional por periodo, universidad y carrera | 247 | RF-34 | S6 | S1 autorización | Debe tener |
| HU-35.1 | Registrar cada decisión con quién, cuándo y por qué, sin edición posterior | 252 | RF-35 | Transversal (cada S + S6 lectura) | Cada servicio + Outbox | Debe tener |
| HU-35.2 | Consultar el registro de decisiones | 256 | RF-35 | Transversal (cada S + S6 lectura) | Cada servicio + Outbox | Debe tener |
| HU-36.1 | Exportar el uso de los fondos y el registro de decisiones para una auditoría | 258 | RF-36 | S6 | Datos S6 anonimizados | Podría tener |
| HU-37.1 | Crear usuarios internos y tipos de usuario con acceso limitado a su función | 262 | RF-37 | S1 | Todos validan permisos | Debe tener |
| HU-37.2 | Iniciar sesión con verificación en dos pasos | 266 | RF-37 | S1 | Todos validan permisos | Debe tener |
| HU-38.1 | Registrar y consultar los periodos académicos | 270 | RF-38 | S2 | S4 reacciona al cambio | Debe tener |
| HU-38.2 | Marcar el periodo actual sin interrumpir renovaciones | 274 | RF-38 | S2 | S4 reacciona al cambio | Debe tener |
| HU-39.1 | Declarar un conflicto de interés y restringir mi participación en ese caso | 278 | RF-39 | S3 | S3 invalidación de asignación | Debería tener |
| HU-40.1 | Ver los indicadores clave actualizados sin recargar la página | 282 | RF-40 | S6 | Eventos de todos los servicios | Podría tener |

## C. Propiedad de las 25 raíces de agregado

| Servicio | Raíz de agregado | Contrato de repositorio | Persistencia escritura | DDDV3.2 |
| --- | --- | --- | --- | --- |
| S1 | Usuario | IUsuarioRepository | PostgreSQL / schema s1 | §6.1 / §18.1 |
| S1 | TipoUsuario | ITipoUsuarioRepository | PostgreSQL / schema s1 | §6.1 / §18.1 |
| S1 | ClienteApiUniversidad | IClienteApiUniversidadRepository | PostgreSQL / schema s1 | §6.1 / §18.1 |
| S2 | CalendarioAcademico | ICalendarioAcademicoRepository | PostgreSQL / schema s2 | §6.1 / §18.1 |
| S2 | UniversidadAliada | IUniversidadAliadaRepository | PostgreSQL / schema s2 | §6.1 / §18.1 |
| S2 | Programa | IProgramaRepository | PostgreSQL / schema s2 | §6.1 / §18.1 |
| S2 | Donante | IDonanteRepository | PostgreSQL / schema s2 | §6.1 / §18.1 |
| S2 | PresupuestoPrograma | IPresupuestoProgramaRepository | PostgreSQL / schema s2 | §6.1 / §18.1 |
| S3 | CicloPostulacion | ICicloPostulacionRepository | PostgreSQL / schema s3 | §6.1 / §18.1 |
| S3 | Postulante | IPostulanteRepository | PostgreSQL / schema s3 | §6.1 / §18.1 |
| S3 | Postulacion | IPostulacionRepository | PostgreSQL / schema s3 | §6.1 / §18.1 |
| S3 | Comite | IComiteRepository | PostgreSQL / schema s3 | §6.1 / §18.1 |
| S3 | Evaluacion | IEvaluacionRepository | PostgreSQL / schema s3 | §6.1 / §18.1 |
| S3 | ConflictoInteres | IConflictoInteresRepository | PostgreSQL / schema s3 | §6.1 / §18.1 |
| S4 | Oferta | IOfertaRepository | PostgreSQL / schema s4 | §6.1 / §18.1 |
| S4 | Beca | IBecaRepository | PostgreSQL / schema s4 | §6.1 / §18.1 |
| S4 | BeneficiosPersona | IBeneficiosPersonaRepository | PostgreSQL / schema s4 | §6.1 / §18.1 |
| S4 | DatosPago | IDatosPagoRepository | PostgreSQL / schema s4 | §6.1 / §18.1 |
| S4 | VentanaRenovacion | IVentanaRenovacionRepository | PostgreSQL / schema s4 | §6.1 / §18.1 |
| S5 | ReporteAcademico | IReporteAcademicoRepository | PostgreSQL / schema s5 | §6.1 / §18.1 |
| S5 | PlazoReporte | IPlazoReporteRepository | PostgreSQL / schema s5 | §6.1 / §18.1 |
| S5 | EvaluacionCumplimiento | IEvaluacionCumplimientoRepository | PostgreSQL / schema s5 | §6.1 / §18.1 |
| S6 | Consulta | IConsultaRepository | PostgreSQL / schema s6 | §6.1 / §18.1 |
| S6 | RegistroEnvio | IRegistroEnvioRepository | PostgreSQL / schema s6 | §6.1 / §18.1 |
| S6 | ConsentimientoDonante | IConsentimientoDonanteRepository | PostgreSQL / schema s6 | §6.1 / §18.1 |

## D. Mapeo de los 34 eventos de dominio

| Evento del DDD | Servicio de origen | Interesados | Visibilidad propuesta | Fuente |
| --- | --- | --- | --- | --- |
| PeriodoAbierto | S2 | S6 | integracion | DDD_V3.2.md §16.2 |
| ProgramaCreado | S2 | S2;S3;S6 | interno + integracion resumida | DDD_V3.2.md §16.2 |
| PostulacionRecibida | S3 | S6 | integracion | DDD_V3.2.md §16.2 |
| DocumentosCompletados | S3 | S3 | interno | DDD_V3.2.md §16.2 |
| PostulacionVerificada | S3 | S6 | integracion | DDD_V3.2.md §16.2 |
| CicloPublicado | S3 | S2;S6 | integracion | DDD_V3.2.md §16.2 |
| PeriodoEvaluacionAbierto | S3 | S2;S6 | integracion | DDD_V3.2.md §16.2 |
| ConflictoDeclarado | S3 | S3;S6 | interno + integracion resumida | DDD_V3.2.md §16.2 |
| EvaluacionEnviada | S3 | S3 | interno | DDD_V3.2.md §16.2 |
| PostulacionConsolidada | S3 | S6 | integracion | DDD_V3.2.md §16.2 |
| ListaEsperaConfirmada | S3 | S6 | integracion | DDD_V3.2.md §16.2 |
| OfertaEmitida | S4 | S2;S3;S6 | integracion | DDD_V3.2.md §16.2 |
| OfertaAceptada | S4 | S2;S3;S6 | integracion | DDD_V3.2.md §16.2 |
| OfertaRechazada | S4 | S2;S3;S6 | integracion | DDD_V3.2.md §16.2 |
| OfertaExpirada | S4 | S2;S3;S6 | integracion | DDD_V3.2.md §16.2 |
| CupoLiberado | S2 | S3;S6 | integracion | DDD_V3.2.md §16.2 |
| AporteRegistrado | S2 | S2;S6 | interno + integracion | DDD_V3.2.md §16.2 |
| CompromisoFirmado | S4 | S6 | integracion | DDD_V3.2.md §16.2 |
| BecaActivada | S4 | S6 | integracion | DDD_V3.2.md §16.2 |
| BecaSuspendida | S4 | S6 | integracion | DDD_V3.2.md §16.2 |
| BecaReanudada | S4 | S6 | integracion | DDD_V3.2.md §16.2 |
| BecaInactivada | S4 | S6 | integracion | DDD_V3.2.md §16.2 |
| BecaRenovada | S4 | S2;S6 | integracion | DDD_V3.2.md §16.2 |
| BecaConcluida | S4 | S2;S6 | integracion | DDD_V3.2.md §16.2 |
| BecaRevocada | S4 | S2;S6 | integracion | DDD_V3.2.md §16.2 |
| RenovacionSolicitada | S4 | S6 | integracion | DDD_V3.2.md §16.2 |
| RenovacionAprobada | S4 | S2;S6 | integracion | DDD_V3.2.md §16.2 |
| RenovacionRechazada | S4 | S2;S6 | integracion | DDD_V3.2.md §16.2 |
| PagoConfirmado | S4 | S2;S6 | integracion | DDD_V3.2.md §16.2 |
| PagoFallido | S4 | S2;S6 | integracion | DDD_V3.2.md §16.2 |
| PagoDevuelto | S4 | S2;S6 | integracion | DDD_V3.2.md §16.2 |
| ReporteAcademicoRegistrado | S5 | S5;S6 | interno + integracion | DDD_V3.2.md §16.2 |
| IncumplimientoDetectado | S5 | S4;S6 | integracion | DDD_V3.2.md §16.2 |
| PeriodoMarcadoActual | S2 | S4;S6 | integracion | DDD_V3.2.md §16.2 |
