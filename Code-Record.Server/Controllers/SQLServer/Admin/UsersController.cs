using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer;
using Code_Record.Server.Extensions.Controllers;

namespace Code_Record.Server.Controllers.SQLServer.Admin
{
    [Route("api/sql/admin/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public UsersController(SQLServerContext context)
        {
            _context = context;
        }

		[HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
			return await _context.Users.ToListAsync();
		}

		[HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null) return NotFound();

			return user;
        }

		[HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, User user)
        {
            if (id != user.Id) return BadRequest();
            var existingUser = GetExistingUser(id).Result;
            user.CreationDate = existingUser.CreationDate;
            user.UpdateDate = DateTime.UtcNow;
			user.AccountPass = EncryptData.EncryptPass(user.AccountPass);
			_context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id)) return NotFound();
				else throw;
			}

            return NoContent();
        }

		[HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
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
