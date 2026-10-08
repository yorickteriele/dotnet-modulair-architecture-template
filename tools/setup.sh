#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
command -v dotnet >/dev/null || { echo "Install .NET SDK 10.0.401 (see global.json)." >&2; exit 1; }
command -v node >/dev/null || { echo "Install Node 24 LTS." >&2; exit 1; }
command -v docker >/dev/null || { echo "Install Docker with Compose." >&2; exit 1; }
docker compose version >/dev/null
python3 tools/init-env.py
set -a
source .env
set +a
dotnet tool restore
dotnet restore src/Starter.slnx --locked-mode
npm ci --prefix src/frontend
docker compose up -d --wait db
dotnet build src/Starter.slnx --no-restore -m:2
dotnet run --project src/backend/MigrationRunner --no-build
mkdir -p .local
if curl -fsS http://127.0.0.1:5001/health >/dev/null 2>&1; then
  echo "Port 5001 is already in use. Stop that service before setup." >&2
  exit 1
fi
(cd src/backend/Host && exec dotnet bin/Debug/net10.0/Host.dll) > .local/generate-host.log 2>&1 &
host_pid=$!
trap 'kill "$host_pid" 2>/dev/null || true; wait "$host_pid" 2>/dev/null || true' EXIT
for attempt in {1..60}; do
  if curl -fsS http://127.0.0.1:5001/health >/dev/null 2>&1; then break; fi
  if ! kill -0 "$host_pid" 2>/dev/null; then cat .local/generate-host.log; exit 1; fi
  sleep 1
done
curl -fsS http://127.0.0.1:5001/health
(cd src/frontend && npm run generate && npm run build)
dotnet test src/Starter.slnx --no-build --no-restore -m:2
python3 tools/smoke.py
echo "Setup verified. Start development with ./tools/dev.sh"
