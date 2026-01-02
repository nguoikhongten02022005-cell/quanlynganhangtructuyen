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

        [HttpGet("profile")]
        public async Task<IActionResult> LayThongTinHoSo()
        {
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
        [HttpPost("kyc")]
        public async Task<IActionResult> GuiYeuCauKyc([FromBody] KycRequest request)
        {
            var maNguoiDungStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(maNguoiDungStr, out var maNguoiDung))
                return Unauthorized(new { thongBao = "Token không hợp lệ." });

            try
            {
                var ketQua = await _dichVuKhachHang.GuiYeuCauKycAsync(maNguoiDung, request);
                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return BadRequest(new { thongBao = ex.Message });
            }
        }
    }
}
