# XRayne Backend Map

## Projects

- `XRayne.Api`: ASP.NET Core API, OpenAPI/Scalar, JWT auth, CORS, static files, SPA fallback, exception filtering.
- `XRayne.Cli`: System.CommandLine executable named `xrayne`, single-file publish support.
- `XRayne.Core`: domain permissions and xray-core selection.
- `XRayne.Infrastructure`: JWT token creation, password hashing, `ICoreService` implementation.
- `XRayne.Repositories`: EF Core `AppDbContext`, PostgreSQL connection, migrations, repositories.
- `XRayne.Contracts`: shared contracts placeholder.
- `XRayne.Test`: tests.

## API Startup Pattern

`XRayne.Api/Program.cs`:

- Creates bootstrap Serilog logger.
- Reads `Docs` to decide OpenAPI/Scalar registration.
- Reads `Cors:SpaOrigins` and registers `SpaClient`.
- Adds controllers with `ApiExceptionFilter`.
- Adds AutoMapper from API assembly.
- Configures JWT Bearer with `JwtOptions`.
- Adds authorization policies via `AddAdminPermissionPolicies`.
- Calls `AddCoreDependencies`, `AddInfrastructure`, and `AddRepositories`.
- Calls `MigrateDatabaseAsync()` on startup.
- Serves default/static files and maps SPA fallback for non-API paths.

## Controllers

- Base route style: `[Route("api/...")]`.
- Use `[EndpointSummary]`, `[EndpointDescription]`, and `[ProducesResponseType]` annotations for API docs.
- Auth endpoints:
  - `GET api/auth/me`: authorized current admin lookup.
  - `POST api/auth/login`: anonymous login, returns `LoginResponse`.
- Admin endpoints require `[Authorize(Policy = AdminPermissionNames.SuperAdmin)]`.
- Core endpoint currently `POST api/core/start`, requires `change_xray_settings`.

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

`AdminPermission` is a `[Flags]` enum using `long`.

String policy names live in `AdminPermissionNames`:

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

`AppDbContext` currently exposes `AdminAccounts`.

`AdminAccount` maps to `admin_accounts`, with:

- `Id` as `Guid`
- unique `Username`, max length 128
- `PasswordHash`, max length 512
- `Permissions`
- `CreatedAt`
- `LastLoginAt`

`AddRepositories` requires `ConnectionStrings:Default`, creates `NpgsqlDataSource`, configures EF with `UseNpgsql`, and registers `IAdminAccountRepository`.

Repository pattern:

- Define interface under the feature folder, for example `XRayne.Repositories/Admins/IAdminAccountRepository.cs`.
- Implement async methods in the same feature folder.
- Use `SingleOrDefaultAsync`, `AnyAsync`, `SaveChangesAsync`, and pass cancellation tokens.

## CLI Pattern

`XRayne.Cli/Program.cs`:

- Builds a generic host.
- Uses appsettings from `AppContext.BaseDirectory`.
- Adds environment variables with prefix `XRAYNE_`.
- Registers core, infrastructure, repositories, and CLI actions.
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
  admin create <username> --password|-p <password> [--permissions]
  xray start
```

Database-dependent commands should call `MigrateDatabaseAsync()` inside their action before using repositories. This keeps commands such as `--help`, Docker/compose management, and xray-core lifecycle commands usable when the database container is not running yet.

## Xray Core

`AddCoreDependencies` reads `XrayInstanceConfig`.

- If `UseProcessCore` is true, register `XrayProcessCore` using `WorkingDirectory`.
- Otherwise register `XrayLibCore` using `Path.Combine(Directory, FileName)`.

`CoreService` currently logs xray version on start and stubs stop/restart behavior.

## Migrations

Use:

```powershell
.\add-migration.ps1 MigrationName
```

Equivalent command:

```powershell
dotnet ef migrations add MigrationName --project XRayne.Repositories --startup-project XRayne.Api --context AppDbContext --output-dir Migrations
```

Apply migrations:

```powershell
dotnet ef database update --project XRayne.Repositories --startup-project XRayne.Api --context AppDbContext
```
