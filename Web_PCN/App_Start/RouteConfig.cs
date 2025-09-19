using System.Web.Mvc;
using System.Web.Routing;

namespace Web_PCN
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Alias: /Login hoặc /Login/Logout -> AccountController
            routes.MapRoute(
                name: "LoginAlias",
                url: "Login/{action}",
                defaults: new { controller = "Account", action = "Login" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            );
        }
    }
}
