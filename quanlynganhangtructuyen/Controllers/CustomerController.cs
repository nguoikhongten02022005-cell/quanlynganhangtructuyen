using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model;
using System.Security.Claims;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/customer")]
    [Authorize(Roles = "CUSTOMER")] // chỉ khách hàng được dùng
    public class CustomerController : ControllerBase
    {
        private readonly NganHangDAL _db;

        public CustomerController(NganHangDAL db)
        {
            _db = db;
        }

        // GET api/customer/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var maNguoiDung))
            {
                return Unauthorized("Token không hợp lệ.");
            }

            var khachHang = await _db.KhachHang
                .FirstOrDefaultAsync(k => k.MaNguoiDung == maNguoiDung);

            if (khachHang == null)
            {
                return NotFound("Không tìm thấy hồ sơ khách hàng.");
            }

            return Ok(new
            {
                khachHang.MaKhachHang,
                khachHang.HoTen,
                khachHang.SoCCCD,
                khachHang.Email,
                khachHang.SoDienThoai,
                khachHang.TrangThaiKYC
            });
        }

        // PUT api/customer/kyc
        [HttpPut("kyc")]
        public async Task<IActionResult> UpdateKyc([FromBody] KycUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.HoTen) ||
                string.IsNullOrWhiteSpace(request.SoCCCD))
            {
                return BadRequest("Họ tên và số CCCD là bắt buộc.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var maNguoiDung))
            {
                return Unauthorized("Token không hợp lệ.");
            }

            var khachHang = await _db.KhachHang
                .FirstOrDefaultAsync(k => k.MaNguoiDung == maNguoiDung);

            if (khachHang == null)
            {
                return NotFound("Không tìm thấy hồ sơ khách hàng.");
            }

            khachHang.HoTen = request.HoTen.Trim();
            khachHang.SoCCCD = request.SoCCCD.Trim();
            khachHang.Email = request.Email?.Trim();
            khachHang.SoDienThoai = request.SoDienThoai?.Trim();

            // sau khi khách cập nhật, để trạng thái lại PENDING
            khachHang.TrangThaiKYC = KycStatuses.Pending;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Đã cập nhật thông tin KYC, vui lòng chờ duyệt.",
                khachHang.TrangThaiKYC
            });
        }
    }
}
