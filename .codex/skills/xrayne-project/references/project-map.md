# XRayne.Node Project Map

## Product

XRayne.Node is a node and admin panel for managing `xray-core`. The intended surface includes CLI commands, a REST API, and a React web UI for local standalone administration and eventual remote-node scenarios.

## Repository Layout

- `XRayne.sln`: .NET solution.
- `Directory.Build.props`: shared .NET settings, currently `net9.0`, nullable enabled, implicit usings enabled.
- `global.json`: SDK `9.0.100` with `latestFeature` roll-forward.
- `XRayne.Api`: ASP.NET Core web API and static web host.
- `XRayne.Cli`: System.CommandLine CLI executable with assembly name `xrayne`.
- `XRayne.Core`: domain-level types and xray-core setup.
- `XRayne.Infrastructure`: auth services and xray service implementations.
- `XRayne.Repositories`: EF Core, PostgreSQL, migrations, repositories.
- `XRayne.Contracts`: shared contracts marker project.
- `XRayne.Test`: test project, currently minimal.
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

- API reads normal ASP.NET Core configuration. `XRayne.Api/appsettings*.json` includes `ConnectionStrings:Default`, `Jwt`, `Docs`, `Cors:SpaOrigins`, and `Xray`.
- CLI sets base path to `AppContext.BaseDirectory`, reads `appsettings.json`, environment-specific appsettings, `/opt/xrayne/.env` when present, and environment variables with the `XRAYNE_` prefix.
- `/opt/xrayne/.env` is the shared installed-runtime source of truth. Store PostgreSQL credentials, host/port values, data folder, API prefix, and API image tag there; derive separate CLI/API connection strings from those values because host networking differs between the host CLI and the API container.
- `Xray:Directory` defaults in appsettings to `../shared/`; `Xray:FileName` is platform-specific and currently Windows-oriented in local appsettings.
- Database connection key is `ConnectionStrings:Default`.

## CI And Packaging

- Release artifact workflow restores and publishes `XRayne.Cli/XRayne.Cli.csproj` for each RID.
- The same workflow builds the API+UI Docker image, saves it as `xrayne-api-image-<version>.tar.gz`, uploads it as a build artifact, and attaches it to tagged releases.
- Release upload happens only for tag refs.
