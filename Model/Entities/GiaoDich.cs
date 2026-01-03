using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entities
{
    public class GiaoDich
    {
        [Key]
        public int MaGiaoDich { get; set; }
        public int MaTaiKhoanGui { get; set; }
        public int MaTaiKhoanNhan { get; set; }
        public decimal SoTien { get; set; }
        public string? NoiDung { get; set; }
        public DateTime NgayGiaoDich { get; set; }
        public string TrangThai { get; set; } = "PENDING"; // PENDING, SUCCESS, FAILED
        public string? MaOTP { get; set; }
        public DateTime? ThoiHanOTP { get; set; }
    }
}
