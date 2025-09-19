using Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public interface IAuthRepository
    {
        Task<LoginResult> LoginAsync(LoginRequest req);
    }
}
