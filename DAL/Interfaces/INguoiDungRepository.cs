using Model.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces;

public interface INguoiDungRepository
{
    Task<NguoiDungDTO?> GetNguoiDungByTenDangNhapAsync(string tenDangNhap);
    Task<bool> KiemTraTonTaiTenDangNhapAsync(string tenDangNhap);
    Task<NguoiDungDTO?> GetNguoiDungByIdAsync(int maNguoiDung);
    Task<string?> GetMatKhauHashByIdAsync(int maNguoiDung);
    Task<int> ThemNguoiDungAsync(string tenDangNhap, string matKhauHash, string vaiTro, string trangThai);
    Task<int> CapNhatMatKhauAsync(int maNguoiDung, string matKhauHash);
    Task<int> CapNhatTrangThaiNguoiDungAsync(int maNguoiDung, string trangThai);
    Task<List<NguoiDungDTO>> GetDanhSachNguoiDungAsync(string? role, string? status);
    Task<NguoiDungDTO?> LayChiTietNguoiDungAsync(int maNguoiDung);
    Task<int> GetTongNguoiDungAsync();
    Task<int> GetTongKhachHangAsync();
    Task<int> GetSoKYCPendingAsync();
    Task<int> GetTongGiaoDichThanhCongAsync();
    Task<decimal> GetTongSoTienGiaoDichAsync();
}