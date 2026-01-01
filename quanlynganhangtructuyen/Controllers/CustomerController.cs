using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.Requests;
using System.Security.Claims;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/customer")]
    [Authorize(Roles = "CUSTOMER")]
    public class CustomerController : ControllerBase
    {
        private readonly IKhachHangService _dichVuKhachHang; // Tên biến tiếng Việt

        public CustomerController(IKhachHangService dichVuKhachHang)
        {
            _dichVuKhachHang = dichVuKhachHang;
        }

        // POST /api/customer/kyc
        [HttpPost("kyc")]
        public async Task<IActionResult> GuiYeuCauKyc([FromBody] KycSubmitRequest yeuCau)
        {
            // 1. Lấy dữ liệu đầu vào
            var soCCCD = (yeuCau.CccdNumber ?? "").Trim();
            if (string.IsNullOrWhiteSpace(soCCCD))
                return BadRequest(new { thongBao = "Vui lòng nhập số CCCD." });

            // 2. Lấy ID người dùng từ Token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var maNguoiDung))
                return Unauthorized(new { thongBao = "Token không hợp lệ." });

            // 3. Gọi Service để xử lý
            try
            {
                await _dichVuKhachHang.GuiYeuCauKycAsync(maNguoiDung, soCCCD);
                return Ok(new { thongBao = "Đã gửi yêu cầu KYC thành công, vui lòng chờ duyệt." });
            }
            catch (Exception ex)
            {
                // Nếu có lỗi (ví dụ không tìm thấy khách hàng), trả về lỗi 400 hoặc 404
                return BadRequest(new { thongBao = ex.Message });
            }
        }

        // GET /api/customer/profile
        [HttpGet("profile")]
        public async Task<IActionResult> LayThongTinHoSo()
        {
            // Lấy ID người dùng từ Token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var maNguoiDung))
                return Unauthorized(new { thongBao = "Token không hợp lệ." });

            try
            {
                var thongTinHoSo = await _dichVuKhachHang.LayThongTinHoSoAsync(maNguoiDung);
                return Ok(thongTinHoSo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { thongBao = ex.Message });
            }
        }
    }
}
