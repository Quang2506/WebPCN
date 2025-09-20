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
    }
}