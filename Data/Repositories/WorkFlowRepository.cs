using System.Collections.Generic;
using System.Data;
using Dapper;
using Core.Dtos;
using System.Data.SqlClient;
using System;

namespace Data.Repositories
{
    public class WorkFlowRepository
    {
        public List<WorkFlow> GetJobStatusHistory(string changeRequestID, string dep_c)
        {

            var result = new List<WorkFlow>();

            using (var conn = (SqlConnection)Db.GetConnection())
            {
                using (var cmd = new SqlCommand("PCN_GetJobStatusHistory", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ChangeRequestID", changeRequestID);
                    cmd.Parameters.AddWithValue("@Dep_c", dep_c);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new WorkFlow
                            {
                                ChangeRequestID = reader["ChangeRequestID"].ToString(),
                                Dep_c = reader["Dep_c"].ToString(),
                                Status = Convert.ToInt32(reader["Status"]),
                                Status_nm = reader["Status_nm"].ToString(),
                                Confirm_per = reader["Confirm_per"].ToString(),
                                Confirm_dt = DateTime.ParseExact(reader["Confirm_dt"].ToString(), "yyyyMMddHHmmss", null)
                            });
                        }
                    }
                }
            }    

            return result;
        }
    }
}
