# API Conventions

## Routing

- Use `api/<resource>` route prefixes.
- Use nouns for resources and verbs only when the operation is not standard CRUD:
  `api/core/start`, `api/settings/panel/restart`.
- Keep route names stable; update this document and frontend API clients when
  routes change.
- Use offset-paginated response fields `items`, `totalItems`, `currentPage`,
  and `totalPages` for list endpoints that need page-number navigation.
- Use plural resource names for user management endpoints, for example
  `api/users`.

## Controllers

- Controllers should be thin:
  authorization, binding, request validation, service/repository call,
  response mapping.
- Do not put EF queries, file writes, process control, Docker logic, or command
  orchestration directly in controllers.
- Use cancellation tokens on async actions.
- Use request and response models for public payloads.

## Validation

- Prefer typed request models over unstructured payloads.
- Validate simple boundary requirements with data annotations, model binding, or
  explicit controller checks when local to the endpoint.
- Move reusable or multi-step validation into a named validator/service in the
  feature's owning layer.
- Return intended validation failures through consistent API errors and documented
  status codes.
- Do not hide validation inside repositories.
- Public API enum string values are declared explicitly with
  `JsonStringEnumMemberName` and are used in JSON and query parameters. Numeric
  enum values are accepted on input.

## Scalar / OpenAPI

Every endpoint must include:

- XML summary in English on public endpoint method/class;
- `EndpointSummary`;
- `EndpointDescription`;
- request model type where applicable;
- response model type;
- all expected status codes through `ProducesResponseType`;
- auth/security scheme when needed.

When API behavior changes, update:

- controller annotations;
- request/response DTOs;
- frontend `api.types.ts`;
- `/docs/conventions/api.md` or architecture docs if the convention changes.

## Authentication And Authorization

- Panel API uses JWT bearer authentication.
- Admin-only endpoints use policies from `AdminPermissionNames`.
- `SuperAdmin` satisfies every admin permission policy.
- Event streams may receive JWT through `access_token` query string for SSE.
- Node-management endpoints are panel API endpoints and use the same JWT and
  admin permission policy model as the rest of the panel.
- Panel-to-node calls use gRPC with the remote node API key sent as
  `X-Node-Api-Key` metadata.

## Middleware And Filters

- Use middleware for cross-cutting request pipeline behavior.
- Use MVC filters for controller-specific cross-cutting behavior.
- Register middleware and filters at the composition root.
- Keep endpoint-specific behavior in controllers/services instead of global
  middleware.

## Errors

- Intended panel API failures should throw `ApiException` subclasses.
- `ApiExceptionFilter` maps these into `ApiErrorResponse`.
- Unknown exceptions are currently mapped by the filter; prefer adding intended
  exception types for expected failures.
- Frontend API failures should be normalized through `ResponseError`.

## Status Codes

- `200 OK`: successful read/update with response body.
- `201 Created`: new resource created.
- `202 Accepted`: scheduled or asynchronous operation accepted.
- `204 No Content`: successful delete or update without response body.
- `400 Bad Request`: invalid request or operation failure.
- `401 Unauthorized`: missing/invalid credentials.
- `403 Forbidden`: authenticated but not permitted.
- `404 Not Found`: missing resource.
- `409 Conflict`: uniqueness or state conflict.

## Streaming

Panel SSE endpoints should:

- use `text/event-stream`;
- send initial state before streaming updates;
- clean up subscriptions in `finally`;
- accept cancellation tokens.
- read remote node runtime updates from panel-local stores and event streams
  when the data is already ingested by `IRemoteNodeConnectionManager`.

Panel-to-node streaming uses gRPC server-streaming from `ProtoTypes/remote_node.proto`
instead of node HTTP/SSE endpoints. Do not open one remote node gRPC stream per
dashboard SSE client; keep one upstream `Connect` stream per active node and
fan out locally.
