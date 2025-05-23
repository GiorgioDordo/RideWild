using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using RideWild.DTO;
using RideWild.Models;

namespace RideWild.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> Login(LoginDTO request);
        Task<AuthResult> Register(RegisterDTO request);

        Task<AuthResult> RefreshTokenAsync(RefreshTokenDTO refreshToken);
        Task<AuthResult> RevokeRefreshTokenAsync(RefreshTokenDTO refreshToken);
        Task<AuthResult> ResetPasswordOldCustomer(ResetPasswordDTO resetPassword);
        Task<AuthResult> UpdatePassword(ResetPasswordDTO resetPassword);
        Task<AuthResult> RequestResetPsw(string email);
        string GenerateJwtTokenResetPwd(string email);

        string GenerateJwtToken(string id, DateTime? lastPasswordChangeUtc);
    }
}
