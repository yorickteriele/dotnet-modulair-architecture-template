#!/usr/bin/env python3
"""Scaffold the six module layers and register the module with the host and solution."""
import argparse
import re
import shutil
import subprocess
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument("name", help="PascalCase module name, for example Catalog")
args = parser.parse_args()
name = args.name
if not re.fullmatch(r"[A-Z][A-Za-z0-9]*", name) or name in {"Host", "Module", "MigrationRunner"}:
    parser.error("Use a PascalCase module name, excluding Host, Module and MigrationRunner")
root = Path(__file__).resolve().parents[1]
backend = root / "src/backend/Modules" / name
frontend = root / "src/frontend/src/modules" / name.lower()
if backend.exists() or frontend.exists():
    parser.error("Module already exists; no files were changed")
subprocess.run(["dotnet", "--version"], cwd=root, check=True)
source = root / "tools/templates/backend-module"
for original in sorted(source.rglob("*")):
    if not original.is_file():
        continue
    path = backend / str(original.relative_to(source)).replace("__MODULE_NAME__", name)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(original.read_text().replace("__MODULE_NAME__", name).replace("__MODULE_SCHEMA__", name.lower()))
frontend.mkdir(parents=True)
(frontend / "api").mkdir()
(frontend / "api/.gitkeep").touch()
projects = [str(p.relative_to(root)) for p in sorted(backend.rglob("*.csproj"))]
subprocess.run(["dotnet", "add", "src/backend/Host/Host.csproj", "reference", f"src/backend/Modules/{name}/{name}.Api/{name}.Api.csproj"], cwd=root, check=True)
subprocess.run(["dotnet", "sln", "src/Starter.slnx", "add", *projects, "--solution-folder", f"Modules/{name}"], cwd=root, check=True)
subprocess.run(["dotnet", "restore", "src/Starter.slnx"], cwd=root, check=True)
print(f"Created {name}. Add entities, generate its EF migration, and regenerate clients after adding API endpoints.")
