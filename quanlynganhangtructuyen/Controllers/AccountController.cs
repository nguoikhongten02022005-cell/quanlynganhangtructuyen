using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/account")]
    [Authorize(Roles = "CUSTOMER")]
    public class AccountController : ControllerBase
    {
        private readonly NganHangDAL _db;
        public AccountController(NganHangDAL db) => _db = db;

        [HttpGet("my-account")]
        public async Task<IActionResult> MyAccount()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { thongBao = "Token không hợp lệ." });

            var kh = await _db.KhachHang.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaNguoiDung == userId);

            if (kh == null) return NotFound(new { thongBao = "Không tìm thấy khách hàng." });
            if (kh.TrangThaiKYC != "ACTIVE")
                return BadRequest(new { thongBao = "KYC chưa được duyệt, không thể xem tài khoản." });

            var tk = await _db.TaiKhoan.AsNoTracking()
                .FirstOrDefaultAsync(t => t.MaKhachHang == kh.MaKhachHang);

            if (tk == null) return NotFound(new { thongBao = "Chưa có tài khoản ngân hàng." });

            return Ok(new { soTaiKhoan = tk.SoTaiKhoan, soDu = tk.SoDu });
        }
    }
}
