using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dtos
{
    public class WorkFlow
    {
        public string ChangeRequestID { get; set; }   // Mã yêu cầu thay đổi
        public string Dep_c { get; set; }             // Phòng ban
        public string Status { get; set; }            // Mã trạng thái
        public string Status_nm { get; set; }         // Tên trạng thái
        public string Confirm_per { get; set; }       // Mã ID người xác nhận
        public string Confirm_per_nm { get; set; }    // Tên người xác nhận
        public DateTime Confirm_dt { get; set; }      // Thời gian xác nhận
    }
}
