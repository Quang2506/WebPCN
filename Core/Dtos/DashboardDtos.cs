using System.Collections.Generic;
namespace Core.Dtos
{
    // Ô trong bảng
    public class DeptCell
    {
        public string Plant { get; set; }       // QSMC / QMH
        public string Line { get; set; }        // FATP / CR / SMT / QA
        public string Dept { get; set; }        // PE / AME / ...
        public string GroupDep { get; set; }    // dep_code duy nhất (cr_pe_qsmc ...)
        public bool IsDone { get; set; }
        public bool CanClick { get; set; }
        public string TargetUrl { get; set; }
        // để build URL/tooltip nếu cần
        public int? ParentId { get; set; }
        public int? ChildId { get; set; }
        public string ParentName { get; set; }
        public string ChildName { get; set; }
    }
    public class DeptCatalogItem
    {
        public string Plant { get; set; }
        public string Line { get; set; }
        public string Dept { get; set; }
        public string GroupDep { get; set; } // dep_code
        public int? ParentId { get; set; }
        public int? ChildId { get; set; }
        public string ParentName { get; set; }
        public string ChildName { get; set; }
    }
    // Core.Dtos
    public class StatusItem
    {
        public string Dep_Code { get; set; }  // từ SP
        public int IsDone { get; set; }  // 0/1 từ SP
    }
    public class UserRight
    {
        public int MenuID { get; set; }
        public bool Allow { get; set; }
    }
    public class PlantSummaryDto
    {
        public string Plant { get; set; }            // QSMC / QMH
        public int SyncRatePercent { get; set; }
        public int NotSyncedCount { get; set; }
        public int Total { get; set; }
    }
    public class DashboardViewModel
    {
        public string Plant { get; set; } = "QSMC";
        public int SyncRate_QSMC { get; set; }
        public int SyncRate_QMH { get; set; }
        public int NotSynced_QSMC { get; set; }
        public int NotSynced_QMH { get; set; }
        public int Total { get; set; }
        public List<string> Lines { get; set; } = new List<string>();
        public List<string> Depts { get; set; } = new List<string>();
        public List<DeptCell> Grid { get; set; } = new List<DeptCell>();
    }
}