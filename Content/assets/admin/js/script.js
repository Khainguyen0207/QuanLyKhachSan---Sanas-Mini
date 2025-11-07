const html = $('html');

// Toggle dark mode

$(document).ready(() => {

    $('#darkModeToggle').on('click', function () {
        if (html.hasClass('dark')) {
            html.removeClass('dark');
            html.addClass('light');
            localStorage.setItem('theme', 'light');
        } else {
            html.removeClass('light');
            html.addClass('dark');

            localStorage.setItem('theme', 'dark');
        }
    });

    $('button[type="reset"][data-bb-toggle="btn-with-href"]').on('click', function (e) {
        e.preventDefault();

        window.location.href = $(this).data('url');
    })

    $('a[data-bs-action="modal-confirm-action"][data-bs-target="#modal-confirm-delete"]').on('click', function () {
        $('#modal-confirm-action').attr('action', $(this).attr('href'))
    })
})
