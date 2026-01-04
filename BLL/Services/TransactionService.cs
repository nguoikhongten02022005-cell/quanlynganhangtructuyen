using DAL;
using Microsoft.EntityFrameworkCore;
using Model.Entities;
using System;
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

        /// <summary>
        /// Khởi tạo giao dịch chuyển tiền với mã OTP
        /// </summary>
        public async Task<object> TaoGiaoDichVoiOTPAsync(int maNguoiDung, string soTaiKhoanNhan, decimal soTien, string noiDung)
        {
            // Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // Bước 1: Tìm khách hàng từ maNguoiDung
                var khachHang = await _db.KhachHang
                    .FirstOrDefaultAsync(x => x.MaNguoiDung == maNguoiDung);

                if (khachHang == null)
                {
                    throw new Exception("Không tìm thấy thông tin khách hàng.");
                }

                // Bước 2: Tìm tài khoản gửi từ mã khách hàng
                var taiKhoanGui = await _db.TaiKhoan
                    .FirstOrDefaultAsync(x => x.MaKhachHang == khachHang.MaKhachHang);

                if (taiKhoanGui == null)
                {
                    throw new Exception("Không tìm thấy tài khoản gửi.");
                }

                if (taiKhoanGui.TrangThai != "ACTIVE")
                {
                    throw new Exception("Tài khoản của bạn đã bị khóa.");
                }

                // Bước 3: Validate số dư đủ không
                if (taiKhoanGui.SoDu < soTien)
                {
                    throw new Exception($"Số dư không đủ. Số dư hiện tại: {taiKhoanGui.SoDu:N0} VNĐ.");
                }

                // Validate số tiền
                if (soTien <= 0)
                {
                    throw new Exception("Số tiền phải lớn hơn 0.");
                }

                // Bước 4: Tìm tài khoản nhận từ số tài khoản
                var taiKhoanNhan = await _db.TaiKhoan
                    .FirstOrDefaultAsync(x => x.SoTaiKhoan == soTaiKhoanNhan);

                if (taiKhoanNhan == null)
                {
                    throw new Exception("Số tài khoản nhận không tồn tại.");
                }

                if (taiKhoanNhan.TrangThai != "ACTIVE")
                {
                    throw new Exception("Tài khoản nhận đã bị khóa.");
                }

                // Validate không tự chuyển cho chính mình
                if (taiKhoanGui.MaTaiKhoan == taiKhoanNhan.MaTaiKhoan)
                {
                    throw new Exception("Không thể chuyển tiền cho chính mình.");
                }

                // Bước 5: Sinh OTP 6 số ngẫu nhiên
                var random = new Random();
                string maOTP = random.Next(100000, 999999).ToString();

                // Bước 6: Set thời hạn OTP = 5 phút
                DateTime thoiHanOTP = DateTime.Now.AddMinutes(5);

                // Bước 7: Tạo giao dịch với trạng thái PENDING
                var giaoDich = new GiaoDich
                {
                    MaTaiKhoanGui = taiKhoanGui.MaTaiKhoan,
                    MaTaiKhoanNhan = taiKhoanNhan.MaTaiKhoan,
                    SoTien = soTien,
                    NoiDung = noiDung,
                    NgayGiaoDich = DateTime.Now,
                    TrangThai = "PENDING",
                    MaOTP = maOTP,
                    ThoiHanOTP = thoiHanOTP
                };

                // Bước 8: Lưu vào database
                _db.GiaoDich.Add(giaoDich);
                await _db.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                // Bước 9: Return thông tin giao dịch + OTP
                return new
                {
                    transactionId = giaoDich.MaGiaoDich,
                    otpCode = maOTP,
                    expiredAt = thoiHanOTP.ToString("yyyy-MM-dd HH:mm:ss")
                };
            }
            catch (Exception)
            {
                // Rollback nếu có lỗi
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}