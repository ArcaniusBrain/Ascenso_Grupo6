# Contratos de integración — ASCENSO v1.3

**Propósito:** formalizar el mínimo que permite programar S1–S6 sin modificar las fronteras del DDD v3.2.

## Reglas de identidad de mensajes

Todos los eventos de integración viajan en `Ascenso.Contracts` con un sobre común:

```json
{"eventId":"uuid", "eventType":"OfertaEmitida", "schemaVersion":1,
 "aggregateId":"uuid", "occurredAtUtc":"2026-11-15T00:00:00Z",
 "producer":"S4", "correlationId":"uuid", "causationId":"uuid",
 "payload":{"ofertaId":"uuid","reservaId":"uuid","programaId":"uuid","codigoPeriodo":"2026-2"}}
```

Una nueva propiedad opcional mantiene compatibilidad; cambiar el significado o eliminar una propiedad requiere nueva versión del esquema y estrategia de consumidores. No serializar entidades, `BeneficiosPersona`, credenciales, cuenta bancaria ni datos socioeconómicos. El productor publica **después** del commit mediante Outbox.

## Publicaciones prioritarias

| Evento de integración | S origen | S que consumen | Resultado local |
|---|---|---|---|
| `CicloPublicado.v1` | S3 | S2, S6 | Crear/actualizar cupo de ciclo, indicadores |
| `PeriodoEvaluacionAbierto.v1` | S3 | S2, S6 | Bloqueo de rúbrica según DDD D-4, aviso |
| `OfertaEmitida.v1` | S4 | S2, S3, S6 | Vincular reserva, marcar oferta, aviso |
| `OfertaAceptada.v1` | S4 | S2, S3, S6 | Confirmar asignación, registrar otorgamiento, aviso |
| `OfertaExpirada.v1` | S4 | S2, S3, S6 | Liberar fondos/cupo, retiro por expiración, sugerir espera |
| `BecaRevocada.v1` | S4 | S2, S6 | Liberación eventual segura, informes |
| `BecaInactivada.v1` | S4 | S6 | Actualizar panel; **no liberar cupo/presupuesto** |
| `PagoConfirmado.v1` | S4 | S2, S6 | Registro/indicadores financieros idempotentes |
| `ReporteAcademicoRegistrado.v1` | S5 | S6 | Proyección de seguimiento |
| `IncumplimientoDetectado.v1` | S5 | S4, S6 | Crear alerta, **no revocar automáticamente** |
| `PeriodoMarcadoActual.v1` | S2 | S4, S6 | Iniciar nuevo periodo por lotes |
| `PostulacionSeleccionada.v1` | S3 | S4, S6 | Habilitar la emisión de la oferta con clave `(postulacionId, cicloId)` |
| `CostoMatriculaVigente.v1` | S2 | S4 | Proyectar el costo vigente por universidad y periodo (C2) |
| `SugerenciaListaEspera.v1` | S3 | S6 | Notificar al coordinador el siguiente elegible sugerido; S6 no decide |

Consulte `04_MATRICES/MAPEO_34_EVENTOS_DOMINIO.csv` para los 34 eventos del DDD. **No todos se exponen en RabbitMQ**; el dominio registra y el módulo decide si existe un hecho externo relevante.

## Garantías por evento

1. `eventId` global único e inmutable; registro de consumo `(consumer, eventId)` atómico con el efecto.
2. Retries con backoff y cola de errores; no ack antes de guardar proyección, efecto o Inbox.
3. `occurredAtUtc` identifica instante; el cómputo de vencimientos aplica la zona de negocio UTC-4 definida por R20.
4. No usar orden global del broker como supuesto. Proyectores manejan reordenamiento por clave/version y reejecución.
5. El retraso de proyección no bloquea comandos; toda decisión financiera vuelve al dueño S2/S4.

## Reserva de presupuesto

- Solicitar `POST /internal/v1/fondos/reservas` desde S4 (no público); solo identificadores de programa, postulación, ciclo y operación. S2 calcula el importe/periodos desde `Programa.MontoPorPeriodo` y el calendario vigente.
- El S2 calcula y confirma de forma autoritativa `PresupuestoPrograma.ReservarOferta()`; igualdad del importe con el disponible sí es válida.
- El S4 no confirma oferta si no tiene `reservaId` persistido.
- En error, compensar sin duplicados. El recolector verifica si la oferta existe en S4 y no borra reservas solamente por timeout.
- El dominio de S2 protege dinero y ambos cupos en una única actualización versionada.

## Contrato HTTP

`openapi.yaml` contiene 29 operaciones principales versionadas, con DTO de comando y errores HTTP. Es **núcleo de arquitectura**, no reemplaza la lista completa de operaciones de las 73 HU ni implica endpoints ya desplegados. Si una HU requiere otra operación, añadir endpoint en su S sin modificar el límite arquitectónico. Los detalles de actor y estado se validan en el servidor.

## Correcciones contractuales v1.1

- `PostulacionSeleccionada.v1`: S3 publica para S4/S6, con postulación y ciclo; S4 crea una sola oferta pendiente por `(postulacionId, cicloId)`. S2 reserva con clave idéntica; `ofertaId` por sí solo no es clave de unicidad.
- `CostoMatriculaVigente.v1`: S2 publica universidad, `CodigoPeriodo`, monto y vigencia; S4 proyecta versión local e invalida su uso si falta dato vigente (C2).
- `BecaActivada.v1`, `BecaSuspendida.v1`, `BecaRevocada.v1`, `BecaInactivada.v1`: S3 consume para proyección de consulta, pero HU-01.2 **siempre confirma con consulta interna S3→S4** antes de aceptar una postulación. Una réplica retrasada nunca habilita la postulación.
- `OfertaExpirada.v1`: S3 calcula sugerencia de lista de espera con datos transaccionales; S6 únicamente notifica.
- Las 25 raíces siguen gobernadas por su propietario S1–S6, no por S6/las proyecciones MongoDB.


## Coherencia de contratos v1.2 (HU-20.2, HU-25.2, HU-38.2)

- API pública: `MarcarPeriodoCommand` y `AceptarOfertaCommand` distintos.
- HU-20.2: marcar incidencia (`FALLIDO|DEVUELTO`) y resolver mediante nuevo pago con descripción, fecha, método, destinatario y OTP `RESOLVER_PAGO`. No se modelan otras formas de resolución porque HU Plan B las deja abiertas.
- Datos bancarios: la solicitud incluye `numeroCuenta` exclusivamente por TLS; el cifrado ocurre en el adaptador de persistencia de S4. Nunca registrar la cuenta sin máscara.
- Interservicios: S3 consulta S4 mediante `personaId` interno opaco y permisos `beneficios.consultar`; no se transmite cédula en path.
- Universidad: S5 guarda recepción idempotente y Outbox antes de responder 202; luego un worker procesa el lote y su ACL es dueña de la tabla de correspondencia `universidadId+codigoEstudiante → BecaId`.


## Coherencia de contratos v1.3 (HU-24.3, HU-25.2, HU-32.1 y OTP)

- OTP: `POST /api/v1/otp/challenges` responde con `ChallengeResponse` (`challengeId` y `expiraEnUtc`), que es lo que después exige `OtpDeOperacion`.
- HU-25.2: `AplicarConsecuenciaCommand` lleva la consecuencia elegida (`LLAMADO_ATENCION`, `SUSPENSION` o `REVOCACION`, enumeración `Consecuencia` del DDD), el siguiente paso y, para la suspensión, la fecha de revisión (nula significa indefinida, HU-21.1). El OTP solo es obligatorio al revocar (HU-22.1). El servidor rechaza una consecuencia que la política vigente del programa no contempla.
- HU-24.3: cada reporte del lote lleva promedio, materias aprobadas, materias reprobadas y estado de matrícula. El resultado por fila (registrado o rechazado, con su motivo) se consulta en `GET /api/v1/universidades/{universidadId}/reportes-academicos/lotes/{loteId}`. El DDD v3.2 solo define `INSCRITO` en `EstadoMatricula`; el catálogo completo queda por cerrar con el equipo.
- HU-32.1: `POST /api/v1/consultas` usa `RegistrarConsultaCommand`, que antes no tenía ruta.
- Se retira `ReservaResponse` del contrato público porque pertenece al contrato interno.
- Eventos catalogados en `04_MATRICES/CATALOGO_EVENTOS_INTEGRACION.csv`: `CostoMatriculaVigente`, `SugerenciaListaEspera` y `LoteAcademicoRecibido`. Este último es interno de S5 y no sale por RabbitMQ hacia otros servicios.
