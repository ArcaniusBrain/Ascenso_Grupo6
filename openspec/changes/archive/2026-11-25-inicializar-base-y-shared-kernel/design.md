# Design

## Context

El repositorio tiene el diseño cerrado (`DDD_v3.3.md`, Arquitectura v1.3 y `ADR_CORRECTIVOS_V1.3.md`), pero todavía no contiene proyectos `.NET 10` ni entorno local reproducible. Los cinco integrantes necesitan trabajar en paralelo sobre `S1`–`S6` desde la Semana 1 sin duplicar tipos base ni romper las reglas de Clean Architecture (ADR-04) y propiedad de schemas (ADR-05).

## Goals / Non-Goals

**Goals:**
- Desbloquear al equipo con un `Ascenso.SharedKernel` mínimo, 100 % puro y cubierto por pruebas unitarias y de arquitectura.
- Materializar `Ascenso.Contracts` con el sobre común y los contratos `v1` del catálogo de integración (`EVENTOS_E_INTERFACES.md`).
- Proveer la base técnica de Outbox/Inbox sin contaminar `Ascenso.SharedKernel` con EF Core o MassTransit.
- Consolidar `contracts/` en la raíz como única fuente vigente de contratos OpenAPI y de eventos.
- Proveer `deploy/docker-compose.yml` con schemas PostgreSQL aislados (`s1..s6`) y CI `.NET 10` en GitHub Actions.

**Non-Goals:**
- Implementar lógica de negocio de ningún microservicio concreto (`S1`–`S6`): cada RF entrará en su propio cambio OpenSpec con specs Gherkin.
- Implementar `RegistroDecision` (`RF-35`) ni el PoC de 2FA en Keycloak (`RF-37`): son los dos cambios inmediatamente siguientes de la Semana 1.
- Configurar `gateway/` (YARP) con rutas de servicios que aún no existen ni `frontend/` (Next.js).

## Decisions

### 1. Solución `.NET 10` y gestión centralizada de paquetes (ADR-24)
- `Ascenso.sln` en la raíz con carpetas de solución `building-blocks/` y `tests/`.
- `Directory.Build.props` fija `TargetFramework` en `net10.0`, `Nullable=enable`, `ImplicitUsings=enable` y `TreatWarningsAsErrors=true`.
- `Directory.Packages.props` (`ManagePackageVersionsCentrally=true`) fija versiones exactas de `MediatR` (`12.5.0`, Apache 2.0 — nunca `>= 13.0.0`, verificado en pruebas/CI), `MediatR.Contracts` (`2.0.1`), xUnit y EF Core 10, evitando rangos flotantes.

### 2. Pureza de `Ascenso.SharedKernel` (DDD §5, ADR-04, ADR-25, ADR-27)
`Ascenso.SharedKernel` **no referencia** EF Core, ASP.NET Core, MassTransit, MongoDB ni Firebase. Contiene únicamente:
- **Tipos base DDD:**
  - `Entidad<TId>`: identidad por `Id`, lista de `EventosPendientes` de solo lectura, `Registrar(IEventoDeDominio)`, `LimpiarEventos()` e igualdad por `Id`.
  - `RaizDeAgregado<TId> : Entidad<TId>`: propiedad `Version` (`int`, inicial en `0`) y método protegido `IncrementarVersion()`, invocado ante cualquier cambio del agregado (incluyendo entidades internas, DDD-07 / ADR-25).
  - `IEventoDeDominio` (`DateTimeOffset Fecha`, extiende `INotification` de `MediatR.Contracts` `2.0.1`) e `IManejador<TEvento>`.
  - `ExcepcionDeDominio` base para violaciones de invariantes.
- **Mediación de casos de uso y eventos con `MediatR` `12.5.0` (ADR-27):**
  - Se adopta `MediatR` `12.5.0` (última versión Apache 2.0) fijado en `Directory.Packages.props` para comandos, consultas y publicación de eventos de dominio en `*.Application`.
  - `Ascenso.SharedKernel` solo referencia el paquete de contratos liviano `MediatR.Contracts` (`2.0.1`, cero dependencias externas) para que los eventos de dominio puedan publicarse directamente vía `IPublisher` sin wrappers repetitivos, manteniendo `Domain` libre de infraestructura.
  - Se añade una verificación automática en pruebas que falla si la versión de `MediatR` configurada en `Directory.Packages.props` sube a `>= 13.0.0`.
- **Reloj y temporalidad (ADR-17, R20):**
  - `IRelojNegocio` (`DateTimeOffset AhoraUtc`, `DateTimeOffset AhoraCaracas`, `DateOnly HoyCaracas`) implementado con `TimeProvider` sobre zona fija `UTC-4` (`America/Caracas`, sin DST).
- **Los 9 Value Objects del Shared Kernel (DDD §5):**
  - `Dinero` (`Cantidad >= 0`, `Moneda` de 3 letras mayúsculas ISO, `Sumar`, `Restar` sin resultado negativo, `EsMayorQue`, prohibido operar monedas distintas).
  - `Cedula` (normalizada, no vacía, formato válido).
  - `Contacto` (`Correo`, `Telefono`, validados al nacer).
  - `VentanaFechas` (`Apertura <= Cierre`; `Contiene(DateTimeOffset momento)` evalúa en UTC-4 incluyendo el día de cierre hasta `23:59:59.9999999` — R20).
  - `FechaLimite` (`Fecha`; `Vencida(DateTimeOffset momento)` es `false` hasta las `23:59:59.9999999` UTC-4 del día límite y `true` a partir de las `00:00:00` del día siguiente — R20).
  - `Promedio` (`Valor` no negativo, `EsMayorOIgual`).
  - `Justificacion` (`Texto` obligatorio y no vacío tras trim).
  - `ReferenciaArchivo` (`RutaFirebase` privada, no URL pública ni vacía).
  - `CodigoPeriodo` (`Valor` no vacío, normalizado, ej. `2026-2`).
- **Frontera de OTP (DDD-10, ADR-10):**
  - Enumeraciones `CanalCodigo` (`CORREO`, `SMS`) y `TipoOperacionSensible` (`CONFIRMAR_PAGO`, `RESOLVER_PAGO`, `MODIFICAR_DATOS_PAGO`, `REVOCAR_BECA`, `APLICAR_CONSECUENCIA_REVOCATORIA`).
  - Interfaz `IServicioOtp` con `SolicitarDesafio(...)` y `ValidarYConsumir(usuarioId, operacion, recursoId, challengeId, codigo)`.

### 3. `Ascenso.Contracts` — Eventos de integración versionados (ADR-13)
- Sin dependencias de `Ascenso.SharedKernel` ni de frameworks (solo tipos primitivos, `Guid`, `decimal`, `string`, `DateTimeOffset`).
- `IntegrationEventEnvelope<TPayload>` con `EventId`, `EventType`, `SchemaVersion`, `AggregateId`, `OccurredAtUtc`, `Producer`, `CorrelationId`, `CausationId`, `Payload`.
- Contratos `v1` inmutables (`sealed record`) alineados con `EVENTOS_E_INTERFACES.md` y `CATALOGO_EVENTOS_INTEGRACION.csv`:
  - `UsuarioRegistradoV1` (S1 → S3, S6)
  - `CicloPublicadoV1` (S3 → S2, S6), `PeriodoEvaluacionAbiertoV1` (S3 → S2, S6), `PostulacionSeleccionadaV1` (S3 → S4, S6), `SugerenciaListaEsperaV1` (S3 → S6)
  - `OfertaEmitidaV1`, `OfertaAceptadaV1`, `OfertaRechazadaV1`, `OfertaExpiradaV1` (S4 → S2, S3, S6)
  - `BecaActivadaV1`, `BecaSuspendidaV1`, `BecaRevocadaV1`, `BecaInactivadaV1` (S4 → S2/S3/S6 según catálogo)
  - `PagoConfirmadoV1`, `PagoFallidoV1`, `PagoDevueltoV1` (S4 → S2, S6)
  - `ReporteAcademicoRegistradoV1` (S5 → S5, S6), `IncumplimientoDetectadoV1` (S5 → S4, S6)
  - `PeriodoMarcadoActualV1` (S2 → S4, S6), `CostoMatriculaVigenteV1` (S2 → S4)
- Ningún contrato transporta entidades del dominio, credenciales, cuentas bancarias ni datos socioeconómicos crudos.

### 4. Base técnica de Outbox e Inbox separada del Shared Kernel (ADR-12, ADR-13, ADR-04)
- Para respetar `v1.3 §12` (*no contaminar `Ascenso.SharedKernel` con EF Core ni infraestructura*), las entidades técnicas `OutboxMessage` e `InboxMessage` y sus interfaces/configuraciones reutilizables de EF Core viven en `building-blocks/Ascenso.Messaging.Outbox/` (solo referenciado por los proyectos `*.Infrastructure` de cada servicio, nunca por `*.Domain`).
- `InboxMessage` impone clave única compuesta `(ConsumerName, EventId)` para idempotencia atómica.

### 5. Contratos en la raíz (`contracts/`)
- Se copian los contratos vigentes desde `docs/arquitectura/v1.3/03_CONTRATOS/` a `contracts/` y se actualiza `contracts/README.md` y `openspec/config.yaml` para declarar `contracts/` como única fuente activa a partir de este cambio.

### 6. Entorno local y portabilidad a servidor en `deploy/` (ADR-05, ADR-07, ADR-09, ADR-13, ADR-20)
- `deploy/docker-compose.yml` levanta la infraestructura reproducible para desarrollo local (y sirve como base directa si el servidor de despliegue usa Docker, superponiendo un archivo `docker-compose.prod.yml`). Ningún servicio `.NET` acopla rutas a `localhost`: toda conexión (`PostgreSQL`, `MongoDB`, `RabbitMQ`, `Keycloak`, `SMTP`) se inyecta vía `IConfiguration` / variables de entorno, permitiendo desplegar igualmente contra servicios externos en un servidor sin cambios de código.
- Servicios con healthchecks y volúmenes nombrados:
  - **PostgreSQL 16** + script `deploy/postgres/init-schemas.sql` que crea los 6 schemas (`s1_identidad`, `s2_catalogo_fondos`, `s3_postulacion_evaluacion`, `s4_becas`, `s5_seguimiento`, `s6_comunicaciones`) y roles aislados de runtime y migración sin permisos cruzados (`REVOKE ALL ON SCHEMA public`, permisos restringidos a su propio schema).
  - **MongoDB 7** (base de lectura CQRS de S6).
  - **RabbitMQ 3** con consola de administración (`15672`).
  - **Keycloak 26** en modo desarrollo (`start-dev`).
  - **Mailpit** (SMTP local + visor web de correos para pruebas de 2FA/OTP/notificaciones).
- `deploy/.env.example` documentando todas las variables sin secretos reales (`.env` permanece en `.gitignore`).

## Risks / Trade-offs

- **[Cambios posteriores en `SharedKernel` o `Contracts` afectan a todos]** → Mantener el núcleo pequeño y estrictamente apegado a DDD v3.3 §5; exigir revisión de todo el equipo en este PR.
- **[Emulador de Firebase Storage en Docker]** → Requiere probar enlaces firmados con el cliente GCS de .NET (pendiente D3, a cargo de Daniel en Semana 1); en este cambio se deja la configuración preparada sin bloquear el arranque de PostgreSQL/MongoDB/RabbitMQ/Keycloak.
- **[Versiones de paquetes NuGet]** → Se fijan explícitamente en `Directory.Packages.props` y se validan en CI con `dotnet build` y `dotnet test`.

## Open Questions

- **C8 (ADR-20):** proveedor real de correo/SMS para demostración (en desarrollo local se usa Mailpit).
- **D3 (ADR-06) y D4 (ADR-09):** pruebas técnicas de Firebase Storage (Daniel) y 2FA correo/SMS en Keycloak (Jesús, Cambio 3 de Semana 1).
