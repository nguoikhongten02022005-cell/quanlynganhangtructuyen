// =============================================
// CAU HINH API
// =============================================
const API_BASE_URL = 'https://localhost:7079/api';

// =============================================
// THONG BAO TOAST
// =============================================
function showToast(noiDung, loai = 'success') {
    const toast = document.getElementById('toast');
    const noiDungToast = document.getElementById('toastMessage');
    const iconToast = toast.querySelector('.toast-icon i');

    // Xoa cac class cu
    toast.classList.remove('show', 'error', 'warning');

    // Thiet lap loai thong bao
    if (loai === 'error') {
        toast.classList.add('error');
        iconToast.className = 'fas fa-times-circle';
    } else if (loai === 'warning') {
        toast.classList.add('warning');
        iconToast.className = 'fas fa-exclamation-triangle';
    } else {
        iconToast.className = 'fas fa-check-circle';
    }

    // Hien thi noi dung
    noiDungToast.textContent = noiDung;
    toast.classList.add('show');

    // Tu dong an sau 4 giay
    setTimeout(() => {
        toast.classList.remove('show');
    }, 4000);
}

// Alias cho ham cu (de tuong thich nguoc)
function hienThiThongBao(noiDung, loai) {
    showToast(noiDung, loai);
}

// =============================================
// AN/HIEN MAT KHAU
// =============================================
function togglePassword(idInput, phanTuIcon) {
    const inputMatKhau = document.getElementById(idInput);
    const icon = phanTuIcon.querySelector('i');

    if (inputMatKhau.type === 'password') {
        inputMatKhau.type = 'text';
        icon.classList.remove('fa-eye');
        icon.classList.add('fa-eye-slash');
    } else {
        inputMatKhau.type = 'password';
        icon.classList.remove('fa-eye-slash');
        icon.classList.add('fa-eye');
    }
}

// Alias cho ham cu
function anHienMatKhau(idInput, phanTuIcon) {
    togglePassword(idInput, phanTuIcon);
}

// =============================================
// HAM KIEM TRA DU LIEU
// =============================================
function kiemTraEmail(email) {
    const regexEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regexEmail.test(email);
}

function kiemTraSoDienThoai(soDienThoai) {
    const regexSDT = /^(0|\+84)[0-9]{9,10}$/;
    return regexSDT.test(soDienThoai.replace(/\s/g, ''));
}

// Alias cho ham cu (de tuong thich nguoc)
function isValidEmail(email) {
    return kiemTraEmail(email);
}

function isValidPhone(phone) {
    return kiemTraSoDienThoai(phone);
}

// =============================================
// XU LY FORM DANG NHAP
// =============================================
function xuLyDangNhap(suKien) {
    suKien.preventDefault();

    const tenDangNhap = document.getElementById('loginUsername').value.trim();
    const matKhau = document.getElementById('loginPassword').value;

    // Kiem tra du lieu
    if (!tenDangNhap) {
        showToast('Vui long nhap ten dang nhap', 'error');
        document.getElementById('loginUsername').focus();
        return;
    }

    if (!matKhau) {
        showToast('Vui long nhap mat khau', 'error');
        document.getElementById('loginPassword').focus();
        return;
    }

    // Hien thi trang thai dang tai
    const nutGuiForm = document.querySelector('#loginForm .submit-btn');
    nutGuiForm.classList.add('loading');

    const duLieuDangNhap = {
        tenDangNhap: tenDangNhap,
        matKhau: matKhau
    };

    // Goi API dang nhap
    goiAPIDangNhap(duLieuDangNhap)
        .finally(() => {
            nutGuiForm.classList.remove('loading');
        });
}

// Alias cho ham cu
function handleLogin(e) {
    xuLyDangNhap(e);
}

// =============================================
// XU LY FORM DANG KY
// =============================================
function xuLyDangKy(suKien) {
    suKien.preventDefault();

    const tenDangNhap = document.getElementById('regUsername').value.trim();
    const matKhau = document.getElementById('regPassword').value;
    const xacNhanMatKhau = document.getElementById('regConfirmPassword').value;
    const hoTen = document.getElementById('regFullName').value.trim();
    const email = document.getElementById('regEmail').value.trim();
    const soDienThoai = document.getElementById('regPhone').value.trim();

    // Kiem tra du lieu
    if (!tenDangNhap || tenDangNhap.length < 4) {
        showToast('Ten dang nhap phai co it nhat 4 ky tu', 'error');
        document.getElementById('regUsername').focus();
        return;
    }

    if (!matKhau || matKhau.length < 6) {
        showToast('Mat khau phai co it nhat 6 ky tu', 'error');
        document.getElementById('regPassword').focus();
        return;
    }

    if (matKhau !== xacNhanMatKhau) {
        showToast('Mat khau xac nhan khong khop', 'error');
        document.getElementById('regConfirmPassword').focus();
        return;
    }

    if (!hoTen) {
        showToast('Vui long nhap ho va ten', 'error');
        document.getElementById('regFullName').focus();
        return;
    }

    if (email && !kiemTraEmail(email)) {
        showToast('Email khong hop le', 'error');
        document.getElementById('regEmail').focus();
        return;
    }

    if (soDienThoai && !kiemTraSoDienThoai(soDienThoai)) {
        showToast('So dien thoai khong hop le', 'error');
        document.getElementById('regPhone').focus();
        return;
    }

    // Hien thi trang thai dang tai
    const nutGuiForm = document.querySelector('#registerForm .submit-btn');
    nutGuiForm.classList.add('loading');

    const duLieuDangKy = {
        tenDangNhap: tenDangNhap,
        matKhau: matKhau,
        hoTen: hoTen,
        email: email,
        soDienThoai: soDienThoai
    };

    // Goi API dang ky
    goiAPIDangKy(duLieuDangKy)
        .finally(() => {
            nutGuiForm.classList.remove('loading');
        });
}

// Alias cho ham cu
function handleRegister(e) {
    xuLyDangKy(e);
}

// =============================================
// CAC HAM GOI API
// =============================================

// API Doi mat khau
async function goiAPIDoiMatKhau(duLieu) {
    try {
        const phanHoi = await fetch(`${API_BASE_URL}/auth/change-password`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            },
            body: JSON.stringify(duLieu)
        });

        const ketQua = await phanHoi.json();

        if (phanHoi.ok) {
            showToast(ketQua.thongBao || 'Doi mat khau thanh cong!', 'success');

            // Quay lai trang dang nhap sau 2 giay
            setTimeout(() => {
                window.location.href = 'index.html';
            }, 2000);
        } else {
            showToast(ketQua.thongBao || 'Doi mat khau that bai', 'error');
        }
    } catch (loi) {
        console.error('Loi doi mat khau:', loi);
        showToast('Loi ket noi server', 'error');
    }
}

// Alias cho ham cu
async function callChangePasswordAPI(data) {
    return goiAPIDoiMatKhau(data);
}

// API Dang nhap
async function goiAPIDangNhap(duLieu) {
    try {
        const phanHoi = await fetch(`${API_BASE_URL}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(duLieu)
        });

        const ketQua = await phanHoi.json();

        if (phanHoi.ok) {
            // Luu thong tin vao localStorage
            localStorage.setItem('token', ketQua.token);
            localStorage.setItem('role', ketQua.role);
            localStorage.setItem('fullName', ketQua.fullName);

            showToast('Dang nhap thanh cong!', 'success');

            // Chuyen huong theo vai tro
            // ADMIN va STAFF dung chung 1 trang quan ly
            setTimeout(() => {
                if (ketQua.role === 'ADMIN' || ketQua.role === 'STAFF') {
                    window.location.href = 'admin-dashboard.html';
                } else {
                    window.location.href = 'dashboard.html';
                }
            }, 1000);
        } else {
            showToast(ketQua.thongBao || 'Dang nhap that bai', 'error');
        }
    } catch (loi) {
        console.error('Loi dang nhap:', loi);
        showToast('Loi ket noi server', 'error');
    }
}

// Alias cho ham cu
async function callLoginAPI(data) {
    return goiAPIDangNhap(data);
}

// API Dang ky
async function goiAPIDangKy(duLieu) {
    try {
        const phanHoi = await fetch(`${API_BASE_URL}/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(duLieu)
        });

        const ketQua = await phanHoi.json();

        if (phanHoi.ok) {
            showToast(ketQua.thongBao || 'Dang ky thanh cong!', 'success');
            setTimeout(() => {
                window.location.href = 'index.html';
            }, 1500);
        } else {
            showToast(ketQua.thongBao || 'Dang ky that bai', 'error');
        }
    } catch (loi) {
        console.error('Loi dang ky:', loi);
        showToast('Loi ket noi server', 'error');
    }
}

// Alias cho ham cu
async function callRegisterAPI(data) {
    return goiAPIDangKy(data);
}

// =============================================
// KHOI TAO KHI TAI TRANG
// =============================================
document.addEventListener('DOMContentLoaded', function() {
    // Form dang nhap
    const formDangNhap = document.getElementById('loginForm');
    if (formDangNhap) {
        formDangNhap.addEventListener('submit', xuLyDangNhap);
    }

    // Form dang ky
    const formDangKy = document.getElementById('registerForm');
    if (formDangKy) {
        formDangKy.addEventListener('submit', xuLyDangKy);
    }

    console.log('VCB Digibank da khoi tao!');
});
