# Especificación: Migraciones EF Core Iniciales y Sembrado Base (ADR-05, RF-35)

## Purpose

Proveer migraciones iniciales de Entity Framework Core 10 versionadas para `S1`, `S2` y `S6`, con fábricas de tiempo de diseño, endurecimiento append-only de `registro_decisiones` (`RF-35`) directamente en la migración inicial, sembrador idempotente de tipos de usuario predefinidos y scripts de migración para PostgreSQL.

## Requirements

### Requirement: Migraciones iniciales aisladas por schema con trigger append-only RF-35

Cada servicio (`S1`, `S2`, `S6`) MUST incluir su migración `InitialCreate` en `Persistencia/Migrations/`, su `IDesignTimeDbContextFactory` sin direcciones hardcodeadas, la tabla de historial `__ef_migrations_history` dentro de su propio schema (`ADR-05`) y la ejecución de `RegistroDecisionSqlAppendOnly.GenerarSqlPostgreSql(...)` dentro de `Up(...)` para endurecer `registro_decisiones` en el motor de base de datos (`RF-35`).

#### Scenario: Aplicacion de migraciones con rol migrator deja tablas y trigger activos

- **WHEN** se ejecuta `dotnet ef database update` (o `deploy/migrar-bd.ps1` / `deploy/migrar-bd.sh`) contra PostgreSQL inicializado con `deploy/postgres/init-schemas.sql`
- **THEN** cada servicio crea todas sus tablas dentro de su schema exclusivo (`s1_identidad`, `s2_catalogo_fondos`, `s6_comunicaciones`)
- **AND** la tabla `registro_decisiones` queda protegida con `REVOKE UPDATE, DELETE, TRUNCATE` al rol `*_app` y trigger `BEFORE UPDATE OR DELETE`

### Requirement: Sembrado idempotente de tipos de usuario predefinidos en S1

`S1` MUST proveer `SembradorIdentidadDesarrollo.SembrarTiposPredefinidosAsync` para poblar los 7 tipos de usuario predefinidos (`DIRECCION`, `COORDINADOR`, `FINANZAS`, `COMITE`, `ENLACE_UNIVERSIDAD`, `POSTULANTE`, `BECARIO`) de forma idempotente sin duplicar ni alterar tipos existentes (`HU-37.1`).

#### Scenario: Segunda ejecucion del sembrador no duplica tipos predefinidos

- **WHEN** `SembrarTiposPredefinidosAsync` se ejecuta dos veces sobre la misma base de datos
- **THEN** la primera ejecución inserta los 7 tipos predefinidos
- **AND** la segunda ejecución retorna `0` inserciones conservando intactos los registros existentes
