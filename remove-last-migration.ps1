param(
    [Parameter(Mandatory = $false, Position = 0)]
    [ValidateNotNullOrEmpty()]
    [string]$Name
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path

if (-not [string]::IsNullOrWhiteSpace($Name)) {
    $migrationsPath = Join-Path $root "Data\Migrations"
    $latestMigration = Get-ChildItem -Path $migrationsPath -Filter "*.cs" -File |
        Where-Object { $_.Name -match '^\d{14}_.+\.cs$' -and $_.Name -notmatch '\.Designer\.cs$' } |
        Sort-Object Name -Descending |
        Select-Object -First 1

    if ($null -eq $latestMigration) {
        throw "Cannot validate migration name '$Name' because no migrations were found in '$migrationsPath'."
    }

    $latestMigrationName = $latestMigration.BaseName -replace '^\d{14}_', ''
    if ($Name -cne $latestMigrationName) {
        throw "Migration name '$Name' does not match the latest migration '$latestMigrationName'."
    }
}

dotnet ef migrations remove `
    --project "$root/Data" `
    --startup-project "$root/Api" `
    --context AppDbContext
