# Revisión de la Arquitectura v1.2 — Seguimiento de hallazgos

ASCENSO · Plataforma de becas de la Fundación Ascenso · 20 de noviembre de 2026

**De 17 puntos pendientes de la v1.1:** 11 corregidos y 6 a medias. **Nuevos:** 8.

## Veredicto

El paquete v1.2 ya está completo y es verificable. De los 17 puntos pendientes de la revisión de la v1.1, 11 quedaron cerrados y 6 siguen a medias: en casi todos se corrigió lo señalado, pero la corrección dejó un hueco al lado. Aparecen 8 hallazgos nuevos (5 importantes y 3 mejoras); ninguno obliga a rediseñar. Lo más urgente: la secuencia 11 volvió a romperse en Mermaid Live y RF-18 quedó asignado al servicio equivocado.

## Decisiones del equipo vigentes

- 16-nov-2026: MongoDB se mantiene como base de lectura (CQRS) en S6, conforme al D-14 del DDD v3.2.
- Fondos y Becas siguen como servicios separados (S2 y S4).
- Pendiente del equipo (T4): dueño de RF-18 y HU-18.1. Se recomienda S4 con S6 de apoyo; la alternativa es S6. En ningún caso S3.

## Estado de los 17 puntos

Prefijos: A = documento de arquitectura · B = archivos del kit · D = diagramas · N = hallazgo nuevo de la v1.1 · T = hallazgo nuevo de la v1.2.

| ID | Severidad | Hallazgo | Estado | Nota |
|---|---|---|---|---|
| A2 | Crítico | La lámina obligatoria se trata como no disponible | Parcial | Decisiones aplicadas; falta adjuntar la imagen. |
| A8 | Importante | Puerto académico, dueños de RF y ranking | Parcial | RF-18 y HU-18.1 en el servicio equivocado (T4). |
| A9 | Mejora | El DDD no refleja la separación de Fondos y Becas | Corregido | Adenda v3.3 incluida como propuesta. |
| B3 | Importante | Al contrato público le faltan operaciones | Parcial | Enum y rutas de HU-20.2 listos; ver T2 y T3. |
| B8 | Mejora | Faltan fuentes en el kit | Corregido | Carpeta 06_FUENTES presente. |
| B9 | Mejora | Contradicción sobre IFuenteAcademica | Parcial | Puerto unificado; queda HU-33.1 con «S4 consentimiento». |
| D4 | Importante | Mapa de contextos sin dos relaciones | Corregido | ACL académicas S3/S5 y Conformist con Keycloak. |
| D13 | Mejora | El diagrama 08 tiene rótulos viejos | Corregido |  |
| D14 | Mejora | Texto cortado en la secuencia 26 | Corregido | Sin texto cortado en las 12 vistas. |
| N1 | Crítico | El paquete no trae lo que promete | Corregido | 117 archivos; tamaños y SHA-256 coinciden. |
| N2 | Crítico | Los anexos no se regeneraron | Corregido | Anexos MD y PDF coinciden con los CSV. |
| N3 | Importante | Un mismo comando para dos intenciones | Corregido | AceptarOfertaCommand y MarcarPeriodoCommand. |
| N4 | Importante | Cifrado de la cuenta del lado del cliente | Corregido | numeroCuenta por TLS; cifra S4. |
| N5 | Importante | La cédula viaja en la URL | Corregido | Ruta por personaId. |
| N6 | Mejora | El evento llega directo al handler | Parcial | Flujo corregido, pero la fuente no compila (T1). |
| N7 | Mejora | Respuesta 202 tras un guardado síncrono | Parcial | Asíncrono real; falta consulta de resultado (T5). |
| N8 | Mejora | La tabla de calidad cita evidencia ausente | Corregido | Toda la evidencia citada existe. |

## Pendientes en detalle (5)

### A2 · Falta adjuntar la lámina obligatoria

- **Dónde:** 06_FUENTES; LEEME_PRIMERO.md; COMO_ABRIR.txt; MANIFIESTO_ENTREGA.md; maestro §1.2, §18 y §20.7; REPORTE_SEGUIMIENTO.md
- **Problema:** El kit dice que Arquitectura_Obligatoria.jpeg «no es accesible», pero el equipo la tiene en el proyecto. Sus decisiones ya figuran como obligatorias; solo falta la imagen. La regla de precedencia del maestro (línea 9 y §18) tampoco la lista.
- **Corrección:** Copiar la imagen a 06_FUENTES, quitar la nota de «no accesible» de esos archivos y agregar la lámina a la precedencia tecnológica.

### A8 / B9 · RF-18 y HU-18.1 en S3; HU-33.1 sin actualizar

- **Dónde:** MATRIZ_40_RF_ARQUITECTURA.csv; MATRIZ_73_HU_ARQUITECTURA.csv; XLSX; anexos; maestro §20.3
- **Problema:** HU-04.1, RF-04 y HU-33.2 quedaron bien. Pero RF-18 y HU-18.1 se unificaron en S3, que no tiene ni el agregado ni la proyección (ver T4), y HU-33.1 sigue diciendo «S4 consentimiento».
- **Corrección:** Aplicar T4 y corregir la colaboración de HU-33.1 a S6.

### B3 · El contrato público sigue con dos huecos de OTP y consecuencias

- **Dónde:** openapi.yaml
- **Problema:** El enum ya incluye APLICAR_CONSECUENCIA_REVOCATORIA y HU-20.2 tiene sus dos rutas. Quedan la respuesta del desafío OTP (T2) y el comando de consecuencias (T3).
- **Corrección:** Aplicar T2 y T3.

### N6 · La secuencia 11 corregida no compila

- **Dónde:** 02_DIAGRAMAS/11_secuencia_reserva_oferta.mmd
- **Problema:** El flujo ya pasa por Outbox S3, RabbitMQ e Inbox S4, pero el archivo fuente dejó de compilar en Mermaid (T1).
- **Corrección:** Aplicar T1.

### N7 · El lote asíncrono no devuelve su resultado

- **Dónde:** openapi.yaml; secuencia 24
- **Problema:** El proceso ya es asíncrono de verdad y la ACL de S5 es dueña de la correspondencia estudiante–beca. Falta la consulta del resultado y varios campos del reporte (T5).
- **Corrección:** Aplicar T5.

## Hallazgos nuevos (8)

### T1 · La secuencia 11 no abre en Mermaid (Importante)

- **Dónde:** 02_DIAGRAMAS/11_secuencia_reserva_oferta.mmd (paso 9); ENLACES_MERMAID_ARQUITECTURA.html
- **Problema:** El paso 9 dice «POST reservas privado; fondos.reservar» y Mermaid toma el punto y coma como fin de instrucción. Es el mismo error del hallazgo B6 de la v1.0. La vista local se ve bien porque se dibuja aparte, pero los dos enlaces a Mermaid Live llevan el código roto.
- **Corrección:** Cambiar «;» por «,», regenerar los dos enlaces y agregar la compilación de las 12 secuencias a la verificación (el reporte admite que no se comprobó).

### T2 · El desafío OTP no devuelve su identificador de forma tipada (Importante)

- **Dónde:** openapi.yaml: POST /api/v1/otp/challenges
- **Problema:** La ruta responde con OperacionConfirmada. El esquema ChallengeResponse (challengeId y expiraEnUtc) existe, pero ninguna ruta lo usa, y todos los comandos con OTP exigen ese challengeId.
- **Corrección:** Que la ruta responda con ChallengeResponse.

### T3 · El comando de consecuencias no permite expresar HU-25.2 (Importante)

- **Dónde:** openapi.yaml: AplicarConsecuenciaCommand
- **Problema:** No indica cuál consecuencia se aplica (llamado de atención, suspensión o revocación), ni el «siguiente paso», ni la fecha de revisión de una suspensión (HU-21.1). Además exige OTP siempre, y la HU solo lo pide al revocar.
- **Corrección:** Agregar tipo de consecuencia, siguiente paso y fecha de revisión opcional; OTP obligatorio solo si es revocación.

### T4 · RF-18 y HU-18.1 quedaron en S3 (Importante)

- **Dónde:** MATRIZ_40_RF_ARQUITECTURA.csv; MATRIZ_73_HU_ARQUITECTURA.csv; anexos; maestro §20.3
- **Problema:** El historial se alimenta de eventos de Beca (DDD v3.2, modelo de lectura historial_becas) y la HU dice que HU-16.1 lo invoca en cada aceptación de oferta, que ocurre en S4. S3 no tiene ni el agregado ni la proyección. La revisión de la v1.1 pedía «el mismo dueño» sin decir cuál; la HU ya estaba bien en S4 y el que debía moverse era el RF.
- **Corrección:** Dueño S4 (consulta autoritativa por persona) con S6 como colaborador (proyección por programa y periodo). S3 solo consume para HU-01.2. Alternativa: dueño S6, siguiendo el modelo de lectura del DDD.

### T5 · El lote universitario no cumple HU-24.3 (Importante)

- **Dónde:** openapi.yaml: ReporteAcademicoLote y LoteAceptado; secuencia 24
- **Problema:** El paso 7 de la HU pide devolver qué reportes se registraron y cuáles se rechazaron, con su motivo; con 202 asíncrono no hay ruta para consultarlo. Cada reporte solo lleva promedio y código de estudiante: faltan materias aprobadas, materias reprobadas y estado de matrícula.
- **Corrección:** Agregar una consulta del resultado del lote por loteId, fila por fila, y los tres campos que faltan.

### T6 · S4 «realiza» interfaces que solo consume (Mejora)

- **Dónde:** 02_DIAGRAMAS/DIAGRAMA_COMPONENTES.drawio
- **Problema:** S4 aparece con flecha de realización hacia IFondosPrivados e IValidacionOtpPrivada, que ofrecen S2 y S1. El diagrama 04 del atlas sí está bien.
- **Corrección:** Cambiar esas dos flechas a dependencia, como la de S3 hacia IFondosPrivados.

### T7 · Tres eventos sin catalogar (Mejora)

- **Dónde:** CATALOGO_EVENTOS_INTEGRACION.csv; EVENTOS_E_INTERFACES.md
- **Problema:** SugerenciaListaEspera.v1 y LoteAcademicoRecibido solo aparecen en las secuencias 26 y 24. CostoMatriculaVigente.v1 está en el maestro y en EVENTOS_E_INTERFACES.md, pero no en el CSV.
- **Corrección:** Catalogarlos con quién publica y quién consume.

### T8 · Restos menores (Mejora)

- **Dónde:** LEEME_PRIMERO.md; nombres de archivo; openapi.yaml
- **Problema:** LEEME_PRIMERO.md titula «Qué cambió en v1.2» lo que corresponde a la v1.1 (33 hallazgos). Hay archivos con «V1.0» y «V1.1» en el nombre dentro de un kit v1.2. El contrato público conserva dos esquemas sin uso: ReservaResponse (interno) y RegistrarConsultaCommand (sin ruta).
- **Corrección:** Corregir el título y quitar los esquemas o darles ruta.

## Orden sugerido de corrección

1. **Antes de presentar:** T1, A2
2. **Contrato público:** T2, T3, T5
3. **Matrices:** T4, HU-33.1 (A8 / B9)
4. **Pulido:** T6, T7, T8

## Lo que está bien

- Paquete: los 117 archivos del inventario existen, y sus tamaños y SHA-256 coinciden. Toda la evidencia citada en las dos matrices de control está en el zip.
- Matrices: las 9 hojas del XLSX son idénticas a sus CSV. Los anexos (MD y PDF) coinciden con los CSV en los 40 RF y las 73 HU.
- Contratos: todas las referencias resuelven (34 en el público y 9 en el interno). AceptarOfertaCommand y MarcarPeriodoCommand están separados, numeroCuenta viaja por TLS y lo cifra S4, y la ruta de beneficios usa personaId.
- Diagramas: el 03 tiene las dos ACL académicas y la relación Conformist con Keycloak; el 08 quedó limpio; en las 12 vistas locales no hay texto cortado; 11 de las 12 secuencias compilan en Mermaid.
- PDF: el atlas y el documento maestro se regeneraron con los cambios.

## Alcance de la revisión

- Zip ASCENSO_V1.2_CORRECCIONES: 118 archivos en 10 carpetas (maestro, 20 diagramas DOT/SVG, 12 secuencias, 2 draw.io, 2 OpenAPI, 9 CSV, XLSX, 3 PDF, visores HTML y registros de revisión).
- Verificado por máquina: inventario y hashes, XLSX frente a CSV, anexos frente a CSV, referencias de OpenAPI, compilación de las 12 secuencias, enlaces de Mermaid Live decodificados y texto cortado en las vistas.
- Revisados como imagen: diagramas 03 y 08 y secuencias 11 y 26. Los otros 18 diagramas DOT y 9 secuencias no cambiaron respecto a la v1.1.
- Comparado contra la revisión de la v1.1 (17 puntos), el DDD v3.2 y las HU Plan B final.

**Limitaciones**

- No hay código implementado: los hallazgos se refieren al diseño.
- Los 2 draw.io se revisaron por su contenido y el estilo de sus flechas, no por su diagramación.
- La vista local de la secuencia 24 se comprobó por medición del texto, no como imagen.
