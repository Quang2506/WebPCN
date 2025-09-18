using Core.Dtos;
using Dapper;
using System.Collections.Generic;
using System.Data;

namespace Data.Repositories
{
    public class MenuRepository
    {
        public IEnumerable<MenuItem> GetMenu(int userId)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.Query<MenuItem>(
                    "dbo.TestMenu",
                    new { functionType = "getlistmenu", UserID = userId },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
    }
}
