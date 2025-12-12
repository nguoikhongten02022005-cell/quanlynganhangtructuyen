using Microsoft.EntityFrameworkCore;
using Model;

namespace DAL;

public class NganHangDAL : DbContext
{
    public NganHangDAL(DbContextOptions<NganHangDAL> options) : base(options) { }

    public DbSet<NguoiDung> NguoiDung { get; set; } = null!;
    public DbSet<KhachHang> KhachHang { get; set; } = null!;
}