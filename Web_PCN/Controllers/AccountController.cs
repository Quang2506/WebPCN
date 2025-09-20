using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;                  // <-- cần cho FormsAuthentication
using Core.Dtos;
using Data.Repositories;
using Services;
using Web_PCN.Models.ViewModels;

namespace Web_PCN.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        // Nếu chưa dùng DI container:
        public AccountController() : this(new AuthService(new AuthRepository())) { }
        public AccountController(IAuthService authService) { _authService = authService; }

        // Cho phép truy cập Login ngay cả khi bạn dùng [Authorize] toàn cục
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
           
            return View("~/Views/Login/Login.cshtml", new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Login/Login.cshtml", vm);

            // Map ViewModel -> DTO
            var dto = new LoginRequest { Login = vm.Login, Password = vm.Password };

            var rs = await _authService.LoginAsync(dto);
            if (rs == null || rs.StatusCode != 0)
            {
                ModelState.AddModelError("", rs?.Message ?? "Login fail!");
                return View("~/Views/Login/Login.cshtml", vm);
            }

            // ==== Đăng nhập thành công: set Session cho LeftMenu ====
            Session["UserName"] = rs.User_id;        // LeftMenu/_menuService.GetMenuTree(user) đang dùng cái này
            Session["DisplayName"] = rs.Fullname;
            Session["Email"] = rs.Email;
            Session["Dept"] = rs.dep_c;
            Session["GroupDept"] = rs.group_dept;
            Session["Permit"] = rs.permit?.ToString();
            Session["Role"] = rs.RoleName;       // có thể null nếu SP không trả

            // ==== QUAN TRỌNG: đánh dấu đã đăng nhập (tránh bị redirect về Login) ====
            FormsAuthentication.SetAuthCookie(rs.User_id, vm.RememberMe);

            // Xử lý ReturnUrl nếu có
            var returnUrl = Request.QueryString["ReturnUrl"];
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            // Xoá cookie xác thực + session
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Login");
        }
    }
}