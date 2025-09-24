using Core.Dtos;
using Data.Repositories;
using System.Collections.Generic;

namespace Services
{
    public class ChangeRequestService
    {
        private readonly ChangeRequestRepository _repo = new ChangeRequestRepository();

        public ChangeRequests RequestDetail(string requestid, string dep_c)
            => _repo.ViewDetailRequest(requestid, dep_c);

        public ReturnMessageResult ProcessAction(string requestid, string dep_c, string action, string user)
            => _repo.ProcessAction(requestid, dep_c, action, user);

        /* LUCAS CREATE REQUEST: overload mới truyền danh sách file (CSV) xuống SP */
        public ReturnMessageResult CreateNewRequestV2(
            string category, string changeTitle, string model, string documentCode,
            string version, string requestDetail, string groupDept, string user,
            short fileQty, string fileNamesCsv
        ) => _repo.CreateNewRequestV2(category, changeTitle, model, documentCode,
                                      version, requestDetail, groupDept, user,
                                      fileQty, fileNamesCsv);

        /* LUCAS CHANGE DETAIL: tiện ích đọc danh sách file từ RS2 của SP detail */
        public IEnumerable<ChangeRequestFileDto> GetRequestFiles_FromDetailSp(string requestid, string dep_c)
            => _repo.GetRequestFiles_FromDetailSp(requestid, dep_c);

        // API cũ vẫn giữ
        public ReturnMessageResult CreateNewRequest(string category, string ChangeTitle, string Model, string DocumentCode, string version, string Request_detail, string group_dept, string user, string jsonFileNames)
         => _repo.CreateNewRequest(category, ChangeTitle, Model, DocumentCode, version, Request_detail, group_dept, user, jsonFileNames);

        public ChangeRequests ViewRequest(string requestid)
          => _repo.ViewRequest(requestid);
    }
}
