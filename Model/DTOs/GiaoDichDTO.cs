using System;

namespace Model.DTOs
{
    public class GiaoDichDTO
    {
        public int MaGiaoDich { get; set; }
        public int MaTaiKhoanGui { get; set; }
        public int MaTaiKhoanNhan { get; set; }
        public string SoTaiKhoanGui { get; set; }
        public string SoTaiKhoanNhan { get; set; }
        public decimal SoTien { get; set; }
        public string NoiDung { get; set; }
        public DateTime NgayGiaoDich { get; set; }
        public string TrangThai { get; set; } = "PENDING";
        public string? MaOTP { get; set; }
        public DateTime? ThoiHanOTP { get; set; }
    }
}
