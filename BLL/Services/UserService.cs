using DAL;
using Microsoft.EntityFrameworkCore;
using Model;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly NganHangDAL _db;

        public UserService(NganHangDAL db)
        {
            _db = db;
        }

        public async Task<object> GetUsersAsync(string? role, string? status)
        {
            var query = _db.NguoiDung.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.VaiTro == role.ToUpperInvariant());
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(u => u.TrangThai == status.ToUpperInvariant());
            }

            var users = await query
                .Select(u => new
                {
                    userId = u.MaNguoiDung,
                    username = u.TenDangNhap,
                    role = u.VaiTro,
                    status = u.TrangThai,
                    createdAt = u.NgayTao
                })
                .ToListAsync();

            return new { total = users.Count, users };
        }

        /// <summary>
        /// Hàm tạo tài khoản dành cho Admin hoặc Staff (Không cần tạo thông tin Khách hàng)
        /// </summary>
        public async Task<Model.NguoiDung> TaoNguoiDungHeThongAsync(string tenDangNhap, string matKhau, string vaiTro)
        {
            // 1. Kiểm tra xem tên đăng nhập đã tồn tại chưa
            bool daTonTai = await _db.NguoiDung.AnyAsync(u => u.TenDangNhap == tenDangNhap);
            if (daTonTai)
            {
                // Ném ra lỗi để Controller bắt được
                throw new Exception("Tên đăng nhập đã tồn tại.");
            }

            // 2. Tạo đối tượng người dùng mới
            var nguoiDungMoi = new NguoiDung
            {
                TenDangNhap = tenDangNhap,
                // Mã hóa mật khẩu trước khi lưu
                MatKhauHash = BCrypt.Net.BCrypt.HashPassword(matKhau),
                VaiTro = vaiTro, // ADMIN hoặc STAFF
                TrangThai = "ACTIVE",
                NgayTao = DateTime.Now
            };

            // 3. Lưu vào Database
            _db.NguoiDung.Add(nguoiDungMoi);
            await _db.SaveChangesAsync();

            return nguoiDungMoi;
        }
    }
}
