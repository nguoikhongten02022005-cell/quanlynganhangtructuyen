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

        public async Task<object> GuiKycAsync(int maNguoiDung, KycRequest request)
        {
            // Kiểm tra CCCD có được cung cấp không
            if (string.IsNullOrWhiteSpace(request.SoCCCD))
            {
                throw new Exception("Vui lòng cung cấp số CCCD.");
            }

            // Tìm khách hàng theo mã người dùng
            var khachHang = await _db.KhachHang.FirstOrDefaultAsync(x => x.MaNguoiDung == maNguoiDung);
            if (khachHang == null)
            {
                throw new Exception("Không tìm thấy hồ sơ khách hàng.");
            }

            // Kiểm tra nếu đã duyệt KYC rồi thì không cho gửi lại
            if (khachHang.TrangThaiKYC == "APPROVED")
            {
                throw new Exception("Hồ sơ KYC của bạn đã được duyệt. Không thể gửi lại.");
            }

            // Cập nhật thông tin KYC
            khachHang.SoCCCD = request.SoCCCD;
            khachHang.TrangThaiKYC = "PENDING"; // Đặt trạng thái chờ duyệt

            // Lưu thay đổi vào database
            await _db.SaveChangesAsync();

            return new
            {
                thongBao = "Đã gửi thông tin KYC thành công. Vui lòng chờ duyệt.",
                soCCCD = khachHang.SoCCCD,
                trangThaiKYC = khachHang.TrangThaiKYC
            };
        }
    }
}
