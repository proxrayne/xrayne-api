# XRayne

XRayne is a CLI-managed API and web panel distribution. The public repository provides ready-to-use release artifacts only: prebuilt CLI binaries and a Docker image archive that contains the API with the built web UI.

## Basic Features

- Install the XRayne CLI as the `xrayne` command.
- Install the API and web panel from a public GitHub release.
- Run the API and PostgreSQL through Docker Compose.
- Manage the API service from the CLI: status, start, stop, restart, update.
- Check the installed CLI version and the installed API version.
- Keep API data and PostgreSQL data in a configurable host data folder.

## Release Files

Each release can contain these files:

| File | Description |
| --- | --- |
| `xrayne-cli-linux-x64.tar.gz` | XRayne CLI for Linux x64. |
| `xrayne-cli-osx-arm64.tar.gz` | XRayne CLI for macOS Apple Silicon. |
| `xrayne-cli-win-x64.zip` | XRayne CLI for Windows x64. |
| `xrayne-api-image-<version>.tar.gz` | Docker image archive for the API with the built web UI. |

The API image is downloaded and loaded by the CLI during `xrayne api install` and `xrayne api update`.

## Install CLI With Scripts

### Linux or macOS

Install the latest CLI:

```bash
sudo bash -c "$(curl -sL https://raw.githubusercontent.com/VanyaKrotov/xrayne/main/scripts/install-cli.sh)"
```

Install a specific version:

```bash
sudo bash -c "$(curl -sL https://raw.githubusercontent.com/VanyaKrotov/xrayne/main/scripts/install-cli.sh)" install-cli --version v1.0.0
```

Install to a custom directory:

```bash
sudo bash -c "$(curl -sL https://raw.githubusercontent.com/VanyaKrotov/xrayne/main/scripts/install-cli.sh)" install-cli --install-dir "$HOME/.local/bin"
```

The shell installer supports:

```text
--version <tag|latest>
--install-dir <path>
```

`latest` resolves to the latest stable GitHub release. Pre-release versions are not supported by the installer.

### Windows PowerShell

Download and run the PowerShell installer with `curl`:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -Command "$script = [scriptblock]::Create((curl.exe -sL 'https://raw.githubusercontent.com/VanyaKrotov/xrayne/main/scripts/install-cli.ps1')); & $script"
```

Install a specific version:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -Command "$script = [scriptblock]::Create((curl.exe -sL 'https://raw.githubusercontent.com/VanyaKrotov/xrayne/main/scripts/install-cli.ps1')); & $script -Version 'v1.0.0'"
```

Install to a custom directory:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -Command "$script = [scriptblock]::Create((curl.exe -sL 'https://raw.githubusercontent.com/VanyaKrotov/xrayne/main/scripts/install-cli.ps1')); & $script -InstallDirectory \"$env:USERPROFILE\.xrayne\bin\""
```

The PowerShell installer supports:

```text
-Version <tag|latest>
-InstallDirectory <path>
-Force
```

`latest` resolves to the latest stable GitHub release. Pre-release versions are not supported by the installer.

## Manual CLI Installation

### Linux x64

```bash
curl -fsSL https://github.com/VanyaKrotov/xrayne/releases/latest/download/xrayne-cli-linux-x64.tar.gz -o xrayne-cli-linux-x64.tar.gz
mkdir -p xrayne-cli
tar -xzf xrayne-cli-linux-x64.tar.gz -C xrayne-cli
sudo install -m 755 xrayne-cli/xrayne /usr/local/bin/xrayne
xrayne version
```

### macOS Apple Silicon

```bash
curl -fsSL https://github.com/VanyaKrotov/xrayne/releases/latest/download/xrayne-cli-osx-arm64.tar.gz -o xrayne-cli-osx-arm64.tar.gz
mkdir -p xrayne-cli
tar -xzf xrayne-cli-osx-arm64.tar.gz -C xrayne-cli
sudo install -m 755 xrayne-cli/xrayne /usr/local/bin/xrayne
xrayne version
```

### Windows x64

```powershell
curl.exe -fsSL https://github.com/VanyaKrotov/xrayne/releases/latest/download/xrayne-cli-win-x64.zip -o xrayne-cli-win-x64.zip
Expand-Archive -Path .\xrayne-cli-win-x64.zip -DestinationPath "$env:LOCALAPPDATA\XRayne\bin" -Force
[Environment]::SetEnvironmentVariable("Path", [Environment]::GetEnvironmentVariable("Path", "User") + ";$env:LOCALAPPDATA\XRayne\bin", "User")
$env:Path = "$env:Path;$env:LOCALAPPDATA\XRayne\bin"
xrayne version
```

For manual installation of a specific stable version, replace `latest` in the URL with `download/<tag>`, for example `download/v1.0.0`.

## Install API And Web Panel

After the CLI is installed, run:

```bash
xrayne api install
```

The installer downloads the API Docker image from the public release, writes `/opt/xrayne/.env`, creates `/opt/xrayne/docker-compose.yml`, and starts Docker Compose.

During installation you will be prompted for:

- API port, default `5000`.
- PostgreSQL password for user `postgres`; an empty value generates a password.
- Data folder, default `/usr/shared/xrayne`.
- Optional API prefix for hiding the panel behind a custom path.

PostgreSQL data is stored in:

```text
<data-folder>/postgres
```

The web panel is served by the API container on the same host and port.

## API Service Commands

```bash
xrayne api version
xrayne api status
xrayne api start
xrayne api stop
xrayne api restart
xrayne api update
```

`xrayne api version` prints the installed API version, the latest available version, and whether an update is available.

`xrayne api update` downloads the latest API image from the release, loads it into Docker, updates `/opt/xrayne/.env`, and restarts Docker Compose.

## CLI Version

```bash
xrayne version
```

Example output:

```text
CLI Version: 0.1.1
Commit: 98dbe14942c18c71b22a19d5d7c0098624249352
```

## Default Paths

| Path | Description |
| --- | --- |
| `/opt/xrayne/.env` | Runtime environment file generated by `xrayne api install`. |
| `/opt/xrayne/docker-compose.yml` | Docker Compose file generated by `xrayne api install`. |
| `/usr/shared/xrayne` | Default shared data folder. |
| `/usr/shared/xrayne/postgres` | Default PostgreSQL data folder. |
