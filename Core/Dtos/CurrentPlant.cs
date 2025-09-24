//// Core/Dtos/DashboardDtos.cs
//using System.Collections.Generic;

//namespace Core.Dtos
//{
//    public class PlantSummaryDto
//    {
//        public string Plant { get; set; }      // "QSMC" | "QMH"
//        public int SyncRate { get; set; }
//        public int NotSyncedCount { get; set; }
//    }

//    public class CellStatusDto
//    {
//        public string Plant { get; set; }      // "QSMC" | "QMH"
//        public string RowKey { get; set; }     // "FATP" | "CR" | "SMT" | "QA"
//        public string ColKey { get; set; }     // "PE" | "AME" | "TE" | ...
//        public string GroupDep { get; set; }   // ví dụ: "fatp_qms", "qms_cr" (từ Menulist.group_dep)
//        public bool IsCompleted { get; set; }
//        public bool HasAccess { get; set; }
//        public string LinkUrl { get; set; }    // link Pending/Query
//    }

//    public class DashboardViewModel
//    {
//        public string User { get; set; }
//        public string CurrentPlant { get; set; } = "QSMC";   // NEW

//        public List<PlantSummaryDto> PlantSummaries { get; set; } = new List<PlantSummaryDto>();
//        public List<string> Rows { get; set; } = new List<string> { "FATP", "CR", "SMT", "QA" };
//        public List<string> Cols { get; set; } = new List<string> { "PE", "AME", "TE", "ME", "SW", "EQ", "PA", "LAB", "SMTQA", "IPQC" };

//        public List<CellStatusDto> Cells { get; set; } = new List<CellStatusDto>();

//        public CellStatusDto GetCell(string plant, string row, string col)
//            => Cells.Find(c => c.Plant == plant && c.RowKey == row && c.ColKey == col);
//    }
//}