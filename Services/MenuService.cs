using Core.Dtos;
using Data.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    public class MenuService
    {
        private readonly MenuRepository _repo = new MenuRepository();

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

            void sort(MenuItem n)
            {
                if (n.Children == null) return;
                n.Children = n.Children.OrderBy(c => c.Sort).ToList();
                foreach (var c in n.Children) sort(c);
            }
            foreach (var r in roots) sort(r);
            return roots.OrderBy(r => r.Sort).ToList();
        }

        // Dùng để bảo vệ action (theo MenuName)
        public bool UserHasMenu(string userName, string menuName)
        {
            var flat = _repo.GetMenuFlat(userName);
            return flat.Any(m => m.MenuText != null &&
                                 m.MenuText.Trim().ToLower() == (menuName ?? "").Trim().ToLower());
        }

    }

}
