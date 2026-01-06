// Địa chỉ API
const API_URL = 'https://localhost:5001/api/customer/profile';

// Kiểm tra đăng nhập
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

// Hàm hiển thị thông báo
function hienThiThongBao(noiDung, loai) {
    // Có thể thêm toast notification nếu cần
    console.log(loai + ': ' + noiDung);
}

// Hàm lấy token từ localStorage
function layToken() {
    return localStorage.getItem('token');
}

// Hàm tải thông tin profile
async function taiThongTinProfile() {
    const token = layToken();
    
    if (!token) {
        window.location.href = 'index.html';
        return;
    }
    
    try {
        const response = await fetch(API_URL, {
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + token,
                'Content-Type': 'application/json'
            }
        });
        
        const data = await response.json();
        
        if (response.ok) {
            // Hiển thị thông tin lên giao diện
            document.getElementById('hoTen').textContent = data.HoTen || '---';
            document.getElementById('email').textContent = data.Email || '---';
            document.getElementById('soDienThoai').textContent = data.SoDienThoai || '---';
            document.getElementById('soCCCD').textContent = data.SoCCCD || '---';
            
            const kycElement = document.getElementById('trangThaiKyc');
            const trangThaiKyc = data.TrangThaiKYC || 'NONE';
            
            // Hiển thị trạng thái KYC bằng tiếng Việt
            let trangThaiText = 'Chưa xác minh';
            if (trangThaiKyc === 'PENDING') trangThaiText = 'Chờ duyệt';
            else if (trangThaiKyc === 'APPROVED') trangThaiText = 'Đã xác minh';
            else if (trangThaiKyc === 'REJECTED') trangThaiText = 'Từ chối';
            
            kycElement.textContent = trangThaiText;
            kycElement.className = 'kyc-status ' + trangThaiKyc;
        } else {
            console.error('Lỗi:', data.message || 'Không thể tải thông tin!');
        }
    } catch (error) {
        console.error('Lỗi kết nối đến server!', error);
    }
}

// Hàm quay lại trang trước
function quayLai() {
    window.history.back();
}

// Hàm chuyển đến trang KYC
function chuyenDenKyc() {
    window.location.href = 'customer-kyc.html';
}

// Tự động tải thông tin khi trang load
window.addEventListener('load', function() {
    if (!kiemTraDangNhap()) {
        return;
    }
    
    hienThiTenNguoiDung();
    taiThongTinProfile();
});
