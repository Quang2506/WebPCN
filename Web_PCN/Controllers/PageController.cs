using System.Linq;
using System.Text;
using System.Web.Mvc;
using Core.Dtos;
using Services;
namespace Web_PCN.Controllers
{
    public class PageController : Controller
    {
        private readonly MenuService _menuService = new MenuService();
        private readonly PageService _page = new PageService();
        // PENDING (mặc định khi click menu)
        [HttpGet]
        public ActionResult Index(string p, string c, int? pid, int? cid)
        {
            var parent = pid.HasValue ? _menuService.GetById(pid.Value)
                                      : _menuService.GetParentByName(p);
            var child = cid.HasValue ? _menuService.GetById(cid.Value)
                                      : _menuService.GetChildByNames(p, c);
            Session["MenuParentId"] = parent?.MenuID;
            Session["MenuParentName"] = parent?.MenuText ?? "";
            Session["MenuChildId"] = child?.MenuID;
            Session["MenuChildName"] = child?.MenuText ?? "";
            Session["ScreenName"] = child?.MenuText ?? "";
            var user = (Session["UserName"] as string) ?? "V4050021";
            var data = _page.GetChangeRequestsByMenu(
                parent?.MenuText, child?.MenuText, user,
                parent?.MenuID, child?.MenuID
            );
            ViewBag.Title = $"{parent?.MenuText} → {child?.MenuText}";
            return View("Pending", data);
        }
        // QUERY (GET): mở form + nạp dropdown
        [HttpGet]
        public ActionResult Query(string p, string c, int? pid, int? cid)
        {
            var parent = pid.HasValue ? _menuService.GetById(pid.Value)
                                      : _menuService.GetParentByName(p);
            var child = cid.HasValue ? _menuService.GetById(cid.Value)
                                      : _menuService.GetChildByNames(p, c);
            Session["MenuParentId"] = parent?.MenuID;
            Session["MenuParentName"] = parent?.MenuText ?? "";
            Session["MenuChildId"] = child?.MenuID;
            Session["MenuChildName"] = child?.MenuText ?? "";
            Session["ScreenName"] = child?.MenuText ?? "";
            var user = (Session["UserName"] as string) ?? "V4050021";
            var vm = new QueryViewModel
            {
                ParentName = parent?.MenuText,
                ChildName = child?.MenuText,
                ParentId = parent?.MenuID,
                ChildId = child?.MenuID,
                // Cho GET lần đầu: hiển thị giống Pending
                Results = _page.GetChangeRequestsByMenu(
                    parent?.MenuText, child?.MenuText, user,
                    parent?.MenuID, child?.MenuID
                )
            };
            // dropdown theo SP lookup
            vm.CategoryList = _page.GetOptionList("category", vm.ParentName, vm.ChildName, user, vm.ParentId, vm.ChildId);
            vm.StatusList = _page.GetOptionList("status", vm.ParentName, vm.ChildName, user, vm.ParentId, vm.ChildId);
            vm.DocumentCodeList = _page.GetOptionList("doc", vm.ParentName, vm.ChildName, user, vm.ParentId, vm.ChildId);
            vm.ChangeTitleList = _page.GetOptionList("title", vm.ParentName, vm.ChildName, user, vm.ParentId, vm.ChildId);
            ViewBag.Title = $"{vm.ParentName} → {vm.ChildName} (Query)";
            return View("Query", vm);
        }
        // QUERY (POST): submit bộ lọc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Query(QueryViewModel vm)
        {
            var user = (Session["UserName"] as string) ?? "V4050021";
            vm.Results = _page.QueryChangeRequests(
                vm.ParentName, vm.ChildName, user,
                vm.Category, vm.DocumentCode, vm.ChangeTitle, vm.Status,
                vm.ParentId, vm.ChildId
            );
            // nạp lại dropdowns sau POST
            vm.CategoryList = _page.GetOptionList("category", vm.ParentName, vm.ChildName, user, vm.ParentId, vm.ChildId);
            vm.StatusList = _page.GetOptionList("status", vm.ParentName, vm.ChildName, user, vm.ParentId, vm.ChildId);
            vm.DocumentCodeList = _page.GetOptionList("doc", vm.ParentName, vm.ChildName, user, vm.ParentId, vm.ChildId);
            vm.ChangeTitleList = _page.GetOptionList("title", vm.ParentName, vm.ChildName, user, vm.ParentId, vm.ChildId);
            ViewBag.Title = $"{vm.ParentName} → {vm.ChildName}";
            return View("Query", vm);
        }
        // DOWNLOAD CSV
        [HttpPost]
        public FileResult Download(QueryViewModel vm)
        {
            var user = (Session["UserName"] as string) ?? "V4050021";
            var rows = _page.QueryChangeRequests(
                vm.ParentName, vm.ChildName, user,
                vm.Category, vm.DocumentCode, vm.ChangeTitle, vm.Status,
                vm.ParentId, vm.ChildId
            ).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("Code,Version,ChangeTitle,Department,Status");
            foreach (var r in rows)
                sb.AppendLine($"{r.Code},{r.Version},\"{r.ChangeTitle}\",{r.Department},{r.Status}");
            return File(Encoding.UTF8.GetBytes(sb.ToString()),
                        "text/csv",
                        "change-requests.csv");
        }
    }
}