using DAL;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Requests;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Services
{
    public class KhachHangService : IKhachHangService
    {
        private readonly NganHangDAL _db;

        public KhachHangService(NganHangDAL db)
        {
            _db = db;
        }

        // --- PHẦN DÀNH CHO KHÁCH HÀNG ---

        public async Task GuiYeuCauKycAsync(int maNguoiDung, string soCCCD)
        {
            // 1. Tìm thông tin khách hàng dựa trên mã người dùng
            var khachHang = await _db.KhachHang.FirstOrDefaultAsync(x => x.MaNguoiDung == maNguoiDung);

            // 2. Kiểm tra xem có tìm thấy không
            if (khachHang == null)
            {
                throw new Exception("Không tìm thấy hồ sơ khách hàng.");
            }

            // 3. Cập nhật thông tin CCCD và chuyển trạng thái
            khachHang.SoCCCD = soCCCD;
            khachHang.TrangThaiKYC = "PENDING"; // Chờ duyệt

            // 4. Lưu vào cơ sở dữ liệu
            await _db.SaveChangesAsync();
        }

        // --- PHẦN DÀNH CHO ADMIN / NHÂN VIÊN ---

        public async Task<object> LayDanhSachChoDuyetAsync()
        {
            // Lấy tất cả khách hàng có trạng thái là PENDING (Chờ duyệt)
            var danhSach = await _db.KhachHang
                .AsNoTracking() // Không cần theo dõi thay đổi (tối ưu tốc độ)
                .Where(k => k.TrangThaiKYC == "PENDING")
                .Select(k => new
                {
                    maKhachHang = k.MaKhachHang,
                    hoTen = k.HoTen,
                    email = k.Email,
                    soDienThoai = k.SoDienThoai,
                    soCCCD = k.SoCCCD,
                    trangThai = k.TrangThaiKYC
                })
                .ToListAsync();

            return new { tongSo = danhSach.Count, danhSach = danhSach };
        }

        public async Task DuyetYeuCauKycAsync(int maKhachHang, string trangThaiMoi, string lyDo)
        {
            // 1. Chuẩn hóa trạng thái (chữ hoa)
            trangThaiMoi = (trangThaiMoi ?? "").Trim().ToUpperInvariant();

            // 2. Kiểm tra trạng thái hợp lệ
            if (trangThaiMoi != "ACTIVE" && trangThaiMoi != "REJECT")
            {
                throw new Exception("Trạng thái chỉ nhận ACTIVE (Duyệt) hoặc REJECT (Từ chối).");
            }

            // 3. Tìm khách hàng trong Database
            var khachHang = await _db.KhachHang.FirstOrDefaultAsync(x => x.MaKhachHang == maKhachHang);
            if (khachHang == null)
            {
                throw new Exception("Không tìm thấy khách hàng.");
            }

            // 4. Chỉ được duyệt khi đang ở trạng thái PENDING (Chờ duyệt)
            if (khachHang.TrangThaiKYC != "PENDING")
            {
                throw new Exception($"Không thể duyệt vì trạng thái hiện tại là {khachHang.TrangThaiKYC}.");
            }

            // 5. Nếu Duyệt (ACTIVE) thì bắt buộc phải có số CCCD
            if (trangThaiMoi == "ACTIVE" && string.IsNullOrWhiteSpace(khachHang.SoCCCD))
            {
                throw new Exception("Khách chưa nộp CCCD, không thể duyệt.");
            }

            // 6. Cập nhật trạng thái
            khachHang.TrangThaiKYC = trangThaiMoi;

            // (Nếu muốn lưu lý do từ chối thì cần thêm cột vào Database, tạm thời bỏ qua)

            // 7. Lưu thay đổi
            await _db.SaveChangesAsync();
        }
    }
}
