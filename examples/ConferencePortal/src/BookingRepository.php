<?php
declare(strict_types=1);
namespace Conference;

use PDO;

final class BookingRepository
{
    public const STATUSES = ['new'=>'Новая', 'assigned'=>'Мероприятие назначено', 'completed'=>'Завершено'];
    public const PAYMENTS = ['onsite'=>'При очном посещении', 'sbp'=>'Перевод по системе СБП'];

    public function __construct(private PDO $db) {}

    public function rooms(): array
    {
        return $this->db->query('SELECT * FROM rooms ORDER BY id')->fetchAll();
    }

    public function create(int $user, int $room, string $date, string $payment): void
    {
        $query = $this->db->prepare('INSERT INTO bookings (user_id,room_id,starts_at,payment) VALUES (?,?,?,?)');
        $query->execute([$user, $room, $date, $payment]);
    }

    public function listing(?int $owner, string $status, int $room, string $date, int $page): array
    {
        $where = ['1=1']; $params = [];
        if ($owner !== null) { $where[] = 'b.user_id = ?'; $params[] = $owner; }
        if ($status !== '') { $where[] = 'b.status = ?'; $params[] = $status; }
        if ($room > 0) { $where[] = 'b.room_id = ?'; $params[] = $room; }
        if ($date !== '') { $where[] = 'b.starts_at >= ? AND b.starts_at < DATE_ADD(?, INTERVAL 1 DAY)'; $params[] = $date; $params[] = $date; }
        $condition = implode(' AND ', $where);
        $query = $this->db->prepare("SELECT COUNT(*) FROM bookings b WHERE $condition");
        $query->execute($params); $total = (int)$query->fetchColumn();
        $page = max(1, min($page, max(1, (int)ceil($total / 12))));
        $offset = ($page - 1) * 12;
        // One joined query for an entire page, including optional reviews (no N+1).
        $query = $this->db->prepare("SELECT b.*, r.name AS room_name, r.image, r.capacity,
            u.full_name, u.phone, u.email, v.body AS review
            FROM bookings b JOIN rooms r ON r.id=b.room_id JOIN users u ON u.id=b.user_id
            LEFT JOIN reviews v ON v.booking_id=b.id WHERE $condition
            ORDER BY b.id DESC LIMIT 12 OFFSET $offset");
        $query->execute($params);
        return ['items'=>$query->fetchAll(), 'total'=>$total, 'page'=>$page, 'pages'=>(int)ceil($total / 12)];
    }

    public function changeStatus(int $id, string $status): bool
    {
        // A completed event stays completed: published feedback cannot become inconsistent.
        $query = $this->db->prepare("UPDATE bookings SET status=? WHERE id=? AND status <> 'completed'");
        $query->execute([$status, $id]);
        return $query->rowCount() > 0;
    }

    public function review(int $id, int $owner, string $body): bool
    {
        // Ownership and state are checked by the database, even for forged POST requests.
        $query = $this->db->prepare("INSERT INTO reviews (booking_id,body)
            SELECT id, ? FROM bookings WHERE id=? AND user_id=? AND status='completed'");
        $query->execute([$body, $id, $owner]);
        return $query->rowCount() === 1;
    }
}
