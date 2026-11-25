# Design — `configurar-controles-de-revision`

## 1. Enfoque técnico

El workflow `.github/workflows/pr-guardrails.yml` se ejecuta únicamente sobre eventos `pull_request` (`opened`, `edited`, `synchronize`, `reopened`, `labeled`, `unlabeled`) contra `main`, con permisos mínimos (`contents: read`, `pull-requests: read`), pasando datos del PR (`body`, `title`, `labels`) vía variables de entorno para evitar inyección de shell.

## 2. Controles implementados en el job `Guardrails de PR`

1. **Higiene de archivos y secretos:**
   - Revisa todos los archivos añadidos o modificados en el diff `base..head`.
   - Bloquea `.env` reales (excepto `.env.example`), `appsettings.*.local.json`, `secrets.json`, `*.pfx`, `*.p12`, `*.key`, credenciales de Firebase, directorios generados (`bin/`, `obj/`, `node_modules/`, `.next/`, `TestResults/`) y carpetas/archivos locales de asistentes (`CLAUDE.local.md`, `GEMINI.local.md`, `.agents/`, `.claude/`, `.gemini/`, `.cursor/`, `.codex/`).
   - Escanea líneas añadidas (`+`) en busca de bloques `BEGIN ... PRIVATE KEY` o credenciales hardcodeadas evidentes.
2. **OpenSpec obligatorio (ADR-23):**
   - Si el PR toca `services/`, `building-blocks/`, `contracts/`, `gateway/` o `frontend/`, exige al menos un archivo modificado/añadido bajo `openspec/`.
   - Permite escape controlado solo si un mantenedor añade explícitamente la etiqueta `sin-openspec` al PR (ej. hotfix trivial de emergencia).
3. **Resguardo de licencia `MediatR` (`ADR-27`):**
   - Si el PR modifica `Directory.Packages.props`, comprueba que la versión fijada de `MediatR` siga por debajo de `13.0.0`.
4. **Alerta de archivos críticos compartidos:**
   - Si el PR toca `building-blocks/`, `contracts/`, `Directory.Packages.props`, `Directory.Build.props`, `Ascenso.sln`, `deploy/postgres/init-schemas.sql` o `.github/`, escribe un aviso destacado en `$GITHUB_STEP_SUMMARY` listando los archivos sensibles para enfocar la revisión del equipo.
5. **Descripción y trazabilidad mínima del PR:**
   - Valida que el cuerpo del PR (eliminando comentarios HTML de la plantilla) tenga contenido real y cite al menos una referencia (`HU-`, `RF-`, `ADR-`, `DDD`, `R1`–`R44`, `C1`–`C13`, `openspec/` o `#<numero>`).
