// Địa chỉ API
const API_URL = '/api/customer/kyc';

// Kiểm tra đăng nhập
function kiemTraDangNhap() {
    const token = localStorage.getItem('token');
    const role = localStorage.getItem('role');
    
    if (!token) {
        window.location.href = '../index.html';
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
        window.location.href = '../index.html';
    }
}

// Hàm hiển thị thông báo
function hienThiThongBao(noiDung, loai) {
    const thongBao = document.getElementById('thongBao');
    thongBao.textContent = noiDung;
    thongBao.className = 'thong-bao ' + loai;
    thongBao.style.display = 'block';
    
    setTimeout(() => {
        thongBao.style.display = 'none';
    }, 5000);
}

// Hàm lấy token từ localStorage
function layToken() {
    return localStorage.getItem('token');
}

// Hàm validate số CMND/CCCD
function kiemTraSoCMND(soCMND) {
    // Phải là 12 chữ số
    const regex = /^\d{12}$/;
    return regex.test(soCMND);
}

// Hàm xử lý submit form
async function xuLyGuiKyc(event) {
    event.preventDefault();
    
    const token = layToken();
    
    if (!token) {
        hienThiThongBao('Vui lòng đăng nhập trước!', 'loi');
        setTimeout(() => {
            window.location.href = '../index.html';
        }, 2000);
        return;
    }
    
    // Lấy dữ liệu từ form
    const soCMND = document.getElementById('soCMND').value.trim();
    
    // Validate
    if (!kiemTraSoCMND(soCMND)) {
        hienThiThongBao('Số CMND/CCCD phải có đúng 12 chữ số!', 'loi');
        return;
    }
    
    // Tạo object dữ liệu gửi đi (chỉ có số CCCD)
    const duLieuKyc = {
        soCCCD: soCMND
    };
    
    // Vô hiệu hóa nút submit
    const btnSubmit = event.target.querySelector('button[type="submit"]');
    btnSubmit.disabled = true;
    btnSubmit.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang gửi...';
    
    try {
        const response = await fetch(API_URL, {
            method: 'POST',
            headers: {
                'Authorization': 'Bearer ' + token,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(duLieuKyc)
        });
        
        const data = await response.json();
        
        if (response.ok) {
            hienThiThongBao('Gửi xác minh KYC thành công! Vui lòng chờ admin duyệt.', 'thanh-cong');
            // Reset form
            document.getElementById('formKyc').reset();
            
            // Chuyển về trang profile sau 2 giây
            setTimeout(() => {
                window.location.href = 'customer-profile.html';
            }, 2000);
        } else {
            hienThiThongBao(data.thongBao || 'Gửi KYC thất bại!', 'loi');
            btnSubmit.disabled = false;
            btnSubmit.innerHTML = '<i class="fas fa-paper-plane"></i> Gửi xác minh';
        }
    } catch (error) {
        hienThiThongBao('Lỗi kết nối đến server!', 'loi');
        console.error('Lỗi:', error);
        btnSubmit.disabled = false;
        btnSubmit.innerHTML = '<i class="fas fa-paper-plane"></i> Gửi xác minh';
    }
}

// Hàm quay lại trang trước
function quayLai() {
    window.history.back();
}

// Khởi tạo khi trang load
window.addEventListener('load', function() {
    if (!kiemTraDangNhap()) {
        return;
    }
    
    hienThiTenNguoiDung();
});

// Gắn sự kiện submit cho form
document.getElementById('formKyc').addEventListener('submit', xuLyGuiKyc);
