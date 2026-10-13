<?php
declare(strict_types=1);
require __DIR__.'/../src/bootstrap.php';

ini_set('session.use_strict_mode', '1');
session_set_cookie_params(['httponly'=>true, 'samesite'=>'Lax', 'secure'=>!empty($_SERVER['HTTPS']) && $_SERVER['HTTPS'] !== 'off']);
session_start();
$_SESSION['csrf'] ??= bin2hex(random_bytes(32));
header('X-Content-Type-Options: nosniff');
header('X-Frame-Options: DENY');
header('Referrer-Policy: same-origin');
header('Cache-Control: no-store');

try {
    (new Conference\Portal(Conference\Database::connect()))->run();
} catch (Throwable $exception) {
    error_log((string)$exception);
    http_response_code(503);
    echo '<!doctype html><html lang="ru"><meta charset="utf-8"><title>Сервис временно недоступен</title><h1>Не удалось открыть портал</h1><p>Проверьте подключение к базе и выполнение bin/init.php. Обновите страницу после восстановления сервиса.</p></html>';
}
