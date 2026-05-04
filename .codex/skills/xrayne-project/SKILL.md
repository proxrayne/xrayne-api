---
name: xrayne-project
description: Project-specific orientation for XRayne.Node. Use when Codex needs to understand repository layout, current product goals, cross-cutting conventions, local commands, infrastructure boundaries, or before making broad changes that touch multiple XRayne projects, README, CI, Docker/compose, or repo organization.
---

# XRayne Project

## Quick Start

Use this skill first for broad tasks, repo cleanup, planning, or changes spanning backend and UI.

Read `references/project-map.md` for the current structure, commands, CI, and non-obvious constraints. Then load the narrower skill if needed:

- Use `$xrayne-backend` for .NET API, CLI, EF Core, auth, repositories, or xray-core service work.
- Use `$xrayne-ui` for React Router, TanStack Query, auth layouts, forms, routes, or UI work.

## Project Rules

- Build API/UI Docker image artifacts in GitHub Actions when publishing releases. The image should contain the API plus the built UI in `wwwroot` and be attached to the release as `tar.gz`.
- Treat docker-compose, if present, as installer/runtime orchestration that uses prebuilt release images rather than local `build:`.
- Preserve user changes in the dirty worktree. This repo often has in-flight edits.
- Prefer established folder boundaries: domain and permission constants in `XRayne.Core`, implementations in `XRayne.Infrastructure`, EF persistence in `XRayne.Repositories`, HTTP in `XRayne.Api`, CLI commands in `XRayne.Cli`, frontend in `XRayne.UI`.
- Verify with focused commands for the touched area rather than broad expensive runs when the change is narrow.
