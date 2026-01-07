using DAL.Interfaces;
using Model.DTOs;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace DAL.Repositories;

public class TaiKhoanRepository : ITaiKhoanRepository
{
    private readonly NganHangContext _context;

    public TaiKhoanRepository(NganHangContext context)
    {
        _context = context;
    }

    public async Task<int> ThemTaiKhoanAsync(int maKhachHang, string soTaiKhoan, decimal soDu, string trangThai)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
            INSERT INTO TaiKhoan (MaKhachHang, SoTaiKhoan, SoDu, TrangThai)
            VALUES (@MaKhachHang, @SoTaiKhoan, @SoDu, @TrangThai)", conn);

        cmd.Parameters.AddWithValue("@MaKhachHang", maKhachHang);
        cmd.Parameters.AddWithValue("@SoTaiKhoan", soTaiKhoan);
        cmd.Parameters.AddWithValue("@SoDu", soDu);
        cmd.Parameters.AddWithValue("@TrangThai", trangThai);

        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<TaiKhoanDTO?> GetTaiKhoanByMaNguoiDungAsync(int maNguoiDung)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
            SELECT tk.SoTaiKhoan, tk.SoDu, tk.TrangThai
            FROM TaiKhoan tk
            INNER JOIN KhachHang kh ON tk.MaKhachHang = kh.MaKhachHang
            WHERE kh.MaNguoiDung = @MaNguoiDung", conn);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new TaiKhoanDTO
        {
            SoTaiKhoan = reader.GetString(0),
            SoDu = reader.GetDecimal(1),
            TrangThai = reader.GetString(2)
        };
    }

    public async Task<int?> GetMaTaiKhoanByMaNguoiDungAsync(int maNguoiDung)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
            SELECT tk.MaTaiKhoan
            FROM TaiKhoan tk
            INNER JOIN KhachHang kh ON tk.MaKhachHang = kh.MaKhachHang
            WHERE kh.MaNguoiDung = @MaNguoiDung", conn);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);

        var result = await cmd.ExecuteScalarAsync();
        return result != null ? System.Convert.ToInt32(result) : null;
    }

    public async Task<string?> GetSoTaiKhoanByMaNguoiDungAsync(int maNguoiDung)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
            SELECT tk.SoTaiKhoan
            FROM TaiKhoan tk
            INNER JOIN KhachHang kh ON tk.MaKhachHang = kh.MaKhachHang
            WHERE kh.MaNguoiDung = @MaNguoiDung", conn);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);

        return await cmd.ExecuteScalarAsync() as string;
    }

    public async Task<bool> KiemTraSoTaiKhoanDaTonTaiAsync(string soTaiKhoan)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT COUNT(*) FROM TaiKhoan WHERE SoTaiKhoan = @SoTaiKhoan", conn);
        cmd.Parameters.AddWithValue("@SoTaiKhoan", soTaiKhoan);
        var count = (int)await cmd.ExecuteScalarAsync();
        return count > 0;
    }

    public async Task<TraCuuNguoiNhanDTO?> GetTaiKhoanNhanBySoTaiKhoanAsync(string soTaiKhoan)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
            SELECT tk.SoTaiKhoan, tk.TrangThai, kh.HoTen
            FROM TaiKhoan tk
            INNER JOIN KhachHang kh ON tk.MaKhachHang = kh.MaKhachHang
            WHERE tk.SoTaiKhoan = @SoTaiKhoan", conn);
        cmd.Parameters.AddWithValue("@SoTaiKhoan", soTaiKhoan);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new TraCuuNguoiNhanDTO
        {
            SoTaiKhoan = reader.GetString(0),
            TrangThai = reader.GetString(1),
            TenKhachHang = reader.GetString(2)
        };
    }

    public async Task<int?> GetMaTaiKhoanBySoTaiKhoanAsync(string soTaiKhoan)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT MaTaiKhoan, TrangThai FROM TaiKhoan WHERE SoTaiKhoan = @SoTaiKhoan", conn);
        cmd.Parameters.AddWithValue("@SoTaiKhoan", soTaiKhoan);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return reader.GetInt32(0);
    }

    public async Task<(int MaTaiKhoan, decimal SoDu, string TrangThai)?> GetTaiKhoanInfoByMaNguoiDungAsync(int maNguoiDung)
    {
        await using var conn = await _context.GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
            SELECT tk.MaTaiKhoan, tk.SoDu, tk.TrangThai
            FROM TaiKhoan tk
            INNER JOIN KhachHang kh ON tk.MaKhachHang = kh.MaKhachHang
            WHERE kh.MaNguoiDung = @MaNguoiDung", conn);
        cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return (reader.GetInt32(0), reader.GetDecimal(1), reader.GetString(2));
    }
}