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

        // =========================
        // PENDING (mặc định khi click menu)
        // URL: /Page/Index?p=FATP&c=QA
        // =========================
        [HttpGet]
        public ActionResult Index(string p, string c)
        {
            // breadcrumb / section
            var parent = _menuService.GetParentByName(p);
            var child = _menuService.GetChildByNames(p, c);

            Session["MenuParentId"] = parent?.MenuID;
            Session["MenuParentName"] = parent?.MenuText ?? "";
            Session["MenuChildId"] = child?.MenuID;
            Session["MenuChildName"] = child?.MenuText ?? "";
            Session["ScreenName"] = child?.MenuText ?? "";

            var user = (Session["UserName"] as string) ?? "V5030587";

            // dữ liệu bảng Pending (danh sách)
            var pending = _page.GetChangeRequestsByMenu(p, c, user);

            ViewBag.Title = $"{p} → {c}";
            return View("Pending", pending); // Views/Page/Pending.cshtml
        }

        // =========================
        // QUERY (GET) – mở form lọc
        // URL: /Page/Query?p=FATP&c=QA
        // =========================
        [HttpGet]
        public ActionResult Query(string p, string c)
        {
            var parent = _menuService.GetParentByName(p);
            var child = _menuService.GetChildByNames(p, c);

            Session["MenuParentId"] = parent?.MenuID;
            Session["MenuParentName"] = parent?.MenuText ?? "";
            Session["MenuChildId"] = child?.MenuID;
            Session["MenuChildName"] = child?.MenuText ?? "";
            Session["ScreenName"] = child?.MenuText ?? "";

            var user = (Session["UserName"] as string) ?? "V5030587";

            var vm = new QueryViewModel
            {
                ParentName = p,
                ChildName = c,
                // có thể để rỗng hoặc nạp mặc định
                Results = _page.GetChangeRequestsByMenu(p, c, user)
            };

            ViewBag.Title = $"{p} → {c} (Query)";
            return View("Query", vm); // Views/Page/Query.cshtml
        }

        // =========================
        // QUERY (POST) – submit bộ lọc
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Query(QueryViewModel vm)
        {
            var user = (Session["UserName"] as string) ?? "V5030587";

            vm.Results = _page.QueryChangeRequests(
                vm.ParentName, vm.ChildName, user,
                vm.Category, vm.DocumentCode, vm.ChangeTitle, vm.Status
            );

            ViewBag.Title = $"{vm.ParentName} → {vm.ChildName}";
            return View("Query", vm);
        }

        // =========================
        // DOWNLOAD CSV từ bộ lọc hiện tại
        // =========================
        [HttpPost]
        public FileResult Download(QueryViewModel vm)
        {
            var user = (Session["UserName"] as string) ?? "V5030587";

            var rows = _page.QueryChangeRequests(
                vm.ParentName, vm.ChildName, user,
                vm.Category, vm.DocumentCode, vm.ChangeTitle, vm.Status
            ).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Code,Version,ChangeTitle,Department,Status");
            foreach (var r in rows)
                sb.AppendLine(string.Format("{0},{1},\"{2}\",{3},{4}",
                    r.Code, r.Version, r.ChangeTitle, r.Department, r.Status));

            return File(Encoding.UTF8.GetBytes(sb.ToString()),
                        "text/csv",
                        "change-requests.csv");
        }
    }
}
