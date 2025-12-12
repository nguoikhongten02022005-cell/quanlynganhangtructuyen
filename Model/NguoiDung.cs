using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class NguoiDung
    {
        public int MaNguoiDung { get; set; }
        public string TenDangNhap { get; set; } = "";
        public string MatKhauHash { get; set; } = "";
        public string VaiTro { get; set; } = "CUSTOMER";
        public DateTime NgayTao { get; set; }
    }
}
