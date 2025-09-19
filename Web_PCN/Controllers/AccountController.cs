using System.Web.Mvc;

namespace Web_PCN.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public ActionResult Login(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                ViewBag.Error = "Nhập tài khoản!";
                return View();
            }

            // TODO: nếu cần kiểm tra mật khẩu thì thêm ở đây
            Session["UserName"] = userName.Trim();

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
