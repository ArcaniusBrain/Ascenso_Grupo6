# Revisión de la Arquitectura v1.0 — Resumen de hallazgos

ASCENSO · Plataforma de becas de la Fundación Ascenso · 16 de noviembre de 2026

**33 hallazgos:** 3 críticos, 17 importantes y 13 mejoras.

## Veredicto

La estructura de la arquitectura es coherente con el PRD, las HU y el DDD v3.2: seis servicios alineados con los Bounded Contexts, cuatro capas Clean, dependencias hacia el dominio, reserva de presupuesto primero (D-1), aceptación dentro de S4 (D-3) y estado, RegistroDecision y Outbox en una sola transacción (D-6). Los 33 hallazgos están en los detalles: contradicciones con la lámina obligatoria, huecos entre servicios, contratos inseguros y diagramas que no reflejan el texto. Ninguno obliga a rediseñar; todos se corrigen en una v1.1.

## Decisiones del equipo registradas

- 16-nov-2026: el equipo mantiene MongoDB como base de lectura (CQRS), conforme al D-14 del DDD v3.2. Deja de ser un hallazgo crítico; queda como mejora de consistencia (A1).

## Tabla general

Prefijos: A = documento de arquitectura · B = archivos del kit (contratos, matrices, Mermaid) · D = diagramas.

| ID | Severidad | Hallazgo |
|---|---|---|
| A2 | Crítico | La lámina obligatoria se trata como no disponible |
| B1 | Crítico | Las rutas internas entre servicios están en la API pública |
| B2 | Crítico | El navegador envía el monto a reservar |
| A3 | Importante | Cuatro HU asignadas a un servicio que no es dueño del agregado |
| A4 | Importante | S4 no sabe qué postulación fue seleccionada y puede emitir dos ofertas |
| A5 | Importante | S4 no tiene de dónde sacar el costo de matrícula |
| A6 | Importante | La regla de no postular con beca activa (HU-01.2) no tiene respaldo |
| A7 | Importante | La sugerencia de la lista de espera está en el servicio equivocado |
| A8 | Importante | Puerto académico, dueños de RF y ranking |
| B3 | Importante | Tres flujos de OTP que se contradicen |
| B4 | Importante | La identidad de quien opera viaja en el cuerpo de la petición |
| B5 | Importante | El lote de las universidades usa un id interno |
| B6 | Importante | Tres diagramas Mermaid no abren en Mermaid Live |
| D1 | Importante | El agregado se guarda a sí mismo |
| D2 | Importante | El dominio publica directo a otro servicio |
| D3 | Importante | Application llama a otros servicios y a la base sin pasar por puertos |
| D4 | Importante | El mapa de contextos está incompleto |
| D5 | Importante | El diagrama UML de componentes no cumple la notación definida |
| D6 | Importante | La migración no carga el presupuesto comprometido |
| D7 | Importante | La vista general está incompleta y confunde capas con niveles |
| A1 | Mejora | El kit no dice MongoDB en todas partes |
| A9 | Mejora | El DDD no refleja la separación de Fondos y Becas |
| B7 | Mejora | El contrato está casi sin tipar |
| B8 | Mejora | Faltan fuentes en el kit |
| B9 | Mejora | Contradicción sobre IFuenteAcademica |
| D8 | Mejora | Canal de correo y tareas programadas |
| D9 | Mejora | Vistas por servicio incompletas |
| D10 | Mejora | El despliegue da por hecho lo que no está probado |
| D11 | Mejora | Quién emite el token de las universidades |
| D12 | Mejora | Listas incompletas |
| D13 | Mejora | Notación UML |
| D14 | Mejora | Legibilidad |
| D15 | Mejora | Nombres de proyectos |

## Críticos (3)

### A2 · La lámina obligatoria se trata como no disponible

- **Dónde:** Maestro §1.2, §16, ADR-01/02/23; DECISIONES_Y_BLOQUEOS.csv; diagramas 01 y DIAGRAMA_CAPAS
- **Problema:** El documento dice que la lámina «Decisiones Arquitectónicas» no está adjunta y deja «por ratificar» lo que esa lámina fija: microservicios, React + Next.js, .NET, PostgreSQL + EF Core, Firebase, máquinas de estado, auditoría y ETL. OpenSpec también figura como dudoso, aunque en la clase del 14-sep el profesor lo declaró obligatorio (sin OpenSpec, la nota del proyecto es cero).
- **Corrección:** Poner la lámina primera en la precedencia para tecnología, marcar esas decisiones como obligatorias y cerrar las preguntas P0 correspondientes.

### B1 · Las rutas internas entre servicios están en la API pública

- **Dónde:** openapi.yaml: reservas, liberar y /otp/consume
- **Problema:** Usan el mismo token de usuario que las rutas públicas. Un usuario autenticado podría liberar el presupuesto de una oferta vigente, lo que rompe la regla 8 y el protocolo D-1.
- **Corrección:** Separar un contrato interno que no se publique en YARP, y exigir credenciales de cliente con un permiso propio para la llamada de S4 a S2.

### B2 · El navegador envía el monto a reservar

- **Dónde:** openapi.yaml: OfertaRequest.planReserva
- **Problema:** El monto lo define el programa (Programa.MontoPorPeriodo, regla C1). Tal como está, el coordinador decidiría cuánto se reserva.
- **Corrección:** Quitar planReserva de la petición: el servidor calcula el plan a partir del programa.

## Importantes (17)

#### Grupo: Servicios y datos

### A3 · Cuatro HU asignadas a un servicio que no es dueño del agregado

- **Dónde:** Anexo A; MATRIZ_73_HU_ARQUITECTURA.csv
- **Problema:** HU-07.1 (rúbrica), HU-17.1 (compromiso requerido) y HU-25.1 (política de consecuencias) viven en el agregado Programa, de S2. HU-25.2 modifica la Beca y exige OTP, así que pertenece a S4.
- **Corrección:** Reasignar HU-07.1, HU-17.1 y la definición de la política de HU-25.1 a S2, y HU-25.2 a S4. S5 conserva la evaluación de cumplimiento.

### A4 · S4 no sabe qué postulación fue seleccionada y puede emitir dos ofertas

- **Dónde:** Maestro §5.1 paso 2; secuencia 11; ReservaRequest
- **Problema:** Ningún evento comunica la selección (HU-09.2, HU-10.1), y la consulta a S3 no está en la lista de llamadas síncronas (§3.1). Además, la clave de reserva es el id de la oferta, así que dos emisiones simultáneas para la misma postulación reservan dos veces.
- **Corrección:** Evento PostulacionSeleccionada; clave de reserva por postulación y ciclo; S4 garantiza una sola oferta pendiente por postulación.

### A5 · S4 no tiene de dónde sacar el costo de matrícula

- **Dónde:** Secuencia 12; maestro §3.1
- **Problema:** La regla C2 (c) necesita el costo de matrícula vigente, que vive en UniversidadAliada (S2). No hay evento ni llamada definida para obtenerlo.
- **Corrección:** Replicarlo a S4 por evento, con su vigencia.

### A6 · La regla de no postular con beca activa (HU-01.2) no tiene respaldo

- **Dónde:** Maestro §3.1; Anexo C
- **Problema:** S3 no aparece como consumidor de los eventos de becas, y BeneficiosPersona solo protege las reglas (b) y (c) de C2, no la de HU-01.2.
- **Corrección:** Agregar a S3 como consumidor y dejar por escrito qué pasa si su copia está atrasada: consulta síncrona a S4 o riesgo aceptado.

### A7 · La sugerencia de la lista de espera está en el servicio equivocado

- **Dónde:** Secuencia 26
- **Problema:** La hace S6, pero el DDD §18 exige decidirla con datos transaccionales, y la lista de espera vive en Postulacion (S3).
- **Corrección:** Que S3 calcule la sugerencia y S6 solo la notifique.

### A8 · Puerto académico, dueños de RF y ranking

- **Dónde:** MATRIZ_PUERTOS.md; Anexo A; maestro §7.2
- **Problema:** IFuenteAcademica solo existe en S5, pero S3 la necesita para el promedio (HU-04.1) y no hay llamada de S3 a S5. RF-18 y RF-28 tienen a S4 como dueño, aunque sus proyecciones están en S6. RF-33 cita «S4 consentimiento», pero ese agregado es de S6. El ranking aparece en S6, cuando HU-09.2 pide generarlo a pedido en S3, consultando el disponible de S2.
- **Corrección:** Puerto propio en S3, corregir los dueños de RF-18, RF-28 y RF-33, y generar el ranking en S3.

#### Grupo: Contratos

### B3 · Tres flujos de OTP que se contradicen

- **Dónde:** DDD §16.4d; secuencia 14; openapi.yaml
- **Problema:** En el DDD lo valida el handler; en la secuencia 14, la API antes del handler; en el OpenAPI, el navegador lo consume antes y solo envía el identificador del desafío, sin ninguna regla que impida reutilizarlo. Además, al tipo de operación le falta la consecuencia que revoca (HU-25.2).
- **Corrección:** Un solo flujo: S4 recibe el código, y el handler lo valida en S1 después de verificar permiso y estado (el diagrama 17 ya muestra ese orden). Agregar la operación de HU-25.2.

### B4 · La identidad de quien opera viaja en el cuerpo de la petición

- **Dónde:** PostulacionRequest.postulanteId; EvaluacionRequest.evaluadorId
- **Problema:** Permite postular o evaluar a nombre de otra persona.
- **Corrección:** Tomar la identidad del token (HU-37.1, HU-39.1).

### B5 · El lote de las universidades usa un id interno

- **Dónde:** openapi.yaml: ReporteAcademicoLote
- **Problema:** Cada reporte se identifica por becaId, un identificador que la universidad no conoce.
- **Corrección:** Identificar por cédula o código de estudiante, y que la capa anticorrupción lo traduzca (C5, HU-24.3).

### B6 · Tres diagramas Mermaid no abren en Mermaid Live

- **Dónde:** Secuencias 12, 13 y 26, sus 6 enlaces y MANIFIESTO_ENTREGA.md
- **Problema:** El punto y coma dentro del texto se interpreta como fin de instrucción. El manifiesto afirma que los 12 funcionan.
- **Corrección:** Reemplazar «;» por «,», regenerar los 6 enlaces y corregir el manifiesto.

#### Grupo: Diagramas

### D1 · El agregado se guarda a sí mismo

- **Dónde:** 20_registro_outbox
- **Problema:** La flecha va del agregado a la transacción de PostgreSQL. Es la violación 2 de la lámina 35 de la Clase 09, y contradice la secuencia 13.
- **Corrección:** Mostrar que el handler guarda mediante el repositorio.

### D2 · El dominio publica directo a otro servicio

- **Dónde:** 12_s3_postulacion; 13_s4_becas
- **Problema:** El dominio aparece enviando eventos a S6. El dominio solo registra eventos; quien publica es Infrastructure, mediante el Outbox.
- **Corrección:** Mover esas flechas a Infrastructure.

### D3 · Application llama a otros servicios y a la base sin pasar por puertos

- **Dónde:** 13_s4_becas (S2 y S1); 14_s6_lecturas (MongoDB)
- **Problema:** El propio kit define los puertos IReservaPresupuestoRemota, IValidadorOtp e IConsultaPanel, pero el diagrama no los usa. La lámina 14 de la Clase 09 dice que la aplicación solo conoce puertos.
- **Corrección:** Dibujar cada puerto en Application y su adaptador en Infrastructure.

### D4 · El mapa de contextos está incompleto

- **Dónde:** 03_particion_contextos
- **Problema:** De las 15 relaciones del DDD §3.1, solo 2 llevan el nombre de su patrón. Faltan, entre otras: Catálogo como proveedor de Postulación, Becas y Seguimiento; Fondos como proveedor de Evaluación (rúbrica); Identidad como servicio abierto y su conformidad con Keycloak; y las capas anticorrupción académica y de Firebase. El título dice «servicios candidatos».
- **Corrección:** Completar las 15 relaciones con su patrón y quitar «candidatos».

### D5 · El diagrama UML de componentes no cumple la notación definida

- **Dónde:** 04_componentes_uml; DIAGRAMA_COMPONENTES.drawio
- **Problema:** El 04 no tiene ninguna interfaz (el maestro §4.1 lo exige), le faltan las tres llamadas síncronas (S4 a S2, S3 a S2, OTP en S1) y tiene un bloque de persistencia colgado del bus. En el draw.io, «provee» se dibuja con flecha de dependencia, y solo S4 publica y solo S6 consume.
- **Corrección:** Interfaces ofrecidas y requeridas, las tres llamadas síncronas, y «provee» como realización.

### D6 · La migración no carga el presupuesto comprometido

- **Dónde:** 18_migracion; secuencia 27
- **Problema:** Las becas históricas se cargan en S4, pero ningún paso actualiza lo comprometido en PresupuestoPrograma (S2) ni reconstruye las proyecciones de S6. El día de la migración, el disponible quedaría inflado (regla 8).
- **Corrección:** Agregar la carga del presupuesto comprometido en S2 y la reconstrucción de proyecciones, y conciliar.

### D7 · La vista general está incompleta y confunde capas con niveles

- **Dónde:** 01_tres_niveles; DIAGRAMA_CAPAS.drawio
- **Problema:** En el 01 faltan la conexión de S6 con PostgreSQL, las de S2, S3 y S5 con RabbitMQ, y Keycloak. DIAGRAMA_CAPAS solo conecta S4 con PostgreSQL, no tiene RabbitMQ ni Keycloak, y llama «capas» a lo que son niveles (lámina 9 de la Clase 09).
- **Corrección:** Completar las conexiones y renombrar el diagrama a «niveles».

## Mejoras (13)

### A1 · El kit no dice MongoDB en todas partes

- **Dónde:** Diagramas 01, 06 y DIAGRAMA_CAPAS; DECISIONES_Y_BLOQUEOS.csv; maestro §16 y ADR-07
- **Problema:** Los diagramas dicen «Motor por aprobar/definir», y la pregunta sobre el motor sigue abierta.
- **Corrección:** Rotular MongoDB, cerrar esa pregunta y dejar dicho en el ADR-07 que es una decisión del equipo frente a la lámina («FireBase con PostgreSQL»).

### A9 · El DDD no refleja la separación de Fondos y Becas

- **Dónde:** DDD v3.2 §3.1
- **Problema:** El DDD dice que Becas y Fondos son «módulos de la misma aplicación», y la arquitectura los separa en dos servicios.
- **Corrección:** Registrarlo en el DDD v3.3.

### B7 · El contrato está casi sin tipar

- **Dónde:** openapi.yaml
- **Problema:** Las 27 operaciones responden 200 con la misma respuesta genérica, los errores no usan ProblemDetails, el lote de S5 no responde 202 y 13 operaciones POST no tienen cuerpo.
- **Corrección:** Tipar respuestas y errores y completar los cuerpos.

### B8 · Faltan fuentes en el kit

- **Dónde:** 06_FUENTES/
- **Problema:** No están la lámina obligatoria ni el PRD. Es la causa raíz de A2.
- **Corrección:** Agregarlos.

### B9 · Contradicción sobre IFuenteAcademica

- **Dónde:** CATALOGO_PUERTOS_ADAPTADORES.csv frente a MATRIZ_PUERTOS.md y el maestro
- **Problema:** Uno dice «S5 (y S3 según flujo)»; los otros, solo S5.
- **Corrección:** Unificar (ver A8).

### D8 · Canal de correo y tareas programadas

- **Dónde:** 09_operacion_eventos
- **Problema:** El correo aparece como canal «offline», cuando es el canal mínimo obligatorio para todos los hitos (HU-29.1). La lista de tareas omite los recordatorios de S6 (RF-30, RF-31) y otras cuatro.
- **Corrección:** Corregir el rótulo y completar la lista.

### D9 · Vistas por servicio incompletas

- **Dónde:** 11_s2_fondos; 12_s3_postulacion; 13_s4_becas
- **Problema:** S2 no muestra que recibe eventos ni el reconciliador. S3 no muestra Firebase (RF-03), ni que recibe eventos de S4, y solo 1 de sus 6 repositorios. S4 no muestra EmitirOferta, DatosPago, VentanaRenovacion ni Firebase.
- **Corrección:** Completar las tres vistas.

### D10 · El despliegue da por hecho lo que no está probado

- **Dónde:** 15_despliegue
- **Problema:** Rotula Keycloak con «2FA integrado», aunque la prueba técnica está pendiente (§8.1). Faltan la conexión del navegador a Keycloak, el correo de pruebas y el emulador de Firebase. Aquí dice Next.js; en el 01, «Next.js propuesto».
- **Corrección:** Ajustar rótulos y completar.

### D11 · Quién emite el token de las universidades

- **Dónde:** Secuencia 24
- **Problema:** El token lo emite S1, pero el ADR-09 dice que cada universidad es un cliente de Keycloak.
- **Corrección:** Alinear con el ADR-09.

### D12 · Listas incompletas

- **Dónde:** 07_propiedad_datos; 06_datos_cqrs
- **Problema:** En el 07 faltan 6 de los 25 agregados (Donante, ClienteApiUniversidad, CicloPostulacion, ConflictoInteres, VentanaRenovacion, PlazoReporte). El 06 dice «Handlers S1–S5», pero S6 también tiene comandos.
- **Corrección:** Completar.

### D13 · Notación UML

- **Dónde:** 08, 09, 16, 17, 18, 05 y secuencia 14
- **Problema:** Los diagramas 08, 09, 16, 17 y 18 dibujan pasos de un flujo con el símbolo de componente. El 05 usa flecha rellena para dependencia. En la secuencia 14, el error 409 va del repositorio a la API saltándose el handler.
- **Corrección:** Diagrama de actividad para los flujos, flecha abierta para dependencias y respuesta a través del handler.

### D14 · Legibilidad

- **Dónde:** 19; vistas locales 11, 25, 26 y 28; 05
- **Problema:** Errata «REGRA» en el 19; números de paso encimados con el texto; un mensaje pegado al borde en el 26; líneas de realización que cruzan todo el 05.
- **Corrección:** Corregir errata y espaciado.

### D15 · Nombres de proyectos

- **Dónde:** Diagrama 02; árbol del repositorio §12; prueba de arquitectura
- **Problema:** Hay tres convenciones: Ascenso.Service.Domain, S4.Scholarships/*.Api (en inglés) y Ascenso.S4.Application.
- **Corrección:** Elegir una sola convención con términos del lenguaje ubicuo.

## Orden sugerido de corrección

1. **Antes de presentar al profesor:** A2, A1, B8, B6, D1, D2, D3, D7
2. **Antes de programar dinero y seguridad:** B1, B2, B3, A4, A5, D6
3. **Antes de cerrar matrices y contratos:** A3, A6, A7, A8, B4, B5, D4, D5
4. **En la v1.1:** A9, B7, B9, D8, D9, D10, D11, D12, D13, D14, D15

## Lo que está bien

- Las flechas de dependencia de los diagramas 02, 10 y 19 apuntan todas hacia el dominio (verificado en el código fuente).
- Las 25 raíces de agregado, los 34 eventos, los 40 RF y las 73 HU coinciden con el DDD v3.2 y el índice de HU.
- Se aplican D-1 (reservar primero en S2), D-3 (aceptación dentro de S4) y D-6 (estado, RegistroDecision y Outbox en una sola transacción).
- Reglas del cliente bien recogidas: C3 (la beca no renovada pasa a Inactiva), R4 (OTP de 15 minutos, 3 intentos y 1 hora de espera) y R20 (23:59 en UTC-4).
- El reconciliador no libera reservas solo porque venció un plazo, y los eventos usan un formato común, Outbox e Inbox.
- IncumplimientoDetectado no revoca automáticamente y BecaInactivada no libera cupo ni presupuesto, como pide el DDD.
- La suite de 05_CALIDAD incluye la prueba de arquitectura de la lámina 31 de la Clase 09.
- Diagramas 16 y 17 y secuencias 21, 22, 23, 25 y 28 correctos. El 17 tiene el orden correcto de autorización, OTP y comando.

## Alcance de la revisión

- Documento maestro, anexos y atlas de la Arquitectura v1.0 (PDF y Markdown).
- Contratos: openapi.yaml, EVENTOS_E_INTERFACES.md y MATRIZ_PUERTOS.md.
- Matrices: los 9 CSV y el XLSX (40 RF, 73 HU, 25 agregados, 34 eventos, 24 ADR).
- Calidad: PRUEBAS_Y_CRITERIOS.md.
- Diagramas: los 20 del atlas, los 2 de draw.io y las 12 secuencias, tanto en Mermaid como en sus vistas locales (46 dibujos revisados como imagen).
- Comparados contra: Historias de Usuario Plan B final, DDD v3.2, PRD, lámina «Decisiones Arquitectónicas» del profesor y Clase 09.

**Limitaciones**

- Los 2 diagramas draw.io se revisaron por su contenido y relaciones; su diagramación no se juzgó, porque se dibujaron con una aproximación de su geometría.
- No hay código implementado: los hallazgos se refieren al diseño.
