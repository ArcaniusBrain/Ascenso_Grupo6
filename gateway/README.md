# API Gateway (YARP)

Punto de entrada del navegador (arquitectura v1.3, §2 y §3.1). Todavía sin código.

- Enruta por prefijo hacia S1–S6, valida el token de Keycloak y deja pasar WebSockets para SignalR.
- **No** expone las rutas privadas entre servicios (`openapi-interno.yaml`); esas se llaman directo con OAuth2 client_credentials.
- No contiene reglas de negocio. Cada servicio vuelve a validar el token y aplica su propia autorización.
