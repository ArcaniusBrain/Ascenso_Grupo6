# ASCENSO — Kit de Arquitectura v1.3

**Abre `ABRIR_PRIMERO.html` en Chrome o Edge.** Es la entrada visual para equipo y profesor.

## Orden de lectura

1. `01_DOCUMENTO_MAESTRO/ARQUITECTURA_V1.0.md` — documento rector y decisiones ADR-01 a ADR-24.
2. `01_DOCUMENTO_MAESTRO/ANEXOS_TECNICOS.md` — los 40 RF, 25 agregados y 34 eventos en tablas.
3. `02_DIAGRAMAS/` — 20 SVG vectoriales, 12 secuencias Mermaid (`.mmd`), DOT, dos diagramas de base `.drawio`, e índice `ENLACES_MERMAID_ARQUITECTURA.html`.
4. `03_CONTRATOS/` — `openapi.yaml` (contrato público), `openapi-interno.yaml` (contrato entre servicios), eventos y puertos.
5. `04_MATRICES/` — CSV abiertos en Excel, incluidos 73 HU, más un XLSX para revisión.
6. `05_CALIDAD/` — pruebas de arquitectura, definición de terminado y riesgos.
7. `07_PDF/` — documentos de presentación, atlas gráfico y anexos.
8. `06_FUENTES/` — DDD v3.2, material del profesor e historias (versiones originales intactas).

## Terminología de estado

**Arquitectura cerrada como diseño de referencia** significa que ya existe una elección, un contrato y una razón documentada para cada frontera. **No significa** aprobación del profesor, programa implementado, software probado ni resolución inventada de preguntas del cliente. Las condiciones externas se consultan en §16 del documento maestro.

No usar los modelos de dominio antiguos: el único dominio vigente es `DDD_V3.2.md`.

## Qué cambió en v1.1
Se corrigen 33 hallazgos del informe de revisión. Abrir `08_REVISION_V1.1/REPORTE_DE_VERIFICACION.md` para localizar cada cambio. `openapi.yaml` es público y `openapi-interno.yaml` es solo servicios.

## Revisión 19-nov-2026 — v1.2
La fuente única de trazabilidad son los CSV de `04_MATRICES`. El documento PDF de anexos se generó desde esos CSV. Para el detalle de las 17 observaciones revisar `09_REVISION_V1.2/REPORTE_SEGUIMIENTO.md`. En esa versión la lámina obligatoria aún no estaba en el kit; se incorporó en la v1.3.

## Revisión 20-nov-2026 — v1.3
Se cierran los 6 puntos a medias y los 8 hallazgos nuevos de la revisión de la v1.2. Ver `10_REVISION_V1.3/REPORTE_DE_CAMBIOS.md` para el detalle por archivo. La lámina del profesor está en `06_FUENTES/Arquitectura_Obligatoria.jpeg`. Las 12 secuencias se renderizan con Mermaid desde sus `.mmd` y los enlaces a Mermaid Live se regeneraron.
