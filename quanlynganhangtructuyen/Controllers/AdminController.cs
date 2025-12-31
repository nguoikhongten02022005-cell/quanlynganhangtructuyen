using DAL;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Requests;

namespace quanlynganhangtructuyen.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public class AdminController : ControllerBase
    {
        private readonly NganHangDAL _db;
        public AdminController(NganHangDAL db) { _db = db; }

        // GET /api/admin/kyc-pending
        [HttpGet("kyc-pending")]
        public async Task<IActionResult> GetKycPending()
        {
            var items = await _db.KhachHang
                .AsNoTracking()
                .Where(k => k.TrangThaiKYC == "PENDING")
                .Select(k => new
                {
                    customerId = k.MaKhachHang,
                    fullName = k.HoTen,
                    email = k.Email,
                    phone = k.SoDienThoai,
                    cccd = k.SoCCCD,
                    kycStatus = k.TrangThaiKYC
                })
                .ToListAsync();

            return Ok(new { total = items.Count, items });
        }
        // POST /api/admin/kyc-approve
        [HttpPost("kyc-approve")]
        public async Task<IActionResult> KycApprove([FromBody] KycApproveRequest req)
        {
            var status = (req.Status ?? "").Trim().ToUpperInvariant();
            if (status != "ACTIVE" && status != "REJECT")
                return BadRequest(new { thongBao = "Status chỉ nhận ACTIVE hoặc REJECT." });

            var khach = await _db.KhachHang.FirstOrDefaultAsync(x => x.MaKhachHang == req.CustomerId);
            if (khach == null)
                return NotFound(new { thongBao = "Không tìm thấy khách hàng." });

            // Chỉ duyệt khi đang PENDING (đúng quy trình)
            if (khach.TrangThaiKYC != "PENDING")
                return BadRequest(new { thongBao = $"Không thể duyệt vì trạng thái hiện tại là {khach.TrangThaiKYC}." });

            // Nếu duyệt ACTIVE mà chưa có CCCD thì chặn
            if (status == "ACTIVE" && string.IsNullOrWhiteSpace(khach.SoCCCD))
                return BadRequest(new { thongBao = "Khách chưa nộp CCCD, không thể duyệt." });

            khach.TrangThaiKYC = status;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                thongBao = status == "ACTIVE" ? "Đã duyệt KYC." : "Đã từ chối KYC.",
                customerId = khach.MaKhachHang,
                kycStatus = khach.TrangThaiKYC,
                reason = req.Reason // hiện DB chưa có cột để lưu nên chỉ trả về cho bạn thấy
            });
        }

    }
}