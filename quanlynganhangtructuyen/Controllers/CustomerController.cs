using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.Requests;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/customer")]
    [Authorize(Roles = "CUSTOMER")]
    public class CustomerController : BaseController
    {
        private readonly IKhachHangService _dichVuKhachHang;

        public CustomerController(IKhachHangService dichVuKhachHang)
        {
            _dichVuKhachHang = dichVuKhachHang;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> LayThongTinHoSo()
        {
            var maNguoiDung = LayMaNguoiDung();
            if (maNguoiDung == null)
                return KhongHopLe();

            try
            {
                var hoSo = await _dichVuKhachHang.LayThongTinHoSoAsync(maNguoiDung.Value);
                return Ok(hoSo);
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> CapNhatThongTinCaNhan([FromBody] UpdateProfileRequest request)
        {
            var maNguoiDung = LayMaNguoiDung();
            if (maNguoiDung == null)
                return KhongHopLe();

            try
            {
                var ketQua = await _dichVuKhachHang.CapNhatThongTinCaNhanAsync(maNguoiDung.Value, request);
                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }

        [HttpPost("kyc")]
        public async Task<IActionResult> GuiYeuCauKyc([FromBody] KycRequest request)
        {
            var maNguoiDung = LayMaNguoiDung();
            if (maNguoiDung == null)
                return KhongHopLe();

            try
            {
                var ketQua = await _dichVuKhachHang.GuiYeuCauKycAsync(maNguoiDung.Value, request);
                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }
    }
}
