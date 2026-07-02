# Skill: .NET Core Backend

Use this skill when working on `XRayne.Api`, `XRayne.Cli`, `XRayne.Contracts`,
`XRayne.Infrastructure`, `XRayne.Repositories`, or `XRayne.Test`.

## Start Here

1. Read [Backend architecture](../architecture/backend.md).
2. Read [.NET styleguide](../styleguide/dotnet.md).
3. Read [API conventions](../conventions/api.md) for controller or endpoint work.
4. Read [Feature conventions](../conventions/features.md) for new vertical work.

## Placement Rules

- Put API controllers, request/response models, filters, auth policies, and mapping
  profiles in `XRayne.Api`.
- Put managed-node API endpoints in `XRayne.Api` and node provisioning,
  reconnect, status, and verification behavior in `XRayne.Infrastructure`.
- Put shared options, enums, permission names, pagination/filter models, and
  path values in `XRayne.Contracts`.
- Put xray-core lifecycle, background jobs, state machines, system info, JWT,
  certificate, settings, and runtime services in `XRayne.Infrastructure`.
- Put EF entities, DbContext, migrations, repositories, config-file utilities, and
  external release/API clients in `XRayne.Repositories`.
- Put CLI commands in `XRayne.Cli/Commands`; put CLI orchestration services in
  `XRayne.Cli/Services`.

## Required Backend Rules

- Add XML `<summary>` documentation in English to all public classes, methods,
  services, DTOs, handlers, controllers, and endpoints.
- Describe every API endpoint for Scalar/OpenAPI with clear summary, description,
  request model, response model, and possible status codes.
- Keep controllers thin. They should authorize, bind inputs, choose status codes,
  and delegate business work.
- Do not mix business logic into controllers, filters, middleware, or DTOs.
- Do not create generic helper classes without a clear architectural home.
- Register services in the nearest `DependencyInjection.cs`.
- Use async EF Core APIs with `CancellationToken`.
- Throw intended `ApiException` subclasses from API flows so
  `ApiExceptionFilter` formats responses consistently.

## Validation

Prefer focused checks for the touched area:

```powershell
dotnet restore XRayne.sln
dotnet build XRayne.sln
dotnet test XRayne.sln
dotnet run --project XRayne.Api
dotnet run --project XRayne.Cli -- --help
```

For EF changes:

```powershell
.\add-migration.ps1 MigrationName
dotnet ef database update --project XRayne.Repositories --startup-project XRayne.Api --context AppDbContext
```
