using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.MongoAtlas.Common.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure
{
	[Route("api/mongo/common/[controller]")]
	[ApiController]
	public class ConversationsController : ControllerBase
	{
		private readonly IMongoCollection<Conversation> _conversationsCollection;

		public ConversationsController(IMongoDatabase database)
		{
			_conversationsCollection = database.GetCollection<Conversation>("Conversations");
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Conversation>>> GetConversations()
		{
			var conversations = await _conversationsCollection.Find(_ => true).ToListAsync();
			return Ok(conversations);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Conversation>> GetConversation(string id)
		{
			if (!ObjectId.TryParse(id, out var resultId))
				return BadRequest(new { error = "Invalid Conversation ID format." });

			var conversation = await _conversationsCollection.Find(c => c.Id == resultId).FirstOrDefaultAsync();

			if (conversation == null) return NotFound();

			return Ok(conversation);
		}

		[HttpPut("me/{id}")]
		public async Task<IActionResult> PutConversation(string id, Conversation conversation)
		{
			var userEmail = GetCurrentUserEmail();

			if (!ObjectId.TryParse(id, out var resultId))
				return BadRequest(new { error = "Invalid Conversation ID format." });

			if (conversation.Id != resultId)
				return BadRequest(new { error = "Conversation ID does not match." });

			var existingConversation = await _conversationsCollection
				.Find(c => c.Id == resultId && c.UserEmail == userEmail)
				.FirstOrDefaultAsync();

			if (existingConversation == null)
				return NotFound(new { error = "Conversation not found or you do not have access." });

			conversation.CreationDate = existingConversation.CreationDate;
			conversation.UpdateDate = DateTime.UtcNow;

			var result = await _conversationsCollection.ReplaceOneAsync(
				c => c.Id == resultId && c.UserEmail == userEmail,
				conversation
			);

			if (result.MatchedCount == 0)
				return NotFound(new { error = "Conversation no longer exists." });

			return NoContent();
		}

		[HttpPost]
		public async Task<ActionResult<Conversation>> PostConversation(Conversation conversation)
		{
			conversation.UpdateDate = null;
			conversation.CreationDate = DateTime.UtcNow;

			await _conversationsCollection.InsertOneAsync(conversation);

			return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id.ToString() }, conversation);
		}

		[HttpDelete("me/{id}")]
		public async Task<IActionResult> DeleteConversation(string id)
		{
			var userEmail = GetCurrentUserEmail();

			if (!ObjectId.TryParse(id, out var resultId))
				return BadRequest(new { error = "Invalid Conversation ID format." });

			var result = await _conversationsCollection.DeleteOneAsync(
				c => c.Id == resultId && c.UserEmail == userEmail
			);

			if (result.DeletedCount == 0)
				return NotFound(new { error = "Conversation not found or you do not have access." });

			return NoContent();
		}

		private string GetCurrentUserEmail()
		{
			return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
				   ?? throw new UnauthorizedAccessException("User is not authenticated.");
		}
	}
}

