// Event delegation also works when Blazor navigates without reloading the page.
if (!window.mocHeaderReady) {
    window.mocHeaderReady = true;
    document.addEventListener('click', event => {
        document.querySelectorAll('.store-header details[open]').forEach(menu => {
            if (!menu.contains(event.target) || event.target.closest('a')) menu.removeAttribute('open');
        });
    });
    document.addEventListener('keydown', event => {
        if (event.key !== 'Escape') return;
        document.querySelectorAll('.store-header details[open]').forEach(menu => {
            menu.removeAttribute('open');
            menu.querySelector('summary').focus();
        });
    });
}

