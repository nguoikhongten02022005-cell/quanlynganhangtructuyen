using System;

namespace Model.DTOs
{
    public class KYCPendingDTO
    {
        public int MaKhachHang { get; set; }
        public string HoTen { get; set; } = "";
        public string SoCCCD { get; set; } = "";
        public string Email { get; set; } = "";
        public string SoDienThoai { get; set; } = "";
    }
}
