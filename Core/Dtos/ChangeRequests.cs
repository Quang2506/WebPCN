using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dtos
{
    public class ChangeRequests
    {
        public string requestid { get; set; }
        public string category_nm { get; set; }
        public string ChangeTitle { get; set; }
        public string Model { get; set; }
        public string DocumentCode { get; set; }
        public string Request_detail { get; set; }
        public string version { get; set; }
        public string Status { get; set; }
        public string status_nm { get; set; }
        public string dep_c { get; set; }
        public string dep_nm { get; set; }
        public string site { get; set; }
        public string factory { get; set; }
        public string group_dept { get; set; }
        public string Routing { get; set; }
        public string Reason { get; set; }
        public string link_file { get; set; }
        public string pms_i_usr { get; set; }
        public string pms_i_ymd { get; set; }

    }
}
