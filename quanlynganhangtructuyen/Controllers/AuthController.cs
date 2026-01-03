using BLL.Services;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Model.Requests;
using Model.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly NganHangDAL _db;
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
                return BadRequest(new { thongBao = "Thiếu thông tin tên đăng nhập, mật khẩu hoặc họ tên." });

            // Validate độ dài và độ mạnh mật khẩu
            if (req.MatKhau.Length < 6)
                return BadRequest(new { thongBao = "Mật khẩu phải có ít nhất 6 ký tự." });

            if (req.MatKhau.Length > 50)
                return BadRequest(new { thongBao = "Mật khẩu không được quá 50 ký tự." });

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
                return BadRequest(new { thongBao = "Vui lòng nhập tên đăng nhập và mật khẩu." });

            try
            {
                var nguoiDung = await _authService.DangNhapAsync(req.TenDangNhap, req.MatKhau);
                var hoTenHienThi = await LayTenHienThi(nguoiDung);
                var token = TaoJwtToken(nguoiDung, hoTenHienThi);

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

        // Quên mật khẩu: nhập tên đăng nhập để nhận token (demo trả token trực tiếp)
        [HttpPost("forgot-password")]
        public async Task<IActionResult> QuenMatKhau([FromBody] QuenMatKhauRequest req)
        {
            var tenDangNhap = (req.TenDangNhap ?? "").Trim();
            if (string.IsNullOrWhiteSpace(tenDangNhap))
                return BadRequest(new { thongBao = "Vui lòng nhập tên đăng nhập." });

            try
            {
                var result = await _authService.TaoTokenQuenMatKhauAsync(tenDangNhap);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { thongBao = ex.Message });
            }
        }

        // Đặt lại mật khẩu: token + mật khẩu mới + nhập lại
        [HttpPost("reset-password")]
        public async Task<IActionResult> DatLaiMatKhau([FromBody] DatLaiMatKhauRequest req)
        {
            var tenDangNhap = (req.TenDangNhap ?? "").Trim();
            var token = (req.Token ?? "").Trim();
            var matKhauMoi = req.MatKhauMoi ?? "";
            var nhapLai = req.NhapLaiMatKhauMoi ?? "";

            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(token))
                return BadRequest(new { thongBao = "Thiếu tên đăng nhập hoặc token." });

            if (string.IsNullOrWhiteSpace(matKhauMoi) || string.IsNullOrWhiteSpace(nhapLai))
                return BadRequest(new { thongBao = "Vui lòng nhập mật khẩu mới và nhập lại." });

            if (matKhauMoi != nhapLai)
                return BadRequest(new { thongBao = "Mật khẩu nhập lại không khớp." });

            if (matKhauMoi.Length < 6)
                return BadRequest(new { thongBao = "Mật khẩu mới phải có ít nhất 6 ký tự." });

            if (matKhauMoi.Length > 50)
                return BadRequest(new { thongBao = "Mật khẩu mới không được quá 50 ký tự." });

            try
            {
                await _authService.DatLaiMatKhauAsync(tenDangNhap, token, matKhauMoi);
                return Ok(new { thongBao = "Đặt lại mật khẩu thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { thongBao = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> DoiMatKhau([FromBody] DoiMatKhauRequest req)
        {
            var maNguoiDung = LayMaNguoiDung();
            if (maNguoiDung == null)
                return KhongHopLe();

            if (string.IsNullOrWhiteSpace(req.MatKhauCu) || string.IsNullOrWhiteSpace(req.MatKhauMoi))
                return BadRequest(new { thongBao = "Thiếu mật khẩu cũ hoặc mật khẩu mới." });

            // Validate độ dài và độ mạnh mật khẩu mới
            if (req.MatKhauMoi.Length < 6)
                return BadRequest(new { thongBao = "Mật khẩu mới phải có ít nhất 6 ký tự." });

            if (req.MatKhauMoi.Length > 50)
                return BadRequest(new { thongBao = "Mật khẩu mới không được quá 50 ký tự." });

            if (req.MatKhauCu == req.MatKhauMoi)
                return BadRequest(new { thongBao = "Mật khẩu mới phải khác mật khẩu cũ." });

            try
            {
                await _authService.DoiMatKhauAsync(maNguoiDung.Value, req.MatKhauCu, req.MatKhauMoi);
                return ThanhCong("Đổi mật khẩu thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { thongBao = ex.Message });
            }
        }

        private async Task<string> LayTenHienThi(NguoiDungDTO nguoiDung)
        {
            if (nguoiDung.VaiTro == "CUSTOMER")
            {
                await using var conn = await _db.GetOpenConnectionAsync();
                var cmd = new SqlCommand("SELECT HoTen FROM KhachHang WHERE MaNguoiDung = @MaNguoiDung", conn);
                cmd.Parameters.AddWithValue("@MaNguoiDung", nguoiDung.MaNguoiDung);
                return (await cmd.ExecuteScalarAsync() as string) ?? "Khách hàng";
            }
            return nguoiDung.VaiTro switch
            {
                "ADMIN" => "Quản Trị Viên",
                "STAFF" => "Giao Dịch Viên",
                _ => "Người dùng"
            };
        }

        private string TaoJwtToken(NguoiDungDTO nguoiDung, string hoTen)
        {
            var key = _config["Jwt:Key"] ?? throw new Exception("Thiếu cấu hình Jwt:Key");
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            int expMinutes = int.TryParse(_config["Jwt:ExpiresMinutes"], out var m) ? m : 60;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, nguoiDung.MaNguoiDung.ToString()),
                new(ClaimTypes.Role, nguoiDung.VaiTro),
                new("hoTen", hoTen ?? ""),
                new("tenDangNhap", nguoiDung.TenDangNhap ?? "")
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
