using System.Threading.Tasks;

namespace BLL.Services
{
    public interface ITransactionService
    {
        Task<object> TraCuuTaiKhoanNhanAsync(string soTaiKhoan);
    }
}