# Design — `gestionar-universidades-convenios` (RF-14 · S2)

## 1. Contexto, agregados DDD y ADRs aplicados

- **Historias de usuario:** `HU-14.1` (10 escenarios), `HU-14.2` (5 escenarios) y `HU-14.3` (6 escenarios, delegando el efecto de cierre de disponibilidades de postulación/lista de espera a `S3` en `HU-02.2`/`HU-10.2`).
- **Modelo de dominio (`DDD_v3.3.md` §7):**
  - **Raíz de agregado `UniversidadAliada : RaizDeAgregado<Guid>`** (con `Version` explícita `ADR-25`):
    - `Nombre` (`string`) y `NombreNormalizado` (`string`, unicidad por universidad).
    - **Value Object `ContactoDesignado`**: `NombreCompleto` (`string`), `Correo` (`string`, validado vía `DatosContacto` o formato correo) y `Telefono` (`string`, opcional o validado según convenio). Si falta, lanza `"Indica el contacto designado de la universidad."` (`HU-14.1 E2`).
    - **Value Object `Convenio`**: `Terminos` (`string`, obligatorio → `"Indica los términos del convenio."`, `HU-14.1 E8`) y `Via` (`ViaDatosAcademicos`: `API` | `MANUAL`, `HU-14.1 E9/E10`).
    - **Entidad interna `CarreraElegible : Entidad<Guid>`**: `UniversidadAliadaId`, `Nombre`, `NombreNormalizado` (aislada por universidad `HU-14.1 E4`; nombre vacío → `"Indica el nombre de la carrera."`, `HU-14.1 E5`; duplicada dentro de la misma universidad → `409 Conflict`).
    - **Entidad interna `CostoMatricula : Entidad<Guid>` (append-only, `HU-14.2`):** `UniversidadAliadaId`, `Periodo` (`CodigoPeriodo`), `Costo` (`Dinero`, `Cantidad > 0` → si `<= 0` lanza `"El costo de matrícula debe ser mayor que cero."`, `HU-14.2 E4`), `VersionRegistro` (`int`, incremental por `(UniversidadAliadaId, Periodo)`), `RegistradoEnUtc` (`DateTimeOffset`).
      - **Sin clave única sobre `(UniversidadAliadaId, Periodo)`** (`Anexo A · HU-14.2`): cada corrección inserta una nueva fila inmutable; `CostoVigente(CodigoPeriodo)` devuelve la entrada con mayor `VersionRegistro` (más reciente) y `HistorialCostos(CodigoPeriodo)` conserva todas las entradas consultables (`HU-14.2 E2`).
    - **Métodos del agregado:** `Crear(...)`, `ActualizarConvenioYContacto(...)`, `AgregarCarrera(string? nombre)`, `RegistrarCosto(CodigoPeriodo periodo, Dinero costo, DateTimeOffset ahoraUtc)`, `CostoVigente(CodigoPeriodo periodo)`, `HistorialCostos(CodigoPeriodo periodo)`.
  - **Integración con `InicioInscripcionesPeriodo` (ya creada en `RF-38` dentro de `s2_catalogo_fondos`):**
    - `HU-14.3` reutiliza la persistencia existente `inicios_inscripciones_periodo` de `S2`, validando la existencia tanto de `UniversidadAliada` (`HU-14.1`) como del periodo en `CalendarioAcademico` (`HU-38.1`) — devolviendo `"No hay periodos académicos registrados."` si el calendario está vacío (`HU-14.3 E4`) y `"Indica la fecha de inicio de inscripciones."` si falta la fecha (`HU-14.3 E5`).
    - Mantiene 100 % compatible el endpoint previo `POST /api/v1/periodos/{codigoPeriodo}/inicios-inscripciones` de `RF-38`.
  - **Eventos de dominio:** `UniversidadRegistradaEvento` y `CostoMatriculaRegistradoEvento` (`IEventoDeDominio`).
- **ADRs aplicados:** `ADR-01/02/04` (Clean Architecture 4 capas + `ArquitecturaCatalogoFondosTests`), `ADR-05` (`CatalogoFondosDbContext` en schema `s2_catalogo_fondos`), `ADR-12/13` (Outbox atómico), `ADR-16/25` (`Version` explícita), `ADR-17/R20` (`IRelojNegocio` UTC-4) y `ADR-27` (`MediatR 12.5.0`).

## 2. Eventos de integración

- **`CostoMatriculaVigente.v1`** (`Ascenso.Contracts.Eventos.CostoMatriculaVigenteV1`, ya existente en `Ascenso.Contracts`, versión `1`):
  - **Campos:** `(UniversidadId, CodigoPeriodo, Monto, Moneda, VersionRegistro, VigenteDesdeUtc)`.
  - **Productor:** `S2` (escrito en `outbox_messages` en la misma transacción local que `UniversidadAliada.RegistrarCosto`).
  - **Consumidor:** `S4` (proyecta el costo vigente por universidad y periodo para el tope de suma de becas `C2`, `HU-01.2` / `HU-16.1`).
  - No se modifica `Ascenso.Contracts` ni `Ascenso.SharedKernel`.

## 3. Contratos públicos de S2 (`contracts/openapi.yaml`)

Rutas nuevas bajo el tag `S2`:
- `GET /api/v1/universidades` (lista de universidades aliadas; accesible a personal interno y postulantes/becarios autenticados para selección en `HU-01.1 E3`).
- `POST /api/v1/universidades` (registra universidad aliada, convenio, vía académica y contacto designado).
- `GET /api/v1/universidades/{universidadId}` (detalle con convenio, contacto, carreras elegibles — o mensaje `"Esta universidad no tiene carreras elegibles registradas."` — y costos vigentes por periodo).
- `PUT /api/v1/universidades/{universidadId}/convenio` (actualiza términos/vía/contacto; bloquea explícitamente al enlace de universidad `HU-14.1 E7`).
- `POST /api/v1/universidades/{universidadId}/carreras` (agrega carrera elegible aislada a esa universidad).
- `POST /api/v1/universidades/{universidadId}/costos-matricula` (registra costo append-only y publica `CostoMatriculaVigente.v1`).
- `GET /api/v1/universidades/{universidadId}/costos-matricula/{codigoPeriodo}` (costo vigente + historial completo, o mensaje `"Sin costo registrado para este periodo"` si aún no existe).
- `POST /api/v1/universidades/{universidadId}/inicios-inscripciones` (registra fecha de inicio de inscripciones para un periodo registrado, `HU-14.3`).
- `GET /api/v1/universidades/{universidadId}/inicios-inscripciones/{codigoPeriodo}` (consulta fecha + indicador `inscripcionesIniciadas` en UTC-4, o mensaje `"Sin fecha de inicio de inscripciones registrada para este periodo"`).

## 4. Decisiones cerradas del Anexo B (`HU-14.1` y `HU-14.3`, 25-nov-2026 — C16)

*(Nota: `HU-14.1 #2` "quién crea la cuenta del enlace" ya estaba cerrada en `C14` = Dirección; `HU-14.3 #1` "hora de inicio UTC-4" ya estaba cerrada en `C15` = `00:00` UTC-4).*

1. **Anexo B · HU-14.1 #4 y #6 (Quién gestiona universidades, convenios, carreras y costos):** **Confirmado** — Dirección y Coordinador de programa; bloqueado para Enlace de universidad, Finanzas, Evaluador, Becario y Postulante.
2. **Anexo B · HU-14.1 #5 (Unicidad y corrección de mayúsculas/minúsculas del nombre):** **Confirmado** — nombre único normalizado (insensible a mayúsculas y espacios repetidos, `409 Conflict` ante duplicado con otra universidad), permitiendo corregir las mayúsculas/minúsculas del nombre de una universidad ya registrada sin colisionar consigo misma.
3. **Anexo B · HU-14.1 #1 (¿Puede el enlace de universidad editar sus propios datos de contacto?):** **Confirmado: no** (solo Dirección y Coordinador editan convenio y contacto; por la API universitaria de `HU-24.3` solo entran reportes académicos a `S5`).
4. **Anexo B · HU-14.3 #2 (Quién registra la fecha de inicio de inscripciones desde `HU-14.3`):** **Confirmado** — Dirección y Coordinador en `/api/v1/universidades/{id}/inicios-inscripciones` (manteniendo solo Dirección en la ruta administrativa del calendario `/api/v1/periodos/{codigo}/inicios-inscripciones`).
5. **Anexo B · HU-14.3 #3 (Corrección de una fecha de inicio ya registrada):** **Confirmado** — se conserva la regla de `HU-38.1 E13` (una sola fecha por `(universidad, periodo)`, rechazando duplicados y conservando la registrada).
6. **Anexo B · HU-14.3 #4 (Universidad sin fecha de inicio registrada en un periodo):** **Confirmado** — disponibilidades abiertas hasta el cierre común del ciclo (`inscripcionesIniciadas = false`) y mensaje `"Sin fecha de inicio de inscripciones registrada para este periodo"`.
7. **Moneda del costo de matrícula (`HU-14.2`):** **Confirmado** — código ISO-3 opcional con **`USD` por defecto**.
