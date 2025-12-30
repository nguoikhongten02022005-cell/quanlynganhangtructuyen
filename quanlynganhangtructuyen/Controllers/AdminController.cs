using DAL;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    }
}