# Codex Rules

This repository contains the XRayne panel backend. Before making backend
changes, Codex must read and follow the shared styleguide at
`../docs/DOTNET_STYLEGUIDE.md`.

The shared styleguide is mandatory when a task touches backend code, API endpoints,
EF Core, PostgreSQL, migrations, repositories, services, DTOs, request/response
models, validation, errors, logging, configuration, dependency injection, tests,
or documentation for those areas.

When general guidance conflicts with the current project architecture, follow
the existing architecture first, then improve it incrementally with small,
reviewable changes. Do not radically rewrite layers, public API contracts, or
database schema without a clear need.

Preserve user changes and avoid mass-formatting unrelated files.
