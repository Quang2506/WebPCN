using System.Web.Mvc;
using Services;

namespace Web_PCN.Controllers
{
    public class HomeController : Controller
    {
        private readonly DashboardService _dashboard = new DashboardService();

        private string CurrentUser
        {
            get
            {
                var u = (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                        ? User.Identity.Name
                        : null;

                if (!string.IsNullOrWhiteSpace(u))
                {
                    Session["UserName"] = u;
                    return u;
                }

                u = Session["UserName"] as string;
                return string.IsNullOrWhiteSpace(u) ? "V5030587" : u; // default
            }
        }

        [HttpGet]
        public ActionResult Index(string plant)
        {
            // Ưu tiên query string; nếu trống dùng Session; nếu vẫn trống mặc định QSMC
            var currentPlant = !string.IsNullOrWhiteSpace(plant)
                                ? plant.ToUpperInvariant()
                                : (Session["Plant"] as string) ?? "QSMC";

            Session["Plant"] = currentPlant;

            var vm = _dashboard.GetDashboard(CurrentUser, currentPlant);
            ViewBag.Title = "PNC Home";
            return View(vm);  // Views/Home/Index.cshtml
        }
    }
}
