using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    internal class AuthResponseModel
    {
        public string Token { get; set; } = "";
        public CustomerModel Customer { get; set; } = new();
    }
}
