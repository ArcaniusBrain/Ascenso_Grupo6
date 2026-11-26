# Proposal

## Why

La gestión de donantes y sus aportes (**RF-13**: **HU-13.1** y **HU-13.2**) completa la fuente de verdad de los fondos en **`S2 Catálogo y Fondos`** (`services/Ascenso.S2.CatalogoFondos/`):

- **HU-13.1** (*Registrar donante*): da de alta donantes de tipo `ORGANIZACION` o `PERSONA` con sus datos de contacto, sin crear cuenta ni rol de acceso a la plataforma (fuera de alcance: portal de donantes), bloqueando roles sin permiso (`RF-37`).
- **HU-13.2** (*Registrar aportes en ledger append-only y consultar historial*): registra aportes `> 0` por `(donante, programa, periodo)` sin perder nunca registros previos (`RNF trazabilidad`), conecta en vivo con el presupuesto total de `Programa` (`HU-11.2`, `S8`), permite filtrar el historial por periodo y bloquea acceso a postulantes y becarios.
- **ADRs aplicados:** `ADR-01/02/04`, `ADR-05` (`s2_catalogo_fondos`), `ADR-12/13` (Outbox `AporteRegistrado.v1`), `ADR-16/25` (`Version` explícita) y `ADR-27` (`MediatR 12.5.0`).

## What Changes

- **Contratos (`Ascenso.Contracts`):** añade de forma aditiva `AporteRegistradoV1` en `EventosCatalogoFondosV1.cs` (catalogado en `CATALOGO_EVENTOS_INTEGRACION.csv`, `S2 → S2, S6`).
- **Dominio S2 (`Ascenso.S2.CatalogoFondos.Domain`):** raíz `Donante : RaizDeAgregado<Guid>` (`Version` explícita), enum `TipoDonante` (`ORGANIZACION` | `PERSONA`), value object `ContactoDonante`, entidad interna append-only `Aporte`, eventos `DonanteRegistradoEvento` y `AporteRegistradoEvento`, e `IDonanteRepository`.
- **Aplicación S2 (`Ascenso.S2.CatalogoFondos.Application`):** `PoliticaAutorizacionDonantes` y comandos/queries `MediatR 12.5.0` para registrar donantes, registrar aportes (publicando `AporteRegistrado.v1` vía Outbox), listar donantes y consultar historial de aportes con filtro por periodo.
- **Infraestructura S2 (`Ascenso.S2.CatalogoFondos.Infrastructure`):** tablas `donantes` y `aportes` (índice por `(donante_id, codigo_periodo)` per Anexo A) en `CatalogoFondosDbContext`, `DonanteRepositoryEfCore` y sustitución de `ConsultorDonantesYAportesPorDefecto` por **`ConsultorDonantesYAportesEfCore`** (integración real `RF-13 ↔ RF-11`).
- **API y contratos (`Ascenso.S2.CatalogoFondos.Api` + `contracts/openapi.yaml`):** implementa `POST /api/v1/programas/{programaId}/aportes` (respetando la ruta y `RegistrarAporteCommand` existentes) y añade las rutas de `/api/v1/donantes` bajo tag `S2`.
- **Pruebas:** unitarias de dominio, integración HTTP + SQLite real cubriendo `HU-13.1` (E1–E5), `HU-13.2` (E1–E9) y la actualización real del presupuesto de `Programa` (`HU-11.2`), más `ArquitecturaCatalogoFondosTests` incluyendo `Donante`.

## Capabilities

### New Capabilities

- `donantes-aportes`: registro de donantes (organización o persona, sin cuenta de acceso), ledger append-only de aportes por programa y periodo, consulta filtrada por periodo e integración en vivo con el presupuesto total de programas (`RF-13`: `HU-13.1` y `HU-13.2`).

### Modified Capabilities

Ninguna.

## Impact

- **Servicios afectados:** amplía `services/Ascenso.S2.CatalogoFondos/` y añade de forma estrictamente aditiva `AporteRegistradoV1` en `building-blocks/Ascenso.Contracts/Eventos/EventosCatalogoFondosV1.cs`.
- **No modifica** `S1` ni `Ascenso.SharedKernel`.
- **Archivos compartidos tocados:** `building-blocks/Ascenso.Contracts/Eventos/EventosCatalogoFondosV1.cs`, `contracts/openapi.yaml` (tag `S2`), `docs/arquitectura/pendientes-v1.3.md` (`C18`) y `docs/guias-integracion/04-s2-catalogo-fondos-calendario.md`.
- **Fuera de alcance en este PR:** las reservas y liberaciones transaccionales sobre el saldo disponible (`PresupuestoPrograma`, `RF-12`).
