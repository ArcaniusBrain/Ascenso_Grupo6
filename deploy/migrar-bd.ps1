<#
.SYNOPSIS
Aplica las migraciones iniciales de EF Core para S1, S2 y S6 contra PostgreSQL (deploy/docker-compose.yml).
Cada servicio se migra con su rol aislado `*_migrator` (ADR-05), instala el trigger append-only RF-35
y siembra los 7 tipos de usuario predefinidos en S1 de forma idempotente.
#>
param(
    [string]$PgHost = "127.0.0.1",
    [int]$PgPort = 5432,
    [string]$Database = "ascenso"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

Push-Location $repoRoot
try {
    $servicios = @(
        @{ Nombre = "S1"; Schema = "s1_identidad"; ConnKey = "ConnectionStrings__IdentidadDb"; Proyecto = "services/Ascenso.S1.Identidad/Ascenso.S1.Identidad.Api/Ascenso.S1.Identidad.Api.csproj" },
        @{ Nombre = "S2"; Schema = "s2_catalogo_fondos"; ConnKey = "ConnectionStrings__CatalogoFondosDb"; Proyecto = "services/Ascenso.S2.CatalogoFondos/Ascenso.S2.CatalogoFondos.Api/Ascenso.S2.CatalogoFondos.Api.csproj" },
        @{ Nombre = "S6"; Schema = "s6_comunicaciones"; ConnKey = "ConnectionStrings__ComunicacionesDb"; Proyecto = "services/Ascenso.S6.ComunicacionesReporteria/Ascenso.S6.ComunicacionesReporteria.Api/Ascenso.S6.ComunicacionesReporteria.Api.csproj" }
    )

    foreach ($svc in $servicios) {
        $rol = "$($svc.Schema)_migrator"
        $conn = "Host=$PgHost;Port=$PgPort;Database=$Database;Username=$rol;Password=dev_$rol"
        [Environment]::SetEnvironmentVariable($svc.ConnKey, $conn, "Process")
        try {
            Write-Host "=== Migrando $($svc.Nombre) en schema '$($svc.Schema)' ==="
            dotnet run --project $svc.Proyecto -- --migrar
            if ($LASTEXITCODE -ne 0) { throw "Falló la migración de $($svc.Nombre)." }
        }
        finally {
            [Environment]::SetEnvironmentVariable($svc.ConnKey, $null, "Process")
        }
    }

    Write-Host "Migraciones aplicadas correctamente en S1, S2 y S6."
}
finally {
    Pop-Location
}
