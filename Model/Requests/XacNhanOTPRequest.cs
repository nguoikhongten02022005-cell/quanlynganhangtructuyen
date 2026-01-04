using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Requests
{
    
    public class XacNhanOTPRequest
    {
       
        public int MaGiaoDich { get; set; }
        public string MaOTP { get; set; } = "";
    }
}
