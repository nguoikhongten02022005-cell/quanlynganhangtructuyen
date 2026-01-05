using System.Data;
using Microsoft.Data.SqlClient;

namespace DAL;

public class NganHangDAL
{
    private readonly string _connectionString;

    public NganHangDAL(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public async Task<SqlConnection> GetOpenConnectionAsync()
    {
        var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }
}