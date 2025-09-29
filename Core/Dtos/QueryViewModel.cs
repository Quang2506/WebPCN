using System.Collections.Generic;
using System.Web.Mvc;
namespace Core.Dtos
{
    public class QueryViewModel
    {
        // ===== Context =====
        public string Plant { get; set; }       // QSMC / QMH
        public string DepCode { get; set; }     // dep_code duy nhất (vd: vi_cr_pe)
        public string ParentName { get; set; }  // để hiển thị breadcrumb (optional)
        public string ChildName { get; set; }
        public int? ParentId { get; set; }
        public int? ChildId { get; set; }
        // ===== Bộ lọc =====
        public string Category { get; set; }       // dropdown
        public string DocumentCode { get; set; }   // text nhập tay
        public string ChangeTitle { get; set; }    // text nhập tay
        public string Status { get; set; }         // text nhập tay

     
        // ===== Nguồn cho dropdown =====
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }

        // ===== Kết quả =====
        public IEnumerable<ChangeRequestDto> Results { get; set; }
    }
}
