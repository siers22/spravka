<?php
declare(strict_types=1);
namespace Conference;

use PDO;

final class Database
{
    public static function connect(): PDO
    {
        $config = ['host'=>'127.0.0.1', 'port'=>'3306', 'database'=>'conference_portal',
            'user'=>'conference', 'password'=>'conference_dev'];
        if (is_file(__DIR__.'/../config.local.php')) {
            $config = array_replace($config, require __DIR__.'/../config.local.php');
        }
        foreach ($config as $key => &$value) {
            $env = getenv('DB_'.strtoupper($key));
            if ($env !== false) $value = $env;
        }
        unset($value);
        return new PDO("mysql:host={$config['host']};port={$config['port']};dbname={$config['database']};charset=utf8mb4",
            $config['user'], $config['password'], [
                PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
                PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
                PDO::ATTR_EMULATE_PREPARES => false,
            ]);
    }
}
