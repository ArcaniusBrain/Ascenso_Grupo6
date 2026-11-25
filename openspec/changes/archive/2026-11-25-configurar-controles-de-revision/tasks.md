# Tasks — `configurar-controles-de-revision`

## 1. CI y Gobernanza (`.github/`)

- [x] **1.1** Crear `.github/workflows/pr-guardrails.yml` con el job `Guardrails de PR` cubriendo secretos/archivos prohibidos, OpenSpec obligatorio (ADR-23), resguardo `MediatR < 13.0.0` (ADR-27), alerta de archivos críticos y trazabilidad en el cuerpo del PR.
  - *Verificación:* revisión de sintaxis YAML y reglas regex ejecutadas localmente.
- [x] **1.2** Actualizar `.github/pull_request_template.md` incorporando lista de verificación de `ADR-25`, `ADR-28` / Figma, archivos críticos compartidos y sección breve para el revisor.
  - *Verificación:* plantilla legible y alineada con `CONTRIBUTING.md`, `CLAUDE.md` y `GEMINI.md`.
- [x] **1.3** Validar OpenSpec (`openspec validate --all --strict --no-interactive`), archivar el cambio y confirmar historial limpio en `chore/configurar-controles-de-revision`.
  - *Verificación:* `openspec validate --all --strict` y `openspec validate --archived` sin errores.
