using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;                 // HttpUtility.UrlEncode
using Core.Dtos;
using Data.Repositories;
using System.Reflection;
namespace Services
{
    public class DashboardService
    {
        private readonly DashboardRepository _repo = new DashboardRepository();
        public DashboardViewModel GetDashboard(string userName, string plant)
        {
            // 0) Chuẩn hoá tham số
            plant = string.IsNullOrWhiteSpace(plant) ? "QSMC" : plant.ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(userName))
            {
                var u = userName.Trim();
                if (u.Contains("\\")) u = u.Split('\\').Last();
                userName = u.ToUpperInvariant();
            }
            // 1) Tổng hợp
            var summaries = _repo.LoadSummary(userName)?.ToList() ?? new List<PlantSummaryDto>();
            // 2) Catalog (Line, Dept, GroupDep=dep_code duy nhất, Parent/Child/Name/Id…)
            var catalog = _repo.LoadDeptCatalog(plant)?.ToList() ?? new List<DeptCatalogItem>();
            // 3) Status theo plant (hỗ trợ cả 2 dạng DTO: Dep_Code/IsDone hoặc GroupDep/Status)
            var statuses = _repo.LoadStatusByPlant(plant)?.ToList() ?? new List<StatusItem>();
            // Hàm đọc thuộc tính bằng reflection (an toàn compile trong mọi biến thể DTO)
            string ReadString(object o, string prop)
            {
                if (o == null) return null;
                var p = o.GetType().GetProperty(prop, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null) return null;
                var v = p.GetValue(o);
                return v?.ToString();
            }
            int? ReadNullableInt(object o, string prop)
            {
                if (o == null) return null;
                var p = o.GetType().GetProperty(prop, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null) return null;
                var v = p.GetValue(o);
                if (v == null) return null;
                if (v is int i) return i;
                if (int.TryParse(v.ToString(), out var ii)) return ii;
                return null;
            }
            bool? ReadNullableBool(object o, string prop)
            {
                if (o == null) return null;
                var p = o.GetType().GetProperty(prop, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null) return null;
                var v = p.GetValue(o);
                if (v == null) return null;
                if (v is bool b) return b;
                if (v is int i) return i != 0;
                var s = v.ToString().Trim().ToLowerInvariant();
                if (s == "1" || s == "true" || s == "y" || s == "yes") return true;
                if (s == "0" || s == "false" || s == "n" || s == "no") return false;
                return null;
            }
            // map: key = dep_code (giữ NGUYÊN, KHÔNG cắt tiền tố), value = IsDone
            var statusMap = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in statuses)
            {
                // ưu tiên Dep_Code, fallback GroupDep
                var key = (ReadString(s, "Dep_Code") ?? ReadString(s, "GroupDep") ?? "").Trim();
                if (key.Length == 0) continue;
                // ưu tiên IsDone (bool/int), fallback Status (string)
                bool isDone;
                var doneN = ReadNullableBool(s, "IsDone");
                if (doneN.HasValue)
                {
                    isDone = doneN.Value;
                }
                else
                {
                    var doneStr = ReadString(s, "Status");
                    isDone = NormalizeDone(doneStr);
                }
                statusMap[key] = isDone; // giữ record cuối
            }
            // 4) Quyền theo MenuID; nếu quyền có cột Plant thì lọc đúng plant (hoặc Plant NULL = dùng chung)
            var rightsRaw = _repo.LoadRights(userName, plant)?.ToList() ?? new List<UserRight>();
            var rights = rightsRaw.Where(r =>
            {
                var rp = ReadString(r, "Plant");
                return string.IsNullOrWhiteSpace(rp) || rp.Equals(plant, StringComparison.OrdinalIgnoreCase);
            });
            // Build map MenuID -> Allow
            var rightMap = new Dictionary<int, bool>();
            foreach (var r in rights)
            {
                var mid = ReadNullableInt(r, "MenuID") ?? ReadNullableInt(r, "MenuId") ?? 0;
                if (mid <= 0) continue;
                var allow = ReadNullableBool(r, "Allow") ?? false;
                rightMap[mid] = allow;
            }
            // 5) Build grid
            var grid = new List<DeptCell>();
            foreach (var it in catalog)
            {
                // dep_code duy nhất (đã bao gồm plant), GIỮ NGUYÊN (không cắt "vi_", "ch_", …)
                var depCode = it.GroupDep ?? string.Empty;
                // trạng thái theo dep_code
                bool isDone = false;
                if (!string.IsNullOrWhiteSpace(depCode) &&
                    statusMap.TryGetValue(depCode, out var d)) isDone = d;
                // quyền click theo ChildId (Menu con)
                bool canClick = false;
                if (it.ChildId.HasValue && it.ChildId.Value > 0)
                {
                    canClick = rightMap.TryGetValue(it.ChildId.Value, out var al) && al;
                }
                var url = BuildTargetUrl(it.ParentName, it.ChildName, it.ParentId, it.ChildId, plant, depCode, canClick);
                grid.Add(new DeptCell
                {
                    Plant = plant,
                    Line = it.Line,
                    Dept = it.Dept,
                    GroupDep = depCode,
                    IsDone = isDone,
                    CanClick = canClick,
                    TargetUrl = url,
                    ParentId = it.ParentId,
                    ChildId = it.ChildId,
                    ParentName = it.ParentName,
                    ChildName = it.ChildName
                });
            }
            // 6) Header hàng/cột
            var lines = catalog.Select(x => x.Line).Distinct().OrderBy(x => x).ToList();
            var depts = catalog.Select(x => x.Dept).Distinct().OrderBy(x => x).ToList();
            // 7) ViewModel
            return new DashboardViewModel
            {
                Plant = plant,
                Lines = lines,
                Depts = depts,
                Grid = grid,
                SyncRate_QSMC = summaries.FirstOrDefault(x => x.Plant == "QSMC")?.SyncRatePercent ?? 0,
                SyncRate_QMH = summaries.FirstOrDefault(x => x.Plant == "QMH")?.SyncRatePercent ?? 0,
                NotSynced_QSMC = summaries.FirstOrDefault(x => x.Plant == "QSMC")?.NotSyncedCount ?? 0,
                NotSynced_QMH = summaries.FirstOrDefault(x => x.Plant == "QMH")?.NotSyncedCount ?? 0
            };
        }
        private static bool NormalizeDone(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return false;
            var s = status.Trim().ToLowerInvariant();
            return s == "done" || s == "finished" || s == "ok" || s == "1" || s == "true" || s == "y";
        }
        private static string BuildTargetUrl(
            string parentName, string childName, int? parentId, int? childId,
            string plant, string depCode, bool canClick)
        {
            if (!canClick) return string.Empty;
            var p = HttpUtility.UrlEncode(parentName ?? "");
            var c = HttpUtility.UrlEncode(childName ?? "");
            var pl = HttpUtility.UrlEncode(plant ?? "QSMC");
            var dp = HttpUtility.UrlEncode(depCode ?? "");
            var qp = parentId.HasValue ? $"&pid={parentId.Value}" : "";
            var qc = childId.HasValue ? $"&cid={childId.Value}" : "";
            // Có thể đổi sang /Page/Pending nếu view của bạn dùng Pending
            return $"/Page/Index?p={p}&c={c}&plant={pl}&dep={dp}{qp}{qc}";
        }
    }
}