using Model.DTOs;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface ITransactionService
    {
        Task<TraCuuNguoiNhanDTO> TraCuuTaiKhoanNhanAsync(string soTaiKhoan);
        Task<TaoGiaoDichResponseDTO> TaoGiaoDichVoiOTPAsync(int maNguoiDung, string soTaiKhoanNhan, decimal soTien, string noiDung);
        Task<object> XacNhanOTPVaChuyenTienAsync(int maGiaoDich, string maOTP);
        Task<PagedResultDTO<GiaoDichDTO>> LayLichSuGiaoDichAsync(int maNguoiDung, int pageSize = 20, int pageNumber = 1);
    }
}