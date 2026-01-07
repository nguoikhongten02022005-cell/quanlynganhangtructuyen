using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace quanlynganhangtructuyen.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected int? LayMaNguoiDung()
        {
            var maStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(maStr, out int ma) ? ma : null;
        }

        protected string? LayVaiTro()
        {
            return User.FindFirstValue(ClaimTypes.Role);
        }

        protected IActionResult ThanhCong(string thongBao)
        {
            return Ok(new { thongBao });
        }

        protected IActionResult ThanhCong(object data, string? thongBao = null)
        {
            if (string.IsNullOrEmpty(thongBao))
                return Ok(data);
            return Ok(new { thongBao, data });
        }

        protected IActionResult ThatBai(string thongBao)
        {
            return BadRequest(new { thongBao });
        }

        protected IActionResult KhongHopLe(string thongBao = "Token không hợp lệ.")
        {
            return Unauthorized(new { thongBao });
        }

        protected IActionResult KhongTimThay(string thongBao)
        {
            return NotFound(new { thongBao });
        }
    }
}
