'use strict';
// Progressive enhancement: registration, filters and booking also work without JS.
document.querySelectorAll('.password-toggle').forEach(button => {
    button.addEventListener('click', () => {
        const field = document.getElementById(button.getAttribute('aria-controls'));
        const showing = field.type === 'password';
        field.type = showing ? 'text' : 'password';
        button.textContent = showing ? 'Скрыть' : 'Показать';
        button.setAttribute('aria-pressed', String(showing));
    });
});

const roomSelect = document.getElementById('room_id');
const startField = document.getElementById('starts_at');
const summary = document.getElementById('booking-summary');
function updateSummary() {
    if (!summary) return;
    const room = roomSelect.selectedOptions[0];
    const date = startField.value ? new Date(startField.value) : null;
    summary.textContent = roomSelect.value && date && !Number.isNaN(date.getTime())
        ? `${room.textContent} · ${date.toLocaleString('ru-RU', {dateStyle: 'long', timeStyle: 'short'})} МСК`
        : 'Выберите помещение и дату для вашей встречи.';
}
document.querySelectorAll('[data-room]').forEach(button => {
    button.addEventListener('click', () => {
        roomSelect.value = button.dataset.room;
        updateSummary();
        roomSelect.focus({preventScroll: true});
        roomSelect.scrollIntoView({behavior: matchMedia('(prefers-reduced-motion: reduce)').matches ? 'instant' : 'smooth', block: 'center'});
    });
});
roomSelect?.addEventListener('change', updateSummary);
startField?.addEventListener('change', updateSummary);
updateSummary();

// Server validation is authoritative. This check only gives quicker inline feedback.
document.querySelectorAll('.portal-form').forEach(form => {
    form.addEventListener('submit', event => {
        let first = null;
        form.querySelectorAll('input[required], select[required]').forEach(field => {
            const valid = field.value.trim() !== '' && field.validity.valid;
            field.classList.toggle('is-invalid', !valid);
            field.setAttribute('aria-invalid', String(!valid));
            const error = document.getElementById(`${field.id}-error`);
            if (error && !valid) error.textContent = field.validity.typeMismatch ? 'Проверьте формат значения.' : 'Заполните поле в указанном формате.';
            else if (error && valid) error.textContent = '';
            if (!valid && !first) first = field;
        });
        if (first) { event.preventDefault(); first.focus(); }
    });
});
