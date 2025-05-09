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
using Microsoft.AspNetCore.Http.HttpResults;

namespace RideWild.Controllers
{
    [Route("api/[controller]")]
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
        public async Task<IActionResult> LoginCustomer(LoginDTO loginRequest)
        {

            var result = await _authService.Login(loginRequest);
            if (!result.Success)
                return Unauthorized(result.Message);

            return Ok(result);
        }

        /*
         * Customer Registration
         */
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterCustomer(RegisterDTO customer)
        {
            var result = await _authService.Register(customer);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }

        /*
        * Customer refresh jwt token
        */
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDTO refreshToken)
        {
            var result = await _authService.RefreshTokenAsync(refreshToken);
            if (!result.Success)
                return Unauthorized(result.Message);

            return Ok(result);
        }

        /*
         * Customer Logout
         */
        [Authorize]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout(RefreshTokenDTO refreshToken)
        {
            var result = await _authService.RevokeRefreshTokenAsync(refreshToken);
            if (!result.Success)
                return Unauthorized(result.Message);

            return Ok(result);
        }

        /*
         * Customer Reset Password
         */
        [HttpPost("ResetPswOldCustomer")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPassword)
        {
            var result = await _authService.ResetPasswordOldCustomer(resetPassword);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok(result);
        }

        /*
         * Send email to reset password
         */
        [HttpPost("UpdatePswRequest")]
        public async Task<IActionResult> UpdatePasswordNewCustomer([FromBody] string email)
        {
            var result = await _authService.RequestResetPsw(email);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok(result);
        }

        /*
         * Customer Reset Password
         */
        [HttpPost("UpdatePsw")]
        public async Task<IActionResult> UpdatePsw(ResetPasswordDTO updatePassword)
        {
            var result = await _authService.UpdatePassword(updatePassword);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok(result);
        }

    }
}
