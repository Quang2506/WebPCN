using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Data
{
    public static class Db
    {
        public static IDbConnection GetConnection()
        {
            var cs = ConfigurationManager.ConnectionStrings["WebPCN"].ConnectionString;
            var conn = new SqlConnection(cs);
            conn.Open();
            return conn;
        }
    }
}
