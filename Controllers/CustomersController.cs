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

        /*
         * POST: /Customers/AddAddress
         * Insert a new address for a customer that use the API
         */
        [Authorize]
        [HttpPost("AddAddress")]
        public async Task<ActionResult<Address>> AddAddress(AddressDTO addressDTO)
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

            return CreatedAtAction(nameof(GetPersonalInfo), new { }, addressDTO);
        }

        /*
         * GET: /Customers/GetAddress
         * Get the addresses of the customer that use the API
         */
        [Authorize]
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
                    AddressId = ca.AddressId,
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
         * PUT: /Customers/ModifyAddress
         * Change the address with addressId=id of the customer that use the API
         */
        [Authorize]
        [HttpPut("ModifyAddress")]
        public async Task<IActionResult> ModifyAddress(AddressDTO addressDTO)
        {
            if (!Helper.TryGetUserId(User, out int userId))
                return Unauthorized("Utente non autenticato o ID non valido");
            var customerAddress = await _context.CustomerAddresses
                .Include(ca => ca.Address)
                .FirstOrDefaultAsync(ca => ca.CustomerId == userId && ca.AddressId == addressDTO.AddressId);
            if (customerAddress == null)
                return NotFound("Indirizzo non trovato o non autorizzato");
            customerAddress.Address.AddressLine1 = addressDTO.AddressLine1;
            customerAddress.Address.AddressLine2 = addressDTO.AddressLine2;
            customerAddress.Address.City = addressDTO.City;
            customerAddress.Address.StateProvince = addressDTO.StateProvince;
            customerAddress.Address.CountryRegion = addressDTO.CountryRegion;
            customerAddress.Address.PostalCode = addressDTO.PostalCode;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /*
         * DELETE: /Customers/DeleteAddress/id
         * Delete the address with addressId=id of the customer that use the API
         */
        [Authorize]
        [HttpDelete("DeleteAddress/{id}")]
        public async Task<IActionResult> DeleteCustomerAddress(int id)
        {
            if (!Helper.TryGetUserId(User, out int userId))
                return Unauthorized("Utente non autenticato o ID non valido");

            var customerAddress = await _context.CustomerAddresses
                .Include(ca => ca.Address)
                .FirstOrDefaultAsync(ca => ca.CustomerId == userId && ca.AddressId == id);

            if (customerAddress == null)
                return NotFound("Indirizzo non trovato o non autorizzato");

            _context.CustomerAddresses.Remove(customerAddress);
            _context.Addresses.Remove(customerAddress.Address);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /*
         * GET: /Customers/GetPersonalInfo
         * Get the personal information of the customer that use the API
         */
        [Authorize]
        [HttpGet("GetPersonalInfo")]
        public async Task<ActionResult<CustomerDTO>> GetPersonalInfo()
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
            };

            return Ok(customerDTO);
        }

        /*
         * PUT: /Customers/ModifyData
         * Change the personal information of the customer that use the API
         */
        [Authorize]
        [HttpPut("ModifyData")]
        public async Task<IActionResult> ModifyData(CustomerDTO customerDto)
        {
            if (!Helper.TryGetUserId(User, out int userId))
                return Unauthorized("Utente non autenticato o ID non valido");

            var customer = await _context.Customers.FindAsync(userId);

            if (customer == null)
                return NotFound("Cliente non trovato");

            customer.FirstName = customerDto.FirstName;
            customer.LastName = customerDto.LastName;
            customer.MiddleName = customerDto.MiddleName;
            customer.Title = customerDto.Title;
            customer.Suffix = customerDto.Suffix;
            customer.CompanyName = customerDto.CompanyName;
            customer.SalesPerson = customerDto.SalesPerson;
            customer.NameStyle = customerDto.NameStyle;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /*
         * Check if the customer exists
         */
        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }

        /*
         * Get All Custumers
         */
        [Authorize(Policy = "Admin")]
        [HttpGet("GetAllCustomers")]
        public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetAllCustomers()
        {
            var customers = await _context.Customers
                .Select(c => new CustomerDTO
                {
                    NameStyle = c.NameStyle,
                    Title = c.Title,
                    FirstName = c.FirstName,
                    MiddleName = c.MiddleName,
                    LastName = c.LastName,
                    Suffix = c.Suffix,
                    CompanyName = c.CompanyName,
                    SalesPerson = c.SalesPerson,
                })
                .ToListAsync();
            return Ok(customers);
        }

        /*
         * Get customer by id
         */
        [Authorize(Policy = "Admin")]
        [HttpGet("GetCustomerById/{id}")]
        public async Task<ActionResult<CustomerDTO>> GetCustomerById(int id)
        {
            var customer = await _context.Customers
                .Where(c => c.CustomerId == id)
                .Select(c => new CustomerDTO
                {
                    NameStyle = c.NameStyle,
                    Title = c.Title,
                    FirstName = c.FirstName,
                    MiddleName = c.MiddleName,
                    LastName = c.LastName,
                    Suffix = c.Suffix,
                    CompanyName = c.CompanyName,
                    SalesPerson = c.SalesPerson,
                })
                .FirstOrDefaultAsync();
            if (customer == null)
            {
                return NotFound("Cliente non trovato");
            }
            return Ok(customer);
        }

        /*
         * Get Addresses by customer id
         */
        [Authorize(Policy = "Admin")]
        [HttpGet("GetAddressesByCustomerId/{id}")]
        public async Task<ActionResult<List<AddressDTO>>> GetAddressesByCustomerId(int id)
        {
            var addresses = await _context.CustomerAddresses
                .Where(ca => ca.CustomerId == id)
                .Include(ca => ca.Address)
                .Select(ca => new AddressDTO
                {
                    AddressId = ca.AddressId,
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


    }
}
