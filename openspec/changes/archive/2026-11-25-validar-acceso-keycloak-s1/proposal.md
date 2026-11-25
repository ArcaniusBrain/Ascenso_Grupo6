# Proposal

## Why

La gestión de usuarios internos, tipos de usuario configurables, control de acceso por función y verificación en dos pasos (**RF-37**: **HU-37.1** y **HU-37.2**) es prerrequisito funcional de toda la plataforma: ningún módulo puede verificar "quién puede hacer qué", firmar un `RegistroDecision` válido (`RF-35`) ni habilitar sesiones privilegiadas sin antes validar la integración de **Keycloak + 2FA por correo o SMS** exigida por **ADR-09** (pendiente técnico `D4`, marcado como *Condicionada a PoC* antes de emitir tokens a roles privilegiados).

Este cambio construye el primer vertical slice completo de microservicio Clean Architecture + DDD del monorepo (**`services/Ascenso.S1.Identidad/`**, según `ARQUITECTURA_V1.0.md` §12), materializando:
- **HU-37.1** (*Crear usuarios internos y tipos de usuario con acceso limitado a su función*, 11 escenarios): gestión exclusiva de Dirección sobre tipos de usuario configurables sin cambios de código y alta de usuarios internos/externos por cédula única (`R1`), marca automática de 2FA para tipos con acceso financiero o de decisión, bloqueo de auto-escalamiento y motor centralizado de permisos aplicado en backend (incluyendo ocultación de puntuaciones ajenas antes del cierre, bloqueo de edición de rúbricas por finanzas y exclusión de datos socioeconómicos en respuestas del servidor).
- **HU-37.2** (*Iniciar sesión con verificación en dos pasos*, 12 escenarios): autenticación en dos pasos obligatoria para roles con acceso financiero o de decisión, evaluadores externos (por correo, `R4f`), enlaces de universidades aliadas (`R3`) y tipos nuevos con ese acceso; código por correo o SMS a elección del usuario (`R4a`), vigencia de **15 minutos** (`R4b`), **3 intentos fallidos** con bloqueo de **1 hora** (`R4c`), reenvío (`R4d`), recuperación por correo (`R4e`) y bloqueo absoluto de sesión si falla el envío o se omite el segundo paso.
- **ADR y reglas aplicadas:** **ADR-01/02/04** (Clean Architecture 4 capas + prueba de arquitectura Clase 09), **ADR-05** (schema aislado `s1_identidad`), **ADR-09** (Keycloak OIDC + 2FA correo/SMS controlado por S1 + PoC), **ADR-11** (política centralizada de permisos/alcance/estado y masking), **ADR-12** (`RegistroDecision` + Outbox atómico), **ADR-16/25** (`Version` explícita en `Usuario`, `TipoUsuario`, `ClienteApiUniversidad`), **ADR-17/R20** (`IRelojNegocio` inyectado) y **ADR-27** (`MediatR 12.5.0`).

## What Changes

- **Primer microservicio completo (`services/Ascenso.S1.Identidad/`):** proyectos `Domain`, `Application`, `Infrastructure`, `Api` y `tests/{Unit,Integration,Architecture}` incorporados a `Ascenso.sln`.
- **Dominio S1 (`Ascenso.S1.Identidad.Domain`):** agregados `Usuario`, `TipoUsuario`, `ClienteApiUniversidad` (DDD v3.3 §13), entidad de estado `DesafioSegundoFactor`, catálogo único de permisos, `MotorAutorizacionS1` (permisos, alcance, estado y masking socioeconómico) y evento `UsuarioRegistradoEvento` (`DDD-11`).
- **Aplicación S1 (`Ascenso.S1.Identidad.Application`):** comandos/queries `MediatR 12.5.0` delgados para tipos de usuario, alta de usuarios, bloqueo de auto-escalamiento, flujo de inicio de sesión 2FA (paso 1, verificación, reenvío y recuperación por correo) y puertos `IProveedorIdentidadKeycloak` / `ICanalEnvioCodigo`.
- **Infraestructura S1 (`Ascenso.S1.Identidad.Infrastructure`):** `IdentidadDbContext` en schema `s1_identidad` con `OutboxMessage`, `InboxMessage`, `RegistroDecision` append-only (`AppendOnlyRegistroDecisionInterceptor`), migración inicial EF Core, adaptador HTTP real contra **Keycloak 26** y adaptadores de envío (SMTP real hacia Mailpit + SMS extensible).
- **PoC Keycloak y despliegue (`deploy/keycloak/realm-ascenso.json`, `deploy/docker-compose.yml`):** configuración declarativa del realm `ascenso` (clientes OIDC y `client_credentials` interservicios) importada automáticamente y verificación del flujo real Keycloak + Mailpit + S1 (cierra `D4` / `ADR-09`).
- **API y contratos (`Ascenso.S1.Identidad.Api` y `contracts/openapi.yaml`):** endpoints públicos versionados de S1 (`/api/v1/tipos-usuario`, `/api/v1/usuarios`, `/api/v1/auth/*`) con `ProblemDetails` RFC 9457, sin hardcodear `localhost`.
- **Pruebas completas:** pruebas unitarias de dominio, pruebas de integración (SQLite transaccional + pruebas de endpoints/autorización + evidencia PoC Keycloak/Mailpit) y pruebas de arquitectura (`S1.Domain` sin infraestructura ni `Application`/`Infrastructure`).

## Capabilities

### New Capabilities

- `identidad-acceso`: gestión configurable de tipos de usuario y alta de usuarios internos/externos con autorización aplicada en servidor (`HU-37.1`) e inicio de sesión con verificación en dos pasos por correo o SMS (`HU-37.2`).

### Modified Capabilities

Ninguna.

## Impact

- **Servicios afectados:** crea `services/Ascenso.S1.Identidad/` (dueño de `RF-37`). No crea proyectos vacíos en `S2`–`S6`.
- **No modifica** `Ascenso.SharedKernel` ni `Ascenso.Contracts` (consume los tipos ya entregados en los Cambios 1 y 2: `RaizDeAgregado`, `Cedula`, `Contacto`, `CanalCodigo`, `IRelojNegocio`, `RegistroDecision`, `UsuarioRegistradoV1`).
- **Toca archivos compartidos del repo:** `Ascenso.sln`, `Directory.Packages.props` (paquetes ASP.NET Core / Npgsql / JWT necesarios para S1, manteniendo `MediatR 12.5.0`), `contracts/openapi.yaml` (rutas públicas de S1), `deploy/docker-compose.yml`, `deploy/keycloak/realm-ascenso.json` y `docs/arquitectura/pendientes-v1.3.md` (cierre de `D4` tras PoC).
- **Fuera de alcance en este PR:** la capacidad hermana `codigos-un-solo-uso` (`ADR-10`: OTP por operación sensible `/api/v1/otp/challenges` e `/internal/v1/otp/validar-y-consumir` para desembolsos y revocaciones), que se implementará como incremento dedicado posterior para mantener este PR acotado y fácil de revisar.
