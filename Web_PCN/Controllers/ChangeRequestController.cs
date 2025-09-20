using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Core.Dtos; // ChangeRequests

namespace Web_PCN.Controllers
{
    public class ChangeRequestController : Controller
    {
        // ===== Helpers (mock) =====
        private void BindCommonDropdowns(string selectedModel = null)
        {
            var models = new List<SelectListItem>
            {
                new SelectListItem { Value = "bug",     Text = "Sửa lỗi" },
                new SelectListItem { Value = "feature", Text = "Tính năng mới" },
                new SelectListItem { Value = "improve", Text = "Cải tiến" }
            };
            ViewBag.ModelList = new SelectList(models, "Value", "Text", selectedModel);
        }

        private ChangeRequests NewMockModel()
        {
            return new ChangeRequests
            {
                Id = 0,
                DocumentCode = "",          // để placeholder hiển thị
                version = "",
                ChangeTitle = "",
                Model = "",
                group_dept = "R&D",
                pms_i_usr = User?.Identity?.Name ?? "System",
                pms_i_ymd = DateTime.Now.ToString("yyyy-MM-dd"),
                Status = "new",
                Request_detail = ""
            };
        }

        // ===== CREATE (UI only) =====
        [HttpGet]
        public ActionResult ChangeRequestCreate()
        {
            ViewBag.HideLeftMenu = true;
            var m = NewMockModel();
            BindCommonDropdowns(m.Model);
            return View("ChangeRequestCreate", m);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(ChangeRequests dto, IEnumerable<HttpPostedFileBase> Files)
        {
            // Không lưu DB. Chỉ để demo UI:
            TempData["ok"] = "Demo: đã 'giả' lưu (không ghi DB).";
            BindCommonDropdowns(dto?.Model);
            ModelState.Clear(); // để input trống hiển thị lại gọn gàng
            return View("ChangeRequestCreate", dto ?? NewMockModel());
        }

        // ===== DETAIL (UI only) =====
        [HttpGet]
        public ActionResult ChangeRequestDetail(int? id)
        {
            ViewBag.HideLeftMenu = true;
            // Bất kể id, trả model giả để xem UI
            var m = NewMockModel();
            m.Id = id ?? 0;
            m.DocumentCode = "CR-2025-001";
            m.version = "v1.0";
            m.ChangeTitle = "Demo Change Title";
            m.Model = "feature";
            m.Status = "new";
            m.Request_detail = "Nội dung mô tả demo…";

            BindCommonDropdowns(m.Model);
            return View("ChangeRequestDetail", m);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Update(ChangeRequests dto, IEnumerable<HttpPostedFileBase> Files)
        {
            // Không cập nhật DB. Chỉ hiển thị lại UI:
            TempData["ok"] = "Demo: đã 'giả' cập nhật (không ghi DB).";
            BindCommonDropdowns(dto?.Model);
            return View("ChangeRequestDetail", dto ?? NewMockModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SubmitForApproval(int id)
        {
            TempData["ok"] = "Demo: đã 'giả' gửi duyệt.";
            return RedirectToAction("ChangeRequestDetail", new { id });
        }
    }
}
