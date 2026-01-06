
using Microsoft.Data.SqlClient;

namespace DAL;

/// <summary>
/// Lớp quản lý connection string cho database
/// </summary>
public class NganHangContext
{
    private readonly string _connectionString;

    public NganHangContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public string ConnectionString => _connectionString;

    public async Task<SqlConnection> GetOpenConnectionAsync()
    {
        var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }
}