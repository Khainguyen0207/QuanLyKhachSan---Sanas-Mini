(function ($) {
    'use strict';

    $(document).ready(function () {
        const resort = $("#resort_id");
        console.log('OK')

        if (resort.length) {
            resort.select2();
            console.log('OK')
        }
    })
})(jQuery);
