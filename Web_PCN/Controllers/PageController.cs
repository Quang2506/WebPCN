using System.Linq;
using System.Text;
using System.Web.Mvc;
using Core.Dtos;
using Services;
namespace Web_PCN.Controllers
{
    public class PageController : Controller
    {
        private readonly ChangeRequestService _RequestService = new ChangeRequestService();
        private readonly MenuService _menuService = new MenuService();
        private readonly PageService _page = new PageService();
        // ===== Các màn khác giữ nguyên =====
        [HttpGet]
        public ActionResult MyApproval()
        {
            ViewBag.HeaderName = "My Approval";
            var user = (Session["UserName"] as string) ?? "V4050021";
            var MyPending = _RequestService.ViewMyPendingRequest(user);
            return PartialView("Pending", MyPending);
        }
        [HttpGet]
        public ActionResult MyChangeRequest()
        {
            ViewBag.HeaderName = "My Change Request";
            var user = (Session["UserName"] as string) ?? "V4050021";
            var MyChangeRequest = _RequestService.MyChangeRequest(user);
            return PartialView("MyChangeRequest", MyChangeRequest);
        }
        // =====================================================================
        // ===============        QUERY THEO PLANT + DEPCODE        =============
        // =====================================================================
        // GET: mở form, nạp Category, và hiển thị danh sách mặc định theo plant + dep
        [HttpGet]
        public ActionResult Query(string plant, string dep)
        {
            // chuẩn hóa
            var user = (Session["UserName"] as string) ?? "V4050021";
            plant = string.IsNullOrWhiteSpace(plant) ? "QSMC" : plant.ToUpperInvariant();
            dep = (dep ?? string.Empty).Trim();
            // ViewModel
            var vm = new QueryViewModel
            {
                Plant = plant,
                DepCode = dep,
                // breadcrumb chỉ để hiển thị; nếu muốn tra theo menu thì có thể
                // map dep -> tên menu ở service, nhưng không bắt buộc:
                ParentName = null,
                ChildName = null,
                ParentId = null,
                ChildId = null
            };
            // Dropdown Category theo dep
            vm.CategoryList = _page.GetCategoryListByDep(plant, dep);
            // Danh sách mặc định (giống Pending nhưng theo dep)
            vm.Results = _page.GetChangeRequestsByDep(plant, dep, user);
            ViewBag.Title = $"[{plant}] {dep} (Query)";
            return View("Query", vm);
        }
        // POST: lọc lại theo các trường — chỉ Category là dropdown, còn lại là text
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Query(QueryViewModel vm)
        {
            // giữ nguyên context
            var user = (Session["UserName"] as string) ?? "V4050021";
            vm.Plant = string.IsNullOrWhiteSpace(vm.Plant) ? "QSMC" : vm.Plant.ToUpperInvariant();
            vm.DepCode = (vm.DepCode ?? string.Empty).Trim();
            // Query theo dep + các filter người dùng nhập
            vm.Results = _page.QueryChangeRequestsByDep(
                vm.Plant,
                vm.DepCode,
                user,
                vm.Category,
                vm.DocumentCode,
                vm.ChangeTitle,
                vm.Status
            );
            // nạp lại dropdown Category theo dep
            vm.CategoryList = _page.GetCategoryListByDep(vm.Plant, vm.DepCode);
            ViewBag.Title = $"[{vm.Plant}] {vm.DepCode} (Query)";
            return View("Query", vm);
        }
        // DOWNLOAD CSV theo kết quả lọc plant + dep
        [HttpPost]
        public FileResult Download(QueryViewModel vm)
        {
            var user = (Session["UserName"] as string) ?? "V4050021";
            var rows = _page.QueryChangeRequestsByDep(
                vm.Plant,
                vm.DepCode,
                user,
                vm.Category,
                vm.DocumentCode,
                vm.ChangeTitle,
                vm.Status
            ).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("Code,Version,ChangeTitle,Department,Status");
            foreach (var r in rows)
                sb.AppendLine($"{r.Code},{r.Version},\"{r.ChangeTitle}\",{r.Department},{r.Status}");
            return File(
                Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                "change-requests.csv"
            );
        }
    }
}