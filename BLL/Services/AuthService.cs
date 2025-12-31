using DAL;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Requests;
using System;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly NganHangDAL _db;

        public AuthService(NganHangDAL db)
        {
            _db = db;
        }

        public async Task<object> DangKyKhachHangAsync(DangKyRequest request)
        {
            // 1. Kiểm tra trùng tên đăng nhập
            bool daTonTai = await _db.NguoiDung.AnyAsync(x => x.TenDangNhap == request.TenDangNhap);
            if (daTonTai)
            {
                throw new Exception("Tên đăng nhập đã tồn tại.");
            }

            // 2. Bắt đầu Transaction (Giao dịch) để đảm bảo toàn vẹn dữ liệu
            // Transaction giúp đảm bảo: Cả 3 bước (Tạo User, Khách, Tài khoản) phải cùng thành công.
            // Nếu 1 bước lỗi -> Hủy bỏ tất cả.
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Bước 2.1: Tạo User (NguoiDung)
                var nguoiDung = new NguoiDung
                {
                    TenDangNhap = request.TenDangNhap,
                    MatKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhau),
                    VaiTro = "CUSTOMER",
                    NgayTao = DateTime.Now,
                    TrangThai = "ACTIVE"
                };
                _db.NguoiDung.Add(nguoiDung);
                await _db.SaveChangesAsync(); // Lưu để lấy được MaNguoiDung

                // Bước 2.2: Tạo Khách Hàng (KhachHang)
                var khachHang = new KhachHang
                {
                    MaNguoiDung = nguoiDung.MaNguoiDung,
                    HoTen = request.HoTen,
                    Email = request.Email,
                    SoDienThoai = request.SoDienThoai,
                    TrangThaiKYC = "NONE" // Chưa KYC
                };
                _db.KhachHang.Add(khachHang);
                await _db.SaveChangesAsync(); // Lưu để lấy được MaKhachHang

                // Bước 2.3: Tạo Tài Khoản Ngân Hàng (TaiKhoan)
                var taiKhoan = new TaiKhoan
                {
                    MaKhachHang = khachHang.MaKhachHang,
                    SoTaiKhoan = await TaoSoTaiKhoanAsync(),
                    SoDu = 0,
                    TrangThai = "ACTIVE"
                };
                _db.TaiKhoan.Add(taiKhoan);
                await _db.SaveChangesAsync();

                // 3. Hoàn tất Transaction
                await tx.CommitAsync();

                // Trả về kết quả
                return new
                {
                    thongBao = "Đăng ký tài khoản thành công.",
                    maNguoiDung = nguoiDung.MaNguoiDung,
                    maKhachHang = khachHang.MaKhachHang,
                    soTaiKhoan = taiKhoan.SoTaiKhoan,
                    trangThaiKyc = khachHang.TrangThaiKYC
                };
            }
            catch (Exception)
            {
                // Nếu có lỗi, rollback (quay lại) trạng thái ban đầu
                await tx.RollbackAsync();
                throw; // Ném lỗi ra ngoài để Controller biết
            }
        }

        public async Task<NguoiDung> DangNhapAsync(string tenDangNhap, string matKhau)
        {
            // 1. Tìm User theo tên đăng nhập
            var nguoiDung = await _db.NguoiDung.FirstOrDefaultAsync(x => x.TenDangNhap == tenDangNhap);

            // 2. Kiểm tra User có tồn tại không
            if (nguoiDung == null)
            {
                throw new Exception("Tài khoản hoặc mật khẩu không chính xác.");
            }

            // 3. Kiểm tra Mật khẩu (So sánh hash)
            bool dungMatKhau = BCrypt.Net.BCrypt.Verify(matKhau, nguoiDung.MatKhauHash);
            if (!dungMatKhau)
            {
                throw new Exception("Tài khoản hoặc mật khẩu không chính xác.");
            }

            // 4. Kiểm tra xem tài khoản có bị khóa không
            if (nguoiDung.TrangThai != "ACTIVE")
            {
                throw new Exception("Tài khoản này đã bị khóa. Vui lòng liên hệ ngân hàng.");
            }

            return nguoiDung;
        }

        public async Task DoiMatKhauAsync(int maNguoiDung, string matKhauCu, string matKhauMoi)
        {
            var nguoiDung = await _db.NguoiDung.FirstOrDefaultAsync(x => x.MaNguoiDung == maNguoiDung);
            if (nguoiDung == null)
            {
                throw new Exception("Người dùng không tồn tại.");
            }

            // Kiểm tra mật khẩu cũ
            bool dungMatKhauCu = BCrypt.Net.BCrypt.Verify(matKhauCu, nguoiDung.MatKhauHash);
            if (!dungMatKhauCu)
            {
                throw new Exception("Mật khẩu cũ không đúng.");
            }

            // Cập nhật mật khẩu mới
            nguoiDung.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(matKhauMoi);
            await _db.SaveChangesAsync();
        }

        // Hàm phụ để sinh số tài khoản ngẫu nhiên
        private async Task<string> TaoSoTaiKhoanAsync()
        {
            while (true)
            {
                // Tạo số tài khoản bắt đầu bằng 10 + 12 số ngẫu nhiên
                string so = "10" + Random.Shared.NextInt64(100000000000, 999999999999).ToString();

                // Kiểm tra xem số này đã có ai dùng chưa
                bool tonTai = await _db.TaiKhoan.AnyAsync(x => x.SoTaiKhoan == so);

                // Nếu chưa dùng thì lấy số này
                if (!tonTai) return so;
            }
        }
    }
}
