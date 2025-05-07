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
using Microsoft.Extensions.Configuration.UserSecrets;
using RideWild.Utility;

namespace RideWild.Controllers
{
    [Route("[controller]")]
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

        /*
         * POST: /Customers/NewAddress
         * Insert a new address for a customer
         */
        [Authorize]
        [HttpPost("NewAddress")]
        public async Task<ActionResult<Address>> AddressCustomer(AddressDTO addressDTO)
        {
            if (!Helper.TryGetUserId(User, out int userId))
                return Unauthorized("Utente non autenticato o ID non valido");

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
                CustomerId = userId,
                AddressId = address.AddressId,
                AddressType = addressDTO.AddressType,
            };
            _context.CustomerAddresses.Add(customerAddress);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAddressById", new { id = address.AddressId }, address);

        }

        /*
         * GET: /Customers/GetAddress
         * Get the address of the customer that use the API
         */
        [HttpGet("GetAddress")]
        public async Task<ActionResult<List<AddressDTO>>> GetAddress()
        {
            if (!Helper.TryGetUserId(User, out int userId))
                return Unauthorized("Utente non autenticato o ID non valido");

            var addresses = await _context.CustomerAddresses
                .Where(ca => ca.CustomerId == userId)
                .Include(ca => ca.Address)
                .Select(ca => new AddressDTO
                {
                    AddressLine1 = ca.Address.AddressLine1,
                    AddressLine2 = ca.Address.AddressLine2,
                    City = ca.Address.City,
                    StateProvince = ca.Address.StateProvince,
                    CountryRegion = ca.Address.CountryRegion,
                    PostalCode = ca.Address.PostalCode,
                    AddressType = ca.AddressType
                })
                .ToListAsync();

            return Ok(addresses);
        }

        /*
         * GET: /Customers/GetPersonalInfo
         * Get the personal information of the customer that use the API
         */
        [Authorize]
        [HttpGet("GetPersonalInfo")]
        public async Task<ActionResult<CustomerDTO>> getPersonalInfo()
        {
            if (!Helper.TryGetUserId(User, out int userId))
                return Unauthorized("Utente non autenticato o ID non valido");

            var customer = await _context.Customers
                .Where(c => c.CustomerId == userId)
                .FirstOrDefaultAsync();

            var customerData = await _contextData.CustomerData
                .Where(c => c.Id == userId)
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                return NotFound("Account non esiste");
            }

            var customerDTO = new CustomerDTO
            {
                NameStyle = customer.NameStyle,
                Title = customer.Title,
                FirstName = customer.FirstName,
                MiddleName = customer.MiddleName,
                LastName = customer.LastName,
                Suffix = customer.Suffix,
                CompanyName = customer.CompanyName,
                SalesPerson = customer.SalesPerson,
                EmailAddress = customerData.EmailAddress,
                Phone = customerData.PhoneNumber,
            };

            return Ok(customerDTO);
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
