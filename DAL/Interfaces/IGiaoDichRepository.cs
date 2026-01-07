using Model.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces;

public interface IGiaoDichRepository
{
    Task<int> ThemGiaoDichAsync(int maTaiKhoanGui, int maTaiKhoanNhan, decimal soTien, string? noiDung, string trangThai, string maOTP, DateTime thoiHanOTP);
    Task<(int MaTaiKhoanGui, int MaTaiKhoanNhan, decimal SoTien, string NoiDung, string TrangThai, string MaOTP, DateTime ThoiHanOTP)?> GetGiaoDichByIdAsync(int maGiaoDich);
    Task<int> CapNhatTrangThaiGiaoDichAsync(int maGiaoDich, string trangThai, DateTime? ngayGiaoDich = null);
    Task<int> DemGiaoDichByMaTaiKhoanAsync(int maTaiKhoan);
    Task<List<GiaoDichDTO>> GetLichSuGiaoDichAsync(int maTaiKhoan, int pageSize, int pageNumber);
    Task<GiaoDichDTO?> GetGiaoDichByIdAsync(int maGiaoDich, string soTaiKhoan);
    Task<int> DemGiaoDichNhanDuocAsync(string soTaiKhoan);
    Task<List<GiaoDichDTO>> GetGiaoDichNhanDuocAsync(string soTaiKhoan, int pageSize, int pageNumber);
    Task<string?> ChuyenTienAsync(int maTaiKhoanGui, int maTaiKhoanNhan, decimal soTien, string noiDung);
}