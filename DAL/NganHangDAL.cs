using Microsoft.Data.SqlClient;

namespace DAL;

/// <summary>
/// Data Access Layer cho kết nối database
/// </summary>
public class NganHangDAL
{
    private readonly NganHangContext _context;

    public NganHangDAL(NganHangContext context)
    {
        _context = context;
    }

    public async Task<SqlConnection> GetOpenConnectionAsync()
    {
        var conn = new SqlConnection(_context.ConnectionString);
        await conn.OpenAsync();
        return conn;
    }
}
