using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dtos
{
    public class LoginRequest
    {
        public string Login { get; set; }     // User_id / fullname / email
        public string Password { get; set; }
    }
}