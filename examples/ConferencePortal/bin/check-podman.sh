#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
# Tests never touch conference_portal; they create and remove their own database.
test_db="conference_test_$(date +%s)_$$"
test_container="spravka-web-test-$$"
mkdir -p tests/results
cleanup() {
  podman rm -f "$test_container" >/dev/null 2>&1 || true
  podman exec -e MYSQL_PWD=local_root_dev spravka-web-db mysql -uroot \
    -e "DROP DATABASE IF EXISTS $test_db;" >/dev/null 2>&1 || true
}
trap cleanup EXIT
podman exec -e MYSQL_PWD=local_root_dev spravka-web-db mysql -uroot \
  -e "CREATE DATABASE $test_db CHARACTER SET utf8mb4; GRANT ALL ON $test_db.* TO 'conference'@'%';"
podman run -d --name "$test_container" --network spravka-web-net \
  -p "127.0.0.1:${TEST_PORT:-8088}:80" -e DB_HOST=spravka-web-db \
  -e "DB_DATABASE=$test_db" -e DB_USER=conference -e DB_PASSWORD=conference_dev \
  -v "$PWD:/var/www/portal:ro" localhost/spravka-conference:dev >/dev/null
podman exec "$test_container" php bin/init.php
podman exec "$test_container" sh -c 'find src views public bin -name "*.php" -exec php -l {} \;'
podman run --rm --network host -e "PORTAL_URL=http://127.0.0.1:${TEST_PORT:-8088}/" \
  -v "$PWD/tests:/tests:ro" -v "$PWD/tests/results:/results" \
  docker.io/library/python:3.13-slim python /tests/http_checks.py
