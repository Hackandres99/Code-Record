using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.SQLServer.Common.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure
{
    [Route("api/sql/common/[controller]")]
    [ApiController]
    public class AnswersController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public AnswersController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Answer>>> GetAnwsers()
        {
            return await _context.Anwsers.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Answer>> GetAnwser(Guid id)
        {
            var anwser = await _context.Anwsers.FindAsync(id);

            if (anwser == null) return NotFound();

			return anwser;
        }

        [HttpPut("me/{id}")]
        public async Task<IActionResult> PutAnwser(Guid id, Answer anwser)
        {
			var userEmail = GetCurrentUserEmail();

			if (id != anwser.Id) 
                return BadRequest(new { error = "Answer ID does not match." });

            var existingAnswer = GetExistingAnwser(id, userEmail).Result;

			if (existingAnswer == null) 
                return NotFound(new { error = "Anwser not found or you do not have access." });

            anwser.CreationDate = existingAnswer.CreationDate;
            anwser.UpdateDate = DateTime.UtcNow;
			_context.Entry(anwser).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!AnwserExists(id)) 
                    return NotFound(new { error = "Anwser no longer exists." });
				else throw;
			}

			return NoContent();
		}

        [HttpPost]
        public async Task<ActionResult<Answer>> PostAnwser(Answer anwser)
        {
            anwser.UpdateDate = null;
            anwser.CreationDate = DateTime.Now;
            _context.Anwsers.Add(anwser);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAnwser", new { id = anwser.Id }, anwser);
        }

        [HttpDelete("me/{id}")]
        public async Task<IActionResult> DeleteAnwser(Guid id)
        {
			var userEmail = GetCurrentUserEmail();

			var answer = await _context.Anwsers
				.FirstOrDefaultAsync(a => a.Id == id && a.UserEmail == userEmail);

			if (answer == null) 
                return NotFound(new { error = "Answer not found or you do not have access." });

			_context.Anwsers.Remove(answer);
			await _context.SaveChangesAsync();

			return NoContent();
		}

        private bool AnwserExists(Guid id)
        {
            return _context.Anwsers.Any(e => e.Id == id);
        }

		private string GetCurrentUserEmail()
		{
			return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
				   ?? throw new UnauthorizedAccessException("User is not authenticated.");
		}

		private async Task<Answer> GetExistingAnwser(Guid id, string userEmail)
		{
			var existingAnwser = await _context.Anwsers
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Id == id && c.UserEmail == userEmail);

			return existingAnwser;
		}
	}
}
