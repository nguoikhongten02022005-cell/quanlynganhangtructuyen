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

            var usersData = await query
                .GroupJoin(_db.KhachHang, 
                    u => u.MaNguoiDung, 
                    k => k.MaNguoiDung, 
                    (u, k) => new { u, k = k.FirstOrDefault() })
                .Select(x => new
                {
                    maNguoiDung = x.u.MaNguoiDung,
                    tenDangNhap = x.u.TenDangNhap,
                    vaiTro = x.u.VaiTro,
                    trangThai = x.u.TrangThai,
                    hoTen = x.k != null ? x.k.HoTen : (x.u.VaiTro == "ADMIN" ? "Quản Trị Viên" : "Nhân Viên"),
                    email = x.k != null ? x.k.Email : null,
                    soDienThoai = x.k != null ? x.k.SoDienThoai : null,
                    ngayTao = x.u.NgayTao
                })
                .ToListAsync();

            return new { tongSo = usersData.Count, danhSach = usersData };
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

        public async Task KhoaMoKhoaTaiKhoanAsync(int maNguoiDung, bool khoa)
        {
            // 1. Tìm người dùng
            var nguoiDung = await _db.NguoiDung.FirstOrDefaultAsync(u => u.MaNguoiDung == maNguoiDung);

            // 2. Kiểm tra tồn tại
            if (nguoiDung == null)
            {
                throw new Exception("Người dùng không tồn tại.");
            }

            // 3. Không cho phép khóa chính mình hoặc khóa Admin khác nếu không đủ quyền (Logic đơn giản hóa: Admin nào cũng khóa được)
            // Tuy nhiên, nên chặn khóa Admin chính (ví dụ ID = 1) nếu cần thiết.

            // 4. Cập nhật trạng thái
            if (khoa)
            {
                nguoiDung.TrangThai = "LOCKED";
            }
            else
            {
                nguoiDung.TrangThai = "ACTIVE";
            }

            // 5. Lưu thay đổi
            await _db.SaveChangesAsync();
        }
    }
}
