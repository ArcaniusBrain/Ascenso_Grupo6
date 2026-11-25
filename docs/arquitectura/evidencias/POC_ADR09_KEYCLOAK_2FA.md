# Evidencia de Prueba Técnica — PoC ADR-09 (D4): Keycloak 26 + 2FA Correo/SMS + S1

- **Fecha de ejecución:** 25 de noviembre de 2026
- **Responsable:** Jesús Noriega (`S1 Identidad y Acceso`)
- **Referencias:** `ADR-09`, `ARQUITECTURA_V1.0.md` §8.1, `RF-37` (`HU-37.1`, `HU-37.2`), `pendientes-v1.3.md` (`D4`).
- **Entorno ejecutado:** Docker `28.2.2` con `quay.io/keycloak/keycloak:26.0`, `axllent/mailpit:v1.21`, `postgres:16-alpine` y `.NET 10` (`Ascenso.sln`).

---

## 1. Arquitectura validada

Keycloak 26 gestiona credenciales primarias, emisión OIDC/JWT y clientes OAuth2 `client_credentials` interservicios mediante el realm declarativo [`deploy/keycloak/realm-ascenso.json`](../../../deploy/keycloak/realm-ascenso.json). Dado que Keycloak 26 no incluye 2FA nativo por correo/SMS con las reglas de `R4` (elección de canal correo/SMS, 15 min, 3 intentos → bloqueo de 1 hora, evaluador externo por correo y recuperación por correo), **`Ascenso.S1.Identidad`** actúa como compuerta obligatoria del segundo factor conforme a `ARQUITECTURA_V1.0.md` §8.1: bloquea la emisión de sesión hasta validar el `DesafioSegundoFactor`.

---

## 2. Comandos ejecutados y resultados verificados

### A. Contenedores e inicialización de PostgreSQL 16 (`init-schemas.sql`)
```bash
cd deploy
docker compose up -d postgres keycloak mailpit
docker exec ascenso-postgres psql -U ascenso_admin -d ascenso -c "\dn s*" -c "SELECT proname FROM pg_proc WHERE proname = 'impedir_modificacion_registro_decisiones';"
```

Resultado observado:
- 6 schemas aislados creados con sus owners `*_migrator`: `s1_identidad`, `s2_catalogo_fondos`, `s3_postulacion_evaluacion`, `s4_becas`, `s5_seguimiento`, `s6_comunicaciones`.
- Función trigger append-only verificada: `impedir_modificacion_registro_decisiones`.

### B. Flujo en vivo Keycloak 26 + Mailpit (`tools/poc/poc-keycloak-2fa.mjs`)
```bash
node tools/poc/poc-keycloak-2fa.mjs
```

Salida real obtenida (`exit code 0`):
```text
1. Esperando Keycloak realm ascenso y Mailpit...
   OK Keycloak issuer: http://localhost:8085/realms/ascenso
   OK Mailpit listo en http://localhost:8025
2. Probando contraseña incorrecta en Keycloak (debe fallar 401)...
   OK Contraseña incorrecta rechazada (401 Unauthorized)
3. Probando contraseña correcta de laura.coordinadora@ascenso.org en Keycloak...
   OK Credenciales primarias validadas por Keycloak 26 (JWT recibido)
4. Probando OAuth2 client_credentials de ascenso-s4-becas con scopes privados...
   OK Token S4 emitido con scopes: fondos.liberar fondos.reservar otp.consumir
5. Enviando código 2FA real vía SMTP a Mailpit y verificando bandeja...
   OK Correo 2FA recibido en Mailpit para laura.coordinadora@ascenso.org con código 482913

=== PoC ADR-09 (D4) COMPLETADO CON ÉXITO ===
```

### C. Suite automatizada `.NET 10` (`Ascenso.sln`)
```bash
dotnet build Ascenso.sln --no-restore --configuration Release
dotnet test Ascenso.sln --no-build --configuration Release --verbosity normal
```

Resultado verificado:
- Compilación `Release` (`TreatWarningsAsErrors=true`): **0 advertencias, 0 errores** (11 proyectos).
- Pruebas aprobadas: **51/51** (`Ascenso.BuildingBlocks.Tests`: 35, `Ascenso.S1.Identidad.UnitTests`: 10, `Ascenso.S1.Identidad.ArchitectureTests`: 3 [Clase 09], `Ascenso.S1.Identidad.IntegrationTests`: 3 cubriendo HTTP de extremo a extremo para `HU-37.1`, `HU-37.2` y `ProveedorIdentidadKeycloakHttp`).

---

## 3. Conclusión

El pendiente técnico **`D4`** (`ADR-09`) queda **cerrado** con evidencia reproducible. El servicio `Ascenso.S1.Identidad` queda habilitado para emitir sesiones autenticadas con segundo factor verificado a roles privilegiados.
