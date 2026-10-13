<?php
declare(strict_types=1);
require __DIR__.'/../src/bootstrap.php';
if (PHP_SAPI !== 'cli') { http_response_code(404); exit; }
$db = Conference\Database::connect();
$db->exec(file_get_contents(__DIR__.'/../sql/schema.sql'));
$admin = $db->prepare('INSERT IGNORE INTO users (login,password_hash,full_name,phone,email,role) VALUES (?,?,?,?,?,?)');
$admin->execute(['Conf2027', password_hash('Demo77', PASSWORD_DEFAULT), 'Администратор портала',
    '8(999)000-00-00', 'admin@conference.test', 'admin']);
$room = $db->prepare('INSERT INTO rooms (name,description,image,capacity) VALUES (?,?,?,?) ON DUPLICATE KEY UPDATE description=VALUES(description),image=VALUES(image),capacity=VALUES(capacity)');
foreach ([
    ['Аудитория','Для лекций, выступлений и встреч с экспертами.','auditorium.webp',80],
    ['Коворкинг','Открытое пространство для совместной работы и дискуссий.','coworking.webp',30],
    ['Кинозал','Большой экран и удобные места для презентаций и показов.','cinema.webp',120],
] as $data) $room->execute($data);
echo "Готово: 4 таблицы, 3 помещения. Администратор Conf2027 / Demo77.\n";
