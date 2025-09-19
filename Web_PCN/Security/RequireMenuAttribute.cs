using System.Web;
using System.Web.Mvc;
using Services;   // để gọi MenuService

namespace Web_PCN.Security
{
    /// <summary>
    /// Attribute check quyền theo tên menu
    /// </summary>
    public class RequireMenuAttribute : AuthorizeAttribute
    {
        private readonly string _menuName;
        public RequireMenuAttribute(string menuName) { _menuName = menuName; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var identity = httpContext.User?.Identity;
            if (identity == null || !identity.IsAuthenticated) return false;

            var userName = identity.Name;   // tên user đã login
            var svc = new MenuService();
            return svc.UserHasMenu(userName, _menuName);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Nếu chưa login thì chuyển về Login
            if (filterContext.HttpContext.User == null || !filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
            else
            {
                // Nếu đã login nhưng không có quyền → AccessDenied
                filterContext.Result = new RedirectResult("~/Account/AccessDenied");
            }
        }
    }
}
