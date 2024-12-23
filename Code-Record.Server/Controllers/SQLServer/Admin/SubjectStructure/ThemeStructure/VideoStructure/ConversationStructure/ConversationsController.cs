using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;

namespace Code_Record.Server.Controllers.SQLServer.Admin.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure
{
    [Route("api/sql/admin/[controller]")]
    [ApiController]
    public class ConversationsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public ConversationsController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutConversation(Guid id, Conversation conversation)
        {
            if (id != conversation.Id) return BadRequest();
            var existingConversation = GetExistingConversation(id).Result;
            conversation.CreationDate = existingConversation.CreationDate;
            conversation.UpdateDate = DateTime.UtcNow;
			_context.Entry(conversation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConversationExists(id)) return NotFound();
				else throw;
			}

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConversation(Guid id)
        {
            var conversation = await _context.Conversations.FindAsync(id);
            if (conversation == null) return NotFound();

			_context.Conversations.Remove(conversation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ConversationExists(Guid id)
        {
            return _context.Conversations.Any(e => e.Id == id);
        }

		private async Task<Conversation> GetExistingConversation(Guid id)
		{
			var existingConversation = await _context.Conversations
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Id == id);

			return existingConversation;
		}
	}
}
