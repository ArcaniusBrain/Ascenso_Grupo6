# Revisión de la Arquitectura v1.1 — Seguimiento de hallazgos

ASCENSO · Plataforma de becas de la Fundación Ascenso · 19 de noviembre de 2026

**De 33 hallazgos previos:** 24 corregidos y 9 parciales o abiertos. **Nuevos:** 8.

## Veredicto

La v1.1 corrige 24 de los 33 hallazgos de la revisión del 16 de noviembre; quedan 9 parciales o abiertos y aparecen 8 hallazgos nuevos. La arquitectura en sí se sostiene: los problemas que quedan son de coherencia entre documentos y de empaquetado. Dos cosas conviene resolverlas antes de presentar al profesor: el zip no trae las fuentes que afirma traer (la tabla de calidad da por corregido algo que no se puede comprobar) y el contrato público deja una HU sin ruta posible.

## Decisiones del equipo vigentes

- 16-nov-2026: MongoDB se mantiene como base de lectura (CQRS) en S6, conforme al D-14 del DDD v3.2.
- Fondos y Becas siguen como servicios separados (S2 y S4).

## Estado de los 33 hallazgos

Prefijos: A = documento de arquitectura · B = archivos del kit (contratos, matrices, Mermaid) · D = diagramas · N = hallazgo nuevo de la v1.1.

| ID | Severidad | Hallazgo | Estado | Nota |
|---|---|---|---|---|
| A1 | Mejora | El kit no dice MongoDB en todas partes | Corregido |  |
| A2 | Crítico | La lámina obligatoria se trata como no disponible | Abierto | Ver detalle. |
| A3 | Importante | Cuatro HU en un servicio que no es dueño del agregado | Corregido | Corregido en la matriz de HU; los anexos siguen con la asignación vieja (N2). |
| A4 | Importante | S4 no sabe qué postulación fue seleccionada | Corregido |  |
| A5 | Importante | S4 no tiene de dónde sacar el costo de matrícula | Corregido |  |
| A6 | Importante | La regla de HU-01.2 no tiene respaldo | Corregido |  |
| A7 | Importante | Sugerencia de lista de espera en el servicio equivocado | Corregido |  |
| A8 | Importante | Puerto académico, dueños de RF y ranking | Parcial | Ver detalle (junto con B9). |
| A9 | Mejora | El DDD no refleja la separación de Fondos y Becas | Parcial | Ver detalle. |
| B1 | Crítico | Rutas internas en la API pública | Corregido |  |
| B2 | Crítico | El navegador envía el monto a reservar | Corregido |  |
| B3 | Importante | Tres flujos de OTP que se contradicen | Parcial | Flujo unificado; falta completar el contrato público. |
| B4 | Importante | Identidad de quien opera en el cuerpo | Corregido |  |
| B5 | Importante | El lote de universidades usa un id interno | Corregido |  |
| B6 | Importante | Tres diagramas Mermaid no abren | Corregido | Las 12 secuencias compilan. |
| B7 | Mejora | El contrato está casi sin tipar | Corregido | Corregido en lo esencial; ver N3 a N5. |
| B8 | Mejora | Faltan fuentes en el kit | Abierto | La tabla de calidad lo marca corregido, pero los archivos no vienen. |
| B9 | Mejora | Contradicción sobre IFuenteAcademica | Parcial | Ver detalle (junto con A8). |
| D1 | Importante | El agregado se guarda a sí mismo | Corregido |  |
| D2 | Importante | El dominio publica directo a otro servicio | Corregido |  |
| D3 | Importante | Application sin pasar por puertos | Corregido |  |
| D4 | Importante | Mapa de contextos incompleto | Parcial | Ver detalle. |
| D5 | Importante | Diagrama de componentes sin notación UML | Corregido | Interfaces, realización y las 3 llamadas síncronas. |
| D6 | Importante | La migración no carga el presupuesto comprometido | Corregido |  |
| D7 | Importante | Vista general incompleta; capas frente a niveles | Corregido |  |
| D8 | Mejora | Canal de correo y tareas programadas | Corregido |  |
| D9 | Mejora | Vistas por servicio incompletas | Corregido |  |
| D10 | Mejora | El despliegue da por hecho lo no probado | Corregido |  |
| D11 | Mejora | Quién emite el token de las universidades | Corregido |  |
| D12 | Mejora | Listas incompletas | Corregido | El diagrama 07 muestra las 25 raíces. |
| D13 | Mejora | Notación UML | Parcial | Ver detalle. |
| D14 | Mejora | Legibilidad | Parcial | Ver detalle. |
| D15 | Mejora | Nombres de proyectos | Corregido |  |

## Pendientes en detalle (8)

### A2 · La lámina obligatoria sigue como «no recibida»

- **Dónde:** Documento maestro, sección de fuentes
- **Problema:** El documento sigue diciendo que la lámina de decisiones arquitectónicas no se recibió, aunque está en el proyecto (Arquitectura_Obligatoria.jpeg).
- **Corrección:** Incluirla en las fuentes, ponerla primera en la precedencia tecnológica y quitar la mención de que falta.

### A8 / B9 · Las matrices contradicen al maestro

- **Dónde:** MATRICES_ARQUITECTURA_V1.xlsx, hojas HU y RF
- **Problema:** HU-04.1 y RF-04 dicen «S5 fuente académica», pero el maestro define IFuenteAcademicaS3 y dice que S3 no depende de S5. RF-18 tiene dueño S3, pero su única HU (HU-18.1) está asignada a S4 con colaboración «S3 lista de espera». HU-33.2 sigue con «S4 consentimiento», aunque ese agregado es de S6.
- **Corrección:** Alinear las tres filas con el maestro: S3 con su propio puerto académico, RF-18 y HU-18.1 con el mismo dueño, y HU-33.2 con S6.

### A9 · La adenda v3.3 del DDD no viene en el entregable

- **Dónde:** Paquete del entregable
- **Problema:** Se cita como la fuente que registra la separación de Fondos y Becas, pero no se incluye.
- **Corrección:** Incluir la adenda en la carpeta de fuentes.

### B3 · Al contrato público le faltan operaciones

- **Dónde:** openapi_publico.yaml: ChallengeRequest y rutas
- **Problema:** El tipo de operación público no incluye APLICAR_CONSECUENCIA_REVOCATORIA (el interno sí), así que HU-25.2 no se puede completar. RESOLVER_PAGO existe pero ninguna ruta pública lo usa (HU-20.2).
- **Corrección:** Agregar el valor al tipo público y definir la ruta que resuelve el pago.

### B8 · Faltan fuentes en el kit

- **Dónde:** Zip v1.1; qa.json
- **Problema:** La tabla de calidad marca B8 como corregido y cita 06_FUENTES/PRD…docx y DECISIONES_…md, pero esa carpeta no viene en el zip.
- **Corrección:** Se resuelve con N1.

### D4 · Mapa de contextos sin dos relaciones

- **Dónde:** Diagrama 03 (Context Map)
- **Problema:** Faltan la capa anticorrupción hacia los sistemas académicos externos (lado de S5 y S3) y la relación Conformist con el proveedor de identidad.
- **Corrección:** Agregar ambas relaciones con su patrón.

### D13 · El diagrama 08 tiene rótulos viejos

- **Dónde:** Diagrama 08
- **Problema:** Sigue diciendo «Keycloak propuesto», «React / Node» y «alcances program», y cada caja empieza con una línea en blanco.
- **Corrección:** Actualizar los rótulos a las decisiones tomadas y quitar la línea en blanco.

### D14 · Texto cortado en la secuencia 26

- **Dónde:** Vista local de 26_secuencia_caducidad_oferta
- **Problema:** Dos mensajes se salen del borde derecho: «8 Calcular siguiente elegible desde lista de…» y «11 Notificar sugerencia, nunca decide ranking». Verificado midiendo el texto; las otras 11 secuencias están bien.
- **Corrección:** Ensanchar el lienzo o acortar los mensajes.

## Hallazgos nuevos (8)

### N1 · El paquete no trae lo que promete (Crítico)

- **Dónde:** Zip; readme.txt; ABRIR_PRIMERO.html
- **Problema:** Solo trae 5 archivos. Faltan las fuentes (.md, .drawio, CSV, SEGURIDAD_INTERSERVICIOS_V1.1.md, adenda v3.3 y PRD), aunque el readme y el HTML dicen que vienen. La carpeta se llama «V1.4» y el zip «V1.1».
- **Corrección:** Volver a empaquetar con la carpeta de fuentes, un solo número de versión y un inventario que coincida con el contenido.

### N2 · Los anexos no se regeneraron (Crítico)

- **Dónde:** 03_ANEXOS_TECNICOS (solo existe embebido en el HTML)
- **Problema:** Contradicen las matrices corregidas: HU-07.1 en S3, HU-17.1 en S4, HU-25.x en S5, RF-18 en S4 y eventos Beca* sin S3 como consumidor.
- **Corrección:** Regenerarlos desde las matrices, o quitarlos y dejar las matrices como fuente única.

### N3 · Un mismo comando para dos intenciones (Importante)

- **Dónde:** openapi_publico.yaml: SeleccionarCommand
- **Problema:** Se usa para aceptar una oferta y para marcar un periodo. Son intenciones de negocio distintas con un solo nombre, lo que va contra el lenguaje ubicuo.
- **Corrección:** Separarlo en dos comandos con nombre de dominio (por ejemplo, AceptarOferta y MarcarPeriodo).

### N4 · Cifrado de la cuenta del lado del cliente (Importante)

- **Dónde:** openapi_publico.yaml: DatosPagoCommand.cuentaEncriptada
- **Problema:** Supone que el navegador cifra la cuenta: la gestión de claves queda en el cliente y la seguridad depende del navegador.
- **Corrección:** Recibir el dato por TLS y cifrarlo en S4 al guardarlo; el nombre del campo no debe hablar de cifrado.

### N5 · La cédula viaja en la URL (Importante)

- **Dónde:** openapi_interno.yaml: ruta de beneficios
- **Problema:** Es un dato personal que queda en los registros del gateway y de los servicios.
- **Corrección:** Usar el PersonaId interno, o enviar la cédula en el cuerpo de una consulta POST.

### N6 · El evento llega directo al handler (Mejora)

- **Dónde:** Secuencia 11
- **Problema:** No pasa por Outbox, broker ni Inbox, y eso contradice D-6 y el ADR de mensajería.
- **Corrección:** Dibujar Outbox → RabbitMQ → Inbox → handler, como en la secuencia 13.

### N7 · Respuesta 202 tras un guardado síncrono (Mejora)

- **Dónde:** Secuencia 24
- **Problema:** Responde «aceptado, se procesará después» cuando el guardado ya terminó. Además, no se dice quién mantiene la correspondencia entre codigoEstudiante y BecaId.
- **Corrección:** Responder 200/201 si es síncrono, o volverlo asíncrono de verdad. Indicar el dueño de esa correspondencia (propuesta: una tabla de referencia de la capa anticorrupción en S5).

### N8 · La tabla de calidad cita evidencia ausente (Mejora)

- **Dónde:** qa.json
- **Problema:** Varias correcciones remiten a archivos de 06_FUENTES que no vienen en el paquete.
- **Corrección:** Se resuelve con N1.

## Orden sugerido de corrección

1. **Antes de presentar (empaquetado):** N1, B8, A9, N2, N8
2. **Coherencia del contrato:** B3, N3, N4, N5
3. **Coherencia de las matrices:** A8 / B9, A2
4. **Diagramas:** D4, D13, D14, N6, N7

## Lo que está bien

- Las 12 secuencias Mermaid compilan sin errores.
- Todas las referencias internas de los dos contratos resuelven: 28 en el público y 9 en el interno.
- El diagrama 04 muestra interfaces, su realización y las llamadas síncronas S4→S2, S3→S2, OTP y S3→S4.
- El diagrama 07 muestra las 25 raíces de agregado.
- La sección §3.2 del maestro cubre A4 a A7 (evento de selección, costo de matrícula, consumidor de eventos de becas y lista de espera en S3).
- Se mantiene MongoDB como base de lectura de S6, según la decisión del equipo.

## Alcance de la revisión

- Zip ASCENSO_ARQUITECTURA_V1.1 (carpeta interna «V1.4»): documento maestro (24 p.), atlas de 20 diagramas, ABRIR_PRIMERO.html, MATRICES_ARQUITECTURA_V1.xlsx y REPORTE_DE_VERIFICACION.pdf.
- Contenido embebido en el HTML: 20 diagramas DOT/SVG, 12 secuencias Mermaid y sus vistas, PDF de anexos, OpenAPI público (25 operaciones) e interno (5), matrices RF/HU/ADR y la tabla de calidad con las 33 correcciones.
- Comparado contra la revisión del 16-nov (33 hallazgos), el DDD v3.2, las HU Plan B final, el PRD y la lámina de decisiones arquitectónicas.

**Limitaciones**

- No hay código implementado: los hallazgos se refieren al diseño.
- Las fuentes que el paquete dice incluir no se pudieron revisar porque no vienen en el zip.
