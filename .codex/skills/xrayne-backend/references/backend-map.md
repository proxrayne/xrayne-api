# XRayne Backend Map

## Projects

Canonical backend documentation lives in `docs/architecture/backend.md`, `docs/styleguide/dotnet.md`, and `docs/conventions/api.md`.

- `Api`: ASP.NET Core API, OpenAPI/Scalar, JWT auth, CORS, static files, SPA fallback, exception filtering.
- `Cli`: System.CommandLine executable named `xrayne`, single-file publish support.
- `Github`: reusable GitHub.com releases/assets client used by CLI, API, and infrastructure code.
- `Infrastructure`: xray-core services, background jobs, infrastructure services, and runtime abstractions.
- `Infrastructure`: JWT token creation through `IJwtTokenService` plus infrastructure utilities such as network address helpers and password hashing/generation.
- Shared random password generation lives in `Infrastructure/Utilities/PasswordGenerator.cs`.
- `Repositories`: EF Core `AppDbContext`, PostgreSQL connection, migrations, entity models, repositories, and runtime config file utilities.
- `Contracts`: shared contracts, configuration DTOs/options, permission enums, permission names, runtime path helpers, and contract-level DI registration.
- `Test`: tests.

## API Startup Pattern

`Api/Program.cs`:

- Creates bootstrap Serilog logger.
- Reads `Docs` to decide OpenAPI/Scalar registration.
- Reads `Cors:SpaOrigins` and registers `SpaClient`.
- Adds controllers with `ApiExceptionFilter`.
- Adds AutoMapper from API assembly.
- Configures JWT Bearer with `JwtOptions` from `XRayne.Contracts.Configurations`.
- Adds authorization policies via `AddAdminPermissionPolicies`.
- Calls `AddInfrastructure`, `AddRepositories`, and `AddContracts`.
- Calls `MigrateDatabaseAsync()` on startup.
- Serves default/static files and maps SPA fallback for non-API paths.

## Controllers

- Base route style: `[Route("api/...")]`.
- Use `[EndpointSummary]`, `[EndpointDescription]`, and `[ProducesResponseType]` annotations for API docs.
- Add English XML `<summary>` documentation to public controllers and endpoint methods.
- Keep request/response models and possible status codes explicit for Scalar/OpenAPI.
- Auth endpoints:
  - `GET api/auth/me`: authorized current admin lookup.
  - `POST api/auth/login`: anonymous login, returns `LoginResponse`.
- Admin endpoints require `[Authorize(Policy = AdminPermissionNames.SuperAdmin)]`.
- Core endpoints live under `api/core`, require `change_xray_settings`, and currently expose:
  - `GET api/core/status`: installation/running/version state.
  - `GET api/core/releases`: paged xray-core release lookup with optional cache bypass.
- Node endpoints live under `api/nodes`; they are part of the panel API and use
  the panel JWT/admin permission model.

## Errors

`ApiExceptionFilter` maps:

- `ApiException` subclasses to `ApiErrorResponse(status, name, detail)`.
- Other exceptions to status 400 with the exception message.

Prefer existing exception classes:

- `UnauthorizedException`
- `ForbiddenException`
- `NotFoundException`
- `ConflictException`

## Permissions

`AdminPermission` is a `[Flags]` enum using `long` and lives in `XRayne.Contracts.Enums`.

String policy names live in `XRayne.Contracts.Values.AdminPermissionNames`:

- `create_users`
- `edit_users`
- `delete_users`
- `reset_traffic`
- `change_xray_settings`
- `view_logs`
- `manage_admins`
- `super_admin`

`SuperAdmin` satisfies every policy through `AdminPermissionPolicies`.

## Persistence

`AppDbContext` currently exposes `AdminAccounts`, `Users`, `Inbounds`,
`Outbounds`, and `Nodes`.

Common entity base classes live in `Repositories/Entities/BaseEntities.cs`:

- `CreatedEntity`: provides `CreatedAt`.
- `CreateUpdateEntity`: extends `CreatedEntity` with nullable `UpdatedAt`.

`AppDbContext.OnModelCreating` applies `CURRENT_TIMESTAMP` as the database default for every entity property named `CreatedAt`. Avoid repeating per-entity `CreatedAt` defaults unless a table needs a different behavior.

`AdminAccount` maps to `admin_accounts`, with:

- `Id` as `Guid`
- unique `Username`, max length 128
- `PasswordHash`, max length 512
- `Permissions`
- `CreatedAt`
- `LastLoginAt`

`User` maps to `users`, with a unique `Username`, nullable lifecycle timestamps, `UserStatus`, nullable `LimitResetStrategy`, many-to-many `Inbounds`, owning `AdminAccount`, and protocol-specific `Options` stored as `jsonb`.

`InboundEntity` maps to `inbounds`, stores the native Xray `Inbound` model as `jsonb`, has `Enabled` defaulting to SQL `TRUE`, many-to-many `Users`, owning `AdminAccount`, and not-mapped computed accessors such as `Tag`, `Listen`, `Protocol`, `Network`, `Security`, `Sniffing`, and `Port`.

`OutboundEntity` maps to `outbounds`, stores the native Xray `Outbound` model as `jsonb`, belongs to an `AdminAccount`, and has not-mapped computed accessors such as `Tag`, `Protocol`, `Network`, and `Security`.

`Node` maps managed remote-node records for the panel. Node provisioning,
reconnect policy, connection verification, protected API secrets, and SSE
installation/connection state live in `Infrastructure`; do not plan these
features in a local standalone node-service project.

PostgreSQL enum mapping is configured for `UserStatus`, `LimitResetStrategy`, and `AdminPermission` both in `AppDbContext.OnModelCreating` through `HasPostgresEnum<T>()` and in repository DI through `ConfigureDataSource(...).MapEnum<T>()`. EF conventions also convert enum properties to strings.

Xray native config payloads use Npgsql dynamic JSON with camelCase `System.Text.Json` options configured in `XRayne.Repositories.DependencyInjection`.

`AddRepositories` accepts a resolved PostgreSQL connection string and throws when it is empty. API passes `ConnectionStrings:Default` from `IConfiguration`; CLI resolves flat `.env`/environment keys such as `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`, `POSTGRES_HOST` or `POSTGRES_HOST_API`, and port values first, then falls back to `ConnectionStrings:Default`. The repository layer creates `NpgsqlDataSource`, configures EF with `UseNpgsql`, and registers `IAdminAccountRepository`.

GitHub.com release and asset access lives in the root `Github` class library. Keep persistence-specific code in `Repositories`; do not add external API clients there.

Repository pattern:

- Define repository interfaces under `Repositories/Contracts`.
- Implement repository classes under `Repositories/Implementations`.
- Use `SingleOrDefaultAsync`, `AnyAsync`, `SaveChangesAsync`, and pass cancellation tokens.
- Repositories for admin-owned entities filter by `AdminId`. Current entities expose `Admin` navigation but not explicit `AdminId`, so repository queries use `EF.Property<Guid>(entity, "AdminId")`.
- Current repositories registered by `AddRepositories`: `IAdminAccountRepository`, `IUserRepository`, `IInboundRepository`, `IOutboundRepository`, and node-related repositories.
- `AddAsync` methods return the saved entity after `SaveChangesAsync` and `ReloadAsync`, so database-generated values are available to callers.
- Shared query/pagination models live in `Contracts/Models`: `CursorQuery`, `CursorPage<T>`, `SortOrder`, plus one filter file per searchable entity such as `UserFilter` and `InboundFilter`. The static cursor helper lives in `Contracts/Utilities/CursorPagination`. Outbound repositories intentionally expose only direct list/CRUD methods, without filtering or cursor pagination.
- New entity repositories expose both admin-scoped methods and unscoped variants for service/internal use.

## CLI Pattern

`Cli/Program.cs`:

- Builds a generic host.
- Uses packaged `appsettings.json` and `appsettings.{Environment}.json` from `AppContext.BaseDirectory` plus runtime `PathProvider.Paths.JsonConfig`.
- Reads `PathProvider.Paths.EnvConfig` through `AddEnvFile(...)` when the runtime API is installed. Reading is done through standard `IConfiguration`; `JsonConfig` and `EnvConfig` in `XRayne.Repositories.Utilities` are only for safe runtime file mutations. Docker Compose runs from the project directory, reads the `.env` beside `docker-compose.yml`, and services use `env_file: .env` when container runtime values are needed.
- `PathProvider` lives in `XRayne.Contracts.Values`. `PathProvider.Paths` uses `PROJECT_PATH` when present, otherwise the OS-specific system project directory. `PathProvider.GetProjectDirectory()` can derive the parent project path from an installed `cli` folder, so `/opt/xrayne/cli` maps to `/opt/xrayne`.
- Adds environment variables without a custom prefix.
- Registers core, infrastructure, repositories, contracts, and CLI actions.
- Does not migrate database on startup, so non-database commands can run before PostgreSQL is available.
- Resolves `RootCommandFactory`, creates `CommandLineConfiguration`, and invokes args.

Command pattern:

- Root command composes feature commands.
- Feature command derives from `Command` and adds subcommands.
- Action command injects `IServiceProvider`, creates `CreateAsyncScope()`, resolves services, writes through `ICliConsole`, logs failures, returns `0` or `1`.

Current command tree:

```text
xrayne
  version
  update [--version latest|tag] [--component all|api|cli] [--force]
  info
  api install [--version latest|tag]
  api version
  api status
  api stop
  api start
  api restart
  cert install [--domain domain | --ip-address ipv4] --email email [--staging] [--force]
  cert status
  cert renew [--force]
  admin create
  xray start
```

Database-dependent commands should call `MigrateDatabaseAsync()` inside their action before using repositories. This keeps commands such as `--help`, Docker/compose management, and xray-core lifecycle commands usable when the database container is not running yet.

`admin create` prompts interactively for username, password confirmation, and permissions instead of accepting credentials through command-line arguments. Leaving password empty generates one and the command prints the created account details.

`api install` downloads API image release assets from the public `VanyaKrotov/xrayne` GitHub repository, loads the image with Docker, writes `.env`, runtime `config.json`, and `docker-compose.yml`, then starts `docker compose up -d`. The source repository is `xrayne-panel`, but public install/update artifacts intentionally remain under `VanyaKrotov/xrayne`. Installation must not require the database to be running before installation. The API compose service uses `network_mode: host` for host-level xray-core networking, so `API_PORT` is the real host port Kestrel listens on; do not add API `ports:` mappings.

Use `EnvConfig` for reading, writing, setting, or removing `.env` values. Do not hand-edit `.env` with command-local line parsing.

`update` resolves the target runtime schema from the selected release through `RuntimeSchemaCatalog`, runs `IRuntimeMigrationService.MigrateToAsync(...)` before replacing the CLI, and supports both `UpAsync` and `DownAsync` migrations so explicit downgrades can roll runtime files back. Runtime migration backups go under `<project>/backups/runtime-migrations`.

CLI service interfaces live under `Cli/Services/Contracts`, with implementations under `Cli/Services`.

Shared CLI helpers live under `Cli/Helpers`; certificate path/config helpers are in `CertificateCommandHelper`.

GitHub release and asset access is implemented by `GitHubRepository` in the `Github` project. CLI commands currently create it with `CliDefaults.XRayneRepositoryUrl`; xray-core release listing uses a `GitHubRepository` targeting `https://github.com/xtls/xray-core`. Do not reintroduce `GitHubReleaseService` under `Cli/Services`.

Docker Compose generation and edits live in `IDockerComposeFileService`/`DockerComposeFileService` and use YamlDotNet; do not build or mutate compose YAML with raw multiline strings or ad hoc text replacement.

`cert install` uses project-local `acme.sh` under `<project>/certificates/acme-sh`, installs API certificate files under `<project>/certificates/letsencrypt`, writes Kestrel HTTPS certificate paths to runtime `config.json`, keeps the API on configured `API_PORT`, enables `acme.sh` cron renewal, and recreates the API container. Domain certificates use normal Let's Encrypt issuance. IP address certificates use public IPv4 only and require the Let's Encrypt `shortlived` certificate profile.

## Xray Core

`AddInfrastructure` registers:

- `ICoreService` -> `CoreService`
- `ICoreStateMachine` -> `CoreStateMachine`
- `InstallCoreJob`

`CoreService` currently tracks an optional `IXrayProcessCore` instance and exposes installed/running/version checks. Core installation runs through Quartz `InstallCoreJob` and `IBackgroundTaskScheduler`.

## Migrations

Use:

```powershell
.\add-migration.ps1 MigrationName
```

Equivalent command:

```powershell
dotnet ef migrations add MigrationName --project Repositories --startup-project Api --context AppDbContext --output-dir Migrations
```

Apply migrations:

```powershell
dotnet ef database update --project Repositories --startup-project Api --context AppDbContext
```
