using Model.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces;

public interface IKhachHangRepository
{
    Task<int> ThemKhachHangAsync(int maNguoiDung, string hoTen, string? email, string? soDienThoai, string trangThaiKYC);
    Task<KhachHangProfileDTO?> GetKhachHangByMaNguoiDungAsync(int maNguoiDung);
    Task<bool> KiemTraCCCDDaSuDungAsync(string soCCCD, int maNguoiDung);
    Task<int> CapNhatCCCDVaKYCAsync(int maNguoiDung, string soCCCD, string trangThaiKYC);
    Task<List<KYCPendingDTO>> GetDanhSachKYCPendingAsync();
    Task<(int MaKhachHang, string TrangThaiKYC)?> GetKhachHangInfoByMaNguoiDungAsync(int maNguoiDung);
    Task<int> CapNhatTrangThaiKYCAsync(int maNguoiDung, string trangThaiKYC);
    Task<bool> KiemTraEmailDaSuDungAsync(string email, int maNguoiDung);
    Task<bool> KiemTraSoDienThoaiDaSuDungAsync(string soDienThoai, int maNguoiDung);
    Task<int> CapNhatThongTinKhachHangAsync(int maNguoiDung, string? hoTen, string? email, string? soDienThoai);
}