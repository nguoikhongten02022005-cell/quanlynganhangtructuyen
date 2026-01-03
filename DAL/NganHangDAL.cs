using Microsoft.EntityFrameworkCore;
using Model;
using Model.Entities;

namespace DAL;

public class NganHangDAL : DbContext
{
    public NganHangDAL(DbContextOptions<NganHangDAL> options) : base(options) { }

    public DbSet<NguoiDung> NguoiDung { get; set; } = null!;
    public DbSet<KhachHang> KhachHang { get; set; } = null!;
    public DbSet<TaiKhoan> TaiKhoan { get; set; } = null!;
    public DbSet<GiaoDich> GiaoDich { get; set; } = null!;
}