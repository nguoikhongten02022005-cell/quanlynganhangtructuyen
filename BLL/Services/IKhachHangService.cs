using Model.Requests;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IKhachHangService
    {
        // Lấy thông tin hồ sơ khách hàng
        Task<object> LayThongTinHoSoAsync(int maNguoiDung);

        // Gửi thông tin KYC
        Task<object> GuiKycAsync(int maNguoiDung, KycRequest request);
    }
}
