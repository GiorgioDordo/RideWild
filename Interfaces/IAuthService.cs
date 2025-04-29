using RideWild.DTO;
using RideWild.Models;

namespace RideWild.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> Login(LoginDTO request);
        Task<AuthResult> Register(CustomerDTO request);

        Task<AuthResult> RefreshTokenAsync(string refreshToken);
    }
}
