// Địa chỉ API
const API_BASE_URL = 'https://localhost:5001/api';

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
    const thongBao = document.getElementById('thongBao');
    thongBao.textContent = noiDung;
    thongBao.className = 'thong-bao ' + loai;
    thongBao.style.display = 'block';
    
    setTimeout(() => {
        thongBao.style.display = 'none';
    }, 5000);
}

// Xu ly form doi mat khau
document.getElementById('changePasswordForm').addEventListener('submit', function(suKien) {
    suKien.preventDefault();

    // Lay gia tri tu form
    const matKhauCu = document.getElementById('oldPassword').value;
    const matKhauMoi = document.getElementById('newPassword').value;
    const xacNhanMatKhauMoi = document.getElementById('confirmNewPassword').value;

    // Kiem tra xac nhan mat khau
    if (matKhauMoi !== xacNhanMatKhauMoi) {
        hienThiThongBao('Mật khẩu xác nhận không khớp', 'loi');
        return;
    }

    // Kiem tra do dai mat khau moi
    if (matKhauMoi.length < 6) {
        hienThiThongBao('Mật khẩu mới phải có ít nhất 6 ký tự', 'loi');
        return;
    }

    // Hien thi trang thai dang tai
    const nutGuiForm = document.querySelector('#changePasswordForm .btn-primary');
    nutGuiForm.disabled = true;
    nutGuiForm.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';

    // Goi API doi mat khau
    goiAPIDoiMatKhau({
        matKhauCu: matKhauCu,
        matKhauMoi: matKhauMoi
    })
    .finally(() => {
        nutGuiForm.disabled = false;
        nutGuiForm.innerHTML = '<i class="fas fa-key"></i> Đổi mật khẩu';
    });
});

// Ham goi API doi mat khau
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
            hienThiThongBao(ketQua.thongBao || 'Đổi mật khẩu thành công!', 'thanh-cong');

            // Quay lai trang dang nhap sau 2 giay
            setTimeout(() => {
                localStorage.removeItem('token');
                localStorage.removeItem('role');
                localStorage.removeItem('fullName');
                window.location.href = 'index.html';
            }, 2000);
        } else {
            hienThiThongBao(ketQua.thongBao || 'Đổi mật khẩu thất bại', 'loi');
        }
    } catch (loi) {
        console.error('Lỗi đổi mật khẩu:', loi);
        hienThiThongBao('Lỗi kết nối server', 'loi');
    }
}

// Khởi tạo khi trang load
window.addEventListener('load', function() {
    if (!kiemTraDangNhap()) {
        return;
    }
    
    hienThiTenNguoiDung();
});
