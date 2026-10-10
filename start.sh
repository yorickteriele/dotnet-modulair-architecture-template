#!/usr/bin/env bash
# Starts the whole development stack in one tmux session: PostgreSQL (Docker), migrations, the API and the frontend.
#
#   ./start.sh            start (or reattach to) the session
#   ./start.sh --detach   start without attaching (for scripts)
#   ./start.sh stop       stop the session (the database keeps running; docker compose stop db stops it)
set -euo pipefail
cd "$(dirname "$0")"
ROOT="$PWD"
SESSION="${SESSION:-$(basename "$ROOT" | tr -c 'A-Za-z0-9_-\n' '-')}"
DETACH=false
[[ "${1:-}" == --detach ]] && DETACH=true

attach() {
  $DETACH && return 0
  if [[ -n "${TMUX:-}" ]]; then exec tmux switch-client -t "$SESSION"; else exec tmux attach -t "$SESSION"; fi
}

if [[ "${1:-}" == stop ]]; then
  tmux kill-session -t "$SESSION" 2>/dev/null && echo "Stopped $SESSION." || echo "$SESSION wasn't running."
  exit 0
fi

command -v tmux >/dev/null || { echo "Install tmux." >&2; exit 1; }
if tmux has-session -t "$SESSION" 2>/dev/null; then
  echo "$SESSION is already running."
  attach
  exit 0
fi

# First run: create .env, install dependencies and generate the API clients.
[[ -f .env && -d src/frontend/node_modules && -f src/frontend/src/modules/identity/api/generated/api-client.ts ]] || ./tools/setup.sh
set -a
source .env
set +a

# The database must be healthy before migrations and the API start.
docker compose up -d --wait db
echo "PostgreSQL is up."
dotnet run --project src/backend/MigrationRunner

for port in 5000 5001; do
  if (exec 3<>"/dev/tcp/127.0.0.1/$port") 2>/dev/null; then
    echo "Port $port is already in use; stop that service first." >&2
    exit 1
  fi
done

# Every window loads .env itself, so restarting a command in its pane picks up changes.
ENV='set -a; source .env; set +a;'
tmux new-session -d -s "$SESSION" -n api -c "$ROOT"
tmux send-keys -t "$SESSION:api" "$ENV DOTNET_WATCH_RESTART_ON_RUDE_EDIT=true dotnet watch --project src/backend/Host run --no-launch-profile" C-m
tmux new-window -t "$SESSION" -n web -c "$ROOT"
tmux send-keys -t "$SESSION:web" "cd src/frontend && node node_modules/vite/bin/vite.js --host 127.0.0.1" C-m
tmux new-window -t "$SESSION" -n db -c "$ROOT"
tmux send-keys -t "$SESSION:db" "docker compose exec db psql -U starter -d starter" C-m
tmux select-window -t "$SESSION:api"

echo "Started tmux session '$SESSION': api (:5001), web (:5000) and a psql shell (db). Stop it with ./start.sh stop."
attach
