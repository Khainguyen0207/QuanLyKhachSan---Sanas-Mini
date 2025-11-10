'use strict'

$(document).ready(function () {
    const isLogin = localStorage.getItem('isLogin');

    if (isLogin) {
        const account = JSON.parse(localStorage.getItem('accounts'))[isLogin];
        
        $('#login').text(account['name']);
        $('#login-small').text(account['name']);

        paymentGenerate();
        
        isLoginPage();
        isUserPage(isLogin);
    } else {
        if (isPaymentPage()) {
            alert('Vui Lòng đăng nhập để đặt phòng');
            window.location.href = '/';
        }

        isUserPage(false);
    }

    // Password visibility toggle functionality
    $('.toggle-password').on('click', function () {
        const target = $(this).data('bb-target');
        const passwordField = $(`[data-bb-toggle="${target}"]`);

        if (passwordField.attr('type') === 'password') {
            passwordField.attr('type', 'text');
            $(this).removeClass('fa-eye').addClass('fa-eye-slash');
        } else {
            passwordField.attr('type', 'password');
            $(this).removeClass('fa-eye-slash').addClass('fa-eye');
        }
    });

    // Header scroll behavior
    let currentScrollY = 0;
    let previousScrollY = 0;
    const header = $('nav.navbar[data-bs-toggle="main-menu"]');
    const scrollThreshold = 50;

    $(window).on('scroll', function () {
        currentScrollY = window.scrollY;

        if (Math.abs(currentScrollY - previousScrollY) > scrollThreshold) {
            if (currentScrollY > previousScrollY) {
                header.hide();
            } else {
                header.show();
            }
            previousScrollY = currentScrollY;
        }
    });

    $('td.room-pick').on('click', function (event) {
        if ($(event.target).is('button')) {
            const tableRow = $(event.target).closest('tr');
            
            const position = $('.address').text();
            const name = tableRow.find('td a.name-room').text();
            const quantity = tableRow.find('td.capacity').data('quantity');
            const utilities = tableRow.find('td span.utilities').text().trim();
            const price = tableRow.find('td .price1').data('price');
            const coupon = tableRow.find('td .coupon').data('coupon');

            const params = new URLSearchParams({
                position: position,
                name: name,
                quantity: quantity,
                utilities: utilities,
                price: price,
                coupon: coupon
            });

            window.location.href = '../payment/order.html?' + params.toString();
        }
    });
});

$(document).ready(function () {
    function showMessage(message) {
        let messageBox = $('#messageBox');
        if (messageBox.length === 0) {
            messageBox = $('<div>', {
                id: 'messageBox',
                css: {
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
                    type: 'number',
                    min: '0',
                    max: '17',
                    placeholder: `Tuổi trẻ em ${i}`,
                    class: 'child-age-input'
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

function paymentGenerate() {
    if (isPaymentPage()) {
        const params = new URLSearchParams(window.location.search);

        let name = params.get('name');
        let quantity = params.get('quantity');
        let utilities = params.get('utilities');
        let price = params.get('price');
        let position = params.get('position');
        let coupon = params.get('coupon');

        if (!name || !quantity || !utilities || !price || !position || !coupon) {
            alert('Chưa chọn phòng')
            window.location.href = '/';
        }

        let text = '';

        utilities.split(',').forEach(e => {
            text += `<li> ${e} </li>`
        })

        price = parseInt(price)
        coupon = parseInt(coupon)
        let total = (price * 0.9) - coupon

        $('h3.name-room').text(name)
        $('.address').text(position)
        $('.quantity').text(quantity)
        $('.discount').text(coupon.toLocaleString())
        $('.features').html(text)
        $('.original-price').text(price.toLocaleString())
        $('.price').text((price * 0.9).toLocaleString())
        $('.total').text(total.toLocaleString())
        $('input[name="user-email"]').val(localStorage.getItem('isLogin'))
    }
}

function isLoginPage() {
    const url = window.location.href;
    const url_parts = url.split('/');

    if (url_parts[url_parts.length - 2] == 'auth') {
        window.location.href = '../user/profile.html'
    }
}

function isUserPage(isLogin) {
    const url = window.location.href;
    const url_parts = url.split('/');

    if (url_parts[url_parts.length - 2] == 'user') {
        if (isLogin) {
            $('#login').closest('a').attr('href', '#')
            $('#login-small').closest('a').attr('href', '#')

            if (window.location.pathname.includes('profile.html')) {
                loadUserProfile()
            }

            if (window.location.pathname.includes('my-booking.html')) {
                myBookingGenerate(localStorage.getItem('isLogin'))
            }
        } else {
            alert('Vui lòng đăng nhập')
            window.location.href = '/'
        }
    }
}

function isPaymentPage() {
    const url = window.location.href;
    const url_parts = url.split('/');

    if (url_parts[url_parts.length - 2] == 'payment') {
        return true
    }

    return false
}

function loadUserProfile() {
    const email = localStorage.getItem('isLogin');

    const accounts = JSON.parse(localStorage.getItem('accounts')) || [];
    const currentUser = accounts[email];

    if (currentUser) {
        $('.username').text(currentUser.name || 'User');
        $('.email').text(currentUser.email || '');

        // Update booking history if on profile page
        if (window.location.pathname.includes('profile.html')) {
            updateBookingHistory(email);
        }
    }
}

function updateBookingHistory(email) {
    const bookings = getBookings();
    const userBookings = bookings.filter(booking => booking['user-email'] === email);
    
    // Sort bookings by check-in date (most recent first)
    userBookings.sort((a, b) => new Date(b['check-in']) - new Date(a['check-in']));
    
    // Get the table body
    const tableBody = $('.history-table tbody');
    if (!tableBody.length) return;
    
    // Clear existing rows
    tableBody.empty();
    
    // Take only the 5 most recent bookings
    const recentBookings = userBookings.slice(0, 5);

    recentBookings.forEach((booking, index) => {
        const checkIn = new Date(booking['check-in']); 
        const checkOutDate = new Date(checkIn);
        checkOutDate.setDate(checkOutDate.getDate() + 2);

        // Determine status color and icon
        let statusColor = '#00ff12'; // Default green for success
        let statusIcon = 'fa-circle-check';
        let statusText = 'Success';
        
        if (booking['order-status'] === 'pending') {
            statusColor = '#ffa500'; // Orange for pending
            statusIcon = 'fa-clock';
            statusText = 'Pending';
        } else if (booking['order-status'] === 'cancelled') {
            statusColor = '#ff0000'; // Red for cancelled
            statusIcon = 'fa-circle-xmark';
            statusText = 'Cancelled';
        }
        
        const row = $(`
            <tr>
                <td class="p-3">${booking['name']}</td>
                <td class="p-3">${booking['check-in']} → ${checkOutDate}</td>
                <td class="p-3" style="color: ${statusColor}">
                    ${statusText} <i class="fa-solid ${statusIcon}"></i>
                </td>
                <td class="p-3">₫ ${formatPrice(booking['price'])}</td>
                <td class="p-3">
                    <button class="btn btn-primary btn-sm" onclick="toggleBookingDetails(${index})">
                        Details
                    </button>
                </td>
            </tr>
        `);
        
        tableBody.append(row);

        // Add details row
        const detailsRow = $(`
            <tr id="booking-details-${index}" style="display: none;">
                <td colspan="5" class="p-3 border-top">
                    <div class="row">
                        <div class="col-md-6">
                            <p><strong>Email:</strong> <span>${booking['email']}</span></p>
                            <p><strong>Last Name:</strong> <span>${booking['lastName']}</span></p>
                            <p><strong>First Name:</strong> <span>${booking['firstName']}</span></p>
                        </div>
                        <div class="col-md-6">
                            <p><strong>Phone Number:</strong> <span>${booking['phoneNumber']}</span></p>
                            <p><strong>Amenities:</strong> <span>${booking['utilities']}</span></p>
                            <p><strong>Payment Status:</strong> <span>${booking['payment-status']}</span></p>
                        </div>
                    </div>
                </td>
            </tr>
        `);
        tableBody.append(detailsRow);
    });
    
    // Update booking count
    $('.booking-time strong').text(`Bookings: ${userBookings.length}`);
}

function getBookings() {
    return JSON.parse(localStorage.getItem('bookings')) || [];
}

function myBookingGenerate(email) {
    const bookings = getBookings();
    const upcomingContainer = $('.upcoming-tab');
    const completedContainer = $('.completed-content');
    const cancelledContainer = $('.cancelled-content');

    if (!upcomingContainer.length || !completedContainer.length || !cancelledContainer.length) return;

    // Function to add days to a date
    function addDays(dateStr, days) {
        const date = new Date(dateStr);
        date.setDate(date.getDate() + days);
        return date.toISOString().split('T')[0];
    }

    // Function to format price
    function formatPrice(price) {
        return parseInt(price).toLocaleString('vi-VN') + ' VND';
    }

    // Clear existing content
    upcomingContainer.empty();
    completedContainer.empty();
    cancelledContainer.empty();

    // Filter bookings by email
    const userBookings = bookings.filter(booking => booking['user-email'] === email);

    // If no bookings at all, show message in upcoming tab
    if (userBookings.length == 0) {
        upcomingContainer.html(`
            <div class="text-center py-5">
                <h4 class="text-muted">Không có giao dịch gần đây</h4>
            </div>
        `);
        return;
    }

    // Group bookings by status
    const pendingBookings = userBookings.filter(booking => booking['order-status'] === 'pending');
    const completedBookings = userBookings.filter(booking => booking['order-status'] === 'completed');
    const cancelledBookings = userBookings.filter(booking => booking['order-status'] === 'cancelled');

    // Show message if no bookings for each status
    if (pendingBookings.length === 0) {
        upcomingContainer.html(`
            <div class="text-center py-5">
                <h4 class="text-muted">Không có đặt phòng đang chờ</h4>
            </div>
        `);
    }

    if (completedBookings.length === 0) {
        completedContainer.html(`
            <div class="text-center py-5">
                <h4 class="text-muted">Không có đặt phòng đã hoàn thành</h4>
            </div>
        `);
    }

    if (cancelledBookings.length === 0) {
        cancelledContainer.html(`
            <div class="text-center py-5">
                <h4 class="text-muted">Không có đặt phòng đã hủy</h4>
            </div>
        `);
    }

    // Function to generate booking HTML
    function generateBookingHTML(booking, index) {
        const checkOutDate = addDays(booking['check-in'], 2);
        return `
            <div class="booking-wrapper">
                <p class="booking-date">${booking['check-in']}</p>

                <div class="upcoming-board rounded-4 container mb-5">
                    <div class="upcoming-board-header d-flex justify-content-between p-3 align-items-center mb-3 flex-wrap gap-2">
                        <button class="upcoming-contact rounded-4 btn-outline-primary btn-sm px-3">
                            💬 Liên hệ dịch vụ trực tuyến SANAS
                        </button>
                        <span class="fw-bold text-dark">ID: ${booking['email']}</span>
                    </div>
                    <div class="row align-items-center p-3 mb-4">
                        <div class="col-md-8 mb-3 mb-md-0">
                            <h6 class="position mb-1 fw-bold">${booking['name']} <span class="dot">•</span> <span class="address fw-bold">${booking['position']}</span></h6>

                            <div class="d-flex gap-4 text-muted small mt-3">
                                <div>
                                    <div class="text-secondary mb-2">Ngày nhận phòng</div>
                                    <div class="check-in text-dark">${booking['check-in']}</div>
                                </div>
                                <div>
                                    <div class="text-secondary mb-2">Ngày trả phòng</div>
                                    <div class="check-out text-dark">${checkOutDate}</div>
                                </div>
                                <div>
                                    <div class="text-secondary mb-2">Địa điểm</div>
                                    <div class="position text-dark">${booking['position']}</div>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-4 text-md-end d-flex flex-column align-items-md-end align-items-start gap-2">
                            <span class="badge ${booking['payment-status'] === 'unpaid' ? 'bg-warning' : 'bg-success'} px-3 py-2 fs-6 rounded-pill">${booking['payment-status'] === 'unpaid' ? 'Chưa thanh toán' : 'Đã thanh toán'}</span>
                            <button class="btn btn-primary" 
                                    onclick="toggleBookingDetails(${index})">
                                Chi tiết
                            </button>
                        </div>
                    </div>
                    <div id="booking-details-${index}" class="booking-details p-3 border-top" style="display: none;">
                        <div class="row">
                            <div class="col-md-6">
                                <p><strong>Email:</strong> <span>${booking['email']}</span></p>
                                <p><strong>Họ:</strong> <span>${booking['lastName']}</span></p>
                                <p><strong>Tên:</strong> <span>${booking['firstName']}</span></p>
                            </div>
                            <div class="col-md-6">
                                <p><strong>Giá:</strong> <span>${formatPrice(booking['price'])}</span></p>
                                <p><strong>Số điện thoại:</strong> <span>${booking['phoneNumber']}</span></p>
                                <p><strong>Tiện nghi:</strong> <span>${booking['utilities']}</span></p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    // Add bookings to their respective containers
    pendingBookings.forEach((booking, index) => {
        upcomingContainer.append(generateBookingHTML(booking, index));
    });

    completedBookings.forEach((booking, index) => {
        completedContainer.append(generateBookingHTML(booking, index));
    });

    cancelledBookings.forEach((booking, index) => {
        cancelledContainer.append(generateBookingHTML(booking, index));
    });
}

// Function to toggle booking details
window.toggleBookingDetails = function (index) {
    const detailsElement = $(`#booking-details-${index}`);
    const button = $(event.target);

    if (detailsElement.length) {
        if (detailsElement.is(':hidden')) {
            detailsElement.show();
            button.text('Ẩn');
        } else {
            detailsElement.hide();
            button.text('Chi tiết');
        }
    }
}

document.addEventListener('DOMContentLoaded', function () {
    document.querySelector('button.btn-pay[data-action="order-information-form-submit"]').addEventListener('click', function (event) {
        event.preventDefault();

        const form = document.querySelector('form[data-bs-toggle="order-information-form"]');
        let isValid = true;
        const inputs = form.querySelectorAll('input, select, textarea');

        inputs.forEach(input => {
            if (! validateInput(input)) {
                isValid = false;
            }
        });
       
        if (isValid) {
            const urlParams = new URLSearchParams(window.location.search);

            urlParams.forEach((value, key) => {
                if (!form.querySelector(`[name="${key}"]`)) {
                    const input = document.createElement('input');
                    input.type = 'hidden';
                    input.name = key;
                    input.value = value;
                    form.appendChild(input);
                }
            });

            form.submit();
        }
    });
});

function validateInput(input) {
    const messageElement = input.parentElement.querySelector('.validation-message');
    let isValid = true;
    let errorMessage = '';
    let checkInDate = new Date();

    input.classList.remove('is-invalid', 'is-valid');

    if (!input.value.trim()) {
        isValid = false;
        errorMessage = 'Vui lòng nhập thông tin';
    } else if (input.getAttribute('name') === 'checkin') {
        checkInDate = new Date(input.value);
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        if (checkInDate < today) {
            isValid = false;
            errorMessage = 'Ngày check-in không được nhỏ hơn ngày hiện tại';
        }
    } else if (input.getAttribute('name') === 'checkout') {
        const checkOutDate = new Date(input.value);
        const inputCheckin = document.querySelector('input[name="checkin"]');
        const checkInDateHere = new Date(inputCheckin.value);

        if (checkOutDate < checkInDateHere) {
            isValid = false;
            errorMessage = 'Ngày check-out không được nhỏ hơn ngày check-in';
        }
    } else if (input.getAttribute('name') === 'email') {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(input.value)) {
            isValid = false;
            errorMessage = 'Email không hợp lệ';
        }
    } else if (input.getAttribute('name') === 'password') {
        if (input.value.length < 6) {
            isValid = false;
            errorMessage = 'Mật khẩu phải có ít nhất 6 ký tự';
        }
    }

    if (isValid) {
        input.classList.add('is-valid');
    } else {
        input.classList.add('is-invalid');
        messageElement.textContent = errorMessage;
    }


    return isValid;
}
