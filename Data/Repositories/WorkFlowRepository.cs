using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Core.Dtos;

namespace Data.Repositories
{
    public class WorkFlowRepository
    {
        public IEnumerable<WorkFlow> GetJobStatusHistory(string changeRequestID, string dep_c)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.Query<WorkFlow>(
                    "dbo.PCN_GetJobStatusHistory",
                    new
                    {
                        ChangeRequestID = changeRequestID,
                        Dep_c = dep_c
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
    }
}
