namespace Model.Requests
{
    public class ResetPasswordRequest
    {
        public int MaNguoiDung { get; set; }
        public string MatKhauMoi { get; set; } = string.Empty;
    }
}
