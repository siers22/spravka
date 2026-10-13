#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
case "${1:-up}" in
  up)
    podman network exists spravka-web-net || podman network create spravka-web-net >/dev/null
    podman volume exists spravka-web-data || podman volume create spravka-web-data >/dev/null
    podman build -q -t localhost/spravka-conference:dev .
    if ! podman container exists spravka-web-db; then
      podman run -d --name spravka-web-db --network spravka-web-net \
        -e MYSQL_ROOT_PASSWORD=local_root_dev -e MYSQL_DATABASE=conference_portal \
        -e MYSQL_USER=conference -e MYSQL_PASSWORD=conference_dev \
        -v spravka-web-data:/var/lib/mysql docker.io/library/mysql:8.4 >/dev/null
    else podman start spravka-web-db >/dev/null; fi
    ready=false
    for attempt in {1..90}; do
      if podman exec -e MYSQL_PWD=conference_dev spravka-web-db mysql -uconference -Dconference_portal -e 'SELECT 1' >/dev/null 2>&1; then ready=true; break; fi
      sleep 1
    done
    if [[ "$ready" != true ]]; then echo 'MySQL не готов. Проверьте podman logs spravka-web-db.' >&2; exit 1; fi
    if ! podman container exists spravka-web-app; then
      podman run -d --name spravka-web-app --network spravka-web-net \
        -p "127.0.0.1:${PORT:-8087}:80" -e DB_HOST=spravka-web-db \
        -e DB_DATABASE=conference_portal -e DB_USER=conference -e DB_PASSWORD=conference_dev \
        -v "$PWD:/var/www/portal:ro" localhost/spravka-conference:dev >/dev/null
    else podman start spravka-web-app >/dev/null; fi
    podman exec spravka-web-app php bin/init.php
    echo "Портал: http://127.0.0.1:${PORT:-8087}/"
    ;;
  down)
    for container in spravka-web-app spravka-web-db; do
      if podman container exists "$container"; then podman stop "$container" >/dev/null; podman rm "$container" >/dev/null; fi
    done
    echo 'Контейнеры удалены. Данные сохранены в томе spravka-web-data.'
    ;;
  *) echo 'Использование: bash bin/podman.sh up|down' >&2; exit 2 ;;
esac
