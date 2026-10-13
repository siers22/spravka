<?php require __DIR__.'/fields.php'; ?>
<div class="auth-grid">
    <section class="auth-story"><span class="eyebrow">ЛЮДИ. ИДЕИ. ПРОСТРАНСТВА.</span><h1>Большие идеи<br>начинаются<br>со встречи<span>.</span></h1><p>Найдите место для вашей конференции.<br>Мы позаботимся о пространстве.</p><img class="story-image" src="assets/auditorium.webp" alt="Пространство для конференции" width="720" height="480"><span class="image-caption">ОТ ПЕРВОГО ВОПРОСА — К НОВЫМ РЕШЕНИЯМ</span></section>
    <section class="form-panel"><span class="eyebrow">С ВОЗВРАЩЕНИЕМ</span><h2>Войти в аккаунт</h2><p class="muted">Ваши встречи и заявки — в одном месте.</p>
        <form method="post" action="index.php?page=login" class="portal-form" novalidate>
            <?= csrf() ?>
            <?php $field('login','Логин','text','', 'autocomplete="username" maxlength="64"'); ?>
            <?php $field('password','Пароль','password','', 'autocomplete="current-password" maxlength="72"'); ?>
            <button class="btn btn-primary w-100" type="submit">Войти <span aria-hidden="true">↗</span></button>
        </form>
        <p class="form-alternative">Еще не зарегистрированы? <a href="index.php?page=register">Регистрация</a></p>
        <div class="trust-note"><span aria-hidden="true">✓</span> Только ваши заявки. Только нужные встречи.</div>
    </section>
</div>
