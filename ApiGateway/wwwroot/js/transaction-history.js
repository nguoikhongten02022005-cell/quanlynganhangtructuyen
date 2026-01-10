// Địa chỉ API
const API_URL = '/api';

// Biến phân trang
let currentPage = 1;
let totalPages = 1;
const pageSize = 10;
let myAccountNumber = '';

// Kiểm tra đăng nhập
function kiemTraDangNhap() {
    const token = localStorage.getItem('token');
    const role = localStorage.getItem('role');
    
    if (!token) {
        // Chưa đăng nhập -> chuyển về trang login
        window.location.href = '../index.html';
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
        window.location.href = '../index.html';
    }
}

// Hiển thị thông báo
function hienThiThongBao(message, isSuccess = true) {
    const thongBao = document.getElementById('thongBao');
    thongBao.className = isSuccess ? 'thong-bao thong-bao-success' : 'thong-bao thong-bao-error';
    thongBao.innerHTML = `<i class="fas fa-${isSuccess ? 'check-circle' : 'exclamation-circle'}"></i> ${message}`;
    thongBao.style.display = 'block';
    
    setTimeout(() => {
        thongBao.style.display = 'none';
    }, 5000);
}

// Lấy số tài khoản của mình
async function layThongTinTaiKhoan() {
    const token = localStorage.getItem('token');
    
    try {
        const response = await fetch(`${API_URL}/account/my-account`, {
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + token,
                'Content-Type': 'application/json'
            }
        });
        
        if (response.ok) {
            const data = await response.json();
            myAccountNumber = data.soTaiKhoan;
        }
    } catch (error) {
        console.error('Error:', error);
    }
}

// Tải lịch sử giao dịch
async function taiLichSuGiaoDich(page = 1) {
    document.getElementById('loadingMessage').style.display = 'block';
    document.getElementById('transactionContainer').style.display = 'none';
    document.getElementById('emptyMessage').style.display = 'none';
    
    const token = localStorage.getItem('token');
    
    try {
        const response = await fetch(`${API_URL}/transaction/history?pageNumber=${page}&pageSize=${pageSize}`, {
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + token,
                'Content-Type': 'application/json'
            }
        });
        
        const data = await response.json();
        
        document.getElementById('loadingMessage').style.display = 'none';
        
        if (!response.ok) {
            hienThiThongBao(data.message || 'Không thể tải lịch sử giao dịch!', false);
            document.getElementById('emptyMessage').style.display = 'block';
            return;
        }
        
        const transactions = data.data.items;
        currentPage = data.data.pageNumber;
        totalPages = data.data.totalPages;
        
        if (!transactions || transactions.length === 0) {
            document.getElementById('emptyMessage').style.display = 'block';
            return;
        }
        
        hienThiDanhSachGiaoDich(transactions);
        capNhatPagination();
        
    } catch (error) {
        console.error('Error:', error);
        document.getElementById('loadingMessage').style.display = 'none';
        document.getElementById('emptyMessage').style.display = 'block';
        hienThiThongBao('Có lỗi xảy ra khi tải dữ liệu!', false);
    }
}

// Hiển thị danh sách giao dịch
function hienThiDanhSachGiaoDich(transactions) {
    const tbody = document.getElementById('transactionTableBody');
    tbody.innerHTML = '';
    
    transactions.forEach(gd => {
        const isNhan = gd.soTaiKhoanNhan === myAccountNumber;
        const loaiGD = isNhan ? 'nhan' : 'gui';
        const nguoiLienQuan = isNhan ? gd.soTaiKhoanGui : gd.soTaiKhoanNhan;
        const tienHieu = isNhan ? '+' : '-';
        
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${formatDateTime(gd.ngayGiaoDich)}</td>
            <td><span class="loai-gd ${loaiGD}">${isNhan ? 'Nhận tiền' : 'Chuyển tiền'}</span></td>
            <td>${nguoiLienQuan}</td>
            <td><span class="so-tien ${loaiGD}">${tienHieu}${formatCurrency(gd.soTien)} VNĐ</span></td>
            <td>${gd.noiDung || 'Không có nội dung'}</td>
        `;
        tbody.appendChild(row);
    });
    
    document.getElementById('transactionContainer').style.display = 'block';
}

// Cập nhật phân trang
function capNhatPagination() {
    document.getElementById('pageInfo').textContent = `Trang ${currentPage} / ${totalPages}`;
    document.getElementById('prevBtn').disabled = currentPage <= 1;
    document.getElementById('nextBtn').disabled = currentPage >= totalPages;
}

// Chuyển trang
function chuyenTrang(direction) {
    const newPage = currentPage + direction;
    if (newPage >= 1 && newPage <= totalPages) {
        taiLichSuGiaoDich(newPage);
    }
}

// Format tiền tệ
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN').format(amount);
}

// Format ngày giờ
function formatDateTime(dateString) {
    const date = new Date(dateString);
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    
    return `${day}/${month}/${year} ${hours}:${minutes}`;
}

// Khởi tạo khi trang được tải
document.addEventListener('DOMContentLoaded', async function() {
    if (kiemTraDangNhap()) {
        hienThiTenNguoiDung();
        await layThongTinTaiKhoan();
        taiLichSuGiaoDich(1);
    }
});
