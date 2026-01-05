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
    }
}
