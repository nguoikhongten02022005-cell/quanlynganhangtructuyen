using DAL;
using Model.DTOs;
using Model.Requests;
using System;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly TaiKhoanDAL _taiKhoanDAL;
        private readonly GiaoDichDAL _giaoDichDAL;

        public TransactionService(TaiKhoanDAL taiKhoanDAL, GiaoDichDAL giaoDichDAL)
        {
            _taiKhoanDAL = taiKhoanDAL;
            _giaoDichDAL = giaoDichDAL;
        }

        public async Task<TraCuuNguoiNhanDTO> TraCuuTaiKhoanNhanAsync(string soTaiKhoan)
        {
            var result = await _taiKhoanDAL.GetTaiKhoanNhanBySoTaiKhoanAsync(soTaiKhoan);
            if (result == null)
            {
                throw new Exception("Số tài khoản không tồn tại.");
            }

            if (result.TrangThai != "ACTIVE")
            {
                throw new Exception("Tài khoản nhận đã bị khóa.");
            }

            return result;
        }

        public async Task<TaoGiaoDichResponseDTO> TaoGiaoDichVoiOTPAsync(int maNguoiDung, string soTaiKhoanNhan, decimal soTien, string noiDung)
        {
            if (soTien <= 0)
            {
                throw new Exception("Số tiền phải lớn hơn 0.");
            }

            var taiKhoanGuiInfo = await _taiKhoanDAL.GetTaiKhoanInfoByMaNguoiDungAsync(maNguoiDung);
            if (taiKhoanGuiInfo == null)
            {
                throw new Exception("Không tìm thấy tài khoản gửi.");
            }

            var (maTaiKhoanGui, soDuGui, trangThaiGui) = taiKhoanGuiInfo.Value;

            if (trangThaiGui != "ACTIVE")
            {
                throw new Exception("Tài khoản của bạn đã bị khóa.");
            }

            if (soDuGui < soTien)
            {
                throw new Exception($"Số dư không đủ. Số dư hiện tại: {soDuGui:N0} VNĐ.");
            }

            var maTaiKhoanNhan = await _taiKhoanDAL.GetMaTaiKhoanBySoTaiKhoanAsync(soTaiKhoanNhan);
            if (maTaiKhoanNhan == null)
            {
                throw new Exception("Số tài khoản nhận không tồn tại.");
            }

            if (maTaiKhoanGui == maTaiKhoanNhan.Value)
            {
                throw new Exception("Không thể chuyển tiền cho chính mình.");
            }

            string maOTP = new Random().Next(100000, 999999).ToString();
            DateTime thoiHanOTP = DateTime.Now.AddMinutes(5);

            var giaoDichId = await _giaoDichDAL.ThemGiaoDichAsync(
                maTaiKhoanGui,
                maTaiKhoanNhan.Value,
                soTien,
                noiDung,
                "PENDING",
                maOTP,
                thoiHanOTP
            );

            return new TaoGiaoDichResponseDTO
            {
                GiaoDichId = giaoDichId,
                Message = "Tạo giao dịch thành công. Mã OTP đã được gửi."
            };
        }

        public async Task<object> XacNhanOTPVaChuyenTienAsync(int maGiaoDich, string maOTP)
        {
            var giaoDichInfo = await _giaoDichDAL.GetGiaoDichByIdAsync(maGiaoDich);
            if (giaoDichInfo == null)
            {
                throw new Exception("Giao dịch không tồn tại!");
            }

            var (maTaiKhoanGui, maTaiKhoanNhan, soTien, noiDung, trangThai, maOTPDAL, thoiHanOTP) = giaoDichInfo.Value;

            if (trangThai != "PENDING")
            {
                throw new Exception($"Giao dịch đã được xử lý với trạng thái: {trangThai}");
            }

            if (maOTPDAL != maOTP)
            {
                throw new Exception("Mã OTP không đúng!");
            }

            if (DateTime.Now > thoiHanOTP)
            {
                await _giaoDichDAL.CapNhatTrangThaiGiaoDichAsync(maGiaoDich, "FAILED");
                throw new Exception("Mã OTP đã hết hạn!");
            }

            var ketQua = await _giaoDichDAL.ChuyenTienAsync(maTaiKhoanGui, maTaiKhoanNhan, soTien, noiDung);
            if (ketQua != "SUCCESS")
            {
                throw new Exception("Chuyển tiền thất bại.");
            }

            await _giaoDichDAL.CapNhatTrangThaiGiaoDichAsync(maGiaoDich, "SUCCESS", DateTime.Now);

            return new
            {
                ketQua = "SUCCESS",
                thongBao = "Chuyển tiền thành công!",
                maGiaoDich = maGiaoDich,
                soTien = soTien,
                ngayGiaoDich = DateTime.Now
            };
        }

        public async Task<PagedResultDTO<GiaoDichDTO>> LayLichSuGiaoDichAsync(int maNguoiDung, int pageSize = 20, int pageNumber = 1)
        {
            var maTaiKhoan = await _taiKhoanDAL.GetMaTaiKhoanByMaNguoiDungAsync(maNguoiDung);
            if (maTaiKhoan == null)
            {
                throw new Exception("Bạn chưa có tài khoản ngân hàng.");
            }

            var tongSo = await _giaoDichDAL.DemGiaoDichByMaTaiKhoanAsync(maTaiKhoan.Value);

            var danhSach = await _giaoDichDAL.GetLichSuGiaoDichAsync(maTaiKhoan.Value, pageSize, pageNumber);

            return new PagedResultDTO<GiaoDichDTO>
            {
                Items = danhSach,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = tongSo,
                TotalPages = (int)Math.Ceiling((double)tongSo / pageSize)
            };
        }

        public async Task<GiaoDichDTO> LayChiTietGiaoDichAsync(int maNguoiDung, int maGiaoDich)
        {
            var soTaiKhoan = await _taiKhoanDAL.GetSoTaiKhoanByMaNguoiDungAsync(maNguoiDung);
            if (soTaiKhoan == null)
            {
                throw new Exception("Không tìm thấy tài khoản ngân hàng.");
            }

            var result = await _giaoDichDAL.GetGiaoDichByIdAsync(maGiaoDich, soTaiKhoan);
            if (result == null)
            {
                throw new Exception("Không tìm thấy giao dịch hoặc bạn không có quyền xem giao dịch này.");
            }

            return result;
        }

        public async Task<PagedResultDTO<GiaoDichDTO>> LayGiaoDichNhanDuocAsync(int maNguoiDung, int pageSize = 20, int pageNumber = 1)
        {
            var soTaiKhoan = await _taiKhoanDAL.GetSoTaiKhoanByMaNguoiDungAsync(maNguoiDung);
            if (soTaiKhoan == null)
            {
                throw new Exception("Không tìm thấy tài khoản ngân hàng.");
            }

            var tongSo = await _giaoDichDAL.DemGiaoDichNhanDuocAsync(soTaiKhoan);

            var danhSach = await _giaoDichDAL.GetGiaoDichNhanDuocAsync(soTaiKhoan, pageSize, pageNumber);

            return new PagedResultDTO<GiaoDichDTO>
            {
                Items = danhSach,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = tongSo,
                TotalPages = (int)Math.Ceiling((double)tongSo / pageSize)
            };
        }
    }
}
