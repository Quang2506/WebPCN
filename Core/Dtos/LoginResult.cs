using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dtos
{

    public class LoginResult
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string User_id { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string dep_c { get; set; }
        public string group_dept { get; set; }
        public int? permit { get; set; }      // mã permit
        public string RoleName { get; set; }  // admin/manager/staff...
        public string site { get; set; }
        public string factory { get; set;}
        public string dep_nm { get; set; }
    }
}