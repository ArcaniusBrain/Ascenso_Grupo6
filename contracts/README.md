# Contratos

Según la arquitectura v1.3 (§12), aquí vivirán los contratos que usa el código:

- `openapi.yaml`: API pública (frontend → gateway → servicios).
- `openapi-interno.yaml`: rutas privadas entre servicios (OAuth2 client_credentials, no expuestas por YARP).
- Esquemas de eventos de integración y matriz de permisos.

**Mientras no haya código, la fuente vigente es [`docs/arquitectura/v1.3/03_CONTRATOS/`](../docs/arquitectura/v1.3/03_CONTRATOS/).** El primer cambio de código los moverá aquí y desde entonces esta carpeta será la única fuente. Un cambio que agregue o modifique una ruta o un evento actualiza el contrato en el mismo pull request.
