# Skill: .NET Core Backend

Use this skill when working on `Api`, `Contracts`,
`Infrastructure`, `Repositories`, or `Test`.

## Start Here

1. Read [Backend architecture](../architecture/backend.md).
2. Read [.NET styleguide](../styleguide/dotnet.md).
3. Read [API conventions](../conventions/api.md) for controller or endpoint work.
4. Read [Feature conventions](../conventions/features.md) for new vertical work.

## Placement Rules

- Put API controllers, request/response models, filters, auth policies, and mapping
  profiles in `Api`.
- Put managed-node API endpoints in `Api` and node provisioning,
  reconnect, status, and verification behavior in `Infrastructure`.
- Put shared options, enums, permission names, pagination/filter models, and
  path values in `Contracts`.
- Put xray-core lifecycle, background jobs, state machines, JWT,
  certificate, settings, and runtime services in `Infrastructure`.
- Put reusable host system information DTOs in `Contracts` and runtime
  collection services in `Infrastructure`.
- Put EF entities, DbContext, migrations, repositories, config-file utilities, and
  persistence utilities in `Repositories`.

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
dotnet restore sln
dotnet build sln
dotnet test sln
dotnet run --project Api
```

For EF changes:

```powershell
.\add-migration.ps1 MigrationName
dotnet ef database update --project Repositories --startup-project Api --context AppDbContext
```
