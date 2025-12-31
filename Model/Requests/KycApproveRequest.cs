using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Requests
{
    public class KycApproveRequest
    {
        public int CustomerId { get; set; }
        public string Status { get; set; } = ""; // ACTIVE hoặc REJECT
        public string? Reason { get; set; }
    }
}
