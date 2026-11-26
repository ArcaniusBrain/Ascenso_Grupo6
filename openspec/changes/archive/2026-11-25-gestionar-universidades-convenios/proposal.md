# Proposal

## Why

Las universidades aliadas (**RF-14**: **HU-14.1**, **HU-14.2** y **HU-14.3**) son el segundo agregado institucional de **`S2 Catálogo y Fondos`** (`services/Ascenso.S2.CatalogoFondos/`):

- **HU-14.1** (*Universidad aliada, convenio y carreras elegibles*): registra nombre, términos del convenio, vía de intercambio académico (`API` o `MANUAL`), contacto designado (enlace) y carreras elegibles propias de cada universidad; impide que el enlace modifique su convenio (`RF-37`).
- **HU-14.2** (*Costo de matrícula append-only por periodo*): guarda cada registro y corrección sin borrar valores previos (`C2`), publica **`CostoMatriculaVigente.v1`** por Outbox hacia `S4` (tope de no doble beneficio en `HU-01.2`/`HU-16.1`) e informa cuando un periodo aún no tiene costo cargado.
- **HU-14.3** (*Fechas de inicio de inscripciones por universidad y periodo*): conecta las fechas UTC-4 (`C4`, `R20`, `C15`) directamente con las universidades aliadas registradas y los periodos de `CalendarioAcademico`, sin romper `RF-38`.
- **ADRs aplicados:** `ADR-01/02/04`, `ADR-05` (`s2_catalogo_fondos`), `ADR-12/13` (Outbox), `ADR-16/25` (`Version` explícita), `ADR-17/R20` (`IRelojNegocio`) y `ADR-27` (`MediatR 12.5.0`).

## What Changes

- **Dominio S2 (`Ascenso.S2.CatalogoFondos.Domain`):** raíz `UniversidadAliada` (`RaizDeAgregado<Guid>` con `Version` explícita), value objects `Convenio` (`ViaDatosAcademicos`: `API` | `MANUAL`) y `ContactoDesignado`, entidades internas `CarreraElegible` y `CostoMatricula` (append-only con `VersionRegistro` incremental), eventos `UniversidadRegistradaEvento` y `CostoMatriculaRegistradoEvento`, e `IUniversidadAliadaRepository`.
- **Aplicación S2 (`Ascenso.S2.CatalogoFondos.Application`):** comandos y queries `MediatR 12.5.0` para registrar universidades, actualizar convenio/contacto (bloqueando al enlace de universidad), agregar carreras elegibles, registrar y consultar costos de matrícula (emitiendo `CostoMatriculaVigenteV1` vía Outbox) y gestionar/consultar inicios de inscripciones desde la universidad (`HU-14.3`).
- **Infraestructura S2 (`Ascenso.S2.CatalogoFondos.Infrastructure`):** tablas `universidades_aliadas`, `carreras_elegibles` y `costos_matricula` en `CatalogoFondosDbContext` (`s2_catalogo_fondos`), repositorio EF Core y actualización de `ArquitecturaCatalogoFondosTests` incluyendo `UniversidadAliada`.
- **API y contratos (`Ascenso.S2.CatalogoFondos.Api` + `contracts/openapi.yaml`):** rutas públicas bajo tag `S2` para `/api/v1/universidades`, carreras, costos de matrícula e inicios de inscripciones por universidad.
- **Pruebas:** unitarias de dominio, integración HTTP + SQLite real (append-only de costos, publicación de `CostoMatriculaVigenteV1` en Outbox, aislamiento de carreras por universidad y bloqueo al enlace) y arquitectura Clase 09.

## Capabilities

### New Capabilities

- `universidades-convenios`: registro y consulta de universidades aliadas, convenios con vía de datos académicos, carreras elegibles, costos de matrícula append-only con publicación de `CostoMatriculaVigente.v1` y fechas de inicio de inscripciones por universidad y periodo (`RF-14`: `HU-14.1`, `HU-14.2`, `HU-14.3`).

### Modified Capabilities

Ninguna.

## Impact

- **Servicios afectados:** amplía `services/Ascenso.S2.CatalogoFondos/` (sin crear nuevos proyectos ni tocar `S1`, `Ascenso.SharedKernel` ni `Ascenso.Contracts`, ya que `CostoMatriculaVigenteV1`, `Dinero`, `CodigoPeriodo` y `DatosContacto` ya existen).
- **Archivos compartidos tocados:** `contracts/openapi.yaml` (rutas públicas de `RF-14` bajo tag `S2`) y `docs/arquitectura/pendientes-v1.3.md` (cierre de preguntas abiertas del Anexo B confirmadas).
- **Fuera de alcance en este PR:** la proyección consumidora de `CostoMatriculaVigente.v1` dentro de `S4` (Leonel/Sebas) y la evaluación de cierre de disponibilidades en `CicloPostulacion` / lista de espera dentro de `S3` (Daniel/Diego).
