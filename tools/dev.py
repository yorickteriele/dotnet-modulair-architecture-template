"""Start the API and frontend, shutting down both process groups on exit."""
import os
import signal
import socket
import subprocess
import time
from pathlib import Path

root = Path(__file__).resolve().parents[1]
processes = []

def stop(signum, frame):
    raise SystemExit(128 + signum)

signal.signal(signal.SIGTERM, stop)
try:
    for port in (5000, 5001):
        with socket.socket() as probe:
            if probe.connect_ex(("127.0.0.1", port)) == 0:
                raise SystemExit(f"Port {port} is already in use; stop that service first.")
    processes.append(subprocess.Popen(["dotnet", "watch", "--project", "src/backend/Host", "run", "--no-launch-profile"], cwd=root, start_new_session=True))
    processes.append(subprocess.Popen(["node", "node_modules/vite/bin/vite.js", "--host", "127.0.0.1"], cwd=root / "src/frontend", start_new_session=True))
    while all(process.poll() is None for process in processes):
        time.sleep(0.5)
    raise SystemExit(next(process.returncode for process in processes if process.returncode is not None))
except KeyboardInterrupt:
    pass
finally:
    for process in processes:
        try:
            os.killpg(process.pid, signal.SIGTERM)
        except ProcessLookupError:
            pass
    for process in processes:
        try:
            process.wait(timeout=10)
        except subprocess.TimeoutExpired:
            os.killpg(process.pid, signal.SIGKILL)
            process.wait()
