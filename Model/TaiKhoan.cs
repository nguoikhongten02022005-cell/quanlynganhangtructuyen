using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class TaiKhoan
    {
        public int MaTaiKhoan { get; set; }
        public int MaKhachHang { get; set; }
        public string SoTaiKhoan { get; set; } = "";
        public decimal SoDu { get; set; } = 0;
        public string TrangThai { get; set; } = "ACTIVE";
    }

}