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
    [Authorize]
    [RoutePrefix("ChangeRequest")]
    public class ChangeRequestController : Controller
    {
        // ================================ SHARED (Services, Repo, Config, Helpers) ================================
        private readonly ChangeRequestService _RequestService = new ChangeRequestService();
        private readonly ChangeRequestService _svc = new ChangeRequestService();

        // Repo gọi SP _PCN_CheckActionRight (hàm trong repo: CheckActionRigh - thiếu 't' -> giữ nguyên)
        private readonly ChangeRequestRepository _repo = new ChangeRequestRepository();

        // Ưu tiên key FileBaseDir; nếu không có -> fallback ~/Documents
        //private static string FileBaseDir =>
        //    ConfigurationManager.AppSettings["FileBaseDir"]
        //    ?? WebConfigurationManager.AppSettings["FileBaseDir"];

        // ----- Helpers dùng chung -----
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

        // Luôn trả về list kiểu mạnh: [FileName, Token] để bind View (an toàn tên file qua token)
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

        // Quét cả FileBaseDir và ~/Documents (nếu 2 thư mục khác nhau)
        private List<string> GetSavedFilesFromDisk_AllPlaces(string requestId)
        {
            var result = new HashSet<string>(new OrdinalIgnoreCaseComparer());
            //string baseDir1 = !string.IsNullOrWhiteSpace(FileBaseDir) ? FileBaseDir : null;
            string baseDir2 = Server.MapPath("~/Documents");

            void scan(string dir)
            {
                if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir)) return;
                foreach (var p in Directory.EnumerateFiles(dir, requestId + "_*.*", SearchOption.TopDirectoryOnly))
                    result.Add(Path.GetFileName(p));
            }

            //scan(baseDir1);
            //if (!string.Equals(baseDir1?.TrimEnd('\\', '/'), baseDir2.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase))
            scan(baseDir2);

            return result.ToList();
        }

        protected static string TokenDecode(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            try
            {
                var b = HttpServerUtility.UrlTokenDecode(token);
                return b == null ? null : Encoding.UTF8.GetString(b);
            }
            catch { return null; }
        }

        // ================================ KHỐI 1: CREATE / EDIT (tạo & chỉnh sửa) ================================

        /* Open form Create (bản mới) */
        public ActionResult ChangeRequestCreate()
        {
            var m = new Core.Dtos.ChangeRequests
            {
                site = Session["Site"].ToString(),
                dep_nm = Session["Dep_nm"].ToString(),
                factory = Session["Factory"].ToString(),
                pms_i_usr = GetCurrentUser(),
                pms_i_ymd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:s"),
                group_dept = GetGroupDept()
            };
            ViewBag.AttFiles = new List<KeyValuePair<string, string>>();
            ViewBag.Status = 1; // coi bản mới là Draft -> mở khoá nhập liệu
            return View("ChangeRequestCreate", m);
        }

        /* Create / Save / SaveAgain / Submit */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNewRequest(Core.Dtos.ChangeRequests vm, string actionType, IEnumerable<HttpPostedFileBase> Files)
        {
            vm.pms_i_usr = GetCurrentUser();
            vm.pms_i_ymd = DateTime.Now.ToString("yyyyMMddHHmmss");
            vm.group_dept = GetGroupDept();

            var fileList = Files?.Where(f => f != null && f.ContentLength > 0).ToList()
                          ?? new List<HttpPostedFileBase>();
            var originalFileNames = fileList.Select(f => Path.GetFileName(f.FileName)).ToList();
            string jsonFileNames = Newtonsoft.Json.JsonConvert.SerializeObject(originalFileNames);

            // ✅ [sửa editing file] Chuẩn hoá actionType
            var act = (actionType ?? Request["actionType"] ?? "").Trim();
            var isNew = string.IsNullOrWhiteSpace(vm.requestid);
            if (string.IsNullOrEmpty(act))
                act = isNew ? "Save" : "SaveAgain";

            var requestId = vm.requestid;

            // ====================== SAVE (CREATE) ======================
            if (act.Equals("Save", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Trace.TraceInformation("[CreateNewRequest][Save] user={0} req={1} files={2}",
                    vm.pms_i_usr, vm.requestid ?? "(new)", originalFileNames.Count);

                // Tạo mới phải có ít nhất 1 file
                if (isNew && originalFileNames.Count == 0)
                {
                    ModelState.AddModelError("", "Please add the attached file.");
                    ViewBag.AttFiles = new List<KeyValuePair<string, string>>();
                    ViewBag.Status = 1;
                    return View("ChangeRequestCreate", vm);
                }

                var create = _RequestService.CreateNewRequest(
                    vm.category_nm, vm.ChangeTitle, vm.Model, vm.DocumentCode, vm.version,
                    vm.Request_detail, vm.group_dept, vm.pms_i_usr, jsonFileNames);

                if (create == null || !string.Equals(create.iResult, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Trace.TraceWarning("[CreateNewRequest][Save] FAIL: {0}", create?.iMessage);
                    ModelState.AddModelError(string.Empty, create?.iMessage ?? "Tạo mới thất bại.");
                    ViewBag.AttFiles = new List<KeyValuePair<string, string>>();
                    ViewBag.Status = 1;
                    return View("ChangeRequestCreate", vm);
                }

                requestId = create.RequestId;
                if (string.IsNullOrWhiteSpace(requestId))
                {
                    ModelState.AddModelError(string.Empty, "Tạo mới thất bại (không nhận được RequestId).");
                    ViewBag.Status = 1;
                    return View("ChangeRequestCreate", vm);
                }

                // Ghi file vật lý theo {requestId}_{rn}{ext}
                try
                {
                    var docs = Server.MapPath("~/Documents");
                    Directory.CreateDirectory(docs);

                    for (int i = 0; i < fileList.Count; i++)
                    {
                        var f = fileList[i];
                        var ext = Path.GetExtension(f.FileName) ?? "";
                        var targetName = $"{requestId}_{i + 1}{ext}";
                        var dest = Path.Combine(docs, targetName);

                        var tmp = Path.Combine(docs, Guid.NewGuid().ToString("N") + ext);
                        f.SaveAs(tmp);
                        if (System.IO.File.Exists(dest)) System.IO.File.Delete(dest);
                        System.IO.File.Move(tmp, dest);
                    }
                    System.Diagnostics.Trace.TraceInformation("[CreateNewRequest][Save] wrote {0} files", fileList.Count);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("[CreateNewRequest][Save] write files WARN: {0}", ex.Message);
                }

                TempData["SavedFiles"] = fileList.Select((f, i) => $"{requestId}_{i + 1}{Path.GetExtension(f.FileName)}").ToList();
                TempData["flash_ok"] = true;
                TempData["flash_msg"] = "Saved successfully.";
                System.Diagnostics.Trace.TraceInformation("[CreateNewRequest][Save] OK requestId={0}", requestId);
                return RedirectToAction(nameof(Edit), new { requestid = requestId });
            }

            // ====================== SAVE AGAIN (UPDATE + SYNC FILES) ======================
            if (act.Equals("SaveAgain", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(vm.requestid))
            {
                System.Diagnostics.Trace.TraceInformation("[CreateNewRequest][SaveAgain] user={0} req={1} files={2}",
                    vm.pms_i_usr, vm.requestid, originalFileNames.Count);

                // Kiểm tra tồn tại + trạng thái draft
                var status = _RequestService.GetRequestStatus(vm.requestid) ?? 0;
                if (status != 1)
                {
                    ModelState.AddModelError("", "Editing is locked by status.");
                    ViewBag.Status = status;
                    var detail = _RequestService.ViewRequest(vm.requestid);
                    return View("ChangeRequestCreate", detail ?? vm);
                }

                // Gọi SP UPDATE: đã tích hợp đồng bộ file theo JSON (xoá khác, thêm thiếu)
                var update = _RequestService.UpdateRequestViaCreateSp(
                    vm.requestid,
                    vm.category_nm, vm.ChangeTitle, vm.Model, vm.DocumentCode, vm.version,
                    vm.Request_detail, vm.group_dept, vm.pms_i_usr,
                    jsonFileNames
                );

                if (update == null || !string.Equals(update.iResult, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Trace.TraceWarning("[CreateNewRequest][SaveAgain] FAIL: {0}", update?.iMessage);
                    ModelState.AddModelError("", update?.iMessage ?? "Update failed.");
                    ViewBag.Status = status;
                    return View("ChangeRequestCreate", vm);
                }

                // Ghi các file upload mới theo {requestId}_{rn}{ext} (rn theo thứ tự fileList)
                try
                {
                    var docs = Server.MapPath("~/Documents");
                    Directory.CreateDirectory(docs);

                    for (int i = 0; i < fileList.Count; i++)
                    {
                        var f = fileList[i];
                        var ext = Path.GetExtension(f.FileName) ?? "";
                        var targetName = $"{vm.requestid}_{i + 1}{ext}";
                        var dest = Path.Combine(docs, targetName);

                        var tmp = Path.Combine(docs, Guid.NewGuid().ToString("N") + ext);
                        f.SaveAs(tmp);
                        if (System.IO.File.Exists(dest)) System.IO.File.Delete(dest);
                        System.IO.File.Move(tmp, dest);
                    }
                    System.Diagnostics.Trace.TraceInformation("[CreateNewRequest][SaveAgain] wrote {0} files", fileList.Count);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("[CreateNewRequest][SaveAgain] write files WARN: {0}", ex.Message);
                }

                TempData["flash_ok"] = true;
                TempData["flash_msg"] = "Saved again.";
                return RedirectToAction(nameof(Edit), new { requestid = vm.requestid });
            }

            // ====================== SUBMIT ======================
            if (act.Equals("Submit", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Trace.TraceInformation("[CreateNewRequest][Submit] user={0} req={1}", vm.pms_i_usr, requestId ?? "(n/a)");

                var submit = _RequestService.ProcessAction(requestId, vm.group_dept, "sendRequest", vm.pms_i_usr);
                if (submit == null || !string.Equals(submit.iResult, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Trace.TraceWarning("[CreateNewRequest][Submit] FAIL: {0}", submit?.iMessage);
                    ModelState.AddModelError(string.Empty, submit?.iMessage ?? "Submit không thành công.");
                    ViewBag.AttFiles = new List<KeyValuePair<string, string>>();
                    vm.requestid = requestId;
                    ViewBag.Status = _RequestService.GetRequestStatus(requestId) ?? 0;
                    return View("ChangeRequestCreate", vm);
                }

                TempData["flash_ok"] = true;
                TempData["flash_msg"] = "Submitted for approval successfully.";
                System.Diagnostics.Trace.TraceInformation("[CreateNewRequest][Submit] OK req={0}", requestId);
                return RedirectToAction(nameof(Edit), new { requestid = requestId });
            }

            // ====== Mặc định: thao tác không hợp lệ ======
            ModelState.AddModelError(string.Empty, "Thao tác không hợp lệ.");
            ViewBag.AttFiles = new List<KeyValuePair<string, string>>();
            ViewBag.Status = isNew ? 1 : (_RequestService.GetRequestStatus(vm.requestid) ?? 0);
            System.Diagnostics.Trace.TraceWarning("[CreateNewRequest] Invalid actionType={0}", actionType);
            return View("ChangeRequestCreate", vm);
        }

        /* Edit (mở lại form Create cho bản đã có) */
        [HttpGet]
        public ActionResult Edit(string requestid)
        {
            var vm = _RequestService.ViewRequest(requestid);
            if (vm == null)
            {
                TempData["Error"] = "Không tìm thấy yêu cầu vừa tạo.";
                vm = new Core.Dtos.ChangeRequests
                {
                    site = Session["Site"].ToString(),
                    dep_nm = Session["Dep_nm"].ToString(),
                    factory = Session["Factory"].ToString(),
                    pms_i_usr = GetCurrentUser(),
                    pms_i_ymd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:s"),
                    group_dept = GetGroupDept()
                };
                ViewBag.AttFiles = new List<KeyValuePair<string, string>>();
                ViewBag.Status = 0;
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

            // 2) Ổ đĩa (FileBaseDir + ~/Documents)
            var filesDisk = GetSavedFilesFromDisk_AllPlaces(requestid);

            // 3) TempData (từ lần save trước)
            var filesTemp = (TempData["SavedFiles"] as IEnumerable<string>)?.ToList() ?? new List<string>();

            // Hợp nhất
            var all = filesDb.Concat(filesDisk).Concat(filesTemp)
                             .Where(s => !string.IsNullOrWhiteSpace(s))
                             .Distinct(new OrdinalIgnoreCaseComparer())
                             .ToList();

            ViewBag.AttFiles = BuildFileListForView(all);
            ViewBag.Status = _RequestService.GetRequestStatus(requestid) ?? 0;
            return View("ChangeRequestCreate", vm);
        }

        // ================================ KHỐI 2: VIEW DETAIL (xem chi tiết) =====================================

        /* Detail: lấy quyền & file, render view */
        [HttpGet]
        [Route("{requestid}/{dep_c}", Name = "RequestDetails")]
        public ActionResult ChangeRequestDetail(string requestid, string dep_c)
        {
            // 1) Dữ liệu detail
            var detail = _RequestService.RequestDetail(requestid, dep_c);

            // 2) Quyền thực hiện action (SP _PCN_CheckActionRight)
            string user = GetCurrentUser();
            string right = _repo.CheckActionRigh(requestid, dep_c, user) ?? string.Empty; // "Approve_Reject" | "Sync" | "fail" | ...

            ViewBag.ActionRight = right;
            ViewBag.CanApproveReject = string.Equals(right, "Approve_Reject", StringComparison.OrdinalIgnoreCase);
            ViewBag.CanSync = string.Equals(right, "Sync", StringComparison.OrdinalIgnoreCase);

            // 3) Files trên ổ đĩa ~/Documents (hiển thị cho detail)
            var docRoot = Server.MapPath("~/Documents");
            var filesOnDisk = new List<string>();
            if (!string.IsNullOrWhiteSpace(requestid) && Directory.Exists(docRoot))
            {
                filesOnDisk = Directory.EnumerateFiles(docRoot, requestid + "_*.*", SearchOption.TopDirectoryOnly)
                                       .Select(Path.GetFileName)
                                       .Where(n => !string.IsNullOrWhiteSpace(n))
                                       .ToList();
            }
            ViewBag.Files = BuildFileListForView(filesOnDisk);

            return PartialView("ChangeRequestDetail", detail);
        }

        /* ViewMyChangeRequest: chuyển thẳng sang Edit */
        [HttpGet]
        [Route("{requestid}")]
        public ActionResult ViewMyChangeRequest(string requestid)
        {
            return RedirectToAction(nameof(Edit), new { requestid });
        }

        // ================================ FILES & ACTIONS (phục vụ cả 2 khối) ====================================

        /* Lưu file lên đĩa (ưu tiên FileBaseDir) */
        private void SaveUploadedFilesToDisk(string requestId, List<HttpPostedFileBase> fileList)
        {
            if (string.IsNullOrWhiteSpace(requestId) || fileList == null || fileList.Count == 0)
                return;

            // Ưu tiên cấu hình FileBaseDir, fallback ~/Documents
            var uploadRoot = Server.MapPath("~/Documents");

            // Đảm bảo thư mục tồn tại
            Directory.CreateDirectory(uploadRoot);

            // Tìm chỉ số tiếp theo dựa trên các file đã có {requestId}_*.* (an toàn khi lưu nhiều đợt)
            int index = NextFileIndexOnDisk(uploadRoot, requestId);

            foreach (var file in fileList.Where(f => f != null && f.ContentLength > 0))
            {
                var ext = Path.GetExtension(file.FileName); // giữ đuôi gốc
                var uniqueName = $"{requestId}_{index}{ext}";
                var fullPath = Path.Combine(uploadRoot, uniqueName);
                file.SaveAs(fullPath);
                index++;
            }
        }

        // Tính index tiếp theo cho tên file {requestId}_{index}.ext
        private int NextFileIndexOnDisk(string folder, string requestId)
        {
            int max = 0;
            if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
            {
                var prefix = requestId + "_";
                foreach (var p in Directory.EnumerateFiles(folder, requestId + "_*.*", SearchOption.TopDirectoryOnly))
                {
                    var nameNoExt = Path.GetFileNameWithoutExtension(p); // ví dụ: REQ123_5
                    if (nameNoExt != null && nameNoExt.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        var suffix = nameNoExt.Substring(prefix.Length); // "5"
                        if (int.TryParse(suffix, out var n))
                            max = Math.Max(max, n);
                    }
                }
            }
            return max + 1;
        }

        /* Serve file (view/download) */
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

            // Nếu DB không biết tên file, vẫn cho phép nếu tên đúng pattern {requestId}_*
            if (fileNames.Count == 0 || !fileNames.Contains(decodedName))
            {
                if (!decodedName.StartsWith(requestId + "_", StringComparison.OrdinalIgnoreCase))
                    return new HttpStatusCodeResult(403, "File does not belong to this request");
            }

            // Ưu tiên FileBaseDir, fallback ~/Documents
            var baseDir = Server.MapPath("~/Documents");
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

        /* ProcessAction: map giá trị UI → SP */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ProcessAction")]
        public ActionResult ProcessAction(string requestId, string dep_c, string actionCode, string user)
        {
            // Ưu tiên user đang đăng nhập
            user = GetCurrentUser();

            // Chuẩn hoá action cho SP _PCN_ProcessAction
            string actionForSp;
            switch ((actionCode ?? "").Trim().ToUpperInvariant())
            {
                case "APPROVE": actionForSp = "Approve"; break;
                case "REJECT": actionForSp = "Reject"; break;
                case "SYNC": actionForSp = "Sync"; break;
                // Các action khác nếu sau này bổ sung:
                case "SENDREQUEST": actionForSp = "sendRequest"; break;
                case "PENDING_SYNC": actionForSp = "Pending Sync"; break;
                default:
                    TempData["Error"] = "Action is not available!";
                    return RedirectToAction("ChangeRequestDetail", new { requestid = requestId, dep_c });
            }

            var result = _RequestService.ProcessAction(requestId, dep_c, actionForSp, user);
            if (string.Equals(result.iResult, "OK", StringComparison.OrdinalIgnoreCase))
                TempData["Success"] = result.iMessage ?? "Thực hiện thành công.";
            else
                TempData["Error"] = result.iMessage ?? "Thực hiện thất bại.";

            return RedirectToAction("ChangeRequestDetail", new { requestid = requestId, dep_c });
        }


        // ================================ DELETE (gọi SP _PCN_CreateNewRequest @Action='DELETE') ================================
        [HttpGet, Route("Delete")]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Error"] = "Invalid request id.";
                return RedirectToAction(nameof(ChangeRequestCreate));
            }

            // (có thể bỏ pre-check; SP vẫn kiểm tra status==1)
            var st = _RequestService.GetRequestStatus(id) ?? 0;
            if (st != 1)
            {
                TempData["Error"] = "Only draft can be deleted.";
                return RedirectToAction(nameof(Edit), new { requestid = id });
            }

            var res = _RequestService.DeleteViaCreateSp(id, GetCurrentUser());
            if (!string.Equals(res?.iResult, "OK", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = res?.iMessage ?? "Delete failed.";
                return RedirectToAction(nameof(Edit), new { requestid = id });
            }

            // Xoá file vật lý sau khi DB xoá thành công
            try
            {
                var dir = Server.MapPath("~/Documents");
                if (Directory.Exists(dir))
                {
                    foreach (var p in Directory.EnumerateFiles(dir, id + "_*.*", SearchOption.TopDirectoryOnly))
                        System.IO.File.Delete(p);
                }
            }
            catch { /* ignore IO errors */ }

            TempData["Success"] = "Deleted successfully.";
            return RedirectToAction(nameof(ChangeRequestCreate));
        }

    }
}
