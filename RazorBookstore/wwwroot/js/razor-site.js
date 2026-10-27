document.querySelectorAll('[data-menu]').forEach(button => button.addEventListener('click', () => { const menu = document.getElementById(button.dataset.menu); const open = menu.classList.toggle('is-open'); button.setAttribute('aria-expanded', String(open)); }));
document.querySelectorAll('form[data-confirm]').forEach(form => form.addEventListener('submit', e => { if (!confirm(form.dataset.confirm)) e.preventDefault(); }));
const file = document.getElementById('Cover');
let previewUrl;
if (file) file.addEventListener('change', () => { const image = document.getElementById('upload-preview'); if (previewUrl) URL.revokeObjectURL(previewUrl); const selected = file.files[0]; if (selected) { previewUrl = URL.createObjectURL(selected); image.src = previewUrl; image.hidden = false; } });
