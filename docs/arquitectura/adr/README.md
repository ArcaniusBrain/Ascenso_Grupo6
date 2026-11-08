# Decisiones arquitectónicas (ADR)

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf).

Estados: **Obligatoria** (la fija el profesor o el cliente), **Decidida** (el equipo ya la tomó), **Propuesta** (para aprobar o ajustar), **Pendiente** (depende del cliente o de una prueba técnica).

Cómo cambiar una decisión: se abre una propuesta en `openspec/changes/` y un pull request que edita el ADR. No se edita un ADR aprobado sin pull request.


## Estructura: estilo, servicios, capas y datos

| ADR | Decisión | Estado |
|---|---|---|
| [ADR-01](ADR-01-estilo-arquitectonico.md) | Estilo arquitectónico | Obligatoria |
| [ADR-02](ADR-02-particion-en-seis-microservicios-y-api-gateway.md) | Partición en seis microservicios y API Gateway | Propuesta |
| [ADR-03](ADR-03-programas-y-becas-separados-reserva-sincrona-con.md) | Programas y Becas separados: reserva síncrona con compensación | Decidida / Propuesta |
| [ADR-04](ADR-04-estructura-interna-de-cada-servicio-clean-architecture-ddd.md) | Estructura interna de cada servicio (Clean Architecture + DDD) | Propuesta |
| [ADR-05](ADR-05-persistencia-transaccional-postgresql-ef-core.md) | Persistencia transaccional: PostgreSQL + EF Core | Obligatoria / Propuesta |
| [ADR-06](ADR-06-archivos-firebase-storage-solo-para-binarios.md) | Archivos: Firebase Storage solo para binarios | Obligatoria |
| [ADR-07](ADR-07-cqrs-base-de-lectura-en-postgresql-alimentada-por-eventos.md) | CQRS: base de lectura en PostgreSQL, alimentada por eventos | Decidida / Propuesta |
| [ADR-08](ADR-08-maquinas-de-estado-stateless-en-beca-y-postulacion.md) | Máquinas de estado: Stateless en Beca y Postulación | Propuesta |

## Seguridad y trazabilidad

| ADR | Decisión | Estado |
|---|---|---|
| [ADR-09](ADR-09-identidad-y-verificacion-en-dos-pasos-keycloak.md) | Identidad y verificación en dos pasos: Keycloak | Propuesta / Pendiente |
| [ADR-10](ADR-10-codigo-de-un-solo-uso-otp-para-operaciones-sensibles.md) | Código de un solo uso (OTP) para operaciones sensibles | Propuesta |
| [ADR-11](ADR-11-autorizacion-fina-por-recurso-y-por-datos.md) | Autorización fina por recurso y por datos | Propuesta |
| [ADR-12](ADR-12-trazabilidad-registrodecision-inmutable.md) | Trazabilidad: RegistroDecision inmutable | Obligatoria / Propuesta |

## Comunicación, procesos y consistencia

| ADR | Decisión | Estado |
|---|---|---|
| [ADR-13](ADR-13-eventos-y-mensajeria-masstransit-8-rabbitmq-con-outbox.md) | Eventos y mensajería: MassTransit 8 + RabbitMQ con Outbox | Propuesta |
| [ADR-14](ADR-14-tareas-programadas-hangfire.md) | Tareas programadas: Hangfire | Propuesta |
| [ADR-15](ADR-15-tiempo-real-signalr-como-unico-canal-de-push.md) | Tiempo real: SignalR como único canal de push | Propuesta |
| [ADR-16](ADR-16-concurrencia-optimista-en-todas-las-raices-de-agregado.md) | Concurrencia optimista en todas las raíces de agregado | Propuesta |
| [ADR-17](ADR-17-reloj-unico-y-fechas-limite-en-utc-4.md) | Reloj único y fechas límite en UTC-4 | Propuesta |

## Datos, integración y calidad

| ADR | Decisión | Estado |
|---|---|---|
| [ADR-18](ADR-18-proteccion-de-datos-personales.md) | Protección de datos personales | Propuesta |
| [ADR-19](ADR-19-integracion-con-universidades-capa-anticorrupcion.md) | Integración con universidades (capa anticorrupción) | Propuesta |
| [ADR-20](ADR-20-notificaciones-por-canal-correo-y-sms.md) | Notificaciones por canal (correo y SMS) | Propuesta |
| [ADR-21](ADR-21-migracion-de-datos-historicos-etl.md) | Migración de datos históricos (ETL) | Obligatoria / Propuesta |
| [ADR-22](ADR-22-indices-de-base-de-datos.md) | Índices de base de datos | Propuesta |
| [ADR-23](ADR-23-calidad-y-gobierno-openspec-pruebas-y-ci.md) | Calidad y gobierno: OpenSpec, pruebas y CI | Obligatoria / Propuesta |
| [ADR-24](ADR-24-versiones-y-licencias.md) | Versiones y licencias | Propuesta |
