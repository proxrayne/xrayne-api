---
name: proxrayne-panel-project
description: Project-specific orientation for the Proxrayne xrayne-panel repository. Use when Codex needs to understand the panel repository layout, current product goals, cross-cutting conventions, local commands, infrastructure boundaries, or before making broad changes that touch multiple panel projects, README, CI, Docker/compose, release packaging, or repo organization.
---

# XRayne Panel Project

## Quick Start

Use this skill first for broad panel tasks, repo cleanup, planning, or backend changes. Use `$proxrayne-project` first when the task may involve more than one Proxrayne repository.

Read `references/project-map.md` and `docs/project-rules.md` for the current structure, commands, CI, and non-obvious constraints. Then load the narrower skill if needed:

- Use `$proxrayne-panel-backend` for .NET API, EF Core, auth, repositories, or xray-core service work.
- Use `$proxrayne-ui-project` in the `xrayne-ui` repository for React Router, TanStack Query, auth layouts, forms, routes, or UI work.

## Project Rules

- Build API Docker image artifacts in GitHub Actions when publishing releases. The web UI image is built by the standalone `xrayne-ui` repository.
- Treat docker-compose, if present, as installer/runtime orchestration that uses prebuilt release images rather than local `build:`.
- Preserve user changes in the dirty worktree. This repo often has in-flight edits.
- Prefer established folder boundaries: shared contracts and permissions in `Contracts`, xray-core runtime services, managed-node orchestration, infrastructure implementations, background tasks, and utilities in `Infrastructure`, EF persistence and external repository clients in `Data`, HTTP in `Api`.
- Verify with focused commands for the touched area rather than broad expensive runs when the change is narrow.
- Keep canonical project documentation under `docs/`; update it when architecture, API, routing, packaging, or conventions change.
