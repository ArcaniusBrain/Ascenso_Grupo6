# Criterios de la lámina «Decisiones Arquitectónicas» referidos en la revisión

**Procedencia:** `ASCENSO_Resumen_Hallazgos_Arquitectura_v1.0.md`, hallazgo A2 (16-nov-2026) y `ASCENSO_Propuesta_Arquitectura_v0.1 (1).pdf` ADR-01, ADR-05, ADR-06, ADR-21, ADR-23.

La lámina original está en `Arquitectura_Obligatoria.jpeg` desde la v1.3 y su transcripción completa en `DECISIONES_ARQUITECTONICAS_LAMINA_OBLIGATORIA_REFERENCIA.md`. Los siguientes puntos son **consigna obligatoria de esa lámina**, salvo el último:

- Arquitectura escalable en capas y **microservicios conectados por APIs**.
- Frontend React + Next.js, backend .NET/C#, persistencia PostgreSQL con Entity Framework Core, Firebase para los archivos de la Fundación.
- Máquinas de estados; enfoque de auditoría; ingeniería de datos y migración ETL.
- OpenSpec es obligatorio según la clase del 14-sep-2026 mencionada en el informe.

RabbitMQ, MassTransit, Hangfire y Keycloak figuran en la lámina como **opcionales**; SignalR y MongoDB no figuran y son elecciones del equipo.
