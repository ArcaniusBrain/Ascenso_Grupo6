# API Gateway (YARP) — `Ascenso.Gateway`

Punto único de entrada HTTP y WebSockets de ASCENSO (`ADR-02`, `ADR-11`, `ADR-15`), implementado en .NET 10 con **YARP 2.3.0** (`Yarp.ReverseProxy`, licencia MIT).

## Responsabilidades

- Enrutar por prefijo público (`/api/v1/*`) hacia los seis microservicios (`S1`–`S6`) y dejar pasar WebSockets para SignalR (`/hubs/*` → `S6`).
- Bloquear en el borde toda ruta privada (`/internal/*`) o intento de path traversal (`..`, `\`, URL-encoded) con HTTP `403 Forbidden` (`application/problem+json`, `code: "FRONTERA_PRIVADA_BLOQUEADA"`), sin reenviar jamás la petición a ningún backend.
- Garantizar y propagar `X-Correlation-Id` en cada petición y respuesta.
- Exponer `GET /health` (`{ "status": "Healthy", "service": "Ascenso.Gateway" }`).
- **Cero lógica de negocio**: cada microservicio vuelve a validar el token JWT y aplica sus propias reglas de autorización.

## Mapa de rutas públicas

| Clúster | Destino (Contenedor / Local Dev) | Prefijos públicos |
| --- | --- | --- |
| `cluster-s1` | `s1-identidad:5101` / `127.0.0.1:5101` | `/api/v1/auth`, `/api/v1/usuarios`, `/api/v1/tipos-usuario`, `/api/v1/otp` |
| `cluster-s2` | `s2-catalogo-fondos:5102` / `127.0.0.1:5102` | `/api/v1/periodos`, `/api/v1/universidades`, `/api/v1/programas`, `/api/v1/donantes` |
| `cluster-s3` | `s3-postulacion-evaluacion:5103` / `127.0.0.1:5103` | `/api/v1/convocatorias`, `/api/v1/postulantes`, `/api/v1/postulaciones`, `/api/v1/evaluaciones`, `/api/v1/rubricas` |
| `cluster-s4` | `s4-becas:5104` / `127.0.0.1:5104` | `/api/v1/ofertas`, `/api/v1/becas`, `/api/v1/desembolsos`, `/api/v1/pagos` |
| `cluster-s5` | `s5-seguimiento:5105` / `127.0.0.1:5105` | `/api/v1/seguimiento`, `/api/v1/reportes-academicos`, `/api/v1/renovaciones`, `/api/v1/pausas` |
| `cluster-s6` | `s6-comunicaciones:5106` / `127.0.0.1:5106` | `/api/v1/consentimientos-donantes`, `/api/v1/reportes`, `/api/v1/notificaciones`, `/api/v1/consultas`, `/api/v1/indicadores`, `/hubs/*` |

## Ejecución local

1. Levantar el Gateway en el puerto `5000` (usa `appsettings.Development.json` apuntando a `127.0.0.1:5101`–`5106`):

```bash
dotnet run --project gateway/Ascenso.Gateway --urls http://127.0.0.1:5000
```

2. Conectar el BFF del frontend (`frontend/.env.local`) al Gateway:

```env
ASCENSO_GATEWAY_URL=http://127.0.0.1:5000
```

Con `ASCENSO_GATEWAY_URL` definido, `frontend/src/lib/bff/router.ts` envía automáticamente todo el tráfico `/api/bff/*` al Gateway en lugar de resolver puertos directos por servicio.

## Pruebas

```bash
dotnet test gateway/tests/Ascenso.Gateway.Tests/Ascenso.Gateway.Tests.csproj
```
