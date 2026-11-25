# Design — `validar-acceso-keycloak-s1` (RF-37 + PoC ADR-09)

## 1. Contexto, agregados DDD y ADRs aplicados

- **Historias de usuario:** `HU-37.1` (11 escenarios) y `HU-37.2` (12 escenarios) en `docs/requisitos/historias-de-usuario/area-7-administracion-general.md`, más sus reglas técnicas en `anexo-a-reglas-tecnicas.md` y preguntas abiertas en `anexo-b-preguntas-abiertas.md`.
- **Modelo de dominio (`DDD_v3.3.md` §13 — Identidad y Acceso):**
  - **Raíces de agregado (todas heredan `RaizDeAgregado<Guid>` con `Version` explícita ADR-25):**
    - `Usuario`: `Id`, `Cedula` (única R1), `NombreCompleto`, `Contacto` (`Correo`, `Telefono`), `CanalPreferido` (`CanalCodigo`: `CORREO`/`SMS`), `Estado` (`EstadoUsuario`: `ACTIVO`/`DESACTIVADO`), `Tipos` (`Set<Guid>` de `TipoUsuarioId`), `ProgramasAsignados` (`Set<Guid>`), `UniversidadId` (`Guid?`, para enlace universitario), `EsEvaluadorExterno` (`bool`, R4f), `KeycloakUserId` (`string?`), `Bloqueado2FaHastaUtc` (`DateTimeOffset?`). Nunca se borra, solo se desactiva.
    - `TipoUsuario`: `Id`, `Nombre` (único, insensible a mayúsculas/espacios), `Permisos` (`Set<string>`), `AccesoFinancieroODecision` (`bool`), `EsEnlaceUniversidad` (`bool`), `EsPredefinido` (`bool`). Método `OtorgarPermiso(permiso)` y `ExigeSegundoFactor()` (resuelve dinámicamente si el tipo incluye permisos financieros/decisión o es enlace universitario, sin listas rígidas en código).
    - `ClienteApiUniversidad`: `Id`, `UniversidadId`, `ClientIdKeycloak`, `Activo`, `PuedeEnviarPor(universidadId)` y `Revocar()`.
  - **Entidad de estado de autenticación 2FA en S1:**
    - `DesafioSegundoFactor` (`RaizDeAgregado<Guid>` con `Version` explícita para impedir carreras de validación): `Id`, `UsuarioId`, `Canal` (`CanalCodigo`), `CodigoHash` (SHA-256 + salt, nunca texto plano), `Salt`, `CreadoEnUtc`, `ExpiraEnUtc` (`+15 min`, R4b), `IntentosFallidos`, `ConsumidoEnUtc`, `InvalidadoEnUtc`, `EsRecuperacionCorreo`.
  - **Eventos de dominio:** `UsuarioRegistradoEvento` (`IEventoDeDominio`, DDD-11; al registrar postulantes dispara `UsuarioRegistradoV1` vía Outbox para S3 y S6).
  - **Servicios de dominio:**
    - `CatalogoPermisos`: catálogo único de permisos del sistema (`usuarios.gestionar`, `tipos_usuario.gestionar`, `rubricas.modificar`, `evaluaciones.puntuar`, `evaluaciones.ver_ajenas_post_cierre`, `datos_socioeconomicos.ver`, `finanzas.consultar`, `finanzas.desembolsar`, `becas.decidir`, `consultas.responder`, `reportes.institucional`, `registro_decisiones.consultar`, etc.) y clasificación de qué permisos implican acceso financiero o de decisión.
    - `MotorAutorizacionS1`: motor centralizado que evalúa **(a) rol/permisos configurables**, **(b) recurso/alcance** (programa o universidad), **(c) condición de estado** (puntuaciones de otros evaluadores ocultas mientras el periodo de revisión esté abierto, HU-37.1 E4 / S9) y **(d) masking en backend** (eliminación de campos socioeconómicos en respuestas para roles sin `datos_socioeconomicos.ver`, HU-37.1 E6).
- **ADRs aplicados:**
  - **ADR-01 / ADR-02 / ADR-04:** microservicio `Ascenso.S1.Identidad` con Clean Architecture estricta en 4 proyectos (`Domain`, `Application`, `Infrastructure`, `Api`) + 3 proyectos de pruebas (`UnitTests`, `IntegrationTests`, `ArchitectureTests`).
  - **ADR-05:** `IdentidadDbContext` sobre el schema `s1_identidad` de PostgreSQL; ningún otro servicio accede a este schema.
  - **ADR-09 (cierre de PoC / pendiente `D4`):** Keycloak 26 gestiona credenciales primarias, OIDC y `client_credentials` interservicios, mientras **S1 controla la puerta del segundo factor por correo o SMS** (R3/R4) porque TOTP de app autenticadora no satisface R4 (`ARQUITECTURA_V1.0.md` §8.1). La emisión de sesión definitiva queda bloqueada hasta validar el `DesafioSegundoFactor` en S1.
  - **ADR-11:** autorización centralizada en backend + masking a nivel de respuesta.
  - **ADR-12 / D-6:** toda acción administrativa sensible en S1 (creación/desactivación de usuarios, creación de tipos de usuario, cambio de rol) persiste con `UnidadAtomicaDecisiones` + `AppendOnlyRegistroDecisionInterceptor` en `s1_identidad.registro_decisiones` + `outbox_messages`.
  - **ADR-16 / ADR-25:** `Version` explícita con `IsConcurrencyToken()` en todas las raíces.
  - **ADR-17 / R20:** todo cálculo de vigencia (15 min) y bloqueo (1 hora) usa `IRelojNegocio` inyectado.
  - **ADR-27:** `MediatR 12.5.0` en `Ascenso.S1.Identidad.Application`.

## 2. Arquitectura del flujo 2FA con Keycloak (`ADR-09` / `ARQUITECTURA_V1.0.md` §8.1)

```text
Cliente/BFF (Next.js)
   │ 1. POST /api/v1/auth/login (correo/cédula + contraseña)
   ▼
Ascenso.S1.Identidad.Api ──▶ IniciarSesionPaso1Handler
   │ 2. Valida contraseña contra Keycloak (IProveedorIdentidadKeycloak -> realm ascenso)
   │    [Si falla contraseña -> 401 sin pedir ni enviar 2FA (HU-37.2 E6)]
   │ 3. Carga Usuario + TipoUsuario desde s1_identidad (fuente de verdad de roles/alcance)
   │ 4. Evalúa Usuario.RequiereSegundoFactor(tipos):
   │    - Si NO requiere -> abre sesión y devuelve tokens.
   │    - Si SÍ requiere (coordinador, finanzas, dirección, evaluador, enlace, o tipo con acceso financiero/decisión):
   │      a) Verifica que Usuario no esté bloqueado por 3 intentos fallidos (1 hora, E10).
   │      b) Resuelve canal: si EsEvaluadorExterno -> CORREO (R4f, E3); sino CanalPreferido (CORREO o SMS, E8).
   │      c) Crea DesafioSegundoFactor (código 6 dígitos hasheado, ExpiraEnUtc = ahora + 15 min, R4b).
   │      d) Envía código por ICanalEnvioCodigo (SMTP Mailpit / SMS).
   │         [Si falla el envío -> NO abre sesión, devuelve 503 y habilita reenvío (E5)].
   │      e) Responde 202 { requiereSegundoFactor: true, desafioId, canalEnviado, expiraEnUtc } (SIN token de sesión, E7).
   │
   │ 5. POST /api/v1/auth/2fa/verificar (desafioId + codigo)
   ▼
VerificarSegundoFactorHandler
   │ 6. Valida desafioId, vigencia <= 15 min (E9), no consumido, sin bloqueo 1h (E10), hash constante.
   │    - Si falla intento 1 o 2 -> incrementa IntentosFallidos, 401 "El segundo factor no es válido." (E2).
   │    - Si falla intento 3 -> marca Usuario.BloquearSegundoFactorHasta(ahora + 1h), 429/423 (E10).
   │    - Si válido -> consume desafío atómicamente (Version++) y emite sesión verificada 2FA con permisos del rol (E1).
```

### PoC ejecutable de Keycloak + Mailpit (`deploy/keycloak/realm-ascenso.json`)
- Se crea la configuración declarativa `deploy/keycloak/realm-ascenso.json` importada automáticamente en `deploy/docker-compose.yml` (`start-dev --import-realm`):
  - Realm `ascenso` con servidor SMTP apuntando al contenedor `mailpit:1025`.
  - Cliente confidencial `ascenso-s1-identidad` para validación de credenciales y administración de usuarios.
  - Clientes `client_credentials` internos para la lista cerrada interservicios (`ascenso-s2-fondos`, `ascenso-s3-postulacion`, `ascenso-s4-becas`) con los scopes exactos de `contracts/openapi-interno.yaml` (`fondos.reservar`, `fondos.liberar`, `fondos.consultar`, `ofertas.consultar-vinculo`, `otp.consumir`, `beneficios.consultar`).
- Todas las URLs y secretos de conexión se leen desde `IConfiguration` / variables de entorno (`Keycloak__BaseUrl`, `ConnectionStrings__IdentidadDb`, `Smtp__Host`), sin hardcodear `localhost` en código para asegurar portabilidad a cualquier servidor.

## 3. Contratos públicos de S1 a incorporar en `contracts/openapi.yaml`

Siguiendo `EVENTOS_E_INTERFACES.md` (*añadir endpoints por HU sin alterar límites arquitectónicos*) y `openspec/config.yaml`, se documentan en `contracts/openapi.yaml` las rutas públicas de S1 bajo tag `S1`:
- `POST /api/v1/tipos-usuario` y `GET /api/v1/tipos-usuario` (solo Dirección; alta configurable de tipos y permisos, HU-37.1 E3/E7/E9/E11).
- `POST /api/v1/usuarios`, `GET /api/v1/usuarios` y `PUT /api/v1/usuarios/{usuarioId}/roles` (solo Dirección; alta de usuarios internos/externos y bloqueo de auto-escalamiento, HU-37.1 E1/E2/E8/E10).
- `POST /api/v1/auth/login` (paso 1 credenciales + disparo de 2FA, HU-37.2 E1/E3–E8/E11).
- `POST /api/v1/auth/2fa/verificar` (paso 2 validación de código 2FA, HU-37.2 E1/E2/E9/E10).
- `POST /api/v1/auth/2fa/reenviar` (reenvío del código tras fallo o vencimiento, HU-37.2 E5/E9).
- `POST /api/v1/auth/2fa/recuperar` (recuperación por correo al perder el medio SMS, HU-37.2 E12).

## 4. Eventos de integración

- `UsuarioRegistrado.v1` (ya existente en `Ascenso.Contracts`, productor `S1`, consumidores `S3`, `S6`): publicado vía Outbox cuando S1 registra cuentas de postulante (flujo `16.4f` / `ADR-30`).
- `DecisionRegistrada.v1` (ya existente en `Ascenso.Contracts`, productor `S1`, consumidor `S6`): publicado vía Outbox cuando se registran cambios administrativos auditables (como cambios de rol o desactivación).
- No se agregan nuevos eventos de integración en `Ascenso.Contracts`.

## 5. Decisiones cerradas del equipo (25-nov-2026) y preguntas residuales (Anexo B)

Las preguntas operativas de `HU-37.1` y `HU-37.2` que condicionaban el modelo de `S1` quedan cerradas así (y registradas en `docs/arquitectura/pendientes-v1.3.md`):
- **Código 2FA de un solo uso (`HU-37.2 #1`):** se rechaza cualquier reuso de un código ya consumido aunque no haya vencido. La vigencia se mantiene en **15 minutos** por defecto conforme a la respuesta fijada por el cliente (`R4b` y `HU-37.2 Escenario 9`), parametrizada en configuración (`Autenticacion2FaOptions.VigenciaMinutos`, por defecto `15`).
- **Reenvío invalida el código anterior (`HU-37.2 #2`):** al emitir un reenvío se invalida inmediatamente cualquier desafío activo previo del usuario (`InvalidadoEnUtc`).
- **Cuenta del enlace de universidad aliada (`HU-37.1 #7`):** la crea **Dirección** dentro de `HU-37.1`, sujeta obligatoriamente a 2FA (`R3`).
- **Tipo nuevo sin acceso financiero ni de decisión (`HU-37.1 #5`):** queda exento de 2FA (`ExigeSegundoFactor = false`), apegado a la regla literal de `HU-37.1`.
- **Integridad de tipos en uso y offboarding (`HU-37.1 #2 y #3`):** `Usuario` se desactiva y nunca se borra (`DDD §13`); un `TipoUsuario` con usuarios asignados no puede eliminarse.
- **Cambio de permisos o roles (`HU-37.1 #4`):** exige **nuevo inicio de sesión obligatorio** (se rota `SelloSeguridad` en S1 y se revocan las sesiones abiertas del usuario en Keycloak para que los permisos actualizados entren en vigor en el siguiente login).
- **Catálogo base de permisos (`HU-37.1 #1`):** se siembra un catálogo inicial configurable en `CatalogoPermisos` derivado de las 73 HU (editable por datos sin redesplegar).
- **Nota residual (`HU-37.2 #3`, riesgo R4e):** se conserva documentada la observación de seguridad de la recuperación por correo por si el cliente solicita posteriormente una verificación adicional.
