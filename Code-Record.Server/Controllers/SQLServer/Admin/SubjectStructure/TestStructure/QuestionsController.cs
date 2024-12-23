using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.TestStructure;

namespace Code_Record.Server.Controllers.SQLServer.Admin.SubjectStructure.TestStructure
{
    [Route("api/sql/admin/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public QuestionsController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestions()
        {
            return await _context.Questions.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Question>> GetQuestion(Guid id)
        {
            var question = await _context.Questions.FindAsync(id);

            if (question == null) return NotFound();

			return question;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuestion(Guid id, Question question)
        {
            if (id != question.Id) return BadRequest();
            var existingQuestion = GetExistingQuestion(id).Result;
            question.CreationDate = existingQuestion.CreationDate;
            question.UpdateDate = DateTime.UtcNow;
			_context.Entry(question).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionExists(id)) return NotFound();
				else throw;
			}

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Question>> PostQuestion(Question question)
        {
            question.CreationDate = DateTime.UtcNow;
            question.UpdateDate = null;
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetQuestion", new { id = question.Id }, question);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null) return NotFound();

			_context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuestionExists(Guid id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }

		private async Task<Question> GetExistingQuestion(Guid id)
		{
			var existingQuestion = await _context.Questions
			.AsNoTracking()
			.FirstOrDefaultAsync(q => q.Id == id);

			return existingQuestion;
		}
	}
}
