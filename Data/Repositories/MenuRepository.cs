using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.Dtos;          // MenuItem
using Dapper;
namespace Data.Repositories
{
    public class MenuRepository
    {
        /// <summary>
        /// Lấy danh sách menu phẳng (đã lọc theo quyền user) từ SP.
        /// Kỳ vọng SP dbo.PCN_MenuList trả về các cột:
        ///   MenuID, ParentID, MenuText, Url, Sort
        /// </summary>
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
        /// <summary>
        /// Lấy node CHA theo tên hiển thị (MenuName) – parent là các dòng có ParentMenuID NULL/0.
        /// </summary>
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
        /// <summary>
        /// Lấy node CON theo tên (có join đúng CHA theo parentName).
        /// </summary>
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
        /// <summary>
        /// Lấy meta menu theo ID (dùng cho controller nhận pid/cid).
        /// </summary>
        public MenuItem GetById(int id)
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
            WHERE MenuID = @id;";
            using (var conn = Db.GetConnection())
            {
                return conn.Query<MenuItem>(sql, new { id }, commandType: CommandType.Text)
                           .FirstOrDefault();
            }
        }
    }
}