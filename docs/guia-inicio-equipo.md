# Guía de inicio para el equipo

Qué hace cada integrante **antes de escribir código**. Son unos 30 minutos de preparación y una tarea de equipo.

## Parte 1 · Preparar tu máquina (cada uno)

### 1. Acceso al repositorio

Acepta la invitación de GitHub (llega al correo de tu cuenta) y clona el repositorio desde Git Bash:

```bash
cd ~/Desktop
git clone https://github.com/ArcaniusBrain/Ascenso_Grupo6.git
cd Ascenso_Grupo6
```

### 2. Tu identidad en git (importante para la nota)

La nota individual sale del historial de commits. GitHub solo te cuenta un commit si el correo coincide con uno de tu cuenta de GitHub:

```bash
git config --global user.name "Tu Nombre"
git config --global user.email "el-correo-de-tu-cuenta-de-github"
```

### 3. Herramientas

| Herramienta | Versión | Cómo comprobar | Para qué |
|---|---|---|---|
| Git (con Git Bash) | cualquiera reciente | `git --version` | Todo |
| Node.js | **24 LTS o superior** (sirve 22.22+) | `node -v` | OpenSpec |
| Python | 3.10 o superior | `python --version` | Regenerar diagramas del DDD |
| .NET SDK | **10** | `dotnet --version` | Etapa de código |
| Docker Desktop | reciente | `docker --version` | Etapa de código (docker-compose) |

Si ya tienes Node pero es más viejo: con nvm-windows, en un CMD como administrador, `nvm install lts` y `nvm use <versión>`; sin nvm, `winget install OpenJS.NodeJS.LTS`. Cierra y abre la terminal después.

.NET 10 y Docker no hacen falta hoy, pero conviene tenerlos instalados antes de que empiece el código.

### 4. OpenSpec

Desde la raíz del repositorio, en Git Bash:

```bash
bash tools/openspec/configurar.sh            # si usas Claude Code
bash tools/openspec/configurar.sh cursor     # si usas otro asistente (lista: openspec init --help)
bash tools/openspec/configurar.sh none       # sin asistente
```

Debe terminar con `Totals: 1 passed, 0 failed`. Opcional, recomendado:

```bash
openspec config profile                      # elegir "Workflows only" y marcar todos con la barra espaciadora
openspec update
openspec config set telemetry.enabled false  # no enviar estadísticas de uso
```

### 5. Lectura (20 minutos)

1. [`README.md`](../README.md): qué hay en el repositorio.
2. [`CONTRIBUTING.md`](../CONTRIBUTING.md): ramas, commits y pull requests.
3. [`docs/guia-openspec.md`](guia-openspec.md): cómo se trabaja un cambio y el catálogo de capacidades.

## Parte 2 · La tarea de equipo antes del código

El modelo de dominio (DDD v3.2) y la arquitectura (ADR v0.1) tienen errores que hay que corregir **antes** de programar, porque el código se escribe a partir de ellos. Esa corrección es el cambio de OpenSpec [`actualizar-ddd-v3-3-y-adr-v0-2`](../openspec/changes/actualizar-ddd-v3-3-y-adr-v0-2/proposal.md): 20 tareas, cada una explicada en [`docs/cambios/cambios_DDD_v3.3_y_ADR_v0.2.md`](cambios/cambios_DDD_v3.3_y_ADR_v0.2.md).

### Reparto sugerido

Todo va en **una sola rama** y **un solo pull request**. Para no pisarse, cada grupo trabaja archivos distintos:

| Quién | Tareas (`tasks.md`) | Archivos |
|---|---|---|
| 2 personas | 1.1 a 1.10 · DDD v3.3 | `docs/dominio/` (una sola persona edita el DDD a la vez: se turnan por tarea) |
| 2 personas | 2.1 a 2.7 · ADR v0.2 | `docs/arquitectura/` (cada una toma ADR distintos) |
| 1 persona | 3.1 a 3.3 · referencias y validación final, más revisar lo de los demás | `docs/requisitos/`, `README.md`, `openspec/config.yaml` |

Empiecen por las tareas **bloqueantes**: 1.2 (DDD-07), 2.1 (ADR-B01) y 2.2 (ADR-B02).

### Pasos

```bash
git switch main && git pull
git switch docs/actualizar-ddd-v3-3-y-adr-v0-2 2>/dev/null || git switch -c docs/actualizar-ddd-v3-3-y-adr-v0-2
```

(La primera persona crea la rama y la sube con `git push -u origin docs/actualizar-ddd-v3-3-y-adr-v0-2`; los demás solo la traen con `git pull`.)

Por cada tarea:

1. Lee su ID en la lista de cambios y aplícalo.
2. Marca la tarea en `openspec/changes/actualizar-ddd-v3-3-y-adr-v0-2/tasks.md` cambiando `- [ ]` por `- [x]`.
3. Un commit por tarea, citando el ID:
   ```bash
   git add -A
   git commit -m "docs(ddd): DDD-07 versión explícita en la raíz"
   git pull --rebase
   git push
   ```

Para regenerar los diagramas después de editar el DDD (tarea 1.10):

```bash
python tools/mermaid/generar_diagramas.py docs/dominio/DDD_v3.3.md
```

Al terminar todas las tareas:

```bash
openspec validate --all --strict
openspec archive actualizar-ddd-v3-3-y-adr-v0-2
git add -A && git commit -m "docs: archivar actualizar-ddd-v3-3-y-adr-v0-2" && git push
```

Luego se abre el pull request contra `main`, lo revisa al menos otra persona y se fusiona con el check **OpenSpec** en verde.

### En paralelo: preguntas pendientes

Una persona se encarga de enviar y registrar las respuestas de:

- Las 6 preguntas al profesor de [`docs/arquitectura/pendientes-y-preguntas.md`](arquitectura/pendientes-y-preguntas.md). Sobre todo la 1 (seis servicios) y la 2 (base de lectura PostgreSQL): si dice que no, cambia la forma del código.
- Las 2 preguntas nuevas al cliente (disparador de *Inactiva*; si R12 aplica a la pausa).

## Parte 3 · Después: empieza el código

Cuando el cambio esté archivado y el profesor haya respondido, el primer cambio de código es la **solución base** (proyectos .NET, gateway, `docker-compose`, integración continua de pruebas). A partir de ahí cada uno trabaja sus capacidades con el flujo de [`guia-openspec.md`](guia-openspec.md): rama, `/openspec-propose`, revisión de la propuesta, implementación, archivo.

## Problemas frecuentes

| Síntoma | Solución |
|---|---|
| Avisos `EBADENGINE` al instalar OpenSpec | Node muy viejo: actualiza a 24 LTS o superior y vuelve a ejecutar el script |
| `openspec: command not found` | Cierra y abre Git Bash; si sigue, vuelve a ejecutar `bash tools/openspec/configurar.sh` |
| `git push` rechazado ("fetch first") | `git pull --rebase` y vuelve a hacer `git push` |
| Mis commits no aparecen en mi perfil de GitHub | El correo de `git config user.email` no es el de tu cuenta: corrígelo (paso 2) |
| Avisos de "LF will be replaced by CRLF" | Normales en Windows; el repositorio ya normaliza los finales de línea |
