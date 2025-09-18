using System.Web.Mvc;
using Services;
using Core.Dtos;

namespace Web_PCN.Controllers
{
    public class HomeController : Controller
    {
        private readonly MenuService _menuService = new MenuService();

        public ActionResult Index()
        {
            ViewBag.Title = "PCN Home";
            return View();
        }

        [ChildActionOnly]
        public PartialViewResult LeftMenu()
        {
            int userId = 1; // sau này lấy từ session
            var menu = _menuService.GetMenu(userId);
            return PartialView("_LeftMenu", menu);
        }
    }
}
