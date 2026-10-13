<?php
declare(strict_types=1);
namespace Conference;

use PDO;

final class UserRepository
{
    public function __construct(private PDO $db) {}

    public function byLogin(string $login): array|false
    {
        $query = $this->db->prepare('SELECT * FROM users WHERE login = ?');
        $query->execute([$login]);
        return $query->fetch();
    }

    public function byId(int $id): array|false
    {
        $query = $this->db->prepare('SELECT id, login, full_name, role FROM users WHERE id = ?');
        $query->execute([$id]);
        return $query->fetch();
    }

    public function create(array $data): void
    {
        $query = $this->db->prepare('INSERT INTO users (login,password_hash,full_name,phone,email) VALUES (?,?,?,?,?)');
        $query->execute([$data['login'], password_hash($data['password'], PASSWORD_DEFAULT),
            $data['full_name'], $data['phone'], $data['email']]);
    }
}
