using System.Web.Mvc;
using Services;

namespace Web_PCN.Controllers
{
    [Authorize] // bắt buộc đã đăng nhập
    public class HomeController : Controller
    {
        private readonly MenuService _menuService = new MenuService();

        // Lấy username hiện tại: ưu tiên từ FormsAuth -> User.Identity.Name
        // fallback sang Session["UserName"] (phòng trường hợp cookieless / custom flows)
        private string CurrentUser
        {
            get
            {
                var u = (User?.Identity?.IsAuthenticated == true) ? User.Identity.Name : null;
                if (!string.IsNullOrWhiteSpace(u))
                {
                    // đồng bộ lại Session nếu cần
                    Session["UserName"] = u;
                    return u;
                }

                u = Session["UserName"] as string;
                return string.IsNullOrWhiteSpace(u) ? "Guest" : u;
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Title = "PCN Home";
            // Nếu muốn truyền luôn menu vào Home, có thể lấy ở đây
            // var tree = _menuService.GetMenuTree(CurrentUser);
            // return View(tree);

            return View(); // còn LeftMenu sẽ lấy tree riêng
        }

        [ChildActionOnly]
        public PartialViewResult LeftMenu()
        {
            var tree = _menuService.GetMenuTree(CurrentUser); // SP lọc theo user
            return PartialView("_LeftMenu", tree);
        }
    }
}
