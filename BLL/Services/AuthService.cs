using DAL.Interfaces;
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

        public AuthService(
            INguoiDungRepository nguoiDungRepo,
            IKhachHangRepository khachHangRepo,
            ITaiKhoanRepository taiKhoanRepo)
        {
            _nguoiDungRepo = nguoiDungRepo;
            _khachHangRepo = khachHangRepo;
            _taiKhoanRepo = taiKhoanRepo;
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
    }
}
