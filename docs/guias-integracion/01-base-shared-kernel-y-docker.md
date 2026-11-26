# 01 · Base del Monorepo, `building-blocks/` y Entorno Docker

Esta guía cubre todo lo que necesitas para compilar, levantar infraestructura local y construir un microservicio `.NET 10` compatible con el resto del equipo.

---

## 1. Estructura de la solución (`Ascenso.sln`)

- **`Directory.Build.props`**: fija `net10.0`, `Nullable=enable`, `ImplicitUsings=enable` y **`TreatWarningsAsErrors=true`** (toda advertencia del compilador rompe el build).
- **`Directory.Packages.props`**: administración central de paquetes (CPM). **Nunca pongas atributo `Version="..."` en un `.csproj`**; usa `<PackageReference Include="..." />`.
  - `MediatR` está fijado en **`12.5.0`** (Apache 2.0, `ADR-27`).
  - Para pruebas con SQLite real usa **`Microsoft.EntityFrameworkCore.Sqlite.Core`** + **`SQLitePCLRaw.bundle_e_sqlite3`** (`3.0.2`). No agregues `Microsoft.EntityFrameworkCore.Sqlite` porque arrastra `e_sqlite3 2.1.11` con vulnerabilidad `NU1903`.

### Comandos diarios de verificación
```bash
dotnet build Ascenso.sln --configuration Release
dotnet test Ascenso.sln --configuration Release --no-build
openspec validate --all --strict --no-interactive
```

---

## 2. Levantar el entorno local con Docker (`deploy/docker-compose.yml`)

Desde `deploy/`:
```bash
docker compose up -d
```

Servicios disponibles por defecto (configurables por variables de entorno):

| Servicio | Contenedor | Puerto | Credenciales locales por defecto |
|---|---|---|---|
| **PostgreSQL 16** | `ascenso-postgres` | `5432` | DB `ascenso` · admin `ascenso_admin` / `dev_change_me` |
| **MongoDB 7** | `ascenso-mongodb` | `27017` | DB `ascenso_lectura` · `ascenso_mongo` / `dev_change_me` |
| **RabbitMQ 3.13** | `ascenso-rabbitmq` | `5672` / UI `15672` | `ascenso` / `dev_change_me` |
| **Keycloak 26** | `ascenso-keycloak` | `8085` | Admin `admin` / `dev_change_me` · Realm importado automáticamente: **`ascenso`** |
| **Mailpit** | `ascenso-mailpit` | SMTP `1025` / UI `8025` | Captura todos los correos locales (2FA, OTP, notificaciones) en `http://localhost:8025` |

### Schemas y roles PostgreSQL (`deploy/postgres/init-schemas.sql`)

Cada microservicio tiene **su propio schema y dos roles** (migrador y runtime) con aislamiento estricto (`ADR-05`):

| Servicio | Schema exacto | Rol migrador (local) | Rol runtime (local) |
|---|---|---|---|
| S1 Identidad | `s1_identidad` | `s1_identidad_migrator` (`dev_s1_identidad_migrator`) | `s1_identidad_app` (`dev_s1_identidad_app`) |
| S2 Catálogo y Fondos | `s2_catalogo_fondos` | `s2_catalogo_fondos_migrator` (`dev_s2_catalogo_fondos_migrator`) | `s2_catalogo_fondos_app` (`dev_s2_catalogo_fondos_app`) |
| S3 Postulación y Evaluación | `s3_postulacion_evaluacion` | `s3_postulacion_evaluacion_migrator` (`dev_s3_postulacion_evaluacion_migrator`) | `s3_postulacion_evaluacion_app` (`dev_s3_postulacion_evaluacion_app`) |
| S4 Gestión de Becas | **`s4_becas`** | `s4_becas_migrator` (`dev_s4_becas_migrator`) | `s4_becas_app` (`dev_s4_becas_app`) |
| S5 Seguimiento Académico | `s5_seguimiento` | `s5_seguimiento_migrator` (`dev_s5_seguimiento_migrator`) | `s5_seguimiento_app` (`dev_s5_seguimiento_app`) |
| S6 Comunicaciones y Reportería | `s6_comunicaciones` | `s6_comunicaciones_migrator` (`dev_s6_comunicaciones_migrator`) | `s6_comunicaciones_app` (`dev_s6_comunicaciones_app`) |

> **Importante para tu `DbContext`:** usa exactamente el nombre de la columna **Schema exacto** y aplícalo solo para PostgreSQL:
> ```csharp
> if (Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
> {
>     modelBuilder.HasDefaultSchema("s4_becas"); // cambia al schema de tu servicio
> }
> ```

---

## 3. Qué ya tienes listo en `building-blocks/`

### A. `Ascenso.SharedKernel` (solo lo referencia tu `*.Domain` y `*.Application`)
- **Tipos base (`Ascenso.SharedKernel.TiposBase`):**
  - `Entidad<TId>`: igualdad por identidad y registro de eventos de dominio con `Registrar(IEventoDeDominio)` / `EventosPendientes` / `LimpiarEventos()`.
  - `RaizDeAgregado<TId>`: añade `int Version` y `protected void IncrementarVersion()` (`ADR-25`, `DDD-07`). **Toda raíz de agregado debe heredar de `RaizDeAgregado<Guid>`** e incrementar `Version` incluso al modificar entidades hijas.
  - `ExcepcionDeDominio(string mensaje, string codigo)`: permite mapear errores de dominio a `ProblemDetails` RFC 9457 de forma uniforme.
  - `IEventoDeDominio : INotification`: contrato para eventos internos con `DateTimeOffset Fecha`.
- **Objetos de valor (`Ascenso.SharedKernel.ValueObjects`):**
  - `CodigoPeriodo` (ej. `"2026-2"`, normalizado a mayúsculas, máx. 20 chars).
  - `Cedula` (normaliza formato venezolano `V-` / `E-`).
  - `Dinero` (`Cantidad`, `Moneda` ISO-3, suma/resta segura en misma moneda).
  - `DatosContacto` (`Correo`, `Telefono`), `Justificacion`, `RangoFechas`, `DocumentoAdjuntoRef`, `Porcentaje`, `EstadoMatriculaClasificado` (`ADR-29`).
- **Reloj de negocio (`Ascenso.SharedKernel.Tiempo`, `ADR-17`, `R20`):**
  - Inyecta siempre `IRelojNegocio` (implementación `RelojSistemaCaracas`). Nunca uses `DateTime.Now`.
  - Helpers útiles: `RelojSistemaCaracas.InicioDeDiaCaracas(DateOnly)` (`00:00` UTC-4) y `RelojSistemaCaracas.FinDeDiaCaracas(DateOnly)` (`23:59:59.999` UTC-4).
- **Seguridad OTP (`Ascenso.SharedKernel.Seguridad`):**
  - `TipoOperacionSensible` (las 5 operaciones OTP), `CanalCodigo` (`CORREO`, `SMS`), `IServicioOtp`.
- **Trazabilidad (`Ascenso.SharedKernel.Trazabilidad`):** ver [Guía 02](02-registro-decisiones-trazabilidad.md).

### B. `Ascenso.Contracts` (eventos de integración versionados v1)
- `IntegrationEventEnvelope<TPayload>`: sobre estándar con `EventId`, `EventType`, `SchemaVersion`, `AggregateId`, `OccurredAtUtc`, `Producer`, `CorrelationId`, `CausationId`, `Payload`.
- Eventos ya definidos por contexto:
  - `EventosIdentidadV1.cs` (`UsuarioRegistradoV1`, etc.)
  - `EventosCatalogoFondosV1.cs` (`PeriodoMarcadoActualV1`, `CostoMatriculaVigenteV1`)
  - `EventosPostulacionV1.cs`, `EventosBecasV1.cs`, `EventosSeguimientoV1.cs`, `EventosTrazabilidadV1.cs` (`DecisionRegistradaV1`).

### C. `Ascenso.Messaging.Outbox` (solo lo referencia tu `*.Infrastructure`)
En `OnModelCreating` de tu `DbContext` añade estas tres líneas:
```csharp
modelBuilder.ConfigurarOutboxEInbox();        // tablas outbox_messages e inbox_messages
modelBuilder.ConfigurarRegistroDecisiones();  // tabla registro_decisiones (RF-35)
builder.ConfigurarVersionExplicita<MiRaiz, Guid>(); // IsConcurrencyToken() en cada raíz (ADR-25)
```
Y al registrar el `DbContext` en `DependencyInjection.cs`:
```csharp
services.AddSingleton<AppendOnlyRegistroDecisionInterceptor>();
services.AddDbContext<MiDbContext>((sp, options) =>
{
    options.AddInterceptors(sp.GetRequiredService<AppendOnlyRegistroDecisionInterceptor>());
    // ... UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", Schema))
});
```

---

## 4. Plantilla de referencia ya validada

Si vas a crear `S3`, `S5`, `S6` o ampliar `S4`, toma como referencia directa cualquiera de los dos servicios ya en `main`:
- `services/Ascenso.S2.CatalogoFondos/` (ejemplo más compacto y limpio para arrancar).
- `services/Ascenso.S1.Identidad/` (ejemplo completo con HTTP externo a Keycloak, SMTP y frontera privada `/internal/v1/...`).
