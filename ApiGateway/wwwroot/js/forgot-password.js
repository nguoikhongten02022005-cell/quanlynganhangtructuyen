// Forgot Password - 3 Step Flow
const API_URL = '/api';

let currentStep = 1;
let savedTenDangNhap = '';
let savedToken = '';

// Toast notification
function showToast(message, type = 'success') {
    const toast = document.getElementById('toast');
    const toastMessage = document.getElementById('toastMessage');
    const toastIcon = toast.querySelector('.toast-icon i');
    
    toastMessage.textContent = message;
    toast.classList.remove('error', 'warning');
    
    if (type === 'error') {
        toast.classList.add('error');
        toastIcon.className = 'fas fa-times-circle';
    } else if (type === 'warning') {
        toast.classList.add('warning');
        toastIcon.className = 'fas fa-exclamation-circle';
    } else {
        toastIcon.className = 'fas fa-check-circle';
    }
    
    toast.classList.add('show');
    setTimeout(() => toast.classList.remove('show'), 3000);
}

// Toggle password visibility
function togglePassword(inputId, iconElement) {
    const input = document.getElementById(inputId);
    const icon = iconElement.querySelector('i');
    
    if (input.type === 'password') {
        input.type = 'text';
        icon.classList.remove('fa-eye');
        icon.classList.add('fa-eye-slash');
    } else {
        input.type = 'password';
        icon.classList.remove('fa-eye-slash');
        icon.classList.add('fa-eye');
    }
}

// Navigate between steps
function goToStep(step) {
    // Hide all forms
    document.querySelectorAll('.form-step').forEach(form => {
        form.classList.remove('active');
    });
    
    // Update step indicators
    for (let i = 1; i <= 3; i++) {
        const stepIndicator = document.getElementById(`step${i}Indicator`);
        stepIndicator.classList.remove('active', 'completed');
        
        if (i < step) {
            stepIndicator.classList.add('completed');
        } else if (i === step) {
            stepIndicator.classList.add('active');
        }
    }
    
    // Update lines
    document.getElementById('line1').classList.toggle('completed', step > 1);
    document.getElementById('line2').classList.toggle('completed', step > 2);
    
    // Show current step form
    if (step === 1) {
        document.getElementById('step1Form').classList.add('active');
    } else if (step === 2) {
        document.getElementById('step2Form').classList.add('active');
    } else if (step === 3) {
        document.getElementById('step3Form').classList.add('active');
    } else if (step === 4) {
        document.getElementById('successMessage').classList.add('active');
    }
    
    currentStep = step;
}

// Step 1: Submit username to get token
document.getElementById('step1Form').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const tenDangNhap = document.getElementById('tenDangNhap').value.trim();
    
    if (!tenDangNhap) {
        showToast('Vui lòng nhập tên đăng nhập!', 'error');
        return;
    }
    
    const submitBtn = e.target.querySelector('.submit-btn');
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
    
    try {
        const response = await fetch(`${API_URL}/auth/forgot-password`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ tenDangNhap })
        });
        
        const data = await response.json();
        
        if (response.ok && data.token) {
            savedTenDangNhap = tenDangNhap;
            savedToken = data.token; // Token trả về từ API (demo)
            
            // Hiển thị token (demo mode)
            document.getElementById('tokenDisplay').textContent = savedToken;
            
            showToast(data.thongBao || 'Mã xác nhận đã được tạo!', 'success');
            goToStep(2);
        } else {
            showToast(data.thongBao || 'Không tìm thấy tài khoản!', 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Lỗi kết nối server!', 'error');
    } finally {
        submitBtn.disabled = false;
        submitBtn.innerHTML = '<i class="fas fa-arrow-right"></i> Tiếp tục';
    }
});

// Step 2: Verify token (just check if entered token matches)
document.getElementById('step2Form').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const tokenInput = document.getElementById('tokenInput').value.trim();
    
    if (!tokenInput) {
        showToast('Vui lòng nhập mã xác nhận!', 'error');
        return;
    }
    
    // Trong demo: chỉ kiểm tra token đã nhập đúng với token được hiển thị
    if (tokenInput === savedToken) {
        showToast('Mã xác nhận hợp lệ!', 'success');
        goToStep(3);
    } else {
        showToast('Mã xác nhận không đúng!', 'error');
    }
});

// Step 3: Reset password
document.getElementById('step3Form').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const matKhauMoi = document.getElementById('matKhauMoi').value;
    const nhapLaiMatKhauMoi = document.getElementById('nhapLaiMatKhauMoi').value;
    
    // Validate
    if (matKhauMoi.length < 6) {
        showToast('Mật khẩu phải có ít nhất 6 ký tự!', 'error');
        return;
    }
    
    if (matKhauMoi !== nhapLaiMatKhauMoi) {
        showToast('Mật khẩu nhập lại không khớp!', 'error');
        return;
    }
    
    const submitBtn = e.target.querySelector('.submit-btn');
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
    
    try {
        const response = await fetch(`${API_URL}/auth/reset-password`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                tenDangNhap: savedTenDangNhap,
                token: savedToken,
                matKhauMoi: matKhauMoi,
                nhapLaiMatKhauMoi: nhapLaiMatKhauMoi
            })
        });
        
        const data = await response.json();
        
        if (response.ok) {
            showToast(data.thongBao || 'Đặt lại mật khẩu thành công!', 'success');
            goToStep(4); // Show success message
        } else {
            showToast(data.thongBao || 'Đặt lại mật khẩu thất bại!', 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Lỗi kết nối server!', 'error');
    } finally {
        submitBtn.disabled = false;
        submitBtn.innerHTML = '<i class="fas fa-save"></i> Đặt lại mật khẩu';
    }
});
