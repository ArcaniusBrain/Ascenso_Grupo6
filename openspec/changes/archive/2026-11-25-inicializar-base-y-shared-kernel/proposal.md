# Proposal

## Why

La arquitectura v1.3 (§12 y §17, paso 1) y el reparto de trabajo del equipo (Semana 1, 24 al 30-nov) fijan como primer incremento de código la base común del monorepo. Ningún servicio (`S1` a `S6`) puede implementar agregados ni eventos sin heredar antes de `Entidad` y `RaizDeAgregado` (con `Version` explícita según DDD-07 y ADR-25), usar los 9 value objects del Shared Kernel (DDD §5, R20, ADR-17), consumir el contrato de OTP (DDD-10, ADR-10) y publicar sobre el sobre común de `Ascenso.Contracts` (ADR-13). Además, los contratos OpenAPI/eventos deben pasar a `contracts/` con el primer cambio de código (`contracts/README.md`) y el equipo necesita `deploy/docker-compose.yml` (ADR-05/07/09/13) e integración continua de `.NET 10` (ADR-23/24) para trabajar en paralelo.

## What Changes

- Solución raíz `Ascenso.sln` (`.NET 10`), `Directory.Build.props` y `Directory.Packages.props` con versiones fijas (ADR-24).
- Proyecto `building-blocks/Ascenso.SharedKernel` (sin dependencias de infraestructura): tipos base DDD (`Entidad`, `RaizDeAgregado` con `Version`, `IEventoDeDominio`, `IManejador`), contratos base compatibles con `MediatR` `12.5.0` fijado en `Directory.Packages.props` (ADR-27), los 9 value objects del Shared Kernel, `IRelojNegocio` (UTC-4, R20), `IServicioOtp` y `TipoOperacionSensible`, con pruebas unitarias y de arquitectura.
- Proyecto `building-blocks/Ascenso.Contracts`: sobre común `IntegrationEventEnvelope` y contratos `v1` de los eventos de integración del catálogo.
- Proyecto `building-blocks/Ascenso.Messaging.Outbox` (base técnica reutilizable de Outbox/Inbox separada del Shared Kernel para mantener el dominio puro).
- Traslado de los contratos vigentes (`openapi.yaml`, `openapi-interno.yaml`, `EVENTOS_E_INTERFACES.md`, `MATRIZ_PUERTOS.md`, `SEGURIDAD_INTERSERVICIOS_V1.1.md`) a `contracts/` como única fuente activa.
- Entorno local en `deploy/docker-compose.yml`, `deploy/.env.example` y `deploy/postgres/init-schemas.sql` (PostgreSQL con schemas y roles aislados `s1..s6`, MongoDB, RabbitMQ, Keycloak y Mailpit).
- Flujo de CI `.github/workflows/dotnet.yml` (`build` y `test` en `.NET 10`).

## Capabilities

### New Capabilities

Ninguna. Es un cambio de infraestructura, contratos y código base sin comportamiento funcional de HU (`skip_specs: true`). El registro de decisiones (`RF-35`) y el acceso con Keycloak (`RF-37`) entran en cambios OpenSpec dedicados con sus specs Gherkin inmediatamente después.

### Modified Capabilities

Ninguna.

## Impact

- **Servicios afectados:** base compartida consumida por todos (`S1`, `S2`, `S3`, `S4`, `S5`, `S6`).
- **Toca `building-blocks/Ascenso.SharedKernel` y `building-blocks/Ascenso.Contracts`:** exige revisión de todo el equipo (Jesús, Daniel, Diego, Leonel y Sebas) antes de fusionar.
- **Carpetas nuevas o actualizadas:** `Ascenso.sln`, `Directory.Build.props`, `Directory.Packages.props`, `building-blocks/`, `contracts/`, `deploy/` y `.github/workflows/dotnet.yml`.
- **No modifica** ninguna regla del DDD v3.3 ni contratos de API; únicamente materializa los ya aprobados.
