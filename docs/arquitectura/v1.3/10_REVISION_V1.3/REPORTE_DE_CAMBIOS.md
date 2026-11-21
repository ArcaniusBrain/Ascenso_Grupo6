# Reporte de cambios — ASCENSO Arquitectura v1.3

**Fecha:** 20 de noviembre de 2026 · **Base:** kit v1.2 y `REVISION_RECIBIDA.md` (revisión de la v1.2).

Se cierran los 6 puntos que la revisión dejó a medias (A2, A8, B9, B3, N6 y N7) y sus 8 hallazgos nuevos (T1 a T8). Al corregir apareció un defecto más (T9). La partición S1–S6 y el DDD v3.2 no cambian. No hay código de producto: todo lo verificado es documental.

## Cambios por punto

| Punto | Qué se cambió | Archivos |
|---|---|---|
| A2 | Se incluyó la lámina «Decisiones Arquitectónicas» del profesor. Su transcripción se comprobó contra la imagen y pasó a ser la primera fuente de las decisiones tecnológicas. Se retiraron las notas de «no accesible» de las guías vigentes. | `06_FUENTES/Arquitectura_Obligatoria.jpeg`, `06_FUENTES/DECISIONES_ARQUITECTONICAS_*.md`, maestro (precedencia, §1.2, §18), `LEEME_PRIMERO.md`, `COMO_ABRIR.txt`, `MANIFIESTO_ENTREGA.md`, `REGISTRO_24_ADR.csv`, `DECISIONES_Y_BLOQUEOS.csv` |
| A8 / B9 / T4 | RF-18 y HU-18.1 pasan a S4, con S6 como colaborador para la proyección `historial_becas`. HU-33.1 nombra a S6 como dueño del consentimiento. | `MATRIZ_40_RF_ARQUITECTURA.csv`, `MATRIZ_73_HU_ARQUITECTURA.csv`, XLSX, `ANEXOS_TECNICOS.md`, maestro §20 y §21 |
| B3 / T2 | El desafío OTP responde con `ChallengeResponse`. | `openapi.yaml` |
| B3 / T3 | `AplicarConsecuenciaCommand` lleva consecuencia, siguiente paso y fecha de revisión; el OTP solo es obligatorio al revocar. | `openapi.yaml` |
| N7 / T5 | Cada reporte del lote lleva promedio, materias aprobadas, materias reprobadas y estado de matrícula. Ruta nueva para consultar el resultado del lote fila por fila. La secuencia 24 muestra esa consulta. | `openapi.yaml`, `24_secuencia_universidad.mmd` |
| N6 / T1 | Se quitó el punto y coma que cortaba el paso 9 de la secuencia 11. | `11_secuencia_reserva_oferta.mmd` |
| T6 | Las flechas de S4 hacia `IFondosPrivados` e `IValidacionOtpPrivada` pasaron de realización a dependencia. | `DIAGRAMA_COMPONENTES.drawio` |
| T7 | Se catalogaron `CostoMatriculaVigente`, `SugerenciaListaEspera` y `LoteAcademicoRecibido`. | `CATALOGO_EVENTOS_INTEGRACION.csv`, `EVENTOS_E_INTERFACES.md` |
| T8 | Título del LEEME corregido. `ReservaResponse` retirado del contrato público. `RegistrarConsultaCommand` ahora tiene ruta (HU-32.1). | `LEEME_PRIMERO.md`, `openapi.yaml` |
| T9 | Los 24 enlaces a Mermaid Live se regeneraron con el formato de estado que el editor espera. Los de la v1.2 llevaban el código sin ese envoltorio, así que no cargaban ningún diagrama. | `ENLACES_MERMAID_ARQUITECTURA.html`, `CORRECCION_ENLACES_MERMAID.txt` |

## Artefactos regenerados

- **Secuencias:** las 12 vistas de `02_DIAGRAMAS/VISTAS_SECUENCIAS_SVG` se generan ahora con Mermaid a partir de los `.mmd`, de modo que la vista y la fuente no pueden divergir. `SECUENCIAS_MERMAID_VISTA_LOCAL.html` las incluye.
- **PDF:** el documento maestro (19 páginas) y los anexos (11 páginas) se regeneraron desde sus Markdown con pandoc y Chromium, usando `00_INICIO/estilo_pdf.css`. En esa hoja se subieron los tamaños de letra (texto de 9 a 10,5 px y tablas de 7,5 a 8,5 px) y el alto máximo de las imágenes. A diferencia del PDF de la v1.2, el maestro ahora muestra los diagramas dentro del texto. El atlas de 20 diagramas no cambió.
- **Visores:** `ABRIR_PRIMERO.html` y `VISOR_COMPLETO_SIN_CARPETAS.html` llevan los datos actualizados (secuencias, matrices, contratos, PDF y control de correcciones).
- **Inventario:** `INVENTARIO_REAL.csv` recalculado con tamaño y SHA-256 de cada archivo.

## Comprobaciones hechas

- Las 12 secuencias compilan con Mermaid CLI.
- Los 24 enlaces decodifican a un estado JSON cuyo código es idéntico al `.mmd` correspondiente.
- En `openapi.yaml` todas las referencias resuelven y no quedan esquemas sin uso; son 29 operaciones. `openapi-interno.yaml` no cambió.
- El esquema del comando de consecuencias se probó con cinco casos: acepta llamado de atención y suspensión sin OTP, y revocación con OTP; rechaza revocación sin OTP y comando sin consecuencia.
- Las 9 hojas del XLSX coinciden con sus CSV, y los anexos coinciden con los CSV en los 40 RF y las 73 HU.
- Inventario: todos los archivos listados existen y sus tamaños y SHA-256 coinciden.

## Supuestos tomados, a confirmar por el equipo

1. **Dueño de RF-18 y HU-18.1:** S4. La alternativa es S6, si se prefiere seguir el modelo de lectura del DDD. En ningún caso S3.
2. **`EstadoMatricula`:** el DDD v3.2 solo define `INSCRITO`. El contrato deja el campo como texto hasta que el equipo cierre el catálogo.
3. **Ruta de consultas:** `POST /api/v1/consultas` usa el esquema que ya existía; si HU-32.1 exige indicar la postulación o la beca, hay que agregar ese dato.

## Lo que no se tocó

- Los 20 diagramas DOT y el atlas PDF.
- `openapi-interno.yaml`, `MATRIZ_PUERTOS.md`, `SEGURIDAD_INTERSERVICIOS_V1.1.md` y `05_CALIDAD`.
- Los registros históricos de `08_REVISION_V1.1` y `09_REVISION_V1.2`.
- Los nombres de archivo con «V1.0» y «V1.1», para no romper referencias internas.

## Observación fuera de alcance

`EVENTOS_E_INTERFACES.md` dice que S3 consume los eventos `Beca*` para una proyección de consulta, pero `CATALOGO_EVENTOS_INTEGRACION.csv` y `MAPEO_34_EVENTOS_DOMINIO.csv` no listan a S3 como interesado. No se cambió porque no formaba parte de la revisión; conviene que el equipo lo alinee.

## Límites

- No se pudo abrir mermaid.live desde el entorno de generación; los enlaces se validaron por formato, igual al de los enlaces del DDD que el equipo ya usa.
- `DIAGRAMA_COMPONENTES.drawio` se revisó por el estilo de sus flechas, no como imagen.
