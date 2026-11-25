# ASCENSO — Instrucciones del Proyecto (Gemini / Antigravity)

Proyecto semestral de **Desarrollo de Software** (Escuela de Ingeniería Informática, UCAB · Grupo 6, equipo de 5 integrantes).
ASCENSO es la plataforma de gestión integral de becas y ayudas estudiantiles de la Fundación Ascenso (postulación, evaluación por comité, otorgamiento, desembolsos, seguimiento académico, renovación, comunicaciones, reportería y auditoría).

> **Idioma obligatorio:** responde, documenta y escribe todos los artefactos en **español**. La única excepción son las palabras clave estructurales que el validador de OpenSpec requiere en inglés (`## Purpose`, `## ADDED Requirements`, `## MODIFIED Requirements`, `## REMOVED Requirements`, `### Requirement:`, `#### Scenario:`, `SHALL`/`MUST` y `**GIVEN**`/`**WHEN**`/`**THEN**`/`**AND**`).

---

## 1. Flujo de Trabajo con OpenSpec 2.0.0

OpenSpec es obligatorio en el proyecto (ADR-23). Antes de modificar código o documentos de diseño, el trabajo debe existir como un cambio en `openspec/changes/<nombre>/`.

### Skills de OpenSpec
Las skills locales se instalan en cada máquina con `bash tools/openspec/configurar.sh` (`.agents/skills/openspec-*/SKILL.md`, ignoradas en Git). Cuando el trabajo corresponda a una fase de OpenSpec, sigue la skill correspondiente junto con el CLI `openspec`:

- **Explorar requisitos o diseño:** `openspec-explore`
- **Proponer un cambio completo (proposal, specs, design, tasks):** `openspec-propose` (o paso a paso con `openspec-new-change`, `openspec-continue-change`, `openspec-ff-change`)
- **Revisar o actualizar artefactos de un cambio:** `openspec-update-change`
- **Implementar tareas:** `openspec-apply-change`
- **Verificar implementación contra specs:** `openspec-verify-change`
- **Sincronizar o archivar:** `openspec-sync-specs`, `openspec-archive-change`, `openspec-bulk-archive-change`

### Reglas operativas de OpenSpec
- **Frontera de planificación:** al crear o proponer un cambio, redacta únicamente los artefactos de planificación en `openspec/changes/<nombre>/` y detente para revisión. No implementes código hasta que el usuario dé luz verde para aplicar el cambio.
- **Configuración y guía del equipo:** respeta siempre `openspec/config.yaml` (esquema `spec-driven`, contexto y reglas por artefacto) y `docs/guia-openspec.md`.
- **Nombres de cambios:** verbo en infinitivo + objeto, en español y `kebab-case` (por ejemplo: `emitir-oferta-con-reserva`, `registrar-postulante`). La rama de Git lleva el mismo nombre con su prefijo (`feat/`, `fix/`, `docs/`, `test/`, `chore/`).
- **`openspec/specs/` no se edita a mano:** las especificaciones vigentes nacen y se actualizan únicamente al archivar cambios con `openspec archive <nombre>`. La única excepción es la sección `## Purpose`.
- **Validación obligatoria antes de dar algo por terminado:**
  ```bash
  openspec validate --all --strict
  ```

---

## 2. Precedencia de Fuentes (y Regla de No Invención)

Si dos documentos difieren, manda el de mayor precedencia en este orden:

1. **Historias de usuario Plan B (73 HU, 714 escenarios en Gherkin)** con decisiones del cliente **C1–C5**, **R1–R44** y supuestos **S1–S12**:
   - `docs/requisitos/historias-de-usuario/` (`anexo-a-reglas-tecnicas.md`, `anexo-b-preguntas-abiertas.md`)
   - `docs/requisitos/respuestas_cliente.md`
   - **Excepción acordada por el equipo:** en **R39, R40 y R44** manda la arquitectura v1.3 (postulación rechazada si no se puede comprobar el doble beneficio, código de estudiante traducido por S5, consulta sin postulación obligatoria).
2. **Modelo de dominio (DDD):**
   - `docs/dominio/DDD_v3.3.md` (vigente: incorpora adenda S2/S4, DDD-07 a DDD-15, Ronda 5 R31–R44 y reparto oficial §2.2). `DDD_v3.2.md` se conserva como histórico.
3. **Lámina «Decisiones Arquitectónicas» del profesor (tecnologías obligatorias):**
   - `docs/arquitectura/v1.3/06_FUENTES/DECISIONES_ARQUITECTONICAS_LAMINA_OBLIGATORIA_REFERENCIA.md`
4. **Arquitectura v1.3 y ADRs correctivos (documentos rectores vigentes):**
   - `docs/arquitectura/v1.3/01_DOCUMENTO_MAESTRO/ARQUITECTURA_V1.0.md` (ADR-01 a ADR-24 en §13)
   - `docs/arquitectura/ADR_CORRECTIVOS_V1.3.md` (ADR-25 a ADR-30: versión explícita, conciliación S2→S4, MediatR 12.5.0 fijado, Next.js BFF, catálogo `EstadoMatricula`, registro S1→S3)
   - `docs/arquitectura/v1.3/01_DOCUMENTO_MAESTRO/ANEXOS_TECNICOS.md`
   - Matrices de trazabilidad (dueño de cada RF, HU, agregado y evento): `docs/arquitectura/v1.3/04_MATRICES/`
   - Pruebas exigidas y definición de terminado: `docs/arquitectura/v1.3/05_CALIDAD/PRUEBAS_Y_CRITERIOS.md`
5. **Contratos de API y eventos (única fuente activa):**
   - `contracts/` (`openapi.yaml`, `openapi-interno.yaml` v1.3, `EVENTOS_E_INTERFACES.md`, `MATRIZ_PUERTOS.md`, `SEGURIDAD_INTERSERVICIOS_V1.1.md`).
6. **Pendientes y decisiones cerradas del equipo:**
   - `docs/arquitectura/pendientes-v1.3.md` (incluye decisiones `C1–C13`, pendientes B5 cliente, B7–B10 profesor, C8/C9, D3–D5).
- El PRD original (`docs/requisitos/prd.md`) solo se consulta cuando ninguna fuente anterior define el tema.

**Regla estricta:** si algo no está definido o aparece como «sin definir» en las HU, **pregunta al usuario; no inventes reglas ni valores**. Copia tal cual los valores fijados por el cliente (montos, plazos, intentos, horarios). Lo pendiente del Anexo B no se especifica en `specs/`: se registra en `design.md` como pregunta abierta con su número.

---

## 3. Arquitectura Vigente (v1.3) e Invariantes Técnicas

### Stack y decisiones base
- **Backend:** 6 microservicios **.NET 10 (C#)** con **Clean Architecture + DDD** detrás de un API Gateway **YARP** (el gateway no contiene reglas de negocio). `MediatR` fijado en `12.5.0` (Apache 2.0, `ADR-27`; prohibido `>= 13.0.0`).
- **Frontend:** **React + Next.js App Router** (`frontend/`) como BFF ligero con cookies `HttpOnly/Secure/SameSite=Lax` (`ADR-28`; prohibido JWT en `localStorage`).
- **Persistencia transaccional (escritura):** **PostgreSQL + EF Core**, con **un schema y un usuario por servicio**. Ningún servicio lee ni escribe el schema de otro.
- **Proyecciones de lectura (CQRS, DDD D-14):** **MongoDB** para reportes y paneles, escrito únicamente por proyectores idempotentes en **S6**. **Ninguna regla de cupo, presupuesto o doble beneficio se decide leyendo MongoDB.**
- **Archivos binarios:** solo en **Firebase Storage** (ADR-06), guardando referencia interna y sirviendo enlaces firmados de corta vigencia.
- **Mensajería y eventos de integración:** **MassTransit 8 + RabbitMQ** con patrón **Outbox** en el productor e **Inbox** en el consumidor (entrega al menos una vez). Contratos en `Ascenso.Contracts`.
- **Identidad y seguridad:** **Keycloak** para identidad y verificación en dos pasos (ADR-09); **OTP** por operación sensible en S1 (ADR-10).
- **Procesos programados y tiempo real:** **Hangfire** para tareas programadas; **SignalR** en S6 para tiempo real.

### Reglas de diseño interno (obligatorias en todos los servicios)
- **Cuatro proyectos por servicio:** `Domain`, `Application`, `Infrastructure`, `Api`, más `tests/{Unit,Integration,Architecture}`.
- **Aislamiento del dominio:** `Domain` y `Ascenso.SharedKernel` no dependen ni conocen EF Core, MassTransit, ASP.NET Core ni Firebase.
- **Handlers delgados:** los handlers de `Application` no contienen reglas de negocio; únicamente cargan agregados, invocan el dominio, persisten y publican.
- **Frontera transaccional:** una transacción modifica un único agregado (única excepción: DDD D-3).
- **Trazabilidad inmutable (RF-35):** toda decisión genera un `RegistroDecision` inmutable (append-only) y su `OutboxMessage` (`DecisionRegistradaV1`) dentro de la misma transacción que el agregado (`ADR-12`, `D-6`, `UnidadAtomicaDecisiones` + `AppendOnlyRegistroDecisionInterceptor`).
- **Concurrencia optimista:** versión explícita `Version: int` en cada raíz de agregado (`ADR-16`, `ADR-25`, `DDD-07`).
- **Tiempo y fechas límite:** zona horaria **UTC-4**, incluyendo el día límite hasta las **23:59** (R20), siempre mediante `IRelojNegocio` inyectado (sin llamadas directas a `DateTime.Now`/`UtcNow` en dominio o aplicación).

### Lista cerrada de llamadas síncronas entre servicios (v1.3 §3.1, ADR-02, ADR-26)
Son rutas internas privadas protegidas con OAuth2 `client_credentials`, **nunca expuestas por YARP**. No se permiten otras llamadas síncronas entre microservicios:
1. `S4 → S2`: reservar (oferta duración completa R32 / renovación R31) y liberar presupuesto.
2. `S3 → S2`: consultar disponibilidad.
3. `S4 → S1`: validar y consumir OTP.
4. `S3 → S4`: consultar beneficios para prevención de doble beneficio (HU-01.2, fail closed).
5. `S2 → S4`: conciliación segura de vínculo de reserva antes de liberar (`GET /internal/v1/reservas/{reservaId}/vinculo`, 30 min gracia, fail closed, ADR-26).

### Servicios y agregados principales (ADR-02)
| Servicio | Carpeta | Contextos / Agregados principales |
|---|---|---|
| **S1** Identidad y Acceso | `services/Ascenso.S1.Identidad` | `Usuario`, `TipoUsuario`, `ClienteApiUniversidad`, OTP |
| **S2** Catálogo y Fondos | `services/Ascenso.S2.CatalogoFondos` | `CalendarioAcademico`, `UniversidadAliada`, `Programa`, `Donante`, `PresupuestoPrograma` |
| **S3** Postulación y Evaluación | `services/Ascenso.S3.PostulacionEvaluacion` | `CicloPostulacion`, `Postulante`, `Postulacion`, `Comite`, `Evaluacion`, `ConflictoInteres` |
| **S4** Gestión de Becas | `services/Ascenso.S4.Becas` | `Oferta`, `Beca`, `BeneficiosPersona`, `DatosPago`, `VentanaRenovacion` |
| **S5** Seguimiento Académico | `services/Ascenso.S5.SeguimientoAcademico` | `ReporteAcademico`, `PlazoReporte`, `EvaluacionCumplimiento` |
| **S6** Comunicaciones y Reportería | `services/Ascenso.S6.ComunicacionesReporteria` | `Consulta`, `RegistroEnvio`, `ConsentimientoDonante`, proyecciones de lectura (MongoDB) y hub SignalR |

### Estructura del repositorio (v1.3 §12)
```text
.
├── frontend/          React + Next.js (BFF ligero, ADR-28)
├── gateway/           API Gateway YARP (sin reglas de negocio)
├── services/          Ascenso.S1 … Ascenso.S6 (Api, Application, Domain, Infrastructure, tests)
├── building-blocks/   Ascenso.SharedKernel, Ascenso.Contracts, Ascenso.Messaging.Outbox y tests
├── contracts/         Única fuente activa de OpenAPI, eventos y permisos
├── deploy/            docker-compose, init-schemas.sql y configuración local sin secretos
├── openspec/          config.yaml, specs/ y changes/
├── docs/              requisitos/, dominio/, arquitectura/ y guías
├── tools/             Scripts de apoyo (OpenSpec, diagramas Mermaid)
└── .github/           CI (.NET 10), plantilla de PR y CODEOWNERS
```

---

## 4. Catálogo de Capacidades de OpenSpec (`openspec/specs/`)

Usa **siempre** estos nombres exactos. Si ninguno aplica, justifica uno nuevo en `proposal.md` y agrégalo a `openspec/config.yaml` en el mismo pull request.

| Capacidad | Servicio | Cobertura |
|---|---|---|
| `identidad-acceso` | S1 | Cuentas, tipos de usuario, permisos, segundo factor (RF-37) |
| `codigos-un-solo-uso` | S1 | OTP por operación sensible (R4–R6; HU-20.1, 20.3, 22.1, 25.2, 37.2) |
| `calendario-academico` | S2 | Periodos académicos y periodo actual (RF-38) |
| `universidades-convenios` | S2 | Universidades, convenios, carreras, costo de matrícula, inicio de inscripciones (RF-14) |
| `programas` | S2 | Configuración de programas y plantilla de rúbrica (RF-11, RF-07 parte) |
| `presupuesto-cupos` | S2 | Presupuesto disponible, reservas, cupo del programa y del ciclo (RF-12) |
| `donantes-aportes` | S2 | Donantes y aportes (RF-13) |
| `ciclos-postulacion` | S3 | Convocatorias, ventanas, criterios, documentos (RF-02) |
| `postulaciones` | S3 | Perfil, postulación, documentos, elegibilidad, retiro (RF-01, 03, 04, 05) |
| `comite-evaluacion` | S3 | Comités, asignación de evaluadores, conflictos de interés (RF-06, 39) |
| `evaluacion-puntuacion` | S3 | Rúbrica aplicada, puntuaciones y comentarios (RF-07, 08) |
| `seleccion-ranking` | S3 | Consolidación, ranking, casos límite, lista de espera (RF-09, 10) |
| `ofertas` | S4 | Emisión, aceptación, rechazo, expiración, no doble beneficio (RF-15, 16) |
| `becas` | S4 | Ciclo de vida, compromiso, suspensión, pausa, revocación, historiales (RF-17, 18, 21, 22, 27, 28) |
| `desembolsos-pagos` | S4 | Calendario de desembolsos, pagos, datos de pago (RF-19, 20) |
| `renovaciones` | S4 | Ventanas y solicitudes de renovación (RF-26) |
| `seguimiento-academico` | S5 | Reportes académicos, plazos, permanencia (RF-24, 25) |
| `notificaciones` | S6 | Notificaciones, recordatorios, avisos al comité (RF-29, 30, 31) |
| `consultas` | S6 | Mensajería con postulantes y becarios (RF-32) |
| `reportes` | S6 | Reporte financiero, de impacto, institucional, panel (RF-23, 33, 34, 40) |
| `registro-decisiones` | Transversal | Registro inmutable en cada servicio y exportación en S6 (RF-35, 36) |

---

## 5. Reglas por Artefacto de OpenSpec

- **`proposal.md`:**
  - En *Why*, citar las HU, RF, decisiones del cliente (`C`/`R`/`S`) y ADR que motivan el cambio.
  - Usar solo capacidades del catálogo. Un cambio debe caber en un solo PR revisable; dividirlo si mezcla HU grandes o servicios sin necesidad.
  - En *Impact*, listar los servicios (`S1–S6`) afectados e indicar explícitamente si toca `Ascenso.Contracts` o `Ascenso.SharedKernel` (ambos requieren acuerdo de todo el equipo).
  - Cambios exclusivos de documentación, herramientas o configuración no declaran capacidades y llevan `skip_specs: true` en `.openspec.yaml`.
- **`specs/<capacidad>/spec.md`:**
  - Derivar requisitos y escenarios de las HU; citar cada HU entre paréntesis, p. ej. `(HU-15.1)`.
  - Traducir `DADO QUE / CUANDO / ENTONCES / Y` a `**GIVEN** / **WHEN** / **THEN** / **AND**`, manteniendo el resto en español.
  - Formato exigido por el validador: escenarios con exactamente cuatro `#` (`#### Scenario:`), al menos un escenario por requisito, cada escenario con `**WHEN**` (o `**GIVEN**`) y `**THEN**`, y descripción del requisito de máximo 500 caracteres (un comportamiento por requisito).
  - **Sin tecnología:** no mencionar clases, tablas, endpoints ni librerías (eso va en `design.md`).
- **`design.md`:**
  - Nombrar los agregados, eventos y servicios de dominio del DDD que intervienen y los ADR aplicables.
  - Cumplir todas las invariantes de la §3. Si el diseño requiere contradecir un ADR o el DDD, declararlo explícitamente y modificar ese documento en el mismo PR.
  - Documentar eventos de integración nuevos o modificados con productor, consumidores y versión.
  - Si agrega o modifica una ruta de API o evento de integración, actualizar el contrato (`openapi.yaml`, `openapi-interno.yaml` o `EVENTOS_E_INTERFACES.md`) y su catálogo CSV en el mismo PR.
- **`tasks.md`:**
  - Agrupar por capa en orden estricto: **Domain → Application → Infrastructure → Api**, incluyendo pruebas en cada grupo.
  - Cada regla de negocio lleva una prueba unitaria en `Domain`; cada escenario de la spec lleva al menos una prueba (unitaria o de integración) que lo nombre.
  - Incluir migración de EF Core cuando cambie el modelo persistido y actualización de versión del contrato cuando cambie un evento de integración.
  - Cada tarea debe indicar cómo se verifica (prueba, comando o resultado observable) y caber en una sesión de trabajo.
- **Implementación (`apply`) y cierre (`archive`):**
  - Implementar las tareas en orden y marcar `- [x]` al terminar cada una.
  - No modificar otros servicios ni el `SharedKernel` sin una tarea que lo solicite.
  - Ejecutar las pruebas del servicio antes de dar por completado un grupo.
  - Archivar (`openspec archive <nombre>`) dentro del mismo PR antes de fusionar, únicamente cuando todas las tareas estén completas, las pruebas pasen y `openspec validate --all --strict` no reporte errores.

---

## 6. Reglas de Git, Documentación y Seguridad

- **Ramas:** nunca trabajar ni commitear directamente en `main`. Crear siempre una rama con prefijo `feat/`, `fix/`, `docs/`, `test/` o `chore/` y el nombre del cambio de OpenSpec (ej. `feat/emitir-oferta-con-reserva`).
- **Commits:** mensajes cortos, en imperativo y en español, referenciando la HU, el ADR o el cambio (ej. `feat(s4): emitir oferta con reserva síncrona en S2 (HU-15.1, ADR-03)`).
- **Sin atribución de herramientas:** no agregar líneas `Co-Authored-By` ni menciones de asistentes o herramientas de IA en mensajes de commit ni en pull requests.
- **Push restringido:** no ejecutar `git push` sin que el usuario lo pida explícitamente.
- **Documentación y diagramas:** los `.md` son la fuente de verdad (los PDF son respaldo). Los ADR aprobados no se editan en silencio; cualquier cambio entra por PR justificado y cambio de OpenSpec. Si se edita un diagrama en `docs/dominio/DDD_vX.md`, regenerar `docs/dominio/diagramas/`.
- **Secretos:** jamás leer para exponer ni subir credenciales, contraseñas, tokens o claves de Firebase/Keycloak. Los valores locales van en `.env` o `appsettings.*.local.json` (ignorados por `.gitignore`); para documentar variables nuevas usar únicamente `.env.example` sin valores reales.

---

## 7. Diseño Frontend Obligatorio (Figma)

- **Fuente visual obligatoria para todas las pantallas de `frontend/`:** sistema de diseño del equipo en Figma (`ASCENSO — Diseño mejorado`, copia de referencia: `https://www.figma.com/design/GPVXLD4GZ2Il17GTXoPonY/ASCENSO---Diseño-mejorado--Copy-`).
- **Estructura del archivo Figma:**
  - `01 · Fundamentos`: paleta `azul → turquesa → verde` (*el color sube con el avance*), `gris` (`50–900`), ámbar/rojo solo alertas o acciones irreversibles; *una superficie por idea*; *un botón primario por vista*; tipografía **Bricolage Grotesque** (títulos y cifras/KPIs) + **Inter** (cuerpo, campos y tablas).
  - `02 · Componentes`: botones, campos, badges `Estado`, stepper `Paso`, casillas OTP, avisos y filas de tabla reutilizables.
  - `03 · Pantallas`: 11 pantallas base. Al implementar cualquier vista en `frontend/`, usar los tokens y componentes del sistema de diseño de Figma (y construir las vistas sin frame propio respetando exactamente esos mismos tokens).
- **Regla de precedencia visual:** las HU, Ronda 5 y `DDD_v3.3.md` mandan sobre el mockup (por ejemplo, en *Becas Ingeniería* la reserva de oferta cubre toda la duración de la beca según R32/A2; en `frontend/` se aplica `ADR-28` Next.js App Router como BFF con cookies `HttpOnly`).
