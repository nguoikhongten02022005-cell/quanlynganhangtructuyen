using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class NguoiDung
    {
        [Key]
        public int MaNguoiDung { get; set; }
        public string TenDangNhap { get; set; } = "";
        public string MatKhauHash { get; set; } = "";
        public string VaiTro { get; set; } = "CUSTOMER";
        public DateTime NgayTao { get; set; }
        public string TrangThai { get; set; } = "ACTIVE";

    }
}

