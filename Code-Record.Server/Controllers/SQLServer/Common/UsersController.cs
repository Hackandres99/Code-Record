using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer;
using Code_Record.Server.Extensions.Controllers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Code_Record.Server.Models.Base.SubjectStructure;

namespace Code_Record.Server.Controllers.SQLServer.Common
{
    [Route("api/sql/common/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public UsersController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
			var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (id == null) return Unauthorized();

			Guid.TryParse(id, out Guid userId);
            var user = await _context.Users.FindAsync(userId);
			if (user == null) return NotFound();

			return Ok(user);
		}

        [HttpPut("me")]
        public async Task<IActionResult> PutUser(User user)
        {
			var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var rol = User.FindFirstValue(ClaimTypes.Role);
			Guid.TryParse(id, out Guid userId);
			if (id == null || user.Id != userId) return Unauthorized();

			var existingUser = GetExistingUser(userId).Result;
			user.CreationDate = existingUser.CreationDate;
			user.UpdateDate = DateTime.UtcNow;
			user.AccountPass = EncryptData.EncryptPass(user.AccountPass);
			if (rol != Models.Base.UserStructure.RolOptions.Admin.ToString())
			{
				user.Rol = Models.Base.UserStructure.RolOptions.Common;
				user.Subscription = Models.Base.UserStructure.SubscriptionOptions.Free;
			}

			if (user.Username.IsNullOrEmpty()) 
                return BadRequest(new {error = "Username null or empty"});
			
            _context.Entry(user).State = EntityState.Modified;

			try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(userId)) return NotFound();
				else throw;
			}

            return NoContent();
		}

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            user.UpdateDate = null;
            user.CreationDate = DateTime.UtcNow;
			user.AccountPass = EncryptData.EncryptPass(user.AccountPass);
			user.Rol = Models.Base.UserStructure.RolOptions.Common;
            user.Subscription = Models.Base.UserStructure.SubscriptionOptions.Free;
			_context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        [HttpDelete("me")]
        public async Task<IActionResult> DeleteUser()
        {
			var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
			Guid.TryParse(id, out Guid userId);
			if (id == null) return Unauthorized();

			var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

			_context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

		private async Task<User> GetExistingUser(Guid id)
		{
			var existingUser = await _context.Users
			.AsNoTracking()
			.FirstOrDefaultAsync(u => u.Id == id);

			return existingUser;
		}
	}
}
