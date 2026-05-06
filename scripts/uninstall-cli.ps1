param(
    [string]$InstallDirectory,
    [string]$BinDirectory,
    [string]$ProjectDirectory
)

$ErrorActionPreference = "Stop"

$ExecutableName = "xrayne"
$isWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
$defaultProjectDirectory = if ($isWindows) {
    Join-Path $env:ProgramFiles "xrayne"
}
else {
    "/opt/xrayne"
}

function Test-Command {
    param([string]$Name)

    $null -ne (Get-Command $Name -ErrorAction SilentlyContinue)
}

function Invoke-Native {
    param(
        [string]$Command,
        [string[]]$Arguments
    )

    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Command $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Invoke-Root {
    param(
        [string]$Command,
        [string[]]$Arguments
    )

    if ([System.Environment]::UserName -eq "root") {
        Invoke-Native $Command $Arguments
        return
    }

    if (-not (Test-Command "sudo")) {
        throw "sudo is required to remove system CLI files."
    }

    $sudoArguments = @($Command) + $Arguments
    Invoke-Native "sudo" $sudoArguments
}

function Remove-WindowsPath {
    param([string]$Directory)

    $userPath = [Environment]::GetEnvironmentVariable("Path", "User")
    if ([string]::IsNullOrWhiteSpace($userPath)) {
        return
    }

    $parts = $userPath.Split(";", [System.StringSplitOptions]::RemoveEmptyEntries) |
        Where-Object { $_.TrimEnd("\") -ine $Directory.TrimEnd("\") }

    [Environment]::SetEnvironmentVariable("Path", ($parts -join ";"), "User")
    $env:Path = ($env:Path.Split(";", [System.StringSplitOptions]::RemoveEmptyEntries) |
        Where-Object { $_.TrimEnd("\") -ine $Directory.TrimEnd("\") }) -join ";"
}

function Remove-UnixPathLine {
    param([string]$Directory)

    $profilePath = Join-Path $HOME ".profile"
    if (-not (Test-Path $profilePath)) {
        return
    }

    $line = "export PATH=`"${Directory}:`$PATH`""
    $lines = Get-Content -Path $profilePath | Where-Object {
        $_ -ne $line -and $_ -ne "# XRayne CLI"
    }

    Set-Content -Path $profilePath -Value $lines
}

function Test-XRayneProjectDirectory {
    param([string]$Directory)

    if ([string]::IsNullOrWhiteSpace($Directory) -or -not (Test-Path $Directory -PathType Container)) {
        return $false
    }

    $markers = @(
        (Join-Path $Directory "cli\$ExecutableName"),
        (Join-Path $Directory "cli\$ExecutableName.exe"),
        (Join-Path $Directory ".env"),
        (Join-Path $Directory "config.json"),
        (Join-Path $Directory "docker-compose.yml"),
        (Join-Path $Directory "docker-compose.yaml")
    )

    foreach ($marker in $markers) {
        if (Test-Path $marker) {
            return $true
        }
    }

    return $false
}

function Assert-SafeProjectDirectory {
    param([string]$Directory)

    if ([string]::IsNullOrWhiteSpace($Directory)) {
        throw "Project directory cannot be empty."
    }

    $resolved = Resolve-Path -Path $Directory -ErrorAction SilentlyContinue
    if ($null -eq $resolved) {
        throw "Project directory '$Directory' was not found."
    }

    $fullPath = [System.IO.Path]::GetFullPath($resolved.Path).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    $root = [System.IO.Path]::GetPathRoot($fullPath).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    if ($fullPath -eq $root) {
        throw "Refusing to remove unsafe project directory '$fullPath'."
    }

    if (-not (Test-XRayneProjectDirectory -Directory $fullPath)) {
        throw "'$fullPath' does not look like an XRayne project directory. Expected at least one of: cli executable, .env, config.json, docker-compose.yml."
    }

    return $fullPath
}

function Stop-ProjectCompose {
    param([string]$Directory)

    $composePath = Join-Path $Directory "docker-compose.yml"
    if (-not (Test-Path $composePath)) {
        $composePath = Join-Path $Directory "docker-compose.yaml"
    }

    if (-not (Test-Path $composePath)) {
        return
    }

    Write-Host "Stopping and removing Docker containers..."
    try {
        if (Test-Command "docker") {
            & docker compose version *> $null
            if ($LASTEXITCODE -eq 0) {
                Invoke-Root "docker" @("compose", "-f", $composePath, "down")
                return
            }
        }

        if (Test-Command "docker-compose") {
            Invoke-Root "docker-compose" @("-f", $composePath, "down")
            return
        }

        Write-Host "Warning: Docker Compose not found. Containers might still be running."
    }
    catch {
        Write-Host "Warning: Failed to stop docker containers, proceeding anyway. $($_.Exception.Message)"
    }
}

if ([string]::IsNullOrWhiteSpace($ProjectDirectory)) {
    if (Test-XRayneProjectDirectory -Directory $defaultProjectDirectory) {
        $ProjectDirectory = $defaultProjectDirectory
    }
    else {
        $ProjectDirectory = Read-Host "Standard project directory '$defaultProjectDirectory' was not found. Enter XRayne project directory"
    }
}

$ProjectDirectory = Assert-SafeProjectDirectory -Directory $ProjectDirectory

if ([string]::IsNullOrWhiteSpace($InstallDirectory)) {
    $InstallDirectory = if ($isWindows) {
        Join-Path $ProjectDirectory "cli"
    }
    else {
        Join-Path $ProjectDirectory "cli"
    }
}

if ([string]::IsNullOrWhiteSpace($BinDirectory)) {
    $BinDirectory = if ($isWindows) {
        $InstallDirectory
    }
    else {
        "/usr/local/bin"
    }
}

Write-Host "Project directory selected for removal: $ProjectDirectory"
$confirmation = Read-Host "This will delete all XRayne data in this directory. Type 'yes' to continue"
if ($confirmation -ne "yes") {
    Write-Host "Uninstall cancelled."
    return
}

Stop-ProjectCompose -Directory $ProjectDirectory

if ($isWindows) {
    Remove-WindowsPath -Directory $InstallDirectory

    if (Test-Path $ProjectDirectory) {
        Remove-Item -Path $ProjectDirectory -Recurse -Force
    }

    Write-Host "XRayne CLI removed."
    Write-Host "Removed project directory: $ProjectDirectory"
    return
}

$commandPath = Join-Path $BinDirectory $ExecutableName
if (Test-Path $commandPath) {
    Invoke-Root "rm" @("-f", $commandPath)
}

if (Test-Path $ProjectDirectory) {
    Invoke-Root "rm" @("-rf", $ProjectDirectory)
}

Remove-UnixPathLine -Directory $BinDirectory

Write-Host "XRayne CLI removed."
Write-Host "Removed project directory: $ProjectDirectory"
Write-Host "Removed command path: $commandPath"
