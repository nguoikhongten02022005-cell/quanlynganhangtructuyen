using DAL;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly NganHangDAL _db;

        public TransactionService(NganHangDAL db)
        {
            _db = db;
        }

        public async Task<object> TraCuuTaiKhoanNhanAsync(string soTaiKhoan)
        {
            var taiKhoan = await _db.TaiKhoan
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SoTaiKhoan == soTaiKhoan);

            if (taiKhoan == null)
            {
                throw new Exception("Số tài khoản không tồn tại.");
            }

            if (taiKhoan.TrangThai != "ACTIVE")
            {
                throw new Exception("Tài khoản nhận đã bị khóa.");
            }

            var khachHang = await _db.KhachHang
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaKhachHang == taiKhoan.MaKhachHang);

            if (khachHang == null)
            {
                throw new Exception("Không tìm thấy thông tin chủ tài khoản nhận.");
            }

            return new
            {
                soTaiKhoan = taiKhoan.SoTaiKhoan,
                hoTen = khachHang.HoTen,
                trangThai = taiKhoan.TrangThai
            };
        }
    }
}