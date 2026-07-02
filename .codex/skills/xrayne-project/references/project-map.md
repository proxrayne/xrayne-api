# XRayne Project Map

## Product

XRayne Panel is an admin panel for managing `xray-core` and remote nodes. The
`xrayne-panel` repository contains panel code only: CLI commands, a REST API,
and a React web UI. Managed nodes are a panel feature, not a local standalone
node-service project.

Canonical documentation lives under `docs/`. Read `docs/project-rules.md` for project-wide rules and `docs/conventions/inconsistencies.md` before broad cleanup work.

## Repository Layout

- `XRayne.sln`: .NET solution.
- `Directory.Build.props`: shared .NET settings, currently `net9.0`, nullable enabled, implicit usings enabled.
- `global.json`: SDK `9.0.100` with `latestFeature` roll-forward.
- `Api`: ASP.NET Core web API and static web host.
- `Cli`: System.CommandLine CLI executable with assembly name `xrayne`.
- `Infrastructure`: xray-core setup, core runtime abstractions, infrastructure services, and background jobs.
- `Infrastructure`: JWT/core service implementations plus infrastructure utilities such as `NetworkAddress` and password hashing/generation.
- `Repositories`: EF Core, PostgreSQL, migrations, entity models, repositories, runtime config file utilities such as `JsonConfig`/`EnvConfig`, and external clients under `External` such as `GitHubRepository`.
- `Contracts`: shared contracts, configuration DTOs, shared API/query models, permission enums, and permission names.
- `Test`: backend test project.
- `Dashboard`: React Router app.
- `.github/workflows/build.yml`: publishes single-file CLI artifacts for `win-x64`, `osx-arm64`, and `linux-x64`, and publishes the API+UI Docker image archive on tags or manual dispatch.
- `.codex/skills`: project-local Codex skills.
- `docs`: canonical architecture, styleguide, conventions, and project skill documentation.

## Important Current State

- API/UI Docker image build is a release artifact: GitHub Actions builds the API image as `xrayne-api-image-<version>` with UI files in `wwwroot`, saves it as `xrayne-api-image-<version>.tar.gz`, and attaches it to releases.
- Public release/install assets intentionally use the `VanyaKrotov/xrayne` GitHub repository; source-level documentation refers to this repository as `xrayne-panel`.
- Node management remains in the panel through API endpoints, infrastructure services, repositories, and dashboard routes.
- `docker-compose.yml` may be absent; if restored for installation, it should run the prebuilt image instead of using `build:`.
- `.env.example` contains PostgreSQL, JWT, CORS, docs, and Xray config keys, not API container settings.
- `Api/Dockerfile` builds dashboard first, copies `Dashboard/build/client` into `Api/wwwroot`, publishes API, and produces the runtime image.

## Common Commands

Run from repo root unless noted.

```powershell
dotnet restore XRayne.sln
dotnet build XRayne.sln
dotnet test XRayne.sln
dotnet run --project Api
dotnet run --project Cli -- admin create admin --password password --permissions super_admin
.\add-migration.ps1 MigrationName
```

For UI:

```powershell
Set-Location Dashboard
npm run dev
npm run typecheck
npm run lint
npm run build
```

## Configuration

- API reads normal ASP.NET Core configuration from packaged `appsettings*.json`, then runtime `PathProvider.Paths.JsonConfig` (`config.json` in the shared project directory) and `PathProvider.Paths.EnvConfig`.
- CLI sets base path to `AppContext.BaseDirectory`, reads packaged `appsettings.json`, environment-specific `appsettings.{Environment}.json`, runtime `PathProvider.Paths.JsonConfig`, `PathProvider.Paths.EnvConfig`, and environment variables through the shared configuration pipeline.
- `PathProvider` in `XRayne.Contracts.Values` centralizes runtime paths: project root, `.env`, `config.json`, `docker-compose.yml`, `logs`, `postgres`, `downloads`, `certificates`, `certificates/letsencrypt`, and `xray`.
- `PathProvider.SystemProjectDirectory` defaults to `C:\Program Files\xrayne` on Windows, `/opt/xrayne` on Linux, and a temp `xrayne` directory elsewhere. `PROJECT_PATH` overrides the runtime root used by `PathProvider.Paths`.
- When CLI is installed under a `cli` folder, `PathProvider.GetProjectDirectory()` returns the parent directory of `AppContext.BaseDirectory`; for example `/opt/xrayne/cli` resolves to `/opt/xrayne`.
- `JsonConfig` and `EnvConfig` in `XRayne.Repositories.Utilities` are static helpers for safe runtime file mutations. Reading is done through standard `IConfiguration`.
- `.env` is static after install and reserved for Docker Compose/bootstrap variables such as `PROJECT_PATH`, `API_IMAGE`, `API_PORT`, and PostgreSQL values. More complex application configuration belongs in runtime `config.json`. Docker Compose runs from the project directory and reads the `.env` file beside `docker-compose.yml`; services should use `env_file: .env` when they need the same values inside containers.
- `XrayOptions` is registered from `Xray` through `XRayne.Contracts.DependencyInjection`; current options include `CorePath`.
- Database connection key is `ConnectionStrings:Default`.
- Repository entities currently include admin accounts, users, inbounds, and outbounds. Xray native inbound/outbound payloads are stored as `jsonb`; PostgreSQL enum mapping is configured for user status, traffic limit reset strategy, and admin permissions.
- Shared query models such as cursor pagination and repository filters live under `Contracts/Models`.

## CI And Packaging

- Release artifact workflow restores and publishes `Cli/Cli.csproj` for each RID.
- The same workflow builds the API+UI Docker image as `xrayne-api-image-<version>`, saves it as `xrayne-api-image-<version>.tar.gz`, uploads it as a build artifact, and attaches it to tagged releases.
- Release upload happens only for tag refs.
