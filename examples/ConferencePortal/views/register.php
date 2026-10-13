<?php require __DIR__.'/fields.php'; ?>
<div class="register-grid"><section class="register-story"><span class="eyebrow">ДАВАЙТЕ ЗНАКОМИТЬСЯ</span><h1>Место для<br>вашей следующей<br>встречи<span>.</span></h1><p>Один аккаунт — все возможности портала.</p><div class="story-benefit"><b>01</b><div><h3>Выбирайте пространство</h3><p>Аудитория, коворкинг или кинозал.</p></div></div><div class="story-benefit"><b>02</b><div><h3>Планируйте встречи</h3><p>Удобные дата, время и способ оплаты.</p></div></div><div class="story-benefit"><b>03</b><div><h3>Следите за заявками</h3><p>Статусы и обратная связь в кабинете.</p></div></div></section>
<section class="form-panel"><span class="eyebrow">НОВЫЙ АККАУНТ</span><h2>Регистрация</h2><p class="muted">Все поля обязательны для заполнения.</p>
<form method="post" action="index.php?page=register" class="portal-form" novalidate>
    <?= csrf() ?>
    <?php $field('full_name','ФИО','text','Кириллица и пробелы', 'autocomplete="name" maxlength="150"'); ?>
    <?php $field('login','Логин','text','От 6 латинских букв и цифр', 'autocomplete="username" minlength="6" maxlength="64"'); ?>
    <?php $field('password','Пароль','password','От 8 символов, максимум 72 байта', 'autocomplete="new-password" minlength="8" maxlength="72"'); ?>
    <div class="form-row">
    <?php $field('phone','Телефон','tel','8(999)123-45-67', 'autocomplete="tel" maxlength="16" placeholder="8(999)123-45-67"'); ?>
    <?php $field('email','Электронная почта','email','', 'autocomplete="email" maxlength="254"'); ?>
    </div>
    <button class="btn btn-primary w-100" type="submit">Создать пользователя <span aria-hidden="true">↗</span></button>
</form><p class="form-alternative">Уже есть аккаунт? <a href="index.php?page=login">Войти</a></p></section></div>
