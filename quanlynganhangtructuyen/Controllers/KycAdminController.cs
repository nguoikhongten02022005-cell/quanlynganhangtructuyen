using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/admin/kyc")]
    [Authorize(Roles = "ADMIN,STAFF")] // chỉ Admin & Nhân viên
    public class KycAdminController : ControllerBase
    {
        private readonly NganHangDAL _db;

        public KycAdminController(NganHangDAL db)
        {
            _db = db;
        }

        // GET api/admin/kyc/pending
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingKyc()
        {
            var list = await _db.KhachHang
                .Where(k => k.TrangThaiKYC == KycStatuses.Pending)
                .Select(k => new
                {
                    k.MaKhachHang,
                    k.HoTen,
                    k.SoCCCD,
                    k.Email,
                    k.SoDienThoai,
                    k.TrangThaiKYC
                })
                .ToListAsync();

            return Ok(list);
        }

        // PUT api/admin/kyc/{maKhachHang}/approve
        [HttpPut("{maKhachHang:int}/approve")]
        public async Task<IActionResult> ApproveKyc(int maKhachHang)
        {
            var kh = await _db.KhachHang.FindAsync(maKhachHang);
            if (kh == null)
                return NotFound("Không tìm thấy khách hàng.");

            kh.TrangThaiKYC = KycStatuses.Approved;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Đã duyệt KYC cho khách hàng.",
                kh.MaKhachHang,
                kh.TrangThaiKYC
            });
        }

        // PUT api/admin/kyc/{maKhachHang}/reject
        [HttpPut("{maKhachHang:int}/reject")]
        public async Task<IActionResult> RejectKyc(
            int maKhachHang,
            [FromBody] KycDecisionRequest request)
        {
            var kh = await _db.KhachHang.FindAsync(maKhachHang);
            if (kh == null)
                return NotFound("Không tìm thấy khách hàng.");

            kh.TrangThaiKYC = KycStatuses.Rejected;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Đã từ chối KYC khách hàng.",
                kh.MaKhachHang,
                kh.TrangThaiKYC,
                reason = request.LyDoTuChoi
            });
        }
    }
}
