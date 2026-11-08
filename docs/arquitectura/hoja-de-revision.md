# Hoja de revisión

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice en [`README.md`](README.md).

Marquen en cada decisión: A = aprobada, C = aprobada con cambios, R = rechazada. Si una decisión se rechaza, anoten el motivo concreto en Observaciones, para corregirla con precisión en la siguiente versión.

| ADR | Decisión | Equipo | Profesor | Observaciones |
|---|---|---|---|---|
| ADR-01 | Estilo arquitectónico | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-02 | Partición en seis microservicios y API Gateway | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-03 | Programas y Becas separados: reserva síncrona con compensación | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-04 | Estructura interna de cada servicio (Clean Architecture + DDD) | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-05 | Persistencia transaccional: PostgreSQL + EF Core | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-06 | Archivos: Firebase Storage solo para binarios | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-07 | CQRS: base de lectura en PostgreSQL, alimentada por eventos | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-08 | Máquinas de estado: Stateless en Beca y Postulación | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-09 | Identidad y verificación en dos pasos: Keycloak | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-10 | Código de un solo uso (OTP) para operaciones sensibles | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-11 | Autorización fina por recurso y por datos | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-12 | Trazabilidad: RegistroDecision inmutable | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-13 | Eventos y mensajería: MassTransit 8 + RabbitMQ con Outbox | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-14 | Tareas programadas: Hangfire | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-15 | Tiempo real: SignalR como único canal de push | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-16 | Concurrencia optimista en todas las raíces de agregado | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-17 | Reloj único y fechas límite en UTC-4 | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-18 | Protección de datos personales | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-19 | Integración con universidades (capa anticorrupción) | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-20 | Notificaciones por canal (correo y SMS) | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-21 | Migración de datos históricos (ETL) | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-22 | Índices de base de datos | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-23 | Calidad y gobierno: OpenSpec, pruebas y CI | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |
| ADR-24 | Versiones y licencias | A ☐ C ☐ R ☐ | A ☐ C ☐ R ☐ |  |

Revisado por el equipo (fecha y nombres): Revisado por el profesor (fecha):

______________________________ ______________________________
