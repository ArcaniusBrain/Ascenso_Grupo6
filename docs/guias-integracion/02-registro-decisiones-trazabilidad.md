# 02 · Registro Inmutable de Decisiones (`RF-35`, `HU-35.1` y `HU-35.2`)

Toda acción que cambie un estado relevante o involucre una decisión de negocio en cualquier microservicio (`S1`–`S6`) **debe persistir un `RegistroDecision` en la misma transacción local** que el agregado modificado (`ADR-12`, `D-6`). Si el registro falla, el cambio de estado también se revierte (`HU-35.1 E3`).

---

## 1. Piezas listas para usar

| Clase / Archivo | Paquete | Para qué sirve |
|---|---|---|
| `RegistroDecision`, `SujetoRef`, `ActorDecision`, `TipoSujeto`, `TipoDecision` | `Ascenso.SharedKernel.Trazabilidad` | Modelo inmutable (no es raíz de agregado, no tiene `Update` ni `Delete`) con los 38 tipos de decisión del sistema |
| `PoliticaAutorizacionDecision`, `IConsultaRegistroDecisiones` | `Ascenso.SharedKernel.Trazabilidad` | Control de lectura por rol (`C12`) |
| `DecisionRegistradaV1` | `Ascenso.Contracts.Eventos` | Evento de integración emitido automáticamente por Outbox hacia `S6` |
| `ConfigurarRegistroDecisiones()` + `AppendOnlyRegistroDecisionInterceptor` | `Ascenso.Messaging.Outbox` | Mapeo EF Core + bloqueo en tiempo de ejecución contra `UPDATE`/`DELETE` (`ExcepcionInmutabilidadRegistro`) |
| `UnidadAtomicaDecisiones.ConfirmarAsync(...)` | `Ascenso.Messaging.Outbox` | Guarda en una sola transacción: **Agregado + `RegistroDecision` + `OutboxMessage(DecisionRegistradaV1)` + otros mensajes Outbox** |
| `ConsultorRegistroDecisionesEfCore` | `Ascenso.Messaging.Outbox` | Implementación lista de `IConsultaRegistroDecisiones` con filtros y autorización |
| `public.impedir_modificacion_registro_decisiones()` | `deploy/postgres/init-schemas.sql` | Función trigger PostgreSQL para blindar tu tabla `registro_decisiones` a nivel de BD |

---

## 2. Cómo registrar una decisión en tu servicio (patrón atómico)

### Decisión manual (tomada por un usuario con rol de decisión)
```csharp
var registro = await UnidadAtomicaDecisiones.ConfirmarAsync(
    dbContext,
    ejecutarOperacionDeDominio: async ct =>
    {
        var beca = await becaRepo.ObtenerPorIdAsync(becaId, ct);
        beca.Suspender(motivo, reloj.AhoraUtc); // IncrementarVersion() dentro del dominio

        return RegistroDecision.Crear(
            tipo: TipoDecision.BECA_SUSPENDIDA,
            sujeto: new SujetoRef(TipoSujeto.BECA, beca.Id.ToString()),
            actor: ActorDecision.Manual(actorUsuarioId, "COORDINADOR"),
            ocurridoEnUtc: reloj.AhoraUtc,
            justificacionTexto: justificacion, // obligatoria; vacía lanza "Escribe la justificación de esta decisión."
            servicioOrigen: "S4.Becas",
            correlationId: operationId,
            programaId: beca.ProgramaId,           // obligatorio para visibilidad de Coordinadores (C12)
            propietarioUsuarioId: beca.PostulanteId, // obligatorio para visibilidad del Becario/Postulante (C12)
            estadoAnterior: "ACTIVA",
            estadoNuevo: "SUSPENDIDA");
    },
    mensajesAdicionales: [outboxBecaSuspendida], // opcional: tus otros eventos de dominio/integración
    cancellationToken: ct);
```

### Decisión automática del sistema (`C11` — ej. entrada automática a `Inactiva` por Hangfire)
```csharp
var registro = RegistroDecision.CrearAutomatica(
    tipo: TipoDecision.BECA_INACTIVADA_AUTOMATICAMENTE,
    sujeto: new SujetoRef(TipoSujeto.BECA, beca.Id.ToString()),
    ocurridoEnUtc: reloj.AhoraUtc,
    justificacionAutomatica: RegistroDecision.JustificacionInactivacionNoRenovacion,
    servicioOrigen: "S4.Becas",
    correlationId: jobExecutionId,
    programaId: beca.ProgramaId,
    propietarioUsuarioId: beca.PostulanteId,
    estadoAnterior: "ACTIVA",
    estadoNuevo: "INACTIVA");
```

> **Regla `C11`:** las entradas automáticas a `Inactiva` siempre van firmadas con `CrearAutomatica` (`ActorEsSistema = true`, rol `SISTEMA`); la salida de `Inactiva` (reactivación o cierre) siempre la decide un coordinador con `Crear(...)` y justificación obligatoria.

---

## 3. Reglas de visibilidad al consultar (`C12`, `C13` — `HU-35.2`)

`PoliticaAutorizacionDecision` y `ConsultorRegistroDecisionesEfCore` aplican automáticamente estos alcances (por eso es **crítico** llenar `ProgramaId` y `PropietarioUsuarioId` al crear cada registro):

| Rol | Qué puede ver en `GET /api/v1/registro-decisiones` |
|---|---|
| **Dirección** | Todas las decisiones de todos los programas |
| **Coordinador** | Solo las decisiones cuyos `ProgramaId` pertenezcan a sus programas asignados |
| **Finanzas** | Solo decisiones de ámbito financiero (`TipoDecision.EsAmbitoFinanciero()` — 6 tipos) |
| **Becario / Postulante** | Solo las decisiones de su propio caso (`PropietarioUsuarioId == usuarioActualId`) |

---

## 4. Blindaje append-only en tu migración PostgreSQL

En la migración inicial de tu servicio, vincula la función trigger compartida sobre tu tabla `registro_decisiones` (puedes usar el helper `RegistroDecisionSqlAppendOnly` de `Ascenso.Messaging.Outbox`):

```sql
CREATE TRIGGER trg_registro_decisiones_inmutable
BEFORE UPDATE OR DELETE ON <tu_schema>.registro_decisiones
FOR EACH ROW EXECUTE FUNCTION public.impedir_modificacion_registro_decisiones();
```
