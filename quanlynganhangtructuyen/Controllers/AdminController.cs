using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.Requests;
using System.Threading.Tasks;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _dichVuNguoiDung;
        private readonly IKhachHangService _dichVuKhachHang;

        public AdminController(IUserService dichVuNguoiDung, IKhachHangService dichVuKhachHang)
        {
            _dichVuNguoiDung = dichVuNguoiDung;
            _dichVuKhachHang = dichVuKhachHang;
        }

        // GET /api/admin/kyc-pending
        [HttpGet("kyc-pending")]
        public async Task<IActionResult> LayDanhSachChoDuyet()
        {
            var ketQua = await _dichVuKhachHang.LayDanhSachChoDuyetAsync();
            return Ok(ketQua);
        }

        // POST /api/admin/kyc-approve
        [HttpPost("kyc-approve")]
        public async Task<IActionResult> DuyetKyc([FromBody] KycApproveRequest yeuCau)
        {
            try
            {
                await _dichVuKhachHang.DuyetYeuCauKycAsync(yeuCau.CustomerId, yeuCau.Status, yeuCau.Reason);

                return Ok(new
                {
                    thongBao = yeuCau.Status == "ACTIVE" ? "Đã duyệt KYC." : "Đã từ chối KYC.",
                    maKhachHang = yeuCau.CustomerId,
                    trangThaiMoi = yeuCau.Status
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { thongBao = ex.Message });
            }
        }

        // GET /api/admin/users
        [HttpGet("users")]
        public async Task<IActionResult> LayDanhSachNguoiDung([FromQuery] string? role, [FromQuery] string? status)
        {
            var ketQua = await _dichVuNguoiDung.GetUsersAsync(role, status);
            return Ok(ketQua);
        }

        // PUT /api/admin/users/{id}/lock
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
                return BadRequest(new { thongBao = ex.Message });
            }
        }
    }
}
