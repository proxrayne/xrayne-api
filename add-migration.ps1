param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateNotNullOrEmpty()]
    [string]$Name
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path

dotnet ef migrations add $Name `
    --project "$root/XRayne.Repositories" `
    --startup-project "$root/XRayne.Api" `
    --context AppDbContext `
    --output-dir Migrations
