# Design — `implementar-codigos-un-solo-uso` (ADR-10 / DDD-10)

## 1. Contexto, agregados y ADRs aplicados

- **Fuentes funcionales:** `HU-20.1` (Escenarios 2, 4, 7, 12, 13 y FA-4/FA-9/FA-10), `HU-20.2`, `HU-20.3` (`R5`, `R6`), `HU-22.1`, `HU-25.2`, `R4a`–`R4d`, `R42` y `Anexo A` (*servicio único de verificación transversal, independiente del 2FA de inicio de sesión*).
- **Contratos vigentes ya definidos (sin cambios incompatibles):**
  - Público (`contracts/openapi.yaml`): `POST /api/v1/otp/challenges` (`ChallengeRequest { operacion, resourceId, canal }` → `ChallengeResponse { challengeId, expiraEnUtc }`).
  - Privado (`contracts/openapi-interno.yaml` + `SEGURIDAD_INTERSERVICIOS_V1.1.md`): `POST /internal/v1/otp/validar-y-consumir` (`OtpInternoCommand { usuarioId, operacion, recursoId, challengeId, codigo, operationId }` → `OtpInternoResponse { consumido, challengeId }`), protegido por OAuth2 `client_credentials` con scope `otp.consumir`, nunca expuesto por YARP.
- **ADRs aplicados:**
  - **ADR-10 / DDD-10:** OTP vinculado a `(usuarioId, operacion: TipoOperacionSensible, recursoId, challengeId)`, hash SHA-256 + salt, 15 min, 3 intentos → bloqueo 1 hora, consumo único atómico.
  - **ADR-04:** mantiene intacto el aislamiento de capas de `Ascenso.S1.Identidad` (`Domain` puro sin EF Core/HTTP).
  - **ADR-05:** tablas propias dentro del schema `s1_identidad` (`desafios_otp_operacion` y `bloqueos_otp_operacion`), separadas de `desafios_segundo_factor`.
  - **ADR-16 / ADR-25:** `Version` explícita con `IsConcurrencyToken()` en `DesafioOtpOperacion` para que dos consumos paralelos compitan de forma segura y solo uno gane.
  - **ADR-17 / R20:** todo cálculo de 15 minutos y 1 hora usa `IRelojNegocio` inyectado.
  - **ADR-20:** reutiliza `ICanalEnvioCodigo` (SMTP real Mailpit/servidor y SMS extensible).

## 2. Modelo de dominio nuevo en `Ascenso.S1.Identidad.Domain`

1. **`DesafioOtpOperacion : RaizDeAgregado<Guid>`** (con `Version` explícita):
   - `Id` (`ChallengeId`), `UsuarioId`, `Operacion` (`TipoOperacionSensible` de `Ascenso.SharedKernel`), `RecursoId` (`string` normalizado), `Canal` (`CanalCodigo`), `CodigoHash` (SHA-256 + salt), `Salt`, `CreadoEnUtc`, `ExpiraEnUtc` (`+15 min`), `IntentosFallidos`, `ConsumidoEnUtc`, `OperationIdConsumidor` (`Guid?`), `InvalidadoEnUtc`.
   - Método `ValidarYConsumir(Guid usuarioId, TipoOperacionSensible operacion, string recursoId, string? codigoIngresado, Guid operationId, DateTimeOffset ahoraUtc)` → devuelve `ResultadoConsumoOtp`:
     - Si ya fue consumido con el **mismo `operationId`** y mismos `(usuarioId, operacion, recursoId)` → `CONSUMIDO_IDEMPOTENTE` (resiste cortes de red en `S4 → S1`).
     - Si ya fue consumido con un **`operationId` distinto** (ej. fallo posterior en S4, `R42`, o replay `HU-20.1 E4`) o invalidado → `RECHAZADO_REUSO_O_INVALIDADO`.
     - Si no coincide `usuarioId`, `operacion` o `recursoId` → `VINCULO_NO_COINCIDE` (sin revelar detalles).
     - Si `ahoraUtc > ExpiraEnUtc` (`> 15 min`, `HU-20.1 E12`) → `EXPIRADO`.
     - Comparación en tiempo constante (`CryptographicOperations.FixedTimeEquals`):
       - Si coincide → fija `ConsumidoEnUtc = ahoraUtc`, `OperationIdConsumidor = operationId`, `IncrementarVersion()` → `CONSUMIDO`.
       - Si no coincide → `IntentosFallidos++`, `IncrementarVersion()`: si alcanza `3`, invalida y devuelve `BLOQUEO_POR_INTENTOS` (`HU-20.1 E13`); si `< 3`, devuelve `CODIGO_INCORRECTO`.
2. **`BloqueoOtpOperacion : RaizDeAgregado<Guid>`** (clave única `(UsuarioId, Operacion, RecursoId)`):
   - Registra `BloqueadoHastaUtc = ahoraUtc.AddHours(1)` cuando ocurre el 3er intento fallido sobre esa terna `(usuarioId, operacion, recursoId)` (`HU-20.1 FA-9`: *"bloquea la confirmación de ese desembolso y no acepta nuevos códigos hasta que pase 1 hora"*).
   - Esto garantiza que bloquear un desembolso concreto no bloquee indebidamente el login 2FA del usuario ni operaciones sobre otros recursos distintos.

## 3. Seguridad de frontera privada (`POST /internal/v1/otp/validar-y-consumir`)

- **Regla de `SEGURIDAD_INTERSERVICIOS_V1.1.md`:** un token de usuario final **nunca** puede invocar `/internal/v1/otp/validar-y-consumir`.
- Se crea un puerto/validador de frontera interna `IValidadorClienteInternoOtp`:
  - Verifica que la solicitud traiga un token/credencial de servicio válida (`Authorization: Bearer <jwt>` emitido por Keycloak con `client_credentials` / claim `scope` que contenga `otp.consumir` y sin ser un usuario final, o header técnico validado por `OpcionesSeguridadInterna` en configuración).
  - Si falta autorización → `401 Unauthorized`; si el token corresponde a un usuario final (ej. postulante) o carece del scope `otp.consumir` → `403 Forbidden` (cubre exactamente la prueba `(a)` de `SEGURIDAD_INTERSERVICIOS_V1.1.md`).
- En concurrencia de consumo (`DbUpdateConcurrencyException` en `DesafioOtpOperacion.Version`), el endpoint devuelve `409 Conflict` con `ProblemDetails` RFC 9457, garantizando que nunca se autorice dos veces el mismo código.

## 4. Eventos de integración y contratos

- No se crean nuevos eventos en `Ascenso.Contracts` ni se cambia `Ascenso.SharedKernel` (ya contiene `IServicioOtp` y `TipoOperacionSensible`).
- Las rutas `POST /api/v1/otp/challenges` (`contracts/openapi.yaml`) y `POST /internal/v1/otp/validar-y-consumir` (`contracts/openapi-interno.yaml`) ya están formalizadas en los contratos v1.3 y se implementan respetando exactamente sus nombres de campos (`resourceId` en `ChallengeRequest`; `recursoId` y `operationId` en `OtpInternoCommand`).

## 5. Preguntas abiertas

Ninguna bloqueante: todos los parámetros de `ADR-10` / `DDD-10` (`R4a`–`R4d`, `R5`, `R6`, `R42` y `C14`) están completamente cerrados.
