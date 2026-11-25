# Proposal

## Why

Todas las operaciones financieras y de cambio legal irreversible en `S4` —confirmar un desembolso (`HU-20.1`), resolver un desembolso fallido o devuelto (`HU-20.2`), registrar o modificar datos de pago del becario (`HU-20.3`, `R5`, `R6`), revocar una beca (`HU-22.1`) y aplicar una consecuencia revocatoria (`HU-25.2`)— exigen un **servicio único transversal de códigos de verificación de un solo uso (OTP)** en `S1` (`ADR-10`, `DDD-10`, `R4a`–`R4d`, `R42`).

Como establece el Anexo A (`HU-20.1`, `HU-37.2`), el OTP por operación sensible es un flujo **independiente** del 2FA de inicio de sesión (`identidad-acceso`), y construirlo ahora dentro de `Ascenso.S1.Identidad` desbloquea de inmediato a Leonel y Sebas para implementar pagos y revocaciones en `S4` sobre el contrato privado oficial `POST /internal/v1/otp/validar-y-consumir` (`contracts/openapi-interno.yaml` v1.3).

## What Changes

- **Dominio S1 (`Ascenso.S1.Identidad.Domain`):** nueva raíz de agregado `DesafioOtpOperacion` (con `Version` explícita `ADR-25`) vinculada a `(UsuarioId, TipoOperacionSensible, RecursoId, ChallengeId)`, con código hasheado (SHA-256 + salt), vigencia de **15 minutos** (`R4b`), consumo atómico de un solo uso con clave de idempotencia `OperationIdConsumidor` (`R42`), conteo de **3 intentos fallidos** con bloqueo de **1 hora** por `(UsuarioId, Operacion, RecursoId)` (`R4c`) e invalidación del desafío previo en reenvío (`R4d`, `C14`).
- **Aplicación S1 (`Ascenso.S1.Identidad.Application`):** comandos y handlers `MediatR 12.5.0` delgados `SolicitarDesafioOtpCommand` (flujo público) y `ValidarYConsumirOtpCommand` (flujo privado `S4 → S1`), más adaptador de servicio que implementa `IServicioOtp` de `Ascenso.SharedKernel`.
- **Infraestructura S1 (`Ascenso.S1.Identidad.Infrastructure`):** persistencia de `DesafioOtpOperacion` y `BloqueoOtpOperacion` en `IdentidadDbContext` (`s1_identidad.desafios_otp_operacion` y `s1_identidad.bloqueos_otp_operacion`), reutilizando `ICanalEnvioCodigo` (SMTP real Mailpit/servidor + SMS extensible) y `IGeneradorCodigoSeguro`.
- **API S1 (`Ascenso.S1.Identidad.Api`):**
  - Endpoint público `POST /api/v1/otp/challenges` (`contracts/openapi.yaml`, devuelve `ChallengeResponse`).
  - Endpoint privado `POST /internal/v1/otp/validar-y-consumir` (`contracts/openapi-interno.yaml`, exige credencial de servicio con scope `otp.consumir` y rechaza tokens de usuario final con `401`/`403` según `SEGURIDAD_INTERSERVICIOS_V1.1.md`).
- **Pruebas completas (`UnitTests`, `IntegrationTests`, `ArchitectureTests`):** pruebas de emisión, vinculación estricta a operación/recurso/usuario, caducidad 15 min, 3 fallos → bloqueo 1 hora, replay, carrera concurrente de consumo único, idempotencia por `operationId` frente a `R42`, y frontera privada rechazando tokens de postulante/usuario final.

## Capabilities

### New Capabilities

- `codigos-un-solo-uso`: emisión pública y validación/consumo atómico privado de códigos OTP de un solo uso vinculados a usuario, operación sensible y recurso (`ADR-10`, `DDD-10`, `R4`–`R6`, `R42`).

### Modified Capabilities

Ninguna.

## Impact

- **Servicios afectados:** exclusivamente `services/Ascenso.S1.Identidad/` (amplía el servicio ya creado en el Cambio 3).
- **No modifica** `Ascenso.SharedKernel` ni `Ascenso.Contracts` (consume `IServicioOtp`, `TipoOperacionSensible`, `CanalCodigo` y `DesafioOtp` ya existentes en `Ascenso.SharedKernel`).
- **No rompe** `contracts/openapi.yaml` ni `contracts/openapi-interno.yaml`: materializa exactamente los endpoints ya definidos en ambos contratos (`POST /api/v1/otp/challenges` y `POST /internal/v1/otp/validar-y-consumir`).
- **Fuera de alcance en este PR:** las reglas de negocio de desembolsos, datos de pago o revocaciones (viven en `S4`), que consumirán este endpoint privado a través de `IValidadorOtp`.
