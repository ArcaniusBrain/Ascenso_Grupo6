# Design

## Context

Ver proposal.md. El kit v1.3 se entregó como carpeta autocontenida (00_INICIO … 10_REVISION_V1.3) con referencias internas entre sus carpetas.

## Goals / Non-Goals

**Goals:**
- Que el repositorio y `openspec/config.yaml` digan lo mismo que la v1.3.
- No romper las referencias internas del kit.

**Non-Goals:**
- Modificar el contenido de la v1.3 o del DDD v3.2.
- Crear código o proyectos .NET.

## Decisions

- **El kit se copia completo en `docs/arquitectura/v1.3/` con su estructura original.** Así sus rutas internas siguen funcionando. Alternativa descartada: repartir sus carpetas por el repositorio (rompía decenas de referencias).
- **`06_FUENTES` se publica parcialmente:** se excluyen la presentación de la Clase 09 (material del profesor), el PRD en Word y los PDF que duplican Markdown o archivos que ya están en el repositorio. Un README en esa carpeta indica dónde está cada fuente.
- **`VISOR_COMPLETO_SIN_CARPETAS.html` no se incluye:** era idéntico a `ABRIR_PRIMERO.html`.
- **Contratos:** la fuente sigue en `v1.3/03_CONTRATOS/` hasta el primer cambio de código, que los moverá a `contracts/`. Evita tener dos copias que se desincronicen.
- **Carpetas de código vacías con README** para fijar la estructura antes del primer proyecto.

## Risks / Trade-offs

- [El repositorio es público y la v1.3 incluye la lámina del profesor] → Se publica solo la lámina de decisiones (necesaria como fuente) y no la presentación de clase; decidir la privacidad del repositorio (pendiente C9).
- [`INVENTARIO_REAL.csv` lista archivos que no se publican] → Se explica en `docs/arquitectura/README.md`.
