using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using RideWild.Models.DataModels;

namespace RideWild.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        [HttpPost]
        public IActionResult SystemLogin(Models.DataModels.Login login)
        {
            if (login.UserName.ToLower() == "admin" && login.Password.ToLower() == "admin")
                return Ok();
            else
            {
                return BadRequest();
            }
        }
    }
}
