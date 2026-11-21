# Pendientes después de la arquitectura v1.3

Lo que la v1.3 **no resuelve**. Al responder cada punto, anotar la respuesta (con fecha y quién respondió) y llevarla a su documento: respuestas del cliente como **Ronda 5** en [`docs/requisitos/respuestas_cliente.md`](../requisitos/respuestas_cliente.md); decisiones técnicas como ADR correctivo en la arquitectura; reglas de dominio en el cambio [`corregir-ddd-v3-3`](../../openspec/changes/corregir-ddd-v3-3/proposal.md).

## A y B. Preguntas al cliente — respondidas en la Ronda 5

Respuestas completas en la [Ronda 5 de `respuestas_cliente.md`](../requisitos/respuestas_cliente.md#ronda-5--preguntas-posteriores-a-la-arquitectura-v13) (R31–R44). Lo que cada respuesta obliga a cambiar. En B1, B2 y B6 el equipo decidió mantener la v1.3 (21/11/2026), así que no hay ADR correctivo:

| # | Respuesta | Qué cambia | Dónde |
|---|---|---|---|
| A1 | El monto de la renovación se fija al enviar la solicitud | Reserva de renovación S4 → S2 con el monto de ese momento | DDD (`corregir-ddd-v3-3`, 2.9) |
| A2 | La reserva cubre toda la duración de la beca | `PlanReserva` de S2 por la duración completa | DDD 2.9; contrato interno S4 → S2 |
| A3 | No se revoca desde *Otorgada* ni *Renovada* | Revocar solo desde *Activa*, *Suspendida* o *Inactiva* | DDD 2.9 (máquina de estados de Beca) |
| A4 | El rechazo lo decide siempre el encargado | Sin transición automática a *Rechazada* | DDD 2.9 (máquina de estados de Postulación) |
| A5 | Prevalece la API; no todas las universidades la tendrán | Registro manual como canal normal, no como excepción | S5 (capa anticorrupción) |
| A6 | Consentimiento por donante e irrevocable | Un consentimiento por becario y donante, sin operación de retiro | DDD 2.9; proyecciones de S6 |
| A7 | Estado de matrícula en texto libre | S5 guarda el texto original y lo clasifica en valores internos; **el equipo define la clasificación** (ver C10) | S5; contrato del lote universitario |
| A8 | Pasa a *Inactiva* en la fecha de cierre | Resuelve C2: el proceso de Hangfire de la v1.3 §9 es el único disparador | DDD 2.4 |
| A9 | Las demás preguntas siguen en el Anexo B de las HU | — | [Anexo B](../requisitos/historias-de-usuario/anexo-b-preguntas-abiertas.md) |
| B1 | Postulación pendiente si no se puede comprobar el doble beneficio | **No adoptada:** el equipo mantiene el «fail closed» de la v1.3 §3.2 | — |
| B2 | Las universidades identifican por cédula | **No adoptada:** se mantiene el código de estudiante traducido por S5 (v1.3) | — |
| B3 | Llamado de atención, suspensión y revocación, en ese orden | Las consecuencias son progresivas | DDD 2.9 |
| B4 | Tras un fallo, el usuario reintenta con un código nuevo | Confirma la v1.3 §8.2 | — |
| B5 | Aprueban los administradores de más alta jerarquía | **Por confirmar:** nombre del rol y fecha de congelamiento de las planillas | Plan de migración (v1.3 §11) |
| B6 | Una consulta siempre es sobre una postulación | **No adoptada:** el contrato de la v1.3 no exige indicarla | — |

### Al profesor (sin respuesta todavía)

| # | Pregunta |
|---|---|
| B7 | La lámina dice «Firebase con PostgreSQL». ¿Acepta MongoDB para la base de lectura (CQRS)? |
| B8 | La lámina pregunta «¿Cómo montar las APIs backend?». ¿Espera algo concreto (controladores, minimal APIs, gateway)? |
| B9 | Para la «posible data warehouse para los reportes», ¿bastan las proyecciones en MongoDB? |
| B10 | SignalR no está en la lámina. ¿Lo acepta para tiempo real? |

## C. Decisiones del equipo

| # | Decisión | Estado en la v1.3 |
|---|---|---|
| C1 | Dueño de RF-18 / HU-18.1: S4 o S6 | S4 (supuesto, §21) |
| C2 | ~~Un solo disparador para Activa → *Inactiva*~~ | **Resuelta por A8:** proceso de Hangfire en la fecha de cierre (v1.3 §9); el DDD retira el disparador de `IniciarNuevoPeriodo()` |
| C3 | Versión de la raíz: debe subir aunque solo cambie una entidad interna (pago, suspensión), o un pago y una revocación simultáneos no chocan | §7.1 dice «versión por raíz», sin esa regla |
| C4 | Eventos `Beca*` hacia S3: `EVENTOS_E_INTERFACES.md` dice que S3 los consume; los catálogos CSV no lo listan | Por alinear |
| C5 | Plazo para conciliar reservas sin vincular | Abierto (ADR-03) |
| C6 | MediatR: desde la versión 13 tiene doble licencia. ¿Despachador propio o MediatR 12.5 fijo? | No mencionado |
| C7 | Frontend: renderizado en servidor o SPA, dónde vive el token de Keycloak, ¿Next.js como BFF? Falta un ADR | No mencionado |
| C8 | Proveedores de correo y SMS (prueba y demostración) | Abierto (ADR-20) |
| C9 | Repo público: decidir si pasa a privado | — |
| C10 | Clasificación del estado de matrícula (A7): valores internos (por ejemplo inscrito, retirado, congelado, egresado, no reconocido), reglas para reconocer el texto de cada universidad y qué pasa con un texto no reconocido | Nueva, por la Ronda 5 |

## D. Huecos de diseño sin documento

| # | Hueco | Dónde se resuelve |
|---|---|---|
| D1 | Desembolso con fecha dentro de una suspensión: se paga al reanudar (R12), pero no está modelado | `corregir-ddd-v3-3` |
| D2 | Registro de cuenta entre S1 (Keycloak) y S3 (Postulante): cédula única, fuente de verdad, qué pasa si falla el segundo paso | `corregir-ddd-v3-3` y ADR correctivo |
| D3 | Firebase Storage desde .NET: el SDK de administración no trae Storage; falta comprobar enlaces firmados con el emulador | Prueba técnica (ADR-06) |
| D4 | Segundo factor por correo/SMS en Keycloak | Prueba técnica (ADR-09) |
| D5 | Gestión de las claves de cifrado de los datos de pago | ADR-18 |
