# Project conventions

This is an Aresis-inspired modular monolith without tenancy. Read README.md before changes.

- Use the existing checkout. Do not create worktrees unless explicitly requested.
- Keep modules in Api, Application, Contracts, Domain, Infrastructure and Tests layers.
- Cross-module references go through Contracts. Each module owns a PostgreSQL schema; no cross-schema foreign keys.
- Controllers delegate business operations to Application services. Infrastructure implements persistence abstractions.
- Default to authorized endpoints. Never log passwords, tokens or personal data.
- Generate EF migrations with `dotnet ef migrations add`; do not hand-write them or edit applied migrations.
- Generate frontend clients with `npm run generate`; do not edit generated files or duplicate generated DTOs.
- Use `tools/create-module.sh` for new bounded contexts. Prefer existing modules for related features.
- Keep credentials out of tracked files. Preserve existing .env values.
- Run backend tests, frontend build, and auth smoke checks when changing authentication or setup.
