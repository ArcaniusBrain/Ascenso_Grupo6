# Design — `implementar-registro-decisiones` (RF-35)

## 1. Contexto y fuentes de diseño

- **Historias de usuario:** `HU-35.1` (8 escenarios base + cierre de *Inactiva*) y `HU-35.2` (4 escenarios base + cierre de alcances de consulta) en `docs/requisitos/historias-de-usuario/area-6-comunicaciones-reporteria-y-cumplimiento.md`, más sus reglas técnicas en `anexo-a-reglas-tecnicas.md` y preguntas abiertas en `anexo-b-preguntas-abiertas.md`.
- **Modelo de dominio (`DDD_v3.3.md`):**
  - **§14 Trazabilidad:** `RegistroDecision` (`<<Registro inmutable>>`, **no** raíz de agregado), `SujetoRef`, `Actor`, `TipoSujeto` (5 valores), `TipoDecision` (38 valores) y `Justificacion`.
  - **§18 Persistencia y CQRS:** sin repositorio de agregado; se escribe en PostgreSQL junto al agregado que registró el evento y se proyecta a la base de lectura (`registro_decisiones`, `IConsultaRegistroDecisiones`; también reutilizado por `linea_tiempo_beca` en HU-28.1 y futura exportación interna nominada de HU-36.1).
  - **Decisiones DDD:** **D-6** (atomicidad `Agregado + RegistroDecision + Outbox`), **DDD-07 / ADR-25** (`Version` explícita en `RaizDeAgregado<TId>`), **DDD-14** (Trazabilidad no intermedia eventos entre servicios).
- **ADRs aplicados:**
  - **ADR-05:** aislamiento estricto de schemas en PostgreSQL (`s1_identidad`..`s6_comunicaciones`); cada servicio escribe su propia tabla `registro_decisiones` en su schema local y nunca lee el schema de otro.
  - **ADR-12:** auditoría `RegistroDecision` y Outbox en commit local (insert-only, correlación y prueba de rollback atómico).
  - **ADR-13:** mensajería con Outbox en el productor e Inbox `(ConsumerName, EventId)` en el consumidor.
  - **ADR-16 / ADR-25:** control de concurrencia optimista con `Version` explícita en la raíz de agregado.
  - **ADR-17 / R20:** instantes persistidos en UTC (`occurredAtUtc`) obtenidos de `IRelojNegocio` (reloj del servidor inyectado, zona de negocio UTC-4).
  - **ADR-24:** versiones centralizadas en `Directory.Packages.props`.

## 2. Decisiones cerradas del equipo (25-nov-2026)

Se cierran las preguntas operativas que condicionaban el modelo de datos de `RF-35` y se registran también en `docs/arquitectura/pendientes-v1.3.md`:

1. **Entrada y salida de *Inactiva* (cierra Anexo B · HU-35.1 #4):**
   - **Entrada automática (`INACTIVACION`):** firmada por `ActorDecision.Sistema("SISTEMA")` (`UsuarioId = null`, `EsSistema = true`) con justificación automática no vacía generada por el proceso programado (cierre de ventana de renovación o vencimiento de pausa).
   - **Salida (`SALIDA_DE_INACTIVA`):** firmada por el coordinador autenticado (`ActorDecision.Usuario(...)`) con justificación obligatoria escrita por él.
2. **Alcance de consulta de `HU-35.2` (cierra Anexo B · HU-35.2 #1 y #2):**
   - **Dirección:** consulta global sobre todos los programas y tipos de decisión.
   - **Coordinador de programa:** consulta restringida estrictamente a los `ProgramaId` asignados al coordinador.
   - **Administrador financiero:** consulta restringida estrictamente a decisiones de **ámbito financiero** (`PAGO_REGISTRADO`, `PAGO_CONFIRMADO`, `PAGO_FALLIDO`, `PAGO_DEVUELTO`, `DATOS_PAGO_MODIFICADOS`, `APORTE_REGISTRADO`), protegiendo la confidencialidad de justificaciones socioeconómicas o deliberaciones de comité.
   - **Postulante / Becario:** consulta completa de **su propio caso** filtrada en la capa de datos por `PropietarioUsuarioId == Solicitante.UsuarioId`, denegando siempre casos ajenos (`HU-35.2 E4`).
   - **Otros roles (Evaluador, Enlace universitario, etc.):** acceso denegado a la consulta general de decisiones.
3. **Auditorías futuras `RF-36 / HU-36.1` (cierra Anexo B · HU-36.1 #5):**
   - No habrá auditorías externas anónimas de gran escala; las auditorías serán **internas y nominadas**. La implementación de `HU-36.1` permanece fuera de este PR (depende de S2/S4/S6), pero el modelo de `RegistroDecision` y `DecisionRegistradaV1` incorpora desde hoy `ProgramaId` y `PropietarioUsuarioId` (UUID opacos, sin PII bancaria ni socioeconómica cruda en RabbitMQ) para soportar la exportación interna nominada sin remodelar.

## 3. Arquitectura y distribución entre proyectos

Siguiendo `ARQUITECTURA_V1.0.md` §12 (*evitar crear 24 proyectos vacíos; construir los bloques transversales y luego verticales completos por servicio*), este incremento se implementa en los `building-blocks/` ya creados en el Cambio 1 sin acoplar el dominio a EF Core ni a infraestructura:

```text
Ascenso.SharedKernel (dominio puro, cero EF Core / ASP.NET / MassTransit / MongoDB)
└── Trazabilidad/
    ├── RegistroDecision.cs              # Registro inmutable (NO RaizDeAgregado)
    ├── SujetoRef.cs                     # VO (TipoSujeto, Id)
    ├── ActorDecision.cs                 # VO (UsuarioId?, Rol, EsSistema) — glosario DDD: Actor
    ├── TipoSujeto.cs                    # Enum exacto DDD §14 (5 valores)
    ├── TipoDecision.cs                  # Enum exacto DDD §14 (38 valores) + clasificación financiera
    ├── ExcepcionInmutabilidadRegistro.cs# ExcepcionDeDominio ("El registro de decisiones no se puede modificar ni eliminar.")
    ├── PoliticaAutorizacionDecision.cs  # Reglas puras: registro (HU-35.1 E7) y consulta por alcance (HU-35.2)
    └── ConsultaRegistroDecisiones.cs    # IConsultaRegistroDecisiones (solo lectura), filtros y DTO proyectado

Ascenso.Contracts (cero dependencias de SharedKernel ni infraestructura)
└── Eventos/
    └── EventosTrazabilidadV1.cs         # DecisionRegistradaV1 (versión 1, sin PII prohibida)

Ascenso.Messaging.Outbox (referenciado solo por *.Infrastructure, nunca por *.Domain)
├── OutboxModelBuilderExtensions.cs      # + ConfigurarRegistroDecisiones() (tabla registro_decisiones + índices)
├── AppendOnlyRegistroDecisionInterceptor.cs # SaveChangesInterceptor: bloquea Modified y Deleted
├── UnidadAtomicaDecisiones.cs           # Ejecuta transacción única: Agregado (Version++) + RegistroDecision + OutboxMessage
├── RegistroDecisionSqlAppendOnly.cs     # Plantilla SQL PostgreSQL (REVOKE UPDATE/DELETE + trigger BEFORE UPDATE OR DELETE)
└── ConsultorRegistroDecisionesEfCore.cs # Implementación AsNoTracking() de IConsultaRegistroDecisiones (solo lectura)
```

### 3.1 Por qué `RegistroDecision` vive en `Ascenso.SharedKernel` y no como un microservicio aparte
1. **Regla D-6 del DDD v3.3 y ADR-12:** `RegistroDecision` NO es un agregado independiente; debe confirmarse en la **misma transacción local** que el agregado (`Beca`, `Postulacion`, `PresupuestoPrograma`, etc.) en cada microservicio (`S1`–`S6`).
2. **Regla DDD-14:** Trazabilidad no es un servicio intermediario. Cada servicio escribe localmente `Agregado + RegistroDecision + OutboxMessage` y publica eventos hacia RabbitMQ para que `S6` proyecte la vista global de solo lectura `registro_decisiones`.
3. **Pureza de Clean Architecture:** el modelo de dominio `RegistroDecision`, sus VOs (`SujetoRef`, `ActorDecision`, `Justificacion`), sus enums (`TipoSujeto`, `TipoDecision`) y el puerto de lectura `IConsultaRegistroDecisiones` quedan en `Ascenso.SharedKernel` (sin EF Core); todo mapeo relacional, interceptor y transacción queda en `Ascenso.Messaging.Outbox` (consumido únicamente por `*.Infrastructure`).

## 4. Modelo de datos y preparación para exportación (`HU-36.1`)

| Campo | Tipo | Obligatorio | Propósito |
|---|---|---|---|
| `Id` | `Guid` | Sí | Identificador único del registro (`RegistroDecisionId`) |
| `Tipo` | `TipoDecision` | Sí | Uno de los 38 tipos cerrados del DDD v3.3 §14 |
| `Sujeto` | `SujetoRef` (`TipoSujeto`, `Id`) | Sí | Caso afectado (`POSTULACION`, `BECA`, `POSTULANTE`, `PROGRAMA`, `DATOS_PAGO`) |
| `ProgramaId` | `Guid?` | Opcional | Clave de ámbito para filtrar por programa (Coordinador `HU-35.2` y exportación `HU-36.1`) sin cruzar schemas |
| `PropietarioUsuarioId` | `Guid?` | Opcional | `UsuarioId` opaco del postulante/becario titular del caso; permite autorización del caso propio en `HU-35.2` sin cruzar schemas PostgreSQL (`ADR-05`) |
| `Actor` | `ActorDecision` (`UsuarioId?`, `Rol`, `EsSistema`) | Sí | Quién tomó la decisión (usuario autenticado con su rol o proceso automático del sistema) |
| `OcurridoEnUtc` | `DateTimeOffset` | Sí | Instante exacto suministrado por `IRelojNegocio.AhoraUtc` (R20) |
| `Justificacion` | `Justificacion` | Sí | Motivo obligatorio. Si falta o está vacía en decisión humana falla con `"Escribe la justificación de esta decisión."` (HU-35.1 E2). En decisiones automáticas del sistema (`EsSistema = true`) exige una justificación técnica no vacía |
| `EstadoAnterior` | `string?` | Opcional | Estado previo a la transición (ej. `Activa`) |
| `EstadoNuevo` | `string?` | Opcional | Estado posterior a la transición (ej. `Suspendida`) |
| `DatosJson` | `string?` | Opcional | Metadatos estructurados no sensibles (ej. fecha de revisión de suspensión, método de pago sin número de cuenta) |
| `ServicioOrigen` | `string` | Sí | Servicio propietario que confirmó la transacción (`S1`..`S6`) |
| `CorrelationId` | `Guid` | Sí | Trazabilidad distribuida (ADR-12) |

### Catálogo exacto de `TipoDecision` (DDD v3.3 §14, 38 valores)
`POSTULACION_RECIBIDA`, `NO_ELEGIBLE`, `ANULACION_NO_ELEGIBLE`, `RETIRO`, `ASIGNACION_EVALUADOR`, `EXCEPCION_UN_EVALUADOR`, `CONFLICTO_DECLARADO`, `CONSOLIDACION`, `CASO_LIMITE_RESUELTO`, `LISTA_ESPERA`, `OFERTA_EMITIDA`, `OFERTA_ACEPTADA`, `OFERTA_RECHAZADA`, `OFERTA_EXPIRADA`, `COMPROMISO_FIRMADO`, `ACTIVACION`, `SUSPENSION`, `REANUDACION`, `PAUSA`, `REACTIVACION`, `INACTIVACION`, `SALIDA_DE_INACTIVA`, `RENOVACION_SOLICITADA`, `RENOVACION_APROBADA`, `RENOVACION_RECHAZADA`, `CONCLUSION`, `REVOCACION`, `PAGO_REGISTRADO`, `PAGO_CONFIRMADO`, `PAGO_FALLIDO`, `PAGO_DEVUELTO`, `DATOS_PAGO_MODIFICADOS`, `DATOS_PAGO_CONSULTADOS`, `DOCUMENTO_RECHAZADO`, `REPORTE_ACADEMICO`, `CUMPLIMIENTO_EVALUADO`, `CONSECUENCIA_APLICADA`, `APORTE_REGISTRADO`.

- **Subconjunto de ámbito financiero (visible para Administrador Financiero en `HU-35.2`):** `PAGO_REGISTRADO`, `PAGO_CONFIRMADO`, `PAGO_FALLIDO`, `PAGO_DEVUELTO`, `DATOS_PAGO_MODIFICADOS`, `APORTE_REGISTRADO`.
- **Nota sobre `DATOS_PAGO_CONSULTADOS` (DDD-12):** el valor permanece en el catálogo oficial del DDD v3.3 §14, mientras la bitácora de lectura de datos bancarios vive separada en `DatosPago.RegistrarConsulta(...)` dentro de S4.

## 5. Garantías técnicas exigidas por `HU-35.1` y `HU-35.2`

1. **Atomicidad real (`D-6`, `ADR-12`, `HU-35.1 E3`):**
   - `UnidadAtomicaDecisiones` ejecuta dentro de una transacción explícita de `DbContext`:
     1. Guarda el agregado modificado (con `Version` incrementada explícitamente, ADR-25).
     2. Inserta el/los `RegistroDecision` en la tabla `registro_decisiones` del schema del servicio.
     3. Inserta el `OutboxMessage` correspondiente a `DecisionRegistradaV1` (y demás eventos del caso de uso).
     4. Ejecuta `SaveChangesAsync` + `CommitAsync`.
   - Ante cualquier error al insertar el registro de decisión, la transacción hace `RollbackAsync` completo y traduce el fallo de persistencia al mensaje exacto `"No se pudo registrar la decisión. No se aplicó ningún cambio."`.
2. **Inmutabilidad real en tres capas (`HU-35.1 E4 y E5`):**
   - **Capa 1 — Dominio:** `RegistroDecision` solo expone propiedades de lectura (`private set`), constructor privado y fábricas estáticas (`Crear`, `CrearAutomatica`); sin métodos de edición ni borrado.
   - **Capa 2 — EF Core (`AppendOnlyRegistroDecisionInterceptor`):** intercepta `SavingChanges` / `SavingChangesAsync`; cualquier entrada de `RegistroDecision` en `Modified` o `Deleted` lanza `ExcepcionInmutabilidadRegistro("El registro de decisiones no se puede modificar ni eliminar.")`.
   - **Capa 3 — Base de datos (PostgreSQL y SQLite en pruebas):**
     - `deploy/postgres/init-schemas.sql` incluye la función compartida `public.impedir_modificacion_registro_decisiones()`.
     - `RegistroDecisionSqlAppendOnly` provee el SQL reutilizable para que las migraciones de S1–S6 revoquen `UPDATE, DELETE` al rol `*_app` y creen el trigger `BEFORE UPDATE OR DELETE`.
     - Las pruebas con SQLite `:memory:` instalan triggers reales `BEFORE UPDATE` / `BEFORE DELETE` y verifican el bloqueo incluso saltándose EF Core mediante SQL directo.
3. **Concurrencia optimista (`ADR-25`, `HU-35.1 E8`):**
   - Dos operaciones simultáneas sobre la misma raíz de agregado cargan la misma `Version`. La primera confirma (`Version = n + 1`, su `RegistroDecision` y su `OutboxMessage`). La segunda dispara `DbUpdateConcurrencyException`, revierte completamente y no deja cambio sin registro ni registro huérfano.
4. **Autorización en la capa de datos (`RF-37`, `HU-35.1 E7`, `HU-35.2`):**
   - `PoliticaAutorizacionDecision` valida:
     - **Registro:** exige poder de decisión sobre el caso y rechaza roles sin poder de decisión (`ENLACE_UNIVERSIDAD`, `POSTULANTE`, `BECARIO`).
     - **Consulta:** evalúa `ContextoSolicitanteDecision` (usuario, rol/permisos, `ProgramasAsignados`):
       - `DIRECCION`: acceso global.
       - `COORDINADOR`: requiere que la consulta quede acotada a `ProgramasAsignados`; si solicita un sujeto o programa fuera de sus programas, acceso denegado.
       - `FINANZAS`: acceso acotado exclusivamente al subconjunto de tipos de decisión financieros.
       - `POSTULANTE` / `BECARIO`: acceso acotado exclusivamente a `PropietarioUsuarioId == Solicitante.UsuarioId`; si intenta consultar un sujeto cuyo propietario es otra persona, acceso denegado (`HU-35.2 E4`).
       - Otros roles sin permiso de consulta: acceso denegado.
     - **Mutación/Borrado:** denegada incondicionalmente para todos los roles, incluida `DIRECCION`.
5. **Consulta de solo lectura (`HU-35.2 E1–E4`):**
   - `IConsultaRegistroDecisiones` solo expone operaciones de lectura (`ConsultarPorSujetoAsync`, `ConsultarAsync`).
   - `ConsultorRegistroDecisionesEfCore` usa `AsNoTracking()`, aplica los filtros de alcance directamente en la consulta SQL/LINQ, ordena por `OcurridoEnUtc ASC, Id ASC` y devuelve `"Este caso aún no tiene decisiones registradas."` cuando el sujeto autorizado aún no tiene registros.

## 6. Contrato de integración nuevo y actualización de contratos

### Evento de integración nuevo: `DecisionRegistrada.v1`
- **Tipo C#:** `Ascenso.Contracts.Eventos.DecisionRegistradaV1` (viaja dentro de `IntegrationEventEnvelope<DecisionRegistradaV1>`, `SchemaVersion = 1`).
- **Productores:** `S1`, `S2`, `S3`, `S4`, `S5` (y `S6` si registra decisiones propias).
- **Consumidor:** `S6` (proyectores idempotentes vía `InboxMessage` para actualizar `registro_decisiones` y `linea_tiempo_beca` en MongoDB).
- **Regla anti-PII:** usa UUIDs opacos (`PropietarioUsuarioId`, `ActorUsuarioId`, `ProgramaId`); no serializa cédulas, contraseñas, números de cuenta bancaria ni bloques socioeconómicos crudos.
- **Contratos actualizados en este PR:**
  - `contracts/EVENTOS_E_INTERFACES.md`
  - `docs/arquitectura/v1.3/04_MATRICES/CATALOGO_EVENTOS_INTEGRACION.csv`
  - `contracts/openapi.yaml` (query params en `GET /api/v1/registro-decisiones` manteniendo `ConsultaResponse` y errores RFC 9457)
  - `docs/arquitectura/pendientes-v1.3.md` (registro del cierre de las 3 decisiones)

## 7. Alineación con DDD v3.3 y ADRs

Este diseño **no contradice** ningún ADR ni regla del `DDD_v3.3.md`. Explicita el evento `DecisionRegistrada.v1` (derivado directo de `D-6` y `DDD-14` para proyectar `registro_decisiones` en S6 sin violar `ADR-05`) y agrega `PropietarioUsuarioId` para cumplir el alcance de consulta propia del becario/postulante en `HU-35.2` sin acoplar schemas.

## 8. Preguntas abiertas residuales (Anexo B)

Tras las decisiones cerradas del 25-nov-2026 (`HU-35.1 #4`, `HU-35.2 #1 y #2`, `HU-36.1 #5`), solo permanecen como preguntas abiertas no bloqueantes del Anexo B:
- **Anexo B · HU-35.1 #1, #2, #3:** encuadre editorial de la narrativa y sustitución cuando quien declara conflicto de interés (`HU-39.1`) es el coordinador o el administrador financiero (pertenece a `HU-39.1`, no a `RF-35`).
- **Anexo B · HU-36.1 #1–#4:** formato concreto de archivo (CSV/JSON/PDF), si Finanzas también exporta uso de fondos, división en entregables y retención de archivo en Firebase (se resolverán al planificar `HU-36.1`).
