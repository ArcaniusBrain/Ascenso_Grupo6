# 03 · `S1` Identidad, Keycloak, 2FA y Códigos OTP (`RF-37`, `ADR-09`, `ADR-10`)

El microservicio **`services/Ascenso.S1.Identidad/`** (schema `s1_identidad`) ya está completo en `main`. Esta guía explica cómo lo consumen **`S4` (OTP)**, **`frontend/` (Login + 2FA + gestión de usuarios)** y **`S5` (clientes universitarios)**.

---

## 1. Qué hace Keycloak y qué hace `S1` (`ADR-09`, verificado en PoC vivo)

- **Keycloak 26 (`deploy/keycloak/realm-ascenso.json`, puerto `8085`)** gestiona contraseñas, usuarios del realm `ascenso`, revocación de sesiones y tokens máquina-a-máquina (`client_credentials`).
- **`S1`** gestiona los tipos de usuario configurables, el gate obligatorio de **2FA por correo/SMS** antes de emitir el token de sesión interna (con vigencia configurable de 15 min por `R4b`), y los **códigos OTP de un solo uso por operación sensible (`ADR-10`)**.

### Usuarios y clientes precargados en Keycloak local (`realm-ascenso.json`)

| Tipo | Identificador | Credencial local | Notas |
|---|---|---|---|
| Usuario Dirección | `director@ascenso.org` | `AscensoDir2026!` | Rol `DIRECCION` |
| Usuario Coordinador | `laura.coordinadora@ascenso.org` | `AscensoCoord2026!` | Rol `COORDINADOR` |
| Usuario Finanzas | `carlos.finanzas@ascenso.org` | `AscensoFin2026!` | Rol `FINANZAS` |
| Cliente confidencial `S1` | `ascenso-s1-identidad` | `dev-s1-identidad-secret` | Valida contraseñas y revoca sesiones |
| Cliente confidencial `S4` | `ascenso-s4-becas` | `dev-s4-becas-secret` | Emite tokens `client_credentials` con scopes `otp.consumir`, `fondos.reservar`, `fondos.liberar` |

Para verificar todo el flujo en vivo contra Docker (`postgres` + `keycloak` + `mailpit`):
```bash
node tools/poc/poc-keycloak-2fa.mjs
```
Evidencia documentada en [`docs/arquitectura/evidencias/POC_ADR09_KEYCLOAK_2FA.md`](../arquitectura/evidencias/POC_ADR09_KEYCLOAK_2FA.md).

---

## 2. Integración CRÍTICA para `S4` (Leonel y Sebas): Códigos OTP por operación sensible

Las **5 operaciones sensibles** (`TipoOperacionSensible` en `Ascenso.SharedKernel.Seguridad`) exigen validar y consumir un OTP de un solo uso antes de confirmar el cambio en `S4`:

1. `CONFIRMAR_PAGO` (Finanzas)
2. `RESOLVER_PAGO` (Finanzas, `HU-20.2`)
3. `MODIFICAR_DATOS_PAGO` (Becario/Finanzas)
4. `REVOCAR_BECA` (Coordinador/Dirección, `HU-22.1`)
5. `APLICAR_CONSECUENCIA_REVOCATORIA` (Coordinador/Dirección, `HU-25.2`)

### Flujo completo extremo a extremo

```text
1. Frontend -> POST /api/v1/otp/challenges (hacia S1, con token del usuario)
   Body: { "operacion": "CONFIRMAR_PAGO", "resourceId": "<desembolsoId>", "canal": "CORREO" }
   Resp: { "challengeId": "<uuid>", "expiraEnUtc": "..." }
   (S1 envía código de 6 dígitos por correo/SMS; vigencia 15 min)

2. Usuario escribe el código en pantalla y confirma la acción.

3. Frontend -> S4 (ej. POST /api/v1/becas/{id}/pagos)
   Body incluye: { "operationId": "<uuid>", "otp": { "challengeId": "<uuid>", "codigo": "123456" }, ... }

4. S4 obtiene token máquina-a-máquina de Keycloak (client_credentials, client_id=ascenso-s4-becas, scope=otp.consumir)
   y llama a la frontera privada de S1 (NUNCA expuesta por YARP):

   POST http://<host-s1>/internal/v1/otp/validar-y-consumir
   Authorization: Bearer <token client_credentials con scope otp.consumir>
   Body:
   {
     "usuarioId": "<usuarioId del token del usuario>",
     "operacion": "CONFIRMAR_PAGO",
     "recursoId": "<desembolsoId>",
     "challengeId": "<uuid>",
     "codigo": "123456",
     "operationId": "<mismo operationId del paso 3>"
   }

5. S1 responde 200 OK { "consumido": true, "challengeId": "<uuid>" }
   -> S4 ejecuta su transacción local (Agregado + RegistroDecision + Outbox).
```

### Reglas y códigos HTTP que `S4` debe manejar al llamar a `S1`

| HTTP de `S1` | Código de error | Significado y qué debe hacer `S4` |
|---|---|---|
| **`200 OK`** | — | OTP válido y consumido (o reintento idempotente con el **mismo `operationId`**, `R42`). Continuar operación. |
| **`401 Unauthorized`** | — | Falta token `Authorization: Bearer ...` de `S4` hacia `S1`. |
| **`403 Forbidden`** | — | El token enviado a `/internal/v1/...` es de usuario final o no tiene el scope `otp.consumir`. |
| **`409 Conflict`** | `OTP_REUTILIZADO` | El código ya fue consumido por otro `operationId` distinto (`R42`). Rechazar la operación. |
| **`422 Unprocessable`** | `OTP_INVALIDO` / `OTP_EXPIRADO` / `OTP_VINCULACION_INVALIDA` | Código incorrecto, vencido (15 min) o emitido para otro usuario/operación/recurso. **Tras un fallo, el usuario debe solicitar un código nuevo (`B4`).** |
| **`429 Too Many Requests`** | `OTP_BLOQUEADO` | 3 fallos acumulados en la terna `(usuarioId, operacion, recursoId)` → bloqueo de 1 hora en esa operación/recurso. |

> **Atención:** en pruebas de integración de `S4` puedes inyectar un adaptador HTTP o fake implementando `IServicioOtp` (`Ascenso.SharedKernel.Seguridad`) mientras integras el cliente real `HttpClient` hacia `/internal/v1/otp/validar-y-consumir`.

---

## 3. Rutas públicas de `S1` (para `frontend/` y pruebas)

Todas documentadas en `contracts/openapi.yaml` bajo el tag `S1`:
- **Autenticación y 2FA:** `POST /api/v1/auth/login`, `POST /api/v1/auth/2fa/verificar`, `POST /api/v1/auth/2fa/reenviar`, `POST /api/v1/auth/2fa/recuperar`.
- **OTP:** `POST /api/v1/otp/challenges`.
- **Tipos de usuario y permisos (solo Dirección escribe):** `GET /api/v1/tipos-usuario`, `POST /api/v1/tipos-usuario`, `PUT /api/v1/tipos-usuario/{tipoUsuarioId}/permisos`, `DELETE /api/v1/tipos-usuario/{tipoUsuarioId}`.
- **Usuarios internos (solo Dirección):** `GET /api/v1/usuarios`, `POST /api/v1/usuarios`, `PUT /api/v1/usuarios/{usuarioId}/roles`.

> **Decisiones cerradas de `S1` (`C14`):** cambiar permisos o roles de un usuario revoca inmediatamente sus sesiones en Keycloak y rota su `SelloSeguridad`, obligando un nuevo inicio de sesión; un `TipoUsuario` asignado a al menos un usuario no puede eliminarse (`409`); un rol sin acceso financiero ni poder de decisión queda exento de 2FA.
