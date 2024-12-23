using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.SQLServer.Common.SubjectStructure
{
    [Route("api/sql/common/[controller]")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public ResourcesController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<Resource>>> GetResources()
        {
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized();

			var resources = await _context.Resources
			.Where(r => _context.Allows
				.Any(a => a.UserId == Guid.Parse(userId) &&
						  a.ResourceId == r.Id &&
						  a.ResourceType == "resource"))
			.ToListAsync();

			return Ok(resources);
        }

        [HttpGet("me/{id}")]
        public async Task<ActionResult<Resource>> GetResource(Guid id)
        {
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null) return Unauthorized();

			var resource = await _context.Resources
			.Where(r => r.Id == id && _context.Allows
				.Any(a => a.UserId == Guid.Parse(userId) &&
						  a.ResourceId == id &&
						  a.ResourceType == "resource"))
			.FirstOrDefaultAsync();

			if (resource == null) return Forbid();

			return Ok(resource);
		}
    }
}
