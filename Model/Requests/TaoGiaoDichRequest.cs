using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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
        [JsonPropertyName("soTaiKhoanNhan")]
        public string ToAccount { get; set; } = "";

        /// <summary>
        /// Số tiền chuyển (đơn vị: VNĐ)
        /// </summary>
        [JsonPropertyName("soTien")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Nội dung chuyển khoản
        /// </summary>
        [JsonPropertyName("noiDung")]
        public string Message { get; set; } = "";
    }
}
