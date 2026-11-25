# ASCENSO — Instrucciones del Proyecto (Claude Code)

Proyecto semestral de **Desarrollo de Software** (Escuela de Ingeniería Informática, UCAB · Grupo 6, equipo de 5 integrantes).
ASCENSO es la plataforma de gestión integral de becas y ayudas estudiantiles de la Fundación Ascenso (postulación, evaluación por comité, otorgamiento, desembolsos, seguimiento académico, renovación, comunicaciones, reportería y auditoría).

> **Idioma obligatorio:** responde, documenta y escribe todos los artefactos en **español**. La única excepción son las palabras clave estructurales que el validador de OpenSpec requiere en inglés (`## Purpose`, `## ADDED Requirements`, `## MODIFIED Requirements`, `## REMOVED Requirements`, `### Requirement:`, `#### Scenario:`, `SHALL`/`MUST` y `**GIVEN**`/`**WHEN**`/`**THEN**`/`**AND**`).

---

## 1. Flujo de Trabajo con OpenSpec 2.0.0

OpenSpec es obligatorio en el proyecto (ADR-23). Antes de modificar código o documentos de diseño, el trabajo debe existir como un cambio en `openspec/changes/<nombre>/`.

### Skills de OpenSpec en Claude Code
Las skills locales se instalan en cada máquina con `bash tools/openspec/configurar.sh` (`.claude/`, ignorado en Git). Usa los flujos `/openspec-explore`, `/openspec-propose`, `/openspec-apply-change`, `/openspec-verify-change` y `/openspec-archive-change` junto con el CLI `openspec`.

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

---

## 4. Catálogo de Capacidades de OpenSpec (`openspec/specs/`)

Usar **siempre** los nombres del catálogo definido en `openspec/config.yaml`: `identidad-acceso`, `codigos-un-solo-uso`, `calendario-academico`, `universidades-convenios`, `programas`, `presupuesto-cupos`, `donantes-aportes`, `ciclos-postulacion`, `postulaciones`, `comite-evaluacion`, `evaluacion-puntuacion`, `seleccion-ranking`, `ofertas`, `becas`, `desembolsos-pagos`, `renovaciones`, `seguimiento-academico`, `notificaciones`, `consultas`, `reportes`, `registro-decisiones`.

---

## 5. Reglas de Git, Documentación, Seguridad y Diseño

- **Ramas:** nunca trabajar ni commitear directamente en `main`. Crear siempre una rama con prefijo `feat/`, `fix/`, `docs/`, `test/` o `chore/` y el nombre del cambio de OpenSpec.
- **Commits:** mensajes cortos, en imperativo y en español, referenciando la HU, el ADR o el cambio.
- **Sin atribución de herramientas:** no agregar líneas `Co-Authored-By` ni menciones de asistentes o herramientas de IA en mensajes de commit ni en pull requests.
- **Push restringido:** no ejecutar `git push` sin que el usuario lo pida explícitamente.
- **Secretos:** jamás subir credenciales, contraseñas, tokens o claves; usar `.env` o `appsettings.*.local.json` (ignorados por `.gitignore`) y documentar variables nuevas solo en `.env.example`.
- **Diseño Frontend obligatorio (Figma):** todas las vistas de `frontend/` siguen el sistema de diseño en Figma (`ASCENSO — Diseño mejorado`, paleta `azul → turquesa → verde` + `gris 50–900`, tipografía **Bricolage Grotesque** + **Inter**, *una superficie por idea*, *un botón primario por vista*). Las HU, Ronda 5 y `DDD_v3.3.md` siempre mandan sobre el mockup.
