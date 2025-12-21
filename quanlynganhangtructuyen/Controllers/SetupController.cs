using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/setup")]
    public class SetupController : ControllerBase
    {
        private readonly NganHangDAL _db;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public SetupController(NganHangDAL db, IConfiguration config, IWebHostEnvironment env)
        {
            _db = db;
            _config = config;
            _env = env;
        }

        public class CreateSystemUserRequest
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
            public string Role { get; set; } = "STAFF"; // ADMIN hoặc STAFF
        }

        [HttpPost("create-system-user")]
        public async Task<IActionResult> CreateSystemUser(
            [FromHeader(Name = "X-Setup-Key")] string setupKey,
            [FromBody] CreateSystemUserRequest req)
        {
            // Chỉ cho chạy ở Development (an toàn)
            if (!_env.IsDevelopment()) return NotFound();

            // Bật/tắt bằng config
            if (!_config.GetValue<bool>("Setup:Enabled")) return NotFound();

            // Check key
            var key = _config["Setup:Key"];
            if (string.IsNullOrWhiteSpace(key) || setupKey != key)
                return Unauthorized(new { thongBao = "Setup key không đúng." });

            // Validate input
            var username = (req.Username ?? "").Trim();
            var password = req.Password ?? "";
            var role = (req.Role ?? "").Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return BadRequest(new { thongBao = "Thiếu username/password." });

            if (role != "ADMIN" && role != "STAFF")
                return BadRequest(new { thongBao = "Role chỉ nhận ADMIN hoặc STAFF." });

            // Không cho tạo trùng username
            if (await _db.NguoiDung.AnyAsync(x => x.TenDangNhap == username))
                return Conflict(new { thongBao = "Tên đăng nhập đã tồn tại." });

            // Hash BCrypt rồi lưu DB (để login Verify được)
            var user = new NguoiDung
            {
                TenDangNhap = username,
                MatKhauHash = BCrypt.Net.BCrypt.HashPassword(password),
                VaiTro = role,
                TrangThai = "ACTIVE",
                NgayTao = DateTime.Now
            };

            _db.NguoiDung.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                thongBao = "Tạo tài khoản hệ thống thành công.",
                maNguoiDung = user.MaNguoiDung,
                role = user.VaiTro
            });
        }
    }
}
