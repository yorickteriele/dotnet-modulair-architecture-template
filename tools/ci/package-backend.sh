#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/../.."
image="${BACKEND_IMAGE:-starter-backend:local}"
dotnet restore src/Starter.slnx --locked-mode
# Clean only this script's generated output, so removed modules cannot enter the package.
rm -rf .local/packages/backend
mkdir -p .local/packages/backend
dotnet publish src/backend/Host/Host.csproj -c Release --no-restore --self-contained false -o .local/packages/backend/host -m:2
dotnet publish src/backend/MigrationRunner/MigrationRunner.csproj -c Release --no-restore --self-contained false -o .local/packages/backend/migrations -m:2
docker build --file src/backend/Dockerfile --tag "$image" \
  --label "org.opencontainers.image.source=https://github.com/${GITHUB_REPOSITORY:-local/starter}" \
  --label "org.opencontainers.image.revision=${GITHUB_SHA:-$(git rev-parse HEAD)}" \
  .local/packages/backend
docker save --output .local/packages/backend-image.tar "$image"
