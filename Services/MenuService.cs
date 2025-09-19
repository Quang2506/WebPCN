using Core.Dtos;
using Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    public class MenuService
    {
        private readonly MenuRepository _repo = new MenuRepository();

        // Dựng cây cho sidebar
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

        // Lấy meta theo TÊN
        public MenuItem GetParentByName(string parentName)
        {
            if (string.IsNullOrWhiteSpace(parentName)) return null;
            return _repo.GetParentByName(parentName);
        }

        public MenuItem GetChildByNames(string parentName, string childName)
        {
            if (string.IsNullOrWhiteSpace(parentName) || string.IsNullOrWhiteSpace(childName)) return null;
            return _repo.GetChildByNames(parentName, childName);
        }

        // Đổi tuple -> DTO
        public IdPairDto GetIdsByNames(string parentName, string childName)
        {
            var p = GetParentByName(parentName);
            var c = GetChildByNames(parentName, childName);
            return new IdPairDto { ParentId = p?.MenuID, ChildId = c?.MenuID };
        }

        // Kiểm tra quyền theo tên (xử lý trường hợp tên con trùng ở nhánh khác)
        public bool UserHasMenu(string userName, string parentName, string childName = null)
        {
            var flat = _repo.GetMenuFlat(userName)?.ToList() ?? new List<MenuItem>();
            if (flat.Count == 0) return false;

            bool Eq(string a, string b) =>
                string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);

            var parent = !string.IsNullOrWhiteSpace(parentName)
                         ? flat.FirstOrDefault(m => Eq(m.MenuText, parentName) && (m.ParentID == null))
                         : null;

            if (string.IsNullOrWhiteSpace(childName))
                return parent != null;

            var child = flat.FirstOrDefault(m =>
                           Eq(m.MenuText, childName) &&
                           (parent == null || m.ParentID == parent.MenuID));

            return child != null;
        }

        // Đổi tuple -> DTO
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
    }
}
