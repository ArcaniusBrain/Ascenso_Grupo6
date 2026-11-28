# Especificación: API Gateway con YARP (ADR-02, ADR-11, ADR-15)

## Purpose

Proveer el punto único de entrada HTTP y WebSockets de ASCENSO (`gateway/Ascenso.Gateway`) con YARP 2.3.0, enrutando por prefijo público hacia los seis microservicios (`S1`–`S6`) y SignalR (`/hubs/*`), propagando trazabilidad (`X-Correlation-Id`) y bloqueando de forma estricta cualquier intento de acceso externo a rutas privadas (`/internal/*`).

## Requirements

### Requirement: Enrutamiento declarativo por prefijo hacia S1..S6 y WebSockets SignalR

El API Gateway MUST declarar en `appsettings.json` (y sobreescribir localmente en `appsettings.Development.json`) las rutas públicas `/api/v1/*` hacia `cluster-s1`..`cluster-s6` y `/hubs/{**catch-all}` hacia `cluster-s6` (`ADR-02`, `ADR-15`), sin direcciones `localhost` hardcodeadas en C# y sin lógica de negocio.

#### Scenario: Solicitudes publicas se reenvian al microservicio propietario con encabezados intactos

- **WHEN** una solicitud llega a `/api/v1/tipos-usuario`, `/api/v1/programas`, `/api/v1/convocatorias/vigentes`, `/api/v1/becas/123/desembolsos`, `/api/v1/seguimiento/becarios`, `/api/v1/reportes/impacto` o `/hubs/notificaciones`
- **THEN** YARP la reenvía al clúster propietario (`cluster-s1`..`cluster-s6`) preservando el camino, los parámetros de consulta y los encabezados `Authorization`, `X-Actor-Usuario-Id` y `X-Correlation-Id`

### Requirement: Bloqueo estricto en el borde de rutas privadas /internal/* y traversal

`FronteraPrivadaMiddleware` MUST ejecutarse antes del proxy inverso YARP y rechazar con HTTP `403 Forbidden` en formato `application/problem+json` (`code: "FRONTERA_PRIVADA_BLOQUEADA"`) cualquier ruta que contenga el segmento `internal` (sin distinguir mayúsculas o minúsculas), secuencias `..` o barras invertidas `\`, incluso si llegan codificadas en URL (`ADR-02`, `ADR-11`).

#### Scenario: Intento externo de acceder a /internal/* recibe 403 sin tocar ningun backend

- **WHEN** un cliente solicita `/internal/v1/fondos/reservas`, `/INTERNAL/v1/reservas/123/vinculo` o `/api/v1/programas/%2e%2e/internal/v1/fondos/reservas`
- **THEN** el Gateway responde `403 Forbidden` con `application/problem+json` y `code: "FRONTERA_PRIVADA_BLOQUEADA"`
- **AND** ninguna solicitud es reenviada hacia los microservicios internos

### Requirement: Trazabilidad X-Correlation-Id y endpoint de salud

`CorrelationIdMiddleware` MUST conservar el encabezado `X-Correlation-Id` entrante (o generar un UUID nuevo si falta), propagarlo al microservicio destino y devolverlo en la respuesta; además, el Gateway MUST exponer `GET /health` devolviendo `200 OK` con `{ "status": "Healthy", "service": "Ascenso.Gateway" }`.

#### Scenario: Peticion a /health devuelve estado saludable con X-Correlation-Id

- **WHEN** un cliente consulta `GET /health` con o sin `X-Correlation-Id`
- **THEN** recibe `200 OK` con `service: "Ascenso.Gateway"` y el encabezado `X-Correlation-Id` poblado en la respuesta
