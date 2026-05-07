# XRayne.Node Project Map

## Product

XRayne.Node is a node and admin panel for managing `xray-core`. The intended surface includes CLI commands, a REST API, and a React web UI for local standalone administration and eventual remote-node scenarios.

## Repository Layout

- `XRayne.sln`: .NET solution.
- `Directory.Build.props`: shared .NET settings, currently `net9.0`, nullable enabled, implicit usings enabled.
- `global.json`: SDK `9.0.100` with `latestFeature` roll-forward.
- `XRayne.Api`: ASP.NET Core web API and static web host.
- `XRayne.Cli`: System.CommandLine CLI executable with assembly name `xrayne`.
- `XRayne.Core`: xray-core setup and core runtime abstractions.
- `XRayne.Infrastructure`: JWT/core service implementations plus static utilities such as `JsonConfig`, `EnvConfig`, `NetworkAddress`, and password hashing/generation.
- `XRayne.Repositories`: EF Core, PostgreSQL, migrations, repositories, and external clients under `External` such as `GitHubRepository`.
- `XRayne.Contracts`: shared contracts, configuration DTOs, permission enums, and permission names.
- `XRayne.Test`: backend test project.
- `XRayne.UI`: React Router app.
- `.github/workflows/build-cli.yml`: publishes single-file CLI artifacts for `win-x64`, `osx-arm64`, and `linux-x64` on tags or manual dispatch.
- `.codex/skills`: project-local Codex skills.

## Important Current State

- API/UI Docker image build is a release artifact: GitHub Actions builds the API image with UI files in `wwwroot`, saves it as `tar.gz`, and attaches it to releases.
- `docker-compose.yml` may be absent; if restored for installation, it should run the prebuilt image instead of using `build:`.
- `.env.example` contains PostgreSQL, JWT, CORS, docs, and Xray config keys, not API container settings.
- `XRayne.Api/Dockerfile` builds UI first, copies `XRayne.UI/build/client` into `XRayne.Api/wwwroot`, publishes API, and produces the runtime image.

## Common Commands

Run from repo root unless noted.

```powershell
dotnet restore XRayne.sln
dotnet build XRayne.sln
dotnet test XRayne.sln
dotnet run --project XRayne.Api
dotnet run --project XRayne.Cli -- admin create admin --password password --permissions super_admin
.\add-migration.ps1 MigrationName
```

For UI:

```powershell
Set-Location XRayne.UI
npm run dev
npm run typecheck
npm run lint
npm run build
```

## Configuration

- API reads normal ASP.NET Core configuration from packaged `appsettings*.json`, then runtime `PathProvider.ConfigPath` (`config.json` in the shared project directory) and `PathProvider.EnvironmentPath`.
- CLI sets base path to `AppContext.BaseDirectory`, reads packaged `appsettings.json`, environment-specific `appsettings.{Environment}.json`, runtime `PathProvider.ConfigPath`, `PathProvider.EnvironmentPath`, and environment variables through the shared configuration pipeline.
- `PathProvider` in `XRayne.Infrastructure.Values` centralizes runtime paths: project directory, `.env`, `config.json`, `docker-compose.yml`, `logs`, `postgres`, and `xray`.
- When CLI is installed under a `cli` folder, `PathProvider.DefaultProjectDirectory` is the parent directory of `AppContext.BaseDirectory`; for example `/opt/xrayne/cli` resolves to `/opt/xrayne`. `PROJECT_PATH` can still override this for runtime reads.
- `JsonConfig` and `EnvConfig` in `XRayne.Infrastructure.Utilities` are static helpers for safe runtime file mutations. Reading is done through standard `IConfiguration`.
- `.env` is static after install and reserved for Docker Compose/bootstrap variables such as `PROJECT_PATH`, `API_IMAGE`, `API_PORT`, and PostgreSQL values. More complex application configuration belongs in runtime `config.json`. Docker Compose runs from the project directory and reads the `.env` file beside `docker-compose.yml`; services should use `env_file: .env` when they need the same values inside containers.
- `Xray:Directory` should come from the runtime project layout as the `xray` folder; keep platform-specific `Xray:FileName` in packaged config unless runtime editing is intentional.
- Database connection key is `ConnectionStrings:Default`.

## CI And Packaging

- Release artifact workflow restores and publishes `XRayne.Cli/XRayne.Cli.csproj` for each RID.
- The same workflow builds the API+UI Docker image, saves it as `xrayne-api-image-<version>.tar.gz`, uploads it as a build artifact, and attaches it to tagged releases.
- Release upload happens only for tag refs.
