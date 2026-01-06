using DAL.Interfaces;
using DAL;
using Model.DTOs;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories;

public class NguoiDungRepository : INguoiDungRepository
{
    private readonly NganHangContext _context;

    public NguoiDungRepository(NganHangContext context)
    {
        _context = context;
    }

    private async Task<SqlConnection> GetOpenConnectionAsync()
    {
        return await _context.GetOpenConnectionAsync();
    }

    public async Task<NguoiDungDTO?> GetNguoiDungByTenDangNhapAsync(string tenDangNhap)
    {
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT MaNguoiDung, TenDangNhap, MatKhauHash, VaiTro, TrangThai, NgayTao FROM NguoiDung WHERE TenDangNhap = @TenDangNhap", conn);
        cmd.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new NguoiDungDTO
        {
            MaNguoiDung = reader.GetInt32(0),
            TenDangNhap = reader.GetString(1),
            MatKhauHash = reader.GetString(2),
            VaiTro = reader.GetString(3),
            TrangThai = reader.GetString(4),
            NgayTao = reader.GetDateTime(5)
        };
    }

    public async Task<bool> KiemTraTonTaiTenDangNhapAsync(string tenDangNhap)
    {
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT COUNT(*) FROM NguoiDung WHERE TenDangNhap = @TenDangNhap", conn);
        cmd.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
        var count = (int)await cmd.ExecuteScalarAsync();
        return count > 0;
    }

    public async Task<NguoiDungDTO?> GetNguoiDungByIdAsync(int maNguoiDung)
    {
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT MaNguoiDung, TenDangNhap, MatKhauHash, VaiTro, TrangThai, NgayTao FROM NguoiDung WHERE MaNguoiDung = @MaNguoiDung", conn);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new NguoiDungDTO
        {
            MaNguoiDung = reader.GetInt32(0),
            TenDangNhap = reader.GetString(1),
            MatKhauHash = reader.GetString(2),
            VaiTro = reader.GetString(3),
            TrangThai = reader.GetString(4),
            NgayTao = reader.GetDateTime(5)
        };
    }

    public async Task<string?> GetMatKhauHashByIdAsync(int maNguoiDung)
    {
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT MatKhauHash FROM NguoiDung WHERE MaNguoiDung = @MaNguoiDung", conn);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
        return await cmd.ExecuteScalarAsync() as string;
    }

    public async Task<int> ThemNguoiDungAsync(string tenDangNhap, string matKhauHash, string vaiTro, string trangThai)
    {
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
            INSERT INTO NguoiDung (TenDangNhap, MatKhauHash, VaiTro, TrangThai, NgayTao)
            OUTPUT INSERTED.MaNguoiDung
            VALUES (@TenDangNhap, @MatKhauHash, @VaiTro, @TrangThai, @NgayTao)", conn);

        cmd.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
        cmd.Parameters.AddWithValue("@MatKhauHash", matKhauHash);
        cmd.Parameters.AddWithValue("@VaiTro", vaiTro);
        cmd.Parameters.AddWithValue("@TrangThai", trangThai);
        cmd.Parameters.AddWithValue("@NgayTao", DateTime.Now);

        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<int> CapNhatMatKhauAsync(int maNguoiDung, string matKhauHash)
    {
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand("UPDATE NguoiDung SET MatKhauHash = @MatKhauHash WHERE MaNguoiDung = @MaNguoiDung", conn);
        cmd.Parameters.AddWithValue("@MatKhauHash", matKhauHash);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> CapNhatTrangThaiNguoiDungAsync(int maNguoiDung, string trangThai)
    {
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand("UPDATE NguoiDung SET TrangThai = @TrangThai WHERE MaNguoiDung = @MaNguoiDung", conn);
        cmd.Parameters.AddWithValue("@TrangThai", trangThai);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<NguoiDungDTO>> GetDanhSachNguoiDungAsync(string? role, string? status)
    {
        await using var conn = await GetOpenConnectionAsync();

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

        return danhSach;
    }

    public async Task<NguoiDungDTO?> LayChiTietNguoiDungAsync(int maNguoiDung)
    {
        await using var conn = await GetOpenConnectionAsync();

        var query = @"
            SELECT
                nd.MaNguoiDung,
                nd.TenDangNhap,
                nd.VaiTro,
                nd.TrangThai,
                nd.NgayTao,
                CASE
                    WHEN kh.HoTen IS NOT NULL THEN kh.HoTen
                    WHEN nd.VaiTro = 'ADMIN' THEN N'Quản Trị Viên'
                    ELSE N'Nhân Viên'
                END AS HoTen,
                kh.Email,
                kh.SoDienThoai,
                kh.SoCCCD,
                kh.TrangThaiKYC
            FROM NguoiDung nd
            LEFT JOIN KhachHang kh ON nd.MaNguoiDung = kh.MaNguoiDung
            WHERE nd.MaNguoiDung = @MaNguoiDung";

        var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new NguoiDungDTO
            {
                MaNguoiDung = reader.GetInt32(0),
                TenDangNhap = reader.GetString(1),
                VaiTro = reader.GetString(2),
                TrangThai = reader.GetString(3),
                NgayTao = reader.GetDateTime(4),
                HoTen = reader.GetString(5),
                Email = reader.IsDBNull(6) ? null : reader.GetString(6)
            };
        }

        return null;
    }

    public async Task<int> GetTongNguoiDungAsync()
    {
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT COUNT(*) FROM NguoiDung", conn);
        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<int> GetTongKhachHangAsync()
    {
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT COUNT(*) FROM KhachHang", conn);
        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<int> GetSoKYCPendingAsync()
    {
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT COUNT(*) FROM KhachHang WHERE TrangThaiKYC = 'PENDING'", conn);
        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<int> GetTongGiaoDichThanhCongAsync()
    {
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT COUNT(*) FROM GiaoDich WHERE TrangThai = 'SUCCESS'", conn);
        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<decimal> GetTongSoTienGiaoDichAsync()
    {
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT ISNULL(SUM(SoTien), 0) FROM GiaoDich WHERE TrangThai = 'SUCCESS'", conn);
        return (decimal)await cmd.ExecuteScalarAsync();
    }
}