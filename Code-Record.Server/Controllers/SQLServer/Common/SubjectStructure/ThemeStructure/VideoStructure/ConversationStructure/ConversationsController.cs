using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.SQLServer.Common.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure
{
    [Route("api/sql/common/[controller]")]
    [ApiController]
    public class ConversationsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public ConversationsController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Conversation>>> GetConversations()
        {
            return await _context.Conversations.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Conversation>> GetConversation(Guid id)
        {
            var conversation = await _context.Conversations.FindAsync(id);

            if (conversation == null) return NotFound();

			return conversation;
        }

        [HttpPut("me/{id}")]
        public async Task<IActionResult> PutConversation(Guid id, Conversation conversation)
        {
			var userEmail = GetCurrentUserEmail();

			if (id != conversation.Id)
				return BadRequest(new { error = "Conversation ID does not match." });

			var existingConversation = GetExistingConversation(id, userEmail).Result;

			if (existingConversation == null)
				return NotFound(new { error = "Conversation not found or you do not have access." });

			conversation.CreationDate = existingConversation.CreationDate;
			conversation.UpdateDate = DateTime.UtcNow;
			_context.Entry(conversation).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ConversationExists(id))
					return NotFound(new { error = "Conversation no longer exists." });
				else throw;
			}

			return NoContent();
		}

        [HttpPost]
        public async Task<ActionResult<Conversation>> PostConversation(Conversation conversation)
        {
            conversation.UpdateDate = null;
            conversation.CreationDate = DateTime.UtcNow;
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetConversation", new { id = conversation.Id }, conversation);
        }

        [HttpDelete("me/{id}")]
        public async Task<IActionResult> DeleteConversation(Guid id)
        {
			var userEmail = GetCurrentUserEmail();

			var conversation = await _context.Conversations
				.FirstOrDefaultAsync(c => c.Id == id && c.UserEmail == userEmail);

			if (conversation == null)
				return NotFound(new { error = "Conversation not found or you do not have access." });

			_context.Conversations.Remove(conversation);
			await _context.SaveChangesAsync();

			return NoContent();
		}

        private bool ConversationExists(Guid id)
        {
            return _context.Conversations.Any(e => e.Id == id);
        }

		private string GetCurrentUserEmail()
		{
			return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
				   ?? throw new UnauthorizedAccessException("User is not authenticated.");
		}

		private async Task<Conversation> GetExistingConversation(Guid id, string userEmail)
		{
			var existingConversation = await _context.Conversations
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Id == id && c.UserEmail == userEmail);

			return existingConversation;
		}

	}
}
