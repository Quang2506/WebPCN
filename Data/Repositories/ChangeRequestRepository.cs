using Core.Dtos;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Data.Repositories
{
    public class ChangeRequestRepository
    {
        /// <summary>
        /// Lấy danh sách TÊN FILE theo Request từ chính SP _PCN_GetRequestDetail (result set 2).
        /// </summary>
        public IEnumerable<ChangeRequestFileDto> GetRequestFiles_FromDetailSp(string requestid, string dep_c)
        {
            using (var conn = Db.GetConnection())
            using (var grid = conn.QueryMultiple(
                       "dbo._PCN_GetRequestDetail",
                       new { ChangeRequestID = requestid, dep_c = dep_c },
                       commandType: CommandType.StoredProcedure))
            {
                // RS1: header (bỏ nếu không cần)
                var _ = grid.Read<ChangeRequests>().FirstOrDefault();

                // RS2: 1 cột FileName
                var fileNames = grid.Read<string>().Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

                return fileNames.Select(fn => new ChangeRequestFileDto
                {
                    ChangeRequestID = requestid,
                    LinkFile = fn   // chỉ TÊN FILE
                }).ToList();
            }
        }

        /* ============================ LUCAS CREATE REQUEST ============================ */
        /// <summary>
        /// Gọi SP tạo mới request và truyền cả danh sách tên file (CSV qua @FileNames).
        /// VẪN truyền @fileQty để SP tương thích caller cũ.
        /// </summary>
        public ReturnMessageResult CreateNewRequestV2(
            string category, string changeTitle, string model, string documentCode,
            string version, string requestDetail, string groupDept, string user,
            short fileQty, string fileNamesCsv
        )
        {
            using (var conn = Db.GetConnection())
            {
                return conn.QueryFirstOrDefault<ReturnMessageResult>(
                    "dbo._PCN_CreateNewRequest",
                    new
                    {
                        category = category,
                        ChangeTitle = changeTitle,
                        Model = model,
                        DocumentCode = documentCode,
                        version = version,
                        Request_detail = requestDetail,
                        group_dept = groupDept,
                        user = user,
                        fileQty = fileQty,
                        /* LUCAS CREATE REQUEST */
                        FileNames = fileNamesCsv   // "a.pdf|b.docx"
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
        /* ============================================================================ */

        // Giữ nguyên các API cũ
        public ChangeRequests ViewDetailRequest(string requestid, string dep_c)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.QueryFirstOrDefault<ChangeRequests>(
                    "dbo._PCN_GetRequestDetail",
                    new { ChangeRequestID = requestid, dep_c = dep_c },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public ReturnMessageResult ProcessAction(string requestid, string dep_c, string action, string user)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.QueryFirstOrDefault<ReturnMessageResult>(
                    "dbo._PCN_ProcessAction",
                    new { ChangeRequestID = requestid, dep_c = dep_c, action = action, user = user },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public string CheckActionRigh(string requestid, string dep_c, string user)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.QueryFirstOrDefault<string>(
                    "dbo._PCN_CheckActionRight",
                    new { user = user, dep_c = dep_c, requestid = requestid },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public ReturnMessageResult CreateNewRequest(string category, string ChangeTitle, string Model, string DocumentCode, string version, string Request_detail, string group_dept, string user, string jsonFileNames)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.QueryFirstOrDefault<ReturnMessageResult>(
                    "dbo._PCN_CreateNewRequest",
                    new
                    {
                        category = category,
                        ChangeTitle = ChangeTitle,
                        Model = Model,
                        DocumentCode = DocumentCode,
                        version = version,
                        Request_detail = Request_detail,
                        group_dept = group_dept,
                        user = user,
                        JsonFileNames = jsonFileNames
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public ChangeRequests ViewRequest(string requestid)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.QueryFirstOrDefault<ChangeRequests>(
                    "dbo._PCN_GetRequest",
                    new { requestid = requestid },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
    }
}
