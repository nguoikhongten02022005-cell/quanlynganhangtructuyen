using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Requests;
using System.Security.Claims;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/customer")]
    [Authorize(Roles = "CUSTOMER")]
    public class CustomerController : ControllerBase
    {
        private readonly NganHangDAL _db;
        public CustomerController(NganHangDAL db) { _db = db; }

        // POST /api/customer/kyc
        [HttpPost("kyc")]
        public async Task<IActionResult> SubmitKyc([FromBody] KycSubmitRequest req)
        {
            var cccd = (req.CccdNumber ?? "").Trim();
            if (string.IsNullOrWhiteSpace(cccd))
                return BadRequest(new { thongBao = "Vui lòng nhập số CCCD." });

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var maNguoiDung))
                return Unauthorized(new { thongBao = "Token không hợp lệ." });

            var khach = await _db.KhachHang.FirstOrDefaultAsync(x => x.MaNguoiDung == maNguoiDung);
            if (khach == null)
                return NotFound(new { thongBao = "Không tìm thấy hồ sơ khách hàng." });

            khach.SoCCCD = cccd;
            khach.TrangThaiKYC = "PENDING";
            await _db.SaveChangesAsync();

            return Ok(new { thongBao = "Đã gửi KYC, vui lòng chờ duyệt.", kycStatus = khach.TrangThaiKYC });
        }
    }
}
