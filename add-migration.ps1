param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateNotNullOrEmpty()]
    [string]$Name
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path

dotnet ef migrations add $Name `
    --project "$root/Repositories" `
    --startup-project "$root/Api" `
    --context AppDbContext `
    --output-dir Migrations
