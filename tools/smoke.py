"""Exercise auth against the local development database without logging credentials."""
import json
import os
import uuid
import urllib.error
import urllib.request

base = os.environ.get("SMOKE_API_URL", "http://127.0.0.1:5001")

def request(path, body=None, token=None, origin=base):
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = "Bearer " + token
    req = urllib.request.Request(origin + path, data=json.dumps(body).encode() if body is not None else None, headers=headers)
    try:
        with urllib.request.urlopen(req, timeout=20) as response:
            raw = response.read()
            return response.status, json.loads(raw) if raw and raw.startswith(b"{") else raw.decode()
    except urllib.error.HTTPError as response:
        return response.code, None

email = f"smoke-{uuid.uuid4().hex}@example.com"
password = "Local-smoke-" + uuid.uuid4().hex
path = "/api/v1/Identity/auth"
health_url = os.environ.get("SMOKE_HEALTH_URL", base + "/health")
assert request("", origin=health_url) == (200, "Healthy")
assert request(path + "/me")[0] == 401
assert request(path + "/register", {"email": "invalid", "password": "weak", "displayName": "Test"})[0] == 400
status, profile = request(path + "/register", {"email": email, "password": password, "displayName": "Smoke Test"})
assert status == 201, f"Registration: {status}"
assert profile["email"] == email
assert request(path + "/register", {"email": email.upper(), "password": password, "displayName": "Duplicate"})[0] == 400
assert request(path + "/login", {"email": email, "password": "incorrect"})[0] == 401
status, auth = request(path + "/login", {"email": email, "password": password})
assert status == 200, f"Login: {status}"
status, me = request(path + "/me", token=auth["accessToken"])
assert status == 200 and me == profile
assert request(path + "/me", token=auth["accessToken"] + "tampered")[0] == 401
for _ in range(5):
    assert request(path + "/login", {"email": email, "password": "incorrect"})[0] == 401
assert request(path + "/login", {"email": email, "password": password})[0] == 401
print("Auth smoke passed: health, validation, registration, duplicate email, login, profile, tampered JWT and lockout")
