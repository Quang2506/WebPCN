using Core.Dtos;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class ChangeRequestRepository
    {
        public ChangeRequests ViewDetailRequest(string  requestid, string dep_c)

        {


            using (var conn = Db.GetConnection())
            {
                return conn.QueryFirstOrDefault<ChangeRequests>(
                    "dbo._PCN_GetRequestDetail",
                    new 
                    { 
                        ChangeRequestID = requestid,
                        dep_c = dep_c 
                    },     // Tên param trùng SP
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public String ProcessAction(string requestid, string dep_c, string action, string user)

        {


            using (var conn = Db.GetConnection())
            {
                return conn.QueryFirstOrDefault<String>(
                    "dbo._PCN_ProcessAction",
                    new
                    {
                        ChangeRequestID = requestid,
                        dep_c = dep_c,
                        action = action,
                        user = user
                    },     // Tên param trùng SP
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public String CheckActionRigh(string requestid, string dep_c, string user)

        {


            using (var conn = Db.GetConnection())
            {
                return conn.QueryFirstOrDefault<String>(
                    "dbo._PCN_CheckActionRight",
                    new
                    {
                        user = user,
                        dep_c = dep_c,
                        requestid = requestid

                    },     // Tên param trùng SP
                    commandType: CommandType.StoredProcedure
                );
            }
        }
    }
}
