# Modular .NET project starter

A modular monolith with .NET 10, PostgreSQL, React 19, TypeScript, Vite, Tailwind CSS and Zustand. Identity is included. There is no tenancy, booking, billing, email-provider or blob-storage dependency.

## Quick start

Use this repository as a GitHub template, or clone it into your new project's directory. Prerequisites: .NET SDK **10.0.401**, Node **24 LTS**, Docker with Compose, Python 3 and curl. Linux/macOS commands below also work in WSL2.

```bash
./tools/setup.sh
./tools/dev.sh
```

Setup generates a private, ignored `.env` with fresh local credentials, installs pinned dependencies, starts PostgreSQL, applies migrations, generates TypeScript clients, builds both applications, runs unit tests and exercises the auth API. Existing `.env` files are preserved. Setup can be run again. It leaves PostgreSQL running and stops its temporary API process.

In containers with low inotify limits, use `export DOTNET_USE_POLLING_FILE_WATCHER=1` before starting development.

Development runs the API on port 5001 and Vite on port 5000 with hot reload. The frontend proxies `/api` to the API, so browser requests are same-origin. Swagger is available at `/swagger` in Development. Stop development with Ctrl+C; stop the database with `docker compose stop db`. `docker compose down -v` deletes your local database.

Create an account in the frontend with a unique email and a password of at least 12 characters, including uppercase, lowercase, a digit and a symbol. You can sign in, fetch your protected profile, and sign out. Tokens are held in memory; refreshing signs you out. Sign-out clears the client session; issued JWTs remain valid until their 30-minute expiry.

## Architecture

```text
src/
  Starter.slnx
  backend/
    Host/                  # Composition, auth middleware, logging, Swagger
    MigrationRunner/       # Explicit migration command, separate from API startup
    Modules/
      Module.Abstractions/  # Module registration and per-schema DbContext base
      Identity/
        Identity.Api/
        Identity.Application/
        Identity.Contracts/
        Identity.Domain/
        Identity.Infrastructure/
        Identity.Tests/
  frontend/
    src/modules/identity/  # API wrapper, generated client and auth state
    scripts/               # Discover modules, fetch OpenAPI, run NSwag
```

Api composes Application and Infrastructure. Application uses Contracts and its own persistence abstractions. Infrastructure implements those abstractions and owns Domain, EF Core and external adapters. Domain references Contracts when needed. Cross-module dependencies go through Contracts only. Each module owns a PostgreSQL schema and its own migration history. No cross-schema foreign keys. The host discovers modules from referenced Api assemblies; controllers contain HTTP concerns and delegate business operations to Application services.

Identity uses ASP.NET Core Identity password hashing and email normalization, JWT issuer/audience/signature/expiry validation, input validation, account lockout after five failed attempts, and rate limits on public auth endpoints. Registration does not require email confirmation. Password reset, refresh tokens, account administration, MFA and outbound email are deliberate extension points, not implemented features.

## Add a module

```bash
./tools/create-module.sh Catalog
```

This creates the six backend layers, registers the API project in the host and solution, and creates a frontend API directory for discovery. It refuses to overwrite an existing module. Generated application code starts empty so you can add your own business behavior.

Use `[ApiExplorerSettings(GroupName = "catalog")]` and `[Route("api/v1/Catalog/...")]` on Catalog controllers. Apply `[Authorize]` by default, opting into `[AllowAnonymous]` only where necessary. Use Contracts for DTOs and mark required response properties with `[property: Required]` so generated clients match the API.

After changing entities, generate migrations with EF tooling, never by hand:

```bash
set -a; source .env; set +a
# Example after adding Catalog entities:
dotnet ef migrations add InitialCatalog \
  --project src/backend/Modules/Catalog/Catalog.Infrastructure

dotnet run --project src/backend/MigrationRunner
```

MigrationRunner migrates modules in name order. Modules must remain independent at the database level. New migrations, their designers and snapshots are tracked. There is no automatic schema mutation during API startup.

After changing controllers or DTOs, with the backend running:

```bash
cd src/frontend
npm run generate
npm run build
```

OpenAPI specs and generated clients are ignored and must be regenerated; never hand-edit them. Frontend module directories and API group names are lowercase. Backend module names use PascalCase.

## Checks

```bash
dotnet test src/Starter.slnx -m:2
(cd src/frontend && npm run build)
python3 tools/smoke.py  # API and PostgreSQL must be running locally
```

The smoke check creates a uniquely named local account and tests health, invalid input, registration, duplicate email rejection, wrong-password rejection, successful login, an authorized profile request, tampered JWT rejection and account lockout. These accounts remain in the local development database. Use it only with disposable development/test data.

CI repeats setup against PostgreSQL on Ubuntu. npm uses `npm ci`; NuGet restores with committed `packages.lock.json` files and locked mode. If changing dependencies intentionally, update the lockfiles with `npm install` or `dotnet restore --force-evaluate` and review them.

## Deployment configuration

Supply your own database connection and JWT signing key through your secret manager. Never reuse local `.env` credentials. Configure `ASPNETCORE_ENVIRONMENT=Production`, your issuer/audience, TLS at your ingress and a same-origin frontend/API reverse proxy. Trust forwarded headers only from your ingress if you add that middleware. The development Vite proxy is not a production server. Deployment/provider integrations are intentionally left to each project.
