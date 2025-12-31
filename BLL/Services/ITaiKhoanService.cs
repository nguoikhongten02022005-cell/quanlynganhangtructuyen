using System.Threading.Tasks;

namespace BLL.Services
{
    public interface ITaiKhoanService
    {
        // Hàm lấy thông tin tài khoản của tôi
        Task<object> LayThongTinTaiKhoanAsync(int maNguoiDung);
    }
}
