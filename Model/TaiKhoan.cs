using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Model
{
    [Table("Taikhoan")]
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }
        public int MaKhachHang { get; set; }
        public string SoTaiKhoan { get; set; } = "";
        public decimal SoDu { get; set; } = 0;
        public string TrangThai { get; set; } = "ACTIVE";
    }

}