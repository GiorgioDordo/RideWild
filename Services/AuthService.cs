using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Common;
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
        private JwtSettings _jwtSettings;

        public AuthService(JwtSettings jwtsettings, AdventureWorksLt2019Context context, AdventureWorksDataContext contextData, IConfiguration configuration, IEmailService emailService)
        {
            _configuration = configuration;
            _context = context;
            _contextData = contextData;
            _emailService = emailService;
            _jwtSettings = jwtsettings;
        }

        public async Task<AuthResult> Login(LoginDTO request)
        {
            string email = request.Email;
            string password = request.Password;

            var customer = await _contextData.CustomerData
                .Where(c => c.EmailAddress == email)
                .FirstOrDefaultAsync();

            if (customer != null)
            {
                var isValid = SecurityLib.PasswordUtility.VerifyPassword(password, customer.PasswordHash, customer.PasswordSalt);
                if (isValid)
                {
                    string jwt = GenerateJwtToken(customer.Id.ToString());
                    string refreshToken = GenerateRefreshToken();

                    customer.RefreshToken = refreshToken;
                    customer.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
                    await _contextData.SaveChangesAsync();

                    return AuthResult.SuccessAuth(jwt, refreshToken);
                }
                else
                {
                    return AuthResult.FailureAuth("Password errata");
                }
            }
            else
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
                    var jwt = GenerateJwtTokenResetPwd(email);
                    var resetLink = $"https://localhost:7023/reset-password?token={jwt}";
                    var subject = "Aggiornamento sistema";
                    var emailContent = $@"
                        <p>Clicca sul link sottostante per reimpostare la password:</p>
                        <p><a href=""{resetLink}"">{resetLink}</a></p>";
                    await _emailService.PswResetEmailAsync(email, subject, emailContent);

                    return AuthResult.FailureAuth($"L'Email ({email}) è registrata nel sistema vecchio");
                }
            }
            
        }

        public async Task<AuthResult> ResetPasswordOldCustomer(ResetPasswordDTO resetPassword)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = handler.ValidateToken(resetPassword.Token, parameters, out _);

                var tokenType = principal.Claims.FirstOrDefault(c => c.Type == "token_type")?.Value;
                if (tokenType != "password_reset")
                    return AuthResult.FailureAuth("Token non valido");

                var userEmail = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var user = await _context.Customers
                    .FirstOrDefaultAsync(c => c.EmailAddress == userEmail);

                if (user == null) 
                    return AuthResult.FailureAuth("Utente non trovato");

                var psw = SecurityLib.PasswordUtility.HashPassword(resetPassword.NewPassword);

                string refreshToken = GenerateRefreshToken();
                var customerData = new CustomerData
                {
                    Id = user.CustomerId,
                    EmailAddress = user.EmailAddress,
                    PasswordHash = psw.Hash,
                    PasswordSalt = psw.Salt,
                    PhoneNumber = user.Phone,
                    AddressLine = "",
                    RefreshToken = refreshToken,
                    RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
                };
                _contextData.CustomerData.Add(customerData);
                await _contextData.SaveChangesAsync();

                user.PasswordHash = "";
                user.PasswordSalt = "";
                user.EmailAddress = "";
                user.Phone = "";
                user.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return AuthResult.SuccessOperation();
            }
            catch
            {
                return AuthResult.FailureAuth("Token scaduto o non valido");
            }
        }

        public async Task<AuthResult> RequestResetPsw(string email)
        {
            var customer = await _contextData.CustomerData
                .Where(c => c.EmailAddress == email)
                .FirstOrDefaultAsync();
            if (customer == null)
            {
                return AuthResult.FailureAuth("Nessun account è associato a questa email");
            }
            else
            {
                var jwt = GenerateJwtTokenResetPwd(email);
                var resetLink = $"https://localhost:7023/update-password?token={jwt}";
                var subject = "Reimposta la tua password";
                var emailContent = $@"
                    <p>Clicca sul link sottostante per reimpostare la password:</p>
                    <p><a href=""{resetLink}"">{resetLink}</a></p>";
                await _emailService.PswResetEmailAsync(email, subject, emailContent);
                return AuthResult.SuccessOperation();
            }
        }
        public async Task<AuthResult> UpdatePassword(ResetPasswordDTO resetPassword)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = handler.ValidateToken(resetPassword.Token, parameters, out _);

                var tokenType = principal.Claims.FirstOrDefault(c => c.Type == "token_type")?.Value;
                if (tokenType != "password_reset")
                    return AuthResult.FailureAuth("Token non valido");

                var userEmail = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var user = await _contextData.CustomerData
                    .FirstOrDefaultAsync(c => c.EmailAddress == userEmail);

                if (user == null)
                    return AuthResult.FailureAuth("Utente non trovato");

                var psw = SecurityLib.PasswordUtility.HashPassword(resetPassword.NewPassword);

                string refreshToken = GenerateRefreshToken();

                user.PasswordHash = psw.Hash;
                user.PasswordSalt = psw.Salt;
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

                await _contextData.SaveChangesAsync();

                return AuthResult.SuccessOperation();
            }
            catch
            {
                return AuthResult.FailureAuth("Token scaduto o non valido");
            }
        }

        public async Task<AuthResult> RefreshTokenAsync(RefreshTokenDTO refreshToken)
        {
            var user = await _contextData.CustomerData
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken.RefreshToken);

            if (user == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
                return AuthResult.FailureAuth("Token non valido o scaduto");

            var newAccessToken = GenerateJwtToken(user.Id.ToString());
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            await _contextData.SaveChangesAsync();

            return AuthResult.SuccessAuth(newAccessToken, newRefreshToken);
        }
        public async Task<AuthResult> RevokeRefreshTokenAsync(RefreshTokenDTO refreshToken)
        {
            var user = await _contextData.CustomerData
                .FirstOrDefaultAsync(c => c.RefreshToken == refreshToken.RefreshToken);

            if (user == null) return AuthResult.FailureAuth("Token non valido o scaduto");

            user.RefreshToken = "";
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(-5);
            await _contextData.SaveChangesAsync();

            return AuthResult.SuccessOperation();
        }
        public async Task<AuthResult> Register(RegisterDTO customer)
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
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    EmailAddress = "",
                    PasswordHash = "",
                    PasswordSalt = "",
                    Phone = "",
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

                return AuthResult.SuccessOperation();
            }
        }

        private string GenerateJwtToken(String id)
        {
            var secretKey = _jwtSettings.SecretKey;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, id)
                }),
                Expires = DateTime.Now.AddMinutes(_jwtSettings.ExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return jwt;
        }
        private string GenerateJwtTokenResetPwd(String email)
        {
            var secretKey = _configuration["JwtSettings:SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("token_type", "password_reset"),
                new Claim(ClaimTypes.Email, email)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
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

        private bool checkEmailExists(string email)
        {
            return _contextData.CustomerData.Any(e => e.EmailAddress == email) || _context.Customers.Any(e => e.EmailAddress == email);
        }


    }

}
