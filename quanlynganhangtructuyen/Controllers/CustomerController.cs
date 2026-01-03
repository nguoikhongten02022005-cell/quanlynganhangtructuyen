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
        private readonly IKhachHangService _dichVuKhachHang;

        public CustomerController(IKhachHangService dichVuKhachHang)
        {
            _dichVuKhachHang = dichVuKhachHang;
        }

        // GET /api/customer/profile
        [HttpGet("profile")]
        public async Task<IActionResult> LayThongTinHoSo()
        {
            // Lấy mã người dùng từ token
            var maNguoiDungStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(maNguoiDungStr, out var maNguoiDung))
                return Unauthorized(new { thongBao = "Token không hợp lệ." });

            try
            {
                var hoSo = await _dichVuKhachHang.LayThongTinHoSoAsync(maNguoiDung);
                return Ok(hoSo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { thongBao = ex.Message });
            }
        }

        // POST /api/customer/kyc
        [HttpPost("kyc")]
        public async Task<IActionResult> GuiKyc([FromBody] KycRequest request)
        {
            // Lấy mã người dùng từ token
            var maNguoiDungStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(maNguoiDungStr, out var maNguoiDung))
                return Unauthorized(new { thongBao = "Token không hợp lệ." });

            try
            {
                var ketQua = await _dichVuKhachHang.GuiKycAsync(maNguoiDung, request);
                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return BadRequest(new { thongBao = ex.Message });
            }
        }
    }
}
