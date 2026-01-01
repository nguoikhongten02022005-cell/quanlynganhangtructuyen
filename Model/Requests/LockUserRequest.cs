namespace Model.Requests
{
    public class KhoaTaiKhoanRequest
    {
        // TRUE = Khóa (LOCKED), FALSE = Mở khóa (ACTIVE)
        public bool Khoa { get; set; }
    }
}
