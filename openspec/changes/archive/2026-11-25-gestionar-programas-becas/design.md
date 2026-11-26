# Design — `gestionar-programas-becas` (RF-11 · S2)

## 1. Contexto, agregados DDD y ADRs aplicados

- **Historias de usuario:** `HU-11.1` (11 escenarios) y `HU-11.2` (14 escenarios).
- **Modelo de dominio (`DDD_v3.3.md` §8):**
  - **Raíz de agregado `Programa : RaizDeAgregado<Guid>`** (con `Version` explícita `ADR-25`):
    - `Nombre` (`string`) y `NombreNormalizado` (`string`, unicidad insensible a mayúsculas/espacios, con corrección permitida de mayúsculas/minúsculas sobre el mismo programa).
    - `PeriodoInicio` (`CodigoPeriodo`, validado contra `CalendarioAcademico`).
    - `CupoBecarios` (`int > 0`), `DuracionPeriodos` (`int > 0`).
    - `MontoPorPeriodo` (`Dinero`, `Cantidad > 0`), `Modalidad` (`ModalidadPago`: `MENSUAL` | `SEMESTRAL` | `DIRECTO_UNIVERSIDAD`), `Alcance` (`AlcancePrograma`: `COMPLETO` | `PARCIAL`), `PermiteAcumulacion` (`bool`, siempre `false` cuando `Alcance == COMPLETO`).
    - **Entidad interna `RestriccionElegibilidad : Entidad<Guid>`**: `ProgramaId`, `UniversidadId`, `NombreUniversidad`, `Carreras` (lista estructurada de carreras elegibles validadas contra `UniversidadAliada`; lista vacía = restricción a nivel de universidad `HU-11.2 E12`).
    - **Entidad interna `AsociacionDonantePrograma : Entidad<Guid>`**: `ProgramaId`, `DonanteId`, `AsociadoEnUtc` (clave única `(ProgramaId, DonanteId)` → duplicado rechaza con `"Este donante ya está asociado al programa."`, `HU-11.2 E10`).
    - **Value Object `CondicionesPermanencia`**: `PromedioMinimo` (`decimal >= 0`), `ExigeHorasServicio` (`bool`), `HorasServicio` (`int? >= 0`; si `!ExigeHorasServicio` devuelve leyenda `"Horas de servicio: no aplica"`, `HU-11.2 E14`), `ReportesExigidos` (`IReadOnlyList<string>`), `DuracionMaxPausaMeses` (`int >= 0`; `0` = no permite pausas), `DescripcionRenovacion` (`string`).
    - **Evento de dominio:** `ProgramaCreadoEvento` (`IEventoDeDominio`).
- **Conexión con agregados vecinos dentro de `S2` (`s2_catalogo_fondos`):**
  - Valida existencia de periodos en `ICalendarioAcademicoRepository` (`RF-38`).
  - Valida universidades aliadas y carreras elegibles en `IUniversidadAliadaRepository` (`RF-14`).
  - Consulta existencia de donantes y sumatoria de aportes por donante mediante el puerto `IConsultorDonantesYAportes` (implementación inicial por defecto sin donantes registrados; cuando entre `RF-13` en el siguiente PR de `S2`, se conecta la implementación EF Core sobre `donantes`/`aportes` sin tocar `Programa`).
  - Actualiza `ConsultorElementosPeriodo` de `RF-38` para contar programas reales ligados a un `CodigoPeriodo` en `CatalogoFondosDbContext`.
- **ADRs aplicados:** `ADR-01/02/04` (Clean Architecture + `ArquitecturaCatalogoFondosTests`), `ADR-05` (`s2_catalogo_fondos`), `ADR-12/13` (Outbox), `ADR-16/25` (`Version` explícita) y `ADR-27` (`MediatR 12.5.0`).

## 2. Eventos de integración

- **`ProgramaCreadoEvento`** (evento de dominio interno en `S2`; el catálogo CSV marca `ProgramaCreado` como productor `S2` → consumidor `S2` para inicializar `PresupuestoPrograma` cuando se implemente `RF-12`).
- No se agregan ni rompen contratos en `Ascenso.Contracts` ni `Ascenso.SharedKernel`.

## 3. Contratos públicos de S2 (`contracts/openapi.yaml`)

- **Ruta existente ampliada de forma compatible:**
  - `POST /api/v1/programas` (crea el programa validando `HU-11.1`, ampliando `CrearProgramaCommand` con `periodoInicio`, `cupoBecarios`, `duracionPeriodos`, `modalidadPago`, `alcance`, `permiteAcumulacion` y devolviendo el detalle creado).
- **Rutas complementarias de `RF-11` bajo tag `S2`:**
  - `GET /api/v1/programas` (lista programas registrados).
  - `GET /api/v1/programas/{programaId}` (detalle completo con donantes/aportes, presupuesto total `S8`, restricciones de elegibilidad y condiciones de permanencia/renovación).
  - `PUT /api/v1/programas/{programaId}/configuracion-base` (actualiza nombre, monto por periodo, modalidad de pago, alcance y `permiteAcumulacion`).
  - `POST /api/v1/programas/{programaId}/donantes` (asocia un donante registrado sin duplicar, `HU-11.2`).
  - `PUT /api/v1/programas/{programaId}/elegibilidad` (configura restricciones opcionales por universidad aliada y carreras elegibles, `HU-11.2`).
  - `PUT /api/v1/programas/{programaId}/condiciones` (configura condiciones de permanencia, duración máxima de pausa y condiciones de renovación, `HU-11.2`).

## 4. Decisiones cerradas del Anexo B (`HU-11.1` y `HU-11.2`, 25-nov-2026 — C17)

1. **Anexo B · HU-11.1 #1 y #2 (Periodo académico al crear programa):** **Confirmado** — exige al menos un periodo registrado (`HU-11.1 E3`) y acepta cualquier periodo en estado `ABIERTO` (rechaza `CERRADO`).
2. **Anexo B · HU-11.1 #3 (Unicidad del nombre del programa):** **Confirmado** — nombre único normalizado insensible a mayúsculas/espacios (`409 Conflict` ante duplicado con otro programa) permitiendo corregir mayúsculas/minúsculas del mismo programa.
3. **Anexo B · HU-11.1 #4 y HU-11.2 #4 (Quién crea y configura programas):** **Confirmado** — Dirección (global) y Coordinador (creación y edición en sus programas asignados si se informa alcance), bloqueando a Evaluador, Finanzas, Enlace universitario, Becario y Postulante.
4. **Anexo B · HU-11.1 #5 (Modalidad de pago):** **Confirmado** — enum `MENSUAL`, `SEMESTRAL`, `DIRECTO_UNIVERSIDAD` obligatorio.
5. **Anexo B · HU-11.2 #1 (Duración máxima de pausa en meses, `RF-27`):** **Confirmado** — `>= 0` (`0` o `null` = no permite pausas; negativos rechazados con `"La duración máxima de pausa no puede ser negativa."`).
6. **Anexo B · HU-11.2 #2 (Condiciones de renovación):** **Confirmado** — `DescripcionRenovacion` descriptiva junto a campos tipados de `CondicionesPermanencia`.
7. **Anexo B · HU-11.2 #3 (Cambios de restricción con postulaciones ya en curso):** **Confirmado** — aplican solo a postulaciones futuras, sin retroactividad.
8. **Anexo B · HU-01.2 #4 (`permiteAcumulacion` en `Programa`):** **Confirmado** — forzado a `false` si `COMPLETO`, configurable si `PARCIAL`.
