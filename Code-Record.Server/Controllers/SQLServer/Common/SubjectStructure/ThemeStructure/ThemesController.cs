using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.SQLServer.Common.SubjectStructure.ThemeStructure
{
    [Route("api/sql/common/[controller]")]
    [ApiController]
    public class ThemesController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public ThemesController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<Theme>>> GetThemes()
        {
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized();

			var themes = await _context.Themes
			.Where(t => _context.Allows
				.Any(a => a.UserId == Guid.Parse(userId) &&
						  a.ResourceId == t.Id &&
						  a.ResourceType == "theme"))
			.ToListAsync();

			return Ok(themes);
		}

        [HttpGet("me/{id}")]
        public async Task<ActionResult<Theme>> GetTheme(Guid id)
        {
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null) return Unauthorized();

			var theme = await _context.Themes
			.Where(t => t.Id == id && _context.Allows
				.Any(a => a.UserId == Guid.Parse(userId) &&
						  a.ResourceId == id &&
						  a.ResourceType == "theme"))
			.FirstOrDefaultAsync();

			if (theme == null) return Forbid();

			return Ok(theme);
		}
    }
}
