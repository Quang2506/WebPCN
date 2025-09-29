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
        /// RS1: header (bỏ qua); RS2: danh sách tên file (1 cột).
        /// </summary>
        public IEnumerable<ChangeRequestFileDto> GetRequestFiles_FromDetailSp(string requestid, string dep_c)
        {
            using (var conn = Db.GetConnection())
            using (var grid = conn.QueryMultiple(
                       "dbo._PCN_GetRequestDetail",
                       new { ChangeRequestID = requestid, dep_c = dep_c },
                       commandType: CommandType.StoredProcedure))
            {
                // RS1: header (không dùng)
                var _ = grid.Read<ChangeRequests>().FirstOrDefault();

                // RS2: 1 cột FileName
                var fileNames = grid.Read<string>()
                                    .Where(s => !string.IsNullOrWhiteSpace(s))
                                    .ToList();

                return fileNames.Select(fn => new ChangeRequestFileDto
                {
                    ChangeRequestID = requestid,
                    LinkFile = fn   // chỉ TÊN FILE
                }).ToList();
            }
        }

        /* ============================ CREATE REQUEST ============================ */

        /// <summary>
        /// V2: Truyền số lượng file + CSV tên file xuống SP (mô hình không có tham số Action).
        /// Ví dụ CSV: "a.pdf|b.docx"
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
                        FileNames = fileNamesCsv   // "a.pdf|b.docx"
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        /// <summary>
        /// API cũ: truyền JSON danh sách file; SP yêu cầu tham số Action='CREATE'.
        /// </summary>
        public ReturnMessageResult CreateNewRequest(
            string category, string ChangeTitle, string Model, string DocumentCode,
            string version, string Request_detail, string group_dept, string user,
            string jsonFileNames
        )
        {
            using (var conn = Db.GetConnection())
            {
                return conn.QueryFirstOrDefault<ReturnMessageResult>(
                    "dbo._PCN_CreateNewRequest",
                    new
                    {
                        Action = "CREATE",
                        requestid = (string)null,
                        category = category,
                        ChangeTitle = ChangeTitle,
                        Model = Model,
                        DocumentCode = DocumentCode,
                        version = version,
                        Request_detail = Request_detail,
                        group_dept = group_dept,
                        user = user,
                        JsonFileNames = jsonFileNames,
                        FileNames = (string)null
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        /// <summary>
        /// Update qua cùng SP với Action='UPDATE' (phục vụ SaveAgain trên bản nháp).
        /// </summary>
        public ReturnMessageResult UpdateRequestViaCreateSp(
            string requestid,
            string category, string changeTitle, string model, string documentCode,
            string version, string requestDetail, string groupDept, string user,
            string jsonFileNames
        )
        {
            using (var conn = Db.GetConnection())
            {
                return conn.QueryFirstOrDefault<ReturnMessageResult>(
                    "dbo._PCN_CreateNewRequest",
                    new
                    {
                        Action = "UPDATE",
                        requestid = requestid,
                        category = category,
                        ChangeTitle = changeTitle,
                        Model = model,
                        DocumentCode = documentCode,
                        version = version,
                        Request_detail = requestDetail,
                        group_dept = groupDept,
                        user = user,
                        JsonFileNames = jsonFileNames,
                        FileNames = (string)null
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        /* ============================ READ / ACTION ============================ */

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

        /// <summary>
        /// Gọi dbo._PCN_CheckActionRight và đọc OUTPUT @checkRight.
        /// Trả về: "Approve_Reject" | "Sync" | "fail" | ... (theo SP).
        /// </summary>
        public string CheckActionRight(string requestid, string dep_c, string user)
        {
            using (var conn = Db.GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@user", user, DbType.String, ParameterDirection.Input, size: 50);
                p.Add("@dep_c", dep_c, DbType.String, ParameterDirection.Input, size: 50);
                p.Add("@requestid", requestid, DbType.String, ParameterDirection.Input, size: 100);
                p.Add("@checkRight", dbType: DbType.String, size: 100, direction: ParameterDirection.Output);

                conn.Execute("dbo._PCN_CheckActionRight", p, commandType: CommandType.StoredProcedure);

                var right = p.Get<string>("@checkRight");
                return string.IsNullOrWhiteSpace(right) ? "fail" : right.Trim();
            }
        }

        /// <summary>
        /// Alias để tương thích nơi khác lỡ gọi sai chính tả.
        /// </summary>
        public string CheckActionRigh(string requestid, string dep_c, string user)
            => CheckActionRight(requestid, dep_c, user);

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

        public IEnumerable<ChangeRequests> ViewMyPendingRequest(string user)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.Query<ChangeRequests>(
                    "dbo._PCN_GetMyPendingRequest",
                    new { userid = user },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public IEnumerable<ChangeRequests> MyChangeRequest(string user)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.Query<ChangeRequests>(
                    "dbo._PCN_GetMyChangeRequest",
                    new { userid = user }, // Tên param trùng SP
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        /// <summary>
        /// Lấy status nhanh để Controller khoá/mở chỉnh sửa (tránh lệ thuộc DTO.status).
        /// </summary>
        public int? GetRequestStatus(string requestid)
        {
            using (var conn = Db.GetConnection())
            {
                const string sql = @"
                    SELECT TOP (1) TRY_CONVERT(int, status)
                    FROM _changerequestdetail
                    WHERE ChangeRequestID = @requestid";
                return conn.QueryFirstOrDefault<int?>(sql, new { requestid });
            }
        }

        /* ============================ DELETE (tích hợp vào SP CREATE) ============================ */
        public ReturnMessageResult DeleteViaCreateSp(string requestid, string user)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.QueryFirstOrDefault<ReturnMessageResult>(
                    "dbo._PCN_CreateNewRequest",
                    new
                    {
                        Action = "DELETE",
                        requestid = requestid,
                        category = (string)null,
                        ChangeTitle = (string)null,
                        Model = (string)null,
                        DocumentCode = (string)null,
                        version = (string)null,
                        Request_detail = (string)null,
                        group_dept = (string)null,
                        user = user,
                        JsonFileNames = (string)null,
                        FileNames = (string)null
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

    }
}
