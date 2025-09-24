using Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Core.Dtos;
using Data.Repositories;
using Newtonsoft.Json;

namespace Web_PCN.Controllers
{
    [RoutePrefix("ChangeRequest")]
    public class ChangeRequestController : Controller
    {
        private readonly ChangeRequestService _RequestService = new ChangeRequestService();
        private readonly ChangeRequestService _svc = new ChangeRequestService();

        // Thư mục lưu file (web.config: <add key="FileBaseDir" value="D:\PCNFiles" />)
        private static string FileBaseDir =>
            ConfigurationManager.AppSettings["FileBaseDir"]
            ?? WebConfigurationManager.AppSettings["FileBaseDir"];

        /* =============== Helpers =============== */
        private string GetCurrentUser()
        {
            var u = Session["UserName"] as string;
            if (string.IsNullOrWhiteSpace(u)) u = User != null ? User.Identity.Name : null;
            return string.IsNullOrWhiteSpace(u) ? "UNKNOWN" : u;
        }

        private string GetGroupDept()
        {
            var g = Session["GroupDept"] as string;
            return string.IsNullOrWhiteSpace(g) ? "" : g.Trim();
        }

        // Luôn trả về list kiểu mạnh: [FileName, Token]
        private static List<KeyValuePair<string, string>> BuildFileListForView(IEnumerable<string> fileNames)
        {
            var list = new List<KeyValuePair<string, string>>();
            foreach (var raw in fileNames.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                var clean = Path.GetFileName(raw);
                var token = HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(clean));
                list.Add(new KeyValuePair<string, string>(clean, token));
            }
            return list;
        }

        private sealed class OrdinalIgnoreCaseComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y) =>
                string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
            public int GetHashCode(string obj) => (obj ?? "").ToUpperInvariant().GetHashCode();
        }

        // Quét cả FileBaseDir và ~/Documents (nếu khác nhau)
        private List<string> GetSavedFilesFromDisk_AllPlaces(string requestId)
        {
            var result = new HashSet<string>(new OrdinalIgnoreCaseComparer());
            string baseDir1 = !string.IsNullOrWhiteSpace(FileBaseDir) ? FileBaseDir : null;
            string baseDir2 = Server.MapPath("~/Documents");

            void scan(string dir)
            {
                if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir)) return;
                foreach (var p in Directory.EnumerateFiles(dir, requestId + "_*.*", SearchOption.TopDirectoryOnly))
                    result.Add(Path.GetFileName(p));
            }

            scan(baseDir1);
            if (!string.Equals(baseDir1?.TrimEnd('\\', '/'), baseDir2.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase))
                scan(baseDir2);

            return result.ToList();
        }

        /* =============== Open Create =============== */
        public ActionResult ChangeRequestCreate()
        {
            var m = new Core.Dtos.ChangeRequests
            {
                pms_i_usr = GetCurrentUser(),
                pms_i_ymd = DateTime.Now.ToString("yyyyMMddHHmmss"),
                group_dept = GetGroupDept()                 // <-- tự lấy từ Session
            };
            ViewBag.AttFiles = new List<KeyValuePair<string, string>>(); // chưa có file
            return View("ChangeRequestCreate", m);
        }

        /* =============== Detail (giữ nguyên) =============== */
        [HttpGet]
        [Route("{requestid}/{dep_c}", Name = "RequestDetails")]
        public ActionResult ChangeRequestDetail(string RequestID, string dep_c)
        {
            var ChangeRequestdetail = _RequestService.RequestDetail(RequestID, dep_c);
            var files = _svc.GetRequestFiles_FromDetailSp(RequestID, dep_c)
                            .Select(f => Path.GetFileName(f.LinkFile ?? string.Empty))
                            .ToList();
            ViewBag.Files = BuildFileListForView(files);
            return PartialView("ChangeRequestDetail", ChangeRequestdetail);
        }

        /* =============== ProcessAction (giữ nguyên) =============== */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ProcessAction")]
        public ActionResult ProcessAction(string requestId, string dep_c, string actionCode, string user)
        {
            var result = _RequestService.ProcessAction(requestId, dep_c, actionCode, user);
            if (string.Equals(result.iResult, "OK", StringComparison.OrdinalIgnoreCase))
                TempData["Success"] = "Thực hiện thành công.";
            else
                TempData["Error"] = result.iMessage;

            return RedirectToAction("ChangeRequestDetail", new { RequestID = requestId, dep_c = dep_c });
        }

        /* =============== Create / Save / Submit =============== */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNewRequest(Core.Dtos.ChangeRequests vm, string actionType, IEnumerable<HttpPostedFileBase> Files)
        {
            // Server authoritative
            vm.pms_i_usr = GetCurrentUser();
            vm.pms_i_ymd = DateTime.Now.ToString("yyyyMMddHHmmss");
            vm.group_dept = GetGroupDept();                 // <-- chốt theo Session, khóa field

            // Files từ trình duyệt
            var fileList = Files?.Where(f => f != null && f.ContentLength > 0).ToList()
                          ?? new List<HttpPostedFileBase>();
            var originalFileNames = fileList.Select(f => Path.GetFileName(f.FileName)).ToList();
            string jsonFileNames = JsonConvert.SerializeObject(originalFileNames);

            var requestId = !string.IsNullOrWhiteSpace(vm.requestid) ? vm.requestid : vm.requestid;

            if (actionType.Equals("Save", StringComparison.OrdinalIgnoreCase))
            {
                if (originalFileNames.Count == 0)
                {
                    ModelState.AddModelError("", "Please add the attached file.");
                    ViewBag.AttFiles = new List<KeyValuePair<string, string>>();
                    return View("ChangeRequestCreate", vm);
                }

                var create = _RequestService.CreateNewRequest(
                    vm.category_nm, vm.ChangeTitle, vm.Model, vm.DocumentCode, vm.version,
                    vm.Request_detail, vm.group_dept, vm.pms_i_usr, jsonFileNames);

                if (create == null)
                {
                    ModelState.AddModelError(string.Empty, "Không nhận được phản hồi từ hệ thống.");
                    ViewBag.AttFiles = new List<KeyValuePair<string, string>>();
                    return View("ChangeRequestCreate", vm);
                }
                if (!string.Equals(create.iResult, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty, create.iMessage ?? "Tạo mới thất bại.");
                    ViewBag.AttFiles = new List<KeyValuePair<string, string>>();
                    return View("ChangeRequestCreate", vm);
                }

                // id thực tế
                requestId = !string.IsNullOrEmpty(create.RequestId) ? create.RequestId : requestId;

                // Lưu file vật lý
                var uploadRoot = !string.IsNullOrWhiteSpace(FileBaseDir) ? FileBaseDir : Server.MapPath("~/Documents");
                Directory.CreateDirectory(uploadRoot);

                var saved = new List<string>();
                int index = 1;
                foreach (var file in fileList)
                {
                    var ext = Path.GetExtension(file.FileName);
                    var uniqueName = $"{requestId}_{index}{ext}";
                    var fullPath = Path.Combine(uploadRoot, uniqueName);
                    file.SaveAs(fullPath);
                    saved.Add(uniqueName);
                    index++;
                }

                ViewBag.SavedFiles = saved;
                TempData["SavedFiles"] = saved;
                TempData["flash_ok"] = true;
                TempData["flash_msg"] = "Saved successfully.";
                return RedirectToAction(nameof(Edit), new { requestid = requestId });
            }

            if (actionType.Equals("Submit", StringComparison.OrdinalIgnoreCase))
            {
                var submit = _RequestService.ProcessAction(requestId, "", "sendRequest", vm.pms_i_usr);
                if (submit == null || !string.Equals(submit.iResult, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty, submit?.iMessage ?? "Submit không thành công.");
                    ViewBag.AttFiles = new List<KeyValuePair<string, string>>();
                    vm.requestid = requestId;
                    return View("ChangeRequestCreate", vm);
                }

                TempData["flash_ok"] = true;
                TempData["flash_msg"] = "Submitted for approval successfully.";
                return RedirectToAction(nameof(Edit), new { requestid = requestId });
            }

            ModelState.AddModelError(string.Empty, "Thao tác không hợp lệ.");
            ViewBag.AttFiles = new List<KeyValuePair<string, string>>();
            return View("ChangeRequestCreate", vm);
        }

        [HttpGet]
        public ActionResult Edit(string requestid)
        {
            var vm = _RequestService.ViewRequest(requestid);
            if (vm == null)
            {
                TempData["Error"] = "Không tìm thấy yêu cầu vừa tạo.";
                vm = new Core.Dtos.ChangeRequests
                {
                    pms_i_usr = GetCurrentUser(),
                    pms_i_ymd = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    group_dept = GetGroupDept()
                };
                ViewBag.AttFiles = new List<KeyValuePair<string, string>>();
                return View("ChangeRequestCreate", vm);
            }

            vm.requestid = string.IsNullOrWhiteSpace(vm.requestid) ? requestid : vm.requestid;
            if (string.IsNullOrWhiteSpace(vm.group_dept))
                vm.group_dept = GetGroupDept();

            // 1) DB (không lọc dep_c)
            var filesDb = _svc.GetRequestFiles_FromDetailSp(requestid, "")
                              .Select(f => Path.GetFileName(f.LinkFile ?? string.Empty))
                              .Where(s => !string.IsNullOrWhiteSpace(s))
                              .ToList();

            // 2) Ổ đĩa
            var filesDisk = GetSavedFilesFromDisk_AllPlaces(requestid);

            // 3) TempData
            var filesTemp = (TempData["SavedFiles"] as IEnumerable<string>)?.ToList() ?? new List<string>();

            // Union
            var all = filesDb.Concat(filesDisk).Concat(filesTemp)
                             .Where(s => !string.IsNullOrWhiteSpace(s))
                             .Distinct(new OrdinalIgnoreCaseComparer())
                             .ToList();

            ViewBag.AttFiles = BuildFileListForView(all);
            return View("ChangeRequestCreate", vm);
        }

        /* =============== Serve file (view/download) =============== */

        protected static string TokenDecode(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            try { var b = HttpServerUtility.UrlTokenDecode(token); return b == null ? null : Encoding.UTF8.GetString(b); }
            catch { return null; }
        }

        [HttpGet, Route("file/view")]
        public ActionResult ViewInline(string requestId, string p)
        {
            var vr = ResolvePathFromDb_ByFileName(requestId, p, out var fullPath, out var fileName, out var mime);
            if (vr != null) return vr;

            Response.AppendHeader("Content-Disposition",
                new System.Net.Mime.ContentDisposition { Inline = true, FileName = fileName }.ToString());
            return File(fullPath, mime);
        }

        [HttpGet, Route("file/download")]
        public ActionResult Download(string requestId, string p)
        {
            var vr = ResolvePathFromDb_ByFileName(requestId, p, out var fullPath, out var fileName, out var mime);
            if (vr != null) return vr;
            return File(fullPath, mime, fileName);
        }

        protected ActionResult ResolvePathFromDb_ByFileName(
            string requestId, string token,
            out string fullPath, out string fileName, out string mime)
        {
            fullPath = fileName = mime = null;

            if (string.IsNullOrWhiteSpace(requestId) || string.IsNullOrWhiteSpace(token))
                return new HttpStatusCodeResult(400, "Invalid parameters");

            var fileNames = _svc.GetRequestFiles_FromDetailSp(requestId, dep_c: "")
                                .Select(f => Path.GetFileName(f.LinkFile ?? string.Empty))
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var decodedName = TokenDecode(token);
            decodedName = Path.GetFileName(decodedName ?? "");
            if (string.IsNullOrWhiteSpace(decodedName))
                return new HttpStatusCodeResult(400, "Invalid token");

            if (fileNames.Count == 0 || !fileNames.Contains(decodedName))
            {
                if (!decodedName.StartsWith(requestId + "_", StringComparison.OrdinalIgnoreCase))
                    return new HttpStatusCodeResult(403, "File does not belong to this request");
            }

            var baseDir = !string.IsNullOrWhiteSpace(FileBaseDir) ? FileBaseDir : Server.MapPath("~/Documents");
            var full = Path.Combine(baseDir, decodedName);
            if (!System.IO.File.Exists(full))
            {
                var alt = Server.MapPath("~/Documents");
                if (!string.Equals(baseDir.TrimEnd('\\', '/'), alt.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase))
                {
                    var altPath = Path.Combine(alt, decodedName);
                    if (System.IO.File.Exists(altPath)) full = altPath;
                }
            }

            if (!System.IO.File.Exists(full))
                return HttpNotFound("File not found");

            fullPath = full;
            fileName = decodedName;
            mime = System.Web.MimeMapping.GetMimeMapping(fileName);
            return null; // OK
        }
    }
}
