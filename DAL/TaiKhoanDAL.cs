using Model.DTOs;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace DAL;

/// <summary>
/// Data Access Layer cho bảng TaiKhoan
/// </summary>
public class TaiKhoanDAL
{
    private readonly NganHangContext _context;

    public TaiKhoanDAL(NganHangContext context)
    {
        _context = context;
    }

    private async Task<SqlConnection> GetOpenConnectionAsync()
    {
        return await _context.GetOpenConnectionAsync();
    }

    public async Task<int> ThemTaiKhoanAsync(int maKhachHang, string soTaiKhoan, decimal soDu, string trangThai)
    {
        await using var conn = await GetOpenConnectionAsync();
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
        await using var conn = await GetOpenConnectionAsync();
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
        await using var conn = await GetOpenConnectionAsync();
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
        await using var conn = await GetOpenConnectionAsync();
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
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT COUNT(*) FROM TaiKhoan WHERE SoTaiKhoan = @SoTaiKhoan", conn);
        cmd.Parameters.AddWithValue("@SoTaiKhoan", soTaiKhoan);
        var count = (int)await cmd.ExecuteScalarAsync();
        return count > 0;
    }

    public async Task<TraCuuNguoiNhanDTO?> GetTaiKhoanNhanBySoTaiKhoanAsync(string soTaiKhoan)
    {
        await using var conn = await GetOpenConnectionAsync();
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
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT MaTaiKhoan FROM TaiKhoan WHERE SoTaiKhoan = @SoTaiKhoan", conn);
        cmd.Parameters.AddWithValue("@SoTaiKhoan", soTaiKhoan);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return reader.GetInt32(0);
    }

    public async Task<(int MaTaiKhoan, string TrangThai)?> GetTrangThaiTaiKhoanByMaTaiKhoanAsync(int maTaiKhoan)
    {
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand("SELECT MaTaiKhoan, TrangThai FROM TaiKhoan WHERE MaTaiKhoan = @MaTaiKhoan", conn);
        cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return (reader.GetInt32(0), reader.GetString(1));
    }

    public async Task<(int MaTaiKhoan, decimal SoDu, string TrangThai)?> GetTaiKhoanInfoByMaNguoiDungAsync(int maNguoiDung)
    {
        await using var conn = await GetOpenConnectionAsync();
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

    public async Task<List<TaiKhoanDTO>> GetAllTaiKhoanAsync()
    {
        var danhSach = new List<TaiKhoanDTO>();
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
            SELECT tk.MaTaiKhoan, tk.MaKhachHang, tk.SoTaiKhoan, tk.SoDu, tk.TrangThai,
                   kh.HoTen, kh.SoCCCD
            FROM TaiKhoan tk
            INNER JOIN KhachHang kh ON tk.MaKhachHang = kh.MaKhachHang", conn);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            danhSach.Add(new TaiKhoanDTO
            {
                MaTaiKhoan = reader.GetInt32(0),
                MaKhachHang = reader.GetInt32(1),
                SoTaiKhoan = reader.GetString(2),
                SoDu = reader.GetDecimal(3),
                TrangThai = reader.GetString(4)
            });
        }
        return danhSach;
    }

    public async Task<TaiKhoanDTO?> GetTaiKhoanByIdAsync(int maTaiKhoan)
    {
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand(@"
            SELECT tk.MaTaiKhoan, tk.MaKhachHang, tk.SoTaiKhoan, tk.SoDu, tk.TrangThai,
                   kh.HoTen, kh.SoCCCD
            FROM TaiKhoan tk
            INNER JOIN KhachHang kh ON tk.MaKhachHang = kh.MaKhachHang
            WHERE tk.MaTaiKhoan = @MaTaiKhoan", conn);
        cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new TaiKhoanDTO
        {
            MaTaiKhoan = reader.GetInt32(0),
            MaKhachHang = reader.GetInt32(1),
            SoTaiKhoan = reader.GetString(2),
            SoDu = reader.GetDecimal(3),
            TrangThai = reader.GetString(4)
        };
    }

    public async Task CapNhatTrangThaiAsync(int maTaiKhoan, string trangThai)
    {
        await using var conn = await GetOpenConnectionAsync();
        var cmd = new SqlCommand("UPDATE TaiKhoan SET TrangThai = @TrangThai WHERE MaTaiKhoan = @MaTaiKhoan", conn);
        cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
        cmd.Parameters.AddWithValue("@TrangThai", trangThai);
        await cmd.ExecuteNonQueryAsync();
    }
}