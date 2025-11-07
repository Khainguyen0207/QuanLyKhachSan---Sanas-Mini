'use strict'

document.addEventListener('DOMContentLoaded', function () {
    userGenerate();
    console.log(getBookings());

    const form = document.querySelector('.needs-validation');
    const inputs = form.querySelectorAll('input[required]');

    inputs.forEach(input => {
        input.addEventListener('input', function () {
            validateInput(this);
        });
    });

    document.querySelector('form[data-bb-toggle="login-form-toggle"]').addEventListener('submit', function (event) {
        event.preventDefault();

        const form = event.currentTarget;
        let isValid = true;

        inputs.forEach(input => {
            if (!validateInput(input)) {
                isValid = false;
            }
        });

        if (isValid) {
            const email = form.querySelector('input[name="email"]').value.trim();
            const password = form.querySelector('input[name="password"]').value;

            const formData = {
                email: email,
                password: password
            };

            const result = login(formData);

            if (result) {
                alert('Đăng nhập tài khoản thành công');
                window.location.href = '/index.html';
            } else {
                alert('Tài khoản hoặc mật khẩu không chính xác');
            }
        }
    });

    document.querySelector('form[data-bb-toggle="register-form-toggle"]').addEventListener('submit', function (event) {
        event.preventDefault();
        const form = event.currentTarget;
        let isValid = true;

        inputs.forEach(input => {
            if (!validateInput(input)) {
                isValid = false;
            }
        });

        const password = form.querySelector('input[name="password"]');
        const confirmPassword = form.querySelector('input[name="confirm-password"]');

        if (password.value !== confirmPassword.value) {
            const confirmPasswordMessage = confirmPassword.parentElement.querySelector('.validation-message');
            confirmPasswordMessage.textContent = 'Mật khẩu không khớp';
            confirmPassword.classList.add('is-invalid');
            confirmPassword.classList.remove('is-valid');
            isValid = false;
        }

        if (isValid) {
            const email = form.querySelector('input[name="email"]').value.trim();
            const name = form.querySelector('input[name="name"]').value.trim();
            const pass = password.value;

            const formData = {
                name: name,
                email: email,
                password: pass
            };

            if (checkExistAccount(email) || !register(formData)) {
                alert('Email đã được đăng ký');
            } else {
                alert('Đăng ký tài khoản thành công');
                window.location.href = 'login.html';
            }
        }
    });

    document.querySelector('button[data-bb-toggle="logout"]').addEventListener('click', function () {
        logout();
        window.location.href = '/';
    });
});

// Payment
document.addEventListener('DOMContentLoaded', function () {
    document.querySelector('button.btn-pay[data-action="order-information-form-submit"]').addEventListener('click', function (event) {
        event.preventDefault();

        const form = document.querySelector('form[data-bs-toggle="order-information-form"]');
        let isValid = true;
        const inputs = form.querySelectorAll('input, select, textarea');

        inputs.forEach(input => {
            if (!validateInput(input)) {
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

    document.querySelector('button.btn-pay[data-action="order-processing-form-submit"]').addEventListener('click', function (event) {
        event.preventDefault();

        const form = document.querySelector('form[data-bs-toggle="order-processing-form"]');
        let isValid = true;
        const inputs = form.querySelectorAll('input, select, textarea');

        inputs.forEach(input => {
            if (!validateInput(input)) {
                isValid = false;
            }
        });

        const formDataInput = {};

        inputs.forEach(input => {
            const name = input.getAttribute('name');
            const value = input.value;

            if (name) {
                if (input.type === 'checkbox') {
                    if (input.checked) {
                        if (formDataInput[name] === undefined) {
                            formDataInput[name] = [];
                        } else if (!Array.isArray(formDataInput[name])) {
                            formDataInput[name] = [formDataInput[name]];
                        }
                        formDataInput[name].push(value);
                    }
                } else if (input.type === 'radio') {
                    if (input.checked) {
                        formDataInput[name] = value;
                    }
                } else {
                    formDataInput[name] = value;
                }
            }
        });

        if (isValid) {
            const urlParams = new URLSearchParams(window.location.search);
            const formData = {};

            urlParams.forEach((value, key) => {
                formData[key] = value;
            });
          
            addBooking({...formData, ...formDataInput});

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

function addBooking(formData) {
    let bookings = JSON.parse(localStorage.getItem('bookings')) || [];
    bookings.push(formData);
    localStorage.setItem('bookings', JSON.stringify(bookings));
    return true;
}

function getBookings() {
    return JSON.parse(localStorage.getItem('bookings')) || [];
}

function removeBookings() {
    localStorage.removeItem('bookings');
}

function register(formData) {
    const email = formData['email'];
    let accounts = JSON.parse(localStorage.getItem('accounts')) || {};
    accounts[email] = formData;
    localStorage.setItem('accounts', JSON.stringify(accounts));
    return true;
}

function login(formData) {
    const email = formData['email'];
    const password = formData['password'];
    let accounts = JSON.parse(localStorage.getItem('accounts')) || {};
    let account = accounts[email];

    if (checkExistAccount(account)) {
        return false;
    }

    if (account['password'] !== password) {
        return false;
    }

    localStorage.setItem('isLogin', account['email']);
    return true;
}

function checkExistAccount(email) {
    let accounts = JSON.parse(localStorage.getItem('accounts')) || {};
    let account = accounts[email];
    return !account;
}

function logout() {
    localStorage.removeItem('isLogin');
}

function validateInput(input) {
    const messageElement = input.parentElement.querySelector('.validation-message');
    let isValid = true;
    let errorMessage = '';

    input.classList.remove('is-invalid', 'is-valid');

    if (!input.value.trim()) {
        isValid = false;
        errorMessage = 'Vui lòng nhập thông tin';
    } else if (input.getAttribute('name') === 'check-in') {
        const checkInDate = new Date(input.value);
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        if (checkInDate < today) {
                isValid = false;
            errorMessage = 'Ngày check-in không được nhỏ hơn ngày hiện tại';
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
        messageElement.textContent = '';
    } else {
        input.classList.add('is-invalid');
        messageElement.textContent = errorMessage;
    }

    return isValid;
}

function userGenerate() {
    const users = getUsers();
    const userContainer = document.querySelector('.user-container');
    
    if (!userContainer) return;

    // Clear existing content
    userContainer.innerHTML = '';

    // If no users, show message
    if (users.length === 0) {
        userContainer.innerHTML = `
            <div class="text-center py-5">
                <h4 class="text-muted">Không có người dùng nào</h4>
            </div>
        `;
        return;
    }

    // Generate user cards
    users.forEach(user => {
        const userCard = `
            <div class="col-md-6 col-lg-4 mb-4">
                <div class="card h-100">
                    <div class="card-body">
                        <h5 class="card-title">${user.firstName} ${user.lastName}</h5>
                        <p class="card-text">
                            <strong>Email:</strong> ${user.email}<br>
                            <strong>Số điện thoại:</strong> ${user.phoneNumber}<br>
                            <strong>Vai trò:</strong> ${user.role}
                        </p>
                        <button class="btn btn-primary" onclick="editUser('${user.email}')">
                            Chỉnh sửa
                        </button>
                        <button class="btn btn-danger" onclick="deleteUser('${user.email}')">
                            Xóa
                        </button>
                    </div>
                </div>
            </div>
        `;
        userContainer.insertAdjacentHTML('beforeend', userCard);
    });
}

function editUser(email) {
    const users = getUsers();
    const user = users.find(u => u.email === email);
    
    if (!user) {
        alert('Không tìm thấy người dùng!');
        return;
    }

    const modal = document.getElementById('editUserModal');
    const form = document.getElementById('editUserForm');
    
    // Fill form with user data
    form.querySelector('[name="email"]').value = user.email;
    form.querySelector('[name="firstName"]').value = user.firstName;
    form.querySelector('[name="lastName"]').value = user.lastName;
    form.querySelector('[name="phoneNumber"]').value = user.phoneNumber;
    form.querySelector('[name="role"]').value = user.role;

    // Show modal
    const modalInstance = new bootstrap.Modal(modal);
    modalInstance.show();
}

function deleteUser(email) {
    if (confirm('Bạn có chắc chắn muốn xóa người dùng này?')) {
        const users = getUsers();
        const updatedUsers = users.filter(user => user.email !== email);
        localStorage.setItem('users', JSON.stringify(updatedUsers));
        userGenerate();
    }
}

// Handle edit user form submission
document.getElementById('editUserForm')?.addEventListener('submit', function(e) {
    e.preventDefault();
    
    const formData = new FormData(this);
    const email = formData.get('email');
    const firstName = formData.get('firstName');
    const lastName = formData.get('lastName');
    const phoneNumber = formData.get('phoneNumber');
    const role = formData.get('role');

    const users = getUsers();
    const userIndex = users.findIndex(u => u.email === email);
    
    if (userIndex === -1) {
        alert('Không tìm thấy người dùng!');
        return;
    }

    // Update user data
    users[userIndex] = {
        ...users[userIndex],
        firstName,
        lastName,
        phoneNumber,
        role
    };

    localStorage.setItem('users', JSON.stringify(users));
    
    // Hide modal
    const modal = document.getElementById('editUserModal');
    const modalInstance = bootstrap.Modal.getInstance(modal);
    modalInstance.hide();
    
    // Refresh user list
    userGenerate();
});

