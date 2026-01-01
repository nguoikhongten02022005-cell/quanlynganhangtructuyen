using Model;
using Model.Requests;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BLL.Services
{
    // Interface định nghĩa các hành động liên quan đến Khách Hàng
    public interface IKhachHangService
    {
        // 1. Chức năng cho Khách hàng
        Task GuiYeuCauKycAsync(int maNguoiDung, string soCCCD);
        Task<object> LayThongTinHoSoAsync(int maNguoiDung);

        // 2. Chức năng cho Admin/Nhân viên
        Task<object> LayDanhSachChoDuyetAsync();
        Task DuyetYeuCauKycAsync(int maKhachHang, string trangThaiMoi, string? lyDo);

        // 3. Chức năng tra cứu thông tin khách hàng
        Task<KhachHang> TraCuuTheoSoTaiKhoanAsync(string soTaiKhoan);
    }
}
