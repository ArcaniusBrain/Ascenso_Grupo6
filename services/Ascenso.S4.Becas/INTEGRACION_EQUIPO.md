# Integración del Equipo — S4 · Gestión de Becas (`Ascenso.S4.Becas`)

Este documento registra cómo se conectan los RF implementados por **Leonel** en `S4` con el trabajo de los demás integrantes del equipo, indicando los puntos exactos en el código y qué piezas están simuladas hasta la integración.

---

## `RF-18` · Consultar historial de becas (`consultar-historial-becas`)

- **Estado:** Implementado en S4 (consulta de solo lectura, `HU-18.1`).
- **Endpoint público:** `GET /api/v1/becas/historial` (`cedula` | `programaId` | `periodo`) en `services/Ascenso.S4.Becas/Ascenso.S4.Becas.Api/Controllers/HistorialBecasController.cs` (documentado en `contracts/openapi.yaml`).

### Conexiones por compañero

| Compañero | Qué comparte / conecta | Punto exacto en código | Qué quedó pendiente / simulado |
|---|---|---|---|
| **Sebas** (S4 — ciclo de vida de `Beca`) | Esqueleto compartido del agregado `Beca`, `TramoBeca`, `EstadoBeca` y `BecaDbContext` (schema `s4`, Outbox/Inbox y `RegistroDecision` listos). | `services/Ascenso.S4.Becas/Ascenso.S4.Becas.Domain/Becas/Beca.cs`, `EnumsBeca.cs`, `TramoBeca.cs`, `PoliticaEstadoBeca.cs` y `services/Ascenso.S4.Becas/Ascenso.S4.Becas.Infrastructure/Persistencia/BecaDbContext.cs` | Cuando acordemos las transiciones de `Beca` (`DDD_v3.3 §15.2`), Sebas agrega sus métodos de ciclo de vida (`Suspender`, `Pausar`, `Renovar`, `Revocar`, `Concluir`) sobre `Beca.cs` sin duplicar `EstadoBeca` ni `BecaDbContext`. |
| **Jesús** (S2 — programas y catálogos) | Proyección local de programas en S4 para mostrar nombre y marca `Completo`/`Parcial` sin leer el schema `s2`. | `ReferenciaPrograma` en `services/Ascenso.S4.Becas/Ascenso.S4.Becas.Infrastructure/Persistencia/ReferenciasLocales.cs` | En pruebas se siembra directamente. Al integrar S2 → S4, un consumidor Inbox actualizará `s4.referencia_programas` desde los eventos de `Programa` de S2. |
| **Daniel y Diego** (S3 — postulantes y personas) | Proyección local mínima de persona (`PersonaId`, `Cedula`, `NombreCompleto`, **sin datos socioeconómicos**) para búsqueda por cédula. | `ReferenciaPersona` en `services/Ascenso.S4.Becas/Ascenso.S4.Becas.Infrastructure/Persistencia/ReferenciasLocales.cs` | En pruebas se siembra directamente. Al integrar S3/S1 → S4, un consumidor Inbox alimentará `s4.referencia_personas`. Para `HU-01.2` (consulta síncrona `S3 -> S4`), se reutilizará `PoliticaEstadoBeca.CuentaComoActiva` en RF-16. |
