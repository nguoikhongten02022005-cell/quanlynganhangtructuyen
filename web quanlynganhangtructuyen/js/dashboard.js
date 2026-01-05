// Địa chỉ API
const API_URL = 'https://localhost:7079/api';

// Kiểm tra đăng nhập
function kiemTraDangNhap() {
    const token = localStorage.getItem('token');
    const role = localStorage.getItem('role');
    
    if (!token) {
        // Chưa đăng nhập -> chuyển về trang login
        window.location.href = 'index.html';
        return false;
    }
    
    if (role !== 'CUSTOMER') {
        // Không phải customer -> chuyển về trang admin
        window.location.href = 'admin-dashboard.html';
        return false;
    }
    
    return true;
}

// Hiển thị tên người dùng
function hienThiTenNguoiDung() {
    const hoTen = localStorage.getItem('fullName');
    if (hoTen) {
        document.getElementById('userName').textContent = hoTen;
    }
}

// Đăng xuất
function dangXuat() {
    if (confirm('Bạn có chắc muốn đăng xuất?')) {
        localStorage.removeItem('token');
        localStorage.removeItem('role');
        localStorage.removeItem('fullName');
        window.location.href = 'index.html';
    }
}

// Tải thông tin tài khoản
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
            // Hiển thị thông tin tài khoản
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

// Tải thông tin profile
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
            // Hiển thị thông tin profile
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

// Khởi tạo khi trang load
window.addEventListener('load', function() {
    // Kiểm tra đăng nhập
    if (!kiemTraDangNhap()) {
        return;
    }
    
    // Hiển thị tên người dùng
    hienThiTenNguoiDung();
    
    // Tải thông tin tài khoản và profile
    taiThongTinTaiKhoan();
    taiThongTinProfile();
});
