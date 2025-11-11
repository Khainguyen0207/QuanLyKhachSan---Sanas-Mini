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

$(document).ready(() => {

    function showMessage(message) {
        let messageBox = $('#messageBox');
        if (messageBox.length === 0) {
            messageBox = $('<div>', {
                id: 'messageBox', css: {
                    position: 'fixed',
                    top: '20px',
                    left: '50%',
                    transform: 'translateX(-50%)',
                    backgroundColor: '#f8d7da',
                    color: '#721c24',
                    padding: '10px 20px',
                    borderRadius: '5px',
                    border: '1px solid #f5c6cb',
                    zIndex: '1000',
                    display: 'none'
                }
            }).appendTo('body');
        }
        messageBox.text(message).show();
        setTimeout(() => {
            messageBox.hide();
        }, 3000);
    }

    window.toggleGuestBox = function () {
        const guestBox = $('#guestBox');
        guestBox.toggle();
        updateGuestSummary();
    };

    window.changeValue = function (type, delta) {
        const element = $(`#${type}`);
        let value = parseInt(element.text());

        value = Math.max(0, value + delta);

        if (type === 'rooms') {
            const adults = parseInt($('#adults').text());
            if (value > adults) {
                showMessage("Số phòng không được vượt quá số người lớn!");
                return;
            }
        }

        if (type === 'children') {
            if (value > 10) {
                showMessage("Tối đa 10 trẻ em!");
                return;
            }
        }

        element.text(value);
        updateGuestSummary();
        updateChildrenAgeInputs();
    };

    window.updateGuestSummary = function () {
        const adults = $('#adults').text();
        const children = $('#children').text();
        const rooms = $('#rooms').text();
        $('#guestSummary').text(`${adults} Người lớn, ${children} Trẻ em, ${rooms} Phòng`);
    };

    window.updateChildrenAgeInputs = function () {
        const children = parseInt($('#children').text());
        const container = $('#childrenAges');

        container.empty();

        if (children >= 1) {
            for (let i = 1; i <= children; i++) {
                $('<input>', {
                    type: 'number', min: '0', max: '17', placeholder: `Tuổi trẻ em ${i}`, class: 'child-age-input'
                }).appendTo(container);
            }
        }
    };

    const promoContainer = $('.promo-container');

    let isDown = false;
    let startX;
    let scrollLeft;

    promoContainer.on('mousedown', function (e) {
        isDown = true;
        $(this).addClass('active');
        startX = e.pageX - $(this).offset().left;
        scrollLeft = $(this).scrollLeft();
    });

    promoContainer.on('mouseleave mouseup', function () {
        isDown = false;
        $(this).removeClass('active');
    });

    promoContainer.on('mousemove', function (e) {
        if (!isDown) return;
        e.preventDefault();
        const x = e.pageX - $(this).offset().left;
        const walk = (x - startX) * 1.5;
        $(this).scrollLeft(scrollLeft - walk);
    });

    // Initial updates when the document loads
    updateGuestSummary();
    updateChildrenAgeInputs();

});
