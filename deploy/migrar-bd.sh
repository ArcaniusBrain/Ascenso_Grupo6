#!/usr/bin/env bash
set -euo pipefail

# Aplica las migraciones iniciales de EF Core para S1, S2 y S6 contra PostgreSQL (ADR-05 + RF-35).
PG_HOST="${PG_HOST:-127.0.0.1}"
PG_PORT="${PG_PORT:-5432}"
PG_DB="${PG_DB:-ascenso}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$REPO_ROOT"

migrar_servicio() {
  local nombre="$1"
  local schema="$2"
  local env_key="$3"
  local proyecto="$4"
  local rol="${schema}_migrator"
  local conn="Host=${PG_HOST};Port=${PG_PORT};Database=${PG_DB};Username=${rol};Password=dev_${rol}"

  echo "=== Migrando ${nombre} en schema '${schema}' ==="
  env "${env_key}=${conn}" dotnet run --project "$proyecto" -- --migrar
}

migrar_servicio "S1" "s1_identidad" "ConnectionStrings__IdentidadDb" "services/Ascenso.S1.Identidad/Ascenso.S1.Identidad.Api/Ascenso.S1.Identidad.Api.csproj"
migrar_servicio "S2" "s2_catalogo_fondos" "ConnectionStrings__CatalogoFondosDb" "services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Api/Ascenso.S2.CatalogoFondos.Api.csproj"
migrar_servicio "S6" "s6_comunicaciones" "ConnectionStrings__ComunicacionesDb" "services/Ascenso.S6.ComunicacionesReporteria/Ascenso.S6.ComunicacionesReporteria.Api/Ascenso.S6.ComunicacionesReporteria.Api.csproj"

echo "Migraciones aplicadas correctamente en S1, S2 y S6."
