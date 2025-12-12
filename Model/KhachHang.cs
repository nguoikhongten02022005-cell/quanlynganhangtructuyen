using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class KhachHang
    {
        public int MaKhachHang { get; set; }
        public int MaNguoiDung { get; set; }
        public string HoTen { get; set; } = "";
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? SoCCCD { get; set; }
        public string TrangThaiKYC { get; set; } = "PENDING";
    }
}
