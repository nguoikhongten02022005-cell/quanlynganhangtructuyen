using Model.DTOs;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface ITransactionService
    {
        Task<TraCuuNguoiNhanDTO> TraCuuTaiKhoanNhanAsync(string soTaiKhoan);
        Task<TaoGiaoDichResponseDTO> TaoGiaoDichVoiOTPAsync(int maNguoiDung, string soTaiKhoanNhan, decimal soTien, string noiDung);
        Task<object> XacNhanOTPVaChuyenTienAsync(int maNguoiDung, int maGiaoDich, string maOTP);
        Task<PagedResultDTO<GiaoDichDTO>> LayLichSuGiaoDichAsync(int maNguoiDung, int pageSize = 20, int pageNumber = 1);
        Task<GiaoDichDTO> LayChiTietGiaoDichAsync(int maNguoiDung, int maGiaoDich);
        Task<PagedResultDTO<GiaoDichDTO>> LayGiaoDichNhanDuocAsync(int maNguoiDung, int pageSize = 20, int pageNumber = 1);
    }
}