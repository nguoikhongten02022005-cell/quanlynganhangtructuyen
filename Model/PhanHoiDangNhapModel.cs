using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class PhanHoiDangNhapModel
    {
        public string Token { get; set; } = "";
        public KhachHangModel Customer { get; set; } = new();
    }
}
