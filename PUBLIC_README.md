# XRayne

XRayne is a CLI-managed API and web panel distribution for managing `xray-core`
and panel-managed nodes. The source repository is `xrayne-panel` and contains
the panel only; it does not contain a standalone remote-node service project.

Public install and update assets are intentionally published from
the public Proxrayne repositories: prebuilt CLI binaries, an API Docker image archive, and a
standalone UI Docker image archive.

## Basic Features

- Install the XRayne CLI as the `xrayne` command.
- Install the API and web panel from a public GitHub release.
- Run the API and PostgreSQL through Docker Compose.
- Manage the API service from the CLI: status, start, stop, restart, update.
- Check the installed CLI version and the installed API version.
- Keep API data and PostgreSQL data in a configurable host project folder.

## Release Files

Each release can contain these files:

| File | Description |
| --- | --- |
| `xrayne-cli-linux-x64.tar.gz` | XRayne CLI for Linux x64. |
| `xrayne-cli-osx-arm64.tar.gz` | XRayne CLI for macOS Apple Silicon. |
| `xrayne-cli-win-x64.zip` | XRayne CLI for Windows x64. |
| `xrayne-api-image-<version>.tar.gz` | Docker image archive containing `xrayne-api-image-<version>` for the API. |
| `xrayne-ui-image-<version>.tar.gz` | Docker image archive containing `xrayne-ui-image-<version>` for the web UI. |

The API and UI images are downloaded and loaded by the CLI during `xrayne api install` and `xrayne update`. Docker Compose uses the loaded image names through `API_IMAGE` and `UI_IMAGE`.

## Install CLI With Scripts

The CLI installer also installs the required Linux system modules for API management: package updates, `curl`, `gzip`, Docker, and Docker Compose v2.

### Linux or macOS

Install the latest CLI:

```bash
sudo bash -c "$(curl -sL https://raw.githubusercontent.com/proxrayne/xrayne/main/scripts/install-cli.sh)"
```

Install a specific version:

```bash
sudo bash -c "$(curl -sL https://raw.githubusercontent.com/proxrayne/xrayne/main/scripts/install-cli.sh)" install-cli --version v1.0.0
```

Install to a custom application directory:

```bash
sudo bash -c "$(curl -sL https://raw.githubusercontent.com/proxrayne/xrayne/main/scripts/install-cli.sh)" install-cli --install-dir "/opt/xrayne/cli"
```

The shell installer supports:

```text
--version <tag|latest>
--install-dir <path>
--bin-dir <path>
```

`latest` resolves to the latest stable GitHub release. Pre-release versions are not supported by the installer.

### Windows PowerShell

Download and run the PowerShell installer with `curl`:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -Command "$script = [scriptblock]::Create((curl.exe -sL 'https://raw.githubusercontent.com/proxrayne/xrayne/main/scripts/install-cli.ps1')); & $script"
```

Install a specific version:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -Command "$script = [scriptblock]::Create((curl.exe -sL 'https://raw.githubusercontent.com/proxrayne/xrayne/main/scripts/install-cli.ps1')); & $script -Version 'v1.0.0'"
```

Install to a custom application directory:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -Command "$script = [scriptblock]::Create((curl.exe -sL 'https://raw.githubusercontent.com/proxrayne/xrayne/main/scripts/install-cli.ps1')); & $script -InstallDirectory \"$env:USERPROFILE\.xrayne\bin\""
```

The PowerShell installer supports:

```text
-Version <tag|latest>
-InstallDirectory <path>
-Force
```

`latest` resolves to the latest stable GitHub release. Pre-release versions are not supported by the installer.

## Uninstall CLI

### Linux or macOS

Remove the CLI application directory, command wrapper, and confirmed XRayne project data:

```bash
sudo bash -c "$(curl -sL https://raw.githubusercontent.com/proxrayne/xrayne/main/scripts/uninstall-cli.sh)"
```

Remove a custom installation:

```bash
sudo bash -c "$(curl -sL https://raw.githubusercontent.com/proxrayne/xrayne/main/scripts/uninstall-cli.sh)" uninstall-cli --project-dir "/opt/xrayne" --install-dir "/opt/xrayne/cli" --bin-dir "/usr/local/bin"
```

### Windows PowerShell

Remove the CLI application directory, user PATH entry, and confirmed XRayne project data:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -Command "$script = [scriptblock]::Create((curl.exe -sL 'https://raw.githubusercontent.com/proxrayne/xrayne/main/scripts/uninstall-cli.ps1')); & $script"
```

Remove a custom installation:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -Command "$script = [scriptblock]::Create((curl.exe -sL 'https://raw.githubusercontent.com/proxrayne/xrayne/main/scripts/uninstall-cli.ps1')); & $script -ProjectDirectory \"$env:ProgramFiles\xrayne\" -InstallDirectory \"$env:ProgramFiles\xrayne\cli\""
```

The uninstall scripts verify the project directory and ask for explicit confirmation before deleting API runtime files, Docker Compose files, PostgreSQL data, and `.env`.

## Manual CLI Installation

### Linux x64

```bash
curl -fsSL https://github.com/proxrayne/xrayne-cli/releases/latest/download/xrayne-cli-linux-x64.tar.gz -o xrayne-cli-linux-x64.tar.gz
mkdir -p xrayne-cli
tar -xzf xrayne-cli-linux-x64.tar.gz -C xrayne-cli
sudo mkdir -p /opt/xrayne/cli
sudo cp -R xrayne-cli/. /opt/xrayne/cli/
sudo chmod +x /opt/xrayne/cli/xrayne
sudo tee /usr/local/bin/xrayne >/dev/null <<'EOF'
#!/usr/bin/env sh
cd "/opt/xrayne/cli"
exec "/opt/xrayne/cli/xrayne" "$@"
EOF
sudo chmod +x /usr/local/bin/xrayne
xrayne version
```

### macOS Apple Silicon

```bash
curl -fsSL https://github.com/proxrayne/xrayne-cli/releases/latest/download/xrayne-cli-osx-arm64.tar.gz -o xrayne-cli-osx-arm64.tar.gz
mkdir -p xrayne-cli
tar -xzf xrayne-cli-osx-arm64.tar.gz -C xrayne-cli
sudo mkdir -p /opt/xrayne/cli
sudo cp -R xrayne-cli/. /opt/xrayne/cli/
sudo chmod +x /opt/xrayne/cli/xrayne
sudo tee /usr/local/bin/xrayne >/dev/null <<'EOF'
#!/usr/bin/env sh
cd "/opt/xrayne/cli"
exec "/opt/xrayne/cli/xrayne" "$@"
EOF
sudo chmod +x /usr/local/bin/xrayne
xrayne version
```

### Windows x64

```powershell
curl.exe -fsSL https://github.com/proxrayne/xrayne-cli/releases/latest/download/xrayne-cli-win-x64.zip -o xrayne-cli-win-x64.zip
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

The installer downloads the API and UI Docker images from the public release, writes `/opt/xrayne/.env`, creates `/opt/xrayne/docker-compose.yml`, and starts Docker Compose.

Install the CLI through the script first. The CLI installer prepares Docker and Docker Compose; `xrayne api install` only configures and starts the API runtime.

During installation you will be prompted for:

- API port, default `5000`.
- UI port, default `8080`.
- PostgreSQL password for user `postgres`; an empty value generates a password.
- Optional API prefix for hiding the panel behind a custom path.

The project path is derived from the installed CLI directory. For the default `/opt/xrayne/cli` CLI installation, the project path is `/opt/xrayne`. The project folder contains `config.json`, `.env`, `logs`, `postgres`, `xray`, and other runtime folders at the same level. PostgreSQL data is stored in:

```text
<project-path>/postgres
```

The web panel is served by the UI container. By default it is available at `http://0.0.0.0:8080`, and it proxies `/api` to the API container listening on `http://0.0.0.0:5000/api`.

The API container runs with Docker host networking. `PORT` is the actual port the API listens on; Docker does not publish a separate API container port mapping.

## API Service Commands

```bash
xrayne api version
xrayne api status
xrayne api start
xrayne api stop
xrayne api restart
xrayne cert install --domain example.com --email admin@example.com
xrayne cert install --ip-address 203.0.113.10 --email admin@example.com
xrayne cert status
xrayne cert renew
xrayne update [--version latest|tag] [--component all|api|ui|cli] [--force]
xrayne info
```

`xrayne api version` prints the installed API version, the latest available version, and whether an update is available.

`xrayne update` checks the selected release and updates CLI, API, and UI by default. Use `--component api`, `--component ui`, or `--component cli` to update only one side. Passing an older release tag through `--version` intentionally downgrades that component.

During update, the CLI migrates runtime files (`.env`, `config.json`, and `docker-compose.yml`) to the schema required by the target release. Migrations support both upgrade and downgrade paths, create backups under `<project-path>/backups/runtime-migrations`, and run before the CLI binary is replaced so downgrade migrations are still handled by the newer CLI.

`xrayne info` prints project/runtime information and checks whether CLI or API updates are available.

## HTTPS Certificates

`xrayne cert install` issues a Let's Encrypt certificate through `acme.sh` and installs it for the ASP.NET Core API container. The command stores `acme.sh` under `<project-path>/certificates/acme-sh`, stores installed certificate files under `<project-path>/certificates/letsencrypt`, writes certificate paths to `<project-path>/.env`, keeps the API listening on the configured `PORT`, enables automatic renewal through `acme.sh`, and recreates the API container.

For a domain certificate, point the domain at the server first, then run:

```bash
xrayne cert install --domain example.com --email admin@example.com
```

For an IP address certificate, pass a public IPv4 address:

```bash
xrayne cert install --ip-address 203.0.113.10 --email admin@example.com
```

If neither `--domain` nor `--ip-address` is passed, the CLI resolves the server public IPv4 address and issues an IP certificate for it. IP address certificates use the Let's Encrypt `shortlived` profile, so they are valid for about six days and must be renewed frequently. The command uses standalone HTTP-01 validation, so port `80` must be reachable from the Internet while the certificate is being issued or renewed. Private, loopback, and reserved IP addresses are rejected. HTTPS uses the same `PORT` that was selected during API installation.

```bash
xrayne cert renew
```

Use `xrayne cert status` to inspect the configured certificate paths, identifier, ACME client, certificate profile, and auto-renew state.

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
| `/opt/xrayne/cli` | Default CLI application directory containing the executable and packaged `appsettings*.json`. |
| `/usr/local/bin/xrayne` | Default Linux/macOS command wrapper added to PATH. |
| `/opt/xrayne` | Default project folder. |
| `/opt/xrayne/config.json` | Default mutable API/CLI configuration file. |
| `/opt/xrayne/certificates` | Default HTTPS certificate storage folder. |
| `/opt/xrayne/logs` | Default logs folder. |
| `/opt/xrayne/postgres` | Default PostgreSQL data folder. |
| `/opt/xrayne/xray` | Default Xray files folder. |
