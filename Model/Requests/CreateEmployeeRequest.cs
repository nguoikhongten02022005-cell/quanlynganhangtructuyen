using System;

namespace Model.Requests
{
    public class CreateEmployeeRequest
    {
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
    }
}
