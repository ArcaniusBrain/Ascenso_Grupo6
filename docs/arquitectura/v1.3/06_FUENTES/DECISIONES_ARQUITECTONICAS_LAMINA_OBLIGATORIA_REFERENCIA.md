# Lámina «Decisiones Arquitectónicas» — transcripción comprobada contra la imagen

La lámina original del profesor está en este mismo directorio: `Arquitectura_Obligatoria.jpeg` (incorporada en la v1.3). Esta transcripción se comprobó contra la imagen el 20-nov-2026; ante cualquier diferencia, manda la imagen.

## Obligatorias

- Arquitectura escalable en capas, de microservicios conectados por APIs.
- Base de datos en Firebase con PostgreSQL + Entity Framework Core (ORM).
- Frontend React + Next.js.
- Backend .NET Core (C#).
- Usar máquina de estados.
- Enfocado en auditoría.
- Ingeniería de Datos y Migración (ETL).

## Opcionales

- Identidad y Accesos: Keycloak o Active Directory o LDAP.
- Tareas en segundo plano: Rabbit vs. MassTransit.
- Procesamiento asíncrono y colas de trabajo (*Background Workers*).
- Tareas programadas («CrumJob» en la lámina): Hangfire.
- Mensajería y bus de datos: MassTransit + RabbitMQ.
- ¿Cómo montar las APIs backend?
- Posible data warehouse para los reportes.
- Uso de índices.

**Nota:** OpenSpec no aparece en la lámina; el kit lo atribuye a la clase del 14-sep-2026. SignalR y MongoDB tampoco aparecen: son elecciones del equipo.
