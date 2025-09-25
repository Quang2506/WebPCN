using System.Collections.Generic;
using System.Data;
using Dapper;
using Core.Dtos;
namespace Data.Repositories
{
    public class PageRepository
    {
        // ==================== HÀM CŨ (giữ nguyên) ====================
        /// <summary>
        /// Lấy danh sách pending theo menu (mặc định màn Pending)
        /// </summary>
        public IEnumerable<ChangeRequestDto> GetChangeRequestsByMenu(
            string parentName,
            string childName,
            string userName,
            int? parentId,
            int? childId)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.Query<ChangeRequestDto>(
                    "dbo.PCN_PagePendingData",
                    new
                    {
                        functionType = "list",
                        ParentMenuName = parentName,
                        ChildMenuName = childName,
                        UserName = userName,
                        ParentId = parentId,
                        ChildId = childId
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
        /// <summary>
        /// Query theo bộ lọc (màn Query cũ)
        /// </summary>
        public IEnumerable<ChangeRequestDto> QueryChangeRequests(
            string parentName,
            string childName,
            string userName,
            string category,
            string documentCode,
            string changeTitle,
            string status,
            int? parentId,
            int? childId)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.Query<ChangeRequestDto>(
                    "dbo.PCN_PagePendingData",
                    new
                    {
                        functionType = "query",
                        ParentMenuName = parentName,
                        ChildMenuName = childName,
                        UserName = userName,
                        Category = category,
                        DocumentCode = documentCode,
                        ChangeTitle = changeTitle,
                        Status = status,
                        ParentId = parentId,
                        ChildId = childId
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
        /// <summary>
        /// Lấy dữ liệu cho các dropdown filter.
        /// type: "category" | "status" | "doc" | "title"
        /// SP trả về cột Text, Value (map vào LookupItem).
        /// </summary>
        //public IEnumerable<LookupItem> GetLookups(
        //    string type,
        //    string parentName,
        //    string childName,
        //    string userName,
        //    int? parentId,
        //    int? childId)
        //{
        //    using (var conn = Db.GetConnection())
        //    {
        //        return conn.Query<LookupItem>(
        //            "dbo.PCN_PageLookup", // SP lookup riêng cho dropdown
        //            new
        //            {
        //                Type = type,
        //                ParentMenuName = parentName,
        //                ChildMenuName = childName,
        //                UserName = userName,
        //                ParentId = parentId,
        //                ChildId = childId
        //            },
        //            commandType: CommandType.StoredProcedure
        //        );
        //    }
        //}
        // ==================== HÀM MỚI (theo Plant + DepCode) ====================
        /// <summary>
        /// Lấy danh sách mặc định cho Query mới theo Plant + DepCode (giống Pending).
        /// </summary>
        public IEnumerable<ChangeRequestDto> GetChangeRequestsByDep(string plant, string depCode, string userName)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.Query<ChangeRequestDto>(
                    "dbo.PCN_QueryByDep",   // SP mới
                    new
                    {
                        Plant = plant,
                        DepCode = depCode,
                        UserName = userName,
                        FunctionType = "list"
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
        /// <summary>
        /// Query có filter theo Plant + DepCode.
        /// Chỉ Category là dropdown, các filter khác (doc, title, status) nhập tay.
        /// </summary>
        public IEnumerable<ChangeRequestDto> QueryChangeRequestsByDep(
            string plant,
            string depCode,
            string userName,
            string category,
            string documentCode,
            string changeTitle,
            string status)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.Query<ChangeRequestDto>(
                    "dbo.PCN_QueryByDep",   // SP mới
                    new
                    {
                        Plant = plant,
                        DepCode = depCode,
                        UserName = userName,
                        Category = category,
                        DocumentCode = documentCode,
                        ChangeTitle = changeTitle,
                        Status = status,
                        FunctionType = "query"
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
        /// <summary>
        /// Lấy danh sách Category theo Plant + DepCode (dropdown).
        /// </summary>
        public IEnumerable<LookupItem> GetCategoryLookupByDep(string plant, string depCode)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.Query<LookupItem>(
                    "dbo.PCN_QueryCategoryByDep",   // SP mới cho category
                    new
                    {
                        Plant = plant,
                        DepCode = depCode
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
    }
}