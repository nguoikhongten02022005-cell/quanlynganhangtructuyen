using DAL.Interfaces;
using Model.DTOs;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories;

public class GiaoDichRepository : IGiaoDichRepository
{
    private readonly NganHangContext _context;

    public GiaoDichRepository(NganHangContext context)
    {
        _context = context;
    }

    public async Task<int> ThemGiaoDichAsync(int maTaiKhoanGui, int maTaiKhoanNhan, decimal soTien, string? noiDung, string trangThai, string maOTP, System.DateTime thoiHanOTP)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
            INSERT INTO GiaoDich (MaTaiKhoanGui, MaTaiKhoanNhan, SoTien, NoiDung, NgayGiaoDich, TrangThai, MaOTP, ThoiHanOTP)
            OUTPUT INSERTED.MaGiaoDich
            VALUES (@MaTaiKhoanGui, @MaTaiKhoanNhan, @SoTien, @NoiDung, @NgayGiaoDich, @TrangThai, @MaOTP, @ThoiHanOTP)", conn);

        cmd.Parameters.AddWithValue("@MaTaiKhoanGui", maTaiKhoanGui);
        cmd.Parameters.AddWithValue("@MaTaiKhoanNhan", maTaiKhoanNhan);
        cmd.Parameters.AddWithValue("@SoTien", soTien);
        cmd.Parameters.AddWithValue("@NoiDung", noiDung ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@NgayGiaoDich", System.DateTime.Now);
        cmd.Parameters.AddWithValue("@TrangThai", trangThai);
        cmd.Parameters.AddWithValue("@MaOTP", maOTP);
        cmd.Parameters.AddWithValue("@ThoiHanOTP", thoiHanOTP);

        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<(int MaTaiKhoanGui, int MaTaiKhoanNhan, decimal SoTien, string NoiDung, string TrangThai, string MaOTP, System.DateTime ThoiHanOTP)?> GetGiaoDichByIdAsync(int maGiaoDich)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT MaTaiKhoanGui, MaTaiKhoanNhan, SoTien, NoiDung, TrangThai, MaOTP, ThoiHanOTP FROM GiaoDich WHERE MaGiaoDich = @MaGiaoDich", conn);
        cmd.Parameters.AddWithValue("@MaGiaoDich", maGiaoDich);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return (
            reader.GetInt32(0),
            reader.GetInt32(1),
            reader.GetDecimal(2),
            reader.IsDBNull(3) ? "" : reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5),
            reader.GetDateTime(6)
        );
    }

    public async Task<int> CapNhatTrangThaiGiaoDichAsync(int maGiaoDich, string trangThai, System.DateTime? ngayGiaoDich = null)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var sql = ngayGiaoDich.HasValue
            ? "UPDATE GiaoDich SET TrangThai = @TrangThai, NgayGiaoDich = @NgayGiaoDich WHERE MaGiaoDich = @MaGiaoDich"
            : "UPDATE GiaoDich SET TrangThai = @TrangThai WHERE MaGiaoDich = @MaGiaoDich";

        var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@TrangThai", trangThai);
        cmd.Parameters.AddWithValue("@MaGiaoDich", maGiaoDich);

        if (ngayGiaoDich.HasValue)
        {
            cmd.Parameters.AddWithValue("@NgayGiaoDich", ngayGiaoDich.Value);
        }

        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> DemGiaoDichByMaTaiKhoanAsync(int maTaiKhoan)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
            SELECT COUNT(*)
            FROM GiaoDich
            WHERE (MaTaiKhoanGui = @MaTaiKhoan OR MaTaiKhoanNhan = @MaTaiKhoan)
            AND TrangThai = 'SUCCESS'", conn);
        cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<List<GiaoDichDTO>> GetLichSuGiaoDichAsync(int maTaiKhoan, int pageSize, int pageNumber)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
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

        cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
        cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
        cmd.Parameters.AddWithValue("@PageSize", pageSize);

        var danhSach = new List<GiaoDichDTO>();
        await using var reader = await cmd.ExecuteReaderAsync();

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

        return danhSach;
    }

    public async Task<GiaoDichDTO?> GetGiaoDichByIdAsync(int maGiaoDich, string soTaiKhoan)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
            SELECT MaGiaoDich, SoTien, NoiDung, NgayGiaoDich, TrangThai,
                   SoTaiKhoanGui, SoTaiKhoanNhan
            FROM GiaoDich
            WHERE MaGiaoDich = @MaGiaoDich
              AND (SoTaiKhoanGui = @SoTaiKhoan OR SoTaiKhoanNhan = @SoTaiKhoan)", conn);

        cmd.Parameters.AddWithValue("@MaGiaoDich", maGiaoDich);
        cmd.Parameters.AddWithValue("@SoTaiKhoan", soTaiKhoan);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new GiaoDichDTO
        {
            MaGiaoDich = reader.GetInt32(0),
            SoTien = reader.GetDecimal(1),
            NoiDung = reader.IsDBNull(2) ? null : reader.GetString(2),
            NgayGiaoDich = reader.GetDateTime(3),
            TrangThai = reader.GetString(4),
            SoTaiKhoanGui = reader.GetString(5),
            SoTaiKhoanNhan = reader.GetString(6)
        };
    }

    public async Task<int> DemGiaoDichNhanDuocAsync(string soTaiKhoan)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
            SELECT COUNT(*)
            FROM GiaoDich
            WHERE SoTaiKhoanNhan = @SoTaiKhoan
              AND TrangThai = 'SUCCESS'", conn);
        cmd.Parameters.AddWithValue("@SoTaiKhoan", soTaiKhoan);
        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<List<GiaoDichDTO>> GetGiaoDichNhanDuocAsync(string soTaiKhoan, int pageSize, int pageNumber)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
            SELECT MaGiaoDich, SoTien, NoiDung, NgayGiaoDich, TrangThai,
                   SoTaiKhoanGui, SoTaiKhoanNhan
            FROM GiaoDich
            WHERE SoTaiKhoanNhan = @SoTaiKhoan
              AND TrangThai = 'SUCCESS'
            ORDER BY NgayGiaoDich DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", conn);

        cmd.Parameters.AddWithValue("@SoTaiKhoan", soTaiKhoan);
        cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
        cmd.Parameters.AddWithValue("@PageSize", pageSize);

        var danhSach = new List<GiaoDichDTO>();
        await using var reader = await cmd.ExecuteReaderAsync();

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

        return danhSach;
    }

    public async Task<string?> ChuyenTienAsync(int maTaiKhoanGui, int maTaiKhoanNhan, decimal soTien, string noiDung)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand("EXEC SP_ChuyenTien @MaTaiKhoanGui, @MaTaiKhoanNhan, @SoTien, @NoiDung", conn);
        cmd.Parameters.AddWithValue("@MaTaiKhoanGui", maTaiKhoanGui);
        cmd.Parameters.AddWithValue("@MaTaiKhoanNhan", maTaiKhoanNhan);
        cmd.Parameters.AddWithValue("@SoTien", soTien);
        cmd.Parameters.AddWithValue("@NoiDung", noiDung);

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        return reader.GetString(0);
    }
}