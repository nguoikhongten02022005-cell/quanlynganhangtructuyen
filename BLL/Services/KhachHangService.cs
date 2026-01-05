using DAL;
using Microsoft.Data.SqlClient;
using Model.Requests;
using Model.DTOs;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BLL.Services
{
    public class KhachHangService : IKhachHangService
    {
        private readonly NganHangDAL _db;

        public KhachHangService(NganHangDAL db)
        {
            _db = db;
        }

        public async Task<KhachHangProfileDTO> LayThongTinHoSoAsync(int maNguoiDung)
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            var cmd = new SqlCommand("SELECT HoTen, Email, SoDienThoai, SoCCCD, TrangThaiKYC FROM KhachHang WHERE MaNguoiDung = @MaNguoiDung", conn);
            cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
            
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                throw new Exception("Không tìm thấy hồ sơ khách hàng.");
            }

            return new KhachHangProfileDTO
            {
                HoTen = reader.GetString(0),
                Email = reader.IsDBNull(1) ? null : reader.GetString(1),
                SoDienThoai = reader.IsDBNull(2) ? null : reader.GetString(2),
                SoCCCD = reader.IsDBNull(3) ? null : reader.GetString(3),
                TrangThaiKYC = reader.GetString(4)
            };
        }

        public async Task<object> GuiYeuCauKycAsync(int maNguoiDung, KycRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SoCCCD) ||
                (request.SoCCCD.Length != 12 && request.SoCCCD.Length != 13))
            {
                throw new Exception("Số CCCD phải có 12 hoặc 13 chữ số.");
            }

            await using var conn = await _db.GetOpenConnectionAsync();
            
            // Kiểm tra CCCD đã được sử dụng chưa
            var checkCmd = new SqlCommand("SELECT COUNT(*) FROM KhachHang WHERE SoCCCD = @SoCCCD AND MaNguoiDung != @MaNguoiDung", conn);
            checkCmd.Parameters.AddWithValue("@SoCCCD", request.SoCCCD);
            checkCmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
            
            var count = (int)await checkCmd.ExecuteScalarAsync();
            if (count > 0)
            {
                throw new Exception("Số CCCD này đã được sử dụng bởi tài khoản khác.");
            }

            // Cập nhật CCCD và trạng thái KYC
            var updateCmd = new SqlCommand("UPDATE KhachHang SET SoCCCD = @SoCCCD, TrangThaiKYC = 'PENDING' WHERE MaNguoiDung = @MaNguoiDung", conn);
            updateCmd.Parameters.AddWithValue("@SoCCCD", request.SoCCCD);
            updateCmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
            
            var rowsAffected = await updateCmd.ExecuteNonQueryAsync();
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
            await using var conn = await _db.GetOpenConnectionAsync();
            var cmd = new SqlCommand(@"
                SELECT tk.SoTaiKhoan, tk.SoDu, tk.TrangThai
                FROM TaiKhoan tk
                INNER JOIN KhachHang kh ON tk.MaKhachHang = kh.MaKhachHang
                WHERE kh.MaNguoiDung = @MaNguoiDung", conn);
            cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
            
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                throw new Exception("Không tìm thấy tài khoản ngân hàng.");
            }

            var trangThai = reader.GetString(2);
            if (trangThai != "ACTIVE")
            {
                throw new Exception("Tài khoản ngân hàng đã bị khóa.");
            }

            return new TaiKhoanDTO
            {
                SoTaiKhoan = reader.GetString(0),
                SoDu = reader.GetDecimal(1),
                TrangThai = trangThai
            };
        }

        public async Task<object> LayDanhSachKycPendingAsync()
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            var cmd = new SqlCommand("SELECT MaKhachHang, MaNguoiDung, HoTen, Email, SoDienThoai, SoCCCD, TrangThaiKYC FROM KhachHang WHERE TrangThaiKYC = 'PENDING'", conn);
            
            var danhSach = new List<object>();
            await using var reader = await cmd.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                danhSach.Add(new
                {
                    maKhachHang = reader.GetInt32(0),
                    maNguoiDung = reader.GetInt32(1),
                    hoTen = reader.GetString(2),
                    email = reader.IsDBNull(3) ? null : reader.GetString(3),
                    soDienThoai = reader.IsDBNull(4) ? null : reader.GetString(4),
                    soCCCD = reader.IsDBNull(5) ? null : reader.GetString(5),
                    trangThaiKYC = reader.GetString(6)
                });
            }

            return new
            {
                danhSach = danhSach,
                tongSo = danhSach.Count
            };
        }

        public async Task<List<KYCPendingDTO>> LayDanhSachKYCChoDuyetAsync()
        {
            await using var conn = await _db.GetOpenConnectionAsync();
            var cmd = new SqlCommand("SELECT MaKhachHang, HoTen, SoCCCD, Email, SoDienThoai FROM KhachHang WHERE TrangThaiKYC = 'PENDING' ORDER BY MaKhachHang DESC", conn);
            
            var danhSach = new List<KYCPendingDTO>();
            await using var reader = await cmd.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                danhSach.Add(new KYCPendingDTO
                {
                    MaKhachHang = reader.GetInt32(0),
                    HoTen = reader.GetString(1),
                    SoCCCD = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Email = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    SoDienThoai = reader.IsDBNull(4) ? "" : reader.GetString(4)
                });
            }

            return danhSach;
        }

        public async Task<object> DuyetKYCAsync(int customerId, string status, string? reason = null)
        {
            if (status != "APPROVED" && status != "REJECTED")
            {
                throw new Exception("Trạng thái không hợp lệ. Chỉ chấp nhận APPROVED hoặc REJECTED.");
            }

            await using var conn = await _db.GetOpenConnectionAsync();
            
            // Kiểm tra trạng thái hiện tại
            var checkCmd = new SqlCommand("SELECT MaKhachHang, TrangThaiKYC FROM KhachHang WHERE MaNguoiDung = @MaNguoiDung", conn);
            checkCmd.Parameters.AddWithValue("@MaNguoiDung", customerId);
            
            await using var reader = await checkCmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                throw new Exception("Không tìm thấy khách hàng.");
            }

            var maKhachHang = reader.GetInt32(0);
            var trangThaiHienTai = reader.GetString(1);
            await reader.CloseAsync();

            if (trangThaiHienTai != "PENDING")
            {
                throw new Exception("Hồ sơ KYC không ở trạng thái chờ duyệt.");
            }

            // Cập nhật trạng thái
            var updateCmd = new SqlCommand("UPDATE KhachHang SET TrangThaiKYC = @TrangThaiKYC WHERE MaNguoiDung = @MaNguoiDung", conn);
            updateCmd.Parameters.AddWithValue("@TrangThaiKYC", status);
            updateCmd.Parameters.AddWithValue("@MaNguoiDung", customerId);
            
            await updateCmd.ExecuteNonQueryAsync();

            return new
            {
                thongBao = status == "APPROVED" ? "Duyệt KYC thành công." : "Từ chối KYC thành công.",
                maKhachHang = maKhachHang,
                maNguoiDung = customerId,
                trangThaiMoi = status,
                lyDo = reason
            };
        }
    }
}
