using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RideWild.DataModels;
//test
namespace RideWild.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AdventureWorksDataContext _context;

        public AuthController(AdventureWorksDataContext context)
        {
            _context = context;
        }

        // GET: api/AuthUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthUser>>> GetAuthUsers()
        {
            return await _context.AuthUsers.ToListAsync();
        }

        // GET: api/AuthUsers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AuthUser>> GetAuthUser(long id)
        {
            var authUser = await _context.AuthUsers.FindAsync(id);

            if (authUser == null)
            {
                return NotFound();
            }

            return authUser;
        }

        // PUT: api/AuthUsers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAuthUser(long id, AuthUser authUser)
        {
            if (id != authUser.Id)
            {
                return BadRequest();
            }

            _context.Entry(authUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuthUserExists(id))
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

        // POST: api/AuthUsers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AuthUser>> PostAuthUser(AuthUser authUser)
        {

            _context.AuthUsers.Add(authUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAuthUser", new { id = authUser.Id }, authUser);
        }

        // DELETE: api/AuthUsers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthUser(long id)
        {
            var authUser = await _context.AuthUsers.FindAsync(id);
            if (authUser == null)
            {
                return NotFound();
            }

            _context.AuthUsers.Remove(authUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AuthUserExists(long id)
        {
            return _context.AuthUsers.Any(e => e.Id == id);
        }
    }
}
