// =============================================
// XU LY DOI MAT KHAU
// =============================================

// Xu ly form doi mat khau
document.getElementById('changePasswordForm').addEventListener('submit', function(suKien) {
    suKien.preventDefault();

    // Lay gia tri tu form
    const matKhauCu = document.getElementById('oldPassword').value;
    const matKhauMoi = document.getElementById('newPassword').value;
    const xacNhanMatKhauMoi = document.getElementById('confirmNewPassword').value;

    // Kiem tra xac nhan mat khau
    if (matKhauMoi !== xacNhanMatKhauMoi) {
        showToast('Mat khau xac nhan khong khop', 'error');
        return;
    }

    // Kiem tra do dai mat khau moi
    if (matKhauMoi.length < 6) {
        showToast('Mat khau moi phai co it nhat 6 ky tu', 'error');
        return;
    }

    // Hien thi trang thai dang tai
    const nutGuiForm = document.querySelector('#changePasswordForm .submit-btn');
    nutGuiForm.classList.add('loading');

    // Goi API doi mat khau
    goiAPIDoiMatKhau({
        oldPass: matKhauCu,
        newPass: matKhauMoi
    })
    .finally(() => {
        nutGuiForm.classList.remove('loading');
    });
});

// Ham goi API doi mat khau (su dung ham tu index.js)
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
            showToast(ketQua.thongBao || 'Doi mat khau thanh cong!', 'success');

            // Quay lai trang dang nhap sau 2 giay
            setTimeout(() => {
                window.location.href = 'index.html';
            }, 2000);
        } else {
            showToast(ketQua.thongBao || 'Doi mat khau that bai', 'error');
        }
    } catch (loi) {
        console.error('Loi doi mat khau:', loi);
        showToast('Loi ket noi server', 'error');
    }
}
