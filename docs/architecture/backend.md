# Backend Architecture

## Overview

The backend is a .NET 9 solution with a layered architecture:

- `Api`: panel REST API.
- `Contracts`: shared contracts and configuration values.
- `Node`: typed gRPC protocol client for standalone remote nodes.
- `Infrastructure`: runtime services, remote-node orchestration, jobs, and state.
- `Data`: EF Core persistence.
- `Test`: test project.

## API Host

`Api/Program.cs` is the composition root:

- builds Serilog bootstrap and runtime loggers;
- loads runtime `config.json` and `.env` through project utilities;
- configures Kestrel from panel settings outside Development;
- registers controllers with `ApiExceptionFilter`;
- configures OpenAPI/Scalar when `Docs` is enabled;
- configures JWT bearer auth and admin permission policies;
- registers infrastructure, data access, contracts, Quartz, and restart services;
- migrates the database on startup.

Controllers use `[Route("api/...")]`, inherit from `ApiControllerBase`, and use
`EndpointSummary`, `EndpointDescription`, and `ProducesResponseType`.

## Managed Nodes

Node management is part of the panel backend, not a separate local project.
Panel-owned node behavior includes:

- HTTP endpoints in `Api`;
- remote node gRPC protocol clients, DTOs, and typed protocol errors in
  `Node`;
- provisioning, reconnect, status streaming, and connection verification services
  in `Infrastructure`;
- node entities, encrypted connection data, and repository access in
  `Data`;
- shared node DTOs, enums, and configuration values in `Contracts`.

`Node` must not depend on `Contracts`, `Infrastructure`, `Data`,
or `Api`.
It is the only panel project that should know the remote node API wire shape.
It creates typed clients for the split `HealthService`, `CoreService`,
`RuntimeConfigService`, `LogService`, `GeoResourceService`, and
other panel-to-node gRPC services over one cached HTTP/2 cleartext channel per
remote node. XRayne does not issue or manage TLS certificates for the remote
node API; operators can terminate TLS in their own infrastructure if needed.
Do not add or reference a local standalone node-service project when
implementing node management features in this repository.

`Infrastructure` owns the live connection lifecycle through
`INodeConnectionManager`. The manager starts saved active node streams at
panel startup, exposes immediate connect/reconnect/disconnect operations for
controllers and services, keeps one worker per node, and persists heartbeat data
on a throttled interval instead of writing every heartbeat to the database.
Each worker is the only persistent upstream gRPC `HealthService.Connect`
subscriber for its node. It updates panel-local stores and dispatches local
event-bus updates for panel SSE clients, so UI tabs do not create additional
remote node streams. The worker treats stream idleness as a failed connection after
`NodeConnection:StreamIdleTimeoutSeconds`, or `3 * StreamHeartbeatSeconds + 5`
seconds when unset, and then uses the normal reconnect policy.
Live node and remote xray-core state is kept in process memory through
data-layer singleton stores. `NodeEntity` does not persist connection
status; it persists `Enabled` only as the durable operator-controlled flag
that allows or stops automatic reconnect attempts. `INodeConnectionStateStore`
is the source of truth for the live connection status, remote node API version,
and remote node uptime as a `DateTimeOffset` start time. `INodeCoreStateStore`
stores whether remote xray-core is installed and running, plus its version and
enum status, started-at timestamp, and uptime when the remote core is started.
The panel exposes connection state through
`GET /api/nodes/{id}/connection` so profile reads can show live node state
without calling remote `ping`.

The standalone remote node `Ping` response and `HealthService.Connect` heartbeat stream
payloads must include `NodeVersion`, `Environment`, `Uptime`, and `Core`.
The `HealthService.Connect` gRPC stream also emits `core_status`, `core_install`, and
`core_log` events from the node runtime state machine. Stream envelopes include
compatibility `type`, typed `eventType`, `sequence`, `droppedCount`, and
`source` metadata. The stream event
timestamp is the heartbeat time persisted by the panel. Host telemetry is
fetched separately from the node `HealthService.GetSystemStatus` RPC and proxied by the panel through
`/api/nodes/{id}/system/status`.
Remote node and xray-core versions are not persisted on `NodeEntity`; they are
runtime telemetry values and are repopulated after panel restart when node
streams reconnect.

Remote node logs are runtime-only and limited to xray-core output. The
standalone node exposes recent xray-core logs through `LogService.GetLogs` and
`LogService.StreamLogs`, and the panel's `INodeConnectionManager` ingests
batched `core_log` events from the existing `HealthService.Connect` gRPC stream. `INodeLogStore`
keeps bounded in-memory ring buffers keyed by node id. The panel exposes
dashboard reads through
`GET /api/nodes/{id}/logs` and live updates through
`GET /api/nodes/{id}/logs/stream`; both use the `view_logs` permission.

Remote node xray-core management is exposed through panel endpoints under
`/api/nodes/{id}/core`. The panel resolves the encrypted node API key, calls the
standalone node gRPC API through `Node`, and streams status/install events back
to the UI from panel-local event streams. Provisioning starts the node container, verifies the node API, then
installs the latest XTLS `xray-core` as the final remote setup step.
Node profile metadata is updated through `PUT /api/nodes/{id}`. Saved node
connection parameters are read through
`GET /api/nodes/{id}/connection-parameters` and updated through
`PUT /api/nodes/{id}/connection-parameters`; SSH secrets are write-only and are
not returned by the read endpoint. Updates that change live connection
parameters reset the node to `Connecting` and schedule a reconnect through the
connection manager. The panel proxies remote node service restarts through
`POST /api/nodes/{id}/restart`, which calls remote `RestartRuntime`
and starts reconnect monitoring.
Each node stores a `ConfigTemplate` value as PostgreSQL `jsonb`, while the
entity model exposes it as `XrayConfig`. The template is editable through
dedicated node core config-template endpoints where `configTemplate` is JSON
text in a string field, but it is not the final runtime config by itself. On
remote core start and restart, the panel clones the template, replaces
`inbounds`, `outbounds`, and `routing.rules` with the enabled node entities in
panel order, and sends one full native `XrayConfig` JSON payload to the
standalone node gRPC service.
Node inbounds are managed through node-scoped endpoints under
`/api/nodes/{id}/inbounds`; item routes use native inbound `tag` as the public
id. Manual inbounds can be created, edited, toggled, and deleted. Update routes
use the old tag, while the JSON payload may carry a new tag to rename the
record. Readonly inbounds are synchronized from `ConfigTemplate` by tag when the
template is saved; their JSON is updated or removed through the template, while
the UI can only enable or disable them. Tag uniqueness is enforced per node
across manual, disabled, and readonly records. Conflicting readonly template
inbounds are skipped instead of creating duplicate semantic ids.
Enabled inbound changes are mirrored to the standalone node runtime when cached
telemetry reports that remote xray-core is running. The live sync payload is the
native inbound JSON and the remote node identifies it by `tag`.
Node outbounds follow the same node-scoped model under
`/api/nodes/{id}/outbounds`; item routes use native outbound `tag` as the
public id. Manual outbounds can be created, edited, toggled, and deleted, while
readonly outbounds are synchronized from `ConfigTemplate` by tag and can only be
enabled or disabled through the UI. Update routes use the old tag, while the
JSON payload may carry a new tag to rename the record. Managed node outbounds
must have tags because tag identity is used for duplicate detection, readonly
template synchronization, and runtime replacement. Tag uniqueness is enforced per
node across manual, disabled, and readonly records; conflicting readonly
template outbounds are skipped instead of creating duplicate semantic ids.
Enabled outbound changes are mirrored to the standalone node runtime when cached
telemetry reports that remote xray-core is running. The live sync payload is the
native outbound JSON and the remote node identifies it by `tag`.
Node routing rules are managed through node-scoped endpoints under
`/api/nodes/{id}/routing-rules`; item routes and reorder payloads use native
Xray `ruleTag` as the public id. Manual rules can be created, edited, toggled,
deleted, and reordered, while readonly rules are synchronized from
`ConfigTemplate.Routing.Rules` in template order by `ruleTag` and can only be
enabled or disabled through the UI. When a created or updated rule omits
`ruleTag`, the panel generates a UUID before persistence. RuleTag uniqueness is
enforced per node across manual, disabled, and readonly records. Readonly rules
stay before manual rules.
Enabled routing rule changes are mirrored to the standalone node runtime by
replacing the full enabled ordered native `routing.rules` list when cached
telemetry reports that remote xray-core is running. The remote node identifies
rules by native `ruleTag`.
Node geo resources are managed through node-scoped endpoints under
`/api/nodes/{id}/geo-resources`. The panel stores metadata only in
`GeoResourceEntity`; remote files live on the standalone node in its managed
assets directory. Static resources can be uploaded, renamed, downloaded, and
deleted. Upload, URL refresh, and rename work is executed by one-off Quartz jobs
after the API has created or updated the metadata row. `GeoResourceEntity.Status`
and `StatusMessage` persist the current stage and transient process buffer; only
`success` resources are exposed as routing-rule file-name suggestions or
downloadable files. Auto-update resources store a download URL and Unix 5-field
cron template; the panel downloads the URL, uploads the bytes to the node,
advances `NextRunAt` only after success, and retries on the next ten-minute job
pass after failures. The panel syncs remote file metadata on first successful connect,
after successful core install events, and through the two-hour recurring sync
job. Manual file changes restart remote xray-core through the existing restart
flow when cached core state reports it is running.
Geo resource upload and download use gRPC streaming chunks so large files are
transferred without buffering one full protobuf message in the panel. Static
geo resource file uploads through the panel REST API are limited to 128 MB.

Connection warehouses are managed through `/api/warehouses`. Warehouses group
node inbounds for user connection distribution, expose list filtering by name,
enabled state, and assigned inbounds, and use offset pagination with a default
page size of ten. Warehouses can be created and updated with zero or more
assigned inbounds. Deletion is blocked while users are assigned to the
warehouse. The warehouse API requires the `manage_warehouses` administrator
permission, which is also satisfied by `super_admin`.

Subscription hosts are managed through `/api/hosts`. Hosts belong to the current
administrator, reference one managed inbound, and are returned as one complete
ordered list for drag-and-drop position management. Partial updates use Optional
fields through `PATCH /api/hosts/{id}`, including enabled-state toggles.
Reordering uses `PUT /api/hosts/order` with the full host id order. Host
management requires the `change_xray_settings` administrator permission, which
is also satisfied by `super_admin`.

Administrator accounts are managed through `/api/admin`. Active administrator
lists support username/email search and offset pagination with a default page
size of ten. Create, patch, password-change, permissions, and delete operations
require `manage_admins`, which is also satisfied by `super_admin`. Additional
guards prevent non-super administrators from assigning, editing, changing
passwords for, or deleting `super_admin` accounts.

Subscription users are managed through `/api/users`. User lists support
username search, multi-status filtering, offset pagination, newest-first default
sorting by creation date, and explicit sorting by username, status, configured
traffic limit, and connection count. Create, update, and delete operations use
the existing `create_users`, `edit_users`, and `delete_users` permissions. Users
must belong to a warehouse; new users do not create connection credentials
automatically, so the connection count starts at zero until a separate
connection flow adds records. User creation uses `onHoldDays` to create an
initial on-hold user and extends `ExpireAt` by the same duration when an
expiration date is provided. User reset strategy is stored only when `ExpireAt`
is configured.

## Contracts Layer

`Contracts` owns shared types used across backend projects:

- configuration options such as `JwtOptions`, `PanelSettings`, and node connection options;
- enums such as admin permissions, user status, and update
  target;
- query/filter models such as cursor pagination and entity filters;
- shared values such as `PathProvider` and `AdminPermissionNames`;
- contract-level DI registration.

Avoid dependencies from contracts back into API, repositories, or
infrastructure.

## Infrastructure Layer

`Infrastructure` owns runtime behavior:

- `BackgroundTaskScheduler`, node provisioning, reconnect, and runtime stores;
- Quartz jobs for remote-node provisioning and geo-resource processing;
- Octokit-backed GitHub release lookup helpers for remote node xray-core installs;
- JWT token creation, settings application, restart scheduling;
- geo resource, routing rule, and node services;
- `Hardware.Info`-backed host system information through `ISystemInfoService`.

Service interfaces live near implementations under `Services/Contracts`. Register
new services in `Infrastructure/DependencyInjection.cs`.

## Data Layer

`Data` owns persistence:

- `AppDbContext` and migrations;
- EF entities under `Entities`;
- repository interfaces under `Contracts`;
- repository implementations under `Implementations`;
- runtime config-file utilities under `Utilities`.

Data repositories expose async APIs, accept `CancellationToken`, and use admin-scoped
overloads when data belongs to an administrator.

## CLI Split

The administrator CLI is now owned by the standalone `xrayne-cli` repository.
Keep install/update flows, runtime migrations, Docker Compose generation,
certificate installation, and shell command orchestration there instead of adding
new CLI code to `xrayne-panel`.

## UI Split

The web UI is now owned by the standalone `xrayne-ui` repository. Keep React
routes, frontend API clients, UI Docker image builds, and frontend release
artifacts there. The panel API no longer builds or serves static frontend files.
