using DAL.Interfaces;
using Model.DTOs;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories;

public class KhachHangRepository : IKhachHangRepository
{
    private readonly NganHangContext _context;

    public KhachHangRepository(NganHangContext context)
    {
        _context = context;
    }

    public async Task<int> ThemKhachHangAsync(int maNguoiDung, string hoTen, string? email, string? soDienThoai, string trangThaiKYC)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
            INSERT INTO KhachHang (MaNguoiDung, HoTen, Email, SoDienThoai, TrangThaiKYC)
            OUTPUT INSERTED.MaKhachHang
            VALUES (@MaNguoiDung, @HoTen, @Email, @SoDienThoai, @TrangThaiKYC)", conn);

        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
        cmd.Parameters.AddWithValue("@HoTen", hoTen);
        cmd.Parameters.AddWithValue("@Email", (object?)email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SoDienThoai", (object?)soDienThoai ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TrangThaiKYC", trangThaiKYC);

        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<KhachHangProfileDTO?> GetKhachHangByMaNguoiDungAsync(int maNguoiDung)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT HoTen, Email, SoDienThoai, SoCCCD, TrangThaiKYC FROM KhachHang WHERE MaNguoiDung = @MaNguoiDung", conn);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new KhachHangProfileDTO
        {
            HoTen = reader.GetString(0),
            Email = reader.IsDBNull(1) ? null : reader.GetString(1),
            SoDienThoai = reader.IsDBNull(2) ? null : reader.GetString(2),
            SoCCCD = reader.IsDBNull(3) ? null : reader.GetString(3),
            TrangThaiKYC = reader.GetString(4)
        };
    }

    public async Task<bool> KiemTraCCCDDaSuDungAsync(string soCCCD, int maNguoiDung)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT COUNT(*) FROM KhachHang WHERE SoCCCD = @SoCCCD AND MaNguoiDung != @MaNguoiDung", conn);
        cmd.Parameters.AddWithValue("@SoCCCD", soCCCD);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
        var count = (int)await cmd.ExecuteScalarAsync();
        return count > 0;
    }

    public async Task<int> CapNhatCCCDVaKYCAsync(int maNguoiDung, string soCCCD, string trangThaiKYC)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand("UPDATE KhachHang SET SoCCCD = @SoCCCD, TrangThaiKYC = @TrangThaiKYC WHERE MaNguoiDung = @MaNguoiDung", conn);
        cmd.Parameters.AddWithValue("@SoCCCD", soCCCD);
        cmd.Parameters.AddWithValue("@TrangThaiKYC", trangThaiKYC);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<KYCPendingDTO>> GetDanhSachKYCPendingAsync()
    {
        await using var conn = await _context.GetOpenConnectionAsync();
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

    public async Task<(int MaKhachHang, string TrangThaiKYC)?> GetKhachHangInfoByMaNguoiDungAsync(int maNguoiDung)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT MaKhachHang, TrangThaiKYC FROM KhachHang WHERE MaNguoiDung = @MaNguoiDung", conn);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return (reader.GetInt32(0), reader.GetString(1));
    }

    public async Task<int> CapNhatTrangThaiKYCAsync(int maNguoiDung, string trangThaiKYC)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand("UPDATE KhachHang SET TrangThaiKYC = @TrangThaiKYC WHERE MaNguoiDung = @MaNguoiDung", conn);
        cmd.Parameters.AddWithValue("@TrangThaiKYC", trangThaiKYC);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> KiemTraEmailDaSuDungAsync(string email, int maNguoiDung)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT COUNT(*) FROM KhachHang WHERE Email = @Email AND MaNguoiDung != @MaNguoiDung", conn);
        cmd.Parameters.AddWithValue("@Email", email);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
        var count = (int)await cmd.ExecuteScalarAsync();
        return count > 0;
    }

    public async Task<bool> KiemTraSoDienThoaiDaSuDungAsync(string soDienThoai, int maNguoiDung)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT COUNT(*) FROM KhachHang WHERE SoDienThoai = @SoDienThoai AND MaNguoiDung != @MaNguoiDung", conn);
        cmd.Parameters.AddWithValue("@SoDienThoai", soDienThoai);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
        var count = (int)await cmd.ExecuteScalarAsync();
        return count > 0;
    }

    public async Task<int> CapNhatThongTinKhachHangAsync(int maNguoiDung, string? hoTen, string? email, string? soDienThoai)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
            UPDATE KhachHang
            SET HoTen = COALESCE(@HoTen, HoTen),
                Email = COALESCE(@Email, Email),
                SoDienThoai = COALESCE(@SoDienThoai, SoDienThoai)
            WHERE MaNguoiDung = @MaNguoiDung", conn);

        cmd.Parameters.AddWithValue("@HoTen", (object?)hoTen ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Email", (object?)email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SoDienThoai", (object?)soDienThoai ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);

        return await cmd.ExecuteNonQueryAsync();
    }
}