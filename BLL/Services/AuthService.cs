using DAL.Interfaces;
using DAL;
using Model.Requests;
using Model.DTOs;
using System;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly INguoiDungRepository _nguoiDungRepo;
        private readonly IKhachHangRepository _khachHangRepo;
        private readonly ITaiKhoanRepository _taiKhoanRepo;
        private readonly PasswordResetDAL _passwordResetDal;

        public AuthService(
            INguoiDungRepository nguoiDungRepo,
            IKhachHangRepository khachHangRepo,
            ITaiKhoanRepository taiKhoanRepo,
            PasswordResetDAL passwordResetDal)
        {
            _nguoiDungRepo = nguoiDungRepo;
            _khachHangRepo = khachHangRepo;
            _taiKhoanRepo = taiKhoanRepo;
            _passwordResetDal = passwordResetDal;
        }

        private static string TaoTokenNgauNhien()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task<object> DangKyKhachHangAsync(DangKyRequest request)
        {
            if (await _nguoiDungRepo.KiemTraTonTaiTenDangNhapAsync(request.TenDangNhap))
            {
                throw new Exception("Tên đăng nhập đã tồn tại.");
            }

            string matKhauHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhau);

            int maNguoiDung = await _nguoiDungRepo.ThemNguoiDungAsync(
                request.TenDangNhap,
                matKhauHash,
                "CUSTOMER",
                "ACTIVE"
            );

            int maKhachHang = await _khachHangRepo.ThemKhachHangAsync(
                maNguoiDung,
                request.HoTen,
                request.Email,
                request.SoDienThoai,
                "NONE"
            );

            Random random = new Random();
            string soTaiKhoan = random.Next(100000000, 999999999).ToString();

            while (await _taiKhoanRepo.KiemTraSoTaiKhoanDaTonTaiAsync(soTaiKhoan))
            {
                soTaiKhoan = random.Next(100000000, 999999999).ToString();
            }

            await _taiKhoanRepo.ThemTaiKhoanAsync(
                maKhachHang,
                soTaiKhoan,
                0,
                "ACTIVE"
            );

            return new
            {
                thongBao = "Đăng ký thành công.",
                maNguoiDung = maNguoiDung,
                maKhachHang = maKhachHang,
                soTaiKhoan = soTaiKhoan
            };
        }

        public async Task<NguoiDungDTO> DangNhapAsync(string tenDangNhap, string matKhau)
        {
            var nguoiDung = await _nguoiDungRepo.GetNguoiDungByTenDangNhapAsync(tenDangNhap);

            if (nguoiDung == null)
            {
                throw new Exception("Tài khoản hoặc mật khẩu không đúng.");
            }

            if (!BCrypt.Net.BCrypt.Verify(matKhau, nguoiDung.MatKhauHash))
            {
                throw new Exception("Tài khoản hoặc mật khẩu không đúng.");
            }

            if (nguoiDung.TrangThai != "ACTIVE")
            {
                throw new Exception("Tài khoản đã bị khóa.");
            }

            return nguoiDung;
        }

        public async Task DoiMatKhauAsync(int maNguoiDung, string matKhauCu, string matKhauMoi)
        {
            var matKhauHashHienTai = await _nguoiDungRepo.GetMatKhauHashByIdAsync(maNguoiDung);

            if (matKhauHashHienTai == null)
            {
                throw new Exception("Không tìm thấy tài khoản.");
            }

            if (!BCrypt.Net.BCrypt.Verify(matKhauCu, matKhauHashHienTai))
            {
                throw new Exception("Mật khẩu cũ không đúng.");
            }

            string matKhauHashMoi = BCrypt.Net.BCrypt.HashPassword(matKhauMoi);

            await _nguoiDungRepo.CapNhatMatKhauAsync(maNguoiDung, matKhauHashMoi);
        }

        public async Task<object> TaoTokenQuenMatKhauAsync(string tenDangNhap)
        {
            var nguoiDung = await _nguoiDungRepo.GetNguoiDungByTenDangNhapAsync(tenDangNhap);
            if (nguoiDung == null)
            {
                // Thực tế nên trả 200 để tránh dò user, nhưng bài tập có thể báo lỗi rõ.
                throw new Exception("Không tìm thấy tài khoản.");
            }

            // Tạo token + lưu hash vào DB, hết hạn 10 phút
            var token = TaoTokenNgauNhien();
            var tokenHash = BCrypt.Net.BCrypt.HashPassword(token);
            var expiresAt = DateTime.UtcNow.AddMinutes(10);

            await _passwordResetDal.TaoTokenAsync(nguoiDung.MaNguoiDung, tokenHash, expiresAt);

            // Demo: trả token ra response để test (thay vì gửi email/sms)
            return new
            {
                thongBao = "Đã tạo token đặt lại mật khẩu.",
                tenDangNhap = nguoiDung.TenDangNhap,
                token = token,
                hetHanSauPhut = 10
            };
        }

        public async Task DatLaiMatKhauAsync(string tenDangNhap, string token, string matKhauMoi)
        {
            var nguoiDung = await _nguoiDungRepo.GetNguoiDungByTenDangNhapAsync(tenDangNhap);
            if (nguoiDung == null)
            {
                throw new Exception("Không tìm thấy tài khoản.");
            }

            // Vì BCrypt mỗi lần hash khác nhau, ta cần truy vấn token hợp lệ theo tokenHash.
            // Cách đơn giản: lưu tokenHash, khi verify thì phải so với hash.
            // Ở DAL hiện tại, LayTokenHopLeAsync nhận tokenHash chính xác; để hỗ trợ BCrypt verify,
            // ta sẽ dùng cách: thử lấy token hợp lệ bằng cách tìm token gần nhất của user rồi verify.
            // Để giữ thay đổi nhỏ, ta làm truy vấn token gần nhất ngay tại DAL theo hướng bổ sung.
            // => Thực hiện kiểm tra qua phương thức mới ở DAL (thêm nhẹ ở dưới nếu cần).

            // Fallback: dùng query riêng tại DAL để lấy tokenHash gần nhất còn hiệu lực.
            var latest = await _passwordResetDal.LayTokenGanNhatHopLeAsync(nguoiDung.MaNguoiDung);
            if (latest == null)
                throw new Exception("Token không hợp lệ hoặc đã hết hạn.");

            if (!BCrypt.Net.BCrypt.Verify(token, latest.Value.TokenHash))
                throw new Exception("Token không đúng.");

            var matKhauHashMoi = BCrypt.Net.BCrypt.HashPassword(matKhauMoi);
            await _nguoiDungRepo.CapNhatMatKhauAsync(nguoiDung.MaNguoiDung, matKhauHashMoi);
            await _passwordResetDal.DanhDauDaSuDungAsync(latest.Value.Id);
        }
    }
}
