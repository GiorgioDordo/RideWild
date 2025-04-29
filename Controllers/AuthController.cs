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
using Microsoft.AspNetCore.Identity.Data;

namespace RideWild.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /*
         * Customer Login
         */
        [HttpPost("Login")]
        public async Task<ActionResult<Customer>> LoginCustomer(LoginDTO loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.Login(loginRequest);
            if (!result.Success)
                return Unauthorized(result.Message);

            return Ok(result);
        }

        /*
         * Customer Registration
         */
        [HttpPost("Register")]
        public async Task<ActionResult<CustomerDTO>> PostCustomer(CustomerDTO customer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.Register(customer);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }

        /*
        * Customer refresh jwt token
        */
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] String refreshToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RefreshTokenAsync(refreshToken);
            if (!result.Success)
                return Unauthorized(result.Message);

            return Ok(result);
        }


    }
}