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
4. [`docs/arquitectura/README.md`](arquitectura/README.md): la arquitectura v1.3 y por dónde empezar a leerla.

## Parte 2 · Lo que falta antes del código

La arquitectura está cerrada como diseño: **v1.3** en [`docs/arquitectura/`](arquitectura/README.md). Léanla antes de programar, empezando por `v1.3/ABRIR_PRIMERO.html` y la §17 (orden de construcción) del documento rector. Quedan tres frentes, todos listados en [`docs/arquitectura/pendientes-v1.3.md`](arquitectura/pendientes-v1.3.md):

### 2.1 Respuestas del cliente (hecho)

Las respuestas de la última reunión ya están como **Ronda 5** (R31–R44) en [`docs/requisitos/respuestas_cliente.md`](requisitos/respuestas_cliente.md), y `pendientes-v1.3.md` dice qué cambia cada una. Léanlas antes de corregir el dominio: varias reglas cambian (reservas, revocación, rechazo, consentimiento, matrícula). Solo queda por confirmar el nombre exacto del rol que aprueba la migración (B5).

### 2.2 Corregir el dominio: cambio `corregir-ddd-v3-3`

El DDD v3.2 sigue siendo la fuente, pero tiene puntos que la v1.3 no resuelve o en los que se contradicen (versión de la raíz, disparador de *Inactiva*, desembolso aplazado, OTP, registro de cuenta, datos de pago). Las tareas están en [`openspec/changes/corregir-ddd-v3-3/tasks.md`](../openspec/changes/corregir-ddd-v3-3/tasks.md).

Todo va en **una sola rama** y **un solo pull request**. El DDD es un solo archivo, así que **una persona edita a la vez**: se turnan por tarea, siempre con `git pull --rebase` antes de empezar.

```bash
git switch main && git pull
git switch docs/corregir-ddd-v3-3 2>/dev/null || git switch -c docs/corregir-ddd-v3-3
```

(La primera persona crea la rama y la sube con `git push -u origin docs/corregir-ddd-v3-3`; los demás la traen con `git pull`.)

Por cada tarea: aplicar el cambio, marcarla en `tasks.md` (`- [ ]` → `- [x]`) y hacer un commit que cite el ID:

```bash
git add -A
git commit -m "docs(ddd): DDD-07 versión explícita en la raíz"
git pull --rebase
git push
```

Para regenerar los diagramas (tarea 3.1):

```bash
python tools/mermaid/generar_diagramas.py docs/dominio/DDD_v3.3.md
```

Al terminar:

```bash
openspec validate --all --strict
openspec archive corregir-ddd-v3-3 -y
git add -A && git commit -m "docs: archivar corregir-ddd-v3-3" && git push
```

Luego el pull request contra `main`, revisado por todo el equipo (toca el Shared Kernel en diseño).

### 2.3 Decisiones y pruebas técnicas

En paralelo, el equipo cierra las decisiones de la sección C de `pendientes-v1.3.md` (dueño de RF-18, MediatR, ADR de frontend, proveedores de correo y SMS…) y alguien hace las dos pruebas técnicas: segundo factor por correo/SMS en Keycloak y Firebase Storage desde .NET con el emulador.

## Parte 3 · Después: empieza el código

El primer cambio de código es la base del monorepo (v1.3 §17, paso 1): la solución .NET, `building-blocks/Ascenso.SharedKernel` y `Ascenso.Contracts`, los contratos movidos a `contracts/`, las pruebas de arquitectura y la integración continua de .NET. Después, S2 (Catálogo y Fondos) con sus pruebas de concurrencia, y luego el resto en el orden de la §17.

Cada servicio tiene su carpeta en `services/` (ver [`services/README.md`](../services/README.md)) y cada trabajo empieza como un cambio de OpenSpec, con el flujo de [`guia-openspec.md`](guia-openspec.md).

## Problemas frecuentes

| Síntoma | Solución |
|---|---|
| Avisos `EBADENGINE` al instalar OpenSpec | Node muy viejo: actualiza a 24 LTS o superior y vuelve a ejecutar el script |
| `openspec: command not found` | Cierra y abre Git Bash; si sigue, vuelve a ejecutar `bash tools/openspec/configurar.sh` |
| `git push` rechazado ("fetch first") | `git pull --rebase` y vuelve a hacer `git push` |
| Mis commits no aparecen en mi perfil de GitHub | El correo de `git config user.email` no es el de tu cuenta: corrígelo (paso 2) |
| Avisos de "LF will be replaced by CRLF" | Normales en Windows; el repositorio ya normaliza los finales de línea |
