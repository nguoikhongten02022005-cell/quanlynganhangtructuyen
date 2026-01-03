// Khởi tạo biến toàn cục
let token = localStorage.getItem('token');
let currentUser = JSON.parse(localStorage.getItem('user') || '{}');

// Hàm kiểm tra xác thực
function checkAuth() {
    if (!token) {
        window.location.href = 'index.html';
        return false;
    }
    return true;
}

// Hàm gọi API
async function callApi(endpoint, method = 'GET', body = null) {
    const url = `https://localhost:7079/api${endpoint}`;
    const options = {
        method: method,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        }
    };

    if (body) {
        options.body = JSON.stringify(body);
    }

    try {
        const response = await fetch(url, options);
        const data = await response.json();
        
        if (!response.ok) {
            throw new Error(data.thongBao || 'Lỗi hệ thống');
        }
        
        return data;
    } catch (error) {
        console.error('API Error:', error);
        alert('Lỗi: ' + error.message);
        return null;
    }
}

// Hàm đăng xuất
function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = 'index.html';
}

// Hàm tải thông tin người dùng
function loadUserInfo() {
    if (currentUser.fullName) {
        document.getElementById('fullName').textContent = currentUser.fullName;
    }
}

// Hàm chuyển đổi giữa các section
function switchSection(sectionId) {
    // Ẩn tất cả các section
    document.querySelectorAll('.section').forEach(section => {
        section.classList.remove('active');
    });

    // Xóa active class khỏi tất cả nav links
    document.querySelectorAll('.nav-link').forEach(link => {
        link.classList.remove('active');
    });

    // Hiển thị section được chọn
    document.getElementById(sectionId).classList.add('active');

    // Đánh dấu nav link tương ứng là active
    document.querySelector(`[data-section="${sectionId}"]`).classList.add('active');

    // Tải dữ liệu cho section tương ứng
    switch (sectionId) {
        case 'dashboard':
            loadDashboardData();
            break;
        case 'users':
            loadUsersData();
            break;
        case 'kyc-pending':
            loadKycPendingData();
            break;
    }
}

// Hàm tải dữ liệu dashboard
async function loadDashboardData() {
    try {
        // Lấy tổng số người dùng
        const usersData = await callApi('/admin/users');
        if (usersData) {
            document.getElementById('totalUsers').textContent = usersData.tongSo || 0;
        }

        // Lấy số KYC chờ duyệt
        const kycData = await callApi('/admin/kyc-pending');
        if (kycData) {
            document.getElementById('pendingKyc').textContent = kycData.data?.tongSo || 0;
        }

        // Tính số tài khoản bị khóa
        const lockedAccounts = usersData?.danhSach?.filter(user => user.trangThai === 'LOCKED').length || 0;
        document.getElementById('lockedAccounts').textContent = lockedAccounts;
    } catch (error) {
        console.error('Lỗi khi tải dữ liệu dashboard:', error);
    }
}

// Hàm tải dữ liệu người dùng
async function loadUsersData() {
    try {
        const role = document.getElementById('roleFilter').value;
        const status = document.getElementById('statusFilter').value;
        
        let endpoint = '/admin/users';
        const params = [];
        if (role) params.push(`role=${role}`);
        if (status) params.push(`status=${status}`);
        
        if (params.length > 0) {
            endpoint += '?' + params.join('&');
        }

        const data = await callApi(endpoint);
        if (data && data.danhSach) {
            const tbody = document.getElementById('usersTableBody');
            tbody.innerHTML = '';

            data.danhSach.forEach(user => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${user.maNguoiDung}</td>
                    <td>${user.tenDangNhap}</td>
                    <td>${user.vaiTro}</td>
                    <td>${user.trangThai}</td>
                    <td>${user.hoTen}</td>
                    <td>${user.email || ''}</td>
                    <td>${user.soDienThoai || ''}</td>
                    <td class="action-buttons">
                        <button class="btn-lock" onclick="toggleUserStatus(${user.maNguoiDung}, true)" ${user.trangThai === 'LOCKED' ? 'style="display:none"' : ''}>Khóa</button>
                        <button class="btn-unlock" onclick="toggleUserStatus(${user.maNguoiDung}, false)" ${user.trangThai === 'ACTIVE' ? 'style="display:none"' : ''}>Mở khóa</button>
                    </td>
                `;
                tbody.appendChild(row);
            });
        }
    } catch (error) {
        console.error('Lỗi khi tải dữ liệu người dùng:', error);
    }
}

// Hàm khóa/mở khóa tài khoản
async function toggleUserStatus(userId, lock) {
    try {
        const result = await callApi(`/admin/users/${userId}/lock`, 'PUT', {
            khoa: lock
        });

        if (result) {
            alert(result.thongBao);
            loadUsersData(); // Tải lại danh sách người dùng
        }
    } catch (error) {
        console.error('Lỗi khi thay đổi trạng thái người dùng:', error);
    }
}

// Hàm tải dữ liệu KYC chờ duyệt
async function loadKycPendingData() {
    try {
        const data = await callApi('/admin/kyc-pending');
        if (data && data.data?.danhSach) {
            const tbody = document.getElementById('kycPendingTableBody');
            tbody.innerHTML = '';

            data.data.danhSach.forEach(kyc => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${kyc.maKhachHang}</td>
                    <td>${kyc.maNguoiDung}</td>
                    <td>${kyc.hoTen}</td>
                    <td>${kyc.email}</td>
                    <td>${kyc.soDienThoai}</td>
                    <td>${kyc.soCCCD}</td>
                    <td>${kyc.trangThaiKYC}</td>
                `;
                tbody.appendChild(row);
            });
        }
    } catch (error) {
        console.error('Lỗi khi tải dữ liệu KYC chờ duyệt:', error);
    }
}

// Hàm duyệt KYC
async function approveKyc() {
    try {
        const customerId = document.getElementById('customerId').value;
        const status = document.getElementById('kycStatus').value;
        const reason = document.getElementById('kycReason').value;

        if (!customerId) {
            alert('Vui lòng nhập mã khách hàng');
            return;
        }

        const result = await callApi('/admin/kyc-approve', 'POST', {
            customerId: parseInt(customerId),
            status: status,
            reason: reason || null
        });

        if (result) {
            alert(result.thongBao);
            document.getElementById('customerId').value = '';
            document.getElementById('kycReason').value = '';
            loadKycPendingData(); // Tải lại danh sách KYC chờ duyệt
        }
    } catch (error) {
        console.error('Lỗi khi duyệt KYC:', error);
    }
}

// Hàm khởi tạo
function init() {
    if (!checkAuth()) return;

    loadUserInfo();

    // Gắn sự kiện cho nút đăng xuất
    document.getElementById('logoutBtn').addEventListener('click', logout);

    // Gắn sự kiện cho các liên kết điều hướng
    document.querySelectorAll('.nav-link').forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const sectionId = link.getAttribute('data-section');
            switchSection(sectionId);
        });
    });

    // Gắn sự kiện cho nút lọc người dùng
    document.getElementById('filterUsersBtn').addEventListener('click', loadUsersData);

    // Gắn sự kiện cho nút duyệt KYC
    document.getElementById('approveKycBtn').addEventListener('click', approveKyc);

    // Mặc định hiển thị trang tổng quan
    switchSection('dashboard');
}

// Khởi chạy khi trang tải xong
document.addEventListener('DOMContentLoaded', init);