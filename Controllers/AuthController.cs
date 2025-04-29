using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using RideWild.Models.DataModels;
using RideWild.DTO;
using RideWild.Interfaces;
using RideWild.Models.AdventureModels;

namespace RideWild.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AdventureWorksLt2019Context _context;
        private readonly AdventureWorksDataContext _contextData;
        private readonly IEmailService _emailService;

        public AuthController(AdventureWorksLt2019Context context, AdventureWorksDataContext contextData, IConfiguration configuration, IEmailService emailService)
        {
            _configuration = configuration;
            _context = context;
            _contextData = contextData;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<Customer>> LoginCustomer(LoginDTO loginDTO)
        {
            string email = loginDTO.EmailAddress;
            string password = loginDTO.Password;

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
                    return NotFound("Account non esiste");
                }
                else
                {
                    var subject = "Aggiornaento sistema";
                    var emailContent = "Ciao, per un aggiornamento di sistema per accedere al tuo profilo devi reimpostare la password." + "Clicca sul link  sottostante per reimpostare la password: <a href='#' ";
                    await _emailService.PswResetEmailAsync(email, subject, emailContent);

                    return Conflict(new
                    {
                        message = $"L'Email ({email}) è registrata nel sistema vecchio",
                        errorCode = "USER_EXISTS"
                    });
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

                    return Ok(new { token = jwt });
                }
                else
                {
                    return Unauthorized("Password non valida!");
                }
            }

        }

        [HttpPost("register")]
        public async Task<ActionResult<CustomerDTO>> PostCustomer(CustomerDTO customer)
        {
            if (checkEmailExists(customer.EmailAddress))
            {
                return Conflict("Email già registrata nel portale");
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

                    return Conflict(new
                    {
                        message = "Errore durante il salvataggio nel database.",
                        details = innerMessage,
                        errorCode = "CUSTOMER_INSERT_ERROR"
                    });
                }
                catch (Exception ex)
                {
                    return Conflict(new
                    {
                        message = ex.Message,
                        errorCode = "CUSTOMER_INSERT_ERROR"
                    });
                }


                var customerData = new CustomerData
                {
                    Id = newCustomer.CustomerId,
                    EmailAddress = customer.EmailAddress,
                    PasswordHash = psw.Hash,
                    PasswordSalt = psw.Salt,
                    PhoneNumber = customer.Phone,
                    AddressLine = ""
                };
                _contextData.CustomerData.Add(customerData);
                await _contextData.SaveChangesAsync();

                return Ok("Utente nuovo registrato");
            }

        }

        /*
         check if email exists
         */
        private bool checkEmailExists(string email)
        {
            return _contextData.CustomerData.Any(e => e.EmailAddress == email) || _context.Customers.Any(e => e.EmailAddress == email);
        }

        [Authorize]
        [HttpGet("personalInfo")]
        public async Task<ActionResult<Customer>> getPersonalInfo()
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Vuoto");

            if (!int.TryParse(userId, out int userIdInt))
                return Unauthorized("Errore");

            var customerWithAddress = await _context.Customers
                .Where(c => c.CustomerId == userIdInt)
                .Include(c => c.CustomerAddresses)
                    .ThenInclude(ca => ca.Address)
                .FirstOrDefaultAsync();

            if (customerWithAddress == null)
            {
                return NotFound("Account non esiste");
            }

            return Ok(customerWithAddress);

        }
    }
}