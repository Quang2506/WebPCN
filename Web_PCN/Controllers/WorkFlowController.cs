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
        //private readonly string _connectionString;
        private readonly WorkFlowRepository _WorkFlow = new WorkFlowRepository();

        public WorkFlowController()
        {
            //_connectionString = ConfigurationManager.ConnectionStrings["WebPCN"].ConnectionString;
        }

        [HttpGet]
        public ActionResult Status(string changeRequestID)
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string dep_c = (Session["Dept"] as string) ?? "";

            var statusHistory = _WorkFlow.GetJobStatusHistory(changeRequestID, dep_c);
            return View("JobStatus", statusHistory);
        }
    }
}
