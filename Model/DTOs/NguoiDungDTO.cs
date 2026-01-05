using System;

namespace Model.DTOs
{
    public class NguoiDungDTO
    {
        public int MaNguoiDung { get; set; }
        public string TenDangNhap { get; set; } = "";
        public string MatKhauHash { get; set; } = "";
        public string VaiTro { get; set; } = "CUSTOMER";
        public string TrangThai { get; set; } = "ACTIVE";
        public DateTime NgayTao { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
    }
}
