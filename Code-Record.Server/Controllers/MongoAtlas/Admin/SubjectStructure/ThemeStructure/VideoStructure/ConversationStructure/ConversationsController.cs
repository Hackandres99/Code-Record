using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;
using MongoDB.Bson;

namespace Code_Record.Server.Controllers.MongoAtlas.Admin.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure
{
	[Route("api/mongo/admin/[controller]")]
	[ApiController]
	public class ConversationsController : ControllerBase
	{
		private readonly IMongoCollection<Conversation> _conversationsCollection;

		public ConversationsController(IMongoDatabase database)
		{
			_conversationsCollection = database.GetCollection<Conversation>("Conversations");
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutConversation(string id, Conversation conversation)
		{
			if (!ObjectId.TryParse(id, out var conversationId))
				return BadRequest(new { error = "Invalid Conversation ID format." });

			if (id != conversation.Id.ToString())
				return BadRequest(new { error = "Conversation ID does not match." });

			var existingConversation = await _conversationsCollection.Find(c => c.Id == conversationId).FirstOrDefaultAsync();
			if (existingConversation == null) return NotFound();

			conversation.CreationDate = existingConversation.CreationDate;
			conversation.UpdateDate = DateTime.UtcNow;

			var result = await _conversationsCollection.ReplaceOneAsync(
				c => c.Id == conversationId,
				conversation
			);

			if (result.MatchedCount == 0)
				return NotFound();

			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteConversation(string id)
		{
			if (!ObjectId.TryParse(id, out var conversationId))
				return BadRequest(new { error = "Invalid Conversation ID format." });

			var result = await _conversationsCollection.DeleteOneAsync(c => c.Id == conversationId);

			if (result.DeletedCount == 0)
				return NotFound();

			return NoContent();
		}
	}
}
