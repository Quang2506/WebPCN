using System.Web.Optimization;

namespace Web_PCN
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // jQuery: dùng file cụ thể. Đặt đúng file vào /Scripts (vd: jquery-3.7.1.min.js)
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-3.7.1.min.js"   // hoặc đổi lại đúng tên file bạn đang có
            ));

            // Modernizr (giữ nếu bạn đang dùng)
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"
            ));

            // Bootstrap 3: chỉ cần bootstrap.min.js (KHÔNG dùng bundle/esm)
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap3/bootstrap.min.js"
            ));

            // CSS: Bootstrap 3 + Site.css
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap3/css/bootstrap.min.css",
                "~/Content/Site.css"
            ));

            // Debug: để false cho dễ dò (khi Release có thể bật true)
            BundleTable.EnableOptimizations = false;
        }
    }
}
