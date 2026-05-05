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

    $release = Invoke-RestMethod `
        -Uri $url `
        -Headers @{
            "User-Agent" = "xrayne-installer"
            "Accept" = "application/vnd.github+json"
            "X-GitHub-Api-Version" = "2022-11-28"
        }

    if ($release.prerelease) {
        throw "Pre-release versions are not supported by this installer. Use a stable release tag."
    }

    return $release
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

function Install-UnixSystemDependencies {
    $scriptPath = Join-Path ([System.IO.Path]::GetTempPath()) "xrayne-install-deps-$([guid]::NewGuid()).sh"
    $script = @'
set -eu

download() {
  url="$1"
  destination="$2"

  if command -v curl >/dev/null 2>&1; then
    curl -fsSL "$url" -o "$destination"
  elif command -v wget >/dev/null 2>&1; then
    wget -q "$url" -O "$destination"
  else
    echo "curl or wget is required." >&2
    exit 1
  fi
}

install_compose_plugin_binary() {
  compose_arch="$(uname -m)"
  case "$compose_arch" in
    x86_64|amd64)
      compose_arch="x86_64"
      ;;
    aarch64|arm64)
      compose_arch="aarch64"
      ;;
    *)
      echo "Unsupported Docker Compose architecture: $compose_arch" >&2
      exit 1
      ;;
  esac

  plugin_dir="/usr/local/lib/docker/cli-plugins"
  plugin_path="$plugin_dir/docker-compose"
  plugin_url="https://github.com/docker/compose/releases/latest/download/docker-compose-linux-$compose_arch"

  echo "Installing Docker Compose plugin from $plugin_url"
  mkdir -p "$plugin_dir"
  tmp_compose="$(mktemp)"
  download "$plugin_url" "$tmp_compose"
  install -m 755 "$tmp_compose" "$plugin_path"
  rm -f "$tmp_compose"
}

if command -v apt-get >/dev/null 2>&1; then
  apt-get update
  env DEBIAN_FRONTEND=noninteractive apt-get install -y ca-certificates curl gzip docker.io
  env DEBIAN_FRONTEND=noninteractive apt-get install -y docker-compose-plugin || \
    env DEBIAN_FRONTEND=noninteractive apt-get install -y docker-compose-v2 || true
elif command -v dnf >/dev/null 2>&1; then
  dnf makecache -y
  dnf install -y ca-certificates curl gzip docker
  dnf install -y docker-compose-plugin || true
elif command -v yum >/dev/null 2>&1; then
  yum makecache -y
  yum install -y ca-certificates curl gzip docker
  yum install -y docker-compose-plugin || true
elif command -v apk >/dev/null 2>&1; then
  apk update
  apk add --no-cache ca-certificates curl gzip docker
  apk add --no-cache docker-cli-compose || true
else
  echo "Unsupported Linux package manager. Install Docker and Docker Compose plugin manually." >&2
  exit 1
fi

if ! docker compose version >/dev/null 2>&1; then
  install_compose_plugin_binary
fi

if command -v systemctl >/dev/null 2>&1; then
  systemctl enable --now docker
elif command -v service >/dev/null 2>&1; then
  service docker start
fi

docker --version
docker compose version
'@

    Set-Content -Path $scriptPath -Value $script
    try {
        if ([System.Environment]::UserName -eq "root") {
            Invoke-Native "sh" @($scriptPath)
        }
        else {
            Invoke-Native "sudo" @("sh", $scriptPath)
        }
    }
    finally {
        if (Test-Path $scriptPath) {
            Remove-Item -Path $scriptPath -Force
        }
    }
}

$assetInfo = Get-PlatformAsset

if ([string]::IsNullOrWhiteSpace($InstallDirectory)) {
    if ($assetInfo.IsWindows) {
        $InstallDirectory = Join-Path $env:LOCALAPPDATA "XRayne\bin"
    }
    else {
        $InstallDirectory = "/opt/xrayne/cli"
    }
}

if (-not $assetInfo.IsWindows) {
    Write-Host "Updating system packages and installing required modules..."
    Install-UnixSystemDependencies
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
            Write-Host "Replacing existing CLI in '$InstallDirectory'."
        }

        Copy-Item -Path (Join-Path $extractDirectory "*") -Destination $InstallDirectory -Recurse -Force
        Add-WindowsPath -Directory $InstallDirectory
    }
    else {
        $binDirectory = "/usr/local/bin"
        $targetPath = Join-Path $binDirectory $ExecutableName
        $wrapperPath = Join-Path $temporaryDirectory "xrayne-wrapper"
        Set-Content -Path $wrapperPath -Value @"
#!/usr/bin/env sh
export XRAYNE_CLI_CONFIG_DIR="$InstallDirectory"
cd "$InstallDirectory"
exec "$InstallDirectory/$ExecutableName" "`$@"
"@

        Invoke-Native "sudo" @("mkdir", "-p", $InstallDirectory)
        Invoke-Native "sudo" @("mkdir", "-p", $binDirectory)
        Invoke-Native "sudo" @("mkdir", "-p", "/opt/xrayne", "/usr/shared/xrayne")
        Invoke-Native "sudo" @("cp", "-R", "$extractDirectory/.", $InstallDirectory)
        Invoke-Native "sudo" @("chmod", "+x", "$InstallDirectory/$ExecutableName")
        Install-UnixExecutable -SourcePath $wrapperPath -TargetDirectory $binDirectory -TargetPath $targetPath
        if ($env:SUDO_UID -and $env:SUDO_GID) {
            Invoke-Native "sudo" @("chown", "$env:SUDO_UID`:$env:SUDO_GID", "/opt/xrayne", "/usr/shared/xrayne")
        }
        Add-UnixPath -Directory $binDirectory
    }

    Write-Host ""
    Write-Host "XRayne CLI installed successfully."
    Write-Host "Version: $($release.tag_name)"
    Write-Host "Application directory: $InstallDirectory"
    Write-Host "Command path: $targetPath"
    Write-Host ""
    Write-Host "Try: xrayne version"
}
finally {
    if (Test-Path $temporaryDirectory) {
        Remove-Item -Path $temporaryDirectory -Recurse -Force
    }
}
