$(document).ready(() => {
    if ($('[data-fd-toggle="password-toggle-btn"]') != undefined) {
        $('[data-fd-toggle="password-toggle-btn"]').on('click', function () {
            const $group = $(this).closest('.input-group');  
            const $input = $group.find('input[data-fd-toggle="password-toggle"]');
            const $button = $group.find('[data-fd-toggle="password-toggle-btn"]');

            const currentType = $input.attr('type');

            if (currentType === 'password') {
                $input.attr('type', 'text');
                $button.toggleClass('mdi-eye-closed')

            } else if (currentType === 'text') {
                $input.attr('type', 'password');
                $button.toggleClass('mdi-eye-closed')
            }
        });
    }
})