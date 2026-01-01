using DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class KhachHangService : IKhachHangService
    {
        private readonly NganHangDAL _db;

        public KhachHangService(NganHangDAL db)
        {
            _db = db;
        }

        public async Task<object> LayThongTinHoSoAsync(int maNguoiDung)
        {
            // Tìm khách hàng theo mã người dùng
            var khachHang = await _db.KhachHang
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaNguoiDung == maNguoiDung);

            if (khachHang == null)
            {
                throw new Exception("Không tìm thấy hồ sơ khách hàng.");
            }

            // Trả về thông tin hồ sơ
            return new
            {
                hoTen = khachHang.HoTen,
                email = khachHang.Email,
                soDienThoai = khachHang.SoDienThoai,
                soCCCD = khachHang.SoCCCD,
                trangThaiKYC = khachHang.TrangThaiKYC
            };
        }
    }
}
