using Model;
using Model.Requests;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IAuthService
    {
        // Hàm đăng ký khách hàng mới
        Task<object> DangKyKhachHangAsync(DangKyRequest request);

        // Hàm đăng nhập
        Task<NguoiDung> DangNhapAsync(string tenDangNhap, string matKhau);

        // Hàm đổi mật khẩu
        Task DoiMatKhauAsync(int maNguoiDung, string matKhauCu, string matKhauMoi);
    }
}
