using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Requests
{
    /// <summary>
    /// Request để khởi tạo giao dịch chuyển tiền với OTP
    /// </summary>
    public class TaoGiaoDichRequest
    {
        /// <summary>
        /// Số tài khoản nhận tiền
        /// </summary>
        public string ToAccount { get; set; } = "";

        /// <summary>
        /// Số tiền chuyển (đơn vị: VNĐ)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Nội dung chuyển khoản
        /// </summary>
        public string Message { get; set; } = "";
    }
}
