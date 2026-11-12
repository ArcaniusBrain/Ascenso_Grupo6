#!/usr/bin/env bash
# Configura OpenSpec en la máquina de un integrante (Git Bash en Windows, macOS o Linux).
#
# Uso, desde la raíz del repositorio:
#   bash tools/openspec/configurar.sh            # asistente: claude (por defecto)
#   bash tools/openspec/configurar.sh cursor     # otro asistente (ver: openspec init --help)
#   bash tools/openspec/configurar.sh none       # sin asistente, solo la línea de comandos
#
# Qué hace:
#   1. Comprueba Node.js 22.22 o superior (o 24.15+), lo que piden las dependencias de OpenSpec.
#   2. Instala la versión de OpenSpec acordada por el equipo (VERSION_OPENSPEC).
#   3. Ejecuta `openspec init` sin tocar openspec/config.yaml (es del equipo).
#   4. Valida los cambios y specs del repositorio.
#
# Las habilidades que instala (carpetas .agents/ y la de tu asistente) son locales:
# están en .gitignore y cada integrante las genera con este script.

set -euo pipefail

VERSION_OPENSPEC="2.0.0"   # cambiarla en un pull request, junto con .github/workflows/openspec.yml
HERRAMIENTA="${1:-claude}"

cd "$(git rev-parse --show-toplevel)"

echo "== 1/4 Node.js"
if ! command -v node >/dev/null 2>&1; then
  echo "No se encontró Node.js. Instala Node.js 24 LTS desde https://nodejs.org y vuelve a ejecutar." >&2
  exit 1
fi
node -e '
  const [ma, mi] = process.versions.node.split(".").map(Number);
  const ok = (ma === 22 && mi >= 22) || (ma === 24 && mi >= 15) || ma >= 26;
  if (!ok) {
    console.error("Node.js " + process.versions.node + " no sirve: instala Node.js 24 LTS (o 22.22+) desde https://nodejs.org.");
    process.exit(1);
  }
  console.log("Node.js " + process.versions.node + " OK");
'

echo "== 2/4 OpenSpec ${VERSION_OPENSPEC}"
ACTUAL="$(openspec --version 2>/dev/null || true)"
if [ "$ACTUAL" != "$VERSION_OPENSPEC" ]; then
  npm install -g "@fission-ai/openspec@${VERSION_OPENSPEC}"
fi
openspec --version

echo "== 3/4 openspec init (herramientas: ${HERRAMIENTA})"
if [ "$HERRAMIENTA" = "none" ]; then
  TOOLS="none"
else
  TOOLS="${HERRAMIENTA},agents"
fi
openspec init --tools "$TOOLS" --no-animation .

echo "== 4/4 Validación"
openspec validate --all --strict --no-interactive

cat <<'FIN'

Listo. Comandos útiles:
  openspec list               cambios en curso
  openspec list --specs       capacidades especificadas
  openspec view               panel interactivo
  openspec validate --all --strict

Guía del equipo: docs/guia-openspec.md
FIN
