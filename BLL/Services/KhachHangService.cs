using DAL;
using Microsoft.EntityFrameworkCore;
using Model.Requests;
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
            var khachHang = await _db.KhachHang
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaNguoiDung == maNguoiDung);

            if (khachHang == null)
            {
                throw new Exception("Không tìm thấy hồ sơ khách hàng.");
            }

            return new
            {
                hoTen = khachHang.HoTen,
                email = khachHang.Email,
                soDienThoai = khachHang.SoDienThoai,
                soCCCD = khachHang.SoCCCD,
                trangThaiKYC = khachHang.TrangThaiKYC
            };
        }
        public async Task<object> GuiYeuCauKycAsync(int maNguoiDung, KycRequest request)
        {
            // Kiểm tra xem khách hàng có tồn tại không
            var khachHang = await _db.KhachHang
                .FirstOrDefaultAsync(x => x.MaNguoiDung == maNguoiDung);

            if (khachHang == null)
            {
                throw new Exception("Không tìm thấy hồ sơ khách hàng.");
            }

            // Kiểm tra số CCCD có hợp lệ không (12 hoặc 13 số)
            if (string.IsNullOrWhiteSpace(request.SoCCCD) ||
                (request.SoCCCD.Length != 12 && request.SoCCCD.Length != 13))
            {
                throw new Exception("Số CCCD phải có 12 hoặc 13 chữ số.");
            }

            // Kiểm tra xem số CCCD đã được sử dụng chưa
            var daTonTai = await _db.KhachHang
                .AnyAsync(x => x.SoCCCD == request.SoCCCD && x.MaNguoiDung != maNguoiDung);

            if (daTonTai)
            {
                throw new Exception("Số CCCD này đã được sử dụng bởi tài khoản khác.");
            }

            // Cập nhật thông tin CCCD và trạng thái KYC
            khachHang.SoCCCD = request.SoCCCD;
            khachHang.TrangThaiKYC = "PENDING"; // Chờ duyệt

            await _db.SaveChangesAsync();

            return new
            {
                thongBao = "Gửi yêu cầu KYC thành công. Vui lòng chờ nhân viên ngân hàng duyệt.",
                soCCCD = khachHang.SoCCCD,
                trangThaiKYC = khachHang.TrangThaiKYC
            };
        }
    }
}
