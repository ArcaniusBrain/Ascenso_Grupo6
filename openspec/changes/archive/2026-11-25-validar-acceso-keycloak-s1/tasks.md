# Tasks — `validar-acceso-keycloak-s1` (RF-37 + PoC ADR-09)

## 1. Domain (`services/Ascenso.S1.Identidad/Ascenso.S1.Identidad.Domain`)

- [x] **1.1** Crear la estructura Clean Architecture de `services/Ascenso.S1.Identidad/` (4 proyectos de servicio + 3 proyectos de pruebas) e incorporarla en `Ascenso.sln` y `Directory.Packages.props` (manteniendo `MediatR 12.5.0` y `TreatWarningsAsErrors=true`).
  - *Verificación:* `dotnet build Ascenso.sln --configuration Release` compila con 0 errores y 0 advertencias.
- [x] **1.2** Implementar los agregados `TipoUsuario`, `Usuario` y `ClienteApiUniversidad` (DDD v3.3 §13, todos con `Version` explícita ADR-25 y `SelloSeguridad` para cierre de sesiones C14).
  - *Verificación:* pruebas unitarias en `Ascenso.S1.Identidad.UnitTests` cubren `HU-37.1` Escenarios 1, 3, 7, 9, 10 y 11.
- [x] **1.3** Implementar la entidad `DesafioSegundoFactor` con código hasheado (SHA-256 + salt), vigencia de **15 minutos** (`R4b`, configurable), consumo único atómico, conteo de **3 intentos fallidos** con bloqueo de **1 hora**, invalidación en reenvío (`C14`) y recuperación por correo (`HU-37.2`).
  - *Verificación:* pruebas unitarias en `Ascenso.S1.Identidad.UnitTests` en verde.
- [x] **1.4** Implementar `CatalogoPermisos` y `MotorAutorizacionS1` (`HU-37.1` Escenarios 2, 4, 5, 6 y 8).
  - *Verificación:* pruebas unitarias en `Ascenso.S1.Identidad.UnitTests` en verde.

## 2. Application (`services/Ascenso.S1.Identidad/Ascenso.S1.Identidad.Application`)

- [x] **2.1** Definir los puertos `IProveedorIdentidadKeycloak`, `ICanalEnvioCodigo`, `IGeneradorCodigoSeguro` y `IServicioSesionAutenticada`, y los repositorios DDD en Domain.
  - *Verificación:* `Ascenso.S1.Identidad.Application` compila sin referencias a `Infrastructure` ni `Api`.
- [x] **2.2** Implementar los comandos y handlers `MediatR 12.5.0` de gestión de identidad (`CrearTipoUsuario`, `ActualizarPermisosTipoUsuario`, `EliminarTipoUsuario`, `CrearUsuarioInterno`, `CambiarRolesUsuario`, `ListarTiposUsuario`, `ListarUsuarios`).
  - *Verificación:* pruebas de integración HTTP cubren `HU-37.1` Escenarios 1, 2, 3, 7, 8, 9, 10, 11 y `C14`.
- [x] **2.3** Implementar los comandos y handlers `MediatR 12.5.0` de inicio de sesión 2FA (`IniciarSesionPaso1`, `VerificarSegundoFactor`, `ReenviarSegundoFactor`, `RecuperarSegundoFactor`).
  - *Verificación:* pruebas de integración HTTP cubren los 12 escenarios de `HU-37.2` (E1 a E12) y `C14`.

## 3. Infrastructure (`services/Ascenso.S1.Identidad/Ascenso.S1.Identidad.Infrastructure` + `deploy/`)

- [x] **3.1** Implementar `IdentidadDbContext` en schema `s1_identidad` (`ConfigurarOutboxEInbox()`, `ConfigurarRegistroDecisiones()`, `ConfigurarVersionExplicita()`, `AppendOnlyRegistroDecisionInterceptor`), repositorios EF Core y `SemillaIdentidadBase`.
  - *Verificación:* pruebas de integración en `Ascenso.S1.Identidad.IntegrationTests` con SQLite transaccional real en verde.
- [x] **3.2** Implementar los adaptadores reales de infraestructura: `ProveedorIdentidadKeycloakHttp` (Keycloak 26 OIDC/Admin API vía `IConfiguration`), `CanalEnvioCodigoCompuesto` (SMTP real hacia Mailpit/servidor + `CanalSmsSimulado`), `GeneradorCodigoSeguroCripto` y `ServicioSesionAutenticadaJwt`.
  - *Verificación:* prueba `ProveedorIdentidadKeycloakHttp_Envia_Grant_Password_Y_Maneja_401_Y_200` en verde.
- [x] **3.3** Crear `deploy/keycloak/realm-ascenso.json` (realm `ascenso`, SMTP Mailpit, clientes OIDC y `client_credentials` con scopes de `openapi-interno.yaml`), actualizar `deploy/docker-compose.yml` (`--import-realm`), ejecutar PoC en vivo contra Keycloak 26 + Mailpit + PostgreSQL 16 y cerrar `D4` en `docs/arquitectura/pendientes-v1.3.md`.
  - *Verificación:* PoC ejecutado en vivo (25-nov-2026): realm `ascenso` importado en `:8085`, password grant `401`/`200`, `client_credentials` S4 con scopes privados, correo 2FA recibido en Mailpit (`:1025`/`:8025`) y 6 schemas + función `impedir_modificacion_registro_decisiones` verificados en PostgreSQL 16.
- [x] **3.4** Implementar la suite obligatoria `Ascenso.S1.Identidad.ArchitectureTests` (Clase 09, láminas 31 y 39).
  - *Verificación:* 3/3 pruebas de arquitectura aprobadas.

## 4. Api y Contratos (`services/Ascenso.S1.Identidad/Ascenso.S1.Identidad.Api` + `contracts/`)

- [x] **4.1** Implementar `Ascenso.S1.Identidad.Api` (`Program.cs`, `ProblemDetails` RFC 9457, `/health` y endpoints públicos `/api/v1/tipos-usuario`, `/api/v1/usuarios`, `/api/v1/usuarios/{usuarioId}/roles`, `/api/v1/auth/login`, `/api/v1/auth/2fa/verificar`, `/api/v1/auth/2fa/reenviar`, `/api/v1/auth/2fa/recuperar`).
  - *Verificación:* pruebas de integración HTTP (`WebApplicationFactory`) en verde.
- [x] **4.2** Actualizar `contracts/openapi.yaml` documentando las nuevas rutas públicas de S1.
  - *Verificación:* rutas de S1 alineadas con `ProblemDetails` y `BearerAuth`.
- [x] **4.3** Ejecutar verificación integral final (`dotnet build Ascenso.sln --configuration Release`, `dotnet test Ascenso.sln --configuration Release` y `openspec validate --all --strict --no-interactive`).
  - *Verificación:* 0 advertencias, 0 errores, 51/51 pruebas aprobadas en 4 proyectos de prueba y OpenSpec válido en modo estricto.
