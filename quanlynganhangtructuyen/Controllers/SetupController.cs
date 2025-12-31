using DAL;
using Microsoft.AspNetCore.Mvc;
using Model.Requests;
using BLL.Services; // Thêm namespace BLL

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/setup")]
    public class SetupController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly IUserService _userService; // Sử dụng UserService thay vì DbContext trực tiếp

        public SetupController(IConfiguration config, IWebHostEnvironment env, IUserService userService)
        {
            _config = config;
            _env = env;
            _userService = userService;
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
            var username = (req.UserName ?? "").Trim();
            var password = req.Password ?? "";
            var role = (req.Role ?? "").Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return BadRequest(new { thongBao = "Thiếu username/password." });

            if (role != "ADMIN" && role != "STAFF")
                return BadRequest(new { thongBao = "Role chỉ nhận ADMIN hoặc STAFF." });

            try
            {
                // Gọi xuống BLL để xử lý logic
                var user = await _userService.TaoNguoiDungHeThongAsync(username, password, role);

                return Ok(new
                {
                    thongBao = "Tạo tài khoản hệ thống thành công.",
                    maNguoiDung = user.MaNguoiDung,
                    role = user.VaiTro
                });
            }
            catch (Exception ex)
            {
                // Nếu BLL ném lỗi (ví dụ trùng username), trả về lỗi 400 hoặc 409
                return BadRequest(new { thongBao = ex.Message });
            }
        }
    }
}
