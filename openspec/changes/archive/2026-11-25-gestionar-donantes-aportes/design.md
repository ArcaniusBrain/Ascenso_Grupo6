# Design — `gestionar-donantes-aportes` (RF-13 · S2)

## 1. Contexto, agregados DDD y ADRs aplicados

- **Historias de usuario:** `HU-13.1` (5 escenarios) y `HU-13.2` (9 escenarios).
- **Modelo de dominio (`DDD_v3.3.md` §8):**
  - **Raíz de agregado `Donante : RaizDeAgregado<Guid>`** (con `Version` explícita `ADR-25`):
    - `Nombre` (`string`) y `NombreNormalizado` (`string`, unicidad insensible a mayúsculas/espacios).
    - `Tipo` (`TipoDonante`: `ORGANIZACION` | `PERSONA`, atributo único según Anexo A `HU-13.1`).
    - **Value Object `ContactoDonante`**: `Correo` (`string?`, validado si se suministra) y `Telefono` (`string?`); exige al menos uno presente → si faltan ambos lanza `"Indica los datos de contacto del donante."` (`HU-13.1 E3`).
    - **Entidad interna append-only `Aporte : Entidad<Guid>`** (`Anexo A · HU-13.2`): `Id`, `DonanteId`, `ProgramaId`, `Periodo` (`CodigoPeriodo`), `Monto` (`Dinero`, `Cantidad > 0` → si `<= 0` lanza `"El monto del aporte debe ser mayor que cero."`, `HU-13.2 E7`), `RegistradoEnUtc` (`DateTimeOffset`).
      - Nunca sobrescribe ni elimina aportes anteriores: cada registro inserta una fila inmutable en `aportes`.
    - **Eventos de dominio:** `DonanteRegistradoEvento` y `AporteRegistradoEvento` (`IEventoDeDominio`).
  - **Integración real en `S2` (`s2_catalogo_fondos`):**
    - `ConsultorDonantesYAportesEfCore` reemplaza a `ConsultorDonantesYAportesPorDefecto`: `GET /api/v1/programas/{programaId}` (`HU-11.2`) lee directamente las tablas `donantes` y `aportes`, sumando los aportes reales de cada donante en el presupuesto total (`S8`).
    - Al registrar un aporte para un programa donde el donante todavía no estaba listado en `programa_donantes`, `Programa` y `Donante` siguen respetando la regla **"una transacción, un agregado"** (la lectura de `ConsultarProgramaDetalleHandler` ya contempla aportes por `donanteId` tanto de donantes asociados explícitamente como de aportes registrados!).
- **ADRs aplicados:** `ADR-01/02/04`, `ADR-05` (`s2_catalogo_fondos`), `ADR-12/13` (Outbox atómico `AporteRegistrado.v1`), `ADR-16/25` (`Version` explícita) y `ADR-27` (`MediatR 12.5.0`).

## 2. Eventos de integración

- **`AporteRegistrado.v1`** (`Ascenso.Contracts.Eventos.AporteRegistradoV1`, versión `1`, aditivo en `EventosCatalogoFondosV1.cs`):
  - **Campos:** `(AporteId, DonanteId, NombreDonante, ProgramaId, CodigoPeriodo, Monto, Moneda, RegistradoEnUtc)`.
  - **Productor:** `S2` (escrito en `outbox_messages` en la misma transacción local que `Donante.RegistrarAporte`).
  - **Consumidores:** `S2` (`PresupuestoPrograma` en `RF-12`) y `S6` (proyección financiera y reportes de impacto a donantes en `RF-33`).

## 3. Contratos públicos de S2 (`contracts/openapi.yaml`)

- **Ruta existente honrada:**
  - `POST /api/v1/programas/{programaId}/aportes` (`RegistrarAporteCommand { donanteId, cantidad, moneda, codigoPeriodo }` → `OperacionConfirmada`).
- **Rutas complementarias de `RF-13` bajo tag `S2`:**
  - `GET /api/v1/donantes` (lista donantes registrados — Dirección, Coordinador, Finanzas).
  - `POST /api/v1/donantes` (registra donante organización/persona sin crear cuenta — Dirección, Coordinador).
  - `GET /api/v1/donantes/{donanteId}` (detalle del donante — Dirección, Coordinador, Finanzas).
  - `GET /api/v1/donantes/{donanteId}/aportes` (historial append-only con query opcional `codigoPeriodo` — Dirección, Coordinador, Finanzas; bloqueado a postulantes/becarios).
  - `POST /api/v1/donantes/{donanteId}/aportes` (registra aporte desde la vista del donante con `{ programaId, cantidad, moneda, codigoPeriodo }`).

## 4. Decisiones del Anexo B (`HU-13.1` y `HU-13.2`) — confirmadas (`C18`)

1. **Anexo B · HU-13.1 #1, #3 y HU-13.2 #2 (Permisos sobre donantes y aportes):** **Dirección y Coordinador** registran donantes y aportes (restringiendo a Coordinador a sus programas asignados si se informa alcance al registrar aportes); **Dirección, Coordinador y Finanzas** consultan donantes e historial financiero de aportes; **Evaluador, Enlace universitario, Becario y Postulante** bloqueados (`403`).
2. **Anexo B · HU-13.1 #2 (Unicidad del nombre del donante):** nombre único normalizado (insensible a mayúsculas/espacios, `409 Conflict` si se repite), coherente con universidades (`C16`) y programas (`C17`).
3. **Anexo B · HU-13.2 #1 (Aportes contra periodos anteriores/cerrados):** se permite registrar aportes contra **cualquier periodo registrado** en `CalendarioAcademico` (incluidos periodos anteriores cerrados, útil para regularización legal/contable en `HU-13.2`), rechazando solo periodos inexistentes (`404`).
4. **Contacto del donante (`HU-13.1 E1–E3`):** exige **al menos correo (validado) o teléfono** no vacío (permite solo correo como en E2 o ambos como en E1).
5. **Evento `AporteRegistradoV1` en `Ascenso.Contracts`:** registro estrictamente aditivo en `EventosCatalogoFondosV1.cs` para `RF-12` y `RF-33` (sin romper contratos existentes).
