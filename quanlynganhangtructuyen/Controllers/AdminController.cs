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

        public AdminController(IUserService dichVuNguoiDung)
        {
            _dichVuNguoiDung = dichVuNguoiDung;
        }

        // ==================== PHẦN KYC - NGƯỜI KHÁC LÀM ====================
        // GET /api/admin/kyc-pending
        // POST /api/admin/kyc-approve
        // Các API này sẽ được người làm phần B (CustomerController) bổ sung sau
        // khi họ tạo IKhachHangService và KhachHangService
        // ===================================================================

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

        // GET /api/admin/dashboard - Báo cáo tổng quan (tối giản)
        // TODO: Sẽ bổ sung sau khi có đầy đủ dữ liệu từ các phần khác
    }
}
