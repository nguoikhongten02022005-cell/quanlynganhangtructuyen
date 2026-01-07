using Model.DTOs;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IUserService
    {
        Task<object> GetUsersAsync(string? role, string? status);

        // Hàm tạo tài khoản Admin hoặc Staff
        Task<NguoiDungDTO> TaoNguoiDungHeThongAsync(string tenDangNhap, string matKhau, string vaiTro);

        // Hàm khóa hoặc mở khóa tài khoản
        Task KhoaMoKhoaTaiKhoanAsync(int maNguoiDung, bool khoa);

        // Lấy chi tiết người dùng
        Task<NguoiDungDTO> LayChiTietNguoiDungAsync(int maNguoiDung);

        // Lấy thống kê dashboard
        Task<AdminDashboardDTO> LayThongKeDashboardAsync();

        // Reset mật khẩu người dùng (chỉ Admin)
        Task ResetMatKhauAsync(int maNguoiDung, string matKhauMoi);

        // Lấy danh sách tài khoản ngân hàng
        Task<List<TaiKhoanDTO>> LayDanhSachTaiKhoanAsync();

        // Lấy chi tiết tài khoản ngân hàng
        Task<TaiKhoanDTO?> LayChiTietTaiKhoanAsync(int maTaiKhoan);

        // Khóa/Mở tài khoản ngân hàng
        Task KhoaMoKhoaTaiKhoanNganHangAsync(int maTaiKhoan, bool khoa);
    }
}
