using Microsoft.EntityFrameworkCore;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class HeThongDbContext : DbContext
    {
        public HeThongDbContext(DbContextOptions<HeThongDbContext> opts) : base(opts) { }

        public DbSet<NguoiDung> NguoiDungs { get; set; } = null!;
        public DbSet<KhachHang> KhachHangs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // NguoiDung
            modelBuilder.Entity<NguoiDung>(b =>
            {
                b.ToTable("NguoiDung");
                b.HasKey(x => x.MaNguoiDung);
                b.Property(x => x.MaNguoiDung).HasColumnName("MaNguoiDung");

                b.Property(x => x.TenDangNhap)
                 .HasColumnName("TenDangNhap")
                 .IsRequired()
                 .HasMaxLength(50);

                b.Property(x => x.MatKhauHash)
                 .HasColumnName("MatKhauHash")
                 .IsRequired()
                 .HasMaxLength(255);

                b.Property(x => x.VaiTro)
                 .HasColumnName("VaiTro")
                 .IsRequired()
                 .HasMaxLength(20);

                b.Property(x => x.NgayTao)
                 .HasColumnName("NgayTao")
                 .HasDefaultValueSql("GETDATE()");

                b.HasIndex(x => x.TenDangNhap).IsUnique();
            });

            // KhachHang
            modelBuilder.Entity<KhachHang>(b =>
            {
                b.ToTable("KhachHang");
                b.HasKey(x => x.MaKhachHang);
                b.Property(x => x.MaKhachHang).HasColumnName("MaKhachHang");

                b.Property(x => x.MaNguoiDung)
                 .HasColumnName("MaNguoiDung")
                 .IsRequired();

                b.Property(x => x.HoTen)
                 .HasColumnName("HoTen")
                 .IsRequired()
                 .HasMaxLength(100);

                b.Property(x => x.SoCCCD).HasColumnName("SoCCCD").HasMaxLength(20);
                b.Property(x => x.Email).HasColumnName("Email").HasMaxLength(100);
                b.Property(x => x.SoDienThoai).HasColumnName("SoDienThoai").HasMaxLength(15);
                b.Property(x => x.TrangThaiKYC).HasColumnName("TrangThaiKYC").HasMaxLength(20).HasDefaultValue("PENDING");

                // Liên kết 1-1
                b.HasOne(k => k.NguoiDung)
                 .WithOne(u => u.KhachHang)
                 .HasForeignKey<KhachHang>(k => k.MaNguoiDung)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
