using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;                  // FormsAuthentication
using Core.Dtos;
using Data.Repositories;
using Services;
using Web_PCN.Models.ViewModels;

namespace Web_PCN.Controllers
{
    [AllowAnonymous]
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
            // Nếu đã đăng nhập rồi thì về thẳng Home
            if (Request.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View("~/Views/Login/Login.cshtml", new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Login/Login.cshtml", vm);

            try
            {
                // Map ViewModel -> DTO
                var dto = new LoginRequest { Login = vm.Login, Password = vm.Password };

                var rs = await _authService.LoginAsync(dto);

                // Thất bại (theo quy ước của bạn: StatusCode != 0)
                if (rs == null || rs.StatusCode != 0)
                {
                    ModelState.AddModelError("", rs?.Message ?? "Login fail!");
                    return View("~/Views/Login/Login.cshtml", vm);
                }

                // ==== Đăng nhập thành công: set Session cho LeftMenu & các phần khác (GIỮ NGUYÊN) ====
                Session["UserName"] = rs.User_id;        // LeftMenu/_menuService.GetMenuTree(user) dùng cái này
                Session["DisplayName"] = rs.Fullname;
                Session["Email"] = rs.Email;
                Session["Dept"] = rs.dep_c;
                Session["GroupDept"] = rs.group_dept;
                Session["Permit"] = rs.permit?.ToString();
                Session["Role"] = rs.RoleName;
                Session["Site"] = rs.site;
                Session["Dep_nm"] = rs.dep_nm;
                Session["Factory"] = rs.factory;

                // ==== Đánh dấu đã đăng nhập (cookie xác thực) ====
                FormsAuthentication.SetAuthCookie(rs.User_id, vm.RememberMe);

                // Nếu có ReturnUrl hợp lệ -> chuyển sau hiệu ứng (vẫn giữ hiệu ứng)
                string nextUrl = Url.Action("Index", "Home");
                var returnUrl = Request.QueryString["ReturnUrl"];
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    nextUrl = returnUrl;

                // ===== MÀN HIỆU ỨNG THÀNH CÔNG (không mất dữ liệu cũ) =====
                ViewBag.NextUrl = nextUrl;
                ViewBag.UserName = rs.Fullname ?? rs.User_id ?? "User";
                return View("~/Views/Login/LoginSuccess.cshtml");
            }
            catch (Exception ex)
            {
                // Không để lộ lỗi thô ra UI; bạn có thể log nội bộ
                ModelState.AddModelError("", "Unexpected error. Please try again.");
                // TODO: log ex
                return View("~/Views/Login/Login.cshtml", vm);
            }
        }

        [HttpGet]
        [AllowAnonymous]
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
