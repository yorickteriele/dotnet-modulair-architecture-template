#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
[[ -f .env && -d src/frontend/node_modules && -f src/frontend/src/modules/identity/api/generated/api-client.ts ]] || ./tools/setup.sh
set -a
source .env
set +a
docker compose up -d --wait db
dotnet run --project src/backend/MigrationRunner
exec python3 tools/dev.py
