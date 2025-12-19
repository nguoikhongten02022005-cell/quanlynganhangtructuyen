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

        /// <summary>
        /// Đăng ký tài khoản mới cho khách hàng:
        /// - Hash mật khẩu
        /// - Tạo NguoiDung (role=CUSTOMER)
        /// - Tạo KhachHang (KYC=PENDING)
        /// - (Khuyến nghị) Tạo TaiKhoan số dư 0
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> DangKy([FromBody] DangKyRequest req)
        {
            // 1) Kiểm tra dữ liệu đầu vào tối thiểu
            if (string.IsNullOrWhiteSpace(req.TenDangNhap) ||
                string.IsNullOrWhiteSpace(req.MatKhau) ||
                string.IsNullOrWhiteSpace(req.HoTen))
            {
                return BadRequest(new { thongBao = "Thiếu TenDangNhap/MatKhau/HoTen." });
            }

            // 2) Kiểm tra trùng tên đăng nhập
            bool daTonTai = await _db.NguoiDung.AnyAsync(x => x.TenDangNhap == req.TenDangNhap);
            if (daTonTai)
                return Conflict(new { thongBao = "Tên đăng nhập đã tồn tại." });

            // 3) Hash mật khẩu
            string matKhauHash = BCrypt.Net.BCrypt.HashPassword(req.MatKhau);

            // 4) Dùng transaction để tạo đồng bộ (NguoiDung + KhachHang + TaiKhoan)
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // 4.1) Tạo người dùng
                var nguoiDung = new NguoiDung
                {
                    TenDangNhap = req.TenDangNhap,
                    MatKhauHash = matKhauHash,
                    VaiTro = "CUSTOMER",
                    NgayTao = DateTime.Now
                };
                _db.NguoiDung.Add(nguoiDung);
                await _db.SaveChangesAsync(); // lấy MaNguoiDung

                // 4.2) Tạo khách hàng (TrangThaiKYC mặc định "PENDING" theo model)
                var khachHang = new KhachHang
                {
                    MaNguoiDung = nguoiDung.MaNguoiDung,
                    HoTen = req.HoTen,
                    Email = req.Email,
                    SoDienThoai = req.SoDienThoai
                };
                _db.KhachHang.Add(khachHang);
                await _db.SaveChangesAsync(); // lấy MaKhachHang

                // 4.3) Tạo tài khoản ngân hàng số dư 0 (nếu bạn đã có entity + DbSet TaiKhoan)
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
                    thongBao = "Đăng ký thành công.",
                    maNguoiDung = nguoiDung.MaNguoiDung,
                    maKhachHang = khachHang.MaKhachHang,
                    soTaiKhoan = taiKhoan.SoTaiKhoan,
                    trangThaiKyc = khachHang.TrangThaiKYC
                });
            }
            catch
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { thongBao = "Lỗi hệ thống khi đăng ký." });
            }
        }

        /// <summary>
        /// Đăng nhập hệ thống -> trả JWT + role + họ tên
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> DangNhap([FromBody] DangNhapRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.TenDangNhap) || string.IsNullOrWhiteSpace(req.MatKhau))
                return BadRequest(new { thongBao = "Thiếu TenDangNhap/MatKhau." });

            var nguoiDung = await _db.NguoiDung.FirstOrDefaultAsync(x => x.TenDangNhap == req.TenDangNhap);
            if (nguoiDung == null)
                return Unauthorized(new { thongBao = "Sai tên đăng nhập hoặc mật khẩu." });

            bool dungMatKhau = BCrypt.Net.BCrypt.Verify(req.MatKhau, nguoiDung.MatKhauHash);
            if (!dungMatKhau)
                return Unauthorized(new { thongBao = "Sai tên đăng nhập hoặc mật khẩu." });

            var khachHang = await _db.KhachHang.FirstOrDefaultAsync(x => x.MaNguoiDung == nguoiDung.MaNguoiDung);
            string hoTen = khachHang?.HoTen ?? "";

            string token = TaoJwtToken(nguoiDung, hoTen);

            return Ok(new
            {
                token,
                role = nguoiDung.VaiTro,
                fullName = hoTen
            });
        }

        /// <summary>
        /// Đổi mật khẩu (người đang đăng nhập)
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> DoiMatKhau([FromBody] DoiMatKhauRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.MatKhauCu) || string.IsNullOrWhiteSpace(req.MatKhauMoi))
                return BadRequest(new { thongBao = "Thiếu MatKhauCu/MatKhauMoi." });

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
                // 14 số (bạn có thể đổi)
                string so = "10" + Random.Shared.NextInt64(100000000000, 999999999999).ToString();

                bool tonTai = await _db.TaiKhoan.AnyAsync(x => x.SoTaiKhoan == so);
                if (!tonTai) return so;
            }
        }

        private string TaoJwtToken(NguoiDung nguoiDung, string hoTen)
        {
            // Bạn cần có trong appsettings.json:
            // "Jwt": { "Key": "...", "Issuer": "...", "Audience": "...", "ExpMinutes": 60 }
            var key = _config["Jwt:Key"] ?? throw new Exception("Thiếu cấu hình Jwt:Key");
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            int expMinutes = int.TryParse(_config["Jwt:ExpMinutes"], out var m) ? m : 60;

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
