using Model.Requests;
using Model.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BLL.Services
{
    public interface IKhachHangService
    {
        Task<object> GuiYeuCauKycAsync(int maNguoiDung, KycRequest request);
        Task<KhachHangProfileDTO> LayThongTinHoSoAsync(int maNguoiDung);
        Task<TaiKhoanDTO> LayThongTinTaiKhoanAsync(int maNguoiDung);
        Task<object> LayDanhSachKycPendingAsync();
        Task<object> DuyetKYCAsync(int maKhachHang, string status, string? reason = null);
        Task<List<KYCPendingDTO>> LayDanhSachKYCChoDuyetAsync();
        Task<object> CapNhatThongTinCaNhanAsync(int maNguoiDung, UpdateProfileRequest request);
    }
}
