(function ($) {
    'use strict';

    $(document).ready(function () {
        const resort = $('select[data-fd-component="select2"]');

        if (resort.length) {
            resort.select2();
        }
    })
})(jQuery);


