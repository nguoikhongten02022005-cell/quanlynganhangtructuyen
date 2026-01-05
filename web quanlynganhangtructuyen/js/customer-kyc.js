// Địa chỉ API
const API_URL = 'http://localhost:5000/api/customer/kyc';

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
            window.location.href = 'index.html';
        }, 2000);
        return;
    }
    
    // Lấy dữ liệu từ form
    const soCMND = document.getElementById('soCMND').value.trim();
    const anhCMNDTruoc = document.getElementById('anhCMNDTruoc').value.trim();
    const anhCMNDSau = document.getElementById('anhCMNDSau').value.trim();
    
    // Validate
    if (!kiemTraSoCMND(soCMND)) {
        hienThiThongBao('Số CMND/CCCD phải có đúng 12 chữ số!', 'loi');
        return;
    }
    
    if (!anhCMNDTruoc || !anhCMNDSau) {
        hienThiThongBao('Vui lòng nhập tên file ảnh CMND/CCCD!', 'loi');
        return;
    }
    
    // Tạo object dữ liệu gửi đi
    const duLieuKyc = {
        soCMND: soCMND,
        anhCMNDTruoc: anhCMNDTruoc,
        anhCMNDSau: anhCMNDSau
    };
    
    // Vô hiệu hóa nút submit
    const btnSubmit = event.target.querySelector('button[type="submit"]');
    btnSubmit.disabled = true;
    btnSubmit.textContent = 'Đang gửi...';
    
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
            hienThiThongBao(data.message || 'Gửi KYC thất bại!', 'loi');
            btnSubmit.disabled = false;
            btnSubmit.textContent = 'Gửi xác minh';
        }
    } catch (error) {
        hienThiThongBao('Lỗi kết nối đến server!', 'loi');
        console.error('Lỗi:', error);
        btnSubmit.disabled = false;
        btnSubmit.textContent = 'Gửi xác minh';
    }
}

// Hàm quay lại trang trước
function quayLai() {
    window.history.back();
}

// Gắn sự kiện submit cho form
document.getElementById('formKyc').addEventListener('submit', xuLyGuiKyc);
