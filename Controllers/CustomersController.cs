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

            return CreatedAtAction(nameof(GetPersonalInfo), new { }, addressDTO);
        }

        /*
         * GET: /Customers/GetAddress
         * Get the address of the customer that use the API
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

            var isAddressShared = await _context.CustomerAddresses.AnyAsync(ca => ca.AddressId == id);
            if (!isAddressShared)
            {
                _context.Addresses.Remove(customerAddress.Address);
            }

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
    }
}
