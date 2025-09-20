using System;
using System.Collections.Generic;
using System.Linq;
using Core.Dtos;            // MenuItem, IdPairDto, BreadcrumbDto
using Data.Repositories;    // MenuRepository
namespace Services
{
    /// <summary>
    /// Dịch vụ xử lý menu: dựng cây, tra cứu theo tên/ID, kiểm tra quyền…
    /// </summary>
    public class MenuService
    {
        private readonly MenuRepository _repo = new MenuRepository();
        /// <summary>
        /// Dựng cây menu cho sidebar theo user (đã lọc quyền).
        /// </summary>
        public IEnumerable<MenuItem> GetMenuTree(string userName)
        {
            var flat = _repo.GetMenuFlat(userName)?.ToList() ?? new List<MenuItem>();
            var byId = flat.ToDictionary(x => x.MenuID);
            var roots = new List<MenuItem>();
            foreach (var item in flat.OrderBy(x => x.Sort))
            {
                if (item.ParentID.HasValue && byId.ContainsKey(item.ParentID.Value))
                    byId[item.ParentID.Value].Children.Add(item);
                else
                    roots.Add(item);
            }
            void Sort(MenuItem n)
            {
                if (n.Children == null || n.Children.Count == 0) return;
                n.Children = n.Children.OrderBy(c => c.Sort).ToList();
                foreach (var c in n.Children) Sort(c);
            }
            foreach (var r in roots) Sort(r);
            return roots.OrderBy(r => r.Sort).ToList();
        }
        /// <summary>Tra parent theo tên hiển thị (MenuName).</summary>
        public MenuItem GetParentByName(string parentName)
        {
            if (string.IsNullOrWhiteSpace(parentName)) return null;
            return _repo.GetParentByName(parentName);
        }
        /// <summary>Tra con theo tên (có xét parentName để tránh trùng ở nhánh khác).</summary>
        public MenuItem GetChildByNames(string parentName, string childName)
        {
            if (string.IsNullOrWhiteSpace(parentName) || string.IsNullOrWhiteSpace(childName)) return null;
            return _repo.GetChildByNames(parentName, childName);
        }
        /// <summary>Tra cứu meta menu theo ID.</summary>
        public MenuItem GetById(int id)
        {
            return _repo.GetById(id);
        }
        /// <summary>Đổi từ tên sang Id (nếu có) — dùng cho nơi cần cả ID lẫn tên.</summary>
        public IdPairDto GetIdsByNames(string parentName, string childName)
        {
            var p = GetParentByName(parentName);
            var c = GetChildByNames(parentName, childName);
            return new IdPairDto { ParentId = p?.MenuID, ChildId = c?.MenuID };
        }
        /// <summary>
        /// Kiểm tra user có quyền vào menu (cha/con) theo tên.
        /// Nếu chỉ truyền parentName thì kiểm tra quyền cha.
        /// </summary>
        public bool UserHasMenu(string userName, string parentName, string childName = null)
        {
            var flat = _repo.GetMenuFlat(userName)?.ToList() ?? new List<MenuItem>();
            if (flat.Count == 0) return false;
            bool Eq(string a, string b) =>
                string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
            // Tìm parent (ParentID == null nghĩa là nút gốc)
            var parent = !string.IsNullOrWhiteSpace(parentName)
                ? flat.FirstOrDefault(m => Eq(m.MenuText, parentName) && m.ParentID == null)
                : null;
            if (string.IsNullOrWhiteSpace(childName))
                return parent != null;
            // Tìm child thuộc đúng parent
            var child = flat.FirstOrDefault(m =>
                Eq(m.MenuText, childName) &&
                (parent == null || m.ParentID == parent.MenuID));
            return child != null;
        }
        /// <summary>
        /// Lấy breadcrumb (cha → con → màn) theo tên.
        /// </summary>
        public BreadcrumbDto GetBreadcrumb(string parentName, string childName, string screen = null)
        {
            var p = GetParentByName(parentName);
            var c = GetChildByNames(parentName, childName);
            return new BreadcrumbDto
            {
                Parent = p?.MenuText ?? "",
                Child = c?.MenuText ?? "",
                Screen = string.IsNullOrWhiteSpace(screen) ? (c?.MenuText ?? "") : screen
            };
        }
        /// <summary>
        /// Resolve meta theo ID (ưu tiên) hoặc theo Name khi ID trống.
        /// Trả về cả Parent/Child đã tra cứu để controller dùng.
        /// </summary>
        public MenuResolveResult ResolveByIdOrName(string parentName, string childName, int? parentId, int? childId)
        {
            var parent = parentId.HasValue ? GetById(parentId.Value) : GetParentByName(parentName);
            var child = childId.HasValue ? GetById(childId.Value) : GetChildByNames(parentName, childName);
            return new MenuResolveResult
            {
                Parent = parent,
                Child = child,
                ParentId = parent?.MenuID,
                ChildId = child?.MenuID,
                ParentName = parent?.MenuText,
                ChildName = child?.MenuText
            };
        }
    }
    /// <summary>
    /// Kết quả resolve meta menu (tránh dùng ValueTuple để tương thích .NET 4.x).
    /// </summary>
    public class MenuResolveResult
    {
        public MenuItem Parent { get; set; }
        public MenuItem Child { get; set; }
        public int? ParentId { get; set; }
        public int? ChildId { get; set; }
        public string ParentName { get; set; }
        public string ChildName { get; set; }
    }
}