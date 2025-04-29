using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RideWild.DTO;
using RideWild.Interfaces;
using RideWild.Models;
using RideWild.Models.AdventureModels;
using RideWild.Models.DataModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RideWild.Services
{
    public class AuthService : IAuthService
    {

        private readonly IConfiguration _configuration;
        private readonly AdventureWorksLt2019Context _context;
        private readonly AdventureWorksDataContext _contextData;
        private readonly IEmailService _emailService;

        public AuthService(AdventureWorksLt2019Context context, AdventureWorksDataContext contextData, IConfiguration configuration, IEmailService emailService)
        {
            _configuration = configuration;
            _context = context;
            _contextData = contextData;
            _emailService = emailService;
        }

        public async Task<AuthResult> LoginAsync(LoginDTO request)
        {
            string email = request.Email;
            string password = request.Password;

            var customer = await _contextData.CustomerData
                .Where(c => c.EmailAddress == email)
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                var oldCustumer = await _context.Customers
                    .Where(c => c.EmailAddress == email)
                    .FirstOrDefaultAsync();

                if (oldCustumer == null)
                {
                    return AuthResult.FailureLogin("Account inesistente");
                }
                else
                {
                    var subject = "Aggiornaento sistema";
                    var emailContent = "Ciao, per un aggiornamento di sistema per accedere al tuo profilo devi reimpostare la password." + "Clicca sul link  sottostante per reimpostare la password: <a href='#' ";
                    await _emailService.PswResetEmailAsync(email, subject, emailContent);

                    return AuthResult.FailureLogin($"L'Email ({email}) è registrata nel sistema vecchio");
                }
            }
            else
            {
                var isValid = SecurityLib.PasswordUtility.VerifyPassword(password, customer.PasswordHash, customer.PasswordSalt);
                if (isValid)
                {
                    var secretKey = _configuration["JwtSettings:SecretKey"];
                    var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString())
                    };

                    var token = new JwtSecurityToken(
                        claims: claims,
                        expires: DateTime.UtcNow.AddHours(1),
                        signingCredentials: creds
                    );

                    var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                    return AuthResult.SuccessLogin(jwt);
                }
                else
                {
                    return AuthResult.FailureLogin("Password errata");
                }
            }
            
        }
    }

}
