using BLL.Services;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Model;
using Model.Requests;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly NganHangDAL _db; // Vẫn cần để lấy tên khách hàng khi login (tạm thời)
        private readonly IConfiguration _config;

        public AuthController(IAuthService authService, NganHangDAL db, IConfiguration config)
        {
            _authService = authService;
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

            try
            {
                var result = await _authService.DangKyKhachHangAsync(req);
                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("tồn tại"))
                    return Conflict(new { thongBao = ex.Message });

                return StatusCode(500, new { thongBao = "Lỗi hệ thống.", loi = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> DangNhap([FromBody] DangNhapRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.TenDangNhap) || string.IsNullOrWhiteSpace(req.MatKhau))
            {
                return BadRequest(new { thongBao = "Vui lòng nhập tên đăng nhập và mật khẩu." });
            }

            try
            {
                var nguoiDung = await _authService.DangNhapAsync(req.TenDangNhap, req.MatKhau);

                string hoTenHienThi = "";

                if (nguoiDung.VaiTro == "CUSTOMER")
                {
                    var khach = await _db.KhachHang.FirstOrDefaultAsync(x => x.MaNguoiDung == nguoiDung.MaNguoiDung);
                    if (khach == null)
                    {
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

                string token = TaoJwtToken(nguoiDung, hoTenHienThi);

                return Ok(new
                {
                    message = "Đăng nhập thành công",
                    token = token,
                    role = nguoiDung.VaiTro,
                    fullName = hoTenHienThi,
                    accountId = nguoiDung.MaNguoiDung
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { thongBao = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> DoiMatKhau([FromBody] DoiMatKhauRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.MatKhauCu) || string.IsNullOrWhiteSpace(req.MatKhauMoi))
                return BadRequest(new { thongBao = "Thiếu mật khẩu cũ hoặc mật khẩu mới." });

            string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out int maNguoiDung))
                return Unauthorized(new { thongBao = "Token không hợp lệ." });

            try
            {
                await _authService.DoiMatKhauAsync(maNguoiDung, req.MatKhauCu, req.MatKhauMoi);
                return Ok(new { thongBao = "Đổi mật khẩu thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { thongBao = ex.Message });
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
