---
name: proxrayne-panel-backend
description: Backend development guidance for the Proxrayne xrayne-panel repository. Use when Codex works on C#/.NET API controllers, CLI commands, dependency injection, auth and permissions, EF Core repositories and migrations, PostgreSQL configuration, xray-core services, managed-node services, system information, config files, or backend tests in Api, Cli, Infrastructure, Repositories, Contracts, System, or Test.
---

# XRayne Panel Backend

## Quick Start

Read `references/backend-map.md`, `docs/architecture/backend.md`, `docs/styleguide/dotnet.md`, and `docs/conventions/api.md` before backend edits. Use `$proxrayne-project` first when the change affects release/install behavior or another Proxrayne repository.

## Implementation Rules

- Keep shared DTOs, configuration contracts, permissions, and API-facing constants in `Contracts`.
- Add English XML `<summary>` documentation to public classes, methods, services, DTOs, handlers, controllers, and endpoints.
- Keep Scalar/OpenAPI metadata complete with endpoint summary, description, request/response models, and status codes.
- Keep xray-core setup, runtime services, and background jobs in `Infrastructure`.
- Keep managed-node provisioning, reconnect, status, and verification services in `Infrastructure`; node HTTP endpoints stay in `Api`.
- Keep EF entity models, base entity classes, migrations, and repository implementations in `Repositories`.
- Register services through the nearest `DependencyInjection.cs` extension.
- Put HTTP behavior in controllers under `Api/Controllers`; keep controllers thin and delegate work to repositories/services.
- Throw `ApiException` subclasses for intended API errors so `ApiExceptionFilter` can format them.
- Use `AdminPermissionNames` from `Contracts.Values` for authorization policies and permission parsing.
- Use EF Core async methods with cancellation tokens in repositories.
- Add migrations through `add-migration.ps1` or equivalent `dotnet ef migrations add` command targeting `AppDbContext`.
- For CLI commands, derive from `System.CommandLine.Command`, inject `IServiceProvider`, create an async scope in `SetAction`, and return integer exit codes.
- Read configuration through standard `IConfiguration`; use `JsonConfig` only when mutating runtime `config.json`, and `EnvConfig` only when mutating `.env`.
- API/UI Docker image artifacts are produced by release GitHub Actions; backend code should keep the image build reproducible and avoid local-only assumptions.
- Update `docs/` when API behavior, backend architecture, configuration, or packaging changes.

## Validation

Prefer focused validation:

```powershell
dotnet build XRayne.sln
dotnet test XRayne.sln
dotnet run --project Api
dotnet run --project Cli -- --help
```

If touching EF:

```powershell
dotnet ef database update --project Repositories --startup-project Api --context AppDbContext
```
