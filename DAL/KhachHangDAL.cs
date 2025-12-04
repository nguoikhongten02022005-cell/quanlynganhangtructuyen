using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class KhachHangDAL
    {
        private readonly NganHangDbContext _db;
        public KhachHangDAL(NganHangDbContext db) => _db = db;

        public Task<KhachHang?> LayTheoIdAsync(int id)
            => _db.KhachHangs.FirstOrDefaultAsync(kh => kh.Id == id);

        public Task<KhachHang?> LayTheoEmailAsync(string email)
            => _db.KhachHangs.FirstOrDefaultAsync(kh => kh.Email == email);

        public async Task ThemAsync(KhachHang kh)
        {
            _db.KhachHangs.Add(kh);
            await _db.SaveChangesAsync();
        }

        public async Task CapNhatAsync(KhachHang kh)
        {
            _db.KhachHangs.Update(kh);
            await _db.SaveChangesAsync();
        }
    }
}
