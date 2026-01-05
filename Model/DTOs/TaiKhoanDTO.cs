namespace Model.DTOs
{
    public class TaiKhoanDTO
    {
        public int MaTaiKhoan { get; set; }
        public int MaKhachHang { get; set; }
        public string SoTaiKhoan { get; set; } = "";
        public decimal SoDu { get; set; } = 0;
        public string TrangThai { get; set; } = "ACTIVE";
    }
}
