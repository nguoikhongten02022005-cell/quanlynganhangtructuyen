using Model;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IUserService
    {
        Task<object> GetUsersAsync(string? role, string? status);

        // Hàm tạo tài khoản Admin hoặc Staff
        Task<Model.NguoiDung> TaoNguoiDungHeThongAsync(string tenDangNhap, string matKhau, string vaiTro);
    }
}
