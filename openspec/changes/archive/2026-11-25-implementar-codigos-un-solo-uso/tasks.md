# Tasks — `implementar-codigos-un-solo-uso` (ADR-10 / DDD-10)

## 1. Domain (`services/Ascenso.S1.Identidad/Ascenso.S1.Identidad.Domain`)

- [x] **1.1** Implementar las raíces `DesafioOtpOperacion` (vinculada a `UsuarioId + TipoOperacionSensible + RecursoId`, hash SHA-256 + salt, vigencia 15 min, consumo único atómico con `OperationIdConsumidor`, idempotencia vs `R42`, 3 intentos fallidos, `Version` explícita) y `BloqueoOtpOperacion` (bloqueo de 1 hora por `(UsuarioId, Operacion, RecursoId)`).
  - *Verificación:* pruebas unitarias en `OtpOperacionDominioTests` en verde.
- [x] **1.2** Declarar `IDesafioOtpOperacionRepository` y `IBloqueoOtpOperacionRepository` en `IRepositoriosIdentidad.cs`.
  - *Verificación:* `S1.Domain` compila sin referencias externas y `ArquitecturaIdentidadTests` (3/3) aprueba.

## 2. Application (`services/Ascenso.S1.Identidad/Ascenso.S1.Identidad.Application`)

- [x] **2.1** Implementar `SolicitarDesafioOtpCommand` / `SolicitarDesafioOtpHandler` (`MediatR 12.5.0`).
  - *Verificación:* pruebas de integración HTTP cubren solicitud por correo y SMS, reenvío invalidando anterior y fallo de envío (`503`).
- [x] **2.2** Implementar `ValidarYConsumirOtpCommand` / `ValidarYConsumirOtpHandler` (`MediatR 12.5.0`) y `ServicioOtpAplicacion : IServicioOtp`.
  - *Verificación:* pruebas de integración cubren consumo, vinculación cruzada, reuso (`409`), idempotencia `operationId` (`200`), expiración (`422`) y bloqueo de 1 hora (`429`).

## 3. Infrastructure (`services/Ascenso.S1.Identidad/Ascenso.S1.Identidad.Infrastructure`)

- [x] **3.1** Mapear `DesafioOtpOperacion` (`desafios_otp_operacion`) y `BloqueoOtpOperacion` (`bloqueos_otp_operacion` con índice único `(usuario_id, operacion, recurso_id)`) en `IdentidadDbContext`, implementar repositorios EF Core y registrar servicios en `DependencyInjection.cs`.
  - *Verificación:* prueba SQLite real `Concurrencia_Consumo_Otp_Solo_Un_Consumidor_Gana_Por_Version_Explicita` en verde.
- [x] **3.2** Implementar `ValidadorScopeInternoOtp` que exige token `client_credentials` con scope `otp.consumir` y rechaza tokens de usuario final (`401`/`403`, prueba (a) de `SEGURIDAD_INTERSERVICIOS_V1.1.md`).
  - *Verificación:* prueba HTTP en `OtpOperacionApiIntegracionTests` verifica `401` sin token, `403` con token de postulante, `403` sin scope y `200` con `otp.consumir`.

## 4. Api y Verificación (`services/Ascenso.S1.Identidad/Ascenso.S1.Identidad.Api`)

- [x] **4.1** Exponer `POST /api/v1/otp/challenges` (`ChallengeRequest` → `ChallengeResponse`, `contracts/openapi.yaml`) y `POST /internal/v1/otp/validar-y-consumir` (`OtpInternoCommand` → `OtpInternoResponse`, `contracts/openapi-interno.yaml`) en `Program.cs` con `ProblemDetails` RFC 9457.
  - *Verificación:* pruebas de integración HTTP (`WebApplicationFactory`) en verde.
- [x] **4.2** Ejecutar verificación final completa (`dotnet build`, `dotnet test` y `openspec validate --all --strict --no-interactive`).
  - *Verificación:* 0 advertencias, 0 errores, **55/55** pruebas aprobadas en 4 suites y OpenSpec válido en modo estricto.
