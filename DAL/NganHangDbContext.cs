using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class NganHangDbContext : DbContext
    {
        public NganHangDbContext(DbContextOptions<NganHangDbContext> tuyChon) : base(tuyChon) { }

        public DbSet<KhachHang> KhachHangs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KhachHang>()
                .HasIndex(kh => kh.Email)
                .IsUnique();
        }
    }
    public class KhachHang
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public string SoDienThoai { get; set; } = "";
        public string MatKhauHash { get; set; } = "";
    }

}
