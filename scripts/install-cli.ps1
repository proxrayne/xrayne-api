param(
    [string]$Version = "latest",
    [string]$InstallDirectory,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$Repository = "VanyaKrotov/xrayne"
$ExecutableName = "xrayne"

function Get-PlatformAsset {
    $os = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
    $architecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant()

    if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
        if ($architecture -ne "x64") {
            throw "Unsupported Windows architecture '$architecture'. Only win-x64 CLI builds are published."
        }

        return @{
            Name = "xrayne-cli-win-x64.zip"
            Executable = "xrayne.exe"
            IsWindows = $true
        }
    }

    if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux)) {
        if ($architecture -ne "x64") {
            throw "Unsupported Linux architecture '$architecture'. Only linux-x64 CLI builds are published."
        }

        return @{
            Name = "xrayne-cli-linux-x64.tar.gz"
            Executable = "xrayne"
            IsWindows = $false
        }
    }

    if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)) {
        if ($architecture -ne "arm64") {
            throw "Unsupported macOS architecture '$architecture'. Only osx-arm64 CLI builds are published."
        }

        return @{
            Name = "xrayne-cli-osx-arm64.tar.gz"
            Executable = "xrayne"
            IsWindows = $false
        }
    }

    throw "Unsupported operating system: $os"
}

function Get-Release {
    param([string]$ReleaseVersion)

    if ($ReleaseVersion -eq "latest") {
        $url = "https://api.github.com/repos/$Repository/releases/latest"
    }
    else {
        $url = "https://api.github.com/repos/$Repository/releases/tags/$([uri]::EscapeDataString($ReleaseVersion))"
    }

    return Invoke-RestMethod `
        -Uri $url `
        -Headers @{
            "User-Agent" = "xrayne-installer"
            "Accept" = "application/vnd.github+json"
            "X-GitHub-Api-Version" = "2022-11-28"
        }
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

function Add-WindowsPath {
    param([string]$Directory)

    $userPath = [Environment]::GetEnvironmentVariable("Path", "User")
    $parts = @()
    if (-not [string]::IsNullOrWhiteSpace($userPath)) {
        $parts = $userPath.Split(";", [System.StringSplitOptions]::RemoveEmptyEntries)
    }

    $alreadyExists = $parts | Where-Object { $_.TrimEnd("\") -ieq $Directory.TrimEnd("\") }
    if (-not $alreadyExists) {
        $newPath = if ([string]::IsNullOrWhiteSpace($userPath)) { $Directory } else { "$userPath;$Directory" }
        [Environment]::SetEnvironmentVariable("Path", $newPath, "User")
    }

    if (($env:Path.Split(";", [System.StringSplitOptions]::RemoveEmptyEntries) | Where-Object { $_.TrimEnd("\") -ieq $Directory.TrimEnd("\") }).Count -eq 0) {
        $env:Path = "$env:Path;$Directory"
    }
}

function Add-UnixPath {
    param([string]$Directory)

    $pathParts = $env:PATH.Split(":", [System.StringSplitOptions]::RemoveEmptyEntries)
    if (($pathParts | Where-Object { $_.TrimEnd("/") -eq $Directory.TrimEnd("/") }).Count -gt 0) {
        return
    }

    $profilePath = Join-Path $HOME ".profile"
    $line = "export PATH=`"${Directory}:`$PATH`""

    if (-not (Test-Path $profilePath) -or -not (Select-String -Path $profilePath -Pattern ([regex]::Escape($line)) -Quiet -ErrorAction SilentlyContinue)) {
        Add-Content -Path $profilePath -Value ""
        Add-Content -Path $profilePath -Value "# XRayne CLI"
        Add-Content -Path $profilePath -Value $line
    }

    $env:PATH = "$Directory`:$env:PATH"
    Write-Host "Added '$Directory' to '$profilePath'. Restart the shell or run: source $profilePath"
}

function Install-UnixExecutable {
    param(
        [string]$SourcePath,
        [string]$TargetDirectory,
        [string]$TargetPath
    )

    if (Test-Path $TargetDirectory) {
        $canWrite = $false
        try {
            $probe = Join-Path $TargetDirectory ".xrayne-write-test"
            Set-Content -Path $probe -Value "" -ErrorAction Stop
            Remove-Item -Path $probe -Force
            $canWrite = $true
        }
        catch {
            $canWrite = $false
        }

        if ($canWrite) {
            Copy-Item -Path $SourcePath -Destination $TargetPath -Force
            chmod +x $TargetPath
            return
        }
    }

    if (-not (Test-Command "sudo")) {
        throw "Cannot write to '$TargetDirectory' and sudo is not available. Re-run with -InstallDirectory pointing to a writable directory."
    }

    Invoke-Native "sudo" @("mkdir", "-p", $TargetDirectory)
    Invoke-Native "sudo" @("install", "-m", "755", $SourcePath, $TargetPath)
}

$assetInfo = Get-PlatformAsset

if ([string]::IsNullOrWhiteSpace($InstallDirectory)) {
    if ($assetInfo.IsWindows) {
        $InstallDirectory = Join-Path $env:LOCALAPPDATA "XRayne\bin"
    }
    else {
        $InstallDirectory = "/usr/local/bin"
    }
}

$release = Get-Release -ReleaseVersion $Version
$asset = $release.assets | Where-Object { $_.name -eq $assetInfo.Name } | Select-Object -First 1
if ($null -eq $asset) {
    throw "Asset '$($assetInfo.Name)' was not found in release '$($release.tag_name)'."
}

$temporaryDirectory = Join-Path ([System.IO.Path]::GetTempPath()) "xrayne-cli-install-$([guid]::NewGuid())"
$archivePath = Join-Path $temporaryDirectory $asset.name
$extractDirectory = Join-Path $temporaryDirectory "extract"

New-Item -Path $temporaryDirectory -ItemType Directory -Force | Out-Null
New-Item -Path $extractDirectory -ItemType Directory -Force | Out-Null

try {
    Write-Host "Downloading XRayne CLI $($release.tag_name) from $Repository..."
    Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $archivePath

    if ($assetInfo.IsWindows) {
        Expand-Archive -Path $archivePath -DestinationPath $extractDirectory -Force
    }
    else {
        Invoke-Native "tar" @("-xzf", $archivePath, "-C", $extractDirectory)
    }

    $executablePath = Get-ChildItem -Path $extractDirectory -Filter $assetInfo.Executable -Recurse -File |
        Select-Object -First 1 -ExpandProperty FullName
    if ([string]::IsNullOrWhiteSpace($executablePath)) {
        throw "Executable '$($assetInfo.Executable)' was not found inside '$($asset.name)'."
    }

    if ($assetInfo.IsWindows) {
        New-Item -Path $InstallDirectory -ItemType Directory -Force | Out-Null
        $targetPath = Join-Path $InstallDirectory "xrayne.exe"

        if ((Test-Path $targetPath) -and -not $Force) {
            Write-Host "Replacing existing CLI at '$targetPath'."
        }

        Copy-Item -Path $executablePath -Destination $targetPath -Force
        Add-WindowsPath -Directory $InstallDirectory
    }
    else {
        $targetPath = Join-Path $InstallDirectory $ExecutableName
        Install-UnixExecutable -SourcePath $executablePath -TargetDirectory $InstallDirectory -TargetPath $targetPath
        Add-UnixPath -Directory $InstallDirectory
    }

    Write-Host ""
    Write-Host "XRayne CLI installed successfully."
    Write-Host "Version: $($release.tag_name)"
    Write-Host "Path: $targetPath"
    Write-Host ""
    Write-Host "Try: xrayne version"
}
finally {
    if (Test-Path $temporaryDirectory) {
        Remove-Item -Path $temporaryDirectory -Recurse -Force
    }
}
