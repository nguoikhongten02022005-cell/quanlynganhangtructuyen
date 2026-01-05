namespace Model.DTOs
{
    public class KhachHangProfileDTO
    {
        public int MaKhachHang { get; set; }
        public int MaNguoiDung { get; set; }
        public string HoTen { get; set; } = "";
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string SoCCCD { get; set; }
        public string TrangThaiKYC { get; set; } = "NONE";
    }
}
