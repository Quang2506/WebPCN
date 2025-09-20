using System.Collections.Generic;
using System.Data;
using Dapper;
using Core.Dtos;
namespace Data.Repositories
{
    public class PageRepository
    {
        public IEnumerable<ChangeRequestDto> GetChangeRequestsByMenu(
            string parentName, string childName, string userName, int? parentId, int? childId)
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
        public IEnumerable<ChangeRequestDto> QueryChangeRequests(
            string parentName, string childName, string userName,
            string category, string documentCode, string changeTitle, string status,
            int? parentId, int? childId)
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
    }
}