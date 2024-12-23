using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;

namespace Code_Record.Server.Controllers.SQLServer.Admin.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure
{
    [Route("api/sql/admin/[controller]")]
    [ApiController]
    public class AnswersController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public AnswersController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnwser(Guid id, Answer anwser)
        {
            if (id != anwser.Id) return BadRequest();
            var existingAnwser = GetExistingAnwser(id).Result;
            anwser.CreationDate = existingAnwser.CreationDate;
            anwser.UpdateDate = DateTime.UtcNow;
			_context.Entry(anwser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnwserExists(id)) return NotFound();
				else throw;
			}

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnwser(Guid id)
        {
            var anwser = await _context.Anwsers.FindAsync(id);
            if (anwser == null) return NotFound();

			_context.Anwsers.Remove(anwser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AnwserExists(Guid id)
        {
            return _context.Anwsers.Any(e => e.Id == id);
        }

		private async Task<Answer> GetExistingAnwser(Guid id)
		{
			var existingAnwser = await _context.Anwsers
			.AsNoTracking()
			.FirstOrDefaultAsync(a => a.Id == id);

			return existingAnwser;
		}
	}
}
