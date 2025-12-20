using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly NganHangDAL _db;
        private readonly IConfiguration _config;

        public AuthController(NganHangDAL db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> DangKy([FromBody] DangKyRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.TenDangNhap) ||
                string.IsNullOrWhiteSpace(req.MatKhau) ||
                string.IsNullOrWhiteSpace(req.HoTen))
            {
                return BadRequest(new { thongBao = "Thiếu thông tin tên đăng nhập, mật khẩu hoặc họ tên." });
            }

            bool daTonTai = await _db.NguoiDung.AnyAsync(x => x.TenDangNhap == req.TenDangNhap);
            if (daTonTai)
                return Conflict(new { thongBao = "Tên đăng nhập đã tồn tại." });

            string matKhauHash = BCrypt.Net.BCrypt.HashPassword(req.MatKhau);

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var nguoiDung = new NguoiDung
                {
                    TenDangNhap = req.TenDangNhap,
                    MatKhauHash = matKhauHash,
                    VaiTro = "CUSTOMER",
                    NgayTao = DateTime.Now
                };
                _db.NguoiDung.Add(nguoiDung);
                await _db.SaveChangesAsync();

                var khachHang = new KhachHang
                {
                    MaNguoiDung = nguoiDung.MaNguoiDung,
                    HoTen = req.HoTen,
                    Email = req.Email,
                    SoDienThoai = req.SoDienThoai
                };
                _db.KhachHang.Add(khachHang);
                await _db.SaveChangesAsync();

                var taiKhoan = new TaiKhoan
                {
                    MaKhachHang = khachHang.MaKhachHang,
                    SoTaiKhoan = await TaoSoTaiKhoanAsync(),
                    SoDu = 0,
                    TrangThai = "ACTIVE"
                };
                _db.TaiKhoan.Add(taiKhoan);
                await _db.SaveChangesAsync();

                await tx.CommitAsync();

                return Ok(new
                {
                    thongBao = "Đăng ký tài khoản thành công.",
                    maNguoiDung = nguoiDung.MaNguoiDung,
                    maKhachHang = khachHang.MaKhachHang,
                    soTaiKhoan = taiKhoan.SoTaiKhoan,
                    trangThaiKyc = khachHang.TrangThaiKYC
                });
            }
            catch
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { thongBao = "Lỗi hệ thống khi đăng ký tài khoản." });
            }
        }

        /// <summary>
        /// Đăng nhập và trả về JWT token + role + họ tên
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> DangNhap([FromBody] DangNhapRequest req)
        {
            // 1. Kiểm tra dữ liệu gửi lên
            if (string.IsNullOrWhiteSpace(req.TenDangNhap) || string.IsNullOrWhiteSpace(req.MatKhau))
            {
                return BadRequest(new { thongBao = "Vui lòng nhập tên đăng nhập và mật khẩu." });
            }

            // 2. Tìm User trong Database
            var nguoiDung = await _db.NguoiDung.FirstOrDefaultAsync(x => x.TenDangNhap == req.TenDangNhap);

            // Nếu không tìm thấy User
            if (nguoiDung == null)
            {
                return Unauthorized(new { thongBao = "Tài khoản hoặc mật khẩu không chính xác." });
            }

            // 3. Kiểm tra Mật khẩu (Dùng BCrypt để so sánh)
            bool dungMatKhau = BCrypt.Net.BCrypt.Verify(req.MatKhau, nguoiDung.MatKhauHash);
            if (!dungMatKhau)
            {
                return Unauthorized(new { thongBao = "Tài khoản hoặc mật khẩu không chính xác." });
            }

            // 4. Kiểm tra xem tài khoản có bị khóa không
            if (nguoiDung.TrangThai != "ACTIVE")
            {
                return Unauthorized(new { thongBao = "Tài khoản này đã bị khóa. Vui lòng liên hệ ngân hàng." });
            }

            // 5. Xử lý hiển thị Họ Tên (Logic phân quyền)
            string hoTenHienThi = "";

            if (nguoiDung.VaiTro == "CUSTOMER")
            {
                // Nếu là Khách -> Phải tìm tên trong bảng KhachHang
                var khach = await _db.KhachHang.FirstOrDefaultAsync(x => x.MaNguoiDung == nguoiDung.MaNguoiDung);
                if (khach == null)
                {
                    // Trường hợp lỗi dữ liệu: Có User nhưng chưa có thông tin Khách
                    hoTenHienThi = "Khách hàng (Lỗi hồ sơ)";
                }
                else
                {
                    hoTenHienThi = khach.HoTen;
                }
            }
            else if (nguoiDung.VaiTro == "ADMIN")
            {
                hoTenHienThi = "Quản Trị Viên (Admin)";
            }
            else if (nguoiDung.VaiTro == "STAFF")
            {
                hoTenHienThi = "Giao Dịch Viên";
            }

            // 6. Tạo Token
            string token = TaoJwtToken(nguoiDung, hoTenHienThi);

            // 7. Trả về kết quả
            return Ok(new
            {
                message = "Đăng nhập thành công",
                token = token,
                role = nguoiDung.VaiTro,
                fullName = hoTenHienThi,
                accountId = nguoiDung.MaNguoiDung
            });
        }

        /// <summary>
        /// Đổi mật khẩu (dành cho người đã đăng nhập)
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> DoiMatKhau([FromBody] DoiMatKhauRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.MatKhauCu) || string.IsNullOrWhiteSpace(req.MatKhauMoi))
                return BadRequest(new { thongBao = "Thiếu mật khẩu cũ hoặc mật khẩu mới." });

            string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out int maNguoiDung))
                return Unauthorized(new { thongBao = "Token không hợp lệ." });

            var nguoiDung = await _db.NguoiDung.FirstOrDefaultAsync(x => x.MaNguoiDung == maNguoiDung);
            if (nguoiDung == null)
                return Unauthorized(new { thongBao = "Người dùng không tồn tại." });

            bool dungMatKhauCu = BCrypt.Net.BCrypt.Verify(req.MatKhauCu, nguoiDung.MatKhauHash);
            if (!dungMatKhauCu)
                return BadRequest(new { thongBao = "Mật khẩu cũ không đúng." });

            nguoiDung.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(req.MatKhauMoi);
            await _db.SaveChangesAsync();

            return Ok(new { thongBao = "Đổi mật khẩu thành công." });
        }

        // ===================== HÀM PHỤ =====================

        private async Task<string> TaoSoTaiKhoanAsync()
        {
            while (true)
            {
                string so = "10" + Random.Shared.NextInt64(100000000000, 999999999999).ToString();

                bool tonTai = await _db.TaiKhoan.AnyAsync(x => x.SoTaiKhoan == so);
                if (!tonTai) return so;
            }
        }

        private string TaoJwtToken(NguoiDung nguoiDung, string hoTen)
        {
            var key = _config["Jwt:Key"] ?? throw new Exception("Thiếu cấu hình Jwt:Key");
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            int expMinutes = int.TryParse(_config["Jwt:ExpiresMinutes"], out var m) ? m : 60;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, nguoiDung.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Role, nguoiDung.VaiTro),
                new Claim("hoTen", hoTen ?? ""),
                new Claim("tenDangNhap", nguoiDung.TenDangNhap ?? "")
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
