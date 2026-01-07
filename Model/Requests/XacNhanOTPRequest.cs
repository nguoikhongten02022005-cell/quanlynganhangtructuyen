using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Model.Requests
{
    
    public class XacNhanOTPRequest
    {
        [JsonPropertyName("giaoDichId")]
        public int MaGiaoDich { get; set; }
        
        [JsonPropertyName("maOTP")]
        public string MaOTP { get; set; } = "";
    }
}
