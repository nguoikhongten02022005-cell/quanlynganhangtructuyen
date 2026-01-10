namespace Model.Requests
{
    public class DatLaiMatKhauRequest
    {
        public string TenDangNhap { get; set; } = "";
        public string Token { get; set; } = "";
        public string MatKhauMoi { get; set; } = "";
        public string NhapLaiMatKhauMoi { get; set; } = "";
    }
}
