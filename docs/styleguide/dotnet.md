# .NET Styleguide

## Language And Project Settings

- Target framework: `net9.0`.
- Nullable reference types are enabled.
- Implicit usings are enabled.
- Prefer file-scoped namespaces.
- Prefer primary constructors when they keep dependency injection concise.
- Use async APIs for I/O and database operations.

## Naming

- Public types: `PascalCase`.
- Interfaces: `IName`.
- Methods and properties: `PascalCase`.
- Local variables and parameters: `camelCase`.
- Private fields: use the existing style in the file. Current code has both
  dependency primary constructors and some `_field` private members.
- Controller names end with `Controller`.
- Request models end with `Request`; response DTOs end with `Response` or `Dto`.
- EF entities use domain names, with `Entity` suffix when needed to avoid a type
  collision with xray SDK models.
- Repository interfaces end with `Repository`; service interfaces end with
  `Service`.

## XML Documentation

All new or modified public classes, methods, services, DTOs, handlers,
controllers, and endpoints must have XML `<summary>` comments in English.

Keep summaries factual and short. Add `<param>` and `<returns>` when they clarify
non-obvious behavior. Do not document private implementation details as public API.

## Layering

- API layer binds HTTP, validates request shape, authorizes, maps status codes,
  and delegates.
- Infrastructure layer owns runtime/business behavior and background jobs.
- Repository layer owns EF queries and persistence.
- Contracts layer owns shared values and data contracts.
- CLI layer owns command-line orchestration.
- Managed-node behavior belongs to the panel layers above; do not introduce a
  local standalone node-service project in this repository.

Do not add cross-layer shortcuts to make one feature easier. If a dependency feels
awkward, update the owning layer rather than bypassing it.

## Controllers And Endpoints

- Inherit from `ApiControllerBase` in `XRayne.Api`.
- Use `[Route("api/...")]`.
- Use authorization policies from `AdminPermissionNames`.
- Add `EndpointSummary`, `EndpointDescription`, and complete
  `ProducesResponseType` attributes.
- Return appropriate `IActionResult`, typed responses, `Created`, `Accepted`,
  `NoContent`, or domain DTOs.
- Use request/response models instead of anonymous HTTP payloads.
- Do not put EF queries, process lifecycle details, or file mutation logic in
  controllers.

## Services

- Define interfaces for services consumed across layers or by multiple callers.
- Put service interfaces under the nearest `Contracts` folder where that pattern
  exists.
- Register services in the nearest `DependencyInjection.cs`.
- Keep services responsible for one cohesive domain operation.
- Prefer domain-specific service names over generic helpers.

## Handlers, Validators, Middleware, And Filters

- Create handlers only for a concrete dispatching pattern. Do not introduce a
  `Handlers` folder as a generic dumping ground.
- Put validators beside the request/feature they validate when validation is
  feature-specific; move shared validation into a named infrastructure or
  contracts utility only when it is reused.
- Middleware is for cross-cutting HTTP pipeline behavior such as authentication,
  forwarded headers, request logging, or service-wide API key checks.
- Filters are for MVC/controller concerns such as API exception formatting.
- Keep business rules out of middleware and filters.
- Public handlers, validators, middleware, and filters must have English XML
  summaries when introduced or modified.

## Repositories

- Define interfaces in `XRayne.Repositories/Contracts`.
- Implement in `XRayne.Repositories/Implementations`.
- Use EF Core async methods with cancellation tokens.
- Use `SingleOrDefaultAsync`, `AnyAsync`, `ToListAsync`, `SaveChangesAsync`.
- Admin-owned data should expose both unscoped and admin-scoped methods when
  internal services need unscoped access.
- Use cursor pagination helpers from `XRayne.Contracts.Utilities` for searchable
  lists.

## Errors

- Use `ApiException` subclasses for intended API errors:
  `UnauthorizedException`, `ForbiddenException`, `NotFoundException`,
  `ConflictException`.
- Let `ApiExceptionFilter` format API errors.
- Log unexpected operational failures with `ILogger<T>` or Serilog request logs.

## Configuration And Files

- Read configuration through `IConfiguration` and options classes.
- Mutate runtime `config.json` with `JsonConfig`.
- Mutate `.env` with `EnvConfig`.
- Do not hand-edit JSON, env, or YAML with ad hoc string replacement when a
  structured utility exists.
