# Arquitectura de ASCENSO

**Versión vigente: v1.3** (20-nov-2026), línea base de diseño adoptada por el equipo. No equivale a aprobación del profesor ni a código implementado: lo pendiente está en su §16 y en [`pendientes-v1.3.md`](pendientes-v1.3.md).

## Por dónde empezar

| Qué | Archivo |
|---|---|
| Vista navegable (diagramas, matrices, ADR, contratos, PDF) | [`v1.3/ABRIR_PRIMERO.html`](v1.3/ABRIR_PRIMERO.html): descárgalo y ábrelo en Chrome o Edge |
| Documento rector, con ADR-01 a ADR-24 en su §13 | [`v1.3/01_DOCUMENTO_MAESTRO/ARQUITECTURA_V1.0.md`](v1.3/01_DOCUMENTO_MAESTRO/ARQUITECTURA_V1.0.md) (el nombre del archivo se conserva; el contenido es la v1.3) |
| 40 RF, 25 agregados y 34 eventos por servicio | [`v1.3/01_DOCUMENTO_MAESTRO/ANEXOS_TECNICOS.md`](v1.3/01_DOCUMENTO_MAESTRO/ANEXOS_TECNICOS.md) |
| Diagramas (20 vistas DOT/SVG, 12 secuencias Mermaid, 2 draw.io) | [`v1.3/02_DIAGRAMAS/`](v1.3/02_DIAGRAMAS/) · enlaces: [`v1.3/ENLACES_MERMAID_ARQUITECTURA.html`](v1.3/ENLACES_MERMAID_ARQUITECTURA.html) |
| Contratos: API pública, API entre servicios, eventos, puertos, seguridad | [`v1.3/03_CONTRATOS/`](v1.3/03_CONTRATOS/) |
| Trazabilidad: dueño de cada RF, HU, agregado y evento | [`v1.3/04_MATRICES/`](v1.3/04_MATRICES/) (los CSV son la fuente; el XLSX es una copia) |
| Pruebas de arquitectura y definición de terminado | [`v1.3/05_CALIDAD/PRUEBAS_Y_CRITERIOS.md`](v1.3/05_CALIDAD/PRUEBAS_Y_CRITERIOS.md) |
| Lámina «Decisiones Arquitectónicas» del profesor | [`v1.3/06_FUENTES/`](v1.3/06_FUENTES/) |
| PDF para presentar | [`v1.3/07_PDF/`](v1.3/07_PDF/) |
| Historial de revisiones de la arquitectura (v1.1 a v1.3) | `v1.3/08_REVISION_V1.1/`, `09_REVISION_V1.2/`, `10_REVISION_V1.3/` |

## Configuración adoptada

- **6 microservicios .NET 10** (S1–S6) detrás de un API Gateway **YARP**, con **Clean Architecture + DDD** en cada uno.
- **PostgreSQL + EF Core** para escribir (un schema y un usuario por servicio) y **MongoDB** para las proyecciones de lectura (CQRS, DDD D-14).
- **Firebase Storage** para archivos; **Keycloak** para identidad y segundo factor; **OTP** por operación en S1.
- **MassTransit 8 + RabbitMQ** con Outbox e Inbox; **Hangfire** para procesos programados; **SignalR** en S6.
- **React + Next.js** en el frontend.

Lo obligatorio según la lámina del profesor: microservicios conectados por APIs, Firebase con PostgreSQL + EF Core, React + Next.js, .NET (C#), máquina de estados, enfoque en auditoría y ETL. El resto son elecciones del equipo.

## Relación con el resto de la documentación

- **Precedencia:** historias de usuario (con C1–C5 y R1–R30) → DDD v3.2 → lámina del profesor → esta arquitectura.
- **Dominio:** el DDD v3.2 no cambia; la ubicación de Programas y Fondos (S2) y de Becas (S4) en servicios distintos está en la [adenda DDD v3.3](../dominio/DDD_v3.3_adenda_S2_S4.md). Las correcciones de dominio que siguen abiertas son el cambio de OpenSpec [`corregir-ddd-v3-3`](../../openspec/changes/corregir-ddd-v3-3/proposal.md).
- **Contratos:** mientras no haya código, la fuente es `v1.3/03_CONTRATOS/`. El primer cambio de código los moverá a `contracts/` en la raíz (estructura de la v1.3, §12).

## Notas sobre esta copia del kit

- `VISOR_COMPLETO_SIN_CARPETAS.html` no se incluye: era idéntico a `ABRIR_PRIMERO.html`.
- De `06_FUENTES` solo se publican los documentos del equipo y la lámina; el resto ya está en el repositorio o es material de clase (ver [`v1.3/06_FUENTES/README.md`](v1.3/06_FUENTES/README.md)). Por eso `INVENTARIO_REAL.csv` lista archivos que no están en esta copia.

## Versiones anteriores

La propuesta v0.1 (24 ADR en archivos separados, 4-nov-2026) queda como referencia en [`historico/v0.1/`](historico/v0.1/). La v1.3 la reemplaza.
