# Design — `gestionar-presupuesto-cupos` (RF-12 · S2)

## 1. Contexto, agregado DDD y ADRs aplicados

- **Historias de usuario:** `HU-12.1` (12 escenarios) y `HU-12.2` (5 escenarios).
- **Raíz de agregado `PresupuestoPrograma : RaizDeAgregado<Guid>` (`DDD_v3.3.md` §8, `ADR-25`):**
  - Saldo materializado 1:1 por `ProgramaId` en `s2_catalogo_fondos.presupuestos_programa`, con `Version` explícita (`IsConcurrencyToken()`).
  - Propiedades persistidas: `ProgramaId`, `Moneda`, `Aportes`, `Otorgado`, `OfertasPendientes`, `RenovacionesPendientes`, `Desembolsado`, `CupoBecarios`, `CuposOcupados`, `ActualizadoEnUtc`.
  - Fórmulas derivadas:
    - `Disponible = Aportes - Otorgado - OfertasPendientes - RenovacionesPendientes` (invariante `>= 0`).
    - `CupoDisponible = CupoBecarios - CuposOcupados` (invariante `>= 0`).
  - **Entidad interna `CupoCiclo : Entidad<Guid>`:** `(Id, PresupuestoProgramaId, CicloId, Limite, Ocupados)` para controlar cupo por convocatoria en la misma transacción que el cupo total del programa (`DDD §8`).
  - **Entidad interna `ReservaPresupuesto : Entidad<Guid>`:**
    - `Id` (`reservaId`), `PresupuestoProgramaId`, `Tipo` (`OFERTA_NUEVA` | `RENOVACION`), `Estado` (`RESERVADA` | `VINCULADA` | `CONFIRMADA` | `LIBERADA`).
    - `ClaveNegocio` única para reservas activas (`OFERTA:{postulacionId}:{cicloId}` o `RENOVACION:{becaId}:{codigoPeriodo}`).
    - `OperationId` único para idempotencia de reintentos HTTP.
    - `PostulacionId`, `CicloId`, `BecaId`, `CodigoPeriodo`, `Monto` (`Dinero`), `DuracionPeriodos`, `CreadaEnUtc`, `VinculadaEnUtc`, `CerradaEnUtc`, `MotivoLiberacion`, `RecursoVinculadoId`.
  - **Sincronización con `RF-11` y `RF-13`:**
    - Al crear un `Programa` (`CrearProgramaHandler`) se crea su `PresupuestoPrograma` con `CupoBecarios` y `Moneda`.
    - Al registrar un `Aporte` (`RegistrarAporteHandler`) se actualiza `PresupuestoPrograma.RegistrarAporte(monto)` de forma idempotente por `AporteId` (usando `operaciones_idempotentes_catalogo` / verificación idempotente), garantizando que `GET /api/v1/programas/{programaId}/presupuesto` refleje cada aporte de inmediato.
  - **Consistencia bajo concurrencia (`ADR-25`):**
    - Toda mutación sobre `PresupuestoPrograma` o sus entidades hijas (`ReservaPresupuesto`, `CupoCiclo`) invoca `IncrementarVersion()`.
    - En `ReservarOferta` / `ReservarRenovacion`, ante `DbUpdateConcurrencyException` se ejecuta un reintento acotado recargando la entidad fresca; si tras recargar el cupo o presupuesto ya fue agotado por el competidor, devuelve el mensaje literal exacto (`"El programa no tiene cupo disponible."` o `"Presupuesto insuficiente: disponible ..., la beca requiere ..."`).
- **ADRs aplicados:** `ADR-01/02/03/04`, `ADR-05` (`s2_catalogo_fondos`), `ADR-12/13` (Outbox `CupoLiberado.v1` + idempotencia por `operationId`), `ADR-16/25` (`Version` explícita), `ADR-26` (conciliación segura `S2 → S4` 30 min + fail closed + timeout 3s / máx. 2 reintentos idempotentes), `ADR-27` (`MediatR 12.5.0`).

## 2. Eventos de integración

- **`CupoLiberado.v1`** (`Ascenso.Contracts.Eventos.CupoLiberadoV1`, versión `1`, aditivo en `EventosCatalogoFondosV1.cs`):
  - **Campos:** `(ProgramaId, CicloId, ReservaId, Motivo, MontoLiberado, Moneda, LiberadoEnUtc)`.
  - **Productor:** `S2` (escrito en `outbox_messages` en la misma transacción al liberar oferta o revocar beca).
  - **Consumidores:** `S3` (sugerencia de lista de espera) y `S6` (proyecciones financieras e indicadores).

## 3. Contratos públicos y privados

### API pública (`contracts/openapi.yaml`, tag `S2`)
- `GET /api/v1/programas/{programaId}/presupuesto` — `DIRECCION`, `COORDINADOR`, `FINANZAS` (`403` para Becario, Postulante, Evaluador, Enlace universitario).

### Frontera privada interservicios (`contracts/openapi-interno.yaml`, nunca expuesta por YARP)
- `POST /internal/v1/fondos/reservas` — scope `fondos.reservar` (`S4 → S2`, `R32/A2`).
- `POST /internal/v1/fondos/reservas/renovaciones` — scope `fondos.reservar` (`S4 → S2`, `R31/A1`).
- `POST /internal/v1/fondos/reservas/{reservaId}/liberar` — scope `fondos.liberar` (`S4 → S2`).
- `GET /internal/v1/fondos/{programaId}/disponibilidad` — scope `fondos.consultar` (`S3 → S2`).
- Consumo saliente `S2 → S4`: `GET /internal/v1/reservas/{reservaId}/vinculo` — scope `ofertas.consultar-vinculo` (`ADR-26`).

## 4. Decisiones del Anexo B (`HU-12.1` y `HU-12.2`) — confirmadas (`C19`)

1. **Saldo materializado en `PresupuestoPrograma` (`Anexo B · HU-11.2/12.1/12.2/13.2`):** `PresupuestoPrograma` opera como saldo materializado con `Version` explícita (`ADR-25`) sincronizado idempotentemente con programas (`RF-11`) y aportes (`RF-13`), siguiendo `DDD_v3.3.md` §8 al pie de la letra.
2. **Permisos de consulta (`Anexo B · HU-12.1 #2`):** `DIRECCION`, `COORDINADOR` y `FINANZAS` consultan `GET /api/v1/programas/{programaId}/presupuesto` (Finanzas solo lectura, sin otorgar ni reservar); `EVALUADOR`, `ENLACE_UNIVERSIDAD`, `BECARIO` y `POSTULANTE` bloqueados (`403`).
3. **Causales de liberación vs `Inactiva` (`Anexo B · HU-12.1 #3`):** `RECHAZO`, `EXPIRACION` y `COMPENSACION` liberan cupo y presupuesto de ofertas pendientes; `REVOCACION` libera cupo y monto no desembolsado; `BecaInactivada` nunca libera cupo ni presupuesto (`R27`, `HU-12.1 E12`).
4. **Concurrencia en renovaciones (`Anexo B · HU-12.2 #2 y #3`):** dado que Ronda 5 (`A1/R31`) fija la reserva al enviar la solicitud, dos solicitudes de renovación simultáneas que exceden el disponible rechazan la segunda con `409 Conflict` (presupuesto insuficiente), manteniendo `Disponible >= 0` siempre. La expiración de renovaciones sin resolver queda gobernada por el cierre de ventana en `S4` (`C2`) y el reconciliador de 30 min (`ADR-26`).
5. **Evento `CupoLiberadoV1` y resiliencia HTTP `S2 → S4` (`ADR-26`):** `CupoLiberadoV1` (aditivo) añadido en `Ascenso.Contracts` y cliente resiliente `S2 → S4` implementado con `HttpClient` estándar (timeout 3 s, máximo 2 reintentos en `GET` idempotente, circuit breaker ligero y política fail closed) sin añadir paquetes externos a `Directory.Packages.props`.
