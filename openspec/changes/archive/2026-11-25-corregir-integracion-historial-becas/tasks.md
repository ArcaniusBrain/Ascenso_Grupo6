# Tasks

## 1. Domain

- [x] 1.1 Agregar `Direccion` a `RolHistorialBecas` y actualizar `PoliticaAccesoHistorialBecas.PuedeConsultarPorCedula` y `PuedeConsultarAgregado` en `services/Ascenso.S4.Becas/Ascenso.S4.Becas.Domain/Becas/PoliticaAccesoHistorialBecas.cs` para habilitar acceso completo de lectura a `Coordinador` y `Direccion` (manteniendo `FINANZAS` fuera hasta decisión del equipo). Verificación: compilación limpia de `Ascenso.S4.Becas.Domain`.
- [x] 1.2 Agregar pruebas unitarias en `services/Ascenso.S4.Becas/tests/Unit/PoliticaAccesoHistorialBecasTests.cs` validando que `Direccion` puede consultar por cédula y en búsquedas agregadas. Verificación: `dotnet test services/Ascenso.S4.Becas/tests/Unit` pasa al 100 %.

## 2. Application

- [x] 2.1 Agregar la prueba unitaria `DireccionConsultaHistorialPorPersonaYPorPrograma` (nombrando el escenario `Dirección consulta el historial por persona y por programa` de `specs/becas/spec.md`) en `services/Ascenso.S4.Becas/tests/Unit/ConsultarHistorialBecasHandlerTests.cs`. Verificación: `dotnet test services/Ascenso.S4.Becas/tests/Unit` pasa con todos los escenarios cubiertos.

## 3. Infrastructure

- [x] 3.1 Agregar `<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" />` a `services/Ascenso.S4.Becas/Ascenso.S4.Becas.Infrastructure/Ascenso.S4.Becas.Infrastructure.csproj`. Verificación: `dotnet restore` y compilación de `Ascenso.S4.Becas.Infrastructure` sin errores.
- [x] 3.2 Actualizar `BecaDbContext.cs` definiendo `public const string Schema = "s4_becas";` y condicionando `modelBuilder.HasDefaultSchema(Schema);` a `Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true`, y actualizar todos los esquemas de `"s4"` a `"s4_becas"` en la migración `services/Ascenso.S4.Becas/Ascenso.S4.Becas.Infrastructure/Migrations/20261125183000_InicialS4Becas.cs`. Verificación: inspección de `BecaDbContext` y la migración, y ejecución de pruebas contra SQLite sin errores de schema.
- [x] 3.3 Actualizar `services/Ascenso.S4.Becas/Ascenso.S4.Becas.Infrastructure/DependencyInjection.cs` para recibir `IConfiguration` y configurar `UseNpgsql` con `ConnectionStrings:BecasDb` y `MigrationsHistoryTable("__ef_migrations_history", BecaDbContext.Schema)` (con override opcional para pruebas, igual que `S2`). Verificación: compilación y ejecución de pruebas de integración.
- [x] 3.4 Optimizar `ConsultaHistorialBecasEfCore.ListarPorPeriodoAsync` filtrando en SQL mediante `Where(b => b.Tramos.Any(t => t.Periodo == codigo))` antes de `ToListAsync` y verificar en `ConsultaHistorialBecasEfCoreTests` que devuelve únicamente el periodo consultado. Verificación: `dotnet test services/Ascenso.S4.Becas/tests/Integration` pasa al 100 %.

## 4. Api, Solución y Contratos

- [x] 4.1 Restaurar `Ascenso.sln` limpio desde `main` (`git checkout origin/main -- Ascenso.sln`) y re-registrar los 7 proyectos de `services/Ascenso.S4.Becas/` con `dotnet sln Ascenso.sln add`, eliminando todo marcador de conflicto. Verificación: `dotnet build Ascenso.sln` compila S1, S2, S4 y `building-blocks` con 0 advertencias y 0 errores.
- [x] 4.2 Actualizar `services/Ascenso.S4.Becas/Ascenso.S4.Becas.Api/Program.cs` registrando `AddAuthentication()`, `AddAuthorization()` y `AddS4Infrastructure(builder.Configuration)` para arranque seguro en runtime. Verificación: prueba de arranque en runtime en `HistorialBecasApiTests` y ejecución de `/health` con `200 OK`.
- [x] 4.3 Mapear `"DIRECCION"` a `RolHistorialBecas.Direccion` en `HistorialBecasController.ResolverRol` y agregar la prueba HTTP `Direccion_ConsultaPorCedulaYPorPrograma_Responde200` en `HistorialBecasApiTests.cs`. Verificación: `dotnet test services/Ascenso.S4.Becas/tests/Integration` pasa al 100 %.
- [x] 4.4 Corregir `nullable: true` a `type: ['string', 'null']` en `HistorialBecasResponse.mensaje` en `contracts/openapi.yaml`, actualizar `services/Ascenso.S4.Becas/INTEGRACION_EQUIPO.md` con el schema `s4_becas` y el rol `DIRECCION`, y confirmar que la rama no mezcla `docs/ASCENSO — División de requerimientos por integrante.md`. Verificación: ejecución completa de `dotnet test Ascenso.sln` y `openspec validate --all --strict` (sin errores en `spec/becas` ni `change/corregir-integracion-historial-becas`).
