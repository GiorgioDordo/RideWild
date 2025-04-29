using RideWild.DTO;
using RideWild.Models;

namespace RideWild.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(LoginDTO request);
    }
}
