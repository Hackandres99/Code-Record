using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;
using MongoDB.Bson;

namespace Code_Record.Server.Controllers.MongoAtlas.Admin.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure
{
	[Route("api/mongo/admin/[controller]")]
	[ApiController]
	public class CommentsController : ControllerBase
	{
		private readonly IMongoCollection<Comment> _commentsCollection;

		public CommentsController(IMongoDatabase database)
		{
			_commentsCollection = database.GetCollection<Comment>("Comments");
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutComment(string id, Comment comment)
		{
			if (!ObjectId.TryParse(id, out var commentId))
				return BadRequest(new { error = "Invalid comment ID format." });

			if (id != comment.Id.ToString())
				return BadRequest(new { error = "Comment ID does not match." });

			var existingComment = await _commentsCollection.Find(c => c.Id == commentId).FirstOrDefaultAsync();
			if (existingComment == null) return NotFound();

			comment.CreationDate = existingComment.CreationDate;
			comment.UpdateDate = DateTime.UtcNow;

			var result = await _commentsCollection.ReplaceOneAsync(
				c => c.Id == commentId,
				comment
			);

			if (result.MatchedCount == 0)
				return NotFound();

			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteComment(string id)
		{
			if (!ObjectId.TryParse(id, out var commentId))
				return BadRequest(new { error = "Invalid comment ID format." });

			var result = await _commentsCollection.DeleteOneAsync(c => c.Id == commentId);

			if (result.DeletedCount == 0)
				return NotFound();

			return NoContent();
		}
	}
}

