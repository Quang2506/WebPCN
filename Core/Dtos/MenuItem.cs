using System.Collections.Generic;

namespace Core.Dtos
{
    public class MenuItem
    {
        public int MenuID { get; set; }
        public int? ParentID { get; set; }
        public string MenuText { get; set; }
        public string Url { get; set; }
        public int Sort { get; set; }

        // Raw
        public string FunctionType { get; set; }
        public string LinkTable { get; set; }
        public int TableType { get; set; }

        public List<MenuItem> Children { get; set; } = new List<MenuItem>();
    }
}
