using DAL;
using Microsoft.Data.SqlClient;
using Model.DTOs;
using System;
using System.Collections.Generic;
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

        public async Task<TraCuuNguoiNhanDTO> TraCuuTaiKhoanNhanAsync(string soTaiKhoan)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            var cmd = new SqlCommand(@"
                SELECT tk.SoTaiKhoan, tk.TrangThai, kh.HoTen
                FROM TaiKhoan tk
                INNER JOIN KhachHang kh ON tk.MaKhachHang = kh.MaKhachHang
                WHERE tk.SoTaiKhoan = @SoTaiKhoan", conn);
            cmd.Parameters.AddWithValue("@SoTaiKhoan", soTaiKhoan);
            
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                throw new Exception("Số tài khoản không tồn tại.");
            }

            var trangThai = reader.GetString(1);
            if (trangThai != "ACTIVE")
            {
                throw new Exception("Tài khoản nhận đã bị khóa.");
            }

            return new TraCuuNguoiNhanDTO
            {
                SoTaiKhoan = reader.GetString(0),
                TrangThai = trangThai,
                TenKhachHang = reader.GetString(2)
            };
        }

        public async Task<TaoGiaoDichResponseDTO> TaoGiaoDichVoiOTPAsync(int maNguoiDung, string soTaiKhoanNhan, decimal soTien, string noiDung)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            await using var transaction = conn.BeginTransaction();

            try
            {
                // Bước 1 & 2: Tìm tài khoản gửi
                var cmdTKGui = new SqlCommand(@"
                    SELECT tk.MaTaiKhoan, tk.SoDu, tk.TrangThai
                    FROM TaiKhoan tk
                    INNER JOIN KhachHang kh ON tk.MaKhachHang = kh.MaKhachHang
                    WHERE kh.MaNguoiDung = @MaNguoiDung", conn, transaction);
                cmdTKGui.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
                
                await using var readerGui = await cmdTKGui.ExecuteReaderAsync();
                if (!await readerGui.ReadAsync())
                {
                    throw new Exception("Không tìm thấy tài khoản gửi.");
                }

                var maTaiKhoanGui = readerGui.GetInt32(0);
                var soDuGui = readerGui.GetDecimal(1);
                var trangThaiGui = readerGui.GetString(2);
                await readerGui.CloseAsync();

                if (trangThaiGui != "ACTIVE")
                {
                    throw new Exception("Tài khoản của bạn đã bị khóa.");
                }

                if (soDuGui < soTien)
                {
                    throw new Exception($"Số dư không đủ. Số dư hiện tại: {soDuGui:N0} VNĐ.");
                }

                if (soTien <= 0)
                {
                    throw new Exception("Số tiền phải lớn hơn 0.");
                }

                // Bước 4: Tìm tài khoản nhận
                var cmdTKNhan = new SqlCommand("SELECT MaTaiKhoan, TrangThai FROM TaiKhoan WHERE SoTaiKhoan = @SoTaiKhoan", conn, transaction);
                cmdTKNhan.Parameters.AddWithValue("@SoTaiKhoan", soTaiKhoanNhan);
                
                await using var readerNhan = await cmdTKNhan.ExecuteReaderAsync();
                if (!await readerNhan.ReadAsync())
                {
                    throw new Exception("Số tài khoản nhận không tồn tại.");
                }

                var maTaiKhoanNhan = readerNhan.GetInt32(0);
                var trangThaiNhan = readerNhan.GetString(1);
                await readerNhan.CloseAsync();

                if (trangThaiNhan != "ACTIVE")
                {
                    throw new Exception("Tài khoản nhận đã bị khóa.");
                }

                if (maTaiKhoanGui == maTaiKhoanNhan)
                {
                    throw new Exception("Không thể chuyển tiền cho chính mình.");
                }

                // Bước 5 & 6: Sinh OTP và thời hạn
                string maOTP = new Random().Next(100000, 999999).ToString();
                DateTime thoiHanOTP = DateTime.Now.AddMinutes(5);

                // Bước 7 & 8: Tạo giao dịch
                var cmdInsert = new SqlCommand(@"
                    INSERT INTO GiaoDich (MaTaiKhoanGui, MaTaiKhoanNhan, SoTien, NoiDung, NgayGiaoDich, TrangThai, MaOTP, ThoiHanOTP)
                    OUTPUT INSERTED.MaGiaoDich
                    VALUES (@MaTaiKhoanGui, @MaTaiKhoanNhan, @SoTien, @NoiDung, @NgayGiaoDich, @TrangThai, @MaOTP, @ThoiHanOTP)", conn, transaction);
                
                cmdInsert.Parameters.AddWithValue("@MaTaiKhoanGui", maTaiKhoanGui);
                cmdInsert.Parameters.AddWithValue("@MaTaiKhoanNhan", maTaiKhoanNhan);
                cmdInsert.Parameters.AddWithValue("@SoTien", soTien);
                cmdInsert.Parameters.AddWithValue("@NoiDung", noiDung ?? (object)DBNull.Value);
                cmdInsert.Parameters.AddWithValue("@NgayGiaoDich", DateTime.Now);
                cmdInsert.Parameters.AddWithValue("@TrangThai", "PENDING");
                cmdInsert.Parameters.AddWithValue("@MaOTP", maOTP);
                cmdInsert.Parameters.AddWithValue("@ThoiHanOTP", thoiHanOTP);

                var giaoDichId = (int)await cmdInsert.ExecuteScalarAsync();

                await transaction.CommitAsync();

                return new TaoGiaoDichResponseDTO
                {
                    GiaoDichId = giaoDichId,
                    Message = "Tạo giao dịch thành công. Mã OTP đã được gửi."
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<object> XacNhanOTPVaChuyenTienAsync(int maGiaoDich, string maOTP)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            await using var transaction = conn.BeginTransaction();

            try
            {
                // Bước 1: Lấy giao dịch
                var cmdGet = new SqlCommand("SELECT MaTaiKhoanGui, MaTaiKhoanNhan, SoTien, NoiDung, TrangThai, MaOTP, ThoiHanOTP FROM GiaoDich WHERE MaGiaoDich = @MaGiaoDich", conn, transaction);
                cmdGet.Parameters.AddWithValue("@MaGiaoDich", maGiaoDich);
                
                await using var reader = await cmdGet.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    throw new Exception("Giao dịch không tồn tại!");
                }

                var trangThai = reader.GetString(4);
                if (trangThai != "PENDING")
                {
                    throw new Exception($"Giao dịch đã được xử lý với trạng thái: {trangThai}");
                }

                var maOTPDb = reader.GetString(5);
                if (maOTPDb != maOTP)
                {
                    throw new Exception("Mã OTP không đúng!");
                }

                var thoiHanOTP = reader.GetDateTime(6);
                if (DateTime.Now > thoiHanOTP)
                {
                    await reader.CloseAsync();
                    var cmdFail = new SqlCommand("UPDATE GiaoDich SET TrangThai = 'FAILED' WHERE MaGiaoDich = @MaGiaoDich", conn, transaction);
                    cmdFail.Parameters.AddWithValue("@MaGiaoDich", maGiaoDich);
                    await cmdFail.ExecuteNonQueryAsync();
                    await transaction.CommitAsync();
                    throw new Exception("Mã OTP đã hết hạn!");
                }

                var maTKGui = reader.GetInt32(0);
                var maTKNhan = reader.GetInt32(1);
                var soTien = reader.GetDecimal(2);
                var noiDung = reader.IsDBNull(3) ? "" : reader.GetString(3);
                await reader.CloseAsync();

                // Bước 4: Gọi SP chuyển tiền
                var cmdSP = new SqlCommand("EXEC SP_ChuyenTien @MaTaiKhoanGui, @MaTaiKhoanNhan, @SoTien, @NoiDung", conn, transaction);
                cmdSP.Parameters.AddWithValue("@MaTaiKhoanGui", maTKGui);
                cmdSP.Parameters.AddWithValue("@MaTaiKhoanNhan", maTKNhan);
                cmdSP.Parameters.AddWithValue("@SoTien", soTien);
                cmdSP.Parameters.AddWithValue("@NoiDung", noiDung);
                
                await using var resultReader = await cmdSP.ExecuteReaderAsync();
                await resultReader.ReadAsync();
                var result = resultReader.GetString(0);
                await resultReader.CloseAsync();

                if (result != "SUCCESS")
                {
                    throw new Exception("Chuyển tiền thất bại từ stored procedure.");
                }

                // Bước 5: Cập nhật trạng thái
                var cmdUpdate = new SqlCommand("UPDATE GiaoDich SET TrangThai = 'SUCCESS', NgayGiaoDich = @NgayGiaoDich WHERE MaGiaoDich = @MaGiaoDich", conn, transaction);
                cmdUpdate.Parameters.AddWithValue("@NgayGiaoDich", DateTime.Now);
                cmdUpdate.Parameters.AddWithValue("@MaGiaoDich", maGiaoDich);
                await cmdUpdate.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                return new
                {
                    ketQua = "SUCCESS",
                    thongBao = "Chuyển tiền thành công!",
                    maGiaoDich = maGiaoDich,
                    soTien = soTien,
                    ngayGiaoDich = DateTime.Now
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                
                // Cập nhật failed
                await using var conn2 = await _db.GetOpenConnectionAsync();
                var cmdFail2 = new SqlCommand("UPDATE GiaoDich SET TrangThai = 'FAILED' WHERE MaGiaoDich = @MaGiaoDich", conn2);
                cmdFail2.Parameters.AddWithValue("@MaGiaoDich", maGiaoDich);
                await cmdFail2.ExecuteNonQueryAsync();
                
                throw;
            }
        }

        public async Task<PagedResultDTO<GiaoDichDTO>> LayLichSuGiaoDichAsync(int maNguoiDung, int pageSize = 20, int pageNumber = 1)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            
            // Lấy MaTaiKhoan
            var cmdTK = new SqlCommand(@"
                SELECT tk.MaTaiKhoan
                FROM TaiKhoan tk
                INNER JOIN KhachHang kh ON tk.MaKhachHang = kh.MaKhachHang
                WHERE kh.MaNguoiDung = @MaNguoiDung", conn);
            cmdTK.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
            
            var maTaiKhoan = await cmdTK.ExecuteScalarAsync();
            if (maTaiKhoan == null)
            {
                throw new Exception("Bạn chưa có tài khoản ngân hàng.");
            }

            // Đếm tổng số
            var cmdCount = new SqlCommand(@"
                SELECT COUNT(*)
                FROM GiaoDich
                WHERE (MaTaiKhoanGui = @MaTaiKhoan OR MaTaiKhoanNhan = @MaTaiKhoan)
                AND TrangThai = 'SUCCESS'", conn);
            cmdCount.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
            var tongSo = (int)await cmdCount.ExecuteScalarAsync();

            // Lấy danh sách
            var cmdList = new SqlCommand(@"
                SELECT 
                    gd.MaGiaoDich,
                    gd.SoTien,
                    gd.NoiDung,
                    gd.NgayGiaoDich,
                    gd.TrangThai,
                    tkGui.SoTaiKhoan AS SoTaiKhoanGui,
                    tkNhan.SoTaiKhoan AS SoTaiKhoanNhan
                FROM GiaoDich gd
                INNER JOIN TaiKhoan tkGui ON gd.MaTaiKhoanGui = tkGui.MaTaiKhoan
                INNER JOIN TaiKhoan tkNhan ON gd.MaTaiKhoanNhan = tkNhan.MaTaiKhoan
                WHERE (gd.MaTaiKhoanGui = @MaTaiKhoan OR gd.MaTaiKhoanNhan = @MaTaiKhoan)
                AND gd.TrangThai = 'SUCCESS'
                ORDER BY gd.NgayGiaoDich DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", conn);
            
            cmdList.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
            cmdList.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
            cmdList.Parameters.AddWithValue("@PageSize", pageSize);

            var danhSach = new List<GiaoDichDTO>();
            await using var reader = await cmdList.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                danhSach.Add(new GiaoDichDTO
                {
                    MaGiaoDich = reader.GetInt32(0),
                    SoTien = reader.GetDecimal(1),
                    NoiDung = reader.IsDBNull(2) ? null : reader.GetString(2),
                    NgayGiaoDich = reader.GetDateTime(3),
                    TrangThai = reader.GetString(4),
                    SoTaiKhoanGui = reader.GetString(5),
                    SoTaiKhoanNhan = reader.GetString(6)
                });
            }

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