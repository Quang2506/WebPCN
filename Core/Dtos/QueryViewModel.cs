using Core.Dtos;
using System.Collections.Generic;
using System.Web.Mvc;


namespace Core.Dtos
{
    public class QueryViewModel
    {
  
        public string Category { get; set; }
        public string DocumentCode { get; set; }
        public string ChangeTitle { get; set; }
        public string Status { get; set; }



        public int? ParentId { get; set; }
        public int? ChildId { get; set; }
        public string ParentName { get; set; }
        public string ChildName { get; set; }

        // danh sách cho dropdown
        public IEnumerable<SelectListItem> CategoryList { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }
        public IEnumerable<SelectListItem> DocumentCodeList { get; set; }
        public IEnumerable<SelectListItem> ChangeTitleList { get; set; }

        public IEnumerable<ChangeRequestDto> Results { get; set; }
    }
}



 
