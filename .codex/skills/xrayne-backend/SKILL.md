---
name: xrayne-backend
description: Backend development guidance for XRayne.Node. Use when Codex works on C#/.NET API controllers, CLI commands, dependency injection, auth and permissions, EF Core repositories and migrations, PostgreSQL configuration, xray-core services, config files, or backend tests in XRayne.Api, XRayne.Cli, XRayne.Core, XRayne.Infrastructure, XRayne.Repositories, XRayne.Contracts, or XRayne.Test.
---

# XRayne Backend

## Quick Start

Read `references/backend-map.md` before backend edits. Use it to place code in the right project and choose the existing pattern.

## Implementation Rules

- Keep domain constants and simple domain types in `XRayne.Core`.
- Register services through the nearest `DependencyInjection.cs` extension.
- Put HTTP behavior in controllers under `XRayne.Api/Controllers`; keep controllers thin and delegate work to repositories/services.
- Throw `ApiException` subclasses for intended API errors so `ApiExceptionFilter` can format them.
- Use `AdminPermissionNames` strings for authorization policies and permission parsing.
- Use EF Core async methods with cancellation tokens in repositories.
- Add migrations through `add-migration.ps1` or equivalent `dotnet ef migrations add` command targeting `AppDbContext`.
- For CLI commands, derive from `System.CommandLine.Command`, inject `IServiceProvider`, create an async scope in `SetAction`, and return integer exit codes.
- Read configuration through standard `IConfiguration`; use `IJsonConfigService`/`JsonConfigService` only when mutating runtime `config.json`.
- API/UI Docker image artifacts are produced by release GitHub Actions; backend code should keep the image build reproducible and avoid local-only assumptions.

## Validation

Prefer focused validation:

```powershell
dotnet build XRayne.sln
dotnet test XRayne.sln
dotnet run --project XRayne.Api
dotnet run --project XRayne.Cli -- --help
```

If touching EF:

```powershell
dotnet ef database update --project XRayne.Repositories --startup-project XRayne.Api --context AppDbContext
```
