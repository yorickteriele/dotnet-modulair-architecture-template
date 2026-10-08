#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/../.."
image="${FRONTEND_IMAGE:-starter-frontend:local}"
(cd src/frontend && npm ci && npm run build)
mkdir -p .local/packages
docker build --file src/frontend/Dockerfile --tag "$image" \
  --label "org.opencontainers.image.source=https://github.com/${GITHUB_REPOSITORY:-local/starter}" \
  --label "org.opencontainers.image.revision=${GITHUB_SHA:-$(git rev-parse HEAD)}" \
  src/frontend
docker save --output .local/packages/frontend-image.tar "$image"
