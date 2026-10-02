# Публикация методички в GitHub Pages: проверенные сведения

Проверено 2 октября 2026 года. Это исследование перед публикацией; оно само не включает GitHub Pages и не отправляет файлы в репозиторий.

## Сборка и путь сайта

У проекта Vue/Vite есть production-сборка в `dist`. Для Pages нужен workflow сборки, а не dev server. В [официальном руководстве Vite](https://vite.dev/guide/static-deploy.html#github-pages) различаются корневой сайт с `base: '/'` и проектный сайт с `base: '/repository/'`. Источник публикации репозитория устанавливается в Settings → Pages → GitHub Actions.

Для автоматического определения пути можно выполнить `actions/configure-pages` **до** сборки с `id: pages`, затем передать `${{ steps.pages.outputs.base_path }}/` в build env или CLI `--base`. Output `base_path` равен `/repository` либо пустой строке, без завершающего `/`. Это подтверждено [action.yml configure-pages](https://github.com/actions/configure-pages/blob/main/action.yml).

Важно: Vite исправляет asset URLs в обработанных CSS/HTML, но строковые ссылки на PDF, ZIP и изображения в Vue/данных должны формироваться с `import.meta.env.BASE_URL`. Hash-маршруты сайта совместимы со статическим хостингом.

## Проверенные версии Actions

Версии подтверждены read-only запросом к официальному GitHub API `/repos/actions/<name>/releases/latest`:

| Action | Последний опубликованный release |
|---|---|
| configure-pages | [v6.0.0](https://github.com/actions/configure-pages/releases/tag/v6.0.0) |
| upload-pages-artifact | [v5.0.0](https://github.com/actions/upload-pages-artifact/releases/tag/v5.0.0) |
| deploy-pages | [v5.0.1](https://github.com/actions/deploy-pages/releases/tag/v5.0.1) |
| checkout | [v7.0.1](https://github.com/actions/checkout/releases/tag/v7.0.1) |
| setup-node | [v7.0.0](https://github.com/actions/setup-node/releases/tag/v7.0.0) |

В GitHub Docs пока встречаются более старые major в примерах. Актуальное Vite-руководство и релизы Actions уже используют новые major; релизные страницы являются источником для выбора версии на эту дату. Можно закрепить actions по проверенному commit SHA.

## Контракт workflow

По [GitHub Docs](https://docs.github.com/en/pages/getting-started-with-github-pages/using-custom-workflows-with-github-pages): `contents: read`, `pages: write`, `id-token: write`; environment `github-pages`, URL из `${{ steps.deployment.outputs.page_url }}`. Если build/deploy — разные jobs, deploy обязан иметь `needs: build`.

Рабочий порядок: checkout → Node → npm ci → content checks → configure-pages → build с верным base → upload `dist` → deploy. Publishing artifact должен содержать **собранный сайт**, а не весь рабочий каталог.

По [upload-pages-artifact](https://github.com/actions/upload-pages-artifact) artifact name по умолчанию `github-pages`; retention один день; hidden files по умолчанию исключены. [deploy-pages](https://github.com/actions/deploy-pages) ожидает совпадающее имя, получает `GITHUB_TOKEN` автоматически и возвращает `page_url`. Персональный токен для самого штатного workflow не требуется.

`configure-pages` не может впервые включить Pages обычным `GITHUB_TOKEN` через `enablement`; первичная настройка выполняется пользователем/авторизованным аккаунтом. Встроенные параметры configure-pages для Nuxt/Next/Gatsby/SvelteKit не настраивают обычный Vite автоматически.

## Размеры проекта

Снимок локальных размеров до финальной новой сборки:

| Каталог | Файлов | Размер, bytes |
|---|---:|---:|
| public/downloads | 6 | 1 920 864 |
| public/sources | 13 | 6 648 076 |
| public/fonts | 15 | 251 814 |
| public/shoes | 33 | 1 052 790 |
| dist | 70 | 10 319 598 |

В `public`/`dist` не обнаружены символические ссылки. Самый большой публичный файл — исходный ZIP программистов, 2 303 786 bytes. Объём сайта мал относительно поддерживаемого Pages лимита в 1 GB, указанного [upload-pages-artifact](https://github.com/actions/upload-pages-artifact#artifact-validation).

Каталог `examples` вместе с локальными bin/obj сейчас занимает около 171.6 MB, `node_modules` около 89.5 MB. Их нельзя копировать в Pages artifact; в `dist` этих каталогов нет.

## Исключение временных файлов

Проектный `.gitignore` исключает `node_modules/`, `dist/`, `tmp/`, `**/bin/`, `**/obj/`, `.env`, `.DS_Store`. Проверено read-only `git check-ignore`.

`.env.local` в текущем рабочем каталоге дополнительно исключён правилом `*.local` **родительского репозитория**. При выделении проекта в самостоятельный репозиторий полезно перенести исключение `.env.*` в его собственный `.gitignore`, при необходимости оставить `!.env.example`.

Оба публичных ZIP примеров проверены по списку записей: 77 и 34 файла; внутри нет bin/obj/node_modules/.git/.env, сертификатов pfx/pem.

## Быстрая проверка возможных секретов

Проверены текстовые исходники вне node_modules/dist/tmp/bin/obj по сигнатурам private keys, GitHub токенов, AWS access keys и явных password-полей. Первые три категории не обнаружены.

Требующие контекстного рассмотрения файлы: `examples/ExamGuide/appsettings.json`, `examples/ShoeStore/appsettings.json` — учебные PostgreSQL connection strings с локальным host и inline password; `src/components/AuthDemo.vue` — учебный демонстратор авторизации. Значения учётных данных в отчёт не включены. Это ограниченная проверка очевидных сигнатур, не полный security audit.

PDF/ZIP/DOCX в public/sources являются исходными документами пользователя и будут доступны всем посетителям сайта при публичной публикации. Это ожидаемая функция материалов методички, а не скрытый внутренний каталог.
