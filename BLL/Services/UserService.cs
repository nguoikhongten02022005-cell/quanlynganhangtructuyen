using DAL;
using Microsoft.Data.SqlClient;
using Model.DTOs;
using System;
using System.Threading.Tasks;
using BCrypt.Net;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly NguoiDungDAL _nguoiDungDAL;

        public UserService(NguoiDungDAL nguoiDungDAL)
        {
            _nguoiDungDAL = nguoiDungDAL;
        }

        public async Task<object> GetUsersAsync(string? role, string? status)
        {
            var danhSach = await _nguoiDungDAL.GetDanhSachNguoiDungAsync(role, status);
            return new { tongSo = danhSach.Count, danhSach = danhSach };
        }

        public async Task<NguoiDungDTO> LayChiTietNguoiDungAsync(int maNguoiDung)
        {
            var result = await _nguoiDungDAL.LayChiTietNguoiDungAsync(maNguoiDung);
            if (result == null)
            {
                throw new Exception("Không tìm thấy người dùng.");
            }
            return result;
        }

        public async Task<AdminDashboardDTO> LayThongKeDashboardAsync()
        {
            var dashboard = new AdminDashboardDTO
            {
                TongNguoiDung = await _nguoiDungDAL.GetTongNguoiDungAsync(),
                TongKhachHang = await _nguoiDungDAL.GetTongKhachHangAsync(),
                SoKYCChoDuyet = await _nguoiDungDAL.GetSoKYCPendingAsync(),
                TongGiaoDich = await _nguoiDungDAL.GetTongGiaoDichThanhCongAsync(),
                TongSoTienGiaoDich = await _nguoiDungDAL.GetTongSoTienGiaoDichAsync()
            };
            return dashboard;
        }

        public async Task<NguoiDungDTO> TaoNguoiDungHeThongAsync(string tenDangNhap, string matKhau, string vaiTro)
        {
            if (await _nguoiDungDAL.KiemTraTonTaiTenDangNhapAsync(tenDangNhap))
            {
                throw new Exception("Tên đăng nhập đã tồn tại.");
            }

            var matKhauHash = BCrypt.Net.BCrypt.HashPassword(matKhau);
            var maNguoiDung = await _nguoiDungDAL.ThemNguoiDungAsync(tenDangNhap, matKhauHash, vaiTro, "ACTIVE");

            return new NguoiDungDTO
            {
                MaNguoiDung = maNguoiDung,
                TenDangNhap = tenDangNhap,
                VaiTro = vaiTro,
                TrangThai = "ACTIVE",
                NgayTao = DateTime.Now
            };
        }

        public async Task KhoaMoKhoaTaiKhoanAsync(int maNguoiDung, bool khoa)
        {
            var nguoiDung = await _nguoiDungDAL.GetNguoiDungByIdAsync(maNguoiDung);
            if (nguoiDung == null)
            {
                throw new Exception("Người dùng không tồn tại.");
            }

            var trangThaiMoi = khoa ? "LOCKED" : "ACTIVE";
            await _nguoiDungDAL.CapNhatTrangThaiNguoiDungAsync(maNguoiDung, trangThaiMoi);
        }

        public async Task ResetMatKhauAsync(int maNguoiDung, string matKhauMoi)
        {
            var nguoiDung = await _nguoiDungDAL.GetNguoiDungByIdAsync(maNguoiDung);
            if (nguoiDung == null)
            {
                throw new Exception("Người dùng không tồn tại.");
            }

            var matKhauHash = BCrypt.Net.BCrypt.HashPassword(matKhauMoi);
            await _nguoiDungDAL.CapNhatMatKhauAsync(maNguoiDung, matKhauHash);
        }
    }
}
