using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.TestStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.SQLServer.Common.SubjectStructure.TestStructure
{
    [Route("api/sql/common/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public TestsController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<Test>>> GetTests()
        {
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized();

			var tests = await _context.Tests
			.Where(t => _context.Allows
				.Any(a => a.UserId == Guid.Parse(userId) &&
						  a.ResourceId == t.Id &&
						  a.ResourceType == "test"))
			.ToListAsync();

			return Ok(tests);
		}

        [HttpGet("me/{id}")]
        public async Task<ActionResult<Test>> GetTest(Guid id)
        {
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null) return Unauthorized();

			var test = await _context.Tests
			.Where(t => t.Id == id && _context.Allows
				.Any(a => a.UserId == Guid.Parse(userId) &&
						  a.ResourceId == id &&
						  a.ResourceType == "test"))
			.FirstOrDefaultAsync();

			if (test == null) return Forbid();

			return Ok(test);
		}
    }
}
