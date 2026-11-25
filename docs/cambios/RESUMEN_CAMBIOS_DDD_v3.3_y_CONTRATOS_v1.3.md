# Resumen de Cambios — DDD v3.3, Contratos v1.3 y ADRs Correctivos

**Fecha:** 24 de noviembre de 2026  
**Rama:** `docs/corregir-ddd-v3-3`  
**Cambio OpenSpec:** `corregir-ddd-v3-3` (archivado con `openspec validate --all --strict` en verde)  
**Versión PDF para compartir:** [`RESUMEN_EQUIPO_DDD_v3.3_y_CONTRATOS_v1.3.pdf`](RESUMEN_EQUIPO_DDD_v3.3_y_CONTRATOS_v1.3.pdf)

---

## 1. Objetivo de este cambio

Alinear el modelo de dominio, los contratos entre servicios y las decisiones técnicas pendientes **antes de escribir código** en `.NET 10` y `Next.js`, eliminando contradicciones entre el DDD v3.2, la arquitectura v1.3, el PRD y las respuestas del cliente de la Ronda 5 (`R31–R44`).

> **Importante:** este cambio toca elementos de diseño del **Shared Kernel** (`RaizDeAgregado.Version` e `IServicioOtp`), por lo que requiere revisión de todo el equipo antes de fusionar el Pull Request.

---

## 2. Qué se hizo exactamente

### A. Limpieza previa del repositorio
- Se eliminó la carpeta residual `_v13/` (copia sin seguimiento de 147 archivos que duplicaba `docs/arquitectura/v1.3/`).
- Se conservó intacto [`docs/dominio/DDD_v3.2.md`](../dominio/DDD_v3.2.md) como referencia histórica de los PDF ya entregados.

### B. Modelo de Dominio — Nuevo [`docs/dominio/DDD_v3.3.md`](../dominio/DDD_v3.3.md)
Se completaron las 16/16 tareas de `corregir-ddd-v3-3`:

| ID / Fuente | Cambio aplicado | Impacto en código |
|---|---|---|
| **DDD-07** *(Bloqueante)* | `RaizDeAgregado.Version` explícita (`int`) que se incrementa ante **cualquier** cambio del agregado, incluidas entidades internas (`Pago`, `Suspension`) + invariante transversal en §16.1. | Evita el fallo de PostgreSQL `xmin` en el caso de concurrencia exigido por el PRD: *confirmar un pago mientras se revoca la misma beca*. |
| **Adenda S2/S4** | §3a y §3.1 reflejan la frontera distribuida entre **S2** (`PresupuestoPrograma`) y **S4** (`Beca`, `Oferta`, `BeneficiosPersona`), manteniendo D-3 local a S4. | Elimina la contradicción de v3.2 (que los ubicaba en una misma app) sin alterar agregados ni invariantes. |
| **DDD-14** | En el Context Map, Trazabilidad deja de ser intermediaria hacia Comunicaciones y Reportería; cada contexto escribe su `RegistroDecision` local y publica eventos directo a S6. | Evita construir un intermediario inexistente entre los servicios y S6. |
| **DDD-08** *(R38 / RF-38)* | Único disparador de `Activa → Inactiva`: `Beca.InactivarPorNoRenovacion()` ejecutado por Hangfire al cierre de la ventana de renovación. Retirado de `IniciarNuevoPeriodo()`. | Resuelve la contradicción entre DDD y v1.3 y cumple RF-38 (no cortar renovaciones en curso al iniciar nuevo periodo). |
| **DDD-09** *(R12)* | Nuevo estado `APLAZADO` en `EstadoDesembolso` (§11b, §15.4, §16.1): `Programado → Aplazado` al suspender, `Aplazado → Programado` al reanudar, `Aplazado → Cancelado` al revocar/concluir. | Modela la regla del cliente R12 (desembolsos en fecha de suspensión se pagan al reanudar). |
| **DDD-10** *(v1.3 §8.2)* | `IServicioOtp` actualizada a `SolicitarDesafio(...)` y `ValidarYConsumir(usuarioId, operacion, recursoId, challengeId, codigo)` + enum `TipoOperacionSensible` + reglas R4 y R42 en §13. | Elimina el riesgo de replay entre operaciones y alinea Shared Kernel con `openapi-interno.yaml`. |
| **DDD-11** *(R1 / HU-01.1)* | Flujo `16.4f` y evento `UsuarioRegistrado`: S1 (fuente de verdad de identidad y cédula única) crea cuenta en Keycloak y publica; S3 crea `Postulante` idempotente por `UsuarioId`. | Define el alta distribuida entre S1 y S3 sin transacciones distribuidas ni cédulas duplicadas. |
| **DDD-12 / DDD-13** | `DatosPago.RegistrarConsulta(usuarioId, motivo)` (bitácora de lectura propia de S4, distinta de `RegistroDecision`) y VO `NumeroCuentaCifrado` con `Enmascarado()`. | Alinea S4 con HU-20.3, cifrado en reposo (ADR-18) y enmascaramiento en API. |
| **Ronda 5** *(R31–R41, R44)* | Monto de renovación al enviar (R31); reserva de oferta por toda la duración (R32); sin revocar desde *Otorgada*/*Renovada* (R33); `Postulacion.Rechazar()` solo manual (R34); API prevalece pero manual es canal normal (R35); `ConsentimientoDonante` por donante e irrevocable (R36); `EstadoMatriculaClasificado` (texto original + 5 estados internos, R37/C10); consecuencias progresivas (R41). Mantiene v1.3 en R39, R40 y R44. | Cierra todos los bloqueos funcionales de §20 que afectaban a S2, S3, S4, S5 y S6. |
| **DDD-15** | Corregidas referencias internas (`14.2→15.2`, `16.4b→16.4d`), tabla §6.3 verificada (**25 raíces, 18 entidades internas, 43 entidades, 35 VO, 35 enums, 35 eventos**) y control de versiones actualizado. | Coherencia documental total. |
| **Reparto §2.2** | Tabla §2.2 actualizada al reparto oficial adoptado por el equipo (`Jesús, Daniel, Diego, Leonel, Sebas`; 8 RF c/u con `RF-33, 34, 36, 40` en el productor de datos). | Alinea el DDD con el documento oficial de división de requerimientos. |

- **Diagramas y enlaces regenerados:** 45 archivos `.mmd` en [`docs/dominio/diagramas/`](../dominio/diagramas/README.md) y [`docs/dominio/enlaces-mermaid-live-DDD_v3.3.html`](../dominio/enlaces-mermaid-live-DDD_v3.3.html).
- **Referencias actualizadas:** [`openspec/config.yaml`](../../openspec/config.yaml), [`README.md`](../../README.md), [`docs/README.md`](../README.md) y [`docs/arquitectura/README.md`](../arquitectura/README.md).

### C. Contratos v1.3 y ADRs Correctivos (Prioridad 3)
- **[`openapi-interno.yaml`](../arquitectura/v1.3/03_CONTRATOS/openapi-interno.yaml) (`v1.3.0`):**
  - `POST /internal/v1/fondos/reservas`: cálculo por duración completa (`R32/A2`) + campo `duracionPeriodos`.
  - **Nuevo** `POST /internal/v1/fondos/reservas/renovaciones`: reserva al enviar solicitud de renovación (`R31/A1`).
  - **Nuevo** `GET /internal/v1/reservas/{reservaId}/vinculo` (scope `ofertas.consultar-vinculo`): conciliación segura `S2 → S4` (`ADR-26`).
- **[`openapi.yaml`](../arquitectura/v1.3/03_CONTRATOS/openapi.yaml):** `ReporteAcademicoLote.estadoMatricula` actualizado a texto libre universitario + clasificación interna (`R37/C10`).
- **[`EVENTOS_E_INTERFACES.md`](../arquitectura/v1.3/03_CONTRATOS/EVENTOS_E_INTERFACES.md) y [`CATALOGO_EVENTOS_INTEGRACION.csv`](../arquitectura/v1.3/04_MATRICES/CATALOGO_EVENTOS_INTEGRACION.csv):** cerrado **C4** (S3 alineado en eventos `Beca*`, manteniendo consulta síncrona fail-closed `S3→S4` en HU-01.2) y añadido `UsuarioRegistrado.v1`.
- **[`ADR_CORRECTIVOS_V1.3.md`](../arquitectura/ADR_CORRECTIVOS_V1.3.md):** seis ADRs adoptados que cierran los pendientes técnicos del equipo:
  - **ADR-25 (C3):** versión explícita `Version: int` en cada raíz (no `xmin`).
  - **ADR-26 (C5):** plazo de 30 min + conciliación `S2→S4` (falla cerrada) + resiliencia HTTP (`3 s`, máx. 2 reintentos idempotentes, circuit breaker).
  - **ADR-27 (C6):** despachador ligero propio en `Ascenso.SharedKernel` (sin riesgo de licencia MediatR 13+; alternativa documentada: `MediatR 12.5.0` fijado).
  - **ADR-28 (C7):** Next.js App Router como BFF ligero con cookies `HttpOnly + Secure + SameSite=Lax` (prohibido `localStorage`).
  - **ADR-29 (C10):** catálogo `EstadoMatricula` (`INSCRITO`, `RETIRADO`, `CONGELADO`, `EGRESADO`, `NO_RECONOCIDO`) conservando siempre `TextoOriginal`.
  - **ADR-30 (D2):** flujo de registro distribuido `S1 → UsuarioRegistrado.v1 → S3`.
- **[`DECISIONES_Y_BLOQUEOS.csv`](../arquitectura/v1.3/04_MATRICES/DECISIONES_Y_BLOQUEOS.csv) y [`pendientes-v1.3.md`](../arquitectura/pendientes-v1.3.md):** marcados como cerrados `A1–A8`, `B1–B4`, `B6`, `C2–C7`, `C10`, `D1` y `D2`.

---

## 3. Impacto rápido por integrante

- **Jesús (`SharedKernel`, `Contracts`, `S1`, `S2`, `RF-35`, `RF-33`):** implementa `RaizDeAgregado.Version`, `IServicioOtp`, despachador ligero (`ADR-27`), `UsuarioRegistrado.v1`, reservas por duración completa (`R32`) y renovación (`R31`), conciliación `S2→S4` (`ADR-26`) y `ConsentimientoDonante` por donante e irrevocable (`R36`).
- **Daniel (`S3` Postulación, `S6` Comms):** consume `UsuarioRegistrado.v1` (Inbox idempotente), mantiene consulta síncrona fail-closed `S3→S4` en HU-01.2 (`R39`), `Consulta` con sujeto opcional (`R44`), y recibe eventos directos en S6 sin Trazabilidad intermedia (`DDD-14`).
- **Diego (`S3` Evaluación, `S6` Panel):** `Postulacion.Rechazar()` solo manual (`R34`); puede empezar dominio en cuanto Jesús publique `Ascenso.SharedKernel`.
- **Leonel (`S4` Ofertas/Desembolsos, `S6` Financiero/Auditoría):** `Beca` incrementa `Version` en pagos (`DDD-07`), estado `APLAZADO` en desembolsos (`R12`), `NumeroCuentaCifrado` + bitácora de lectura (`DDD-12/13`), y expone `GET /internal/v1/reservas/{reservaId}/vinculo` para S2 (`ADR-26`).
- **Sebas (`S4` Ciclo de Beca, `S5` Seguimiento, `S6` Institucional):** único disparador `InactivarPorNoRenovacion()` al cierre de ventana (`R38`), sin revocar desde *Otorgada*/*Renovada* (`R33`), `EstadoMatriculaClasificado` (`ADR-29`), API prevalece sobre manual (`R35`) y consecuencias progresivas (`R41`).

---

## 4. Verificación ejecutada

- `openspec validate --all --strict`: **0 errores**.
- Auditoría sobre `DDD_v3.3.md`: 45 diagramas regenerados, 0 firmas viejas de `IServicioOtp`, 0 referencias rotas, 1 única transición `Activa --> Inactiva`, conteos exactos en §6.3, `DDD_v3.2.md` intacto.
- Contratos YAML y matrices CSV verificados sin errores de formato.
