<?php
declare(strict_types=1);

date_default_timezone_set('Europe/Moscow');
spl_autoload_register(function (string $class): void {
    $prefix = 'Conference\\';
    if (str_starts_with($class, $prefix)) {
        $file = __DIR__.'/'.substr($class, strlen($prefix)).'.php';
        if (is_file($file)) require $file;
    }
});

function e(mixed $value): string { return htmlspecialchars((string)$value, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8'); }
function input(string $key, bool $trim = true): string {
    $value = $_POST[$key] ?? '';
    return is_string($value) ? ($trim ? trim($value) : $value) : '';
}
function getValue(string $key): string {
    $value = $_GET[$key] ?? '';
    return is_string($value) ? trim($value) : '';
}
function redirect(string $page): never { header('Location: index.php?page='.$page, true, 303); exit; }
function csrf(): string { return '<input type="hidden" name="csrf" value="'.e($_SESSION['csrf']).'">'; }
