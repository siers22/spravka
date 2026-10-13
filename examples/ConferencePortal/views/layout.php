<!doctype html>
<html lang="ru">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title><?= e($title) ?> · Конференции.РФ</title>
    <link rel="icon" href="assets/mark.svg" type="image/svg+xml">
    <link rel="stylesheet" href="assets/bootstrap.min.css">
    <link rel="stylesheet" href="assets/app.css">
    <script src="assets/app.js" defer></script>
</head>
<body>
<a class="skip" href="#main">Перейти к содержанию</a>
<header class="site-header">
    <a class="brand" href="index.php"><img src="assets/mark.svg" width="38" height="38" alt=""><span>Конференции<span class="brand-dot">.РФ</span><small>ПРОСТРАНСТВО ДЛЯ ВАШИХ ИДЕЙ</small></span></a>
    <nav aria-label="Основная навигация">
        <?php if ($user): ?>
        <a href="index.php?page=dashboard" <?= $view==='dashboard'?'aria-current="page"':'' ?>>Мои заявки</a>
        <?php if ($user['role']==='admin'): ?><a href="index.php?page=admin" <?= $view==='admin'?'aria-current="page"':'' ?>>Администратор</a><?php endif ?>
        <form method="post" action="index.php?page=logout"><?= csrf() ?><button class="nav-logout" type="submit">Выйти</button></form>
        <?php else: ?><span class="nav-note">ВСТРЕЧАЙТЕСЬ. ОБСУЖДАЙТЕ. СОЗДАВАЙТЕ.</span><?php endif ?>
    </nav>
</header>
<main id="main" class="container portal-main" tabindex="-1">
    <?php if ($flash): ?><div class="alert alert-success notice" role="status"><?= e($flash) ?></div><?php endif ?>
    <?php if (isset($errors['form'])): ?><div class="alert alert-danger" role="alert"><?= e($errors['form']) ?></div><?php endif ?>
    <?= $content ?>
</main>
<footer class="site-footer"><span>© <?= date('Y') ?> Конференции.РФ</span><span>Место, где начинается диалог</span></footer>
</body>
</html>
