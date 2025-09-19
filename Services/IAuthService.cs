using System.Threading.Tasks;
using Core.Dtos;

namespace Services
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(LoginRequest req);
    }
}
