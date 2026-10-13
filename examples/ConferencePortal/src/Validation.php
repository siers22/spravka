<?php
declare(strict_types=1);
namespace Conference;

final class Validation
{
    public static function registration(array $data): array
    {
        $errors = [];
        if (!preg_match('/\A[A-Za-z0-9]{6,64}\z/', $data['login']))
            $errors['login'] = 'Логин: от 6 до 64 латинских букв и цифр.';
        // Passwords are never trimmed: a space is a possible password character.
        if (preg_match_all('/./us', $data['password']) < 8 || strlen($data['password']) > 72)
            $errors['password'] = 'Пароль: от 8 символов, не более 72 байт.';
        if (!preg_match('/\A[А-Яа-яЁё]+(?: +[А-Яа-яЁё]+)*\z/u', $data['full_name']) || self::length($data['full_name']) > 150)
            $errors['full_name'] = 'ФИО: только кириллица и пробелы, до 150 символов.';
        if (!preg_match('/\A8\([0-9]{3}\)[0-9]{3}-[0-9]{2}-[0-9]{2}\z/', $data['phone']))
            $errors['phone'] = 'Телефон в формате 8(999)123-45-67.';
        if (!filter_var($data['email'], FILTER_VALIDATE_EMAIL) || strlen($data['email']) > 254)
            $errors['email'] = 'Введите корректный адрес электронной почты.';
        return $errors;
    }

    public static function length(string $value): int
    {
        return preg_match_all('/./us', $value) ?: 0;
    }

    public static function date(string $value): ?string
    {
        $date = \DateTimeImmutable::createFromFormat('!Y-m-d\TH:i', $value);
        return $date && $date->format('Y-m-d\TH:i') === $value
            ? $date->format('Y-m-d H:i:s') : null;
    }
}
