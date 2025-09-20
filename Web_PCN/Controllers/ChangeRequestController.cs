using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web_PCN.Controllers
{
    [RoutePrefix("ChangeRequest")]
    public class ChangeRequestController : Controller
    {
        private readonly ChangeRequestService _RequestService = new ChangeRequestService();
        // GET: ChangeRequest
        public ActionResult ChangeRequestCreate()
        {
            return View("ChangeRequestCreate");
        }


        [HttpGet]
        [Route("{requestid}/{dep_c}", Name = "RequestDetails")]
        

        public ActionResult ChangeRequestDetail(string  RequestID, string dep_c)
        {

            var ChangeRequestdetail = _RequestService.RequestDetail(RequestID, dep_c); // SP sẽ lọc theo user
            return PartialView("ChangeRequestDetail", ChangeRequestdetail);
            //return View("ChangeRequestDetail");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ProcessAction")]
        public ActionResult ProcessAction(string requestId, string dep_c, string actionCode, string user)
        {
            var result = _RequestService.ProcessAction(requestId, dep_c, actionCode, user); // SP lọc theo user
            if (string.Equals(result, "OK", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Success"] = "Thực hiện thành công.";
            }
            else
            {
                TempData["Error"] = string.IsNullOrWhiteSpace(result) ? "Có lỗi xảy ra." : result;
            }

            // PRG: chuyển hướng để tránh double-submit và refresh giao diện
            return RedirectToAction("ChangeRequestDetail", new { RequestID = requestId, dep_c = dep_c });
        }
    }
}