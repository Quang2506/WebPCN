using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using Core.Dtos;
using Data.Repositories;
using Services;

namespace Web_PCN.Controllers
{
    public class WorkFlowController : Controller
    {
        private readonly WorkFlowService _WorkFlow = new WorkFlowService();

        public WorkFlowController()
        {
        }

        [HttpGet]
        public ActionResult Status(string changeRequestID, string dep_c)
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //string dep_c = (Session["Dept"] as string) ?? "";

            var data = _WorkFlow.GetJobStatusHistory(changeRequestID, dep_c);

            return View("JobStatus", data);
        }
    }
}
