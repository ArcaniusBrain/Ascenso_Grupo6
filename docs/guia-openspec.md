# Guía de OpenSpec del equipo

OpenSpec es obligatorio en el proyecto (ADR-23): antes de escribir código se acuerda **qué** debe hacer el sistema en una propuesta revisable, y al terminar esa propuesta queda archivada con fecha. Esta guía dice cómo lo usamos en ASCENSO.

- Versión acordada: **OpenSpec 2.0.0** (fijada en `tools/openspec/configurar.sh` y en la integración continua).
- Requisito: **Node.js 22.12 o superior**.

## 1. Configuración (una vez por integrante)

Desde Git Bash, en la raíz del repositorio:

```bash
bash tools/openspec/configurar.sh            # si usas Claude Code
bash tools/openspec/configurar.sh cursor     # si usas otro asistente (lista: openspec init --help)
bash tools/openspec/configurar.sh none       # solo la línea de comandos
```

El script instala la versión acordada, ejecuta `openspec init` (sin tocar `openspec/config.yaml`, que es del equipo) y valida el repositorio. Las habilidades que instala para tu asistente (`.agents/`, `.claude/`, etc.) son locales y están en `.gitignore`.

**Privacidad:** OpenSpec envía estadísticas de uso anónimas (solo el nombre del comando y la versión). Para desactivarlas en tu máquina:

```bash
openspec config set telemetry.enabled false
```

## 2. Qué hay en el repositorio

```
openspec/
├── config.yaml          Contexto del proyecto y reglas para cada artefacto (lo leen todos los asistentes)
├── specs/               Especificación vigente, una carpeta por capacidad: <capacidad>/spec.md
└── changes/
    ├── <cambio>/        Cambios en curso: proposal.md, specs/, design.md, tasks.md
    └── archive/         Cambios terminados, con fecha: AAAA-MM-DD-<cambio>/
```

- **`specs/`** es la verdad de lo que el sistema hace. Empieza vacía y crece al archivar cambios. No se edita a mano (salvo `## Purpose`).
- **Un cambio** describe una diferencia: qué capacidades agrega o modifica (deltas `ADDED` / `MODIFIED` / `REMOVED`), cómo se construye y qué tareas faltan.

## 3. Flujo de un cambio

Un cambio = una rama = un pull request. El nombre del cambio va en infinitivo, en español y en kebab-case; la rama lleva el mismo nombre con prefijo (`feat/`, `fix/`, `docs/`).

| Paso | Con asistente | Solo con la terminal |
|---|---|---|
| 1. Rama | `git switch -c feat/emitir-oferta-con-reserva` | igual |
| 2. Explorar (opcional) | `/openspec-explore` | leer las HU y el DDD |
| 3. Propuesta | `/openspec-propose emitir-oferta-con-reserva` | `openspec new change emitir-oferta-con-reserva` y escribir cada artefacto con la guía de `openspec instructions <artefacto> --change emitir-oferta-con-reserva` |
| 4. Validar | — | `openspec validate emitir-oferta-con-reserva --strict` |
| 5. Revisión de la propuesta | Pull request **en borrador** solo con `openspec/changes/...`; el equipo aprueba el *qué* | igual |
| 6. Implementar | `/openspec-apply-change` | trabajar las tareas de `tasks.md` y marcarlas `- [x]` |
| 7. Archivar | `/openspec-archive-change` | `openspec archive emitir-oferta-con-reserva` |
| 8. Fusionar | Pull request listo para revisión, con la integración continua en verde | igual |

Antes de abrir o actualizar el pull request: `openspec validate --all --strict`. La integración continua ejecuta lo mismo y además comprueba que los cambios archivados tengan todas sus tareas completas.

Comandos de consulta: `openspec list` (cambios en curso), `openspec list --specs` (capacidades), `openspec show <nombre>`, `openspec status --change <nombre>`, `openspec view` (panel).

## 4. Reglas del equipo

Las reglas completas están en `openspec/config.yaml` (los asistentes las leen solas). Lo esencial:

1. **Las HU mandan.** Los requisitos y escenarios salen de las reglas y criterios de aceptación de las HU, y cada requisito cita su HU: `(HU-15.1)`. No se inventan valores que las HU dejan "sin definir"; lo pendiente del Anexo B va a `design.md` como pregunta abierta.
2. **Idioma:** todo en español, salvo los encabezados de OpenSpec (`### Requirement:`, `#### Scenario:`…), `SHALL`/`MUST` y `**GIVEN**`/`**WHEN**`/`**THEN**`/`**AND**`, que el validador necesita en inglés.
3. **Capacidades del catálogo.** Usa solo estos nombres; una nueva se justifica en la propuesta y se agrega al catálogo de `config.yaml` en el mismo pull request.
4. **Specs sin tecnología.** Nada de clases, tablas, endpoints ni librerías en `specs/`: eso va en `design.md`.
5. **Documentación o herramientas sin cambio de comportamiento:** el cambio no declara capacidades y su `.openspec.yaml` lleva `skip_specs: true` (como `actualizar-ddd-v3-3-y-adr-v0-2`).
6. **Shared Kernel y contratos de eventos:** cualquier cambio que los toque necesita la revisión de todo el equipo.

### Catálogo de capacidades

| Capacidad | Servicio | Cubre |
|---|---|---|
| `identidad-acceso` | S1 | Cuentas, tipos de usuario, permisos, segundo factor (RF-37) |
| `codigos-un-solo-uso` | S1 | OTP por operación sensible (R4–R6) |
| `calendario-academico` | S2 | Periodos y periodo actual (RF-38) |
| `universidades-convenios` | S2 | Universidades, convenios, carreras, costo, inicio de inscripciones (RF-14) |
| `programas` | S2 | Configuración de programas y plantilla de rúbrica (RF-11) |
| `presupuesto-cupos` | S2 | Presupuesto, reservas, cupos (RF-12) |
| `donantes-aportes` | S2 | Donantes y aportes (RF-13) |
| `ciclos-postulacion` | S3 | Convocatorias, ventanas, criterios, documentos (RF-02) |
| `postulaciones` | S3 | Perfil, postulación, documentos, elegibilidad, retiro (RF-01, 03, 04, 05) |
| `comite-evaluacion` | S3 | Comités, asignaciones, conflictos de interés (RF-06, 39) |
| `evaluacion-puntuacion` | S3 | Rúbrica aplicada y puntuaciones (RF-07, 08) |
| `seleccion-ranking` | S3 | Consolidación, ranking, casos límite, lista de espera (RF-09, 10) |
| `ofertas` | S4 | Emisión, aceptación, rechazo, expiración, no doble beneficio (RF-15, 16) |
| `becas` | S4 | Ciclo de vida, compromiso, suspensión, pausa, revocación, historiales (RF-17, 18, 21, 22, 27, 28) |
| `desembolsos-pagos` | S4 | Calendario, pagos, datos de pago (RF-19, 20) |
| `renovaciones` | S4 | Ventanas y solicitudes de renovación (RF-26) |
| `seguimiento-academico` | S5 | Reportes académicos, plazos, permanencia (RF-24, 25) |
| `notificaciones` | S6 | Notificaciones, recordatorios, avisos al comité (RF-29, 30, 31) |
| `consultas` | S6 | Mensajería con postulantes y becarios (RF-32) |
| `reportes` | S6 | Reporte financiero, de impacto, institucional, panel (RF-23, 33, 34, 40) |
| `registro-decisiones` | transversal | Registro inmutable y exportación (RF-35, 36) |

## 5. Ejemplo de spec

Así se ve un requisito en `openspec/changes/<cambio>/specs/codigos-un-solo-uso/spec.md` (la sección `## Purpose` solo va cuando la capacidad es nueva):

```markdown
## Purpose

Exigir un código de un solo uso antes de cualquier operación que mueva dinero o cambie el estado legal de una beca.

## ADDED Requirements

### Requirement: Vigencia del código
El sistema SHALL rechazar un código de un solo uso usado más de 15 minutos después de emitido (R4, HU-37.2).

#### Scenario: Código vencido
- **GIVEN** un código emitido a las 10:00
- **WHEN** el usuario lo ingresa a las 10:16
- **THEN** el sistema rechaza la operación e indica que el código venció
- **AND** la operación no cambia nada
```

Reglas de formato que el validador exige: escenarios con exactamente cuatro `#`, al menos un escenario por requisito, cada escenario con **WHEN** (o **GIVEN**) y **THEN**, y la descripción de cada requisito en 500 caracteres o menos (un comportamiento por requisito).

## 6. Actualizar OpenSpec

Todo el equipo usa la misma versión. Para cambiarla: un pull request que modifique `VERSION_OPENSPEC` en `tools/openspec/configurar.sh` y `OPENSPEC_VERSION` en `.github/workflows/openspec.yml`; después cada integrante vuelve a ejecutar el script.
