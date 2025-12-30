// =============================================
// VCB Digibank - Authentication JavaScript
// =============================================

// API Base URL (sẽ cấu hình sau khi kết nối API)
const API_BASE_URL = 'https://localhost:7079/api';

// =============================================
// TOAST NOTIFICATION
// =============================================
function showToast(message, type = 'success') {
    const toast = document.getElementById('toast');
    const toastMessage = document.getElementById('toastMessage');
    const toastIcon = toast.querySelector('.toast-icon i');
    
    toast.classList.remove('show', 'error', 'warning');
    
    if (type === 'error') {
        toast.classList.add('error');
        toastIcon.className = 'fas fa-times-circle';
    } else if (type === 'warning') {
        toast.classList.add('warning');
        toastIcon.className = 'fas fa-exclamation-triangle';
    } else {
        toastIcon.className = 'fas fa-check-circle';
    }
    
    toastMessage.textContent = message;
    toast.classList.add('show');
    
    setTimeout(() => {
        toast.classList.remove('show');
    }, 4000);
}

// =============================================
// PASSWORD TOGGLE
// =============================================
function togglePassword(inputId, iconElement) {
    const passwordInput = document.getElementById(inputId);
    const icon = iconElement.querySelector('i');
    
    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        icon.classList.remove('fa-eye');
        icon.classList.add('fa-eye-slash');
    } else {
        passwordInput.type = 'password';
        icon.classList.remove('fa-eye-slash');
        icon.classList.add('fa-eye');
    }
}

// =============================================
// VALIDATION HELPERS
// =============================================
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function isValidPhone(phone) {
    const phoneRegex = /^(0|\+84)[0-9]{9,10}$/;
    return phoneRegex.test(phone.replace(/\s/g, ''));
}

// =============================================
// LOGIN FORM HANDLER
// =============================================
function handleLogin(e) {
    e.preventDefault();
    
    const username = document.getElementById('loginUsername').value.trim();
    const password = document.getElementById('loginPassword').value;
    
    // Validation
    if (!username) {
        showToast('Vui lòng nhập tên đăng nhập', 'error');
        document.getElementById('loginUsername').focus();
        return;
    }
    
    if (!password) {
        showToast('Vui lòng nhập mật khẩu', 'error');
        document.getElementById('loginPassword').focus();
        return;
    }
    
    const submitBtn = document.querySelector('#loginForm .submit-btn');
    submitBtn.classList.add('loading');
    
    const loginData = {
        tenDangNhap: username,
        matKhau: password
    };
    
    // Gọi API đăng nhập
    callLoginAPI(loginData)
        .finally(() => {
            submitBtn.classList.remove('loading');
        });
}

// =============================================
// REGISTER FORM HANDLER
// =============================================
function handleRegister(e) {
    e.preventDefault();
    
    const username = document.getElementById('regUsername').value.trim();
    const password = document.getElementById('regPassword').value;
    const confirmPassword = document.getElementById('regConfirmPassword').value;
    const fullName = document.getElementById('regFullName').value.trim();
    const email = document.getElementById('regEmail').value.trim();
    const phone = document.getElementById('regPhone').value.trim();
    
    // Validation
    if (!username || username.length < 4) {
        showToast('Tên đăng nhập phải có ít nhất 4 ký tự', 'error');
        document.getElementById('regUsername').focus();
        return;
    }
    
    if (!password || password.length < 6) {
        showToast('Mật khẩu phải có ít nhất 6 ký tự', 'error');
        document.getElementById('regPassword').focus();
        return;
    }
    
    if (password !== confirmPassword) {
        showToast('Mật khẩu xác nhận không khớp', 'error');
        document.getElementById('regConfirmPassword').focus();
        return;
    }
    
    if (!fullName) {
        showToast('Vui lòng nhập họ và tên', 'error');
        document.getElementById('regFullName').focus();
        return;
    }
    
    if (email && !isValidEmail(email)) {
        showToast('Email không hợp lệ', 'error');
        document.getElementById('regEmail').focus();
        return;
    }
    
    if (phone && !isValidPhone(phone)) {
        showToast('Số điện thoại không hợp lệ', 'error');
        document.getElementById('regPhone').focus();
        return;
    }
    
    const submitBtn = document.querySelector('#registerForm .submit-btn');
    submitBtn.classList.add('loading');
    
    const registerData = {
        tenDangNhap: username,
        matKhau: password,
        hoTen: fullName,
        email: email,
        soDienThoai: phone
    };
    
    // Gọi API đăng ký
    callRegisterAPI(registerData)
        .finally(() => {
            submitBtn.classList.remove('loading');
        });
}

// =============================================
// API FUNCTIONS (Chuẩn bị sẵn)
// =============================================
async function callLoginAPI(data) {
    try {
        const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        
        const result = await response.json();
        
        if (response.ok) {
            localStorage.setItem('token', result.token);
            localStorage.setItem('role', result.role);
            localStorage.setItem('fullName', result.fullName);
            
            showToast('Đăng nhập thành công!', 'success');
            
            setTimeout(() => {
                if (result.role === 'ADMIN') {
                    window.location.href = 'admin-dashboard.html';
                } else if (result.role === 'STAFF') {
                    window.location.href = 'staff-dashboard.html';
                } else {
                    window.location.href = 'dashboard.html';
                }
            }, 1000);
        } else {
            showToast(result.thongBao || 'Đăng nhập thất bại', 'error');
        }
    } catch (error) {
        console.error('Login error:', error);
        showToast('Lỗi kết nối server', 'error');
    }
}

async function callRegisterAPI(data) {
    try {
        const response = await fetch(`${API_BASE_URL}/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        
        const result = await response.json();
        
        if (response.ok) {
            showToast(result.thongBao || 'Đăng ký thành công!', 'success');
            setTimeout(() => {
                window.location.href = 'index.html';
            }, 1500);
        } else {
            showToast(result.thongBao || 'Đăng ký thất bại', 'error');
        }
    } catch (error) {
        console.error('Register error:', error);
        showToast('Lỗi kết nối server', 'error');
    }
}

// =============================================
// INITIALIZATION
// =============================================
document.addEventListener('DOMContentLoaded', function() {
    // Login form
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
    }
    
    // Register form
    const registerForm = document.getElementById('registerForm');
    if (registerForm) {
        registerForm.addEventListener('submit', handleRegister);
    }
    
    console.log('VCB Digibank initialized!');
});
