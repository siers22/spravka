<?php
declare(strict_types=1);
namespace Conference;

use PDO;
use PDOException;

final class Portal
{
    private UserRepository $users;
    private BookingRepository $bookings;
    private array $errors = [];
    private array $old = [];
    private array|false $user;

    public function __construct(PDO $db)
    {
        $this->users = new UserRepository($db);
        $this->bookings = new BookingRepository($db);
        $this->user = $this->users->byId((int)($_SESSION['user_id'] ?? 0));
    }

    public function run(): void
    {
        $page = getValue('page') ?: ($this->user ? 'dashboard' : 'login');
        if (!in_array($page, ['login','register','dashboard','booking','admin','logout'], true)) {
            http_response_code(404); $this->render('message', ['title'=>'Страница не найдена', 'message'=>'Проверьте адрес или вернитесь в личный кабинет.']); return;
        }
        if (in_array($page, ['dashboard','booking','admin','logout'], true) && !$this->user) redirect('login');
        if ($page === 'admin' && $this->user['role'] !== 'admin') {
            http_response_code(403); $this->render('message', ['title'=>'Доступ запрещён', 'message'=>'Эта страница доступна администратору.']); return;
        }
        if ($_SERVER['REQUEST_METHOD'] === 'POST') {
            $token = input('csrf');
            if (!$token || !hash_equals($_SESSION['csrf'], $token)) {
                http_response_code(403); $this->render('message', ['title'=>'Форма устарела', 'message'=>'Обновите страницу и повторите действие.']); return;
            }
            $this->post($page);
        } elseif ($_SERVER['REQUEST_METHOD'] !== 'GET') {
            header('Allow: GET, POST'); http_response_code(405); return;
        }
        if ($page === 'logout') redirect('dashboard');
        if ($this->user && in_array($page, ['register','login'], true)) redirect('dashboard');
        $data = [];
        if (in_array($page, ['booking','dashboard','admin'], true)) $data['rooms'] = $this->bookings->rooms();
        if (in_array($page, ['dashboard','admin'], true)) {
            $status = getValue('status');
            if (!array_key_exists($status, BookingRepository::STATUSES)) $status = '';
            $room = max(0, (int)getValue('room'));
            $date = getValue('date');
            if (!Validation::date($date.'T00:00')) $date = '';
            $data += ['status'=>$status, 'room'=>$room, 'date'=>$date,
                'listing'=>$this->bookings->listing($page === 'admin' ? null : (int)$this->user['id'], $status, $room, $date, max(1, (int)getValue('p')))];
        }
        $this->render($page, $data);
    }

    private function post(string $page): void
    {
        if ($page === 'logout') {
            $_SESSION = []; session_regenerate_id(true); redirect('login');
        }
        if ($page === 'register') {
            foreach (['login','full_name','phone','email'] as $key) $this->old[$key] = input($key);
            $data = $this->old + ['password'=>input('password', false)];
            $this->errors = Validation::registration($data);
            if (!$this->errors) {
                try { $this->users->create($data); $this->flash('Аккаунт создан. Войдите с вашим логином и паролем.'); redirect('login'); }
                catch (PDOException $e) { if (($e->errorInfo[1] ?? 0) !== 1062) throw $e; $this->errors['login'] = 'Этот логин уже занят.'; }
            }
        }
        if ($page === 'login') {
            $this->old['login'] = input('login');
            $account = $this->users->byLogin($this->old['login']);
            if (!$account || !password_verify(input('password', false), $account['password_hash'])) {
                $this->errors['login'] = 'Неверный логин или пароль.';
            } else {
                session_regenerate_id(true); $_SESSION['user_id'] = (int)$account['id'];
                $_SESSION['csrf'] = bin2hex(random_bytes(32)); redirect($account['role'] === 'admin' ? 'admin' : 'dashboard');
            }
        }
        if ($page === 'booking') {
            foreach (['room_id','starts_at','payment'] as $key) $this->old[$key] = input($key);
            $roomIds = array_column($this->bookings->rooms(), 'id');
            if (!ctype_digit($this->old['room_id']) || !in_array((int)$this->old['room_id'], $roomIds, true)) $this->errors['room_id'] = 'Выберите помещение из списка.';
            $date = Validation::date($this->old['starts_at']);
            if (!$date || $date < date('Y-m-d H:i:s')) $this->errors['starts_at'] = 'Выберите корректные будущие дату и время.';
            if (!isset(BookingRepository::PAYMENTS[$this->old['payment']])) $this->errors['payment'] = 'Выберите способ оплаты.';
            if (!$this->errors) {
                $this->bookings->create((int)$this->user['id'], (int)$this->old['room_id'], $date, $this->old['payment']);
                $this->flash('Заявка отправлена. Администратор рассмотрит её и назначит мероприятие.'); redirect('dashboard');
            }
        }
        if ($page === 'admin') {
            $status = input('status');
            if (!isset(BookingRepository::STATUSES[$status]) || !ctype_digit(input('id')) || !$this->bookings->changeStatus((int)input('id'), $status)) {
                $this->errors['form'] = 'Статус не изменён: проверьте заявку. Завершённые мероприятия доступны только для просмотра.';
            } else { $this->flash('Статус заявки обновлён.'); redirect('admin'); }
        }
        if ($page === 'dashboard') {
            $body = input('body');
            if (!$body || Validation::length($body) > 2000 || !ctype_digit(input('id'))) $this->errors['form'] = 'Введите отзыв от 1 до 2000 символов.';
            else {
                try {
                    if ($this->bookings->review((int)input('id'), (int)$this->user['id'], $body)) { $this->flash('Спасибо! Ваш отзыв сохранён.'); redirect('dashboard'); }
                    $this->errors['form'] = 'Отзыв доступен только для вашей завершённой заявки.';
                } catch (PDOException $e) {
                    if (($e->errorInfo[1] ?? 0) !== 1062) throw $e;
                    $this->errors['form'] = 'Вы уже оставили отзыв об этом мероприятии.';
                }
            }
        }
        if ($this->errors) http_response_code(422);
    }

    private function flash(string $text): void { $_SESSION['flash'] = $text; }

    private function render(string $view, array $data = []): void
    {
        $user = $this->user; $errors = $this->errors; $old = $this->old;
        $flash = $_SESSION['flash'] ?? ''; unset($_SESSION['flash']);
        extract($data, EXTR_SKIP);
        $titles = ['login'=>'Войти в аккаунт', 'register'=>'Создать аккаунт', 'dashboard'=>'Мои заявки', 'booking'=>'Забронировать помещение', 'admin'=>'Заявки пользователей'];
        $title = $title ?? $titles[$view] ?? 'Конференции.РФ';
        ob_start(); require __DIR__.'/../views/'.$view.'.php'; $content = ob_get_clean();
        require __DIR__.'/../views/layout.php';
    }
}
