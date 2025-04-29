using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using RideWild.Models.DataModels;
using RideWild.DTO;
using RideWild.Models.AdventureModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RideWild.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly AdventureWorksDataContext _contextData;

        public CustomersController(AdventureWorksLt2019Context context, AdventureWorksDataContext contextData)
        {
            _context = context;
            _contextData = contextData;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        // PUT: api/Customers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return BadRequest();
            }

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Customers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CustomerDTO>> PostCustomer(CustomerDTO customer)
        {
            if(checkEmailExists(customer.EmailAddress))
            {
                return Conflict("Email already exists");
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

                return CreatedAtAction("GetCustomer", new { id = newCustomer.CustomerId }, customer);
            }
           
        }

        // POST: api/Customers/Login
        [HttpPost("Login")]
        public async Task<ActionResult<Customer>> LoginCustomer(LoginDTO loginDTO)
        {
            string email = loginDTO.Email;
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
                    return Conflict(new
                    {
                        message = email +"email registrata nel sistema vecchio",
                        errorCode = "USER_EXISTS"
                    });
                }

            }
            else
            {
                var isValid = SecurityLib.PasswordUtility.VerifyPassword(password, customer.PasswordHash, customer.PasswordSalt);
                if (isValid)
                {
                    return Ok(customer);
                }
                else
                {
                    return Unauthorized("Invalid password");
                }
            }

        }

        /*
         * POST: api/Customers/Address
         * Insert a new address for a customer
         */
        [HttpPost("Address")]
        public async Task<ActionResult<Address>> AddressCustomer(AddressDTO addressDTO)
        {
            Address address = new Address
            {
                AddressLine1 = addressDTO.AddressLine1,
                AddressLine2 = addressDTO.AddressLine2,
                City = addressDTO.City,
                StateProvince = addressDTO.StateProvince,
                CountryRegion = addressDTO.CountryRegion,
                PostalCode = addressDTO.PostalCode
            };
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            CustomerAddress customerAddress = new CustomerAddress
            {
                CustomerId = addressDTO.CustomerId,
                AddressId = address.AddressId,
                AddressType = addressDTO.AddressType,
            };
            _context.CustomerAddresses.Add(customerAddress);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAddressById", new { id = address.AddressId }, address);

        }

        // GET: api/Customers/Address/5
        [HttpGet("Address/{id}")]
        public async Task<ActionResult<Address>> GetAddressById(int id)
        {
            var address = await _context.Addresses.FindAsync(id);

            if (address == null)
            {
                return NotFound();
            }

            return address;
        }

        /*
         check if email exists
         */
        private bool checkEmailExists(string email)
        {
            return  _contextData.CustomerData.Any(e => e.EmailAddress == email) || _context.Customers.Any(e => e.EmailAddress == email);
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

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}
