using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.Requests;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public class AdminController : BaseController
    {
        private readonly IUserService _dichVuNguoiDung;
        private readonly IKhachHangService _dichVuKhachHang;

        public AdminController(IUserService dichVuNguoiDung, IKhachHangService dichVuKhachHang)
        {
            _dichVuNguoiDung = dichVuNguoiDung;
            _dichVuKhachHang = dichVuKhachHang;
        }

        [HttpGet("users")]
        public async Task<IActionResult> LayDanhSachNguoiDung([FromQuery] string? role, [FromQuery] string? status)
        {
            var ketQua = await _dichVuNguoiDung.GetUsersAsync(role, status);
            return Ok(ketQua);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> LayChiTietNguoiDung(int id)
        {
            try
            {
                var nguoiDung = await _dichVuNguoiDung.LayChiTietNguoiDungAsync(id);
                return Ok(new { success = true, data = nguoiDung });
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> LayThongKeDashboard()
        {
            try
            {
                var thongKe = await _dichVuNguoiDung.LayThongKeDashboardAsync();
                return Ok(new { success = true, data = thongKe });
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }

        [HttpGet("kyc-pending")]
        public async Task<IActionResult> LayDanhSachKYCChoDuyet()
        {
            try
            {
                var danhSach = await _dichVuKhachHang.LayDanhSachKYCChoDuyetAsync();
                return Ok(new { success = true, data = danhSach, tongSo = danhSach.Count });
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }

        [HttpPut("users/{id}/lock")]
        public async Task<IActionResult> KhoaTaiKhoan(int id, [FromBody] KhoaTaiKhoanRequest yeuCau)
        {
            try
            {
                await _dichVuNguoiDung.KhoaMoKhoaTaiKhoanAsync(id, yeuCau.Khoa);
                return Ok(new
                {
                    thongBao = yeuCau.Khoa ? "Đã khóa tài khoản thành công." : "Đã mở khóa tài khoản thành công.",
                    maNguoiDung = id,
                    trangThaiMoi = yeuCau.Khoa ? "LOCKED" : "ACTIVE"
                });
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }

        [HttpPost("kyc-approve")]
        public async Task<IActionResult> DuyetKyc([FromBody] KycApproveRequest request)
        {
            try
            {
                var ketQua = await _dichVuKhachHang.DuyetKYCAsync(request.CustomerId, request.Status, request.Reason);
                return Ok(ketQua);
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }

        [HttpPost("users/{id}/reset-password")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ResetMatKhau(int id, [FromBody] ResetPasswordRequest request)
        {
            try
            {
                await _dichVuNguoiDung.ResetMatKhauAsync(id, request.MatKhauMoi);
                return Ok(new { success = true, message = "Reset mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> LayDanhSachTaiKhoan()
        {
            try
            {
                var danhSach = await _dichVuNguoiDung.LayDanhSachTaiKhoanAsync();
                return Ok(new { success = true, data = danhSach, tongSo = danhSach.Count });
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }

        [HttpGet("accounts/{id}")]
        public async Task<IActionResult> LayChiTietTaiKhoan(int id)
        {
            try
            {
                var taiKhoan = await _dichVuNguoiDung.LayChiTietTaiKhoanAsync(id);
                if (taiKhoan == null)
                    return KhongTimThay("Tài khoản không tồn tại.");
                return Ok(new { success = true, data = taiKhoan });
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }

        [HttpPut("accounts/{id}/lock")]
        public async Task<IActionResult> KhoaTaiKhoanNganHang(int id, [FromBody] KhoaTaiKhoanRequest yeuCau)
        {
            try
            {
                await _dichVuNguoiDung.KhoaMoKhoaTaiKhoanNganHangAsync(id, yeuCau.Khoa);
                return Ok(new
                {
                    thongBao = yeuCau.Khoa ? "Đã khóa tài khoản ngân hàng." : "Đã mở khóa tài khoản ngân hàng.",
                    maTaiKhoan = id,
                    trangThaiMoi = yeuCau.Khoa ? "LOCKED" : "ACTIVE"
                });
            }
            catch (Exception ex)
            {
                return ThatBai(ex.Message);
            }
        }
    }
}
