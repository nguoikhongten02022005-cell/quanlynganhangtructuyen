// API URL
const API_URL = '/api';

// Transaction data
let nguoiNhanData = null;
let giaoDichId = null;

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

// Show notification
function hienThiThongBao(message, isSuccess = true) {
    const thongBao = document.getElementById('thongBao');
    thongBao.className = isSuccess ? 'thong-bao thong-bao-success' : 'thong-bao thong-bao-error';
    thongBao.innerHTML = `<i class="fas fa-${isSuccess ? 'check-circle' : 'exclamation-circle'}"></i> ${message}`;
    thongBao.style.display = 'block';

    setTimeout(() => {
        thongBao.style.display = 'none';
    }, 5000);
}

// Step 1: Search recipient
async function timKiemNguoiNhan() {
    const keyword = document.getElementById('keyword').value.trim();

    if (!keyword) {
        hienThiThongBao('Vui lòng nhập số tài khoản hoặc tên người nhận!', false);
        return;
    }

    const token = localStorage.getItem('token');

    try {
        const response = await fetch(`${API_URL}/transaction/lookup?keyword=${encodeURIComponent(keyword)}`, {
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + token,
                'Content-Type': 'application/json'
            }
        });

        const data = await response.json();

        if (!response.ok) {
            hienThiThongBao(data.message || 'Không tìm thấy người nhận!', false);
            document.getElementById('ketQuaTimKiem').style.display = 'none';
            return;
        }

        nguoiNhanData = data.data;
        document.getElementById('soTaiKhoanNhan').textContent = nguoiNhanData.soTaiKhoan;
        document.getElementById('hoTenNhan').textContent = nguoiNhanData.tenKhachHang;
        document.getElementById('trangThaiNhan').textContent = nguoiNhanData.trangThai;
        document.getElementById('ketQuaTimKiem').style.display = 'block';

        hienThiThongBao('Tìm thấy người nhận!', true);
    } catch (error) {
        console.error('Error:', error);
        hienThiThongBao('Có lỗi xảy ra khi tìm kiếm!', false);
    }
}

// Go to step 2
function chuyenSangBuoc2() {
    if (!nguoiNhanData) {
        hienThiThongBao('Vui lòng tìm kiếm người nhận trước!', false);
        return;
    }

    document.getElementById('step1').style.display = 'none';
    document.getElementById('step2').style.display = 'block';
    document.getElementById('nguoiNhanInfo').textContent = `${nguoiNhanData.tenKhachHang} - ${nguoiNhanData.soTaiKhoan}`;
}

// Back to step 1
function quayLaiBuoc1() {
    document.getElementById('step2').style.display = 'none';
    document.getElementById('step1').style.display = 'block';
    document.getElementById('soTien').value = '';
    document.getElementById('noiDung').value = '';
}

// Step 2: Confirm transfer (create transaction and send OTP)
async function xacNhanChuyenTien() {
    const soTien = parseFloat(document.getElementById('soTien').value);
    const noiDung = document.getElementById('noiDung').value.trim();

    if (!soTien || soTien < 1000) {
        hienThiThongBao('Số tiền phải ít nhất 1,000 VNĐ!', false);
        return;
    }

    if (!noiDung) {
        hienThiThongBao('Vui lòng nhập nội dung chuyển tiền!', false);
        return;
    }

    const token = localStorage.getItem('token');

    try {
        const response = await fetch(`${API_URL}/transaction/verify`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                soTaiKhoanNhan: nguoiNhanData.soTaiKhoan,
                soTien: soTien,
                noiDung: noiDung
            })
        });

        const data = await response.json();

        if (!response.ok) {
            hienThiThongBao(data.message || 'Không thể tạo giao dịch!', false);
            return;
        }

        giaoDichId = data.data.giaoDichId;

        document.getElementById('step2').style.display = 'none';
        document.getElementById('step3').style.display = 'block';
        document.getElementById('nguoiNhanInfoOTP').textContent = `${nguoiNhanData.tenKhachHang} - ${nguoiNhanData.soTaiKhoan}`;
        document.getElementById('soTienInfoOTP').textContent = soTien.toLocaleString('vi-VN') + ' VNĐ';
        document.getElementById('noiDungInfoOTP').textContent = noiDung;

        hienThiThongBao('Mã OTP đã được gửi đến email của bạn!', true);
    } catch (error) {
        console.error('Error:', error);
        hienThiThongBao('Có lỗi xảy ra khi tạo giao dịch!', false);
    }
}

// Cancel transaction
function huyGiaoDich() {
    if (confirm('Bạn có chắc muốn hủy giao dịch này?')) {
        giaoDichId = null;
        nguoiNhanData = null;
        document.getElementById('step3').style.display = 'none';
        document.getElementById('step1').style.display = 'block';
        document.getElementById('keyword').value = '';
        document.getElementById('ketQuaTimKiem').style.display = 'none';
        document.getElementById('soTien').value = '';
        document.getElementById('noiDung').value = '';
        document.getElementById('maOTP').value = '';
    }
}

// Step 3: Confirm OTP and complete transfer
async function xacNhanOTP() {
    const maOTP = document.getElementById('maOTP').value.trim();

    if (!maOTP || maOTP.length !== 6) {
        hienThiThongBao('Mã OTP phải có 6 chữ số!', false);
        return;
    }

    if (!giaoDichId) {
        hienThiThongBao('Không tìm thấy thông tin giao dịch!', false);
        return;
    }

    const token = localStorage.getItem('token');

    try {
        const response = await fetch(`${API_URL}/transaction/confirm`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                giaoDichId: giaoDichId,
                maOTP: maOTP
            })
        });

        const data = await response.json();

        if (!response.ok) {
            hienThiThongBao(data.message || 'Mã OTP không đúng hoặc đã hết hạn!', false);
            return;
        }

        hienThiThongBao('Chuyển tiền thành công!', true);

        setTimeout(() => {
            window.location.href = 'dashboard.html';
        }, 2000);

    } catch (error) {
        console.error('Error:', error);
        hienThiThongBao('Có lỗi xảy ra khi xác nhận OTP!', false);
    }
}

// Initialize when page loads
document.addEventListener('DOMContentLoaded', function() {
    if (kiemTraDangNhap()) {
        hienThiTenNguoiDung();
    }
});
