# XRayne Backend Map

## Projects

Canonical backend documentation lives in `docs/architecture/backend.md`, `docs/styleguide/dotnet.md`, and `docs/conventions/api.md`.

- `Api`: ASP.NET Core API, OpenAPI/Scalar, JWT auth, CORS, static files, SPA fallback, exception filtering.
- `Github`: reusable GitHub.com releases/assets client used by API and infrastructure code.
- `Infrastructure`: xray-core services, background jobs, infrastructure services, and runtime abstractions.
- `Infrastructure`: JWT token creation through `IJwtTokenService` plus infrastructure utilities such as network address helpers and password hashing/generation.
- Shared random password generation lives in `Infrastructure/Utilities/PasswordGenerator.cs`.
- `Data`: EF Core `AppDbContext`, PostgreSQL connection, migrations, entity models, repositories, and runtime config file utilities.
- `Contracts`: shared contracts, configuration DTOs/options, permission enums, permission names, runtime path helpers, and contract-level DI registration.
- `Test`: tests.

## API Startup Pattern

`Api/Program.cs`:

- Creates bootstrap Serilog logger.
- Reads `Docs` to decide OpenAPI/Scalar registration.
- Reads `Cors:SpaOrigins` and registers `SpaClient`.
- Adds controllers with `ApiExceptionFilter`.
- Adds AutoMapper from API assembly.
- Configures JWT Bearer with `JwtOptions` from `Contracts.Configurations`.
- Adds authorization policies via `AddAdminPermissionPolicies`.
- Calls `AddInfrastructure`, `AddData`, and `AddContracts`.
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

`AdminPermission` is a `[Flags]` enum using `long` and lives in `Contracts.Enums`.

String policy names live in `Contracts.Values.AdminPermissionNames`:

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

Common entity base classes live in `Data/Entities/BaseEntities.cs`:

- `CreatedEntity`: provides `CreatedAt`.
- `CreateUpdateEntity`: extends `CreatedEntity` with nullable `UpdatedAt`.

`AppDbContext.OnModelCreating` applies `CURRENT_TIMESTAMP` as the database default for every entity property named `CreatedAt`. Avoid repeating per-entity `CreatedAt` defaults unless a table needs a different behavior.

`AdminAccount` maps to `admin_accounts`, with:

- `Id` as database-generated `long`
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

Xray native config payloads use Npgsql dynamic JSON with camelCase `System.Text.Json` options configured in `Data.DependencyInjection`.

`AddData` accepts a resolved PostgreSQL connection string and throws when it is empty. API passes `ConnectionStrings:Default` from `IConfiguration`. The repository layer creates `NpgsqlDataSource`, configures EF with `UseNpgsql`, and registers `IAdminAccountRepository`.

GitHub.com release and asset access lives in the root `Github` class library. Keep persistence-specific code in `Data`; do not add external API clients there.

Host system information DTOs live in `Contracts.Models`. `Infrastructure` registers `ISystemInfoService` with panel runtime paths from `PathProvider` and reads host data through the `Hardware.Info` NuGet package.

Repository pattern:

- Define repository interfaces under `Data/Contracts`.
- Implement repository classes under `Data/Implementations`.
- Use `SingleOrDefaultAsync`, `AnyAsync`, `SaveChangesAsync`, and pass cancellation tokens.
- Data for admin-owned entities filter by explicit `long AdminId` foreign-key properties.
- Current repositories registered by `AddData`: `IAdminAccountRepository`, `IUserRepository`, `IInboundRepository`, `IOutboundRepository`, and node-related repositories.
- `AddAsync` methods return the saved entity after `SaveChangesAsync` and `ReloadAsync`, so database-generated values are available to callers.
- Shared query/pagination models live in `Contracts/Models`: `CursorQuery`, `CursorPage<T>`, `SortOrder`, plus one filter file per searchable entity such as `UserFilter` and `InboundFilter`. The static cursor helper lives in `Contracts/Utilities/CursorPagination`. Outbound repositories intentionally expose only direct list/CRUD methods, without filtering or cursor pagination.
- New entity repositories expose both admin-scoped methods and unscoped variants for service/internal use.

## CLI Split

The administrator CLI is owned by the standalone `xrayne-cli` repository. Keep
command definitions, runtime migrations, Docker Compose generation, certificate
installation, and CLI release/update behavior there.
## Xray Core

`AddInfrastructure` registers:

- `ICoreService` -> `CoreService`
- `ICoreStateMachine` -> `CoreStateMachine`
- `ISystemInfoService` -> `SystemInfoService`
- `InstallCoreJob`

`CoreService` currently tracks an optional `IXrayProcessCore` instance and exposes installed/running/version checks. Core installation runs through Quartz `InstallCoreJob` and `IBackgroundTaskScheduler`.

## Migrations

Use:

```powershell
.\add-migration.ps1 MigrationName
```

Equivalent command:

```powershell
dotnet ef migrations add MigrationName --project Data --startup-project Api --context AppDbContext --output-dir Migrations
```

Apply migrations:

```powershell
dotnet ef database update --project Data --startup-project Api --context AppDbContext
```
