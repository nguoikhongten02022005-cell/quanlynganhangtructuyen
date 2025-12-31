using DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class TaiKhoanService : ITaiKhoanService
    {
        private readonly NganHangDAL _db;

        public TaiKhoanService(NganHangDAL db)
        {
            _db = db;
        }

        public async Task<object> LayThongTinTaiKhoanAsync(int maNguoiDung)
        {
            // 1. Tìm khách hàng tương ứng với mã người dùng
            var khachHang = await _db.KhachHang.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaNguoiDung == maNguoiDung);

            if (khachHang == null)
            {
                throw new Exception("Không tìm thấy thông tin khách hàng.");
            }

            // 2. Kiểm tra xem khách hàng đã được duyệt KYC chưa (ACTIVE)
            if (khachHang.TrangThaiKYC != "ACTIVE")
            {
                throw new Exception("Tài khoản chưa được duyệt KYC (Xác minh danh tính), không thể xem số dư.");
            }

            // 3. Tìm tài khoản ngân hàng của khách
            var taiKhoan = await _db.TaiKhoan.AsNoTracking()
                .FirstOrDefaultAsync(t => t.MaKhachHang == khachHang.MaKhachHang);

            if (taiKhoan == null)
            {
                throw new Exception("Chưa có tài khoản ngân hàng.");
            }

            // 4. Trả về thông tin
            return new
            {
                soTaiKhoan = taiKhoan.SoTaiKhoan,
                soDu = taiKhoan.SoDu
            };
        }
    }
}
