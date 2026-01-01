using DAL;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Requests;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Services
{
    public class KhachHangService : IKhachHangService
    {
        private readonly NganHangDAL _db;

        public KhachHangService(NganHangDAL db)
        {
            _db = db;
        }

        public async Task GuiYeuCauKycAsync(int maNguoiDung, string soCCCD)
        {
            var khachHang = await _db.KhachHang.FirstOrDefaultAsync(x => x.MaNguoiDung == maNguoiDung);

            if (khachHang == null)
            {
                throw new Exception("Không tìm thấy hồ sơ khách hàng.");
            }

            khachHang.SoCCCD = soCCCD;
            khachHang.TrangThaiKYC = "PENDING";

            await _db.SaveChangesAsync();
        }

        public async Task<object> LayDanhSachChoDuyetAsync()
        {
            var danhSach = await _db.KhachHang
                .AsNoTracking()
                .Where(k => k.TrangThaiKYC == "PENDING")
                .Select(k => new
                {
                    maKhachHang = k.MaKhachHang,
                    hoTen = k.HoTen,
                    email = k.Email,
                    soDienThoai = k.SoDienThoai,
                    soCCCD = k.SoCCCD,
                    trangThai = k.TrangThaiKYC
                })
                .ToListAsync();

            return new { tongSo = danhSach.Count, danhSach = danhSach };
        }

        public async Task DuyetYeuCauKycAsync(int maKhachHang, string trangThaiMoi, string? lyDo)
        {
            trangThaiMoi = (trangThaiMoi ?? "").Trim().ToUpperInvariant();

            if (trangThaiMoi != "ACTIVE" && trangThaiMoi != "REJECT")
            {
                throw new Exception("Trạng thái chỉ nhận ACTIVE hoặc REJECT.");
            }

            var khachHang = await _db.KhachHang.FirstOrDefaultAsync(x => x.MaKhachHang == maKhachHang);
            if (khachHang == null)
            {
                throw new Exception("Không tìm thấy khách hàng.");
            }

            if (khachHang.TrangThaiKYC != "PENDING")
            {
                throw new Exception($"Không thể duyệt vì trạng thái hiện tại là {khachHang.TrangThaiKYC}.");
            }

            if (trangThaiMoi == "ACTIVE" && string.IsNullOrWhiteSpace(khachHang.SoCCCD))
            {
                throw new Exception("Khách chưa nộp CCCD, không thể duyệt.");
            }

            khachHang.TrangThaiKYC = trangThaiMoi;
            await _db.SaveChangesAsync();
        }

        public async Task<KhachHang> TraCuuTheoSoTaiKhoanAsync(string soTaiKhoan)
        {
            var taiKhoan = await _db.Set<TaiKhoan>()
                .FirstOrDefaultAsync(tk => tk.SoTaiKhoan == soTaiKhoan);

            if (taiKhoan == null)
            {
                return null;
            }

            var khachHang = await _db.KhachHang
                .FirstOrDefaultAsync(kh => kh.MaKhachHang == taiKhoan.MaKhachHang);

            return khachHang;
        }

        public async Task<object> LayThongTinHoSoAsync(int maNguoiDung)
        {
            var khachHang = await _db.KhachHang
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaNguoiDung == maNguoiDung);

            if (khachHang == null)
            {
                throw new Exception("Không tìm thấy hồ sơ khách hàng.");
            }

            return new
            {
                hoTen = khachHang.HoTen,
                email = khachHang.Email,
                soDienThoai = khachHang.SoDienThoai,
                soCCCD = khachHang.SoCCCD,
                trangThaiKYC = khachHang.TrangThaiKYC,
                ngayTao = khachHang.NgayTao
            };
        }
    }
}
