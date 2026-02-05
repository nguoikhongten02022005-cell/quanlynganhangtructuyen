using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace DAL
{
    public class PasswordResetDAL
    {
        private readonly NganHangContext _context;

        public PasswordResetDAL(NganHangContext context)
        {
            _context = context;
        }

        private async Task<SqlConnection> GetOpenConnectionAsync()
        {
            return await _context.GetOpenConnectionAsync();
        }

        public async Task<int> TaoTokenAsync(int maNguoiDung, string tokenHash, DateTime expiresAt)
        {
            await using var conn = await GetOpenConnectionAsync();
            var cmd = new SqlCommand(@"
                INSERT INTO PasswordResetToken (MaNguoiDung, TokenHash, ExpiresAt, CreatedAt)
                OUTPUT INSERTED.Id
                VALUES (@MaNguoiDung, @TokenHash, @ExpiresAt, @CreatedAt)", conn);

            cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
            cmd.Parameters.AddWithValue("@TokenHash", tokenHash);
            cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

            return (int)await cmd.ExecuteScalarAsync();
        }

        public async Task<int?> LayTokenHopLeAsync(int maNguoiDung, string tokenHash)
        {
            await using var conn = await GetOpenConnectionAsync();
            var cmd = new SqlCommand(@"
                SELECT TOP 1 Id
                FROM PasswordResetToken
                WHERE MaNguoiDung = @MaNguoiDung
                  AND TokenHash = @TokenHash
                  AND UsedAt IS NULL
                  AND ExpiresAt > @Now
                ORDER BY CreatedAt DESC", conn);

            cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
            cmd.Parameters.AddWithValue("@TokenHash", tokenHash);
            cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);

            var result = await cmd.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value) return null;
            return Convert.ToInt32(result);
        }

        public async Task<int> DanhDauDaSuDungAsync(int tokenId)
        {
            await using var conn = await GetOpenConnectionAsync();
            var cmd = new SqlCommand(@"
                UPDATE PasswordResetToken
                SET UsedAt = @UsedAt
                WHERE Id = @Id", conn);

            cmd.Parameters.AddWithValue("@UsedAt", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@Id", tokenId);
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<(int Id, string TokenHash)?> LayTokenGanNhatHopLeAsync(int maNguoiDung)
        {
            await using var conn = await GetOpenConnectionAsync();
            var cmd = new SqlCommand(@"
                SELECT TOP 1 Id, TokenHash
                FROM PasswordResetToken
                WHERE MaNguoiDung = @MaNguoiDung
                  AND UsedAt IS NULL
                  AND ExpiresAt > @Now
                ORDER BY CreatedAt DESC", conn);

            cmd.Parameters.AddWithValue("@MaNguoiDung", maNguoiDung);
            cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return (reader.GetInt32(0), reader.GetString(1));
        }
    }
}
