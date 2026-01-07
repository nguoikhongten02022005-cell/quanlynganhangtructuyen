// API URL
const API_URL = '/api';

// Check login
function kiemTraDangNhap() {
    const token = localStorage.getItem('token');
    const role = localStorage.getItem('role');

    if (!token) {
        window.location.href = 'index.html';
        return false;
    }

    if (role !== 'CUSTOMER') {
        window.location.href = 'admin-dashboard.html';
        return false;
    }

    return true;
}

// Display user name
function hienThiTenNguoiDung() {
    const hoTen = localStorage.getItem('fullName');
    if (hoTen) {
        document.getElementById('userName').textContent = hoTen;
    }
}

// Logout
function dangXuat() {
    if (confirm('Bạn có chắc muốn đăng xuất?')) {
        localStorage.removeItem('token');
        localStorage.removeItem('role');
        localStorage.removeItem('fullName');
        window.location.href = 'index.html';
    }
}

// Load account info
async function taiThongTinTaiKhoan() {
    const token = localStorage.getItem('token');

    try {
        const response = await fetch(`${API_URL}/account/my-account`, {
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + token,
                'Content-Type': 'application/json'
            }
        });

        const data = await response.json();

        if (response.ok) {
            document.getElementById('soTaiKhoan').textContent = data.soTaiKhoan || '---';
            document.getElementById('soDu').textContent = (data.soDu || 0).toLocaleString('vi-VN') + ' VNĐ';
            document.getElementById('trangThai').textContent = data.trangThai || '---';
        } else {
            console.error('Không thể tải thông tin tài khoản:', data);
        }
    } catch (error) {
        console.error('Lỗi kết nối:', error);
    }
}

// Load profile info
async function taiThongTinProfile() {
    const token = localStorage.getItem('token');

    try {
        const response = await fetch(`${API_URL}/customer/profile`, {
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + token,
                'Content-Type': 'application/json'
            }
        });

        const data = await response.json();

        if (response.ok) {
            document.getElementById('hoTen').textContent = data.hoTen || '---';
            document.getElementById('email').textContent = data.email || '---';
            document.getElementById('soDienThoai').textContent = data.soDienThoai || '---';
            document.getElementById('trangThaiKyc').textContent = data.trangThaiKyc || 'Chưa xác minh';
        } else {
            console.error('Không thể tải thông tin profile:', data);
        }
    } catch (error) {
        console.error('Lỗi kết nối:', error);
    }
}

// Initialize when page loads
window.addEventListener('load', function() {
    if (!kiemTraDangNhap()) {
        return;
    }

    hienThiTenNguoiDung();
    taiThongTinTaiKhoan();
    taiThongTinProfile();
});
