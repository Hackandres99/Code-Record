using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.TestStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.SQLServer.Common.SubjectStructure.TestStructure
{
    [Route("api/sql/common/[controller]")]
    [ApiController]
    public class OptionsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public OptionsController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<Option>>> GetOptions()
        {
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized();

			var options = await _context.Options
			.Where(o => _context.Allows
				.Any(a => a.UserId == Guid.Parse(userId) &&
						  a.ResourceId == o.Id &&
						  a.ResourceType == "option"))
			.ToListAsync();

			return Ok(options);
		}

        [HttpGet("me/{id}")]
        public async Task<ActionResult<Option>> GetOption(Guid id)
        {
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null) return Unauthorized();

			var option = await _context.Options
			.Where(o => o.Id == id && _context.Allows
				.Any(a => a.UserId == Guid.Parse(userId) &&
						  a.ResourceId == id &&
						  a.ResourceType == "option"))
			.FirstOrDefaultAsync();

			if (option == null) return Forbid();

			return Ok(option);
		}
    }
}
