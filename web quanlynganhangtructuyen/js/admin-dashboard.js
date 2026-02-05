const API_URL = 'https://localhost:5001/api';
let token = localStorage.getItem('token');

// Toast notification
function showToast(message, type = 'success') {
    const toast = document.getElementById('toast');
    const toastMessage = document.getElementById('toastMessage');

    toast.className = 'toast';
    if (type === 'error') {
        toast.classList.add('error');
    }

    toastMessage.textContent = message;
    toast.classList.add('show');

    setTimeout(() => {
        toast.classList.remove('show');
    }, 3000);
}

// Check login
if (!token) {
    window.location.href = 'index.html';
}

// Tab switching
document.querySelectorAll('.nav-tab').forEach(tab => {
    tab.addEventListener('click', (e) => {
        e.preventDefault();

        document.querySelectorAll('.nav-tab').forEach(t => t.classList.remove('active'));
        document.querySelectorAll('.section').forEach(s => s.classList.remove('active'));

        tab.classList.add('active');
        const sectionId = tab.getAttribute('data-section');
        document.getElementById(sectionId).classList.add('active');

        if (sectionId === 'dashboard') {
            loadDashboard();
        } else if (sectionId === 'users') {
            loadUsers();
        } else if (sectionId === 'accounts') {
            loadAccounts();
        } else if (sectionId === 'kyc-pending') {
            loadKYCPending();
        }
    });
});

// Logout
document.getElementById('logoutBtn').addEventListener('click', () => {
    localStorage.removeItem('token');
    window.location.href = 'index.html';
});

// Load Dashboard
async function loadDashboard() {
    try {
        const response = await fetch(`${API_URL}/admin/dashboard`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            const data = await response.json();
            const stats = data.data;

            document.getElementById('totalUsers').textContent = stats.tongNguoiDung;
            document.getElementById('totalCustomers').textContent = stats.tongKhachHang;
            document.getElementById('pendingKyc').textContent = stats.soKYCChoDuyet;
            document.getElementById('totalTransactions').textContent = stats.tongGiaoDich;
            document.getElementById('totalAmount').textContent =
                new Intl.NumberFormat('vi-VN').format(stats.tongSoTienGiaoDich) + ' VNĐ';
        } else {
            console.error('Dashboard API error:', response.status);
            showToast('Không thể tải thống kê', 'error');
        }
    } catch (error) {
        console.error('Error loading dashboard:', error);
        showToast('Lỗi kết nối server', 'error');
    }
}

// Load Users
async function loadUsers() {
    const role = document.getElementById('roleFilter').value;
    const status = document.getElementById('statusFilter').value;

    let url = `${API_URL}/admin/users`;
    const params = new URLSearchParams();
    if (role) params.append('role', role);
    if (status) params.append('status', status);
    if (params.toString()) url += '?' + params.toString();

    try {
        const response = await fetch(url, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            const data = await response.json();
            const users = data.danhSach || [];

            const tbody = document.getElementById('usersTableBody');
            tbody.innerHTML = '';

            if (users.length === 0) {
                tbody.innerHTML = '<tr><td colspan="7" style="text-align: center;">Không có người dùng</td></tr>';
                return;
            }

            users.forEach(user => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>${user.maNguoiDung}</td>
                    <td>${user.tenDangNhap}</td>
                    <td>${user.hoTen || 'N/A'}</td>
                    <td>${user.email || 'N/A'}</td>
                    <td><span class="badge ${getBadgeClass(user.vaiTro)}">${user.vaiTro}</span></td>
                    <td><span class="badge ${user.trangThai === 'ACTIVE' ? 'success' : 'danger'}">${user.trangThai}</span></td>
                    <td>
                        <button class="btn ${user.trangThai === 'ACTIVE' ? 'btn-danger' : 'btn-success'}"
                                onclick="lockUser(${user.maNguoiDung}, ${user.trangThai === 'ACTIVE'})">
                            ${user.trangThai === 'ACTIVE' ? 'Khóa' : 'Mở khóa'}
                        </button>
                    </td>
                `;
                tbody.appendChild(tr);
            });
        } else {
            showToast('Không thể tải danh sách người dùng', 'error');
        }
    } catch (error) {
        console.error('Error loading users:', error);
        showToast('Lỗi kết nối server', 'error');
    }
}

function getBadgeClass(role) {
    if (role === 'ADMIN') return 'danger';
    if (role === 'STAFF') return 'warning';
    return 'primary';
}

// Lock/Unlock User
async function lockUser(userId, shouldLock) {
    try {
        const response = await fetch(`${API_URL}/admin/users/${userId}/lock`, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                khoa: shouldLock
            })
        });

        if (response.ok) {
            showToast(shouldLock ? 'Đã khóa người dùng' : 'Đã mở khóa người dùng');
            loadUsers();
        } else {
            showToast('Có lỗi xảy ra', 'error');
        }
    } catch (error) {
        showToast('Có lỗi xảy ra', 'error');
    }
}

// Load KYC Pending
async function loadKYCPending() {
    try {
        const response = await fetch(`${API_URL}/admin/kyc-pending`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            const result = await response.json();
            const kycList = result.data || [];

            const tbody = document.getElementById('kycTableBody');
            tbody.innerHTML = '';

            if (kycList.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" style="text-align: center;">Không có KYC chờ duyệt</td></tr>';
                return;
            }

            kycList.forEach(kyc => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>${kyc.maKhachHang}</td>
                    <td>${kyc.hoTen}</td>
                    <td>${kyc.soCCCD || 'N/A'}</td>
                    <td>${kyc.email || 'N/A'}</td>
                    <td>${kyc.soDienThoai || 'N/A'}</td>
                    <td>
                        <button class="btn btn-success" onclick="approveKYC(${kyc.maKhachHang}, 'APPROVED')">Duyệt</button>
                        <button class="btn btn-danger" onclick="approveKYC(${kyc.maKhachHang}, 'REJECTED')">Từ chối</button>
                    </td>
                `;
                tbody.appendChild(tr);
            });
        } else {
            showToast('Không thể tải danh sách KYC', 'error');
        }
    } catch (error) {
        console.error('Error loading KYC:', error);
        showToast('Có lỗi xảy ra khi tải KYC', 'error');
    }
}

// Approve KYC
async function approveKYC(customerId, status) {
    let reason = null;
    if (status === 'REJECTED') {
        reason = prompt('Nhập lý do từ chối:');
        if (!reason) return;
    }

    try {
        const response = await fetch(`${API_URL}/admin/kyc-approve`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                customerId: customerId,
                status: status,
                reason: reason
            })
        });

        if (response.ok) {
            showToast(status === 'APPROVED' ? 'Đã duyệt KYC' : 'Đã từ chối KYC');
            loadKYCPending();
            loadDashboard();
        } else {
            showToast('Có lỗi xảy ra', 'error');
        }
    } catch (error) {
        showToast('Có lỗi xảy ra', 'error');
    }
}

// Load User Detail
async function loadUserDetail() {
    const userId = document.getElementById('userIdInput').value;
    if (!userId) {
        showToast('Vui lòng nhập mã người dùng', 'error');
        return;
    }

    try {
        const response = await fetch(`${API_URL}/admin/users/${userId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            const result = await response.json();
            const user = result.data;

            const content = document.getElementById('userDetailContent');
            content.innerHTML = `
                <div class="user-detail-card">
                    <div class="user-detail-header">
                        <div class="user-avatar">
                            <i class="fas fa-user"></i>
                        </div>
                        <div class="user-detail-info">
                            <h3>${user.hoTen || user.tenDangNhap}</h3>
                            <div class="user-meta">
                                <span class="badge ${getBadgeClass(user.vaiTro)}">${user.vaiTro}</span>
                                <span class="badge ${user.trangThai === 'ACTIVE' ? 'success' : 'danger'}">${user.trangThai === 'ACTIVE' ? 'Hoạt động' : 'Đã khóa'}</span>
                            </div>
                        </div>
                    </div>

                    <div class="user-detail-body">
                        <div class="detail-item">
                            <label>Mã người dùng</label>
                            <div class="value">${user.maNguoiDung}</div>
                        </div>
                        <div class="detail-item">
                            <label>Tên đăng nhập</label>
                            <div class="value">${user.tenDangNhap}</div>
                        </div>
                        <div class="detail-item">
                            <label>Họ tên</label>
                            <div class="value">${user.hoTen || 'Chưa cập nhật'}</div>
                        </div>
                        <div class="detail-item">
                            <label>Email</label>
                            <div class="value">${user.email || 'Chưa cập nhật'}</div>
                        </div>
                        <div class="detail-item">
                            <label>Vai trò</label>
                            <div class="value">${user.vaiTro}</div>
                        </div>
                        <div class="detail-item">
                            <label>Ngày tạo</label>
                            <div class="value">${new Date(user.ngayTao).toLocaleString('vi-VN')}</div>
                        </div>
                    </div>

                    <div class="user-detail-actions">
                        <button class="btn ${user.trangThai === 'ACTIVE' ? 'btn-danger' : 'btn-success'}"
                                onclick="lockUser(${user.maNguoiDung}, ${user.trangThai === 'ACTIVE'})">
                            <i class="fas fa-${user.trangThai === 'ACTIVE' ? 'lock' : 'lock-open'}"></i>
                            ${user.trangThai === 'ACTIVE' ? 'Khóa tài khoản' : 'Mở khóa tài khoản'}
                        </button>
                    </div>
                </div>
            `;
        } else {
            const content = document.getElementById('userDetailContent');
            content.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-user-slash"></i>
                    <p>Không tìm thấy người dùng với mã ${userId}</p>
                </div>
            `;
            showToast('Không tìm thấy người dùng', 'error');
        }
    } catch (error) {
        showToast('Có lỗi xảy ra', 'error');
    }
}

// Filter users when select changes
document.getElementById('roleFilter').addEventListener('change', loadUsers);
document.getElementById('statusFilter').addEventListener('change', loadUsers);

// Load Accounts
async function loadAccounts() {
    try {
        const response = await fetch(`${API_URL}/admin/accounts`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            const result = await response.json();
            const accounts = result.data || [];

            const tbody = document.getElementById('accountsTableBody');
            tbody.innerHTML = '';

            if (accounts.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" style="text-align: center;">Không có tài khoản</td></tr>';
                return;
            }

            accounts.forEach(acc => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>${acc.maTaiKhoan}</td>
                    <td>${acc.maKhachHang}</td>
                    <td>${acc.soTaiKhoan}</td>
                    <td>${new Intl.NumberFormat('vi-VN').format(acc.soDu)} VNĐ</td>
                    <td><span class="badge ${acc.trangThai === 'ACTIVE' ? 'success' : 'danger'}">${acc.trangThai}</span></td>
                    <td>
                        <button class="btn ${acc.trangThai === 'ACTIVE' ? 'btn-danger' : 'btn-success'}"
                                onclick="lockAccount(${acc.maTaiKhoan}, ${acc.trangThai === 'ACTIVE'})">
                            ${acc.trangThai === 'ACTIVE' ? 'Khóa' : 'Mở khóa'}
                        </button>
                    </td>
                `;
                tbody.appendChild(tr);
            });
        } else {
            showToast('Không thể tải danh sách tài khoản', 'error');
        }
    } catch (error) {
        console.error('Error loading accounts:', error);
        showToast('Lỗi kết nối server', 'error');
    }
}

// Lock/Unlock Account
async function lockAccount(accountId, shouldLock) {
    try {
        const response = await fetch(`${API_URL}/admin/accounts/${accountId}/lock`, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                khoa: shouldLock
            })
        });

        if (response.ok) {
            showToast(shouldLock ? 'Đã khóa tài khoản' : 'Đã mở khóa tài khoản');
            loadAccounts();
        } else {
            showToast('Có lỗi xảy ra', 'error');
        }
    } catch (error) {
        showToast('Có lỗi xảy ra', 'error');
    }
}

// Load initial data
loadDashboard();
