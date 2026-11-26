# Guías de Integración y Desarrollo — Lo ya construido en `main`

Esta carpeta explica de forma práctica y directa **cómo usar y conectarse con todo lo que ya está implementado en `main`**, para que cualquier integrante del equipo (`S3`, `S4`, `S5`, `S6` o `frontend/`) pueda programar sobre una base sólida sin adivinar contratos, schemas ni patrones.

---

## Mapa rápido de lo implementado en `main`

| Módulo / Servicio | Qué resuelve | Estado en `main` | Guía detallada |
|---|---|---|---|
| **Base del monorepo + `building-blocks/` + `deploy/`** | `Ascenso.sln` (.NET 10), `Ascenso.SharedKernel`, `Ascenso.Contracts`, `Ascenso.Messaging.Outbox`, `docker-compose` (Postgres, Mongo, RabbitMQ, Keycloak, Mailpit), CI y guardrails de PR | Listo (35 pruebas en verde) | [01 · Base, SharedKernel y Docker](01-base-shared-kernel-y-docker.md) |
| **Trazabilidad transversal (`RF-35`, `HU-35.1` / `HU-35.2`)** | `RegistroDecision` inmutable (append-only), transacción atómica `UnidadAtomicaDecisiones`, evento `DecisionRegistradaV1`, trigger SQL y consulta filtrada por rol | Listo (transversal para `S1`–`S6`) | [02 · Registro de Decisiones (RF-35)](02-registro-decisiones-trazabilidad.md) |
| **`S1` Identidad y Acceso (`RF-37`, `ADR-09`, `ADR-10`)** | Tipos de usuario configurables, gestión de usuarios, login Keycloak + 2FA por correo/SMS y servicio de códigos **OTP de un solo uso** (`POST /api/v1/otp/challenges` + frontera privada `POST /internal/v1/otp/validar-y-consumir`) | Listo (20 pruebas + PoC vivo Docker) | [03 · S1 Identidad, Keycloak, 2FA y OTP](03-s1-identidad-keycloak-2fa-y-otp.md) |
| **`S2` Catálogo y Fondos (`RF-38`, `RF-14`, `RF-11`)** | Calendario académico (`PeriodoMarcadoActual.v1`), universidades/convenios/costos append-only (`CostoMatriculaVigente.v1`) y programas de becas (monto fijo, modalidad, alcance, elegibilidad y condiciones de permanencia/pausa) | Listo (35 pruebas en verde) | [04 · S2 Calendario, Universidades y Programas](04-s2-catalogo-fondos-calendario.md) |

> **Verificación vigente:** `dotnet build Ascenso.sln --configuration Release` (**0 advertencias, 0 errores**, 18 proyectos) · `dotnet test Ascenso.sln --configuration Release` (**90/90 pruebas aprobadas**) · `openspec validate --all --strict` (**6 specs activas en verde**).

---

## Qué debes leer según tu servicio

| Si trabajas en… | Lee primero | Integraciones clave que ya puedes consumir |
|---|---|---|
| **`S3` Postulación y Evaluación** (Daniel / Diego) | [Guía 01](01-base-shared-kernel-y-docker.md) · [Guía 02](02-registro-decisiones-trazabilidad.md) · [Guía 04](04-s2-catalogo-fondos-calendario.md) | Schema `s3_postulacion_evaluacion`, `CodigoPeriodo`, `Cedula`, `IRelojNegocio`, `UnidadAtomicaDecisiones`, `CalendarioAcademico` de `S2` |
| **`S4` Gestión de Becas** (Leonel / Sebas) | [Guía 01](01-base-shared-kernel-y-docker.md) · [Guía 02](02-registro-decisiones-trazabilidad.md) · [Guía 03](03-s1-identidad-keycloak-2fa-y-otp.md) · [Guía 04](04-s2-catalogo-fondos-calendario.md) | Schema **`s4_becas`** (no `s4`), frontera privada `POST /internal/v1/otp/validar-y-consumir` (token `ascenso-s4-becas` con scope `otp.consumir`), evento `PeriodoMarcadoActual.v1` de `S2` |
| **`S5` Seguimiento Académico** | [Guía 01](01-base-shared-kernel-y-docker.md) · [Guía 02](02-registro-decisiones-trazabilidad.md) · [Guía 03](03-s1-identidad-keycloak-2fa-y-otp.md) | Schema `s5_seguimiento`, `IRelojNegocio` (UTC-4), `EstadoMatricula`, clientes API universitarios en Keycloak (`S1`) |
| **`S6` Comunicaciones y Reportería** | [Guía 01](01-base-shared-kernel-y-docker.md) · [Guía 02](02-registro-decisiones-trazabilidad.md) · [Guía 04](04-s2-catalogo-fondos-calendario.md) | Schema `s6_comunicaciones`, proyección de `DecisionRegistradaV1` y `PeriodoMarcadoActual.v1`, MongoDB en `deploy/docker-compose.yml` |
| **`frontend/` (Next.js BFF, `ADR-28`)** | [Guía 03](03-s1-identidad-keycloak-2fa-y-otp.md) · [Guía 04](04-s2-catalogo-fondos-calendario.md) | Login + 2FA, solicitud de desafío OTP (`POST /api/v1/otp/challenges`), calendario académico (`GET/POST /api/v1/periodos`) |

---

## Reglas no negociables del repositorio (evitan fallos en CI y conflictos)

1. **Nunca subir ni hacer commit directo a `main`:** siempre crear una rama (`feat/...`, `fix/...`, `docs/...`), abrir Pull Request y pasar los checks (`dotnet` + `Guardrails de PR`).
2. **Todo cambio de código o contratos exige OpenSpec (`ADR-23`):** el workflow `Guardrails de PR` rechaza PRs que toquen `services/`, `building-blocks/`, `contracts/`, `gateway/` o `frontend/` sin actualizar `openspec/`.
3. **Nombres exactos de schemas en PostgreSQL (`deploy/postgres/init-schemas.sql`):**
   - `s1_identidad` · `s2_catalogo_fondos` · `s3_postulacion_evaluacion` · `s4_becas` · `s5_seguimiento` · `s6_comunicaciones`
   - En EF Core, aplica `HasDefaultSchema(...)` **solo cuando el proveedor es Npgsql** (para no romper pruebas SQLite).
4. **`MediatR` fijado en `12.5.0` (`ADR-27`):** prohibido actualizar a `>= 13.0.0` (licencia comercial; CI lo bloquea).
5. **Pruebas SQLite sin vulnerabilidades (`NU1903`):** en proyectos `*IntegrationTests.csproj`, usa `Microsoft.EntityFrameworkCore.Sqlite.Core` + `SQLitePCLRaw.bundle_e_sqlite3` (ambos ya definidos en `Directory.Packages.props`) e invoca `SQLitePCL.Batteries_V2.Init();` antes de abrir la conexión.
6. **Cero `localhost` hardcodeado en código C#:** toda cadena de conexión o URL externa se lee de `IConfiguration` (`appsettings.json` deja cadenas vacías por defecto).
7. **Pruebas de arquitectura obligatorias (Clase 09):** cada microservicio incluye `tests/*.ArchitectureTests` verificando que `*.Domain` no referencia EF Core, ASP.NET, Npgsql, MassTransit, MongoDB, Firebase ni capas superiores.
