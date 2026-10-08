from pathlib import Path
import os
import secrets

path = Path(__file__).resolve().parents[1] / ".env"
try:
    fd = os.open(path, os.O_WRONLY | os.O_CREAT | os.O_EXCL, 0o600)
except FileExistsError:
    print("Preserving existing .env")
else:
    password = secrets.token_hex(24)
    jwt = secrets.token_hex(32)
    with os.fdopen(fd, "w") as env:
        env.write(f"DB_PASSWORD={password}\nJwt__Secret={jwt}\nJwt__Issuer=Starter\nJwt__Audience=Starter\n")
        env.write("ASPNETCORE_ENVIRONMENT=Development\nASPNETCORE_URLS=http://127.0.0.1:5001\n")
        env.write(f"ConnectionStrings__DefaultConnection='Host=127.0.0.1;Port=5432;Database=starter;Username=starter;Password={password}'\n")
    print("Created local .env with fresh development credentials")
