using System.Web.Mvc;
using Services;

namespace Web_PCN.Controllers
{
    public class HomeController : Controller
    {
      
        private readonly MenuService _menuService = new MenuService();


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

        public ActionResult Index()
        {
            ViewBag.Title = "PNC Home";
            
          
            return View();
        }

        [ChildActionOnly]
        public PartialViewResult LeftMenu()
        {
            var tree = _menuService.GetMenuTree(CurrentUser); 
            return PartialView("_LeftMenu", tree);
        }
    }
}
