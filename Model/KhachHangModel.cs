using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class KhachHangModel
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public string SoDienThoai { get; set; } = "";
    }
}
