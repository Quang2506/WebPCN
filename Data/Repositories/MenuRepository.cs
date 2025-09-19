
using Core.Dtos;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Data.Repositories
{
    public class MenuRepository
    {
        public IEnumerable<MenuItem> GetMenuFlat(string userName)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.Query<MenuItem>(
                    "dbo.PCN_MenuList",
                    new { functionType = "getlistmenu", UserName = userName },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public MenuItem GetParentByName(string parentName)
        {
            const string sql = @"
            SELECT TOP 1
                MenuID,
                MenuName AS MenuText,
                CASE WHEN ParentMenuID IS NULL OR ParentMenuID=0 OR ParentMenuID=MenuID
                     THEN NULL ELSE ParentMenuID END AS ParentID,
                LinkTable AS Url,
                MenuID AS Sort
            FROM dbo.Menulist
            WHERE (ParentMenuID IS NULL OR ParentMenuID = 0)
              AND MenuName = @parentName;";

            using (var conn = Db.GetConnection())
            {
                return conn.Query<MenuItem>(sql, new { parentName }, commandType: CommandType.Text)
                           .FirstOrDefault();
            }
        }

        public MenuItem GetChildByNames(string parentName, string childName)
        {
            const string sql = @"
            SELECT TOP 1
                c.MenuID,
                c.MenuName AS MenuText,
                CASE WHEN c.ParentMenuID IS NULL OR c.ParentMenuID=0 OR c.ParentMenuID=c.MenuID
                     THEN NULL ELSE c.ParentMenuID END AS ParentID,
                c.LinkTable AS Url,
                c.MenuID AS Sort
            FROM dbo.Menulist AS c
            JOIN dbo.Menulist AS p ON p.MenuID = c.ParentMenuID
            WHERE p.MenuName = @parentName
              AND c.MenuName = @childName;";

            using (var conn = Db.GetConnection())
            {
                return conn.Query<MenuItem>(sql, new { parentName, childName }, commandType: CommandType.Text)
                           .FirstOrDefault();
            }
        }




    }
}
