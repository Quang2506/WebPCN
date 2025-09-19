using System.Collections.Generic;

namespace Core.Dtos
{
    public class QueryViewModel
    {
  
        public string Category { get; set; }
        public string DocumentCode { get; set; }
        public string ChangeTitle { get; set; }
        public string Status { get; set; }

   
        public IEnumerable<ChangeRequestDto> Results { get; set; }

  
        public string ParentName { get; set; }
        public string ChildName { get; set; }
    }
}
