using System.Web.Mvc;

namespace Web_PCN.Controllers
{
    public class ChangeRequestController : Controller
    {
        // GET: /ChangeRequest/ChangeRequestCreate
        [HttpGet]
        public ActionResult ChangeRequestCreate()
        {
            // Nếu view trùng tên action và nằm đúng thư mục Views/ChangeRequest/, chỉ cần return View();
            return View(); // sẽ tìm Views/ChangeRequest/ChangeRequestCreate.cshtml
        }

        // POST: /ChangeRequest/Create  (form trong view đang post tới action này)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FormCollection form) // hoặc ViewModel của bạn
        {
            // TODO: xử lý lưu
            // Sau khi lưu xong chuyển trang:
            return RedirectToAction("Index", "Home");
        }

        // (tuỳ chọn) GET: /ChangeRequest/ChangeRequestDetail
        [HttpGet]
        public ActionResult ChangeRequestDetail()
        {
            return View(); // Views/ChangeRequest/ChangeRequestDetail.cshtml
        }
    }
}
