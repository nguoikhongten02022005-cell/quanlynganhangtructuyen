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
        private readonly IKhachHangService _dichVuKhachHang;

        public AccountController(IKhachHangService dichVuKhachHang)
        {
            _dichVuKhachHang = dichVuKhachHang;
        }

        [HttpGet("my-account")]
        public async Task<IActionResult> LayThongTinTaiKhoan()
        {
            var maNguoiDungStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(maNguoiDungStr, out var maNguoiDung))
                return Unauthorized(new { thongBao = "Token không hợp lệ." });

            try
            {
                var thongTinTaiKhoan = await _dichVuKhachHang.LayThongTinTaiKhoanAsync(maNguoiDung);
                return Ok(new { data = thongTinTaiKhoan });
            }
            catch (Exception ex)
            {
                return BadRequest(new { thongBao = ex.Message });
            }
        }
    }
}