using Model.DTOs;
using System.Threading.Tasks;

namespace DAL.Interfaces;

public interface ITaiKhoanRepository
{
    Task<int> ThemTaiKhoanAsync(int maKhachHang, string soTaiKhoan, decimal soDu, string trangThai);
    Task<TaiKhoanDTO?> GetTaiKhoanByMaNguoiDungAsync(int maNguoiDung);
    Task<int?> GetMaTaiKhoanByMaNguoiDungAsync(int maNguoiDung);
    Task<string?> GetSoTaiKhoanByMaNguoiDungAsync(int maNguoiDung);
    Task<bool> KiemTraSoTaiKhoanDaTonTaiAsync(string soTaiKhoan);
    Task<TraCuuNguoiNhanDTO?> GetTaiKhoanNhanBySoTaiKhoanAsync(string soTaiKhoan);
    Task<int?> GetMaTaiKhoanBySoTaiKhoanAsync(string soTaiKhoan);
    Task<(int MaTaiKhoan, decimal SoDu, string TrangThai)?> GetTaiKhoanInfoByMaNguoiDungAsync(int maNguoiDung);
}