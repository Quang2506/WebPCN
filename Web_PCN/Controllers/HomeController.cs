using System.Web.Mvc;
using Services;

namespace Web_PCN.Controllers
{
    public class HomeController : Controller
    {
        private const string DefaultTestUser = "UserA";   // user cố định để test
        private readonly MenuService _menuService = new MenuService();

        
        private string GetCurrentUser()
        {
            var u = Session["UserName"] as string;
            if (string.IsNullOrWhiteSpace(u))
            {
                u = DefaultTestUser;
                Session["UserName"] = u; 
            }
            return u;
        }

        public ActionResult Index()
        {
            ViewBag.Title = "PNC Home";
            
            var user = GetCurrentUser(); 
            return View();
        }

        [ChildActionOnly]
        public PartialViewResult LeftMenu()
        {
            var user = GetCurrentUser(); // luôn có giá trị (UserA khi chưa login)
            var tree = _menuService.GetMenuTree(user); // SP sẽ lọc theo user
            return PartialView("_LeftMenu", tree);
        }
    }
}
