// ====================
// CONFIG & UTILITIES
// ====================
const API_URL = '/api';

// Toast notification
function showToast(message, type = 'success') {
    const toast = document.getElementById('toast');
    const messageEl = document.getElementById('toastMessage');
    const iconEl = toast.querySelector('.toast-icon i');

    toast.classList.remove('show', 'error', 'warning');

    const icons = { error: 'fa-times-circle', warning: 'fa-exclamation-triangle', success: 'fa-check-circle' };

    if (type !== 'success') toast.classList.add(type);
    iconEl.className = `fas ${icons[type]}`;

    messageEl.textContent = message;
    toast.classList.add('show');

    setTimeout(() => toast.classList.remove('show'), 4000);
}

// Toggle password visibility
function togglePassword(inputId, iconBtn) {
    const input = document.getElementById(inputId);
    const icon = iconBtn.querySelector('i');
    input.type = input.type === 'password' ? 'text' : 'password';
    icon.classList.toggle('fa-eye');
    icon.classList.toggle('fa-eye-slash');
}

// Validation
const validators = {
    email: (email) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email),
    phone: (phone) => /^(0|\+84)[0-9]{9,10}$/.test(phone.replace(/\s/g, ''))
};

// API call helper
async function callAPI(endpoint, data, options = {}) {
    try {
        const response = await fetch(`${API_URL}${endpoint}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', ...options.headers },
            body: JSON.stringify(data)
        });

        const result = await response.json();

        if (response.ok) return result;
        throw new Error(result.thongBao || 'Thất bại');
    } catch (error) {
        console.error(`API Error: ${endpoint}`, error);
        showToast(error.message || 'Lỗi kết nối server', 'error');
        throw error;
    }
}

// Save user data to localStorage
function saveUserData(data) {
    localStorage.setItem('token', data.token);
    localStorage.setItem('role', data.role);
    localStorage.setItem('fullName', data.fullName);
}

// Redirect based on role
function redirectByRole(role) {
    const redirect = role === 'CUSTOMER' ? 'html/dashboard.html' : 'html/admin-dashboard.html';
    setTimeout(() => window.location.href = redirect, 1000);
}

// ====================
// FORM HANDLERS
// ====================

// Login
async function handleLogin(event) {
    event.preventDefault();

    const username = document.getElementById('loginUsername').value.trim();
    const password = document.getElementById('loginPassword').value;

    if (!username || !password) {
        showToast('Vui lòng nhập tên đăng nhập và mật khẩu', 'error');
        return;
    }

    const btn = event.target.querySelector('.submit-btn');
    btn.classList.add('loading');

    try {
        const result = await callAPI('/auth/login', { tenDangNhap: username, matKhau: password });
        saveUserData(result);
        showToast('Đăng nhập thành công!', 'success');
        redirectByRole(result.role);
    } finally {
        btn.classList.remove('loading');
    }
}

// Register
async function handleRegister(event) {
    event.preventDefault();

    const data = {
        tenDangNhap: document.getElementById('regUsername').value.trim(),
        matKhau: document.getElementById('regPassword').value,
        xacNhanMatKhau: document.getElementById('regConfirmPassword').value,
        hoTen: document.getElementById('regFullName').value.trim(),
        email: document.getElementById('regEmail').value.trim(),
        soDienThoai: document.getElementById('regPhone').value.trim()
    };

    // Validations
    if (data.tenDangNhap.length < 4) {
        showToast('Tên đăng nhập phải có ít nhất 4 ký tự', 'error');
        return;
    }

    if (data.matKhau.length < 6) {
        showToast('Mật khẩu phải có ít nhất 6 ký tự', 'error');
        return;
    }

    if (data.matKhau !== data.xacNhanMatKhau) {
        showToast('Mật khẩu xác nhận không khớp', 'error');
        return;
    }

    if (!data.hoTen) {
        showToast('Vui lòng nhập họ và tên', 'error');
        return;
    }

    if (data.email && !validators.email(data.email)) {
        showToast('Email không hợp lệ', 'error');
        return;
    }

    if (data.soDienThoai && !validators.phone(data.soDienThoai)) {
        showToast('Số điện thoại không hợp lệ', 'error');
        return;
    }

    const btn = event.target.querySelector('.submit-btn');
    btn.classList.add('loading');

    try {
        await callAPI('/auth/register', data);
        showToast('Đăng ký thành công!', 'success');
        setTimeout(() => window.location.href = '../index.html', 1500);
    } finally {
        btn.classList.remove('loading');
    }
}

// ====================
// INITIALIZATION
// ====================
document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('loginForm')?.addEventListener('submit', handleLogin);
    document.getElementById('registerForm')?.addEventListener('submit', handleRegister);
    console.log('VCB Digibank initialized');
});
