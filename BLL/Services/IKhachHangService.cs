using Model.Requests;
using Model.DTOs;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IKhachHangService
    {
        Task<object> GuiYeuCauKycAsync(int maNguoiDung, KycRequest request);
        Task<KhachHangProfileDTO> LayThongTinHoSoAsync(int maNguoiDung);
        Task<TaiKhoanDTO> LayThongTinTaiKhoanAsync(int maNguoiDung);
        Task<object> LayDanhSachKycPendingAsync();
        Task<object> DuyetKYCAsync(int customerId, string status, string? reason = null);
    }
}
