param(
    [string]$InstallDirectory,
    [string]$BinDirectory
)

$ErrorActionPreference = "Stop"

$ExecutableName = "xrayne"

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

$isWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)

if ([string]::IsNullOrWhiteSpace($InstallDirectory)) {
    $InstallDirectory = if ($isWindows) {
        Join-Path $env:LOCALAPPDATA "XRayne\bin"
    }
    else {
        "/opt/xrayne/cli"
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

if ($isWindows) {
    if (Test-Path $InstallDirectory) {
        Remove-Item -Path $InstallDirectory -Recurse -Force
    }

    Remove-WindowsPath -Directory $InstallDirectory

    Write-Host "XRayne CLI removed."
    Write-Host "Removed application directory: $InstallDirectory"
    return
}

$commandPath = Join-Path $BinDirectory $ExecutableName
if (Test-Path $commandPath) {
    Invoke-Root "rm" @("-f", $commandPath)
}

if (Test-Path $InstallDirectory) {
    Invoke-Root "rm" @("-rf", $InstallDirectory)
}

Remove-UnixPathLine -Directory $BinDirectory

Write-Host "XRayne CLI removed."
Write-Host "Removed application directory: $InstallDirectory"
Write-Host "Removed command path: $commandPath"
