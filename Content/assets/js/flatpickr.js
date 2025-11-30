

(function ($) {
    'use strict';

    $(document).ready(function () {

        $('[data-fd-component="flatpickr"]').each(function () {
            const $el = $(this);
            const mode = $el.attr('data-flatpickr-mode') || 'single';
            const minDate = $el.attr('data-flatpickr-minDate') || null;

            flatpickr(this, {
                mode: mode,
                minDate: minDate,
                defaultDate: mode === 'range'
                    ? ["today", new Date().fp_incr(1)]
                    : "today"
            });
        });

        $('[data-fd-component="flatpickr-month"]').each(function () {
            const $el = $(this);
            const mode = $el.attr('data-flatpickr-mode') || 'single';
            const minDate = $el.attr('data-flatpickr-minDate') || null;

            flatpickr(this, {
                plugins: [
                    new monthSelectPlugin({
                        shorthand: true, //defaults to false
                        dateFormat: "m.y", //defaults to "F Y"
                        altFormat: "F Y", //defaults to "F Y"
                        theme: "dark" // defaults to "light"
                    })
                ]
            });
        });
    });
})(jQuery);


