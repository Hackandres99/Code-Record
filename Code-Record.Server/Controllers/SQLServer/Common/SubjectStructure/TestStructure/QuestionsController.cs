using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.TestStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.SQLServer.Common.SubjectStructure.TestStructure
{
    [Route("api/sql/common/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public QuestionsController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestions()
        {
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized();

			var questions = await _context.Questions
			.Where(q => _context.Allows
				.Any(a => a.UserId == Guid.Parse(userId) &&
						  a.ResourceId == q.Id &&
						  a.ResourceType == "question"))
			.ToListAsync();

			return Ok(questions);
		}

        [HttpGet("me/{id}")]
        public async Task<ActionResult<Question>> GetQuestion(Guid id)
        {
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null) return Unauthorized();

			var question = await _context.Questions
			.Where(q => q.Id == id && _context.Allows
				.Any(a => a.UserId == Guid.Parse(userId) &&
						  a.ResourceId == id &&
						  a.ResourceType == "question"))
			.FirstOrDefaultAsync();

			if (question == null) return Forbid();

			return Ok(question);
		}
    }
}
