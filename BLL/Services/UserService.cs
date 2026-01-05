using DAL;
using Microsoft.Data.SqlClient;
using Model.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly NganHangDAL _db;

        public UserService(NganHangDAL db)
        {
            _db = db;
        }

        public async Task<object> GetUsersAsync(string? role, string? status)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            
            var whereClause = "WHERE 1=1";
            if (!string.IsNullOrEmpty(role))
            {
                whereClause += " AND nd.VaiTro = @Role";
            }
            if (!string.IsNullOrEmpty(status))
            {
                whereClause += " AND nd.TrangThai = @Status";
            }

            var query = $@"
                SELECT 
                    nd.MaNguoiDung,
                    nd.TenDangNhap,
                    nd.VaiTro,
                    nd.TrangThai,
                    CASE 
                        WHEN kh.HoTen IS NOT NULL THEN kh.HoTen
                        WHEN nd.VaiTro = 'ADMIN' THEN N'Quản Trị Viên'
                        ELSE N'Nhân Viên'
                    END AS HoTen,
                    kh.Email
                FROM NguoiDung nd
                LEFT JOIN KhachHang kh ON nd.MaNguoiDung = kh.MaNguoiDung
                {whereClause}";

            var cmd = new SqlCommand(query, conn);
            if (!string.IsNullOrEmpty(role))
            {
                cmd.Parameters.AddWithValue("@Role", role.ToUpperInvariant());
            }
            if (!string.IsNullOrEmpty(status))
            {
                cmd.Parameters.AddWithValue("@Status", status.ToUpperInvariant());
            }

            var danhSach = new List<NguoiDungDTO>();
            await using var reader = await cmd.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                danhSach.Add(new NguoiDungDTO
                {
                    MaNguoiDung = reader.GetInt32(0),
                    TenDangNhap = reader.GetString(1),
                    VaiTro = reader.GetString(2),
                    TrangThai = reader.GetString(3),
                    HoTen = reader.GetString(4),
                    Email = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }

            return new { tongSo = danhSach.Count, danhSach = danhSach };
        }

        public async Task<NguoiDungDTO> TaoNguoiDungHeThongAsync(string tenDangNhap, string matKhau, string vaiTro)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            
            // Kiểm tra trùng
            var checkCmd = new SqlCommand("SELECT COUNT(*) FROM NguoiDung WHERE TenDangNhap = @TenDangNhap", conn);
            checkCmd.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
            var count = (int)await checkCmd.ExecuteScalarAsync();
            
            if (count > 0)
            {
                throw new Exception("Tên đăng nhập đã tồn tại.");
            }

            // Tạo người dùng
            var insertCmd = new SqlCommand(@"
                INSERT INTO NguoiDung (TenDangNhap, MatKhauHash, VaiTro, TrangThai, NgayTao)
                OUTPUT INSERTED.MaNguoiDung
                VALUES (@TenDangNhap, @MatKhauHash, @VaiTro, @TrangThai, @NgayTao)", conn);
            
            insertCmd.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
            insertCmd.Parameters.AddWithValue("@MatKhauHash", BCrypt.Net.BCrypt.HashPassword(matKhau));
            insertCmd.Parameters.AddWithValue("@VaiTro", vaiTro);
            insertCmd.Parameters.AddWithValue("@TrangThai", "ACTIVE");
            insertCmd.Parameters.AddWithValue("@NgayTao", DateTime.Now);
            
            var maNguoiDung = (int)await insertCmd.ExecuteScalarAsync();

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
            await using var conn = await _db.GetOpenConnectionAsync();
            
            // Kiểm tra tồn tại
            var checkCmd = new SqlCommand("SELECT COUNT(*) FROM NguoiDung WHERE MaNguoiDung = @MaNguoiDung", conn);
            checkCmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
            var count = (int)await checkCmd.ExecuteScalarAsync();
            
            if (count == 0)
            {
                throw new Exception("Người dùng không tồn tại.");
            }

            // Cập nhật trạng thái
            var updateCmd = new SqlCommand("UPDATE NguoiDung SET TrangThai = @TrangThai WHERE MaNguoiDung = @MaNguoiDung", conn);
            updateCmd.Parameters.AddWithValue("@TrangThai", khoa ? "LOCKED" : "ACTIVE");
            updateCmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
            
            await updateCmd.ExecuteNonQueryAsync();
        }
    }
}
