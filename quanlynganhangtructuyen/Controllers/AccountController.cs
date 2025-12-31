using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/account")]
    [Authorize(Roles = "CUSTOMER")]
    public class AccountController : ControllerBase
    {
        private readonly ITaiKhoanService _dichVuTaiKhoan;

        public AccountController(ITaiKhoanService dichVuTaiKhoan)
        {
            _dichVuTaiKhoan = dichVuTaiKhoan;
        }

        [HttpGet("my-account")]
        public async Task<IActionResult> LayThongTinTaiKhoan()
        {
            // Lấy ID người dùng từ Token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var maNguoiDung))
                return Unauthorized(new { thongBao = "Token không hợp lệ." });

            try
            {
                var thongTin = await _dichVuTaiKhoan.LayThongTinTaiKhoanAsync(maNguoiDung);
                return Ok(thongTin);
            }
            catch (Exception ex)
            {
                // Nếu lỗi do nghiệp vụ (chưa KYC, không có tài khoản...)
                return BadRequest(new { thongBao = ex.Message });
            }
        }
    }
}
