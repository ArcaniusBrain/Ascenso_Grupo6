# Pendientes después de la arquitectura v1.3

Lo que la v1.3 **no resuelve**. Al responder cada punto, anotar la respuesta (con fecha y quién respondió) y llevarla a su documento: respuestas del cliente como **Ronda 5** en [`docs/requisitos/respuestas_cliente.md`](../requisitos/respuestas_cliente.md); decisiones técnicas como ADR correctivo en la arquitectura; reglas de dominio en el cambio [`corregir-ddd-v3-3`](../../openspec/changes/corregir-ddd-v3-3/proposal.md).

## A y B. Preguntas al cliente — respondidas en la Ronda 5

Respuestas completas en la [Ronda 5 de `respuestas_cliente.md`](../requisitos/respuestas_cliente.md#ronda-5--preguntas-posteriores-a-la-arquitectura-v13) (R31–R44). Lo que cada respuesta obliga a cambiar. En B1, B2 y B6 el equipo decidió mantener la v1.3 (21/11/2026), así que no hay ADR correctivo:

| # | Respuesta | Qué cambia | Dónde | Estado |
|---|---|---|---|---|
| A1 | El monto de la renovación se fija al enviar la solicitud | Reserva de renovación S4 → S2 con el monto de ese momento | DDD v3.3; `openapi-interno.yaml` v1.3 | **Cerrado** |
| A2 | La reserva cubre toda la duración de la beca | Reserva de oferta S2 por la duración completa | DDD v3.3; `openapi-interno.yaml` v1.3 | **Cerrado** |
| A3 | No se revoca desde *Otorgada* ni *Renovada* | Revocar solo desde *Activa*, *Suspendida* o *Inactiva* | DDD v3.3 §15.2 | **Cerrado** |
| A4 | El rechazo lo decide siempre el encargado | Sin transición automática a *Rechazada* | DDD v3.3 §9b, §15.1 | **Cerrado** |
| A5 | Prevalece la API; no todas las universidades la tendrán | Registro manual como canal normal, no como excepción | DDD v3.3 §12; S5 | **Cerrado** |
| A6 | Consentimiento por donante e irrevocable | Un consentimiento por becario y donante, sin operación de retiro | DDD v3.3 §14; S6 | **Cerrado** |
| A7 | Estado de matrícula en texto libre | S5 guarda texto original y clasifica en valores internos (ver C10 / ADR-29) | DDD v3.3 §12; `openapi.yaml`; ADR-29 | **Cerrado** |
| A8 | Pasa a *Inactiva* en la fecha de cierre | Resuelve C2: Hangfire en fecha de cierre es único disparador (`InactivarPorNoRenovacion`) | DDD v3.3 §11b, §15.2, §16.2 | **Cerrado** |
| A9 | Las demás preguntas siguen en el Anexo B de las HU | — | [Anexo B](../requisitos/historias-de-usuario/anexo-b-preguntas-abiertas.md) | Abierto (detalles menores) |
| B1 | Postulación pendiente si no se puede comprobar el doble beneficio | **No adoptada:** el equipo mantiene el «fail closed» de la v1.3 §3.2 | Verificado en DDD v3.3 §16.1 | **Cerrado** |
| B2 | Las universidades identifican por cédula | **No adoptada:** se mantiene el código de estudiante traducido por S5 (v1.3) | Verificado en DDD v3.3 §12 | **Cerrado** |
| B3 | Llamado de atención, suspensión y revocación, en ese orden | Las consecuencias son progresivas | DDD v3.3 §8, §12, §16.1 | **Cerrado** |
| B4 | Tras un fallo, el usuario reintenta con un código nuevo | Confirma la v1.3 §8.2 | DDD v3.3 §13, §16.4d | **Cerrado** |
| B5 | Aprueban los administradores de más alta jerarquía | **Por confirmar:** nombre del rol y fecha de congelamiento de las planillas | Plan de migración (v1.3 §11) | **Pendiente cliente** |
| B6 | Una consulta siempre es sobre una postulación | **No adoptada:** el contrato de la v1.3 no exige indicarla | Verificado en DDD v3.3 §14 (`0..1`) | **Cerrado** |

### Al profesor (sin respuesta todavía)

| # | Pregunta |
|---|---|
| B7 | La lámina dice «Firebase con PostgreSQL». ¿Acepta MongoDB para la base de lectura (CQRS)? |
| B8 | La lámina pregunta «¿Cómo montar las APIs backend?». ¿Espera algo concreto (controladores, minimal APIs, gateway)? |
| B9 | Para la «posible data warehouse para los reportes», ¿bastan las proyecciones en MongoDB? |
| B10 | SignalR no está en la lámina. ¿Lo acepta para tiempo real? |

## C. Decisiones del equipo

| # | Decisión | Estado actual (24-nov-2026) |
|---|---|---|
| C1 | Dueño de RF-18 / HU-18.1: S4 o S6 | S4 (adoptado en v1.3 §21; S6 proyecta lectura) |
| C2 | ~~Un solo disparador para Activa → *Inactiva*~~ | **Cerrada (A8 / DDD v3.3):** proceso de Hangfire en la fecha de cierre (`InactivarPorNoRenovacion`); retirado de `IniciarNuevoPeriodo()` |
| C3 | ~~Versión de la raíz ante cambios en entidades internas~~ | **Cerrada (DDD-07 / ADR-25):** `Version: int` explícita en `RaizDeAgregado`, incrementada también al cambiar hijos |
| C4 | ~~Eventos `Beca*` hacia S3 entre `EVENTOS_E_INTERFACES.md` y CSV~~ | **Cerrada:** alineados en `CATALOGO_EVENTOS_INTEGRACION.csv` y `EVENTOS_E_INTERFACES.md` (más `UsuarioRegistrado`) |
| C5 | ~~Plazo para conciliar reservas sin vincular~~ | **Cerrada (ADR-26):** 30 min de gracia + consulta obligatoria `S2 → S4` (`GET /internal/v1/reservas/{reservaId}/vinculo`, falla cerrada) |
| C6 | ~~MediatR (licencia v13+) vs despachador propio~~ | **Cerrada (ADR-27):** `MediatR 12.5.0` (Apache 2.0) fijado en `Directory.Packages.props` + `MediatR.Contracts 2.0.1` + check en CI contra `>= 13.0.0` |
| C7 | ~~Arquitectura Frontend Next.js y sesión Keycloak~~ | **Cerrada (ADR-28):** Next.js App Router como BFF ligero con cookies `HttpOnly/Secure/SameSite=Lax`, nunca `localStorage` |
| C8 | Proveedores de correo y SMS (prueba y demostración) | **Abierto** (ADR-20: por elegir en sprint de infraestructura; MailHog/SMTP local para dev) |
| C9 | Repo público: decidir si pasa a privado | **Abierto** (decisión administrativa del equipo) |
| C10 | ~~Clasificación del estado de matrícula (A7)~~ | **Cerrada (ADR-29 / DDD v3.3):** `EstadoMatriculaClasificado` conserva texto original + enum `INSCRITO`, `RETIRADO`, `CONGELADO`, `EGRESADO`, `NO_RECONOCIDO` |
| C11 | ~~Autoría y justificación de entrada/salida de *Inactiva* (Anexo B HU-35.1 #4)~~ | **Cerrada (25-nov-2026):** entrada automática firmada por `SISTEMA` con justificación automática no vacía (cierre de ventana o pausa vencida); salida decidida por coordinador con justificación obligatoria |
| C12 | ~~Alcance de consulta del registro de decisiones (Anexo B HU-35.2 #1 y #2)~~ | **Cerrada (25-nov-2026):** Dirección (global), Coordinador (solo sus programas), Finanzas (solo decisiones financieras) y Becario/Postulante (solo su propio caso vía `PropietarioUsuarioId`) |
| C13 | ~~Naturaleza de auditorías RF-36 (Anexo B HU-36.1 #5)~~ | **Cerrada (25-nov-2026):** auditorías internas y nominadas (no habrá auditorías externas anónimas de gran escala); modelo `RegistroDecision` preparado con `ProgramaId` y `PropietarioUsuarioId` |
| C14 | ~~Reglas operativas 2FA y tipos de usuario RF-37 (Anexo B HU-37.1 #3/#4/#5/#7, HU-37.2 #1/#2)~~ | **Cerrada (25-nov-2026):** código 2FA de un solo uso y reenvío invalida el anterior (vigencia 15 min por R4b, configurable); Dirección crea enlace universitario; tipo sin acceso financiero/decisión exento de 2FA; tipo en uso no se borra; cambio de permisos/rol revoca sesión y exige nuevo login |
| C15 | ~~Reglas operativas del calendario académico RF-38 (Anexo B HU-38.1 #1–#4, HU-38.2 #3/#4/#6)~~ | **Cerrada (25-nov-2026):** secuencia ordenada cronológicamente ascendente (`2025-2` antes que `2026-1`); lectura para todo el personal interno y escritura exclusiva de Dirección; periodos no se editan ni eliminan; inicio de inscripciones interpretado desde `00:00` UTC-4 (`R20`); solo periodos `ABIERTO` pueden marcarse como actuales; marcar periodo actual usa solo 2FA de sesión (sin OTP); carrera concurrente devuelve HTTP `409 Conflict` (`ProblemDetails`) |
| C16 | ~~Reglas operativas de universidades aliadas y convenios RF-14 (Anexo B HU-14.1 #1/#4/#5/#6, HU-14.3 #2/#3/#4)~~ | **Cerrada (25-nov-2026):** Dirección y Coordinador gestionan universidades, convenios, carreras, costos y fechas de inicio desde `HU-14.3` (enlace de universidad siempre bloqueado para editar convenio o contacto; por API universitaria en `S5` solo llegan reportes académicos); nombre único normalizado insensible a mayúsculas/espacios con capacidad de corregir mayúsculas/minúsculas del nombre registrado; una única fecha de inicio por `(universidad, periodo)` coherente con `HU-38.1 E13`; sin fecha cargada las disponibilidades siguen abiertas (`inscripcionesIniciadas = false`); costo de matrícula en moneda ISO-3 con `USD` por defecto |
| C17 | ~~Reglas operativas de programas de becas RF-11 (Anexo B HU-11.1 #1–#5, HU-11.2 #1–#4, HU-01.2 #4)~~ | **Cerrada (25-nov-2026):** crear programa exige al menos un periodo registrado y acepta cualquier periodo `ABIERTO` (rechaza `CERRADO`); nombre único normalizado con corrección permitida de mayúsculas/minúsculas; Dirección (global) y Coordinador (creación y sus programas asignados) gestionan programas; modalidad de pago obligatoria (`MENSUAL`, `SEMESTRAL`, `DIRECTO_UNIVERSIDAD`); duración máxima de pausa `>= 0` (`0` = no permite pausas, negativos rechazados); condiciones de renovación descriptivas junto a campos tipados de permanencia; cambios de restricción de universidad/carrera aplican solo a postulaciones futuras (sin retroactividad); `permiteAcumulacion` incluido en `Programa` (forzado a `false` si `COMPLETO`) |

## D. Huecos de diseño y pruebas técnicas

| # | Hueco | Estado / dónde se resuelve |
|---|---|---|
| D1 | ~~Desembolso con fecha dentro de una suspensión (R12)~~ | **Cerrado** en `DDD_v3.3.md` (estado `Aplazado`, DDD-09) |
| D2 | ~~Registro de cuenta entre S1 (Keycloak) y S3 (Postulante)~~ | **Cerrado** en `DDD_v3.3.md` (flujo `16.4f`, `UsuarioRegistrado`, DDD-11) y **ADR-30** |
| D3 | Firebase Storage desde .NET: el SDK de administración no trae Storage; falta comprobar enlaces firmados con el emulador | **Pendiente:** prueba técnica (1 día, ADR-06) antes de implementar carga de documentos |
| D4 | ~~Segundo factor por correo/SMS en Keycloak~~ | **Cerrado (25-nov-2026, ADR-09):** PoC ejecutado en vivo contra `keycloak:26.0`, `mailpit:v1.21` y `postgres:16-alpine`; script reproducible en [`tools/poc/poc-keycloak-2fa.mjs`](../../tools/poc/poc-keycloak-2fa.mjs) y evidencia documentada en [`evidencias/POC_ADR09_KEYCLOAK_2FA.md`](evidencias/POC_ADR09_KEYCLOAK_2FA.md) |
| D5 | Gestión de las claves de cifrado de los datos de pago | **Pendiente:** definir proveedor/rotación de claves (ADR-18; Data Protection / KMS / env local dev) |
