using Model.Requests;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IKhachHangService
    {
        Task<object> GuiYeuCauKycAsync(int maNguoiDung, KycRequest request);
        Task<object> LayThongTinHoSoAsync(int maNguoiDung);
        Task<object> LayThongTinTaiKhoanAsync(int maNguoiDung);
    }
}
