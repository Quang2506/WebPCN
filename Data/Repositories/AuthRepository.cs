using Core.Dtos;
using Dapper;
using System.Data;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        public async Task<LoginResult> LoginAsync(LoginRequest req)
        {
            using (var conn = Db.GetConnection())   // giữ nguyên Db.GetConnection() như structure của bạn
            {
                var p = new DynamicParameters();
                p.Add("@Login", req.Login, DbType.String, size: 150);
                p.Add("@Password", req.Password, DbType.String, size: 200);

                var rs = await conn.QueryFirstOrDefaultAsync<LoginResult>(
                    "dbo.LoginUser",
                    p,
                    commandType: CommandType.StoredProcedure
                );

                return rs ?? new LoginResult
                {
                    StatusCode = -1,
                    Message = "SP không trả dữ liệu."
                };
            }
        }
    }
}
