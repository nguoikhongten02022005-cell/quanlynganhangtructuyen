using DAL;
using Microsoft.Data.SqlClient;
using Model.Requests;
using Model.DTOs;
using System;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly NganHangDAL _db;

        public AuthService(NganHangDAL db)
        {
            _db = db;
        }

        public async Task<object> DangKyKhachHangAsync(DangKyRequest request)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            await using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Kiểm tra trùng tên đăng nhập
                var checkCmd = new SqlCommand("SELECT COUNT(*) FROM NguoiDung WHERE TenDangNhap = @TenDangNhap", conn, transaction);
                checkCmd.Parameters.AddWithValue("@TenDangNhap", request.TenDangNhap);
                var count = (int)await checkCmd.ExecuteScalarAsync();
                if (count > 0)
                {
                    throw new Exception("Tên đăng nhập đã tồn tại.");
                }

                // 2.1: Tạo User (NguoiDung)
                var insertUserCmd = new SqlCommand(@"
                    INSERT INTO NguoiDung (TenDangNhap, MatKhauHash, VaiTro, NgayTao, TrangThai)
                    OUTPUT INSERTED.MaNguoiDung
                    VALUES (@TenDangNhap, @MatKhauHash, @VaiTro, @NgayTao, @TrangThai)", conn, transaction);
                
                insertUserCmd.Parameters.AddWithValue("@TenDangNhap", request.TenDangNhap);
                insertUserCmd.Parameters.AddWithValue("@MatKhauHash", BCrypt.Net.BCrypt.HashPassword(request.MatKhau));
                insertUserCmd.Parameters.AddWithValue("@VaiTro", "CUSTOMER");
                insertUserCmd.Parameters.AddWithValue("@NgayTao", DateTime.Now);
                insertUserCmd.Parameters.AddWithValue("@TrangThai", "ACTIVE");
                
                var maNguoiDung = (int)await insertUserCmd.ExecuteScalarAsync();

                // 2.2: Tạo Khách Hàng
                var insertKhachCmd = new SqlCommand(@"
                    INSERT INTO KhachHang (MaNguoiDung, HoTen, Email, SoDienThoai, TrangThaiKYC)
                    OUTPUT INSERTED.MaKhachHang
                    VALUES (@MaNguoiDung, @HoTen, @Email, @SoDienThoai, @TrangThaiKYC)", conn, transaction);
                
                insertKhachCmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
                insertKhachCmd.Parameters.AddWithValue("@HoTen", request.HoTen);
                insertKhachCmd.Parameters.AddWithValue("@Email", request.Email ?? (object)DBNull.Value);
                insertKhachCmd.Parameters.AddWithValue("@SoDienThoai", request.SoDienThoai ?? (object)DBNull.Value);
                insertKhachCmd.Parameters.AddWithValue("@TrangThaiKYC", "NONE");
                
                var maKhachHang = (int)await insertKhachCmd.ExecuteScalarAsync();

                // 2.3: Tạo Tài Khoản
                var soTaiKhoan = await TaoSoTaiKhoanAsync(conn, transaction);
                var insertTaiKhoanCmd = new SqlCommand(@"
                    INSERT INTO TaiKhoan (MaKhachHang, SoTaiKhoan, SoDu, TrangThai)
                    VALUES (@MaKhachHang, @SoTaiKhoan, @SoDu, @TrangThai)", conn, transaction);
                
                insertTaiKhoanCmd.Parameters.AddWithValue("@MaKhachHang", maKhachHang);
                insertTaiKhoanCmd.Parameters.AddWithValue("@SoTaiKhoan", soTaiKhoan);
                insertTaiKhoanCmd.Parameters.AddWithValue("@SoDu", 0);
                insertTaiKhoanCmd.Parameters.AddWithValue("@TrangThai", "ACTIVE");
                
                await insertTaiKhoanCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                return new
                {
                    thongBao = "Đăng ký tài khoản thành công.",
                    maNguoiDung = maNguoiDung,
                    maKhachHang = maKhachHang,
                    soTaiKhoan = soTaiKhoan,
                    trangThaiKyc = "NONE"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<NguoiDungDTO> DangNhapAsync(string tenDangNhap, string matKhau)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            var cmd = new SqlCommand("SELECT MaNguoiDung, TenDangNhap, MatKhauHash, VaiTro, TrangThai, NgayTao FROM NguoiDung WHERE TenDangNhap = @TenDangNhap", conn);
            cmd.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
            
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                throw new Exception("Tài khoản hoặc mật khẩu không chính xác.");
            }

            var matKhauHash = reader.GetString(2);
            if (!BCrypt.Net.BCrypt.Verify(matKhau, matKhauHash))
            {
                throw new Exception("Tài khoản hoặc mật khẩu không chính xác.");
            }

            var trangThai = reader.GetString(4);
            if (trangThai != "ACTIVE")
            {
                throw new Exception("Tài khoản này đã bị khóa. Vui lòng liên hệ ngân hàng.");
            }

            return new NguoiDungDTO
            {
                MaNguoiDung = reader.GetInt32(0),
                TenDangNhap = reader.GetString(1),
                MatKhauHash = matKhauHash,
                VaiTro = reader.GetString(3),
                TrangThai = trangThai,
                NgayTao = reader.GetDateTime(5)
            };
        }

        public async Task DoiMatKhauAsync(int maNguoiDung, string matKhauCu, string matKhauMoi)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            
            // Lấy mật khẩu hiện tại
            var selectCmd = new SqlCommand("SELECT MatKhauHash FROM NguoiDung WHERE MaNguoiDung = @MaNguoiDung", conn);
            selectCmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
            
            var matKhauHash = await selectCmd.ExecuteScalarAsync() as string;
            if (matKhauHash == null)
            {
                throw new Exception("Người dùng không tồn tại.");
            }

            if (!BCrypt.Net.BCrypt.Verify(matKhauCu, matKhauHash))
            {
                throw new Exception("Mật khẩu cũ không đúng.");
            }

            // Cập nhật mật khẩu mới
            var updateCmd = new SqlCommand("UPDATE NguoiDung SET MatKhauHash = @MatKhauHash WHERE MaNguoiDung = @MaNguoiDung", conn);
            updateCmd.Parameters.AddWithValue("@MatKhauHash", BCrypt.Net.BCrypt.HashPassword(matKhauMoi));
            updateCmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
            
            await updateCmd.ExecuteNonQueryAsync();
        }

        private async Task<string> TaoSoTaiKhoanAsync(SqlConnection conn, SqlTransaction transaction)
        {
            while (true)
            {
                string so = "10" + Random.Shared.NextInt64(100000000000, 999999999999).ToString();
                
                var checkCmd = new SqlCommand("SELECT COUNT(*) FROM TaiKhoan WHERE SoTaiKhoan = @SoTaiKhoan", conn, transaction);
                checkCmd.Parameters.AddWithValue("@SoTaiKhoan", so);
                
                var count = (int)await checkCmd.ExecuteScalarAsync();
                if (count == 0) return so;
            }
        }
    }
}
