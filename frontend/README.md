# Frontend (React + Next.js)

Obligatorio según la lámina del profesor. Todavía sin código.

Antes de crearlo falta un ADR del equipo (pendiente C7 en [`docs/arquitectura/pendientes-v1.3.md`](../docs/arquitectura/pendientes-v1.3.md)): renderizado en servidor o SPA, dónde vive el token de Keycloak y si Next.js actúa como BFF frente al gateway. Reglas ya fijadas por la arquitectura v1.3:

- Entra siempre por el API Gateway (YARP); nunca llama rutas privadas entre servicios.
- No contiene reglas de negocio ni financieras.
- Tras reconectar SignalR, vuelve a consultar el estado real al servidor.
