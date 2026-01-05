// Địa chỉ API
const API_URL = 'http://localhost:5000/api/customer/profile';

// Hàm hiển thị thông báo
function hienThiThongBao(noiDung, loai) {
    const thongBao = document.getElementById('thongBao');
    thongBao.textContent = noiDung;
    thongBao.className = 'thong-bao ' + loai;
}

// Hàm lấy token từ localStorage
function layToken() {
    return localStorage.getItem('token');
}

// Hàm tải thông tin profile
async function taiThongTinProfile() {
    const token = layToken();
    
    if (!token) {
        hienThiThongBao('Vui lòng đăng nhập trước!', 'loi');
        setTimeout(() => {
            window.location.href = 'index.html';
        }, 2000);
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
            document.getElementById('hoTen').textContent = data.hoTen || '---';
            document.getElementById('email').textContent = data.email || '---';
            document.getElementById('soDienThoai').textContent = data.soDienThoai || '---';
            document.getElementById('diaChi').textContent = data.diaChi || '---';
            
            const kycElement = document.getElementById('trangThaiKyc');
            kycElement.textContent = data.trangThaiKyc || 'Chưa xác minh';
            kycElement.className = 'kyc-status ' + (data.trangThaiKyc || '');
        } else {
            hienThiThongBao(data.message || 'Không thể tải thông tin!', 'loi');
        }
    } catch (error) {
        hienThiThongBao('Lỗi kết nối đến server!', 'loi');
        console.error('Lỗi:', error);
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
window.addEventListener('load', taiThongTinProfile);
