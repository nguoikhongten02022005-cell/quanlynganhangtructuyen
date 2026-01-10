using DAL;
using Model.Requests;
using Model.DTOs;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BLL.Services
{
    public class KhachHangService : IKhachHangService
    {
        private readonly KhachHangDAL _khachHangDAL;
        private readonly TaiKhoanDAL _taiKhoanDAL;

        public KhachHangService(KhachHangDAL khachHangDAL, TaiKhoanDAL taiKhoanDAL)
        {
            _khachHangDAL = khachHangDAL;
            _taiKhoanDAL = taiKhoanDAL;
        }

        public async Task<KhachHangProfileDTO> LayThongTinHoSoAsync(int maNguoiDung)
        {
            var result = await _khachHangDAL.GetKhachHangByMaNguoiDungAsync(maNguoiDung);
            if (result == null)
            {
                throw new Exception("Không tìm thấy hồ sơ khách hàng.");
            }
            return result;
        }

        public async Task<object> GuiYeuCauKycAsync(int maNguoiDung, KycRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SoCCCD) ||
                (request.SoCCCD.Length != 12 && request.SoCCCD.Length != 13))
            {
                throw new Exception("Số CCCD phải có 12 hoặc 13 chữ số.");
            }

            if (await _khachHangDAL.KiemTraCCCDDaSuDungAsync(request.SoCCCD, maNguoiDung))
            {
                throw new Exception("Số CCCD này đã được sử dụng bởi tài khoản khác.");
            }

            var rowsAffected = await _khachHangDAL.CapNhatCCCDVaKYCAsync(maNguoiDung, request.SoCCCD, "PENDING");
            if (rowsAffected == 0)
            {
                throw new Exception("Không tìm thấy hồ sơ khách hàng.");
            }

            return new
            {
                thongBao = "Gửi yêu cầu KYC thành công. Vui lòng chờ nhân viên ngân hàng duyệt.",
                soCCCD = request.SoCCCD,
                trangThaiKYC = "PENDING"
            };
        }

        public async Task<TaiKhoanDTO> LayThongTinTaiKhoanAsync(int maNguoiDung)
        {
            var result = await _taiKhoanDAL.GetTaiKhoanByMaNguoiDungAsync(maNguoiDung);
            if (result == null)
            {
                throw new Exception("Không tìm thấy tài khoản ngân hàng.");
            }

            if (result.TrangThai != "ACTIVE")
            {
                throw new Exception("Tài khoản ngân hàng đã bị khóa.");
            }

            return result;
        }

        public async Task<object> LayDanhSachKycPendingAsync()
        {
            var danhSach = await _khachHangDAL.GetDanhSachKYCPendingAsync();
            var result = new List<object>();
            foreach (var item in danhSach)
            {
                result.Add(new
                {
                    maKhachHang = item.MaKhachHang,
                    maNguoiDung = 0,
                    hoTen = item.HoTen,
                    email = item.Email,
                    soDienThoai = item.SoDienThoai,
                    soCCCD = item.SoCCCD,
                    trangThaiKYC = "PENDING"
                });
            }
            return new { danhSach = result, tongSo = result.Count };
        }

        public async Task<List<KYCPendingDTO>> LayDanhSachKYCChoDuyetAsync()
        {
            return await _khachHangDAL.GetDanhSachKYCPendingAsync();
        }

        public async Task<object> DuyetKYCAsync(int maKhachHang, string status, string? reason = null)
        {
            status = (status ?? string.Empty).Trim().ToUpperInvariant();

            if (status != "APPROVED" && status != "REJECTED")
            {
                throw new Exception("Trạng thái không hợp lệ. Chỉ chấp nhận APPROVED hoặc REJECTED.");
            }

            var khachHangInfo = await _khachHangDAL.GetKhachHangInfoByMaKhachHangAsync(maKhachHang);
            if (khachHangInfo == null)
            {
                throw new Exception("Không tìm thấy khách hàng.");
            }

            if (khachHangInfo.Value.TrangThaiKYC != "PENDING")
            {
                throw new Exception("Hồ sơ KYC không ở trạng thái chờ duyệt.");
            }

            await _khachHangDAL.CapNhatTrangThaiKYCAsync(maKhachHang, status);

            return new
            {
                thongBao = status == "APPROVED" ? "Duyệt KYC thành công." : "Từ chối KYC thành công.",
                maKhachHang = khachHangInfo.Value.MaKhachHang,
                trangThaiMoi = status,
                lyDo = reason
            };
        }

        public async Task<object> CapNhatThongTinCaNhanAsync(int maNguoiDung, UpdateProfileRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                if (await _khachHangDAL.KiemTraEmailDaSuDungAsync(request.Email, maNguoiDung))
                {
                    throw new Exception("Email này đã được sử dụng bởi tài khoản khác.");
                }
            }

            if (!string.IsNullOrWhiteSpace(request.SoDienThoai))
            {
                if (await _khachHangDAL.KiemTraSoDienThoaiDaSuDungAsync(request.SoDienThoai, maNguoiDung))
                {
                    throw new Exception("Số điện thoại này đã được sử dụng bởi tài khoản khác.");
                }
            }

            var rowsAffected = await _khachHangDAL.CapNhatThongTinKhachHangAsync(maNguoiDung, request.HoTen, request.Email, request.SoDienThoai);
            if (rowsAffected == 0)
            {
                throw new Exception("Không tìm thấy hồ sơ khách hàng.");
            }

            return new
            {
                thongBao = "Cập nhật thông tin cá nhân thành công.",
                hoTen = request.HoTen,
                email = request.Email,
                soDienThoai = request.SoDienThoai
            };
        }
    }
}
