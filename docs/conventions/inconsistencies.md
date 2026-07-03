# Inconsistencies / To Review

These items describe observed drift between current code and desired conventions.
They should be handled as separate cleanup work unless a feature directly touches
the affected area.

## XML Documentation Coverage

New backend rules require XML `<summary>` comments on public classes, methods,
services, DTOs, handlers, controllers, and endpoints. Existing code mostly uses
`EndpointSummary` and `EndpointDescription` but does not consistently include XML
documentation.

Recommended follow-up: add XML docs incrementally when files are touched, then run
a dedicated documentation cleanup pass.

## OpenAPI Description Coverage

Some existing endpoints have `EndpointSummary` without `EndpointDescription` or
without all error response types. New endpoints must include full Scalar/OpenAPI
metadata.

Recommended follow-up: audit controllers and fill missing descriptions/status
codes.

## Frontend Loading States

Some route components still use raw loading placeholders such as `loading...`.
The styleguide now requires polished loading, error, and empty states using shared
UI primitives.

Recommended follow-up: replace raw placeholders during route UI work.

## Existing Documentation Outside `/docs`

`README.md`, `PUBLIC_README.md`, `.codex/skills`, and
Frontend styleguide ownership moved to the standalone `xrayne-ui` repository.
They should remain, but `/docs` is now the canonical source for architecture and
styleguide details.

Recommended follow-up: keep external docs short and link to `/docs` where possible.

## Test Project Build Drift

At the time of this documentation pass, full solution build can fail in
`Test` due to stale references around settings/repository types. This is
separate from the documentation structure.

Recommended follow-up: repair test project references and add CI coverage for the
new conventions.
