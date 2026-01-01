using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IKhachHangService
    {
        Task<object> LayThongTinHoSoAsync(int maNguoiDung);
    }
}
