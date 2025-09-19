using System.Threading.Tasks;
using Core.Dtos;
using Data.Repositories;

namespace Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repo;
        public AuthService(IAuthRepository repo) { _repo = repo; }

        public Task<LoginResult> LoginAsync(LoginRequest req) => _repo.LoginAsync(req);
    }
}
