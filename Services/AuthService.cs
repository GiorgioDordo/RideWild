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
using System.Security.Cryptography;
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

        public async Task<AuthResult> Login(LoginDTO request)
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
                    return AuthResult.FailureAuth("Account inesistente");
                }
                else
                {
                    var subject = "Aggiornaento sistema";
                    var emailContent = "Ciao, per un aggiornamento di sistema per accedere al tuo profilo devi reimpostare la password." + "Clicca sul link  sottostante per reimpostare la password: <a href='#' ";
                    await _emailService.PswResetEmailAsync(email, subject, emailContent);

                    return AuthResult.FailureAuth($"L'Email ({email}) è registrata nel sistema vecchio");
                }
            }
            else
            {
                var isValid = SecurityLib.PasswordUtility.VerifyPassword(password, customer.PasswordHash, customer.PasswordSalt);
                if (isValid)
                {
                    string jwt = GenerateJwtToken(customer.Id.ToString());
                    string refreshToken = GenerateRefreshToken();

                    customer.RefreshToken = refreshToken;
                    customer.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
                    await _contextData.SaveChangesAsync();

                    return AuthResult.SuccessAuth(jwt,refreshToken);
                }
                else
                {
                    return AuthResult.FailureAuth("Password errata");
                }
            }
            
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            var user = await _contextData.CustomerData
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
                return AuthResult.FailureAuth("Token non valido o scaduto");

            var newAccessToken = GenerateJwtToken(user.Id.ToString());
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return AuthResult.SuccessAuth(newAccessToken, newRefreshToken);
        }

        private string GenerateJwtToken(String id)
        {
            var secretKey = _configuration["JwtSettings:SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        private string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        public async Task<AuthResult> Register(CustomerDTO customer)
        {
            if (checkEmailExists(customer.EmailAddress))
            {
                return AuthResult.FailureAuth("Utente già presente con questa email");
            }
            else
            {
                var psw = SecurityLib.PasswordUtility.HashPassword(customer.Password);

                Customer newCustomer = new Customer
                {
                    NameStyle = customer.NameStyle,
                    Title = customer.Title,
                    FirstName = customer.FirstName,
                    MiddleName = customer.MiddleName,
                    LastName = customer.LastName,
                    Suffix = customer.Suffix,
                    CompanyName = customer.CompanyName,
                    SalesPerson = customer.SalesPerson,
                    EmailAddress = "",
                    PasswordHash = "",
                    PasswordSalt = ""
                };
                try
                {
                    _context.Customers.Add(newCustomer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    //DA CONTROLLARE
                    var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                    return AuthResult.FailureAuth($"Errore durante il salvataggio nel DB: {innerMessage}");
                }
                catch (Exception ex)
                {
                    return AuthResult.FailureAuth($"Errore durante il salvataggio nel DB: {ex.Message}");
                }
                string refreshToken = GenerateRefreshToken();
                var customerData = new CustomerData
                {
                    Id = newCustomer.CustomerId,
                    EmailAddress = customer.EmailAddress,
                    PasswordHash = psw.Hash,
                    PasswordSalt = psw.Salt,
                    PhoneNumber = customer.Phone,
                    AddressLine = "",
                    RefreshToken = refreshToken,
                    RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
                };
                _contextData.CustomerData.Add(customerData);
                await _contextData.SaveChangesAsync();

                return AuthResult.SuccessAuth("","");
            }
        }

        private bool checkEmailExists(string email)
        {
            return _contextData.CustomerData.Any(e => e.EmailAddress == email) || _context.Customers.Any(e => e.EmailAddress == email);
        }


    }

}
