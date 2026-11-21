# ASCENSO · Lista de cambios para DDD v3.3 y Propuesta de Arquitectura v0.2

> **Estado (21-nov-2026):** la **Parte B (ADR v0.2) quedó superada** por la arquitectura v1.3 ([`docs/arquitectura/`](../arquitectura/README.md)); lo que sigue abierto de ella está en [`pendientes-v1.3.md`](../arquitectura/pendientes-v1.3.md). De la **Parte A** siguen vigentes DDD-07 a DDD-15, que se trabajan en el cambio [`corregir-ddd-v3-3`](../../openspec/changes/corregir-ddd-v3-3/proposal.md); DDD-01 a DDD-04 ya no hacen falta (la v1.3 decidió MongoDB, Keycloak, Stateless y la partición, y la adenda S2/S4 cubre DDD-04).

**Base revisada:** `DDD_V3.2.md` (04-nov-2026), `ASCENSO_Propuesta_Arquitectura_v0.1.pdf` (04-nov-2026) y `enlaces-mermaid-live-DDD_V3.2.html`.
**Precedencia:** se mantiene la del propio DDD: mandan las HU Plan B (C1–C5, R1–R30); luego el DDD; luego el ADR.

## Cómo usar esta lista

- Cada cambio tiene un **ID** (`DDD-xx` para el modelo, `ADR-xx` para la arquitectura, `EQ-xx` para acciones del equipo), una **prioridad** y la **ubicación** exacta.
- Prioridades:
  - **Bloqueante**: corregir antes de aprobar la versión, porque permite romper una regla del cliente (presupuesto, pagos, concurrencia).
  - **Alta**: incoherencia entre documentos o una regla del cliente sin modelar.
  - **Media**: falta de detalle que se notará al implementar.
  - **Baja**: redacción, referencias o forma.
- Los números de línea son del `DDD_V3.2.md` tal como se entregó; cambian al editar, así que conviene aplicar los cambios de abajo hacia arriba o buscar por el texto citado.

## Resumen

| Prioridad | DDD v3.3 | ADR v0.2 | Equipo |
|---|---|---|---|
| Bloqueante | 1 | 2 | — |
| Alta | 6 | 5 | 2 |
| Media | 6 | 7 | 3 |
| Baja | 2 | 2 | — |
| **Total** | **15** | **16** | **5** |

---

## Parte A · Cambios al DDD (versión 3.3)

### A.1 Cambios que ya pide la sección 5 del ADR (incorporarlos)

El ADR v0.1 ya lista estos cambios; se detalla aquí dónde aplicar cada uno para que no quede ninguno a medias.

#### DDD-01 · Base de consulta en PostgreSQL, no NoSQL · Alta
- **Dónde:** registro de cambios fila 44 (línea 128); sección 18, viñeta "Base de consulta" (línea 4235); diagrama CQRS, clase `BdConsulta` (línea 4286: `NoSQL, desnormalizada`); D-14 (línea 4444); 20.2, fila "Motor de la base de consulta" (línea 4484).
- **Cambio:**
  - `BdConsulta` pasa a "PostgreSQL aparte, desnormalizada, propiedad de S6".
  - D-14 describe los **dos niveles de lectura** del ADR-07: nivel 1, consultas operativas de cada servicio sobre su propio schema (incluido el ranking en S3), y nivel 2, la base de lectura aparte.
  - En 18.2, la columna "Colección" pasa a "Tabla" y el ranking se mueve al nivel 1.
  - Se cierra la fila de 20.2 como resuelta por el ADR-07.
- **Motivo:** restricción del equipo (solo PostgreSQL y Firebase) y HU-32.2 (no guardar estado en Firebase).

#### DDD-02 · Keycloak como proveedor de identidad · Media
- **Dónde:** 3.1, fila "Proveedor de identidad → Identidad y Acceso" (línea 604); D-8 (línea 4438); 20.2, fila "Proveedor de identidad".
- **Cambio:** decir "Keycloak (ADR-09); segundo factor por correo o SMS sujeto a prueba técnica, con plan B en S1". Cerrar la fila de 20.2.

#### DDD-03 · Máquinas de estado con Stateless · Media
- **Dónde:** nota de `Beca` en el diagrama 11b (línea 3266: "patrón State"); texto bajo 15.2 (línea 3802); 16.3 (línea 4012).
- **Cambio:** "Beca y Postulación usan una máquina de estados explícita, implementada con la librería Stateless dentro del agregado (ADR-08); el resto, enum con tabla de transiciones validada". Mantener la mención al patrón State escrito a mano como alternativa si el profesor lo pide.

#### DDD-04 · Mapeo de contextos a servicios · Alta
- **Dónde:** 2.2 (reparto del equipo), 3.1 fila "Becas ↔ Fondos" (línea 600: "viven como módulos de la misma aplicación").
- **Cambio:**
  - Agregar una tabla nueva **"2.3 Contextos y servicios"** con S1 a S6 (contextos, agregados y responsable), copiada del ADR-02.
  - En la fila Becas ↔ Fondos, reemplazar "módulos de la misma aplicación" por "servicios separados (S2 y S4) con reserva síncrona y compensación (ADR-03)".
  - Ajustar 2.2 para que el reparto coincida con la tabla de servicios (ver EQ-03).

#### DDD-05 · API de entrada para universidades · Media
- **Dónde:** 3b, diagrama de integraciones externas.
- **Cambio:** agregar el flujo de entrada: `UNI` → API de S5 (autenticada como `ClienteApiUniversidad`, cliente propio de Keycloak), además de la consulta por `IFuenteAcademica`. Nota: "lotes asíncronos, paginados e idempotentes; solo datos de su universidad (HU-24.3)".

#### DDD-06 · Anexo A de las HU: revocación y presupuesto en transacciones separadas · Baja
- **Dónde:** 20.2, fila "Actualizar el Anexo A de HU-20.1, 22.1, 22.2 y 26.2".
- **Cambio:** dejarla como acción explícita del documento de HU, con responsable y fecha (no es un cambio del modelo).

### A.2 Correcciones nuevas al modelo

#### DDD-07 · Versión explícita en la raíz, incrementada en cada guardado · **Bloqueante**
- **Dónde:** Shared Kernel, `RaizDeAgregado` (línea 672: `+Version: int`; nota en la línea 740); 16.4d, texto bajo el diagrama (línea 4127).
- **Problema:** el ADR-16 propone mapear la versión a la columna de sistema de PostgreSQL (`xmin`). Esa columna solo cambia cuando cambia la **fila de la raíz**. Si dos operaciones modifican hijos distintos de `Beca` (un `Pago` y una `Suspension`), la fila `Beca` no se toca y no hay conflicto: justo el caso "pagar mientras se revoca" que pide el RNF.
- **Cambio:**
  - Mantener `Version: int` en `RaizDeAgregado` y agregar a la nota: "Toda operación que cambie el agregado, aunque solo toque una entidad interna, incrementa la versión de la raíz. La base compara esa versión al guardar".
  - Agregar en 16.1 una invariante transversal: "Cualquier cambio dentro de un agregado incrementa la versión de su raíz".
- **Se coordina con:** ADR-B01.

#### DDD-08 · Un solo disparador para Activa → Inactiva · Alta
- **Dónde:** 15.2 (línea 3792: `Activa --> Inactiva : IniciarNuevoPeriodo() [renovación rechazada o no solicitada]`); 16.2; 16.4e.
- **Problema:** el DDD la dispara al marcar el periodo actual; el ADR-14 la dispara con un proceso de Hangfire "al cerrar cada ventana de renovación". Con los dos, una beca puede inactivarse en momentos distintos según el camino.
- **Cambio:** elegir **uno** y dejarlo igual en ambos documentos. Recomendación:
  - `RechazarRenovacion()` no cambia el estado de la beca: sigue Activa hasta que termine el periodo en curso.
  - **Ambos casos** (rechazada o no solicitada) se resuelven en `IniciarNuevoPeriodo()`, disparado por `PeriodoMarcadoActual` (16.4e). Así la beca nunca queda Inactiva mientras el periodo en curso sigue vigente.
  - El proceso de Hangfire del ADR-14 "Resolver becas no renovadas" se elimina (ver ADR-B03).
- **Validar:** si el cliente espera que la inactivación ocurra al cerrar la ventana (antes del nuevo periodo), invertir la elección en ambos documentos.

#### DDD-09 · Desembolso con fecha dentro de una suspensión (R12) · Alta
- **Dónde:** 15.4 (Desembolso), diagrama 11b (`Beca`), 16.1 fila `Beca` (línea 3964).
- **Problema:** el cliente respondió (R12) que un desembolso con fecha dentro de la suspensión **se paga al reanudar**. El DDD prohíbe pagar en Suspendida, pero no dice qué pasa con ese desembolso.
- **Cambio:**
  - Agregar el estado **`Aplazado`** al desembolso programado: `Programado --> Aplazado : Suspender() [fecha dentro de la suspensión]` y `Aplazado --> Programado : Reanudar()`. También `Aplazado --> Cancelado : Revocar() o Concluir()`.
  - En `Beca.Reanudar()`, la nota: "los desembolsos aplazados vuelven a Programado con fecha de pago al reanudar (R12)".
  - En 16.1, fila `Beca`, agregar: "un desembolso con fecha dentro de una suspensión queda Aplazado y se paga al reanudar (R12)".
  - Actualizar la enumeración `EstadoDesembolso` en el diagrama 11b y en 6.2c.
- **Pendiente que se mantiene:** Anexo B: 12 (si aplica igual a la pausa). Dejar la fila 9 de 20.1 como "R12 resuelto para suspensión; pausa pendiente".

#### DDD-10 · El OTP se vincula a operación y recurso · Alta
- **Dónde:** 3b, `IServicioOtp` (líneas 531-535); 18.1, `IServicioOtp` (línea 4354); 16.4d (`OTP->>Validar(usuarioId, codigo)`); 13 (texto de la línea 3529).
- **Problema:** la firma `Validar(usuarioId, codigo)` permite reutilizar un código pedido para otra operación. El ADR-10 propone vincularlo, pero el DDD no lo refleja.
- **Cambio:**
  - `+Enviar(usuarioId: UsuarioId, operacion: TipoOperacionSensible, recursoId: string, canal: CanalCodigo) void`
  - `+Validar(usuarioId: UsuarioId, operacion: TipoOperacionSensible, recursoId: string, codigo: string) bool`
  - Nueva enumeración `TipoOperacionSensible`: `CONFIRMAR_PAGO`, `RESOLVER_PAGO`, `DATOS_PAGO`, `REVOCAR_BECA`, `CONSECUENCIA_QUE_REVOCA` (la lista del ADR-10).
  - Actualizar las llamadas de 16.4d y del diagrama 18.1.
  - En 13, agregar las reglas R4 como invariantes de S1: vigencia de 15 minutos, un solo uso, 3 intentos y bloqueo de 1 hora, reenvío permitido, código guardado como hash.

#### DDD-11 · Registro de cuenta entre Identidad y Postulación · Alta
- **Dónde:** 9a (Postulante), 13 (Usuario), 16.4 (agregar un flujo **f**).
- **Problema:** registrarse crea un `Usuario` (S1, Keycloak) y un `Postulante` (S3), y la cédula debe ser única en los dos (R1). El modelo no dice quién manda ni qué pasa si el segundo paso falla.
- **Cambio:** agregar el flujo **16.4f Registro del postulante**:
  1. S1 crea el `Usuario` (tipo Postulante) y su cuenta en Keycloak; la cédula única la garantiza S1 (`IUsuarioRepository.ObtenerPorCedula`).
  2. S1 publica `UsuarioRegistrado`.
  3. S3 crea el `Postulante` al consumir el evento (idempotente por `UsuarioId`).
  4. Mientras no exista el `Postulante`, el frontend muestra "completando el registro".
  - Dejar dicho que **S1 es la fuente de verdad** de la identidad y Keycloak, de las credenciales.
- **Agregar** `UsuarioRegistrado` a la tabla 16.2 y actualizar los totales de 6.3.

#### DDD-12 · Bitácora de lectura de datos de pago · Media
- **Dónde:** 11a (`DatosPago`), 16.1.
- **Problema:** HU-20.3 pide registrar quién consulta los datos de pago. El ADR-12 lo menciona como "auditoría de lectura", pero el modelo no tiene dónde guardarlo.
- **Cambio:** agregar a `DatosPago` el método `RegistrarConsulta(usuarioId, motivo)`, que deja un registro de lectura (no un `RegistroDecision`), y la invariante "toda consulta de los datos completos deja registro".

#### DDD-13 · Cifrado de la cuenta bancaria · Media
- **Dónde:** `CuentaBancaria` (líneas 1461 y 2869).
- **Cambio:** el número de cuenta pasa a un VO **`NumeroCuentaCifrado`** con método `Enmascarado()` (últimos dígitos), en línea con el ADR-18. Nota: "Se guarda cifrado; solo se descifra para el pago".

#### DDD-14 · Context Map: Trazabilidad no es intermediaria · Media
- **Dónde:** 3a (líneas 469-470: `TRZ "U" --> "D" COM` y `TRZ "U" --> "D" REP`); 3.1 fila "Trazabilidad → Comunicaciones y Reportería" (línea 607).
- **Problema:** en el ADR los eventos de integración salen directo de cada servicio hacia S6; Trazabilidad es un componente dentro de cada servicio. El diagrama sugiere un servicio intermedio.
- **Cambio:** reemplazar esas dos relaciones por "contextos de negocio → Comunicaciones y Reportería: Published Language (eventos de integración)" y describir Trazabilidad como el registro que cada contexto escribe en su propia transacción.

### A.3 Referencias y forma

#### DDD-15 · Referencias desactualizadas · Baja
- Línea 3964 (16.1, fila `Beca`): "Solo transiciones de **14.2**" → "**15.2**".
- Línea 3993 (16.2, `BecaRevocada`): "(**16.4b**)" → "(**16.4d**)".
- Después de aplicar todos los cambios, actualizar **6.3 Totales** (eventos, enumeraciones, VO), el **control de versiones** (fila 3.3) y el **registro de cambios** (nueva tabla "Cambios de la versión 3.3" con los IDs de esta lista).

---

## Parte B · Cambios a la Propuesta de Arquitectura (v0.2)

### Bloqueantes

#### ADR-B01 · ADR-16: versión explícita, no `xmin` · **Bloqueante**
- **Dónde:** ADR-16, primera viñeta de la decisión.
- **Cambio:** "La versión de cada raíz es una columna `Version` propia, marcada como token de concurrencia en EF Core. Toda operación sobre el agregado la incrementa, aunque solo cambie una entidad interna (DDD-07)". Agregar a la lista de pruebas: "dos operaciones concurrentes sobre hijos distintos de la misma `Beca` deben chocar".
- **Motivo:** `xmin` no cambia si solo cambia una fila hija; el caso pago contra revocación quedaría sin protección.

#### ADR-B02 · ADR-03: la liberación de reservas sin vincular debe ser segura · **Bloqueante**
- **Dónde:** ADR-03, viñeta "Red de seguridad"; ADR-14, proceso "Liberar reservas sin vincular".
- **Problema:** si el evento `OfertaEmitida` llega tarde (RabbitMQ caído, Outbox atrasado) y vence el plazo, el proceso libera una reserva que sí tiene oferta: el disponible queda mayor de lo real y se puede sobregirar.
- **Cambio:**
  - Antes de liberar, S2 **pregunta a S4** si existe la oferta o la solicitud de renovación de esa clave. Solo libera si S4 responde que no existe; si S4 no responde, **no libera** y reintenta después (falla segura).
  - Agregar esta llamada a la **lista cerrada de llamadas síncronas** del ADR-02 (S2 → S4, consulta de existencia).
  - Fijar el plazo de vinculación con margen sobre el atraso máximo del Outbox (por ejemplo, 30 minutos) y monitorear las reservas sin vincular.

### Alta

#### ADR-B03 · ADR-14: quitar "Resolver becas no renovadas" · Alta
- **Dónde:** ADR-14, tabla de procesos; ADR-08, fila T9.
- **Cambio:** según DDD-08, la transición la hace `IniciarNuevoPeriodo()` al consumir `PeriodoMarcadoActual`. Eliminar la fila del proceso programado y ajustar T9: "Al iniciar el nuevo periodo, si la renovación fue rechazada o no se solicitó (C3, R18a)". Actualizar la sección 4 ("cinco procesos más" pasa a cuatro).

#### ADR-B04 · ADR-08: estado Aplazado en desembolsos · Alta
- **Dónde:** ADR-08, lista de entidades con enum; ADR-14.
- **Cambio:** mencionar que el desembolso programado incluye `Aplazado` (DDD-09, R12) y que `Reanudar()` lo devuelve a Programado. No requiere proceso programado.

#### ADR-B05 · ADR-02: tiempos límite y reintentos en las llamadas síncronas · Alta
- **Dónde:** ADR-02 y ADR-03.
- **Cambio:** agregar "Toda llamada síncrona entre servicios tiene tiempo límite (por ejemplo, 3 s), reintentos acotados solo para operaciones idempotentes y corte de circuito (Polly / `Microsoft.Extensions.Http.Resilience`). Si se agota, la operación se rechaza (falla segura)". Sin esto, una caída lenta de S2 o S1 bloquea hilos en S4.

#### ADR-B06 · ADR-09: registro de cuenta y fuente de verdad · Alta
- **Dónde:** ADR-09.
- **Cambio:** describir el flujo de DDD-11 (S1 crea Usuario y cuenta en Keycloak, publica `UsuarioRegistrado`, S3 crea el Postulante). Dejar dicho: "S1 es la fuente de verdad de usuarios, tipos y alcance; Keycloak guarda credenciales y segundo factor. Los cambios de rol se hacen en S1 y se sincronizan a Keycloak".

#### ADR-B07 · ADR-10: alinear con el DDD · Alta
- **Dónde:** ADR-10 y sección 5.
- **Cambio:** agregar a la sección 5 la fila "Sección 3b, 13, 16.4d y 18.1: firma de `IServicioOtp` con operación y recurso (DDD-10)". En el ADR-10, nombrar la enumeración `TipoOperacionSensible` y su lista.

### Media

#### ADR-B08 · ADR-06: cliente de almacenamiento en .NET y emulador · Media
- **Dónde:** ADR-06, decisión y riesgos; sección 6.
- **Cambio:** el SDK de Firebase Admin para .NET no trae módulo de Storage; el adaptador usa el cliente de Google Cloud Storage para .NET. Agregar una **prueba técnica** (1 día): subir, descargar y generar un enlace firmado contra el emulador de Firebase Storage y contra la cuenta real. Si el emulador no firma enlaces, el adaptador sirve la descarga a través de S3 o S4 tras autorizar, solo en desarrollo.

#### ADR-B09 · ADR-17: cómo se guardan las fechas · Media
- **Dónde:** ADR-17, tercera viñeta.
- **Cambio:** reemplazar "Las fechas se guardan con su zona horaria explícita" por "Los instantes se guardan en UTC (`timestamptz`); las fechas límite del negocio se interpretan en la zona `America/Caracas` (UTC-4, sin horario de verano) a través de `FechaLimite` y `VentanaFechas`". PostgreSQL no conserva la zona original.

#### ADR-B10 · ADR-12 y ADR-07: exportación para auditoría · Media
- **Dónde:** ADR-12, viñeta de la línea de tiempo; ADR-07.
- **Cambio:** la consulta del registro (HU-35.2) puede leer la base de lectura, pero la **exportación para auditoría (HU-36.1)** se arma desde las tablas `RegistroDecision` de cada servicio, que son las inmutables. Opcional: una huella (hash) por fila encadenada con la anterior, para detectar alteraciones hechas con el usuario dueño del schema.

#### ADR-B11 · ADR-12: bitácora de lectura de datos de pago · Media
- **Dónde:** ADR-12, viñeta "Aparte".
- **Cambio:** decir dónde vive (tabla propia en el schema de S4, solo inserción), qué guarda (quién, cuándo, qué beca, motivo) y quién la consulta.

#### ADR-B12 · Nueva sección: observabilidad y operación · Media
- **Dónde:** nuevo ADR-25 (o subsección del ADR-23).
- **Cambio:** registro estructurado con identificador de correlación de punta a punta (gateway, servicios, mensajes), verificación de salud de cada servicio y de RabbitMQ, panel de la cola de errores de MassTransit y del panel de Hangfire, y respaldo diario de PostgreSQL para la demostración.

#### ADR-B13 · Nueva sección: arquitectura del frontend · Media
- **Dónde:** nuevo ADR-26.
- **Cambio:** decidir si Next.js trabaja con renderizado en servidor o como SPA, dónde vive el token (preferible en el servidor de Next.js, con cookie segura) y si Next.js actúa como BFF frente al gateway. Hoy no hay ninguna decisión del frontend.

#### ADR-B14 · Nueva sección: vista de despliegue de la demostración · Media
- **Dónde:** ADR-02 o nuevo ADR-27.
- **Cambio:** dónde corre la demostración (máquina local, servidor de la universidad o nube), con qué `docker-compose`, cómo se cargan los datos semilla y el script del profesor, y qué cambia al usar la cuenta real de Firebase.

### Baja

#### ADR-B15 · ADR-24: MediatR opcional · Baja
- **Dónde:** ADR-04 y ADR-24.
- **Cambio:** agregar como alternativa "despachador propio de comandos y eventos (pocas decenas de líneas), sin dependencia de licencia". Decidir en la revisión.

#### ADR-B16 · Sección 5 y sección 6 al día · Baja
- **Cambio:** agregar a la sección 5 las filas de DDD-07 a DDD-14, y a la sección 6 las pruebas técnicas nuevas (EQ-01) y la decisión de DDD-08.

---

## Parte C · Acciones del equipo y preguntas

| ID | Prioridad | Acción | Responsable | Para cuándo |
|---|---|---|---|---|
| EQ-01 | Alta | **Pruebas técnicas:** (a) segundo factor por correo y SMS en Keycloak (ADR-09); (b) enlaces firmados contra el emulador de Firebase (ADR-B08). Si alguna falla, se aplica su plan B antes de cerrar la v0.2. | Integrante 1 + un apoyo | Antes de la revisión con el profesor |
| EQ-02 | Alta | **Llevar al profesor** las 6 preguntas de la sección 6 del ADR, empezando por la 1 (12 contextos en 6 servicios) y la 2 (base de lectura PostgreSQL). Si rechaza alguna, cambia la forma de la arquitectura. | Equipo | Próxima clase |
| EQ-03 | Media | **Repartir la carga:** hoy el Integrante 1 tiene S1, S2, Trazabilidad y el panel (los dos servicios críticos). Propuesta: mover S1 o la parte de Catálogo de S2 a otra persona y nombrar al **responsable de integración de S6**. Ajustar el reparto 2.2 del DDD. | Equipo | Con la v0.2 |
| EQ-04 | Media | **Retirar el kit del 01-nov** (`ASCENSO ARQUITECTURA KIT`): su arquitectura de un solo backend contradice la lámina obligatoria de microservicios y su modelo es anterior al DDD v3.2. Dejar una nota que remita al DDD v3.3 y al ADR v0.2. | Quien publicó el kit | Inmediato |
| EQ-05 | Media | **Primer recorrido de punta a punta:** postular → evaluar → ofertar con reserva → aceptar → crear beca → pagar con OTP, con los seis servicios mínimos, antes de profundizar en cualquier módulo. | Equipo | Primer sprint de código |

**Preguntas al cliente que salen de esta lista** (agregarlas al Anexo B si no están):

1. ¿La beca no renovada pasa a Inactiva al cerrar la ventana de renovación o al empezar el nuevo periodo? (DDD-08)
2. ¿La regla de R12 (se paga al reanudar) aplica igual a la pausa? (DDD-09; Anexo B: 12)

---

## Parte D · Verificación después de aplicar los cambios

- [ ] Los diagramas Mermaid del DDD v3.3 se dibujan sin errores (hoy son 44 y todos se dibujan).
- [ ] Regenerar `enlaces-mermaid-live-DDD_V3.3.html` y comprobar que cada enlace contiene el mismo código que el `.md`.
- [ ] 6.3 Totales coincide con lo dibujado (eventos, enumeraciones, value objects y máquinas de estado).
- [ ] Ninguna referencia interna apunta a una sección que no existe (buscar "14.", "16.4b").
- [ ] Las transiciones de Beca son idénticas en el DDD 15.2 y en la tabla T1–T14 del ADR-08.
- [ ] La lista cerrada de llamadas síncronas del ADR-02 incluye la consulta S2 → S4 de ADR-B02.
- [ ] La sección 5 del ADR lista todos los cambios DDD-01 a DDD-14.
