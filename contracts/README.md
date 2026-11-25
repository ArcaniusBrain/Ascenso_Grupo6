# Contratos

Según la arquitectura v1.3 (§12), esta carpeta es la **única fuente activa** de contratos que consume el código:

- [`openapi.yaml`](openapi.yaml): API pública (frontend → gateway YARP → servicios).
- [`openapi-interno.yaml`](openapi-interno.yaml): rutas privadas entre servicios (OAuth2 `client_credentials`, nunca expuestas por YARP).
- [`EVENTOS_E_INTERFACES.md`](EVENTOS_E_INTERFACES.md): reglas del sobre común, eventos de integración y contratos de reserva.
- [`MATRIZ_PUERTOS.md`](MATRIZ_PUERTOS.md): puertos y adaptadores entre capas.
- [`SEGURIDAD_INTERSERVICIOS_V1.1.md`](SEGURIDAD_INTERSERVICIOS_V1.1.md): reglas de autenticación y scopes entre servicios.

> **Regla del equipo:** cualquier cambio que agregue o modifique una ruta HTTP o un evento de integración debe actualizar el archivo correspondiente en esta carpeta y el catálogo CSV en el mismo pull request. La copia en `docs/arquitectura/v1.3/03_CONTRATOS/` queda como respaldo documental de la línea base v1.3.
