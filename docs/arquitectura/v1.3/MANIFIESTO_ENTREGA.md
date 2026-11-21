# Manifiesto de entrega — ASCENSO Arquitectura v1.3

**Control documental:** DDD v3.2 original sin cambios. Las 33 observaciones del informe de 16-nov-2026 se han atendido mediante modificaciones a contratos, diagramas, matrices y documentación. El informe de evidencias está en `08_REVISION_V1.1/`.

## Punto de entrada

- `ABRIR_PRIMERO.html`: tablero completo autónomo (20 vistas de arquitectura, 12 secuencias, 40 RF, 73 HU, 24 ADR, contratos y documentos PDF incluidos).
- `VISOR_COMPLETO_SIN_CARPETAS.html`: respaldo autónomo.
- `07_PDF/`: documento maestro, atlas vectorial y anexos PDF, conservando los nombres previos de archivo por compatibilidad.
- `ENLACES_MERMAID_ARQUITECTURA.html`: enlaces de 12 secuencias Ver/Editar, y `SECUENCIAS_MERMAID_VISTA_LOCAL.html` con vistas offline generadas con Mermaid. La apertura remota de Mermaid Live no fue comprobada aquí.

## Estructura preservada

- `00_INICIO`: guía de lectura.
- `01_DOCUMENTO_MAESTRO`: fuente Markdown de arquitectura, versión interna v1.3; anexos.
- `02_DIAGRAMAS`: 20 diagramas DOT/SVG, 12 secuencias MMD/SVG y 2 draw.io.
- `03_CONTRATOS`: OpenAPI **pública** y **privada separadas**, eventos, interfaces, puertos y seguridad entre servicios.
- `04_MATRICES`: nueve CSV y workbook XLSX con trazabilidad.
- `05_CALIDAD`: condiciones para verificación cuando exista implementación.
- `06_FUENTES`: antecedentes y la revisión, adenda **propuesta** al DDD v3.2.
- `07_PDF`: salidas académicas.
- `08_REVISION_V1.1`: matriz de los 33 hallazgos, reporte de verificaciones y PDF.
- `09_REVISION_V1.2`: seguimiento de 17 puntos.
- `10_REVISION_V1.3`: revisión recibida de la v1.2, control de 15 puntos y reporte de cambios.

## Verificación técnica

Se validaron presencia de fuentes, estructura XML draw.io, sintaxis de Graphviz, referencias de OpenAPI, inventarios y formato JavaScript del tablero; la apertura del HTML fue comprobada en Chromium sin errores. No se ejecutaron microservicios ni se validaron integraciones reales.

## Límites verificables

1. La lámina **original** «Decisiones Arquitectónicas» está incluida desde la v1.3 en `06_FUENTES/Arquitectura_Obligatoria.jpeg` (A2).
2. El DDD v3.2 se preservó intacto: la separación S2/S4 se documenta en una adenda para aprobación de sus autores, no se afirma una adopción institucional de v3.3 (A9).
3. La arquitectura es línea base técnica de diseño. No equivale a código implementado ni aprobación del profesor.

## Seguimiento 19-nov-2026

- `09_REVISION_V1.2/REVISION_CONTROL_17_HALLAZGOS.csv` y `REPORTE_SEGUIMIENTO.md`: 9 anteriores y 8 nuevos.
- `INVENTARIO_REAL.csv`: manifiesto de SHA-256 y tamaño de cada archivo incluido.
- `06_FUENTES/DECISIONES_ARQUITECTONICAS_LAMINA_OBLIGATORIA_REFERENCIA.md`: transcripción de la lámina, comprobada contra la imagen en la v1.3.
- Las matrices CSV y los anexos PDF provienen de la misma fuente de datos; en v1.2 no se admiten tablas desactualizadas incrustadas en HTML.

## Correcciones 20-nov-2026 (v1.3)

- `10_REVISION_V1.3/REPORTE_DE_CAMBIOS.md` y `CONTROL_15_PUNTOS.csv`: qué se cambió, en qué archivo y cómo se comprobó.
- Los PDF del documento maestro y de los anexos se regeneraron desde sus Markdown con la misma hoja de estilo (`00_INICIO/estilo_pdf.css`), esta vez con pandoc y Chromium en lugar de WeasyPrint; la diagramación puede variar levemente. El atlas de 20 diagramas no cambió.
- Supuestos a confirmar por el equipo: dueño de RF-18 y HU-18.1 (S4) y catálogo de `EstadoMatricula`.
