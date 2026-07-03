# XRayne Panel

`xrayne-panel` owns the XRayne ASP.NET Core API and backend runtime services.
The administrator CLI lives in `xrayne-cli`, and the standalone web UI lives in
`xrayne-ui`.

## Repository Layout

- `Api`: REST API, authentication, controllers, filters, mapping, and API models.
- `Contracts`: shared options, enums, DTO-like models, values, and utilities.
- `Infrastructure`: xray-core runtime services, background jobs, managed-node orchestration, and cross-cutting services.
- `Repositories`: EF Core DbContext, migrations, entities, repositories, and runtime config-file utilities.
- `System`: host system information services.
- `Github`: GitHub release/assets client.
- `Test`: backend unit and integration tests.

## Build

```powershell
dotnet restore XRayne.sln
dotnet build XRayne.sln
dotnet test XRayne.sln
dotnet run --project Api
docker build -f Api/Dockerfile -t xrayne-api-image:local .
```

The release workflow publishes the API-only image archive
`xrayne-api-image-<version>.tar.gz`. The UI image is built and published by
`xrayne-ui`.
