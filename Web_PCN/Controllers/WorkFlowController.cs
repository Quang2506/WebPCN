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
        public ActionResult Status(string changeRequestID, string changeTitle, string dep_c, string dep_nm, string site, string factory)
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //string dep_c = (Session["Dept"] as string) ?? "";

            var data = _WorkFlow.GetJobStatusHistory(changeRequestID, dep_c);

            ViewBag.ChangeRequestID = changeRequestID;
            ViewBag.ChangeRequestTitle = changeTitle;
            ViewBag.Dep_c = dep_c;
            ViewBag.Dep_nm = dep_nm;
            ViewBag.Site = site;
            ViewBag.Factory = factory;

            return View("JobStatus", data);
        }
    }
}
