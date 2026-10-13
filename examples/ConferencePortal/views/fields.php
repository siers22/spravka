<?php
// Shared, escaped field renderer; no password is ever repopulated.
$field = function (string $name, string $label, string $type='text', string $help='', string $attributes='') use ($errors, $old): void {
    $invalid = isset($errors[$name]);
    ?>
    <div class="field">
        <label class="form-label" for="<?= e($name) ?>"><?= e($label) ?></label>
        <div class="<?= $type==='password'?'password-wrap':'' ?>">
            <input class="form-control <?= $invalid?'is-invalid':'' ?>" id="<?= e($name) ?>" name="<?= e($name) ?>" type="<?= e($type) ?>" value="<?= $type==='password'?'':e($old[$name]??'') ?>" required <?= $attributes ?> aria-describedby="<?= e($name) ?>-help <?= e($name) ?>-error" <?= $invalid?'aria-invalid="true"':'' ?>>
            <?php if ($type==='password'): ?><button type="button" class="password-toggle" aria-controls="<?= e($name) ?>" aria-pressed="false">Показать</button><?php endif ?>
        </div>
        <small id="<?= e($name) ?>-help" class="field-help"><?= e($help) ?></small>
        <span id="<?= e($name) ?>-error" class="field-error" <?= $invalid?'role="alert"':'' ?>><?= e($errors[$name]??'') ?></span>
    </div>
    <?php
};
