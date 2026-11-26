# Design

## Context

Véase `proposal.md` para la motivación y los seis hallazgos de revisión tras la integración de `S1` y `S2` en `main`. En `deploy/postgres/init-schemas.sql` (ADR-05), el schema asignado al microservicio S4 es `s4_becas` (con roles `s4_becas_migrator` y `s4_becas_app`). Asimismo, `S2` (`CatalogoFondosDbContext.cs` y `DependencyInjection.cs`) estableció la convención del repositorio para condicionar `HasDefaultSchema(...)` a Npgsql, configurar `MigrationsHistoryTable` en el schema propio y leer `ConnectionStrings:<ServicioDb>` con fallback opcional para pruebas.

## Goals / Non-Goals

**Goals:**
- Unificar `Ascenso.sln` partiendo de la versión limpia de `main` (con `S1` y `S2`) y re-registrando los 7 proyectos de `S4`.
- Alinear `BecaDbContext` y la migración `20261125183000_InicialS4Becas` al schema `s4_becas`, condicionando `HasDefaultSchema("s4_becas")` a Npgsql para mantener compatibilidad con SQLite en pruebas.
- Dejar `Ascenso.S4.Becas.Api` ejecutable en runtime leyendo `ConnectionStrings:BecasDb` con `UseNpgsql` y registrando `AddAuthentication()` y `AddAuthorization()`.
- Habilitar el rol `DIRECCION` en `PoliticaAccesoHistorialBecas` (Domain) y `HistorialBecasController` (Api).
- Filtrar `ListarPorPeriodoAsync` en SQL evitando la carga completa de la tabla en memoria.
- Corregir `contracts/openapi.yaml` a sintaxis OpenAPI 3.1 (`type: ['string', 'null']`) y mantener ordenada la documentación `INTEGRACION_EQUIPO.md` en esta rama.

**Non-Goals:**
- Habilitar `FINANZAS` o `Evaluador del comité` (permanecen abiertos según Anexo B `HU-18.1` #1 y #2 hasta confirmación del cliente).
- Implementar otros RF de S4 (`RF-15`, `RF-16`, `RF-17`, `RF-19`, `RF-20`) o consumidores reales de eventos de S1/S2.
- Modificar `S1`, `S2`, `S3`, `S5`, `S6` o los proyectos de `building-blocks/`.

## Decisions

### 1. Schema `s4_becas` condicionado a Npgsql y `AddS4Infrastructure` configurable
- **Decisión:** Definir `public const string Schema = "s4_becas";` en `BecaDbContext`, aplicando `modelBuilder.HasDefaultSchema(Schema);` únicamente cuando `Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true` (mismo patrón que `CatalogoFondosDbContext`). Actualizar todas las tablas e índices de la migración `20261125183000_InicialS4Becas` al schema `s4_becas`. Agregar `<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" />` (ya declarado en `Directory.Packages.props`) a `Ascenso.S4.Becas.Infrastructure.csproj` y actualizar `AddS4Infrastructure(IConfiguration configuration, Action<DbContextOptionsBuilder>? configurarDbContext = null)` para usar `UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", BecaDbContext.Schema))` cuando exista `ConnectionStrings:BecasDb`.
- **Alternativa considerada:** Mantener `"s4"` o aplicar `HasDefaultSchema` incondicionalmente. **Descartada** porque falla contra `deploy/postgres/init-schemas.sql` por permisos/schema y ensucia tablas en SQLite.

### 2. Arranque en runtime de `Ascenso.S4.Becas.Api`
- **Decisión:** En `Program.cs`, registrar `builder.Services.AddAuthentication();` y `builder.Services.AddAuthorization();` antes de `builder.Build()` y pasar `builder.Configuration` a `AddS4Infrastructure(builder.Configuration)`. De este modo el servicio arranca correctamente fuera de pruebas y mantiene compatibilidad con `WebApplicationFactory<Program>`.
- **Alternativa considerada:** Mantener el registro de autenticación únicamente en `HistorialBecasApiTests`. **Descartada** porque provoca fallo en runtime al llamar `UseAuthentication()` / `UseAuthorization()`.

### 3. Habilitación del rol `DIRECCION` en dominio y controlador
- **Decisión:** Agregar `Direccion` a `RolHistorialBecas` en `Ascenso.S4.Becas.Domain`, actualizar `PoliticaAccesoHistorialBecas.PuedeConsultarPorCedula` y `PuedeConsultarAgregado` para autorizar a `Coordinador` y `Direccion`, y mapear `"DIRECCION"` en `HistorialBecasController.ResolverRol`.
- **Alternativa considerada:** Mapear `"DIRECCION"` como `"COORDINADOR"` en el controlador. **Descartada** para preservar la semántica del rol en dominio y pruebas unitarias.

### 4. Filtrado en SQL de `ListarPorPeriodoAsync`
- **Decisión:** Reemplazar la carga completa en `ConsultaHistorialBecasEfCore.ListarPorPeriodoAsync` por:
  ```csharp
  var codigo = new CodigoPeriodo(periodoNormalizado);
  var filas = await (
      from b in _db.Becas.AsNoTracking().Include(x => x.Tramos)
      join p in _db.ReferenciasPersonas.AsNoTracking() on b.PostulanteId equals p.PersonaId
      join pr in _db.ReferenciasProgramas.AsNoTracking() on b.ProgramaId equals pr.ProgramaId
      where b.Tramos.Any(t => t.Periodo == codigo)
      select new { Beca = b, Persona = p, Programa = pr })
      .ToListAsync(cancellationToken);
  ```
- **Alternativa considerada:** Crear una consulta separada sobre `TramosBeca` sin `Beca`. **Descartada** porque la proyección por `Beca` con `Any` traduce directamente a SQL `EXISTS` manteniendo intacto el mapeo de tramos del periodo solicitado.

### 5. `Ascenso.sln`, `contracts/openapi.yaml` y orden de rama
- **Decisión:** Restaurar `Ascenso.sln` desde `main` (`git checkout origin/main -- Ascenso.sln`) y ejecutar `dotnet sln Ascenso.sln add ... --solution-folder services/Ascenso.S4.Becas` para los 7 proyectos de S4. En `contracts/openapi.yaml`, reemplazar `nullable: true` en `HistorialBecasResponse.mensaje` por `type: ['string', 'null']` (OpenAPI 3.1.0). Confirmar que `services/Ascenso.S4.Becas/INTEGRACION_EQUIPO.md` permanezca en esta rama (actualizado con el ajuste de `s4_becas` y `DIRECCION`) y no se mezcle `docs/ASCENSO — División de requerimientos por integrante.md`.

## Risks / Trade-offs

- **[Cambiar `HasDefaultSchema` solo para Npgsql requiere validar compatibilidad de `CodigoPeriodo` en SQLite/Npgsql]** → Cubrir la consulta `ListarPorPeriodoAsync` con prueba de integración EF Core/SQLite verificando que el filtro `.Where(b => b.Tramos.Any(t => t.Periodo == codigo))` traduce y retorna exactamente el periodo pedido.
- **[Configurar `AddAuthentication()` sin esquema por defecto en `Program.cs` hasta conectar Keycloak]** → La configuración de JWT/Keycloak entra en la integración con S1; mientras tanto, registrar `AddAuthentication()` y `AddAuthorization()` evita excepciones en runtime y las pruebas de integración siguen sobrescribiendo el esquema de pruebas.

## Open Questions

- **Anexo B — `HU-18.1` #1 y #2:** el alcance de acceso directo para **Finanzas** y **Evaluador del comité** sigue abierto hasta definición del cliente/equipo.
