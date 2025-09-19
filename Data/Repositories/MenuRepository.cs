
using Core.Dtos;
using Dapper;
using System.Collections.Generic;
using System.Data;

namespace Data.Repositories
{
    public class MenuRepository
    {
        public IEnumerable<MenuItem> GetMenuFlat(string userName)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.Query<MenuItem>(
                    "dbo.TestMenu",
                    new { functionType = "getlistmenu", UserName = userName },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
    }
}
