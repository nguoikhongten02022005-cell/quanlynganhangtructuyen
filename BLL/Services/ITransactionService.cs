using System.Threading.Tasks;

namespace BLL.Services
{
    public interface ITransactionService
    {
        Task<object> TraCuuTaiKhoanNhanAsync(string soTaiKhoan);
        Task<object> TaoGiaoDichVoiOTPAsync(int maNguoiDung, string soTaiKhoanNhan, decimal soTien, string noiDung);
    }
}