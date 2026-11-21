# ADR-14 · Tareas programadas: Hangfire

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Propuesta

## Contexto

La lámina propone Hangfire para tareas periódicas. Varias reglas del cliente se disparan por el paso del tiempo, no por una acción de usuario.

## Decisión

- Hangfire con almacenamiento en PostgreSQL, en el schema del servicio que ejecuta cada proceso.
- **Reglas comunes:** todas las programaciones en UTC-4; cada proceso es idempotente (verifica el estado antes de escribir), paginado y reanudable; el proceso no contiene reglas de negocio, solo invoca comandos que cambian los agregados; en RegistroDecision el actor queda como «sistema».

| Proceso | Servicio | Cuándo (UTC-4) | Efecto | Fuente |
|---|---|---|---|---|
| Expirar ofertas vencidas | S4 | Diario, después de las 23:59 | La oferta pasa a Expirada; por evento, S2 libera el presupuesto y la postulación pasa a Retirada. El coordinador ve la sugerencia del siguiente en la lista de espera; la nueva oferta la emite una persona. | RF-15, HU-15.2 |
| Resolver becas no renovadas | S4 | Al cerrar cada ventana de renovación | Activa pasa a Inactiva (no a Concluida). Paginado y reanudable. | C3, R18a, HU-26.2 |
| Vencer pausas | S4 | Diario | Una beca suspendida por pausa vencida pasa a Inactiva. | C3, R18b |
| Aviso de fecha de revisión de una suspensión | S4 (envía S6) | Diario | Avisa al coordinador. | R18c, HU-21.3 |
| Fin de la lista de espera por universidad | S3 | Diario | Termina la marca de espera cuando la universidad inicia sus inscripciones. | R18d, HU-10.2 |
| Recordatorios de plazos | S6 | Diario | Según las anticipaciones configuradas (por ejemplo, 7 y 2 días): documento rechazado, ventana de renovación, reporte académico. | RF-30, HU-30.1 |
| Recordatorio al comité | S6 | Diario | Aviso antes del cierre de la evaluación. | RF-31, HU-31.1 |
| Reporte académico no entregado | S5 | Diario, tras el plazo | Alerta de entrega tardía. | HU-24.1; DDD 16.2 |
| Liberar reservas sin vincular | S2 | Cada pocos minutos | Libera reservas que nunca se vincularon a una oferta. | ADR-03 |

- **No son procesos programados** (se disparan por eventos): el cambio de periodo actual, que recorre las becas (HU-38.2), y la consolidación de puntuaciones al enviarse una evaluación (HU-09.1).
- Para los recordatorios, S6 necesita consultar las fechas límite de otros servicios: se le replican por eventos (Anexo A de HU-30.1, «fecha límite consultable»).

## Corrige el borrador anterior

- La beca no renovada pasaba a Concluida; según C3 pasa a Inactiva.
- La expiración «promovía» al siguiente de la lista de espera; según HU-15.2 el sistema solo lo sugiere.
- Faltaban cinco procesos: pausas, revisión de suspensiones, fin de lista de espera, reporte tardío y reservas sin vincular.

## Fuentes

R18a a R18d; Anexo A de HU-15.2, HU-21.3, HU-26.2, HU-30.1 y HU-38.2; DDD v3.2, sección 16.2.
