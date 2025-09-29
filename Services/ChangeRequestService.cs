using Core.Dtos;
using Data.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    public class ChangeRequestService
    {
        private readonly ChangeRequestRepository _repo = new ChangeRequestRepository();

        /* ===================== Detail & Action ===================== */

        public ChangeRequests RequestDetail(string requestid, string dep_c)
            => _repo.ViewDetailRequest(requestid, dep_c);

        public ReturnMessageResult ProcessAction(string requestid, string dep_c, string action, string user)
            => _repo.ProcessAction(requestid, dep_c, action, user);

        /* ===================== Create (các biến thể) ===================== */

        // API cũ: truyền json danh sách file (giữ tương thích)
        public ReturnMessageResult CreateNewRequest(
            string category, string ChangeTitle, string Model, string DocumentCode,
            string version, string Request_detail, string group_dept, string user,
            string jsonFileNames
        ) => _repo.CreateNewRequest(category, ChangeTitle, Model, DocumentCode,
                                    version, Request_detail, group_dept, user, jsonFileNames);

        // API mới (V2): truyền số lượng + CSV tên file xuống SP
        public ReturnMessageResult CreateNewRequestV2(
            string category, string changeTitle, string model, string documentCode,
            string version, string requestDetail, string groupDept, string user,
            short fileQty, string fileNamesCsv
        ) => _repo.CreateNewRequestV2(category, changeTitle, model, documentCode,
                                      version, requestDetail, groupDept, user,
                                      fileQty, fileNamesCsv);

        // Cập nhật bản ghi qua cùng SP create (dành cho SaveAgain trên bản nháp)
        public ReturnMessageResult UpdateRequestViaCreateSp(
            string requestid,
            string category, string changeTitle, string model, string documentCode,
            string version, string requestDetail, string groupDept, string user,
            string jsonFileNames
        ) => _repo.UpdateRequestViaCreateSp(requestid, category, changeTitle, model,
                                            documentCode, version, requestDetail, groupDept,
                                            user, jsonFileNames);

        /* ===================== Read helpers ===================== */

        // Lấy DS file từ RS2 của SP detail
        public IEnumerable<ChangeRequestFileDto> GetRequestFiles_FromDetailSp(string requestid, string dep_c)
            => _repo.GetRequestFiles_FromDetailSp(requestid, dep_c);

        public ChangeRequests ViewRequest(string requestid)
            => _repo.ViewRequest(requestid);

        // Danh sách pending theo user
        public IEnumerable<ChangeRequests> ViewMyPendingRequest(string user)
            => _repo.ViewMyPendingRequest(user);

        // Danh sách yêu cầu của tôi (bản mở rộng)
        public IEnumerable<ChangeRequests> MyChangeRequest(string user)
            => _repo.MyChangeRequest(user);

        // Lấy trạng thái hiện tại (để controller không cần dựa vào DTO.status)
        public int? GetRequestStatus(string requestid)
            => _repo.GetRequestStatus(requestid);

        public ReturnMessageResult DeleteViaCreateSp(string requestid, string user)
    => _repo.DeleteViaCreateSp(requestid, user);



    }
}
