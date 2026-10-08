#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/../.."
backend_image="${BACKEND_IMAGE:-starter-backend:local}"
frontend_image="${FRONTEND_IMAGE:-}"
api_port="${API_PORT:-5011}"
frontend_port="${FRONTEND_PORT:-5010}"
docker image inspect "$backend_image" >/dev/null
if [[ -n "$frontend_image" ]]; then docker image inspect "$frontend_image" >/dev/null; fi
prefix="starter-ci-${BASHPID}-${RANDOM}"
network="${prefix}-network"
db="${prefix}-db"
backend="${prefix}-backend"
frontend="${prefix}-frontend"
mkdir -p .local
credentials=$(mktemp -d .local/package-test.XXXXXX)
cleanup() {
  status=$?
  trap - EXIT
  docker rm -f -v "$frontend" "$backend" "$db" >/dev/null 2>&1 || true
  docker network rm "$network" >/dev/null 2>&1 || true
  rm -rf "$credentials"
  exit "$status"
}
trap cleanup EXIT
trap 'exit 130' INT
trap 'exit 143' TERM
python3 - "$credentials" <<'ENVFILES'
import os
import secrets
import sys
from pathlib import Path
root = Path(sys.argv[1])
password = secrets.token_hex(24)
files = {
    'postgres.env': f'POSTGRES_USER=starter\nPOSTGRES_DB=starter\nPOSTGRES_PASSWORD={password}\n',
    'backend.env': 'ASPNETCORE_ENVIRONMENT=Development\nASPNETCORE_URLS=http://+:8080\n'
        + f'Jwt__Secret={secrets.token_hex(32)}\nJwt__Issuer=Starter\nJwt__Audience=Starter\n'
        + f'ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=starter;Username=starter;Password={password}\n',
}
for name, content in files.items():
    with os.fdopen(os.open(root / name, os.O_WRONLY | os.O_CREAT | os.O_EXCL, 0o600), 'w') as stream:
        stream.write(content)
ENVFILES
wait_for_http() {
  local url="$1" container="$2"
  for attempt in {1..60}; do
    if curl -fsS "$url" >/dev/null 2>&1; then return; fi
    if [[ "$(docker inspect --format '{{.State.Running}}' "$container")" != true ]]; then break; fi
    sleep 1
  done
  docker logs "$container" >&2
  echo "Readiness failed for $container" >&2
  return 1
}
docker network create "$network" >/dev/null
docker run -d --name "$db" --network "$network" --network-alias db \
  --env-file "$credentials/postgres.env" \
  --health-cmd 'pg_isready -U starter -d starter' --health-interval 1s --health-timeout 5s --health-retries 30 \
  postgres:18-alpine >/dev/null
for attempt in {1..60}; do
  state=$(docker inspect --format '{{.State.Health.Status}}' "$db")
  if [[ "$state" == healthy ]]; then break; fi
  if [[ "$state" == unhealthy ]]; then docker logs "$db" >&2; exit 1; fi
  sleep 1
done
[[ "$state" == healthy ]] || { docker logs "$db" >&2; exit 1; }
docker run --rm --network "$network" --env-file "$credentials/backend.env" \
  --entrypoint dotnet "$backend_image" /app/migrations/MigrationRunner.dll
docker run -d --name "$backend" --network "$network" --network-alias backend \
  --env-file "$credentials/backend.env" --publish "127.0.0.1:${api_port}:8080" "$backend_image" >/dev/null
wait_for_http "http://127.0.0.1:${api_port}/health" "$backend"
if [[ "${GENERATE_CLIENTS:-0}" == 1 ]]; then
  (cd src/frontend && OPENAPI_BASE_URL="http://127.0.0.1:${api_port}" npm run generate)
  node tools/ci/archive-clients.mjs
fi
smoke_url="http://127.0.0.1:${api_port}"
if [[ -n "$frontend_image" ]]; then
  docker run -d --name "$frontend" --network "$network" \
    --publish "127.0.0.1:${frontend_port}:8080" "$frontend_image" >/dev/null
  wait_for_http "http://127.0.0.1:${frontend_port}/" "$frontend"
  # nginx must serve SPA routes and preserve authentication through the API proxy.
  python3 - "$frontend_port" <<'SPACHECK'
import sys
import urllib.request
base = 'http://127.0.0.1:' + sys.argv[1]
for path in ('/', '/management/profile'):
    with urllib.request.urlopen(base + path, timeout=10) as response:
        assert response.status == 200 and b'<div id="root">' in response.read()
print('Packaged frontend passed root and SPA route checks')
SPACHECK
  smoke_url="http://127.0.0.1:${frontend_port}"
fi
SMOKE_API_URL="$smoke_url" SMOKE_HEALTH_URL="http://127.0.0.1:${api_port}/health" python3 tools/smoke.py
