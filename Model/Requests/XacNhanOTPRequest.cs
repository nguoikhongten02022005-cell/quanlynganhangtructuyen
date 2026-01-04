using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Requests
{
    /// <summary>
    /// Request để xác nhận OTP và hoàn tất giao dịch chuyển tiền
    /// </summary>
    public class XacNhanOTPRequest
    {
        /// <summary>
        /// Mã giao dịch từ bước verify
        /// </summary>
        public int MaGiaoDich { get; set; }

        /// <summary>
        /// Mã OTP 6 số người dùng nhập
        /// </summary>
        public string MaOTP { get; set; } = "";
    }
}
